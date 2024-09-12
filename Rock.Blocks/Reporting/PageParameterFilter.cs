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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Reporting.PageParameterFilter;
using Rock.ViewModels.Cms;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Reporting
{
    /// <summary>
    /// Filter block that passes the filter values through the Obsidian browser bus and updates query string parameters to match.
    /// </summary>

    [DisplayName( "Page Parameter Filter" )]
    [Category( "Reporting" )]
    [Description( "Filter block that passes the filter values through the Obsidian browser bus and updates query string parameters to match." )]
    //[SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [TextField( "Block Title Text",
        Key = AttributeKey.BlockTitleText,
        Description = "The text to display as the block title.",
        Category = "CustomSetting",
        DefaultValue = "" )]

    [TextField( "Block Title Icon CSS Class",
        Key = AttributeKey.BlockTitleIconCssClass,
        Description = "The CSS class name to use for the block title icon.",
        Category = "CustomSetting",
        DefaultValue = "fa fa-filter" )]

    [BooleanField( "Show Block Title",
        Key = AttributeKey.ShowBlockTitle,
        Description = "Determines if the block title should be displayed.",
        Category = "CustomSetting",
        DefaultBooleanValue = true )]

    [TextField( "Filter Button Text",
        Key = AttributeKey.FilterButtonText,
        Description = "The text to display on the filter button.",
        Category = "CustomSetting",
        DefaultValue = "Filter" )]

    [CustomDropdownListField( "Filter Button Size",
        Key = AttributeKey.FilterButtonSize,
        Description = "The size of the filter button.",
        Category = "CustomSetting",
        ListSource = "1^Normal,2^Small,3^Extra Small",
        DefaultValue = "3" )]

    [BooleanField( "Show Filter Button",
        Key = AttributeKey.ShowFilterButton,
        Description = "Determines if the filter button should be displayed.",
        Category = "CustomSetting",
        DefaultBooleanValue = true )]

    [BooleanField( "Show Reset Filters Button",
        Key = AttributeKey.ShowResetFiltersButton,
        Description = "Determines if the reset filters button should be displayed.",
        Category = "CustomSetting",
        DefaultBooleanValue = true )]

    [IntegerField( "Filters Per Row",
        Key = AttributeKey.FiltersPerRow,
        Description = "The number of filters to display per row. Maximum is 12.",
        Category = "CustomSetting",
        DefaultIntegerValue = 2 )]

    [LinkedPage( "Redirect Page",
        Key = AttributeKey.RedirectPage,
        Description = "If set, will redirect to the selected page when applying filters.",
        Category = "CustomSetting",
        DefaultValue = "" )]

    [CustomDropdownListField( "Filter Selection Action",
        Key = AttributeKey.FilterSelectionAction,
        Description = @"Describes the action to take when a non-textbox filter is selected by the individual. If ""Apply Filters"", all filters are applied instantly without the need to click the filter button. If ""Update Filters"", any filters whose available values rely on the selected values of other filters will be updated, but the user must click the filter button to apply them. If ""Do Nothing"", no updates happen, and the user must click the button to apply filters.",
        Category = "CustomSetting",
        ListSource = "0^Do Nothing,1^Update Filters,2^Apply Filters",
        DefaultValue = "0" )]

    [BooleanField( "Enable Legacy Reload",
        Key = AttributeKey.EnableLegacyReload,
        Description = "If enabled, a full page reload will be triggered to apply the filter selections (helpful when using this block to drive the behavior of legacy blocks on the page). If disabled, the filter selections will be communicated directly to any Obsidian blocks listening for these filters, so they can respond accordingly.",
        Category = "CustomSetting",
        DefaultBooleanValue = false )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "59F94307-B2B0-4383-9C2C-88A4E154C461" )]
    [Rock.SystemGuid.BlockTypeGuid( "842DFBC2-DA38-465D-BFD2-B6C8585AA3BF" )]
    public class PageParameterFilter : RockBlockType, IHasCustomActions
    {
        #region Legacy Block Checklist

        //#region Enums
        //private enum SelectionAction
        //{
        //    Nothing = 0,
        //    UpdateBlock = 1,
        //    UpdatePage = 2,
        //}
        //#endregion

        //#region Properties
        //protected Dictionary<string, object> CurrentParameters { get; set; }
        //#endregion

        //#region Base Control Methods

        ///// <summary>
        ///// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        ///// </summary>
        ///// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        //protected override void LoadViewState( object savedState )
        //{
        //    base.LoadViewState( savedState );

        //    CurrentParameters = ViewState["CurrentParameters"] as Dictionary<string, object>;
        //}

        ///// <summary>
        ///// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        ///// </summary>
        ///// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        //protected override void OnInit( EventArgs e )
        //{
        //    base.OnInit( e );

        //    _reloadOnSelection = GetSelectAction();

        //    // This is needed so that we can get the data from the controls after all control events
        //    // have run so that their values are updated.
        //    Page.LoadComplete += Page_LoadComplete;
        //}

        ///// <summary>
        ///// Handles the LoadComplete event of the Page control.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        //private void Page_LoadComplete( object sender, EventArgs e )
        //{
        //    // Add postback controls
        //    if ( Page.IsPostBack )
        //    {
        //        if ( _reloadOnSelection == SelectionAction.UpdateBlock )
        //        {
        //            // See if hidden field has 'true' already set
        //            if ( hfPostBack.Value.IsNullOrWhiteSpace() )
        //            {
        //                var control = Page.FindControl( Request.Form["__EVENTTARGET"] );
        //                if ( control != null && control.UniqueID.Contains( "attribute_field_" ) )
        //                {
        //                    // We need to update the form action so that the partial postback call post to the new parameterized URL.
        //                    Page.Form.Action = GetParameterizedUrl();
        //                    hfPostBack.Value = "True";
        //                    ScriptManager.RegisterStartupScript( control, control.GetType(), "Refresh-Controls", @"console.log('Doing Postback');  __doPostBack('" + Request.Form["__EVENTTARGET"] + @"','');", true );
        //                }
        //            }
        //            else
        //            {
        //                // Reset hidden field for next time.
        //                hfPostBack.Value = string.Empty;
        //            }
        //        }
        //        else if ( _reloadOnSelection == SelectionAction.UpdatePage )
        //        {
        //            var control = Page.FindControl( Request.Form["__EVENTTARGET"] );
        //            if ( control != null && control.UniqueID.Contains( "attribute_field_" ) )
        //            {
        //                btnFilter_Click( null, null );
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        ///// </summary>
        ///// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        //protected override void OnLoad( EventArgs e )
        //{
        //    if ( !Page.IsPostBack )
        //    {
        //        CurrentParameters = this.RockPage.PageParameters();

        //        // Get list of attributes with default values (4/12/2022 JME replaces code that read
        //        // this from the DB with the call below that reads from cache.
        //        var attribsWithDefaultValue = AttributeCache.AllForEntityType( _blockTypeEntityId )
        //            .Where( a =>
        //                a.EntityTypeQualifierColumn == "Id"
        //                && a.EntityTypeQualifierValue == _block.Id.ToString()
        //                && a.DefaultValue != null
        //                && a.DefaultValue != string.Empty )
        //            .ToList();

        //        // If we have any filters with default values, we want to load this block with the page parameters already set.
        //        if ( attribsWithDefaultValue.Any() && !this.RockPage.PageParameters().Any() )
        //        {
        //            ResetFilters();
        //        }
        //        else
        //        {
        //            LoadFilters();
        //        }
        //    }
        //    else
        //    {
        //        LoadFilters();
        //    }

        //    base.OnLoad( e );
        //}

        //protected override object SaveViewState()
        //{
        //    ViewState["CurrentParameters"] = CurrentParameters;

        //    return base.SaveViewState();
        //}
        //#endregion

        //#region Settings

        ///// <summary>
        ///// Shows the settings.
        ///// </summary>
        //protected override void ShowSettings()
        //{
        //    ddlSelectionAction.SelectedValue = GetSelectAction().ConvertToInt().ToString();
        //}

        ///// <summary>
        ///// Handles the SaveClick event of the mdSettings control.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        //protected void mdSettings_SaveClick( object sender, EventArgs e )
        //{
        //    SetAttributeValue( AttributeKey.DoesSelectionCausePostback, ddlSelectionAction.SelectedValue );

        //    // reload the page to make sure we have a clean load
        //    ResetFilters();
        //    NavigateToCurrentPageReference();
        //}

        //#endregion Settings

        //#region Events

        ///// <summary>
        ///// Handles the BlockUpdated event of the control.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        //protected void Block_BlockUpdated( object sender, EventArgs e )
        //{
        //    LoadFilters();
        //}

        ///// <summary>
        ///// Handles the btnFilter event.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        //protected void btnFilter_Click( object sender, EventArgs e )
        //{
        //    Response.Redirect( GetParameterizedUrl(), false );
        //}

        ///// <summary>
        ///// Handles the btnResetFilters event.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        //protected void btnResetFilters_Click( object sender, EventArgs e )
        //{
        //    ResetFilters();
        //}

        ///// <summary>
        ///// Handles the SelectItem event from an ItemPicker (fake event to register the postback)
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void FilterControl_ItemChanged( object sender, EventArgs e )
        //{
        //    // Hopefully an xhr happens here
        //}

        //#endregion

        //#region Methods

        ///// <summary>
        ///// Loads the filters.
        ///// </summary>
        //private void LoadFilters()
        //{
        //    BuildControls();

        //    if ( _block.Attributes != null )
        //    {
        //        foreach ( var attribute in _block.Attributes )
        //        {
        //            var attributeCache = AttributeCache.Get( attribute.Value.Guid );

        //            Control control = phAttributes.FindControl( string.Format( "attribute_field_{0}", attribute.Value.Id ) );
        //            if ( control != null )
        //            {
        //                var value = PageParameter( attribute.Key );
        //                if ( value.IsNotNullOrWhiteSpace() )
        //                {
        //                    attributeCache.FieldType.Field.SetEditValue( control, null, value );
        //                }
        //                else if ( attribute.Value.DefaultValue.IsNotNullOrWhiteSpace() )
        //                {
        //                    attributeCache.FieldType.Field.SetEditValue( control, null, attribute.Value.DefaultValue );
        //                }

        //                // Enable ListControls postback and Event
        //                if ( control is ListControl listControl && _reloadOnSelection != SelectionAction.Nothing )
        //                {
        //                    listControl.AutoPostBack = true;
        //                }

        //                // Enable ItemPicker postback event
        //                if ( control is ItemPicker itemPicker && _reloadOnSelection != SelectionAction.Nothing )
        //                {
        //                    itemPicker.SelectItem += FilterControl_ItemChanged;
        //                }

        //                // Enable Toggle postback event
        //                if ( control is Toggle toggle && _reloadOnSelection != SelectionAction.Nothing )
        //                {
        //                    toggle.CheckedChanged += FilterControl_ItemChanged;
        //                }
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// Resets the filters to their original state.  Any filters with default values will be set as well.
        ///// </summary>
        //private void ResetFilters()
        //{
        //    BuildControls();

        //    NameValueCollection queryString = GenerateQueryString();

        //    // 4/12/2022 JME
        //    // Updated the redirects to set the endResponse = true (was false). This prevents
        //    // child blocks from fully loading, redirecting and loading again. The child blocks
        //    // are typically SQL so that could mean a very slow initial page load as they would
        //    // be run twice. Not sure why these were originally set to false. 
        //    if ( queryString.AllKeys.Any() )
        //    {
        //        Response.Redirect( $"{Request.UrlProxySafe().AbsolutePath}?{queryString}", true );
        //    }
        //    else
        //    {
        //        Response.Redirect( Request.UrlProxySafe().AbsolutePath, true );
        //    }
        //}

        ///// <summary>
        ///// Generates the query string.
        ///// </summary>
        ///// <returns></returns>
        //private NameValueCollection GenerateQueryString()
        //{
        //    var queryString = HttpUtility.ParseQueryString( String.Empty );

        //    // Don't create a query string if the block's page does not match the current page. This
        //    // would be the case when editing the settings from 'Admin Tools > CMS Settings > Pages'.
        //    // Without this check the block would thrown an exception as CurrentParameters would be
        //    // null. This may not be the _best_ place for this check, but the correct change may
        //    // need a major refactor.
        //    if ( RockPage.PageId != BlockCache.PageId )
        //    {
        //        return queryString;
        //    }

        //    foreach ( var parameter in CurrentParameters )
        //    {
        //        if ( parameter.Key != "PageId" )
        //        {
        //            queryString.Set( parameter.Key, parameter.Value.ToString() );
        //        }
        //    }

        //    _block.LoadAttributes( new RockContext() );

        //    if ( _block.Attributes != null )
        //    {
        //        foreach ( var attribute in _block.Attributes )
        //        {
        //            Control control = phAttributes.FindControl( string.Format( "attribute_field_{0}", attribute.Value.Id ) );
        //            if ( control != null )
        //            {
        //                string value = attribute.Value.FieldType.Field.GetEditValue( control, attribute.Value.QualifierValues );

        //                // If there is no value use the attribute's default value
        //                if ( value.IsNullOrWhiteSpace() )
        //                {
        //                    value = attribute.Value.DefaultValue;
        //                }

        //                if ( value.IsNotNullOrWhiteSpace() )
        //                {
        //                    queryString.Set( attribute.Key, value );
        //                    CurrentPageReference.Parameters.AddOrReplace( attribute.Key, value );
        //                }
        //                else
        //                {
        //                    queryString.Remove( attribute.Key );
        //                    CurrentPageReference.Parameters.Remove( attribute.Key );
        //                }
        //            }
        //        }
        //    }

        //    return queryString;
        //}

        ///// <summary>
        ///// Gets the parameterized URL.
        ///// </summary>
        ///// <returns></returns>
        //private string GetParameterizedUrl()
        //{
        //    var queryString = GenerateQueryString();
        //    var url = Request.UrlProxySafe().AbsolutePath;

        //    var pageGuid = GetAttributeValue( AttributeKey.RedirectPage ).AsGuidOrNull();
        //    if ( pageGuid.HasValue )
        //    {
        //        var page = PageCache.Get( pageGuid.Value );

        //        url = VirtualPathUtility.ToAbsolute( string.Format( "~/page/{0}", page.Id ) );
        //    }

        //    return queryString.AllKeys.Any() ? $"{url}?{queryString}" : url;
        //}

        ///// <summary>
        ///// Gets the select action.
        ///// </summary>
        ///// <returns></returns>
        //private SelectionAction GetSelectAction()
        //{
        //    var attributeValue = GetAttributeValue( AttributeKey.DoesSelectionCausePostback );
        //    if ( attributeValue.IsNullOrWhiteSpace() )
        //    {
        //        return SelectionAction.Nothing;
        //    }

        //    if ( attributeValue.Equals( "false", StringComparison.InvariantCultureIgnoreCase ) )
        //    {
        //        return SelectionAction.Nothing;
        //    }

        //    if ( attributeValue.Equals( "true", StringComparison.InvariantCultureIgnoreCase ) )
        //    {
        //        return SelectionAction.UpdateBlock;
        //    }

        //    return attributeValue.ConvertToEnum<SelectionAction>();
        //}
        //#endregion

        #endregion Legacy Block Checklist

        #region Keys

        private static class AttributeKey
        {
            public const string BlockTitleText = "BlockTitleText";
            public const string BlockTitleIconCssClass = "BlockTitleIconCSSClass";
            public const string ShowBlockTitle = "ShowBlockTitle";
            public const string FilterButtonText = "FilterButtonText";
            public const string FilterButtonSize = "FilterButtonSize";
            public const string ShowFilterButton = "ShowFilterButton";
            public const string ShowResetFiltersButton = "ShowResetFiltersButton";
            public const string FiltersPerRow = "FiltersPerRow";
            public const string RedirectPage = "RedirectPage";
            public const string FilterSelectionAction = "DoesSelectionCausePostback";
            public const string EnableLegacyReload = "EnableLegacyReload";
        }

        private static class NavigationUrlKey
        {
            public const string RedirectPage = "RedirectPage";
        }

        #endregion Keys

        #region Fields

        private List<ListItemBag> _filterButtonSizeItems;

        private List<ListItemBag> _filterSelectionActionItems;

        #endregion Fields

        #region Properties

        private List<ListItemBag> FilterButtonSizeItems
        {
            get
            {
                if ( _filterButtonSizeItems == null )
                {
                    _filterButtonSizeItems = new List<ListItemBag>
                    {
                        FilterButtonSize.Normal,
                        FilterButtonSize.Small,
                        FilterButtonSize.ExtraSmall
                    };
                }

                return _filterButtonSizeItems;
            }
        }

        private string DefaultFilterButtonSize => FilterButtonSize.ExtraSmall.Value;

        private int DefaultFiltersPerRow => 2;

        private List<ListItemBag> FilterSelectionActionItems
        {
            get
            {
                if ( _filterSelectionActionItems == null )
                {
                    _filterSelectionActionItems = new List<ListItemBag>
                    {
                        FilterSelectionAction.DoNothing,
                        FilterSelectionAction.UpdateFilters,
                        FilterSelectionAction.ApplyFilters
                    };
                }

                return _filterSelectionActionItems;
            }
        }

        private string DefaultFilterSelectionAction => FilterSelectionAction.DoNothing.Value;

        private int AttributeEntityTypeId => EntityTypeCache.Get( Rock.SystemGuid.EntityType.ATTRIBUTE ).Id;

        private int BlockEntityTypeId => EntityTypeCache.Get( Rock.SystemGuid.EntityType.BLOCK ).Id;

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var customSettingsBox = GetCustomSettingsBox();
            var settings = customSettingsBox.Settings;

            var filterAttributes = GetOrderedFilterAttributes();

            var (
                publicFilterValues,
                filterPageParameters
            ) = GetFilterValuesFromURLOrDefaults( filterAttributes );

            var box = new PageParameterFilterInitializationBox
            {
                BlockTitleText = settings.BlockTitleText,
                BlockTitleIconCssClass = settings.BlockTitleIconCssClass,
                IsBlockTitleVisible = settings.IsBlockTitleVisible,
                FilterButtonText = settings.FilterButtonText,
                FilterButtonSize = settings.FilterButtonSize,
                IsFilterButtonVisible = settings.IsFilterButtonVisible,
                IsResetFiltersButtonVisible = settings.IsResetFiltersButtonVisible,
                FiltersPerRow = settings.FiltersPerRow,
                PublicFilters = GetPublicFilters( filterAttributes ),
                PublicFilterValues = publicFilterValues,
                FilterPageParameters = filterPageParameters,
                FilterSelectionAction = settings.FilterSelectionAction,
                IsLegacyReloadEnabled = settings.IsLegacyReloadEnabled,
                SecurityGrantToken = customSettingsBox.SecurityGrantToken,
                NavigationUrls = GetBoxNavigationUrls()
            };

            return box;
        }

        /// <summary>
        /// Gets the custom settings box that contains the current block settings and available options.
        /// </summary>
        /// <returns>The custom settings box that contains the current block settings and available options.</returns>
        private CustomSettingsBox<PageParameterFilterCustomSettingsBag, PageParameterFilterCustomSettingsOptionsBag> GetCustomSettingsBox()
        {
            var options = new PageParameterFilterCustomSettingsOptionsBag
            {
                FilterButtonSizeItems = this.FilterButtonSizeItems,
                FilterSelectionActionItems = this.FilterSelectionActionItems,
                FiltersGridDefinition = GetFiltersGridBuilder().BuildDefinition(),
                FiltersReservedKeyNames = GetFiltersReservedKeyNames()
            };

            var filterButtonSize = GetAttributeValue( AttributeKey.FilterButtonSize );
            if ( !this.FilterButtonSizeItems.Any( s => s.Value == filterButtonSize ) )
            {
                filterButtonSize = this.DefaultFilterButtonSize;
            }

            var filtersPerRow = GetAttributeValue( AttributeKey.FiltersPerRow ).AsIntegerOrNull();
            if ( !filtersPerRow.HasValue || filtersPerRow.Value < 1 || filtersPerRow.Value > 12 )
            {
                filtersPerRow = this.DefaultFiltersPerRow;
            }

            /*
                10/2/2024 - JPH

                When the Rock Core Team adopted this legacy block from Bema Services in March of 2020, a boolean field
                type was used for the `DoesSelectionCausePostback` block setting. If true, all of the following would
                happen when non-text filter selections were made by the individual:

                    A. Automatically trigger a partial postback.

                       See: https://github.com/SparkDevNetwork/Rock/blob/46ebf998acc0eb7a2a0e63c0cfb330a2a7fcea91/RockWeb/Blocks/Reporting/PageParameterFilter.ascx.cs#L541-L553

                    B. Manually trigger a second postback via JavaScript.

                       See: https://github.com/SparkDevNetwork/Rock/blob/46ebf998acc0eb7a2a0e63c0cfb330a2a7fcea91/RockWeb/Blocks/Reporting/PageParameterFilter.ascx.cs#L200-L217

                    C. Manually update the `Request.QueryString` to reflect the filter selections.

                       See:
                       https://github.com/SparkDevNetwork/Rock/blob/46ebf998acc0eb7a2a0e63c0cfb330a2a7fcea91/RockWeb/Blocks/Reporting/PageParameterFilter.ascx.cs#L539
                       https://github.com/SparkDevNetwork/Rock/blob/46ebf998acc0eb7a2a0e63c0cfb330a2a7fcea91/RockWeb/Blocks/Reporting/PageParameterFilter.ascx.cs#L498-L507
                       https://github.com/SparkDevNetwork/Rock/blob/46ebf998acc0eb7a2a0e63c0cfb330a2a7fcea91/RockWeb/Blocks/Reporting/PageParameterFilter.ascx.cs#L604-L621

                This behavior was noted to be useful when one or more filter attributes had dynamic values built from
                the query string (e.g., a single select filter that populates via a SQL query referencing the query
                string in Lava). When the partial postback occurred, the block would create a virtual query string for
                Lava to use, handled by the `GenerateQueryString()` method.

                See: https://github.com/SparkDevNetwork/Rock/blob/4f037e42237e2756997027c3524b42ad8f7a62dd/RockWeb/Blocks/Reporting/PageParameterFilter.ascx.cs#L45-L68

                --------------------------------------------------------------------------------------------------------

                In March of 2021, the `DoesSelectionCausePostback` setting was changed to a single select field type to
                allow more control over HOW the block triggered postbacks when non-text filter selections were made by
                the individual. The setting could now be one of the following:

                    D. [SelectionAction.Nothing (null, whitespace, false, 0)]: Do nothing upon filter selection (not
                       even a partial postback).

                    E. [SelectionAction.UpdateBlock (true, 1)]: Perform the previous behavior (A-C above), but with
                       improvements:

                       1. Auto postback behavior now included toggle controls.

                          See: https://github.com/SparkDevNetwork/Rock/blob/790e28c2fbf237ebcc294a3204ceeed8d6e35548/RockWeb/Blocks/Reporting/PageParameterFilter.ascx.cs#L705-L721

                       2. Manual `Request.QueryString` update logic was replaced with a new approach.

                          See: https://github.com/SparkDevNetwork/Rock/blob/f055d53e90aaf30043c6003dafb3d8fecdbdb3e4/RockWeb/Blocks/Reporting/PageParameterFilter.ascx.cs#L286-L287

                    F. [SelectionAction.UpdatePage (2)]: Manually trigger a full page reload by programmatically clicking
                       the [Filter] button. This reload may be on the current page or a redirect page, with the latest
                       filter selections being reflected in the query string for either approach.

                       See:
                       https://github.com/SparkDevNetwork/Rock/blob/f055d53e90aaf30043c6003dafb3d8fecdbdb3e4/RockWeb/Blocks/Reporting/PageParameterFilter.ascx.cs#L297-L304
                       https://github.com/SparkDevNetwork/Rock/blob/f055d53e90aaf30043c6003dafb3d8fecdbdb3e4/RockWeb/Blocks/Reporting/PageParameterFilter.ascx.cs#L570-L578

                --------------------------------------------------------------------------------------------------------

                In transitioning this block to Obsidian, we are going to slightly repurpose the `DoesSelectionCausePostback`
                block setting once more. The setting can now be one of the following:

                    G. [FilterSelectionAction.DoNothing (null, whitespace, false, 0)]: Do nothing upon filter selection.
                       The individual will be required to click the filter button for filters to be applied.

                    H. [FilterSelectionAction.UpdateFilters (true, 1)]: Send a block action request to the server with
                       the current filter selections, so any filters whose AVAILABLE values are dependent upon the
                       SELECTED values of other filters can refresh their available values and update the UI. The filter
                       values will NOT be applied at this time.

                    I. [FilterSelectionAction.ApplyFilters (2)]:

                        1. If `RedirectPage` is defined, redirect to the specified page with the current filter selections
                           in the query string.

                        2. If `EnableLegacyReload` is true, trigger a full page reload with the current filter selections
                           in the query string.

                        3. If `EnableLegacyReload` is false, send a message to the Obsidian browser bus with the current
                           filter selections as the payload, so any blocks listening for these messages can respond
                           accordingly.

                Reason: Evolution of `DoesSelectionCausePostback` block setting.
             */

            var filterSelectionAction = GetAttributeValue( AttributeKey.FilterSelectionAction );
            if ( !this.FilterSelectionActionItems.Any( s => s.Value == filterSelectionAction ) )
            {
                filterSelectionAction = this.DefaultFilterSelectionAction;
            }

            var settings = new PageParameterFilterCustomSettingsBag
            {
                BlockTitleText = GetAttributeValue( AttributeKey.BlockTitleText ),
                BlockTitleIconCssClass = GetAttributeValue( AttributeKey.BlockTitleIconCssClass ),
                IsBlockTitleVisible = GetAttributeValue( AttributeKey.ShowBlockTitle ).AsBoolean( true ),
                FilterButtonText = GetAttributeValue( AttributeKey.FilterButtonText ),
                FilterButtonSize = filterButtonSize,
                IsFilterButtonVisible = GetAttributeValue( AttributeKey.ShowFilterButton ).AsBoolean( true ),
                IsResetFiltersButtonVisible = GetAttributeValue( AttributeKey.ShowResetFiltersButton ).AsBoolean( true ),
                FiltersPerRow = filtersPerRow.Value,
                RedirectPage = GetAttributeValue( AttributeKey.RedirectPage ).ToPageRouteValueBag(),
                FilterSelectionAction = filterSelectionAction,
                IsLegacyReloadEnabled = GetAttributeValue( AttributeKey.EnableLegacyReload ).AsBoolean()
            };

            return new CustomSettingsBox<PageParameterFilterCustomSettingsBag, PageParameterFilterCustomSettingsOptionsBag>
            {
                Settings = settings,
                Options = options,
                SecurityGrantToken = new Rock.Security.SecurityGrant().ToToken()
            };
        }

        /// <summary>
        /// Gets the grid builder for the filters grid.
        /// </summary>
        /// <returns>The grid builder for the filters grid.</returns>
        private GridBuilder<AttributeCache> GetFiltersGridBuilder()
        {
            return new GridBuilder<AttributeCache>()
                .AddField( "guid", a => a.Guid )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "description", a => a.Description )
                .AddTextField( "filterType", a => FieldTypeCache.Get( a.FieldTypeId ).ToString() )
                .AddTextField( "defaultValue", a =>
                {
                    //return ExtensionMethods.GetAttributeCondensedHtmlValue( this.BlockCache, a.Key );
                    return "TODO...";
                } );
        }

        /// <summary>
        /// Gets the reserved key names for the filters.
        /// <para>
        /// This list will include ALL of this block's attribute keys, including those that aren't specifically tied to
        /// the filters themselves, as duplicate keys across all block settings can lead to issues.
        /// </para>
        /// </summary>
        /// <param name="ignoreFilterGuid">The optional unique identifier of the filter whose key should be ignored.
        /// This is helpful when editing an existing filter, so we don't indicate a false key collision.</param>
        /// <returns>The reserved key names for the filters.</returns>
        private List<string> GetFiltersReservedKeyNames( Guid? ignoreFilterGuid = null )
        {
            return this.BlockCache.Attributes
                .Select( a => a.Value )
                .Where( a =>
                    !ignoreFilterGuid.HasValue
                    || ignoreFilterGuid.Equals( Guid.Empty )
                    || !a.Guid.Equals( ignoreFilterGuid.Value )
                )
                .Select( a => a.Key )
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Gets the ordered filter attributes.
        /// </summary>
        /// <returns>The ordered filter attributes.</returns>
        private List<AttributeCache> GetOrderedFilterAttributes()
        {
            return AttributeCache.AllForEntityType( this.BlockEntityTypeId )
                .Where( a =>
                    // Filter attributes are tied directly to this block instance (hence the qualifier column value of
                    // "Id"), rather than being tied to the block type, as traditional block settings are.
                    a.EntityTypeQualifierColumn == "Id"
                    && a.EntityTypeQualifierValue == this.BlockId.ToString()
                )
                .OrderBy( a => a.Order )
                .ToList();
        }

        /// <summary>
        /// Gets the public filters available for selection.
        /// <para>
        /// For each filter, any configuration values (e.g. values to be used within a drop down list's source) will
        /// also be provided here.
        /// </para>
        /// </summary>
        /// <param name="filterAttributes">The filter attributes.</param>
        /// <returns>The public filters available for selection.</returns>
        private Dictionary<string, PublicAttributeBag> GetPublicFilters( List<AttributeCache> filterAttributes )
        {
            return this.BlockCache.GetPublicAttributesForEdit( this.RequestContext.CurrentPerson, enforceSecurity: true, a => filterAttributes.Any( f => f.Key == a.Key ) );
        }

        /// <summary>
        /// Gets the editable filter value and page parameter for each filter, from the URL or default values.
        /// </summary>
        /// <param name="filterAttributes">The filter attributes.</param>
        /// <returns>The editable filter value and page parameter for each filter, from the URL or default values.</returns>
        private (
            Dictionary<string, string> publicFilterValues,
            Dictionary<string, string> filterPageParameters
        ) GetFilterValuesFromURLOrDefaults( List<AttributeCache> filterAttributes )
        {
            var values = (
                publicFilterValues: new Dictionary<string, string>(),
                filterPageParameters: new Dictionary<string, string>()
            );

            foreach ( var filterAttribute in filterAttributes )
            {
                var filterKey = filterAttribute.Key;

                // Look in the URL for a value first.
                var privateFilterValue = PageParameter( filterKey );

                if ( privateFilterValue.IsNullOrWhiteSpace() )
                {
                    // Fall back to the default value, if any;
                    privateFilterValue = GetAttributeValue( filterKey );
                }

                // Add the editable filter value, so the individual can make selections using each filter's field type.
                values.publicFilterValues.Add( filterKey, PublicAttributeHelper.GetPublicEditValue( filterAttribute, privateFilterValue ) );

                // Add the private filter value as a page parameter, so the client can properly convey filters to other blocks.
                values.filterPageParameters.Add( filterKey, privateFilterValue );
            }

            return values;
        }

        /// <summary>
        /// Sets page parameters on the rock request context, using the provided public filter values.
        /// </summary>
        /// <param name="filterAttributes">The filter attributes.</param>
        /// <param name="publicFilterValues">The public filter values to use when setting page parameters.</param>
        private void SetPageParametersFromFilters( List<AttributeCache> filterAttributes, Dictionary<string, string> publicFilterValues )
        {
            // Get the private filter parameters using the public filter values.
            var filterParameters = GetFilterPageParametersFromPublicFilterValues( filterAttributes, publicFilterValues );

            var pageParameters = this.RequestContext.GetPageParameters();
            foreach ( var filterParameter in filterParameters )
            {
                var filterKey = filterParameter.Key;
                var filterValue = filterParameter.Value;

                if ( filterValue.IsNullOrWhiteSpace() )
                {
                    if ( pageParameters.ContainsKey( filterKey ) )
                    {
                        pageParameters.Remove( filterKey );
                    }

                    continue;
                }

                pageParameters[filterKey] = filterValue;
            }

            /*
                10/7/2024 - JPH

                Local testing suggests it's safe to overwrite RockRequestContext's page parameters (in order to provide
                low-level Lava templates, Etc. with the latest page parameter values). However, if we find this approach
                to be problematic in the future, an alternative approach might be:

                    1. Create a secondary instance of the RockRequestContext class and copy all property values from the
                       primary instance.

                    2. Overwrite the page parameters on that secondary instance.

                    3. Set the secondary instance on the RockRequestContextAccessor for low-level processes to grab and
                       use for Lava templates.

                Reason: Provide alternative approach for unconventional page parameter manipulation.
             */
            this.RequestContext.SetPageParameters( pageParameters );
        }

        /// <summary>
        /// Gets the filter page parameters from the public filter values.
        /// <para>
        /// The public filter values will sometimes be a serialized JSON object, for example. What we need for the query
        /// string, however, are simple string values. These will often be entity identifiers (e.g. Guids).
        /// </para>
        /// </summary>
        /// <param name="filterAttributes">The filter attributes.</param>
        /// <param name="publicFilterValues">The public filter values for which to get filter parameters.</param>
        /// <returns>The filter page parameters from the public filter values.</returns>
        private Dictionary<string, string> GetFilterPageParametersFromPublicFilterValues( List<AttributeCache> filterAttributes, Dictionary<string, string> publicFilterValues )
        {
            var filterParameters = new Dictionary<string, string>();

            if ( publicFilterValues?.Any() != true )
            {
                return filterParameters;
            }

            foreach ( var publicFilterValue in publicFilterValues )
            {
                var key = publicFilterValue.Key;
                var attributeCache = filterAttributes.FirstOrDefault( a => a.Key == key );
                if ( attributeCache == null )
                {
                    continue;
                }

                filterParameters.Add( key, PublicAttributeHelper.GetPrivateValue( attributeCache, publicFilterValue.Value ) );
            }

            return filterParameters;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var urls = new Dictionary<string, string>();

            var redirectPage = GetAttributeValue( AttributeKey.RedirectPage );
            if ( redirectPage.IsNotNullOrWhiteSpace() )
            {
                urls.Add( NavigationUrlKey.RedirectPage, this.GetLinkedPageUrl( AttributeKey.RedirectPage ) );
            }

            return urls;
        }

        /// <summary>
        /// Flushes the block instance from cache.
        /// </summary>
        private void FlushBlockFromCache()
        {
            // Because of the unconventional way this block uses attributes to represent the filters, we need to ensure
            // subsequent loads of the filters are not served from stale cache values after making administrative changes.
            BlockCache.FlushItem( this.BlockId );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the values and all other required details that will be needed to display the custom settings modal.
        /// </summary>
        /// <returns>A box that contains the custom settings values and additional data.</returns>
        [BlockAction]
        public BlockActionResult GetCustomSettings()
        {
            if ( !this.BlockCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, this.RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "Not authorized to edit block settings." );
            }

            var customSettingsBox = GetCustomSettingsBox();

            return ActionOk( customSettingsBox );
        }

        /// <summary>
        /// Saves the updates to the custom setting values for this block, for the custom settings modal.
        /// </summary>
        /// <param name="box">The box that contains the setting values.</param>
        /// <returns>A response that indicates if the save was successful or not.</returns>
        [BlockAction]
        public BlockActionResult SaveCustomSettings( CustomSettingsBox<PageParameterFilterCustomSettingsBag, PageParameterFilterCustomSettingsOptionsBag> box )
        {
            if ( !this.BlockCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, this.RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "Not authorized to edit block settings." );
            }

            var block = new BlockService( this.RockContext ).Get( this.BlockId );
            block.LoadAttributes( this.RockContext );

            box.IfValidProperty( nameof( box.Settings.BlockTitleText ),
                () => block.SetAttributeValue( AttributeKey.BlockTitleText, box.Settings.BlockTitleText ) );

            box.IfValidProperty( nameof( box.Settings.BlockTitleIconCssClass ),
                () => block.SetAttributeValue( AttributeKey.BlockTitleIconCssClass, box.Settings.BlockTitleIconCssClass ) );

            box.IfValidProperty( nameof( box.Settings.IsBlockTitleVisible ),
                () => block.SetAttributeValue( AttributeKey.ShowBlockTitle, box.Settings.IsBlockTitleVisible.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.FilterButtonText ),
                () => block.SetAttributeValue( AttributeKey.FilterButtonText, box.Settings.FilterButtonText ) );

            box.IfValidProperty( nameof( box.Settings.FilterButtonSize ), () =>
                {
                    if ( !this.FilterButtonSizeItems.Any( s => s.Value == box.Settings.FilterButtonSize ) )
                    {
                        box.Settings.FilterButtonSize = this.DefaultFilterButtonSize;
                    }

                    block.SetAttributeValue( AttributeKey.FilterButtonSize, box.Settings.FilterButtonSize );
                } );

            box.IfValidProperty( nameof( box.Settings.IsFilterButtonVisible ),
                () => block.SetAttributeValue( AttributeKey.ShowFilterButton, box.Settings.IsFilterButtonVisible.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.IsResetFiltersButtonVisible ),
                () => block.SetAttributeValue( AttributeKey.ShowResetFiltersButton, box.Settings.IsResetFiltersButtonVisible.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.FiltersPerRow ), () =>
            {
                if ( box.Settings.FiltersPerRow < 1 || box.Settings.FiltersPerRow > 12 )
                {
                    box.Settings.FiltersPerRow = this.DefaultFiltersPerRow;
                }

                block.SetAttributeValue( AttributeKey.FiltersPerRow, box.Settings.FiltersPerRow.ToString() );
            } );

            box.IfValidProperty( nameof( box.Settings.RedirectPage ),
                () => block.SetAttributeValue( AttributeKey.RedirectPage, box.Settings.RedirectPage.ToCommaDelimitedPageRouteValues() ) );

            box.IfValidProperty( nameof( box.Settings.FilterSelectionAction ), () =>
            {
                if ( !this.FilterSelectionActionItems.Any( a => a.Value == box.Settings.FilterSelectionAction ) )
                {
                    box.Settings.FilterSelectionAction = this.DefaultFilterSelectionAction;
                }

                block.SetAttributeValue( AttributeKey.FilterSelectionAction, box.Settings.FilterSelectionAction );
            } );

            box.IfValidProperty( nameof( box.Settings.IsLegacyReloadEnabled ),
                () => block.SetAttributeValue( AttributeKey.EnableLegacyReload, box.Settings.IsLegacyReloadEnabled.ToString() ) );

            // Get the filter attribute keys so we can exclude them from the list of keys to save (they've already been saved).
            var filterAttributeKeys = GetOrderedFilterAttributes()
                .Select( a => a.Key )
                .ToList();

            // Get the keys of the non-filter attributes.
            var nonFilterAttributeKeys = block.Attributes
                .Where( a => !filterAttributeKeys.Contains( a.Key, StringComparer.OrdinalIgnoreCase ) )
                .Select( a => a.Key );

            block.SaveAttributeValues( nonFilterAttributeKeys, this.RockContext );

            FlushBlockFromCache();

            return ActionOk();
        }

        /// <summary>
        /// Gets the filters grid row data, for the custom settings modal.
        /// </summary>
        /// <returns>A bag containing the filters grid row data.</returns>
        [BlockAction]
        public BlockActionResult GetFiltersGridRowData()
        {
            if ( !this.BlockCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, this.RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "Not authorized to edit block settings." );
            }

            var attributes = GetOrderedFilterAttributes();
            var builder = GetFiltersGridBuilder();
            var gridDataBag = builder.Build( attributes );

            return ActionOk( gridDataBag );
        }

        /// <summary>
        /// Gets the information needed to add new or edit existing filter, for the custom settings modal.
        /// </summary>
        /// <param name="filterGuid">The unique identifier of the filter to edit or <see langword="null"/> if adding a new filter.</param>
        /// <returns>A bag containing the editable filter and/or the current reserved key names.</returns>
        [BlockAction]
        public BlockActionResult AddOrEditFilter( Guid? filterGuid )
        {
            if ( !this.BlockCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, this.RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "Not authorized to edit block settings." );
            }

            var editableFilter = new EditableFilterBag
            {
                FiltersReservedKeyNames = GetFiltersReservedKeyNames( filterGuid )
            };

            if ( filterGuid.HasValue && !filterGuid.Equals( Guid.Empty ) )
            {
                var attribute = new AttributeService( this.RockContext ).Get( filterGuid.Value );
                if ( attribute == null )
                {
                    return ActionBadRequest( "Unable to find filter to edit." );
                }

                editableFilter.Filter = PublicAttributeHelper.GetPublicEditableAttributeViewModel( attribute );
            }

            return ActionOk( editableFilter );
        }

        /// <summary>
        /// Saves the filter, for the custom settings modal.
        /// </summary>
        /// <param name="bag">The information needed to save the filter.</param>
        /// <returns>A grid row object containing information about the saved filter.</returns>
        [BlockAction]
        public BlockActionResult SaveFilter( PublicEditableAttributeBag bag )
        {
            if ( !this.BlockCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, this.RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "Not authorized to edit block settings." );
            }

            if ( bag == null )
            {
                return ActionBadRequest();
            }

            // Prevent duplicate key values.
            var filtersReservedKeyNames = GetFiltersReservedKeyNames( bag.Guid );
            if ( filtersReservedKeyNames.Contains( bag.Key.Trim(), StringComparer.CurrentCultureIgnoreCase ) )
            {
                return ActionBadRequest( "There is already a filter with the key value you entered. Please enter a different key." );
            }

            // Always assign filter attributes to the "CustomSetting" category.
            bag.Categories = CategoryCache.All()
                .Where( c =>
                    c.Name == "CustomSetting"
                    && c.EntityTypeId == this.AttributeEntityTypeId
                    && c.EntityTypeQualifierColumn == "EntityTypeId"
                    && c.EntityTypeQualifierValue == this.BlockEntityTypeId.ToString()
                )
                .ToListItemBagList();

            var attribute = Helper.SaveAttributeEdits( bag, this.BlockEntityTypeId, "Id", this.BlockId.ToString(), this.RockContext );

            // Attribute will be null if it was not valid.
            if ( attribute == null )
            {
                return ActionBadRequest();
            }

            FlushBlockFromCache();

            // Return a grid row representing the attribute so it can be added or updated within the filters grid.
            var attributeCache = AttributeCache.Get( attribute.Id );
            var builder = GetFiltersGridBuilder();
            var filters = new List<AttributeCache> { attributeCache };
            var gridDataBag = builder.Build( filters );

            return ActionOk( gridDataBag.Rows[0] );
        }

        /// <summary>
        /// Changes the ordered position of a single filter, for the custom settings modal.
        /// </summary>
        /// <param name="filterKey">The identifier of the filter that will be moved.</param>
        /// <param name="beforeFilterKey">The identifier of the filter it will be placed before.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult ReorderFilter( string filterKey, string beforeFilterKey )
        {
            if ( !this.BlockCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, this.RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "Not authorized to edit block settings." );
            }

            var attributes = new AttributeService( this.RockContext )
                .Get( this.BlockEntityTypeId, "Id", this.BlockId.ToString() )
                .OrderBy( a => a.Order )
                .ToList();

            if ( !attributes.ReorderEntity( filterKey, beforeFilterKey ) )
            {
                return ActionBadRequest( "Invalid reorder attempt." );
            }

            this.RockContext.SaveChanges();

            FlushBlockFromCache();

            return ActionOk();
        }

        /// <summary>
        /// Deletes the specified filter from the database, for the custom settings modal.
        /// </summary>
        /// <param name="filterGuid">The unique identifier of the filter to delete.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult DeleteFilter( Guid filterGuid )
        {
            if ( !this.BlockCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, this.RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "Not authorized to edit block settings." );
            }

            var attributeService = new AttributeService( this.RockContext );
            var attribute = attributeService.Get( filterGuid );
            if ( attribute == null )
            {
                return ActionBadRequest( "Unable to find filter to delete." );
            }

            attributeService.Delete( attribute );
            this.RockContext.SaveChanges();

            FlushBlockFromCache();

            return ActionOk();
        }

        /// <summary>
        /// Gets updated filters based on the provided filter values.
        /// </summary>
        /// <param name="bag">The information needed to get updated filters.</param>
        /// <returns>A bag containing updated filters.</returns>
        [BlockAction]
        public BlockActionResult GetUpdatedFilters( GetUpdatedFiltersRequestBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest();
            }

            var filterAttributes = GetOrderedFilterAttributes();

            // Overwrite the page parameter values on the rock request context using the currently-selected filter values.
            // This will allow the `GetPublicFilters()` call below to properly set the available values on each filter.
            SetPageParametersFromFilters( filterAttributes, bag.PublicFilterValues );

            var response = new GetUpdatedFiltersResponseBag
            {
                PublicFilters = GetPublicFilters( filterAttributes )
            };

            // For each of the currently-selected filter values, ensure they're still allowed based on each filter's
            // updated configuration values.

            // TODO.....

            return ActionOk( response );
        }

        /// <summary>
        /// Gets filter page parameters for the provided filter values.
        /// </summary>
        /// <param name="bag">The information needed to get filter page parameters.</param>
        /// <returns>A bag containing filter page parameters.</returns>
        [BlockAction]
        public BlockActionResult GetFilterPageParameters( GetFilterPageParametersRequestBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest();
            }

            var filterAttributes = GetOrderedFilterAttributes();

            var response = new GetFilterPageParametersResponseBag
            {
                FilterPageParameters = GetFilterPageParametersFromPublicFilterValues( filterAttributes, bag.PublicFilterValues )
            };

            return ActionOk( response );
        }

        #endregion Block Actions

        #region IHasCustomActions

        /// <inheritdoc/>
        List<BlockCustomActionBag> IHasCustomActions.GetCustomActions( bool canEdit, bool canAdministrate )
        {
            var actions = new List<BlockCustomActionBag>();

            if ( canAdministrate )
            {
                actions.Add( new BlockCustomActionBag
                {
                    IconCssClass = "fa fa-edit",
                    Tooltip = "Settings",
                    ComponentFileUrl = "/Obsidian/Blocks/Reporting/pageParameterFilterCustomSettings.obs"
                } );
            }

            return actions;
        }

        #endregion

        #region SupportingMembers

        /// <summary>
        /// A POCO to represent available filter button sizes for the page parameter filter block.
        /// </summary>
        private class FilterButtonSize
        {
            private static readonly ListItemBag _normal = new ListItemBag { Text = "Normal", Value = "1" };
            public static ListItemBag Normal => _normal;

            private static readonly ListItemBag _small = new ListItemBag { Text = "Small", Value = "2" };
            public static ListItemBag Small => _small;

            private static readonly ListItemBag _extraSmall = new ListItemBag { Text = "Extra Small", Value = "3" };
            public static ListItemBag ExtraSmall => _extraSmall;
        }

        /// <summary>
        /// A POCO to represent available filter selection actions for the page parameter filter block.
        /// </summary>
        private class FilterSelectionAction
        {
            private static readonly ListItemBag _doNothing = new ListItemBag { Text = "Do Nothing", Value = "0" };
            public static ListItemBag DoNothing => _doNothing;

            private static readonly ListItemBag _updateFilters = new ListItemBag { Text = "Update Filters", Value = "1" };
            public static ListItemBag UpdateFilters => _updateFilters;

            private static readonly ListItemBag _applyFilters = new ListItemBag { Text = "Apply Filters", Value = "2" };
            public static ListItemBag ApplyFilters => _applyFilters;
        }

        #endregion SupportingMembers
    }
}
