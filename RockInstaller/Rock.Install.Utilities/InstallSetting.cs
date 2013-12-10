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
        static string _internetCheckSite = "www.google.com";
        static string _dotNetVersionRequired = "4.5";
        static double _iisVersionRequired = 7.0;
        static string _rockInstallFile = "http://rockchms.blob.core.windows.net/install/test-zip-file.zip";

        static string _rockLogoIco = "http://rockchms.blob.core.windows.net/install/rock-chms.ico";
        static string _rockStyles = "/install.css";
        static string _rockWelcomeImg = "http://rockchms.blob.core.windows.net/install/welcome.jpg";
#else
        static string _internetCheckSite = "www.google.com";
        static string _dotNetVersionRequired = "4.5";
        static double _iisVersionRequired = 7.0;
        static string _rockInstallFile = "http://rockchms.blob.core.windows.net/install/rock-chms-latest.zip";

        static string _rockLogoIco = "http://rockchms.blob.core.windows.net/install/rock-chms.ico";
        static string _rockStyles = "http://rockchms.blob.core.windows.net/install/install.css";
        static string _rockWelcomeImg = "http://rockchms.blob.core.windows.net/install/welcome.jpg";
#endif



        public static string InternetCheckSite
        {
            get
            {
                return _internetCheckSite;
            }
        }

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

        public static string RockInstallFile
        {
            get
            {
                return _rockInstallFile;
            }
        }

        public static string RockLogoIco
        {
            get
            {
                return _rockLogoIco;
            }
        }

        public static string RockStyles
        {
            get
            {
                return _rockStyles;
            }
        }

        public static string RockWelcomeImg
        {
            get
            {
                return _rockWelcomeImg;
            }
        }

    }
}
