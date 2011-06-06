using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using System.Xml.Linq;
using System.Xml.Xsl;
using System.IO;

namespace RockWeb.Blocks.Cms
{
    public partial class PageXslt : Rock.Cms.CmsBlock
    {
        protected override void Render( HtmlTextWriter writer )
        {
            XslCompiledTransform xslTransformer = new XslCompiledTransform();
            xslTransformer.Load( Server.MapPath( "~/Assets/XSLT/PageList.xslt" ) );

            XDocument pageXml = PageInstance.MenuXml( System.Web.Security.Membership.GetUser() );

            MemoryStream ms = new MemoryStream();
            xslTransformer.Transform( pageXml.CreateReader(), null, writer );
        }
    }
}