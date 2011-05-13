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
        Rock.Services.Cms.PageService pageService = new Rock.Services.Cms.PageService();
        List<Rock.Models.Cms.Page> pages;

        protected override void OnInit( EventArgs e )
        {
            rGrid.GridReorder += new Rock.Controls.GridReorderEventHandler( rGrid_GridReorder );

            pages = pageService.GetPagesByParentPageId( null ).ToList();
            rGrid.DataSource = pages;
            rGrid.DataBind();

            base.OnInit( e );
        }

        void rGrid_GridReorder( object sender, Rock.Controls.GridReorderEventArgs e )
        {
            Rock.Models.Cms.Page page = pages[e.OldIndex];
            pages.RemoveAt( e.OldIndex );
            pages.Insert( e.NewIndex > e.OldIndex ? e.NewIndex - 1 : e.NewIndex, page );

            pageService.SaveOrder( pages, CurrentPersonId );
        }
    }
}