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

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

// This is to get the enums without the prefix
using static Rock.Web.UI.Controls.SlidingDateRangePicker;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    ///
    /// </summary>
    [Description( "Filter people based on the date of their first contribution " )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "First Contribution Date Filter" )]
    [Rock.SystemGuid.EntityTypeGuid( "B4B70487-E620-4BC1-8983-124578118BC0")]
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

        #endregion Properties

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
    var useSundayDate = $('.js-use-sunday-date', $content).is(':checked');
    var sundayString = useSundayDate == true ? 'Sunday ' : '';

    var accountPicker = $('.account-picker', $content);
    var accountNames = accountPicker.find('.selected-names').text();
    if(accountNames) {
        accountNames = ' to accounts: ' + accountNames;
    }

    var dateRangeText = $('.js-slidingdaterange-text-value', $content).val();

    return 'First contribution ' + sundayString + 'date' + accountNames  + '. DateRange: ' + dateRangeText;
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
            SelectionConfig selectionConfig = SelectionConfig.Parse( selection );

            string accountNames = string.Empty;
            if ( selectionConfig.AccountGuids != null && selectionConfig.AccountGuids.Any() )
            {
                accountNames = FinancialAccountCache.GetByGuids( selectionConfig.AccountGuids ).Select( a => a.Name ).ToList().AsDelimited( "," );
            }

            string sundayDateString = selectionConfig.UseSundayDate == true ? "Sunday " : string.Empty;
            string accountsString = accountNames.IsNullOrWhiteSpace() ? string.Empty : " to accounts: " + accountNames;
            string dateRangeString = $" Date Range: {SlidingDateRangePicker.FormatDelimitedValues( selectionConfig.DelimitedValues )}";

            return $"First contribution {sundayDateString}date{accountsString}.{dateRangeString}";
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
            accountPicker.Label = "Accounts";
            filterControl.Controls.Add( accountPicker );

            SlidingDateRangePicker slidingDateRangePicker = new SlidingDateRangePicker();
            slidingDateRangePicker.ID = filterControl.ID + "_slidingDateRangePicker";
            slidingDateRangePicker.AddCssClass( "js-sliding-date-range" );
            slidingDateRangePicker.Label = "Date Range";
            slidingDateRangePicker.Help = "The date range of the transactions using the 'Sunday Date' of each transaction";
            slidingDateRangePicker.Required = true;
            filterControl.Controls.Add( slidingDateRangePicker );

            RockCheckBox cbUseSundayDate = new RockCheckBox();
            cbUseSundayDate.ID = filterControl.ID + "_cbUseSundayDate";
            cbUseSundayDate.Label = "Use Sunday Date";
            cbUseSundayDate.Help = "Use the Sunday Date instead of the actual transaction date.";
            cbUseSundayDate.AddCssClass( "js-use-sunday-date" );
            filterControl.Controls.Add( cbUseSundayDate );

            var controls = new Control[3] { accountPicker, slidingDateRangePicker, cbUseSundayDate };

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
            SelectionConfig selectionConfig = new SelectionConfig();

            var accountIdList = ( controls[0] as AccountPicker ).SelectedValuesAsInt().ToList();
            string accountGuids = string.Empty;
            var accounts = FinancialAccountCache.GetByIds( accountIdList );
            if ( accounts != null && accounts.Any() )
            {
                selectionConfig.AccountGuids = accounts.Select( a => a.Guid ).ToList();
            }

            SlidingDateRangePicker slidingDateRangePicker = controls[1] as SlidingDateRangePicker;
            selectionConfig.StartDate = slidingDateRangePicker.DateRangeModeStart;
            selectionConfig.EndDate = slidingDateRangePicker.DateRangeModeEnd;
            selectionConfig.DateRangeMode = slidingDateRangePicker.SlidingDateRangeMode;
            selectionConfig.TimeUnit = slidingDateRangePicker.TimeUnit;
            selectionConfig.NumberOfTimeUnits = slidingDateRangePicker.NumberOfTimeUnits;

            selectionConfig.UseSundayDate = ( controls[2] as RockCheckBox ).Checked;

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
            SelectionConfig selectionConfig = SelectionConfig.Parse( selection );

            var accountPicker = controls[0] as AccountPicker;
            var accounts = FinancialAccountCache.GetByGuids( selectionConfig.AccountGuids );
            if ( accounts != null && accounts.Any() )
            {
                accountPicker.SetValuesFromCache( accounts );
            }

            var slidingDateRangePicker = controls[1] as SlidingDateRangePicker;
            slidingDateRangePicker.DelimitedValues = selectionConfig.DelimitedValues;

            var cbUseSundayDate = controls[2] as RockCheckBox;
            cbUseSundayDate.Checked = selectionConfig.UseSundayDate;
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

            SelectionConfig selectionConfig = SelectionConfig.Parse( selection );
            if ( selectionConfig == null )
            {
                return null;
            }

            DateRange dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( selectionConfig.DelimitedValues );

            int transactionTypeContributionId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() ).Id;
            var financialTransactionBaseQry = new FinancialTransactionService( rockContext )
                .Queryable()
                .Where( xx => xx.TransactionTypeValueId == transactionTypeContributionId );

            var accountIdList = FinancialAccountCache.GetByGuids( selectionConfig.AccountGuids ).Select( a => a.Id ).ToList();
            if ( accountIdList.Any() )
            {
                if ( accountIdList.Count == 1 )
                {
                    int accountId = accountIdList.First();
                    financialTransactionBaseQry = financialTransactionBaseQry.Where( xx => xx.TransactionDetails.Any( a => a.AccountId == accountId ) );
                }
                else
                {
                    financialTransactionBaseQry = financialTransactionBaseQry.Where( xx => xx.TransactionDetails.Any( a => accountIdList.Contains( a.AccountId ) ) );
                }
            }

            var personAliasQry = new PersonAliasService( rockContext ).Queryable();
            var personQryForJoin = new PersonService( rockContext ).Queryable();

            // Create explicit joins to person alias and person tables so that rendered SQL has an INNER Joins vs OUTER joins on PersonAlias
            var financialTransactionsQry = financialTransactionBaseQry
                .Join( personAliasQry, t => t.AuthorizedPersonAliasId, pa => pa.Id, ( t, pa ) => new
                {
                    txn = t,
                    PersonId = pa.PersonId
                } );

            var firstContributionDateQry = financialTransactionsQry
                .GroupBy( xx => xx.PersonId )
                .Select( ss => new
                {
                    PersonId = ss.Key,
                    FirstTransactionDate = ss.Min( a => selectionConfig.UseSundayDate == true ? a.txn.SundayDate : a.txn.TransactionDateTime )
                } );

            if ( dateRange.Start.HasValue )
            {
                firstContributionDateQry = firstContributionDateQry.Where( xx => xx.FirstTransactionDate >= dateRange.Start.Value );
            }

            if ( dateRange.End.HasValue )
            {
                firstContributionDateQry = firstContributionDateQry.Where( xx => xx.FirstTransactionDate < dateRange.End.Value );
            }

            var innerQry = firstContributionDateQry.Select( xx => xx.PersonId ).AsQueryable();
            var qry = new PersonService( rockContext ).Queryable().Where( p => innerQry.Any( xx => xx == p.Id ) );

            return FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );
        }

        #endregion Public Methods

        /// <summary>
        /// Get and set the filter settings from DataViewFilter.Selection
        /// </summary>
        protected class SelectionConfig
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SelectionConfig"/> class.
            /// </summary>
            public SelectionConfig()
            {
                AccountGuids = new List<Guid>();
            }

            /// <summary>
            /// Gets or sets the account guids.
            /// </summary>
            /// <value>
            /// The account guids.
            /// </value>
            public List<Guid> AccountGuids { get; set; }

            /// <summary>
            /// Gets a pipe delimited string of the property values. This is to use the SlidingDateRangePicker's existing logic.
            /// </summary>
            /// <value>
            /// The delimited values.
            /// </value>
            public string DelimitedValues
            {
                get
                {
                    return CreateSlidingDateRangePickerDelimitedValues();
                }
            }

            /// <summary>
            /// Gets or sets the date range mode.
            /// </summary>
            /// <value>
            /// The date range mode.
            /// </value>
            public SlidingDateRangeType DateRangeMode { get; set; }

            /// <summary>
            /// Gets or sets the number of time units.
            /// </summary>
            /// <value>
            /// The number of time units.
            /// </value>
            public int? NumberOfTimeUnits { get; set; }

            /// <summary>
            /// Gets or sets the time unit.
            /// </summary>
            /// <value>
            /// The time unit.
            /// </value>
            public TimeUnitType TimeUnit { get; set; }

            /// <summary>
            /// Gets or sets the start date.
            /// </summary>
            /// <value>
            /// The start date.
            /// </value>
            public DateTime? StartDate { get; set; }

            /// <summary>
            /// Gets or sets the end date.
            /// </summary>
            /// <value>
            /// The end date.
            /// </value>
            public DateTime? EndDate { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [use sunday date].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [use sunday date]; otherwise, <c>false</c>.
            /// </value>
            public bool UseSundayDate { get; set; }

            /// <summary>
            /// Parses the specified selection from a JSON or delimited string.
            /// </summary>
            /// <param name="selection">The selection.</param>
            /// <returns></returns>
            public static SelectionConfig Parse( string selection )
            {
                var selectionConfig = selection.FromJsonOrNull<SelectionConfig>();
                if ( selectionConfig == null )
                {
                    selectionConfig = new SelectionConfig();

                    // If the configuration is a delimited string then try to parse it the old fashioned way
                    string[] selectionValues = selection.Split( '|' );

                    // The first two values in the delimited string are legacy, usually blank, and can be ignored.
                    // Index 2 is the account guids
                    if ( selectionValues.Count() > 2 )
                    {
                        selectionConfig.AccountGuids = selectionValues[2].Split( ',' ).AsGuidList();
                    }
                    else
                    {
                        // If there are not at least 3 values in the selection string then it is not valid.
                        return null;
                    }

                    // Index 3 is the date range values
                    if ( selectionValues.Count() > 3 )
                    {
                        string[] dateRangeValues = selectionValues[3].Split( ',' );
                        if ( dateRangeValues.Count() > 3 )
                        {
                            // DateRange index 0 is the mode
                            selectionConfig.DateRangeMode = dateRangeValues[0].ConvertToEnum<SlidingDateRangeType>();

                            // DateRange index 1 is the number of time units
                            selectionConfig.NumberOfTimeUnits = dateRangeValues[1].AsIntegerOrNull();

                            // DateRange index 2 is the time unit
                            selectionConfig.TimeUnit = dateRangeValues[2].ConvertToEnum<TimeUnitType>();

                            // DateRange index 3 is the start date
                            selectionConfig.StartDate = dateRangeValues[3].AsDateTime();

                            // DateRange index 4 is the end date if it exists
                            if ( dateRangeValues.Count() > 4 )
                            {
                                selectionConfig.EndDate = dateRangeValues[4].AsDateTime();
                            }
                        }
                        else if ( dateRangeValues.Any() )
                        {
                            // Try to get a DateRange from what we have
                            selectionConfig.DateRangeMode = SlidingDateRangeType.DateRange;
                            selectionConfig.StartDate = dateRangeValues[0].AsDateTime();

                            if ( dateRangeValues.Count() > 1 )
                            {
                                selectionConfig.EndDate = dateRangeValues[1].AsDateTime();
                                if ( selectionConfig.EndDate.HasValue)
                                {
                                    // This value would have been from the DatePicker which does not automatically add a day.
                                    selectionConfig.EndDate.Value.AddDays( 1 );
                                }
                            }
                        }
                    }

                    // Index 4 is the UseSundayDate boolean
                    if ( selectionValues.Count() > 4 )
                    {
                        selectionConfig.UseSundayDate = selectionValues[4].AsBooleanOrNull() ?? true;
                    }
                }

                return selectionConfig;
            }

            /// <summary>
            /// Creates the sliding date range picker delimited values.
            /// </summary>
            /// <returns></returns>
            private string CreateSlidingDateRangePickerDelimitedValues()
            {
                return string.Format(
                    "{0}|{1}|{2}|{3}|{4}",
                    this.DateRangeMode,
                    ( SlidingDateRangeType.Last | SlidingDateRangeType.Previous | SlidingDateRangeType.Next | SlidingDateRangeType.Upcoming ).HasFlag( DateRangeMode ) ? this.NumberOfTimeUnits : ( int? ) null,
                    ( SlidingDateRangeType.Last | SlidingDateRangeType.Previous | SlidingDateRangeType.Next | SlidingDateRangeType.Upcoming | SlidingDateRangeType.Current ).HasFlag( this.DateRangeMode ) ? this.TimeUnit : ( TimeUnitType? ) null,
                    this.DateRangeMode == SlidingDateRangeType.DateRange ? this.StartDate : null,
                    this.DateRangeMode == SlidingDateRangeType.DateRange ? this.EndDate : null );
            }
        }
    }
}