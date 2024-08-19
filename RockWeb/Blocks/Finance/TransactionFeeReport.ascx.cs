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
using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// </summary>
    [DisplayName( "Transaction Fee Report" )]
    [Category( "Finance" )]
    [Description( "Block that reports transaction fees." )]

    [Rock.SystemGuid.BlockTypeGuid( "D75AF7AE-94B8-4604-B768-A124A2F55449" )]
    public partial class TransactionFeeReport : RockBlock
    {
        #region UserPreferenceKeys

        private static class UserPreferenceKey
        {
            public const string AccountIds = "AccountIds";
            public const string SlidingDateRangeDelimitedValues = "SlidingDateRangeDelimitedValues";
        }

        #endregion UserPreferenceKeys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindFilter();

                ShowReportOutput();
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            var preferences = GetBlockPersonPreferences();
            var slidingDateRangeDelimitedValues = preferences.GetValue( UserPreferenceKey.SlidingDateRangeDelimitedValues );
            if ( slidingDateRangeDelimitedValues.IsNotNullOrWhiteSpace() )
            {
                srpFilterDates.DelimitedValues = slidingDateRangeDelimitedValues;
            }
            else
            {
                srpFilterDates.SlidingDateRangeMode = Rock.Web.UI.Controls.SlidingDateRangePicker.SlidingDateRangeType.Last;
                srpFilterDates.TimeUnit = Rock.Web.UI.Controls.SlidingDateRangePicker.TimeUnitType.Month;
                srpFilterDates.NumberOfTimeUnits = 3;
            }

            var accountIds = preferences.GetValue( UserPreferenceKey.AccountIds ).SplitDelimitedValues().AsIntegerList();
            apAccounts.SetValues( accountIds );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            this.NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Handles the Click event of the bbtnApply control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bbtnApply_Click( object sender, EventArgs e )
        {
            ShowReportOutput();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the report output.
        /// </summary>
        public void ShowReportOutput()
        {
            var rockContext = new RockContext();
            var financialTransactionDetailService = new FinancialTransactionDetailService( rockContext );
            var preferences = GetBlockPersonPreferences();

            var qry = financialTransactionDetailService.Queryable();

            var startDateTime = srpFilterDates.SelectedDateRange.Start;
            var endDateTime = srpFilterDates.SelectedDateRange.End;

            preferences.SetValue( UserPreferenceKey.SlidingDateRangeDelimitedValues, srpFilterDates.DelimitedValues );
            preferences.SetValue( UserPreferenceKey.AccountIds, apAccounts.SelectedIds.ToList().AsDelimited( "," ) );
            preferences.Save();

            qry = qry.Where( a => a.Transaction.TransactionDateTime >= startDateTime && a.Transaction.TransactionDateTime < endDateTime );

            var selectedAccountIds = apAccounts.SelectedIds;
            if ( selectedAccountIds.Any() )
            {
                if ( selectedAccountIds.Count() == 1 )
                {
                    var accountId = selectedAccountIds[0];
                    qry = qry.Where( a => a.AccountId == accountId );
                }
                else
                {
                    qry = qry.Where( a => selectedAccountIds.Contains( a.AccountId ) );
                }
            }

            qry = qry.Where( a => a.Transaction.TransactionDetails.Any( x => x.FeeCoverageAmount.HasValue ) );

            var totals = qry.Select( a => new { a.FeeCoverageAmount, a.TransactionId, a.Transaction.FinancialPaymentDetail.CurrencyTypeValueId } );
            var transactionFeeCoverageList = totals.ToList();

            var totalsByTransactionId = transactionFeeCoverageList
                .GroupBy( a => a.TransactionId )
                .Select( a => new
                {
                    TransactionId = a.Key,

                    // There is only one currency per transaction, so FirstOrDefault works here
                    CurrencyTypeValueId = a.FirstOrDefault().CurrencyTypeValueId,
                    FeeCoverageAmount = a.Sum( x => x.FeeCoverageAmount ?? 0.00M ),
                } );

            var currencyTypeIdCreditCard = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid() );
            var currencyTypeIdACH = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH.AsGuid() );

            var creditCardTransactions = totalsByTransactionId.Where( a => a.CurrencyTypeValueId == currencyTypeIdCreditCard ).ToList();
            var achTransactions = totalsByTransactionId.Where( a => a.CurrencyTypeValueId == currencyTypeIdACH ).ToList();

            var achCount = achTransactions.Count();
            var achFeeCoverageTotal = achTransactions.Sum( a => a.FeeCoverageAmount );
            var creditCardCount = creditCardTransactions.Count();
            var creditCardFeeCoverageTotal = creditCardTransactions.Sum( a => a.FeeCoverageAmount );

            var kpiLava = @"
{[kpis style:'card' columncount:'3']}
  [[ kpi icon:'fa-list' value:'{{TotalFeeCoverageAmount | FormatAsCurrency }}' label:'{{TotalFeeCoverageLabel}}' color:'blue-500']][[ endkpi ]]
  [[ kpi icon:'fa-credit-card' value:'{{CreditCardFeeCoverageAmount | FormatAsCurrency }}' label:'{{CreditFeeCoverageLabel}}' color:'green-500']][[ endkpi ]]
  [[ kpi icon:'fa fa-money-check-alt' value:'{{ACHFeeCoverageAmount | FormatAsCurrency }}' label:'{{ACHFeeCoverageLabel}}' color:'indigo-500' ]][[ endkpi ]]
{[endkpis]}";

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage );
            mergeFields.Add( "TotalFeeCoverageAmount", achFeeCoverageTotal + creditCardFeeCoverageTotal );
            mergeFields.Add( "CreditCardFeeCoverageAmount", creditCardFeeCoverageTotal );
            mergeFields.Add( "ACHFeeCoverageAmount", achFeeCoverageTotal );

            var totalFeeCoverageLabel = string.Format( "Total Fees<br/>{0:N0} Transactions with Fees", achCount + creditCardCount );
            var creditCardFeeCoverageLabel = string.Format( "Credit Card Fees<br/>{0:N0} Transactions with Fees", creditCardCount );
            var achFeeCoverageLabel = string.Format( "ACH Fees<br/>{0:N0} Transactions with Fees", achCount );

            mergeFields.Add( "TotalFeeCoverageLabel", totalFeeCoverageLabel );
            mergeFields.Add( "CreditFeeCoverageLabel", creditCardFeeCoverageLabel );
            mergeFields.Add( "ACHFeeCoverageLabel", achFeeCoverageLabel );

            var kpiHtml = kpiLava.ResolveMergeFields( mergeFields );
            lKPIHtml.Text = kpiHtml;
        }

        #endregion
    }
}