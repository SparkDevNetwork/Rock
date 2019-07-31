using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RockInstallTools
{
    public class RockScriptParser
    {
        public string[] ScriptCollection {
            get
            {
                return _scriptCollection;
            }
        }

        public int ScriptCount
        {
            get
            {
                if ( _scriptCollection != null )
                {
                    return _scriptCollection.Count();
                }
                else
                {
                    return 0;
                }
            }
        }

        private string _script;
        private string[] _scriptCollection;
        private string sql;

        public RockScriptParser( string script )
        {
            _script = script;
            ParseScript();
        }

        private void ParseScript() {
            _scriptCollection = Regex.Split( _script, "\r\nGO\r\n" );
        }
    }
}
