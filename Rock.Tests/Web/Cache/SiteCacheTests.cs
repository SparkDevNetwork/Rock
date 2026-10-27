using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Web.Cache;

namespace Rock.Tests.Web.Cache
{
    /// <summary>
    /// This suite checks the SiteCache object to make sure that
    /// all logic works as intended.
    /// </summary>
    /// <seealso cref="SiteCache"/>
    [TestClass]
    public class SiteCacheTests
    {
        #region IsSafeRedirectUrl

        private static readonly Uri RequestUri = new Uri( "https://rock.church.org/page/3" );

        private const string OrganizationWebsite = "www.church.org";

        private const string PublicApplicationRoot = "https://app.church.org/";

        [TestMethod]
        public void IsSafeRedirectUrl_EmptyAttributeAllowsOrganizationWebsite()
        {
            var output = SiteCache.IsSafeRedirectUrl( "https://www.church.org/x", RequestUri, OrganizationWebsite, PublicApplicationRoot, new string[0] );
            Assert.IsTrue( output );
        }

        [TestMethod]
        public void IsSafeRedirectUrl_EmptyAttributeAllowsPublicApplicationRoot()
        {
            var output = SiteCache.IsSafeRedirectUrl( "https://app.church.org/page/1", RequestUri, OrganizationWebsite, PublicApplicationRoot, new string[0] );
            Assert.IsTrue( output );
        }

        [TestMethod]
        public void IsSafeRedirectUrl_EmptyAttributeRejectsOtherHosts()
        {
            var output = SiteCache.IsSafeRedirectUrl( "https://unsafe.com", RequestUri, OrganizationWebsite, PublicApplicationRoot, new string[0] );
            Assert.IsFalse( output );
        }

        [TestMethod]
        public void IsSafeRedirectUrl_AllowsRedirectDomain()
        {
            var output = SiteCache.IsSafeRedirectUrl( "https://giving.example.com/give", RequestUri, OrganizationWebsite, PublicApplicationRoot, new[] { "giving.example.com" } );
            Assert.IsTrue( output );
        }

        [TestMethod]
        public void IsSafeRedirectUrl_AllowsRequestHost()
        {
            var output = SiteCache.IsSafeRedirectUrl( "https://rock.church.org/page/1", RequestUri, null, null, new string[0] );
            Assert.IsTrue( output );
        }

        #endregion IsSafeRedirectUrl

        #region IsSiteDomain

        [TestMethod]
        public void IsSiteDomain_ExactMatchIgnoresCase()
        {
            var output = SiteCache.IsSiteDomain( "WWW.Church.org", new[] { "www.church.org" } );
            Assert.IsTrue( output );
        }

        [TestMethod]
        public void IsSiteDomain_StoredDomainWithSchemeAndSlashMatches()
        {
            var output = SiteCache.IsSiteDomain( "www.church.org", new[] { "https://www.church.org/" } );
            Assert.IsTrue( output );
        }

        [TestMethod]
        public void IsSiteDomain_LookalikeDoesNotMatch()
        {
            var output = SiteCache.IsSiteDomain( "evilchurch.org", new[] { "church.org" } );
            Assert.IsFalse( output );
        }

        [TestMethod]
        public void IsSiteDomain_WwwSubdomainOfBareDomainMatches()
        {
            var output = SiteCache.IsSiteDomain( "www.church.org", new[] { "church.org" } );
            Assert.IsTrue( output );
        }

        [TestMethod]
        public void IsSiteDomain_OtherSubdomainOfBareDomainMatches()
        {
            var output = SiteCache.IsSiteDomain( "es.church.org", new[] { "church.org" } );
            Assert.IsTrue( output );
        }

        [TestMethod]
        public void IsSiteDomain_BareNameOfWwwDomainMatches()
        {
            var output = SiteCache.IsSiteDomain( "church.org", new[] { "www.church.org" } );
            Assert.IsTrue( output );
        }

        [TestMethod]
        public void IsSiteDomain_BareNameOfOtherSubdomainDoesNotMatch()
        {
            var output = SiteCache.IsSiteDomain( "church.org", new[] { "es.church.org" } );
            Assert.IsFalse( output );
        }

        [TestMethod]
        public void IsSiteDomain_SuffixedHostDoesNotMatch()
        {
            var output = SiteCache.IsSiteDomain( "church.org.evil.test", new[] { "church.org" } );
            Assert.IsFalse( output );
        }

        [TestMethod]
        public void IsSiteDomain_EmptyHostDoesNotMatch()
        {
            var output = SiteCache.IsSiteDomain( "", new[] { "church.org" } );
            Assert.IsFalse( output );
        }

        [TestMethod]
        public void IsSiteDomain_WhitespaceHostDoesNotMatch()
        {
            var output = SiteCache.IsSiteDomain( "   ", new[] { "church.org" } );
            Assert.IsFalse( output );
        }

        [TestMethod]
        public void IsSiteDomain_NullDomainListDoesNotMatch()
        {
            var output = SiteCache.IsSiteDomain( "church.org", null );
            Assert.IsFalse( output );
        }

        [TestMethod]
        public void IsSiteDomain_LocalhostMatchesWithoutDomains()
        {
            var output = SiteCache.IsSiteDomain( "localhost", new string[0] );
            Assert.IsTrue( output );
        }

        [TestMethod]
        public void IsSiteDomain_LoopbackIPv4MatchesWithoutDomains()
        {
            var output = SiteCache.IsSiteDomain( "127.0.0.1", new string[0] );
            Assert.IsTrue( output );
        }

        [TestMethod]
        public void IsSiteDomain_LoopbackIPv6MatchesWithoutDomains()
        {
            var output = SiteCache.IsSiteDomain( "::1", new string[0] );
            Assert.IsTrue( output );
        }

        [TestMethod]
        public void IsSiteDomain_BracketedLoopbackIPv6MatchesWithoutDomains()
        {
            var output = SiteCache.IsSiteDomain( "[::1]", new string[0] );
            Assert.IsTrue( output );
        }

        [TestMethod]
        public void IsSiteDomain_OtherIPDoesNotMatch()
        {
            var output = SiteCache.IsSiteDomain( "10.0.0.5", new[] { "www.church.org" } );
            Assert.IsFalse( output );
        }

        [TestMethod]
        public void IsSiteDomain_WeglotSiblingOfWwwDomainMatches()
        {
            var output = SiteCache.IsSiteDomain( "es.church.org", new[] { "www.church.org" } );
            Assert.IsTrue( output );
        }

        [TestMethod]
        public void IsSiteDomain_InternalSiblingOfWwwDomainMatches()
        {
            var output = SiteCache.IsSiteDomain( "rock.church.org", new[] { "www.church.org" } );
            Assert.IsTrue( output );
        }

        [TestMethod]
        public void IsSiteDomain_LookalikeOfWwwDomainDoesNotMatch()
        {
            var output = SiteCache.IsSiteDomain( "evil-church.org", new[] { "www.church.org" } );
            Assert.IsFalse( output );
        }

        [TestMethod]
        public void IsSiteDomain_LookalikeWithoutDotOfWwwDomainDoesNotMatch()
        {
            var output = SiteCache.IsSiteDomain( "evilchurch.org", new[] { "www.church.org" } );
            Assert.IsFalse( output );
        }

        [TestMethod]
        public void IsSiteDomain_SuffixedHostOfWwwDomainDoesNotMatch()
        {
            var output = SiteCache.IsSiteDomain( "church.org.evil.test", new[] { "www.church.org" } );
            Assert.IsFalse( output );
        }

        [TestMethod]
        public void IsSiteDomain_WwwPrefixedLookalikeDoesNotMatch()
        {
            var output = SiteCache.IsSiteDomain( "wwwchurch.org", new[] { "www.church.org" } );
            Assert.IsFalse( output );
        }

        [TestMethod]
        public void IsSiteDomain_WwwTopLevelDomainDoesNotAllowSiblings()
        {
            var output = SiteCache.IsSiteDomain( "evil.org", new[] { "www.org" } );
            Assert.IsFalse( output );
        }

        [TestMethod]
        public void IsSiteDomain_SlashDelimiterInHostDoesNotMatch()
        {
            var output = SiteCache.IsSiteDomain( "evil.test/x.church.org", new[] { "church.org" } );
            Assert.IsFalse( output );
        }

        [TestMethod]
        public void IsSiteDomain_HashDelimiterInHostDoesNotMatch()
        {
            var output = SiteCache.IsSiteDomain( "evil.test#.church.org", new[] { "www.church.org" } );
            Assert.IsFalse( output );
        }

        [TestMethod]
        public void IsSiteDomain_QueryDelimiterInHostDoesNotMatch()
        {
            var output = SiteCache.IsSiteDomain( "evil.test?.church.org", new[] { "www.church.org" } );
            Assert.IsFalse( output );
        }

        [TestMethod]
        public void IsSiteDomain_SlashDelimiterBeforeWwwDomainDoesNotMatch()
        {
            var output = SiteCache.IsSiteDomain( "evil.test/.www.church.org", new[] { "www.church.org" } );
            Assert.IsFalse( output );
        }

        [TestMethod]
        public void IsSiteDomain_HostWithPortDoesNotMatch()
        {
            var output = SiteCache.IsSiteDomain( "www.church.org:8080", new[] { "www.church.org" } );
            Assert.IsFalse( output );
        }

        [DataTestMethod]
        [DataRow( "evil.test\uFF0Fx.church.org" )]
        [DataRow( "evil.test\u2215x.church.org" )]
        [DataRow( "evil.test\uFF03x.church.org" )]
        [DataRow( "evil.test\uFF1Fx.church.org" )]
        public void IsSiteDomain_LookalikeDelimiterInHostDoesNotMatch( string host )
        {
            var output = SiteCache.IsSiteDomain( host, new[] { "church.org" } );
            Assert.IsFalse( output );
        }

        [TestMethod]
        public void IsSiteDomain_InternationalizedDomainMatches()
        {
            var output = SiteCache.IsSiteDomain( "b\u00FCcher.org", new[] { "b\u00FCcher.org" } );
            Assert.IsTrue( output );
        }

        [TestMethod]
        public void IsSiteDomain_HostWithUnderscoreMatches()
        {
            var output = SiteCache.IsSiteDomain( "rock_server", new[] { "rock_server" } );
            Assert.IsTrue( output );
        }

        [TestMethod]
        public void IsSiteDomain_IPAddressDomainMatchesExactly()
        {
            var output = SiteCache.IsSiteDomain( "10.0.0.5", new[] { "10.0.0.5" } );
            Assert.IsTrue( output );
        }

        [TestMethod]
        public void IsSiteDomain_IPAddressDomainIsNotASuffix()
        {
            var output = SiteCache.IsSiteDomain( "1.10.0.0.5", new[] { "10.0.0.5" } );
            Assert.IsFalse( output );
        }

        #endregion IsSiteDomain
    }
}
