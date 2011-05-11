using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace RockWeb.Blocks
{
    public partial class TestGrid : Rock.Cms.CmsBlock
    {
        protected override void OnInit( EventArgs e )
        {
            Rock.Services.Cms.PageService pageService = new Rock.Services.Cms.PageService();
            rGrid.DataSource = pageService.Queryable().ToList();
            rGrid.DataBind();

            base.OnInit( e );
        }
    }
}