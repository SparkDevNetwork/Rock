// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Cms.Utm;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.Cms;
using Rock.Tests.Integration.Core;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Transactions;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Core.Model
{
    [TestClass]
    [TestCategory( "Interactions" )]
    [TestCategory( "Feature.Cms.UtmTracking" )]
    public class PageShortLinkTests : DatabaseTestsBase
    {
        [ClassInitialize]
        public static void ClassInitialize( TestContext context )
        {
            InitializeUtmTestData();
        }

        #region UTM

        private const string _testUtmCampaign1Guid = "c6b1bbff-4a08-4aad-a670-23e05aa439b3";
        private const string _testUtmCampaign2Guid = "d45748c1-f0d9-4c01-a5ce-efa57b62ccb7";
        private const string _testUtmCampaign3Guid = "53e99c64-7dd2-4481-a785-062e3ac752b6";

        private const string _testShortLink1Guid = "0cf981d9-b7a8-444f-8c6f-f64610cea1d4";

        private const string _testSourceName = "source:test!";
        private const string _testMediumName = "medium:test!";
        private const string _testCampaignNameBlog = "Blog Promotion";
        private const string _testCampaignNameGiving = "Giving Promotion";
        private const string _testCampaignNameSpecial = "New Campaign (*pending)";

        [TestMethod]
        public void Interaction_SetUtmFieldsForUtmElementWithMatchedDefinedValue_SetsUtmValueField()
        {
            var dvSource = DefinedTypeCache.Get( SystemGuid.DefinedType.UTM_SOURCE ).DefinedValues.FirstOrDefault( v => v.Value == "youtube" );
            var dvMedium = DefinedTypeCache.Get( SystemGuid.DefinedType.UTM_MEDIUM ).DefinedValues.FirstOrDefault( v => v.Value == "organic" );
            var dvCampaign = DefinedTypeCache.Get( SystemGuid.DefinedType.UTM_CAMPAIGN ).DefinedValues.FirstOrDefault( v => v.Value == _testCampaignNameBlog );

            var interaction = new Interaction();

            interaction.SetUTMFieldsFromURL( $"http://www.rocksolidchurchdemo.com/signup?utm_source={dvSource.Value}&utm_medium={dvMedium.Value}&utm_campaign={dvCampaign.Value}" );

            // Verify that the elements are recorded as defined values, but the free-form text field is not populated.
            Assert.That.AreEqual( dvSource.Id, interaction.SourceValueId );
            Assert.That.IsEmpty( interaction.Source );

            Assert.That.AreEqual( dvMedium.Id, interaction.MediumValueId );
            Assert.That.IsEmpty( interaction.Medium );

            Assert.That.AreEqual( dvCampaign.Id, interaction.CampaignValueId );
            Assert.That.IsEmpty( interaction.Campaign );
        }

        [TestMethod]
        public void Interaction_SetUtmFieldsForUtmElementWithUnmatchedDefinedValue_SetsUtmTextField()
        {
            var interaction = new Interaction();

            interaction.SetUTMFieldsFromURL( "http://www.rocksolidchurchdemo.com/signup?utm_source=undefinedsource&utm_medium=undefinedmedium&utm_campaign=undefinedcampaign" );

            // Verify that the unmatched values are recorded as free-form text, but the value field is not populated.
            Assert.That.IsNull( interaction.SourceValueId );
            Assert.That.AreEqual( "undefinedsource", interaction.Source );

            Assert.That.IsNull( interaction.MediumValueId );
            Assert.That.AreEqual( "undefinedmedium", interaction.Medium );

            Assert.That.IsNull( interaction.CampaignValueId );
            Assert.That.AreEqual( "undefinedcampaign", interaction.Campaign );
        }

        [TestMethod]
        public void Interaction_SetUtmFieldsWithUnspecifiedValues_ResetsEmptyFieldValue()
        {
            var interaction = new Interaction();

            // Verify that only fields with specified values are modified by a subsequent update.
            interaction.SetUTMFieldsFromURL( $"http://www.rocksolidchurchdemo.com/signup?utm_source=google&utm_medium=organic&utm_campaign" );
            interaction.SetUTMFieldsFromURL( $"http://www.rocksolidchurchdemo.com/signup?utm_source=google&utm_medium=organic" );

            Assert.That.IsEmpty( $"{interaction.Campaign}{interaction.CampaignValueId}" );
            Assert.That.IsNotEmpty( $"{interaction.Source}{interaction.SourceValueId}" );
        }

        [TestMethod]
        public void Interaction_SetUtmFieldsFromUrlWithFragment_ExcludesFragmentText()
        {
            var interaction = new Interaction();

            interaction.SetUTMFieldsFromURL( $"/give?utm_content=newsletter-image#howtogive" );

            Assert.That.AreEqual( "newsletter-image", interaction.Content );
        }

        [TestMethod]
        public void Interaction_SetUtmFieldsWithParameterSpecifiedTwice_SecondValueOverwritesFirstValue()
        {
            var interaction = new Interaction();

            // Verify that fields specified more than once are overwritten by the later value.
            interaction.SetUTMFieldsFromURL( $"http://www.rocksolidchurchdemo.com/signup?utm_source=source1&utm_medium=organic&utm_source=source2" );

            Assert.That.AreEqual( "source2", interaction.Source );
        }

        [TestMethod]
        public void InteractionTransaction_SetUtmFieldsForUtmElementWithMatchedDefinedValue_SetsUtmValueField()
        {
            // An Interaction Transaction should create an Interaction with ValueId fields populated for UTM values that match a Defined Value.
            var dvSource = DefinedTypeCache.Get( SystemGuid.DefinedType.UTM_SOURCE ).DefinedValues.FirstOrDefault( v => v.Value == "youtube" );
            var dvMedium = DefinedTypeCache.Get( SystemGuid.DefinedType.UTM_MEDIUM ).DefinedValues.FirstOrDefault( v => v.Value == "organic" );
            var dvCampaign = DefinedTypeCache.Get( SystemGuid.DefinedType.UTM_CAMPAIGN ).DefinedValues.FirstOrDefault( v => v.Value == _testCampaignNameBlog );

            var utmInfo = new UtmCookieData
            {
                Source = dvSource.Value,
                Medium = dvMedium.Value,
                Campaign = dvCampaign.Value
            };

            var interaction = CreateNewPageInteractionWithTransaction( utmInfo );

            // Verify that the elements are recorded as defined values, but the free-form text field is not populated.
            Assert.That.AreEqual( dvSource.Id, interaction.SourceValueId );
            Assert.That.IsEmpty( interaction.Source );

            Assert.That.AreEqual( dvMedium.Id, interaction.MediumValueId );
            Assert.That.IsEmpty( interaction.Medium );

            Assert.That.AreEqual( dvCampaign.Id, interaction.CampaignValueId );
            Assert.That.IsEmpty( interaction.Campaign );
        }

        [TestMethod]
        public void InteractionTransaction_SetUtmFieldsForUtmElementWithUnmatchedDefinedValue_SetsUtmTextField()
        {
            // An Interaction Transaction should create an Interaction with Text fields populated for UTM values that do not match a Defined Value.
            var utmInfo = new UtmCookieData
            {
                Source = "undefined_source",
                Medium = "undefined_medium",
                Campaign = "undefined_campaign"
            };

            var interaction = CreateNewPageInteractionWithTransaction( utmInfo );

            // Verify that the elements are recorded as defined values, but the free-form text field is not populated.
            Assert.That.AreEqual( utmInfo.Source, interaction.Source );
            Assert.That.IsNull( interaction.SourceValueId );

            Assert.That.AreEqual( utmInfo.Medium, interaction.Medium );
            Assert.That.IsNull( interaction.MediumValueId );

            Assert.That.AreEqual( utmInfo.Campaign, interaction.Campaign );
            Assert.That.IsNull( interaction.CampaignValueId );
        }

        private Interaction CreateNewPageInteractionWithTransaction( UtmCookieData utmInfo )
        {
            var externalSite = SiteCache.Get( SystemGuid.Site.EXTERNAL_SITE );
            var page = PageCache.Get( SystemGuid.Page.GIVE );

            var interactionInfo = new InteractionTransactionInfo
            {
                InteractionTimeToServe = 11,
                InteractionChannelCustomIndexed1 = "localhost",
                InteractionChannelCustom2 = "my-search"
            };

            UtmHelper.AddUtmInfoToInteractionTransactionInfo( interactionInfo, utmInfo );

            var pageViewTransaction = new InteractionTransaction( DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE ),
                externalSite,
                page,
                interactionInfo );

            // Execute then transaction immediately, and retrieve the result.
            pageViewTransaction.Execute();

            var rockContext = new RockContext();
            var interactionService = new InteractionService( rockContext );
            var interaction = interactionService.Queryable()
                .OrderByDescending( i => i.Id )
                .FirstOrDefault();

            return interaction;
        }

        [TestMethod]
        public void PageShortlink_WithModifiedUtmValues_IsPersisted()
        {
            var externalSite = EntityLookup.GetByNameOrThrow<Rock.Model.Site>( "External Website" );

            var shortlink = WebsiteDataManager.Current.CreatePageShortLink( externalSite.Id,
                "my-promotion",
                "https://mywebsite.com" );
            shortlink.Guid = _testShortLink1Guid.AsGuid();

            var sourceValueId = CoreDataManager.Current.GetDefinedValueIdOrNull( SystemGuid.DefinedType.UTM_SOURCE, "youtube" );
            var mediumValueId = CoreDataManager.Current.GetDefinedValueIdOrNull( SystemGuid.DefinedType.UTM_MEDIUM, "email" );
            var campaignValueId = CoreDataManager.Current.GetDefinedValueIdOrNull( SystemGuid.DefinedType.UTM_CAMPAIGN, "Test Campaign C" );

            var utmSettings = shortlink.GetAdditionalSettings<Rock.Model.PageShortLink.UtmSettings>();

            utmSettings.UtmSourceValueId = sourceValueId;
            utmSettings.UtmMediumValueId = mediumValueId;
            utmSettings.UtmCampaignValueId = campaignValueId;
            utmSettings.UtmTerm = "special";
            utmSettings.UtmContent = "banner_image";

            shortlink.SetAdditionalSettings( utmSettings );

            WebsiteDataManager.Current.SavePageShortLink( shortlink );

            // Retrieve the shortlink and verify the stored values.
            shortlink = EntityLookup.GetByIdentifier<PageShortLink>( _testShortLink1Guid );

            utmSettings = shortlink.GetAdditionalSettings<Rock.Model.PageShortLink.UtmSettings>();

            Assert.That.AreEqual( sourceValueId, utmSettings.UtmSourceValueId );
            Assert.That.AreEqual( mediumValueId, utmSettings.UtmMediumValueId );
            Assert.That.AreEqual( campaignValueId, utmSettings.UtmCampaignValueId );
            Assert.That.AreEqual( "special", utmSettings.UtmTerm );
            Assert.That.AreEqual( "banner_image", utmSettings.UtmContent );
        }

        [DataTestMethod]
        [DataRow( "/give?param1=1&param2=2#howtogive", "google", "/give?param1=1&param2=2&utm_source=google#howtogive" )]
        [DataRow( "~/blog#entry123", "google", "~/blog?utm_source=google#entry123" )]
        [DataRow( "https://prealpha.rocksolidchurchdemo.com/watch", null, "https://prealpha.rocksolidchurchdemo.com/watch" )]
        public void PageShortlink_WithPartialUrl_ReturnsPartialUrlWithUtmQueryParameters( string baseUrl, string utmSource, string expectedOutput )
        {
            // If the shortlink Url is incomplete, the UrlWithUtm property should continue to return the correct query parameters.
            var shortlink = CreateTestPageShortlinkWithUtmValues( "test-promotion", baseUrl, utmSource, null, null, null, null );

            Assert.That.AreEqual( expectedOutput, shortlink.UrlWithUtm );
        }

        [DataTestMethod]
        [DataRow( "google", null, null, null, null, "utm_source=google" )]
        [DataRow( null, "post", null, null, null, "utm_medium=post" )]
        [DataRow( null, null, _testCampaignNameBlog, null, null, "utm_campaign=blog%20promotion" )]
        [DataRow( null, null, null, "my+search+terms", null, "utm_term=my%2Bsearch%2Bterms" )]
        [DataRow( null, null, null, null, "banner_image", "utm_content=banner_image" )]
        [DataRow( "google", "post", _testCampaignNameBlog, "my+search+terms", "banner_image", "utm_source=google&utm_medium=post&utm_campaign=blog%20promotion&utm_term=my%2Bsearch%2Bterms&utm_content=banner_image" )]
        public void PageShortlink_WithUnspecifiedUtmValues_OmitsEmptyValuesFromQueryString( string sourceValue, string mediumValue, string campaignValue, string term, string content, string expectedUnencodedQueryString )
        {
            var baseUrl = "https://mywebsite.com/promotion";

            var shortlink = CreateTestPageShortlinkWithUtmValues( "test-promotion", baseUrl, sourceValue, mediumValue, campaignValue, term, content );

            // Build the Url, excluding the port from the output.
            var uriBuilder = new UriBuilder( baseUrl );
            uriBuilder.Port = -1;
            uriBuilder.Query = expectedUnencodedQueryString;

            var expectedUrl = uriBuilder.Uri.OriginalString;

            Assert.That.AreEqual( expectedUrl, shortlink.UrlWithUtm );
        }

        [TestMethod]
        public void PageShortlink_GetUrlWithUtm_ProducesEncodedUrlWithUtmParameters()
        {
            // Create a shortlink with a URL and UTM parameters that contain special characters.
            var shortlink = CreateTestPageShortlinkWithUtmValues( "test-promotion",
                "https://mywebsite.com\r\n\r\n",
                _testSourceName,
                _testMediumName,
                _testCampaignNameSpecial,
                "term1&term2",
                "http://embedded-link" );

            Assert.That.AreEqual( "https://mywebsite.com/?utm_source=source%3Atest%21&utm_medium=medium%3Atest%21&utm_campaign=new%20campaign%20%28%2Apending%29&utm_term=term1%26term2&utm_content=http%3A%2F%2Fembedded-link",
                shortlink.UrlWithUtm );
        }

        private static PageShortLink CreateTestPageShortlinkWithUtmValues( string token, string url, string sourceValue, string mediumValue, string campaignValue, string term, string content )
        {
            var externalSite = EntityLookup.GetByNameOrThrow<Rock.Model.Site>( "External Website" );

            url = url ?? "https://mywebsite.com";

            var shortlink = WebsiteDataManager.Current.CreatePageShortLink( externalSite.Id,
                token,
                 url );

            var sourceValueId = CoreDataManager.Current.GetDefinedValueIdOrNull( SystemGuid.DefinedType.UTM_SOURCE, sourceValue );
            var mediumValueId = CoreDataManager.Current.GetDefinedValueIdOrNull( SystemGuid.DefinedType.UTM_MEDIUM, mediumValue );
            var campaignValueId = CoreDataManager.Current.GetDefinedValueIdOrNull( SystemGuid.DefinedType.UTM_CAMPAIGN, campaignValue );

            var utmSettings = shortlink.GetAdditionalSettings<Rock.Model.PageShortLink.UtmSettings>();

            utmSettings.UtmSourceValueId = sourceValueId;
            utmSettings.UtmMediumValueId = mediumValueId;
            utmSettings.UtmCampaignValueId = campaignValueId;
            utmSettings.UtmTerm = term;
            utmSettings.UtmContent = content;

            shortlink.SetAdditionalSettings( utmSettings );

            return shortlink;
        }

        private static void InitializeUtmTestData()
        {
            var rockContext = new RockContext();

            // Add UTM Campaigns
            var utmCampaignDefinedTypeId = DefinedTypeCache.GetId( SystemGuid.DefinedType.UTM_CAMPAIGN.AsGuid() ) ?? 0;

            CoreDataManager.Current.AddOrUpdateDefinedValue( utmCampaignDefinedTypeId, _testUtmCampaign1Guid, _testCampaignNameBlog, "This is the first campaign.", rockContext );
            CoreDataManager.Current.AddOrUpdateDefinedValue( utmCampaignDefinedTypeId, _testUtmCampaign2Guid, _testCampaignNameGiving, "This is the second campaign.", rockContext );
            CoreDataManager.Current.AddOrUpdateDefinedValue( utmCampaignDefinedTypeId, _testUtmCampaign3Guid, _testCampaignNameSpecial, "This campaign has special characters.", rockContext );

            // Add UTM Source
            var utmSourceDefinedTypeId = DefinedTypeCache.GetId( SystemGuid.DefinedType.UTM_SOURCE.AsGuid() ) ?? 0;
            CoreDataManager.Current.AddOrUpdateDefinedValue( utmSourceDefinedTypeId, "", _testSourceName, "A test source containing special characters.", rockContext );

            // Add UTM Medium
            var utmMediumDefinedTypeId = DefinedTypeCache.GetId( SystemGuid.DefinedType.UTM_MEDIUM.AsGuid() ) ?? 0;
            CoreDataManager.Current.AddOrUpdateDefinedValue( utmMediumDefinedTypeId, "", _testMediumName, "A test medium containing special characters.", rockContext );

            rockContext.SaveChanges();


            // Add Sample Data Shortlinks
            PageShortLink shortlink;

            shortlink = CreateTestPageShortlinkWithUtmValues( "blog-promotion",
                " ~/blog",
                "facebook",
                "social",
                "Blog Promotion",
                "rocksolid+blog",
                "text-link" );
            shortlink.Guid = "64c61237-0e6f-4120-bbf5-8c1dd825ad27".AsGuid();
            WebsiteDataManager.Current.SavePageShortLink( shortlink );

            shortlink = CreateTestPageShortlinkWithUtmValues( "giving-promotion",
                "/give?param1=1&param2=2#howtogive",
                "website",
                "email",
                "Giving Promotion",
                term: null,
                "newsletter-image" );
            shortlink.Guid = "4ef87dd2-fc5a-4bc1-a76c-1463af55a234".AsGuid();
            WebsiteDataManager.Current.SavePageShortLink( shortlink );

            shortlink = CreateTestPageShortlinkWithUtmValues( "lava-filter-size",
                "https://community.rockrms.com/lava/filters/text-filters#size",
                null, null, null, null, null );
            shortlink.Guid = "777ce8eb-e46f-4711-9b07-3420b52e12db".AsGuid();
            WebsiteDataManager.Current.SavePageShortLink( shortlink );
        }

        #endregion
    }
}
