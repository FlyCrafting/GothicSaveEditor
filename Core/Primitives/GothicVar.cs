using System.Collections.Generic;

namespace GothicSaveEditor.Core.Primitives
{
    public struct GothicVar
    {
        public int[] Positions { get; }
        public int[] Values { get; }

        public GothicVar(int[] positions, int[] values)
        {
            Positions = positions;
            Values = values;
        }
    }
}
