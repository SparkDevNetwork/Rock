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
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter people based on their total contribution amount" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Giving Amount Filter" )]
    public class GivingAmountFilter : DataFilterComponent
    {
        #region Properties

        /// <summary>
        /// Gets the entity type that filter applies to.
        /// </summary>
        /// <value>
        /// The entity that filter applies to.
        /// </value>
        public override string AppliesToEntityType
        {
            get { return typeof( Rock.Model.Person ).FullName; }
        }

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get { return "Additional Filters"; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <value>
        /// The title.
        /// </value>
        public override string GetTitle( Type entityType )
        {
            return "Giving Amount";
        }

        /// <summary>
        /// Formats the selection on the client-side.  When the filter is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before 
        /// referencing this property.
        /// </summary>
        /// <value>
        /// The client format script.
        /// </value>
        public override string GetClientFormatSelection( Type entityType )
        {
            return @"
function() {
    var comparisonText = $('select:first', $content).find(':selected').text();
    var totalAmount = $('.number-box', $content).find('input').val();
    var dateRangeText = $('.js-slidingdaterange-text-value', $content).val()
    var combineGiving = $('[id$=""cbCombineGiving""]', $content).is(':checked')
    var accountPicker = $('.js-account-picker', $content);
    var accountNames = accountPicker.find('.selected-names').text()

    var result = '';    
    if (combineGiving) {
        result += 'Combined Giving amount total ';
    }
    else {
        result += 'Giving amount total ';
    }
    result += comparisonText.toLowerCase() + ' $' + totalAmount; 
    result += ' to accounts:' + accountNames;
    result += ' DateRange: ' + dateRangeText;
    

    return result;
}
";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            string result = "Giving Amount";
            var selectionConfig = SelectionConfig.Parse( selection );

            ComparisonType comparisonType = selectionConfig.ComparisonType;
            decimal amount = selectionConfig.Amount ?? 0.00M;
            string accountNames = string.Empty;

            var accountGuids = selectionConfig.AccountGuids;
            if ( selectionConfig.AccountGuids != null && selectionConfig.AccountGuids.Any() )
            {
                accountNames = new FinancialAccountService( new RockContext() ).GetByGuids( accountGuids ).Select( a => a.Name ).ToList().AsDelimited( ", ", " and " );
            }

            bool combineGiving = selectionConfig.CombineGiving;

            string toAccountsDescription;
            if ( !accountNames.IsNullOrWhiteSpace() )
            {
                toAccountsDescription = " to accounts: " + accountNames;
                if ( selectionConfig.IncludeChildAccounts )
                {
                    toAccountsDescription += " including child accounts";
                }
            }
            else
            {
                toAccountsDescription = string.Empty;
            }

            result = $@"
{( combineGiving ? "Combined " : string.Empty ) }Giving
amount total {comparisonType.ConvertToString().ToLower()} {amount.FormatAsCurrency()}
{toAccountsDescription}.
Date Range: {SlidingDateRangePicker.FormatDelimitedValues( selectionConfig.SlidingDateRangePickerDelimitedValues )}";

            if ( selectionConfig.UseAnalyticsModels )
            {
                result += " using analytics models";
            }

            return result;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var comparisonControl = ComparisonHelper.ComparisonControl( ComparisonType.LessThan | ComparisonType.GreaterThanOrEqualTo | ComparisonType.EqualTo );
            comparisonControl.ID = filterControl.ID + "_comparisonControl";
            filterControl.Controls.Add( comparisonControl );

            var globalAttributes = GlobalAttributesCache.Get();

            CurrencyBox numberBoxAmount = new CurrencyBox();
            numberBoxAmount.ID = filterControl.ID + "_numberBoxAmount";
            numberBoxAmount.Label = "Amount";

            filterControl.Controls.Add( numberBoxAmount );

            AccountPicker accountPicker = new AccountPicker();
            accountPicker.AllowMultiSelect = true;
            accountPicker.ID = filterControl.ID + "_accountPicker";
            accountPicker.AddCssClass( "js-account-picker" );
            accountPicker.Label = "Accounts";
            filterControl.Controls.Add( accountPicker );

            RockCheckBox cbIncludeChildAccounts = new RockCheckBox();
            cbIncludeChildAccounts.ID = filterControl.ID + "_cbIncludeChildAccounts";
            cbIncludeChildAccounts.Text = "Include Child Accounts";
            cbIncludeChildAccounts.CssClass = "js-include-child-accounts";
            filterControl.Controls.Add( cbIncludeChildAccounts );

            RockCheckBox cbIgnoreInactiveAccounts = new RockCheckBox();
            cbIgnoreInactiveAccounts.ID = filterControl.ID + "_cbIgnoreInactiveAccounts";
            cbIgnoreInactiveAccounts.Text = "Ignore Inactive Accounts";
            cbIgnoreInactiveAccounts.CssClass = "js-ignore-inactive-accounts";
            filterControl.Controls.Add( cbIgnoreInactiveAccounts );

            SlidingDateRangePicker slidingDateRangePicker = new SlidingDateRangePicker();
            slidingDateRangePicker.ID = filterControl.ID + "_slidingDateRangePicker";
            slidingDateRangePicker.AddCssClass( "js-sliding-date-range" );
            slidingDateRangePicker.Label = "Date Range";
            slidingDateRangePicker.Help = "The date range of the transactions using the transaction date of each transaction";
            slidingDateRangePicker.Required = true;
            filterControl.Controls.Add( slidingDateRangePicker );

            RockCheckBox cbCombineGiving = new RockCheckBox();
            cbCombineGiving.ID = filterControl.ID + "_cbCombineGiving";
            cbCombineGiving.Label = "Combine Giving";
            cbCombineGiving.CssClass = "js-combine-giving";
            cbCombineGiving.Help = "Combine individuals in the same giving group when calculating totals and reporting the list of individuals.";
            filterControl.Controls.Add( cbCombineGiving );

            RockCheckBox cbUseAnalytics = new RockCheckBox();
            cbUseAnalytics.ID = filterControl.ID + "_cbUseAnalytics";
            cbUseAnalytics.Label = "Use Analytics Models";
            cbUseAnalytics.CssClass = "js-use-analytics";
            cbUseAnalytics.Help = "Using Analytics Data might be faster than querying real-time data, but it may not include data that has been added or updated in the last 24 hours.";
            filterControl.Controls.Add( cbUseAnalytics );

            var controls = new Control[8] { comparisonControl, numberBoxAmount, accountPicker, cbIncludeChildAccounts, cbIgnoreInactiveAccounts, slidingDateRangePicker, cbCombineGiving, cbUseAnalytics };

            // set an initial config for the selection
            var selectionConfig = new SelectionConfig
            {
                ComparisonType = ComparisonType.GreaterThanOrEqualTo
            };

            SetSelection( entityType, controls, selectionConfig.ToJson() );

            return controls;
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            base.RenderControls( entityType, filterControl, writer, controls );
        }

        /// <summary>
        /// Class SelectionConfig.
        /// </summary>
        protected class SelectionConfig
        {
            /// <summary>
            /// Gets or sets the type of the comparison.
            /// </summary>
            /// <value>The type of the comparison.</value>
            public ComparisonType ComparisonType { get; set; }

            /// <summary>
            /// Gets or sets the amount.
            /// </summary>
            /// <value>The amount.</value>
            public decimal? Amount { get; set; }

            /// <summary>
            /// Gets or sets the sliding date range picker delimited values.
            /// </summary>
            /// <value>The sliding date range picker delimited values.</value>
            public string SlidingDateRangePickerDelimitedValues { get; set; }

            /// <summary>
            /// Gets or sets the account guids.
            /// </summary>
            /// <value>The account guids.</value>
            public List<Guid> AccountGuids { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [combine giving].
            /// </summary>
            /// <value><c>true</c> if [combine giving]; otherwise, <c>false</c>.</value>
            public bool CombineGiving { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [use analytics models].
            /// </summary>
            /// <value><c>true</c> if [use analytics models]; otherwise, <c>false</c>.</value>
            public bool UseAnalyticsModels { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [include child accounts].
            /// </summary>
            /// <value><c>true</c> if [include child accounts]; otherwise, <c>false</c>.</value>
            public bool IncludeChildAccounts { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [ignore inactive accounts].
            /// </summary>
            /// <value><c>true</c> if [ignore inactive accounts]; otherwise, <c>false</c>.</value>
            public bool IgnoreInactiveAccounts { get; set; }

            /// <summary>
            /// Parses from old Pipe Delimited format (pre-V13)
            /// </summary>
            /// <param name="selection">The selection.</param>
            /// <returns>System.String.</returns>
            private static SelectionConfig ParseFromLegacyConfig( string selection )
            {
                var selectionConfig = new SelectionConfig();
                string[] selectionValues = selection.Split( '|' );
                if ( selectionValues.Length < 4 )
                {
                    return null;
                }

                selectionConfig.ComparisonType = selectionValues[0].ConvertToEnum<ComparisonType>( Rock.Model.ComparisonType.GreaterThanOrEqualTo );
                selectionConfig.Amount = selectionValues[1].AsDecimalOrNull() ?? 0.00M;
                if ( selectionValues.Length >= 7 )
                {
                    string slidingDelimitedValues = selectionValues[6].Replace( ',', '|' );
                    selectionConfig.SlidingDateRangePickerDelimitedValues = slidingDelimitedValues;
                }
                else
                {
                    // if converting from a previous version of the selection
                    DateTime? startDate = selectionValues[2].AsDateTime();
                    DateTime? endDate = selectionValues[3].AsDateTime();
                    var slidingDateRangePicker = new SlidingDateRangePicker();
                    slidingDateRangePicker.SlidingDateRangeMode = SlidingDateRangePicker.SlidingDateRangeType.DateRange;
                    slidingDateRangePicker.DateRangeModeStart = startDate;
                    slidingDateRangePicker.DateRangeModeEnd = endDate;
                    selectionConfig.SlidingDateRangePickerDelimitedValues = slidingDateRangePicker.DelimitedValues;
                }

                var accountIdList = new List<int>();
                if ( selectionValues.Length >= 5 )
                {
                    selectionConfig.AccountGuids = selectionValues[4].Split( ',' ).Select( a => a.AsGuid() ).ToList();
                }

                if ( selectionValues.Length >= 6 )
                {
                    selectionConfig.CombineGiving = selectionValues[5].AsBooleanOrNull() ?? false;
                }

                selectionConfig.UseAnalyticsModels = false;
                selectionConfig.IncludeChildAccounts = false;
                selectionConfig.IgnoreInactiveAccounts = false;

                return selectionConfig;
            }

            /// <summary>
            /// Parses the specified selection.
            /// </summary>
            /// <param name="selection">The selection.</param>
            /// <returns>SelectionConfig.</returns>
            public static SelectionConfig Parse( string selection )
            {
                var selectionConfig = selection.FromJsonOrNull<SelectionConfig>();
                if ( selectionConfig != null )
                {
                    return selectionConfig;
                }

                // If selection config is NULL, it might be in the old pipe delimited format.
                return ParseFromLegacyConfig( selection );
            }
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            var comparisonControl = controls[0] as DropDownList;
            var numberBoxAmount = controls[1] as CurrencyBox;
            var accountPicker = controls[2] as AccountPicker;
            var cbIncludeChildAccounts = controls[3] as RockCheckBox;
            var cbIgnoreInactiveAccounts = controls[4] as RockCheckBox;
            var slidingDateRangePicker = controls[5] as SlidingDateRangePicker;
            var cbCombineGiving = controls[6] as RockCheckBox;
            var cbUseAnalyticsTables = controls[7] as RockCheckBox;

            var comparisonType = comparisonControl.SelectedValue.ConvertToEnum<ComparisonType>();
            decimal? amount = numberBoxAmount.Text.AsDecimal();

            var accountIdList = accountPicker.SelectedValuesAsInt().ToList();
            List<Guid> accountGuids;
            var accounts = new FinancialAccountService( new RockContext() ).GetByIds( accountIdList );
            if ( accounts != null && accounts.Any() )
            {
                accountGuids = accounts.Select( a => a.Guid ).ToList();
            }
            else
            {
                accountGuids = null;
            }

            var selectionConfig = new SelectionConfig
            {
                ComparisonType = comparisonType,
                Amount = amount,
                AccountGuids = accountGuids,
                IncludeChildAccounts = cbIncludeChildAccounts.Checked,
                IgnoreInactiveAccounts = cbIgnoreInactiveAccounts.Checked,
                CombineGiving = cbCombineGiving.Checked,
                SlidingDateRangePickerDelimitedValues = slidingDateRangePicker.DelimitedValues,
                UseAnalyticsModels = cbUseAnalyticsTables.Checked,
            };

            return selectionConfig.ToJson();
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var selectionConfig = SelectionConfig.Parse( selection );

            var comparisonControl = controls[0] as DropDownList;
            var numberBoxAmount = controls[1] as CurrencyBox;
            var accountPicker = controls[2] as AccountPicker;
            var cbIncludeChildAccounts = controls[3] as RockCheckBox;
            var cbIgnoreInactiveAccounts = controls[4] as RockCheckBox;
            var slidingDateRangePicker = controls[5] as SlidingDateRangePicker;
            var cbCombineGiving = controls[6] as RockCheckBox;
            var cbUseAnalyticsTables = controls[7] as RockCheckBox;

            // Comparison Control
            slidingDateRangePicker.DelimitedValues = selectionConfig.SlidingDateRangePickerDelimitedValues;
            comparisonControl.SetValue( selectionConfig.ComparisonType.ConvertToInt().ToString() );

            // Amount
            decimal? amount = selectionConfig.Amount;
            if ( amount.HasValue )
            {
                numberBoxAmount.Text = amount.Value.ToString( "F2" );
            }
            else
            {
                numberBoxAmount.Text = string.Empty;
            }

            // Accounts
            var accountGuids = selectionConfig.AccountGuids;
            List<FinancialAccount> accounts;
            if ( accountGuids != null && accountGuids.Any() )
            {
                accounts = new FinancialAccountService( new RockContext() ).GetByGuids( accountGuids ).ToList();
            }
            else
            {
                accounts = new List<FinancialAccount>();
            }

            accountPicker.SetValues( accounts );

            // everything else
            cbIncludeChildAccounts.Checked = selectionConfig.IncludeChildAccounts;
            cbIgnoreInactiveAccounts.Checked = selectionConfig.IgnoreInactiveAccounts;
            cbCombineGiving.Checked = selectionConfig.CombineGiving;
            cbUseAnalyticsTables.Checked = selectionConfig.UseAnalyticsModels;
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            var rockContext = ( RockContext ) serviceInstance.Context;

            var selectionConfig = SelectionConfig.Parse( selection ) ?? new SelectionConfig();

            ComparisonType comparisonType = selectionConfig.ComparisonType;
            decimal amount = selectionConfig.Amount ?? 0.00M;
            DateRange dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( selectionConfig.SlidingDateRangePickerDelimitedValues );

            var accountGuids = selectionConfig.AccountGuids;
            List<int> accountIdList;
            if ( accountGuids != null && accountGuids.Any() )
            {
                var financialAccountService = new FinancialAccountService( ( RockContext ) serviceInstance.Context );
                accountIdList = financialAccountService.GetByGuids( accountGuids ).Select( a => a.Id ).ToList();
                if ( selectionConfig.IncludeChildAccounts )
                {
                    var parentAccountIds = accountIdList.ToList();
                    foreach ( var parentAccountId in parentAccountIds )
                    {
                        var descendantChildAccountIds = financialAccountService.GetAllDescendentIds( parentAccountId );
                        accountIdList.AddRange( descendantChildAccountIds );
                    }
                }
            }
            else
            {
                accountIdList = new List<int>();
            }

            bool combineGiving = selectionConfig.CombineGiving;

            int transactionTypeContributionId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() ).Id;
            bool useAnalyticsModels = selectionConfig.UseAnalyticsModels;

            IQueryable<TransactionDetailData> financialTransactionDetailBaseQry;

            if ( useAnalyticsModels )
            {
                financialTransactionDetailBaseQry = new AnalyticsSourceFinancialTransactionService( rockContext ).Queryable()
                          .Where( xx => xx.AuthorizedPersonAliasId.HasValue )
                          .Where( xx => xx.TransactionTypeValueId == transactionTypeContributionId )
                          .Select( ss => new TransactionDetailData
                          {
                              AuthorizedPersonAliasId = ss.AuthorizedPersonAliasId.Value,
                              TransactionDateTime = ss.TransactionDateTime,
                              Amount = ss.Amount,
                              AccountId = ss.AccountId ?? 0
                          } );
            }
            else
            {
                financialTransactionDetailBaseQry = new FinancialTransactionDetailService( rockContext ).Queryable()
                          .Where( xx => xx.Transaction.AuthorizedPersonAliasId.HasValue )
                          .Where( xx => xx.Transaction.TransactionDateTime.HasValue )
                          .Where( xx => xx.Transaction.TransactionTypeValueId == transactionTypeContributionId )
                          .Select( ss => new TransactionDetailData
                          {
                              AuthorizedPersonAliasId = ss.Transaction.AuthorizedPersonAliasId.Value,
                              TransactionDateTime = ss.Transaction.TransactionDateTime.Value,
                              Amount = ss.Amount,
                              AccountId = ss.AccountId
                          } );
            }

            if ( dateRange.Start.HasValue )
            {
                financialTransactionDetailBaseQry = financialTransactionDetailBaseQry.Where( xx => xx.TransactionDateTime >= dateRange.Start.Value );
            }

            if ( dateRange.End.HasValue )
            {
                financialTransactionDetailBaseQry = financialTransactionDetailBaseQry.Where( xx => xx.TransactionDateTime < dateRange.End.Value );
            }

            if ( accountIdList.Any() )
            {
                if ( accountIdList.Count() == 1 )
                {
                    var accountId = accountIdList[0];
                    financialTransactionDetailBaseQry = financialTransactionDetailBaseQry.Where( x => accountId == x.AccountId );
                }
                else
                {
                    financialTransactionDetailBaseQry = financialTransactionDetailBaseQry.Where( x => accountIdList.Contains( x.AccountId ) );
                }
            }

            if ( selectionConfig.IgnoreInactiveAccounts )
            {
                var inactiveAccountIdQuery = new FinancialAccountService( rockContext ).Queryable().Where( a => !a.IsActive ).Select( a => a.Id );
                financialTransactionDetailBaseQry = financialTransactionDetailBaseQry.Where( a => !inactiveAccountIdQuery.Contains( a.AccountId ) );
            }

            bool excludePersonsWithTransactions = false;

            // Create explicit joins to person alias and person tables so that rendered SQL has an INNER Joins vs OUTER joins on Person and PersonAlias
            var personAliasQry = new PersonAliasService( rockContext ).Queryable();
            var personQryForJoin = new PersonService( rockContext ).Queryable( true );
            var financialTransactionDetailAmountQry = financialTransactionDetailBaseQry
                .Join(
                    personAliasQry,
                    t => t.AuthorizedPersonAliasId,
                    pa => pa.Id,
                    ( t, pa ) => new { TransactionDetailData = t, PersonId = pa.PersonId } )
                .Join(
                    personQryForJoin,
                    j1 => j1.PersonId,
                    p => p.Id,
                    ( j1, p ) => new { Amount = j1.TransactionDetailData.Amount, Person = p } );

            IQueryable<GiverAmountInfo> financialTransactionGivingAmountQry;

            if ( combineGiving )
            {
                var financialTransactionGroupByQuery = financialTransactionDetailAmountQry.GroupBy( xx => xx.Person.GivingId );

                financialTransactionGivingAmountQry = financialTransactionGroupByQuery
                    .Select( xx => new GiverAmountInfo { GivingId = xx.Key, TotalAmount = xx.Sum( ss => ss.Amount ) } );
            }
            else
            {
                var financialTransactionGroupByQuery = financialTransactionDetailAmountQry.GroupBy( xx => xx.Person.Id );

                financialTransactionGivingAmountQry = financialTransactionGroupByQuery
                    .Select( xx => new GiverAmountInfo { PersonId = xx.Key, TotalAmount = xx.Sum( ss => ss.Amount ) } );
            }

            if ( comparisonType == ComparisonType.LessThan )
            {
                // NOTE: Since we want people that have less than the specified, but also want to include people to didn't give anything at all (no transactions)
                // make this query the same as the GreaterThan, but use it to EXCLUDE people that gave MORE than the specified amount. That
                // way the filter will include people that had no transactions for the specified date/range and account 
                financialTransactionGivingAmountQry = financialTransactionGivingAmountQry.Where( xx => xx.TotalAmount >= amount );
                excludePersonsWithTransactions = true;
            }
            else if ( comparisonType == ComparisonType.EqualTo )
            {
                if ( amount == 0.00M )
                {
                    // NOTE: If we want to list people that gave $0.00 (they didn't giving anything)
                    // EXCLUDE people that gave any amount
                    excludePersonsWithTransactions = true;
                }
                else
                {
                    financialTransactionGivingAmountQry = financialTransactionGivingAmountQry.Where( xx => xx.TotalAmount == amount );
                }
            }
            else if ( comparisonType == ComparisonType.GreaterThanOrEqualTo )
            {
                // NOTE: if the amount filter is 'they gave $0.00 or more', and doing a GreaterThanOrEqualTo, then we don't need to calculate and compare against TotalAmount
                if ( amount == 0.00M )
                {
                    // no need to filter by amount if greater than or equal to $0.00
                }
                else
                {
                    financialTransactionGivingAmountQry = financialTransactionGivingAmountQry.Where( xx => xx.TotalAmount >= amount );
                }
            }

            IQueryable<Model.Person> qry;

            if ( combineGiving )
            {
                if ( excludePersonsWithTransactions )
                {
                    // the filter is for people that gave LESS than the specified amount, so return people that didn't give MORE than the specified amount
                    qry = new PersonService( rockContext ).Queryable().Where( p => !financialTransactionGivingAmountQry.Any( xx => xx.GivingId == p.GivingId ) );
                }
                else
                {
                    qry = new PersonService( rockContext ).Queryable().Where( p => financialTransactionGivingAmountQry.Any( xx => xx.GivingId == p.GivingId ) );
                }
            }
            else
            {
                if ( excludePersonsWithTransactions )
                {
                    // the filter is for people that gave LESS than the specified amount, so return people that didn't give MORE than the specified amount
                    qry = new PersonService( rockContext ).Queryable().Where( p => !financialTransactionGivingAmountQry.Any( xx => xx.PersonId == p.Id ) );
                }
                else
                {
                    qry = new PersonService( rockContext ).Queryable().Where( p => financialTransactionGivingAmountQry.Any( xx => xx.PersonId == p.Id ) );
                }
            }

            Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

            return extractedFilterExpression;
        }

        private class TransactionDetailData
        {
            public DateTime TransactionDateTime { get; set; }

            public decimal Amount { get; set; }

            public int AccountId { get; set; }

            public int AuthorizedPersonAliasId { get; internal set; }
        }

        private class GiverAmountInfo
        {
            public string GivingId { get; set; }

            public decimal TotalAmount { get; set; }

            public int PersonId { get; set; }
        }

        #endregion
    }
}