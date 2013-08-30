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
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Cms
{
    [IntegerField( "Max Items", "", true, int.MinValue, "", 0 )]
    [LinkedPage ("Detail Page", "", false, "", "", 1)]
    [CustomCheckboxListField("Image Types", "The types of images to display",  
        "SELECT A.[name] AS [Text], A.[key] AS [Value] FROM [EntityType] E INNER JOIN [attribute] a ON A.[EntityTypeId] = E.[Id] INNER JOIN [FieldType] F ON F.Id = A.[FieldTypeId]	AND F.Guid = '" +
        Rock.SystemGuid.FieldType.IMAGE + "' WHERE E.Name = 'Rock.Model.MarketingCampaignAd' ORDER BY [Key]", false, "", "", 2)]
    [TextField( "XSLT File", "The path to the XSLT File ", true, "~/Assets/XSLT/AdList.xslt", "", 3 )]

    [CampusesField( "Campuses", "Display Ads for selected campus", false, "", "Filter", 4 )]
    [CustomCheckboxListField( "Ad Types", "Types of Ads to display",
        "SELECT [Name] AS [Text], [Id] AS [Value] FROM [MarketingCampaignAdType] ORDER BY [Name]", true, "", "Filter", 5 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE, "Audience", "The Audience", false, "", "Filter", 6 )]
    [CustomCheckboxListField( "Audience Primary Secondary", "Primary or Secondary Audience", "1:Primary,2:Secondary", false, "1,2", "Filter", 7 )]

    [BooleanField( "Show Debug", "Output the XML to be transformed.", false, "Advanced", 8 )]

    [IntegerField("Image Width", "Width that the image should be resized to. Leave height/width blank to get original size.", false, int.MinValue, "", 9)]
    [IntegerField("Image Height", "Height that the image should be resized to. Leave height/width blank to get original size.", false, int.MinValue, "", 10)]
    
    
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

            // ensure xslt file exists
            string xsltFile = Server.MapPath(GetAttributeValue("XSLTFile"));
            if (System.IO.File.Exists( xsltFile ))
            {
                TransformXml(xsltFile);
            }
            else
            {
                string errorMessage = "<div class='alert warning' style='margin: 24px auto 0 auto; max-width: 500px;' ><strong>Warning!</strong><p>The XSLT file required to process the ad list could not be found.</p><p><em>" + xsltFile + "</em></p>";
                phContent.Controls.Add(new LiteralControl(errorMessage));
            }
        }


        /// <summary>
        /// Transforms the XML.
        /// </summary>
        private void TransformXml(string xsltFile)
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
                var idList = new List<int>();
                foreach ( string guid in audience.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
                {
                    var definedValue = DefinedValueCache.Read( new Guid( guid ) );
                    if ( definedValue != null )
                    {
                        idList.Add( definedValue.Id );
                    }
                }
                qry = qry.Where( a => a.MarketingCampaign.MarketingCampaignAudiences.Any( x => idList.Contains( x.AudienceTypeValueId ) ) );
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
                qry = qry.Where( a => a.MarketingCampaign.MarketingCampaignCampuses.Any( x => idlist.Contains( x.CampusId ) ) );
            }

            // Ad Types
            string adtypes = GetAttributeValue( "AdTypes" );
            if ( !string.IsNullOrWhiteSpace( adtypes ) )
            {
                List<int> idlist = adtypes.SplitDelimitedValues().Select( a => int.Parse( a ) ).ToList();
                qry = qry.Where( a => idlist.Contains( a.MarketingCampaignAdTypeId ) );
            }

            // Image Types
            string imageTypes = GetAttributeValue( "ImageTypes" );
            List<string> imageTypeFilter = null;
            if ( !string.IsNullOrWhiteSpace( imageTypes ) )
            {
                imageTypeFilter = imageTypes.SplitDelimitedValues().ToList();
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
                    Rock.Model.Page detailPage = new PageService().Get( new Guid( detailPageGuid ) );
                    if ( detailPage != null )
                    {
                        Dictionary<string, string> queryString = new Dictionary<string, string>();
                        queryString.Add( "ad", marketingCampaignAd.Id.ToString() );
                        detailPageUrl = new PageReference( detailPage.Id, 0, queryString ).BuildUrl();
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
                
                // create image resize width/height from block settings
                Dictionary<string, Rock.Field.ConfigurationValue> imageConfig = new Dictionary<string, Rock.Field.ConfigurationValue>();
                if (!string.IsNullOrWhiteSpace(GetAttributeValue("ImageWidth"))
                    && Int32.Parse(GetAttributeValue("ImageWidth")) != Int16.MinValue)
                    imageConfig.Add("width", new Rock.Field.ConfigurationValue(GetAttributeValue("ImageWidth")));

                if (!string.IsNullOrWhiteSpace(GetAttributeValue("ImageHeight"))
                    && Int32.Parse(GetAttributeValue("ImageHeight")) != Int16.MinValue)
                    imageConfig.Add("height", new Rock.Field.ConfigurationValue(GetAttributeValue("ImageHeight")));
                
                foreach ( var item in marketingCampaignAd.Attributes )
                {
                    AttributeCache attribute = item.Value;
                    List<AttributeValue> attributeValues = marketingCampaignAd.AttributeValues[attribute.Key];
                    foreach ( AttributeValue attributeValue in attributeValues )
                    {
                        string valueHtml = string.Empty;
                        
                        // If Block Attributes limit image types, limit images 
                        if (attribute.FieldType.Guid.Equals(new Guid(Rock.SystemGuid.FieldType.IMAGE)))
                        {
                            if (imageTypeFilter != null)
                            {
                                if (!imageTypeFilter.Contains(attribute.Key))
                                {
                                    // skip to next attribute if this is an image attribute and it doesn't match the image key filter
                                    continue;
                                }
                                else
                                {
                                    valueHtml = attribute.FieldType.Field.FormatValue(this, attributeValue.Value, imageConfig, false);
                                }
                            }
                        }
                        else
                        {
                            valueHtml = attribute.FieldType.Field.FormatValue(this, attributeValue.Value, attribute.QualifierValues, false);
                        }

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

            
            string outputXml;
            if ( showDebug || string.IsNullOrWhiteSpace( xsltFile ) )
            {
                outputXml = "<pre><code>" + HttpUtility.HtmlEncode( doc.ToString() ) + "</code></pre>";
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                TextWriter tw = new StringWriter( sb );

                // try compiling the XSLT
                try
                {
                    XslCompiledTransform xslTransformer = new XslCompiledTransform();

                    // pass in xslt parms
                    XsltArgumentList xsltArgs = new XsltArgumentList();
                    xsltArgs.AddParam("application_path", "", HttpRuntime.AppDomainAppVirtualPath);
                    // todo add theme path JME
                                        
                    xslTransformer.Load(xsltFile);
                    xslTransformer.Transform(doc.CreateReader(), xsltArgs, tw);
                    outputXml = sb.ToString();
                }
                catch (Exception ex)
                {
                    // xslt compile error
                    string exMessage = "An excception occurred while compiling the XSLT template.";
                    
                    if (ex.InnerException != null)
                        exMessage += "<br /><em>" + ex.InnerException.Message + "</em>";

                    outputXml = "<div class='alert warning' style='margin: 24px auto 0 auto; max-width: 500px;' ><strong>XSLT Compile Error</strong><p>" + exMessage + "</p></div>";
                }
            }

            phContent.Controls.Clear();

            phContent.Controls.Add( new LiteralControl( outputXml ) );
        }
    }
}