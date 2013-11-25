//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// Handles displaying and editing a block's properties.
    /// </summary>
    public partial class BlockProperties : RockBlock
    {
        #region Fields

        private readonly List<string> _tabs = new List<string> { "Basic Settings", "Advanced Settings" };

        /// <summary>
        /// Gets or sets the current tab.
        /// </summary>
        /// <value>
        /// The current tab.
        /// </value>
        protected string CurrentTab
        {
            get
            {
                object currentProperty = ViewState["CurrentTab"];
                return currentProperty != null ? currentProperty.ToString() : "Basic Settings";
            }

            set
            {
                ViewState["CurrentTab"] = value;
            }
        }

        #endregion

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
                BlockCache _block = BlockCache.Read( blockId );

                if ( _block.IsAuthorized( "Administrate", CurrentPerson ) )
                {
                    phAttributes.Controls.Clear();
                    phAdvancedAttributes.Controls.Clear();

                    if ( _block.Attributes != null )
                    {
                        foreach ( var attributeCategory in Rock.Attribute.Helper.GetAttributeCategories( _block ) )
                        { 
                            if (attributeCategory.Category != null && attributeCategory.Category.Name.Equals("advanced", StringComparison.OrdinalIgnoreCase))
                            {
                                Rock.Attribute.Helper.AddEditControls(
                                    string.Empty, attributeCategory.Attributes.Select( a => a.Key ).ToList(),
                                    _block, phAdvancedAttributes, string.Empty, !Page.IsPostBack, new List<string>());
                            }
                            else
                            {
                                Rock.Attribute.Helper.AddEditControls(
                                    attributeCategory.Category != null ? attributeCategory.Category.Name : string.Empty,
                                    attributeCategory.Attributes.Select( a => a.Key ).ToList(),
                                    _block, phAttributes, string.Empty, !Page.IsPostBack, new List<string>() );
                            }
                        }
                    }
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
            BlockCache _block = BlockCache.Read( blockId );

            if ( !Page.IsPostBack && _block.IsAuthorized( "Administrate", CurrentPerson ) )
            {
                rptProperties.DataSource = _tabs;
                rptProperties.DataBind();

                tbBlockName.Text = _block.Name;
                tbCssClass.Text = _block.CssClass;
                tbCacheDuration.Text = _block.OutputCacheDuration.ToString();
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Handles the Click event of the lbProperty control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbProperty_Click( object sender, EventArgs e )
        {
            LinkButton lb = sender as LinkButton;
            if ( lb != null )
            {
                CurrentTab = lb.Text;

                rptProperties.DataSource = _tabs;
                rptProperties.DataBind();
            }

            ShowSelectedPane();
        }

        /// <summary>
        /// Handles the OnSave event of the masterPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void masterPage_OnSave( object sender, EventArgs e )
        {
            int blockId = Convert.ToInt32( PageParameter( "BlockId" ) );
            BlockCache _block = BlockCache.Read( blockId );
            if ( Page.IsValid )
            {
                using ( new Rock.Data.UnitOfWorkScope() )
                {
                    var blockService = new Rock.Model.BlockService();
                    var block = blockService.Get( _block.Id );

                    block.LoadAttributes();

                    block.Name = tbBlockName.Text;
                    block.CssClass = tbCssClass.Text;
                    block.OutputCacheDuration = Int32.Parse( tbCacheDuration.Text );
                    blockService.Save( block, CurrentPersonId );

                    Rock.Attribute.Helper.GetEditValues( phAttributes, _block );
                    if ( phAdvancedAttributes.Controls.Count > 0 )
                    {
                        Rock.Attribute.Helper.GetEditValues( phAdvancedAttributes, _block );
                    }
                    _block.SaveAttributeValues( CurrentPersonId );

                    Rock.Web.Cache.BlockCache.Flush( _block.Id );
                }

                string script = @"window.parent.Rock.controls.modal.close();";
                ScriptManager.RegisterStartupScript( this.Page, this.GetType(), "close-modal", script, true );
            }
        }

        #region Internal Methods

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

        /// <summary>
        /// Gets the tab class.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        protected string GetTabClass( object property )
        {
            if ( property.ToString() == CurrentTab )
            {
                return "active";
            }

            return string.Empty;
        }

        /// <summary>
        /// Shows the selected pane.
        /// </summary>
        private void ShowSelectedPane()
        {
            if ( CurrentTab.Equals( "Basic Settings" ) )
            {
                pnlBasicProperty.Visible = true;
                pnlAdvancedSettings.Visible = false;
                pnlBasicProperty.DataBind();
            }
            else if ( CurrentTab.Equals( "Advanced Settings" ) )
            {
                pnlBasicProperty.Visible = false;
                pnlAdvancedSettings.Visible = true;
                pnlAdvancedSettings.DataBind();
            }
        }

        #endregion
        
    }
}