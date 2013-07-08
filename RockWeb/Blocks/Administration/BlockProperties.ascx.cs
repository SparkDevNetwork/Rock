//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Rock;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    public partial class BlockProperties : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:Init" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            Rock.Web.UI.DialogMasterPage masterPage = this.Page.Master as Rock.Web.UI.DialogMasterPage;
            if ( masterPage != null )
            {
                masterPage.OnSave += new EventHandler<EventArgs>( masterPage_OnSave );
            }
            
            try
            {
                int blockId = Convert.ToInt32( PageParameter( "BlockId" ) );
                BlockCache _block = BlockCache.Read( blockId, CurrentPage.SiteId );

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

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            int blockId = Convert.ToInt32( PageParameter( "BlockId" ) );
            BlockCache _block = BlockCache.Read( blockId, CurrentPage.SiteId );

            if ( !Page.IsPostBack && _block.IsAuthorized( "Administrate", CurrentPerson ) )
            {
                tbBlockName.Text = _block.Name;
                tbBlockType.Text = _block.BlockType.Name;
                tbCacheDuration.Text = _block.OutputCacheDuration.ToString();
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Handles the OnSave event of the masterPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void masterPage_OnSave( object sender, EventArgs e )
        {
            int blockId = Convert.ToInt32( PageParameter( "BlockId" ) );
            BlockCache _block = BlockCache.Read( blockId, CurrentPage.SiteId );
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

                string script = @"window.parent.Rock.controls.modal.close();";
                ScriptManager.RegisterStartupScript( this.Page, this.GetType(), "close-modal", script, true );
            }
        }

        /// <summary>
        /// Displays the error.
        /// </summary>
        /// <param name="message">The message.</param>
        private void DisplayError( string message )
        {
            pnlMessage.Controls.Clear();
            pnlMessage.Controls.Add( new LiteralControl( message ) );
            pnlMessage.Visible = true;

            phContent.Visible = false;
        }
    }
}