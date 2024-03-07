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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting
{
    [DisplayName( "Dynamic Report" )]
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

    // NOTE: attribute names should have been called *DataFilter* not *DataField*, but probably can't change to keep backward compat :(
    [TextField( "SelectedDataFieldGuids", "The DataFilters to present to the user", false, "", "CustomSetting" )]
    [TextField( "ConfigurableDataFieldGuids", "Of the DataFilters that are presented to the user, which are configurable vs just a checkbox", false, "", "CustomSetting" )]
    [TextField( "TogglableDataFieldGuids", "The configurable DataFilters that include a checkbox that can disable/enable the filter", false, "", "CustomSetting" )]

    [TextField( "PersonIdField", "If this isn't a Person report, but there is a person id field, specify the name of the field", false, "", "CustomSetting" )]

    [TextField( "DataFiltersPrePostHtmlConfig", "JSON for the Dictionary<Guid,DataFilterPrePostHtmlConfig>", false, "", "CustomSetting" )]
    [Rock.SystemGuid.BlockTypeGuid( "C7C069DB-9EEE-4245-9DF2-34E3A1FF4CCB" )]
    public partial class DynamicReport : RockBlockCustomSettings
    {
        private List<Guid> _selectedDataFilterGuids = null;
        private List<Guid> _configurableDataFilterGuids = null;
        private List<Guid> _togglableDataFilterGuids = null;
        private Dictionary<Guid, DataFilterPrePostHtmlConfig> _dataFiltersPrePostHtmlConfig = null;

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

            _selectedDataFilterGuids = ( this.GetAttributeValue( "SelectedDataFieldGuids" ) ?? string.Empty ).Split( '|' ).AsGuidList();
            _configurableDataFilterGuids = ( this.GetAttributeValue( "ConfigurableDataFieldGuids" ) ?? string.Empty ).Split( '|' ).AsGuidList();
            _togglableDataFilterGuids = ( this.GetAttributeValue( "TogglableDataFieldGuids" ) ?? string.Empty ).Split( '|' ).AsGuidList();
            _dataFiltersPrePostHtmlConfig = this.GetAttributeValue( "DataFiltersPrePostHtmlConfig" ).FromJsonOrNull<Dictionary<Guid, DataFilterPrePostHtmlConfig>>() ?? new Dictionary<Guid, DataFilterPrePostHtmlConfig>();

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
        protected void gReport_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindReportGrid( e.IsCommunication );
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

        #region View

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
        /// Creates the filter control.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="reportEntityType">Type of the report entity.</param>
        /// <param name="setSelection">if set to <c>true</c> [set selection].</param>
        /// <param name="rockContext">The rock context.</param>
        private void CreateFilterControl(
            Control parentControl,
            DataViewFilter filter,
            EntityType reportEntityType,
            bool setSelection,
            RockContext rockContext )
        {
            try
            {
                var preferences = GetBlockPersonPreferences();
                var filteredEntityTypeName = EntityTypeCache.Get( reportEntityType ).Name;
                if ( filter.ExpressionType == FilterExpressionType.Filter )
                {
                    var filterControl = new FilterField();
                    bool filterIsVisible = _selectedDataFilterGuids.Contains( filter.Guid );

                    if ( filterIsVisible )
                    {
                        // only set FilterMode to simple if the filter is visible since SimpleFilters might have a different filtering behavior
                        filterControl.FilterMode = FilterMode.SimpleFilter;
                    }
                    else
                    {
                        filterControl.FilterMode = FilterMode.AdvancedFilter;
                    }

                    bool filterIsConfigurable = _configurableDataFilterGuids.Contains( filter.Guid );
                    bool showCheckbox = _togglableDataFilterGuids.Contains( filter.Guid ) || !filterIsConfigurable;
                    var dataFilterPrePostHtmlConfig = _dataFiltersPrePostHtmlConfig.GetValueOrNull( filter.Guid ) ?? new DataFilterPrePostHtmlConfig();

                    filterControl.Visible = filterIsVisible;

                    parentControl.Controls.Add( filterControl );

                    filterControl.DataViewFilterGuid = filter.Guid;

                    filterControl.HideFilterCriteria = !filterIsConfigurable;
                    filterControl.ID = string.Format( "ff_{0}", filterControl.DataViewFilterGuid.ToString( "N" ) );
                    filterControl.FilteredEntityTypeName = filteredEntityTypeName;

                    if ( filter.EntityTypeId.HasValue )
                    {
                        var entityTypeCache = EntityTypeCache.Get( filter.EntityTypeId.Value, rockContext );
                        if ( entityTypeCache != null )
                        {
                            filterControl.FilterEntityTypeName = entityTypeCache.Name;
                        }
                    }

                    filterControl.Expanded = true;

                    filterControl.ShowCheckbox = filterIsVisible && showCheckbox;
                    filterControl.HideDescription = true;

                    var reportEntityTypeCache = EntityTypeCache.Get( reportEntityType );
                    var reportEntityTypeModel = reportEntityTypeCache.GetEntityType();

                    var filterEntityType = EntityTypeCache.Get( filter.EntityTypeId ?? 0 );
                    var component = Rock.Reporting.DataFilterContainer.GetComponent( filterEntityType.Name );
                    if ( component != null )
                    {
                        string selectionUserPreference = null;
                        bool? checkedUserPreference = null;
                        if ( setSelection && filterIsVisible )
                        {
                            if ( filterIsConfigurable )
                            {
                                selectionUserPreference = preferences.GetValue( $"{filterControl.DataViewFilterGuid:N}_Selection" );
                            }

                            if ( filterControl.ShowCheckbox )
                            {
                                checkedUserPreference = preferences.GetValue( $"{filterControl.DataViewFilterGuid:N}_Checked" ).AsBooleanOrNull() ?? true;
                            }
                        }

                        if ( checkedUserPreference.HasValue )
                        {
                            filterControl.SetCheckBoxChecked( checkedUserPreference.Value );
                        }

                        if ( setSelection )
                        {
                            var selection = filter.Selection;
                            if ( !string.IsNullOrWhiteSpace( selectionUserPreference ) )
                            {
                                if ( component is Rock.Reporting.DataFilter.PropertyFilter )
                                {
                                    selection = ( component as Rock.Reporting.DataFilter.PropertyFilter ).UpdateSelectionFromUserPreferenceSelection( selection, selectionUserPreference );
                                }
                                else
                                {
                                    selection = selectionUserPreference;
                                }
                            }

                            if ( component is Rock.Reporting.DataFilter.IUpdateSelectionFromPageParameters )
                            {
                                selection = ( component as Rock.Reporting.DataFilter.IUpdateSelectionFromPageParameters ).UpdateSelectionFromPageParameters( selection, this );
                            }

                            try
                            {
                                filterControl.SetSelection( selection );

                                // if the selection is the same as what is stored in the database for that DataViewFilter,
                                // Do a GetSelection to get the selection in the current format for that filter
                                // This will prevent this dynamic report from thinking the selection has changed from the orig filter
                                if ( selection == filter.Selection )
                                {
                                    var normalizedSelection = filterControl.GetSelection();
                                    if ( normalizedSelection != filter.Selection )
                                    {
                                        // if the format of the filter.Selection has changed, update the dataViewFilter's Selection to match the current format
                                        filter.Selection = normalizedSelection;
                                        using ( var updateSelectionContext = new RockContext() )
                                        {
                                            var dataViewFilter = new DataViewFilterService( updateSelectionContext ).Get( filter.Id );
                                            dataViewFilter.Selection = normalizedSelection;
                                            updateSelectionContext.SaveChanges();
                                        }
                                    }
                                }
                            }
                            catch ( Exception ex )
                            {
                                this.LogException( new Exception( "Exception setting selection for DataViewFilter: " + filter.Guid, ex ) );
                            }
                        }

                        string defaultFilterLabel;
                        if ( !filterIsConfigurable )
                        {
                            // not configurable so just label it with the selection summary
                            defaultFilterLabel = component.FormatSelection( reportEntityTypeModel, filter.Selection );

                            // configuration not visible, so set the selection to what it was in the dataview when it was saved, even if setSelection=False
                            filterControl.SetSelection( filter.Selection );
                        }
                        else if ( component is Rock.Reporting.DataFilter.PropertyFilter )
                        {
                            defaultFilterLabel = ( component as Rock.Reporting.DataFilter.PropertyFilter ).GetSelectionLabel( reportEntityTypeModel, filter.Selection );
                        }
                        else if ( component is Rock.Reporting.DataFilter.EntityFieldFilter )
                        {
                            defaultFilterLabel = ( component as Rock.Reporting.DataFilter.EntityFieldFilter ).GetSelectedFieldName( filter.Selection );
                        }
                        else
                        {
                            defaultFilterLabel = component.GetTitle( reportEntityTypeModel );
                        }

                        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson, new Rock.Lava.CommonMergeFieldsOptions() );
                        mergeFields.Add( "Filter", filter );
                        mergeFields.Add( "Label", defaultFilterLabel );

                        filterControl.Label = dataFilterPrePostHtmlConfig.LabelHtml.ResolveMergeFields( mergeFields );

                        if ( !string.IsNullOrEmpty( dataFilterPrePostHtmlConfig.PreHtml ) )
                        {
                            filterControl.PreHtml = dataFilterPrePostHtmlConfig.PreHtml.ResolveMergeFields( mergeFields );
                        }

                        if ( !string.IsNullOrEmpty( dataFilterPrePostHtmlConfig.PostHtml ) )
                        {
                            filterControl.PostHtml = dataFilterPrePostHtmlConfig.PostHtml.ResolveMergeFields( mergeFields );
                        }

                        if ( component is Rock.Reporting.DataFilter.OtherDataViewFilter )
                        {
                            // don't include the actual DataView Picker filter, just the child filters
                            parentControl.Controls.Remove( filterControl );

                            Rock.Reporting.DataFilter.OtherDataViewFilter otherDataViewFilter = component as Rock.Reporting.DataFilter.OtherDataViewFilter;
                            var otherDataView = otherDataViewFilter.GetSelectedDataView( filter.Selection );
                            if ( otherDataView != null )
                            {
                                CreateFilterControl( parentControl, otherDataView.DataViewFilter, reportEntityType, setSelection, rockContext );
                            }
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
                        CreateFilterControl( groupControl, childFilter, reportEntityType, setSelection, rockContext );
                    }
                }
            }
            catch ( Exception ex )
            {
                this.LogException( new Exception( "Exception creating FilterControl for DataViewFilter: " + filter.Guid, ex ) );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnFilter_Click( object sender, EventArgs e )
        {
            // Don't use WithPrefix() here so that we get better performance
            // when saving.
            var preferences = GetBlockPersonPreferences();

            var dataViewFilterService = new DataViewFilterService( new RockContext() );
            foreach ( var filterControl in phFilters.ControlsOfTypeRecursive<FilterField>() )
            {
                string selectionKey = $"{filterControl.DataViewFilterGuid:N}_Selection";
                string checkedKey = $"{filterControl.DataViewFilterGuid:N}_Checked";
                if ( filterControl.Visible )
                {
                    if ( !filterControl.HideFilterCriteria )
                    {
                        // only save the preference if it is different from the original
                        var origFilter = dataViewFilterService.Get( filterControl.DataViewFilterGuid );
                        var selection = filterControl.GetSelection();
                        if ( origFilter != null && origFilter.Selection != selection )
                        {
                            preferences.SetValue( selectionKey, selection );
                        }
                        else
                        {
                            preferences.SetValue( selectionKey, string.Empty );
                        }
                    }

                    if ( filterControl.ShowCheckbox )
                    {
                        preferences.SetValue( checkedKey, filterControl.CheckBoxChecked.ToString() );
                    }
                }
                else
                {
                    preferences.SetValue( selectionKey, string.Empty );
                    preferences.SetValue( checkedKey, string.Empty );
                }
            }

            preferences.Save();

            BindReportGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnFilterSetDefault control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnFilterSetDefault_Click( object sender, EventArgs e )
        {
            var preferences = GetBlockPersonPreferences();

            foreach ( var key in preferences.GetKeys() )
            {
                preferences.SetValue( key, string.Empty );
            }

            preferences.Save();

            ShowFilters( true );
        }

        /// <summary>
        /// Binds the report grid.
        /// </summary>
        private void BindReportGrid( bool isCommunication = false )
        {
            var rockContext = new RockContext();
            var reportService = new ReportService( rockContext );
            var reportGuid = this.GetAttributeValue( "Report" ).AsGuidOrNull();
            var personIdField = this.GetAttributeValue( "PersonIdField" );
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
            else if ( report.DataView.EntityTypeId != report.EntityTypeId )
            {
                nbConfigurationWarning.Visible = true;
                nbConfigurationWarning.Text = string.Format( "The {0} report's EntityType doesn't match the dataview's EntityType", report );
                pnlView.Visible = false;
            }
            else
            {
                nbConfigurationWarning.Visible = false;


                DataViewFilterOverrides dataViewFilterOverrides = ReportingHelper.GetFilterOverridesFromControls( report.DataView, phFilters );

                ReportingHelper.BindGridOptions bindGridOptions = new ReportingHelper.BindGridOptions
                {
                    CurrentPerson = this.CurrentPerson,
                    DataViewFilterOverrides = dataViewFilterOverrides,
                    DatabaseTimeoutSeconds = null,
                    IsCommunication = isCommunication
                };


                nbReportErrors.Visible = false;

                try
                {
                    bindGridOptions.ReportDbContext = Reflection.GetDbContextForEntityType( EntityTypeCache.Get( report.EntityTypeId.Value ).GetEntityType() );
                    ReportingHelper.BindGrid( report, gReport, bindGridOptions );

                    if ( report.EntityTypeId != EntityTypeCache.GetId<Rock.Model.Person>() )
                    {
                        var personColumn = gReport.ColumnsOfType<BoundField>().Where( a => a.HeaderText == personIdField ).FirstOrDefault();
                        if ( personColumn != null )
                        {
                            gReport.PersonIdField = personColumn.SortExpression;
                        }
                    }
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                    var sqlTimeoutException = ReportingHelper.FindSqlTimeoutException( ex );

                    if ( sqlTimeoutException != null )
                    {
                        nbReportErrors.NotificationBoxType = NotificationBoxType.Warning;
                        nbReportErrors.Text = "This report did not complete in a timely manner.";
                    }
                    else
                    {
                        if ( ex is RockDataViewFilterExpressionException )
                        {
                            RockDataViewFilterExpressionException rockDataViewFilterExpressionException = ex as RockDataViewFilterExpressionException;
                            nbReportErrors.Text = rockDataViewFilterExpressionException.GetFriendlyMessage( ( IDataViewDefinition ) report.DataView );
                        }
                        else
                        {
                            nbReportErrors.Text = "There was a problem with one of the filters for this report's dataview.";
                        }

                        nbReportErrors.NotificationBoxType = NotificationBoxType.Danger;

                        nbReportErrors.Details = ex.Message;
                        nbReportErrors.Visible = true;
                    }
                }
            }
        }

        #endregion View

        #region Configuration

        /// <summary>
        /// Shows the settings.
        /// </summary>
        protected override void ShowSettings()
        {
            pnlConfigure.Visible = true;

            Guid? reportGuid = this.GetAttributeValue( "Report" ).AsGuidOrNull();
            int? reportId = reportGuid != null ? new ReportService( new RockContext() ).GetId( reportGuid.Value ) : null;

            rpReport.SetValue( reportId );
            txtResultsTitle.Text = this.GetAttributeValue( "ResultsTitle" );
            txtResultsIconCssClass.Text = this.GetAttributeValue( "ResultsIconCssClass" );
            txtFilterTitle.Text = this.GetAttributeValue( "FilterTitle" );
            txtFilterIconCssClass.Text = this.GetAttributeValue( "FilterIconCssClass" );
            BindDataFiltersGrid();
            ddlPersonIdField.SetValue( this.GetAttributeValue( "PersonIdField" ) );
            mdConfigure.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdConfigure control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdConfigure_SaveClick( object sender, EventArgs e )
        {
            if ( rptDataFilters.Visible )
            {
                _selectedDataFilterGuids = new List<Guid>();
                _configurableDataFilterGuids = new List<Guid>();
                _togglableDataFilterGuids = new List<Guid>();
                _dataFiltersPrePostHtmlConfig = new Dictionary<Guid, DataFilterPrePostHtmlConfig>();
                foreach ( var item in rptDataFilters.Items.OfType<RepeaterItem>() )
                {
                    HiddenField hfDataFilterGuid = item.FindControl( "hfDataFilterGuid" ) as HiddenField;
                    RockCheckBox cbIsVisible = item.FindControl( "cbIsVisible" ) as RockCheckBox;
                    RockCheckBox cbIsConfigurable = item.FindControl( "cbIsConfigurable" ) as RockCheckBox;
                    RockCheckBox cbIsTogglable = item.FindControl( "cbIsTogglable" ) as RockCheckBox;
                    RockTextBox tbLabelHtml = item.FindControl( "tbLabelHtml" ) as RockTextBox;
                    RockTextBox tbPreHtml = item.FindControl( "tbPreHtml" ) as RockTextBox;
                    RockTextBox tbPostHtml = item.FindControl( "tbPostHtml" ) as RockTextBox;

                    Guid dataFilterGuid = hfDataFilterGuid.Value.AsGuid();
                    if ( cbIsVisible.Checked )
                    {
                        _selectedDataFilterGuids.Add( dataFilterGuid );
                    }

                    if ( cbIsConfigurable.Checked )
                    {
                        _configurableDataFilterGuids.Add( dataFilterGuid );
                    }

                    if ( cbIsTogglable.Checked )
                    {
                        _togglableDataFilterGuids.Add( dataFilterGuid );
                    }

                    _dataFiltersPrePostHtmlConfig.Add(
                        dataFilterGuid,
                        new DataFilterPrePostHtmlConfig
                        {
                            LabelHtml = tbLabelHtml.Text,
                            PreHtml = tbPreHtml.Text,
                            PostHtml = tbPostHtml.Text
                        } );
                }

                this.SetAttributeValue( "SelectedDataFieldGuids", _selectedDataFilterGuids.Select( a => a.ToString() ).ToList().AsDelimited( "|" ) );
                this.SetAttributeValue( "ConfigurableDataFieldGuids", _configurableDataFilterGuids.Select( a => a.ToString() ).ToList().AsDelimited( "|" ) );
                this.SetAttributeValue( "TogglableDataFieldGuids", _togglableDataFilterGuids.Select( a => a.ToString() ).ToList().AsDelimited( "|" ) );
                this.SetAttributeValue( "DataFiltersPrePostHtmlConfig", _dataFiltersPrePostHtmlConfig.ToJson() );
            }

            mdConfigure.Hide();
            pnlConfigure.Visible = false;

            this.SetAttributeValue( "ResultsTitle", txtResultsTitle.Text );
            this.SetAttributeValue( "ResultsIconCssClass", txtResultsIconCssClass.Text );
            this.SetAttributeValue( "FilterTitle", txtFilterTitle.Text );
            this.SetAttributeValue( "FilterIconCssClass", txtFilterIconCssClass.Text );

            Guid? reportGuid = rpReport.SelectedValueAsId().HasValue ? new ReportService( new RockContext() ).GetGuid( rpReport.SelectedValueAsId().Value ) : null;
            this.SetAttributeValue( "Report", reportGuid.ToString() );
            this.SetAttributeValue( "PersonIdField", ddlPersonIdField.SelectedValue );
            SaveAttributeValues();

            this.Block_BlockUpdated( sender, e );
        }

        /// <summary>
        /// Handles the SelectItem event of the rpReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rpReport_SelectItem( object sender, EventArgs e )
        {
            // reset the configs since there is a new report selected
            _configurableDataFilterGuids = null;
            _selectedDataFilterGuids = null;
            _togglableDataFilterGuids = null;
            _dataFiltersPrePostHtmlConfig = new Dictionary<Guid, DataFilterPrePostHtmlConfig>();
            BindDataFiltersGrid();
        }

        /// <summary>
        /// Binds the data filters grid in the Settings dialog
        /// </summary>
        protected void BindDataFiltersGrid()
        {
            var rockContext = new RockContext();
            var reportService = new ReportService( rockContext );

            var reportId = rpReport.SelectedValueAsId();
            Report report = null;
            if ( reportId.HasValue )
            {
                report = reportService.Get( reportId.Value );
            }

            nbConfigurationWarning.Visible = false;

            if ( report != null && report.DataView != null && report.DataView.DataViewFilter != null )
            {
                var filters = ReportingHelper.GetFilterInfoList( report.DataView );

                // remove the top level group filter if it is just a GROUPALL
                filters = filters.Where( a => a.ParentFilter != null || a.FilterExpressionType != FilterExpressionType.GroupAll ).ToList();

                // set the Title and Summary of Grouped Filters based on the GroupFilter's child filter titles
                foreach ( var groupedFilter in filters.Where( a => a.FilterExpressionType != FilterExpressionType.Filter ) )
                {
                    groupedFilter.Title = string.Format( "[{0}]", groupedFilter.FilterExpressionType.ConvertToString() );
                }

                ddlPersonIdField.Visible = report.EntityTypeId != EntityTypeCache.GetId<Rock.Model.Person>();
                ddlPersonIdField.Items.Clear();
                ddlPersonIdField.Items.Add( new ListItem() );
                ddlPersonIdField.Items.Add( new ListItem( "Id", "Id" ) );
                foreach ( var reportField in report.ReportFields )
                {
                    ddlPersonIdField.Items.Add( new ListItem( reportField.ColumnHeaderText, reportField.ColumnHeaderText ) );
                }

                rptDataFilters.Visible = true;
                mdConfigure.ServerSaveLink.Disabled = false;
                rptDataFilters.DataSource = filters;

                rptDataFilters.DataBind();
            }
            else
            {
                rptDataFilters.Visible = false;
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptDataFilters control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptDataFilters_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var filterInfo = e.Item.DataItem as ReportingHelper.FilterInfo;
            if ( filterInfo != null )
            {
                var showInputs = true;
                if ( filterInfo.FilterExpressionType != FilterExpressionType.Filter )
                {
                    // Don't show Checkboxes or PrePostHtml for any of the Grouped Filter types
                    showInputs = false;
                }
                else
                {
                    var otherDataViewFilter = filterInfo.Component as Rock.Reporting.DataFilter.OtherDataViewFilter;
                    if ( otherDataViewFilter != null && otherDataViewFilter.GetSelectedDataView( filterInfo.Selection ) != null )
                    {
                        // Don't show the Row (or Checkboxes) for any of the OtherDataView filters
                        e.Item.Visible = false;
                        showInputs = false;
                    }
                }

                HiddenField hfDataFilterGuid = e.Item.FindControl( "hfDataFilterGuid" ) as HiddenField;
                RockCheckBox cbIsVisible = e.Item.FindControl( "cbIsVisible" ) as RockCheckBox;
                RockCheckBox cbIsConfigurable = e.Item.FindControl( "cbIsConfigurable" ) as RockCheckBox;
                RockCheckBox cbIsTogglable = e.Item.FindControl( "cbIsTogglable" ) as RockCheckBox;
                RockTextBox tbLabelHtml = e.Item.FindControl( "tbLabelHtml" ) as RockTextBox;
                RockTextBox tbPreHtml = e.Item.FindControl( "tbPreHtml" ) as RockTextBox;
                RockTextBox tbPostHtml = e.Item.FindControl( "tbPostHtml" ) as RockTextBox;
                Literal lFilterDetails = e.Item.FindControl( "lFilterDetails" ) as Literal;

                cbIsVisible.Visible = showInputs;
                cbIsConfigurable.Visible = showInputs;
                cbIsTogglable.Visible = showInputs;
                tbLabelHtml.Visible = showInputs;
                tbPreHtml.Visible = showInputs;
                tbPostHtml.Visible = showInputs;

                hfDataFilterGuid.Value = filterInfo.Guid.ToString();
                if ( _selectedDataFilterGuids != null )
                {
                    cbIsVisible.Checked = _selectedDataFilterGuids.Contains( filterInfo.Guid );
                }
                else
                {
                    cbIsVisible.Checked = true;
                }

                if ( _configurableDataFilterGuids != null )
                {
                    cbIsConfigurable.Checked = _configurableDataFilterGuids.Contains( filterInfo.Guid );
                }
                else
                {
                    cbIsConfigurable.Checked = true;
                }

                if ( _togglableDataFilterGuids != null )
                {
                    cbIsTogglable.Checked = _togglableDataFilterGuids.Contains( filterInfo.Guid );
                }
                else
                {
                    cbIsTogglable.Checked = true;
                }

                var dataFilterPrePostHtmlConfig = _dataFiltersPrePostHtmlConfig.GetValueOrNull( filterInfo.Guid );
                if ( dataFilterPrePostHtmlConfig != null )
                {
                    tbLabelHtml.Text = dataFilterPrePostHtmlConfig.LabelHtml;
                    tbPreHtml.Text = dataFilterPrePostHtmlConfig.PreHtml;
                    tbPostHtml.Text = dataFilterPrePostHtmlConfig.PostHtml;
                }
                else
                {
                    tbLabelHtml.Text = "{{ Label }}";
                }

                lFilterDetails.Text = new DescriptionList()
                    .Add( "Filter", filterInfo.TitlePath )
                    .Add( "Summary", filterInfo.Summary )
                    .Add( "Parent Data View", filterInfo.FromDataViewName )
                    .Html;
            }
        }

        #endregion Configuration

        #region Configuration Classes

        /// <summary>
        ///
        /// </summary>
        public class DataFilterPrePostHtmlConfig
        {
            /// <summary>
            /// Gets or sets the label HTML (or the Checkbox text)
            /// </summary>
            /// <value>
            /// The label HTML.
            /// </value>
            public string LabelHtml { get; set; }

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

        #endregion Configuration Classes


    }
}