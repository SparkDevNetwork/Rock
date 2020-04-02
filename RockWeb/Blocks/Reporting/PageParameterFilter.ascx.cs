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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web.UI;
using System.Web;
using Rock.Security;
using Rock.Field.Types;
using System.Collections.Specialized;

namespace RockWeb.Blocks.Reporting
{

    /// <summary>
    /// Filter block that passes the filter values as query string parameters.
    /// </summary>
    [DisplayName( "Page Parameter Filter" )]
    [Category( "Reporting" )]
    [Description( "Filter block that passes the filter values as query string parameters." )]

    #region Block Attributes
    [BooleanField(
        "Show Block Title",
        Key = AttributeKey.ShowBlockTitle,
        Description = "Determines if the Block Title should be displayed",
        DefaultBooleanValue = true,
        Category = "CustomSetting",
        Order=1 )]

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

    [BooleanField(
        "Does Selection Cause Postback",
        Key = AttributeKey.DoesSelectionCausePostback,
        Description = "If set, selecting a filter will force a PostBack, recalculating the available selections. Useful for SQL values.",
        DefaultBooleanValue = false,
        Category = "CustomSetting",
        Order = 9  )]
    #endregion

    public partial class PageParameterFilter : RockBlockCustomSettings, IDynamicAttributesBlock
    {
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
        }

        #endregion Attribute Keys

        #region Properties
        Dictionary<string, object> CurrentParameters { get; set; }
        #endregion

        #region Fields

        int _blockTypeEntityId;
        Block _block;
        int _filtersPerRow;
        bool _reloadOnSelection;

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

            // hide these attribute settings manually since there isn't a property to do so.
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

            // this event gets fired after block settings are updated.
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            _reloadOnSelection = GetAttributeValue( AttributeKey.DoesSelectionCausePostback ).AsBoolean();
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

                // if we have any filters with default values, we want to load this block with the page parameters already set.
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

            //add postback controls
            if ( Page.IsPostBack && _reloadOnSelection )
            {
                //See if hidden field has 'true' already set
                if ( hfPostBack.Value.IsNullOrWhiteSpace() )
                {
                    var control = Page.FindControl( Request.Form["__EVENTTARGET"] );
                    if ( control != null && control.UniqueID.Contains( "attribute_field_" ) )
                    {
                        hfPostBack.Value = "True";
                        ScriptManager.RegisterStartupScript( control, control.GetType(), "Refresh-Controls", @"console.log('Doing Postback');  __doPostBack('" + Request.Form["__EVENTTARGET"] + @"','');", true );
                    }
                }
                else //reset hidden field for next time
                {
                    hfPostBack.Value = "";
                }
            }
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
            rtbFilterButtonText.Text = GetAttributeValue( AttributeKey.FilterButtonText );
            ddlFilterButtonSize.SetValue( GetAttributeValue( AttributeKey.FilterButtonSize ).AsInteger() );
            var ppFieldType = new PageReferenceFieldType();
            ppFieldType.SetEditValue( ppRedirectPage, null, GetAttributeValue( AttributeKey.RedirectPage ) );
            cbDoesSelectionCausePostback.Checked = GetAttributeValue( AttributeKey.DoesSelectionCausePostback ).AsBoolean();
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
            SetAttributeValue( AttributeKey.FilterButtonText, rtbFilterButtonText.Text );
            SetAttributeValue( AttributeKey.FilterButtonSize, ddlFilterButtonSize.SelectedValue );
            var ppFieldType = new PageReferenceFieldType();
            SetAttributeValue( AttributeKey.RedirectPage, ppFieldType.GetEditValue( ppRedirectPage, null ) );
            SetAttributeValue( AttributeKey.DoesSelectionCausePostback, cbDoesSelectionCausePostback.Checked.ToString() );

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
            NameValueCollection queryString = GenerateQueryString();

            string url = Request.Url.AbsolutePath;

            Guid? pageGuid = GetAttributeValue( AttributeKey.RedirectPage ).AsGuidOrNull();
            if ( pageGuid.HasValue )
            {
                var page = PageCache.Get( pageGuid.Value );

                url = System.Web.VirtualPathUtility.ToAbsolute( string.Format( "~/page/{0}", page.Id ) );
            }

            if ( queryString.AllKeys.Any() )
            {
                Response.Redirect( string.Format( "{0}?{1}", url, queryString ), false );
            }
            else
            {
                Response.Redirect( url, false );
            }
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
            //hopefully an xhr happens here
        }

        /// <summary>
        /// Handles the ListControl Selected Index Changed event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Control_PreRender( object sender, EventArgs e )
        {
            // Update the internal URL querystring via postback call
            PostBackUpdateQueryString();
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
                        var value = Request.QueryString[attribute.Key];
                        if ( value != null )
                        {
                            attributeCache.FieldType.Field.SetEditValue( control, null, value );
                        }
                        else if ( attribute.Value.DefaultValue != null )
                        {
                            attributeCache.FieldType.Field.SetEditValue( control, null, attribute.Value.DefaultValue );
                        }

                        control.PreRender += Control_PreRender;

                        // Enable ListControls postback and Event
                        if ( control is ListControl && _reloadOnSelection )
                        {
                            var listControl = control as ListControl;
                            listControl.AutoPostBack = true;
                        }

                        // Enable ItemPicker postback event
                        if ( control is ItemPicker && _reloadOnSelection )
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

            Helper.AddEditControls( "", attributeKeys, _block, phAttributes, "", false, exclusions, _filtersPerRow );
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
        /// Updates the internal Query String with control's selections
        /// </summary>
        private void PostBackUpdateQueryString()
        {
            var queryString = GenerateQueryString();

            //Change query string without redirect ( a little bit of a hack )
            System.Reflection.PropertyInfo isreadonly = typeof( NameValueCollection ).GetProperty( "IsReadOnly", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic );

            if ( Request != null )
            {
                isreadonly.SetValue( Request.QueryString, false, null );
                Request.QueryString.Clear();
                Request.QueryString.Add( queryString );
                isreadonly.SetValue( Request.QueryString, true, null );
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

        #endregion

    }
}