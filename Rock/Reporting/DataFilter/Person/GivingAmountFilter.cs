// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    var startDate = $('.date-range-picker', $content).find('input:first').val();
    var endDate = $('.date-range-picker', $content).find('input:last').val();
    
    var accountPicker = $('.js-account-picker', $content);
    var accountNames = accountPicker.find('.selected-names').text()

    return 'Giving amount total ' + comparisonText.toLowerCase() + ' $' + totalAmount + ' to accounts:' + accountNames  + ' between ' + startDate + ' and ' + endDate;
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
                DateTime startDate = selectionValues[2].AsDateTime() ?? DateTime.MinValue;
                DateTime endDate = selectionValues[3].AsDateTime() ?? DateTime.MaxValue;
                string accountNames = string.Empty;
                if ( selectionValues.Length >= 5 )
                {
                    var accountGuids = selectionValues[4].Split( ',' ).Select( a => a.AsGuid() ).ToList();
                    accountNames = new FinancialAccountService( new RockContext() ).GetByGuids( accountGuids ).Select( a => a.Name ).ToList().AsDelimited( "," );
                }

                result = string.Format(
                    "Giving amount total {0} {1} {2} between {3} and {4}",
                    comparisonType.ConvertToString().ToLower(),
                    amount.ToString( "C" ),
                    !string.IsNullOrWhiteSpace( accountNames ) ? " to accounts:" + accountNames : string.Empty,
                    startDate.ToShortDateString(),
                    endDate.ToShortDateString() );
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
            comparisonControl.ID = filterControl.ID + "_0";
            filterControl.Controls.Add( comparisonControl );

            var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Read();

            NumberBox numberBoxAmount = new NumberBox();
            numberBoxAmount.PrependText = globalAttributes.GetValue( "CurrencySymbol" ) ?? "$";
            numberBoxAmount.NumberType = ValidationDataType.Currency;
            numberBoxAmount.ID = filterControl.ID + "_1";
            numberBoxAmount.Label = "Amount";

            filterControl.Controls.Add( numberBoxAmount );

            AccountPicker accountPicker = new AccountPicker();
            accountPicker.AllowMultiSelect = true;
            accountPicker.ID = filterControl.ID + "_accountPicker";
            accountPicker.AddCssClass( "js-account-picker" );
            accountPicker.Label = "Accounts";
            filterControl.Controls.Add( accountPicker );

            DateRangePicker dateRangePicker = new DateRangePicker();
            dateRangePicker.ID = filterControl.ID + "_2";
            dateRangePicker.Label = "Date Range";
            dateRangePicker.Required = true;
            filterControl.Controls.Add( dateRangePicker );

            var controls = new Control[4] { comparisonControl, numberBoxAmount, accountPicker, dateRangePicker };

            SetSelection( entityType, controls, string.Format( "{0}||||", ComparisonType.GreaterThanOrEqualTo.ConvertToInt().ToString() ) );

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

            DateRangePicker dateRangePicker = controls[3] as DateRangePicker;
            return string.Format( "{0}|{1}|{2}|{3}|{4}", comparisonType, amount, dateRangePicker.LowerValue, dateRangePicker.UpperValue, accountGuids );
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
                var dateRangePicker = controls[3] as DateRangePicker;

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

                dateRangePicker.LowerValue = selectionValues[2].AsDateTime();
                dateRangePicker.UpperValue = selectionValues[3].AsDateTime();

                if ( selectionValues.Length >= 5 )
                {
                    var accountGuids = selectionValues[4].Split( ',' ).Select( a => a.AsGuid() ).ToList();
                    var accounts = new FinancialAccountService( new RockContext() ).GetByGuids( accountGuids );
                    if ( accounts != null && accounts.Any() )
                    {
                        accountPicker.SetValues( accounts );
                    }
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
            DateTime? startDate = selectionValues[2].AsDateTime();
            DateTime? endDate = selectionValues[3].AsDateTime(); ;
            var accountIdList = new List<int>();
            if ( selectionValues.Length >= 5 )
            {
                var accountGuids = selectionValues[4].Split( ',' ).Select( a => a.AsGuid() ).ToList();
                accountIdList = new FinancialAccountService( (RockContext)serviceInstance.Context ).GetByGuids( accountGuids ).Select( a => a.Id ).ToList();
            }

            int transactionTypeContributionId = Rock.Web.Cache.DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid()).Id;

            var financialTransactionQry = new FinancialTransactionService( rockContext ).Queryable()
                .Where( xx => xx.AuthorizedPersonAliasId.HasValue)
                .Where( xx => xx.TransactionTypeValueId == transactionTypeContributionId );

            if (startDate.HasValue)
            {
                financialTransactionQry = financialTransactionQry.Where(xx => xx.TransactionDateTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                financialTransactionQry = financialTransactionQry.Where(xx => xx.TransactionDateTime < endDate.Value);
            }

            bool limitToAccounts = accountIdList.Any();

            var financialTransactionDetailsQry = financialTransactionQry
                .GroupBy( xx => xx.AuthorizedPersonAlias.PersonId ).Select( xx =>
                    new
                    {
                        PersonId = xx.Key,
                        TotalAmount = xx.Sum( ss => ss.TransactionDetails.Where( td => !limitToAccounts || accountIdList.Contains( td.AccountId ) ).Sum( td => td.Amount ) )
                    } );

            if ( comparisonType == ComparisonType.LessThan )
            {
                financialTransactionDetailsQry = financialTransactionDetailsQry.Where( xx => xx.TotalAmount < amount );
            }
            else if ( comparisonType == ComparisonType.EqualTo )
            {
                financialTransactionDetailsQry = financialTransactionDetailsQry.Where( xx => xx.TotalAmount == amount );
            }
            else if ( comparisonType == ComparisonType.GreaterThanOrEqualTo )
            {
                financialTransactionDetailsQry = financialTransactionDetailsQry.Where( xx => xx.TotalAmount >= amount );
            }

            var innerQry = financialTransactionDetailsQry.Select( xx => xx.PersonId ).AsQueryable();

            var qry = new PersonService( rockContext ).Queryable()
                .Where( p => innerQry.Any( xx => xx == p.Id ) );

            Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

            return extractedFilterExpression;
        }

        #endregion
    }
}