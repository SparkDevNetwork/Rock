using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Net;
using Rock.Tests.Shared;
using Rock.Web.Cache;

namespace Rock.Tests.Utility.ExtensionMethods
{
    [TestClass]
    public class RequestExtensionsTests
    {
        #region UrlProxySafe

        /// <summary>
        /// The site domains configured for the tests that go through GetProxySafeUri.
        /// </summary>
        private static readonly string[] SiteDomains = { "www.rocksolidchurchdemo.com" };

        /// <summary>
        /// The PublicApplicationRoot used by the tests that go through GetProxySafeUri.
        /// </summary>
        private const string PublicApplicationRoot = "https://www.rocksolidchurchdemo.com/";

        /// <summary>
        /// Builds the proxy safe URL of the test request with the test site domains
        /// and the given PublicApplicationRoot.
        /// </summary>
        private static Uri GetProxySafeUri( Request request, string publicApplicationRoot = PublicApplicationRoot )
        {
            return global::Rock.ExtensionMethods.GetProxySafeUri( request.RequestUri, request.Headers, host => SiteCache.IsSiteDomain( host, SiteDomains ), () => publicApplicationRoot );
        }

        /// <summary>
        /// Tests the <see cref="global::Rock.ExtensionMethods.UrlProxySafe(IRequest)"/> method
        /// and verifies that it returns original URL when no headers are present.
        /// </summary>
        [TestMethod]
        public void UrlProxySafe_NonForwardedRequestReturnsOriginalUrl()
        {
            var expectedUrl = "https://www.rocksolidchurchdemo.com/page/12";

            var request = new Request( "https://www.rocksolidchurchdemo.com/page/12" );
            var actualUri = GetProxySafeUri( request );

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

            var actualUri = GetProxySafeUri( request );

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

            var actualUri = GetProxySafeUri( request );

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

            var actualUri = GetProxySafeUri( request );

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

            var actualUri = GetProxySafeUri( request );

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

            var actualUri = GetProxySafeUri( request );

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

            var actualUri = GetProxySafeUri( request );

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

            var actualUri = GetProxySafeUri( request );

            Assert.That.AreEqual( expectedUrl, actualUri.ToString() );
        }

        /// <summary>
        /// Verifies that a forwarded host that is not a site domain is replaced by the
        /// PublicApplicationRoot host while the path and query are kept.
        /// </summary>
        [TestMethod]
        public void UrlProxySafe_ForwardedHostNotSiteDomainUsesPublicApplicationRoot()
        {
            var request = new Request( "https://rock.proxy.me/page/12?a=1" );
            request.Headers.Add( "X-Forwarded-Host", "th082-marker.invalid" );
            request.Headers.Add( "X-Forwarded-Proto", "https" );

            var actualUri = GetProxySafeUri( request );

            Assert.AreEqual( "https://www.rocksolidchurchdemo.com/page/12?a=1", actualUri.ToString() );
        }

        /// <summary>
        /// Verifies that an X-Original-Host that is not a site domain is not used.
        /// </summary>
        [TestMethod]
        public void UrlProxySafe_OriginalHostNotSiteDomainUsesPublicApplicationRoot()
        {
            var request = new Request( "https://rock.proxy.me/page/12?a=1" );
            request.Headers.Add( "X-Original-Host", "th082-marker.invalid" );
            request.Headers.Add( "X-Forwarded-Proto", "https" );

            var actualUri = GetProxySafeUri( request );

            Assert.AreEqual( "https://www.rocksolidchurchdemo.com/page/12?a=1", actualUri.ToString() );
        }

        /// <summary>
        /// Verifies that a weglot-forwarded-host that is not a site domain is not used.
        /// </summary>
        [TestMethod]
        public void UrlProxySafe_WeglotHostNotSiteDomainUsesPublicApplicationRoot()
        {
            var request = new Request( "https://rock.proxy.me/page/12?a=1" );
            request.Headers.Add( "weglot-forwarded-host", "th082-marker.invalid" );
            request.Headers.Add( "X-Forwarded-Proto", "https" );

            var actualUri = GetProxySafeUri( request );

            Assert.AreEqual( "https://www.rocksolidchurchdemo.com/page/12?a=1", actualUri.ToString() );
        }

        /// <summary>
        /// Verifies that the request's own host is not used when it is not a site domain.
        /// </summary>
        [TestMethod]
        public void UrlProxySafe_RequestHostNotSiteDomainUsesPublicApplicationRoot()
        {
            var request = new Request( "http://10.0.0.5/page/12" );

            var actualUri = GetProxySafeUri( request );

            Assert.AreEqual( "https://www.rocksolidchurchdemo.com/page/12", actualUri.ToString() );
        }

        /// <summary>
        /// Verifies that a request on a site domain keeps its own port.
        /// </summary>
        [TestMethod]
        public void UrlProxySafe_RequestHostSiteDomainKeepsPort()
        {
            var request = new Request( "http://www.rocksolidchurchdemo.com:8080/page/12" );

            var actualUri = GetProxySafeUri( request );

            Assert.AreEqual( "http://www.rocksolidchurchdemo.com:8080/page/12", actualUri.ToString() );
        }

        /// <summary>
        /// Verifies that the site domain match ignores case.
        /// </summary>
        [TestMethod]
        public void UrlProxySafe_SiteDomainMatchIgnoresCase()
        {
            var request = new Request( "https://rock.proxy.me/page/12" );
            request.Headers.Add( "X-Forwarded-Host", "WWW.RockSolidChurchDemo.com" );
            request.Headers.Add( "X-Forwarded-Proto", "https" );

            var actualUri = GetProxySafeUri( request );

            Assert.AreEqual( "https://www.rocksolidchurchdemo.com/page/12", actualUri.ToString() );
        }

        /// <summary>
        /// Verifies that the PublicApplicationRoot port is kept when falling back to it.
        /// </summary>
        [TestMethod]
        public void UrlProxySafe_PublicApplicationRootPortIsKept()
        {
            var request = new Request( "https://th082-marker.invalid/page/12" );

            var actualUri = GetProxySafeUri( request, "http://localhost:6229/" );

            Assert.AreEqual( "http://localhost:6229/page/12", actualUri.ToString() );
        }

        /// <summary>
        /// Verifies that a blank PublicApplicationRoot returns the URL the server received
        /// and never falls back to the forwarded host.
        /// </summary>
        [TestMethod]
        public void UrlProxySafe_BlankPublicApplicationRootReturnsRequestUrl()
        {
            var request = new Request( "https://rock.proxy.me/page/12" );
            request.Headers.Add( "X-Forwarded-Host", "th082-marker.invalid" );
            request.Headers.Add( "X-Forwarded-Proto", "https" );

            var actualUri = GetProxySafeUri( request, "" );

            Assert.AreEqual( "https://rock.proxy.me/page/12", actualUri.ToString() );
        }

        /// <summary>
        /// Verifies that an invalid PublicApplicationRoot returns the URL the server received
        /// and never falls back to the forwarded host.
        /// </summary>
        [TestMethod]
        public void UrlProxySafe_InvalidPublicApplicationRootReturnsRequestUrl()
        {
            var request = new Request( "https://rock.proxy.me/page/12" );
            request.Headers.Add( "X-Forwarded-Host", "th082-marker.invalid" );
            request.Headers.Add( "X-Forwarded-Proto", "https" );

            var actualUri = GetProxySafeUri( request, "not a url" );

            Assert.AreEqual( "https://rock.proxy.me/page/12", actualUri.ToString() );
        }

        /// <summary>
        /// Verifies that a request on localhost keeps its own URL even when the
        /// PublicApplicationRoot is a production host.
        /// </summary>
        [TestMethod]
        public void UrlProxySafe_LoopbackRequestHostKeepsUrl()
        {
            var request = new Request( "http://localhost:6229/page/12" );

            var actualUri = GetProxySafeUri( request, "https://www.church.org/" );

            Assert.AreEqual( "http://localhost:6229/page/12", actualUri.ToString() );
        }

        /// <summary>
        /// Verifies that a forwarded sibling host of a listed www domain (a Weglot
        /// language host) is kept.
        /// </summary>
        [TestMethod]
        public void UrlProxySafe_WeglotSiblingOfWwwDomainIsKept()
        {
            var request = new Request( "https://rock.proxy.me/page/12" );
            request.Headers.Add( "weglot-forwarded-host", "es.rocksolidchurchdemo.com" );
            request.Headers.Add( "X-Forwarded-Proto", "https" );

            var actualUri = GetProxySafeUri( request );

            Assert.AreEqual( "https://es.rocksolidchurchdemo.com/page/12", actualUri.ToString() );
        }

        /// <summary>
        /// Verifies that a forwarded host that only ends with the bare domain name
        /// (no dot before it) is not treated as a sibling of the www domain.
        /// </summary>
        [TestMethod]
        public void UrlProxySafe_LookalikeOfWwwDomainUsesPublicApplicationRoot()
        {
            var request = new Request( "https://rock.proxy.me/page/12" );
            request.Headers.Add( "X-Forwarded-Host", "evilrocksolidchurchdemo.com" );
            request.Headers.Add( "X-Forwarded-Proto", "https" );

            var actualUri = GetProxySafeUri( request, "https://app.example.org/" );

            Assert.AreEqual( "https://app.example.org/page/12", actualUri.ToString() );
        }

        /// <summary>
        /// Verifies that a forwarded host containing a URL delimiter ("/", "#" or "?") never passes
        /// the site domain check, even though it ends with a listed domain, and the
        /// PublicApplicationRoot host is used instead.
        /// </summary>
        [DataTestMethod]
        [DataRow( "evil.test/x.www.rocksolidchurchdemo.com" )]
        [DataRow( "evil.test#.rocksolidchurchdemo.com" )]
        [DataRow( "evil.test?.rocksolidchurchdemo.com" )]
        [DataRow( "evil.test/.www.rocksolidchurchdemo.com" )]
        public void UrlProxySafe_ForwardedHostWithDelimiterUsesPublicApplicationRoot( string forwardedHost )
        {
            var request = new Request( "http://origin.internal/page/123?code=SECRET" );
            request.Headers.Add( "X-Forwarded-Host", forwardedHost );
            request.Headers.Add( "X-Forwarded-Proto", "https" );

            var actualUri = GetProxySafeUri( request, "https://app.example.org/" );

            Assert.AreEqual( "https://app.example.org/page/123?code=SECRET", actualUri.ToString() );
            Assert.AreNotEqual( "evil.test", actualUri.Host );
        }

        /// <summary>
        /// Verifies that an X-Forwarded-Proto other than http or https is not used and the
        /// PublicApplicationRoot is used instead, even on a site domain.
        /// </summary>
        [DataTestMethod]
        [DataRow( "javascript" )]
        [DataRow( "ftp" )]
        public void UrlProxySafe_ForwardedProtoNotHttpUsesPublicApplicationRoot( string forwardedProto )
        {
            var request = new Request( "http://origin.internal/page/12" );
            request.Headers.Add( "X-Forwarded-Host", "www.rocksolidchurchdemo.com" );
            request.Headers.Add( "X-Forwarded-Proto", forwardedProto );

            var actualUri = GetProxySafeUri( request, "https://app.example.org/" );

            Assert.AreEqual( "https://app.example.org/page/12", actualUri.ToString() );
        }

        /// <summary>
        /// Verifies that X-Forwarded-Proto is accepted regardless of case and surrounding whitespace.
        /// </summary>
        [TestMethod]
        public void UrlProxySafe_ForwardedProtoIgnoresCaseAndWhitespace()
        {
            var request = new Request( "http://origin.internal/page/12" );
            request.Headers.Add( "X-Forwarded-Host", "www.rocksolidchurchdemo.com" );
            request.Headers.Add( "X-Forwarded-Proto", " HTTPS " );

            var actualUri = GetProxySafeUri( request, "https://app.example.org/" );

            Assert.AreEqual( "https://www.rocksolidchurchdemo.com/page/12", actualUri.ToString() );
        }

        /// <summary>
        /// Verifies that a forwarded IPv6 loopback host is kept even though the Uri class
        /// writes the address in a different form.
        /// </summary>
        [TestMethod]
        public void UrlProxySafe_ForwardedIPv6LoopbackIsKept()
        {
            var request = new Request( "http://origin.internal/page/12" );
            request.Headers.Add( "X-Forwarded-Host", "::1" );
            request.Headers.Add( "X-Forwarded-Proto", "http" );

            var actualUri = GetProxySafeUri( request, "https://app.example.org/" );

            Assert.IsTrue( actualUri.IsLoopback );
            Assert.AreEqual( "/page/12", actualUri.PathAndQuery );
        }

        /// <summary>
        /// Verifies that a forwarded host containing a look-alike delimiter (fullwidth slash,
        /// division slash, fullwidth number sign or fullwidth question mark) is not used and the
        /// PublicApplicationRoot host is used instead.
        /// </summary>
        [DataTestMethod]
        [DataRow( "evil.test\uFF0Fx.www.rocksolidchurchdemo.com" )]
        [DataRow( "evil.test\u2215x.www.rocksolidchurchdemo.com" )]
        [DataRow( "evil.test\uFF03x.www.rocksolidchurchdemo.com" )]
        [DataRow( "evil.test\uFF1Fx.www.rocksolidchurchdemo.com" )]
        public void UrlProxySafe_ForwardedHostWithLookalikeDelimiterUsesPublicApplicationRoot( string forwardedHost )
        {
            var request = new Request( "http://origin.internal/page/123?code=SECRET" );
            request.Headers.Add( "X-Forwarded-Host", forwardedHost );
            request.Headers.Add( "X-Forwarded-Proto", "https" );

            var actualUri = GetProxySafeUri( request, "https://app.example.org/" );

            Assert.AreEqual( "https://app.example.org/page/123?code=SECRET", actualUri.ToString() );
        }

        /// <summary>
        /// Verifies that look-alike delimiter characters are not plain host names.
        /// </summary>
        [DataTestMethod]
        [DataRow( "evil.test\uFF0Fx.church.org" )]
        [DataRow( "evil.test\u2215x.church.org" )]
        [DataRow( "evil.test\uFF03x.church.org" )]
        [DataRow( "evil.test\uFF1Fx.church.org" )]
        public void IsPlainHostName_LookalikeDelimiterIsRejected( string host )
        {
            Assert.IsFalse( global::Rock.ExtensionMethods.IsPlainHostName( host ) );
        }

        /// <summary>
        /// Verifies that normal ASCII names, internationalized names, names with an underscore
        /// (which passed before the look-alike check was added) and IP addresses are plain host names.
        /// </summary>
        [DataTestMethod]
        [DataRow( "www.church.org" )]
        [DataRow( "localhost" )]
        [DataRow( "b\u00FCcher.org" )]
        [DataRow( "rock_server" )]
        [DataRow( "10.0.0.5" )]
        [DataRow( "::1" )]
        [DataRow( "[::1]" )]
        public void IsPlainHostName_ValidHostIsAccepted( string host )
        {
            Assert.IsTrue( global::Rock.ExtensionMethods.IsPlainHostName( host ) );
        }

        /// <summary>
        /// Verifies through the real <see cref="global::Rock.ExtensionMethods.UrlProxySafe(IRequest)"/>
        /// that a client-sent X-Forwarded-Host is never used when it cannot be confirmed as a site domain.
        /// </summary>
        [TestMethod]
        public void UrlProxySafe_UnconfirmedForwardedHostIsNotUsed()
        {
            var request = new Request( "https://rock.proxy.me/page/12" );
            request.Headers.Add( "X-Forwarded-Host", "th082-marker.invalid" );
            request.Headers.Add( "X-Forwarded-Proto", "https" );

            var actualUri = request.UrlProxySafe();

            Assert.AreNotEqual( "th082-marker.invalid", actualUri.Host );
        }

        /// <summary>
        /// Same as <see cref="UrlProxySafe_UnconfirmedForwardedHostIsNotUsed"/> for X-Original-Host.
        /// </summary>
        [TestMethod]
        public void UrlProxySafe_UnconfirmedOriginalHostIsNotUsed()
        {
            var request = new Request( "https://rock.proxy.me/page/12" );
            request.Headers.Add( "X-Original-Host", "th082-marker.invalid" );
            request.Headers.Add( "X-Forwarded-Proto", "https" );

            var actualUri = request.UrlProxySafe();

            Assert.AreNotEqual( "th082-marker.invalid", actualUri.Host );
        }

        /// <summary>
        /// Same as <see cref="UrlProxySafe_UnconfirmedForwardedHostIsNotUsed"/> for weglot-forwarded-host.
        /// </summary>
        [TestMethod]
        public void UrlProxySafe_UnconfirmedWeglotHostIsNotUsed()
        {
            var request = new Request( "https://rock.proxy.me/page/12" );
            request.Headers.Add( "weglot-forwarded-host", "th082-marker.invalid" );
            request.Headers.Add( "X-Forwarded-Proto", "https" );

            var actualUri = request.UrlProxySafe();

            Assert.AreNotEqual( "th082-marker.invalid", actualUri.Host );
        }

        #endregion

        #region GetForwardedRequestUri

        /// <summary>
        /// Verifies that the unvalidated helper keeps the old behavior: the request URL when not forwarded.
        /// </summary>
        [TestMethod]
        public void GetForwardedRequestUri_NonForwardedRequestReturnsOriginalUrl()
        {
            var request = new Request( "http://10.0.0.5/page/12" );

            var actualUri = global::Rock.ExtensionMethods.GetForwardedRequestUri( request.RequestUri, request.Headers );

            Assert.AreEqual( "http://10.0.0.5/page/12", actualUri.ToString() );
        }

        /// <summary>
        /// Verifies that the unvalidated helper keeps the old behavior: any forwarded host is used.
        /// </summary>
        [TestMethod]
        public void GetForwardedRequestUri_XForwardedHostRequestReturnsForwardedUrl()
        {
            var request = new Request( "https://rock.proxy.me/page/12" );
            request.Headers.Add( "X-FORWARDED-HOST", "unlisted.example.org" );
            request.Headers.Add( "X-FORWARDED-PROTO", "http" );

            var actualUri = global::Rock.ExtensionMethods.GetForwardedRequestUri( request.RequestUri, request.Headers );

            Assert.AreEqual( "http://unlisted.example.org/page/12", actualUri.ToString() );
        }

        /// <summary>
        /// Verifies that the unvalidated helper keeps the old behavior for X-Original-Host and ports.
        /// </summary>
        [TestMethod]
        public void GetForwardedRequestUri_XOriginalHostWithPortReturnsFullPortUrl()
        {
            var request = new Request( "https://rock.proxy.me:8443/page/12" );
            request.Headers.Add( "X-ORIGINAL-HOST", "www.rocksolidchurchdemo.com" );
            request.Headers.Add( "X-FORWARDED-PROTO", "http" );
            request.Headers.Add( "X-FORWARDED-PORT", "8080" );

            var actualUri = global::Rock.ExtensionMethods.GetForwardedRequestUri( request.RequestUri, request.Headers );

            Assert.AreEqual( "http://www.rocksolidchurchdemo.com:8080/page/12", actualUri.ToString() );
        }

        /// <summary>
        /// Verifies that the unvalidated helper keeps the old behavior for weglot-forwarded-host
        /// and resets the port when no port header is sent.
        /// </summary>
        [TestMethod]
        public void GetForwardedRequestUri_WeglotHostWithoutPortReturnsConciseUrl()
        {
            var request = new Request( "https://rock.proxy.me:8443/page/12" );
            request.Headers.Add( "weglot-forwarded-host", "es.example.org" );
            request.Headers.Add( "X-FORWARDED-PROTO", "https" );

            var actualUri = global::Rock.ExtensionMethods.GetForwardedRequestUri( request.RequestUri, request.Headers );

            Assert.AreEqual( "https://es.example.org/page/12", actualUri.ToString() );
        }

        /// <summary>
        /// Verifies that the unvalidated helper ignores a forwarded host when no X-Forwarded-Proto is sent.
        /// </summary>
        [TestMethod]
        public void GetForwardedRequestUri_ForwardedHostWithoutProtoReturnsOriginalUrl()
        {
            var request = new Request( "https://rock.proxy.me/page/12" );
            request.Headers.Add( "X-FORWARDED-HOST", "unlisted.example.org" );

            var actualUri = global::Rock.ExtensionMethods.GetForwardedRequestUri( request.RequestUri, request.Headers );

            Assert.AreEqual( "https://rock.proxy.me/page/12", actualUri.ToString() );
        }

        private class Request : IRequest
        {
            public IPAddress RemoteAddress => throw new NotImplementedException();

            public Uri RequestUri { get; }

            public NameValueCollection QueryString => throw new NotImplementedException();

            public IDictionary<string, object> RouteData => throw new NotImplementedException();

            public NameValueCollection Headers { get; } = new NameValueCollection( StringComparer.OrdinalIgnoreCase );

            public IDictionary<string, string> Cookies => throw new NotImplementedException();

            public string Method => throw new NotImplementedException();

            public bool CookiesValuesAreUrlDecoded => throw new NotImplementedException();

            public Request( string uri )
            {
                RequestUri = new Uri( uri );
            }
        }

        #endregion
    }
}
