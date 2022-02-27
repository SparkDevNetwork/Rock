using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace RockWeb.Plugins.rocks_pillars.Cms
{
    /// <summary>
    /// Filter block that passes filter values as query string parameters.
    /// </summary>
    [DisplayName( "Page Parameter Filters" )]
    [Category( "Pillars > Cms" )]
    [Description( "A customizable set of filters that set query string parameters." )]

    [TextField( "Heading", "The text to display as the heading.", true, "Filters", "", 1 )]
    [TextField( "Heading Icon CSS Class", "The css class name to use for the heading icon. ", true, "fa fa-filter", "", 2 )]
    [BooleanField( "Show Reset Filters", "Determines if the Reset Filters button should be displayed", true, "", 3 )]
    [TextField( "Filter Button Text", "Sets the button text for the filter button.", true, "Filter", "", 4)]
    [LinkedPage( "Page Redirect", "If set, the filter button will redirect to the selected page.", false, "", "", 5 )]

    [TextField( "FilterAttributesExtendedSettings", "Internal JSON used for the additional filter attribute settings.", false, "", "CustomSetting" )]
    public partial class PageParameterFilters : RockBlock, IDynamicAttributesBlock
    {
        #region Fields

        private int _blockTypeEntityId;
        private Block _block;
        private Dictionary<Guid, FilterAttributeExtendedSettings> _filterAttributesExtendedSettings = null;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _blockTypeEntityId = EntityTypeCache.GetId<Block>().Value;
            _block = new BlockService( new RockContext() ).Get( this.BlockId );

            _filterAttributesExtendedSettings = GetAttributeValue( "FilterAttributesExtendedSettings" ).FromJsonOrNull<Dictionary<Guid, FilterAttributeExtendedSettings>>() ?? new Dictionary<Guid, FilterAttributeExtendedSettings>();

              lHeading.Text = GetAttributeValue( "Heading" );
            lHeadingIcon.Text = "<i class='" + GetAttributeValue( "HeadingIconCSSClass" ) + "'></i>";

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
            var pwAdvanced = edtFilter.FindControl( "pwAdvanced" ) as PanelWidget;
            if ( pwAdvanced != null )
            {
                var cbEnableHistory = pwAdvanced.FindControl( "_cbEnableHistory" );
                if ( cbEnableHistory != null )
                {
                    cbEnableHistory.Visible = false;
                }
            }

            var cbRequired = edtFilter.FindControl( "cbRequired" );
            if ( cbRequired != null )
            {
                cbRequired.Visible = false;
            }

            lbEdit.Visible = IsUserAuthorized( "Edit" );

            var filterButtonText = GetAttributeValue( "FilterButtonText" );
            btnFilter.Text = filterButtonText.IsNotNullOrWhiteSpace() ? filterButtonText : "Filter";

            btnResetFilters.Visible = GetAttributeValue( "ShowResetFilters" ).AsBoolean();

            var securityField = gFilters.ColumnsOfType<SecurityField>().FirstOrDefault();
            if ( securityField != null )
            {
                securityField.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Attribute ) ).Id;
            }

            lbClose.NavigateUrl = Request.Url.AbsolutePath;

            mdFilter.SaveClick += mdFilter_SaveClick;

            // this event gets fired after block settings are updated.
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if (!Page.IsPostBack)
            {
                var query = new AttributeService( new RockContext() ).Get( _blockTypeEntityId, "Id", _block.Id.ToString() );
                var attribsWithDefaultValue = query.AsQueryable().Where( a => a.DefaultValue != null && a.DefaultValue != "" ).ToList();

                // if we have any filters with default values, we want to load this block with the page parameters already set.
                if ( attribsWithDefaultValue.Any() && !this.RockPage.PageParameters().Where( p => attribsWithDefaultValue.Select( a => a.Key ).Contains( p.Key ) ).Any() )
                {
                    ResetFilters();
                }
                else
                {
                    BuildControls();
                }                
            }
            else
            {
                BuildControls();
            }          

            base.OnLoad( e );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Builds the filter controls.
        /// </summary>
        private void BuildControls()
        {
            var query = new AttributeService( new RockContext() ).Get( _blockTypeEntityId, "Id", _block.Id.ToString() );
            var attributes = query.OrderBy( a => a.Order ).ToList();

            var exclusions = new List<string>();
            exclusions.Add( "PageRedirect" );

            _block.LoadAttributes();
            phAttributes.Controls.Clear();

            foreach ( var attribute in attributes )
            {
                // get extended settings
                bool hideLabel = false;
                string preHtml = "";
                string postHtml = "";
                var filterAttribExtendedSetting = _filterAttributesExtendedSettings.GetValueOrNull( attribute.Guid );
                if ( filterAttribExtendedSetting != null )
                {
                    hideLabel = filterAttribExtendedSetting.HideLabel;
                    preHtml = filterAttribExtendedSetting.PreHtml;
                    postHtml = filterAttribExtendedSetting.PostHtml;
                }

                // pre html
                if ( preHtml.IsNotNullOrWhiteSpace() )
                {
                    phAttributes.Controls.Add( new LiteralControl( filterAttribExtendedSetting.PreHtml ) );
                }

                // build filter control (if allowed)
                if ( attribute.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    var attributeCache = AttributeCache.Get( attribute.Guid );
                    var value = Request.QueryString[attribute.Key];
                    if ( value != null )
                    {
                        attributeCache.AddControl( phAttributes.Controls, value, BlockValidationGroup, true, true, false, hideLabel ? "" : attribute.Name, attribute.Description );
                    }
                    else if ( attribute.DefaultValue != null )
                    {
                        attributeCache.AddControl( phAttributes.Controls, attribute.DefaultValue, BlockValidationGroup, true, true, false, hideLabel ? "" : attribute.Name, attribute.Description );
                    }
                }

                // post html
                if ( postHtml.IsNotNullOrWhiteSpace() )
                {
                    phAttributes.Controls.Add( new LiteralControl( filterAttribExtendedSetting.PostHtml ) );
                }
            }
        }

        /// <summary>
        /// Resets the filters to their original state.  Any filters with default values will be set as well.
        /// </summary>
        private void ResetFilters()
        {
            var queryString = HttpUtility.ParseQueryString( String.Empty );

            BuildControls();

            if ( _block.Attributes != null )
            {
                foreach ( var attribute in _block.Attributes )
                {
                    Control control = phAttributes.FindControl( string.Format( "attribute_field_{0}", attribute.Value.Id ) );
                    if ( control != null )
                    {
                        foreach( var parameter in GetQueryStringParameters( attribute.Value, attribute.Value.DefaultValue ) )
                        { 
                            queryString.Set( parameter.Key, parameter.Value );
                        }
                    }
                }
            }

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

        /// <summary>
        /// Gets the query string paramters for the control
        /// </summary>
        private Dictionary<string, string> GetQueryStringParameters( AttributeCache attribute, string value )
        {
            var parameters = new Dictionary<string, string>();

            if ( attribute != null && attribute.FieldType != null && value.IsNotNullOrWhiteSpace() )
            {
                parameters.Add( attribute.Key, value );

                if ( attribute.FieldType.Guid == Rock.SystemGuid.FieldType.SLIDING_DATE_RANGE.AsGuid() )
                {
                    var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( value );
                    if ( dateRange != null )
                    {
                        if ( dateRange.Start != null )
                        {
                            parameters.Add( attribute.Key + "Start", dateRange.Start.Value.ToShortDateTimeString() );
                        }
                        if ( dateRange.End != null )
                        {
                            parameters.Add( attribute.Key + "End", dateRange.End.Value.ToShortDateTimeString() );
                        }
                    }
                }

                else if ( attribute.FieldType.Guid == Rock.SystemGuid.FieldType.DATE_RANGE.AsGuid() )
                {
                    var dateRange = DateRangePicker.CalculateDateRangeFromDelimitedValues( value );
                    if ( dateRange != null )
                    {
                        if ( dateRange.Start != null )
                        {
                            parameters.Add( attribute.Key + "Start", dateRange.Start.Value.ToShortDateTimeString() );
                        }
                        if ( dateRange.End != null )
                        {
                            parameters.Add( attribute.Key + "End", dateRange.End.Value.ToShortDateTimeString() );
                        }
                    }
                }
            }

            return parameters;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BuildControls();
        }

        /// <summary>
        /// Handles the btnFilter event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnFilter_Click( object sender, EventArgs e )
        {
            var queryString = HttpUtility.ParseQueryString( String.Empty );
            _block.LoadAttributes( new RockContext() );

            if ( _block.Attributes != null )
            {
                foreach ( var attribute in _block.Attributes )
                {
                    Control control = phAttributes.FindControl( string.Format( "attribute_field_{0}", attribute.Value.Id ) );
                    if ( control != null )
                    {
                        foreach ( var parameter in GetQueryStringParameters( attribute.Value, attribute.Value.FieldType.Field.GetEditValue( control, attribute.Value.QualifierValues ) ) )
                        {
                            queryString.Set( parameter.Key, parameter.Value );
                        }
                    }
                }
            }

            string url = Request.Url.AbsolutePath;

            Guid? pageGuid = GetAttributeValue( "PageRedirect" ).AsGuidOrNull();
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
        /// Handles the EditFilters event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEditFilters_Click( object sender, EventArgs e )
        {
            BindGrid();

            lHeading.Text = "Filter Settings";
            lHeadingIcon.Visible = false;
            lbEdit.Visible = false;

            pnlEdit.Visible = true;
            pnlFilters.Visible = false;
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
            edtFilter.AttributeGuid = Guid.NewGuid();
            edtFilter.IsFieldTypeEditable = true;
            edtFilter.SetAttributeFieldType( FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT ).Id, null );
            edtFilter.PreHtml = "";
            edtFilter.PostHtml = "";
            cbHideLabel.Checked = false;

            edtFilter.ReservedKeyNames = attributeService.Get( _blockTypeEntityId, "Id", _block.Id.ToString() )
                 .Select( a => a.Key )
                 .Distinct()
                 .ToList();

            mdFilter.Title = "Add Filter";
            mdFilter.Show();
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

            _filterAttributesExtendedSettings = GetAttributeValue( "FilterAttributesExtendedSettings" ).FromJsonOrNull<Dictionary<Guid, FilterAttributeExtendedSettings>>() ?? new Dictionary<Guid, FilterAttributeExtendedSettings>();

            var filterAttribExtendedSetting = _filterAttributesExtendedSettings.GetValueOrNull( attribute.Guid );
            if ( filterAttribExtendedSetting != null )
            {
                edtFilter.PreHtml = filterAttribExtendedSetting.PreHtml;
                edtFilter.PostHtml = filterAttribExtendedSetting.PostHtml;
                cbHideLabel.Checked = filterAttribExtendedSetting.HideLabel;
            }
            else
            {
                edtFilter.PreHtml = "";
                edtFilter.PreHtml = "";
                cbHideLabel.Checked = false;
            }

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
            BuildControls();
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
                int attributeId = (int)gFilters.DataKeys[e.Row.RowIndex].Value;

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

            // save additional settings
            _filterAttributesExtendedSettings = GetAttributeValue( "FilterAttributesExtendedSettings" ).FromJsonOrNull<Dictionary<Guid, FilterAttributeExtendedSettings>>() ?? new Dictionary<Guid, FilterAttributeExtendedSettings>();

            FilterAttributeExtendedSettings formFields = new FilterAttributeExtendedSettings();
            formFields.HideLabel = cbHideLabel.Checked;
            formFields.PreHtml = edtFilter.PreHtml;
            formFields.PostHtml = edtFilter.PostHtml;

            _filterAttributesExtendedSettings.AddOrReplace( attribute.Guid, formFields );
            SetAttributeValue( "FilterAttributesExtendedSettings", _filterAttributesExtendedSettings.ToJson() );
            SaveAttributeValues();

            // Attribute will be null if it was not valid
            if ( attribute == null )
            {
                return;
            }

            mdFilter.Hide();
            BindGrid();
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        class FilterAttributeExtendedSettings
        {
            /// <summary>
            /// Gets or sets the hide label
            /// </summary>
            /// <value>
            /// The hide label setting.
            /// </value>
            public bool HideLabel { get; set; }

            /// <summary>
            /// Gets or sets the pre HTML.
            /// </summary>
            /// <value>
            /// The pre HTML.
            /// </value>
            public string PreHtml { get; set; }

            /// <summary>
            /// Gets or sets the post HTML.
            /// </summary>
            /// <value>
            /// The post HTML.
            /// </value>
            public string PostHtml { get; set; }
        }
    }
}