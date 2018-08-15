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
    [DisplayName( "Composite Index Scores" )]
    [Category( "NewPointe Metrics" )]
    [Description( "Composite Index Scores" )]
    [DateField( "Fiscal Year Start Date", "Select the date the Fiscal Year starts", true )]


    public partial class CompositeScores : Rock.Web.UI.RockBlock
    {

        public string SelectedCampus = string.Empty;
        public int SelectedCampusId;
        public double? CompositeScore;

        RockContext rockContext = new RockContext();


        protected void Page_Load( object sender, EventArgs e )
        {



            if ( !Page.IsPostBack )
            {

                //Generate Campus List
                string[] campusList = { "Canton Campus", "Coshocton Campus", "Dover Campus", "Millersburg Campus", "Wooster Campus" };
                cpCampus.DataSource = campusList;
                cpCampus.DataBind();

                //Get the campus of the currently logged in person
                PersonService personService = new PersonService( rockContext );
                var personObject = personService.Get( CurrentPerson.Guid );
                var campus = personObject.GetFamilies().FirstOrDefault().Campus ?? new Campus();
                SelectedCampusId = campus.Id;
                cpCampus.SelectedValue = campus.Name;
                SelectedCampus = campus.Name;

            }

            CalulateMetrics();

        }


        protected void cpCampus_OnSelectionChanged( object sender, EventArgs e )
        {

            SelectedCampus = cpCampus.SelectedValue;
            switch ( SelectedCampus )
            {
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

            CalulateMetrics();

        }



        protected void CalulateMetrics()
        {
            
            DateTime start2020 = new DateTime( 2020, 1, 1 );
            DateTime end2020 = new DateTime( 2020, 12, 31 );
            
            var _now = DateTime.Now.SundayDate().AddDays( -7 * 3 );

            var CurrentMonthInFiscalYear = (( _now.Month - 8 ) % 12) +1;
            var GoalOffsetMultiplier = ( 1.0 / 12 ) * CurrentMonthInFiscalYear;
            var SecondaryGoalOffsetMultiplier = 0.88 + ( CurrentMonthInFiscalYear * 0.01 );

            var _6wkEnd = _now;
            var _6wkStart = _6wkEnd.AddDays( -7 * 6 );
            var _6wkLYEnd = _6wkEnd.AddYears( -1 );
            var _6wkLYStart = _6wkLYEnd.AddDays( -7 * 6 );

            var _8wkEnd = _now;
            var _8wkStart = _6wkEnd.AddDays( -7 * 8 );
            var _8wkLYEnd = _6wkEnd.AddYears( -1 );
            var _8wkLYStart = _6wkLYEnd.AddDays( -7 * 8 );

            DateTime fiscalYearStartDate;
            var fiscalYearStartDateFromAttribute = GetAttributeValue( "FiscalYearStartDate" ).AsDateTime();
            if( fiscalYearStartDateFromAttribute.HasValue )
            {
                fiscalYearStartDate = new DateTime( _now.Year, fiscalYearStartDateFromAttribute.Value.Month, fiscalYearStartDateFromAttribute.Value.Day );
            }
            else
            {
                fiscalYearStartDate = new DateTime( _now.Year, 1, 1 );
            }
            if(fiscalYearStartDate > _now)
            {
                fiscalYearStartDate.AddYears( -1 );
            }
            
            var _fyEnd = _now;
            var _fyStart = fiscalYearStartDate;



            List<MetricRow> metricList = new List<MetricRow>();
            MetricValueService mvServ = new MetricValueService( rockContext );
            var metric = mvServ.Queryable().Where( mv => mv.MetricValuePartitions.Where( mvp => mvp.MetricPartition.EntityTypeId == 67 ).Select( mvp => mvp.EntityId ).Contains( SelectedCampusId ) );
            var metric_Goal = metric.Where( mv => mv.MetricValueType == MetricValueType.Goal );
            var metric_Measure = metric.Where( mv => mv.MetricValueType == MetricValueType.Measure );

            var metric_Att_Adult = new MetricRow();
            metric_Att_Adult.SecondaryGoalOffsetMultiplier = SecondaryGoalOffsetMultiplier;
            metric_Att_Adult.Name = "6wk Average Adult Attendance";
            metric_Att_Adult.YearToDateValue = ( int? ) metric_Measure.Where( mv => mv.MetricId == 2 && mv.MetricValueDateTime >= _6wkStart && mv.MetricValueDateTime < _6wkEnd ).ToList().GroupBy( mv => mv.MetricValueDateTime.Value.SundayDate() ).Select( mv => mv.Sum( m => m.YValue ) ).Average() ?? 0;
            metric_Att_Adult.LastYearToDateValue = ( int? ) metric_Measure.Where( mv => mv.MetricId == 2 && mv.MetricValueDateTime >= _6wkLYStart && mv.MetricValueDateTime < _6wkLYEnd ).ToList().GroupBy( mv => mv.MetricValueDateTime.Value.SundayDate() ).Select( mv => mv.Sum( m => m.YValue ) ).Average() ?? 0;
            metric_Att_Adult.Goal = ( int? ) metric_Goal.Where(mv => mv.MetricId == 2 && mv.MetricValueDateTime >= start2020 && mv.MetricValueDateTime <= end2020 ).Sum( mv => mv.YValue ) ?? 0;
            metricList.Add( metric_Att_Adult );

            var metric_Att_Kid = new MetricRow();
            metric_Att_Kid.SecondaryGoalOffsetMultiplier = SecondaryGoalOffsetMultiplier;
            metric_Att_Kid.Name = "6wk Average Kids Attendance";
            metric_Att_Kid.YearToDateValue = ( int? ) metric_Measure.Where( mv => new int[] { 3, 4 }.Contains( mv.MetricId ) && mv.MetricValueDateTime >= _6wkStart && mv.MetricValueDateTime < _6wkEnd ).ToList().GroupBy( mv => mv.MetricValueDateTime.Value.SundayDate() ).Select( mv => mv.Sum( m => m.YValue ) ).Average() ?? 0;
            metric_Att_Kid.LastYearToDateValue = ( int? ) metric_Measure.Where( mv => new int[] { 3, 4 }.Contains( mv.MetricId ) && mv.MetricValueDateTime >= _6wkLYStart && mv.MetricValueDateTime < _6wkLYEnd ).ToList().GroupBy( mv => mv.MetricValueDateTime.Value.SundayDate() ).Select( mv => mv.Sum( m => m.YValue ) ).Average() ?? 0;
            metric_Att_Kid.Goal = ( int? ) metric_Goal.Where( mv => new int[] { 3, 4 }.Contains( mv.MetricId ) && mv.MetricValueDateTime >= start2020 && mv.MetricValueDateTime <= end2020 ).Sum( mv => mv.YValue ) ?? 0;
            metricList.Add( metric_Att_Kid );

            var metric_Att_Stud = new MetricRow();
            metric_Att_Stud.SecondaryGoalOffsetMultiplier = SecondaryGoalOffsetMultiplier;
            metric_Att_Stud.Name = "6wk Average Students Attendance";
            metric_Att_Stud.YearToDateValue = ( int? ) metric_Measure.Where( mv => new int[] { 5 }.Contains( mv.MetricId ) && mv.MetricValueDateTime >= _6wkStart && mv.MetricValueDateTime < _6wkEnd ).ToList().GroupBy( mv => mv.MetricValueDateTime.Value.SundayDate() ).Select( mv => mv.Sum( m => m.YValue ) ).Average() ?? 0;
            metric_Att_Stud.LastYearToDateValue = ( int? ) metric_Measure.Where( mv => new int[] { 5 }.Contains( mv.MetricId ) && mv.MetricValueDateTime >= _6wkLYStart && mv.MetricValueDateTime < _6wkLYEnd ).ToList().GroupBy( mv => mv.MetricValueDateTime.Value.SundayDate() ).Select( mv => mv.Sum( m => m.YValue ) ).Average() ?? 0;
            metric_Att_Stud.Goal = ( int? ) metric_Goal.Where( mv => new int[] { 5 }.Contains( mv.MetricId ) && mv.MetricValueDateTime >= start2020 && mv.MetricValueDateTime <= end2020 ).Sum( mv => mv.YValue ) ?? 0;
            metricList.Add( metric_Att_Stud );

            var metric_Att_HS = new MetricRow();
            metric_Att_HS.SecondaryGoalOffsetMultiplier = SecondaryGoalOffsetMultiplier;
            metric_Att_HS.Name = "6wk Average High School Attendance";
            metric_Att_HS.YearToDateValue = ( int? ) metric_Measure.Where( mv => new int[] { 23 }.Contains( mv.MetricId ) && mv.MetricValueDateTime >= _6wkStart && mv.MetricValueDateTime < _6wkEnd ).ToList().GroupBy( mv => mv.MetricValueDateTime.Value.SundayDate() ).Select( mv => mv.Sum( m => m.YValue ) ).Average() ?? 0;
            metric_Att_HS.LastYearToDateValue = ( int? ) metric_Measure.Where( mv => new int[] { 23 }.Contains( mv.MetricId ) && mv.MetricValueDateTime >= _6wkLYStart && mv.MetricValueDateTime < _6wkLYEnd ).ToList().GroupBy( mv => mv.MetricValueDateTime.Value.SundayDate() ).Select( mv => mv.Sum( m => m.YValue ) ).Average() ?? 0;
            metric_Att_HS.Goal = ( int? ) metric_Goal.Where( mv => new int[] { 23 }.Contains( mv.MetricId ) && mv.MetricValueDateTime >= start2020 && mv.MetricValueDateTime <= end2020 ).Sum( mv => mv.YValue ) ?? 0;
            metricList.Add( metric_Att_HS );



            gMetricsChart.DataSource = metricList;
            gMetricsChart.DataBind();
        }
    }

    public class MetricRow
    {
        public string Name { get; set; }
        public int YearToDateValue { get; set; }
        public int LastYearToDateValue { get; set; }
        public int YearToDateGrowth { get { return LastYearToDateValue != 0 ? ( int ) ( ( ( ( double ) YearToDateValue / LastYearToDateValue ) - 1 ) * 100 ) : 0; } }
        public int Goal { get; set; }
        public double SecondaryGoalOffsetMultiplier { get; set; }
        public int GoalProgress { get { return ( Goal != 0 && SecondaryGoalOffsetMultiplier != 0 ) ? ( int ) ( YearToDateValue / ( Goal * SecondaryGoalOffsetMultiplier ) * 100 ) : 0; } }
    }
}