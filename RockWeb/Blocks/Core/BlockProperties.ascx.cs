﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
using System.Web;
using Rock.Web.UI.Controls;
using System.Text;

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

        private CustomGridColumnsConfig CustomGridColumnsConfigState
        {
            get
            {
                return ViewState["CustomGridColumnsConfig"] as CustomGridColumnsConfig;
            }

            set
            {
                ViewState["CustomGridColumnsConfig"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the blocktype implements ICustomGridColumns
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is custom grid columns block; otherwise, <c>false</c>.
        /// </value>
        private bool ShowCustomGridColumns { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this blocktype implements ICustomGridOptions
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is custom columns columns block; otherwise, <c>false</c>.
        /// </value>
        private bool ShowCustomGridOptions { get; set; }

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
                int blockId = PageParameter( "BlockId" ).AsInteger();
                var _block = BlockCache.Get( blockId );
                dialogPage.Title = _block.BlockType.Name;
                dialogPage.SubTitle = string.Format("{0} / Id: {1}", _block.BlockType.Category, blockId);

                if ( _block.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
                {
                    var blockTypeId = _block.BlockTypeId;
                    var blockType = BlockTypeCache.Get( blockTypeId );
                    if ( blockType != null && !blockType.IsInstancePropertiesVerified )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            string blockTypePath = BlockTypeCache.Get( blockTypeId ).Path;
                            var blockCompiledType = System.Web.Compilation.BuildManager.GetCompiledType( blockTypePath );
                            int? blockEntityTypeId = EntityTypeCache.Get( typeof( Block ) ).Id;
                            bool attributesUpdated = Rock.Attribute.Helper.UpdateAttributes( blockCompiledType, blockEntityTypeId, "BlockTypeId", blockTypeId.ToString(), rockContext );
                            BlockTypeCache.Get( blockTypeId ).MarkInstancePropertiesVerified( true );
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
                DisplayError( ex.Message, "<pre>" + HttpUtility.HtmlEncode( ex.StackTrace ) + "</pre>" );
            }

            base.OnInit( e );
        }

        /// <summary>
        /// Gets the tabs.
        /// </summary>
        /// <param name="blockType">Type of the block.</param>
        /// <returns></returns>
        private List<string> GetTabs( BlockTypeCache blockType )
        {
            var result = new List<string> { "Basic Settings", "Advanced Settings" };

            if ( this.ShowCustomGridOptions || this.ShowCustomGridColumns )
            {
                result.Add( "Custom Grid Options" );
            }

            return result;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            int blockId = Convert.ToInt32( PageParameter( "BlockId" ) );
            BlockCache _block = BlockCache.Get( blockId );

            var blockControlType = System.Web.Compilation.BuildManager.GetCompiledType( _block.BlockType.Path );
            this.ShowCustomGridColumns = typeof( Rock.Web.UI.ICustomGridColumns ).IsAssignableFrom( blockControlType );
            this.ShowCustomGridOptions = typeof( Rock.Web.UI.ICustomGridOptions ).IsAssignableFrom( blockControlType );

            if ( !Page.IsPostBack && _block.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
            {
                if ( _block.Attributes != null )
                {
                    avcAdvancedAttributes.IncludedCategoryNames = new string[] { "advanced" };
                    avcAdvancedAttributes.AddEditControls( _block );

                    avcAttributes.ExcludedCategoryNames = new string[] { "advanced", "customsetting" };
                    avcAttributes.AddEditControls( _block );
                }

                rptProperties.DataSource = GetTabs(_block.BlockType );
                rptProperties.DataBind();

                tbBlockName.Text = _block.Name;
                tbCssClass.Text = _block.CssClass;
                cePreHtml.Text = _block.PreHtml;
                cePostHtml.Text = _block.PostHtml;

                // Hide the Cache duration block for now;
                tbCacheDuration.Visible = false;
                //tbCacheDuration.Text = _block.OutputCacheDuration.ToString();

                rcwCustomGridColumns.Visible = this.ShowCustomGridColumns;
                tglEnableStickyHeader.Visible = this.ShowCustomGridOptions;

                if ( this.ShowCustomGridColumns )
                {
                    CustomGridColumnsConfigState = _block.GetAttributeValue( CustomGridColumnsConfig.AttributeKey ).FromJsonOrNull<CustomGridColumnsConfig>() ?? new CustomGridColumnsConfig();
                    BindCustomColumnsConfig();
                }
                else
                {
                    CustomGridColumnsConfigState = null;
                }

                if ( this.ShowCustomGridOptions )
                {
                    tglEnableStickyHeader.Checked = _block.GetAttributeValue( CustomGridOptionsConfig.EnableStickyHeadersAttributeKey ).AsBoolean();
                }
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

                int blockId = Convert.ToInt32( PageParameter( "BlockId" ) );
                BlockCache _block = BlockCache.Get( blockId );
                rptProperties.DataSource = GetTabs( _block.BlockType );
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
            bool reloadPage = false;
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
                block.OutputCacheDuration = 0; //Int32.Parse( tbCacheDuration.Text );
                rockContext.SaveChanges();

                avcAttributes.GetEditValues( block );
                avcAdvancedAttributes.GetEditValues( block );

                SaveCustomColumnsConfigToViewState();
                if ( this.CustomGridColumnsConfigState != null && this.CustomGridColumnsConfigState.ColumnsConfig.Any() )
                {
                    if ( !block.Attributes.Any( a => a.Key == CustomGridColumnsConfig.AttributeKey ) )
                    {
                        block.Attributes.Add( CustomGridColumnsConfig.AttributeKey, null );
                    }

                    var customGridColumnsJSON = this.CustomGridColumnsConfigState.ToJson();
                    if ( block.GetAttributeValue( CustomGridColumnsConfig.AttributeKey ) != customGridColumnsJSON )
                    {
                        block.SetAttributeValue( CustomGridColumnsConfig.AttributeKey, customGridColumnsJSON );

                        // if the CustomColumns changed, reload the whole page so that we can avoid issues with columns changing between postbacks
                        reloadPage = true;
                    }
                }
                else
                {
                    if ( block.Attributes.Any( a => a.Key == CustomGridColumnsConfig.AttributeKey ) )
                    {
                        if ( block.GetAttributeValue( CustomGridColumnsConfig.AttributeKey ) != null )
                        {
                            // if the CustomColumns were removed, reload the whole page so that we can avoid issues with columns changing between postbacks
                            reloadPage = true;
                        }

                        block.SetAttributeValue( CustomGridColumnsConfig.AttributeKey, null );
                    }
                }

                if ( tglEnableStickyHeader.Checked )
                {
                    if ( !block.Attributes.Any( a => a.Key == CustomGridOptionsConfig.EnableStickyHeadersAttributeKey ) )
                    {
                        block.Attributes.Add( CustomGridOptionsConfig.EnableStickyHeadersAttributeKey, null );
                    }
                }

                if ( block.GetAttributeValue( CustomGridOptionsConfig.EnableStickyHeadersAttributeKey ).AsBoolean() != tglEnableStickyHeader.Checked )
                {
                    block.SetAttributeValue( CustomGridOptionsConfig.EnableStickyHeadersAttributeKey, tglEnableStickyHeader.Checked.ToTrueFalse() );

                    // if EnableStickyHeaders changed, reload the page
                    reloadPage = true;
                }

                block.SaveAttributeValues( rockContext );

                // If this is a page menu block then we need to also flush the LavaTemplateCache for the block ID
                if ( block.BlockType.Guid == Rock.SystemGuid.BlockType.PAGE_MENU.AsGuid() )
                {
                    var cacheKey = string.Format( "Rock:PageMenu:{0}", block.Id );
                    LavaTemplateCache.Remove( cacheKey );
                }

                StringBuilder scriptBuilder = new StringBuilder();

                if ( reloadPage )
                {
                    scriptBuilder.AppendLine( "window.parent.location.reload();" );
                }
                else
                {
                    scriptBuilder.AppendLine( string.Format( "window.parent.Rock.controls.modal.close('BLOCK_UPDATED:{0}');", blockId ) );
                }

                ScriptManager.RegisterStartupScript( this.Page, this.GetType(), "close-modal", scriptBuilder.ToString(), true );
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
            pnlAdvancedSettings.Visible = CurrentTab.Equals( "Advanced Settings" );
            pnlBasicProperty.Visible = CurrentTab.Equals( "Basic Settings" );
            pnlCustomGridTab.Visible = CurrentTab.Equals( "Custom Grid Options" );
        }

        #endregion

        #region Custom Grid Columns

        /// <summary>
        /// Binds the custom columns configuration.
        /// </summary>
        private void BindCustomColumnsConfig()
        {
            int blockId = Convert.ToInt32( PageParameter( "BlockId" ) );
            BlockCache _block = BlockCache.Get( blockId );

            rptCustomGridColumns.DataSource = CustomGridColumnsConfigState.ColumnsConfig;
            rptCustomGridColumns.DataBind();
        }

        /// <summary>
        /// Saves the state of the custom columns configuration to view.
        /// </summary>
        private void SaveCustomColumnsConfigToViewState()
        {
            this.CustomGridColumnsConfigState = new CustomGridColumnsConfig();
            foreach ( var item in rptCustomGridColumns.Items.OfType<RepeaterItem>())
            {
                var columnConfig = new CustomGridColumnsConfig.ColumnConfig();

                var nbRelativeOffset = item.FindControl( "nbRelativeOffset" ) as NumberBox;
                columnConfig.PositionOffset = nbRelativeOffset.Text.AsInteger();

                var ddlOffsetType = item.FindControl( "ddlOffsetType" ) as RockDropDownList;
                columnConfig.PositionOffsetType = ddlOffsetType.SelectedValueAsEnum<CustomGridColumnsConfig.ColumnConfig.OffsetType>();

                var tbHeaderText = item.FindControl( "tbHeaderText" ) as RockTextBox;
                columnConfig.HeaderText = tbHeaderText.Text;
                var tbHeaderClass = item.FindControl( "tbHeaderClass" ) as RockTextBox;
                columnConfig.HeaderClass = tbHeaderClass.Text;
                var tbItemClass = item.FindControl( "tbItemClass" ) as RockTextBox;
                columnConfig.ItemClass = tbItemClass.Text;
                var ceLavaTemplate = item.FindControl( "ceLavaTemplate" ) as CodeEditor;
                columnConfig.LavaTemplate = ceLavaTemplate.Text;

                this.CustomGridColumnsConfigState.ColumnsConfig.Add( columnConfig );
            }
        }


        /// <summary>
        /// Handles the Click event of the lbAddColumns control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddColumns_Click( object sender, EventArgs e )
        {
            SaveCustomColumnsConfigToViewState();
            this.CustomGridColumnsConfigState.ColumnsConfig.Add( new CustomGridColumnsConfig.ColumnConfig() );
            BindCustomColumnsConfig();
        }

        /// <summary>
        /// Handles the Click event of the btnDeleteColumn control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDeleteColumn_Click( object sender, EventArgs e )
        {
            SaveCustomColumnsConfigToViewState();
            int? columnIndex = ( sender as LinkButton ).CommandArgument.AsIntegerOrNull();
            if ( columnIndex.HasValue )
            {
                this.CustomGridColumnsConfigState.ColumnsConfig.RemoveAt( columnIndex.Value );
            }

            BindCustomColumnsConfig();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptCustomGridColumns control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptCustomGridColumns_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            CustomGridColumnsConfig.ColumnConfig columnConfig = e.Item.DataItem as CustomGridColumnsConfig.ColumnConfig;
            if ( columnConfig != null )
            {
                var nbRelativeOffset = e.Item.FindControl( "nbRelativeOffset" ) as NumberBox;
                nbRelativeOffset.Text = columnConfig.PositionOffset.ToString();

                var ddlOffsetType = e.Item.FindControl( "ddlOffsetType" ) as RockDropDownList;
                ddlOffsetType.SetValue( (int)columnConfig.PositionOffsetType );
                ddlOffsetType_SelectedIndexChanged( ddlOffsetType, null );

                var tbHeaderText = e.Item.FindControl( "tbHeaderText" ) as RockTextBox;
                tbHeaderText.Text = columnConfig.HeaderText;
                var tbHeaderClass = e.Item.FindControl( "tbHeaderClass" ) as RockTextBox;
                tbHeaderClass.Text = columnConfig.HeaderClass;
                var tbItemClass = e.Item.FindControl( "tbItemClass" ) as RockTextBox;
                tbItemClass.Text = columnConfig.ItemClass;

                var ceLavaTemplate = e.Item.FindControl( "ceLavaTemplate" ) as CodeEditor;
                ceLavaTemplate.Text = columnConfig.LavaTemplate;
                var btnDeleteColumn = e.Item.FindControl( "btnDeleteColumn" ) as LinkButton;
                btnDeleteColumn.CommandName = "ColumnIndex";
                btnDeleteColumn.CommandArgument = e.Item.ItemIndex.ToString();
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlOffsetType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlOffsetType_SelectedIndexChanged( object sender, EventArgs e )
        {
            var ddlOffsetType = sender as RockDropDownList;
            if ( ddlOffsetType != null )
            {
                var repeaterItem = ddlOffsetType.BindingContainer as System.Web.UI.WebControls.RepeaterItem;
                if ( repeaterItem != null )
                {
                    NumberBox nbRelativeOffset = repeaterItem.FindControl( "nbRelativeOffset" ) as NumberBox;
                    if ( ddlOffsetType.SelectedValue.AsInteger() == 1 )
                    {
                        nbRelativeOffset.PrependText = "<i class='fa fa-minus'></i>";
                    }
                    else
                    {
                        nbRelativeOffset.PrependText = "<i class='fa fa-plus'></i>";
                    }
                }
            }
        }

        #endregion
    }
}