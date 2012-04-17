//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

using Rock.Web.UI.Controls;

namespace RockWeb.Blocks
{
    public partial class TestGrid : Rock.Web.UI.Block
    {
        Rock.CMS.PageRepository pageRepository = new Rock.CMS.PageRepository();

        protected override void OnInit( EventArgs e )
        {
            rGrid.DataKeyNames = new string[] { "id" };
            rGrid.Actions.EnableAdd = true;
            //rGrid.Actions.EnableExcelExport = true;
            //rGrid.ClientAddScript = "return addItem();";
            rGrid.Actions.AddClick += rGrid_GridAdd;
            rGrid.RowDeleting += new GridViewDeleteEventHandler( rGrid_RowDeleting );
            rGrid.GridReorder += new GridReorderEventHandler( rGrid_GridReorder );
            rGrid.GridRebind += new GridRebindEventHandler( rGrid_GridRebind );

            string script = string.Format( @"
    Sys.Application.add_load(function () {{
        $('{0} td.grid-icon-cell.delete a').click(function(){{
            return confirm('Are you sure you want to delete this Page?');
            }});
    }});
", rGrid.ClientID );

            this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "grid-confirm-delete-{0}", rGrid.ClientID ), script, true );

            base.OnInit( e );
        }

        protected override void OnLoad( EventArgs e )
        {
            BindGrid();
            base.OnLoad( e );
        }

        private void BindGrid()
        {
            rGrid.DataSource = pageRepository.GetByParentPageId( null ).ToList();
            rGrid.DataBind();
        }

        void rGrid_GridAdd( object sender, EventArgs e )
        {
            Rock.CMS.Page page = new Rock.CMS.Page();
            page.Name = "New Page";

            Rock.CMS.Page lastPage = pageRepository.AsQueryable().
                Where( p => !p.ParentPageId.HasValue).
                OrderByDescending( b => b.Order ).FirstOrDefault();

            if ( lastPage != null )
                page.Order = lastPage.Order + 1;
            else
                page.Order = 0;

            pageRepository.Add( page, CurrentPersonId );
            pageRepository.Save( page, CurrentPersonId );

            BindGrid();
        }

        protected void rGrid_RowDeleting( object sender, GridViewDeleteEventArgs e )
        {
            Rock.CMS.Page page = pageRepository.Get((int)e.Keys["id"]);
            if ( page != null )
            {
                pageRepository.Delete( page, CurrentPersonId );
                pageRepository.Save( page, CurrentPersonId );
            }

            BindGrid();
        }

        void rGrid_GridReorder( object sender, GridReorderEventArgs e )
        {
            pageRepository.Reorder( (List<Rock.CMS.Page>)rGrid.DataSource, 
                e.OldIndex, e.NewIndex, CurrentPersonId );
        }

        void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }
    }
}