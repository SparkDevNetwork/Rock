using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RockWeb.Blocks.Cms
{
	public partial class Blocks : Rock.Cms.CmsBlock
	{
		private string _action = string.Empty;
		private int _blockId = 0;

		protected void Page_Init( object sender, EventArgs e )
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

		private void DisplayList()
		{
			phList.Visible = true;
			phDetails.Visible = false;

			Rock.Services.Cms.BlockService service = new Rock.Services.Cms.BlockService();
			gvList.DataSource = service.Queryable().ToList();
			gvList.DataBind();
		}

		private void DisplayEdit( int blockId )
		{
			phList.Visible = false;
			phDetails.Visible = true;

			using ( new Rock.Helpers.UnitOfWorkScope() )
			{
                Rock.Services.Cms.BlockService blockService = new Rock.Services.Cms.BlockService();

				if ( blockId > 0 )
				{
					Rock.Models.Cms.Block block = blockService.GetBlock( Convert.ToInt32( PageParameter( "BlockId" ) ) );
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

		protected void lbSave_Click( object sender, EventArgs e )
		{
			using ( new Rock.Helpers.UnitOfWorkScope() )
			{
                Rock.Services.Cms.BlockService blockService = new Rock.Services.Cms.BlockService();

				Rock.Models.Cms.Block block = _action == "add" ?
					new Rock.Models.Cms.Block() :
					blockService.GetBlock( _blockId );

				block.Path = tbPath.Text;
				block.Name = tbName.Text;
				block.Description = tbDescription.Text;

				if ( _action == "add" )
					blockService.AddBlock( block );
				blockService.Save( block, CurrentPersonId );

				Response.Redirect( "~/Bloc/list" );
			}
		}
	}
}