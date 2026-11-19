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
using System.Diagnostics;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock;
using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Tests.Integration.TestData.Core;
using Rock.Tests.Integration.TestFramework.Database;
using Rock.Tests.Shared.Constants;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Reporting.ReportBuilder
{
    /// <summary>
    /// Create and manage test data for the Rock CRM module.
    /// </summary>
    [TestClass]
    [TestCategory( TestFeatures.Reporting )]
    public class ReportBuilderTests : DatabaseTestsBase
    {
        #region Tests

        /// <summary>
        /// Verify the Report Builder can correctly build all Person Attribute columns.
        /// </summary>
        [TestMethod]
        public void AllPersonAttributeColumnsCanBuild()
        {
            var dataContext = RockApp.Current.CreateRockContext();

            var personService = new PersonService( dataContext );

            // Get Admin Person as Report User.
            var adminPerson = GetAdminPersonOrThrow( personService );

            var parameterExpression = personService.ParameterExpression;

            var filterQuery = personService.Queryable();

            var filterExpression = FilterExpressionExtractor.Extract<Person>( filterQuery, parameterExpression, "p" );

            // Get Person Attributes
            var person = new Person();

            person.LoadAttributes();

            // Create a report for each Person Attribute, and build the output.
            var sortedAttributes = person.Attributes.OrderBy( x => x.Value.Name ).Select( x => x.Value ).ToList();

            foreach ( var attribute in sortedAttributes )
            {
                Debug.Print( $"Building Person Report [IncludedAttribute={attribute.Name}]..." );

                // Create the Report Template.
                var templateBuilder = new ReportTemplateBuilder( typeof( Person ) );

                var attributeName = attribute.Name;

                var field = templateBuilder.AddAttributeField( attribute.Guid, "AttributeValue" );

                field.SortDirection = System.Web.UI.WebControls.SortDirection.Ascending;
                field.SortOrder = 1;

                var report = templateBuilder.Report;

                // Build the output data for the Report by combining the report template with the filter.
                var builder = new ReportOutputBuilder( report, dataContext );

                var results = builder.GetReportData( adminPerson,
                    filterExpression,
                    parameterExpression,
                    dataContext,
                    ReportOutputBuilder.ReportOutputBuilderFieldContentSpecifier.FormattedText );

                var dataTable = results.Data;

                var valueCount = dataTable.Select( "[AttributeValue] > ''" ).Count();

                if ( valueCount == 0 )
                {
                    Debug.Print( $"WARNING: Report contains no values for this Attribute." );
                }
            }
        }

        /// <summary>
        /// Verify the Report Builder returns an empty Attribute column if the user generating the report does not have View permission for the Attribute.
        /// </summary>
        [TestMethod]
        public void UnauthorizedUserCannotViewAttributeColumnOutput()
        {
            var dataContext = RockApp.Current.CreateRockContext();

            var personService = new PersonService( dataContext );

            // Get Admin User (authorized) and Staff Member (unauthorized) as our Report Users.
            var adminPerson = GetAdminPersonOrThrow( personService );
            var unauthorizedPerson = GetStaffPersonOrThrow( personService );

            // Create a basic query for the report.
            var parameterExpression = personService.ParameterExpression;

            var filterQuery = personService.Queryable();

            var filterExpression = FilterExpressionExtractor.Extract<Person>( filterQuery, parameterExpression, "p" );

            // Get a Person Attribute for which the unauthorized Person does not have View permission.
            var person = new Person();

            person.LoadAttributes();

            var unauthorizedAttribute = person.Attributes
                 .Select( x => x.Value )
                 .FirstOrDefault( a => a.Key == "BackgroundCheckDate" && !a.IsAuthorized( Rock.Security.Authorization.VIEW, unauthorizedPerson ) );

            Assert.IsNotNull( unauthorizedAttribute, "Test User must have at least one unauthorized Attribute." );

            // Ensure the unauthorized Person has a value for the unauthorized Attribute.
            unauthorizedPerson.LoadAttributes( dataContext );
            unauthorizedPerson.SetAttributeValue( unauthorizedAttribute.Key, "2026-06-01" );
            unauthorizedPerson.SaveAttributeValues( dataContext );

            // Create a report template containing the test Attribute.
            var templateBuilder = new ReportTemplateBuilder( typeof( Person ) );

            var attributeName = unauthorizedAttribute.Name;

            var field = templateBuilder.AddAttributeField( unauthorizedAttribute.Guid, "AttributeValue" );

            field.SortDirection = System.Web.UI.WebControls.SortDirection.Ascending;
            field.SortOrder = 1;

            var report = templateBuilder.Report;

            // Build the output data for the Report by combining the report template with the filter.
            var builder = new ReportOutputBuilder( report, dataContext );

            // Build and verify the report output for the authorized user.
            var results1 = builder.GetReportData( adminPerson,
                filterExpression,
                parameterExpression,
                dataContext,
                ReportOutputBuilder.ReportOutputBuilderFieldContentSpecifier.FormattedText );

            var valueCount1 = results1.Data.Select( "[AttributeValue] > ''" ).Count();

            Assert.IsGreaterThan( 0, valueCount1, "Attribute column must contain at least one value." );

            // Build and verify the report output for the unauthorized user.
            builder.OutputFieldMask = "@@@";

            var results2 = builder.GetReportData( unauthorizedPerson,
                filterExpression,
                parameterExpression,
                dataContext,
                ReportOutputBuilder.ReportOutputBuilderFieldContentSpecifier.FormattedText );

            var valueCount2 = results2.Data.Select( "[AttributeValue] > '' AND [AttributeValue] <> '@@@'" ).Count();

            Assert.AreEqual( 0, valueCount2, "Attribute column contains unauthorized values." );
        }

        /// <summary>
        /// Verify a report that mixes entity fields and attribute columns, sorted
        /// on an attribute and then an entity field, returns the expected rows,
        /// values and order, both for the whole result set and for a single page.
        /// </summary>
        [TestMethod]
        public void ReportWithEntityAttributeAndSortColumns_ReturnsExpectedValues()
        {
            var dataContext = new RockContext();
            var personService = new PersonService( dataContext );
            var adminPerson = GetAdminPersonOrThrow( personService );

            var testPeople = AddReportTestAttributeValues( dataContext, includeTieOnRank: true );
            var testPersonIds = testPeople.Select( p => p.Id ).ToList();

            var parameterExpression = personService.ParameterExpression;
            var filterQuery = personService.Queryable().Where( p => testPersonIds.Contains( p.Id ) );
            var filterExpression = FilterExpressionExtractor.Extract<Person>( filterQuery, parameterExpression, "p" );

            // Sort on Rank (integer, ValueAsNumeric) descending, then Note
            // (text, Value) ascending. Entity fields are output only, because
            // ReportOutputBuilder can't sort on a property column: it sorts by
            // the DataTable column name, which doesn't match the query field.
            var templateBuilder = new ReportTemplateBuilder( typeof( Person ) );

            var firstNameField = templateBuilder.AddPropertyField( "FirstName", "FirstName" );
            var lastNameField = templateBuilder.AddPropertyField( "LastName", "LastName" );

            var rankField = templateBuilder.AddAttributeField( _rankAttributeGuid, "Rank" );
            rankField.SortDirection = System.Web.UI.WebControls.SortDirection.Descending;
            rankField.SortOrder = 1;

            var noteField = templateBuilder.AddAttributeField( _noteAttributeGuid, "Note" );
            noteField.SortDirection = System.Web.UI.WebControls.SortDirection.Ascending;
            noteField.SortOrder = 2;

            var builder = new ReportOutputBuilder( templateBuilder.Report, dataContext );

            // All rows.
            var allResults = builder.GetReportData( adminPerson,
                filterExpression,
                parameterExpression,
                dataContext,
                ReportOutputBuilder.ReportOutputBuilderFieldContentSpecifier.RawValue );

            var allRows = ReadRows( allResults, firstNameField, lastNameField, rankField, noteField );

            // FirstName is the legal first name, not the nick name. Ties on
            // Rank are broken by Note. People without a Rank value sort last
            // because SQL Server sorts NULL lowest.
            var expectedRows = new List<string>
            {
                "Alisha|Marble|30|Note Alisha",
                "William|Marble|20|Note Bill",
                "Theodore|Decker|20|Note Ted",
                "Cynthia|Decker|10|Note Cindy",
                "Noah|Decker||"
            };

            CollectionAssert.AreEqual( expectedRows, allRows, "Actual rows: " + string.Join( " / ", allRows ) );
            Assert.AreEqual( 5, allResults.ReportRowCount );

            // Second page of two.
            var pageResults = builder.GetReportData( adminPerson,
                filterExpression,
                parameterExpression,
                dataContext,
                ReportOutputBuilder.ReportOutputBuilderFieldContentSpecifier.RawValue,
                pageIndex: 1,
                pageSize: 2 );

            var pageRows = ReadRows( pageResults, firstNameField, lastNameField, rankField, noteField );

            CollectionAssert.AreEqual( expectedRows.Skip( 2 ).Take( 2 ).ToList(), pageRows, "Actual rows: " + string.Join( " / ", pageRows ) );
            Assert.AreEqual( 5, pageResults.ReportRowCount );
            Assert.AreEqual( 3, pageResults.PageCount );
        }

        /// <summary>
        /// Verify <see cref="Report.GetQueryable(ReportGetQueryableArgs)"/> returns
        /// the expected attribute values and order for a report with an entity
        /// field and attribute columns.
        /// </summary>
        [TestMethod]
        public void ReportGetQueryable_WithAttributeColumns_ReturnsExpectedValues()
        {
            AssertReportGetQueryableOutput( useAttributePersistedTextValues: false );
        }

        /// <summary>
        /// Verify <see cref="Report.GetQueryable(ReportGetQueryableArgs)"/> returns
        /// the persisted text of attribute values when
        /// <see cref="ReportGetQueryableArgs.UseAttributePersistedTextValues"/> is set.
        /// </summary>
        [TestMethod]
        public void ReportGetQueryable_WithPersistedTextValues_ReturnsExpectedValues()
        {
            AssertReportGetQueryableOutput( useAttributePersistedTextValues: true );
        }

        #endregion

        #region Support Methods

        private static readonly Guid _rankAttributeGuid = new Guid( "6B1F0A52-6E4A-4C6B-9C34-3F7C2A1D8E01" );
        private static readonly Guid _noteAttributeGuid = new Guid( "6B1F0A52-6E4A-4C6B-9C34-3F7C2A1D8E02" );

        /// <summary>
        /// Runs a report through <see cref="Report.GetQueryable(ReportGetQueryableArgs)"/>
        /// sorted on the Rank attribute, and checks the top rows.
        /// </summary>
        /// <param name="useAttributePersistedTextValues">Passed through to the queryable args.</param>
        private void AssertReportGetQueryableOutput( bool useAttributePersistedTextValues )
        {
            var dataContext = new RockContext();

            AddReportTestAttributeValues( dataContext, includeTieOnRank: false );

            var rankAttribute = AttributeCache.Get( _rankAttributeGuid );
            var noteAttribute = AttributeCache.Get( _noteAttributeGuid );

            var entityFields = new Dictionary<int, EntityField>
            {
                [1] = EntityHelper.GetEntityFields( typeof( Person ), false, false )
                    .First( f => f.FieldKind == FieldKind.Property && f.Name == "LastName" )
            };

            var attributes = new Dictionary<int, AttributeCache>
            {
                [2] = rankAttribute,
                [3] = noteAttribute
            };

            var rankColumn = $"Attribute_{rankAttribute.Id}_2";
            var noteColumn = $"Attribute_{noteAttribute.Id}_3";

            // No DataView, so the report covers everyone. Only the four test
            // people have a Rank, and they all have distinct values, so the
            // top four rows sorted on Rank are exactly those people.
            var report = new Report
            {
                EntityTypeId = EntityTypeCache.GetId<Person>(),
                FetchTop = 4
            };

            var queryable = report.GetQueryable( new ReportGetQueryableArgs
            {
                ReportDbContext = dataContext,
                EntityFields = entityFields,
                Attributes = attributes,
                SelectComponents = new Dictionary<int, ReportField>(),
                SortProperty = new Rock.Web.UI.Controls.SortProperty
                {
                    Direction = System.Web.UI.WebControls.SortDirection.Ascending,
                    // GetQueryable doesn't trim sort columns, so no space after the comma.
                    Property = $"{rankColumn} desc,Id"
                },
                UseAttributePersistedTextValues = useAttributePersistedTextValues
            } );

            var rows = new List<string>();
            foreach ( var row in queryable )
            {
                var rowType = row.GetType();
                var lastName = rowType.GetProperty( "Entity_LastName_1" ).GetValue( row );
                var rank = rowType.GetProperty( rankColumn ).GetValue( row );
                var note = rowType.GetProperty( noteColumn ).GetValue( row );

                if ( useAttributePersistedTextValues )
                {
                    Assert.IsInstanceOfType( rank, typeof( string ), "Persisted text Rank must be a string." );
                }
                else
                {
                    Assert.IsInstanceOfType( rank, typeof( decimal ), "Rank must come from ValueAsNumeric." );
                }

                rows.Add( $"{lastName}|{Convert.ToDecimal( rank ):0}|{note}" );
            }

            // Persisted text values are strings, so they sort as text.
            var expectedRows = useAttributePersistedTextValues
                ? new List<string>
                {
                    "Marble|5|Note Bill",
                    "Marble|30|Note Alisha",
                    "Decker|20|Note Ted",
                    "Decker|10|Note Cindy"
                }
                : new List<string>
                {
                    "Marble|30|Note Alisha",
                    "Decker|20|Note Ted",
                    "Decker|10|Note Cindy",
                    "Marble|5|Note Bill"
                };

            CollectionAssert.AreEqual( expectedRows, rows, "Actual rows: " + string.Join( " / ", rows ) );
        }

        /// <summary>
        /// Adds an integer "Rank" and a text "Note" Person attribute and sets
        /// values for a few known people. Noah Decker gets no values.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        /// <param name="includeTieOnRank">If true, Ted Decker and Bill Marble share a Rank, so the second sort column decides their order.</param>
        /// <returns>The test people: Ted, Cindy and Noah Decker, Alisha and Bill Marble.</returns>
        private List<Person> AddReportTestAttributeValues( RockContext dataContext, bool includeTieOnRank )
        {
            if ( AttributeCache.Get( _rankAttributeGuid ) == null )
            {
                EntityAttributeDataManager.Instance.AddEntityAttributes( new List<AddEntityAttributeArgs>
                {
                    new AddEntityAttributeArgs
                    {
                        ForeignKey = "IntegrationTest",
                        Guid = _rankAttributeGuid,
                        EntityTypeIdentifier = SystemGuid.EntityType.PERSON,
                        Key = "IntegrationTestReportRank",
                        FieldTypeIdentifier = SystemGuid.FieldType.INTEGER
                    },
                    new AddEntityAttributeArgs
                    {
                        ForeignKey = "IntegrationTest",
                        Guid = _noteAttributeGuid,
                        EntityTypeIdentifier = SystemGuid.EntityType.PERSON,
                        Key = "IntegrationTestReportNote",
                        FieldTypeIdentifier = SystemGuid.FieldType.TEXT
                    }
                } );
            }

            var personService = new PersonService( dataContext );
            var people = new List<Person>
            {
                GetPersonOrThrow( personService, "Ted", "Decker" ),
                GetPersonOrThrow( personService, "Cindy", "Decker" ),
                GetPersonOrThrow( personService, "Noah", "Decker" ),
                GetPersonOrThrow( personService, "Alisha", "Marble" ),
                GetPersonOrThrow( personService, "Bill", "Marble" )
            };

            var ranks = new Dictionary<string, string>
            {
                ["Ted"] = "20",
                ["Cindy"] = "10",
                ["Noah"] = string.Empty,
                ["Alisha"] = "30",
                ["Bill"] = includeTieOnRank ? "20" : "5"
            };

            foreach ( var person in people )
            {
                var rank = ranks[person.NickName];

                person.LoadAttributes( dataContext );
                person.SetAttributeValue( "IntegrationTestReportRank", rank );
                person.SetAttributeValue( "IntegrationTestReportNote", rank.IsNullOrWhiteSpace() ? string.Empty : $"Note {person.NickName}" );
                person.SaveAttributeValues( dataContext );
            }

            return people;
        }

        /// <summary>
        /// Reads the report rows as "FirstName|LastName|Rank|Note" strings.
        /// </summary>
        private static List<string> ReadRows( ReportOutputBuilder.TabularReportOutputResult results, ReportField firstNameField, ReportField lastNameField, ReportField rankField, ReportField noteField )
        {
            var columnMap = results.ReportFieldToDataColumnMap;

            return results.Data.Rows
                .Cast<System.Data.DataRow>()
                .Select( r =>
                {
                    var rank = r[columnMap[rankField.Guid]];
                    var rankText = rank == DBNull.Value || rank == null ? string.Empty : Convert.ToDecimal( rank ).ToString( "0" );

                    return $"{r[columnMap[firstNameField.Guid]]}|{r[columnMap[lastNameField.Guid]]}|{rankText}|{r[columnMap[noteField.Guid]]}";
                } )
                .ToList();
        }

        /// <summary>
        /// Get a known Person from the sample data by nick name and last name.
        /// </summary>
        private Person GetPersonOrThrow( PersonService personService, string nickName, string lastName )
        {
            var person = personService.Queryable().FirstOrDefault( x => x.NickName == nickName && x.LastName == lastName );

            if ( person == null )
            {
                throw new Exception( $"Person {nickName} {lastName} not found in test data set." );
            }

            return person;
        }

        /// <summary>
        /// Get a known Person who has been assigned a security role of Administrator.
        /// </summary>
        /// <param name="personService"></param>
        /// <returns></returns>
        private Person GetAdminPersonOrThrow( PersonService personService )
        {
            var adminPerson = personService.Queryable().FirstOrDefault( x => x.FirstName == "Alisha" && x.LastName == "Marble" );

            if ( adminPerson == null )
            {
                throw new Exception( "Admin Person not found in test data set." );
            }

            return adminPerson;
        }

        /// <summary>
        /// Get a known Person who has been assigned a security role of Staff Member.
        /// </summary>
        /// <param name="personService"></param>
        /// <returns></returns>
        private Person GetStaffPersonOrThrow( PersonService personService )
        {
            var staffPerson = personService.Queryable().FirstOrDefault( x => x.NickName == "Ted" && x.LastName == "Decker" );

            if ( staffPerson == null )
            {
                throw new Exception( "Staff Person not found in test data set." );
            }

            return staffPerson;
        }

        #endregion
    }
}
