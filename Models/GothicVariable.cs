using System;
using System.Windows;

namespace GothicSaveTools
{
    public class GothicVariable: IComparable
    {
        //Для DataGrid VariableName, Value
        public string VariableName { get; }
        public int Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    Modified = true;
                    _value = value;
                }
            }
        }
        private int _value;

        public bool Modified { get; private set; } = false;

        public void Saved()
        {
            Modified = false;
        }

        public int Position { get; }

        private readonly int? _arrayIndex;

        //Получение имени вместе с номером элемента
        public string FullName => _arrayIndex == null ? VariableName : $"{VariableName}[{_arrayIndex}]";

        public int CompareTo(object obj)
        {
            GothicVariable p = obj as GothicVariable;
            return string.Compare(FullName, p.FullName);
        }

        public GothicVariable(string varName, int pos, int value, int? arrayIndex = null)
        {
            VariableName = varName;
            Position = pos;
            _value = value;
            _arrayIndex = arrayIndex;
        }
    }
}
