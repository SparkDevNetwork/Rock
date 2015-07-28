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
    [BooleanField( "Show 'Merge Template' action on grid", "", defaultValue: true, key: "ShowGridMergeTemplateAction" )]
    [BooleanField( "Show 'Communications' action on grid", "", defaultValue: true, key: "ShowGridCommunicationsAction" )]

    // CustomSetting Dialog
    [TextField( "ResultsIconCssClass", "Title for the results list.", false, "fa fa-list", "CustomSetting" )]
    [TextField( "ResultsTitle", "Title for the results list.", false, "Results", "CustomSetting" )]
    [TextField( "FilterTitle", "Title for the results list.", false, "Filters", "CustomSetting" )]
    [TextField( "FilterIconCssClass", "Title for the results list.", false, "fa fa-filter", "CustomSetting" )]
    [TextField( "Report", "The report to use for this block", false, "", "CustomSetting" )]
    [TextField( "SelectedDataFieldGuids", "The DataFilters to present to the user", false, "", "CustomSetting" )]
    [TextField( "ConfigurableDataFieldGuids", "Of the DataFilters that are presented to the user, which are configurable vs just a checkbox", false, "", "CustomSetting" )]
    [TextField( "TogglableDataFieldGuids", "The configurable datafilters that include a checkbox that can disable/enable the filter", false, "", "CustomSetting" )]
    public partial class DynamicReport : RockBlockCustomSettings
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
            gReport.Actions.ShowMergeTemplate = this.GetAttributeValue( "ShowGridMergeTemplateAction" ).AsBooleanOrNull() ?? true;
            gReport.Actions.ShowCommunicate = this.GetAttributeValue( "ShowGridCommunicationsAction" ).AsBooleanOrNull() ?? true;

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
            // reload the full page since controls are dynamically created based on block settings
            NavigateToPage( this.CurrentPageReference );
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

            if ( !this.IsPostBack )
            {
                ShowReport();
            }
        }

        /// <summary>
        /// Shows the report.
        /// </summary>
        private void ShowReport()
        {
            lResultsTitle.Text = GetAttributeValue( "ResultsTitle" );

            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "ResultsIconCssClass" ) ) )
            {
                lResultsIconCssClass.Text = string.Format( "<i class='{0}'></i>", GetAttributeValue( "ResultsIconCssClass" ) );
            }

            lFilterTitle.Text = GetAttributeValue( "FilterTitle" );

            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "FilterIconCssClass" ) ) )
            {
                lFilterIconCssClass.Text = string.Format( "<i class='{0}'></i>", GetAttributeValue( "FilterIconCssClass" ) );
            }

            BindReportGrid();
        }

        /// <summary>
        /// Shows the settings.
        /// </summary>
        protected override void ShowSettings()
        {
            pnlConfigure.Visible = true;
            LoadDropDowns();
            ddlReport.SetValue( this.GetAttributeValue( "Report" ).AsGuidOrNull() );
            txtResultsTitle.Text = this.GetAttributeValue( "ResultsTitle" );
            txtResultsIconCssClass.Text = this.GetAttributeValue( "ResultsIconCssClass" );
            txtFilterTitle.Text = this.GetAttributeValue( "FilterTitle" );
            txtFilterIconCssClass.Text = this.GetAttributeValue( "FilterIconCssClass" );
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
            if ( grdDataFilters.Visible )
            {
                var selectCols = grdDataFilters.Columns.OfType<SelectField>().ToArray();
                var selectedDataFieldGuids = selectCols[0].SelectedKeys.OfType<Guid>();
                var configurableDataFieldGuids = selectCols[1].SelectedKeys.OfType<Guid>();
                var togglableDataFieldGuids = selectCols[2].SelectedKeys.OfType<Guid>();

                this.SetAttributeValue( "SelectedDataFieldGuids", selectedDataFieldGuids.Select( a => a.ToString() ).ToList().AsDelimited( "|" ) );
                this.SetAttributeValue( "ConfigurableDataFieldGuids", configurableDataFieldGuids.Select( a => a.ToString() ).ToList().AsDelimited( "|" ) );
                this.SetAttributeValue( "TogglableDataFieldGuids", togglableDataFieldGuids.Select( a => a.ToString() ).ToList().AsDelimited( "|" ) );
            }
            else
            {
                this.SetAttributeValue( "SelectedDataFieldGuids", null );
                this.SetAttributeValue( "ConfigurableDataFieldGuids", null );
                this.SetAttributeValue( "TogglableDataFieldGuids", null );
            }

            mdConfigure.Hide();
            pnlConfigure.Visible = false;

            this.SetAttributeValue( "ResultsTitle", txtResultsTitle.Text );
            this.SetAttributeValue( "ResultsIconCssClass", txtResultsIconCssClass.Text );
            this.SetAttributeValue( "FilterTitle", txtFilterTitle.Text );
            this.SetAttributeValue( "FilterIconCssClass", txtFilterIconCssClass.Text );
            this.SetAttributeValue( "Report", ddlReport.SelectedValue.AsGuidOrNull().ToString() );
            SaveAttributeValues();

            this.Block_BlockUpdated( sender, e );
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
            nbFiltersError.Visible = false;
            try
            {
                var rockContext = new RockContext();
                var reportService = new ReportService( rockContext );

                var reportGuid = this.GetAttributeValue( "Report" ).AsGuidOrNull();
                var selectedDataFieldGuids = ( this.GetAttributeValue( "SelectedDataFieldGuids" ) ?? string.Empty ).Split( '|' ).AsGuidList();
                var configurableDataFieldGuids = ( this.GetAttributeValue( "ConfigurableDataFieldGuids" ) ?? string.Empty ).Split( '|' ).AsGuidList();
                var togglableDataFieldGuids = ( this.GetAttributeValue( "TogglableDataFieldGuids" ) ?? string.Empty ).Split( '|' ).AsGuidList();
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
                                togglableDataFieldGuids,
                                rockContext );
                        }
                    }

                    // only show the filter and button if there visible filters
                    pnlFilter.Visible = phFilters.ControlsOfTypeRecursive<FilterField>().Any( a => a.Visible );
                }
            }
            catch ( Exception ex )
            {
                this.LogException( ex );
                nbFiltersError.Text = "An error occurred trying to load the filters. Click on 'Set Default' to try again with the default filter.";
                nbFiltersError.Details = "see the exception log for additional details";
                nbFiltersError.Visible = true;
            }
        }

        /// <summary>
        /// Gets the key prefix to use for User Preferences for ReportData filters
        /// </summary>
        /// <returns></returns>
        private string GetReportDataKeyPrefix()
        {
            string keyPrefix = string.Format( "reportdata-filter-{0}-", this.BlockId );
            return keyPrefix;
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
            List<Guid> togglableDataFieldGuids,
            RockContext rockContext )
        {
            var filteredEntityTypeName = EntityTypeCache.Read( reportEntityType ).Name;
            if ( filter.ExpressionType == FilterExpressionType.Filter )
            {
                var filterControl = new FilterField();

                bool filterIsVisible = selectedDataFieldGuids.Contains( filter.Guid );
                bool filterIsConfigurable = configurableDataFieldGuids.Contains( filter.Guid );
                bool showCheckbox = togglableDataFieldGuids.Contains( filter.Guid ) || !filterIsConfigurable;
                filterControl.Visible = filterIsVisible;
                parentControl.Controls.Add( filterControl );
                filterControl.DataViewFilterGuid = filter.Guid;

                filterControl.HideFilterCriteria = !filterIsConfigurable;
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
                filterControl.FilterMode = FilterMode.SimpleFilter;
                filterControl.ShowCheckbox = filterIsVisible && showCheckbox;

                var reportEntityTypeCache = EntityTypeCache.Read( reportEntityType );
                var reportEntityTypeModel = reportEntityTypeCache.GetEntityType();

                var filterEntityType = EntityTypeCache.Read( filter.EntityTypeId ?? 0 );
                var component = Rock.Reporting.DataFilterContainer.GetComponent( filterEntityType.Name );
                if ( component != null )
                {
                    string selectionUserPreference = null;
                    bool? checkedUserPreference = null;
                    if ( setSelection && filterIsVisible && filterIsConfigurable )
                    {
                        selectionUserPreference = this.GetUserPreference( string.Format( "{0}_{1}_Selection", GetReportDataKeyPrefix(), filterControl.DataViewFilterGuid.ToString( "N" ) ) );
                    }
                    else if ( setSelection && filterIsVisible && !filterIsConfigurable )
                    {
                        checkedUserPreference = this.GetUserPreference( string.Format( "{0}_{1}_Checked", GetReportDataKeyPrefix(), filterControl.DataViewFilterGuid.ToString( "N" ) ) ).AsBooleanOrNull();
                    }

                    if ( checkedUserPreference.HasValue )
                    {
                        filterControl.SetCheckBoxChecked( checkedUserPreference.Value );
                    }

                    if ( filterIsVisible && !filterIsConfigurable )
                    {
                        // not configurable so just label it with the selection summary
                        filterControl.Label = component.FormatSelection( reportEntityTypeModel, filter.Selection );
                    }
                    else if ( component is Rock.Reporting.DataFilter.PropertyFilter )
                    {
                        // a configurable property filter
                        var propertyFilter = component as Rock.Reporting.DataFilter.PropertyFilter;
                        if ( setSelection )
                        {
                            var selection = filter.Selection;

                            if ( !string.IsNullOrWhiteSpace( selectionUserPreference ) )
                            {
                                selection = propertyFilter.UpdateSelectionFromUserPreferenceSelection( selection, selectionUserPreference );
                            }

                            selection = propertyFilter.UpdateSelectionFromPageParameters( selection, this );

                            filterControl.SetSelection(selection);
                        }
                    }
                    else
                    {
                        if ( setSelection )
                        {
                            var selection = filter.Selection;
                            if ( !string.IsNullOrWhiteSpace( selectionUserPreference ) )
                            {
                                selection = selectionUserPreference;
                            }

                            if ( component is Rock.Reporting.DataFilter.IUpdateSelectionFromPageParameters )
                            {
                                selection = ( component as Rock.Reporting.DataFilter.IUpdateSelectionFromPageParameters ).UpdateSelectionFromPageParameters( selection, this );
                            }

                            filterControl.SetSelection( selection );
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
                    CreateFilterControl( groupControl, childFilter, reportEntityType, setSelection, selectedDataFieldGuids, configurableDataFieldGuids, togglableDataFieldGuids, rockContext );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnFilter_Click( object sender, EventArgs e )
        {
            var dataViewFilterService = new DataViewFilterService( new RockContext() );
            foreach ( var filterControl in phFilters.ControlsOfTypeRecursive<FilterField>() )
            {
                string selectionKey = string.Format( "{0}_{1}_Selection", GetReportDataKeyPrefix(), filterControl.DataViewFilterGuid.ToString( "N" ) );
                string checkedKey = string.Format( "{0}_{1}_Checked", GetReportDataKeyPrefix(), filterControl.DataViewFilterGuid.ToString( "N" ) );
                if ( filterControl.Visible )
                {
                    if ( !filterControl.HideFilterCriteria )
                    {
                        // only save the preference if it is different from the original
                        var origFilter = dataViewFilterService.Get( filterControl.DataViewFilterGuid );
                        var selection = filterControl.GetSelection();
                        if ( origFilter != null && origFilter.Selection != selection )
                        {
                            this.SetUserPreference( selectionKey, selection );
                        }
                        else
                        {
                            this.DeleteUserPreference( selectionKey );
                        }
                    }

                    if ( filterControl.ShowCheckbox )
                    {
                        this.SetUserPreference( checkedKey, filterControl.CheckBoxChecked.ToString() );
                    }
                }
                else
                {
                    this.DeleteUserPreference( selectionKey );
                    this.DeleteUserPreference( checkedKey );
                }
            }

            BindReportGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnFilterSetDefault control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnFilterSetDefault_Click( object sender, EventArgs e )
        {
            var keyPrefix = GetReportDataKeyPrefix();
            foreach ( var item in this.GetUserPreferences( keyPrefix ) )
            {
                this.DeleteUserPreference( item.Key );
            }

            ShowFilters( true );
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
            else if ( report.DataView == null )
            {
                nbConfigurationWarning.Visible = true;
                nbConfigurationWarning.Text = string.Format( "The {0} report does not have a dataview", report );
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
                    nbReportErrors.Visible = false;
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
        /// Binds the data filters grid in the Settings dialog
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

            nbConfigurationWarning.Visible = false;

            if ( report != null && report.DataView != null && report.DataView.DataViewFilter != null )
            {
                var selectedDataFieldGuids = ( this.GetAttributeValue( "SelectedDataFieldGuids" ) ?? string.Empty ).Split( '|' ).AsGuidList();
                var configurableDataFieldGuids = ( this.GetAttributeValue( "ConfigurableDataFieldGuids" ) ?? string.Empty ).Split( '|' ).AsGuidList();
                var togglableDataFieldGuids = ( this.GetAttributeValue( "TogglableDataFieldGuids" ) ?? string.Empty ).Split( '|' ).AsGuidList();

                var filters = new List<FilterInfo>();
                GetFilterListRecursive( filters, report.DataView.DataViewFilter, report.EntityType );

                // remove the top level group filter
                filters = filters.Where( a => a.ParentFilter != null ).ToList();

                // set the Title and Summary of Grouped Filters based on the GroupFilter's child filter titles
                foreach ( var groupedFilter in filters.Where( a => a.FilterExpressionType != FilterExpressionType.Filter ) )
                {
                    groupedFilter.Title = string.Format( "[{0}]", groupedFilter.FilterExpressionType.ConvertToString() );
                    groupedFilter.Summary = filters.Where( a => a.ParentFilter == groupedFilter ).Select( a => a.Summary ?? string.Empty ).ToList().AsDelimited( ", ", groupedFilter.FilterExpressionType == FilterExpressionType.GroupAny ? " or " : " and " );
                }

                // remove any filter that are part of a child group filter
                filters = filters.Where( a => a.ParentFilter != null && a.ParentFilter.ParentFilter == null ).ToList();

                grdDataFilters.Visible = true;
                mdConfigure.ServerSaveLink.Disabled = false;
                grdDataFilters.DataSource = filters.Select( a => new
                {
                    a.Guid,
                    a.Title,
                    a.TitlePath,
                    a.Summary,
                    a.FilterExpressionType,
                    a.ParentFilter,
                    ShowAsFilter = selectAll || selectedDataFieldGuids.Contains( a.Guid ),
                    IsConfigurable = selectAll || configurableDataFieldGuids.Contains( a.Guid ),
                    IsTogglable = selectAll || togglableDataFieldGuids.Contains( a.Guid )
                } );

                grdDataFilters.DataBind();
            }
            else
            {
                grdDataFilters.Visible = false;
            }
        }

        /// <summary>
        /// private class just for this block used to show the configuration of which fields are shown and configurable
        /// </summary>
        private class FilterInfo
        {
            /// <summary>
            /// Gets or sets the unique identifier.
            /// </summary>
            /// <value>
            /// The unique identifier.
            /// </value>
            public Guid Guid { get; set; }

            /// <summary>
            /// Gets or sets the title.
            /// </summary>
            /// <value>
            /// The title.
            /// </value>
            public string Title { get; set; }

            /// <summary>
            /// Gets the title including the parent filter titles
            /// </summary>
            /// <value>
            /// The title path.
            /// </value>
            public string TitlePath
            {
                get
                {
                    string parentPath = this.Title;
                    var parentFilter = this.ParentFilter;
                    while ( parentFilter != null )
                    {
                        if ( parentFilter.ParentFilter == null )
                        {
                            // don't include the root group filter
                            break;
                        }

                        parentPath = parentFilter.Title + " > " + parentPath;
                        parentFilter = parentFilter.ParentFilter;
                    }

                    return parentPath;
                }
            }

            /// <summary>
            /// Gets or sets the summary.
            /// </summary>
            /// <value>
            /// The summary.
            /// </value>
            public string Summary { get; set; }

            /// <summary>
            /// Gets or sets the type of the filter expression.
            /// </summary>
            /// <value>
            /// The type of the filter expression.
            /// </value>
            public FilterExpressionType FilterExpressionType { get; set; }

            /// <summary>
            /// Gets or sets the parent filter.
            /// </summary>
            /// <value>
            /// The parent filter.
            /// </value>
            public FilterInfo ParentFilter { get; set; }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return TitlePath;
            }
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

            var filterInfo = new FilterInfo();
            filterInfo.Guid = filter.Guid;
            filterInfo.FilterExpressionType = filter.ExpressionType;
            filterInfo.ParentFilter = filter.ParentId.HasValue ? filterList.FirstOrDefault( a => a.Guid == filter.Parent.Guid ) : null;
            if ( entityType != null )
            {
                var component = Rock.Reporting.DataFilterContainer.GetComponent( entityType.Name );
                if ( component != null )
                {
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
                        filterInfo.Title = component.GetTitle( reportEntityType.GetType() );
                    }

                    filterInfo.Summary = component.FormatSelection( reportEntityTypeModel, filter.Selection );

                }
            }

            filterList.Add( filterInfo );

            foreach ( var childFilter in filter.ChildFilters )
            {
                GetFilterListRecursive( filterList, childFilter, reportEntityType );
            }
        }

        /// <summary>
        /// Handles the RowDataBound event of the grdDataFilters control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void grdDataFilters_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var dataItem = e.Row.DataItem;
            if ( dataItem != null )
            {
                if ( (FilterExpressionType)dataItem.GetPropertyValue( "FilterExpressionType" ) != FilterExpressionType.Filter )
                {
                    foreach ( var selectField in grdDataFilters.Columns.OfType<SelectField>() )
                    {
                        var cb = e.Row.Cells[selectField.ColumnIndex].ControlsOfTypeRecursive<CheckBox>().FirstOrDefault();
                        if ( cb != null )
                        {
                            // hide the checkbox/selectfields if this is a GroupAny/GroupAll filter
                            cb.Visible = false;
                        }
                    }
                }
            }
        }
    }
}
