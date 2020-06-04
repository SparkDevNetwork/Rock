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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataSelect.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Select the total giving amount of the person" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Select Person's Total Giving Amount" )]
    public class TotalGivingAmountSelect : DataSelectComponent
    {
        /// <summary>
        /// Gets the name of the entity type. Filter should be an empty string
        /// if it applies to all entities
        /// </summary>
        /// <value>
        /// The name of the entity type.
        /// </value>
        public override string AppliesToEntityType
        {
            get
            {
                return typeof( Rock.Model.Person ).FullName;
            }
        }

        /// <summary>
        /// The PropertyName of the property in the anonymous class returned by the SelectExpression
        /// </summary>
        /// <value>
        /// The name of the column property.
        /// </value>
        public override string ColumnPropertyName
        {
            get
            {
                return "Total Giving";
            }
        }

        /// <summary>
        /// Gets the type of the column field.
        /// </summary>
        /// <value>
        /// The type of the column field.
        /// </value>
        public override Type ColumnFieldType
        {
            get
            {
                return typeof( decimal? );
            }
        }

        /// <summary>
        /// Gets the default column header text.
        /// </summary>
        /// <value>
        /// The default column header text.
        /// </value>
        public override string ColumnHeaderText
        {
            get
            {
                return "Total Giving";
            }
        }

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
            return "Total Giving";
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( System.Web.UI.Control parentControl )
        {
            var comparisonControl = ComparisonHelper.ComparisonControl( ComparisonType.LessThan | ComparisonType.GreaterThanOrEqualTo | ComparisonType.EqualTo );
            comparisonControl.ID = parentControl.ID + "_0";
            parentControl.Controls.Add( comparisonControl );

            var globalAttributes = GlobalAttributesCache.Get();

            NumberBox numberBoxAmount = new NumberBox();
            numberBoxAmount.PrependText = globalAttributes.GetValue( "CurrencySymbol" ) ?? "$";
            numberBoxAmount.NumberType = ValidationDataType.Currency;
            numberBoxAmount.ID = parentControl.ID + "_1";
            numberBoxAmount.Label = "Amount";

            parentControl.Controls.Add( numberBoxAmount );

            AccountPicker accountPicker = new AccountPicker();
            accountPicker.AllowMultiSelect = true;
            accountPicker.ID = parentControl.ID + "_accountPicker";
            accountPicker.AddCssClass( "js-account-picker" );
            accountPicker.Label = "Accounts";
            parentControl.Controls.Add( accountPicker );

            SlidingDateRangePicker slidingDateRangePicker = new SlidingDateRangePicker();
            slidingDateRangePicker.ID = parentControl.ID + "_slidingDateRangePicker";
            slidingDateRangePicker.AddCssClass( "js-sliding-date-range" );
            slidingDateRangePicker.Label = "Date Range";
            slidingDateRangePicker.Help = "The date range of the transactions using the transaction date of each transaction";
            slidingDateRangePicker.Required = true;
            parentControl.Controls.Add( slidingDateRangePicker );

            RockCheckBox cbCombineGiving = new RockCheckBox();
            cbCombineGiving.ID = parentControl.ID + "_cbCombineGiving";
            cbCombineGiving.Label = "Combine Giving";
            cbCombineGiving.CssClass = "js-combine-giving";
            cbCombineGiving.Help = "Combine individuals in the same giving group when calculating totals and reporting the list of individuals.";
            parentControl.Controls.Add( cbCombineGiving );

            RockCheckBox cbUseAnalytics = new RockCheckBox();
            cbUseAnalytics.ID = parentControl.ID + "_cbUseAnalytics";
            cbUseAnalytics.Label = "Use Analytics Models";
            cbUseAnalytics.CssClass = "js-use-analytics";
            cbUseAnalytics.Help = "Using Analytics Data is MUCH faster than querying real-time data, but it may not include data that has been added or updated in the last 24 hours.";
            parentControl.Controls.Add( cbUseAnalytics );

            var controls = new Control[6] { comparisonControl, numberBoxAmount, accountPicker, slidingDateRangePicker, cbCombineGiving, cbUseAnalytics };

            SetSelection( controls, $"{ComparisonType.GreaterThanOrEqualTo.ConvertToInt().ToString()}|||||||" );

            return controls;
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( System.Web.UI.Control parentControl, System.Web.UI.HtmlTextWriter writer, System.Web.UI.Control[] controls )
        {
            base.RenderControls( parentControl, writer, controls );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Control[] controls )
        {
            string comparisonType = ( ( DropDownList ) controls[0] ).SelectedValue;
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
            RockCheckBox cbUseAnalytics = controls[5] as RockCheckBox;

            // {2} and {3} used to store the DateRange before, but now we using the SlidingDateRangePicker
            return $"{comparisonType}|{amount}|||{accountGuids}|{cbCombineGiving.Checked}|{delimitedValues}|{cbUseAnalytics.Checked}";
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Control[] controls, string selection )
        {
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 4 )
            {
                var comparisonControl = controls[0] as DropDownList;
                var numberBox = controls[1] as NumberBox;
                var accountPicker = controls[2] as AccountPicker;
                SlidingDateRangePicker slidingDateRangePicker = controls[3] as SlidingDateRangePicker;
                var cbCombineGiving = controls[4] as RockCheckBox;
                var cbUseAnalytics = controls[5] as RockCheckBox;

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

                if ( selectionValues.Length >= 8 )
                {
                    cbUseAnalytics.Checked = selectionValues[7].AsBooleanOrNull() ?? false;
                }
            }
        }

        /// <summary>
        /// Gets the grid field.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override DataControlField GetGridField( Type entityType, string selection )
        {
            ComparisonType comparisonType;
            decimal totalAmountCutoff;

            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length < 2 )
            {
                // shouldn't happen, but just in case, just do the default
                comparisonType = ComparisonType.GreaterThanOrEqualTo;
                totalAmountCutoff = 0.00M;
            }
            else
            {

                comparisonType = selectionValues[0].ConvertToEnum<ComparisonType>( ComparisonType.GreaterThanOrEqualTo );
                totalAmountCutoff = selectionValues[1].AsDecimalOrNull() ?? 0.00M;
            }

            // if it is just greater than or equal to 0, they want to show total giving, regardless of amount
            // so there is no need to do the comparison logic
            bool skipComparison = ( comparisonType == ComparisonType.GreaterThanOrEqualTo && totalAmountCutoff == 0.00M );

            var callbackField = new CallbackField();
            callbackField.OnFormatDataValue += ( sender, e ) =>
            {
                decimal? totalGiving = e.DataValue as decimal?;
                if ( !totalGiving.HasValue || totalGiving.Value == 0.00M )
                {
                    e.FormattedValue = string.Empty;
                }
                else if ( skipComparison || ComparisonHelper.CompareNumericValues( comparisonType, totalGiving, totalAmountCutoff ) )
                {
                    // it meets the comparison criteria, so display total amount
                    e.FormattedValue = totalGiving?.ToString();
                }
                else
                {
                    // if the total giving is an amount that doesn't meet the comparison criteria, show blank
                    e.FormattedValue = string.Empty;
                }
            };

            return callbackField;
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entityIdProperty">The entity identifier property.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override System.Linq.Expressions.Expression GetExpression( Data.RockContext context, System.Linq.Expressions.MemberExpression entityIdProperty, string selection )
        {
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length < 4 )
            {
                return null;
            }

            /* 2020-05-19 MDP
             * The TotalAmount Comparison logic is that the displayed TotalAmount will show blank if the criteria doesn't match
             * For example:
             *   Total Amount >= $100.00
             *   If a person's total giving is $100.01, $100.01 will be displayed as the total giving in the report
             *   If the person's total giving is $99.99, the total giving in the report will just show blank
             *
             *
             *  This display logic is done in the GetGridField method
             */

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
                var accountGuids = selectionValues[4].Split( ',' ).Select( a => a.AsGuid() ).Where( a => a != Guid.Empty ).ToList();
                accountIdList = new FinancialAccountService( context ).GetByGuids( accountGuids ).Select( a => a.Id ).ToList();
            }

            bool combineGiving = false;
            if ( selectionValues.Length >= 6 )
            {
                combineGiving = selectionValues[5].AsBooleanOrNull() ?? false;
            }

            bool useAnalytics = false;
            if ( selectionValues.Length >= 8 )
            {
                useAnalytics = selectionValues[7].AsBooleanOrNull() ?? false;
            }

            int transactionTypeContributionId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() ).Id;

            IQueryable<decimal> personTotalAmountQry;

            if ( useAnalytics )
            {
                /* 2020-05-20 MDP
                    Analytics tables don't have a reference between a transaction and it's refund (unless we join the analytics tables to the TransactionRefund table).
                    That isn't a problem unless the refund for a transaction is later than the specified date range.
                    We discussed this and decided to not worry abou the late refund problem right now.

                    Also, the total giving will be correct even when factoring in refunds
                    because the Analytics tables will have a negative amount for refund transactions

                 */

                var analyticsFinancialTransactionQry = new AnalyticsSourceFinancialTransactionService( context ).Queryable()
                    .Where( xx => xx.TransactionTypeValueId == transactionTypeContributionId )
                    .Where( xx => xx.AuthorizedPersonAliasId.HasValue );

                if ( dateRange.Start.HasValue )
                {
                    analyticsFinancialTransactionQry = analyticsFinancialTransactionQry.Where( xx => xx.TransactionDateTime >= dateRange.Start.Value );
                }

                if ( dateRange.End.HasValue )
                {
                    analyticsFinancialTransactionQry = analyticsFinancialTransactionQry.Where( xx => xx.TransactionDateTime < dateRange.End.Value );
                }

                bool limitToAccounts = accountIdList.Any();
                if ( limitToAccounts )
                {
                    analyticsFinancialTransactionQry = analyticsFinancialTransactionQry.Where( xx => xx.AccountId.HasValue && accountIdList.Contains( xx.AccountId.Value ) );
                }

                if ( combineGiving )
                {
                    var analyticsPersonAmountQry = new AnalyticsDimPersonCurrentService( context ).Queryable()
                        .Join( analyticsFinancialTransactionQry, p => p.GivingId, f => f.GivingId, ( p, f ) => new
                        {
                            p.PersonId,
                            f.Amount
                        } );

                    personTotalAmountQry = new PersonService( context ).Queryable()
                        .Select( p => analyticsPersonAmountQry
                            .Where( ww => ww.PersonId == p.Id )
                            .Sum( ww => ww.Amount ) );
                }
                else
                {
                    var analyticsPersonAmountQry = new AnalyticsDimPersonCurrentService( context ).Queryable()
                        .Join( analyticsFinancialTransactionQry, p => p.Id, f => f.AuthorizedPersonKey, ( p, f ) => new
                        {
                            p.PersonId,
                            f.Amount
                        } );

                    personTotalAmountQry = new PersonService( context ).Queryable()
                        .Select( p => analyticsPersonAmountQry
                            .Where( ww => ww.PersonId == p.Id )
                            .Sum( ww => ww.Amount ) );
                }
            }
            else
            {
                var financialTransactionQry = new FinancialTransactionDetailService( context ).Queryable()
                    .Where( xx => xx.Transaction.TransactionTypeValueId == transactionTypeContributionId )
                    .Where( xx => xx.Transaction.AuthorizedPersonAliasId.HasValue );

                if ( dateRange.Start.HasValue )
                {
                    financialTransactionQry = financialTransactionQry.Where( xx => xx.Transaction.TransactionDateTime >= dateRange.Start.Value );
                }

                if ( dateRange.End.HasValue )
                {
                    financialTransactionQry = financialTransactionQry.Where( xx => xx.Transaction.TransactionDateTime < dateRange.End.Value );
                }

                bool limitToAccounts = accountIdList.Any();
                if ( limitToAccounts )
                {
                    financialTransactionQry = financialTransactionQry.Where( xx => accountIdList.Contains( xx.AccountId ) );
                }

                // exclude the financial transactions that were used for refunds.
                // This is because we'll get the refund transactions of each non-refund transaction when getting the total amount

                var refundsQry = new FinancialTransactionRefundService( context ).Queryable();

                financialTransactionQry = financialTransactionQry.Where( xx => !refundsQry.Any( r => r.Id == xx.TransactionId ) );

                /* 2020-05-02 MDP
                 * To factor in Refunds, subtract (but actually add since the amount will be negative)
                 * the refund amount if there is a refund associated with that transaction.
                 *
                 * Also, don't apply a date filter on the refund since we want to factor in refunds
                 * that might have occurred after the date range
                 *
                 * The Linq is written in a way to avoid the RefundAmount getting queried twice (once if it is null and another if it not null)
                */

                if ( combineGiving )
                {
                    personTotalAmountQry = new PersonService( context ).Queryable()
                                    .Select( p =>
                                        financialTransactionQry.Where( ww => p.GivingId == ww.Transaction.AuthorizedPersonAlias.Person.GivingId
                                    )
                                    .Select( x => new
                                    {
                                        x.Amount,
                                        RefundAmount = ( x.Transaction.RefundDetails.FinancialTransaction
                                                .TransactionDetails
                                                .Where( r => r.AccountId == x.AccountId )
                                                    .Sum( r => ( decimal? ) r.Amount )
                                                )
                                    } )
                                    .Sum
                                    (
                                        aa => aa.Amount + ( aa.RefundAmount ?? 0.00M )
                                    )
                            );
                }
                else
                {
                    personTotalAmountQry = new PersonService( context ).Queryable()
                                    .Select( p =>
                                        financialTransactionQry.Where( ww => ww.Transaction.AuthorizedPersonAlias.PersonId == p.Id
                                     )
                                    .Select( x => new
                                    {
                                        x.Amount,
                                        RefundAmount = ( x.Transaction.RefundDetails.FinancialTransaction
                                                    .TransactionDetails
                                                    .Where( r => r.AccountId == x.AccountId )
                                                        .Sum( r => ( decimal? ) r.Amount )
                                                    )
                                    } )
                                    .Sum
                                    (
                                        aa => aa.Amount + ( aa.RefundAmount ?? 0.00M )
                                    )
                            );
                }
            }

            var selectExpression = SelectExpressionExtractor.Extract( personTotalAmountQry, entityIdProperty, "p" );

            return selectExpression;
        }
    }
}
