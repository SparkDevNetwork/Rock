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

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

namespace RockWeb.Blocks.Reporting
{
    [DisplayName( "Insights" )]
    [Category( "Reporting" )]
    [Description( "Shows high-level statistics of the Rock database." )]

    [Rock.SystemGuid.BlockTypeGuid( "B215F5FA-410C-4674-8C47-43DC40AF9F67" )]
    public partial class Insights : RockBlock
    {
        #region Fields

        private readonly Dictionary<string, string> LabelColorMap = new Dictionary<string, string>()
        {
            { "Male", "#FB7185" },
            { "Female", "#818CF8" },
            { "Unknown", "#CCCCCC" },

            { "Attendee", "#818CF8" },
            { "Visitor", "#4ADE80" },
            { "Member", "#FBBF24" },
            { "Participant", "#FB7185" },
            { "Prospect", "#22D3EE" },

            { "Married", "#EEAAAA" },
            { "Single", "#818CF8" },
            { "Divorced", "#FBBF24" },

            { "Non Hispanic or Latino", "#4ADE80" },
            { "Hispanic or Latino", "#FB7185" },

            { "White", "#818CF8" },
            { "American Indian or Alaskan Native", "#FBBF24" },
            { "Black or African American", "#4ADE80" },
            { "Native Hawaiian or Pacific Islander", "#E879F9" },
            { "Other", "#E879F9" },
            { "Asian", "#38BDF8" },

            { "Age", "#818CF8" },
            { "Gender", "#38BDF8" },
            { "Active Email", "#22D3EE" },
            { "Mobile Phone", "#2DD4BF" },
            { "Marital Status", "#4ADE80" },
            { "Photo", "#FBBE24" },
            { "Date of Birth", "#FB923C" },
            { "Home Address", "#F87171" },

            { "Disc", "#818CF8" },
            { "Motivators", "#38BDF8" },
            { "EQ", "#22D3EE" },
            { "Spiritual Gifts", "#2DD4BF" },
            { "Conflict Profile", "#4ADE80" },

            { "Active", "#4ADE80" },
            { "Inactive", "#CCCCCC" },

            { "0-12", "#FB7185" },
            { "13-17", "#A78BFA" },
            { "18-24", "#22D3EE" },
            { "25-34", "#FBBF24" },
            { "35-44", "#F87171" },
            { "45-54", "#38BDF8" },
            { "55-64", "#F472B6" },
            { "65+", "#4ADE80" },
        };

        const string PieChartConfig = "{[ chart type:'pie' chartheight:'200px' legendshow:'true' legendposition:'right' ]}";

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
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

            InitializeBlock();
        }

        #endregion Base Control Methods

        #region Methods

        /// <summary>
        /// Initialize the block.
        /// </summary>
        private void InitializeBlock()
        {
            var rockContext = new RockContext();
            rockContext.Database.Log = strQry => Debug.WriteLine( strQry );
            var personService = new PersonService( rockContext );

            var qry = personService.Queryable();
            var total = qry.Count();
            var personRecordDefinedValueGuid = Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid();
            IEnumerable<Person> alivePersonsQry;
            IEnumerable<Person> activeAlivePersonsQry;

            alivePersonsQry = qry.Where( p => p.RecordTypeValue.Guid == personRecordDefinedValueGuid && !p.IsDeceased );
            activeAlivePersonsQry = alivePersonsQry.Where( p => p.RecordStatusValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() );

            GetDemographics( activeAlivePersonsQry );
            GetInformationStatistics( activeAlivePersonsQry, rockContext, total );
            GetPercentOfActiveIndividualsWithAssessments( rockContext, total );
            GetPercentOfActiveRecords( alivePersonsQry, total );
        }

        /// <summary>
        /// Gets the demographics.
        /// </summary>
        private void GetDemographics( IEnumerable<Person> persons )
        {
            var demographics = new List<DemographicItem>()
            {
                new DemographicItem( "Gender", GetGenderLava( persons ) ),
                new DemographicItem( "Connection Status", GetConnectionStatusLava( persons ) ),
                new DemographicItem( "Marital Status", GetMaritalStatusLava( persons ) ),
                new DemographicItem( "Age", GetAgeLava( persons ) ),
                new DemographicItem( "Race", GetRaceLava( persons ) ),
                new DemographicItem( "Ethnicity", GetEthnicityLava( persons ) ),
            };

            rptDemographics.DataSource = demographics;
            rptDemographics.DataBind();
        }

        /// <summary>
        /// Gets the gender lava.
        /// </summary>
        /// <returns></returns>
        private string GetGenderLava( IEnumerable<Person> qry )
        {
            var dataItems = qry.GroupBy( p => p.Gender )
                .Select( p => new DataItem() { Label = p.Key.ToString(), Value = p.Count().ToString() } )
                .ToList();

            return PopulateShortcodeDataItems( PieChartConfig, dataItems );
        }

        /// <summary>
        /// Gets the connection status lava.
        /// </summary>
        /// <returns></returns>
        private string GetConnectionStatusLava( IEnumerable<Person> qry )
        {
            var dataItems = qry.GroupBy( p => p.ConnectionStatusValue?.Value )
                .Select( p => new DataItem() { Label = p.Key ?? "Unknown", Value = p.Count().ToString() } )
                .ToList();

            return PopulateShortcodeDataItems( PieChartConfig, dataItems );
        }

        /// <summary>
        /// Gets the marital status lava.
        /// </summary>
        /// <returns></returns>
        private string GetMaritalStatusLava( IEnumerable<Person> qry )
        {
            var dataItems = qry.Select( p => new DataItem() { Label = p.MaritalStatusValue?.Value ?? "Unknown" } )
                .GroupBy( p => p.Label )
                .ToList();

            return PopulateShortcodeDataItems( PieChartConfig, dataItems.Select( p => new DataItem( p.Key, p.Count().ToString() ) ).ToList() );
        }

        /// <summary>
        /// Gets the age lava.
        /// </summary>
        /// <returns></returns>
        private string GetAgeLava( IEnumerable<Person> qry )
        {
            var dataItems = new List<DataItem>();
            var peopleWithAgeQry = qry.Where( p => p.BirthDate.HasValue );

            var zeroTo12RangeSql = peopleWithAgeQry.Count( p => p.Age >= 0 && p.Age <= 12 );
            dataItems.Add( new DataItem( "0-12", zeroTo12RangeSql.ToString() ) );

            var thirteenToSeventeenRangeSql = peopleWithAgeQry.Count( p => p.Age >= 13 && p.Age <= 17 );
            dataItems.Add( new DataItem( "13-17", thirteenToSeventeenRangeSql.ToString() ) );

            var eighteenAndTwentyFour = peopleWithAgeQry.Count( p => p.Age >= 18 && p.Age <= 24 );
            dataItems.Add( new DataItem( "18-24", eighteenAndTwentyFour.ToString() ) );

            var twentyFiveAndThirtyFour = peopleWithAgeQry.Count( p => p.Age >= 25 && p.Age <= 34 );
            dataItems.Add( new DataItem( "25-34", twentyFiveAndThirtyFour.ToString() ) );

            var thirtyFiveAndFortyFour = peopleWithAgeQry.Count( p => p.Age >= 35 && p.Age <= 44 );
            dataItems.Add( new DataItem( "35-44", thirtyFiveAndFortyFour.ToString() ) );

            var fortyFiveAndFiftyFour = peopleWithAgeQry.Count( p => p.Age >= 45 && p.Age <= 54 );
            dataItems.Add( new DataItem( "45-54", fortyFiveAndFiftyFour.ToString() ) );

            var fiftyFiveAndSixtyFour = peopleWithAgeQry.Count( p => p.Age >= 55 && p.Age <= 64 );
            dataItems.Add( new DataItem( "55-64", fiftyFiveAndSixtyFour.ToString() ) );

            var overSixtyFive = peopleWithAgeQry.Count( predicate: p => p.Age >= 60 );
            dataItems.Add( new DataItem( ">60", overSixtyFive.ToString() ) );

            var unknown = qry.Count( p => !p.BirthDate.HasValue );
            dataItems.Add( new DataItem( "Unknown", unknown.ToString() ) );

            return PopulateShortcodeDataItems( PieChartConfig, dataItems );
        }

        /// <summary>
        /// Gets the race lava.
        /// </summary>
        /// <returns></returns>
        private string GetRaceLava( IEnumerable<Person> qry )
        {
            var dataItems = qry.GroupBy( p => p.RaceValue?.Value )
                .Select( p => new DataItem() { Label = p.Key ?? "Unknown", Value = p.Count().ToString() } )
                .ToList();

            return PopulateShortcodeDataItems( PieChartConfig, dataItems );
        }

        /// <summary>
        /// Gets the ethnicity lava.
        /// </summary>
        /// <returns></returns>
        private string GetEthnicityLava( IEnumerable<Person> qry )
        {
            var dataItems = qry.GroupBy( p => p.EthnicityValue?.Value )
                .Select( p => new DataItem() { Label = p.Key ?? "Unknown", Value = p.Count().ToString() } )
                .ToList();

            return PopulateShortcodeDataItems( PieChartConfig, dataItems );
        }

        /// <summary>
        /// Gets the information statistics.
        /// </summary>
        private void GetInformationStatistics( IEnumerable<Person> qry, RockContext rockContext, int total )
        {
            const string chartConfig = "{[ chart type:'bar' yaxismin:'0' yaxismax:'100' yaxisstepsize:'20' ]}";

            var dataItems = new List<DataItem>();

            var hasAgeCount = qry.Count( p => p.BirthDate.HasValue );
            dataItems.Add( new DataItem( "Age", DataItem.GetPercentage( hasAgeCount, total ) ) );

            var hasGenderCount = qry.Count( p => p.Gender != Gender.Unknown );
            dataItems.Add( new DataItem( "Gender", DataItem.GetPercentage( hasGenderCount, total ) ) );

            var hasActiveEmailCount = qry.Count( p => p.IsEmailActive && p.Email.IsNotNullOrWhiteSpace() );
            dataItems.Add( new DataItem( "Active Email", DataItem.GetPercentage( hasActiveEmailCount, total ) ) );

            var mobilePhoneTypeGuid = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid();
            var hasMobilePhoneCount = new PhoneNumberService( rockContext ).Queryable().Count( p => p.NumberTypeValue.Guid == mobilePhoneTypeGuid );
            dataItems.Add( new DataItem( "Mobile Phone", DataItem.GetPercentage( hasMobilePhoneCount, total ) ) );

            var hasMaritalStatusCount = qry.Count( p => p.MaritalStatusValueId.HasValue && p.MaritalStatusValue.Value != "Unknown" );
            dataItems.Add( new DataItem( "Marital Status", DataItem.GetPercentage( hasMaritalStatusCount, total ) ) );

            var hasPhotoCount = qry.Count( p => p.PhotoId.HasValue );
            dataItems.Add( new DataItem( "Photo", DataItem.GetPercentage( hasPhotoCount, total ) ) );

            var hasBirthDateCount = qry.Count( p => p.BirthDate.HasValue );
            dataItems.Add( new DataItem( "Date of Birth", DataItem.GetPercentage( hasBirthDateCount, total ) ) );

            var homeLocationTypeGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid();
            var grpLocationQry = new GroupLocationService( rockContext ).Queryable();
            var hasHomeAddressCount = qry.Join( grpLocationQry, person => person.PrimaryFamilyId,
                groupLocation => groupLocation.GroupId,
                ( person, groupLocation ) => new { Person = person, GroupLocation = groupLocation } )
                .Where( personGroupLocation => personGroupLocation.GroupLocation.GroupLocationTypeValue.Guid == homeLocationTypeGuid );
            dataItems.Add( new DataItem( "Home Address", DataItem.GetPercentage( hasHomeAddressCount.Count(), total ) ) );

            rlInformationCompleteness.Text = PopulateShortcodeDataItems( chartConfig, dataItems );
        }

        /// <summary>
        /// Gets the percent of active individuals with assessments.
        /// </summary>
        private void GetPercentOfActiveIndividualsWithAssessments( RockContext rockContext, int total )
        {
            var personRecordDefinedValueGuid = Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid();
            var activePersonDefinedValueGuid = Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid();
            var assessmentQry = new AssessmentService( rockContext ).Queryable()
                .Where( a => a.PersonAlias.Person.RecordTypeValue.Guid == personRecordDefinedValueGuid
                    && a.PersonAlias.Person.RecordStatusValue.Guid == activePersonDefinedValueGuid
                    && !a.PersonAlias.Person.IsDeceased );
            var assessmentTypeQry = new AssessmentTypeService( rockContext ).Queryable();

            // Perform an outer join so we get records for every assessmentType even those with no assessments.
            var groupedQuery = assessmentTypeQry.GroupJoin( assessmentQry, assessmentType => assessmentType.Id,
                assessment => assessment.AssessmentTypeId,
                ( assessmentType, assessments ) => new { AssessmentType = assessmentType, Assessments = assessments } )
                .SelectMany( assessmentTypeAssessments => assessmentTypeAssessments.Assessments.DefaultIfEmpty(),
                ( assessmentTypeAssessments, assessment ) => new { AssessmentType = assessmentTypeAssessments.AssessmentType, Assessment = assessment } )
                .GroupBy( assessmentTypePerson => assessmentTypePerson.AssessmentType.Title )
                .ToList();

            var dataItems = groupedQuery.Select( a => new DataItem( a.Key, DataItem.GetPercentage( a.Count( m => m.Assessment != null ), total ) ) ).ToList();

            const string chartConfig = "{[ chart type:'bar' yaxismin:'0' yaxismax:'100' yaxisstepsize:'20' ]}";
            const string noItemsNotification = @"
<div class=""alert alert-info"">
    <span class=""js-notification-text"">There is no data on active individuals with assessments.</span>
</div>";

            rlActiveIndividualsWithAssessments.Text = dataItems.Count == 0 ? noItemsNotification : PopulateShortcodeDataItems( chartConfig, dataItems );
        }

        /// <summary>
        /// Gets the percent of active records.
        /// </summary>
        private void GetPercentOfActiveRecords( IEnumerable<Person> alivePersonsQry, int total )
        {
            var dataItems = new List<DataItem>();

            var activeCount = alivePersonsQry.Count( p => p.RecordStatusValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() );
            dataItems.Add( new DataItem( "Active", DataItem.GetPercentage( activeCount, total ) ) );

            var inActiveCount = alivePersonsQry.Count( p => p.RecordStatusValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() );
            dataItems.Add( new DataItem( "InActive", DataItem.GetPercentage( inActiveCount, total ) ) );

            rlActiveRecords.Text = PopulateShortcodeDataItems( PieChartConfig, dataItems );
        }

        /// <summary>
        /// Converts the passed <paramref name="dataItems"/> into lava short code for the generation of the chart
        /// </summary>
        /// <param name="chartConfig">The chart configuration i.e it's type, x and y axis config etc.</param>
        /// <param name="dataItems">The data items.</param>
        /// <returns></returns>
        private string PopulateShortcodeDataItems( string chartConfig, List<DataItem> dataItems )
        {
            const string dataItemFormat = "[[ dataitem label:'{0}' value:'{1}' fillcolor:'{2}' ]] [[ enddataitem ]]";

            var sb = new StringBuilder( chartConfig );

            foreach ( var dataItem in dataItems )
            {
                sb.AppendFormat( dataItemFormat, dataItem.Label, dataItem.Value, GetFillColor( dataItem.Label ) ).AppendLine();
            }

            sb.AppendLine( "{[ endchart ]}" );

            return sb.ToString().ResolveMergeFields( new Dictionary<string, object>() );
        }

        private string GetFillColor( string label )
        {
            if ( LabelColorMap.ContainsKey( label ) )
            {
                return LabelColorMap[label];
            }
            else
            {
                return "#818CF8";
            }
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
            NavigateToCurrentPageReference();
        }

        #endregion Events

        #region Helper Classes

        /// <summary>
        /// Class representing a demographic statistic
        /// </summary>
        private sealed class DemographicItem
        {
            public DemographicItem( string title, string chart )
            {
                Title = title;
                Chart = chart;
            }

            public DemographicItem()
            {
            }

            /// <summary>
            /// Gets or sets the title.
            /// </summary>
            /// <value>
            /// The title.
            /// </value>
            public string Title { get; set; }

            /// <summary>
            /// Gets or sets the chart.
            /// </summary>
            /// <value>
            /// The chart.
            /// </value>
            public string Chart { get; set; }
        }

        /// <summary>
        /// Represents a data item
        /// </summary>
        private sealed class DataItem
        {
            public DataItem( string label, string value )
            {
                Label = label;
                Value = value;
            }

            public DataItem()
            {
            }

            /// <summary>
            /// Gets or sets the label.
            /// </summary>
            /// <value>
            /// The label.
            /// </value>
            public string Label { get; set; }

            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            public string Value { get; set; }

            internal static string GetPercentage( int count, int total )
            {
                var asDecimal = decimal.Divide( count, total );
                var percent = decimal.Round( asDecimal * 100 );
                return percent.ToString();
            }
        }

        #endregion
    }
}