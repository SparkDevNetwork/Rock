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
    [BlockInstanceProperty( "XSLT File", "The path to the XSLT File ", "~/Assets/XSLT/PageList.xslt" )]
    [BlockInstanceProperty( "Root Page", "The root page to use for the pages" )]
    public partial class PageXslt : Rock.Cms.CmsBlock
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.AttributesUpdated += new AttributesUpdatedEventHandler( PageXslt_AttributesUpdated );
            this.AddAttributeUpdateTrigger( upContent );

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

            if ( AttributeValue( "RootPage" ) != string.Empty )
            {
                int pageId = Convert.ToInt32( AttributeValue("RootPage") );
                if ( pageId == -1 )
                    rootPage = PageInstance;
                else
                    rootPage = Rock.Cms.Cached.Page.Read( pageId );
            }
            else
                rootPage = PageInstance;

            XDocument pageXml = rootPage.MenuXml( System.Web.Security.Membership.GetUser() );

            StringBuilder sb = new StringBuilder();
            TextWriter tw = new StringWriter( sb );
            xslTransformer.Transform( pageXml.CreateReader(), null, tw );

            phContent.Controls.Clear();
            phContent.Controls.Add( new LiteralControl( sb.ToString() ) );
        }
    }
}