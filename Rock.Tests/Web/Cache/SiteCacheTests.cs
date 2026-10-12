using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Tests.Shared;
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
            Assert.That.AreEqual( true, output );
        }

        [TestMethod]
        public void IsSafeRedirectUrl_EmptyAttributeAllowsPublicApplicationRoot()
        {
            var output = SiteCache.IsSafeRedirectUrl( "https://app.church.org/page/1", RequestUri, OrganizationWebsite, PublicApplicationRoot, new string[0] );
            Assert.That.AreEqual( true, output );
        }

        [TestMethod]
        public void IsSafeRedirectUrl_EmptyAttributeRejectsOtherHosts()
        {
            var output = SiteCache.IsSafeRedirectUrl( "https://unsafe.com", RequestUri, OrganizationWebsite, PublicApplicationRoot, new string[0] );
            Assert.That.AreEqual( false, output );
        }

        [TestMethod]
        public void IsSafeRedirectUrl_AllowsRedirectDomain()
        {
            var output = SiteCache.IsSafeRedirectUrl( "https://giving.example.com/give", RequestUri, OrganizationWebsite, PublicApplicationRoot, new[] { "giving.example.com" } );
            Assert.That.AreEqual( true, output );
        }

        [TestMethod]
        public void IsSafeRedirectUrl_AllowsRequestHost()
        {
            var output = SiteCache.IsSafeRedirectUrl( "https://rock.church.org/page/1", RequestUri, null, null, new string[0] );
            Assert.That.AreEqual( true, output );
        }

        #endregion IsSafeRedirectUrl

        #region IsHostAllowed

        private static readonly string[] SiteDomains = new[] { "www.church.org", "rock.church.org" };

        [TestMethod]
        public void IsHostAllowed_AllowsSiteDomain()
        {
            var output = SiteCache.IsHostAllowed( "rock.church.org", SiteDomains );
            Assert.That.AreEqual( true, output );
        }

        [TestMethod]
        public void IsHostAllowed_IgnoresCase()
        {
            var output = SiteCache.IsHostAllowed( "ROCK.Church.org", SiteDomains );
            Assert.That.AreEqual( true, output );
        }

        [TestMethod]
        public void IsHostAllowed_IgnoresPort()
        {
            var output = SiteCache.IsHostAllowed( "rock.church.org:8443", SiteDomains );
            Assert.That.AreEqual( true, output );
        }

        [TestMethod]
        public void IsHostAllowed_RejectsOtherHosts()
        {
            var output = SiteCache.IsHostAllowed( "unsafe.com", SiteDomains );
            Assert.That.AreEqual( false, output );
        }

        [TestMethod]
        public void IsHostAllowed_RejectsHostContainingSiteDomain()
        {
            var output = SiteCache.IsHostAllowed( "rock.church.org.unsafe.com", SiteDomains );
            Assert.That.AreEqual( false, output );
        }

        [TestMethod]
        public void IsHostAllowed_RejectsEmptyHost()
        {
            var output = SiteCache.IsHostAllowed( " ", SiteDomains );
            Assert.That.AreEqual( false, output );
        }

        #endregion IsHostAllowed
    }
}
