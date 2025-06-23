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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Crm;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

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
        private const string Unknown = "Unknown";
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

        private int _personRecordDefinedValueId;
        private int _activeStatusDefinedValueId;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            _personRecordDefinedValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
            _activeStatusDefinedValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            InitializeBlock();

            base.OnLoad( e );
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
            //rockContext.Database.Log = sql => System.Diagnostics.Debug.WriteLine( sql );
            //rockContext.SqlLogging( true );

            var personRecordDefinedValueGuid = Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid();
            var activeStatusDefinedValueGuid = Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid();

            IQueryable<PersonViewModel> alivePersonsQry;

            var giverAnonymousPersonGuid = Rock.SystemGuid.Person.GIVER_ANONYMOUS.AsGuid();

            alivePersonsQry = personService.Queryable().AsNoTracking()
                .Where( p => p.RecordTypeValueId == _personRecordDefinedValueId && !p.IsDeceased && p.Guid != giverAnonymousPersonGuid )
                .Select( p => new PersonViewModel()
                {
                    Id = p.Id,
                    AgeBracket = p.AgeBracket,
                    BirthDate = p.BirthDate,
                    Gender = p.Gender,
                    HasActiveEmail = p.IsEmailActive && !string.IsNullOrEmpty( p.Email ),
                    PhotoId = p.PhotoId,
                    PrimaryFamilyId = p.PrimaryFamilyId,
                    ConnectionStatusValueId = p.ConnectionStatusValueId,
                    RecordStatusValueId = p.RecordStatusValueId,
                    MaritalStatusValueId = p.MaritalStatusValueId,
                    RaceValueId = p.RaceValueId,
                    EthnicityValueId = p.EthnicityValueId
                } );

            /*
                6/6/2025 - N.A.

                Converted the activeAlivePersonsQry query result to a ToList() to minimize repeated db calls during counting.
                This change reduces the number of SQL calls significantly by materializing the result in memory.

                Reason: Reduced database calls from 22+ to 6, improving performance.
                        Before: _callCounts:22, _callMSTotal:437.476 ms, avg: 19.89 ms
                        After:  _callCounts:6,  _callMSTotal:178.882 ms, avg: 29.81 ms
            */
            var activeAlivePersonsQry = alivePersonsQry.Where( p => p.RecordStatusValueId == _activeStatusDefinedValueId ).ToList();
            var total = activeAlivePersonsQry.Count();

            GetDemographics( activeAlivePersonsQry );
            GetInformationStatistics( activeAlivePersonsQry, rockContext, total );
            GetPercentOfActiveIndividualsWithAssessments( rockContext, total );
            GetPercentOfActiveRecords( alivePersonsQry );
            //rockContext.SqlLogging( false );
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
            var dataItems = qry.Select( p => new DataItem()
            {
                Label = p.ConnectionStatusValueId.HasValue
                    ? DefinedValueCache.Get( p.ConnectionStatusValueId.Value )?.Value ?? Unknown
                    : Unknown
            } ).GroupBy( di => di.Label )
                .ToList();

            return PopulateShortcodeDataItems( PieChartConfig, dataItems.Select( di => new DataItem( di.Key, di.Count().ToString() ) ).ToList() );
        }

        /// <summary>
        /// Gets the marital status lava.
        /// </summary>
        /// <returns></returns>
        private string GetMaritalStatusLava( IEnumerable<PersonViewModel> qry )
        {
            var dataItems = qry.Select( p => new DataItem()
            {
                Label = p.MaritalStatusValueId.HasValue
                    ? DefinedValueCache.Get( p.MaritalStatusValueId.Value )?.Value ?? Unknown
                    : Unknown
            } ).GroupBy( di => di.Label )
            .ToList();

            return PopulateShortcodeDataItems( PieChartConfig, dataItems.Select( di => new DataItem( di.Key, di.Count().ToString() ) ).ToList() );
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

            var overSixtyFive = peopleWithAgeQry.Count( predicate: p => p.AgeBracket == Rock.Enums.Crm.AgeBracket.SixtyFiveOrOlder );
            dataItems.Add( new DataItem( ">65", overSixtyFive.ToString() ) );

            var unknown = qry.Count( p => p.AgeBracket == Rock.Enums.Crm.AgeBracket.Unknown );
            dataItems.Add( new DataItem( Unknown, unknown.ToString() ) );

            return PopulateShortcodeDataItems( PieChartConfig, dataItems );
        }

        /// <summary>
        /// Gets the race lava.
        /// </summary>
        /// <returns></returns>
        private string GetRaceLava( IEnumerable<PersonViewModel> qry )
        {
            var dataItems = qry.Select( p => new DataItem()
            {
                Label = p.RaceValueId.HasValue
                    ? DefinedValueCache.Get( p.RaceValueId.Value )?.Value ?? Unknown
                    : Unknown
            } ).GroupBy( di => di.Label )
            .ToList();

            return PopulateShortcodeDataItems( PieChartConfig, dataItems.Select( di => new DataItem( di.Key, di.Count().ToString() ) ).ToList() );
        }

        /// <summary>
        /// Gets the ethnicity lava.
        /// </summary>
        /// <returns></returns>
        private string GetEthnicityLava( IEnumerable<PersonViewModel> qry )
        {
            var dataItems = qry.Select( p => new DataItem()
            {
                Label = p.EthnicityValueId.HasValue
                    ? DefinedValueCache.Get( p.EthnicityValueId.Value )?.Value ?? Unknown
                    : Unknown
            } ).GroupBy( di => di.Label )
            .ToList();

            return PopulateShortcodeDataItems( PieChartConfig, dataItems.Select( di => new DataItem( di.Key, di.Count().ToString() ) ).ToList() );
        }

        /// <summary>
        /// Gets the information statistics.
        /// </summary>
        private void GetInformationStatistics( IEnumerable<PersonViewModel> qry, RockContext rockContext, int total )
        {
            const string chartConfig = "{[ chart type:'bar' yaxismin:'0' yaxismax:'100' yaxisstepsize:'20' valueformat:'percentage' ]}";

            var dataItems = new List<DataItem>();

            // Materialize the necessary flags in one pass
            var personStats = qry.Select( p => new
            {
                p.Id,
                p.PrimaryFamilyId,
                HasBirthDate = p.BirthDate.HasValue,
                HasGender = p.Gender != Gender.Unknown,
                HasActiveEmail = p.HasActiveEmail,
                HasMaritalStatus = p.MaritalStatusValueId.HasValue,
                HasPhoto = p.PhotoId.HasValue
            } ).ToList();

            dataItems.Add( new DataItem( "Age", DataItem.GetPercentage( personStats.Count( p => p.HasBirthDate ), total ) ) );
            dataItems.Add( new DataItem( "Gender", DataItem.GetPercentage( personStats.Count( p => p.HasGender ), total ) ) );
            dataItems.Add( new DataItem( "Active Email", DataItem.GetPercentage( personStats.Count( p => p.HasActiveEmail ), total ) ) );

            var mobilePhoneTypeGuid = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid();

            var personPhoneQry = new PhoneNumberService( rockContext )
                .Queryable().AsNoTracking()
                .Where( pn => pn.NumberTypeValue.Guid == mobilePhoneTypeGuid )
                .Select( pn => pn.PersonId ); // only fetch needed field

            var hasMobilePhoneCount = qry.Join( personPhoneQry,
                person => person.Id,
                phonePersonId => phonePersonId,
                ( person, phoneNumber ) => new { person, phoneNumber } )
                .Count();
            dataItems.Add( new DataItem( "Mobile Phone", DataItem.GetPercentage( hasMobilePhoneCount, total ) ) );

            dataItems.Add( new DataItem( "Marital Status", DataItem.GetPercentage( personStats.Count( p => p.HasMaritalStatus ), total ) ) );
            dataItems.Add( new DataItem( "Photo", DataItem.GetPercentage( personStats.Count( p => p.HasPhoto ), total ) ) );
            dataItems.Add( new DataItem( "Date of Birth", DataItem.GetPercentage( personStats.Count( p => p.HasBirthDate ), total ) ) );

            var homeLocationTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() ).Id;

            // The GroupIds of GroupLocations of type Home
            var grpLocationQry = new GroupLocationService( rockContext ).Queryable().AsNoTracking()
                .Where( gl => gl.GroupLocationTypeValueId == homeLocationTypeValueId )
                .Select( gl => new
                {
                    gl.GroupId,
                } );

            var hasHomeAddressCount = qry.Join( grpLocationQry,
                person => person.PrimaryFamilyId,
                groupLocation => groupLocation.GroupId,
                ( person, groupLocation ) => new { Person = person, GroupLocation = groupLocation } )
                .Select( p => p.Person.Id )
                .Distinct()
                .Count();

            dataItems.Add( new DataItem( "Home Address", DataItem.GetPercentage( hasHomeAddressCount, total ) ) );

            rlInformationCompleteness.Text = PopulateShortcodeDataItems( chartConfig, dataItems );
        }

        /// <summary>
        /// Gets the percent of active individuals with assessments.
        /// </summary>
        private void GetPercentOfActiveIndividualsWithAssessments( RockContext rockContext, int total )
        {
            // Valid Person IDs
            var validPersonIds = new PersonService( rockContext ).Queryable().AsNoTracking()
                .Where( p => p.RecordTypeValueId == _personRecordDefinedValueId
                    && p.RecordStatusValueId == _activeStatusDefinedValueId
                    && !p.IsDeceased )
                .Select( p => p.Id );

            // Get one completed assessment per person per assessment type
            var distinctCompletedAssessments = new AssessmentService( rockContext ).Queryable().AsNoTracking()
                .Where( a => a.Status == AssessmentRequestStatus.Complete
                    && validPersonIds.Contains( a.PersonAlias.PersonId ) )
                .GroupBy( a => new { a.AssessmentTypeId, PersonId = a.PersonAlias.PersonId } )
                .Select( g => g.Key ); // distinct (AssessmentTypeId, PersonId)

            // Join with assessment types
            var assessmentTypeCounts = new AssessmentTypeService( rockContext ).Queryable().AsNoTracking()
                .GroupJoin(
                    distinctCompletedAssessments,
                    at => at.Id,
                    a => a.AssessmentTypeId,
                    ( at, assessments ) => new
                    {
                        Title = at.Title,
                        Count = assessments.Count()
                    } )
                .ToList();

            // Final projection
            var dataItems = assessmentTypeCounts
                .OrderBy( a => a.Title )
                .Select( a => new DataItem( a.Title, DataItem.GetPercentage( a.Count, total ) ) )
                .ToList();

            const string chartConfig = "{[ chart type:'bar' yaxismin:'0' yaxismax:'100' yaxisstepsize:'20' valueformat:'percentage' ]}";
            const string noItemsNotification = @"
<div class=""alert alert-info"">
    <span class=""js-notification-text"">There is no data on active individuals with assessments.</span>
</div>";
            rlActiveIndividualsWithAssessments.Text =
            dataItems.All( d => decimal.Parse( d.Value ) == 0 )
                ? noItemsNotification
                : PopulateShortcodeDataItems( chartConfig, dataItems );
        }

        /// <summary>
        /// Gets the percent of active records.
        /// </summary>
        private void GetPercentOfActiveRecords( IQueryable<PersonViewModel> alivePersonsQry )
        {
            var dataItems = new List<DataItem>();
            var total = alivePersonsQry.Count();
            var activeRecordStatuses = DefinedTypeCache
                .Get( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS )
                .DefinedValues
                .Where( dv => dv.IsActive )
                .ToList();

            var activeStatusIds = activeRecordStatuses.Select( dv => dv.Id ).ToList();

            // Perform single grouped count query
            var countsByStatus = alivePersonsQry
                .Where( p => activeStatusIds.Contains( p.RecordStatusValueId ?? 0 ) )
                .GroupBy( p => p.RecordStatusValueId )
                .Select( g => new { RecordStatusValueId = g.Key.Value, Count = g.Count() } )
                .ToDictionary( g => g.RecordStatusValueId, g => g.Count );

            foreach ( var recordStatus in activeRecordStatuses )
            {
                countsByStatus.TryGetValue( recordStatus.Id, out int count );
                dataItems.Add( new DataItem( $"{recordStatus.Value}", DataItem.GetPercentage( count, total ) ) );
            }

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
                if ( dataItem.Label != Unknown )
                {
                    skipCount++;
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
            if ( label == Unknown )
            {
                return "#E7E5E4";
            }
            else
            {
                return FillColorSource().Skip( skip ).FirstOrDefault();
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
                if ( total == 0 )
                {
                    return "0";
                }

                var asDecimal = decimal.Divide( count, total );
                var percent = ( decimal ) count / total * 100;
                return percent.ToString( "0.##" ); // 2 decimal places max
            }
        }

        /// <summary>
        /// Represents the person model.
        /// </summary>
        private sealed class PersonViewModel
        {
            public int Id { get; set; }
            public int? RecordStatusValueId { get; set; }
            public int? ConnectionStatusValueId { get; set; }
            public int? MaritalStatusValueId { get; set; }
            public int? RaceValueId { get; set; }
            public int? EthnicityValueId { get; set; }
            public Gender Gender { get; set; }
            public DateTime? BirthDate { get; set; }
            public AgeBracket AgeBracket { get; set; }
            public bool HasActiveEmail { get; set; }
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
