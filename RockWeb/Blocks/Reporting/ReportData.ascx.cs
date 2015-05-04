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
using Rock.Reporting;
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

            gReport.GridRebind += gReport_GridRebind;

            var setFilterValues = !this.IsPostBack;
            ShowFilters( setFilterValues );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowFilters( true );
        }

        /// <summary>
        /// Handles the GridRebind event of the gReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gReport_GridRebind( object sender, EventArgs e )
        {
            BindReportGrid();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
        }

        /// <summary>
        /// Shows the settings.
        /// </summary>
        protected override void ShowSettings()
        {
            pnlConfigure.Visible = true;
            LoadDropDowns();
            ddlReport.SetValue( this.GetAttributeValue( "Report" ).AsGuidOrNull() );
            BindDataFiltersGrid( false );
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

            ShowFilters( true );
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
        /// Creates and shows the Filter controls
        /// </summary>
        /// <param name="setSelection">if set to <c>true</c> [set selection].</param>
        protected void ShowFilters( bool setSelection )
        {
            var rockContext = new RockContext();
            var reportService = new ReportService( rockContext );

            var reportGuid = this.GetAttributeValue( "Report" ).AsGuidOrNull();
            var selectedDataFieldGuids = ( this.GetAttributeValue( "SelectedDataFieldGuids" ) ?? string.Empty ).Split( '|' ).AsGuidList();
            var configurableDataFieldGuids = ( this.GetAttributeValue( "ConfigurableDataFieldGuids" ) ?? string.Empty ).Split( '|' ).AsGuidList();
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
                    phFilters.Controls.Clear();
                    if ( report.DataView.DataViewFilter != null && report.EntityTypeId.HasValue )
                    {

                        CreateFilterControl(
                            phFilters,
                            report.DataView.DataViewFilter,
                            report.EntityType,
                            setSelection,
                            selectedDataFieldGuids,
                            configurableDataFieldGuids,
                            rockContext );
                    }
                }
            }
        }

        /// <summary>
        /// Creates the filter control.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="reportEntityType">Type of the report entity.</param>
        /// <param name="filteredEntityTypeName">Name of the filtered entity type.</param>
        /// <param name="setSelection">if set to <c>true</c> [set selection].</param>
        /// <param name="selectedDataFieldGuids">The selected data field guids.</param>
        /// <param name="configurableDataFieldGuids">The configurable data field guids.</param>
        /// <param name="rockContext">The rock context.</param>
        private void CreateFilterControl(
            Control parentControl,
            DataViewFilter filter,
            EntityType reportEntityType,
            bool setSelection,
            List<Guid> selectedDataFieldGuids,
            List<Guid> configurableDataFieldGuids,
            RockContext rockContext )
        {
            var filteredEntityTypeName = EntityTypeCache.Read( reportEntityType ).Name;
            if ( filter.ExpressionType == FilterExpressionType.Filter )
            {

                var filterControl = new FilterField();
                filterControl.Visible = selectedDataFieldGuids.Contains( filter.Guid );
                parentControl.Controls.Add( filterControl );
                filterControl.DataViewFilterGuid = filter.Guid;
                bool configurable = configurableDataFieldGuids.Contains( filter.Guid );
                filterControl.HideFilterCriteria = !configurable;
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
                filterControl.HideFilterTypePicker = true;
                filterControl.ShowCheckbox = !configurable;
                if ( setSelection )
                {
                    filterControl.Selection = filter.Selection;
                }

                var reportEntityTypeCache = EntityTypeCache.Read( reportEntityType );
                var reportEntityTypeModel = reportEntityTypeCache.GetEntityType();

                var filterEntityType = EntityTypeCache.Read( filter.EntityTypeId ?? 0 );
                var component = Rock.Reporting.DataFilterContainer.GetComponent( filterEntityType.Name );
                if ( component != null )
                {
                    if ( !configurable )
                    {
                        // not configurable so just label it with the selection summary
                        filterControl.Label = component.FormatSelection( reportEntityTypeModel, filter.Selection );
                    }
                    else if ( component is Rock.Reporting.DataFilter.PropertyFilter )
                    {
                        // a configurable property filter
                        var propertyFilter = component as Rock.Reporting.DataFilter.PropertyFilter;
                        propertyFilter.HideEntityFieldPicker();
                        if ( setSelection )
                        {
                            filterControl.Selection = propertyFilter.UpdateSelectionFromPageParameters( filter.Selection, this );
                        }

                        var fieldName = propertyFilter.GetSelectedFieldName( filter.Selection );

                        if ( !string.IsNullOrWhiteSpace( fieldName ) )
                        {
                            var entityFields = EntityHelper.GetEntityFields( reportEntityTypeModel );
                            var entityField = entityFields.Where( a => a.Name == fieldName ).FirstOrDefault();
                            if ( entityField != null )
                            {
                                filterControl.Label = entityField.Title;
                            }
                        }
                    }
                    else
                    {
                        if ( component is Rock.Reporting.DataFilter.IUpdateSelectionFromPageParameters )
                        {
                            filterControl.Selection = ( component as Rock.Reporting.DataFilter.IUpdateSelectionFromPageParameters ).UpdateSelectionFromPageParameters( filter.Selection, this );
                        }
                        
                        // a configurable data filter
                        filterControl.Label = component.GetTitle( reportEntityTypeModel );
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
                groupControl.IsDeleteEnabled = false;
                groupControl.HidePanelHeader = true;
                if ( setSelection )
                {
                    groupControl.FilterType = filter.ExpressionType;
                }

                foreach ( var childFilter in filter.ChildFilters )
                {
                    CreateFilterControl( groupControl, childFilter, reportEntityType, setSelection, selectedDataFieldGuids, configurableDataFieldGuids, rockContext );
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
            BindReportGrid();
        }

        /// <summary>
        /// Binds the report grid.
        /// </summary>
        private void BindReportGrid()
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

                report.DataView.DataViewFilter = ReportingHelper.GetFilterFromControls( phFilters );

                string errorMessage;
                ReportingHelper.BindGrid( report, gReport, this.CurrentPerson, null, out errorMessage );

                if ( !string.IsNullOrWhiteSpace( errorMessage ) )
                {
                    nbReportErrors.NotificationBoxType = NotificationBoxType.Warning;
                    nbReportErrors.Text = errorMessage;
                    nbReportErrors.Visible = true;
                }
                else
                {
                    nbReportErrors.Visible = true;
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlReport_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindDataFiltersGrid( true );
        }

        /// <summary>
        /// Binds the data filters grid.
        /// </summary>
        protected void BindDataFiltersGrid( bool selectAll )
        {
            var rockContext = new RockContext();
            var reportService = new ReportService( rockContext );

            var reportGuid = ddlReport.SelectedValueAsGuid();
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

                    var filters = new List<FilterInfo>();
                    GetFilterListRecursive( filters, report.DataView.DataViewFilter, report.EntityType );
                    grdDataFilters.DataSource = filters.Select( a => new
                    {
                        a.Guid,
                        a.Title,
                        a.Summary,
                        ShowAsFilter = selectAll || selectedDataFieldGuids.Contains( a.Guid ),
                        IsConfigurable = selectAll || configurableDataFieldGuids.Contains( a.Guid )
                    } );

                    grdDataFilters.DataBind();
                }
            }
        }

        private class FilterInfo
        {
            public Guid Guid { get; set; }
            public string Title { get; set; }
            public string Summary { get; set; }
        }

        /// <summary>
        /// Gets the filter recursive.
        /// </summary>
        /// <param name="filterList">The filter list.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="reportEntityType">Type of the report entity.</param>
        private void GetFilterListRecursive( List<FilterInfo> filterList, DataViewFilter filter, EntityType reportEntityType )
        {
            var result = new Dictionary<Guid, string>();

            var entityType = EntityTypeCache.Read( filter.EntityTypeId ?? 0 );
            var reportEntityTypeCache = EntityTypeCache.Read( reportEntityType );
            var reportEntityTypeModel = reportEntityTypeCache.GetEntityType();

            if ( entityType != null )
            {
                var component = Rock.Reporting.DataFilterContainer.GetComponent( entityType.Name );
                if ( component != null )
                {
                    var filterInfo = new FilterInfo { Guid = filter.Guid };
                    if ( component is Rock.Reporting.DataFilter.EntityFieldFilter )
                    {
                        var entityFieldFilter = component as Rock.Reporting.DataFilter.EntityFieldFilter;
                        var fieldName = entityFieldFilter.GetSelectedFieldName( filter.Selection );
                        if ( !string.IsNullOrWhiteSpace( fieldName ) )
                        {
                            var entityFields = EntityHelper.GetEntityFields( reportEntityTypeModel );
                            var entityField = entityFields.Where( a => a.Name == fieldName ).FirstOrDefault();
                            if ( entityField != null )
                            {
                                filterInfo.Title = entityField.Title;
                            }
                            else
                            {
                                filterInfo.Title = fieldName;
                            }
                        }
                    }
                    else
                    {
                        filterInfo.Title = component.GetTitle( reportEntityType.GetType() ); ;
                    }

                    filterInfo.Summary = component.FormatSelection( reportEntityTypeModel, filter.Selection );
                    filterList.Add( filterInfo );
                }
            }

            foreach ( var childFilter in filter.ChildFilters )
            {
                GetFilterListRecursive( filterList, childFilter, reportEntityType );
            }
        }
    }
}
