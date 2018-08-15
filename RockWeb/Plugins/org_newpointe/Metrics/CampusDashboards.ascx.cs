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


namespace RockWeb.Plugins.org_newpointe.Metrics
{

    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName("Campus Dashboards")]
    [Category("NewPointe Metrics")]
    [Description("Campus Dashboards")]
    [DateField("Fiscal Year Start Date","Select the date the Fiscal Year starts", true)]


    public partial class CampusDashboards : Rock.Web.UI.RockBlock
    {


        public string FiscalYearStartDate;
        public string SelectedCampus = string.Empty;
        public int SelectedCampusId;

        public string AttendanceAverage;
        public string AttendanceAveragePast6Weeks;

        public string AttendanceLastWeekAll;
        public string AttendanceLastWeekAud;
        public string AttendanceLastWeekChild;
        public string AttendanceLastWeekStudent;

        public string AttendanceGoalCurrent;
        public string AttendanceGoal2020;
        public string AttendanceGoalProgress = "<span class='label label-danger'>Below Target</span>";
 
        public string AttendanceAudGoalCurrent;
        public string AttendanceAudGoal2020;
        public string AttendanceAudGoalProgress = "<span class='label label-danger'>Below Target</span>";

        public string AttendanceChildGoalCurrent;
        public string AttendanceChildGoal2020;
        public string AttendanceChildGoalProgress = "<span class='label label-danger'>Below Target</span>";

        public string AttendanceStudentGoalCurrent;
        public string AttendanceStudentGoal2020;
        public string AttendanceStudentGoalProgress = "<span class='label label-danger'>Below Target</span>";

        public string AttendanceAllGoalCurrent;
        public string AttendanceAllGoal2020;
        public string AttendanceAllGoalProgress = "<span class='label label-danger'>Below Target</span>";
        
        public string Baptisms;
        public string BaptismsGoalCurrent;
        public string BaptismsGoal2020;
        public string BaptismsGoalProgress = "<span class='label label-danger'>Below Target</span>";

        public string Commitments;
        public string CommitmentsGoalCurrent;
        public string CommitmentsGoal2020;
        public string CommitmentsGoalProgress = "<span class='label label-danger'>Below Target</span>";

        public string Recommitments;
        public string RecommitmentsGoalCurrent;
        public string RecommitmentsGoal2020;
        public string RecommitmentsGoalProgress = "<span class='label label-danger'>Below Target</span>";

        public string AllCommitments;
        public string AllCommitmentsGoalCurrent;
        public string AllCommitmentsGoal2020;
        public string AllCommitmentsGoalProgress = "<span class='label label-danger'>Below Target</span>";

        public string NewHere;
        public string NewHereGoalCurrent;
        public string NewHereGoal2020;
        public string NewHereGoalProgress = "<span class='label label-danger'>Below Target</span>";

        public string Partners;
        public string PartnersGoalCurrent;
        public string PartnersGoal2020;
        public string PartnersGoalProgress = "<span class='label label-danger'>Below Target</span>";

        public string SmallGroupLeaders;
        public string SmallGroupLeadersGoalCurrent;
        public string SmallGroupLeadersGoal2020;
        public string SmallGroupLeadersGoalProgress = "<span class='label label-danger'>Below Target</span>";

        public string Volunteers;
        public string VolunteersGoalCurrent;
        public string VolunteersGoal2020;
        public string VolunteersGoalProgress = "<span class='label label-danger'>Below Target</span>";

        public string Involvement;
        public string InvolvementGoalCurrent;
        public string InvolvementGoal2020;
        public string InvolvementGoalProgress = "<span class='label label-danger'>Below Target</span>";

        public string NewtoNewPointe;
        public string NewtoNewPointeGoalCurrent;
        public string NewtoNewPointeGoal2020;
        public string NewtoNewPointeGoalProgress = "<span class='label label-danger'>Below Target</span>";

        public string DiscoverGroups;
        public string DiscoverGroupsGoalCurrent;
        public string DiscoverGroupsGoal2020;
        public string DiscoverGroupsGoalProgress = "<span class='label label-danger'>Below Target</span>";

        public string SmallGroupParticipants;
        public string SmallGroupParticipantsGoalCurrent;
        public string SmallGroupParticipantsGoal2020;
        public string SmallGroupParticipantsGoalProgress = "<span class='label label-danger'>Below Target</span>";

        public string Assimilation;
        public string AssimilationGoalCurrent;
        public string AssimilationGoal2020;
        public string AssimilationGoalProgress = "<span class='label label-danger'>Below Target</span>";


        public string SundayDate;

        public string InactiveFollowup;
        public string InactiveFollowupComplete;
        public string InactiveFollowupIncomplete;
        public string InactiveFollowupPercentage;
        public string InactiveFollowupColor;


        public string SundayDateSQLFormatted;
        public string AttendanceLastWeekCampus;
        public string AttendanceLastWeekLastYearAll;
        public string AttendanceLastWeekLastYearCampus;

        public string GivingLastWeek;
        public string GivingYtd; 
        public string GivingLastWeekCampus;
        public string GivingYtdCampus;

        public string GivingTwoWeeksAgo;
        public string GivingTwoWeeksAgoCampus;


        public string FinancialStartDate;
        public string FinancialEndDate;
        public string FinancialStartDateLastWeek;
        public string FinancialEndDateLastWeek;

        public int CurrentMonthInFiscalYear =1;
        public decimal GoalOffsetMultiplier = 1;
        public decimal SecondaryGoalOffsetMultiplier = 1;
        public decimal GoalTarget = .9M;
        
        public string sMonth;

        public int? iInactiveFollowupComplete;
        public int? iInactiveFollowupIncomplete;
        public string InactiveFollowupGoalProgress;
        public string InactiveFollowupAll;


        public string CompositeScore;
        public string CompAttendance;
        public string CompVolunteers;
        public string CompGroups;
        public string CompGroupParticipants;




        RockContext rockContext = new RockContext();


        protected void Page_Load(object sender, EventArgs e)
        {

            FiscalYearStartDate = GetAttributeValue("FiscalYearStartDate");

            //Set Last Sunday Date
            DateTime now = DateTime.Now;
            DateTime dt = DateTime.Now.StartOfWeek(DayOfWeek.Sunday);
            SundayDate = dt.ToShortDateString();
            SundayDateSQLFormatted = dt.Date.ToString("yyyy-MM-dd");

            DateTime lastTuesday = DateTime.Now.AddDays(-1);
            while (lastTuesday.DayOfWeek != DayOfWeek.Wednesday)
                lastTuesday = lastTuesday.AddDays(-1);

            DateTime lastWednesday = DateTime.Now.AddDays(-1);
            while (lastWednesday.DayOfWeek != DayOfWeek.Wednesday)
                lastWednesday = lastWednesday.AddDays(-1);

            FinancialStartDate = lastTuesday.AddDays(-7).ToString("yyyy-MM-dd");
            FinancialEndDate = lastWednesday.ToString("yyyy-MM-dd");

            FinancialStartDateLastWeek = lastTuesday.AddDays(-14).ToString("yyyy-MM-dd");
            FinancialEndDateLastWeek = lastWednesday.AddDays(-7).ToString("yyyy-MM-dd");

            sMonth = DateTime.Now.ToString("MM");

            switch (sMonth)
            {
                case "09":
                    CurrentMonthInFiscalYear = 1;
                    GoalOffsetMultiplier = .083M;
                    SecondaryGoalOffsetMultiplier = .89M;
                    break;
                case "10":
                    CurrentMonthInFiscalYear = 2;
                    GoalOffsetMultiplier = .167M;
                    SecondaryGoalOffsetMultiplier = .90M;
                    break;
                case "11":
                    CurrentMonthInFiscalYear = 3;
                    GoalOffsetMultiplier = .25M;
                    SecondaryGoalOffsetMultiplier = .91M;
                    break;
                case "12":
                    CurrentMonthInFiscalYear = 4;
                    GoalOffsetMultiplier =.333M;
                    SecondaryGoalOffsetMultiplier = .92M;
                    break;
                case "01":
                    CurrentMonthInFiscalYear = 5;
                    GoalOffsetMultiplier = .417M;
                    SecondaryGoalOffsetMultiplier = .93M;
                    break;
                case "02":
                    CurrentMonthInFiscalYear = 6;
                    GoalOffsetMultiplier = .5M;
                    SecondaryGoalOffsetMultiplier = .94M;
                    break;
                case "03":
                    CurrentMonthInFiscalYear = 7;
                    GoalOffsetMultiplier = .583M;
                    SecondaryGoalOffsetMultiplier = .95M;
                    break;
                case "04":
                    CurrentMonthInFiscalYear = 8;
                    GoalOffsetMultiplier =.667M;
                    SecondaryGoalOffsetMultiplier = .96M;
                    break;
                case "05":
                    CurrentMonthInFiscalYear = 9;
                    GoalOffsetMultiplier = .75M;
                    SecondaryGoalOffsetMultiplier = .97M;
                    break;
                case "06":
                    CurrentMonthInFiscalYear = 10;
                    GoalOffsetMultiplier = .883M;
                    SecondaryGoalOffsetMultiplier = .98M;
                    break;
                case "07":
                    CurrentMonthInFiscalYear = 11;
                    GoalOffsetMultiplier = .917M;
                    SecondaryGoalOffsetMultiplier = .99M;
                    break;
                case "08":
                    CurrentMonthInFiscalYear = 12;
                    GoalOffsetMultiplier = 1;
                    SecondaryGoalOffsetMultiplier = 1;
                    break;
            }

            if (!Page.IsPostBack)
            {
                //Get Attributes
                FiscalYearStartDate = GetAttributeValue("FiscalYearStartDate").ToString();

                //Generate Campus List
                string[] campusList = {"All Org", "Canton Campus", "Coshocton Campus", "Dover Campus", "Millersburg Campus", "Wooster Campus"};
                cpCampus.DataSource = campusList;
                cpCampus.DataBind();

                //Get the campus of the currently logged in person
                //PersonService personService = new PersonService(rockContext);
                //var personObject = personService.Get(CurrentPerson.Guid);
                //var campus = personObject.GetFamilies().FirstOrDefault().Campus ?? new Campus();
                //SelectedCampusId = campus.Id;
                //cpCampus.SelectedValue = campus.Name;
                //SelectedCampus = campus.Name;


                PersonService personService = new PersonService(rockContext);
                GroupService groupService = new GroupService(rockContext);
                GroupMemberService groupMemberService = new GroupMemberService(rockContext);
                var personObject = personService.Get(CurrentPerson.Guid);

                //Is Person in Akron Campus?
                if (groupMemberService.GetByGroupIdAndPersonId(74786, (int)CurrentPersonId).Any() == true)
                {
                    SelectedCampusId = 5;
                    cpCampus.SelectedValue = "Akron Campus";
                    SelectedCampus = "Akron Campus";
                }
                //Is Person in Canton Campus?
                if (groupMemberService.GetByGroupIdAndPersonId(74787, (int)CurrentPersonId).Any() == true)
                {
                    SelectedCampusId = 2;
                    cpCampus.SelectedValue = "Canton Campus";
                    SelectedCampus = "Canton Campus";
                }
                //Is Person in Coshocton Campus?
                if (groupMemberService.GetByGroupIdAndPersonId(74788, (int)CurrentPersonId).Any() == true)
                {
                    SelectedCampusId = 3;
                    cpCampus.SelectedValue = "Coshocton Campus";
                    SelectedCampus = "Coshocton Campus";
                }
                //Is Person in Dover Campus?
                if (groupMemberService.GetByGroupIdAndPersonId(74789, (int)CurrentPersonId).Any() == true)
                {
                    SelectedCampusId = 1;
                    cpCampus.SelectedValue = "Dover Campus";
                    SelectedCampus = "Dover Campus";
                }
                //Is Person in Millersburg Campus?
                if (groupMemberService.GetByGroupIdAndPersonId(74790, (int)CurrentPersonId).Any() == true)
                {
                    SelectedCampusId = 4;
                    cpCampus.SelectedValue = "Millersburg Campus";
                    SelectedCampus = "Millersburg Campus";
                }
                //Is Person in Wooster Campus?
                if (groupMemberService.GetByGroupIdAndPersonId(74791, (int)CurrentPersonId).Any() == true)
                {
                    SelectedCampusId = 6;
                    cpCampus.SelectedValue = "Wooster Campus";
                    SelectedCampus = "Wooster Campus";
                }
                //Is Person in Central?
                if (groupMemberService.GetByGroupIdAndPersonId(74785, (int)CurrentPersonId).Any() == true)
                {
                    cpCampus.SelectedValue = "All Org";
                    SelectedCampus = "All Org";
                    SelectedCampusId = 0;
                }


            }

            DoSQL();

        }


        protected void cpCampus_OnSelectionChanged(object sender, EventArgs e)
        {

            SelectedCampus = cpCampus.SelectedValue.ToString();
            switch (SelectedCampus)
            {
                case "Akron Campus":
                    SelectedCampusId = 6;
                    break;
                case "Canton Campus":
                    SelectedCampusId = 2;
                    break;
                case "Coshocton Campus":
                    SelectedCampusId = 3;
                    break;
                case "Dover Campus":
                    SelectedCampusId = 1;
                    break;
                case "Millersburg Campus":
                    SelectedCampusId = 4;
                    break;
                case "Wooster Campus":
                    SelectedCampusId = 5;
                    break;
                case "All Org":
                    SelectedCampusId = 0;
                    break;
            }

            DoSQL();

        }



        protected void DoSQL()
        {
            //Find the average attendacne over the past year
            if (SelectedCampusId == 0)
            {
                AttendanceAverage = rockContext.Database.SqlQuery<int>(@"SELECT CAST(AVG(att) as int)
                FROM
                (
                SELECT TOP 50 SUM(mv.YValue) as att, DATEPART(isowk, mv.MetricValueDateTime) as weeknumber
                FROM MetricValue mv
                WHERE mv.MetricValueType = 0 AND (mv.MetricId = 2 OR mv.MetricId = 3 OR mv.MetricId = 4 OR mv.MetricId = 5) AND mv.EntityId != 8 AND mv.MetricValueDateTime > DATEADD(week, -50, GETDATE())
                GROUP by DATEPART(isowk, mv.MetricValueDateTime)
                ) inner_query
                ").ToList<int>()[0].ToString();
            }
            else
            {
                AttendanceAverage = rockContext.Database.SqlQuery<int>(@"SELECT CAST(AVG(att) as int)
                FROM
                (
                SELECT TOP 50 SUM(mv.YValue) as att, DATEPART(isowk, mv.MetricValueDateTime) as weeknumber
                FROM MetricValue mv
                WHERE mv.MetricValueType = 0 AND (mv.MetricId = 2 OR mv.MetricId = 3 OR mv.MetricId = 4 OR mv.MetricId = 5) AND mv.EntityId = @CampusId AND mv.MetricValueDateTime > DATEADD(week, -50, GETDATE())
                GROUP by DATEPART(isowk, mv.MetricValueDateTime)
                ) inner_query
                ", new SqlParameter("CampusId", SelectedCampusId)).ToList<int>()[0].ToString();
            }




            //Find the average attendacne over the past 6 weeks
            if (SelectedCampusId == 0)
            {
                AttendanceAveragePast6Weeks = rockContext.Database.SqlQuery<int>(@"SELECT CAST(AVG(att) as int)
                FROM
                (
                SELECT TOP 50 SUM(mv.YValue) as att, DATEPART(isowk, mv.MetricValueDateTime) as weeknumber
                FROM MetricValue mv
                WHERE mv.MetricValueType = 0 AND (mv.MetricId = 2 OR mv.MetricId = 3 OR mv.MetricId = 4 OR mv.MetricId = 5) AND mv.EntityId != 8 AND mv.MetricValueDateTime > DATEADD(week, -6, GETDATE())
                GROUP by DATEPART(isowk, mv.MetricValueDateTime)
                ) inner_query
                ").ToList<int>()[0].ToString();
            }
            else
            {
                AttendanceAveragePast6Weeks = rockContext.Database.SqlQuery<int>(@"SELECT CAST(AVG(att) as int)
                FROM
                (
                SELECT TOP 50 SUM(mv.YValue) as att, DATEPART(isowk, mv.MetricValueDateTime) as weeknumber
                FROM MetricValue mv
                WHERE mv.MetricValueType = 0 AND (mv.MetricId = 2 OR mv.MetricId = 3 OR mv.MetricId = 4 OR mv.MetricId = 5) AND mv.EntityId = @CampusId AND mv.MetricValueDateTime > DATEADD(week, -6, GETDATE())
                GROUP by DATEPART(isowk, mv.MetricValueDateTime)
                ) inner_query
                ", new SqlParameter("CampusId", SelectedCampusId)).ToList<int>()[0].ToString();
            }






            //Attendance Last Week - All Environments
            if (SelectedCampusId == 0)
            {
                int? iAttendanceLastWeekAll = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(SUM(YValue) as int) as att
                FROM MetricValue mv
                WHERE mv.MetricValueType = 0 AND (mv.MetricId = 2 OR mv.MetricId = 3 OR mv.MetricId = 4 OR mv.MetricId = 5) AND(DATEPART(isowk, mv.MetricValueDateTime) = DATEPART(isowk, GETDATE()) - 1)
                AND(DATEPART(yy, mv.MetricValueDateTime) = DATEPART(yy, GETDATE())) AND mv.EntityId != 8; ")
                    .ToList<int?>()[0];

                int? iAttendanceGoalCurrent = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(SUM(YValue) as int) as att
                FROM MetricValue mv
                WHERE mv.MetricValueType = 1 AND (mv.MetricId = 2 OR mv.MetricId = 3 OR mv.MetricId = 4 OR mv.MetricId = 5 OR mv.MetricId = 23) 
				AND DATEPART(YEAR,mv.MetricValueDateTime) = DATEPART(YEAR,GETDATE())")
                    .ToList<int?>()[0];

                AttendanceGoal2020 = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(SUM(YValue) as int) as att
                FROM MetricValue mv
                WHERE mv.MetricValueType = 1 AND (mv.MetricId = 2 OR mv.MetricId = 3 OR mv.MetricId = 4 OR mv.MetricId = 5 OR mv.MetricId = 23) 
				AND DATEPART(YEAR,mv.MetricValueDateTime) = '2020'")
                    .ToList<int?>()[0].ToString();


                AttendanceLastWeekAll = iAttendanceLastWeekAll.ToString();
                AttendanceGoalCurrent = iAttendanceGoalCurrent.ToString();

                decimal? goalProgress = iAttendanceLastWeekAll / (iAttendanceGoalCurrent * SecondaryGoalOffsetMultiplier);
                if (goalProgress >= GoalTarget)
                {
                    AttendanceGoalProgress = "<span class='label label-success'>On Target (" + Math.Truncate(iAttendanceGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier) + ")</span>";
                }
                else
                {
                    AttendanceGoalProgress = "<span class='label label-danger'>Below Target (" + Math.Truncate(iAttendanceGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier) + ")</span>";
                }


            }
            else
            {
                int? iAttendanceLastWeekAll = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(SUM(YValue) as int) as att
                FROM MetricValue mv
                WHERE mv.MetricValueType = 0 AND (mv.MetricId = 2 OR mv.MetricId = 3 OR mv.MetricId = 4 OR mv.MetricId = 5) AND(DATEPART(isowk, mv.MetricValueDateTime) = DATEPART(isowk, GETDATE()) - 1)
                AND(DATEPART(yy, mv.MetricValueDateTime) = DATEPART(yy, GETDATE())) AND mv.EntityId = @CampusId; ", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0];

                int? iAttendanceGoalCurrent = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(SUM(YValue) as int) as att
                FROM MetricValue mv
                WHERE mv.MetricValueType = 1 AND (mv.MetricId = 2 OR mv.MetricId = 3 OR mv.MetricId = 4 OR mv.MetricId = 5 OR mv.MetricId = 23) 
				AND DATEPART(YEAR,mv.MetricValueDateTime) = DATEPART(YEAR,GETDATE()) AND mv.EntityId = @CampusId", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0];

                AttendanceGoal2020 = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(SUM(YValue) as int) as att
                FROM MetricValue mv
                WHERE mv.MetricValueType = 1 AND (mv.MetricId = 2 OR mv.MetricId = 3 OR mv.MetricId = 4 OR mv.MetricId = 5 OR mv.MetricId = 23) 
				AND DATEPART(YEAR,mv.MetricValueDateTime) = '2020' AND mv.EntityId = @CampusId", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0].ToString();

                AttendanceLastWeekAll = iAttendanceLastWeekAll.ToString();
                AttendanceGoalCurrent = iAttendanceGoalCurrent.ToString();

                decimal? goalProgress = iAttendanceLastWeekAll / (iAttendanceGoalCurrent * SecondaryGoalOffsetMultiplier);
                if (goalProgress >= GoalTarget)
                {
                    AttendanceGoalProgress = "<span class='label label-success'>On Target (" + Math.Truncate(iAttendanceGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier) + ")</span>";
                }
                else
                {
                    AttendanceGoalProgress = "<span class='label label-danger'>Below Target (" + Math.Truncate(iAttendanceGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier) + ")</span>";
                }

            }





            //Attendance Last Week - Auditorium
            if (SelectedCampusId == 0)
            {
                int? iAttendanceLastWeekAud = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(SUM(YValue) as int) as att
                FROM MetricValue mv 
                WHERE mv.MetricValueType = 0 AND (mv.MetricId = 2) AND DATEPART(isowk, mv.MetricValueDateTime) = DATEPART(isowk, GETDATE())-1 AND DATEPART(year, mv.MetricValueDateTime) = DATEPART(year, GETDATE()) AND mv.EntityId != 8").ToList<int?>()[0];

                int? iAttendanceAudGoalCurrent = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 2 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = DATEPART(YEAR,GETDATE())").ToList<int?>()[0];

                AttendanceAudGoal2020 = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 2 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = '2020'").ToList<int?>()[0].ToString();

                AttendanceLastWeekAud = iAttendanceLastWeekAud.ToString();
                AttendanceAudGoalCurrent = iAttendanceAudGoalCurrent.ToString();

                decimal? goalProgress = iAttendanceLastWeekAud / (iAttendanceAudGoalCurrent * SecondaryGoalOffsetMultiplier);
                if (goalProgress >= GoalTarget)
                {
                    AttendanceAudGoalProgress = "<span class='label label-success'>On Target (" + Math.Truncate(iAttendanceAudGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier) + ")</span>";
                }
                else
                {
                    AttendanceAudGoalProgress = "<span class='label label-danger'>Below Target (" + Math.Truncate(iAttendanceAudGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier) + ")</span>";
                }


            }

            else
            {
                int? iAttendanceLastWeekAud = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(SUM(YValue) as int) as att
                FROM MetricValue mv 
                WHERE mv.MetricValueType = 0 AND (mv.MetricId = 2) AND DATEPART(isowk, mv.MetricValueDateTime) = DATEPART(isowk, GETDATE())-1 
                AND DATEPART(year, mv.MetricValueDateTime) = DATEPART(year, GETDATE()) AND mv.EntityId = @CampusId; ", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0];

                int? iAttendanceAudGoalCurrent = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE (mv.MetricId = 2) AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = DATEPART(YEAR,GETDATE()) AND mv.EntityId = @CampusId;", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0];

                AttendanceAudGoal2020 = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE (mv.MetricId = 2) AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = '2020' AND mv.EntityId = @CampusId;", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0].ToString();

                AttendanceLastWeekAud = iAttendanceLastWeekAud.ToString();
                AttendanceAudGoalCurrent = iAttendanceAudGoalCurrent.ToString();

                decimal? goalProgress = iAttendanceLastWeekAud / (iAttendanceAudGoalCurrent * SecondaryGoalOffsetMultiplier);
                if (goalProgress >= GoalTarget)
                {
                    AttendanceAudGoalProgress = "<span class='label label-success'>On Target (" + Math.Truncate(iAttendanceAudGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier) + ")</span>";
                }
                else
                {
                    AttendanceAudGoalProgress = "<span class='label label-danger'>Below Target (" + Math.Truncate(iAttendanceAudGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier) + ")</span>";
                }
            }

            //Attendance Last Week - Rainforest + Velocity
            if (SelectedCampusId == 0)
            {
                int? iAttendanceLastWeekChild = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(SUM(YValue) as int) as att
                FROM MetricValue mv 
                WHERE mv.MetricValueType = 0 AND (mv.MetricId = 3 OR mv.MetricId = 4) AND DATEPART(isowk, mv.MetricValueDateTime) = DATEPART(isowk, GETDATE())-1 AND DATEPART(year, mv.MetricValueDateTime) = DATEPART(year, GETDATE()) AND mv.EntityId != 8")
                    .ToList<int?>()[0];

                int? iAttendanceChildGoalCurrent = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE (mv.MetricId = 3 OR mv.MetricId = 4) AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = DATEPART(YEAR,GETDATE())").ToList<int?>()[0];

                AttendanceChildGoal2020 = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE (mv.MetricId = 3 OR mv.MetricId = 4) AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = '2020'").ToList<int?>()[0].ToString();

                AttendanceLastWeekChild = iAttendanceLastWeekChild.ToString();
                AttendanceChildGoalCurrent = iAttendanceChildGoalCurrent.ToString();

                decimal? goalProgress = iAttendanceLastWeekChild / (iAttendanceChildGoalCurrent * SecondaryGoalOffsetMultiplier);
                if (goalProgress >= GoalTarget)
                {
                    AttendanceChildGoalProgress = "<span class='label label-success'>On Target (" + Math.Truncate(iAttendanceChildGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier) + ")</span>";
                }
                else
                {
                    AttendanceChildGoalProgress = "<span class='label label-danger'>Below Target (" + Math.Truncate(iAttendanceChildGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier) + ")</span>";
                }



            }
            else
            {
                int? iAttendanceLastWeekChild = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(SUM(YValue) as int) as att
                FROM MetricValue mv 
                WHERE mv.MetricValueType = 0 AND (mv.MetricId = 3 OR mv.MetricId = 4) AND DATEPART(isowk, mv.MetricValueDateTime) = DATEPART(isowk, GETDATE())-1
                AND DATEPART(year, mv.MetricValueDateTime) = DATEPART(year, GETDATE()) AND mv.EntityId = @CampusId; ", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0];

                int? iAttendanceChildGoalCurrent = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE (mv.MetricId = 3 OR mv.MetricId = 4) AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = DATEPART(YEAR,GETDATE()) AND mv.EntityId = @CampusId;", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0];

                AttendanceChildGoal2020 = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE (mv.MetricId = 3 OR mv.MetricId = 4) AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = '2020' AND mv.EntityId = @CampusId;", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0].ToString();

                AttendanceLastWeekChild = iAttendanceLastWeekChild.ToString();
                AttendanceChildGoalCurrent = iAttendanceChildGoalCurrent.ToString();

                decimal? goalProgress = iAttendanceLastWeekChild / (iAttendanceChildGoalCurrent * SecondaryGoalOffsetMultiplier);
                if (goalProgress >= GoalTarget)
                {
                    AttendanceChildGoalProgress = "<span class='label label-success'>On Target (" + Math.Truncate(iAttendanceChildGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier) + ")</span>";
                }
                else
                {
                    AttendanceChildGoalProgress = "<span class='label label-danger'>Below Target (" + Math.Truncate(iAttendanceChildGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier) + ")</span>";
                }

            }




            //Attendance Last Week - The Collective
            if (SelectedCampusId == 0)
            {
                int? iAttendanceLastWeekStudent = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(SUM(YValue) as int) as att
                FROM MetricValue mv 
                WHERE mv.MetricValueType = 0 AND (mv.MetricId = 5 OR mv.MetricId = 23) AND DATEPART(isowk, mv.MetricValueDateTime) = DATEPART(isowk, GETDATE())-1 AND DATEPART(year, mv.MetricValueDateTime) = DATEPART(year, GETDATE()) AND mv.EntityId != 8")
                    .ToList<int?>()[0];

                int? iAttendanceStudentGoalCurrent = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE (mv.MetricId = 5 OR mv.MetricId = 23) AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = DATEPART(YEAR,GETDATE())").ToList<int?>()[0];

                AttendanceStudentGoal2020 = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE (mv.MetricId = 5 OR mv.MetricId = 23) AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = '2020'").ToList<int?>()[0].ToString();

                AttendanceLastWeekStudent = iAttendanceLastWeekStudent.ToString();
                AttendanceStudentGoalCurrent = iAttendanceStudentGoalCurrent.ToString();

                decimal? goalProgress = iAttendanceLastWeekStudent / (iAttendanceStudentGoalCurrent * SecondaryGoalOffsetMultiplier);
                if (goalProgress >= GoalTarget)
                {
                    AttendanceStudentGoalProgress = "<span class='label label-success'>On Target (" + Math.Truncate(iAttendanceStudentGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier) + ")</span>";
                }
                else
                {
                    AttendanceStudentGoalProgress = "<span class='label label-danger'>Below Target (" + Math.Truncate(iAttendanceStudentGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier) + ")</span>";
                }


            }
            else
            {
                int? iAttendanceLastWeekStudent = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue),0) as int) as att
                FROM MetricValue mv 
                WHERE mv.MetricValueType = 0 AND (mv.MetricId = 5 OR mv.MetricId = 23) AND DATEPART(isowk, mv.MetricValueDateTime) = 
                DATEPART(isowk, GETDATE())-1 AND DATEPART(year, mv.MetricValueDateTime) = DATEPART(year, GETDATE()) AND mv.EntityId = @CampusId; ", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0];

                int? iAttendanceStudentGoalCurrent = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE (mv.MetricId = 5 OR mv.MetricId = 23) AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = DATEPART(YEAR,GETDATE()) AND mv.EntityId = @CampusId;", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0];

                AttendanceStudentGoal2020 = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE (mv.MetricId = 5 OR mv.MetricId = 23) AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = '2020' AND mv.EntityId = @CampusId;", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0].ToString();

                AttendanceLastWeekStudent = iAttendanceLastWeekStudent.ToString();
                AttendanceStudentGoalCurrent = iAttendanceStudentGoalCurrent.ToString();

                decimal? goalProgress = iAttendanceLastWeekStudent / (iAttendanceStudentGoalCurrent * SecondaryGoalOffsetMultiplier);
                if (goalProgress >= GoalTarget)
                {
                    AttendanceStudentGoalProgress = "<span class='label label-success'>On Target (" + Math.Truncate(iAttendanceStudentGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier * GoalTarget) + ")</span>";
                }
                else
                {
                    AttendanceStudentGoalProgress = "<span class='label label-danger'>Below Target (" + Math.Truncate(iAttendanceStudentGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier * GoalTarget) + ")</span>";
                }

            }

            
            //AttendanceGoalCurrent = (Int32.Parse(AttendanceStudentGoalCurrent) + Int32.Parse(AttendanceChildGoalCurrent) + Int32.Parse(AttendanceGoalCurrent)).ToString();
            //AttendanceGoal2020 = (Int32.Parse(AttendanceStudentGoal2020) + Int32.Parse(AttendanceChildGoal2020) + Int32.Parse(AttendanceGoal2020)).ToString();


            //Baptisms
            if (SelectedCampusId == 0)
            {
                int? iBaptisms = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
                FROM MetricValue mv 
                WHERE mv.MetricValueType = 0 AND (mv.MetricId = 11) AND mv.MetricValueDateTime >= '2015-09-01'").ToList<int?>()[0];

                int? iBaptismsGoalCurrent = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 11 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = DATEPART(YEAR,GETDATE())").ToList<int?>()[0];

                BaptismsGoal2020 = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 11 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = '2020'").ToList<int?>()[0].ToString();

                Baptisms = iBaptisms.ToString();
                BaptismsGoalCurrent = iBaptismsGoalCurrent.ToString();

                decimal? goalProgress = iBaptisms / (iBaptismsGoalCurrent * GoalOffsetMultiplier);
                if (goalProgress >= GoalTarget)
                {
                    BaptismsGoalProgress = "<span class='label label-success'>On Target (" + Math.Truncate(iBaptismsGoalCurrent.GetValueOrDefault() * GoalOffsetMultiplier * GoalTarget) + ")</span>";
                }
                else
                {
                    BaptismsGoalProgress = "<span class='label label-danger'>Below Target (" + Math.Truncate(iBaptismsGoalCurrent.GetValueOrDefault() * GoalOffsetMultiplier * GoalTarget) + ")</span>";
                }

            }
            else
            {
                int? iBaptisms = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
                FROM MetricValue mv 
                WHERE mv.MetricValueType = 0 AND (mv.MetricId = 11) AND mv.MetricValueDateTime >= '2015-09-01' AND mv.EntityId = @CampusId; ", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0];

                int? iBaptismsGoalCurrent = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 11 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = DATEPART(YEAR,GETDATE()) AND mv.EntityId = @CampusId;", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0];

                BaptismsGoal2020 = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 11 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = '2020' AND mv.EntityId = @CampusId;", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0].ToString();

                Baptisms = iBaptisms.ToString();
                BaptismsGoalCurrent = iBaptismsGoalCurrent.ToString();

                decimal? goalProgress = iBaptisms / (iBaptismsGoalCurrent * GoalOffsetMultiplier);
                if (goalProgress >= GoalTarget)
                {
                    BaptismsGoalProgress = "<span class='label label-success'>On Target (" + Math.Truncate(iBaptismsGoalCurrent.GetValueOrDefault() * GoalOffsetMultiplier * GoalTarget) + ")</span>";
                }
                else
                {
                    BaptismsGoalProgress = "<span class='label label-danger'>Below Target (" + Math.Truncate(iBaptismsGoalCurrent.GetValueOrDefault() * GoalOffsetMultiplier * GoalTarget) + ")</span>";
                }
            }



            //First Time Commitments
            if (SelectedCampusId == 0)
            {
                int? item = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
                FROM MetricValue mv 
                WHERE mv.MetricValueType = 0 AND (mv.MetricId = 12) AND mv.MetricValueDateTime >= '2015-09-01'").ToList<int?>()[0];

                int? itemGoalCurrent = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 12 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = DATEPART(YEAR,GETDATE())").ToList<int?>()[0];

                CommitmentsGoal2020 = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 12 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = '2020'").ToList<int?>()[0].ToString();

                Commitments = item.ToString();
                CommitmentsGoalCurrent = itemGoalCurrent.ToString();

                decimal? goalProgress = item / (itemGoalCurrent * GoalOffsetMultiplier);
                if (goalProgress >= GoalTarget)
                {
                    CommitmentsGoalProgress = "<span class='label label-success'>On Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * GoalOffsetMultiplier * GoalTarget) + ")</span>";
                }
                else
                {
                    CommitmentsGoalProgress = "<span class='label label-danger'>Below Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * GoalOffsetMultiplier * GoalTarget) + ")</span>";
                }


            }
            else
            {
                int? item = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
                FROM MetricValue mv 
                WHERE mv.MetricValueType = 0 AND (mv.MetricId = 12) AND mv.MetricValueDateTime >= '2015-09-01' AND mv.EntityId = @CampusId; ", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0];

                int? itemGoalCurrent = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 12 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = DATEPART(YEAR,GETDATE()) AND mv.EntityId = @CampusId;", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0];

                CommitmentsGoal2020 = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 12 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = '2020' AND mv.EntityId = @CampusId;", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0].ToString();

                Commitments = item.ToString();
                CommitmentsGoalCurrent = itemGoalCurrent.ToString();

                decimal? goalProgress = item / (itemGoalCurrent * GoalOffsetMultiplier);
                if (goalProgress >= GoalTarget)
                {
                    CommitmentsGoalProgress = "<span class='label label-success'>On Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * GoalOffsetMultiplier * GoalTarget) + ")</span>";
                }
                else
                {
                    CommitmentsGoalProgress = "<span class='label label-danger'>Below Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * GoalOffsetMultiplier * GoalTarget) + ")</span>";
                }
            }


            //Re-commitments
            if (SelectedCampusId == 0)
            {
                int? item = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
                FROM MetricValue mv 
                WHERE mv.MetricValueType = 0 AND mv.MetricValueType = 0 AND (mv.MetricId = 13) AND mv.MetricValueDateTime >= '2015-09-01'").ToList<int?>()[0];

                int? itemGoalCurrent = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 13 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = DATEPART(YEAR,GETDATE())").ToList<int?>()[0];

                RecommitmentsGoal2020 = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 13 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = '2020'").ToList<int?>()[0].ToString();

                Recommitments = item.ToString();
                RecommitmentsGoalCurrent = itemGoalCurrent.ToString();

                decimal? goalProgress = item / (itemGoalCurrent * GoalOffsetMultiplier);
                if (goalProgress >= GoalTarget)
                {
                    RecommitmentsGoalProgress = "<span class='label label-success'>On Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * GoalOffsetMultiplier * GoalTarget) + ")</span>";
                }
                else
                {
                    RecommitmentsGoalProgress = "<span class='label label-danger'>Below Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * GoalOffsetMultiplier * GoalTarget) + ")</span>";
                }


            }
            else
            {
                int? item = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
                FROM MetricValue mv 
                WHERE mv.MetricValueType = 0 AND (mv.MetricId = 13) AND mv.MetricValueDateTime >= '2015-09-01' AND mv.EntityId = @CampusId; ", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0];

                int? itemGoalCurrent =
                    rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 13 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = DATEPART(YEAR,GETDATE()) AND mv.EntityId = @CampusId;",
                        new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0];

                RecommitmentsGoal2020 = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 13 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = '2020' AND mv.EntityId = @CampusId;", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0].ToString();

                Recommitments = item.ToString();
                RecommitmentsGoalCurrent = itemGoalCurrent.ToString();

                decimal? goalProgress = item / (itemGoalCurrent * GoalOffsetMultiplier);
                if (goalProgress >= GoalTarget)
                {
                    RecommitmentsGoalProgress = "<span class='label label-success'>On Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * GoalOffsetMultiplier * GoalTarget) + ")</span>";
                }
                else
                {
                    RecommitmentsGoalProgress = "<span class='label label-danger'>Below Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * GoalOffsetMultiplier * GoalTarget) + ")</span>";
                }

            }

            //Total Commitments
            if (SelectedCampusId == 0)
            {
                AllCommitments = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
                FROM MetricValue mv 
                WHERE mv.MetricValueType = 0 AND (mv.MetricId = 12 OR mv.MetricId = 13) AND mv.MetricValueDateTime >= '2015-09-01'").ToList<int?>()[0].ToString();
            }
            else
            {
                AllCommitments = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
                FROM MetricValue mv 
                WHERE mv.MetricValueType = 0 AND (mv.MetricId = 12 OR mv.MetricId = 13) AND mv.MetricValueDateTime >= '2015-09-01' AND mv.EntityId = @CampusId; ", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0].ToString();
            }

            //New Here Guests
            if (SelectedCampusId == 0)
            {
                int? item = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(SUM(YValue) as int) as att
                FROM MetricValue mv 
                WHERE mv.MetricValueType = 0 AND (mv.MetricId = 14) AND mv.MetricValueDateTime >= '2015-09-01'").ToList<int?>()[0];

                int? itemGoalCurrent =
                    rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 14 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = DATEPART(YEAR,GETDATE())").ToList<int?>()[0];

                NewHereGoal2020 = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 14 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = '2020'").ToList<int?>()[0].ToString();


                NewHere = item.ToString();
                NewHereGoalCurrent = itemGoalCurrent.ToString();

                decimal? goalProgress = item / (itemGoalCurrent * GoalOffsetMultiplier);
                if (goalProgress >= GoalTarget)
                {
                    NewHereGoalProgress = "<span class='label label-success'>On Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * GoalOffsetMultiplier * GoalTarget) + ")</span>";
                }
                else
                {
                    NewHereGoalProgress = "<span class='label label-danger'>Below Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * GoalOffsetMultiplier * GoalTarget) + ")</span>";
                }
            }

            else
            {
                int? item = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(SUM(YValue) as int) as att
                FROM MetricValue mv 
                WHERE mv.MetricValueType = 0 AND (mv.MetricId = 14) AND mv.MetricValueDateTime >= '2015-09-01' AND mv.EntityId = @CampusId; ", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0];

                int? itemGoalCurrent = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 14 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = DATEPART(YEAR,GETDATE()) AND mv.EntityId = @CampusId;", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0];

                NewHereGoal2020 = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 14 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = '2020' AND mv.EntityId = @CampusId;", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0].ToString();

                NewHere = item.ToString();
                NewHereGoalCurrent = itemGoalCurrent.ToString();

                decimal? goalProgress = item / (itemGoalCurrent * GoalOffsetMultiplier);
                if (goalProgress >= GoalTarget)
                {
                    NewHereGoalProgress = "<span class='label label-success'>On Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * GoalOffsetMultiplier * GoalTarget) + ")</span>";
                }
                else
                {
                    NewHereGoalProgress = "<span class='label label-danger'>Below Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * GoalOffsetMultiplier * GoalTarget) + ")</span>";
                }

            }

            //Partners
            if (SelectedCampusId == 0)
            {
                int? item = rockContext.Database.SqlQuery<int?>(@"SELECT TOP 5 CAST(ISNULL(SUM(YValue), 0) as int) as att
                FROM MetricValue mv 
                WHERE mv.MetricId = 20 AND mv.MetricValueType = 0
				AND DATEPART(YEAR,mv.MetricValueDateTime) = DATEPART(YEAR,GETDATE())
				AND DATEPART(Month,mv.MetricValueDateTime) = DATEPART(Month,GETDATE() -1)").ToList<int?>()[0];

                int? itemGoalCurrent = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 20 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = DATEPART(YEAR,GETDATE())").ToList<int?>()[0];

                PartnersGoal2020 = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 20 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = '2020'").ToList<int?>()[0].ToString();

                Partners = item.ToString();
                PartnersGoalCurrent = itemGoalCurrent.ToString();

                decimal? goalProgress = item / (itemGoalCurrent * SecondaryGoalOffsetMultiplier);
                if (goalProgress >= GoalTarget)
                {
                    PartnersGoalProgress = "<span class='label label-success'>On Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier * GoalTarget) + ")</span>";
                }
                else
                {
                    PartnersGoalProgress = "<span class='label label-danger'>Below Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier * GoalTarget) + ")</span>";
                }

            }
            else
            {
                int? item = rockContext.Database.SqlQuery<int?>(@"SELECT TOP 5 CAST(ISNULL(SUM(YValue), 0) as int) as att
                FROM MetricValue mv 
                WHERE mv.MetricId = 20 AND mv.MetricValueType = 0
				AND DATEPART(YEAR,mv.MetricValueDateTime) = DATEPART(YEAR,GETDATE())
				AND DATEPART(Month,mv.MetricValueDateTime) = DATEPART(Month,GETDATE() -1)
				AND mv.EntityId = @CampusId", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0];

                int? itemGoalCurrent =
                    rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 20 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = DATEPART(YEAR,GETDATE()) AND mv.EntityId = @CampusId;",
                        new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0];

                PartnersGoal2020 = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 20 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = '2020' AND mv.EntityId = @CampusId;", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0].ToString();

                Partners = item.ToString();
                PartnersGoalCurrent = itemGoalCurrent.ToString();

                decimal? goalProgress = item / (itemGoalCurrent * SecondaryGoalOffsetMultiplier);
                if (goalProgress >= GoalTarget)
                {
                    PartnersGoalProgress = "<span class='label label-success'>On Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier * GoalTarget) + ")</span>";
                }
                else
                {
                    PartnersGoalProgress = "<span class='label label-danger'>Below Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier * GoalTarget) + ")</span>";
                }
            }

            //Small Group Leaders
            if (SelectedCampusId == 0)
            {
                int? item =
                    rockContext.Database.SqlQuery<int?>(
                        @"SELECT TOP 1 CAST(ISNULL(SUM(YValue), 0) as int) as att
	                    FROM MetricValue mv 
	                    WHERE mv.MetricValueType = 0 AND (mv.MetricId = 18) AND DATEPART(month, mv.MetricValueDateTime) = DATEPART(month, GETDATE()) AND DATEPART(year, mv.MetricValueDateTime) = DATEPART(year, GETDATE())").ToList<int?>()[0];

                int? itemGoalCurrent = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 18 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = DATEPART(YEAR,GETDATE())").ToList<int?>()[0];

                SmallGroupLeadersGoal2020 = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 18 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = '2020'").ToList<int?>()[0].ToString();

                SmallGroupLeaders = item.ToString();
                SmallGroupLeadersGoalCurrent = itemGoalCurrent.ToString();

                decimal? goalProgress = item / (itemGoalCurrent * SecondaryGoalOffsetMultiplier);
                if (goalProgress >= GoalTarget)
                {
                    SmallGroupLeadersGoalProgress = "<span class='label label-success'>On Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier * GoalTarget) + ")</span>";
                }
                else
                {
                    SmallGroupLeadersGoalProgress = "<span class='label label-danger'>Below Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier * GoalTarget) + ")</span>";
                }

            }
            else
            {
                int? item =
                    rockContext.Database.SqlQuery<int?>(
                        @"SELECT TOP 1 CAST(ISNULL(SUM(YValue), 0) as int) as att
	                    FROM MetricValue mv 
	                    WHERE mv.MetricValueType = 0 AND (mv.MetricId = 18) AND DATEPART(month, mv.MetricValueDateTime) = DATEPART(month, GETDATE()) AND DATEPART(year, mv.MetricValueDateTime) = DATEPART(year, GETDATE()) 
                        AND mv.EntityId = @CampusId", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0];

                int? itemGoalCurrent = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 18 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = DATEPART(YEAR,GETDATE()) AND mv.EntityId = @CampusId;", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0];

                SmallGroupLeadersGoal2020 = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 18 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = '2020' AND mv.EntityId = @CampusId;", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0].ToString();

                SmallGroupLeaders = item.ToString();
                SmallGroupLeadersGoalCurrent = itemGoalCurrent.ToString();

                decimal? goalProgress = item / (itemGoalCurrent * SecondaryGoalOffsetMultiplier);
                if (goalProgress >= GoalTarget)
                {
                    SmallGroupLeadersGoalProgress = "<span class='label label-success'>On Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier * GoalTarget) + ")</span>";
                }
                else
                {
                    SmallGroupLeadersGoalProgress = "<span class='label label-danger'>Below Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier * GoalTarget) + ")</span>";
                }
            }



            //Volunteers
            if (SelectedCampusId == 0)
            {
                int? item =
                    rockContext.Database.SqlQuery<int?>(
                        @"SELECT TOP 1 CAST(ISNULL(SUM(YValue), 0) as int) as att
	                    FROM MetricValue mv 
	                    WHERE mv.MetricValueType = 0 AND (mv.MetricId = 16) AND DATEPART(month, mv.MetricValueDateTime) = DATEPART(month, GETDATE()) 
                        AND DATEPART(year, mv.MetricValueDateTime) = DATEPART(year, GETDATE()) ").ToList<int?>()[0];

                int? itemGoalCurrent = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 16 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = DATEPART(YEAR,GETDATE())").ToList<int?>()[0];

                VolunteersGoal2020 = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 16 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = '2020'").ToList<int?>()[0].ToString();

                Volunteers = item.ToString();
                VolunteersGoalCurrent = itemGoalCurrent.ToString();

                decimal? goalProgress = item / (itemGoalCurrent * SecondaryGoalOffsetMultiplier);
                if (goalProgress >= GoalTarget)
                {
                    VolunteersGoalProgress = "<span class='label label-success'>On Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier * GoalTarget) + ")</span>";
                }
                else
                {
                    VolunteersGoalProgress = "<span class='label label-danger'>Below Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier * GoalTarget) + ")</span>";
                }


            }
            else
            {
                int? item =
                    rockContext.Database.SqlQuery<int?>(
                        @"SELECT TOP 1 CAST(ISNULL(SUM(YValue), 0) as int) as att
	                    FROM MetricValue mv 
	                    WHERE mv.MetricValueType = 0 AND (mv.MetricId = 16) AND DATEPART(month, mv.MetricValueDateTime) = DATEPART(month, GETDATE()) AND DATEPART(year, mv.MetricValueDateTime) = DATEPART(year, GETDATE()) 
                        AND mv.EntityId = @CampusId", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0];

                int? itemGoalCurrent = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 16 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = DATEPART(YEAR,GETDATE()) AND mv.EntityId = @CampusId;", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0];

                VolunteersGoal2020 = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 16 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = '2020' AND mv.EntityId = @CampusId;", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0].ToString();

                Volunteers = item.ToString();
                VolunteersGoalCurrent = itemGoalCurrent.ToString();

                decimal? goalProgress = item / (itemGoalCurrent * SecondaryGoalOffsetMultiplier);
                if (goalProgress >= GoalTarget)
                {
                    VolunteersGoalProgress = "<span class='label label-success'>On Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier * GoalTarget) + ")</span>";
                }
                else
                {
                    VolunteersGoalProgress = "<span class='label label-danger'>Below Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier * GoalTarget) + ")</span>";
                }

            }

            

            //Total Involvement
            if (SelectedCampusId == 0)
            {
                Involvement =
                    rockContext.Database.SqlQuery<int?>(
                        @"SELECT COUNT(DISTINCT PersonId) as [count] FROM [GroupMember] gm
                JOIN [Group] g on gm.GroupId = g.Id
                WHERE ((g.GroupTypeId = 25 and GroupRoleId = 24) OR g.GroupTypeId = 42) and g.IsActive = 'true';")
                        .ToList<int?>()[0].ToString();
            }
            else
            {
                Involvement =
                    rockContext.Database.SqlQuery<int?>(
                        @"SELECT COUNT(DISTINCT PersonId) as [count] FROM [GroupMember] gm
                        JOIN [Group] g on gm.GroupId = g.Id
                        WHERE ((g.GroupTypeId = 25 and GroupRoleId = 24) OR g.GroupTypeId = 42) and g.IsActive = 'true' 
                        AND g.CampusId = @CampusId", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0].ToString();
            }




            //New to NewPointe
            if (SelectedCampusId == 0)
            {
                int? item =
                    rockContext.Database.SqlQuery<int?>(
                        @"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
                FROM MetricValue mv 
                WHERE mv.MetricValueType = 0 AND (mv.MetricId = 21) AND mv.MetricValueDateTime >= '2015-09-01'").ToList<int?>()[0];

                int? itemGoalCurrent = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 21 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = DATEPART(YEAR,GETDATE())").ToList<int?>()[0];

                NewtoNewPointeGoal2020 = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 21 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = '2020'").ToList<int?>()[0].ToString();

                NewtoNewPointe = item.ToString();
                NewtoNewPointeGoalCurrent = itemGoalCurrent.ToString();

                decimal? goalProgress = item / (itemGoalCurrent * GoalOffsetMultiplier);
                if (goalProgress >= GoalTarget)
                {
                    NewtoNewPointeGoalProgress = "<span class='label label-success'>On Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * GoalOffsetMultiplier * GoalTarget) + ")</span>";
                }
                else
                {
                    NewtoNewPointeGoalProgress = "<span class='label label-danger'>Below Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * GoalOffsetMultiplier * GoalTarget) + ")</span>";
                }


            }

            else
            {
                int? item =
                    rockContext.Database.SqlQuery<int?>(
                        @"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
                FROM MetricValue mv 
                WHERE mv.MetricValueType = 0 AND (mv.MetricId = 21) AND mv.MetricValueDateTime >= '2015-09-01'
				AND mv.EntityId = @CampusId", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0];

                int? itemGoalCurrent = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 21 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = DATEPART(YEAR,GETDATE()) AND mv.EntityId = @CampusId;", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0];

                NewtoNewPointeGoal2020 = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 21 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = '2020' AND mv.EntityId = @CampusId;", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0].ToString();

                NewtoNewPointe = item.ToString();
                NewtoNewPointeGoalCurrent = itemGoalCurrent.ToString();

                decimal? goalProgress = item / (itemGoalCurrent * GoalOffsetMultiplier);
                if (goalProgress >= GoalTarget)
                {
                    NewtoNewPointeGoalProgress = "<span class='label label-success'>On Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * GoalOffsetMultiplier * GoalTarget) + ")</span>";
                }
                else
                {
                    NewtoNewPointeGoalProgress = "<span class='label label-danger'>Below Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * GoalOffsetMultiplier * GoalTarget) + ")</span>";
                }


            }

            //Discover Groups
            if (SelectedCampusId == 0)
            {
                int? item =
                    rockContext.Database.SqlQuery<int?>(
                        @"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
                FROM MetricValue mv 
                WHERE mv.MetricValueType = 0 AND (mv.MetricId = 22) AND mv.MetricValueDateTime >= '2015-09-01'").ToList<int?>()[0];

                int? itemGoalCurrent = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 22 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = DATEPART(YEAR,GETDATE())").ToList<int?>()[0];

                DiscoverGroupsGoal2020 = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 22 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = '2020'").ToList<int?>()[0].ToString();

                DiscoverGroups = item.ToString();
                DiscoverGroupsGoalCurrent = itemGoalCurrent.ToString();

                decimal? goalProgress = item / (itemGoalCurrent * GoalOffsetMultiplier);
                if (goalProgress >= GoalTarget)
                {
                    DiscoverGroupsGoalProgress = "<span class='label label-success'>On Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * GoalOffsetMultiplier * GoalTarget) + ")</span>";
                }
                else
                {
                    DiscoverGroupsGoalProgress = "<span class='label label-danger'>Below Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * GoalOffsetMultiplier * GoalTarget) + ")</span>";
                }

            }


            else
            {
                int? item =
                   rockContext.Database.SqlQuery<int?>(
                       @"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
                FROM MetricValue mv 
                WHERE mv.MetricValueType = 0 AND (mv.MetricId = 22) AND mv.MetricValueDateTime >= '2015-09-01'
				AND mv.EntityId = @CampusId", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0];

                int? itemGoalCurrent = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 22 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = DATEPART(YEAR,GETDATE()) AND mv.EntityId = @CampusId;", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0];

               DiscoverGroupsGoal2020 = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 22 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = '2020' AND mv.EntityId = @CampusId;", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0].ToString();

                DiscoverGroups = item.ToString();
                DiscoverGroupsGoalCurrent = itemGoalCurrent.ToString();

                decimal? goalProgress = item / (itemGoalCurrent * GoalOffsetMultiplier);
                if (goalProgress >= GoalTarget)
                {
                    DiscoverGroupsGoalProgress = "<span class='label label-success'>On Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * GoalOffsetMultiplier * GoalTarget) + ")</span>";
                }
                else
                {
                    DiscoverGroupsGoalProgress = "<span class='label label-danger'>Below Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * GoalOffsetMultiplier * GoalTarget) + ")</span>";
                }

            }

            //Small Group Participants
            if (SelectedCampusId == 0)
            {
                int? item =
                    rockContext.Database.SqlQuery<int?>(
                        @"SELECT TOP 1 CAST(ISNULL(SUM(YValue), 0) as int) as att
	                    FROM MetricValue mv 
	                    WHERE mv.MetricValueType = 0 AND (mv.MetricId = 17) AND DATEPART(month, mv.MetricValueDateTime) = DATEPART(month, GETDATE()) 
                        AND DATEPART(year, mv.MetricValueDateTime) = DATEPART(year, GETDATE()) ").ToList<int?>()[0];

                int? itemGoalCurrent = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 17 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = DATEPART(YEAR,GETDATE())").ToList<int?>()[0];

                SmallGroupParticipantsGoal2020 = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 17 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = '2020'").ToList<int?>()[0].ToString();

                SmallGroupParticipants = item.ToString();
                SmallGroupParticipantsGoalCurrent = itemGoalCurrent.ToString();

                decimal? goalProgress = item / (itemGoalCurrent * SecondaryGoalOffsetMultiplier);
                if (goalProgress >= GoalTarget)
                {
                    SmallGroupParticipantsGoalProgress = "<span class='label label-success'>On Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier * GoalTarget) + ")</span>";
                }
                else
                {
                    SmallGroupParticipantsGoalProgress = "<span class='label label-danger'>Below Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier * GoalTarget) + ")</span>";
                }

            }
            else
            {
                int? item =
                    rockContext.Database.SqlQuery<int?>(
                        @"SELECT TOP 1 CAST(ISNULL(SUM(YValue), 0) as int) as att
	                    FROM MetricValue mv 
	                    WHERE mv.MetricValueType = 0 AND (mv.MetricId = 17) AND DATEPART(month, mv.MetricValueDateTime) = DATEPART(month, GETDATE()) AND DATEPART(year, mv.MetricValueDateTime) = DATEPART(year, GETDATE()) 
                        AND mv.EntityId = @CampusId", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0];

                int? itemGoalCurrent = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 17 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = DATEPART(YEAR,GETDATE()) AND mv.EntityId = @CampusId;", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0];

                SmallGroupParticipantsGoal2020 = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(ISNULL(SUM(YValue), 0) as int) as att
	            FROM MetricValue mv 
                WHERE mv.MetricId = 17 AND MetricValueType = 1
	            AND DATEPART(YEAR,mv.MetricValueDateTime) = '2020' AND mv.EntityId = @CampusId;", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0].ToString();

                SmallGroupParticipants = item.ToString();
                SmallGroupParticipantsGoalCurrent = itemGoalCurrent.ToString();

                decimal? goalProgress = item / (itemGoalCurrent * SecondaryGoalOffsetMultiplier);
                if (goalProgress >= GoalTarget)
                {
                    SmallGroupParticipantsGoalProgress = "<span class='label label-success'>On Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier * GoalTarget) + ")</span>";
                }
                else
                {
                    SmallGroupParticipantsGoalProgress = "<span class='label label-danger'>Below Target (" + Math.Truncate(itemGoalCurrent.GetValueOrDefault() * SecondaryGoalOffsetMultiplier * GoalTarget) + ")</span>";
                }

                }



            //All Assimilation
            if (SelectedCampusId == 0)
            {
                Assimilation = rockContext.Database.SqlQuery<int?>(@"SELECT SUM(thetotal) FROM
                (
                SELECT COUNT(DISTINCT PersonAliasId) as thetotal FROM Attendance a
                JOIN [Group] g on a.GroupId = g.Id
                WHERE (g.GroupTypeId = 62 OR g.GroupTypeId = 63 OR g.GroupTypeId = 65 OR g.GroupTypeId = 66 OR g.GroupTypeId = 67  OR g.GroupTypeId = 72 OR g.GroupTypeId = 86
                OR g.GroupTypeId = 96 OR g.GroupTypeId = 97 OR g.GroupTypeId = 98 OR g.GroupTypeId = 108 OR g.GroupTypeId = 113 OR g.GroupTypeId = 120
                OR g.GroupTypeId = 142 OR g.GroupTypeId = 143  OR g.GroupTypeId = 144 OR g.GroupTypeId = 64 OR g.GroupTypeId = 71 OR g.GroupTypeId = 121 OR g.GroupTypeId = 122 OR g.GroupTypeId = 123 OR g.GroupTypeId = 124 OR g.GroupTypeId = 125)
                AND a.StartDateTime > '2015-09-01'
                UNION
                SELECT COUNT(DISTINCT PersonId) as thetotal FROM [GroupMember] gm
                JOIN [Group] g on gm.GroupId = g.Id
                WHERE g.GroupTypeId = 25 and g.IsActive = 'true'
                ) s").ToList<int?>()[0].ToString();
            }
            else
            {
                Assimilation = rockContext.Database.SqlQuery<int?>(@"SELECT SUM(thetotal) FROM
                (
                SELECT COUNT(DISTINCT PersonAliasId) as thetotal FROM Attendance a
                JOIN [Group] g on a.GroupId = g.Id
                WHERE (g.GroupTypeId = 62 OR g.GroupTypeId = 63 OR g.GroupTypeId = 65 OR g.GroupTypeId = 66 OR g.GroupTypeId = 67  OR g.GroupTypeId = 72 OR g.GroupTypeId = 86
                OR g.GroupTypeId = 96 OR g.GroupTypeId = 97 OR g.GroupTypeId = 98 OR g.GroupTypeId = 108 OR g.GroupTypeId = 113 OR g.GroupTypeId = 120
                OR g.GroupTypeId = 142 OR g.GroupTypeId = 143  OR g.GroupTypeId = 144 OR g.GroupTypeId = 64 OR g.GroupTypeId = 71 OR g.GroupTypeId = 121 OR g.GroupTypeId = 122 OR g.GroupTypeId = 123 OR g.GroupTypeId = 124 OR g.GroupTypeId = 125)
                AND a.StartDateTime > '2015-09-01' AND g.CampusId = @CampusId
                UNION
                SELECT COUNT(DISTINCT PersonId) as thetotal FROM [GroupMember] gm
                JOIN [Group] g on gm.GroupId = g.Id
                WHERE g.GroupTypeId = 25 and g.IsActive = 'true' AND g.CampusId = @CampusId
                ) s", new SqlParameter("CampusId", SelectedCampusId)).ToList<int?>()[0].ToString();
            }

            //Inactive Follow-up
            if (SelectedCampusId == 0)
            {
                 iInactiveFollowupComplete = rockContext.Database.SqlQuery<int?>(@"SELECT COUNT(wf.Id) as Workflows
                  FROM [rock-production].[dbo].[Workflow] wf 
                  JOIN AttributeValue av ON wf.Id = av.EntityID
                  WHERE wf.WorkflowTypeId = 120 AND av.AttributeId = 10213 AND wf.[Status] = 'Completed'
                  AND Month(ActivatedDateTime) = Month(GETDATE()) AND Year(ActivatedDateTime) = Year(GETDATE())")
                    .ToList<int?>()[0];
            }
            else
            {
                 iInactiveFollowupComplete = rockContext.Database.SqlQuery<int?>(@"SELECT COUNT(wf.Id) as Workflows
                  FROM [rock-production].[dbo].[Workflow] wf 
                  JOIN AttributeValue av ON wf.Id = av.EntityID
                  WHERE wf.WorkflowTypeId = 120 AND av.AttributeId = 10213 AND wf.[Status] = 'Completed' AND av.Value = @CampusName
                  AND Month(ActivatedDateTime) = Month(GETDATE()) AND Year(ActivatedDateTime) = Year(GETDATE())", new SqlParameter("CampusName", SelectedCampus)).ToList<int?>()[0];
            }

            if (SelectedCampusId == 0)
            {
                 iInactiveFollowupIncomplete = rockContext.Database.SqlQuery<int?>(@"SELECT COUNT(wf.Id) as Workflows
                  FROM [rock-production].[dbo].[Workflow] wf 
                  JOIN AttributeValue av ON wf.Id = av.EntityID
                  WHERE wf.WorkflowTypeId = 120 AND av.AttributeId = 10213 AND (wf.[Status] = 'New' OR wf.[Status] = 'In Progress' OR wf.[Status] = 'Transferred' OR wf.[Status] = 'Active')
                  AND Month(ActivatedDateTime) = Month(GETDATE()) AND Year(ActivatedDateTime) = Year(GETDATE())")
                    .ToList<int?>()[0];
            }
            else
            {
                 iInactiveFollowupIncomplete = rockContext.Database.SqlQuery<int?>(@"SELECT COUNT(wf.Id) as Workflows
                  FROM [rock-production].[dbo].[Workflow] wf 
                  JOIN AttributeValue av ON wf.Id = av.EntityID
                  WHERE wf.WorkflowTypeId = 120 AND av.AttributeId = 10213 AND (wf.[Status] = 'New' OR wf.[Status] = 'In Progress' OR wf.[Status] = 'Transferred' OR wf.[Status] = 'Active')
                  AND av.Value = @CampusName
                  AND Month(ActivatedDateTime) = Month(GETDATE()) AND Year(ActivatedDateTime) = Year(GETDATE())", new SqlParameter("CampusName", SelectedCampus)).ToList<int?>()[0];
            }

            InactiveFollowupComplete = iInactiveFollowupComplete.ToString();
            InactiveFollowupIncomplete = iInactiveFollowupIncomplete.ToString();
            int? iInactiveFollowupAll = iInactiveFollowupComplete + iInactiveFollowupIncomplete;
            InactiveFollowupAll = iInactiveFollowupAll.ToString();

            decimal? followupProgress = iInactiveFollowupComplete / (iInactiveFollowupComplete + iInactiveFollowupIncomplete);
            if (followupProgress >= iInactiveFollowupAll)
            {
                InactiveFollowupGoalProgress = "<span class='label label-success'>On Target (" + iInactiveFollowupAll.ToString() + ")</span>";
            }
            else
            {
                InactiveFollowupGoalProgress = "<span class='label label-danger'>Below Target (" + iInactiveFollowupAll.ToString() + ")</span>";
            }




            // ToDo: Finish this section -----------

            //Composite Scores

            Decimal dCompAttendance = Decimal.Parse(AttendanceAveragePast6Weeks) / Decimal.Parse(AttendanceGoalCurrent);
            CompAttendance = String.Format("{0:P2}", dCompAttendance);

            Decimal dCompVolunteers = Decimal.Parse(Volunteers) / Decimal.Parse(VolunteersGoalCurrent);
            CompVolunteers = String.Format("{0:P2}", dCompVolunteers);

            Decimal dCompGroupParticipants = Decimal.Parse(SmallGroupParticipants) / Decimal.Parse(SmallGroupParticipantsGoalCurrent);
            CompGroupParticipants = String.Format("{0:P2}", dCompGroupParticipants);

            Decimal dCompositeScore = (dCompAttendance + (dCompVolunteers * (decimal)1.5) + (dCompGroupParticipants * (decimal)1.5)) / 4;
            CompositeScore = String.Format("{0:P2}", dCompositeScore);


            // --------------------------------------



        }
    }
}