using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting
{
    [DisplayName( "Report Data" )]
    [Category( "Reporting" )]
    [Description( "Block to display a report with options to edit the filter" )]

    [TextField( "Report", "The report to use for this block", false, "", "CustomSetting" )]
    [TextField( "SelectedDataFieldGuids", "The DataFilters to present to the user", false, "", "CustomSetting" )]
    [TextField( "ConfigurableDataFieldGuids", "Of the DataFilters that are presented to the user, which are configurable vs just a checkbox", false, "", "CustomSetting" )]
    public partial class ReportData : RockBlockCustomSettings
    {

        /// <summary>
        /// Gets the settings tool tip.
        /// </summary>
        /// <value>
        /// The settings tool tip.
        /// </value>
        public override string SettingsToolTip
        {
            get
            {
                return "Configure";
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            if ( !this.IsPostBack )
            {

                BindFilter();
            }
        }

        /// <summary>
        /// Shows the settings.
        /// </summary>
        protected override void ShowSettings()
        {
            pnlConfigure.Visible = true;
            LoadDropDowns();
            ddlReport.SetValue( this.GetAttributeValue( "Report" ).AsGuidOrNull() );
            ddlReport_SelectedIndexChanged( null, null );
            mdConfigure.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdConfigure control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdConfigure_SaveClick( object sender, EventArgs e )
        {
            mdConfigure.Hide();
            pnlConfigure.Visible = false;

            var selectCols = grdDataFilters.Columns.OfType<SelectField>().ToArray();
            var selectedDataFieldGuids = selectCols[0].SelectedKeys.OfType<Guid>();
            var configurableDataFieldGuids = selectCols[1].SelectedKeys.OfType<Guid>();

            this.SetAttributeValue( "SelectedDataFieldGuids", selectedDataFieldGuids.Select( a => a.ToString() ).ToList().AsDelimited( "|" ) );
            this.SetAttributeValue( "ConfigurableDataFieldGuids", configurableDataFieldGuids.Select( a => a.ToString() ).ToList().AsDelimited( "|" ) );
            this.SetAttributeValue( "Report", ddlReport.SelectedValue.AsGuidOrNull().ToString() );
            SaveAttributeValues();

            BindFilter();
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        protected void LoadDropDowns()
        {
            var rockContext = new RockContext();
            var reportService = new ReportService( rockContext );
            var reportQry = reportService.Queryable().OrderBy( a => a.Name );

            ddlReport.Items.Clear();
            ddlReport.Items.Add( new ListItem() );
            foreach ( var report in reportQry.ToList() )
            {
                ddlReport.Items.Add( new ListItem( report.Name, report.Guid.ToString() ) );
            }
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        protected void BindFilter()
        {
            var rockContext = new RockContext();
            var reportService = new ReportService( rockContext );

            var reportGuid = this.GetAttributeValue( "Report" ).AsGuidOrNull();
            var dataFilterGuids = ( this.GetAttributeValue( "DataFilters" ) ?? string.Empty ).Split( '|' ).AsGuidList();
            Report report = null;
            if ( reportGuid.HasValue )
            {
                report = reportService.Get( reportGuid.Value );
            }

            if ( report == null )
            {
                nbConfigurationWarning.Visible = true;
                nbConfigurationWarning.Text = "A report needs to be configured in block settings";
                pnlView.Visible = false;
            }
            else
            {
                nbConfigurationWarning.Visible = false;
                if ( report.DataView != null && report.DataView.DataViewFilter != null )
                {
                    CreateFilterControl( report.EntityTypeId, report.DataView.DataViewFilter, true, rockContext );
                }
            }
        }

        /// <summary>
        /// Creates the filter control.
        /// </summary>
        /// <param name="filteredEntityTypeId">The filtered entity type identifier.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="setSelection">if set to <c>true</c> [set selection].</param>
        /// <param name="rockContext">The rock context.</param>
        private void CreateFilterControl( int? filteredEntityTypeId, DataViewFilter filter, bool setSelection, RockContext rockContext )
        {
            phFilters.Controls.Clear();
            if ( filter != null && filteredEntityTypeId.HasValue )
            {
                var filteredEntityType = EntityTypeCache.Read( filteredEntityTypeId.Value );
                CreateFilterControl( phFilters, filter, filteredEntityType.Name, setSelection, rockContext );
            }
        }

        /// <summary>
        /// Creates the filter control.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="filteredEntityTypeName">Name of the filtered entity type.</param>
        /// <param name="setSelection">if set to <c>true</c> [set selection].</param>
        /// <param name="rockContext">The rock context.</param>
        private void CreateFilterControl( Control parentControl, DataViewFilter filter, string filteredEntityTypeName, bool setSelection, RockContext rockContext )
        {
            if ( filter.ExpressionType == FilterExpressionType.Filter )
            {
                var filterControl = new FilterField();
                parentControl.Controls.Add( filterControl );
                filterControl.DataViewFilterGuid = filter.Guid;
                filterControl.ID = string.Format( "ff_{0}", filterControl.DataViewFilterGuid.ToString( "N" ) );
                filterControl.FilteredEntityTypeName = filteredEntityTypeName;

                if ( filter.EntityTypeId.HasValue )
                {
                    var entityTypeCache = Rock.Web.Cache.EntityTypeCache.Read( filter.EntityTypeId.Value, rockContext );
                    if ( entityTypeCache != null )
                    {
                        filterControl.FilterEntityTypeName = entityTypeCache.Name;
                    }
                }

                filterControl.Expanded = true;
                filterControl.IsFilterTypeConfigurable = false;
                filterControl.Selection = filter.Selection;

                var entityType = EntityTypeCache.Read( filter.EntityTypeId ?? 0 );
                var component = Rock.Reporting.DataFilterContainer.GetComponent( entityType.Name );
                if ( component != null )
                {
                    if ( component is Rock.Reporting.DataFilter.PropertyFilter )
                    {
                        var propertyFilter = ( component as Rock.Reporting.DataFilter.PropertyFilter );
                        propertyFilter.IsEntityFieldConfigurable = false;
                        var fieldName = propertyFilter.GetSelectedFieldName( filter.Selection );
                        if ( !string.IsNullOrWhiteSpace( fieldName ) )
                        {
                           // TODO filterList.Add( filter.Guid, fieldName );
                        }
                    }
                    else
                    {
                        // TODO filterList.Add( filter.Guid, component.GetTitle( reportEntityType.GetType() ) );
                    }
                }
            }
            else
            {
                var groupControl = new FilterGroup();
                parentControl.Controls.Add( groupControl );
                groupControl.DataViewFilterGuid = filter.Guid;
                groupControl.ID = string.Format( "fg_{0}", groupControl.DataViewFilterGuid.ToString( "N" ) );
                groupControl.FilteredEntityTypeName = filteredEntityTypeName;
                groupControl.IsDeleteEnabled = parentControl is FilterGroup;
                if ( setSelection )
                {
                    groupControl.FilterType = filter.ExpressionType;
                }

                foreach ( var childFilter in filter.ChildFilters )
                {
                    CreateFilterControl( groupControl, childFilter, filteredEntityTypeName, setSelection, rockContext );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnRun control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRun_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var reportService = new ReportService( rockContext );
            var reportGuid = this.GetAttributeValue( "Report" ).AsGuidOrNull();
            Report report = null;
            if ( reportGuid.HasValue )
            {
                report = reportService.Get( reportGuid.Value );
            }

            if ( report == null )
            {
                nbConfigurationWarning.Visible = true;
                nbConfigurationWarning.Text = "A report needs to be configured in block settings";
                pnlView.Visible = false;
            }
            else
            {
                nbConfigurationWarning.Visible = false;
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlReport_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindDataFiltersGrid();
        }

        /// <summary>
        /// Binds the data filters grid.
        /// </summary>
        protected void BindDataFiltersGrid()
        {
            var rockContext = new RockContext();
            var reportService = new ReportService( rockContext );

            var reportGuid = ddlReport.SelectedValueAsGuid();
            var dataFilterGuids = ( this.GetAttributeValue( "DataFilters" ) ?? string.Empty ).Split( '|' ).AsGuidList();
            Report report = null;
            if ( reportGuid.HasValue )
            {
                report = reportService.Get( reportGuid.Value );
            }

            if ( report == null )
            {
                nbConfigurationWarning.Visible = true;
                nbConfigurationWarning.Text = "A report needs to be configured in block settings";
                pnlView.Visible = false;
            }
            else
            {
                nbConfigurationWarning.Visible = false;
                if ( report.DataView != null && report.DataView.DataViewFilter != null )
                {
                    var selectedDataFieldGuids = ( this.GetAttributeValue( "SelectedDataFieldGuids" ) ?? string.Empty ).Split( '|' ).AsGuidList();
                    var configurableDataFieldGuids = ( this.GetAttributeValue( "ConfigurableDataFieldGuids" ) ?? string.Empty ).Split( '|' ).AsGuidList();

                    var filters = new Dictionary<Guid, string>();
                    GetFilterListRecursive( filters, report.DataView.DataViewFilter, report.EntityType );
                    grdDataFilters.DataSource = filters.Select( a => new
                    {
                        Guid = a.Key,
                        Name = a.Value,
                        ShowAsFilter = selectedDataFieldGuids.Contains( a.Key ),
                        IsConfigurable = configurableDataFieldGuids.Contains( a.Key )
                    } );

                    grdDataFilters.DataBind();
                }
            }


        }

        /// <summary>
        /// Gets the filter recursive.
        /// </summary>
        /// <param name="filterList">The filter list.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="reportEntityType">Type of the report entity.</param>
        private void GetFilterListRecursive( Dictionary<Guid, string> filterList, DataViewFilter filter, EntityType reportEntityType )
        {
            var result = new Dictionary<Guid, string>();

            var entityType = EntityTypeCache.Read( filter.EntityTypeId ?? 0 );
            if ( entityType != null )
            {
                var component = Rock.Reporting.DataFilterContainer.GetComponent( entityType.Name );
                if ( component != null )
                {
                    if ( component is Rock.Reporting.DataFilter.EntityFieldFilter )
                    {
                        var entityFieldFilter = ( component as Rock.Reporting.DataFilter.EntityFieldFilter );
                        var fieldName = entityFieldFilter.GetSelectedFieldName( filter.Selection );
                        if ( !string.IsNullOrWhiteSpace( fieldName ) )
                        {
                            filterList.Add( filter.Guid, fieldName );
                        }
                    }
                    else
                    {
                        filterList.Add( filter.Guid, component.GetTitle( reportEntityType.GetType() ) );
                    }
                }
            }

            foreach ( var childFilter in filter.ChildFilters )
            {
                GetFilterListRecursive( filterList, childFilter, reportEntityType );
            }

        }
    }
}
