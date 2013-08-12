//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Xml.Linq;
using System.Xml.Xsl;
using Rock;
using Rock.Attribute;

namespace RockWeb.Blocks.Cms
{
    [TextField( "XSLT File", "The path to the XSLT File ", true, "~/Assets/XSLT/PageList.xslt" )]
    [LinkedPage( "Root Page", "The root page to use for the page collection. Defaults to the current page instance if not set.", false, "" )]
    [TextField( "Number of Levels", "Number of parent-child page levels to display. Default 3.", false, "3" )]
    [TextField("CSS File", "Optional CSS file to add to the page for styling.", false, "")]
    [BooleanField( "Include Current Parameters", "Flag indicating if current page's parameters should be used when building url for child pages", false )]
    [BooleanField( "Include Current QueryString", "Flag indicating if current page's QueryString should be used when building url for child pages", false )]
    public partial class PageXslt : Rock.Web.UI.RockBlock
    {
        private static readonly string ROOT_PAGE = "RootPage";
        private static readonly string NUM_LEVELS = "NumberofLevels";

        protected override void OnInit( EventArgs e )
        {
            this.EnableViewState = false;

            base.OnInit( e );

            this.AttributesUpdated += PageXslt_AttributesUpdated;
            //this.AddAttributeUpdateTrigger( upContent );
            //upContent.ContentTemplateContainer.Controls.Add( )

            // add css file to page
            if (GetAttributeValue( "CSSFile" ).Trim() != string.Empty)
                CurrentPage.AddCSSLink( Page, ResolveUrl("~/CSS/jquery.tagsinput.css"));

            TransformXml();
        }

        void PageXslt_AttributesUpdated( object sender, EventArgs e )
        {
            TransformXml();
        }

        private void TransformXml()
        {
            XslCompiledTransform xslTransformer = new XslCompiledTransform();
            xslTransformer.Load( Server.MapPath( GetAttributeValue( "XSLTFile" ) ) );

            Rock.Web.Cache.PageCache rootPage = null;

            Guid pageGuid = Guid.Empty;
            if ( Guid.TryParse( GetAttributeValue( ROOT_PAGE ), out pageGuid ) )
            {
                rootPage = Rock.Web.Cache.PageCache.Read( pageGuid );
            }

            if ( rootPage == null )
            {
                rootPage = CurrentPage;
            }

            int levelsDeep = Convert.ToInt32( GetAttributeValue( NUM_LEVELS ) );

            Dictionary<string, string> pageParameters = null;
            if ( GetAttributeValue( "IncludeCurrentParameters" ).AsBoolean() )
            {
                pageParameters = CurrentPageReference.Parameters;
            }

            NameValueCollection queryString = null;
            if ( GetAttributeValue( "IncludeCurrentQueryString" ).AsBoolean() )
            {
                queryString = CurrentPageReference.QueryString;
            }

            XDocument pageXml = rootPage.MenuXml( levelsDeep, CurrentPerson, CurrentPage, pageParameters, queryString );

            StringBuilder sb = new StringBuilder();
            TextWriter tw = new StringWriter( sb );
            xslTransformer.Transform( pageXml.CreateReader(), null, tw );

            phContent.Controls.Clear();
            phContent.Controls.Add( new LiteralControl( sb.ToString() ) );
        }
    }
}