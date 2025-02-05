using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Tests.Shared;
using Rock.Utility;

namespace Rock.Tests.Utility
{
    [TestClass]
    public class WebRequestHelperTests
    {
        #region GetXForwardedForIpAddress

        /// <summary>
        /// Tests the <see cref="global::Rock.Utility.WebRequestHelper.GetXForwardedForIpAddress(HttpRequestBase)"/> method
        /// and verifies that it returns null when no forwareded header is present.
        /// </summary>
        [TestMethod]
        public void GetXForwardedForIpAddress_NonForwardedRequestReturnsNull()
        {
            var ipAddress = WebRequestHelper.GetXForwardedForIpAddress( null );

            Assert.That.IsNull( ipAddress );
        }

        /// <summary>
        /// Tests the <see cref="global::Rock.Utility.WebRequestHelper.GetXForwardedForIpAddress(HttpRequestBase)"/> method
        /// and verifies that it returns the IP address from the forwarded header.
        /// </summary>
        [TestMethod]
        public void GetXForwardedForIpAddress_SingleForwardedRequestReturnsIPv4()
        {
            var ipAddress = WebRequestHelper.GetXForwardedForIpAddress( "169.254.18.24" );

            Assert.That.AreEqual( "169.254.18.24", ipAddress );
        }

        /// <summary>
        /// Tests the <see cref="global::Rock.Utility.WebRequestHelper.GetXForwardedForIpAddress(HttpRequestBase)"/> method
        /// and verifies that it returns the IP address from the forwarded header.
        /// </summary>
        [TestMethod]
        public void GetXForwardedForIpAddress_SingleForwardedRequestReturnsIPv6()
        {
            var ipAddress = WebRequestHelper.GetXForwardedForIpAddress( "fe80::260:97ff:fe02:6ea5" );

            Assert.That.AreEqual( "fe80::260:97ff:fe02:6ea5", ipAddress );
        }

        /// <summary>
        /// Tests the <see cref="global::Rock.Utility.WebRequestHelper.GetXForwardedForIpAddress(HttpRequestBase)"/> method
        /// and verifies that it returns the IP address from the forwarded header.
        /// </summary>
        [TestMethod]
        public void GetXForwardedForIpAddress_MultiForwardedRequestReturnsFirstIp()
        {
            var ipAddress = WebRequestHelper.GetXForwardedForIpAddress( "169.254.18.24,169.254.212.7" );

            Assert.That.AreEqual( "169.254.18.24", ipAddress );
        }

        /// <summary>
        /// Tests the <see cref="global::Rock.Utility.WebRequestHelper.GetXForwardedForIpAddress(HttpRequestBase)"/> method
        /// and verifies that it returns the IP address from the forwarded header.
        /// </summary>
        [TestMethod]
        public void GetXForwardedForIpAddress_ForwardedRequestStripsIPv4Port()
        {
            var ipAddress = WebRequestHelper.GetXForwardedForIpAddress( "169.254.18.24:28372" );

            Assert.That.AreEqual( "169.254.18.24", ipAddress );
        }

        /// <summary>
        /// Tests the <see cref="global::Rock.Utility.WebRequestHelper.GetXForwardedForIpAddress(HttpRequestBase)"/> method
        /// and verifies that it returns the IP address from the forwarded header.
        /// </summary>
        [TestMethod]
        public void GetXForwardedForIpAddress_ForwardedRequestStripsIPv6Port()
        {
            var ipAddress = WebRequestHelper.GetXForwardedForIpAddress( "[fe80::260:97ff:fe02:6ea5]:28372" );

            Assert.That.AreEqual( "fe80::260:97ff:fe02:6ea5", ipAddress );
        }

        #endregion
    }
}
