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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Field.Types;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting
{

    /// <summary>
    /// Filter block that passes the filter values as query string parameters.
    /// </summary>
    [DisplayName( "Page Parameter Filter" )]
    [Category( "Reporting" )]
    [Description( "Filter block that passes the filter values as query string parameters." )]

    /*
    ========================================
    SPECIAL NOTES
    ========================================

    1. Selection Action
    -------------------
    4/5/2020 - JME
    This block has a setting 'Selection Action' that allows the block to either do:
    A. Parital Postback (aka Update Block)
    B. Full Postback (aka Update Page)

    The 'Update Block' is odd in that it will take the values from the filter attributes
    and re-build the attribute controls. This is helpful if one or more of the filter attributes
    have dynamic values that are built off of the query string (like a single select that populates
    off of a SQL query that looks at the querystring via Lava).

    When this partial postback occurs the block will create a virtual querystring from all of the
    filter attribute values for Lava to use. This is in the GenerateQueryString() method.

    The concept and code came from a Bema PR.

    ========================================
    */


    #region Block Attributes
    [BooleanField(
        "Show Block Title",
        Key = AttributeKey.ShowBlockTitle,
        Description = "Determines if the Block Title should be displayed",
        DefaultBooleanValue = true,
        Category = "CustomSetting",
        Order = 1 )]

    [TextField(
        "Block Title Text",
        Key = AttributeKey.BlockTitleText,
        Description = "The text to display as the block title.",
        IsRequired = true,
        DefaultValue = "BlockTitle",
        Category = "CustomSetting",
        Order = 2 )]

    [TextField(
        "Block Title Icon CSS Class",
        Key = AttributeKey.BlockTitleIconCssClass,
        Description = "The css class name to use for the block title icon.",
        IsRequired = true,
        DefaultValue = "fa fa-filter",
        Category = "CustomSetting",
        Order = 3 )]

    [IntegerField(
        "Filters Per Row",
        Key = AttributeKey.FiltersPerRow,
        Description = "The number of filters to have per row.  Maximum is 12.",
        IsRequired = true,
        DefaultIntegerValue = 2,
        Category = "CustomSetting",
        Order = 4 )]

    [BooleanField(
        "Show Reset Filters Button",
        Key = AttributeKey.ShowResetFiltersButton,
        Description = "Determines if the Reset Filters button should be displayed",
        DefaultBooleanValue = true,
        Category = "CustomSetting",
        Order = 5 )]

    [TextField(
        "Filter Button Text",
        Key = AttributeKey.FilterButtonText,
        Description = "Sets the button text for the filter button.",
        IsRequired = true,
        DefaultValue = "Filter",
        Category = "CustomSetting",
        Order = 6 )]

    [CustomRadioListField(
        "Filter Button Size",
        Key = AttributeKey.FilterButtonSize,
        Description = "",
        ListSource = "1^Normal, 2^Small, 3^Extra Small",
        IsRequired = true,
        DefaultValue = "3",
        Category = "CustomSetting",
        Order = 7 )]

    [LinkedPage(
        "Redirect Page",
        Key = AttributeKey.RedirectPage,
        Description = "If set, the filter button will redirect to the selected page.",
        IsRequired = false,
        DefaultValue = "",
        Category = "CustomSetting",
        Order = 8 )]

    [CustomDropdownListField( "Selection Action",
        Description = "Specifies what should happen when a value is changed. Nothing, update page, or update block.",
        ListSource = "nothing^,block^Update Block,page^Update Page",
        DefaultValue = "nothing",
        Category = "CustomSetting",
        Key = AttributeKey.DoesSelectionCausePostback,
        Order = 9 )]

    [BooleanField(
        "Hide Filter Actions",
        Key = AttributeKey.HideFilterActions,
        Description = "Hides the filter buttons. This is useful when the Selection action is set to reload the page. Be sure to use this only when the page re-load will be quick.",
        DefaultBooleanValue = false,
        Category = "CustomSetting",
        Order = 10 )]
    #endregion

    public partial class PageParameterFilter : RockBlockCustomSettings, IDynamicAttributesBlock
    {
        #region Enums
        private enum SelectionAction
        {
            Nothing = 0,
            UpdateBlock = 1,
            UpdatePage = 2,
        }
        #endregion

        #region Attribute Keys

        private static class AttributeKey
        {
            public const string ShowBlockTitle = "ShowBlockTitle";
            public const string BlockTitleText = "BlockTitleText";
            public const string BlockTitleIconCssClass = "BlockTitleIconCSSClass";
            public const string FiltersPerRow = "FiltersPerRow";
            public const string ShowResetFiltersButton = "ShowResetFiltersButton";
            public const string FilterButtonText = "FilterButtonText";
            public const string FilterButtonSize = "FilterButtonSize";
            public const string RedirectPage = "RedirectPage";
            public const string DoesSelectionCausePostback = "DoesSelectionCausePostback";
            public const string HideFilterActions = "HideFilterActions";
        }

        #endregion Attribute Keys

        #region Properties
        Dictionary<string, object> CurrentParameters { get; set; }
        #endregion

        #region Fields

        int _blockTypeEntityId;
        Block _block;
        int _filtersPerRow;
        SelectionAction _reloadOnSelection;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            CurrentParameters = ViewState["CurrentParameters"] as Dictionary<string, object>;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _blockTypeEntityId = EntityTypeCache.GetId<Block>().Value;
            _block = new BlockService( new RockContext() ).Get( this.BlockId );

            pnlHeading.Visible = GetAttributeValue( AttributeKey.ShowBlockTitle ).AsBoolean();
            lBlockTitle.Text = GetAttributeValue( AttributeKey.BlockTitleText );
            lBlockTitleIcon.Text = "<i class='" + GetAttributeValue( AttributeKey.BlockTitleIconCssClass ) + "'></i>";

            var filterButtonSize = GetAttributeValue( AttributeKey.FilterButtonSize ).AsInteger();
            btnResetFilters.RemoveCssClass( "btn-sm" ).RemoveCssClass( "btn-xs" );
            btnFilter.RemoveCssClass( "btn-sm" ).RemoveCssClass( "btn-xs" );
            if ( filterButtonSize == 2 )
            {
                btnResetFilters.AddCssClass( "btn-sm" );
                btnFilter.AddCssClass( "btn-sm" );
            }
            else if ( filterButtonSize == 3 )
            {
                btnResetFilters.AddCssClass( "btn-xs" );
                btnFilter.AddCssClass( "btn-xs" );
            }

            int perRow = GetAttributeValue( AttributeKey.FiltersPerRow ).AsInteger();
            if ( perRow > 12 )
            {
                perRow = 12;
            }

            _filtersPerRow = perRow;

            gFilters.DataKeyNames = new string[] { "Id" };
            gFilters.Actions.ShowAdd = true;
            gFilters.Actions.AddClick += gFilters_Add;
            gFilters.Actions.ShowExcelExport = false;
            gFilters.Actions.ShowMergeTemplate = false;

            edtFilter.IsActive = true;
            edtFilter.IsAnalyticsVisible = false;
            edtFilter.IsAnalyticHistory = false;
            edtFilter.IsCategoriesVisible = false;
            edtFilter.IsIndexingEnabled = false;
            edtFilter.IsIconCssClassVisible = false;
            edtFilter.AllowSearchVisible = false;
            edtFilter.IsShowInGridVisible = false;

            // Hide these attribute settings manually since there isn't a property to do so.
            Control cbEnableHistory = edtFilter.FindControl( "_cbEnableHistory" );
            if ( cbEnableHistory != null )
            {
                cbEnableHistory.Visible = false;
            }
            Control cbRequired = edtFilter.FindControl( "cbRequired" );
            if ( cbRequired != null )
            {
                cbRequired.Visible = false;
            }

            var filterButtonText = GetAttributeValue( AttributeKey.FilterButtonText );
            btnFilter.Text = filterButtonText.IsNotNullOrWhiteSpace() ? filterButtonText : "Filter";

            btnResetFilters.Visible = GetAttributeValue( AttributeKey.ShowResetFiltersButton ).AsBoolean();

            var securityField = gFilters.ColumnsOfType<SecurityField>().FirstOrDefault();
            if ( securityField != null )
            {
                securityField.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Attribute ) ).Id;
            }

            mdFilter.SaveClick += mdFilter_SaveClick;

            // This event gets fired after block settings are updated.
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            _reloadOnSelection = GetSelectAction();

            // This is needed so that we can get the data from the controls after all control events
            // have run so that their values are updated.
            Page.LoadComplete += Page_LoadComplete;
        }

        /// <summary>
        /// Handles the LoadComplete event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Page_LoadComplete( object sender, EventArgs e )
        {
            // Add postback controls
            if ( Page.IsPostBack )
            {
                if ( _reloadOnSelection == SelectionAction.UpdateBlock )
                {
                    // See if hidden field has 'true' already set
                    if ( hfPostBack.Value.IsNullOrWhiteSpace() )
                    {
                        var control = Page.FindControl( Request.Form["__EVENTTARGET"] );
                        if ( control != null && control.UniqueID.Contains( "attribute_field_" ) )
                        {
                            // We need to update the form action so that the partial postback call post to the new parameterized url.
                            Page.Form.Action = GetParameterizedUrl();
                            hfPostBack.Value = "True";
                            ScriptManager.RegisterStartupScript( control, control.GetType(), "Refresh-Controls", @"console.log('Doing Postback');  __doPostBack('" + Request.Form["__EVENTTARGET"] + @"','');", true );
                        }
                    }
                    else // Reset hidden field for next time
                    {
                        hfPostBack.Value = "";
                    }
                }
                else if ( _reloadOnSelection == SelectionAction.UpdatePage )
                {
                    var control = Page.FindControl( Request.Form["__EVENTTARGET"] );
                    if ( control != null && control.UniqueID.Contains( "attribute_field_" ) )
                    {
                        btnFilter_Click( null, null );
                    }
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                CurrentParameters = this.RockPage.PageParameters();

                var query = new AttributeService( new RockContext() ).Get( _blockTypeEntityId, "Id", _block.Id.ToString() );
                var attribsWithDefaultValue = query.AsQueryable().Where( a => a.DefaultValue != null && a.DefaultValue != "" ).ToList();

                // If we have any filters with default values, we want to load this block with the page parameters already set.
                if ( attribsWithDefaultValue.Any() && !this.RockPage.PageParameters().Any() )
                {
                    ResetFilters();
                }
                else
                {
                    LoadFilters();
                }
            }
            else
            {
                LoadFilters();
            }

            base.OnLoad( e );

            var hideFilterButtons = GetAttributeValue( AttributeKey.HideFilterActions ).AsBoolean();
            btnFilter.Visible = !hideFilterButtons;
            btnResetFilters.Visible = !hideFilterButtons;
        }

        protected override object SaveViewState()
        {
            ViewState["CurrentParameters"] = CurrentParameters;

            return base.SaveViewState();
        }
        #endregion

        #region Settings

        /// <summary>
        /// Shows the settings.
        /// </summary>
        protected override void ShowSettings()
        {
            pnlSettings.Visible = true;
            cbShowBlockTitle.Checked = GetAttributeValue( AttributeKey.ShowBlockTitle ).AsBoolean();
            rtbBlockTitleText.Text = GetAttributeValue( AttributeKey.BlockTitleText );
            rtbBlockTitleIconCssClass.Text = GetAttributeValue( AttributeKey.BlockTitleIconCssClass );
            nbFiltersPerRow.Text = GetAttributeValue( AttributeKey.FiltersPerRow );
            cbShowResetFiltersButton.Checked = GetAttributeValue( AttributeKey.ShowResetFiltersButton ).AsBoolean();
            cbHideFilterActions.Checked = GetAttributeValue( AttributeKey.HideFilterActions ).AsBoolean();
            rtbFilterButtonText.Text = GetAttributeValue( AttributeKey.FilterButtonText );
            ddlFilterButtonSize.SetValue( GetAttributeValue( AttributeKey.FilterButtonSize ).AsInteger() );
            var ppFieldType = new PageReferenceFieldType();
            ppFieldType.SetEditValue( ppRedirectPage, null, GetAttributeValue( AttributeKey.RedirectPage ) );

            ddlSelectionAction.SelectedValue = GetSelectAction().ConvertToInt().ToString();

            BindGrid();

            mdSettings.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdSettings_SaveClick( object sender, EventArgs e )
        {
            SetAttributeValue( AttributeKey.ShowBlockTitle, cbShowBlockTitle.Checked.ToString() );
            SetAttributeValue( AttributeKey.BlockTitleText, rtbBlockTitleText.Text );
            SetAttributeValue( AttributeKey.BlockTitleIconCssClass, rtbBlockTitleIconCssClass.Text );
            SetAttributeValue( AttributeKey.FiltersPerRow, nbFiltersPerRow.Text );
            SetAttributeValue( AttributeKey.ShowResetFiltersButton, cbShowResetFiltersButton.Checked.ToString() );
            SetAttributeValue( AttributeKey.HideFilterActions, cbHideFilterActions.Checked.ToString() );
            SetAttributeValue( AttributeKey.FilterButtonText, rtbFilterButtonText.Text );
            SetAttributeValue( AttributeKey.FilterButtonSize, ddlFilterButtonSize.SelectedValue );
            var ppFieldType = new PageReferenceFieldType();
            SetAttributeValue( AttributeKey.RedirectPage, ppFieldType.GetEditValue( ppRedirectPage, null ) );
            SetAttributeValue( AttributeKey.DoesSelectionCausePostback, ddlSelectionAction.SelectedValue );

            SaveAttributeValues();

            mdSettings.Hide();
            pnlSettings.Visible = false;

            // reload the page to make sure we have a clean load
            ResetFilters();
            NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Handles the GridReorder events.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gFilters_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = new RockContext();
            var attributeService = new AttributeService( rockContext );
            var qry = attributeService.Get( _blockTypeEntityId, "Id", _block.Id.ToString() );
            qry = qry.OrderBy( a => a.Order );
            var updatedAttributeIds = attributeService.Reorder( qry.ToList(), e.OldIndex, e.NewIndex );

            rockContext.SaveChanges();

            BindGrid();
            LoadFilters();
        }

        /// <summary>
        /// Handles the RowDataBound event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs" /> instance containing the event data.</param>
        protected void gFilters_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                int attributeId = ( int ) gFilters.DataKeys[e.Row.RowIndex].Value;

                var attribute = Rock.Web.Cache.AttributeCache.Get( attributeId );
                var fieldType = FieldTypeCache.Get( attribute.FieldTypeId );

                Literal lDefaultValue = e.Row.FindControl( "lDefaultValue" ) as Literal;
                if ( lDefaultValue != null )
                {
                    lDefaultValue.Text = fieldType.Field.FormatValueAsHtml( lDefaultValue, attribute.EntityTypeId, null, attribute.DefaultValue, attribute.QualifierValues, true );
                }
            }
        }

        /// <summary>
        /// Handles the Edit filters event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gFilters_Edit( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var attributeService = new AttributeService( rockContext );
            var attribute = new AttributeService( rockContext ).Get( e.RowKeyId );

            edtFilter.ReservedKeyNames = attributeService.Get( _blockTypeEntityId, "Id", _block.Id.ToString() )
                 .Where( a => a.Id != e.RowKeyId )
                 .Select( a => a.Key )
                 .Distinct()
                 .ToList();

            edtFilter.SetAttributeProperties( attribute );
  
            mdFilter.Title = "Edit Filter";
            mdFilter.Show();
        }

        /// <summary>
        /// Handes the Delete filters event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gFilters_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var attributeService = new AttributeService( rockContext );

            var attribute = attributeService.Get( e.RowKeyId );
            if ( attribute != null )
            {
                attributeService.Delete( attribute );

                rockContext.SaveChanges();
            }

            BindGrid();
            LoadFilters();
        }

        /// <summary>
        /// Handles the Add actions event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gFilters_Add( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var attributeService = new AttributeService( rockContext );

            // reset editor
            edtFilter.Name = "";
            edtFilter.Key = "";
            edtFilter.AttributeId = null;
            edtFilter.IsFieldTypeEditable = true;
            edtFilter.SetAttributeFieldType( FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT ).Id, null );

            edtFilter.ReservedKeyNames = attributeService.Get( _blockTypeEntityId, "Id", _block.Id.ToString() )
                 .Select( a => a.Key )
                 .Distinct()
                 .ToList();

            mdFilter.Title = "Add Filter";
            mdFilter.Show();
        }

        /// <summary>
        /// Handles the Save filters event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdFilter_SaveClick( object sender, EventArgs e )
        {
            Rock.Model.Attribute attribute = null;

            // sets the attribute to use the "CustomSetting" attribute
            var entityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.ATTRIBUTE ).Id;
            var entityId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.BLOCK ).Id;

            edtFilter.CategoryIds = new CategoryService( new RockContext() ).Queryable().Where( c => c.Name == "CustomSetting" &&
                                                                                                c.EntityTypeId == entityTypeId &&
                                                                                                c.EntityTypeQualifierColumn == "EntityTypeId" &&
                                                                                                c.EntityTypeQualifierValue == entityId.ToString() )
                                                                                        .Select( c => c.Id );

            attribute = Helper.SaveAttributeEdits( edtFilter, _blockTypeEntityId, "Id", _block.Id.ToString() );

            // Attribute will be null if it was not valid
            if ( attribute == null )
            {
                return;
            }

            mdFilter.Hide();
            BindGrid();
        }

        /// <summary>
        /// Binds the filter grid.
        /// </summary>
        private void BindGrid()
        {
            IQueryable<Rock.Model.Attribute> query = null;
            var attributeService = new AttributeService( new RockContext() );

            query = attributeService.Get( _blockTypeEntityId, "Id", _block.Id.ToString() );
            gFilters.DataSource = query.OrderBy( a => a.Order ).ToList();
            gFilters.DataBind();
        }

        #endregion Settings

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            LoadFilters();
        }

        /// <summary>
        /// Handles the btnFilter event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnFilter_Click( object sender, EventArgs e )
        {
            Response.Redirect( GetParameterizedUrl(), false );
        }

        /// <summary>
        /// Handles the btnResetFilters event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnResetFilters_Click( object sender, EventArgs e )
        {
            ResetFilters();
        }

        /// <summary>
        /// Handles the SelectItem event from an ItemPicker (fake event to register the postback)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemPicker_SelectItem( object sender, EventArgs e )
        {
            // Hopefully an xhr happens here
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the filters.
        /// </summary>
        private void LoadFilters()
        {
            BuildControls();

            if ( _block.Attributes != null )
            {
                foreach ( var attribute in _block.Attributes )
                {
                    var attributeCache = AttributeCache.Get( attribute.Value.Guid );

                    Control control = phAttributes.FindControl( string.Format( "attribute_field_{0}", attribute.Value.Id ) );
                    if ( control != null )
                    {
                        var value = PageParameter( attribute.Key );
                        if ( value.IsNotNullOrWhiteSpace() )
                        {
                            attributeCache.FieldType.Field.SetEditValue( control, null, value );
                        }
                        else if ( attribute.Value.DefaultValue.IsNotNullOrWhiteSpace() )
                        {
                            attributeCache.FieldType.Field.SetEditValue( control, null, attribute.Value.DefaultValue );
                        }

                        // Enable ListControls postback and Event
                        if ( control is ListControl && _reloadOnSelection != SelectionAction.Nothing )
                        {
                            var listControl = control as ListControl;
                            listControl.AutoPostBack = true;
                        }

                        // Enable ItemPicker postback event
                        if ( control is ItemPicker && _reloadOnSelection != SelectionAction.Nothing )
                        {
                            var itemPicker = control as ItemPicker;
                            itemPicker.SelectItem += ItemPicker_SelectItem;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Builds the filter controls.
        /// </summary>
        private void BuildControls()
        {
            var query = new AttributeService( new RockContext() ).Get( _blockTypeEntityId, "Id", _block.Id.ToString() );
            var attributes = query.OrderBy( a => a.Order ).ToList();

            var exclusions = new List<string>();
            exclusions.Add( AttributeKey.RedirectPage );

            _block.LoadAttributes();
            phAttributes.Controls.Clear();

            var attributeKeys = new List<string>();
            foreach ( var attribute in attributes )
            {
                if ( attribute.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    attributeKeys.Add( attribute.Key );
                }
            }

            try
            {
                Helper.AddEditControls( "", attributeKeys, _block, phAttributes, "", false, exclusions, _filtersPerRow );
            }
            catch
            {
                pnlFilters.Visible = false;
                nbBuildErrors.Visible = true;
                nbBuildErrors.Text = "Not all filter controls could be built. The most likely cause of this issue is a mis-configured filter.";
            }
        }

        /// <summary>
        /// Resets the filters to their original state.  Any filters with default values will be set as well.
        /// </summary>
        private void ResetFilters()
        {
            BuildControls();

            NameValueCollection queryString = GenerateQueryString();

            if ( queryString.AllKeys.Any() )
            {
                Response.Redirect( string.Format( "{0}?{1}", Request.Url.AbsolutePath, queryString ), false );
            }
            else
            {
                Response.Redirect( Request.Url.AbsolutePath, false );
            }
        }

        /// <summary>
        /// Generates the query string.
        /// </summary>
        /// <returns></returns>
        private NameValueCollection GenerateQueryString()
        {
            var queryString = HttpUtility.ParseQueryString( String.Empty );
            foreach ( var parameter in CurrentParameters )
            {
                if ( parameter.Key != "PageId" )
                {
                    queryString.Set( parameter.Key, parameter.Value.ToString() );
                }
            }

            _block.LoadAttributes( new RockContext() );

            if ( _block.Attributes != null )
            {
                foreach ( var attribute in _block.Attributes )
                {
                    Control control = phAttributes.FindControl( string.Format( "attribute_field_{0}", attribute.Value.Id ) );
                    if ( control != null )
                    {
                        string value = attribute.Value.FieldType.Field.GetEditValue( control, attribute.Value.QualifierValues );

                        if ( value.IsNotNullOrWhiteSpace() )
                        {
                            queryString.Set( attribute.Key, value );
                        }
                        else
                        {
                            queryString.Remove( attribute.Key );
                        }
                    }
                }
            }

            return queryString;
        }

        /// <summary>
        /// Gets the parameterized URL.
        /// </summary>
        /// <returns></returns>
        private string GetParameterizedUrl()
        {
            var queryString = GenerateQueryString();
            var url = Request.Url.AbsolutePath;

            var pageGuid = GetAttributeValue( AttributeKey.RedirectPage ).AsGuidOrNull();
            if ( pageGuid.HasValue )
            {
                var page = PageCache.Get( pageGuid.Value );

                url = VirtualPathUtility.ToAbsolute( string.Format( "~/page/{0}", page.Id ) );
            }

            if ( queryString.AllKeys.Any() )
            {
                // JE 2/19/2021
                // Fixing to support routes. This should probably be in the GenerateQueryString() but it's difficult to understand
                // the logic of that method. This is slightly slower, but a safer change. A re-write of the filter redirect may be in order.
                var pageReference = CurrentPageReference;

                foreach ( var key in queryString.AllKeys )
                {
                    pageReference.Parameters.AddOrReplace( key, queryString[key] );
                }

                return pageReference.BuildUrl();
            }
            else
            {
                return url;
            }
        }

        /// <summary>
        /// Gets the select action.
        /// </summary>
        /// <returns></returns>
        private SelectionAction GetSelectAction()
        {
            var attributeValue = GetAttributeValue( AttributeKey.DoesSelectionCausePostback );
            if ( attributeValue.IsNullOrWhiteSpace() )
            {
                return SelectionAction.Nothing;
            }

            if ( attributeValue.Equals( "false", StringComparison.InvariantCultureIgnoreCase ) )
            {
                return SelectionAction.Nothing;
            }

            if ( attributeValue.Equals( "true", StringComparison.InvariantCultureIgnoreCase ) )
            {
                return SelectionAction.UpdateBlock;
            }

            return attributeValue.ConvertToEnum<SelectionAction>();
        }
        #endregion

    }
}