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
            var ipAddress = WebRequestHelper.GetXForwardedForIpAddress( "10.18.20.1" );

            Assert.That.AreEqual( "10.18.20.1", ipAddress );
        }

        /// <summary>
        /// Tests the <see cref="global::Rock.Utility.WebRequestHelper.GetXForwardedForIpAddress(HttpRequestBase)"/> method
        /// and verifies that it returns the IP address from the forwarded header.
        /// </summary>
        [TestMethod]
        public void GetXForwardedForIpAddress_SingleForwardedRequestReturnsIPv6()
        {
            var ipAddress = WebRequestHelper.GetXForwardedForIpAddress( "2001:4860:4860::8888" );

            Assert.That.AreEqual( "2001:4860:4860::8888", ipAddress );
        }

        /// <summary>
        /// Tests the <see cref="global::Rock.Utility.WebRequestHelper.GetXForwardedForIpAddress(HttpRequestBase)"/> method
        /// and verifies that it returns the IP address from the forwarded header.
        /// </summary>
        [TestMethod]
        public void GetXForwardedForIpAddress_MultiForwardedRequestReturnsFirstIp()
        {
            var ipAddress = WebRequestHelper.GetXForwardedForIpAddress( "10.18.20.1,10.203.14.7" );

            Assert.That.AreEqual( "10.18.20.1", ipAddress );
        }

        /// <summary>
        /// Tests the <see cref="global::Rock.Utility.WebRequestHelper.GetXForwardedForIpAddress(HttpRequestBase)"/> method
        /// and verifies that it returns the IP address from the forwarded header.
        /// </summary>
        [TestMethod]
        public void GetXForwardedForIpAddress_ForwardedRequestStripsIPv4Port()
        {
            var ipAddress = WebRequestHelper.GetXForwardedForIpAddress( "10.18.20.1:28372" );

            Assert.That.AreEqual( "10.18.20.1", ipAddress );
        }

        /// <summary>
        /// Tests the <see cref="global::Rock.Utility.WebRequestHelper.GetXForwardedForIpAddress(HttpRequestBase)"/> method
        /// and verifies that it returns the IP address from the forwarded header.
        /// </summary>
        [TestMethod]
        public void GetXForwardedForIpAddress_ForwardedRequestStripsIPv6Port()
        {
            var ipAddress = WebRequestHelper.GetXForwardedForIpAddress( "[2001:4860:4860::8888]:28372" );

            Assert.That.AreEqual( "2001:4860:4860::8888", ipAddress );
        }

        #endregion
    }
}
