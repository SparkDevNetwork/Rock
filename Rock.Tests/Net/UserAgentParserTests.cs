using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Net;

namespace Rock.Tests.Net
{
    [TestClass]
    public class UserAgentParserTests
    {
        private static readonly UAParser.Parser _parser = UAParser.Parser.GetDefault();

        [TestMethod]
        [DataRow( "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36", "Windows 10" )]
        [DataRow( "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML like Gecko) Chrome/36.0.1985.143 Safari/537.36", "Windows 7" )]
        [DataRow( "Mozilla/5.0 (Windows NT 6.1; rv:31.0) Gecko/20100101 Firefox/31.0", "Windows 7" )]
        [DataRow( "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:32.0) Gecko/20100101 Firefox/32.0", "Windows 8.1" )]
        [DataRow( "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)", "Windows 7" )]
        [DataRow( "Mozilla/5.0 (Macintosh; Intel Mac OS X 1094) AppleWebKit/537.77.4 (KHTML like Gecko) Version/7.0.5 Safari/537.77.4", "Mac OS X" )]
        [DataRow( "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.9; rv:32.0) Gecko/20100101 Firefox/32.0", "Mac OS X 10.9" )]
        [DataRow( "Mozilla/5.0 (iPhone; CPU iPhone OS 712 like Mac OS X) AppleWebKit/537.51.2 (KHTML like Gecko) Version/7.0 Mobile/11D257 Safari/9537.53", "iOS 7.0" )]
        [DataRow( "Mozilla/5.0 (iPad; CPU OS 613 like Mac OS X) AppleWebKit/536.26 (KHTML like Gecko) Version/6.0 Mobile/10B329 Safari/8536.25", "iOS 6.0" )]
        [DataRow( "Mozilla/5.0 (X11; Ubuntu; Linux x8664; rv:32.0) Gecko/20100101 Firefox/32.0", "Ubuntu" )]
        [DataRow( "Mozilla/5.0 (X11; Linux x8664) AppleWebKit/537.36 (KHTML like Gecko) Chrome/36.0.1985.143 Safari/537.36", "Linux" )]
        [DataRow( "Microsoft Office/16.0 (Windows NT 10.0; Microsoft Outlook 16.0.4266; Pro)", "Windows 10" )]
        [DataRow( "Mozilla/5.0 (compatible; proximic; +http://www.proximic.com/info/spider.php)", "Other" )]
        [DataRow( "", "Other" )]
        public void GetOSFamilyVersion_ReturnsCorrectValue( string userAgent, string expected )
        {
            var parser = new UserAgentParser();
            var info = parser.Parse( userAgent );

            Assert.AreEqual( expected, info.GetOSFamilyVersion() );
        }

        [TestMethod]
        [DataRow( "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36", "Chrome 91.0.4472" )]
        [DataRow( "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML like Gecko) Chrome/36.0.1985.143 Safari/537.36", "Chrome 36.0.1985" )]
        [DataRow( "Mozilla/5.0 (Windows NT 6.1; rv:31.0) Gecko/20100101 Firefox/31.0", "Firefox 31.0" )]
        [DataRow( "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:32.0) Gecko/20100101 Firefox/32.0", "Firefox 32.0" )]
        [DataRow( "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)", "IE 10.0" )]
        [DataRow( "Mozilla/5.0 (Macintosh; Intel Mac OS X 1094) AppleWebKit/537.77.4 (KHTML like Gecko) Version/7.0.5 Safari/537.77.4", "Safari 7.0.5" )]
        [DataRow( "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.9; rv:32.0) Gecko/20100101 Firefox/32.0", "Firefox 32.0" )]
        [DataRow( "Mozilla/5.0 (iPhone; CPU iPhone OS 712 like Mac OS X) AppleWebKit/537.51.2 (KHTML like Gecko) Version/7.0 Mobile/11D257 Safari/9537.53", "Mobile Safari 7.0" )]
        [DataRow( "Mozilla/5.0 (iPad; CPU OS 613 like Mac OS X) AppleWebKit/536.26 (KHTML like Gecko) Version/6.0 Mobile/10B329 Safari/8536.25", "Mobile Safari 6.0" )]
        [DataRow( "Mozilla/5.0 (X11; Ubuntu; Linux x8664; rv:32.0) Gecko/20100101 Firefox/32.0", "Firefox 32.0" )]
        [DataRow( "Mozilla/5.0 (X11; Linux x8664) AppleWebKit/537.36 (KHTML like Gecko) Chrome/36.0.1985.143 Safari/537.36", "Chrome 36.0.1985" )]
        [DataRow( "Microsoft Office/16.0 (Windows NT 10.0; Microsoft Outlook 16.0.4266; Pro)", "Outlook 2016" )]
        [DataRow( "Mozilla/5.0 (compatible; proximic; +http://www.proximic.com/info/spider.php)", "spider" )]
        [DataRow( "", "Other" )]
        public void GetBrowserFamilyVersion_ReturnsCorrectValue( string userAgent, string expected )
        {
            var parser = new UserAgentParser();
            var info = parser.Parse( userAgent );

            Assert.AreEqual( expected, info.GetBrowserFamilyVersion() );
        }

        [TestMethod]
        [DataRow( "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36", "Desktop" )]
        [DataRow( "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML like Gecko) Chrome/36.0.1985.143 Safari/537.36", "Desktop" )]
        [DataRow( "Mozilla/5.0 (Windows NT 6.1; rv:31.0) Gecko/20100101 Firefox/31.0", "Desktop" )]
        [DataRow( "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:32.0) Gecko/20100101 Firefox/32.0", "Desktop" )]
        [DataRow( "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)", "Desktop" )]
        [DataRow( "Mozilla/5.0 (Macintosh; Intel Mac OS X 1094) AppleWebKit/537.77.4 (KHTML like Gecko) Version/7.0.5 Safari/537.77.4", "Desktop" )]
        [DataRow( "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.9; rv:32.0) Gecko/20100101 Firefox/32.0", "Desktop" )]
        [DataRow( "Mozilla/5.0 (iPhone; CPU iPhone OS 712 like Mac OS X) AppleWebKit/537.51.2 (KHTML like Gecko) Version/7.0 Mobile/11D257 Safari/9537.53", "Mobile" )]
        [DataRow( "Mozilla/5.0 (iPad; CPU OS 613 like Mac OS X) AppleWebKit/536.26 (KHTML like Gecko) Version/6.0 Mobile/10B329 Safari/8536.25", "Tablet" )]
        [DataRow( "Mozilla/5.0 (X11; Ubuntu; Linux x8664; rv:32.0) Gecko/20100101 Firefox/32.0", "Desktop" )]
        [DataRow( "Mozilla/5.0 (X11; Linux x8664) AppleWebKit/537.36 (KHTML like Gecko) Chrome/36.0.1985.143 Safari/537.36", "Desktop" )]
        // Known to fail with the current implementation of GetClientType, this
        // will start working after the implementation is updated to use our new pattern.
        [DataRow( "Microsoft Office/16.0 (Windows NT 10.0; Microsoft Outlook 16.0.4266; Pro)", "Outlook" )]
        [DataRow( "Mozilla/5.0 (compatible; proximic; +http://www.proximic.com/info/spider.php)", "Crawler" )]
        [DataRow( "", "None" )]
        public void ClientType_ReturnsCorrectValue( string userAgent, string expected )
        {
            var parser = new UserAgentParser();
            var info = parser.Parse( userAgent );

            Assert.AreEqual( expected, info.ClientType );
        }

        [TestMethod]
        [DataRow( "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36", "Windows 10 Other Chrome 91.0.4472" )]
        [DataRow( "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML like Gecko) Chrome/36.0.1985.143 Safari/537.36", "Windows 7 Other Chrome 36.0.1985" )]
        [DataRow( "Mozilla/5.0 (Windows NT 6.1; rv:31.0) Gecko/20100101 Firefox/31.0", "Windows 7 Other Firefox 31.0" )]
        [DataRow( "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:32.0) Gecko/20100101 Firefox/32.0", "Windows 8.1 Other Firefox 32.0" )]
        [DataRow( "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)", "Windows 7 Other IE 10.0" )]
        [DataRow( "Mozilla/5.0 (Macintosh; Intel Mac OS X 1094) AppleWebKit/537.77.4 (KHTML like Gecko) Version/7.0.5 Safari/537.77.4", "Mac OS X Mac Safari 7.0.5" )]
        [DataRow( "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.9; rv:32.0) Gecko/20100101 Firefox/32.0", "Mac OS X 10.9 Mac Firefox 32.0" )]
        [DataRow( "Mozilla/5.0 (iPhone; CPU iPhone OS 712 like Mac OS X) AppleWebKit/537.51.2 (KHTML like Gecko) Version/7.0 Mobile/11D257 Safari/9537.53", "iOS 7.0 iPhone Mobile Safari 7.0" )]
        [DataRow( "Mozilla/5.0 (iPad; CPU OS 613 like Mac OS X) AppleWebKit/536.26 (KHTML like Gecko) Version/6.0 Mobile/10B329 Safari/8536.25", "iOS 6.0 iPad Mobile Safari 6.0" )]
        [DataRow( "Mozilla/5.0 (X11; Ubuntu; Linux x8664; rv:32.0) Gecko/20100101 Firefox/32.0", "Ubuntu Other Firefox 32.0" )]
        [DataRow( "Mozilla/5.0 (X11; Linux x8664) AppleWebKit/537.36 (KHTML like Gecko) Chrome/36.0.1985.143 Safari/537.36", "Linux Other Chrome 36.0.1985" )]
        [DataRow( "Microsoft Office/16.0 (Windows NT 10.0; Microsoft Outlook 16.0.4266; Pro)", "Windows 10 Other Outlook 2016" )]
        [DataRow( "Mozilla/5.0 (compatible; proximic; +http://www.proximic.com/info/spider.php)", "Other Spider spider" )]
        [DataRow( "", "Other Other Other" )]
        public void UserAgentInfo_ToString_ReturnsCorrectValue( string userAgent, string expected )
        {
            var parser = new UserAgentParser();
            var info = parser.Parse( userAgent );

            Assert.AreEqual( expected, info.ToString() );
        }
    }
}
