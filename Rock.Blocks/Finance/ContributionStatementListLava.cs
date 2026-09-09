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
using Rock.Lava;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Displays a listing of years for which contribution statements are available for the target person's giving unit.
    /// </summary>
    [DisplayName( "Contribution Statement List Lava" )]
    [Category( "Finance" )]
    [Description( "Block for displaying a listing of years where contribution statements are available." )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [ConfigurationChangedReload( Rock.Enums.Cms.BlockReloadMode.Page )]

    #region Block Attributes

    [AccountsField( "Accounts",
        Key = AttributeKey.Accounts,
        Description = "The accounts checked for giving when determining which years to list. Leave blank to use all tax-deductible accounts.",
        IsRequired = false,
        Order = 0 )]

    [IntegerField( "Max Years To Display",
        Key = AttributeKey.MaxYearsToDisplay,
        Description = "The maximum number of statement years listed, counting the current year.",
        IsRequired = true,
        DefaultValue = "3",
        Order = 1 )]

    [LinkedPage( "Detail Page",
        Key = AttributeKey.DetailPage,
        Description = "The page each statement year links to. Receives the selected year as the StatementYear parameter.",
        Order = 2 )]

    [CodeEditorField( "Lava Template",
        Key = AttributeKey.LavaTemplate,
        Description = "The template that renders the list of available statement years. Merge fields: StatementYears, DetailPage, PersonGuid.",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        EditorHeight = 500,
        IsRequired = true,
        DefaultValue = @"{% assign currentYear = 'Now' | Date:'yyyy' %}

<h4>Available Contribution Statements</h4>

<div class=""margin-b-md"">
{% for statementyear in StatementYears %}
    {% if currentYear == statementyear.Year %}
        <a href=""{{ DetailPage }}?StatementYear={{ statementyear.Year }}"" class=""btn btn-primary"">{{ statementyear.Year }} <small>YTD</small></a>
    {% else %}
        <a href=""{{ DetailPage }}?StatementYear={{ statementyear.Year }}"" class=""btn btn-primary"">{{ statementyear.Year }}</a>
    {% endif %}
{% endfor %}
</div>",
        Order = 3 )]

    [BooleanField( "Use Person Context",
        Key = AttributeKey.UsePersonContext,
        Description = "The person whose statements are listed. When enabled, the block uses the page's context person rather than the signed-in person.",
        DefaultBooleanValue = false,
        Order = 5 )]

    #endregion Block Attributes

    [Rock.Web.UI.ContextAware( typeof( Person ) )]
    [Rock.SystemGuid.EntityTypeGuid( "51A373C4-0F6C-4583-8228-090BA588D826" )]
    [Rock.SystemGuid.BlockTypeGuid( "22BF5B51-6511-4D31-8A48-4978A454C386" )]
    public class ContributionStatementListLava : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string Accounts = "Accounts";
            public const string MaxYearsToDisplay = "MaxYearsToDisplay";
            public const string DetailPage = "DetailPage";
            public const string LavaTemplate = "LavaTemplate";
            public const string UsePersonContext = "UsePersonContext";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        protected override string GetInitialHtmlContent()
        {
            try
            {
                return GetStatementsHtml();
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                return "<div class='alert alert-danger'>An error occurred while getting the available contribution statements.</div>";
            }
        }

        #endregion Methods

        #region Private Methods

        /// <summary>
        /// Resolves the configured Lava template against the years for which the target person's
        /// giving unit has contributions.
        /// </summary>
        /// <returns>The rendered HTML, or an empty string when the person has no qualifying transactions.</returns>
        private string GetStatementsHtml()
        {
            var statementYears = GetStatementYears();

            // Render nothing when the person has no qualifying transactions.
            if ( statementYears.Count == 0 )
            {
                return string.Empty;
            }

            var targetPerson = GetTargetPerson();

            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.Add( "DetailPage", new PageReference( GetAttributeValue( AttributeKey.DetailPage ) ).Route );
            mergeFields.Add( "StatementYears", statementYears );

            if ( targetPerson != null )
            {
                mergeFields.Add( "PersonGuid", targetPerson.Guid );
            }

            return GetAttributeValue( AttributeKey.LavaTemplate ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Gets the descending list of years, limited by the "Max Years To Display" setting, for which
        /// the target person's giving unit has contributions in the configured (or tax-deductible) accounts.
        /// </summary>
        /// <returns>The list of statement years, most recent first.</returns>
        private List<StatementYearInfo> GetStatementYears()
        {
            var targetPerson = GetTargetPerson();

            // Without a target person there is no giving unit to report on.
            if ( targetPerson == null )
            {
                return new List<StatementYearInfo>();
            }

            var numberOfYears = GetAttributeValue( AttributeKey.MaxYearsToDisplay ).AsInteger();

            // Gather every PersonAliasId that shares the target person's giving unit so a family's
            // giving is reported together and the query stays a single set-based lookup.
            var personAliasIds = new PersonAliasService( RockContext )
                .Queryable()
                .Where( a => a.Person.GivingId == targetPerson.GivingId )
                .Select( a => a.Id )
                .ToList();

            var qry = new FinancialTransactionDetailService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( t =>
                    t.Transaction.AuthorizedPersonAliasId.HasValue
                    && personAliasIds.Contains( t.Transaction.AuthorizedPersonAliasId.Value )
                    && t.Transaction.TransactionDateTime.HasValue );

            var accountGuids = GetAttributeValue( AttributeKey.Accounts ).SplitDelimitedValues().AsGuidList();
            if ( accountGuids.Any() )
            {
                var accountIds = FinancialAccountCache.GetByGuids( accountGuids ).Select( a => a.Id ).ToList();
                qry = qry.Where( t => accountIds.Contains( t.AccountId ) );
            }
            else
            {
                qry = qry.Where( t => t.Account.IsTaxDeductible );
            }

            return qry
                .GroupBy( t => t.Transaction.TransactionDateTime.Value.Year )
                .Select( g => g.Key )
                .OrderByDescending( year => year )
                .Take( numberOfYears )
                .ToList()
                .Select( year => new StatementYearInfo { Year = year } )
                .ToList();
        }

        /// <summary>
        /// Gets the person whose statements should be listed: the context person when the block is
        /// configured to use person context, otherwise the current person.
        /// </summary>
        /// <returns>The target person, or <c>null</c> when no person is available.</returns>
        private Person GetTargetPerson()
        {
            if ( GetAttributeValue( AttributeKey.UsePersonContext ).AsBoolean() )
            {
                return RequestContext.GetContextEntity<Person>();
            }

            return RequestContext.CurrentPerson;
        }

        #endregion Private Methods

        #region Support Classes

        /// <summary>
        /// A single statement year exposed to the Lava template.
        /// </summary>
        public class StatementYearInfo : LavaDataObject
        {
            /// <summary>
            /// Gets or sets the year a contribution statement is available for.
            /// </summary>
            public int Year { get; set; }
        }

        #endregion Support Classes
    }
}
