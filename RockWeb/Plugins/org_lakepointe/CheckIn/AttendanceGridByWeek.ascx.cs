using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;


namespace RockBlocks.Plugins.org_lakepointe.Checkin
{
    [DisplayName( "Attendance Grid By Week" )]
    [Category( "LPC > Check-in" )]
    [Description( "Displays a grid of individuals who have checked in to an attendance group over a specified date range." )]
    [BooleanField( "Show Schedule Filter", "Should the Schedules Filter be displayed?", true, "Filters", 0 )]
    [BooleanField( "Show Campus Filter", "Should the Campus Filter be displayed?", true, "Filters", 1 )]
    [BooleanField( "Include Inactive Campuses", "Should campus filter include inactive campuses?", false, "Filters", 2 )]

    public partial class AttendanceGridByWeek : RockBlock
    {
        #region Fields
        RockContext _rockContext = null;
        bool? _showScheduleFilter = null;
        bool? _includeInactiveCampuses = null;
        bool? _showCampusFilter = null;
        string _checkin_GroupType_Guid = "9741518D-E832-43ED-B5F6-604949218630";
        GroupTypeCache _checkin_GroupType = null;

        #endregion

        #region Propeties
        private DataTable AttendanceData
        {
            get
            {
                var dataKey = String.Concat( "AttendanceData_", this.BlockId );
                return (DataTable)ViewState[dataKey];

            }
            set
            {
                var dataKey = String.Concat( "AttendanceData_", this.BlockId );
                ViewState[dataKey] = value;
            }
        }

        private List<DateTime> AttendedDates
        {
            get
            {
                var dataKey = String.Concat( "AttendedDates", this.BlockId );
                return (List<DateTime>)ViewState[dataKey];
            }
            set
            {
                var datakey = String.Concat( "AttendedDates", this.BlockId );
                ViewState[datakey] = value;
            }
        }

        private GroupTypeCache CheckInGroupType
        {
            get
            {
                if (_checkin_GroupType == null)
                {
                    _checkin_GroupType = GroupTypeCache.Get( _checkin_GroupType_Guid.AsGuid(), _rockContext );
                }

                return _checkin_GroupType;
            }
        }

        private bool ShowScheduleFilter
        {
            get
            {
                if ( !_showScheduleFilter.HasValue )
                {
                    _showScheduleFilter = GetAttributeValue( "ShowScheduleFilter" ).AsBoolean();
                }

                return _showScheduleFilter ?? false;
            }
        }

        private bool ShowCampusFilter
        {
            get
            {
                if ( !_showCampusFilter.HasValue )
                {
                    _showCampusFilter = GetAttributeValue( "ShowCampusFilter" ).AsBoolean();
                }

                return _showCampusFilter ?? false;
            }
        }

        private bool IncludeInactiveCampuses
        {
            get
            {
                if ( !_includeInactiveCampuses.HasValue )
                {
                    _includeInactiveCampuses = GetAttributeValue( "IncludeInactiveCampuses" ).AsBoolean();
                }
                return _includeInactiveCampuses ?? false;
            }
        }
        #endregion

        #region BaseControlMethods
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            _rockContext = new RockContext();
            gAttendees.Actions.ShowBulkUpdate = false;
            gAttendees.Actions.ShowExcelExport = true;
            gAttendees.Actions.ShowMergeTemplate = true;
            gAttendees.Actions.ShowMergePerson = false;

            gAttendees.GridRebind += gAttendees_GridRebind;
        }



        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !IsPostBack )
            {
                LoadFilters();
                LoadSettings();
                LoadAttendanceData();

                if ( AttendanceData != null )
                {
                    BindGrid();
                }
            }
        }

        #endregion

        #region Events
        protected void btnFilter_Click( object sender, EventArgs e )
        {
            UpdateUserPreferences();
            LoadAttendanceData();
            if ( AttendanceData != null )
            {
                BindGrid();
            }
        }

        protected void btnFilterSetDefault_Click( object sender, EventArgs e )
        {
            LoadSettings();
            LoadAttendanceData();
            if ( AttendanceData != null )
            {
                BindGrid();
            }
        }

        private void gAttendees_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindGrid();
        }
        protected void gtpAttendanceConfiguration_SelectedIndexChanged( object sender, EventArgs e )
        {
            ConfigureGroupPicker( gtpAttendanceConfiguration.SelectedGroupTypeId );
        }

        #endregion

        #region Methods

        private void BindGrid()
        {
            nbAttendeesError.Visible = false;
            nbAttendeesError.Title = "";
            nbAttendeesError.Text = "";

            if ( AttendanceData == null )
            {
                pnlResults.Visible = false;
                return;
            }


            var personField = gAttendees.Columns.OfType<HyperLinkField>().Where( c => c.HeaderText == "Name" ).FirstOrDefault();

            if ( personField != null )
            {
                personField.DataNavigateUrlFormatString = ( (RockPage)this.Page ).ResolveRockUrl( "~/Person/{0}" );
            }


            var currentColumns = gAttendees.Columns.OfType<RockBoundField>().Where( c => c.DataField.StartsWith( "occurrence_" ) ).ToList();
            foreach ( var column in currentColumns )
            {
                gAttendees.Columns.Remove( column );
            }

            foreach ( var date in AttendedDates )
            {
                var dateColumn = new RockBoundField();
                var columnName = string.Format( "occurrence_{0:yyyy_MM_dd}", date );

                dateColumn.DataField = columnName;
                dateColumn.HeaderText = string.Format( "{0:d}", date );
                dateColumn.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
                dateColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;

                gAttendees.Columns.Add( dateColumn );
            }

            var attendanceList = AttendanceData.AsEnumerable();

            var sortProperty = gAttendees.SortProperty;

            if ( sortProperty != null )
            {
                if ( sortProperty.Property == "GroupName" )
                {
                    if ( sortProperty.Direction == SortDirection.Ascending )
                    {
                        attendanceList = attendanceList.OrderBy( a => a.Field<string>( "GroupName" ) )
                            .ThenBy( a => a.Field<string>( "PersonLastName" ) )
                            .ThenBy( a => a.Field<string>( "PersonNickName" ) );
                    }
                    else
                    {
                        attendanceList = attendanceList.OrderByDescending( a => a.Field<string>( "GroupName" ) )
                            .ThenBy( a => a.Field<string>( "PersonLastName" ) )
                            .ThenBy( a => a.Field<string>( "PersonNickName" ) );
                    }
                }
                else if ( sortProperty.Property == "PersonLastName, PersonFirstName" )
                {
                    if ( sortProperty.Direction == SortDirection.Ascending )
                    {
                        attendanceList = attendanceList.OrderBy( a => a.Field<string>( "PersonLastName" ) )
                            .ThenBy( a => a.Field<string>( "PersonNickName" ) );
                    }
                    else
                    {
                        attendanceList = attendanceList.OrderByDescending( a => a.Field<string>( "PersonLastName" ) )
                            .ThenByDescending( a => a.Field<string>( "PersonNickName" ) );
                    }
                }
                else if ( sortProperty.Property == "AttendanceCount" )
                {
                    if ( sortProperty.Direction == SortDirection.Ascending )
                    {
                        attendanceList = attendanceList.OrderBy( a => a.Field<int>( "AttendanceCount" ) );
                    }
                    else
                    {
                        attendanceList = attendanceList.OrderByDescending( a => a.Field<int>( "AttendanceCount" ) );
                    }
                }
                else
                {
                    attendanceList = attendanceList.OrderBy( a => a.Field<string>( "PersonLastName" ) )
                        .ThenBy( a => a.Field<string>( "PersonNickName" ) );
                }

            }
            else
            {
                attendanceList = attendanceList.OrderBy( a => a.Field<string>( "PersonLastName" ) )
                    .ThenBy( a => a.Field<string>( "PersonNickName" ) );
            }

            if ( attendanceList.Count() > 0 )
            {
                AttendanceData = attendanceList.CopyToDataTable();
            }

            gAttendees.DataSource = AttendanceData;
            gAttendees.DataBind();

        }

        private void ConfigureGroupPicker( int? baseGroupTypeId )
        {
            if ( !baseGroupTypeId.HasValue )
            {
                gpGroups.IncludedGroupTypeIds = null;
                gpGroups.Enabled = false;
                return;
            }

            var groupTypeIds = new GroupTypeService( _rockContext ).GetCheckinAreaDescendants( baseGroupTypeId.Value )
                .Select( gt => gt.Id )
                .ToList();

            if (CheckInGroupType != null)
            {
                groupTypeIds.Add( CheckInGroupType.Id );
            }

            gpGroups.IncludedGroupTypeIds = groupTypeIds;
            gpGroups.Enabled = true;
        }

        private string GetSetting( string prefix, string key )
        {
            string setting = Request.QueryString[key];
            if ( setting != null )
            {
                return setting;
            }

            return this.GetUserPreference( prefix + key );

        }

        private void LoadAttendanceConfigurations()
        {
            gtpAttendanceConfiguration.Items.Clear();

            var checkInPurposeGuid = Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid();
            gtpAttendanceConfiguration.GroupTypes = new GroupTypeService( _rockContext ).Queryable()
                .Where( a => a.GroupTypePurposeValue.Guid == checkInPurposeGuid )
                .OrderBy( a => a.Name )
                .ToList();
        }

        private void LoadAttendanceData()
        {

            string absoluteLimitNote = "Using Absolute Limits.";
            var attendanceConfigurationGTId = gtpAttendanceConfiguration.SelectedValueAsInt();
            var attendanceService = new AttendanceService( _rockContext );
            var groupMemberService = new GroupMemberService( _rockContext );
            if ( !attendanceConfigurationGTId.HasValue )
            {
                pnlResults.Visible = false;
                AttendanceData = null;
                return;
            }

            pnlResults.Visible = true;
            var selectedGroupTypeIds = new GroupTypeService( _rockContext ).GetCheckinAreaDescendants( attendanceConfigurationGTId.Value )
                .Select( gt => gt.Id );

            var selectedGroupIds = gpGroups.SelectedValuesAsInt().Where( g => g > 0 ).ToList();

            var selectedCampusId = cpCampus.SelectedCampusId;
            var selectedScheduleId = spSchedule.SelectedValueAsInt();

            var startDate = dpStart.SelectedDate.Value;
            var endDate = dpEnd.SelectedDate;

            if ( !endDate.HasValue )
            {
                endDate = RockDateTime.Today.AddDays( 1 );
            }

            var personInactiveDefinedValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ).Id;
            var groupMemberQry = groupMemberService.Queryable().AsNoTracking()
                .Where( gm => gm.GroupMemberStatus == GroupMemberStatus.Active )
                .Where( gm => gm.Person.RecordStatusValueId != personInactiveDefinedValueId );

            var attendanceQry = attendanceService.Queryable().AsNoTracking()
                .Where( a => a.DidAttend.HasValue && a.DidAttend.Value )
                .Where( a => a.StartDateTime >= startDate && a.StartDateTime <= endDate );

            if ( selectedGroupIds.Count > 0 )
            {
                groupMemberQry = groupMemberQry.Where( gm => selectedGroupIds.Contains( gm.GroupId ) );

            }
            else
            {
                groupMemberQry = groupMemberQry.Where( gm => selectedGroupTypeIds.Contains( gm.Group.GroupTypeId ) );
            }

            attendanceQry = attendanceQry.Where( a => selectedGroupTypeIds.Contains( a.Occurrence.Group.GroupTypeId ) );

            if ( selectedCampusId.HasValue )
            {
                groupMemberQry = groupMemberQry.Where( gm => gm.Group.CampusId == selectedCampusId.Value || gm.Group.CampusId == null );
                attendanceQry = attendanceQry.Where( a => a.CampusId == selectedCampusId.Value );
            }

            if ( selectedScheduleId.HasValue )
            {
                attendanceQry = attendanceQry.Where( a => a.Occurrence.ScheduleId == selectedScheduleId.Value );
            }

            var attendanceData = attendanceQry
                .Select( a => new
                {
                    AttendanceId = a.Id,
                    PersonId = a.PersonAlias.PersonId,
                    DateAttended = a.StartDateTime,
                    AttendanceNote = a.Note,
                    a.Occurrence.GroupId,
                    a.Occurrence.SundayDate

                } )
                .ToList();

            AttendedDates = attendanceData.Select( a => a.DateAttended.Date ).Distinct().OrderBy( d => d ).ToList();

            var groupMemberData = groupMemberQry
                .Select( gm => new
                {
                    GroupMemberId = gm.Id,
                    GroupTypeId = gm.Group.GroupTypeId,
                    GroupId = gm.GroupId,
                    GroupName = gm.Group.Name,
                    Person = gm.Person
                } )
                .ToList();

            var groupMemberAttendance = groupMemberData
                .Join( attendanceData,
                    groupMember => groupMember.Person.Id,
                    attendance => attendance.PersonId,
                    ( gm, a ) => new
                    {
                        GroupMember = gm,
                        Attendance = a
                    } )
                .GroupBy( a => a.GroupMember.GroupMemberId )
                .Select( ag => new
                {
                    GroupMemberId = ag.Key,
                    Person = ag.First().GroupMember.Person,
                    GroupTypeId = ag.First().GroupMember.GroupTypeId,
                    GroupId = ag.First().GroupMember.GroupId,
                    GroupName = ag.First().GroupMember.GroupName,
                    Attendance = ag.Select( ag1 => ag1.Attendance )
                } )
                .ToList();

            DataTable dt = new System.Data.DataTable();
            dt.Columns.Add( new DataColumn( "GroupId", typeof( int ) ) );
            dt.Columns.Add( new DataColumn( "GroupName", typeof( string ) ) );
            dt.Columns.Add( new DataColumn( "GroupMemberId", typeof( int ) ) );
            dt.Columns.Add( new DataColumn( "PersonId", typeof( int ) ) );
            dt.Columns.Add( new DataColumn( "PersonLastName", typeof( string ) ) );
            dt.Columns.Add( new DataColumn( "PersonNickName", typeof( string ) ) );
            dt.Columns.Add( new DataColumn( "PersonName", typeof( string ) ) );
            dt.Columns.Add( new DataColumn( "Birthdate", typeof( DateTime ) ) );
            dt.Columns.Add( new DataColumn( "Age", typeof( int ) ) );
            dt.Columns.Add( new DataColumn( "Gender", typeof( string ) ) );
            dt.Columns.Add( new DataColumn( "ParentsName", typeof( string ) ) );
            dt.Columns.Add( new DataColumn( "AttendanceCount", typeof( int ) ) );
            dt.Columns.Add( new DataColumn( "MainPhone", typeof( string ) ) );

            var familyGroupType = GroupTypeCache.GetFamilyGroupType();
            var adultRole = familyGroupType.Roles.SingleOrDefault( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() );
            var childRole = familyGroupType.Roles.SingleOrDefault( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() );
            var mainPhoneType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME );
            // add date columns
            foreach ( var date in AttendedDates )
            {
                var columnName = string.Format( "occurrence_{0:yyyy_MM_dd}", date );
                dt.Columns.Add( new DataColumn( columnName, typeof( string ) ) );
            }

            var personService = new PersonService( _rockContext );

            foreach ( var gma in groupMemberAttendance )
            {
                DataRow dr = dt.NewRow();
                dr["GroupId"] = gma.GroupId;
                dr["GroupName"] = gma.GroupName;
                dr["GroupMemberId"] = gma.GroupMemberId;
                dr["PersonId"] = gma.Person.Id;
                dr["PersonLastName"] = gma.Person.LastName;
                dr["PersonNickName"] = gma.Person.NickName;
                dr["PersonName"] = gma.Person.FullName;
                dr["Birthdate"] = (object)gma.Person.BirthDate ?? DBNull.Value;
                dr["Gender"] = gma.Person.Gender;
                dr["Age"] = gma.Person.Age ?? -1;
                dr["AttendanceCount"] = gma.Attendance.GroupBy( a => a.DateAttended.Date ).Count();

                var familyMembers = personService.GetFamilyMembers( gma.Person.Id, true ).ToList();
                if ( familyMembers.Count( fm => fm.PersonId == gma.Person.Id && fm.GroupRoleId == childRole.Id ) > 0 )
                {
                    dr["ParentsName"] = familyMembers
                        .Where( fm => fm.GroupRoleId == adultRole.Id )
                        .OrderBy( fm => fm.Person.Gender )
                        .Select( fm => fm.Person.FullName )
                        .ToList().AsDelimited( ", " );
                }

                DateTime firstVisitDate = attendanceService.Queryable()
                 .AsNoTracking()
                 .Where( a =>
                     a.Occurrence.Group != null &&
                     a.Occurrence.Group.GroupTypeId == gma.GroupTypeId &&
                     a.PersonAlias.PersonId == gma.Person.Id &&
                     a.DidAttend == true )
                 .OrderBy( a => a.StartDateTime )
                 .Select( a => a.StartDateTime )
                 .FirstOrDefault();



                var mainPhoneNumber = personService.GetPhoneNumber( gma.Person, mainPhoneType );
                if ( mainPhoneNumber != null )
                {
                    dr["MainPhone"] = mainPhoneNumber.NumberFormatted;
                }

                foreach ( var a in gma.Attendance )
                {

                    string columnName = string.Format( "occurrence_{0:yyyy_MM_dd}", a.DateAttended );
                    string value = null;
                    if ( firstVisitDate != DateTime.MinValue && firstVisitDate.Date == a.DateAttended.Date )
                    {
                        value = "F";
                    }
                    else
                    {
                        value = "X";
                    }


                    if ( !String.IsNullOrWhiteSpace( a.AttendanceNote ) && a.AttendanceNote.Equals( absoluteLimitNote, StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        value = value + "*";
                    }

                    dr[columnName] = value;
                }
                dt.Rows.Add( dr );
            }

            AttendanceData = dt;

        }


        private void LoadCampuses()
        {
            cpCampus.Items.Clear();
            pnlCampus.Visible = ShowCampusFilter;
            if ( ShowCampusFilter )
            {
                cpCampus.DataSource = CampusCache.All( IncludeInactiveCampuses )
                    .OrderBy( c => c.Name )
                    .ToList();
            }
        }

        private void LoadFilters()
        {
            LoadAttendanceConfigurations();
            LoadCampuses();
            ShowSchedules();
        }

        private void LoadSettings()
        {
            string keyPrefix = string.Format( "attendance-grid-By-week-{0}", this.BlockId );
            int? templateGroupTypeId = GetSetting( keyPrefix, "TemplateGroupTypeId" ).AsIntegerOrNull();

            if ( templateGroupTypeId.HasValue )
            {
                gtpAttendanceConfiguration.SelectedGroupTypeId = templateGroupTypeId.Value;
                ConfigureGroupPicker( templateGroupTypeId.Value );
                gpGroups.Enabled = true;

                string selectedGroupIds = GetSetting( keyPrefix, "GroupIds" );

                if ( selectedGroupIds.IsNotNullOrWhiteSpace() )
                {
                    var groupIdList = selectedGroupIds.SplitDelimitedValues( false )
                        .Select( g => g.AsInteger() )
                        .Where( g => g > 0 )
                        .ToList();
                    gpGroups.SetValues( groupIdList );
                }
            }
            else
            {
                gtpAttendanceConfiguration.SelectedGroupTypeId = null;
                gpGroups.Enabled = false;
            }

            if ( ShowCampusFilter )
            {
                int? campusId = GetSetting( keyPrefix, "CampusId" ).AsIntegerOrNull();

                cpCampus.SelectedCampusId = campusId;
            }
            if ( ShowScheduleFilter )
            {
                int? scheduleId = GetSetting( keyPrefix, "ScheduleId" ).AsIntegerOrNull();
                if ( scheduleId.HasValue )
                {
                    spSchedule.SetValue( scheduleId.Value );

                }
            }
            DateTime? selectedStartDate = GetSetting( keyPrefix, "StartDate" ).AsDateTime();
            DateTime? selectedEndDate = GetSetting( keyPrefix, "EndDate" ).AsDateTime();


            if ( selectedStartDate.HasValue )
            {
                dpStart.SelectedDate = selectedStartDate.Value;
            }
            else
            {
                dpStart.SelectedDate = RockDateTime.Today;
            }

            if ( selectedEndDate.HasValue )
            {
                dpEnd.SelectedDate = selectedEndDate.Value;
            }
            else
            {
                dpEnd.SelectedDate = RockDateTime.Today.AddDays( 1 );
            }


        }

        private void ShowSchedules()
        {
            pnlSchedule.Visible = ShowScheduleFilter;

            if ( !ShowScheduleFilter )
            {
                spSchedule.SetValue( null );
            }
        }


        private void UpdateUserPreferences()
        {
            string keyPrefix = string.Format( "attendance-grid-By-week-{0}", this.BlockId );
            this.SetUserPreference( keyPrefix + "TemplateGroupTypeId", gtpAttendanceConfiguration.SelectedValue, false );
            this.SetUserPreference( keyPrefix + "GroupIds", gpGroups.SelectedValues.ToList().AsDelimited( "," ), false );
            this.SetUserPreference( keyPrefix + "CampusId", cpCampus.SelectedValue, false );
            this.SetUserPreference( keyPrefix + "ScheduleId", spSchedule.SelectedValue, false );
            this.SetUserPreference( keyPrefix + "StartDate", string.Format( "{0:g}", dpStart.SelectedDate ), false );
            this.SetUserPreference( keyPrefix + "EndDate", string.Format( "{0:g}", dpEnd.SelectedDate ), false );

            this.SaveUserPreferences( keyPrefix );

        }
        #endregion




    }
}