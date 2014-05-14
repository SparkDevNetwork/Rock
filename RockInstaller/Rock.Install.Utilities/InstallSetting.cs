using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Install.Utilities
{
    public static class InstallSetting
    {

#if DEBUG
        static string _dotNetVersionRequired = "4.5";
        static double _iisVersionRequired = 7.0;

#else
        static string _dotNetVersionRequired = "4.5";
        static double _iisVersionRequired = 7.0;

#endif


        public static string DotNetVersionRequired
        {
            get
            {
                return _dotNetVersionRequired;
            }
        }

        public static double IisVersionRequired
        {
            get
            {
                return _iisVersionRequired;
            }
        }
    }
}
