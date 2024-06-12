using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Net;
using Rock.Tests.Shared;

namespace Rock.Tests.Utility.ExtensionMethods
{
    [TestClass]
    public class RequestExtensionsTests
    {
        #region UrlProxySafe

        /// <summary>
        /// Tests the <see cref="global::Rock.ExtensionMethods.UrlProxySafe(IRequest)"/> method
        /// and verifies that it returns original URL when no headers are present.
        /// </summary>
        [TestMethod]
        public void UrlProxySafe_NonForwardedRequestReturnsOriginalUrl()
        {
            var expectedUrl = "https://www.rocksolidchurchdemo.com/page/12";

            var request = new Request( "https://www.rocksolidchurchdemo.com/page/12" );
            var actualUri = request.UrlProxySafe();

            Assert.That.AreEqual( expectedUrl, actualUri.ToString() );
        }

        /// <summary>
        /// Tests the <see cref="global::Rock.ExtensionMethods.UrlProxySafe(IRequest)"/> method
        /// and verifies that it returns forwarded host name when header is present.
        /// </summary>
        [TestMethod]
        public void UrlProxySafe_XForwardedHostRequestReturnsForwardedUrl()
        {
            var request = new Request( "https://rock.proxy.me/page/12" );
            var expectedUrl = "https://www.rocksolidchurchdemo.com/page/12";

            request.Headers.Add( "X-FORWARDED-HOST", "www.rocksolidchurchdemo.com" );
            request.Headers.Add( "X-FORWARDED-PROTO", "https" );

            var actualUri = request.UrlProxySafe();

            Assert.That.AreEqual( expectedUrl, actualUri.ToString() );
        }

        /// <summary>
        /// Tests the <see cref="global::Rock.ExtensionMethods.UrlProxySafe(IRequest)"/> method
        /// and verifies that it returns forwarded host name when header is present.
        /// </summary>
        [TestMethod]
        public void UrlProxySafe_XForwardedProtoRequestReturnsForwardedProto()
        {
            var request = new Request( "https://rock.proxy.me/page/12" );
            var expectedUrl = "http://www.rocksolidchurchdemo.com/page/12";

            request.Headers.Add( "X-FORWARDED-HOST", "www.rocksolidchurchdemo.com" );
            request.Headers.Add( "X-FORWARDED-PROTO", "http" );

            var actualUri = request.UrlProxySafe();

            Assert.That.AreEqual( expectedUrl, actualUri.ToString() );
        }

        /// <summary>
        /// Tests the <see cref="global::Rock.ExtensionMethods.UrlProxySafe(IRequest)"/> method
        /// and verifies that it returns forwarded host name when header is present.
        /// </summary>
        [TestMethod]
        public void UrlProxySafe_XOriginalHostRequestReturnsForwardedUrl()
        {
            var request = new Request( "https://rock.proxy.me/page/12" );
            var expectedUrl = "https://www.rocksolidchurchdemo.com/page/12";

            request.Headers.Add( "X-ORIGINAL-HOST", "www.rocksolidchurchdemo.com" );
            request.Headers.Add( "X-FORWARDED-PROTO", "https" );

            var actualUri = request.UrlProxySafe();

            Assert.That.AreEqual( expectedUrl, actualUri.ToString() );
        }

        /// <summary>
        /// Tests the <see cref="global::Rock.ExtensionMethods.UrlProxySafe(IRequest)"/> method
        /// and verifies that it returns a concise URL without the port number in the string
        /// when port 80 is used.
        /// </summary>
        [TestMethod]
        public void UrlProxySafe_XForwardedPort80RequestReturnsConciseUrl()
        {
            var request = new Request( "https://rock.proxy.me/page/12" );
            var expectedUrl = "http://www.rocksolidchurchdemo.com/page/12";

            request.Headers.Add( "X-ORIGINAL-HOST", "www.rocksolidchurchdemo.com" );
            request.Headers.Add( "X-FORWARDED-PROTO", "http" );
            request.Headers.Add( "X-FORWARDED-PORT", "80" );

            var actualUri = request.UrlProxySafe();

            Assert.That.AreEqual( expectedUrl, actualUri.ToString() );
        }

        /// <summary>
        /// Tests the <see cref="global::Rock.ExtensionMethods.UrlProxySafe(IRequest)"/> method
        /// and verifies that it returns a concise URL without the port number in the string
        /// when port 443 is used.
        /// </summary>
        [TestMethod]
        public void UrlProxySafe_XForwardedPort443RequestReturnsConciseUrl()
        {
            var request = new Request( "https://rock.proxy.me/page/12" );
            var expectedUrl = "https://www.rocksolidchurchdemo.com/page/12";

            request.Headers.Add( "X-ORIGINAL-HOST", "www.rocksolidchurchdemo.com" );
            request.Headers.Add( "X-FORWARDED-PROTO", "https" );
            request.Headers.Add( "X-FORWARDED-PORT", "443" );

            var actualUri = request.UrlProxySafe();

            Assert.That.AreEqual( expectedUrl, actualUri.ToString() );
        }

        /// <summary>
        /// Tests the <see cref="global::Rock.ExtensionMethods.UrlProxySafe(IRequest)"/> method
        /// and verifies that it returns a concise URL without the port number in the string
        /// when port 443 is used.
        /// </summary>
        [TestMethod]
        public void UrlProxySafe_XForwardedPort8080RequestReturnsFullPortUrl()
        {
            var request = new Request( "https://rock.proxy.me/page/12" );
            var expectedUrl = "http://www.rocksolidchurchdemo.com:8080/page/12";

            request.Headers.Add( "X-ORIGINAL-HOST", "www.rocksolidchurchdemo.com" );
            request.Headers.Add( "X-FORWARDED-PROTO", "http" );
            request.Headers.Add( "X-FORWARDED-PORT", "8080" );

            var actualUri = request.UrlProxySafe();

            Assert.That.AreEqual( expectedUrl, actualUri.ToString() );
        }

        /// <summary>
        /// Tests the <see cref="global::Rock.ExtensionMethods.UrlProxySafe(IRequest)"/> method
        /// and verifies that it returns a concise URL without the port number in the string
        /// when port 443 is used.
        /// </summary>
        [TestMethod]
        public void UrlProxySafe_XForwardedPort443RequestWithCustomPortReturnsConciseUrl()
        {
            var request = new Request( "https://rock.proxy.me:8443/page/12" );
            var expectedUrl = "https://www.rocksolidchurchdemo.com/page/12";

            request.Headers.Add( "X-ORIGINAL-HOST", "www.rocksolidchurchdemo.com" );
            request.Headers.Add( "X-FORWARDED-PROTO", "https" );
            request.Headers.Add( "X-FORWARDED-PORT", "443" );

            var actualUri = request.UrlProxySafe();

            Assert.That.AreEqual( expectedUrl, actualUri.ToString() );
        }

        private class Request : IRequest
        {
            public IPAddress RemoteAddress => throw new NotImplementedException();

            public Uri RequestUri { get; }

            public NameValueCollection QueryString => throw new NotImplementedException();

            public NameValueCollection Headers { get; } = new NameValueCollection( StringComparer.OrdinalIgnoreCase );

            public IDictionary<string, string> Cookies => throw new NotImplementedException();

            public string Method => throw new NotImplementedException();

            public Request( string uri )
            {
                RequestUri = new Uri( uri );
            }
        }

        #endregion
    }
}
