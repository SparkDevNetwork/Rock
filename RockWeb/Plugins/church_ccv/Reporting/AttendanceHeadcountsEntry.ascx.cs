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
using Rock.Web.UI;

namespace RockWeb.Plugins.church_ccv.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Attendance Headcounts Entry" )]
    [Category( "CCV > Reporting" )]
    [Description( "Block for easily adding/editing headcounts for a CCV worship service" )]

    [IntegerField( "Weeks Back", "The number of weeks back to display in the 'Week of' selection.", false, 8, "", 1 )]
    [IntegerField( "Weeks Ahead", "The number of weeks ahead to display in the 'Week of' selection.", false, 0, "", 2 )]

    [IntegerField( "HeadcountsMetricCategoryId", Category = "CustomSetting" )]
    [SchedulesField( "Schedules", Category = "CustomSetting" )]
    public partial class AttendanceHeadcountsEntry : RockBlockCustomSettings
    {
        #region Base Control Methods

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

            nbMetricsSaved.Visible = false;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                LoadDropDowns();
                BindMetrics();
            }
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Shows the settings.
        /// </summary>
        protected override void ShowSettings()
        {
            mpHeadcountsMetric.SetValue( this.GetAttributeValue( "HeadcountsMetricCategoryId" ).AsIntegerOrNull() );
            var rockContext = new RockContext();
            var schedules = new ScheduleService( rockContext ).GetByGuids( this.GetAttributeValue( "Schedules" ).SplitDelimitedValues().AsGuidList() ).ToList();
            spSchedules.SetValues( schedules );

            pnlConfigure.Visible = true;

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

            this.SetAttributeValue( "HeadcountsMetricCategoryId", mpHeadcountsMetric.SelectedValue );
            var rockContext = new RockContext();
            var schedules = new ScheduleService( rockContext ).GetByIds( spSchedules.SelectedValues.AsIntegerList() ).ToList();
            this.SetAttributeValue( "Schedules", schedules.Select( a => a.Guid ).ToList().AsDelimited( "," ) );
            SaveAttributeValues();

            this.Block_BlockUpdated( sender, e );
        }

        #endregion

        #region methods

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            bddlCampus.Items.Clear();
            bddlWeekend.Items.Clear();

            // Load Campuses
            foreach ( var campus in CampusCache.All( false ) )
            {
                bddlCampus.Items.Add( new ListItem( campus.Name, campus.Id.ToString() ) );
            }

            var selectedCampusId = GetBlockUserPreference( "CampusId" ).AsIntegerOrNull();

            bddlCampus.SetValue( selectedCampusId );

            // Load Weeks
            var weeksBack = GetAttributeValue( "WeeksBack" ).AsInteger();
            var weeksAhead = GetAttributeValue( "WeeksAhead" ).AsInteger();
            foreach ( var date in GetWeekendDates( weeksBack, weeksAhead ) )
            {
                bddlWeekend.Items.Add( new ListItem( string.Format( "Weekend of {0} - {1}", date.AddDays( -1 ).ToShortDateString(), date.ToShortDateString() ), date.ToString( "o" ) ) );
            }

            var defaultDate = RockDateTime.Today.SundayDate(); ;

            // make it default to no later than current weekend
            while ( defaultDate > RockDateTime.Today.AddDays( 1 ) )
            {
                defaultDate = defaultDate.AddDays( -7 );
            }

            bddlWeekend.SetValue( defaultDate.ToString( "o" ) );
        }

        /// <summary>
        /// Gets the weekend dates.
        /// </summary>
        /// <param name="weeksBack">The weeks back.</param>
        /// <param name="weeksAhead">The weeks ahead.</param>
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
        /// Gets the service times for campus.
        /// </summary>
        /// <param name="campus">The campus.</param>
        /// <returns></returns>
        private List<Schedule> GetServiceTimesForCampus( CampusCache campus )
        {
            var rockContext = new RockContext();
            var serviceTimesForCampus = new List<Schedule>();
            var scheduleService = new ScheduleService( rockContext );

            var scheduleLookupList = scheduleService.Queryable().Where( a => a.Name != null ).ToList().Select( a => new
            {
                a.Id,
                a.FriendlyScheduleText
            } );

            var selectedScheduleIds = new ScheduleService( rockContext ).GetByGuids( this.GetAttributeValue( "Schedules" ).SplitDelimitedValues().AsGuidList() ).Select( a => a.Id ).ToList();

            var campusServiceTimes = campus.ServiceTimes;

            // add all the advertised schedules
            foreach ( var serviceTime in campusServiceTimes )
            {
                var serviceTimeFriendlyText = string.Format( "{0} at {1}", serviceTime.Day, serviceTime.Time ).Replace( "*", "" ).Replace("%", "").Trim();
                var scheduleLookup = scheduleLookupList.FirstOrDefault( a => a.FriendlyScheduleText.StartsWith( serviceTimeFriendlyText, StringComparison.OrdinalIgnoreCase ) );
                if ( scheduleLookup != null && selectedScheduleIds.Contains( scheduleLookup.Id ) )
                {
                    var schedule = scheduleService.Get( scheduleLookup.Id );
                    serviceTimesForCampus.Add( schedule );
                }
            }

            return serviceTimesForCampus;
        }

        /// <summary>
        /// Binds the metrics.
        /// </summary>
        private void BindMetrics()
        {
            int campusEntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Campus ) ).Id;
            int scheduleEntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Schedule ) ).Id;
            int definedValueEntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.DefinedValue ) ).Id;

            int? campusId = bddlCampus.SelectedValueAsInt();
            DateTime? weekendSundayDate = bddlWeekend.SelectedValue.AsDateTime();

            int? headcountsMetricCategoryId = this.GetAttributeValue( "HeadcountsMetricCategoryId" ).AsIntegerOrNull();
            var rockContext = new RockContext();
            var metricCategory = new MetricCategoryService( rockContext ).Get( headcountsMetricCategoryId ?? 0 );

            if ( !campusId.HasValue )
            {
                return;
            }

            if ( metricCategory == null )
            {
                return;
            }

            var metricValueService = new MetricValueService( rockContext );

            var metric = metricCategory.Metric;
            var metricCampusPartitionId = metric.MetricPartitions.Where( p => p.EntityTypeId.HasValue && p.EntityTypeId.Value == campusEntityTypeId ).Select( p => p.Id ).FirstOrDefault();
            var metricSchedulePartitionId = metric.MetricPartitions.Where( p => p.EntityTypeId.HasValue && p.EntityTypeId.Value == scheduleEntityTypeId ).Select( p => p.Id ).FirstOrDefault();
            var metricDefinedValuePartition = metric.MetricPartitions.Where( p => p.EntityTypeId.HasValue && p.EntityTypeId.Value == definedValueEntityTypeId )
                .Select( p => new
                {
                    p.Id,
                    DefinedTypeId = p.EntityTypeQualifierValue
                } ).FirstOrDefault();

            var definedType = DefinedTypeCache.Read( metricDefinedValuePartition.DefinedTypeId.AsInteger() );
            var definedValueMain = definedType.DefinedValues.FirstOrDefault( a => a.Value == "Main" );
            var definedValueOverflow = definedType.DefinedValues.FirstOrDefault( a => a.Value == "Overflow" );

            var serviceMetricValuesList = new List<ServiceMetricValues>();
            var campusServiceSchedules = GetServiceTimesForCampus( CampusCache.Read( campusId.Value ) );

            foreach ( var campusSchedule in campusServiceSchedules )
            {
                var serviceMetricValue = new ServiceMetricValues( campusSchedule.Id, campusSchedule.FriendlyScheduleText );

                var campusScheduleMetricValues = metricValueService
                                .Queryable().AsNoTracking()
                                .Where( v =>
                                    v.MetricId == metric.Id &&
                                    v.MetricValueDateTime.HasValue && v.MetricValueDateTime.Value == weekendSundayDate.Value &&
                                    v.MetricValuePartitions.Count == 3 &&
                                    v.MetricValuePartitions.Any( p => p.MetricPartitionId == metricCampusPartitionId && p.EntityId.HasValue && p.EntityId.Value == campusId.Value ) &&
                                    v.MetricValuePartitions.Any( p => p.MetricPartitionId == metricSchedulePartitionId && p.EntityId.HasValue && p.EntityId.Value == campusSchedule.Id ) ).ToList();

                foreach ( var campusScheduleMetricValue in campusScheduleMetricValues )
                {
                    var definedValue = campusScheduleMetricValue.MetricValuePartitions.Where( p => p.MetricPartitionId == metricDefinedValuePartition.Id ).Select( a => DefinedValueCache.Read( a.EntityId ?? 0 ) ).FirstOrDefault();
                    if ( definedValue != null )
                    {
                        if ( definedValue.Id == definedValueOverflow.Id )
                        {
                            serviceMetricValue.OverflowValue = (int?)campusScheduleMetricValue.YValue;
                        }
                        else
                        {
                            serviceMetricValue.MainValue = (int?)campusScheduleMetricValue.YValue;
                            serviceMetricValue.Note = campusScheduleMetricValue.Note;
                        }
                    }
                }

                serviceMetricValuesList.Add( serviceMetricValue );
            }

            rptrMetric.DataSource = serviceMetricValuesList;
            rptrMetric.DataBind();
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

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            int campusEntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Campus ) ).Id;
            int scheduleEntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Schedule ) ).Id;
            int definedValueEntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.DefinedValue ) ).Id;

            int? campusId = bddlCampus.SelectedValueAsInt();
            DateTime? weekendSundayDate = bddlWeekend.SelectedValue.AsDateTime();

            int? headcountsMetricCategoryId = this.GetAttributeValue( "HeadcountsMetricCategoryId" ).AsIntegerOrNull();
            var rockContext = new RockContext();
            var metricCategory = new MetricCategoryService( rockContext ).Get( headcountsMetricCategoryId ?? 0 );

            if ( !campusId.HasValue )
            {
                return;
            }

            if ( metricCategory == null )
            {
                return;
            }

            var metricValueService = new MetricValueService( rockContext );

            var metric = metricCategory.Metric;
            var metricCampusPartitionId = metric.MetricPartitions.Where( p => p.EntityTypeId.HasValue && p.EntityTypeId.Value == campusEntityTypeId ).Select( p => p.Id ).FirstOrDefault();
            var metricSchedulePartitionId = metric.MetricPartitions.Where( p => p.EntityTypeId.HasValue && p.EntityTypeId.Value == scheduleEntityTypeId ).Select( p => p.Id ).FirstOrDefault();
            var metricDefinedValuePartition = metric.MetricPartitions.Where( p => p.EntityTypeId.HasValue && p.EntityTypeId.Value == definedValueEntityTypeId )
                .Select( p => new
                {
                    p.Id,
                    DefinedTypeId = p.EntityTypeQualifierValue
                } ).FirstOrDefault();

            var definedType = DefinedTypeCache.Read( metricDefinedValuePartition.DefinedTypeId.AsInteger() );
            var definedValueMain = definedType.DefinedValues.FirstOrDefault( a => a.Value == "Main" );
            var definedValueOverflow = definedType.DefinedValues.FirstOrDefault( a => a.Value == "Overflow" );


            foreach ( RepeaterItem item in rptrMetric.Items )
            {
                var hfScheduleId = item.FindControl( "hfScheduleId" ) as HiddenField;
                var nbMetricMainValue = item.FindControl( "nbMetricMainValue" ) as NumberBox;
                var nbMetricOverflowValue = item.FindControl( "nbMetricOverflowValue" ) as NumberBox;
                var tbNote = item.FindControl( "tbNote" ) as RockTextBox;

                int scheduleId = hfScheduleId.Value.AsInteger();

                var campusScheduleMetricValues = metricValueService
                                    .Queryable()
                                    .Where( v =>
                                        v.MetricId == metric.Id &&
                                        v.MetricValueDateTime.HasValue && v.MetricValueDateTime.Value == weekendSundayDate.Value &&
                                        v.MetricValuePartitions.Count == 3 &&
                                        v.MetricValuePartitions.Any( p => p.MetricPartitionId == metricCampusPartitionId && p.EntityId.HasValue && p.EntityId.Value == campusId.Value ) &&
                                        v.MetricValuePartitions.Any( p => p.MetricPartitionId == metricSchedulePartitionId && p.EntityId.HasValue && p.EntityId.Value == scheduleId ) ).ToList();

                var campusScheduleMetricValueMain = campusScheduleMetricValues.Where( v =>
                    v.MetricValuePartitions.Any( p => p.MetricPartitionId == metricDefinedValuePartition.Id && p.EntityId.HasValue && p.EntityId == definedValueMain.Id ) ).FirstOrDefault();

                var campusScheduleMetricValueOverflow = campusScheduleMetricValues.Where( v =>
                    v.MetricValuePartitions.Any( p => p.MetricPartitionId == metricDefinedValuePartition.Id && p.EntityId.HasValue && p.EntityId == definedValueOverflow.Id ) ).FirstOrDefault();

                if ( campusScheduleMetricValueMain == null )
                {
                    campusScheduleMetricValueMain = new MetricValue();
                    campusScheduleMetricValueMain.MetricValueType = MetricValueType.Measure;
                    campusScheduleMetricValueMain.MetricId = metric.Id;
                    campusScheduleMetricValueMain.MetricValueDateTime = weekendSundayDate.Value;
                    metricValueService.Add( campusScheduleMetricValueMain );

                    var campusValuePartition = new MetricValuePartition();
                    campusValuePartition.MetricPartitionId = metricCampusPartitionId;
                    campusValuePartition.EntityId = campusId.Value;
                    campusScheduleMetricValueMain.MetricValuePartitions.Add( campusValuePartition );

                    var scheduleValuePartition = new MetricValuePartition();
                    scheduleValuePartition.MetricPartitionId = metricSchedulePartitionId;
                    scheduleValuePartition.EntityId = scheduleId;
                    campusScheduleMetricValueMain.MetricValuePartitions.Add( scheduleValuePartition );

                    var definedValuePartition = new MetricValuePartition();
                    definedValuePartition.MetricPartitionId = metricDefinedValuePartition.Id;
                    definedValuePartition.EntityId = definedValueMain.Id;
                    campusScheduleMetricValueMain.MetricValuePartitions.Add( definedValuePartition );
                }

                if ( campusScheduleMetricValueOverflow == null )
                {
                    campusScheduleMetricValueOverflow = new MetricValue();
                    campusScheduleMetricValueOverflow.MetricValueType = MetricValueType.Measure;
                    campusScheduleMetricValueOverflow.MetricId = metric.Id;
                    campusScheduleMetricValueOverflow.MetricValueDateTime = weekendSundayDate.Value;
                    metricValueService.Add( campusScheduleMetricValueOverflow );

                    var campusValuePartition = new MetricValuePartition();
                    campusValuePartition.MetricPartitionId = metricCampusPartitionId;
                    campusValuePartition.EntityId = campusId.Value;
                    campusScheduleMetricValueOverflow.MetricValuePartitions.Add( campusValuePartition );

                    var scheduleValuePartition = new MetricValuePartition();
                    scheduleValuePartition.MetricPartitionId = metricSchedulePartitionId;
                    scheduleValuePartition.EntityId = scheduleId;
                    campusScheduleMetricValueOverflow.MetricValuePartitions.Add( scheduleValuePartition );

                    var definedValuePartition = new MetricValuePartition();
                    definedValuePartition.MetricPartitionId = metricDefinedValuePartition.Id;
                    definedValuePartition.EntityId = definedValueOverflow.Id;
                    campusScheduleMetricValueOverflow.MetricValuePartitions.Add( definedValuePartition );
                }

                campusScheduleMetricValueMain.YValue = nbMetricMainValue.Text.AsDecimalOrNull();
                campusScheduleMetricValueMain.Note = tbNote.Text;
                campusScheduleMetricValueOverflow.YValue = nbMetricOverflowValue.Text.AsDecimalOrNull();

                rockContext.SaveChanges();

                nbMetricsSaved.Visible = true;
            }
        }

        #endregion

        /// <summary>
        /// Handles the SelectionChanged event of the bddlCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bddlCampus_SelectionChanged( object sender, EventArgs e )
        {
            SetBlockUserPreference( "CampusId", bddlCampus.SelectedValue );
            BindMetrics();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the bddlWeekend control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bddlWeekend_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindMetrics();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ServiceMetricValues
    {
        public int ScheduleId { get; set; }

        public string ServiceName { get; set; }

        public int? MainValue { get; set; }

        public int? OverflowValue { get; set; }

        public string Note { get; set; }

        public ServiceMetricValues( int scheduleId, string serviceName )
        {
            ScheduleId = scheduleId;
            ServiceName = serviceName;
        }
    }
}