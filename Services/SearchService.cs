using GothicSaveTools;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GothicSaveEditor.Services
{
    public static class SearchService
    {
        public static List<GothicVariable> Search(string searchText, List<GothicVariable> variables)
        {
            Regex regx;
            try
            {
                regx = new Regex(Regex.Escape(searchText), RegexOptions.IgnoreCase);
            }
            catch (Exception ex) 
            {
                throw ex;
            }

            List<GothicVariable> searchMatches = new List<GothicVariable>();

            foreach (GothicVariable gv in variables)
            {
                if (regx.IsMatch(gv.VariableName))
                {
                    searchMatches.Add(gv);
                }
            }
            return searchMatches;
        }
    }
}
