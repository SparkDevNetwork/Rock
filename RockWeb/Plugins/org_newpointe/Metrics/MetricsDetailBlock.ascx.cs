using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System.Data.SqlClient;


namespace RockWeb.Plugins.org_newpointe.Reporting
{

    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Metrics Dashboard Block" )]
    [Category( "NewPointe Reporting" )]
    [Description( "Metrics stuff." )]
    public partial class MetricsDetailBlock : Rock.Web.UI.RockBlock
    {

        public string AllCommitments;
        public string AttendanceColor;
        public string AttendanceLastWeekAll;
        public string AttendanceLastWeekAud;
        public string AttendanceLastWeekChild;
        public string AttendanceLastWeekStudent;
        public string AttendancePercentage;
        public string Commitments;
        public string InactiveFollowup;
        public string InactiveFollowupColor;
        public string InactiveFollowupComplete;
        public string InactiveFollowupIncomplete;
        public string InactiveFollowupPercentage;
        public string Involvement;
        public string Recommitments;
        public string SelectedCampusName;
        public string SmallGroupLeaders;
        public string TotalCommitmentColor;
        public string TotalCommitmentPercentage;
        public string Volunteers;
        public string SundayDate;

        RockContext rockContext = new RockContext();

        protected void Page_Load( object sender, EventArgs e )
        {

            if ( !Page.IsPostBack )
            {

                var campList = CampusCache.All( false ).Select( cc => new { cc.Id, cc.Name } ).ToList();
                campList.Insert( 0, new { Id = 0, Name = "All Org" } );
                cpCampus.DataSource = campList;
                cpCampus.DataValueField = "Id";
                cpCampus.DataTextField = "Name";
                cpCampus.DataBind();

                var firstOrDefault = CurrentPerson.Members.FirstOrDefault(gm => gm.Group.GroupType.Name == "Staff" && gm.Group.CampusId != null);
                if (firstOrDefault != null)
                    cpCampus.SelectedValue = firstOrDefault.Group.CampusId.ToString();

                if ( cpCampus.SelectedValueAsId() == null )
                    cpCampus.SelectedValue = "0";

            }
            DoSQL();
        }


        protected void cpCampus_OnSelectionChanged( object sender, EventArgs e )
        {
            DoSQL();
        }



        protected void DoSQL()
        {
            int SelectedCampusId = cpCampus.SelectedValue.AsInteger();
            CampusCache SelectedCampus = CampusCache.Get( SelectedCampusId );
            string SelectedCampusName = SelectedCampus != null ? SelectedCampus.Name : "";

            var sunday = DateTime.Today.SundayDate().AddDays( -6 );
            var lastSunday = sunday.AddDays( -7 );

            SundayDate = sunday.ToShortDateString();

            MetricValueService mvServ = new MetricValueService( rockContext );

            var metricValues = SelectedCampusId == 0 ? mvServ.Queryable() : mvServ.Queryable().Where( mv => mv.MetricValuePartitions.Where( mvp => mvp.MetricPartition.EntityTypeId == 67 ).Select( mvp => mvp.EntityId ).Contains( SelectedCampusId ) );
            var metric_Goal = metricValues.Where( mv => mv.MetricValueType == MetricValueType.Goal );
            var metric_Measure = metricValues.Where( mv => mv.MetricValueType == MetricValueType.Measure );
            var metric_LastWeek = metric_Measure.Where( mv => mv.MetricValueDateTime > lastSunday && mv.MetricValueDateTime <= sunday );


            //Attendance Last Week - All Environments
            int iAttendanceGoalCurrent = decimal.ToInt32( metric_Goal.Where( mv => new int[] { 2, 3, 4, 5 }.Contains( mv.MetricId ) && mv.MetricValueDateTime.Value.Year == sunday.Year ).Sum( mv => mv.YValue ) ?? 0 );
            int iAttendanceLastWeekAll = decimal.ToInt32( metric_LastWeek.Where( mv => new int[] { 2, 3, 4, 5 }.Contains( mv.MetricId ) ).Sum( mv => mv.YValue ) ?? 0 );
            int iAttendancePercent = iAttendanceGoalCurrent != 0 ? ( int ) ( ( ( double ) iAttendanceLastWeekAll / iAttendanceGoalCurrent ) * 100 ) : 0;

            AttendanceLastWeekAll = iAttendanceLastWeekAll.ToString();
            AttendancePercentage = iAttendancePercent.ToString();

            AttendanceColor = GetColorForPercent( iAttendancePercent );

            //Attendance Last Week - Auditorium
            AttendanceLastWeekAud = decimal.ToInt32( metric_LastWeek.Where( mv => new int[] { 2 }.Contains( mv.MetricId ) ).Sum( mv => mv.YValue ) ?? 0 ).ToString();

            //Attendance Last Week - Rainforest + Velocity
            AttendanceLastWeekChild = decimal.ToInt32( metric_LastWeek.Where( mv => new int[] { 3, 4 }.Contains( mv.MetricId ) ).Sum( mv => mv.YValue ) ?? 0 ).ToString();

            //Attendance Last Week - The Collective
            AttendanceLastWeekStudent = decimal.ToInt32( metric_LastWeek.Where( mv => new int[] { 5 }.Contains( mv.MetricId ) ).Sum( mv => mv.YValue ) ?? 0 ).ToString();


            //Comitments
            var commitStartDate = DateTime.Parse( "2015-09-01" );

            //First Time Commitments
            int iCommitments = decimal.ToInt32( metric_Measure.Where( mv => mv.MetricId == 12 && mv.MetricValueDateTime >= commitStartDate ).Sum( mv => mv.YValue ) ?? 0 );
            Commitments = iCommitments.ToString();


            //Re-commitments
            int iRecommitments = decimal.ToInt32( metric_Measure.Where( mv => mv.MetricId == 13 && mv.MetricValueDateTime >= commitStartDate ).Sum( mv => mv.YValue ) ?? 0 );
            Recommitments = iRecommitments.ToString();


            //TotalCommitments
            int iTotalCommitments = decimal.ToInt32( metric_Measure.Where( mv => new int[] { 12, 13 }.Contains( mv.MetricId ) && mv.MetricValueDateTime >= commitStartDate ).Sum( mv => mv.YValue ) ?? 0 );
            int iTotalCommitmentGoal = decimal.ToInt32( metric_Goal.Where( mv => new int[] { 12, 13 }.Contains( mv.MetricId ) && mv.MetricValueDateTime.Value.Year == sunday.Year ).Sum( mv => mv.YValue ) ?? 0 );
            int iTotalCommitmentPercent = iTotalCommitmentGoal != 0 ? ( int ) ( ( ( double ) iTotalCommitments / iTotalCommitmentGoal ) * 100 ) : 0;

            AllCommitments = iTotalCommitments.ToString();
            TotalCommitmentPercentage = iTotalCommitmentPercent.ToString();

            TotalCommitmentColor = GetColorForPercent( iTotalCommitmentPercent );


            //Involvement
            var groupMembers = new GroupMemberService( rockContext ).Queryable().Where( gm => gm.Group.IsActive && gm.GroupMemberStatus == GroupMemberStatus.Active );
            if ( SelectedCampusId != 0 )
            {
                groupMembers = groupMembers.Where( gm => gm.Group.CampusId == SelectedCampusId );
            }

            //Small Group Leaders
            SmallGroupLeaders = groupMembers.Where( gm => gm.Group.GroupTypeId == 25 && gm.GroupRoleId == 24 ).DistinctBy( gm => gm.PersonId ).Count().ToString();

            //Volunteers
            Volunteers = groupMembers.Where( gm => gm.Group.GroupTypeId == 42 ).DistinctBy( gm => gm.PersonId ).Count().ToString();

            //Total Involvement
            Involvement = groupMembers.Where( gm => ( gm.Group.GroupTypeId == 25 && gm.GroupRoleId == 24 ) || gm.Group.GroupTypeId == 42 ).DistinctBy( gm => gm.PersonId ).Count().ToString();
            

            WorkflowService workServ = new WorkflowService( rockContext );

            var today = DateTime.Now;

            int iInactiveFollowupComplete;
            int iInactiveFollowupIncomplete;
            //Inactive Follow-up
            if ( SelectedCampusId == 0 )
            {
                iInactiveFollowupComplete = workServ.Queryable().Where( w => w.WorkflowTypeId == 120 && w.Status == "Completed" && w.ActivatedDateTime.Value.Year == today.Year && w.ActivatedDateTime.Value.Month == today.Month ).Count();
            }
            else
            {
                iInactiveFollowupComplete = workServ.Queryable().Where( w => w.WorkflowTypeId == 120 && w.Status == "Completed" && w.ActivatedDateTime.Value.Year == today.Year && w.ActivatedDateTime.Value.Month == today.Month ).WhereAttributeValue( rockContext, "Campus", SelectedCampusName ).Count();
            }

            if ( SelectedCampusId == 0 )
            {
                iInactiveFollowupIncomplete = workServ.Queryable().Where( w => w.WorkflowTypeId == 120 && w.Status != "Completed" && w.ActivatedDateTime.Value.Year == today.Year && w.ActivatedDateTime.Value.Month == today.Month ).Count();
            }
            else
            {
                iInactiveFollowupIncomplete = workServ.Queryable().Where( w => w.WorkflowTypeId == 120 && w.Status != "Completed" && w.ActivatedDateTime.Value.Year == today.Year && w.ActivatedDateTime.Value.Month == today.Month ).WhereAttributeValue( rockContext, "Campus", SelectedCampusName ).Count();
            }

            int iInactiveFollowup = iInactiveFollowupComplete + iInactiveFollowupIncomplete;
            int iInactiveFollowupPercentage = iInactiveFollowup != 0 ? ( int ) ( ( ( double ) iInactiveFollowupComplete / iInactiveFollowup ) * 100 ) : 0;

            InactiveFollowupComplete = iInactiveFollowupComplete.ToString();
            InactiveFollowupIncomplete = iInactiveFollowupIncomplete.ToString();
            InactiveFollowup = iInactiveFollowup.ToString();
            InactiveFollowupPercentage = iInactiveFollowupPercentage.ToString();

            InactiveFollowupColor = GetColorForPercent( iInactiveFollowupPercentage );

        }

        private string GetColorForPercent( int percentOf100 )
        {
            if ( percentOf100 <= 30 )
            {
                return "danger";
            }
            else if ( percentOf100 <= 60 )
            {
                return "warning";
            }
            else if ( percentOf100 <= 90 )
            {
                return "info";
            }
            else if ( percentOf100 <= 100 )
            {
                return "success";
            }
            return "";
        }
    }
}