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
using System.Diagnostics;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Reporting.ReportBuilder
{
    /// <summary>
    /// Create and manage test data for the Rock CRM module.
    /// </summary>
    [TestClass]
    public class ReportBuilderTests : DatabaseTestsBase
    {
        #region Tests

        /// <summary>
        /// Verify the Report Builder can correctly build all Person Attribute columns.
        /// </summary>
        [TestMethod]
        [TestCategory( "Rock.Reporting.Tests" )]
        [TestProperty( "Feature", TestFeatures.Reporting )]
        public void AllPersonAttributeColumnsCanBuild()
        {
            var dataContext = new RockContext();

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
        [TestCategory( "Rock.Reporting.Tests" )]
        [TestProperty( "Feature", TestFeatures.Reporting )]
        public void UnauthorizedUserCannotViewAttributeColumnOutput()
        {
            var dataContext = new RockContext();

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
                 .Select( x => x.Value ).FirstOrDefault( a => !a.IsAuthorized( Rock.Security.Authorization.VIEW, unauthorizedPerson ) );

            Assert.That.IsNotNull( unauthorizedAttribute, "Test User must have at least one unauthorized Attribute." );

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

            Assert.That.IsTrue( valueCount1 > 0, "Attribute column must contain at least one value." );

            // Build and verify the report output for the unauthorized user.
            builder.OutputFieldMask = "@@@";

            var results2 = builder.GetReportData( unauthorizedPerson,
                filterExpression,
                parameterExpression,
                dataContext,
                ReportOutputBuilder.ReportOutputBuilderFieldContentSpecifier.FormattedText );

            var valueCount2 = results2.Data.Select( "[AttributeValue] > '' AND [AttributeValue] <> '@@@'" ).Count();

            Assert.That.IsTrue( ( valueCount2 == 0 ), "Attribute column contains unauthorized values." );
        }

        #endregion

        #region Support Methods

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
