using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Plugin
{
    [AttributeUsage( AttributeTargets.Method, AllowMultiple=false)]
    public class RockVersion : System.Attribute
    {
        public string Version { get; set; }
        public RockVersion( string version )
            : base()
        {
            Version = version;
        }
    }
}
