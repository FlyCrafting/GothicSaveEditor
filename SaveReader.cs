using GothicSaveEditor;
using GothicSaveEditor.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace GothicSaveTools
{

    /// <summary>
    /// Структура сейва:
    /// 1. Заголовок, инфа о сейве
    /// 2. Диалоги
    /// 3. MIS (дневник)
    /// 4. Обычные переменные
    /// 5. Мусор(после определенного значения)
    /// </summary>
    public class SaveReader
    {
        private bool _dialogBegin;
        private bool _dialogEnd;
        private bool _dialog = true;
        private bool _mission;

        private IEnumerable<int> TryToReadVariable(ref byte[] bytes, ref int rIndex, ref PosDict rPositions)
        {
            var arrVar = new int[1];
            var arrIteration = -1;
            var isArray = false;
            while (bytes[rIndex] == 0x12) // Начало значения переменной
            {
                rIndex += 2;

                if (bytes[rIndex] == 0x03 // массивы типа int
                    || bytes[rIndex] == 0xd3) // массивы типа bool
                    isArray = true; // Фикс от 2.11.2019 на чтение переменных Готики1

                rIndex += 3;
                if (bytes[rIndex] == 0x01)
                {
                    break; // Если 0x01 то это текст, значит мы зашли сюда по ошибке.
                }

                rIndex++;
                if (bytes[rIndex - 1] == 0x02) // Значение начинается здесь
                {
                    if (_dialogBegin)
                    {
                        _mission = true;
                        _dialog = false;
                        _dialogEnd = true;
                        _dialogBegin = false;
                    }

                    if (!isArray)
                        arrIteration = 0;

                    if (arrIteration <= -1) // Для переменных если это первый блок то это длина массива
                    {
                        arrVar = new int[bytes[rIndex]];
                        for (var i = 0; i < arrVar.Length; i++) // Везде ставим нолики
                        {
                            arrVar[i] = 0;
                        }
                    }
                    else if (arrIteration < arrVar.Length)
                    {
                        arrVar[arrIteration] = BitConverter.ToInt32(bytes, rIndex);
                        rPositions[arrIteration] = rIndex;
                    }
                    arrIteration++;
                }
                else if (bytes[rIndex - 1] == 0x06) // Начало диалога
                {
                    arrVar[0] = BitConverter.ToInt32(bytes, rIndex);
                    rPositions[0] = rIndex;
                }
                rIndex += 4; // Integer занимает 4 байта

                // ReSharper disable once InvertIf
                if (_dialog && bytes[rIndex - 5] == 0x06)
                {
                    _dialogBegin = true;
                    return arrVar;
                }
            }
            return arrVar;
        }

        private static int ReadLength(ref byte[] bytes, ref int rIndex) //Читает 
        {
            rIndex += 1;
            int length = BitConverter.ToInt16(bytes, rIndex);
            rIndex += 2;
            return length;
        }

        /// <summary>
        /// Возвращает строку если _mission был равен false, в противном случае, возвращает пустую строку. 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="rIndex"></param>
        /// <param name="strLength"></param>
        /// <returns></returns>
        private string ReadName(ref byte[] bytes, ref int rIndex, int strLength)
        {
            var sb = new StringBuilder(2048);
            var readTo = rIndex + strLength;
            for (; rIndex < readTo; rIndex++)
            {
                sb.Append((char)bytes[rIndex]);
            }
            rIndex--;

            var str = sb.ToString();
            if (_mission == false)
            {
                return str;
            }

            if (str == "[]")
                _mission = false;

            return "";
        }

        /// <summary>
        /// Читает сейвгейм и возвращает массив байтов
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static byte[] ReadSaveBytes(string path)
        {
            byte[] byteArray;
            try
            {
                byteArray = File.ReadAllBytes(path); // Читаем сейв побайтово
            }
            catch (Exception ex)
            {
                throw new Exception("ReadBytesError", ex); // Ошибка чтения байтов файла
            }

            return byteArray;
        }

        /// <summary>
        /// Находит точку входа - где начинаются переменные, и точку выхода - где заканчиваются переменные.
        /// </summary>
        /// <returns></returns>
        private static (int, int) FindControlPoints(byte[] bytes)
        {
            var index = 0; // Итератор цикла
            // Скипаем ненужную bytes в начале сейва.
            while (index < bytes.Length)
            {
                if (bytes[index] == 0x02
                    && bytes[index + 1] == 0x00
                    && bytes[index + 2] == 0x00
                    && bytes[index + 3] == 0x00
                    && bytes[index + 4] == 0x01
                    && bytes[index + 5] == 0x00
                    && bytes[index + 6] == 0x00
                    && bytes[index + 7] == 0x00)
                {
                    break;
                }
                index++;
            }
            index += 8; // После того как скипнули начальную ненужную часть, нужно скипнуть следующие байты: 02 00 00 00 01 00 00 00

            int maxByte;
            try
            {
                maxByte = BitConverter.ToInt32(bytes, index); // Последний байт, до которого следует читать (далее идет мусор)
            }
            catch (Exception ex)
            {
                throw new Exception("MaxByteBroken", ex); // Ошибка чтения номера последнего байта, сейвгейм сломан.
            }
            if (maxByte == 0)
                throw new Exception("MaxByteBroken");
            index += 4;

            return (index, maxByte);
        }

        private List<GothicVariable> ParseSaveGame(byte[] byteArray, int startIndex, int maxIndex)
        {
            var needToReadValue = false;
            var varname = ""; // Название текущей переменной
            var values = new List<int>();

            var positions = new PosDict(); // Служит для закрепления позиции за каждой переменной
            var variablesList = new List<GothicVariable>();

            // Начинаем парсить переменные!
            for (var index = startIndex; index < maxIndex; index++)
            {
                if (byteArray[index] == 0x12) // Начало значения переменной
                {
                    if (_dialog) // В диалоге сначала идет чтение значения и только потом строка
                    {
                        values = TryToReadVariable(ref byteArray, ref index, ref positions).ToList(); // Передача идет по ссылке
                    }
                    else
                    {
                        // ReSharper disable once InvertIf
                        if (needToReadValue && varname.Trim().Length > 0) // Уже считали название переменной, считываем ее значение
                        {
                            values = TryToReadVariable(ref byteArray, ref index, ref positions).ToList(); // Читаем значение
                            if (_dialogEnd) // Если достигнут конец, переменная не будет сохранена, поскольку последний слайд получает неверное значение
                            {
                                _dialogEnd = false;
                            }
                            else
                            {
                                if (values.Count == 1)
                                {
                                    variablesList.Add(new GothicVariable(varname, positions[0], values[0]));
                                }
                                else if (values.Count > 1)
                                {
                                    variablesList.AddRange(values.Select((t, ki) => new GothicVariable(varname, positions[ki], t, ki)));
                                }
                                positions.Clear();
                            }
                            index--; //one back because of the while conditional i ++
                            needToReadValue = false; // Новая переменная строка должна читаться, пока не будет прочитано значение
                        }
                    }
                }
                else if (byteArray[index] == 0x01) // Начало строки(названия переменной)
                {
                    var stringLength = ReadLength(ref byteArray, ref index); // Длина названия текущей переменной
                    if (stringLength > 0)
                    {
                        varname = ReadName(ref byteArray, ref index, stringLength);
                    }
                    if (varname.Trim().Length > 0)
                    {
                        if (_dialog && _dialogBegin) // В диалоговом режиме центрирование переменной заканчивается, поэтому переменная генерируется здесь
                        {
                            if (values.Count == 1)
                            {
                                variablesList.Add(new GothicVariable(varname, positions[0], values[0]));
                            }
                            else if (values.Count > 1)
                            {
                                variablesList.AddRange(values.Select((t, ki) => new GothicVariable(varname, positions[ki], t, ki)));
                            }
                            positions.Clear();
                        }
                        else
                        {
                            needToReadValue = true; // Название было считано, теперь нужно считать значение
                        }
                    }
                }
            }
            if (variablesList.Count == 0)
            {
                throw new Exception("EmptyVariablesList");
            }

            variablesList.Sort(new VariablesComparer());
            return variablesList;
        }

        public List<GothicVariable> Read(string path)
        {
            var byteArray = ReadSaveBytes(path);
            var (startIndex, maxIndex) = FindControlPoints(byteArray);
            return ParseSaveGame(byteArray, startIndex, maxIndex);
        }
    }
}
