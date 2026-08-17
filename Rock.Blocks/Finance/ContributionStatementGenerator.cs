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
using System.ComponentModel;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Financial;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Finance.ContributionStatementGenerator;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Generates and displays a person's contribution statement as a rendered HTML document.
    /// </summary>
    /// <seealso cref="RockBlockType" />

    [DisplayName( "Contribution Statement Generator" )]
    [Category( "Finance" )]
    [Description( "Block for generating a Contribution Statement" )]

    #region Block Attributes

    [BooleanField(
        "Allow Person QueryString",
        Key = AttributeKey.AllowPersonQueryString,
        Description = "Determines if any person other than the currently logged in person is allowed to be passed through the query string. For security reasons this is not allowed by default.",
        DefaultBooleanValue = false,
        Order = 0 )]

    [FinancialStatementTemplateField(
        "Statement Template",
        Key = AttributeKey.FinancialStatementTemplate,
        DefaultValue = Rock.SystemGuid.FinancialStatementTemplate.ROCK_DEFAULT,
        Order = 1 )]

    #endregion Block Attributes

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Primary )]
    [Rock.SystemGuid.EntityTypeGuid( "2F36F257-B634-406F-9A75-14B44D5C2245" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "108E64AA-D77E-460D-82CB-952C4264FF66" )]
    [Rock.SystemGuid.BlockTypeGuid( "E0A699C3-61AA-4522-9067-1FE56FA80972" )]
    public class ContributionStatementGenerator : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string FinancialStatementTemplate = "FinancialStatementTemplate";
            public const string AllowPersonQueryString = "AllowPersonQueryString";
        }

        private static class PageParameterKey
        {
            public const string StatementYear = "StatementYear";
            public const string StatementStartMonth = "StatementStartMonth";
            public const string StatementEndMonth = "StatementEndMonth";
            public const string PersonActionIdentifier = "rckid";
            public const string PersonGuid = "PersonGuid";
        }

        /// <summary>
        /// The action string the person action identifier (rckid) must encode to view a statement.
        /// </summary>
        private const string PersonActionIdentifierAction = "contribution-statement";

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<ContributionStatementGeneratorBag, ContributionStatementGeneratorOptionsBag>();

            var targetPerson = GetTargetPerson();

            if ( targetPerson == null )
            {
                box.ErrorMessage = "Unable to load the contribution statement. The requested person could not be found.";
                return box;
            }

            try
            {
                var options = BuildStatementOptions();
                var recipient = BuildRecipient( targetPerson );

                var request = new FinancialStatementGeneratorRecipientRequest( options )
                {
                    FinancialStatementGeneratorRecipient = recipient
                };

                // The current (logged-in) person is passed for authorization, while the recipient identifies whose statement to build.
                var result = FinancialStatementGeneratorHelper.GetStatementGeneratorRecipientResult( request, RequestContext.CurrentPerson );

                box.Bag = new ContributionStatementGeneratorBag
                {
                    StatementHtml = BuildHtmlWithFooter( result )
                };
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                box.ErrorMessage = "An error occurred while generating the contribution statement. Please try again later.";
            }

            return box;
        }

        /// <summary>
        /// Determines whose statement should be generated, enforcing the block's person-access rules.
        /// </summary>
        /// <returns>The target <see cref="Person"/>, or <c>null</c> when no valid person can be resolved.</returns>
        private Person GetTargetPerson()
        {
            var targetPerson = RequestContext.CurrentPerson;
            var allowPersonQueryString = GetAttributeValue( AttributeKey.AllowPersonQueryString ).AsBoolean();
            var personService = new PersonService( RockContext );

            var personActionId = PageParameter( PageParameterKey.PersonActionIdentifier );
            var personGuid = PageParameter( PageParameterKey.PersonGuid ).AsGuidOrNull();

            if ( personActionId.IsNotNullOrWhiteSpace() )
            {
                var person = personService.GetByPersonActionIdentifier( personActionId, PersonActionIdentifierAction );

                if ( person != null )
                {
                    var isCurrentPersonsBusiness = targetPerson != null
                        && targetPerson.GetBusinesses( RockContext ).Any( b => b.Id == person.Id );

                    if ( allowPersonQueryString || isCurrentPersonsBusiness )
                    {
                        targetPerson = person;
                    }
                }
            }
            else if ( personGuid.HasValue )
            {
                // Gate on the requested Guid before resolving the person; when query-string access is
                // disabled, only a Guid belonging to one of the current person's businesses is allowed.
                var isCurrentPersonsBusiness = targetPerson != null
                    && targetPerson.GetBusinesses( RockContext ).Any( b => b.Guid == personGuid.Value );

                if ( allowPersonQueryString || isCurrentPersonsBusiness )
                {
                    var person = personService.Get( personGuid.Value );

                    if ( person != null )
                    {
                        targetPerson = person;
                    }
                }
            }

            return targetPerson;
        }

        /// <summary>
        /// Builds the statement generator options (date range and template) from the page parameters and block settings.
        /// </summary>
        /// <returns>The configured <see cref="FinancialStatementGeneratorOptions"/>.</returns>
        private FinancialStatementGeneratorOptions BuildStatementOptions()
        {
            var statementYear = PageParameter( PageParameterKey.StatementYear ).AsIntegerOrNull() ?? RockDateTime.Now.Year;
            var statementStartMonth = PageParameter( PageParameterKey.StatementStartMonth ).AsIntegerOrNull() ?? 0;
            var statementEndMonth = PageParameter( PageParameterKey.StatementEndMonth ).AsIntegerOrNull() ?? 0;

            // A start/end month outside 1-12 (or omitted) falls back to a full calendar year.
            var startMonth = ( statementStartMonth >= 1 && statementStartMonth <= 12 ) ? statementStartMonth : 1;
            var endMonth = ( statementEndMonth >= 1 && statementEndMonth <= 12 ) ? statementEndMonth : 12;

            var startDate = new DateTime( statementYear, startMonth, 1 );

            // EndDate is the exclusive upper bound: the first day of the month after the end month.
            var endDate = new DateTime( statementYear, endMonth, 1 ).AddMonths( 1 );

            // When the end month is on or before the start month, the range wraps into the following year (e.g., a fiscal year).
            if ( endDate <= startDate )
            {
                endDate = endDate.AddYears( 1 );
            }

            var templateGuid = GetAttributeValue( AttributeKey.FinancialStatementTemplate ).AsGuidOrNull()
                ?? Rock.SystemGuid.FinancialStatementTemplate.ROCK_DEFAULT.AsGuid();

            return new FinancialStatementGeneratorOptions
            {
                StartDate = startDate,
                EndDate = endDate,
                RenderMedium = "Html",
                FinancialStatementTemplateId = new FinancialStatementTemplateService( RockContext ).GetId( templateGuid )
            };
        }

        /// <summary>
        /// Builds the recipient that identifies whose giving the statement covers.
        /// </summary>
        /// <param name="targetPerson">The person the statement is being generated for.</param>
        /// <returns>The configured <see cref="FinancialStatementGeneratorRecipient"/>.</returns>
        private FinancialStatementGeneratorRecipient BuildRecipient( Person targetPerson )
        {
            var recipient = new FinancialStatementGeneratorRecipient
            {
                // The LocationId is required so the generator can fetch all the data the Lava template needs.
                LocationId = targetPerson.GetMailingLocation( RockContext )?.Id
            };

            if ( targetPerson.GivingGroupId.HasValue )
            {
                recipient.GroupId = targetPerson.GivingGroupId.Value;
            }
            else
            {
                recipient.GroupId = targetPerson.PrimaryFamilyId ?? 0;
                recipient.PersonId = targetPerson.Id;
            }

            return recipient;
        }

        /// <summary>
        /// Combines the generated statement HTML with its footer fragment.
        /// </summary>
        /// <param name="result">The statement generation result.</param>
        /// <returns>The complete HTML document, or <c>null</c> when no statement was produced.</returns>
        private string BuildHtmlWithFooter( FinancialStatementGeneratorRecipientResult result )
        {
            var html = result.Html;

            if ( html.IsNullOrWhiteSpace() )
            {
                return null;
            }

            if ( result.FooterHtmlFragment.IsNotNullOrWhiteSpace() )
            {
                // Insert the footer immediately before the closing body tag, falling back to appending it.
                var insertPosition = html.IndexOf( "</body>" );

                html = insertPosition >= 0
                    ? html.Insert( insertPosition, result.FooterHtmlFragment )
                    : html + result.FooterHtmlFragment;
            }

            return html;
        }

        #endregion Methods
    }
}
