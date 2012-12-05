//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;

using Rock;

namespace RockWeb.Blocks.Administration
{
    public partial class BlockProperties : Rock.Web.UI.RockBlock
    {
        private Rock.Web.Cache.BlockCache _block = null;
        private string _zoneName = string.Empty;

        protected override void OnInit( EventArgs e )
        {
            Rock.Web.UI.DialogMasterPage masterPage = this.Page.Master as Rock.Web.UI.DialogMasterPage;
            if ( masterPage != null )
                masterPage.OnSave += new EventHandler<EventArgs>( masterPage_OnSave );
            
            try
            {
                int blockId = Convert.ToInt32( PageParameter( "BlockId" ) );
                _block = Rock.Web.Cache.BlockCache.Read( blockId );

                if ( _block.IsAuthorized( "Administrate", CurrentPerson ) )
                {
                    phAttributes.Controls.Clear();
                    Rock.Attribute.Helper.AddEditControls( _block, phAttributes, !Page.IsPostBack );
                }
                else
                {
                    DisplayError( "You are not authorized to edit this block" );
                }
            }
            catch ( SystemException ex )
            {
                DisplayError( ex.Message );
            }

            base.OnInit( e );
        }

        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack && _block.IsAuthorized( "Administrate", CurrentPerson ) )
            {
                tbBlockName.Text = _block.Name;
                tbCacheDuration.Text = _block.OutputCacheDuration.ToString();
            }

            base.OnLoad( e );
        }

        protected void masterPage_OnSave( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                using ( new Rock.Data.UnitOfWorkScope() )
                {
                    var blockService = new Rock.Model.BlockService();
                    var block = blockService.Get( _block.Id );

                    block.LoadAttributes();

                    block.Name = tbBlockName.Text;
                    block.OutputCacheDuration = Int32.Parse( tbCacheDuration.Text );
                    blockService.Save( block, CurrentPersonId );

                    Rock.Attribute.Helper.GetEditValues( phAttributes, _block );
                    _block.SaveAttributeValues( CurrentPersonId );

                    Rock.Web.Cache.BlockCache.Flush( _block.Id );
                }

                string script = "window.parent.closeModal()";
                ScriptManager.RegisterStartupScript( this.Page, this.GetType(), "close-modal", script, true );
            }
            else
                Rock.Attribute.Helper.SetErrorIndicators( phAttributes, _block );

        }

        private void DisplayError( string message )
        {
            pnlMessage.Controls.Clear();
            pnlMessage.Controls.Add( new LiteralControl( message ) );
            pnlMessage.Visible = true;

            phContent.Visible = false;
        }
    }
}