//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace RockWeb.Blocks.Administration
{
    public partial class BlockProperties : Rock.Web.UI.Block
    {
        private Rock.Web.Cache.BlockInstance _blockInstance = null;
        private string _zoneName = string.Empty;

        protected override void OnInit( EventArgs e )
        {
            Rock.Web.UI.DialogMasterPage masterPage = this.Page.Master as Rock.Web.UI.DialogMasterPage;
            if ( masterPage != null )
                masterPage.OnSave += new EventHandler<EventArgs>( masterPage_OnSave );
            
            try
            {
                int blockInstanceId = Convert.ToInt32( PageParameter( "BlockInstance" ) );
                _blockInstance = Rock.Web.Cache.BlockInstance.Read( blockInstanceId );

                if ( _blockInstance.IsAuthorized( "Configure", CurrentUser ) )
                {
                    var attributeControls = Rock.Attribute.Helper.GetEditControls( _blockInstance, !Page.IsPostBack );
                    foreach ( HtmlGenericControl fs in attributeControls )
                        phAttributes.Controls.Add( fs );
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
            if ( !Page.IsPostBack && _blockInstance.IsAuthorized( "Configure", CurrentUser ) )
            {
                tbBlockName.Text = _blockInstance.Name;
                tbCacheDuration.Text = _blockInstance.OutputCacheDuration.ToString();
            }

            base.OnLoad( e );
        }

        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            if ( Page.IsPostBack && !Page.IsValid )
                Rock.Attribute.Helper.SetErrorIndicators( phAttributes, _blockInstance );
        }

        protected void masterPage_OnSave( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                using ( new Rock.Data.UnitOfWorkScope() )
                {
                    Rock.CMS.BlockInstanceService blockInstanceService = new Rock.CMS.BlockInstanceService();
                    Rock.CMS.BlockInstance blockInstance = blockInstanceService.Get( _blockInstance.Id );

                    Rock.Attribute.Helper.LoadAttributes( blockInstance );

                    blockInstance.Name = tbBlockName.Text;
                    blockInstance.OutputCacheDuration = Int32.Parse( tbCacheDuration.Text );
                    blockInstanceService.Save( blockInstance, CurrentPersonId );

                    Rock.Attribute.Helper.GetEditValues( phAttributes, _blockInstance );
                    _blockInstance.SaveAttributeValues( CurrentPersonId );

                    Rock.Web.Cache.BlockInstance.Flush( _blockInstance.Id );
                }

                string script = "window.parent.closeModal()";
                ScriptManager.RegisterStartupScript( this.Page, this.GetType(), "close-modal", script, true );
            }
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