using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADSync.Common.Models
{
    public class ScriptObject
    {
        public string ScriptName { get; set; }
        public Dictionary<string, string> ScriptArgs { get; set; }
        public bool IsRunning { get; set; }

        public ScriptObject(string scriptName, Dictionary<string, string> scriptArgs = null)
        {
            ScriptName = scriptName;
            ScriptArgs = scriptArgs;
        }
        public ScriptObject()
        {

        }
    }
}
