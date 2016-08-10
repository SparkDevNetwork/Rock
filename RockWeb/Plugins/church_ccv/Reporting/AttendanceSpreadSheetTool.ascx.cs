using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
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

    [IntegerField( "MetricCategoryId", Category = "CustomSetting" )]
    public partial class AttendanceSpreadSheetTool : RockBlockCustomSettings
    {
        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gList.GridRebind += gList_GridRebind;

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

            var scheduleIdList = this.GetBlockUserPreference( "ScheduleIds" ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsIntegerList();
            spSchedules.SetValues( scheduleIdList );

            var formatString = "dddd MMM d, yyyy";
            ddlSundayDate.Items.Clear();
            while ( lastSundayDate > RockDateTime.Today.AddMonths( -6 ) )
            {
                ddlSundayDate.Items.Add( new ListItem( lastSundayDate.ToString( formatString ), lastSundayDate.ToString() ) );
                lastSundayDate = lastSundayDate.AddDays( -7 );
            }

            var groupIdList = this.GetBlockUserPreference( "GroupIds" ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();

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

            BindGrid();
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

            mpMetric.SetValue( this.GetAttributeValue( "MetricCategoryId" ).AsIntegerOrNull() );
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
        /// Gets the selected group ids.
        /// </summary>
        /// <returns></returns>
        private List<int> GetSelectedGroupIds()
        {
            var selectedGroupIds = new List<int>();
            var checkboxListControls = rptGroupTypes.ControlsOfTypeRecursive<RockCheckBoxList>();
            foreach ( var cblGroup in checkboxListControls )
            {
                selectedGroupIds.AddRange( cblGroup.SelectedValuesAsInt );
            }

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
        private void gList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Methods

        private List<int> _selectedScheduleIds = null;

        private void AddScheduleColumns( Metric metric, DateTime sundayDate )
        {
            var rockContext = new RockContext();

            // clear out any existing schedule columns and add the ones that match the current filter setting
            gList.Columns.Clear();
            gList.Columns.Add( new RockBoundField { DataField = "GroupName", HeaderText = "Worship" } );

            var groupIds = this.GetSelectedGroupIds();
            _selectedScheduleIds = spSchedules.SelectedValuesAsInt().ToList();
            var scheduleList = new ScheduleService( rockContext ).Queryable().Where( a => a.Name != null ).ToList().Select( a => new
            {
                a.Id,
                a.FriendlyScheduleText
            } );


            var entityTypeIdSchedule = EntityTypeCache.Read<Schedule>().Id;
            var entityTypeIdGroup = EntityTypeCache.Read<Group>().Id;
            var selectedGroupIds = this.GetSelectedGroupIds();

            var campuses = CampusCache.All( false );
            foreach ( var campus in campuses.OrderBy( a => a.Id ) )
            {
                var campusServiceTimes = campus.ServiceTimes;

                // add all the advertised schedules first
                foreach ( var serviceTime in campusServiceTimes )
                {
                    var serviceTimeFriendlyText = string.Format( "{0} at {1}", serviceTime.Day, serviceTime.Time ).Replace( "*", "" ).Trim();
                    var schedule = scheduleList.FirstOrDefault( a => a.FriendlyScheduleText.StartsWith( serviceTimeFriendlyText, StringComparison.OrdinalIgnoreCase ) );
                    if ( schedule != null && _selectedScheduleIds.Contains( schedule.Id ) )
                    {
                        string campusScheduleFieldName = string.Format( "campusScheduleField_Campus{0}_Schedule{1}", campus.Id, schedule.Id );

                        var headerText = string.Format( "{0} - {1}", campus.Name, schedule.FriendlyScheduleText );
                        RockBoundField campusScheduleField = new RockBoundField { HeaderText = headerText, DataField = campusScheduleFieldName };
                        gList.Columns.Add( campusScheduleField );
                    }
                }

                // look up all the schedules that were used and also add those if they aren't there already
                var metricCampusScheduleIds = new MetricValueService( rockContext ).Queryable()
                    .Where( a => a.MetricId == metric.Id && a.MetricValueDateTime == sundayDate )
                    .Where( a => selectedGroupIds.Contains( a.MetricValuePartitions.FirstOrDefault( x => x.MetricPartition.EntityTypeId == entityTypeIdGroup ).EntityId ?? 0 ) )
                    .SelectMany( a => a.MetricValuePartitions )
                    .Where( a => a.MetricPartition.EntityTypeId == entityTypeIdSchedule )
                    .Select( a => a.EntityId ).Distinct().ToList();

                var metricCampusSchedules = scheduleList.Where( a => metricCampusScheduleIds.Contains( a.Id ) && _selectedScheduleIds.Contains( a.Id ) );
                foreach ( var schedule in metricCampusSchedules )
                {
                    string campusScheduleFieldName = string.Format( "campusScheduleField_Campus{0}_Schedule{1}", campus.Id, schedule.Id );
                    if ( !gList.Columns.OfType<RockBoundField>().Any( a => a.DataField == campusScheduleFieldName ) )
                    {
                        var headerText = string.Format( "{0} - {1}", campus.Name, schedule.FriendlyScheduleText );
                        RockBoundField campusScheduleField = new RockBoundField { HeaderText = headerText, DataField = campusScheduleFieldName };
                        gList.Columns.Add( campusScheduleField );
                    }
                }

                string campusSummaryFieldName = string.Format( "campusSummaryField_Campus{0}", campus.Id );
                RockBoundField campusSummaryField = new RockBoundField { HeaderText = campus.Name, DataField = campusSummaryFieldName };
                gList.Columns.Add( campusSummaryField );
            }

            gList.Columns.Add( new RockBoundField { DataField = "GrandTotal", HeaderText = "Grand Total" } );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            RockContext rockContext = new RockContext();
            var metricCategoryService = new MetricCategoryService( rockContext );

            Metric metric = null;
            var metricCategoryId = this.GetAttributeValue( "MetricCategoryId" ).AsIntegerOrNull();
            if ( metricCategoryId.HasValue )
            {
                var metricCategory = metricCategoryService.Get( metricCategoryId.Value );
                if ( metricCategory != null )
                {
                    metric = metricCategory.Metric;
                }
            }

            if ( metric == null )
            {
                nbMetricWarning.Visible = true;
                return;
            }
            else
            {
                nbMetricWarning.Visible = false;
            }

            var sundayDate = ddlSundayDate.SelectedValue.AsDateTime();
            AddScheduleColumns( metric, sundayDate.Value );

            var selectedGroupIds = this.GetSelectedGroupIds();

            var entityTypeIdGroup = EntityTypeCache.Read<Group>().Id;
            var entityTypeIdCampus = EntityTypeCache.Read<Campus>().Id;
            var entityTypeIdSchedule = EntityTypeCache.Read<Schedule>().Id;
            var metricValueService = new MetricValueService( rockContext );
            var metricValuesQuery = metricValueService.Queryable()
                .Where( a => a.MetricId == metric.Id && a.MetricValueDateTime == sundayDate )
                .Where( a => selectedGroupIds.Contains( a.MetricValuePartitions.FirstOrDefault( x => x.MetricPartition.EntityTypeId == entityTypeIdGroup ).EntityId ?? 0 ) )
                .Where( a => _selectedScheduleIds.Contains( a.MetricValuePartitions.FirstOrDefault( x => x.MetricPartition.EntityTypeId == entityTypeIdSchedule ).EntityId ?? 0 ) )
                .Include( a => a.MetricValuePartitions );

            var scheduleLookup = new ScheduleService( rockContext ).Queryable().Where( a => a.Name != null ).ToList();

            var list = metricValuesQuery.ToList();
            var groupService = new GroupService( rockContext );
            var groupAttendanceMetricsList = new List<GroupAttendanceMetrics>();

            // display grid in the same order that the Groups are displayed
            foreach ( var groupId in selectedGroupIds )
            {
                var group = groupService.Get( groupId );
                var groupMetricValues = list.Where( a => a.MetricValuePartitions.FirstOrDefault( x => x.MetricPartition.EntityTypeId == entityTypeIdGroup ).EntityId == groupId ).ToList();
                groupAttendanceMetricsList.Add( new GroupAttendanceMetrics { Group = group, MetricValues = groupMetricValues } );
            }

            DataTable dataTable = new DataTable( "AttendanceExportData" );
            foreach ( var boundField in gList.Columns.OfType<RockBoundField>() )
            {
                DataColumn dataColumn = new DataColumn( boundField.DataField );
                if ( boundField.DataField == "GroupName" )
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
                    if ( dataColumn.ColumnName == "GroupName" )
                    {
                        dataRow[dataColumn] = groupAttendanceMetrics.Group.Name;
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
                            _selectedScheduleIds.Contains( a.MetricValuePartitions.FirstOrDefault( x => x.MetricPartition.EntityTypeId == entityTypeIdSchedule ).EntityId ?? 0 ) ).ToList();

                        dataRow[dataColumn] = (int)campusSummaryValues.Sum( a => a.YValue ?? 0.00M );
                    }
                    else if ( dataColumn.ColumnName == "GrandTotal" )
                    {
                        dataRow[dataColumn] = (int)groupAttendanceMetrics.MetricValues.Where( a => _selectedScheduleIds.Contains( a.MetricValuePartitions.FirstOrDefault( x => x.MetricPartition.EntityTypeId == entityTypeIdSchedule ).EntityId ?? 0 ) ).Sum( a => a.YValue ?? 0.00M );
                    }
                }

                dataTable.Rows.Add( dataRow );
            }

            gList.DataSource = dataTable;
            gList.DataBind();
        }

        #endregion

        /// <summary>
        /// Handles the Click event of the btnUpdate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnUpdate_Click( object sender, EventArgs e )
        {
            var selectedGroupIds = GetSelectedGroupIds();
            this.SetBlockUserPreference( "GroupIds", selectedGroupIds.AsDelimited( "," ), true );
            this.SetBlockUserPreference( "ScheduleIds", spSchedules.SelectedValues.ToList().AsDelimited( "," ), true );

            BindGrid();
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
            this.SetAttributeValue( "MetricCategoryId", mpMetric.SelectedValue );
            SaveAttributeValues();

            this.Block_BlockUpdated( sender, e );
        }

        private class GroupAttendanceMetrics
        {
            public Group Group { get; set; }
            public List<MetricValue> MetricValues { get; set; }
        }
    }
}