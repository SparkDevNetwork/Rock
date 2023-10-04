using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.SqlClient;
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

namespace RockBlocks.Plugins.org_lakepointe.Checkin
{
    [DisplayName( "Attendance By Timestamp" )]
    [Category( "LPC > Check-in" )]
    [Description( "Displays who was checked in to a group at a specific date and time." )]
    [BooleanField( "Show Schedule Filter", "Should the Schedules Filter be displayed?", true, "Filters", 0 )]
    [BooleanField( "Show Campus Filter", "Should the Campus Filter be displayed?", true, "Filters", 1 )]
    [BooleanField( "Include Inactive Campuses", "Should campus filter include inactive campuses?", false, "Filters", 2 )]
    [BooleanField( "Show Bulk Update Option", "Should the Bulk Update option be allowed from the attendance grid?", true, "", 11 )]

    public partial class AttendanceReportingByTimestamp : RockBlock
    {
        #region Fields
        private RockContext _rockContext = null;
        private bool? _showCampus = null;
        private bool? _includeInactiveCampuses = null;
        private bool? _showSchedule = null;
        private bool? _showBulkUpdate = null;
        private string _checkin_GroupType_Guid = "9741518D-E832-43ED-B5F6-604949218630";
        private GroupTypeCache _checkin_GroupType = null;

        #endregion

        #region Properties
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
        private bool IncludeInactiveCampuses
        {
            get
            {
                if ( _includeInactiveCampuses == null )
                {
                    _includeInactiveCampuses = GetAttributeValue( "IncludeInactiveCampuses" ).AsBoolean();
                }

                return _includeInactiveCampuses ?? false;
            }
        }

        private bool ShowBulkUpdate
        {
            get
            {
                if ( _showBulkUpdate == null )
                {
                    _showBulkUpdate = GetAttributeValue( "ShowBulkUpdateOption" ).AsBoolean();
                }

                return _showBulkUpdate ?? false;
            }
        }

        private bool ShowCampus
        {
            get
            {
                if ( _showCampus == null )
                {
                    _showCampus = GetAttributeValue( "ShowCampusFilter" ).AsBoolean();
                }

                return _showCampus ?? false;
            }
        }

        private bool ShowSchedule
        {
            get
            {
                if ( _showSchedule == null )
                {
                    _showSchedule = GetAttributeValue( "ShowScheduleFilter" ).AsBoolean();
                }
                return _showSchedule ?? false;
            }
        }
        #endregion

        #region Base Control Methods
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            _rockContext = new RockContext();
            _rockContext.Database.CommandTimeout = 120;

            gAttendees.GridRebind += GAttendees_GridRebind;
            gAttendees.Actions.ShowBulkUpdate = ShowBulkUpdate;
            gAttendees.Actions.ShowMergePerson = true;
            gAttendees.Actions.ShowMergeTemplate = true;
            gAttendees.PersonIdField = "PersonId";
            gAttendees.DataKeyNames = new string[] { "AttendanceId", "PersonId" };
        }



        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                LoadFilters();
                try
                {
                    LoadSettings();
                    LoadGrid();
                }
                catch ( Exception ex )
                {
                    LogAndShowException( ex );
                }
            }
        }
        #endregion

        #region Events
        protected void btnFilter_Click( object sender, EventArgs e )
        {
            UpdateUserPreferences();
            LoadGrid();
        }

        protected void btnFilterSetDefault_Click( object sender, EventArgs e )
        {
            LoadSettings();
            LoadGrid();
        }

        private void GAttendees_GridRebind( object sender, GridRebindEventArgs e )
        {
            LoadGrid();
        }

        protected void gtpAttendanceConfiguration_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( gtpAttendanceConfiguration.SelectedGroupTypeId.HasValue )
            {
                LoadGroupsByGroupType( gtpAttendanceConfiguration.SelectedGroupTypeId.Value );
                gpGroups.Enabled = true;
            }
            else
            {
                gpGroups.SetValue( null );
                gpGroups.Enabled = false;
            }

        }
        #endregion

        #region Methods

        private void LoadAttendanceCategories()
        {
            gtpAttendanceConfiguration.Items.Clear();

            var checkInPurposeGuid = Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid();
            gtpAttendanceConfiguration.GroupTypes = new GroupTypeService( _rockContext ).Queryable()
                .Where( a => a.GroupTypePurposeValue.Guid == checkInPurposeGuid )
                .OrderBy( a => a.Name ).ToList();
        }

        private void LoadCampuses()
        {
            cpCampus.Items.Clear();

            if ( ShowCampus )
            {
                pnlCampus.Visible = true;
                cpCampus.Campuses = CampusCache.All( IncludeInactiveCampuses )
                    .OrderBy( c => c.Name )
                    .ToList();
            }
            else
            {
                pnlCampus.Visible = false;
            }
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


        private void LoadFilters()
        {
            LoadAttendanceCategories();
            LoadCampuses();
            pnlSchedule.Visible = ShowSchedule;
        }

        private void LoadSettings()
        {
            string keyPrefix = string.Format( "attendance-byTimestamp-{0}", this.BlockId );
            int? templateGroupTypeId = GetSetting( keyPrefix, "TemplateGroupTypeId" ).AsIntegerOrNull();

            if ( templateGroupTypeId.HasValue )
            {
                gtpAttendanceConfiguration.SelectedGroupTypeId = templateGroupTypeId.Value;
                LoadGroupsByGroupType( templateGroupTypeId.Value );
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

            int? campusId = GetSetting( keyPrefix, "CampusId" ).AsIntegerOrNull();
            cpCampus.SelectedCampusId = campusId;

            int? scheduleId = GetSetting( keyPrefix, "ScheduleId" ).AsIntegerOrNull();
            if ( scheduleId.HasValue )
            {
                spSchedule.SetValue( scheduleId.Value );

            }

            DateTime? selectedStartDate = GetSetting( keyPrefix, "StartDate" ).AsDateTime();
            DateTime? selectedEndDate = GetSetting( keyPrefix, "EndDate" ).AsDateTime();


            if ( selectedStartDate.HasValue )
            {
                dtpStart.SelectedDateTime = selectedStartDate.Value;
            }
            else
            {
                dtpStart.SelectedDateTime = RockDateTime.Today;
            }

            if ( selectedEndDate.HasValue )
            {
                dtpEnd.SelectedDateTime = selectedEndDate.Value;
            }
            else
            {
                dtpEnd.SelectedDateTime = RockDateTime.Today.AddDays( 1 );
            }

            bool showRollover = GetSetting( keyPrefix, "ShowRollover" ).AsBoolean();

            if ( showRollover )
            {
                hfHardStop.Value = "1";
                if ( !btnHardStopFlagYes.CssClass.Contains( "active" ) )
                {
                    btnHardStopFlagYes.CssClass = btnHardStopFlagYes.CssClass + " active";
                }
                btnHardStopFlagNo.CssClass = btnHardStopFlagNo.CssClass.Replace( "active", "" );
            }
            else
            {
                hfHardStop.Value = "0";
                if ( !btnHardStopFlagNo.CssClass.Contains( "active" ) )
                {
                    btnHardStopFlagNo.CssClass = btnHardStopFlagNo.CssClass + " active";
                }
                btnHardStopFlagYes.CssClass = btnHardStopFlagYes.CssClass.Replace( "active", "" );
            }

        }
        private void LoadGroupsByGroupType( int groupTypeId )
        {
            var groupTypeService = new GroupTypeService( _rockContext );
            List<int> groupTypeIds = new List<int>();
            groupTypeIds.Add( groupTypeId );
            groupTypeIds.AddRange( groupTypeService.GetCheckinAreaDescendants( groupTypeId ).Select( gt => gt.Id ) );

            if (CheckInGroupType != null)
            {
                groupTypeIds.Add( CheckInGroupType.Id );
            }

            gpGroups.IncludedGroupTypeIds = groupTypeIds;


            gpGroups.DataBind();

        }

        private void LoadGrid()
        {
            nbAttendeesError.Visible = false;
            nbAttendeesError.Text = null;
            nbAttendeesError.Title = null;

            var personUrlFormatString = ( (RockPage)this.Page ).ResolveRockUrl( "~/Person/{0}" );

            var personHyperLinkField = gAttendees.Columns.OfType<HyperLinkField>().FirstOrDefault( a => a.HeaderText == "Name" );
            if ( personHyperLinkField != null )
            {
                personHyperLinkField.DataNavigateUrlFormatString = personUrlFormatString;
            }
            var checkedInByLinkField = gAttendees.Columns.OfType<HyperLinkField>().FirstOrDefault( a => a.HeaderText == "Check In By" );
            if ( checkedInByLinkField != null )
            {
                checkedInByLinkField.DataNavigateUrlFormatString = personUrlFormatString;
            }


            var checkedOutByLinkField = gAttendees.Columns.OfType<HyperLinkField>().FirstOrDefault( a => a.HeaderText == "Check Out By" );
            if ( checkedOutByLinkField != null )
            {
                checkedOutByLinkField.DataNavigateUrlFormatString = personUrlFormatString;
            }

            var attendanceConfigurationId = gtpAttendanceConfiguration.SelectedGroupTypeId;

            if ( !attendanceConfigurationId.HasValue || dtpStart.SelectedDateTimeIsBlank )
            {
                pnlResults.Visible = false;
                return;
            }

            var selectedGroupIds = gpGroups.SelectedValuesAsInt().Where(g => g > 0).ToList();

            List<int> selectedGroupTypeIds = new List<int>();
            if ( selectedGroupIds.Count == 0 )
            {
                selectedGroupTypeIds = new GroupTypeService( _rockContext )
                    .GetCheckinAreaDescendants( attendanceConfigurationId.Value )
                    .Select( gt => gt.Id )
                    .ToList();
                selectedGroupTypeIds.Add( attendanceConfigurationId.Value );
            }


            var campusId = cpCampus.SelectedCampusId;
            var scheduleId = spSchedule.SelectedValueAsInt();
            var searchStartTime = dtpStart.SelectedDateTime.Value;
            var minDateTime = searchStartTime.Date;  //
            var searchEndTime = dtpEnd.SelectedDateTime ?? RockDateTime.Now;

            var rolloverDisplayColumn = gAttendees.Columns.OfType<RockTemplateField>().FirstOrDefault( a => a.HeaderText == "Rollover" );
            var rolloverReportColumn = gAttendees.Columns.OfType<RockBoundField>().FirstOrDefault( a => a.HeaderText == "Is Rollover" );
            if ( hfHardStop.Value.Equals( "1", StringComparison.InvariantCultureIgnoreCase ) )
            {
                if ( rolloverDisplayColumn != null )
                {
                    rolloverDisplayColumn.Visible = true;
                }
                if ( rolloverReportColumn != null )
                {
                    rolloverReportColumn.ExcelExportBehavior = ExcelExportBehavior.AlwaysInclude;
                }
            }
            else
            {
                if ( rolloverDisplayColumn != null )
                {
                    rolloverDisplayColumn.Visible = false;
                }
                if ( rolloverReportColumn != null )
                {
                    rolloverReportColumn.ExcelExportBehavior = ExcelExportBehavior.NeverInclude;
                }
            }

            var attendanceResultsQry = _rockContext.Database.SqlQuery<AttendanceByTimeStampResult>( "[dbo].[_org_lakepointe_spCheckin_AttendanceReportByDateTime] @StartDateTime, @EndDateTime, @CampusId, @ScheduleId, @GroupTypeIds, @GroupIds",
                    new SqlParameter( "@StartDateTime", searchStartTime ),
                    new SqlParameter( "@EndDateTime", searchEndTime ),
                    new SqlParameter( "@CampusId", campusId.HasValue ? (object)campusId.Value : DBNull.Value ),
                    new SqlParameter( "@ScheduleId", scheduleId.HasValue ? (object)scheduleId : DBNull.Value ),
                    new SqlParameter( "@GroupTypeIds", string.Join( ",", selectedGroupTypeIds ) ),
                    new SqlParameter( "@GroupIds", string.Join( ",", selectedGroupIds ) ) ).AsQueryable();

            var sortProperty = gAttendees.SortProperty;
            if ( sortProperty != null )
            {
                if ( sortProperty.Property.Equals( "AttendedPersonLastName, AttendedPersonNickName", StringComparison.InvariantCultureIgnoreCase ) )
                {
                    if ( sortProperty.Direction == SortDirection.Ascending )
                    {
                        attendanceResultsQry = attendanceResultsQry.OrderBy( a => a.AttendedPersonLastName ).ThenBy( a => a.AttendedPersonNickName );
                    }
                    else
                    {
                        attendanceResultsQry = attendanceResultsQry.OrderByDescending( a => a.AttendedPersonLastName ).ThenByDescending( a => a.AttendedPersonNickName );
                    }
                }
                else if ( sortProperty.Property.Equals( "CheckInGroupName", StringComparison.InvariantCultureIgnoreCase ) )
                {
                    if ( sortProperty.Direction == SortDirection.Ascending )
                    {
                        attendanceResultsQry = attendanceResultsQry.OrderBy( a => a.CheckInGroupName )
                            .ThenBy( a => a.CheckedInByPersonLastName ).ThenBy( a => a.CheckedInByPersonNickName );
                    }
                    else
                    {
                        attendanceResultsQry = attendanceResultsQry.OrderByDescending( a => a.CheckInGroupName )
                            .ThenBy( a => a.CheckedInByPersonLastName ).ThenBy( a => a.CheckedInByPersonNickName );
                    }
                }
                else if ( sortProperty.Property.Equals( "CheckInLocationName", StringComparison.InvariantCultureIgnoreCase ) )
                {
                    if ( sortProperty.Direction == SortDirection.Ascending )
                    {
                        attendanceResultsQry = attendanceResultsQry.OrderBy( a => a.CheckInLocationName )
                            .ThenBy( a => a.CheckedInByPersonLastName ).ThenBy( a => a.CheckedInByPersonNickName );
                    }
                    else
                    {
                        attendanceResultsQry = attendanceResultsQry.OrderByDescending( a => a.CheckInLocationName )
                            .ThenBy( a => a.CheckedInByPersonLastName ).ThenBy( a => a.CheckedInByPersonNickName );
                    }
                }
                else if ( sortProperty.Property.Equals( "StartDateTime" ) )
                {
                    if ( sortProperty.Direction == SortDirection.Ascending )
                    {
                        attendanceResultsQry = attendanceResultsQry.OrderBy( a => a.StartDateTime );
                    }
                    else
                    {
                        attendanceResultsQry = attendanceResultsQry.OrderByDescending( a => a.StartDateTime );
                    }
                }
                else if ( sortProperty.Property.Equals( "EndDateTimeSortValue" ) )
                {
                    if ( sortProperty.Direction == SortDirection.Ascending )
                    {
                        attendanceResultsQry = attendanceResultsQry.OrderBy( a => a.EndDateTimeSortValue );
                    }
                    else
                    {
                        attendanceResultsQry = attendanceResultsQry.OrderByDescending( a => a.EndDateTimeSortValue );
                    }
                }
                else if ( sortProperty.Property.Equals( "IsRollover" ) )
                {
                    if ( sortProperty.Direction == SortDirection.Ascending )
                    {
                        attendanceResultsQry = attendanceResultsQry.OrderBy( a => a.IsRollover )
                            .ThenBy( a => a.CheckedInByPersonLastName ).ThenBy( a => a.CheckedInByPersonNickName );
                    }
                    else
                    {
                        attendanceResultsQry = attendanceResultsQry.OrderByDescending( a => a.IsRollover )
                            .ThenBy( a => a.CheckedInByPersonLastName ).ThenBy( a => a.CheckedInByPersonNickName );
                    }
                }
            }

            else
            {
                attendanceResultsQry = attendanceResultsQry.OrderBy( a => a.StartDateTime )
                    .ThenBy( a => a.CheckedInByPersonLastName ).ThenBy( a => a.CheckedInByPersonNickName );
            }

            var attendanceResults = attendanceResultsQry.ToList();

            var uniqueScheduleIds = attendanceResults.Select( ar => ar.ScheduleId ).ToList();

            var schedules = new ScheduleService( _rockContext ).Queryable().AsNoTracking()
                .Where( s => uniqueScheduleIds.Contains( s.Id ) ).ToList();

            var attendanceIdsToRemove = new List<int>();

			//remove all who did not check out but their schedule was not active during the window
			foreach ( var schedule in schedules )
            {
                foreach ( var a in attendanceResults.Where( ar => !ar.EndDateTime.HasValue && ar.ScheduleId == schedule.Id ).ToList() ) 
                {
                    var checkinStart = schedule.GetNextStartDateTime( a.StartDateTime.Date );

                    if ( checkinStart.HasValue )
                    {
                        var checkInEnd = checkinStart.Value.AddMinutes( schedule.CheckInEndOffsetMinutes ?? 0 );
                        if ( searchStartTime > checkInEnd )
                        {
                            attendanceIdsToRemove.Add( a.AttendanceId );
                        }
                    }
                }
            }
			
            attendanceResults.RemoveAll( a => attendanceIdsToRemove.Contains( a.AttendanceId ) );

            gAttendees.DataSource = attendanceResults;
            gAttendees.DataBind();
            pnlResults.Visible = true;




        }
        private void LogAndShowException( Exception ex )
        {
            LogException( ex );
            string errorMessage = null;
            string stackTrace = string.Empty;

            while ( ex != null )
            {
                errorMessage = ex.Message;
                stackTrace += ex.StackTrace;
                if ( ex is System.Data.SqlClient.SqlException )
                {
                    // if there was a SQL Server Timeout, have the warning be a friendly message about that.
                    if ( ( ex as System.Data.SqlClient.SqlException ).Number == -2 )
                    {
                        errorMessage = "The attendee report did not complete in a timely manner. Try again using a smaller date range and fewer campuses and groups.";
                        break;
                    }
                    else
                    {
                        ex = ex.InnerException;
                    }
                }
                else
                {
                    ex = ex.InnerException;
                }
            }

            nbAttendeesError.Text = errorMessage;
            nbAttendeesError.Details = stackTrace;
            nbAttendeesError.Visible = true;
        }

        private void UpdateUserPreferences()
        {
            string keyPrefix = string.Format( "attendance-byTimestamp-{0}", this.BlockId );
            this.SetUserPreference( keyPrefix + "TemplateGroupTypeId", gtpAttendanceConfiguration.SelectedValue, false );
            this.SetUserPreference( keyPrefix + "GroupIds", gpGroups.SelectedValues.ToList().AsDelimited( "," ), false );
            this.SetUserPreference( keyPrefix + "CampusId", cpCampus.SelectedValue, false );
            this.SetUserPreference( keyPrefix + "ScheduleId", spSchedule.SelectedValue, false );
            this.SetUserPreference( keyPrefix + "StartDate", string.Format( "{0:g}", dtpStart.SelectedDateTime ), false );
            this.SetUserPreference( keyPrefix + "EndDate", string.Format( "{0:g}", dtpEnd.SelectedDateTime ), false );
            this.SetUserPreference( keyPrefix + "ShowRollover", hfHardStop.Value, false );

            this.SaveUserPreferences( keyPrefix );

        }

        #endregion

    }

    public class AttendanceByTimeStampResult
    {
        #region Fields
        const string ROLLOVER_NOTE = "Using Absolute Limits.";
        #endregion

        #region Properties
        public int AttendanceId { get; set; }
        public int AttendancePersonAliasId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public int? AttendanceCodeId { get; set; }
        public string AttendanceCodeValue { get; set; }
        public string AttendanceNote { get; set; }
        public int PersonId { get; set; }
        public string AttendedPersonFirstName { get; set; }
        public string AttendedPersonLastName { get; set; }
        public string AttendedPersonNickName { get; set; }
        public DateTime? AttendedPersonBirthdate { get; set; }
        public int? AttendedPersonAge { get; set; }
        public string AttendedPersonEmail { get; set; }
        public string AttendedPersonGender { get; set; }
        public int? CheckedInByPersonAliasId { get; set; }
        public int? CheckedInByPersonId { get; set; }
        public string CheckedInByPersonLastName { get; set; }
        public string CheckedInByPersonFirstName { get; set; }
        public string CheckedInByPersonNickName { get; set; }
        public int? CheckedOutByPersonAliasId { get; set; }
        public int? CheckedOutByPersonId { get; set; }
        public string CheckedOutByPersonLastName { get; set; }
        public string CheckedOutByPersonFirstName { get; set; }
        public string CheckedOutByPersonNickName { get; set; }
        public int? ScheduleId { get; set; }
        public string ScheduleName { get; set; }
        public int? CheckInGroupId { get; set; }
        public string CheckInGroupName { get; set; }
        public int? CheckInLocationId { get; set; }
        public string CheckInLocationName { get; set; }

        public string AttendedPersonFullName
        {
            get
            {
                return string.Concat( AttendedPersonNickName, " ", AttendedPersonLastName );
            }
        }

        public string CheckedInByPersonFullName
        {
            get
            {
                if ( CheckedInByPersonAliasId.HasValue )
                {
                    return string.Concat( CheckedInByPersonNickName, " ", CheckedInByPersonLastName );
                }
                return null;
            }
        }

        public string CheckedOutByPersonFullName
        {
            get
            {
                if ( CheckedOutByPersonAliasId.HasValue )
                {
                    return string.Concat( CheckedOutByPersonNickName, " ", CheckedOutByPersonLastName );
                }
                return null;
            }
        }

        public bool IsRollover
        {
            get
            {
                if ( String.IsNullOrWhiteSpace( AttendanceNote ) )
                {
                    return false;
                }
                else
                {
                    return AttendanceNote.Equals( ROLLOVER_NOTE, StringComparison.InvariantCultureIgnoreCase );
                }
            }
        }

        public DateTime EndDateTimeSortValue
        {
            get
            {
                if(!EndDateTime.HasValue)
                {
                    return StartDateTime.Date.AddDays( 1 ).AddSeconds( -1 );
                }
                return EndDateTime.Value;

            }
        }
        #endregion  

    }
}