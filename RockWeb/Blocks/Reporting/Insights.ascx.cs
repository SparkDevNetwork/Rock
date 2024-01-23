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
using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Crm;
using Rock.Model;
using Rock.Web.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

namespace RockWeb.Blocks.Reporting
{
    [DisplayName( "Insights" )]
    [Category( "Reporting" )]
    [Description( "Shows high-level statistics of the Rock database." )]

    #region Block Attributes

    [BooleanField(
        "Show Ethnicity",
        Key = AttributeKey.ShowEthnicity,
        Description = "When enabled the Ethnicity chart will be displayed.",
        DefaultValue = "false",
        Order = 0 )]

    [BooleanField(
        "Show Race",
        Key = AttributeKey.ShowRace,
        Description = "When enabled the Race chart will be displayed.",
        DefaultValue = "false",
        Order = 1 )]

    #endregion

    [Rock.SystemGuid.BlockTypeGuid( "B215F5FA-410C-4674-8C47-43DC40AF9F67" )]
    public partial class Insights : RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string ShowEthnicity = "ShowEthnicity";
            public const string ShowRace = "ShowRace";
        }

        #endregion

        #region Fields

        /// <summary>
        /// The available colors for the charts
        /// </summary>
        private readonly List<string> _availableColors = new List<string>()
        {
             "#38BDF8",  // Sky
             "#34D399",  // Emerald
             "#FB7185",  // Rose
             "#A3E635",  // Lime
             "#818CF8",  // Indigo
             "#FB923C",  // Orange
             "#C084FC",  // Purple
             "#FBBF24",  // Amber
             "#A8A29E",  // Stone
        };

        /// <summary>
        /// Lava short code pie chart configuration
        /// </summary>
        const string PieChartConfig = "{[ chart type:'pie' chartheight:'200px' legendshow:'true' legendposition:'right' ]}";

        /// <summary>
        /// Keeps track of the number of colors to skip when selecting the fill color for the charts
        /// </summary>
        private int skipCount = 0;

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
            var personService = new PersonService( rockContext );
            rockContext.Database.Log = sql => System.Diagnostics.Debug.WriteLine( sql );

            var personRecordDefinedValueGuid = Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid();
            var activeStatusDefinedValueGuid = Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid();
            IQueryable<PersonViewModel> alivePersonsQry;
            IQueryable<PersonViewModel> activeAlivePersonsQry;

            alivePersonsQry = personService.Queryable().Where( p => p.RecordTypeValue.Guid == personRecordDefinedValueGuid && !p.IsDeceased ).Select( p => new PersonViewModel()
            {
                Id = p.Id,
                AgeBracket = p.AgeBracket,
                BirthDate = p.BirthDate,
                Gender = p.Gender,
                IsEmailActive = p.IsEmailActive,
                Email = p.Email,
                PhotoId = p.PhotoId,
                PrimaryFamilyId = p.PrimaryFamilyId,
                ConnectionStatusValue = new DefinedValueViewModel() { Guid = p.ConnectionStatusValue.Guid, Value = p.ConnectionStatusValue.Value },
                RecordTypeValue = new DefinedValueViewModel() { Guid = p.RecordTypeValue.Guid, Value = p.RecordTypeValue.Value },
                RecordStatusValue = new DefinedValueViewModel() { Guid = p.RecordStatusValue.Guid, Value = p.RecordStatusValue.Value },
                MaritalStatusValue = new DefinedValueViewModel() { Guid = p.MaritalStatusValue.Guid, Value = p.MaritalStatusValue.Value },
                RaceValue = new DefinedValueViewModel() { Guid = p.RaceValue.Guid, Value = p.RaceValue.Value },
                EthnicityValue = new DefinedValueViewModel() { Guid = p.EthnicityValue.Guid, Value = p.EthnicityValue.Value },
            } );

            activeAlivePersonsQry = alivePersonsQry.Where( p => p.RecordStatusValue.Guid == activeStatusDefinedValueGuid );
            var total = activeAlivePersonsQry.Count();

            GetDemographics( activeAlivePersonsQry );
            GetInformationStatistics( activeAlivePersonsQry, rockContext, total );
            GetPercentOfActiveIndividualsWithAssessments( rockContext, total );
            GetPercentOfActiveRecords( alivePersonsQry );
        }

        /// <summary>
        /// Gets the demographics.
        /// </summary>
        private void GetDemographics( IEnumerable<PersonViewModel> persons )
        {
            var demographics = new List<DemographicItem>()
            {
                new DemographicItem( "Gender", GetGenderLava( persons ) ),
                new DemographicItem( "Connection Status", GetConnectionStatusLava( persons ) ),
                new DemographicItem( "Marital Status", GetMaritalStatusLava( persons ) ),
                new DemographicItem( "Age", GetAgeLava( persons ) ),
            };

            if ( GetAttributeValue( AttributeKey.ShowRace ).AsBoolean() )
            {
                demographics.Add( new DemographicItem( "Race", GetRaceLava( persons ) ) );
            }

            if ( GetAttributeValue( AttributeKey.ShowEthnicity ).AsBoolean() )
            {
                demographics.Add( new DemographicItem( "Ethnicity", GetEthnicityLava( persons ) ) );
            }

            rptDemographics.DataSource = demographics;
            rptDemographics.DataBind();
        }

        /// <summary>
        /// Gets the gender lava.
        /// </summary>
        /// <returns></returns>
        private string GetGenderLava( IEnumerable<PersonViewModel> qry )
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
        private string GetConnectionStatusLava( IEnumerable<PersonViewModel> qry )
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
        private string GetMaritalStatusLava( IEnumerable<PersonViewModel> qry )
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
        private string GetAgeLava( IEnumerable<PersonViewModel> qry )
        {
            var dataItems = new List<DataItem>();
            var peopleWithAgeQry = qry.Where( p => p.BirthDate.HasValue );

            var zeroToFiveRangeSql = peopleWithAgeQry.Count( p => p.AgeBracket == Rock.Enums.Crm.AgeBracket.ZeroToFive );
            dataItems.Add( new DataItem( "0-5", zeroToFiveRangeSql.ToString() ) );

            var sixToTwelveRangeSql = peopleWithAgeQry.Count( p => p.AgeBracket == Rock.Enums.Crm.AgeBracket.SixToTwelve );
            dataItems.Add( new DataItem( "6-12", sixToTwelveRangeSql.ToString() ) );

            var thirteenToSeventeenRangeSql = peopleWithAgeQry.Count( p => p.AgeBracket == Rock.Enums.Crm.AgeBracket.ThirteenToSeventeen );
            dataItems.Add( new DataItem( "13-17", thirteenToSeventeenRangeSql.ToString() ) );

            var eighteenAndTwentyFour = peopleWithAgeQry.Count( p => p.AgeBracket == Rock.Enums.Crm.AgeBracket.EighteenToTwentyFour );
            dataItems.Add( new DataItem( "18-24", eighteenAndTwentyFour.ToString() ) );

            var twentyFiveAndThirtyFour = peopleWithAgeQry.Count( p => p.AgeBracket == Rock.Enums.Crm.AgeBracket.TwentyFiveToThirtyFour );
            dataItems.Add( new DataItem( "25-34", twentyFiveAndThirtyFour.ToString() ) );

            var thirtyFiveAndFortyFour = peopleWithAgeQry.Count( p => p.AgeBracket == Rock.Enums.Crm.AgeBracket.ThirtyFiveToFortyFour );
            dataItems.Add( new DataItem( "35-44", thirtyFiveAndFortyFour.ToString() ) );

            var fortyFiveAndFiftyFour = peopleWithAgeQry.Count( p => p.AgeBracket == Rock.Enums.Crm.AgeBracket.FortyFiveToFiftyFour );
            dataItems.Add( new DataItem( "45-54", fortyFiveAndFiftyFour.ToString() ) );

            var fiftyFiveAndSixtyFour = peopleWithAgeQry.Count( p => p.AgeBracket == Rock.Enums.Crm.AgeBracket.FiftyFiveToSixtyFour );
            dataItems.Add( new DataItem( "55-64", fiftyFiveAndSixtyFour.ToString() ) );

            var overSixtyFive = peopleWithAgeQry.Count( predicate: p => p.AgeBracket == Rock.Enums.Crm.AgeBracket.SixtyFiveOrOlder);
            dataItems.Add( new DataItem( ">65", overSixtyFive.ToString() ) );

            var unknown = qry.Count( p => p.AgeBracket == Rock.Enums.Crm.AgeBracket.Unknown );
            dataItems.Add( new DataItem( "Unknown", unknown.ToString() ) );

            return PopulateShortcodeDataItems( PieChartConfig, dataItems );
        }

        /// <summary>
        /// Gets the race lava.
        /// </summary>
        /// <returns></returns>
        private string GetRaceLava( IEnumerable<PersonViewModel> qry )
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
        private string GetEthnicityLava( IEnumerable<PersonViewModel> qry )
        {
            var dataItems = qry.GroupBy( p => p.EthnicityValue?.Value )
                .Select( p => new DataItem() { Label = p.Key ?? "Unknown", Value = p.Count().ToString() } )
                .ToList();

            return PopulateShortcodeDataItems( PieChartConfig, dataItems );
        }

        /// <summary>
        /// Gets the information statistics.
        /// </summary>
        private void GetInformationStatistics( IEnumerable<PersonViewModel> qry, RockContext rockContext, int total )
        {
            const string chartConfig = "{[ chart type:'bar' yaxismin:'0' yaxismax:'100' yaxisstepsize:'20' valueformat:'percentage' ]}";

            var dataItems = new List<DataItem>();

            var hasAgeCount = qry.Count( p => p.BirthDate.HasValue );
            dataItems.Add( new DataItem( "Age", DataItem.GetPercentage( hasAgeCount, total ) ) );

            var hasGenderCount = qry.Count( p => p.Gender != Gender.Unknown );
            dataItems.Add( new DataItem( "Gender", DataItem.GetPercentage( hasGenderCount, total ) ) );

            var hasActiveEmailCount = qry.Count( p => p.IsEmailActive && p.Email.IsNotNullOrWhiteSpace() );
            dataItems.Add( new DataItem( "Active Email", DataItem.GetPercentage( hasActiveEmailCount, total ) ) );

            var mobilePhoneTypeGuid = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid();
            var personPhoneQry = new PhoneNumberService( rockContext ).Queryable().Where( pn => pn.NumberTypeValue.Guid == mobilePhoneTypeGuid );
            var hasMobilePhoneCount = qry.Join( personPhoneQry, person => person.Id,
                phoneNumber => phoneNumber.PersonId,
                ( person, phoneNumber ) => new { person, phoneNumber } )
                .Count();
            dataItems.Add( new DataItem( "Mobile Phone", DataItem.GetPercentage( hasMobilePhoneCount, total ) ) );

            var hasMaritalStatusCount = qry.Count( p => p.MaritalStatusValue != null && p.MaritalStatusValue.Value != "Unknown" );
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

            const string chartConfig = "{[ chart type:'bar' yaxismin:'0' yaxismax:'100' yaxisstepsize:'20' valueformat:'percentage' ]}";
            const string noItemsNotification = @"
<div class=""alert alert-info"">
    <span class=""js-notification-text"">There is no data on active individuals with assessments.</span>
</div>";

            rlActiveIndividualsWithAssessments.Text = dataItems.Count == 0 ? noItemsNotification : PopulateShortcodeDataItems( chartConfig, dataItems );
        }

        /// <summary>
        /// Gets the percent of active records.
        /// </summary>
        private void GetPercentOfActiveRecords( IQueryable<PersonViewModel> alivePersonsQry )
        {
            var dataItems = new List<DataItem>();
            var total = alivePersonsQry.Count();

            var activeCount = alivePersonsQry.Count( p => p.RecordStatusValue.Guid.ToString() == Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE );
            dataItems.Add( new DataItem( "Active", DataItem.GetPercentage( activeCount, total ) ) );

            var inActiveCount = alivePersonsQry.Count( p => p.RecordStatusValue.Guid.ToString() == Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE );
            dataItems.Add( new DataItem( "Inactive", DataItem.GetPercentage( inActiveCount, total ) ) );

            var pendingCount = alivePersonsQry.Count( p => p.RecordStatusValue.Guid.ToString() == Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING );
            dataItems.Add( new DataItem( "Pending", DataItem.GetPercentage( pendingCount, total ) ) );

            const string chartConfig = "{[ chart type:'pie' chartheight:'200px' legendshow:'true' legendposition:'right' valueformat:'percentage' ]}";

            rlActiveRecords.Text = PopulateShortcodeDataItems( chartConfig, dataItems );
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

            // Reset the skip count
            skipCount = 0;

            foreach ( var dataItem in dataItems )
            {
                sb.AppendFormat( dataItemFormat, dataItem.Label, dataItem.Value, GetFillColor( skipCount, dataItem.Label ) ).AppendLine();           
                // Only skip if a color was used
                if ( dataItem.Label != "Unknown" )
                {
                    skipCount ++;
                }
            }

            sb.AppendLine( "{[ endchart ]}" );

            return sb.ToString().ResolveMergeFields( new Dictionary<string, object>() );
        }

        /// <summary>
        /// Gets the fill color of the charts.
        /// </summary>
        /// <param name="skip">The number of colors to skip.</param>
        /// <param name="label">The label of the chart.</param>
        /// <returns></returns>
        private string GetFillColor( int skip, string label )
        {
            if ( label == "Unknown" )
            {
                return "#E7E5E4";
            }
            else
            {
                return FillColorSource().Skip(skip).FirstOrDefault();
            }
        }

        /// <summary>
        /// Perpetually yields a color from the available colors source so we can skip and take in a loop.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> FillColorSource()
        {
            while ( true )
            {
                foreach ( var color in _availableColors )
                {
                    yield return color;
                }
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
                var percent = decimal.Round( asDecimal * 100, 1 );
                return percent.ToString();
            }
        }

        /// <summary>
        /// Represents the person model.
        /// </summary>
        private sealed class PersonViewModel
        {
            public int Id { get; set; }
            public DefinedValueViewModel RecordTypeValue { get; set; }
            public DefinedValueViewModel RecordStatusValue { get; set; }
            public DefinedValueViewModel ConnectionStatusValue { get; set; }
            public DefinedValueViewModel MaritalStatusValue { get; set; }
            public DefinedValueViewModel RaceValue { get; set; }
            public DefinedValueViewModel EthnicityValue { get; set; }
            public Gender Gender { get; set; }
            public DateTime? BirthDate { get; set; }
            public AgeBracket AgeBracket { get; set; }
            public bool IsEmailActive { get; set; }
            public string Email { get; set; }
            public int? PhotoId { get; set; }
            public int? PrimaryFamilyId { get; set; }
        }

        /// <summary>
        /// Represents the DefinedValue model
        /// </summary>
        private sealed class DefinedValueViewModel
        {
            public Guid? Guid { get; set; }
            public string Value { get; set; }
        }

        #endregion
    }
}
