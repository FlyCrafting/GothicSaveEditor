using System;

namespace GothicSaveEditor.Models
{
    public class GothicVariable: IComparable
    {
        //Для DataGrid VariableName, Value
        public string VariableName { get; }
        private int _value;
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

        public bool Modified { get; private set; }


        public void SetUnModified()
        {
            Modified = false;
        }

        public int Position { get; }

        private readonly int? _arrayIndex;

        //Получение имени вместе с номером элемента
        public string FullName => _arrayIndex == null ? VariableName : $"{VariableName}[{_arrayIndex}]";

        public int CompareTo(object obj)
        {
            var p = obj as GothicVariable;
            return string.CompareOrdinal(FullName, p.FullName);
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
