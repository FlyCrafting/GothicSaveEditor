using System.Collections.Generic;

namespace GothicSaveTools
{
    public class PosDict
    {
        private readonly Dictionary<int, int> _dict = new Dictionary<int, int>();

        public void Clear()
        {
            _dict.Clear();
        }

        public int this[int index]
        {
            get
            {
                if (!_dict.ContainsKey(index))
                {
                    _dict[index] = 0;
                }
                return _dict[index];
            }
            set => _dict[index] = value;
        }
    }
}
