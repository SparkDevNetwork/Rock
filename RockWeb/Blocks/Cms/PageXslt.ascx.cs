using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using System.Xml.Xsl;

using Rock.Cms;

namespace RockWeb.Blocks.Cms
{
    [Rock.Attribute.Property( "XSLT File", "The path to the XSLT File ", "~/Assets/XSLT/PageList.xslt" )]
    [Rock.Attribute.Property( "Root Page", "The root page to use for the page collection. Defaults to the current page instance if not set." )]
    [Rock.Attribute.Property( "Number of Levels", "Number of parent-child page levels to display. Default 3.", "3" )]
    public partial class PageXslt : Rock.Cms.CmsBlock
    {
		private static readonly string ROOT_PAGE = "RootPage";
		private static readonly string NUM_LEVELS = "NumberofLevels";

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.AttributesUpdated += PageXslt_AttributesUpdated;
            //this.AddAttributeUpdateTrigger( upContent );
			//upContent.ContentTemplateContainer.Controls.Add( )

            TransformXml();
        }

        void PageXslt_AttributesUpdated( object sender, EventArgs e )
        {
            TransformXml();
        }

        private void TransformXml()
        {
            XslCompiledTransform xslTransformer = new XslCompiledTransform();
            xslTransformer.Load( Server.MapPath( AttributeValue("XSLTFile") ) );

            Rock.Cms.Cached.Page rootPage;
			if ( AttributeValue( ROOT_PAGE ) != string.Empty )
            {
				int pageId = Convert.ToInt32( AttributeValue( ROOT_PAGE ) );
                if ( pageId == -1 )
                    rootPage = PageInstance;
                else
                    rootPage = Rock.Cms.Cached.Page.Read( pageId );
            }
            else
                rootPage = PageInstance;

			int levelsDeep = Convert.ToInt32( AttributeValue( NUM_LEVELS ) );

			XDocument pageXml = rootPage.MenuXml( levelsDeep, CurrentUser );

            StringBuilder sb = new StringBuilder();
            TextWriter tw = new StringWriter( sb );
            xslTransformer.Transform( pageXml.CreateReader(), null, tw );

            phContent.Controls.Clear();
            phContent.Controls.Add( new LiteralControl( sb.ToString() ) );
        }
    }
}