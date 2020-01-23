using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GothicSaveEditor.Models;

namespace GothicSaveEditor.Core.Utils
{
    public static class Extensions
    {
        public static List<GothicVariable> Search(this List<GothicVariable> variables, string searchText)
        {
            var regx = new Regex(Regex.Escape(searchText), RegexOptions.IgnoreCase);
            return variables.Where(gv => regx.IsMatch(gv.VariableName)).ToList();
        }
    }
}
