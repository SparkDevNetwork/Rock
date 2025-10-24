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
using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Crm;
using Rock.Model;
using Rock.ViewModels.Blocks.Reporting;
using Rock.ViewModels.Blocks.Reporting.Insights;
using Rock.ViewModels.Reporting;
using Rock.ViewModels.Reporting.BarChart;
using Rock.ViewModels.Reporting.PieChart;
using Rock.Web.Cache;

namespace Rock.Blocks.Reporting
{
    /// <summary>
    /// Displays the details of a particular achievement type.
    /// </summary>
    [DisplayName( "Insights" )]
    [Category( "Reporting" )]
    [Description( "Shows insights regarding demongraphics, information, and records." )]
    [IconCssClass( "ti ti-question-mark" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    [BooleanField(
        "Show Demographics",
        Key = AttributeKey.ShowDemographics,
        Description = "When enabled the Demographics panel will be displayed.",
        DefaultValue = "true",
        Order = 0 )]

    [BooleanField(
        "Show Demographics > Connection Status",
        Key = AttributeKey.ShowConnectionStatus,
        Description = "When enabled the Connection Status chart will be displayed on the Demographics panel.",
        DefaultValue = "true",
        Order = 1 )]

    [BooleanField(
        "Show Demographics > Age",
        Key = AttributeKey.ShowAge,
        Description = "When enabled the Age chart will be displayed on the Demographics panel.",
        DefaultValue = "true",
        Order = 2 )]

    [BooleanField(
        "Show Demographics > Marital Status",
        Key = AttributeKey.ShowMaritalStatus,
        Description = "When enabled the Marital Status chart will be displayed on the Demographics panel.",
        DefaultValue = "true",
        Order = 3 )]

    [BooleanField(
        "Show Demographics > Gender",
        Key = AttributeKey.ShowGender,
        Description = "When enabled the Gender chart will be displayed on the Demographics panel.",
        DefaultValue = "true",
        Order = 4 )]

    [BooleanField(
        "Show Demographics > Ethnicity",
        Key = AttributeKey.ShowEthnicity,
        Description = "When enabled the Ethnicity chart will be displayed on the Demographics panel.",
        DefaultValue = "false",
        Order = 5 )]

    [BooleanField(
        "Show Demographics > Race",
        Key = AttributeKey.ShowRace,
        Description = "When enabled the Race chart will be displayed on the Demographics panel.",
        DefaultValue = "false",
        Order = 6 )]

    [BooleanField(
        "Show Information Statistics",
        Key = AttributeKey.ShowInformationStatistics,
        Description = "When enabled the Information Statistics panel will be displayed.",
        DefaultValue = "true",
        Order = 7 )]

    [BooleanField(
        "Show Information Statistics > Information Completeness",
        Key = AttributeKey.ShowInformationCompleteness,
        Description = "When enabled the Information Completeness chart will be displayed on the Information Statistics panel.",
        DefaultValue = "true",
        Order = 8 )]

    [BooleanField(
        "Show Information Statistics > % of Active Individuals with Assessments",
        Key = AttributeKey.ShowPercentageOfActiveIndividualsWithAssessments,
        Description = "When enabled the % of Active Individuals with Assessments chart will be displayed on the Information Statistics panel.",
        DefaultValue = "true",
        Order = 9 )]

    [BooleanField(
        "Show Information Statistics > Record Statuses",
        Key = AttributeKey.ShowRecordStatuses,
        Description = "When enabled the Record Statuses chart will be displayed on the Information Statistics panel.",
        DefaultValue = "true",
        Order = 10 )]

    #endregion Block Attributes

    [SystemGuid.EntityTypeGuid( "6739DD77-A510-4826-8263-2C2E53D31DF9" )]
    // Was [SystemGuid.BlockTypeGuid( "551DB463-A013-476C-A619-57CC234DC410" )]
    [Rock.SystemGuid.BlockTypeGuid( "B215F5FA-410C-4674-8C47-43DC40AF9F67" )]
    public class Insights : RockBlockType
    {
        #region Fields

        private int _personRecordDefinedValueId;
        private int _activeStatusDefinedValueId;

        /// <summary>
        /// The default colors for the charts
        /// </summary>
        private static List<string> _defaultColors = new List<string>()
        {
            "#38BDF8", // Blue
            "#34D399", // Green
            "#FB7185", // Pink
            "#A3E635", // Lime
            "#818CF8", // Indigo
            "#FB923C", // Orange
            "#C084FC", // Purple
            "#FBBF24", // Yellow
            "#A8A29E", // Stone
            "#E7E5E4", // Gray
        };

        #endregion Fields

        #region Keys

        private static class AttributeKey
        {
            public const string ShowDemographics = "ShowDemographics";
            public const string ShowConnectionStatus = "ShowConnectionStatus";
            public const string ShowAge = "ShowAge";
            public const string ShowMaritalStatus = "ShowMaritalStatus";
            public const string ShowGender = "ShowGender";
            public const string ShowEthnicity = "ShowEthnicity";
            public const string ShowRace = "ShowRace";
            public const string ShowInformationStatistics = "ShowInformationStatistics";
            public const string ShowInformationCompleteness = "ShowInformationCompleteness";
            public const string ShowPercentageOfActiveIndividualsWithAssessments = "ShowPercentageOfActiveIndividualsWithAssessments";
            public const string ShowRecordStatuses = "ShowRecordStatuses";
        }

        private static class ChartTypeKey
        {
            public const string BarChart = "bar";
            public const string PieChart = "pie";
        }

        #endregion Keys

        #region Methods

        #region Methods > Initialization Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            _personRecordDefinedValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
            _activeStatusDefinedValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;

            var box = GetInitializationBox( RockContext );
            return box;
        }

        /// <summary>
        /// Sets the initial entity state of the box.
        /// </summary>
        private InsightsInitializationBox GetInitializationBox( RockContext rockContext )
        {
            var box = new InsightsInitializationBox();

            var alivePersons = GetAlivePersons( rockContext );
            var activeAlivePersons = alivePersons.Where( p => p.RecordStatusValueId == _activeStatusDefinedValueId ).ToList();
            var activeAlivePersonsCount = activeAlivePersons.Count();

            box.ChartDataBags = new List<InsightsChartDataBag>();
            box.OptionsBags = new Dictionary<string, InsightsOptionsBag>();

            box.OptionsBags.Add( "BlockProperties", GetBlockPropertiesOptionsBag() );

            if ( GetAttributeValue( AttributeKey.ShowDemographics ).AsBoolean() )
            {
                if ( GetAttributeValue( AttributeKey.ShowGender ).AsBoolean() )
                {
                    var chartDataBag = GetGenderChartDataBag( activeAlivePersons );
                    box.ChartDataBags.Add( chartDataBag );
                }
                if ( GetAttributeValue( AttributeKey.ShowConnectionStatus ).AsBoolean() )
                {
                    box.ChartDataBags.Add( GetChartDataBagByPercentageOfPersonProperty<BarSeriesBag>( activeAlivePersons, "ConnectionStatusValueId", "Demographics", "ConnectionStatus", true ) );
                }
                if ( GetAttributeValue( AttributeKey.ShowMaritalStatus ).AsBoolean() )
                {
                    box.ChartDataBags.Add( GetChartDataBagByPercentageOfPersonProperty<BarSeriesBag>( activeAlivePersons, "MaritalStatusValueId", "Demographics", "MaritalStatus", true ) );
                }
                if ( GetAttributeValue( AttributeKey.ShowAge ).AsBoolean() )
                {
                    box.ChartDataBags.Add( GetAgeChartDataBag( activeAlivePersons ) );
                }
                if ( GetAttributeValue( AttributeKey.ShowEthnicity ).AsBoolean() )
                {
                    box.ChartDataBags.Add( GetChartDataBagByPercentageOfPersonProperty<PieSeriesBag>( activeAlivePersons, "EthnicityValueId", "Demographics", "Ethnicity", false ) );
                }
                if ( GetAttributeValue( AttributeKey.ShowRace ).AsBoolean() )
                {
                    box.ChartDataBags.Add( GetChartDataBagByPercentageOfPersonProperty<PieSeriesBag>( activeAlivePersons, "RaceValueId", "Demographics", "Race", false ) );
                }
            }

            if ( GetAttributeValue( AttributeKey.ShowInformationStatistics ).AsBoolean() )
            {
                if ( GetAttributeValue( AttributeKey.ShowInformationCompleteness ).AsBoolean() )
                {
                    box.ChartDataBags.Add( GetInformationCompletenessDataBag( activeAlivePersons, rockContext ) );
                }
                if ( GetAttributeValue( AttributeKey.ShowPercentageOfActiveIndividualsWithAssessments ).AsBoolean() )
                {
                    box.ChartDataBags.Add( GetActiveIndividualsWithAssessmentsDataBag( activeAlivePersons, rockContext ) );
                }
                if ( GetAttributeValue( AttributeKey.ShowRecordStatuses ).AsBoolean() )
                {
                    box.ChartDataBags.Add( GetRecordStatusesDataBag( alivePersons ) );
                }
            }

            return box;
        }

        #endregion Methods > Initialization Methods

        #region Methods > Bag Generation Methods

        #region Methods > Bag Generation Methods > Options Bags

        /// <summary>
        /// Maps the value of all current AttributeKeys to an InsightsOptionsBag.
        /// </summary>
        private InsightsOptionsBag GetBlockPropertiesOptionsBag()
        {
            var isDemographicsShown = GetAttributeValue( AttributeKey.ShowDemographics ).AsBoolean();
            var isInformationStatisticsShown = GetAttributeValue( AttributeKey.ShowInformationStatistics ).AsBoolean();

            return new InsightsOptionsBag
            {
                // Demongraphics Section
                IsDemographicsShown = isDemographicsShown,
                IsConnectionStatusShown = isDemographicsShown ? GetAttributeValue( AttributeKey.ShowConnectionStatus ).AsBoolean() : false,
                IsAgeShown = isDemographicsShown ? GetAttributeValue( AttributeKey.ShowAge ).AsBoolean() : false,
                IsMaritalStatusShown = isDemographicsShown ? GetAttributeValue( AttributeKey.ShowMaritalStatus ).AsBoolean() : false,
                IsGenderShown = isDemographicsShown ? GetAttributeValue( AttributeKey.ShowGender ).AsBoolean() : false,
                IsEthnicityShown = isDemographicsShown ? GetAttributeValue( AttributeKey.ShowEthnicity ).AsBoolean() : false,
                IsRaceShown = isDemographicsShown ? GetAttributeValue( AttributeKey.ShowRace ).AsBoolean() : false,
                // Information Statistics Section
                IsInformationStatisticsShown = isInformationStatisticsShown,
                IsInformationCompletenessShown = isInformationStatisticsShown ? GetAttributeValue( AttributeKey.ShowInformationCompleteness ).AsBoolean() : false,
                IsPercentageOfIndividualsWithAssessmentsShown = isInformationStatisticsShown ? GetAttributeValue( AttributeKey.ShowPercentageOfActiveIndividualsWithAssessments ).AsBoolean() : false,
                IsRecordStatusesShown = isInformationStatisticsShown ? GetAttributeValue( AttributeKey.ShowRecordStatuses ).AsBoolean() : false,
            };
        }

        #endregion Methods > Bag Generation Methods > Options Bags

        #region Methods > Bag Generation Methods > Demographic Chart Data Bags

        /// <summary>
        /// Gets the gender chart data bag.
        /// </summary>
        /// <param name="activeAlivePersons">A collection of <see cref="PersonViewModel"/> representing the persons to be included in the chart data.</param>
        /// <returns>
        /// An <see cref="InsightsChartDataBag"/> containing the percentage distribution of gender among the provided persons.
        /// </returns>
        private InsightsChartDataBag GetGenderChartDataBag( IEnumerable<PersonViewModel> activeAlivePersons )
        {
            var chartBag = BuildChartBag<PieSeriesBag>();

            var chartData = activeAlivePersons
                .GroupBy( person => person.Gender )
                .ToDictionary(
                    grouping => grouping.Key.ToString(),
                    grouping => grouping.Count().ToString()
                );

            PushChartDataIntoChartBag<PieSeriesBag>( chartBag, chartData, isPercentage: false );

            if ( chartBag.SeriesBags[0] != null )
            {
                ((PieSeriesBag)chartBag.SeriesBags[0]).Colors = new List<string>
                {
                    "#E7E5E4",
                    "#B2D7FF",
                    "#FFB2C1",
                };
            }

            return new InsightsChartDataBag
            {
                InsightCategory = "Demographics",
                InsightSubcategory = "Gender",
                ChartBag = chartBag,
            };
        }

        /// <summary>
        /// Gets the age chart data bag.
        /// </summary>
        /// <param name="activeAlivePersons">A collection of <see cref="PersonViewModel"/> representing the persons to be included in the chart data.</param>
        /// <returns>
        /// An <see cref="InsightsChartDataBag"/> containing the percentage distribution of age among the provided persons.
        /// </returns>
        private InsightsChartDataBag GetAgeChartDataBag( IEnumerable<PersonViewModel> activeAlivePersons )
        {
            var chartBag = BuildChartBag<BarSeriesBag>();

            var totalPersons = activeAlivePersons.Count();
            var personsWithAge = activeAlivePersons.Where( person => person.BirthDate.HasValue ).ToList();
            var personsWithoutAge = activeAlivePersons.Where( person => !person.BirthDate.HasValue );
            var ageBrackets = Enum.GetValues( typeof( AgeBracket ) ).Cast<AgeBracket>();

            var chartData = new Dictionary<string, string>();
            foreach ( var ageBracket in ageBrackets )
            {
                if ( ageBracket == AgeBracket.Unknown )
                {
                    chartData["Unknown"] = GetPercentage( personsWithoutAge.Count(), totalPersons ).ToString();
                }
                else
                {
                    chartData[ConvertAgeBracketToNumber( ageBracket )] = GetPercentage( personsWithAge.Count( p => p.AgeBracket == ageBracket ), totalPersons ).ToString();
                }
            }

            PushChartDataIntoChartBag<BarSeriesBag>( chartBag, chartData, isPercentage: true );

            return new InsightsChartDataBag
            {
                InsightCategory = "Demographics",
                InsightSubcategory = "Age",
                ChartBag = chartBag,
            };
        }

        /// <summary>
        /// Gets the chart data bag by percentage of the provided property, using a generic chart series type.
        /// </summary>
        /// <typeparam name="TSeries">The type of chart series to use, must implement <see cref="IChartSeriesBag"/> and have a parameterless constructor.</typeparam>
        /// <param name="persons">A collection of <see cref="PersonViewModel"/> representing the persons to be included in the chart data.</param>
        /// <param name="propertyName">A string representing a valid property of <see cref="PersonViewModel"/>.</param>
        /// <param name="category">The insight category.</param>
        /// <param name="subcategory">The insight subcategory.</param>
        /// <returns>
        /// An <see cref="InsightsChartDataBag"/> containing the percentage distribution of the specified property among the provided persons.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the property name does not exist on <see cref="PersonViewModel"/>.</exception>
        private InsightsChartDataBag GetChartDataBagByPercentageOfPersonProperty<TSeries>(
            IEnumerable<PersonViewModel> persons,
            string propertyName,
            string category,
            string subcategory,
            bool isPercentage )
            where TSeries : class, IChartSeriesBag, new()
        {
            var chartBag = BuildChartBag<TSeries>();

            var property = typeof(PersonViewModel).GetProperty(propertyName);

            if (property == null)
            {
                throw new ArgumentException($"Property '{propertyName}' does not exist on PersonViewModel.");
            }

            var labelCounts = isPercentage
                ? persons
                    .Select(person =>
                    {
                        var value = property.GetValue(person);
                        if (value == null)
                        {
                            return "Unknown";
                        }
                        if (value is int intValue)
                        {
                            var definedValue = DefinedValueCache.Get(intValue);
                            return definedValue?.Value ?? "Unknown";
                        }
                        return value.ToString();
                    })
                    .GroupBy(label => label)
                    .ToDictionary(g => g.Key, g => GetPercentage(g.Count(), persons.Count()).ToString())
                : persons
                    .Select(person =>
                    {
                        var value = property.GetValue(person);
                        if (value == null)
                        {
                            return "Unknown";
                        }
                        if (value is int intValue)
                        {
                            var definedValue = DefinedValueCache.Get(intValue);
                            return definedValue?.Value ?? "Unknown";
                        }
                        return value.ToString();
                    })
                    .GroupBy(label => label)
                    .ToDictionary(g => g.Key, g => g.Count().ToString());

            PushChartDataIntoChartBag<TSeries>( chartBag, labelCounts, isPercentage );

            return new InsightsChartDataBag
            {
                InsightCategory = category,
                InsightSubcategory = subcategory,
                ChartBag = chartBag,
            };
        }

        #endregion Methods > Bag Generation Methods > Demographic Chart Data Bags

        #region Methods > Bag Generation Methods > Information Statistics Data Bags

        /// <summary>
        /// Gets the chart data bag based on supplied persons and their information completeness.
        /// </summary>
        /// <param name="activeAlivePersons">A collection of <see cref="PersonViewModel"/> representing the persons to be included in the chart data.</param>
        /// <param name="rockContext">The database context to use for querying.</param>
        /// <returns>
        /// An <see cref="InsightsChartDataBag"/> containing the percentage distribution of the information completeness of the provided persons.
        /// </returns>
        private InsightsChartDataBag GetInformationCompletenessDataBag( IEnumerable<PersonViewModel> activeAlivePersons, RockContext rockContext )
        {
            var chartBag = BuildChartBag<BarSeriesBag>();
            int total = activeAlivePersons.Count();

            var personsInfo = activeAlivePersons.Select( person => new
            {
                HasAge = person.AgeBracket != AgeBracket.Unknown,
                HasGender = person.Gender != Gender.Unknown,
                person.HasActiveEmail,
                HasMaritalStatus = person.MaritalStatusValueId.HasValue,
                HasPhoto = person.PhotoId.HasValue,
                HasBirthDate = person.BirthDate.HasValue,
                person.PrimaryFamilyId
            } ).ToList();

            int hasMobilePhoneCount = CountPersonsWithMobileNumber( activeAlivePersons, rockContext );
            int hasHomeAddressCount = CountPersonsWithHomeAddress( activeAlivePersons, rockContext );

            var chartData = new Dictionary<string, string>
            {
                { "Age", GetPercentage(personsInfo.Count(p => p.HasAge), total) },
                { "Gender", GetPercentage(personsInfo.Count(p => p.HasGender), total) },
                { "Active Email", GetPercentage(personsInfo.Count(p => p.HasActiveEmail), total) },
                { "Mobile Phone", GetPercentage(hasMobilePhoneCount, total) },
                { "Marital Status", GetPercentage(personsInfo.Count(p => p.HasMaritalStatus), total) },
                { "Photo", GetPercentage(personsInfo.Count(p => p.HasPhoto), total) },
                { "Date of Birth", GetPercentage(personsInfo.Count(p => p.HasBirthDate), total) },
                { "Home Address", GetPercentage(hasHomeAddressCount, total) }
            };

            PushChartDataIntoChartBag<BarSeriesBag>( chartBag, chartData, isPercentage: true );

            return new InsightsChartDataBag
            {
                InsightCategory = "InformationStatistics",
                InsightSubcategory = "InformationCompleteness",
                ChartBag = chartBag,
            };
        }

        /// <summary>
        /// Gets the chart data bag based on supplied persons that are active and have completed assessments.
        /// </summary>
        /// <param name="activeAlivePersons">A collection of <see cref="PersonViewModel"/> representing the persons to be included in the chart data.</param>
        /// <param name="rockContext">The database context to use for querying.</param>
        /// <returns>
        /// An <see cref="InsightsChartDataBag"/> containing the percentage distribution of the information completeness of the provided persons.
        /// </returns>
        private InsightsChartDataBag GetActiveIndividualsWithAssessmentsDataBag( IEnumerable<PersonViewModel> activeAlivePersons, RockContext rockContext )
        {
            var chartBag = BuildChartBag<BarSeriesBag>();

            var validPersonIds = new PersonService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where(
                person => person.RecordTypeValueId == _personRecordDefinedValueId
                && person.RecordStatusValueId == _activeStatusDefinedValueId
                && !person.IsDeceased
                )
                .Select( person => person.Id );

            var distinctCompletedAssessments = new AssessmentService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where(
                    assessment => assessment.Status == AssessmentRequestStatus.Complete
                    && validPersonIds.Contains( assessment.PersonAlias.PersonId )
                )
                .GroupBy( assessment => new { assessment.AssessmentTypeId, assessment.PersonAlias.PersonId } )
                .Select( grouping => grouping.Key );

            var assessmentTypeCounts = new AssessmentTypeService( rockContext )
                .Queryable()
                .AsNoTracking()
                .GroupJoin(
                    distinctCompletedAssessments,
                    assessmentType => assessmentType.Id,
                    assessment => assessment.AssessmentTypeId,
                    ( assessmentType, assessments ) => new
                    {
                        assessmentType.Title,
                        Count = assessments.Count()
                    }
                )
                .ToList();

            var chartData = assessmentTypeCounts
                .OrderBy( a => a.Title )
                .ToDictionary( a => a.Title, a => GetPercentage( a.Count, activeAlivePersons.Count() ) );

            PushChartDataIntoChartBag<BarSeriesBag>( chartBag, chartData, isPercentage: true );

            return new InsightsChartDataBag
            {
                InsightCategory = "InformationStatistics",
                InsightSubcategory = "ActiveIndividualsWithAssessments",
                ChartBag = chartBag,
            };
        }

        /// <summary>
        /// Gets the chart data bag based on supplied persons and their statuses.
        /// </summary>
        /// <param name="alivePersons">A collection of <see cref="PersonViewModel"/> representing the persons to be included in the chart data.</param>
        /// <returns>
        /// An <see cref="InsightsChartDataBag"/> containing the percentage distribution of the statuses of the provided persons.
        /// </returns>
        private InsightsChartDataBag GetRecordStatusesDataBag( IEnumerable<PersonViewModel> alivePersons )
        {
            var chartBag = BuildChartBag<PieSeriesBag>();

            var activeRecordStatuses = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS ).DefinedValues
                .Where( dv => dv.IsActive )
                .ToList();

            var alivePersonsCount = alivePersons.Count();
            var statusCounts = alivePersons
                .GroupBy( p => p.RecordStatusValueId )
                .ToDictionary( g => g.Key, g => g.Count() );

            var chartData = activeRecordStatuses.ToDictionary(
                status => status.Value,
                status => GetPercentage( statusCounts.GetValueOrDefault( status.Id, 0 ), alivePersonsCount )
            );

            PushChartDataIntoChartBag<PieSeriesBag>( chartBag, chartData, isPercentage: true );

            return new InsightsChartDataBag
            {
                InsightCategory = "InformationStatistics",
                InsightSubcategory = "RecordStatuses",
                ChartBag = chartBag,
            };
        }

        #endregion Methods > Bag Generation Methods > Information Statistics Data Bags

        #endregion Methods > Bag Generation Methods

        #region Methods > Helper Methods

        /// <summary>
        /// Gets the alive persons.
        /// </summary>
        /// <param name="rockContext">The database context to use for querying persons.</param>
        /// <returns>
        /// A list of <see cref="PersonViewModel"/> representing persons who are alive and not an anonymous giver.
        /// </returns>
        private List<PersonViewModel> GetAlivePersons( RockContext rockContext )
        {
            var personService = new PersonService( rockContext );
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

            return alivePersonsQry.ToList();
        }

        /// <summary>
        /// Counts the number of persons in the provided collection who have a mobile phone number.
        /// </summary>
        /// <param name="persons">A collection of <see cref="PersonViewModel"/> representing the persons to check for mobile numbers.</param>
        /// <param name="rockContext">The <see cref="RockContext"/> used to query phone numbers from the database.</param>
        /// <returns>
        /// An <see cref="int"/> representing the number of persons in the collection who have a mobile phone number.
        /// </returns>
        private static int CountPersonsWithMobileNumber( IEnumerable<PersonViewModel> persons, RockContext rockContext )
        {
            var mobilePhoneTypeGuid = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid();
            var personIdSet = new HashSet<int>( persons.Select( person => person.Id ) );

            var mobilePersonIds = new PhoneNumberService( rockContext ).Queryable()
                .AsNoTracking()
                .Where( number => number.NumberTypeValue.Guid == mobilePhoneTypeGuid )
                .Select( number => number.PersonId )
                .Distinct()
                .ToList();

            return mobilePersonIds.Count( id => personIdSet.Contains( id ) );
        }

        /// <summary>
        /// Counts the number of persons in the provided collection who have a home address.
        /// </summary>
        /// <param name="persons">A collection of <see cref="PersonViewModel"/> representing the persons to check for home addresses.</param>
        /// <param name="rockContext">The <see cref="RockContext"/> used to query group locations from the database.</param>
        /// <returns>
        /// An <see cref="int"/> representing the number of persons in the collection who have a home address.
        /// </returns>
        private static int CountPersonsWithHomeAddress( IEnumerable<PersonViewModel> persons, RockContext rockContext )
        {
            var homeLocationTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() ).Id;

            // Extract PrimaryFamilyIds (non-null)
            var familyIdSet = new HashSet<int>(
                persons.Select( p => p.PrimaryFamilyId )
                       .Where( id => id.HasValue )
                       .Select( id => id.Value )
            );

            var groupLocationService = new GroupLocationService( rockContext );
            var groupIdsWithHomeAddress = new HashSet<int>();
            const int batchSize = 200;

            // Manual batching over familyIdSet
            var batch = new List<int>( batchSize );
            foreach ( var id in familyIdSet )
            {
                batch.Add( id );

                if ( batch.Count >= batchSize )
                {
                    var batchResults = groupLocationService.Queryable()
                        .AsNoTracking()
                        .Where( gl =>
                            gl.GroupLocationTypeValueId == homeLocationTypeValueId &&
                            batch.Contains( gl.GroupId ) )
                        .Select( gl => gl.GroupId )
                        .Distinct()
                        .ToList();

                    foreach ( var gid in batchResults )
                    {
                        groupIdsWithHomeAddress.Add( gid );
                    }

                    batch.Clear();
                }
            }

            // Handle remaining batch
            if ( batch.Count > 0 )
            {
                var batchResults = groupLocationService.Queryable()
                    .AsNoTracking()
                    .Where( gl =>
                        gl.GroupLocationTypeValueId == homeLocationTypeValueId &&
                        batch.Contains( gl.GroupId ) )
                    .Select( gl => gl.GroupId )
                    .Distinct()
                    .ToList();

                foreach ( var gid in batchResults )
                {
                    groupIdsWithHomeAddress.Add( gid );
                }
            }

            // Final count based on matched family IDs
            var count = persons.Count( p =>
                p.PrimaryFamilyId.HasValue &&
                groupIdsWithHomeAddress.Contains( p.PrimaryFamilyId.Value ) );

            return count;
        }


        /// <summary>
        /// Calculates the percentage of a part relative to a total and returns it as a formatted string.
        /// </summary>
        /// <param name="count">The part value to calculate the percentage for. Must be non-negative.</param>
        /// <param name="total">The total value to calculate the percentage against. Must be non-negative.</param>
        /// <returns>A string representing the percentage value formatted to a maximum of two decimal places.  Returns "0" if
        /// <paramref name="total"/> is 0.</returns>
        private static string GetPercentage( int count, int total )
        {
            if ( total == 0 )
            {
                return "0";
            }

            var percent = ( decimal ) count / total * 100;

            if ( percent > 0 && percent < 0.01m )
            {
                return "0.01";
            }

            return percent.ToString( "0.##" ); // 2 decimal places max
        }

        /// <summary>
        /// Converts an <see cref="AgeBracket"/> enumeration value to its corresponding age range string.
        /// </summary>
        /// <param name="ageBracket">The age bracket to convert.</param>
        /// <returns>A string representing the age range corresponding to the specified <paramref name="ageBracket"/>. Returns
        /// "Unknown" if the age bracket is not recognized.</returns>
        private static string ConvertAgeBracketToNumber( AgeBracket ageBracket )
        {
            switch ( ageBracket )
            {
                case AgeBracket.ZeroToFive:
                    return "0-5";
                case AgeBracket.SixToTwelve:
                    return "6-12";
                case AgeBracket.ThirteenToSeventeen:
                    return "13-17";
                case AgeBracket.EighteenToTwentyFour:
                    return "18-24";
                case AgeBracket.TwentyFiveToThirtyFour:
                    return "25-34";
                case AgeBracket.ThirtyFiveToFortyFour:
                    return "35-44";
                case AgeBracket.FortyFiveToFiftyFour:
                    return "45-54";
                case AgeBracket.FiftyFiveToSixtyFour:
                    return "55-64";
                case AgeBracket.SixtyFiveOrOlder:
                    return "65+";
                case AgeBracket.Unknown:
                default:
                    return "Unknown";
            }
        }

        /// <summary>
        /// Pushes chart data into the provided chart bag by creating new series for each data point.
        /// </summary>
        /// <typeparam name="TSeries">The type of chart series to add, must implement <see cref="IChartSeriesBag"/> and have a parameterless constructor.</typeparam>
        /// <param name="chartBag">The chart bag to which the series will be added.</param>
        /// <param name="chartData">A dictionary containing chart data, where each key is a label and each value is the percentage string.</param>
        private static void PushChartDataIntoChartBag<TSeries>( ChartBag chartBag, Dictionary<string, string> chartData, bool isPercentage = false )
            where TSeries : class, IChartSeriesBag, new()
        {
            foreach ( var kvp in chartData )
            {
                if ( double.TryParse( kvp.Value, out var value ) )
                {
                    if ( chartBag.SeriesBags.Count() == 0 )
                    {
                        chartBag.SeriesBags.Add( new TSeries() );
                    }

                    chartBag.Labels.Add( kvp.Key );

                    if ( isPercentage )
                    {
                        chartBag.SeriesBags[0].Data.Add( value / 100 );
                    }
                    else
                    {
                        chartBag.SeriesBags[0].Data.Add( value );
                    }    
                }
            }
        }

        /// <summary>
        /// Builds a new chart bag of the specified type with default colors and empty data.
        /// </summary>
        /// <typeparam name="TChartBag">The type of chart bag to build (BarChartBag or PieChartBag).</typeparam>
        /// <typeparam name="TSeriesBag">The type of series bag to use (BarSeriesBag or PieSeriesBag).</typeparam>
        /// <returns>
        /// An instance of <typeparamref name="TChartBag"/> with default color settings and empty labels/data.
        /// </returns>
        private ChartBag BuildChartBag<TSeriesBag>()
            where TSeriesBag : IChartSeriesBag, new()
        {
            var seriesBag = new TSeriesBag
            {
                Data = new List<double>(),
            };

            // Bar and Line Chart
            var colorsProperty = typeof( TSeriesBag ).GetProperty( "Color" );
            if ( colorsProperty != null && colorsProperty.CanWrite )
            {
                if ( colorsProperty.PropertyType == typeof( string ) )
                {
                    // Line Chart
                    colorsProperty.SetValue( seriesBag, _defaultColors.First() );
                }
                else if ( colorsProperty.PropertyType == typeof( List<string> ) )
                {
                    // Bar Chart
                    colorsProperty.SetValue( seriesBag, _defaultColors );
                }
            }

            // Pie Chart
            colorsProperty = typeof( TSeriesBag ).GetProperty( "Colors" );
            if ( colorsProperty != null && colorsProperty.CanWrite )
            {
                colorsProperty.SetValue( seriesBag, _defaultColors );
            }

            var chartBag = new ChartBag
            {
                Labels = new List<string>(),
                SeriesBags = new List<IChartSeriesBag> { seriesBag },
                ChartOptionsBag = new ChartOptionsBag(),
            };
            return chartBag;
        }

        #endregion Methods > Helper Methods

        #endregion

        #region Block Actions

        #endregion

        #region Helper Classes

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

        #endregion
    }
}
