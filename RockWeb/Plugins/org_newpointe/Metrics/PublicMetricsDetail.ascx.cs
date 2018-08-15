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
    [DisplayName("Public Weekly Metrics")]
    [Category("NewPointe Metrics")]
    [Description("Public Weekly Metrics")]
    [DateField("Fiscal Year Start Date","Select the date the Fiscal Year starts", true)]


    public partial class PublicMetricsDetail : Rock.Web.UI.RockBlock
    {
        public string FiscalYearStartDate;
        public string SelectedCampus = string.Empty;
        public int SelectedCampusId;
        public string SundayDate;
        public string SundayDateSQLFormatted;

        public string AttendanceAverage;
        public string AttendanceLastWeekAll;
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




            if (!Page.IsPostBack)
            {
                //Get Attributes
                FiscalYearStartDate = GetAttributeValue("FiscalYearStartDate").ToString();

                //Generate Campus List
                string[] campusList = {"Akron Campus", "Canton Campus", "Coshocton Campus", "Dover Campus", "Millersburg Campus", "Wooster Campus"};
                cpCampus.DataSource = campusList;
                cpCampus.DataBind();

                //Get the campus of the currently logged in person
                PersonService personService = new PersonService(rockContext);
                var personObject = personService.Get(CurrentPerson.Guid);
                var campus = personObject.GetFamilies().FirstOrDefault().Campus ?? new Campus();
                SelectedCampusId = campus.Id;
                cpCampus.SelectedValue = campus.Name;
                SelectedCampus = campus.Name;
                

            }

            GetOrgMetrics();
            GetCampusMetrics();

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
            }

            GetCampusMetrics();

        }



        protected void GetOrgMetrics()
        {

            //Attendance Last Week - All Environments All Campuses
                AttendanceLastWeekAll = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(SUM(YValue) as int) as att
                FROM MetricValue mv
                WHERE mv.MetricValueType = 0 AND (mv.MetricId = 2 OR mv.MetricId = 3 OR mv.MetricId = 4 OR mv.MetricId = 5) AND(DATEPART(isowk, mv.MetricValueDateTime) = DATEPART(isowk, @SundayDate))
                AND(DATEPART(yy, mv.MetricValueDateTime) = DATEPART(yy, @SundayDate)) AND mv.EntityId != 8; ", new SqlParameter("SundayDate", SundayDateSQLFormatted)).ToList<int?>()[0].ToString();


            //Attendance Last Week Last Year - All Environments All Campuses
            AttendanceLastWeekLastYearAll = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(SUM(YValue) as int) as att
                FROM MetricValue mv
                WHERE mv.MetricValueType = 0 AND (mv.MetricId = 2 OR mv.MetricId = 3 OR mv.MetricId = 4 OR mv.MetricId = 5) AND(DATEPART(isowk, mv.MetricValueDateTime) = DATEPART(isowk, @SundayDate) - 1)
                AND(DATEPART(yy, mv.MetricValueDateTime) = DATEPART(yy, @SundayDate) -1) AND mv.EntityId != 8; ", new SqlParameter("SundayDate", SundayDateSQLFormatted)).ToList<int?>()[0].ToString();


            //Giving Last Week - All Campuses
            GivingLastWeek = rockContext.Database.SqlQuery<decimal>(@"	SELECT ISNULL(SUM(Amount), 0) as 'Total Giving' FROM FinancialTransaction ft
		        RIGHT JOIN FinancialTransactionDetail ftd ON ftd.TransactionId = ft.Id
		        JOIN FinancialAccount fa ON ftd.AccountId = fa.Id
		        JOIN FinancialBatch fb ON ft.BatchId = fb.Id 
	            WHERE BatchStartDateTime >= @p0 + ' 00:00:00' AND BatchStartDateTime <= @p1 + ' 23:59:59'
	            AND fa.Name LIKE '%General%'; ", FinancialStartDate, FinancialEndDate).ToList<decimal>()[0].ToString("C0");

            //Giving 2 Weeks Ago - All Campuses
            GivingTwoWeeksAgo = rockContext.Database.SqlQuery<decimal>(@"	SELECT ISNULL(SUM(Amount), 0) as 'Total Giving' FROM FinancialTransaction ft
		        RIGHT JOIN FinancialTransactionDetail ftd ON ftd.TransactionId = ft.Id
		        JOIN FinancialAccount fa ON ftd.AccountId = fa.Id
		        JOIN FinancialBatch fb ON ft.BatchId = fb.Id 
	            WHERE BatchStartDateTime >= @p0 + ' 00:00:00' AND BatchStartDateTime <= @p1 + ' 23:59:59'
	            AND fa.Name LIKE '%General%'; ", FinancialStartDateLastWeek, FinancialEndDateLastWeek).ToList<decimal>()[0].ToString("C0");

            //Giving YTD - All Campuses
            GivingYtd = rockContext.Database.SqlQuery<decimal>(@"	SELECT ISNULL(SUM(Amount), 0)  as 'Total Giving' FROM FinancialTransaction ft
		        RIGHT JOIN FinancialTransactionDetail ftd ON ftd.TransactionId = ft.Id
		        JOIN FinancialAccount fa ON ftd.AccountId = fa.Id
		        JOIN FinancialBatch fb ON ft.BatchId = fb.Id 
	            WHERE BatchStartDateTime >= @p0 + ' 00:00:00' AND BatchStartDateTime <= @p1 + ' 23:59:59'
	            AND fa.Name LIKE '%General%'; ", "2015-09-01", FinancialEndDate).ToList<decimal>()[0].ToString("C0");

        }



        protected void GetCampusMetrics()
        {

            //Attendance Last Week - Selected Campus
            AttendanceLastWeekCampus = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(SUM(YValue) as int) as att
                FROM MetricValue mv
                WHERE mv.MetricValueType = 0 AND (mv.MetricId = 2 OR mv.MetricId = 3 OR mv.MetricId = 4 OR mv.MetricId = 5) AND(DATEPART(isowk, mv.MetricValueDateTime) = DATEPART(isowk, @p1))
                AND(DATEPART(yy, mv.MetricValueDateTime) = DATEPART(yy, @p1)) AND mv.EntityId = @p0; ", SelectedCampusId, SundayDateSQLFormatted).ToList<int?>()[0].ToString();

            //Attendance Last Week Last Year - Selected Campus
            AttendanceLastWeekLastYearCampus = rockContext.Database.SqlQuery<int?>(@"SELECT CAST(SUM(YValue) as int) as att
                FROM MetricValue mv
                WHERE mv.MetricValueType = 0 AND (mv.MetricId = 2 OR mv.MetricId = 3 OR mv.MetricId = 4 OR mv.MetricId = 5) AND(DATEPART(isowk, mv.MetricValueDateTime) = DATEPART(isowk, @p1))
                AND(DATEPART(yy, mv.MetricValueDateTime) = DATEPART(yy, @p1) -1) AND mv.EntityId = @p0; ", SelectedCampusId, SundayDateSQLFormatted).ToList<int?>()[0].ToString();


            //Giving Last Week - Selected Campus
            GivingLastWeekCampus = rockContext.Database.SqlQuery<decimal>(@"	SELECT ISNULL(SUM(Amount), 0)  as 'Total Giving' FROM FinancialTransaction ft
		        RIGHT JOIN FinancialTransactionDetail ftd ON ftd.TransactionId = ft.Id
		        JOIN FinancialAccount fa ON ftd.AccountId = fa.Id
		        JOIN FinancialBatch fb ON ft.BatchId = fb.Id 
	            WHERE BatchStartDateTime >= @p0 + ' 00:00:00' AND BatchStartDateTime <= @p1 + ' 23:59:59'
	            AND fa.Name LIKE '%General%' AND fa.CampusId = @p2; ", FinancialStartDate, FinancialEndDate, SelectedCampusId).ToList<decimal>()[0].ToString("C0");

            //Giving 2 Weeks Ago - All Campuses
            GivingTwoWeeksAgoCampus = rockContext.Database.SqlQuery<decimal>(@"	SELECT ISNULL(SUM(Amount), 0)  as 'Total Giving' FROM FinancialTransaction ft
		        RIGHT JOIN FinancialTransactionDetail ftd ON ftd.TransactionId = ft.Id
		        JOIN FinancialAccount fa ON ftd.AccountId = fa.Id
		        JOIN FinancialBatch fb ON ft.BatchId = fb.Id 
	            WHERE BatchStartDateTime >= @p0 + ' 00:00:00' AND BatchStartDateTime <= @p1 + ' 23:59:59'
	            AND fa.Name LIKE '%General%' AND fa.CampusId = @p2; ", FinancialStartDateLastWeek, FinancialEndDateLastWeek, SelectedCampusId).ToList<decimal>()[0].ToString("C0");

            //Giving YTD - Selected Campus
            GivingYtdCampus = rockContext.Database.SqlQuery<decimal>(@"	SELECT ISNULL(SUM(Amount), 0)  as 'Total Giving' FROM FinancialTransaction ft
		        RIGHT JOIN FinancialTransactionDetail ftd ON ftd.TransactionId = ft.Id
		        JOIN FinancialAccount fa ON ftd.AccountId = fa.Id
		        JOIN FinancialBatch fb ON ft.BatchId = fb.Id 
	            WHERE BatchStartDateTime >= @p0 + ' 00:00:00' AND BatchStartDateTime <= @p1 + ' 23:59:59'
	            AND fa.Name LIKE '%General%' AND fa.CampusId = @p2; ", "2015-09-01", FinancialEndDate, SelectedCampusId).ToList<decimal>()[0].ToString("C0");


        }
    }
}