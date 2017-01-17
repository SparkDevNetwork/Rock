using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using OfficeOpenXml;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.church_ccv.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Attendance Spreadsheet Tool" )]
    [Category( "CCV > Reporting" )]
    [Description( "Helps create the weekly CCV Attendance Spreadsheet" )]

    // stored as comma-delimited GroupTypeIds
    [TextField( "AttendanceTypes", Category = "CustomSetting" )]

    // stored as comma-delimited CampusIds
    [TextField( "Campuses", Category = "CustomSetting" )]

    [IntegerField( "AttendanceMetricCategoryId", Category = "CustomSetting" )]
    [IntegerField( "HeadcountsMetricCategoryId", Category = "CustomSetting" )]
    [SchedulesField( "Schedules", Category = "CustomSetting", DefaultValue = "7883cac8-6e30-482b-95a7-2f0dee859be1,33ff69e9-059b-4702-b1e5-4d499cb7b07a,ff6fb240-0c32-4542-be40-159c522f7e51,4628d917-ec9c-4269-b39c-5bcbef3658e7,1f6c15da-982f-43b1-bde9-d4e70cfbcb45,e258b1e1-9930-417a-b2f8-e6f0b5cf8120,ad7cb062-2867-4d87-989a-9f6c73fa8fbf,2263f399-52d4-4871-a00d-ab9def6f63f0,5f087082-5c57-4fd1-8e76-4a2e86b4cfff" )]
    [TextField( "GroupIds", Category = "CustomSetting", DefaultValue = "1199222,1199221,1199223,1199220,1790001,1199600,1199601,1199602,1789997,1199224,1199225,1559672,1199226,1790006,1790007,1199227,1199228,1199231,1199232,1199233,1790005,1790008,1199229,1199230,1542108,1199596,1199597,1199598,1199599,1542107" )]
    public partial class AttendanceSpreadSheetTool : RockBlockCustomSettings
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gCheckinAttendanceExport.GridRebind += gCheckinAttendanceExport_GridRebind;
            gHeadcountsExport.GridRebind += gHeadcountsExport_GridRebind;

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

            BuildGroupTypesUI();

            if ( !Page.IsPostBack )
            {
                ShowDetails();
            }
        }

        /// <summary>
        /// Shows the details.
        /// </summary>
        private void ShowDetails()
        {
            var lastSundayDate = RockDateTime.Today.SundayDate();
            if ( lastSundayDate > RockDateTime.Today )
            {
                lastSundayDate = lastSundayDate.AddDays( -7 );
            }

            var formatString = "MMM d, yyyy";
            ddlSundayDate.Items.Clear();
            while ( lastSundayDate > RockDateTime.Today.AddMonths( -6 ) )
            {
                ddlSundayDate.Items.Add( new ListItem( string.Format( "{0} - {1}", lastSundayDate.AddDays( -1 ).ToString( formatString ), lastSundayDate.ToString( formatString ) ), lastSundayDate.ToString() ) );
                lastSundayDate = lastSundayDate.AddDays( -7 );
            }

            BindCheckinAttendanceGrid();
            BindHeadcountsGrid();
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        public void LoadDropDownsForSettings()
        {
            var rockContext = new RockContext();

            cblCampuses.Items.Clear();
            foreach ( var campus in CampusCache.All().OrderBy( a => a.Name ) )
            {
                var listItem = new ListItem();
                listItem.Text = campus.Name;
                listItem.Value = campus.Id.ToString();
                cblCampuses.Items.Add( listItem );
            }

            cblCampuses.SetValues( this.GetAttributeValue( "Campuses" ).SplitDelimitedValues().AsIntegerList() );

            cblAttendanceTypes.Items.Clear();
            var groupTypeService = new GroupTypeService( rockContext );
            Guid groupTypePurposeGuid = Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid();
            var attendanceTypes = groupTypeService.Queryable()
                    .Where( a => a.GroupTypePurposeValue.Guid == groupTypePurposeGuid )
                    .OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();

            foreach ( var attendanceType in attendanceTypes )
            {
                cblAttendanceTypes.Items.Add( new ListItem( attendanceType.Name, attendanceType.Id.ToString() ) );
            }

            cblAttendanceTypes.SetValues( this.GetAttributeValue( "AttendanceTypes" ).SplitDelimitedValues().AsIntegerList() );

            mpAttendanceMetric.SetValue( this.GetAttributeValue( "AttendanceMetricCategoryId" ).AsIntegerOrNull() );
            mpHeadcountsMetric.SetValue( this.GetAttributeValue( "HeadcountsMetricCategoryId" ).AsIntegerOrNull() );

            var scheduleService = new ScheduleService( rockContext );
            var selectedSchedules = scheduleService.GetByGuids( this.GetAttributeValue( "Schedules" ).SplitDelimitedValues().AsGuidList() );
            spSchedules.SetValues( selectedSchedules );

            var groupIdList = this.GetAttributeValue( "GroupIds" ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();

            // if no groups are selected, default to showing all of them
            var selectAll = groupIdList.Count == 0;

            var checkboxListControls = rptGroupTypes.ControlsOfTypeRecursive<RockCheckBoxList>();
            foreach ( var cblGroup in checkboxListControls )
            {
                foreach ( ListItem item in cblGroup.Items )
                {
                    item.Selected = selectAll || groupIdList.Contains( item.Value );
                }
            }
        }

        /// <summary>
        /// Shows the settings.
        /// </summary>
        protected override void ShowSettings()
        {
            LoadDropDownsForSettings();
            pnlConfigure.Visible = true;

            mdConfigure.Show();
        }

        /// <summary>
        /// Builds the group types UI
        /// </summary>
        private void BuildGroupTypesUI()
        {
            var groupTypes = this.GetSelectedGroupTypes();
            if ( groupTypes.Any() )
            {
                nbGroupTypeWarning.Visible = false;

                // only add each grouptype/group once in case they are a child of multiple parents
                _addedGroupTypeIds = new List<int>();
                _addedGroupIds = new List<int>();
                rptGroupTypes.DataSource = groupTypes.ToList();
                rptGroupTypes.DataBind();
            }
            else
            {
                nbGroupTypeWarning.Visible = true;
            }
        }

        /// <summary>
        /// Gets the type of the selected template group (Check-In Type)
        /// </summary>
        /// <returns></returns>
        private List<GroupType> GetSelectedGroupTypes()
        {
            var rockContext = new RockContext();
            var result = new List<GroupType>();
            var attendanceTypes = this.GetAttributeValue( "AttendanceTypes" ).SplitDelimitedValues().AsIntegerList();

            foreach ( var groupTypeId in attendanceTypes )
            {
                var groupTypes = new GroupTypeService( rockContext )
                        .GetChildGroupTypes( groupTypeId )
                        .OrderBy( a => a.Order )
                        .ThenBy( a => a.Name )
                        .ToList();

                result.AddRange( groupTypes );
            }

            return result;
        }

        /// <summary>
        /// Gets the selected group ids in sorted order
        /// </summary>
        /// <returns></returns>
        private List<int> GetSelectedGroupIds()
        {
            var selectedGroupIds = this.GetAttributeValue( "GroupIds" ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList().AsIntegerList();

            return selectedGroupIds;
        }

        // list of grouptype ids that have already been rendered (in case a group type has multiple parents )
        private List<int> _addedGroupTypeIds;

        private List<int> _addedGroupIds;

        /// <summary>
        /// Adds the group type controls.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="pnlGroupTypes">The PNL group types.</param>
        private void AddGroupTypeControls( GroupTypeCache groupType, HtmlGenericContainer liGroupTypeItem, RockContext rockContext )
        {
            if ( !_addedGroupTypeIds.Contains( groupType.Id ) )
            {
                _addedGroupTypeIds.Add( groupType.Id );

                var groupService = new GroupService( rockContext );
                var childGroupTypes = groupType.ChildGroupTypes;

                // limit to Groups that don't have a Parent, or the ParentGroup is a different grouptype so we don't end up with infinite recursion
                var childGroups = groupService.Queryable().Where( a => a.GroupTypeId == groupType.Id )
                    .Where( g => !g.ParentGroupId.HasValue || ( g.ParentGroup.GroupTypeId != groupType.Id ) )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name )
                    .Include( a => a.GroupLocations )
                    .ToList();

                if ( childGroups.Any() )
                {
                    var cblGroupTypeGroups = new RockCheckBoxList { ID = "cblGroupTypeGroups" + groupType.Id };

                    cblGroupTypeGroups.Label = groupType.Name;
                    cblGroupTypeGroups.Items.Clear();

                    foreach ( var group in childGroups )
                    {
                        AddGroupControls( group, cblGroupTypeGroups, groupService );
                    }

                    liGroupTypeItem.Controls.Add( cblGroupTypeGroups );
                }
                else
                {
                    if ( childGroupTypes.Any() )
                    {
                        liGroupTypeItem.Controls.Add( new Label { Text = groupType.Name, ID = "lbl" + groupType.Name } );
                    }
                }

                if ( childGroupTypes.Any() )
                {
                    var ulGroupTypeList = new HtmlGenericContainer( "ul", "list-unstyled" );

                    liGroupTypeItem.Controls.Add( ulGroupTypeList );
                    foreach ( var childGroupType in childGroupTypes.OrderBy( a => a.Order ).ThenBy( a => a.Name ) )
                    {
                        var liChildGroupTypeItem = new HtmlGenericContainer( "li" );
                        liChildGroupTypeItem.ID = "liGroupTypeItem" + childGroupType.Id;
                        ulGroupTypeList.Controls.Add( liChildGroupTypeItem );
                        AddGroupTypeControls( childGroupType, liChildGroupTypeItem, rockContext );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptGroupTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptGroupTypes_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var groupType = e.Item.DataItem as GroupType;

                var liGroupTypeItem = new HtmlGenericContainer( "li", "rocktree-item rocktree-folder" );
                liGroupTypeItem.ID = "liGroupTypeItem" + groupType.Id;
                e.Item.Controls.Add( liGroupTypeItem );

                var rockContext = new RockContext();
                AddGroupTypeControls( GroupTypeCache.Read( groupType.Id ), liGroupTypeItem, rockContext );
            }
        }

        /// <summary>
        /// Adds the group controls.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="checkBoxList">The check box list.</param>
        /// <param name="service">The service.</param>
        /// <param name="showGroupAncestry">if set to <c>true</c> [show group ancestry].</param>
        private void AddGroupControls( Group group, RockCheckBoxList checkBoxList, GroupService service )
        {
            // Only show groups that actually have a schedule
            if ( group != null )
            {
                if ( !_addedGroupIds.Contains( group.Id ) )
                {
                    _addedGroupIds.Add( group.Id );
                    if ( group.ScheduleId.HasValue || group.GroupLocations.Any( l => l.Schedules.Any() ) )
                    {
                        checkBoxList.Items.Add( new ListItem( group.Name, group.Id.ToString() ) );
                    }

                    if ( group.Groups != null )
                    {
                        foreach ( var childGroup in group.Groups
                            .OrderBy( a => a.Order )
                            .ThenBy( a => a.Name )
                            .ToList() )
                        {
                            AddGroupControls( childGroup, checkBoxList, service );
                        }
                    }
                }
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetails();
        }

        /// <summary>
        /// Handles the GridRebind event of the gPledges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gCheckinAttendanceExport_GridRebind( object sender, EventArgs e )
        {
            BindCheckinAttendanceGrid();
        }

        private void gHeadcountsExport_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindHeadcountsGrid();
        }

        #endregion

        #region Methods

        private List<int> GetSelectedScheduleIds()
        {
            var scheduleService = new ScheduleService( new RockContext() );
            var selectedSchedules = scheduleService.GetByGuids( this.GetAttributeValue( "Schedules" ).SplitDelimitedValues().AsGuidList() );

            return selectedSchedules.Select( a => a.Id ).ToList();
        }

        /// <summary>
        /// Adds the headcounts schedule columns.
        /// </summary>
        /// <param name="metric">The metric.</param>
        /// <param name="sundayDate">The sunday date.</param>
        private void AddHeadcountsScheduleColumns( Metric metric, DateTime sundayDate )
        {
            var rockContext = new RockContext();

            gHeadcountsExport.Columns.Clear();

            gHeadcountsExport.Columns.Add( new RockBoundField { HeaderText = "Area", DataField = "Area" } );

            var entityTypeIdCampus = EntityTypeCache.Read<Campus>().Id;
            var entityTypeIdSchedule = EntityTypeCache.Read<Schedule>().Id;

            var scheduleList = new ScheduleService( rockContext ).Queryable().Where( a => a.Name != null ).ToList().Select( a => new
            {
                a.Id,
                a.FriendlyScheduleText
            } );

            var selectedScheduleIds = GetSelectedScheduleIds();
            var campuses = CampusCache.All( false );
            foreach ( var campus in campuses.OrderBy( a => a.Id ) )
            {
                var campusServiceTimes = campus.ServiceTimes;

                // add all the advertised schedules first
                foreach ( var serviceTime in campusServiceTimes )
                {
                    var serviceTimeFriendlyText = string.Format( "{0} at {1}", serviceTime.Day, serviceTime.Time ).Replace( "*", "" ).Trim();
                    var schedule = scheduleList.FirstOrDefault( a => a.FriendlyScheduleText.StartsWith( serviceTimeFriendlyText, StringComparison.OrdinalIgnoreCase ) );
                    if ( schedule != null && selectedScheduleIds.Contains( schedule.Id ) )
                    {
                        string campusScheduleFieldName = string.Format( "campusScheduleField_Campus{0}_Schedule{1}", campus.Id, schedule.Id );

                        var headerText = string.Format( "{0} - {1}", campus.Name, schedule.FriendlyScheduleText );
                        RockBoundField campusScheduleField = new RockBoundField { HeaderText = headerText, DataField = campusScheduleFieldName };
                        gHeadcountsExport.Columns.Add( campusScheduleField );
                    }
                }

                // look up all the schedules that were used in the Headcount Metric for this campus and also add those if they aren't there already
                var metricCampusScheduleIds = new MetricValueService( rockContext ).Queryable()
                    .Where( a => a.MetricId == metric.Id && a.MetricValueDateTime == sundayDate )
                    .Where( a => a.MetricValuePartitions.FirstOrDefault( x => x.MetricPartition.EntityTypeId == entityTypeIdCampus ).EntityId == campus.Id )
                    .SelectMany( a => a.MetricValuePartitions )
                    .Where( a => a.MetricPartition.EntityTypeId == entityTypeIdSchedule )
                    .Select( a => a.EntityId ?? 0 ).Distinct().ToList();

                var metricCampusSchedules = scheduleList.Where( a => metricCampusScheduleIds.Contains( a.Id ) && selectedScheduleIds.Contains( a.Id ) );
                foreach ( var schedule in metricCampusSchedules )
                {
                    string campusScheduleFieldName = string.Format( "campusScheduleField_Campus{0}_Schedule{1}", campus.Id, schedule.Id );
                    if ( !gHeadcountsExport.Columns.OfType<RockBoundField>().Any( a => a.DataField == campusScheduleFieldName ) )
                    {
                        var headerText = string.Format( "{0} - {1}", campus.Name, schedule.FriendlyScheduleText );
                        RockBoundField campusScheduleField = new RockBoundField { HeaderText = headerText, DataField = campusScheduleFieldName };
                        gHeadcountsExport.Columns.Add( campusScheduleField );
                    }
                }

                string campusSummaryFieldName = string.Format( "campusSummaryField_Campus{0}", campus.Id );
                RockBoundField campusSummaryField = new RockBoundField { HeaderText = campus.Name, DataField = campusSummaryFieldName };

                // add a blank "dummy" column to help the Excel Export of Headcounts and Attendance line up
                gHeadcountsExport.Columns.Add( new RockBoundField { HeaderText = "-", DataField = string.Format( "campusDummyField_Campus{0}", campus.Id ) } );

                gHeadcountsExport.Columns.Add( campusSummaryField );
            }

            gHeadcountsExport.Columns.Add( new RockBoundField { DataField = "GrandTotal", HeaderText = "Grand Totals" } );
        }

        /// <summary>
        /// Adds the attendance schedule columns.
        /// </summary>
        /// <param name="metric">The metric.</param>
        /// <param name="sundayDate">The sunday date.</param>
        private void AddAttendanceScheduleColumns( Metric metric, DateTime sundayDate )
        {
            var rockContext = new RockContext();

            // clear out any existing schedule columns and add the ones that match the current filter setting
            gCheckinAttendanceExport.Columns.Clear();
            gCheckinAttendanceExport.Columns.Add( new Rock.Web.UI.Controls.ReorderField() );

            gCheckinAttendanceExport.Columns.Add( new RockBoundField { DataField = "GroupType", HeaderText = "Area" } );
            gCheckinAttendanceExport.Columns.Add( new RockBoundField { DataField = "GroupName", HeaderText = "" } );

            var groupIds = this.GetSelectedGroupIds();

            var scheduleList = new ScheduleService( rockContext ).Queryable().Where( a => a.Name != null ).ToList().Select( a => new
            {
                a.Id,
                a.FriendlyScheduleText
            } );

            var entityTypeIdSchedule = EntityTypeCache.Read<Schedule>().Id;
            var entityTypeIdGroup = EntityTypeCache.Read<Group>().Id;
            var entityTypeIdCampus = EntityTypeCache.Read<Campus>().Id;
            var selectedGroupIds = this.GetSelectedGroupIds();
            var selectedScheduleIds = GetSelectedScheduleIds();

            var campuses = CampusCache.All( false );
            foreach ( var campus in campuses.OrderBy( a => a.Id ) )
            {
                var campusServiceTimes = campus.ServiceTimes;

                // add all the advertised schedules first
                foreach ( var serviceTime in campusServiceTimes )
                {
                    var serviceTimeFriendlyText = string.Format( "{0} at {1}", serviceTime.Day, serviceTime.Time ).Replace( "*", "" ).Trim();
                    var schedule = scheduleList.FirstOrDefault( a => a.FriendlyScheduleText.StartsWith( serviceTimeFriendlyText, StringComparison.OrdinalIgnoreCase ) );
                    if ( schedule != null && selectedScheduleIds.Contains( schedule.Id ) )
                    {
                        string campusScheduleFieldName = string.Format( "campusScheduleField_Campus{0}_Schedule{1}", campus.Id, schedule.Id );

                        var headerText = string.Format( "{0} - {1}", campus.Name, schedule.FriendlyScheduleText.Replace( " at ", "\n" ) );
                        RockBoundField campusScheduleField = new RockBoundField { HeaderText = headerText, DataField = campusScheduleFieldName };
                        gCheckinAttendanceExport.Columns.Add( campusScheduleField );
                    }
                }

                // look up all the schedules that were used and also add those if they aren't there already
                var metricCampusScheduleIds = new MetricValueService( rockContext ).Queryable()
                    .Where( a => a.MetricId == metric.Id && a.MetricValueDateTime == sundayDate )
                    .Where( a => a.MetricValuePartitions.FirstOrDefault( x => x.MetricPartition.EntityTypeId == entityTypeIdCampus ).EntityId == campus.Id )
                    .Where( a => selectedGroupIds.Contains( a.MetricValuePartitions.FirstOrDefault( x => x.MetricPartition.EntityTypeId == entityTypeIdGroup ).EntityId ?? 0 ) )
                    .SelectMany( a => a.MetricValuePartitions )
                    .Where( a => a.MetricPartition.EntityTypeId == entityTypeIdSchedule )
                    .Select( a => a.EntityId ).Distinct().ToList();

                var metricCampusSchedules = scheduleList.Where( a => metricCampusScheduleIds.Contains( a.Id ) && selectedScheduleIds.Contains( a.Id ) );
                foreach ( var schedule in metricCampusSchedules )
                {
                    string campusScheduleFieldName = string.Format( "campusScheduleField_Campus{0}_Schedule{1}", campus.Id, schedule.Id );
                    if ( !gCheckinAttendanceExport.Columns.OfType<RockBoundField>().Any( a => a.DataField == campusScheduleFieldName ) )
                    {
                        var headerText = string.Format( "{0} - {1}", campus.Name, schedule.FriendlyScheduleText.Replace( " at ", "\n" ) );
                        RockBoundField campusScheduleField = new RockBoundField { HeaderText = headerText, DataField = campusScheduleFieldName };
                        gCheckinAttendanceExport.Columns.Add( campusScheduleField );
                    }
                }

                string campusSummaryFieldName = string.Format( "campusSummaryField_Campus{0}", campus.Id );
                RockBoundField campusSummaryField = new RockBoundField { HeaderText = campus.Name + " Totals", DataField = campusSummaryFieldName };
                gCheckinAttendanceExport.Columns.Add( campusSummaryField );
            }

            gCheckinAttendanceExport.Columns.Add( new RockBoundField { DataField = "GrandTotal", HeaderText = "Grand Totals" } );
        }

        /// <summary>
        /// Binds the headcounts grid.
        /// </summary>
        private void BindHeadcountsGrid()
        {
            RockContext rockContext = new RockContext();
            var metricCategoryService = new MetricCategoryService( rockContext );

            Metric headcountsMetric = null;
            var headcountsMetricCategoryId = this.GetAttributeValue( "HeadcountsMetricCategoryId" ).AsIntegerOrNull();
            if ( headcountsMetricCategoryId.HasValue )
            {
                var metricCategory = metricCategoryService.Get( headcountsMetricCategoryId.Value );
                if ( metricCategory != null )
                {
                    headcountsMetric = metricCategory.Metric;
                }
            }

            if ( headcountsMetric == null )
            {
                nbHeadcountsMetricWarning.Visible = true;
                return;
            }
            else
            {
                nbHeadcountsMetricWarning.Visible = false;
            }

            var sundayDate = ddlSundayDate.SelectedValue.AsDateTime().Value;
            lSundayDate.Text = ddlSundayDate.SelectedItem.Text;
            AddHeadcountsScheduleColumns( headcountsMetric, sundayDate );

            var entityTypeIdCampus = EntityTypeCache.Read<Campus>().Id;
            var entityTypeIdSchedule = EntityTypeCache.Read<Schedule>().Id;
            int entityTypeIdDefinedValue = EntityTypeCache.Read( typeof( Rock.Model.DefinedValue ) ).Id;
            var metricValueService = new MetricValueService( rockContext );
            var headcountsMetricValuesQuery = metricValueService.Queryable()
                .Where( a => a.MetricId == headcountsMetric.Id && a.MetricValueDateTime == sundayDate )
                .Include( a => a.MetricValuePartitions );

            var scheduleLookup = new ScheduleService( rockContext ).Queryable().Where( a => a.Name != null ).ToList();
            var selectedScheduleIds = GetSelectedScheduleIds();

            var headcountMetricValueList = headcountsMetricValuesQuery.ToList();

            DataTable dataTable = new DataTable( "HeadcountsExportData" );
            foreach ( var boundField in gHeadcountsExport.Columns.OfType<RockBoundField>() )
            {
                DataColumn dataColumn;
                if ( boundField.DataField == "Area" )
                {
                    dataColumn = new DataColumn( boundField.DataField, typeof( string ) );
                }
                else if ( boundField.DataField.StartsWith( "campusDummyField_Campus" ) )
                {
                    dataColumn = new DataColumn( boundField.DataField, typeof( int ) );
                }
                else
                {
                    dataColumn = new DataColumn( boundField.DataField, typeof( int ) );
                }

                dataTable.Columns.Add( dataColumn );
            }

            DataRow dataRowMain = dataTable.NewRow();
            DataRow dataRowOverflow = dataTable.NewRow();

            foreach ( var dataColumn in dataTable.Columns.OfType<DataColumn>() )
            {
                if ( dataColumn.ColumnName.Equals( "Area" ) )
                {
                    dataRowMain[dataColumn] = "Main";
                    dataRowOverflow[dataColumn] = "Overflow";
                }
                else if ( dataColumn.ColumnName.StartsWith( "campusScheduleField_" ) )
                {
                    // "campusScheduleField_Campus{0}_Schedule{1}"
                    var idParts = dataColumn.ColumnName.Replace( "campusScheduleField_Campus", string.Empty ).Replace( "_Schedule", "," ).Split( ',' ).ToArray();
                    var campusId = idParts[0].AsInteger();
                    var scheduleId = idParts[1].AsInteger();
                    var campusScheduleValues = headcountMetricValueList.Where( a =>
                        a.MetricValuePartitions.FirstOrDefault( x => x.MetricPartition.EntityTypeId == entityTypeIdCampus ).EntityId == campusId
                        &&
                        a.MetricValuePartitions.FirstOrDefault( x => x.MetricPartition.EntityTypeId == entityTypeIdSchedule ).EntityId == scheduleId ).ToList();

                    dataRowMain[dataColumn] = (int)campusScheduleValues.Where( a =>
                         DefinedValueCache.Read( a.MetricValuePartitions.FirstOrDefault( x => x.MetricPartition.EntityTypeId == entityTypeIdDefinedValue ).EntityId ?? 0 ).Value == "Main" )
                         .Sum( a => a.YValue ?? 0.00M );

                    dataRowOverflow[dataColumn] = (int)campusScheduleValues.Where( a =>
                         DefinedValueCache.Read( a.MetricValuePartitions.FirstOrDefault( x => x.MetricPartition.EntityTypeId == entityTypeIdDefinedValue ).EntityId ?? 0 ).Value == "Overflow" )
                         .Sum( a => a.YValue ?? 0.00M );
                }
                else if ( dataColumn.ColumnName.StartsWith( "campusDummyField_Campus" ) )
                {
                    //dataRowMain[dataColumn] = ;
                    //dataRowOverflow[dataColumn] = -1;
                }
                else if ( dataColumn.ColumnName.StartsWith( "campusSummaryField_" ) )
                {
                    // "campusSummaryField_Campus{0}"
                    var campusId = dataColumn.ColumnName.Replace( "campusSummaryField_Campus", string.Empty ).AsInteger();
                    var campusSummaryValues = headcountMetricValueList.Where( a =>
                        a.MetricValuePartitions.FirstOrDefault( x => x.MetricPartition.EntityTypeId == entityTypeIdCampus ).EntityId == campusId
                        &&
                        selectedScheduleIds.Contains( a.MetricValuePartitions.FirstOrDefault( x => x.MetricPartition.EntityTypeId == entityTypeIdSchedule ).EntityId ?? 0 ) ).ToList();

                    dataRowMain[dataColumn] = (int)campusSummaryValues.Where( a =>
                         DefinedValueCache.Read( a.MetricValuePartitions.FirstOrDefault( x => x.MetricPartition.EntityTypeId == entityTypeIdDefinedValue ).EntityId ?? 0 ).Value == "Main" )
                         .Sum( a => a.YValue ?? 0.00M );

                    dataRowOverflow[dataColumn] = (int)campusSummaryValues.Where( a =>
                         DefinedValueCache.Read( a.MetricValuePartitions.FirstOrDefault( x => x.MetricPartition.EntityTypeId == entityTypeIdDefinedValue ).EntityId ?? 0 ).Value == "Overflow" )
                         .Sum( a => a.YValue ?? 0.00M );
                }
                else if ( dataColumn.ColumnName == "GrandTotal" )
                {
                    dataRowMain[dataColumn] = (int)headcountMetricValueList.Where( a =>
                         DefinedValueCache.Read( a.MetricValuePartitions.FirstOrDefault( x => x.MetricPartition.EntityTypeId == entityTypeIdDefinedValue ).EntityId ?? 0 ).Value == "Main" )
                         .Where( a => selectedScheduleIds.Contains( a.MetricValuePartitions.FirstOrDefault( x => x.MetricPartition.EntityTypeId == entityTypeIdSchedule ).EntityId ?? 0 ) ).Sum( a => a.YValue ?? 0.00M );

                    dataRowOverflow[dataColumn] = (int)headcountMetricValueList.Where( a =>
                         DefinedValueCache.Read( a.MetricValuePartitions.FirstOrDefault( x => x.MetricPartition.EntityTypeId == entityTypeIdDefinedValue ).EntityId ?? 0 ).Value == "Overflow" )
                         .Where( a => selectedScheduleIds.Contains( a.MetricValuePartitions.FirstOrDefault( x => x.MetricPartition.EntityTypeId == entityTypeIdSchedule ).EntityId ?? 0 ) ).Sum( a => a.YValue ?? 0.00M );
                }
            }

            dataTable.Rows.Add( dataRowMain );
            dataTable.Rows.Add( dataRowOverflow );

            gHeadcountsExport.ExportFilename = string.Format( "HeadcountsExport_{0}_{1}", sundayDate.AddDays( -1 ).ToString( "yyyyMMdd" ), sundayDate.ToString( "yyyyMMdd" ) );
            gHeadcountsExport.DataSource = dataTable;
            gHeadcountsExport.DataBind();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindCheckinAttendanceGrid( bool combineJrAndHighSchool = false )
        {
            RockContext rockContext = new RockContext();
            var metricCategoryService = new MetricCategoryService( rockContext );

            Metric attendanceMetric = null;
            var metricCategoryId = this.GetAttributeValue( "AttendanceMetricCategoryId" ).AsIntegerOrNull();
            if ( metricCategoryId.HasValue )
            {
                var metricCategory = metricCategoryService.Get( metricCategoryId.Value );
                if ( metricCategory != null )
                {
                    attendanceMetric = metricCategory.Metric;
                }
            }

            if ( attendanceMetric == null )
            {
                nbAttendanceMetricWarning.Visible = true;
                return;
            }
            else
            {
                nbAttendanceMetricWarning.Visible = false;
            }

            var sundayDate = ddlSundayDate.SelectedValue.AsDateTime().Value;
            lSundayDate.Text = ddlSundayDate.SelectedItem.Text;
            AddAttendanceScheduleColumns( attendanceMetric, sundayDate );

            var selectedGroupIds = this.GetSelectedGroupIds();
            var selectedScheduleIds = GetSelectedScheduleIds();

            var entityTypeIdGroup = EntityTypeCache.Read<Group>().Id;
            var entityTypeIdCampus = EntityTypeCache.Read<Campus>().Id;
            var entityTypeIdSchedule = EntityTypeCache.Read<Schedule>().Id;
            var metricValueService = new MetricValueService( rockContext );
            var metricValuesQuery = metricValueService.Queryable()
                .Where( a => a.MetricId == attendanceMetric.Id && a.MetricValueDateTime == sundayDate )
                .Where( a => selectedGroupIds.Contains( a.MetricValuePartitions.FirstOrDefault( x => x.MetricPartition.EntityTypeId == entityTypeIdGroup ).EntityId ?? 0 ) )
                .Where( a => selectedScheduleIds.Contains( a.MetricValuePartitions.FirstOrDefault( x => x.MetricPartition.EntityTypeId == entityTypeIdSchedule ).EntityId ?? 0 ) )
                .Include( a => a.MetricValuePartitions );

            var scheduleLookup = new ScheduleService( rockContext ).Queryable().Where( a => a.Name != null ).ToList();

            var list = metricValuesQuery.ToList();
            var groupService = new GroupService( rockContext );
            var groupAttendanceMetricsList = new List<GroupAttendanceMetrics>();

            int jrHighGroupTypeId = 21;
            int highScheduleGroupTypeId = 22;

            var groupList = groupService.GetByIds( selectedGroupIds ).ToList();

            // display grid in the same order that the Groups are displayed
            // but do additional custom sorting and grouping based on what the printed spreadsheet requirements
            foreach ( var groupId in selectedGroupIds )
            {
                var group = groupService.Get( groupId );
                var groupMetricValues = list.Where( a => a.MetricValuePartitions.FirstOrDefault( x => x.MetricPartition.EntityTypeId == entityTypeIdGroup ).EntityId == groupId ).ToList();
                GroupAttendanceMetrics groupAttendanceMetrics;
                if ( group.GroupTypeId == jrHighGroupTypeId && combineJrAndHighSchool )
                {
                    groupAttendanceMetrics = groupAttendanceMetricsList.FirstOrDefault( a => a.Group.GroupTypeId == jrHighGroupTypeId );
                    if ( groupAttendanceMetrics != null )
                    {
                        groupAttendanceMetrics.MetricValues.AddRange( groupMetricValues );
                    }
                    else
                    {
                        group.Name = "Jr. High";
                        groupAttendanceMetrics = new GroupAttendanceMetrics { Group = group, MetricValues = groupMetricValues };
                        groupAttendanceMetricsList.Add( groupAttendanceMetrics );
                    }
                }
                else if ( group.GroupTypeId == highScheduleGroupTypeId && combineJrAndHighSchool )
                {
                    groupAttendanceMetrics = groupAttendanceMetricsList.FirstOrDefault( a => a.Group.GroupTypeId == highScheduleGroupTypeId );
                    if ( groupAttendanceMetrics != null )
                    {
                        groupAttendanceMetrics.MetricValues.AddRange( groupMetricValues );
                    }
                    else
                    {
                        group.Name = "High School";
                        groupAttendanceMetrics = new GroupAttendanceMetrics { Group = group, MetricValues = groupMetricValues };
                        groupAttendanceMetricsList.Add( groupAttendanceMetrics );
                    }
                }
                else
                {
                    groupAttendanceMetrics = new GroupAttendanceMetrics { Group = group, MetricValues = groupMetricValues };
                    groupAttendanceMetricsList.Add( groupAttendanceMetrics );
                }
            }

            DataTable dataTable = new DataTable( "AttendanceExportData" );
            foreach ( var boundField in gCheckinAttendanceExport.Columns.OfType<RockBoundField>() )
            {
                DataColumn dataColumn = new DataColumn( boundField.DataField );
                if ( boundField.DataField == "GroupName" || boundField.DataField == "GroupType" )
                {
                    dataColumn.DataType = typeof( string );
                }
                else
                {
                    dataColumn.DataType = typeof( int );
                }

                dataTable.Columns.Add( dataColumn );
            }

            foreach ( var groupAttendanceMetrics in groupAttendanceMetricsList )
            {
                DataRow dataRow = dataTable.NewRow();
                foreach ( var dataColumn in dataTable.Columns.OfType<DataColumn>() )
                {
                    if ( dataColumn.ColumnName == "SortKey" )
                    {
                        dataRow[dataColumn] = groupAttendanceMetricsList.IndexOf( groupAttendanceMetrics );
                    }
                    else if ( dataColumn.ColumnName == "GroupName" )
                    {
                        dataRow[dataColumn] = groupAttendanceMetrics.Group.Name;
                    }
                    else if ( dataColumn.ColumnName == "GroupType" )
                    {
                        dataRow[dataColumn] = groupAttendanceMetrics.Group.GroupType.Name;
                    }
                    else if ( dataColumn.ColumnName.StartsWith( "campusScheduleField_" ) )
                    {
                        // "campusScheduleField_Campus{0}_Schedule{1}"
                        var idParts = dataColumn.ColumnName.Replace( "campusScheduleField_Campus", string.Empty ).Replace( "_Schedule", "," ).Split( ',' ).ToArray();
                        var campusId = idParts[0].AsInteger();
                        var scheduleId = idParts[1].AsInteger();
                        var campusScheduleValues = groupAttendanceMetrics.MetricValues.Where( a =>
                            a.MetricValuePartitions.FirstOrDefault( x => x.MetricPartition.EntityTypeId == entityTypeIdCampus ).EntityId == campusId
                            &&
                            a.MetricValuePartitions.FirstOrDefault( x => x.MetricPartition.EntityTypeId == entityTypeIdSchedule ).EntityId == scheduleId ).ToList();

                        dataRow[dataColumn] = (int)campusScheduleValues.Sum( a => a.YValue ?? 0.00M );
                    }
                    else if ( dataColumn.ColumnName.StartsWith( "campusSummaryField_" ) )
                    {
                        // "campusSummaryField_Campus{0}"
                        var campusId = dataColumn.ColumnName.Replace( "campusSummaryField_Campus", string.Empty ).AsInteger();
                        var campusSummaryValues = groupAttendanceMetrics.MetricValues.Where( a =>
                            a.MetricValuePartitions.FirstOrDefault( x => x.MetricPartition.EntityTypeId == entityTypeIdCampus ).EntityId == campusId
                            &&
                            selectedScheduleIds.Contains( a.MetricValuePartitions.FirstOrDefault( x => x.MetricPartition.EntityTypeId == entityTypeIdSchedule ).EntityId ?? 0 ) ).ToList();

                        dataRow[dataColumn] = (int)campusSummaryValues.Sum( a => a.YValue ?? 0.00M );
                    }
                    else if ( dataColumn.ColumnName == "GrandTotal" )
                    {
                        dataRow[dataColumn] = (int)groupAttendanceMetrics.MetricValues.Where( a => selectedScheduleIds.Contains( a.MetricValuePartitions.FirstOrDefault( x => x.MetricPartition.EntityTypeId == entityTypeIdSchedule ).EntityId ?? 0 ) ).Sum( a => a.YValue ?? 0.00M );
                    }
                }

                dataTable.Rows.Add( dataRow );
            }

            gHeadcountsExport.ExportFilename = string.Format( "CheckinExport_{0}_{1}", sundayDate.AddDays( -1 ).ToString( "yyyyMMdd" ), sundayDate.ToString( "yyyyMMdd" ) );
            gCheckinAttendanceExport.DataSource = dataTable;
            gCheckinAttendanceExport.DataBind();
        }

        #endregion

        /// <summary>
        /// Handles the Click event of the btnUpdate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnUpdate_Click( object sender, EventArgs e )
        {
            BindHeadcountsGrid();
            BindCheckinAttendanceGrid( false );
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

            this.SetAttributeValue( "AttendanceTypes", cblAttendanceTypes.SelectedValues.AsDelimited( "," ) );
            this.SetAttributeValue( "Campuses", cblCampuses.SelectedValues.AsDelimited( "," ) );
            this.SetAttributeValue( "AttendanceMetricCategoryId", mpAttendanceMetric.SelectedValue );
            this.SetAttributeValue( "HeadcountsMetricCategoryId", mpHeadcountsMetric.SelectedValue );
            var selectedScheduleIds = spSchedules.SelectedValuesAsInt().ToList();
            var selectedScheduleGuids = new ScheduleService( new RockContext() ).GetByIds( selectedScheduleIds ).Select( a => a.Guid ).ToList();
            this.SetAttributeValue( "Schedules", selectedScheduleGuids.AsDelimited( "," ) );

            var selectedGroupIds = new List<int>();
            var checkboxListControls = rptGroupTypes.ControlsOfTypeRecursive<RockCheckBoxList>();
            foreach ( var cblGroup in checkboxListControls )
            {
                selectedGroupIds.AddRange( cblGroup.SelectedValuesAsInt );
            }

            this.SetAttributeValue( "GroupIds", selectedGroupIds.AsDelimited( "," ) );

            SaveAttributeValues();

            this.Block_BlockUpdated( sender, e );
        }

        /// <summary>
        /// 
        /// </summary>
        private class GroupAttendanceMetrics
        {
            /// <summary>
            /// Gets or sets the group.
            /// </summary>
            /// <value>
            /// The group.
            /// </value>
            public Group Group { get; set; }

            /// <summary>
            /// Gets or sets the metric values.
            /// </summary>
            /// <value>
            /// The metric values.
            /// </value>
            public List<MetricValue> MetricValues { get; set; }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return Group.GroupType.Name + " | " + Group.Name;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCreateSpreadsheet control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCreateSpreadsheet_Click( object sender, EventArgs e )
        {
            var sundayDate = ddlSundayDate.SelectedValue.AsDateTime().Value;
            string filename = string.Format( "AttendanceSpreadsheet_{0}_{1}.xlsx", sundayDate.AddDays( -1 ).ToString( "yyyyMMdd" ), sundayDate.ToString( "yyyyMMdd" ) );
            string workSheetName = "Export";

            ExcelPackage excel = new ExcelPackage();

            excel.Workbook.Properties.Title = "Attendance Spreadsheet";

            // add author info
            Rock.Model.UserLogin userLogin = Rock.Model.UserLoginService.GetCurrentUser();
            if ( userLogin != null )
            {
                excel.Workbook.Properties.Author = userLogin.Person.FullName;
            }
            else
            {
                excel.Workbook.Properties.Author = "Rock";
            }

            // add the page that created this
            excel.Workbook.Properties.SetCustomPropertyValue( "Source", this.Page.Request.Url.OriginalString );

            ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add( workSheetName );
            worksheet.DefaultColWidth = 5;

            var headerRows = 3;
            int rowCounter = headerRows;
            int columnCounter = 1;

            gCheckinAttendanceExport.AllowPaging = false;
            BindCheckinAttendanceGrid( true );
            BindHeadcountsGrid();

            var gridDataFields = gCheckinAttendanceExport.Columns.OfType<BoundField>().ToList();
            DataTable data = gCheckinAttendanceExport.DataSourceAsDataTable;

            // three blank rows
            data.Rows.Add( data.NewRow() );
            data.Rows.Add( data.NewRow() );
            data.Rows.Add( data.NewRow() );

            // Combine Attendance Grid data and HeadCounts Grid data
            var headCountsData = gHeadcountsExport.DataSourceAsDataTable;
            var overflowRow = headCountsData.Rows[1];
            var mainRow = headCountsData.Rows[0];

            var newOverflowRow = data.NewRow();
            data.Rows.Add( newOverflowRow );

            var newMainRow = data.NewRow();
            data.Rows.Add( newMainRow );

            // copy AreaName to GroupName columns
            newOverflowRow["GroupName"] = overflowRow["Area"];
            newMainRow["GroupName"] = mainRow["Area"];

            // copy the counts columns
            foreach ( DataColumn column in headCountsData.Columns )
            {
                if ( data.Columns.OfType<DataColumn>().Any( a => a.ColumnName == column.ColumnName ) )
                {
                    newOverflowRow[column.ColumnName] = overflowRow[column.ColumnName];
                    newMainRow[column.ColumnName] = mainRow[column.ColumnName];
                }
            }

            // Totals
            DataRow totalsDataRow = data.NewRow();
            totalsDataRow["GroupName"] = "Totals";

            int grandTotalTotal = 0;

            foreach ( var col in data.Columns.OfType<DataColumn>().Where( a => a.DataType == typeof( int ) ) )
            {
                totalsDataRow[col] = data.Rows.OfType<DataRow>().Where( a => a[col] is int && a[col] != null ).Select( a => (int)a[col] ).Sum();
                if ( col.ColumnName == "GrandTotal" )
                {
                    grandTotalTotal = (int)totalsDataRow[col];
                }
            }

            data.Rows.Add( totalsDataRow );

            // two blank rows
            var firstBlankRow = data.NewRow();
            data.Rows.Add( firstBlankRow );
            data.Rows.Add( data.NewRow() );

            // PercentOfTotals
            DataRow percentOfTotalsRow = data.NewRow();
            percentOfTotalsRow["GroupName"] = "% of Total";

            foreach ( var col in data.Columns.OfType<DataColumn>().Where( a => a.ColumnName.StartsWith( "campusSummaryField_Campus" ) ) )
            {
                int campusTotal = data.Rows.OfType<DataRow>().Where( a => a != totalsDataRow && a[col] is int && a[col] != null ).Select( a => (int)a[col] ).Sum();
                if ( grandTotalTotal > 0 )
                {
                    double campusPercentOfTotal = campusTotal * 100 / (double)grandTotalTotal;
                    percentOfTotalsRow[col] = campusPercentOfTotal;
                }
            }

            data.Rows.Add( percentOfTotalsRow );

            columnCounter = 0;

            // Set up the columns 
            foreach ( DataColumn column in data.Columns )
            {
                columnCounter++;

                // Print column headings
                var gridField = gridDataFields.FirstOrDefault( a => a.DataField == column.ColumnName );
                var headerCell = worksheet.Cells[rowCounter, columnCounter];
                headerCell.Value = gridField != null ? gridField.HeaderText : column.ColumnName.SplitCase();
                if ( !column.ColumnName.Equals( "GroupName" ) )
                {
                    headerCell.Style.TextRotation = 90;
                    headerCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    headerCell.Style.WrapText = true;
                    headerCell.Style.Font.Name = "Arial Narrow";
                    headerCell.Style.Font.Size = 8;
                }

                if ( column.ColumnName.StartsWith( "campusSummaryField_Campus" ) )
                {
                    var campusId = column.ColumnName.Replace( "campusSummaryField_Campus", string.Empty ).AsInteger();

                    headerCell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    headerCell.Style.Fill.BackgroundColor.SetColor( GetExcelColorForCampus( campusId ) );
                    headerCell.Style.Font.Bold = true;
                }
                else if ( column.ColumnName.Equals( "GrandTotal" ) )
                {
                    headerCell.Style.Font.Bold = true;
                    headerCell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    headerCell.Style.Fill.BackgroundColor.SetColor( System.Drawing.ColorTranslator.FromHtml( "#C0C0C0" ) );
                }
                else
                {
                    headerCell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    headerCell.Style.Fill.BackgroundColor.SetColor( System.Drawing.ColorTranslator.FromHtml( "#C0C0C0" ) );
                }

                headerCell.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                headerCell.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                headerCell.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                headerCell.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                // Set the initial column format
                worksheet.Column( columnCounter ).Style.Numberformat.Format = ExcelHelper.DefaultColumnFormat( column.DataType );
            }

            bool fillInCampusColor = true;

            // print data
            foreach ( DataRowView rowView in data.DefaultView )
            {
                rowCounter++;

                for ( int i = 0; i < data.Columns.Count; i++ )
                {
                    var column = data.Columns[i];
                    var value = rowView.Row[i].ReverseCurrencyFormatting();
                    var dataCell = worksheet.Cells[rowCounter, i + 1];
                    ExcelHelper.SetExcelValue( dataCell, value );

                    // Update column formatting based on data
                    var worksheetColumn = worksheet.Column( i + 1 );

                    worksheetColumn.Style.Numberformat.Format = ExcelHelper.FinalColumnFormat( value, worksheetColumn.Style.Numberformat.Format );

                    dataCell.Style.Font.Name = "Book Antiqua";
                    dataCell.Style.Font.Size = 10;

                    if ( rowView.Row == totalsDataRow )
                    {
                        dataCell.Style.Font.Color.SetColor( System.Drawing.Color.Red );
                    }

                    dataCell.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    dataCell.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    dataCell.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    dataCell.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                    if ( column.ColumnName == "GroupName" )
                    {
                        if ( rowView.Row[1].ToString().Equals( "Totals" ) )
                        {
                            dataCell.Style.Font.Bold = true;
                            dataCell.Style.Font.Size = 12;
                        }

                        if ( rowView.Row[1].ToString().Equals( "% of Total" ) )
                        {
                            // nothing special, but just in case there was, this is where to put it
                        }

                        dataCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    }

                    if ( rowView.Row[0].ToString().StartsWith( "Volunteer -" ) )
                    {
                        dataCell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        dataCell.Style.Fill.BackgroundColor.SetColor( System.Drawing.ColorTranslator.FromHtml( "#C0C0C0" ) );
                        if ( column.ColumnName.Equals( "GroupName" ) )
                        {
                            ExcelHelper.SetExcelValue( dataCell, value.ToString() + " Support" );
                        }
                    }

                    if ( column.ColumnName.StartsWith( "campusSummaryField_Campus" ) )
                    {
                        if ( rowView.Row == firstBlankRow )
                        {
                            // stop filling in the campus coloring if we have gotten to the end of the Totals row
                            fillInCampusColor = false;
                        }

                        if ( fillInCampusColor )
                        {
                            var campusId = column.ColumnName.Replace( "campusSummaryField_Campus", string.Empty ).AsInteger();
                            dataCell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            dataCell.Style.Fill.BackgroundColor.SetColor( GetExcelColorForCampus( campusId ) );
                            dataCell.Style.Font.Bold = true;
                            dataCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        }
                    }
                    else if ( column.ColumnName.Equals( "GrandTotal" ) )
                    {
                        dataCell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        dataCell.Style.Fill.BackgroundColor.SetColor( System.Drawing.Color.White );
                        dataCell.Style.Font.Bold = true;
                        dataCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    }
                    else if ( column.ColumnName.StartsWith( "campusScheduleField_Campus" ) )
                    {
                        dataCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    }

                    if ( rowView.Row == percentOfTotalsRow )
                    {
                        dataCell.Style.Numberformat.Format = "0%";
                        if ( dataCell.Value is int )
                        {
                            dataCell.Value = string.Format( "{0}%", (int)dataCell.Value );
                        }
                    }
                }
            }

            // Campus Offering Excel Rows
            foreach ( var campus in CampusCache.All().OrderBy( a => a.Id ) )
            {
                rowCounter++;
                var nameCell = worksheet.Cells[rowCounter, 2];
                nameCell.Value = string.Format( "{0} Offering", campus.Name );
                nameCell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                nameCell.Style.Fill.BackgroundColor.SetColor( GetExcelColorForCampus( campus.Id ) );
                nameCell.Style.Border.BorderAround( OfficeOpenXml.Style.ExcelBorderStyle.Thin );

                var totalCell = worksheet.Cells[rowCounter, 3, rowCounter, 5];
                totalCell.Merge = true;

                // TODO: This could be populated from the database if we know which Metric these come from
                totalCell.Value = null;
                totalCell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                totalCell.Style.Fill.BackgroundColor.SetColor( GetExcelColorForCampus( campus.Id ) );
                totalCell.Style.Border.BorderAround( OfficeOpenXml.Style.ExcelBorderStyle.Thin );
            }

            // "other" offering excel row
            rowCounter++;
            var otherOfferingNameCell = worksheet.Cells[rowCounter, 2];
            otherOfferingNameCell.Value = "Other";
            otherOfferingNameCell.Style.Border.BorderAround( OfficeOpenXml.Style.ExcelBorderStyle.Thin );

            var otherOfferingTotalCell = worksheet.Cells[rowCounter, 3, rowCounter, 5];
            otherOfferingTotalCell.Merge = true;
            otherOfferingTotalCell.Value = null;
            otherOfferingTotalCell.Style.Border.BorderAround( OfficeOpenXml.Style.ExcelBorderStyle.Thin );

            // Total Offering excel row
            rowCounter++;
            var totalOfferingNameCell = worksheet.Cells[rowCounter, 2];
            totalOfferingNameCell.Value = "Total Offering";
            totalOfferingNameCell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            totalOfferingNameCell.Style.Fill.BackgroundColor.SetColor( System.Drawing.ColorTranslator.FromHtml( "#ccffff" ) );
            totalOfferingNameCell.Style.Border.BorderAround( OfficeOpenXml.Style.ExcelBorderStyle.Thin );
            totalOfferingNameCell.Style.Font.Bold = true;

            var totalOfferingTotalCell = worksheet.Cells[rowCounter, 3, rowCounter, 5];
            totalOfferingTotalCell.Merge = true;
            totalOfferingTotalCell.Value = null;
            totalOfferingTotalCell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            totalOfferingTotalCell.Style.Fill.BackgroundColor.SetColor( System.Drawing.ColorTranslator.FromHtml( "#ccffff" ) );
            totalOfferingTotalCell.Style.Border.BorderAround( OfficeOpenXml.Style.ExcelBorderStyle.Thin );
            totalOfferingTotalCell.Style.Font.Bold = true;

            // don't include GroupType in the SpreadSheet
            worksheet.DeleteColumn( 1 );

            // autofit the GroupName/Area column
            worksheet.Column( 1 ).AutoFit();

            // send the spreadsheet to the browser
            excel.SendToBrowser( this.Page, filename );
        }

        /// <summary>
        /// Gets the excel color for campus.
        /// </summary>
        /// <param name="campusId">The campus identifier.</param>
        /// <returns></returns>
        private static System.Drawing.Color GetExcelColorForCampus( int campusId )
        {
            var campus = CampusCache.Read( campusId );
            string campusHexColor = "#000000";
            if ( campus != null )
            {
                switch ( campus.ShortCode )
                {
                    case "PEO":
                        campusHexColor = "#E6B8B7";
                        break;
                    case "SUR":
                        campusHexColor = "#C4D79B";
                        break;
                    case "SCO":
                        campusHexColor = "#B7DEE8";
                        break;
                    case "EAV":
                        campusHexColor = "#FCD5B4";
                        break;
                    case "ATH":
                        campusHexColor = "#CCC0DA";
                        break;
                    case "AVD":
                        campusHexColor = "#DDD9C4";
                        break;
                    case "MID":
                        campusHexColor = "#FFC000";
                        break;
                }
            }

            return System.Drawing.ColorTranslator.FromHtml( campusHexColor );
        }

        /// <summary>
        /// Handles the GridReorder event of the gCheckinAttendanceExport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gCheckinAttendanceExport_GridReorder( object sender, GridReorderEventArgs e )
        {
            var selectedGroupIds = this.GetSelectedGroupIds();
            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );
            List<Group> sortedGroups = new List<Group>();
            int order = 0;
            foreach ( var groupId in selectedGroupIds )
            {
                var group = groupService.Get( groupId );
                group.Order = order++;
                sortedGroups.Add( group );
            }

            groupService.Reorder( sortedGroups, e.OldIndex, e.NewIndex );

            var sortedGroupIds = sortedGroups.OrderBy( a => a.Order ).Select( a => a.Id ).ToList().AsDelimited( "," );

            this.SetAttributeValue( "GroupIds", sortedGroupIds );

            SaveAttributeValues();

            BindCheckinAttendanceGrid( false );
        }
    }
}