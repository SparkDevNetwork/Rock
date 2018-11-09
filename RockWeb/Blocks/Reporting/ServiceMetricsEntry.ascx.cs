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
using System.Data.Entity;

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// Block for easily adding/editing metric values for any metric that has partitions of campus and service time.
    /// </summary>
    [DisplayName( "Service Metrics Entry" )]
    [Category( "Reporting" )]
    [Description( "Block for easily adding/editing metric values for any metric that has partitions of campus and service time." )]

    [CategoryField( "Schedule Category", "The schedule category to use for list of service times.", false, "Rock.Model.Schedule", "", "", true, "", "", 0 )]
    [IntegerField( "Weeks Back", "The number of weeks back to display in the 'Week of' selection.", false, 8, "", 1 )]
    [IntegerField( "Weeks Ahead", "The number of weeks ahead to display in the 'Week of' selection.", false, 0, "", 2 )]
    [MetricCategoriesField( "Metric Categories", "Select the metric categories to display (note: only metrics in those categories with a campus and schedule partition will displayed).", true, "", "", 3 )]
    public partial class ServiceMetricsEntry : Rock.Web.UI.RockBlock
    {
        #region Fields

        private int? _selectedCampusId { get; set; }
        private DateTime? _selectedWeekend { get; set; }
        private int? _selectedServiceId { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            _selectedCampusId = ViewState["SelectedCampusId"] as int?;
            _selectedWeekend = ViewState["SelectedWeekend"] as DateTime?;
            _selectedServiceId = ViewState["SelectedServiceId"] as int?;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbMetricsSaved.Visible = false;

            if ( !Page.IsPostBack )
            {
                _selectedCampusId = GetBlockUserPreference( "CampusId" ).AsIntegerOrNull();
                _selectedServiceId = GetBlockUserPreference( "ScheduleId" ).AsIntegerOrNull();

                if ( CheckSelection() )
                {
                    LoadDropDowns();
                    BindMetrics();
                }
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["SelectedCampusId"] = _selectedCampusId;
            ViewState["SelectedWeekend"] = _selectedWeekend;
            ViewState["SelectedServiceId"] = _selectedServiceId;
            return base.SaveViewState();
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
            BindMetrics();
        }


        protected void rptrSelection_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            switch( e.CommandName )
            {
                case "Campus":
                    _selectedCampusId = e.CommandArgument.ToString().AsIntegerOrNull();
                    break;
                case "Weekend":
                    _selectedWeekend = e.CommandArgument.ToString().AsDateTime();
                    break;
                case "Service":
                    _selectedServiceId = e.CommandArgument.ToString().AsIntegerOrNull();
                    break;
            }

            if ( CheckSelection() )
            {
                LoadDropDowns();
                BindMetrics();
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptrMetric control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptrMetric_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item )
            {
                var nbMetricValue = e.Item.FindControl( "nbMetricValue" ) as NumberBox;
                if ( nbMetricValue != null )
                {
                    nbMetricValue.ValidationGroup = BlockValidationGroup;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            int campusEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Campus ) ).Id;
            int scheduleEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Schedule ) ).Id;

            int? campusId = bddlCampus.SelectedValueAsInt();
            int? scheduleId = bddlService.SelectedValueAsInt();
            DateTime? weekend = bddlWeekend.SelectedValue.AsDateTime();

            if ( campusId.HasValue && scheduleId.HasValue && weekend.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var metricService = new MetricService( rockContext );
                    var metricValueService = new MetricValueService( rockContext );

                    foreach ( RepeaterItem item in rptrMetric.Items )
                    {
                        var hfMetricIId = item.FindControl( "hfMetricId" ) as HiddenField;
                        var nbMetricValue = item.FindControl( "nbMetricValue" ) as NumberBox;

                        if ( hfMetricIId != null && nbMetricValue != null  )
                        {
                            int metricId = hfMetricIId.ValueAsInt();
                            var metric = new MetricService( rockContext ).Get( metricId );

                            if ( metric != null )
                            {
                                int campusPartitionId = metric.MetricPartitions.Where( p => p.EntityTypeId.HasValue && p.EntityTypeId.Value == campusEntityTypeId ).Select( p => p.Id ).FirstOrDefault();
                                int schedulePartitionId = metric.MetricPartitions.Where( p => p.EntityTypeId.HasValue && p.EntityTypeId.Value == scheduleEntityTypeId ).Select( p => p.Id ).FirstOrDefault();

                                var metricValue = metricValueService
                                    .Queryable()
                                    .Where( v =>
                                        v.MetricId == metric.Id &&
                                        v.MetricValueDateTime.HasValue && v.MetricValueDateTime.Value == weekend.Value &&
                                        v.MetricValuePartitions.Count == 2 &&
                                        v.MetricValuePartitions.Any( p => p.MetricPartitionId == campusPartitionId && p.EntityId.HasValue && p.EntityId.Value == campusId.Value ) &&
                                        v.MetricValuePartitions.Any( p => p.MetricPartitionId == schedulePartitionId && p.EntityId.HasValue && p.EntityId.Value == scheduleId.Value ) )
                                    .FirstOrDefault();

                                if ( metricValue == null )
                                {
                                    metricValue = new MetricValue();
                                    metricValue.MetricValueType = MetricValueType.Measure;
                                    metricValue.MetricId = metric.Id;
                                    metricValue.MetricValueDateTime = weekend.Value;
                                    metricValueService.Add( metricValue );

                                    var campusValuePartition = new MetricValuePartition();
                                    campusValuePartition.MetricPartitionId = campusPartitionId;
                                    campusValuePartition.EntityId = campusId.Value;
                                    metricValue.MetricValuePartitions.Add( campusValuePartition );

                                    var scheduleValuePartition = new MetricValuePartition();
                                    scheduleValuePartition.MetricPartitionId = schedulePartitionId;
                                    scheduleValuePartition.EntityId = scheduleId.Value;
                                    metricValue.MetricValuePartitions.Add( scheduleValuePartition );
                                }

                                metricValue.YValue = nbMetricValue.Text.AsDecimalOrNull();
                                metricValue.Note = tbNote.Text;
                            }
                        }
                    }

                    rockContext.SaveChanges();
                }

                nbMetricsSaved.Text = string.Format( "Your metrics for the {0} service on {1} at the {2} Campus have been saved.",
                    bddlService.SelectedItem.Text, bddlWeekend.SelectedItem.Text, bddlCampus.SelectedItem.Text );
                nbMetricsSaved.Visible = true;

                BindMetrics();

            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the filter controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bddl_SelectionChanged( object sender, EventArgs e )
        {
            BindMetrics();
        }

        #endregion

        #region Methods

        private bool CheckSelection()
        {
            // If campus and schedule have been selected before, assume current weekend
            if ( _selectedCampusId.HasValue && _selectedServiceId.HasValue && !_selectedWeekend.HasValue )
            {
                _selectedWeekend = RockDateTime.Today.SundayDate();
            }

            var options = new List<ServiceMetricSelectItem>();

            if ( !_selectedCampusId.HasValue )
            {
                lSelection.Text = "Select Location:";
                foreach ( var campus in GetCampuses() )
                {
                    options.Add( new ServiceMetricSelectItem( "Campus", campus.Id.ToString(), campus.Name ) );
                }
            }

            if ( !options.Any() && !_selectedWeekend.HasValue )
            {
                lSelection.Text = "Select Week of:";
                foreach ( var weekend in GetWeekendDates( 1, 0 ) )
                {
                    options.Add( new ServiceMetricSelectItem( "Weekend", weekend.ToString( "o" ), "Sunday " + weekend.ToShortDateString() ) );
                }
            }

            if ( !options.Any() && !_selectedServiceId.HasValue )
            {
                lSelection.Text = "Select Service Time:";
                foreach ( var service in GetServices() )
                {
                    options.Add( new ServiceMetricSelectItem( "Service", service.Id.ToString(), service.Name ) );
                }
            }

            if ( options.Any() )
            {
                rptrSelection.DataSource = options;
                rptrSelection.DataBind();

                pnlSelection.Visible = true;
                pnlMetrics.Visible = false;

                return false;
            }
            else
            {
                pnlSelection.Visible = false;
                pnlMetrics.Visible = true;

                return true;
            }
        }

        private void BuildCampusSelection()
        {
            foreach ( var campus in CampusCache.All()
                .Where( c => c.IsActive.HasValue && c.IsActive.Value )
                .OrderBy( c => c.Name ) )
            {
                bddlCampus.Items.Add( new ListItem( campus.Name, campus.Id.ToString() ) );
            }
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            bddlCampus.Items.Clear();
            bddlWeekend.Items.Clear();
            bddlService.Items.Clear();

            // Load Campuses
            foreach ( var campus in GetCampuses() )
            {
                bddlCampus.Items.Add( new ListItem( campus.Name, campus.Id.ToString() ) );
            }
            bddlCampus.SetValue( _selectedCampusId.Value );

            // Load Weeks
            var weeksBack = GetAttributeValue( "WeeksBack" ).AsInteger();
            var weeksAhead = GetAttributeValue( "WeeksAhead" ).AsInteger();
            foreach ( var date in GetWeekendDates( weeksBack, weeksAhead ) )
            {
                bddlWeekend.Items.Add( new ListItem( "Sunday " + date.ToShortDateString(), date.ToString( "o" ) ) );
            }
            bddlWeekend.SetValue( _selectedWeekend.Value.ToString( "o" ) );

            // Load service times
            foreach( var service in GetServices() )
            {
                bddlService.Items.Add( new ListItem( service.Name, service.Id.ToString() ) );
            }
            bddlService.SetValue( _selectedServiceId.Value );
        }

        /// <summary>
        /// Gets the campuses.
        /// </summary>
        /// <returns></returns>
        private List<CampusCache> GetCampuses()
        {
            var campuses = new List<CampusCache>();

            foreach ( var campus in CampusCache.All()
                .Where( c => c.IsActive.HasValue && c.IsActive.Value )
                .OrderBy( c => c.Name ) )
            {
                campuses.Add( campus );
            }

            return campuses;
        }

        /// <summary>
        /// Gets the weekend dates.
        /// </summary>
        /// <returns></returns>
        private List<DateTime> GetWeekendDates( int weeksBack, int weeksAhead )
        {
            var dates = new List<DateTime>();

            // Load Weeks
            var sundayDate = RockDateTime.Today.SundayDate();
            var daysBack = weeksBack * 7;
            var daysAhead = weeksAhead * 7;
            var startDate = sundayDate.AddDays( 0 - daysBack );
            var date = sundayDate.AddDays( daysAhead );
            while ( date >= startDate )
            {
                dates.Add( date );
                date = date.AddDays( -7 );
            }

            return dates;
        }

        /// <summary>
        /// Gets the services.
        /// </summary>
        /// <returns></returns>
        private List<Schedule> GetServices()
        {
            var services = new List<Schedule>();

            var scheduleCategory = CategoryCache.Get( GetAttributeValue( "ScheduleCategory" ).AsGuid() );
            if ( scheduleCategory != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    foreach ( var schedule in new ScheduleService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( s =>
                            s.CategoryId.HasValue &&
                            s.CategoryId.Value == scheduleCategory.Id )
                        .OrderBy( s => s.Name ) )
                    {
                        services.Add( schedule );
                    }
                }
            }

            return services;
        }

        /// <summary>
        /// Binds the metrics.
        /// </summary>
        private void BindMetrics()
        {
            var serviceMetricValues = new List<ServiceMetric>();

            int campusEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Campus ) ).Id;
            int scheduleEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Schedule ) ).Id;

            int? campusId = bddlCampus.SelectedValueAsInt();
            int? scheduleId = bddlService.SelectedValueAsInt();
            DateTime? weekend = bddlWeekend.SelectedValue.AsDateTime();

            var notes = new List<string>();

            if ( campusId.HasValue && scheduleId.HasValue && weekend.HasValue )
            {

                SetBlockUserPreference( "CampusId", campusId.HasValue ? campusId.Value.ToString() : "" );
                SetBlockUserPreference( "ScheduleId", scheduleId.HasValue ? scheduleId.Value.ToString() : "" );

                var metricCategories = MetricCategoriesFieldAttribute.GetValueAsGuidPairs( GetAttributeValue( "MetricCategories" ) );
                var metricGuids = metricCategories.Select( a => a.MetricGuid ).ToList();
                using ( var rockContext = new RockContext() )
                {
                    var metricValueService = new MetricValueService( rockContext );
                    foreach ( var metric in new MetricService( rockContext )
                        .GetByGuids( metricGuids )
                        .Where( m =>
                            m.MetricPartitions.Count == 2 &&
                            m.MetricPartitions.Any( p => p.EntityTypeId.HasValue && p.EntityTypeId.Value == campusEntityTypeId ) &&
                            m.MetricPartitions.Any( p => p.EntityTypeId.HasValue && p.EntityTypeId.Value == scheduleEntityTypeId ) )
                        .OrderBy( m => m.Title )
                        .Select( m => new
                        {
                            m.Id,
                            m.Title,
                            CampusPartitionId = m.MetricPartitions.Where( p => p.EntityTypeId.HasValue && p.EntityTypeId.Value == campusEntityTypeId ).Select( p => p.Id ).FirstOrDefault(),
                            SchedulePartitionId = m.MetricPartitions.Where( p => p.EntityTypeId.HasValue && p.EntityTypeId.Value == scheduleEntityTypeId ).Select( p => p.Id ).FirstOrDefault(),
                        } ) )
                    {
                        var serviceMetric = new ServiceMetric( metric.Id, metric.Title );

                        if ( campusId.HasValue && weekend.HasValue && scheduleId.HasValue )
                        {
                            var metricValue = metricValueService
                                .Queryable().AsNoTracking()
                                .Where( v =>
                                    v.MetricId == metric.Id &&
                                    v.MetricValueDateTime.HasValue && v.MetricValueDateTime.Value == weekend.Value &&
                                    v.MetricValuePartitions.Count == 2 &&
                                    v.MetricValuePartitions.Any( p => p.MetricPartitionId == metric.CampusPartitionId && p.EntityId.HasValue && p.EntityId.Value == campusId.Value ) &&
                                    v.MetricValuePartitions.Any( p => p.MetricPartitionId == metric.SchedulePartitionId && p.EntityId.HasValue && p.EntityId.Value == scheduleId.Value ) )
                                .FirstOrDefault();

                            if ( metricValue != null )
                            {
                                serviceMetric.Value = metricValue.YValue;

                                if ( !string.IsNullOrWhiteSpace ( metricValue.Note) &&
                                    !notes.Contains( metricValue.Note ) )
                                {
                                    notes.Add( metricValue.Note );
                                }

                            }
                        }

                        serviceMetricValues.Add( serviceMetric );
                    }
                }
            }

            rptrMetric.DataSource = serviceMetricValues;
            rptrMetric.DataBind();

            tbNote.Text = notes.AsDelimited( Environment.NewLine + Environment.NewLine );
        }

        #endregion

    }

    public class ServiceMetricSelectItem
    {
        public string CommandName { get; set; }
        public string CommandArg { get; set; }
        public string OptionText { get; set; }
        public ServiceMetricSelectItem( string commandName, string commandArg, string optionText )
        {
            CommandName = commandName;
            CommandArg = commandArg;
            OptionText = optionText;   
        }
    }

    public class ServiceMetric
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal? Value { get; set; }

        public ServiceMetric( int id, string name )
        {
            Id = id;
            Name = name;
        }
    }
}
