using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GothicSaveEditor.Models
{
    public class Script
    {
        public string Name { get; set; }
        public Dictionary<string, int> actions = new Dictionary<string, int>();
        private readonly Action<Script> executeVoid;

        public RelayCommand ImportScriptCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    Task.Run(() => executeVoid(this));
                });
            }
        }


        public Script(string name, Dictionary<string, int> actions, Action<Script> executeVoid)
        {
            Name = name;
            this.actions = actions;
            this.executeVoid = executeVoid;
        }
    }
}
