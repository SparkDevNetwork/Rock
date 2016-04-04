﻿// <copyright>
// Copyright by the Spark Development Network
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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter people based on the date of their first contribution " )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "First Contribution Date Filter" )]
    public class FirstContributionDateFilter : DataFilterComponent
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
            return "First Contribution Date";
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
    
    var dateRangeText = $('.js-slidingdaterange-text-value', $content).val()
    var accountPicker = $('.js-account-picker', $content);
    var accountNames = accountPicker.find('.selected-names').text() 

   return 'First contribution date to accounts ' + accountNames  + '. DateRange: ' + dateRangeText;
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
            string result = "First Contribution Date";
            string[] selectionValues = selection.Split( '|' );

            if ( selectionValues.Length >= 3 )
            {
                SlidingDateRangePicker fakeSlidingDateRangePicker = new SlidingDateRangePicker();

                if ( selectionValues.Length >= 4 )
                {
                    // convert comma delimited to pipe
                    fakeSlidingDateRangePicker.DelimitedValues = selectionValues[3].Replace( ',', '|' );
                }
                else
                {
                    // if converting from a previous version of the selection
                    var lowerValue = selectionValues[0].AsDateTime();
                    var upperValue = selectionValues[1].AsDateTime();

                    fakeSlidingDateRangePicker.SlidingDateRangeMode = SlidingDateRangePicker.SlidingDateRangeType.DateRange;
                    fakeSlidingDateRangePicker.SetDateRangeModeValue( new DateRange( lowerValue, upperValue ) );
                }

                string accountNames = string.Empty;
                var accountGuids = selectionValues[2].Split( ',' ).Select( a => a.AsGuid() ).ToList();
                accountNames = new FinancialAccountService( new RockContext() ).GetByGuids( accountGuids ).Select( a => a.Name ).ToList().AsDelimited( "," );

                result = string.Format(
                    "First contribution date{0}. Date Range: {1}",
                    !string.IsNullOrWhiteSpace( accountNames ) ? " to accounts:" + accountNames : string.Empty,
                    SlidingDateRangePicker.FormatDelimitedValues( fakeSlidingDateRangePicker.DelimitedValues )
                    )
                    ;
            }

            return result;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
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
            slidingDateRangePicker.Help = "The date range of the transactions using the 'Sunday Date' of each transaction";
            slidingDateRangePicker.Required = true;
            filterControl.Controls.Add( slidingDateRangePicker );

            var controls = new Control[2] { accountPicker, slidingDateRangePicker };

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
            var accountIdList = ( controls[0] as AccountPicker ).SelectedValuesAsInt().ToList();
            string accountGuids = string.Empty;
            var accounts = new FinancialAccountService( new RockContext() ).GetByIds( accountIdList );
            if ( accounts != null && accounts.Any() )
            {
                accountGuids = accounts.Select( a => a.Guid ).ToList().AsDelimited( "," );
            }

            SlidingDateRangePicker slidingDateRangePicker = controls[1] as SlidingDateRangePicker;

            // convert pipe to comma delimited
            var delimitedValues = slidingDateRangePicker.DelimitedValues.Replace( "|", "," );

            // {1} and {2} used to store the DateRange before, but now we using the SlidingDateRangePicker
            return string.Format( "{0}|{1}|{2}|{3}", string.Empty, string.Empty, accountGuids, delimitedValues );
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
            if ( selectionValues.Length >= 3 )
            {
                var accountPicker = controls[0] as AccountPicker;
                var slidingDateRangePicker = controls[1] as SlidingDateRangePicker;

                if ( selectionValues.Length >= 4 )
                {
                    // convert comma delimited to pipe
                    slidingDateRangePicker.DelimitedValues = selectionValues[3].Replace( ',', '|' );
                }
                else
                {
                    // if converting from a previous version of the selection
                    var lowerValue = selectionValues[0].AsDateTime();
                    var upperValue = selectionValues[1].AsDateTime();

                    slidingDateRangePicker.SlidingDateRangeMode = SlidingDateRangePicker.SlidingDateRangeType.DateRange;
                    slidingDateRangePicker.SetDateRangeModeValue( new DateRange( lowerValue, upperValue ) );
                }

                var accountGuids = selectionValues[2].Split( ',' ).Select( a => a.AsGuid() ).ToList();
                var accounts = new FinancialAccountService( new RockContext() ).GetByGuids( accountGuids );
                if ( accounts != null && accounts.Any() )
                {
                    accountPicker.SetValues( accounts );
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
            if ( selectionValues.Length < 3 )
            {
                return null;
            }

            DateRange dateRange;

            if ( selectionValues.Length >= 4 )
            {
                string slidingDelimitedValues = selectionValues[3].Replace( ',', '|' );
                dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( slidingDelimitedValues );
            }
            else
            {
                // if converting from a previous version of the selection
                DateTime? startDate = selectionValues[0].AsDateTime();
                DateTime? endDate = selectionValues[1].AsDateTime();
                dateRange = new DateRange( startDate, endDate );

                if ( dateRange.End.HasValue )
                {
                    // the DateRange picker doesn't automatically add a full day to the end date
                    dateRange.End.Value.AddDays( 1 );
                }
            }

            var accountGuids = selectionValues[2].Split( ',' ).Select( a => a.AsGuid() ).ToList();
            var accountIdList = new FinancialAccountService( (RockContext)serviceInstance.Context ).GetByGuids( accountGuids ).Select( a => a.Id ).ToList();

            int transactionTypeContributionId = Rock.Web.Cache.DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() ).Id;

            var financialTransactionsQry = new FinancialTransactionService( rockContext ).Queryable()
                .Where( xx => xx.TransactionTypeValueId == transactionTypeContributionId );

            if ( accountIdList.Any() )
            {
                if ( accountIdList.Count == 1 )
                {
                    int accountId = accountIdList.First();
                    financialTransactionsQry = financialTransactionsQry.Where( xx => xx.TransactionDetails.Any( a => a.AccountId == accountId ) );
                }
                else
                {
                    financialTransactionsQry = financialTransactionsQry.Where( xx => xx.TransactionDetails.Any( a => accountIdList.Contains( a.AccountId ) ) );
                }
            }

            var firstContributionDateQry = financialTransactionsQry
                .GroupBy( xx => xx.AuthorizedPersonAlias.PersonId )
                .Select( ss => new
                {
                    PersonId = ss.Key,
                    FirstTransactionSundayDate = ss.Min( a => a.SundayDate )
                } );

            if ( dateRange.Start.HasValue )
            {
                firstContributionDateQry = firstContributionDateQry.Where( xx => xx.FirstTransactionSundayDate >= dateRange.Start.Value );
            }

            if ( dateRange.End.HasValue )
            {
                firstContributionDateQry = firstContributionDateQry.Where( xx => xx.FirstTransactionSundayDate < dateRange.End.Value );
            }

            var innerQry = firstContributionDateQry.Select( xx => xx.PersonId ).AsQueryable();

            var qry = new PersonService( rockContext ).Queryable()
                .Where( p => innerQry.Any( xx => xx == p.Id ) );

            Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

            return extractedFilterExpression;
        }

        #endregion
    }
}