//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Xml.Linq;
using System.Xml.Xsl;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Cms
{
    [CustomCheckboxListField("Image Attribute Keys", "The types of images to display",  
        "SELECT A.[name] AS [Text], A.[key] AS [Value] FROM [EntityType] E INNER JOIN [attribute] a ON A.[EntityTypeId] = E.[Id] INNER JOIN [FieldType] F ON F.Id = A.[FieldTypeId]	AND F.Guid = '" +
        Rock.SystemGuid.FieldType.IMAGE + "' WHERE E.Name = 'Rock.Model.MarketingCampaignAd' ORDER BY [Key]")]
    [DetailPage]
    [IntegerField( "Max Items" )]
    [BooleanField( "Show Debug", "Output XML", false )]
    [TextField( "XSLT File", "The path to the XSLT File ", true, "~/Assets/XSLT/AdList.xslt" )]

    [CustomCheckboxListField( "Ad Types", "Types of Ads to display",
        "SELECT [Name] AS [Text], [Id] AS [Value] FROM [MarketingCampaignAdType] ORDER BY [Name]", true, "", "Filter", 0 )]
    [CampusesField( "Campuses", "Display Ads for selected campus", false, "", "Filter", 1 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE, "Audience", "The Audience", false, "", "Filter", 2 )]
    [CustomCheckboxListField( "Audience Primary Secondary", "Primary or Secondary Audience", "1:Primary,2:Secondary", false, "1,2", "Filter", 3 )]
    [ContextAware( typeof(Campus) )]
    public partial class MarketingCampaignAdsXslt : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.AttributesUpdated += MarketingCampaignAdsXslt_AttributesUpdated;

            TransformXml();
        }

        /// <summary>
        /// Handles the AttributesUpdated event of the MarketingCampaignAdsXslt control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void MarketingCampaignAdsXslt_AttributesUpdated( object sender, EventArgs e )
        {
            TransformXml();
        }

        /// <summary>
        /// Transforms the XML.
        /// </summary>
        private void TransformXml()
        {
            MarketingCampaignAdService marketingCampaignAdService = new MarketingCampaignAdService();
            var qry = marketingCampaignAdService.Queryable();

            // limit to date range
            DateTime currentDateTime = DateTime.Now.Date;
            qry = qry.Where( a => ( a.StartDate <= currentDateTime ) && ( currentDateTime <= a.EndDate ) );

            // limit to approved
            qry = qry.Where( a => a.MarketingCampaignAdStatus == MarketingCampaignAdStatus.Approved );

            /* Block Attributes */

            // Audience
            string audience = GetAttributeValue( "Audience" );
            if ( !string.IsNullOrWhiteSpace( audience ) )
            {
                List<int> idlist = audience.SplitDelimitedValues().Select( a => int.Parse( a ) ).ToList();
                qry = qry.Where( a => a.MarketingCampaign.MarketingCampaignAudiences.Any( x => idlist.Contains( x.Id ) ) );
            }

            // AudiencePrimarySecondary
            string audiencePrimarySecondary = GetAttributeValue( "AudiencePrimarySecondary" );
            if ( !string.IsNullOrWhiteSpace( audiencePrimarySecondary ) )
            {
                // 1 = Primary, 2 = Secondary
                List<int> idlist = audiencePrimarySecondary.SplitDelimitedValues().Select( a => int.Parse( a ) ).ToList();

                if ( idlist.Contains( 1 ) && !idlist.Contains( 2 ) )
                {
                    // only show to Primary Audiences
                    qry = qry.Where( a => a.MarketingCampaign.MarketingCampaignAudiences.Any( x => x.IsPrimary == true ) );
                }
                else if ( idlist.Contains( 2 ) && !idlist.Contains( 1 ) )
                {
                    // only show to Secondary Audiences
                    qry = qry.Where( a => a.MarketingCampaign.MarketingCampaignAudiences.Any( x => x.IsPrimary == false ) );
                }
            }

            // Campuses
            string campuses = GetAttributeValue( "Campuses" );
            if ( !string.IsNullOrWhiteSpace( campuses ) )
            {
                List<int> idlist = campuses.SplitDelimitedValues().Select( a => int.Parse( a ) ).ToList();
                qry = qry.Where( a => a.MarketingCampaign.MarketingCampaignCampuses.Any( x => idlist.Contains( x.Id ) ) );
            }

            // Ad Types
            string adtypes = GetAttributeValue( "AdTypes" );
            if ( !string.IsNullOrWhiteSpace( adtypes ) )
            {
                List<int> idlist = adtypes.SplitDelimitedValues().Select( a => int.Parse( a ) ).ToList();
                qry = qry.Where( a => idlist.Contains( a.MarketingCampaignAdTypeId ) );
            }

            // Image Attribute Keys
            string imageAttributeKeys = GetAttributeValue( "ImageAttributeKeys" );
            List<string> imageAttributeKeyFilter = null;
            if ( !string.IsNullOrWhiteSpace( imageAttributeKeys ) )
            {
                imageAttributeKeyFilter = imageAttributeKeys.SplitDelimitedValues().ToList();
            }

            // Campus Context
            Campus campusContext = this.ContextEntity<Campus>();
            if ( campusContext != null )
            {
                // limit to ads that are targeted to the current campus context
                qry = qry.Where( a => a.MarketingCampaign.MarketingCampaignCampuses.Any( x => x.Id.Equals( campusContext.Id ) ) );
            }
            

            // Max Items
            string maxItems = GetAttributeValue( "MaxItems" );
            int? maxAdCount = null;
            if ( !string.IsNullOrWhiteSpace( maxItems ) )
            {
                int parsedCount = 0;
                if ( int.TryParse( maxItems, out parsedCount ) )
                {
                    maxAdCount = parsedCount;
                }
            }

            List<MarketingCampaignAd> marketingCampaignAdList;
            qry = qry.OrderBy( a => a.Priority ).ThenBy( a => a.StartDate ).ThenBy( a => a.MarketingCampaign.Title );
            if ( maxAdCount == null )
            {
                marketingCampaignAdList = qry.ToList();
            }
            else
            {
                marketingCampaignAdList = qry.Take( maxAdCount.Value ).ToList();
            }

            // build Xml doc
            XDocument doc = new XDocument();
            XElement rootNode = new XElement( "Ads" );
            doc.Add( rootNode );

            foreach ( var marketingCampaignAd in marketingCampaignAdList )
            {
                XElement xmlElement = new XElement( "Ad" );

                // DetailPage
                string detailPageUrl = string.Empty;
                string detailPageGuid = GetAttributeValue( "DetailPage" );
                if ( !string.IsNullOrWhiteSpace( detailPageGuid ) )
                {
                    Rock.Model.Page detailPage = new PageService().Get( new Guid( "detailPageGuid" ) );
                    if ( detailPage != null )
                    {
                        Dictionary<string, string> queryString = new Dictionary<string, string>();
                        queryString.Add( "marketingCampaignAd", marketingCampaignAd.Id.ToString() );
                        detailPageUrl = CurrentPage.BuildUrl( detailPage.Id, queryString );
                    }
                }

                string eventGroupName = marketingCampaignAd.MarketingCampaign.EventGroup != null ? marketingCampaignAd.MarketingCampaign.EventGroup.Name : string.Empty;

                // Marketing Campaign Fields
                xmlElement.Add(
                    new XAttribute( "Title", marketingCampaignAd.MarketingCampaign.Title ),
                    new XAttribute( "ContactEmail", marketingCampaignAd.MarketingCampaign.ContactEmail ),
                    new XAttribute( "ContactFullName", marketingCampaignAd.MarketingCampaign.ContactFullName ),
                    new XAttribute( "ContactPhoneNumber", marketingCampaignAd.MarketingCampaign.ContactPhoneNumber ),
                    new XAttribute( "LinkedEvent", eventGroupName ) );

                // Specific Ad Fields
                xmlElement.Add(
                    new XAttribute( "AdType", marketingCampaignAd.MarketingCampaignAdType.Name ),
                    new XAttribute( "StartDate", marketingCampaignAd.StartDate.ToString() ),
                    new XAttribute( "EndDate", marketingCampaignAd.EndDate.ToString() ),
                    new XAttribute( "Priority", marketingCampaignAd.Priority ),
                    new XAttribute( "Url", marketingCampaignAd.Url ),
                    new XAttribute( "DetailPageUrl", detailPageUrl ) );

                // Ad Attributes
                XElement attribsNode = new XElement( "Attributes" );
                xmlElement.Add( attribsNode );

                marketingCampaignAd.LoadAttributes();
                Rock.Attribute.Helper.AddDisplayControls( marketingCampaignAd, phContent );
                foreach ( var item in marketingCampaignAd.Attributes )
                {
                    AttributeCache attribute = item.Value;
                    List<AttributeValue> attributeValues = marketingCampaignAd.AttributeValues[attribute.Key];
                    foreach ( AttributeValue attributeValue in attributeValues )
                    {
                        // If Block Attributes limit image types, limit images 
                        if ( attribute.FieldType.Guid.Equals( new Guid( Rock.SystemGuid.FieldType.IMAGE ) ) )
                        {
                            if ( imageAttributeKeyFilter != null )
                            {
                                if ( !imageAttributeKeyFilter.Contains( attribute.Key ) )
                                {
                                    // skip to next attribute if this is an image attribute and it doesn't match the image key filter
                                    continue;
                                }
                            }
                        }

                        string valueHtml = attribute.FieldType.Field.FormatValue( this, attributeValue.Value, attribute.QualifierValues, false );
                        XElement valueNode = new XElement( "Attribute" );
                        valueNode.Add( new XAttribute( "Key", attribute.Key ) );
                        valueNode.Add( new XAttribute( "Name", attribute.Name ) );
                        valueNode.Add( new XAttribute( "Value", valueHtml ) );
                        attribsNode.Add( valueNode );
                    }
                }

                rootNode.Add( xmlElement );
            }

            string showDebugValue = GetAttributeValue( "ShowDebug" ) ?? string.Empty;
            bool showDebug = showDebugValue.Equals( "true", StringComparison.OrdinalIgnoreCase );

            string fileName = GetAttributeValue( "XSLTFile" );
            string outputXml;
            if ( showDebug || string.IsNullOrWhiteSpace( fileName ) )
            {
                outputXml = HttpUtility.HtmlEncode( doc.ToString() );
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                TextWriter tw = new StringWriter( sb );

                XslCompiledTransform xslTransformer = new XslCompiledTransform();
                xslTransformer.Load( Server.MapPath( fileName ) );
                xslTransformer.Transform( doc.CreateReader(), null, tw );
                outputXml = sb.ToString();
            }

            phContent.Controls.Clear();

            phContent.Controls.Add( new LiteralControl( outputXml ) );
        }
    }
}