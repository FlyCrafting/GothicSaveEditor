using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Win32;

namespace GothicSaveTools
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
