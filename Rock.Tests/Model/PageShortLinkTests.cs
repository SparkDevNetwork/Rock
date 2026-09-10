using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock;
using Rock.Cms;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tests.Model
{
    [TestClass]
    [TestCategory( "Cms.Interactions.UtmTracking" )]
    public class PageShortLinkTests
    {
        [TestMethod]
        public void Interaction_SetUtmFieldsFromUrlWithFragment_ExcludesFragmentText()
        {
            var interaction = new Interaction();

            interaction.SetUTMFieldsFromURL( $"/give?utm_content=newsletter-image#howtogive" );

            Assert.AreEqual( "newsletter-image", interaction.Content );
        }

        [TestMethod]
        public void PageShortlink_GetUrlWithUtm_WithConfiguredAndBaked_ConfiguredReplacesBaked()
        {
            // Latent bug fix: a configured utm_term replaces a destination-baked utm_term rather than appending to it.
            // Before the fix, NameValueCollection.Add produced two utm_term entries in the query string which downstream
            // parsers then lost entirely. Parsing via SetUTMFieldsFromURL would have returned "baked,alpha" (comma-joined)
            // for the duplicate-key shape, so asserting Term == "alpha" guards against regression.
            var settings = new UtmSettings { UtmTerm = "alpha" };

            var result = PageShortLinkCache.GetUrlWithUtm(
                "https://mywebsite.com/landing?utm_term=baked",
                settings,
                null );

            var parsed = new Interaction();
            parsed.SetUTMFieldsFromURL( result );

            Assert.AreEqual( "https://mywebsite.com/landing", new Uri( result ).GetLeftPart( UriPartial.Path ) );
            Assert.AreEqual( "alpha", parsed.Term );
            Assert.IsTrue( parsed.Source.IsNullOrWhiteSpace() );
            Assert.IsTrue( parsed.Medium.IsNullOrWhiteSpace() );
            Assert.IsTrue( parsed.Campaign.IsNullOrWhiteSpace() );
            Assert.IsTrue( parsed.Content.IsNullOrWhiteSpace() );
        }
    }
}
