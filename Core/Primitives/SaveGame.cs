using System.Collections.Generic;
using GothicSaveEditor.Models;

namespace GothicSaveEditor.Core.Primitives
{
    public struct SaveGame
    {
        public string FilePath { get; }

        public List<GothicVariable> VariablesList { get;}

        public SaveGame(string filePath, List<GothicVariable> varList)
        {
            FilePath = filePath;
            VariablesList = varList;
        }

    }
}
