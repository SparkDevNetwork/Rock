// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Security;
using Rock.Data;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Handles displaying and editing a block's properties.
    /// </summary>
    [DisplayName( "Block Properties" )]
    [Category( "Core" )]
    [Description( "Allows you to administrate a block's properties." )]
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
            Rock.Web.UI.DialogPage dialogPage = this.Page as Rock.Web.UI.DialogPage;
            if ( dialogPage != null )
            {
                dialogPage.OnSave += new EventHandler<EventArgs>( masterPage_OnSave );
            }
            
            try
            {
                int blockId = Convert.ToInt32( PageParameter( "BlockId" ) );
                Block _block = new BlockService( new RockContext() ).Get( blockId );

                if ( _block.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
                {
                    phAttributes.Controls.Clear();
                    phAdvancedAttributes.Controls.Clear();

                    _block.LoadAttributes();
                    if ( _block.Attributes != null )
                    {
                        foreach ( var attributeCategory in Rock.Attribute.Helper.GetAttributeCategories( _block ) )
                        {
                            if ( attributeCategory.Category != null && attributeCategory.Category.Name.Equals( "customsetting", StringComparison.OrdinalIgnoreCase ) )
                            {
                            }
                            else if (attributeCategory.Category != null && attributeCategory.Category.Name.Equals("advanced", StringComparison.OrdinalIgnoreCase))
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
                    DisplayError( "You are not authorized to edit this block", null );
                }
            }
            catch ( SystemException ex )
            {
                DisplayError( ex.Message, "<pre>" + ex.StackTrace + "</pre>" );
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

            if ( !Page.IsPostBack && _block.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
            {
                rptProperties.DataSource = _tabs;
                rptProperties.DataBind();

                tbBlockName.Text = _block.Name;
                tbCssClass.Text = _block.CssClass;
                cePreHtml.Text = _block.PreHtml;
                cePostHtml.Text = _block.PostHtml;
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
            if ( Page.IsValid )
            {
                var rockContext = new RockContext();
                var blockService = new Rock.Model.BlockService( rockContext );
                var block = blockService.Get( blockId );

                block.LoadAttributes();

                block.Name = tbBlockName.Text;
                block.CssClass = tbCssClass.Text;
                block.PreHtml = cePreHtml.Text;
                block.PostHtml = cePostHtml.Text;
                block.OutputCacheDuration = Int32.Parse( tbCacheDuration.Text );
                rockContext.SaveChanges();

                Rock.Attribute.Helper.GetEditValues( phAttributes, block );
                if ( phAdvancedAttributes.Controls.Count > 0 )
                {
                    Rock.Attribute.Helper.GetEditValues( phAdvancedAttributes, block );
                }
                block.SaveAttributeValues( rockContext );

                Rock.Web.Cache.BlockCache.Flush( block.Id );

                string script = string.Format( "window.parent.Rock.controls.modal.close('BLOCK_UPDATED:{0}');", blockId );
                ScriptManager.RegisterStartupScript( this.Page, this.GetType(), "close-modal", script, true );
            }
        }

        #region Internal Methods

        /// <summary>
        /// Displays the error.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="details">The details.</param>
        private void DisplayError( string message, string details )
        {
            nbMessage.Text = message;
            nbMessage.Details = details;

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
            }
            else if ( CurrentTab.Equals( "Advanced Settings" ) )
            {
                pnlBasicProperty.Visible = false;
                pnlAdvancedSettings.Visible = true;
            }
        }

        #endregion
        
    }
}