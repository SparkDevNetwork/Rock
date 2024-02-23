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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Humanizer;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Giving Overview" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Block used to view the giving." )]

    [IntegerField(
        "Inactive Giver Cutoff (days)",
        Key = AttributeKey.InactiveGiverCutoff,
        Description = "The number of days after which a person is considered an inactive giver.",
        IsRequired = true,
        DefaultIntegerValue = 365,
        Order = 0 )]

    [LinkedPage(
        "Alert List Page",
        Description = "The page to see a list of alerts for the person.",
        Order = 1,
        Key = AttributeKey.AlertListPage,
        DefaultValue = Rock.SystemGuid.Page.GIVING_ALERTS )]

    [Rock.SystemGuid.BlockTypeGuid( "896D807D-2110-4007-AFD1-4D953B83375B" )]
    public partial class GivingOverview : Rock.Web.UI.PersonBlock
    {
        #region Constants

        /// <summary>
        /// The HTML escaped "plus or minus" symbol to use for markup strings.
        /// </summary>
        private const string PlusOrMinus = "&#177;";

        #endregion Constants

        #region Attribute Keys

        private static class AttributeKey
        {
            public const string InactiveGiverCutoff = "InactiveGiverCutoff";
            public const string AlertListPage = "AlertListPage";
        }

        #endregion Attribute Keys

        #region Properties

        /// <summary>
        /// Is the yearly summary expanded.
        /// </summary>
        public bool IsYearlySummaryExpanded
        {
            get
            {
                return ViewState["IsYearlySummaryExpanded"].ToStringSafe().AsBoolean();
            }

            set
            {
                ViewState["IsYearlySummaryExpanded"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of notifications currently being displayed.
        /// </summary>
        public decimal MaxGiftAmount
        {
            get
            {
                return ViewState["MaxGiftAmount"] as decimal? ?? 0;
            }

            set
            {
                ViewState["MaxGiftAmount"] = value;
            }
        }

        #endregion Properties

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            var isVisible = Person != null && Person.Id != 0;

            pnlContent.Visible = isVisible;
            if ( isVisible )
            {
                RockPage.AddCSSLink( "~/Styles/Blocks/Crm/GivingOverview.css", true );
                ShowDetail();
            }
        }

        #endregion Base Control Methods

        #region Events

        /// <summary>
        /// Handles the Click event of the lbShowMoreYearlySummary control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbShowMoreYearlySummary_Click( object sender, EventArgs e )
        {
            IsYearlySummaryExpanded = true;
            BindYearlySummary();
        }

        /// <summary>
        /// Handles the Click event of the lbShowLessYearlySummary control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbShowLessYearlySummary_Click( object sender, EventArgs e )
        {
            IsYearlySummaryExpanded = false;
            BindYearlySummary();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptYearSummary control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptYearSummary_ItemDataBound( object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e )
        {
            var contributionSummary = e.Item.DataItem as ContributionSummary;
            if ( contributionSummary == null )
            {
                return;
            }

            var lYearlySummaryYear = e.Item.FindControl( "lYearlySummaryYear" ) as Literal;
            lYearlySummaryYear.Text = contributionSummary.Year.ToString();

            var lAccount = e.Item.FindControl( "lAccount" ) as Literal;

            var accountsHtml = string.Empty;
            foreach ( var item in contributionSummary.SummaryRecords )
            {
                accountsHtml += string.Format(
                @"<tr><td class='pr-4'>{0}</td><td class='text-right'>{1}</td></tr>",
                item.AccountName,
                item.TotalAmount.FormatAsCurrency() );
            }

            lAccount.Text = accountsHtml;

            var lTotalAmount = e.Item.FindControl( "lTotalAmount" ) as Literal;
            lTotalAmount.Text = contributionSummary.TotalAmount.FormatAsCurrency();
        }

        #endregion Events

        #region Internal Methods

        /// <summary>
        /// Shows the message if stale.
        /// </summary>
        private void ShowMessageIfStale()
        {
            /* 2021-09-30 MDP

             Rules for when giving characteristics are considered stale

            Show the ‘stale’ message when the last gift was over { TypicalFrequency + 2* Frequency Standard Deviation }   days.

            Message should be worded as:

            The giving characteristics below were generated (stale time) ago at the time of the last gift.
            Information on bin, percentile and typical gift patterns represent values from that time period.

            */

            nbGivingCharacteristicsStaleWarning.Visible = false;
            var lastTransaction = new FinancialTransactionService( new RockContext() ).GetGivingAutomationSourceTransactionQueryByGivingId( Person.GivingId ).Max( a => ( DateTime? ) a.TransactionDateTime );
            var frequencyMeanDays = Person.GetAttributeValue( Rock.SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS.AsGuid() ).AsDecimalOrNull();
            var frequencyStandardDeviationDays = Person.GetAttributeValue( Rock.SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS.AsGuid() ).AsDecimalOrNull();
            if ( lastTransaction.HasValue && frequencyMeanDays.HasValue && frequencyStandardDeviationDays.HasValue )
            {
                var consideredStaleAfterDays = frequencyMeanDays.Value + ( frequencyStandardDeviationDays.Value * 2 );
                var timeSpanSinceLastUpdated = RockDateTime.Now - lastTransaction.Value;
                if ( ( decimal ) timeSpanSinceLastUpdated.TotalDays > consideredStaleAfterDays )
                {
                    nbGivingCharacteristicsStaleWarning.Text = $@"The giving characteristics below were generated {lastTransaction.ToElapsedString().ToLower()} at the time of the last gift. Information on bin, percentile and typical gift patterns represent values from that time period.";
                    nbGivingCharacteristicsStaleWarning.Visible = true;
                }
            }
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            pnlNoGiving.Visible = false;
            pnlGiving.Visible = false;
            pnlInactiveGiver.Visible = false;

            var contributionType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );
            if ( contributionType == null )
            {
                return;
            }

            Person.LoadAttributes();
            var rockContext = new RockContext();
            var financialTransactionService = new FinancialTransactionService( rockContext );
            var givingId = Person.GivingId;

            // Get the past 3 years of monthly giving history.
            var threeYearsAgo = RockDateTime.Now.AddMonths( -35 ).StartOfMonth();
            var threeYearsOfMonthlyAccountGiving = GetMonthlyGivingHistory( threeYearsAgo );

            if ( threeYearsOfMonthlyAccountGiving.Any() )
            {
                var inactiveGiverCutOffDate = RockDateTime.Now.AddDays( -GetAttributeValue( AttributeKey.InactiveGiverCutoff ).AsInteger() ).Date;
                pnlGiving.Visible = true;
                var hasGiftsAfterCutoff = threeYearsOfMonthlyAccountGiving
                    .Any( h =>
                        h.Amount > 0 && (
                            h.Year >= inactiveGiverCutOffDate.Year ||
                            ( h.Year == inactiveGiverCutOffDate.Year && h.Month >= inactiveGiverCutOffDate.Month )
                        ) );

                if ( !hasGiftsAfterCutoff )
                {
                    var lastGaveObject = threeYearsOfMonthlyAccountGiving.FirstOrDefault( h => h.Amount > 0 );
                    var lastGaveDate = lastGaveObject != null ? ( DateTime? ) new DateTime( lastGaveObject.Year, lastGaveObject.Month, 1 ) : null;
                    pnlInactiveGiver.Visible = true;
                    pnlGivingStats.AddCssClass( "inactive-giving" );
                    lLastGiver.Text = lastGaveDate.HasValue ? lastGaveDate.Value.ToString( "MMM yyyy" ) : string.Empty;
                }
            }
            else
            {
                pnlNoGiving.Visible = true;
                return;
            }

            var contributionByMonths = new Dictionary<DateTime, decimal>();
            for ( var i = 35; i >= 0; i-- )
            {
                var currentMonthlyDate = RockDateTime.Now.StartOfMonth().AddMonths( -i );
                var month = currentMonthlyDate.Month;
                var year = currentMonthlyDate.Year;
                var total = threeYearsOfMonthlyAccountGiving.Where( h => h.Year == year && h.Month == month ).Sum( h => h.Amount );
                contributionByMonths[currentMonthlyDate] = total;
            }

            MaxGiftAmount = contributionByMonths.Max( a => a.Value );

            // Giving By Month Chart
            rptGivingByMonth.DataSource = contributionByMonths.OrderBy( a => a.Key );
            rptGivingByMonth.DataBind();

            // Community View
            ShowCommunityView();

            ShowGivingStatsForLast12Months();

            ShowGivingCharacteristicsKPI( rockContext );

            ShowMessageIfStale();

            BindYearlySummary( threeYearsOfMonthlyAccountGiving );

            var eraFirstGave = Person.GetAttributeValue( "core_EraFirstGave" ).AsDateTime();
            bdgFirstGift.Text = $"First Gift: {eraFirstGave.ToElapsedString()}";
            bdgFirstGift.ToolTip = eraFirstGave.ToShortDateString();

            var eraLastGive = Person.GetAttributeValue( "core_EraLastGave" ).AsDateTime();
            bdgLastGift.Text = $"Last Gift: {eraLastGive.ToElapsedString()}";
            bdgLastGift.ToolTip = eraLastGive.ToShortDateString();

            ShowGivingAlerts();
        }

        private List<MonthlyAccountGivingHistory> GetMonthlyGivingHistory( DateTime? startDate = null )
        {
            var rockContext = new RockContext();
            var givingId = Person.GivingId;

            var financialTransactionService = new FinancialTransactionService( rockContext );
            var givingHistories = financialTransactionService.GetGivingAutomationMonthlyAccountGivingHistory( givingId, startDate, includeNegativeTransactions:true );
            return givingHistories;
        }

        /// <summary>
        /// Shows the giving stats for last12 months.
        /// </summary>
        private void ShowGivingStatsForLast12Months()
        {
            var givingId = Person.GivingId;
            var rockContext = new RockContext();
            var financialTransactionService = new FinancialTransactionService( rockContext );
            var oneYearAgo = RockDateTime.Now.AddMonths( -12 );

            var twelveMonthsTransactionsQry = financialTransactionService
                    .GetGivingAutomationSourceTransactionQueryByGivingId( givingId, true )
                    .Where( t => t.TransactionDateTime >= oneYearAgo );

            var twelveMonthTransactions = twelveMonthsTransactionsQry
                .Select( a => new
                {
                    TransactionDateTime = a.TransactionDateTime,
                    TotalAmountBeforeRefund = a.TransactionDetails
                        .Select( d => d.Amount )
                        .DefaultIfEmpty( 0.0M )
                        .Sum(),
                    // For each Refund (there could be more than one) get the refund amount for each if the refunds's Detail records for the Account.
                    // Then sum that up for the total refund amount for the account
                    TotalRefundAmount = a
                            .Refunds.Select( r => r.FinancialTransaction.TransactionDetails
                            .Sum( rrrr => ( decimal? ) rrrr.Amount ) ).Sum() ?? 0.0M
                } )
                .ToList();

            var last12MonthTotal = twelveMonthTransactions.Sum( t => t.TotalAmountBeforeRefund + t.TotalRefundAmount );
            var last12MonthCount = twelveMonthTransactions.Count;

            // Last 12 Months KPI
            string kpiLast12Months;
            var last12MonthCountText = $"{last12MonthCount} {"gift".PluralizeIf( last12MonthCount != 1 )}";
            kpiLast12Months = GetKpiShortCode(
                "Last 12 Months",
                $"<span class=\"currency-span\">{FormatAsCurrency( last12MonthTotal )}</span>",
                subValue: $"<div class=\"small\">{last12MonthCountText}</div>" );

            // Last 90 Days KPI
            var oneHundredEightyDaysAgo = RockDateTime.Now.AddDays( -180 );
            var ninetyDaysAgo = RockDateTime.Now.AddDays( -90 );
            var transactionPriorNinetyDayTotal = twelveMonthTransactions.Where( t => t.TransactionDateTime >= oneHundredEightyDaysAgo && t.TransactionDateTime < ninetyDaysAgo ).Sum( t => t.TotalAmountBeforeRefund + t.TotalRefundAmount );
            var baseGrowthContribution = transactionPriorNinetyDayTotal;

            var last90DaysContribution = twelveMonthTransactions.Where( t => t.TransactionDateTime >= ninetyDaysAgo ).Sum( t => t.TotalAmountBeforeRefund + t.TotalRefundAmount );

            decimal growthPercent = 0;

            if ( baseGrowthContribution == 0 )
            {
                growthPercent = 100;
            }
            else
            {
                growthPercent = ( last90DaysContribution - baseGrowthContribution ) / baseGrowthContribution * 100;
            }

            var isGrowthPositive = growthPercent >= 0;

            var growthPercentText = Math.Abs( growthPercent ).ToString( "N1" ) + "%";

            string growthPercentDisplay;

            // Show growth Percent
            // If more than 1000% show HIGH or LOW
            if ( growthPercent > 1000 )
            {
                growthPercentDisplay = "HIGH";
            }
            else if ( growthPercent < -1000 )
            {
                growthPercentDisplay = "LOW";
            }
            else
            {
                growthPercentDisplay = growthPercentText;
            }

            var last90DayCount = twelveMonthTransactions.Count( t => t.TransactionDateTime >= ninetyDaysAgo );
            var last90DayCountText = $"{last90DayCount} {"gift".PluralizeIf( last90DayCount != 1 )}";

            var last90DaysSubValue =
$@"<span title=""{growthPercentText}"" class=""small text-{( isGrowthPositive ? "success" : "danger" )}"">
    <i class=""fa {( isGrowthPositive ? "fa-arrow-up" : "fa-arrow-down" )}""></i>
    {growthPercentDisplay}
</span>
<div class=""small"">{last90DayCountText}</div>";

            var kpiLast90Days = GetKpiShortCode(
                "Last 90 Days",
                $"<span class=\"currency-span\">{FormatAsCurrency( last90DaysContribution )}</span>",
                subValue: last90DaysSubValue );

            // Gives as family / individual KPI
            var givesAs = Person.GivingGroupId.HasValue ? "Family" : "Individual";
            var givesAsIcon = Person.GivingGroupId.HasValue ? "fa-fw fa-users" : "fa-fw fa-user";
            var kpiGivesAs = GetKpiShortCode( "Gives As", givesAs, icon: givesAsIcon );

            // Giving Journey
            var journeyStage = ( GivingJourneyStage ) Person.GetAttributeValue( Rock.SystemGuid.Attribute.PERSON_GIVING_CURRENT_GIVING_JOURNEY_STAGE.AsGuid() ).AsInteger();
            var journeyStageName = journeyStage.GetDescription() ?? journeyStage.ConvertToString();
            var kpiGivingJourney = GetKpiShortCode( "Giving Journey", journeyStageName, icon: "fa fa-fw fa-hiking" );

            // Combined KPIs
            var kpi = kpiLast12Months + kpiLast90Days + kpiGivesAs + kpiGivingJourney;

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
            lLastGiving.Text = string.Format( @"{{[kpis style:'edgeless' iconbackground:'false' columnmin:'180px' columncount:'4' columncountmd:'4' columncountsm:'2']}}{0}{{[endkpis]}}", kpi ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Shows the community view.
        /// </summary>
        private void ShowCommunityView()
        {
            var givingPercentileAttribute = AttributeCache.Get( Rock.SystemGuid.Attribute.PERSON_GIVING_PERCENTILE );
            if ( givingPercentileAttribute != null )
            {
                var givingPercentile = Person.GetAttributeValue( givingPercentileAttribute.Key ).AsInteger();
                var percentileStage = 10 - ( givingPercentile / 10 );
                if ( givingPercentile % 10 == 0 && givingPercentile != 0 )
                {
                    percentileStage += 1;
                }

                lPercent.Text = givingPercentile.ToStringSafe();

                var lStage = pnlGiving.FindControl( "lStage" + percentileStage ) as HtmlGenericControl;
                if ( lStage != null )
                {
                    lStage.AddCssClass( "bg-primary" );
                }
            }

            var givingBinAttribute = AttributeCache.Get( Rock.SystemGuid.Attribute.PERSON_GIVING_BIN );
            if ( givingBinAttribute != null )
            {
                var givingBin = Person.GetAttributeValue( givingBinAttribute.Key ).AsInteger();
                lGivingBin.Text = givingBin.ToString();

                var lBin = pnlGiving.FindControl( "lBin" + givingBin ) as HtmlGenericControl;
                if ( lBin != null )
                {
                    lBin.AddCssClass( "bg-primary" );
                }
            }

            if ( givingBinAttribute != null && givingPercentileAttribute != null )
            {
                var givingPercentile = Person.GetAttributeValue( givingPercentileAttribute.Key ).AsInteger();

                lHelpText.Text = Person.NickName.ToPossessive() + " giving is in the " + givingPercentile.Ordinalize() + " percentile, this is classified as Bin " + lGivingBin.Text + ".";
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptGivingByMonth control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptGivingByMonth_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( !( e.Item.DataItem as KeyValuePair<DateTime, decimal>? ).HasValue )
            {
                return;
            }

            var lGivingByMonthPercentHtml = e.Item.FindControl( "lGivingByMonthPercentHtml" ) as Literal;
            if ( lGivingByMonthPercentHtml == null )
            {
                return;
            }

            var contributionByMonth = ( KeyValuePair<DateTime, decimal> ) e.Item.DataItem;

            lGivingByMonthPercentHtml.Text = $@"<li title='{( contributionByMonth.Key.ToString( "MMM yyyy" ) )} {contributionByMonth.Value.FormatAsCurrency()}' />
  <span style='{GetGivingByMonthPercentStyle( contributionByMonth.Value )}'></span>
</li>";
        }

        /// <summary>
        /// Gets the giving by month percent style.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <returns>System.String.</returns>
        protected string GetGivingByMonthPercentStyle( decimal amount )
        {
            return string.Format( "height: {0}%", MaxGiftAmount == 0 ? 0 : amount / MaxGiftAmount * 100 );
        }

        /// <summary>
        /// Shows the giving characteristics kpi.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void ShowGivingCharacteristicsKPI( RockContext rockContext )
        {
            var stringBuilder = new StringBuilder();
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );

            // Typical gift KPI
            var giftAmountMedian = Person.GetAttributeValue( "GiftAmountMedian" ).AsDecimal();
            var giftAmountIqr = Person.GetAttributeValue( "GiftAmountIQR" ).AsDecimal();

            var typicalGiftKpi = GetKpiShortCode(
                "Typical Gift",
                $"<span class=\"currency-span\">{FormatAsCurrency( giftAmountMedian )}</span>",
                $"{giftAmountIqr}",
                "fa-fw fa-money-bill",
                "left",
                $"A typical gift amount has a median value of ${giftAmountMedian} with an IQR variance of ${giftAmountIqr}." );

            stringBuilder.Append( typicalGiftKpi );

            // Add KPI for the average days between gifts and the standard deviation of days between gifts.
            var giftFrequencyDaysMean = Person.GetAttributeValue( Rock.SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS.AsGuid() ).AsDecimal().ToString( "N0" );
            var giftFrequencyDaysMeanUnits = giftFrequencyDaysMean == "1" ? "day" : "days";
            var giftFrequencyDaysStdDev = Person.GetAttributeValue( Rock.SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS.AsGuid() ).AsDecimal().ToString( "N1" );
            var giftFrequencyDaysStdDevUnits = giftFrequencyDaysStdDev == "1.0" ? "day" : "days";

            var typicalFrequencyKpi = GetKpiShortCode(
                "Typical Frequency",
                giftFrequencyDaysMean + "d",
                $"{PlusOrMinus}{giftFrequencyDaysStdDev}d",
                "fa-fw fa-clock",
                description: $"A typical gift frequency has a mean value of {giftFrequencyDaysMean} {giftFrequencyDaysMeanUnits} with a standard deviation variance of {giftFrequencyDaysStdDev} {giftFrequencyDaysStdDevUnits}." );

            stringBuilder.Append( typicalFrequencyKpi );

            // Percent of gifts that are scheduled KPI
            stringBuilder.Append( GetKpiShortCode( "Percent Scheduled", Person.GetAttributeValue( "PercentofGiftsScheduled" ).AsInteger() + "%", icon: "fa-fw fa-percent" ) );

            // Frequency label KPI
            var frequencyLabelAttribute = AttributeCache.Get( Rock.SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_LABEL );
            if ( frequencyLabelAttribute != null )
            {
                var frequencyLabel = frequencyLabelAttribute.FieldType.Field.FormatValue( null, Person.GetAttributeValue( "FrequencyLabel" ), frequencyLabelAttribute.QualifierValues, false );
                stringBuilder.Append( GetKpiShortCode( "Frequency", frequencyLabel, icon: "fa-fw fa-calendar-alt", textAlign: "left" ) );
            }

            // Preferred currency KPI
            var currencyTypeIconCssClassAttr = AttributeCache.Get( Rock.SystemGuid.Attribute.DEFINED_TYPE_CURRENCY_TYPE_ICONCSSCLASS );
            if ( currencyTypeIconCssClassAttr != null )
            {
                var iconCssClass = currencyTypeIconCssClassAttr.DefaultValue;

                var preferredCurrencyGuidValue = Person.GetAttributeValue( "PreferredCurrency" ).AsGuidOrNull();
                if ( preferredCurrencyGuidValue.HasValue )
                {
                    var preferredCurrencyValue = DefinedValueCache.Get( preferredCurrencyGuidValue.Value );
                    if ( preferredCurrencyValue.GetAttributeValue( "IconCssClass" ).IsNotNullOrWhiteSpace() )
                    {
                        iconCssClass = preferredCurrencyValue.GetAttributeValue( "IconCssClass" );
                    }

                    stringBuilder.Append( GetKpiShortCode( "Preferred Currency", preferredCurrencyValue.Value, icon: "fa-fw " + iconCssClass ) );
                }
                else
                {
                    stringBuilder.Append( GetKpiShortCode( "Preferred Currency", string.Empty, icon: "fa-fw " + iconCssClass ) );
                }
            }

            // Preferred source KPI
            var transactionSourceIconCssClassAttr = AttributeCache.Get( Rock.SystemGuid.Attribute.DEFINED_TYPE_TRANSACTION_SOURCE_ICONCSSCLASS );
            if ( transactionSourceIconCssClassAttr != null )
            {
                var iconCssClass = transactionSourceIconCssClassAttr.DefaultValue;
                var preferredSourceGuidValue = Person.GetAttributeValue( "PreferredSource" ).AsGuidOrNull();
                if ( preferredSourceGuidValue.HasValue )
                {
                    var preferredSourceValue = DefinedValueCache.Get( preferredSourceGuidValue.Value );
                    if ( preferredSourceValue.GetAttributeValue( "IconCssClass" ).IsNotNullOrWhiteSpace() )
                    {
                        iconCssClass = preferredSourceValue.GetAttributeValue( "IconCssClass" );
                    }

                    stringBuilder.Append( GetKpiShortCode( "Preferred Source", preferredSourceValue.Value, icon: "fa-fw " + iconCssClass ) );
                }
                else
                {
                    stringBuilder.Append( GetKpiShortCode( "Preferred Source", string.Empty, icon: "fa-fw " + iconCssClass ) );
                }
            }

            lGivingCharacteristicsHtml.Text = string.Format( @"{{[kpis columnmin:'200px' style:'edgeless' iconbackground:'false' columncount:'3' columncountmd:'2' columncountsm:'2']}}{0}{{[endkpis]}}", stringBuilder ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Shows the giving alerts
        /// </summary>
        private void ShowGivingAlerts()
        {
            var rockContext = new RockContext();
            var financialTransactionAlertService = new FinancialTransactionAlertService( rockContext );
            var givingId = Person.GivingId;
            var givingIdPersonAliasIdQuery = new PersonAliasService( rockContext ).Queryable().Where( a => a.Person.GivingId == givingId ).Select( a => a.Id );
            var financialTransactionAlertQry = financialTransactionAlertService.Queryable().AsNoTracking()
                    .Where( a => givingIdPersonAliasIdQuery.Contains( a.PersonAliasId ) );

            var financialTransactionGratitudeCount = financialTransactionAlertQry.Where( a => a.FinancialTransactionAlertType.AlertType == AlertType.Gratitude ).Count();
            var financialTransactionFollowupCount = financialTransactionAlertQry.Where( a => a.FinancialTransactionAlertType.AlertType == AlertType.FollowUp ).Count();

            var alertListUrl = LinkedPageUrl( AttributeKey.AlertListPage, new Dictionary<string, string> { { "PersonGuid", Person.Guid.ToString() } } );

            var hasAlertListLink = !alertListUrl.IsNullOrWhiteSpace();

            var givingAlertsBadges = $"<span class=\"badge badge-success align-text-bottom\">{financialTransactionGratitudeCount}</span> <span class=\"badge badge-warning align-text-bottom\">{financialTransactionFollowupCount}</span>";
            if ( hasAlertListLink )
            {
                lGivingAlertsBadgesHtml.Text = $"<a href=\"{alertListUrl}\">" + givingAlertsBadges + "</a>";
            }
            else
            {
                lGivingAlertsBadgesHtml.Text = givingAlertsBadges;
            }
        }

        /// <summary>
        /// Gets the kpi short code.
        /// </summary>
        /// <param name="label">The label.</param>
        /// <param name="value">The value.</param>
        /// <param name="subValue">The sub value.</param>
        /// <param name="icon">The icon.</param>
        /// <param name="textAlign">The text align.</param>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        private string GetKpiShortCode( string label, string value, string subValue = "", string icon = "", string textAlign = "", string description = "" )
        {
            if ( subValue.IsNotNullOrWhiteSpace() )
            {
                subValue = string.Format( "subvalue:'{0}'", subValue );
            }

            if ( icon.IsNotNullOrWhiteSpace() )
            {
                icon = string.Format( "icon:'{0}'", icon );
            }

            if ( textAlign.IsNotNullOrWhiteSpace() )
            {
                textAlign = string.Format( "textalign:'{0}'", textAlign );
            }

            var kpi = $"[[ kpi {icon} labellocation:'top' value:'{value}' {subValue} label:'{label}' {textAlign} description:'{description}' ]][[ endkpi ]]";
            return kpi;
        }

        /// <summary>
        /// Binds the yearly summary.
        /// </summary>
        private void BindYearlySummary( List<MonthlyAccountGivingHistory> givingHistories = null )
        {
            var givingId = Person.GivingId;
            using ( var rockContext = new RockContext() )
            {
                DateTime? startDate;
                if ( !IsYearlySummaryExpanded )
                {
                    // Only show this current year and last year
                    startDate = RockDateTime.Now.StartOfYear().AddYears( -1 );
                }
                else
                {
                    startDate = null;
                }

                // If a list of giving histories is not supplied, retrieve it now.
                if ( givingHistories == null )
                {
                    givingHistories = GetMonthlyGivingHistory( startDate );
                }

                var previousYearMonthlyGivingSummaries = givingHistories;

                if ( startDate != null )
                {
                    previousYearMonthlyGivingSummaries = givingHistories
                        .Where( s => s.Year >= startDate.Value.Year )
                        .ToList();
                }

                var financialAccounts = new FinancialAccountService( rockContext ).Queryable()
                    .AsNoTracking()
                    .ToDictionary( k => k.Id, v => new FinancialAccountInfo { Name = v.Name, Order = v.Order } );

                var summaryList = previousYearMonthlyGivingSummaries
                    .GroupBy( a => new { a.Year, a.AccountId } )
                    .Select( t => new SummaryRecord
                    {
                        Year = t.Key.Year,
                        AccountId = t.Key.AccountId,
                        TotalAmount = t.Sum( d => d.Amount )
                    } )
                    .OrderByDescending( a => a.Year )
                    .ToList();

                // Create yearly contribution summaries by account.
                var contributionSummaries = new List<ContributionSummary>();
                foreach ( var item in summaryList.GroupBy( a => a.Year ) )
                {
                    var contributionSummary = new ContributionSummary();
                    contributionSummary.Year = item.Key;

                    var summaryRecords = new List<SummaryRecord>();
                    foreach ( var a in item )
                    {
                        if ( financialAccounts.ContainsKey( a.AccountId ) )
                        {
                            var account = financialAccounts[a.AccountId];
                            a.AccountName = account.Name;
                            a.Order = account.Order;
                        }
                        else
                        {
                            a.AccountName = string.Empty;
                            a.Order = 0;
                        }

                        summaryRecords.Add( a );
                    }

                    // Display the accounts in the order specified by the Accounts list.
                    contributionSummary.SummaryRecords = summaryRecords
                        .OrderBy( s => s.Order )
                        .ThenBy( s => s.AccountName )
                        .ToList();

                    contributionSummary.TotalAmount = item.Sum( a => a.TotalAmount );
                    contributionSummaries.Add( contributionSummary );
                }

                rptYearSummary.DataSource = contributionSummaries;
                rptYearSummary.DataBind();

                // Show the correct button to expand or collapse
                lbShowLessYearlySummary.Visible = IsYearlySummaryExpanded;
                lbShowMoreYearlySummary.Visible = !IsYearlySummaryExpanded;
            }
        }

        private string FormatAsCurrency( decimal value )
        {
            // wrap the first value returned with a span for styling
            string val = value.FormatAsCurrencyWithDecimalPlaces( 0 );
            return String.Format( "<span>{0}</span>{1}", val.Substring( 0, 1 ), val.Substring( 1 ) );
        }

        #endregion Methods

        /// <summary>
        ///
        /// </summary>
        protected class SummaryRecord
        {
            public int Year { get; set; }

            public int AccountId { get; set; }

            public string AccountName { get; set; }

            public decimal TotalAmount { get; set; }

            public int Order { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        protected class ContributionSummary
        {
            public int Year { get; set; }

            public List<SummaryRecord> SummaryRecords { get; set; }

            public decimal TotalAmount { get; set; }
        }

        private class FinancialAccountInfo
        {
            public string Name { get; set; }

            public int Order { get; set; }
        }
    }
}