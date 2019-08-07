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
            string[] selectionValues = selection.Split( '|' );

            if ( selectionValues.Length >= 4 )
            {
                ComparisonType comparisonType = selectionValues[0].ConvertToEnum<ComparisonType>( ComparisonType.GreaterThanOrEqualTo );
                decimal amount = selectionValues[1].AsDecimalOrNull() ?? 0.00M;
                string accountNames = string.Empty;
                if ( selectionValues.Length >= 5 )
                {
                    var accountGuids = selectionValues[4].Split( ',' ).Select( a => a.AsGuid() ).ToList();
                    accountNames = new FinancialAccountService( new RockContext() ).GetByGuids( accountGuids ).Select( a => a.Name ).ToList().AsDelimited( "," );
                }

                bool combineGiving = false;
                if ( selectionValues.Length >= 6 )
                {
                    combineGiving = selectionValues[5].AsBooleanOrNull() ?? false;
                }

                SlidingDateRangePicker fakeSlidingDateRangePicker = new SlidingDateRangePicker();

                if ( selectionValues.Length >= 7 )
                {
                    // convert comma delimited to pipe
                    fakeSlidingDateRangePicker.DelimitedValues = selectionValues[6].Replace( ',', '|' );
                }
                else
                {
                    // if converting from a previous version of the selection
                    var lowerValue = selectionValues[2].AsDateTime();
                    var upperValue = selectionValues[3].AsDateTime();

                    fakeSlidingDateRangePicker.SlidingDateRangeMode = SlidingDateRangePicker.SlidingDateRangeType.DateRange;
                    fakeSlidingDateRangePicker.SetDateRangeModeValue( new DateRange( lowerValue, upperValue ) );
                }

                result = string.Format(
                    "{4}Giving amount total {0} {1} {2}. Date Range: {3}",
                    comparisonType.ConvertToString().ToLower(),
                    amount.ToString( "C" ),
                    !string.IsNullOrWhiteSpace( accountNames ) ? " to accounts:" + accountNames : string.Empty,
                    SlidingDateRangePicker.FormatDelimitedValues( fakeSlidingDateRangePicker.DelimitedValues ),
                    combineGiving ? "Combined " : string.Empty);
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

            NumberBox numberBoxAmount = new NumberBox();
            numberBoxAmount.PrependText = globalAttributes.GetValue( "CurrencySymbol" ) ?? "$";
            numberBoxAmount.NumberType = ValidationDataType.Currency;
            numberBoxAmount.ID = filterControl.ID + "_numberBoxAmount";
            numberBoxAmount.Label = "Amount";

            filterControl.Controls.Add( numberBoxAmount );

            AccountPicker accountPicker = new AccountPicker();
            accountPicker.AllowMultiSelect = true;
            accountPicker.ID = filterControl.ID + "_accountPicker";
            accountPicker.AddCssClass( "js-account-picker" );
            accountPicker.Label = "Accounts";
            filterControl.Controls.Add( accountPicker );

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

            var controls = new Control[5] { comparisonControl, numberBoxAmount, accountPicker, slidingDateRangePicker, cbCombineGiving };

            SetSelection( entityType, controls, string.Format( "{0}||||||", ComparisonType.GreaterThanOrEqualTo.ConvertToInt().ToString() ) );

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
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            string comparisonType = ( (DropDownList)controls[0] ).SelectedValue;
            decimal? amount = ( controls[1] as NumberBox ).Text.AsDecimal();

            var accountIdList = ( controls[2] as AccountPicker ).SelectedValuesAsInt().ToList();
            string accountGuids = string.Empty;
            var accounts = new FinancialAccountService( new RockContext() ).GetByIds( accountIdList );
            if ( accounts != null && accounts.Any() )
            {
                accountGuids = accounts.Select( a => a.Guid ).ToList().AsDelimited( "," );
            }

            SlidingDateRangePicker slidingDateRangePicker = controls[3] as SlidingDateRangePicker;

            // convert pipe to comma delimited
            var delimitedValues = slidingDateRangePicker.DelimitedValues.Replace( "|", "," );

            RockCheckBox cbCombineGiving = controls[4] as RockCheckBox;

            return string.Format( "{0}|{1}|{2}|{3}|{4}|{5}|{6}", comparisonType, amount, string.Empty, string.Empty, accountGuids, cbCombineGiving.Checked, delimitedValues );
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 4 )
            {
                var comparisonControl = controls[0] as DropDownList;
                var numberBox = controls[1] as NumberBox;
                var accountPicker = controls[2] as AccountPicker;
                var slidingDateRangePicker = controls[3] as SlidingDateRangePicker;

                if ( selectionValues.Length >= 7 )
                {
                    // convert comma delimited to pipe
                    slidingDateRangePicker.DelimitedValues = selectionValues[6].Replace( ',', '|' );
                }
                else
                {
                    // if converting from a previous version of the selection
                    var lowerValue = selectionValues[2].AsDateTime();
                    var upperValue = selectionValues[3].AsDateTime();

                    slidingDateRangePicker.SlidingDateRangeMode = SlidingDateRangePicker.SlidingDateRangeType.DateRange;
                    slidingDateRangePicker.SetDateRangeModeValue( new DateRange( lowerValue, upperValue ) );
                }

                var cbCombineGiving = controls[4] as RockCheckBox;

                comparisonControl.SetValue( selectionValues[0] );
                decimal? amount = selectionValues[1].AsDecimal();
                if ( amount.HasValue )
                {
                    numberBox.Text = amount.Value.ToString( "F2" );
                }
                else
                {
                    numberBox.Text = string.Empty;
                }

                if ( selectionValues.Length >= 5 )
                {
                    var accountGuids = selectionValues[4].Split( ',' ).Select( a => a.AsGuid() ).ToList();
                    var accounts = new FinancialAccountService( new RockContext() ).GetByGuids( accountGuids );
                    if ( accounts != null && accounts.Any() )
                    {
                        accountPicker.SetValues( accounts );
                    }
                }

                if ( selectionValues.Length >= 6 )
                {
                    cbCombineGiving.Checked = selectionValues[5].AsBooleanOrNull() ?? false;
                }
            }
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
            var rockContext = (RockContext)serviceInstance.Context;

            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length < 4 )
            {
                return null;
            }

            ComparisonType comparisonType = selectionValues[0].ConvertToEnum<ComparisonType>( ComparisonType.GreaterThanOrEqualTo );
            decimal amount = selectionValues[1].AsDecimalOrNull() ?? 0.00M;
            DateRange dateRange;

            if ( selectionValues.Length >= 7 )
            {
                string slidingDelimitedValues = selectionValues[6].Replace( ',', '|' );
                dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( slidingDelimitedValues );
            }
            else
            {
                // if converting from a previous version of the selection
                DateTime? startDate = selectionValues[2].AsDateTime();
                DateTime? endDate = selectionValues[3].AsDateTime();
                dateRange = new DateRange( startDate, endDate );

                if ( dateRange.End.HasValue )
                {
                    // the DateRange picker doesn't automatically add a full day to the end date
                    dateRange.End.Value.AddDays( 1 );
                }
            }

            var accountIdList = new List<int>();
            if ( selectionValues.Length >= 5 )
            {
                var accountGuids = selectionValues[4].Split( ',' ).Select( a => a.AsGuid() ).ToList();
                accountIdList = new FinancialAccountService( (RockContext)serviceInstance.Context ).GetByGuids( accountGuids ).Select( a => a.Id ).ToList();
            }

            bool combineGiving = false;
            if ( selectionValues.Length >= 6 )
            {
                combineGiving = selectionValues[5].AsBooleanOrNull() ?? false;
            }

            int transactionTypeContributionId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() ).Id;

            var financialTransactionQry = new FinancialTransactionService( rockContext ).Queryable()
                .Where( xx => xx.AuthorizedPersonAliasId.HasValue )
                .Where( xx => xx.TransactionTypeValueId == transactionTypeContributionId );

            if ( dateRange.Start.HasValue )
            {
                financialTransactionQry = financialTransactionQry.Where( xx => xx.TransactionDateTime >= dateRange.Start.Value );
            }

            if ( dateRange.End.HasValue )
            {
                financialTransactionQry = financialTransactionQry.Where( xx => xx.TransactionDateTime < dateRange.End.Value );
            }

            bool limitToAccounts = accountIdList.Any();

            // Create an explicit join to person alias so that rendered SQL is an INNER Join vs OUTER join
            var personAliasQry = new PersonAliasService( rockContext ).Queryable();
            var financialTransactionGivingGroupQry = financialTransactionQry
                .Join( personAliasQry, t => t.AuthorizedPersonAliasId, p => p.Id, ( t, p ) => new
                {
                    Txn = t,
                    GivingGroupId = p.Person.GivingGroupId
                } );

            // query transactions for individuals.  
            // If CombineGiving, exclude people that are Giving Group, and we'll get those when we union with CombineGiving
            var financialTransactionDetailsIndividualQry = financialTransactionGivingGroupQry.Where( a => !combineGiving || !a.GivingGroupId.HasValue).Select( a => a.Txn )
                .GroupBy( xx => xx.AuthorizedPersonAlias.PersonId
                ).Select( xx =>
                    new
                    {
                        PersonId = xx.Key,
                        AnyAmount = xx.Any(ss => ss.TransactionDetails.Where( td => !limitToAccounts || accountIdList.Contains( td.AccountId ) ).Any() ),
                        TotalAmount = xx.Sum( ss => ss.TransactionDetails.Where( td => !limitToAccounts || accountIdList.Contains( td.AccountId ) ).Sum( td => td.Amount ) )
                    } );

            bool excludePersonsWithTransactions = false;

            if ( comparisonType == ComparisonType.LessThan )
            {
                // NOTE: Since we want people that have less than the specified, but also want to include people to didn't give anything at all (no transactions)
                // make this query the same as the GreaterThan, but use it to EXCLUDE people that gave MORE than the specified amount. That
                // way the filter will include people that had no transactions for the specified date/range and account 
                financialTransactionDetailsIndividualQry = financialTransactionDetailsIndividualQry.Where( xx => xx.TotalAmount >= amount );
                excludePersonsWithTransactions = true;
            }
            else if ( comparisonType == ComparisonType.EqualTo )
            {
                financialTransactionDetailsIndividualQry = financialTransactionDetailsIndividualQry.Where( xx => xx.TotalAmount == amount );
            }
            else if ( comparisonType == ComparisonType.GreaterThanOrEqualTo )
            {
                // NOTE: if the amount filter is 'they gave $0.00 or more', and doing a GreaterThanOrEqualTo, then we don't need to calculate and compare against TotalAmount
                if ( amount == 0.00M )
                {
                    // just query if there is 'any' amount
                    financialTransactionDetailsIndividualQry = financialTransactionDetailsIndividualQry.Where( xx => xx.AnyAmount );
                }
                else
                {
                    financialTransactionDetailsIndividualQry = financialTransactionDetailsIndividualQry.Where( xx => xx.TotalAmount >= amount );
                }
            }

            var innerQryIndividual = financialTransactionDetailsIndividualQry.Select( xx => xx.PersonId ).AsQueryable();

            IQueryable<int> qryTransactionPersonIds;

            if ( combineGiving )
            {
                // if CombineGiving=true, do another query to total by GivingGroupId for people with GivingGroupId specified
                var financialTransactionDetailsGivingGroupQry = financialTransactionGivingGroupQry.Where( a => a.GivingGroupId.HasValue )
                .GroupBy( xx => new
                {
                    xx.GivingGroupId
                } ).Select( xx =>
                    new
                    {
                        GivingGroupId = xx.Key,
                        AnyAmount = xx.Any( ss => ss.Txn.TransactionDetails.Where( td => !limitToAccounts || accountIdList.Contains( td.AccountId ) ).Any() ),
                        TotalAmount = xx.Sum( ss => ss.Txn.TransactionDetails.Where( td => !limitToAccounts || accountIdList.Contains( td.AccountId ) ).Sum( td => td.Amount ) )
                    } );

                if ( comparisonType == ComparisonType.LessThan )
                {
                    // NOTE: Since we want people that have less than the specified amount, but also want to include people to didn't give anything at all (no transactions)
                    // make this query the same as the GreaterThan, but use it to EXCLUDE people that gave MORE than the specified amount. That
                    // way the filter will include people that had no transactions for the specified date/range and account
                    financialTransactionDetailsGivingGroupQry = financialTransactionDetailsGivingGroupQry.Where( xx => xx.TotalAmount >= amount );
                    excludePersonsWithTransactions = true;
                }
                else if ( comparisonType == ComparisonType.EqualTo )
                {
                    financialTransactionDetailsGivingGroupQry = financialTransactionDetailsGivingGroupQry.Where( xx => xx.TotalAmount == amount );

                }
                else if ( comparisonType == ComparisonType.GreaterThanOrEqualTo )
                {
                    // NOTE: if the amount filter is 'they gave $0.00 or more', and doing a GreaterThanOrEqualTo, then we don't need to calculate and compare against TotalAmount
                    if ( amount == 0.00M )
                    {
                        // don't query against TotalAmount if we don't care about amount or accounts
                        financialTransactionDetailsGivingGroupQry = financialTransactionDetailsGivingGroupQry.Where( xx => xx.AnyAmount );
                    }
                    else
                    {
                        financialTransactionDetailsGivingGroupQry = financialTransactionDetailsGivingGroupQry.Where( xx => xx.TotalAmount >= amount );
                    }
                }

                var personService = new PersonService( rockContext );
                IQueryable<int> innerQryGivingGroupPersons = personService.Queryable()
                    .Where( a => financialTransactionDetailsGivingGroupQry.Select( xx => xx.GivingGroupId ).AsQueryable().Any( gg => gg.GivingGroupId == a.GivingGroupId ) )
                    .Select( s => s.Id );

                // include people that either give as individuals or are members of a giving group
                qryTransactionPersonIds = innerQryIndividual.Union( innerQryGivingGroupPersons );
            }
            else
            {
                // don't factor in GivingGroupId.  Only include people that are directly associated with the transaction
                qryTransactionPersonIds = innerQryIndividual;
            }

            IQueryable<Rock.Model.Person> qry;

            if ( excludePersonsWithTransactions )
            {
                // the filter is for people that gave LESS than the specified amount, so return people that didn't give MORE than the specified amount
                qry = new PersonService( rockContext ).Queryable().Where( p => !qryTransactionPersonIds.Any( xx => xx == p.Id ) );
            }
            else
            {
                qry = new PersonService( rockContext ).Queryable().Where( p => qryTransactionPersonIds.Any( xx => xx == p.Id ) );
            }

            Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

            return extractedFilterExpression;
        }

        #endregion
    }
}