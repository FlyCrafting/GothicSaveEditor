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
    public class SaveParser
    {
        private bool _dialogMode = true;
        private bool _dialogBegin;
        private bool _dialogEnd;
        private bool _diaryMode;

        public List<GothicVariable> Parse(string path)
        {
            //Вторая проверка на чтение байтов из файла
            byte[] byteArray; //Хранит весь сейв в байтах
            try
            {
                byteArray = File.ReadAllBytes(path); //Чтение байтов с сейва
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                MessageBox.Show(ResourceService.GetString("UnableToLoadSavegameItsBroken"));
                return null;
            }

            int index = 0; //Основная переменная итерации цикла парсинга
            //Отсеиваем ненужную часть сейва(данные о сейве, там где нет переменных)
            while (index < byteArray.Length)
            {
                if (byteArray[index] == 0x02
                    && byteArray[index + 1] == 0x00
                    && byteArray[index + 2] == 0x00
                    && byteArray[index + 3] == 0x00
                    && byteArray[index + 4] == 0x01
                    && byteArray[index + 5] == 0x00
                    && byteArray[index + 6] == 0x00
                    && byteArray[index + 7] == 0x00)
                {
                    break;
                }
                index++;
            }
            index += 8; // 02 00 00 00 01 00 00 00
            int maxByte;
            try
            {
                maxByte = BitConverter.ToInt32(byteArray, index); //Последний байт
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                MessageBox.Show(ResourceService.GetString("UnableToLoadSavegameItsBroken"));
                return null;
            }
            index += 3;

            // Инициализация всех необходимых переменных
            bool readMode = false;
            string varname = ""; // Название текущей переменной
            List<int> values = new List<int>();

            var positions = new PosDict(); //Служит для закрепления позиции за каждой переменной
            var variablesList = new List<GothicVariable>();

            //Начинаем парсить переменные!
            while (index < maxByte)
            {
                try
                {
                    if (byteArray[index] == 0x12)//Начало значения переменной
                    {
                        if (_dialogMode) // В диалоге сначала идет чтение значения и только потом переменной
                        {
                            values = JumpNr(ref byteArray, ref index, ref positions).ToList(); //Передача идет по ссылке
                        }
                        else
                        {
                            if (readMode && varname.Trim().Length > 0)//readmode specifies whether a string has already been read, because the value for normal variables after the variable string is read.
                            {
                                values = JumpNr(ref byteArray, ref index, ref positions).ToList();//Читаем значение
                                if (_dialogEnd)//If the end is reached, no variable is to be saved, since the last slide then gets an incorrect value
                                {
                                    _dialogEnd = false;
                                }
                                else
                                {
                                    if (values.Count == 1)
                                    {
                                        variablesList.Add(new GothicVariable(varname, positions[0], values[0]));//Create variable
                                    }
                                    else if (values.Count > 1)
                                    {
                                        for (int ki = 0; ki < values.Count; ki++)
                                        {
                                            variablesList.Add(new GothicVariable(varname, positions[ki], values[ki], ki));
                                        }
                                    }
                                    positions.Clear();
                                }
                                index--; //one back because of the while conditional i ++
                                readMode = false;//a new variable string is to be read until a value is read
                            }
                        }
                    }
                    else if (byteArray[index] == 0x01)//Начало строки(названия переменной)
                    {
                        var stringLength = ReadLength(ref byteArray, ref index); // Длина названия текущей переменной
                        if (stringLength > 0)//Valid?
                        {
                            varname = ReadName(ref byteArray, ref index, stringLength);//Name Lesen
                        }
                        if (varname.Trim().Length > 0)//Valid?
                        {
                            if (_dialogMode && _dialogBegin)//In the dialog mode, variable centering comes to an end, so the variable is generated here
                            {
                                if (values.Count == 1)
                                {
                                    variablesList.Add(new GothicVariable(varname, positions[0], values[0]));//Create variable
                                }
                                else if (values.Count > 1)
                                {
                                    for (int ki = 0; ki < values.Count; ki++)
                                    {
                                        variablesList.Add(new GothicVariable(varname, positions[ki], values[ki], ki));
                                    }
                                }
                                positions.Clear();
                            }
                            else
                            {
                                readMode = true;//Name has been read, now the value can be read
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    MessageBox.Show(ResourceService.GetString("UnableToLoadSavegameItsBroken"));
                    return null;
                }
                index++;
            }
            if (variablesList.Count == 0)
            {
                Logger.Log("Savegame variables length is 0!");
                MessageBox.Show(ResourceService.GetString("UnableToLoadSavegameItsBroken"));
                return null;
            }

            variablesList.Sort(new VariablesComparer());
            return variablesList;
        }

        private int[] JumpNr(ref byte[] bytes, ref int rIndex, ref PosDict rPositions)//Reads the values
        {
            int[] arrVar = new int[1];
            var arrIteration = -1;
            while (bytes[rIndex] == 0x12) //Начало значения переменной
            {
                rIndex += 5;   //the numbering does not interest us, so skip it
                if (bytes[rIndex] == 0x01) continue;
                rIndex++;
                if (bytes[rIndex - 1] == 0x02) //Variables starts here!
                {
                    if (_dialogBegin)
                    {
                        _diaryMode = true;
                        _dialogMode = false;
                        _dialogEnd = true;
                        _dialogBegin = false;
                    }
                    if (arrIteration <= -1) // Для переменных если это первый блок то длина
                    {
                        arrVar = new int[bytes[rIndex]];
                        for (int i = 0; i < arrVar.Length; i++)
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
                else if (bytes[rIndex - 1] == 0x06) // Dialogues and unknown types
                {
                    arrVar[0] = BitConverter.ToInt32(bytes, rIndex);
                    rPositions[0] = rIndex;
                }
                rIndex += 4;//Integer is 4 bytes long
                if (!_dialogMode || bytes[rIndex - 5] != 0x06) continue;
                _dialogBegin = true;
                return arrVar;
                //Is there no return? Because only the last block stores the value (yes, the format of the PBs is very 'interesting')
            }
            return arrVar;
        }

        private int ReadLength(ref byte[] bytes, ref int rIndex)//Reads the length of the variable string
        {
            rIndex += 1;
            int length = BitConverter.ToInt16(bytes, rIndex);
            rIndex += 2;
            return length;
        }

        private string ReadName(ref byte[] bytes, ref int rIndex, int strLength)//Reads the name of the variable
        {
            var sB = new StringBuilder(2048); //65k: Yes, is a short, therefore, take a maximum of a short (meaningless to take so long variable names, but sure is safe)
            int i;
            for (i = rIndex; i < rIndex + strLength; i++)//all bytes
            {
                sB.Append((char)(bytes[i]));
            }
            var str = sB.ToString();
            rIndex += str.Length - 1;//index update
            if (!_diaryMode || str != ("[]")) return _diaryMode ? "" : str;
            _diaryMode = false;
            return "";
        }

    }
}
