using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GothicSaveTools
{
    public class VariablesComparer: IComparer<GothicVariable>
    {
        private enum ChunkType { Alphanumeric, Numeric };
        private bool InChunk(char ch, char otherCh)
        {
            ChunkType type = ChunkType.Alphanumeric;
            if (char.IsDigit(otherCh))
            {
                type = ChunkType.Numeric;
            }
            if ((type == ChunkType.Alphanumeric && char.IsDigit(ch))
                || (type == ChunkType.Numeric && !char.IsDigit(ch)))
            {
                return false;
            }
            return true;
        }

        public int Compare(GothicVariable x, GothicVariable y)
        {
            String s1 = x.FullName;
            String s2 = y.FullName;
            if (s1 == null || s2 == null)
            {
                return 0;
            }
            int thisMarker = 0;
            int thatMarker = 0;
            while ((thisMarker < s1.Length) || (thatMarker < s2.Length))
            {
                if (thisMarker >= s1.Length)
                {
                    return -1;
                }
                if (thatMarker >= s2.Length)
                {
                    return 1;
                }
                char thisCh = s1[thisMarker];
                char thatCh = s2[thatMarker];
                StringBuilder thisChunk = new StringBuilder();
                StringBuilder thatChunk = new StringBuilder();
                while ((thisMarker < s1.Length) && (thisChunk.Length == 0 || InChunk(thisCh, thisChunk[0])))
                {
                    thisChunk.Append(thisCh);
                    thisMarker++;
                    if (thisMarker < s1.Length)
                    {
                        thisCh = s1[thisMarker];
                    }
                }
                while ((thatMarker < s2.Length) && (thatChunk.Length == 0 || InChunk(thatCh, thatChunk[0])))
                {
                    thatChunk.Append(thatCh);
                    thatMarker++;
                    if (thatMarker < s2.Length)
                    {
                        thatCh = s2[thatMarker];
                    }
                }
                int result = 0;
                // If both chunks contain numeric characters, sort them numerically
                if (char.IsDigit(thisChunk[0]) && char.IsDigit(thatChunk[0]))
                {
                    var thisNumericChunk = Convert.ToInt32(thisChunk.ToString());
                    var thatNumericChunk = Convert.ToInt32(thatChunk.ToString());
                    if (thisNumericChunk < thatNumericChunk)
                    {
                        result = -1;
                    }

                    if (thisNumericChunk > thatNumericChunk)
                    {
                        result = 1;
                    }
                }
                else
                {
                    result = thisChunk.ToString().CompareTo(thatChunk.ToString());
                }
                if (result != 0)
                {
                    return result;
                }
            }
            return 0;
        }
    }
}
