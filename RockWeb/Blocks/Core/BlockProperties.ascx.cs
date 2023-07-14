// <copyright>
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
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Blocks;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Security;
using Rock.Data;
using System.Web;
using Rock.Web.UI.Controls;
using System.Text;
using Rock.Web;
using Rock.Lava;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Handles displaying and editing a block's properties.
    /// </summary>
    [DisplayName( "Block Properties" )]
    [Category( "Core" )]
    [Description( "Allows you to administrate a block's properties." )]
    [Rock.SystemGuid.BlockTypeGuid( "5EC45388-83D4-4E99-BF25-3FA00327F08B" )]
    public partial class BlockProperties : RockBlock
    {
        #region Fields

        /// <summary>
        /// Gets or sets the state of the custom launchers configuration.
        /// </summary>
        /// <value>
        /// The state of the custom launchers configuration.
        /// </value>
        private List<CustomActionConfig> CustomActionsConfigState
        {
            get
            {
                return ( ViewState["CustomLaunchersConfigState"] as List<CustomActionConfig> ) ?? new List<CustomActionConfig>();
            }

            set
            {
                ViewState["CustomLaunchersConfigState"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the state of the custom grid columns configuration.
        /// </summary>
        /// <value>
        /// The state of the custom grid columns configuration.
        /// </value>
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
        /// Gets or sets a value indicating whether the blocktype supports custom columns.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this custom grid columns should be shown; otherwise, <c>false</c>.
        /// </value>
        private bool ShowCustomGridColumns { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this blocktype supports custom sticky header option.
        /// </summary>
        /// <value>
        ///   <c>true</c> if sticky header option should be shown; otherwise, <c>false</c>.
        /// </value>
        private bool ShowCustomGridStickyHeader { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this blocktype implements custom grid actions
        /// </summary>
        /// <value>
        ///   <c>true</c> if custom grid actions should be shown; otherwise, <c>false</c>.
        /// </value>
        private bool ShowCustomGridActions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this blocktype has any 'custommobile' category attributes.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance should show the Mobile Options tab; otherwise, <c>false</c>.
        /// </value>
        private bool ShowMobileOptions { get; set; }

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

        /// <summary>
        /// Gets or sets the custom settings providers. These are defined by RockCustomSettingsProvider instances.
        /// </summary>
        /// <value>
        /// The custom settings providers.
        /// </value>
        protected Dictionary<RockCustomSettingsProvider, Control> CustomSettingsProviders { get; set; }

        #endregion

        /// <summary>
        /// Raises the <see cref="E:Init" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            try
            {
                int? blockId = PageParameter( "BlockId" ).AsIntegerOrNull();
                if ( !blockId.HasValue )
                {
                    return;
                }

                var _block = BlockCache.Get( blockId.Value );

                Rock.Web.UI.DialogPage dialogPage = this.Page as Rock.Web.UI.DialogPage;
                if ( dialogPage != null )
                {
                    dialogPage.OnSave += new EventHandler<EventArgs>( masterPage_OnSave );
                    dialogPage.Title = _block.BlockType.Name;
                    dialogPage.SubTitle = string.Format( "{0} / Id: {1}", _block.BlockType.Category, blockId );
                }

                if ( _block.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
                {
                    var blockTypeId = _block.BlockTypeId;
                    var blockType = BlockTypeCache.Get( blockTypeId );
                    if ( blockType != null && !blockType.IsInstancePropertiesVerified )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            var blockCompiledType = _block.BlockType.GetCompiledType();
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

            LoadCustomSettingsTabs();
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            //
            // Ensure the proper tab is selected if it's a custom tab.
            //
            ShowSelectedPane();
        }

        /// <summary>
        /// Gets the tabs.
        /// </summary>
        /// <param name="blockType">Type of the block.</param>
        /// <returns></returns>
        private List<string> GetTabs( BlockTypeCache blockType )
        {
            var result = new List<string> { "Basic Settings", "Advanced Settings" };

            if ( this.ShowMobileOptions )
            {
                result.Insert( 1, "Mobile Local Settings" );
            }

            if ( this.ShowCustomGridActions || this.ShowCustomGridColumns || this.ShowCustomGridStickyHeader )
            {
                result.Add( "Custom Grid Options" );
            }

            var customSettingTabNames = CustomSettingsProviders.Keys
                .Where( p => p.CustomSettingsTitle != "Basic Settings" )
                .Where( p => p.CustomSettingsTitle != "Advanced Settings" )
                .Select( p => p.CustomSettingsTitle );
            result.AddRange( customSettingTabNames );

            return result;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            Rock.Web.UI.DialogPage dialogPage = this.Page as Rock.Web.UI.DialogPage;
            if ( dialogPage != null )
            {
                dialogPage.ValidationGroup = this.BlockValidationGroup;
            }

            valSummaryTop.ValidationGroup = this.BlockValidationGroup;

            // Set the validation group on any custom settings providers.
            SetValidationGroup( CustomSettingsProviders.Values.ToArray(), this.BlockValidationGroup );

            int? blockId = PageParameter( "BlockId" ).AsIntegerOrNull();
            if ( !blockId.HasValue )
            {
                return;
            }

            var _block = BlockCache.Get( blockId.Value );
            SiteCache _site = null;

            // Get site info from Page -> Layout -> Site
            if ( _block.Page != null )
            {
                _site = SiteCache.Get( _block.Page.SiteId );
            }
            else if ( _block.Layout != null )
            {
                _site = SiteCache.Get( _block.Layout.SiteId );
            }
            else if ( _block.SiteId.HasValue )
            {
                _site = SiteCache.Get( _block.SiteId.Value );
            }

            // Change Pre/Post text labels if this is a mobile block
            if ( _site != null &&  _site.SiteType == SiteType.Mobile )
            {
                cePostHtml.Label = "Post-XAML";
                cePreHtml.Label = "Pre-XAML";
            }

            var blockControlType = _block.BlockType.GetCompiledType();

            var customizedGrid = blockControlType.GetCustomAttribute<CustomizedGridAttribute>();

            this.ShowCustomGridActions = typeof( Rock.Web.UI.ICustomGridOptions ).IsAssignableFrom( blockControlType )
                || customizedGrid?.IsCustomActionsSupported == true;
            this.ShowCustomGridColumns = typeof( Rock.Web.UI.ICustomGridColumns ).IsAssignableFrom( blockControlType )
                || customizedGrid?.IsCustomColumnsSupported == true;
            this.ShowCustomGridStickyHeader = typeof( Rock.Web.UI.ICustomGridOptions ).IsAssignableFrom( blockControlType )
                || customizedGrid?.IsStickyHeaderSupported == true;
            this.ShowMobileOptions = _block.Attributes.Any( a => a.Value.Categories.Any( c => c.Name == "custommobile" ) );

            if ( !Page.IsPostBack && _block.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
            {
                if ( _block.Attributes != null )
                {
                    avcAdvancedAttributes.IncludedCategoryNames = new string[] { "advanced" };
                    avcAdvancedAttributes.AddEditControls( _block );

                    avcMobileAttributes.IncludedCategoryNames = new string[] { "custommobile" };
                    avcMobileAttributes.AddEditControls( _block );

                    avcAttributes.ExcludedCategoryNames = new string[] { "advanced", "customsetting", "custommobile" };
                    avcAttributes.AddEditControls( _block );
                }

                foreach ( var kvp in CustomSettingsProviders )
                {
                    kvp.Key.ReadSettingsFromEntity( _block, kvp.Value );
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

                pwCustomActions.Visible = this.ShowCustomGridActions;
                pwCustomGridColumns.Visible = this.ShowCustomGridColumns;
                tglEnableStickyHeader.Visible = this.ShowCustomGridStickyHeader;

                if ( this.ShowCustomGridColumns )
                {
                    CustomGridColumnsConfigState = _block.GetAttributeValue( CustomGridColumnsConfig.AttributeKey ).FromJsonOrNull<CustomGridColumnsConfig>() ?? new CustomGridColumnsConfig();
                    BindCustomColumnsConfig();
                }
                else
                {
                    CustomGridColumnsConfigState = null;
                }

                if ( this.ShowCustomGridStickyHeader )
                {
                    tglEnableStickyHeader.Checked = _block.GetAttributeValue( CustomGridOptionsConfig.EnableStickyHeadersAttributeKey ).AsBoolean();
                }

                if ( this.ShowCustomGridActions )
                {
                    tglEnableDefaultWorkflowLauncher.Checked = _block.GetAttributeValue( CustomGridOptionsConfig.EnableDefaultWorkflowLauncherAttributeKey ).AsBoolean();

                    CustomActionsConfigState = _block.GetAttributeValue( CustomGridOptionsConfig.CustomActionsConfigsAttributeKey ).FromJsonOrNull<List<CustomActionConfig>>();
                    BindCustomActionsConfig();
                }
                else
                {
                    CustomActionsConfigState = null;
                }

                ShowSelectedPane();
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
            if ( !Page.IsValid )
            {
                return;
            }

            var rockContext = new RockContext();
            rockContext.WrapTransaction( () =>
            {
                var blockService = new Rock.Model.BlockService( rockContext );
                var block = blockService.Get( blockId );

                block.LoadAttributes();

                block.Name = tbBlockName.Text;
                block.CssClass = tbCssClass.Text;
                block.PreHtml = cePreHtml.Text;
                block.PostHtml = cePostHtml.Text;
                block.OutputCacheDuration = 0; //Int32.Parse( tbCacheDuration.Text );

                avcAttributes.GetEditValues( block );
                avcMobileAttributes.GetEditValues( block );
                avcAdvancedAttributes.GetEditValues( block );

                foreach ( var kvp in CustomSettingsProviders )
                {
                    kvp.Key.WriteSettingsToEntity( block, kvp.Value, rockContext );
                }

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

                // Save the custom action configs
                SaveCustomActionsConfigToViewState();

                if ( CustomActionsConfigState != null && CustomActionsConfigState.Any() )
                {
                    if ( !block.Attributes.Any( a => a.Key == CustomGridOptionsConfig.CustomActionsConfigsAttributeKey ) )
                    {
                        block.Attributes.Add( CustomGridOptionsConfig.CustomActionsConfigsAttributeKey, null );
                    }

                    var json = CustomActionsConfigState.ToJson();

                    if ( block.GetAttributeValue( CustomGridOptionsConfig.CustomActionsConfigsAttributeKey ) != json )
                    {
                        block.SetAttributeValue( CustomGridOptionsConfig.CustomActionsConfigsAttributeKey, json );

                        // if the actions changed, reload the whole page so that we can avoid issues with launchers changing between postbacks
                        reloadPage = true;
                    }
                }
                else
                {
                    if ( block.Attributes.Any( a => a.Key == CustomGridOptionsConfig.CustomActionsConfigsAttributeKey ) )
                    {
                        if ( block.GetAttributeValue( CustomGridOptionsConfig.CustomActionsConfigsAttributeKey ) != null )
                        {
                            // if the actions were removed, reload the whole page so that we can avoid issues with launchers changing between postbacks
                            reloadPage = true;
                        }

                        block.SetAttributeValue( CustomGridOptionsConfig.CustomActionsConfigsAttributeKey, null );
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

                // Save the default launcher enabled setting
                var isDefaultLauncherEnabled = tglEnableDefaultWorkflowLauncher.Checked;

                if ( isDefaultLauncherEnabled && !block.Attributes.Any( a => a.Key == CustomGridOptionsConfig.EnableDefaultWorkflowLauncherAttributeKey ) )
                {
                    block.Attributes.Add( CustomGridOptionsConfig.EnableDefaultWorkflowLauncherAttributeKey, null );
                }

                if ( block.GetAttributeValue( CustomGridOptionsConfig.EnableDefaultWorkflowLauncherAttributeKey ).AsBoolean() != isDefaultLauncherEnabled )
                {
                    block.SetAttributeValue( CustomGridOptionsConfig.EnableDefaultWorkflowLauncherAttributeKey, isDefaultLauncherEnabled.ToTrueFalse() );

                    // since the setting changed, reload the page
                    reloadPage = true;
                }

                rockContext.SaveChanges();
                block.SaveAttributeValues( rockContext );

                // If this is a PageMenu block then we need to also flush the lava template cache for the block here.
                // Changes to the PageMenu block configuration will handle this in the PageMenu_BlockUpdated event handler,
                // but here we address the situation where child pages are modified using the "CMS Configuration | Pages" menu option.
                if ( block.BlockType.Guid == Rock.SystemGuid.BlockType.PAGE_MENU.AsGuid() )
                {
                    var cacheKey = string.Format( "Rock:PageMenu:{0}", block.Id );

                    if ( LavaService.RockLiquidIsEnabled )
                    {
#pragma warning disable CS0618 // Type or member is obsolete
                        LavaTemplateCache.Remove( cacheKey );
#pragma warning restore CS0618 // Type or member is obsolete
                    }

                    LavaService.RemoveTemplateCacheEntry( cacheKey );
                }

                StringBuilder scriptBuilder = new StringBuilder();

                if ( reloadPage )
                {
                    scriptBuilder.AppendLine( "window.parent.location.reload();" );
                }
                else
                {
                    scriptBuilder.AppendLine( $"window.parent.Rock.controls.modal.close('BLOCK_UPDATED:{block.Id}:{block.Guid}');" );
                }

                ScriptManager.RegisterStartupScript( this.Page, this.GetType(), "close-modal", scriptBuilder.ToString(), true );
            } );
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
        /// Loads the custom settings tabs.
        /// </summary>
        protected void LoadCustomSettingsTabs()
        {
            int blockId = PageParameter( "BlockId" ).AsInteger();
            var block = BlockCache.Get( blockId );
            var site = block.Site ?? block.Layout?.Site ?? block.Page?.Layout?.Site;

            CustomSettingsProviders = new Dictionary<RockCustomSettingsProvider, Control>();

            // Site really shouldn't ever be null, but just in case we somehow
            // get here if the block configuration is bad, bail out with an
            // empty set of custom setting providers.
            if ( site == null )
            {
                return;
            }

            var providers = RockCustomSettingsProvider.GetProvidersForType( block.BlockType.GetCompiledType(), site.SiteType ).Reverse();
            foreach ( var provider in providers )
            {
                // Place the custom controls in a naming container to avoid
                // ID collisions.
                var controlContainer = new CompositePlaceHolder();

                if ( provider.CustomSettingsTitle == "Basic Settings" )
                {
                    phCustomBasicSettings.Controls.Add( controlContainer );
                }
                else if ( provider.CustomSettingsTitle == "Advanced Settings" )
                {
                    phCustomAdvancedSettings.Controls.Add( controlContainer );
                }
                else
                {
                    phCustomSettings.Controls.Add( controlContainer );
                }

                var control = provider.GetCustomSettingsControl( block, phCustomSettings );
                control.Visible = false;
                controlContainer.Controls.Add( control );

                CustomSettingsProviders.Add( provider, control );
            }
        }

        /// <summary>
        /// Shows the selected pane.
        /// </summary>
        private void ShowSelectedPane()
        {
            pnlAdvancedSettings.Visible = CurrentTab.Equals( "Advanced Settings" );
            pnlBasicProperty.Visible = CurrentTab.Equals( "Basic Settings" );
            pnlMobileSettings.Visible = CurrentTab.Equals( "Mobile Local Settings" );
            pnlCustomGridTab.Visible = CurrentTab.Equals( "Custom Grid Options" );

            foreach ( var kvp in CustomSettingsProviders )
            {
                kvp.Value.Visible = CurrentTab.Equals( kvp.Key.CustomSettingsTitle );
            }
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
            foreach ( var item in rptCustomGridColumns.Items.OfType<RepeaterItem>() )
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
                ddlOffsetType.SetValue( ( int ) columnConfig.PositionOffsetType );
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

        #region Custom Actions

        /// <summary>
        /// Binds the custom actions configuration.
        /// </summary>
        private void BindCustomActionsConfig()
        {
            var blockId = Convert.ToInt32( PageParameter( "BlockId" ) );
            var _block = BlockCache.Get( blockId );

            rptCustomActions.DataSource = CustomActionsConfigState;
            rptCustomActions.DataBind();
        }

        /// <summary>
        /// Saves the state of the custom action configuration to view.
        /// </summary>
        private void SaveCustomActionsConfigToViewState()
        {
            CustomActionsConfigState = new List<CustomActionConfig>();

            foreach ( var item in rptCustomActions.Items.OfType<RepeaterItem>() )
            {
                var rtbName = item.FindControl( "rtbName" ) as RockTextBox;
                var rtbRoute = item.FindControl( "rtbRoute" ) as RockTextBox;
                var rtbIcon = item.FindControl( "rtbIcon" ) as RockTextBox;
                var rtbHelp = item.FindControl( "rtbHelp" ) as RockTextBox;

                var config = new CustomActionConfig
                {
                    Name = rtbName.Text,
                    Route = rtbRoute.Text,
                    IconCssClass = rtbIcon.Text,
                    HelpText = rtbHelp.Text
                };

                CustomActionsConfigState.Add( config );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbAddCustomAction control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddCustomAction_Click( object sender, EventArgs e )
        {
            SaveCustomActionsConfigToViewState();
            CustomActionsConfigState.Add( new CustomActionConfig() );
            BindCustomActionsConfig();
        }

        /// <summary>
        /// Handles the Click event of the btnDeleteCustomAction control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDeleteCustomAction_Click( object sender, EventArgs e )
        {
            SaveCustomActionsConfigToViewState();
            var index = ( sender as LinkButton ).CommandArgument.AsIntegerOrNull();

            if ( index.HasValue )
            {
                CustomActionsConfigState.RemoveAt( index.Value );
            }

            BindCustomActionsConfig();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptCustomActions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptCustomActions_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var config = e.Item.DataItem as CustomActionConfig;

            if ( config == null )
            {
                return;
            }

            var rtbName = e.Item.FindControl( "rtbName" ) as RockTextBox;
            rtbName.Text = config.Name;

            var rtbRoute = e.Item.FindControl( "rtbRoute" ) as RockTextBox;
            rtbRoute.Text = config.Route;

            var rtbIcon = e.Item.FindControl( "rtbIcon" ) as RockTextBox;
            rtbIcon.Text = config.IconCssClass;

            var rtbHelp = e.Item.FindControl( "rtbHelp" ) as RockTextBox;
            rtbHelp.Text = config.HelpText;

            var btnDeleteLauncher = e.Item.FindControl( "btnDeleteCustomAction" ) as LinkButton;
            btnDeleteLauncher.CommandName = "Index";
            btnDeleteLauncher.CommandArgument = e.Item.ItemIndex.ToString();
        }

        #endregion Custom Workflow Launchers
    }
}