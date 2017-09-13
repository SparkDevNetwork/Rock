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

            var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Read();

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

            int transactionTypeContributionId = Rock.Web.Cache.DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() ).Id;

            IQueryable<decimal> personTotalAmountQry;

            if ( useAnalytics )
            {
                var financialTransactionQry = new AnalyticsSourceFinancialTransactionService( context ).Queryable()
                    .Where( xx => xx.TransactionTypeValueId == transactionTypeContributionId )
                    .Where( xx => xx.AuthorizedPersonAliasId.HasValue );
                if ( dateRange.Start.HasValue )
                {
                    financialTransactionQry = financialTransactionQry.Where( xx => xx.TransactionDateTime >= dateRange.Start.Value );
                }

                if ( dateRange.End.HasValue )
                {
                    financialTransactionQry = financialTransactionQry.Where( xx => xx.TransactionDateTime < dateRange.End.Value );
                }

                bool limitToAccounts = accountIdList.Any();
                if ( limitToAccounts )
                {
                    financialTransactionQry = financialTransactionQry.Where( xx => xx.AccountId.HasValue && accountIdList.Contains( xx.AccountId.Value ) );
                }

                if ( comparisonType == ComparisonType.LessThan )
                {
                    financialTransactionQry = financialTransactionQry.Where( xx => xx.Amount < amount );
                }
                else if ( comparisonType == ComparisonType.EqualTo )
                {
                    financialTransactionQry = financialTransactionQry.Where( xx => xx.Amount == amount );
                }
                else if ( comparisonType == ComparisonType.GreaterThanOrEqualTo )
                {
                    financialTransactionQry = financialTransactionQry.Where( xx => xx.Amount >= amount );
                }

                if ( combineGiving )
                {
                    var personAmount = new AnalyticsSourcePersonHistoricalService( context ).Queryable()
                        .Join( financialTransactionQry, p => p.GivingId, f => f.GivingId, ( p, f ) => new
                        {
                            p.PersonId,
                            f.Amount
                        } );

                    personTotalAmountQry = new PersonService( context ).Queryable()
                        .Select( p => personAmount
                            .Where( ww => ww.PersonId == p.Id )
                            .Sum( ww => ww.Amount ) );
                }
                else
                {
                    var personAmount = new AnalyticsSourcePersonHistoricalService( context ).Queryable()
                        .Join( financialTransactionQry, p => p.Id, f => f.AuthorizedPersonKey, ( p, f ) => new
                        {
                            p.PersonId,
                            f.Amount
                        } );

                    personTotalAmountQry = new PersonService( context ).Queryable()
                        .Select( p => personAmount
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

                if ( comparisonType == ComparisonType.LessThan )
                {
                    financialTransactionQry = financialTransactionQry.Where( xx => xx.Amount < amount );
                }
                else if ( comparisonType == ComparisonType.EqualTo )
                {
                    financialTransactionQry = financialTransactionQry.Where( xx => xx.Amount == amount );
                }
                else if ( comparisonType == ComparisonType.GreaterThanOrEqualTo )
                {
                    financialTransactionQry = financialTransactionQry.Where( xx => xx.Amount >= amount );
                }

                if ( combineGiving )
                {
                    //// if combineGiving..
                    // if they aren't in a giving group, sum up transactions amounts by the person
                    // if they are in a giving group, sum up transactions amounts by the persons that are in the person's giving group
                    personTotalAmountQry = new PersonService( context ).Queryable()
                                    .Select( p => financialTransactionQry
                                    .Where( ww =>
                                        ( !p.GivingGroupId.HasValue && ww.Transaction.AuthorizedPersonAlias.PersonId == p.Id )
                                        ||
                                        ( p.GivingGroupId.HasValue && ww.Transaction.AuthorizedPersonAlias.Person.GivingGroupId == p.GivingGroupId ) )
                                    .Sum( aa => aa.Amount ) );
                }
                else
                {
                    personTotalAmountQry = new PersonService( context ).Queryable()
                    .Select( p => financialTransactionQry
                    .Where( ww => ww.Transaction.AuthorizedPersonAlias.PersonId == p.Id )
                    .Sum( aa => aa.Amount ) );
                }
            }

            var selectExpression = SelectExpressionExtractor.Extract( personTotalAmountQry, entityIdProperty, "p" );

            return selectExpression;
        }
    }
}
