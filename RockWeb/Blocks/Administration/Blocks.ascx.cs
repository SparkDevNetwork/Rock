//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
	public partial class Blocks : Rock.Web.UI.Block
	{
		private string _action = string.Empty;
		private int _blockId = 0;

        protected override void OnInit( EventArgs e )
        {
			_action = PageParameter( "action" ).ToLower();
			switch ( _action )
			{
				case "":
				case "list":
					DisplayList();
					break;
				case "add":
					_blockId = 0;
					DisplayEdit( _blockId );
					break;
				case "edit":
					if ( Int32.TryParse( PageParameter( "BlockId" ), out _blockId ) )
						DisplayEdit( _blockId );
					else
                        throw new System.Exception( "Invalid Block Id" );
					break;
			}
		}

        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                _action = PageParameter( "action" ).ToLower();
                switch ( _action )
                {
                    case "":
                    case "list":
                        BindGrid();
                        break;
                }
            }
        }


        private void DisplayList()
        {
            phList.Visible = true;
            phDetails.Visible = false;

            using ( new Rock.Data.UnitOfWorkScope() )
            {
                Rock.CMS.BlockService blockService = new Rock.CMS.BlockService();

                // Add any unregistered blocks
                foreach ( Rock.CMS.Block block in blockService.GetUnregisteredBlocks( Request.MapPath( "~" ) ) )
                {
                    try
                    {
                        Control control = LoadControl( block.Path );
                        if ( control is Rock.Web.UI.Block )
                        {
                            block.Name = Path.GetFileNameWithoutExtension( block.Path );
                            block.Description = block.Path;

                            blockService.Add( block, CurrentPersonId );
                            Rock.CMS.Block testBlock = block;
                            blockService.Save( block, CurrentPersonId );
                        }
                    }
                    catch
                    {
                    }
                }

            }

            rGrid.DataKeyNames = new string[] { "id" };
            rGrid.RowDeleting += new GridViewDeleteEventHandler( rGrid_RowDeleting );
            rGrid.GridRebind += new GridRebindEventHandler( rGrid_GridRebind );

            rGrid.Actions.EnableAdd = true;
            rGrid.Actions.AddClick += new EventHandler( Actions_AddClick );

            string script = string.Format( @"
    Sys.Application.add_load(function () {{
        $('td.grid-icon-cell.delete a').click(function(){{
            return confirm('Are you sure you want to delete this Block?');
            }});
    }});
", rGrid.ClientID );
            this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "grid-confirm-delete-{0}", rGrid.ClientID ), script, true );

        }

        private void BindGrid()
        {
            Rock.CMS.BlockService blockService = new Rock.CMS.BlockService();
            rGrid.DataSource = blockService.Queryable().ToList();
            rGrid.DataBind();
        }

		private void DisplayEdit( int blockId )
		{
			phList.Visible = false;
			phDetails.Visible = true;

			using ( new Rock.Data.UnitOfWorkScope() )
			{
                Rock.CMS.BlockService blockService = new Rock.CMS.BlockService();

				if ( blockId > 0 )
				{
					Rock.CMS.Block block = blockService.Get( Convert.ToInt32( PageParameter( "BlockId" ) ) );
                    if (block == null)
                        throw new System.Exception( "Invalid Block Id" );

					tbPath.Text = block.Path;
					tbName.Text = block.Name;
					tbDescription.Text = block.Description;
				}
				else
				{
					tbPath.Text = string.Empty;
					tbName.Text = string.Empty;
					tbDescription.Text = string.Empty;
				}
			}
		}

        void Actions_AddClick( object sender, EventArgs e )
        {
            Response.Redirect( "~/Bloc/Add" );
        }

        protected void rGrid_RowDeleting( object sender, GridViewDeleteEventArgs e )
        {
            Rock.CMS.BlockService blockService = new Rock.CMS.BlockService();
            Rock.CMS.Block block = blockService.Get( ( int )e.Keys["id"] );
            if ( block != null )
            {
                blockService.Delete( block, CurrentPersonId );
                blockService.Save( block, CurrentPersonId );
            }

            BindGrid();
        }

        void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        protected void lbSave_Click( object sender, EventArgs e )
		{
			using ( new Rock.Data.UnitOfWorkScope() )
			{
                Rock.CMS.BlockService blockService = new Rock.CMS.BlockService();

				Rock.CMS.Block block = _action == "add" ?
					new Rock.CMS.Block() :
					blockService.Get( _blockId );

				block.Path = tbPath.Text;
				block.Name = tbName.Text;
				block.Description = tbDescription.Text;

				if ( _action == "add" )
                    blockService.Add( block, CurrentPersonId );
				blockService.Save( block, CurrentPersonId );

				Response.Redirect( "~/Bloc/list" );
			}
		}
	}
}