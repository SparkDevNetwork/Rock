using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Model;

namespace Rock.Tests.Model
{
    [TestClass]
    public class PersonalLinkServiceTests
    {
        /// <summary>
        /// Script schemes, schemes outside the allowed list and empty values
        /// must be refused, including when control characters or whitespace
        /// that browsers strip are used to hide the scheme.
        /// </summary>
        [TestMethod]
        [DataRow( null )]
        [DataRow( "" )]
        [DataRow( "   " )]
        [DataRow( "javascript:alert(1)" )]
        [DataRow( "JavaScript:alert(1)" )]
        [DataRow( " javascript:alert(1)" )]
        [DataRow( "java\tscript:alert(1)" )]
        [DataRow( "java\nscript:alert(1)" )]
        [DataRow( "javas\r\ncript:alert(1)" )]
        [DataRow( "\u0001javascript:alert(1)" )]
        [DataRow( "vbscript:msgbox(1)" )]
        [DataRow( "data:text/html,<script>" )]
        [DataRow( "sms:5551234" )]
        [DataRow( "localhost:6229/page/12" )]
        public void IsUrlSchemeAllowed_RefusesUnsafeUrls( string url )
        {
            Assert.IsFalse( PersonalLinkService.IsUrlSchemeAllowed( url ) );
        }

        /// <summary>
        /// The allowed schemes and relative URLs must be accepted.
        /// </summary>
        [TestMethod]
        [DataRow( "https://www.rockrms.com" )]
        [DataRow( "HTTP://x.org" )]
        [DataRow( "mailto:a@b.org" )]
        [DataRow( "tel:+15551234" )]
        [DataRow( "/page/12" )]
        [DataRow( "page/12?a=b:c" )]
        [DataRow( "?q=a:b" )]
        [DataRow( "#top" )]
        [DataRow( "//cdn.example.org/x" )]
        public void IsUrlSchemeAllowed_AllowsSafeUrls( string url )
        {
            Assert.IsTrue( PersonalLinkService.IsUrlSchemeAllowed( url ) );
        }

        /// <summary>
        /// An entity encoded colon is not a scheme separator to the server
        /// helper, so the value is treated as a relative URL and allowed. It
        /// is only safe because the render side HTML encodes the href, which
        /// keeps the browser from decoding it into a javascript URL. Do not
        /// remove that render encoding on the assumption that this helper
        /// covers it.
        /// </summary>
        [TestMethod]
        [DataRow( "javascript&#58;alert(1)" )]
        public void IsUrlSchemeAllowed_AllowsEntityEncodedColonAsRelative( string url )
        {
            Assert.IsTrue( PersonalLinkService.IsUrlSchemeAllowed( url ) );
        }
    }
}
