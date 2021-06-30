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
using System.Text;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

using Humanizer;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Giving Overview" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Block used to view the giving." )]

    [IntegerField(
        "Inactive Giver Cutoff (Days)",
        Key = AttributeKey.InactiveGiverCutoff,
        Description = "The number of days after which a person is considered an inactive giver.",
        IsRequired = true,
        DefaultIntegerValue = 365,
        Order = 0 )]

    [LinkedPage(
        "Alert List Page",
        Description = "The page to see a list of alerts for the person.",
        Order = 1,
        Key = AttributeKey.AlertListPage )]

    public partial class GivingOverview : Rock.Web.UI.PersonBlock
    {
        #region Constants

        /// <summary>
        /// The "plus or minus" symbol to use for markup strings
        /// </summary>
        private const string PlusOrMinus = "±";

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
                SyncLastUpdatedLabel();
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
        /// Synchronizes the last updated label.
        /// </summary>
        private void SyncLastUpdatedLabel()
        {
            var lastUpdated = Person.GetAttributeValue( Rock.SystemGuid.Attribute.PERSON_GIVING_LAST_CLASSIFICATION_DATE.AsGuid() ).AsDateTime();

            if ( lastUpdated.HasValue )
            {
                hlLastUpdated.Text = string.Format( "Last Update: {0}", lastUpdated.ToElapsedString() );
            }
            else
            {
                hlLastUpdated.Text = "Last Update: Unknown";
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
            var contributionByMonths = new Dictionary<DateTime, decimal>();

            var historyJson = Person.GetAttributeValue( Rock.SystemGuid.Attribute.PERSON_GIVING_HISTORY_JSON.AsGuid() );
            var historyObjects = FinancialTransactionService.GetGivingAnalyticsMonthlyAccountGivingHistoryFromJson( historyJson );

            for ( var i = 35; i >= 0; i-- )
            {
                var currentMonthlyDate = RockDateTime.Now.StartOfMonth().AddMonths( -i );
                var month = currentMonthlyDate.Month;
                var year = currentMonthlyDate.Year;
                var total = historyObjects.Where( h => h.Year == year && h.Month == month ).Sum( h => h.Amount );
                contributionByMonths[currentMonthlyDate] = total;
            }

            var threeYearsAgo = RockDateTime.Now.AddMonths( -35 ).StartOfMonth();
            var threeYearsGiftData = historyObjects
                .Where( h =>
                    h.Year >= threeYearsAgo.Year ||
                    ( h.Year == threeYearsAgo.Year && h.Month >= threeYearsAgo.Month )
                )
                .ToList();

            if ( threeYearsGiftData.Any() )
            {
                var inactiveGiverCutOffDate = RockDateTime.Now.AddDays( -GetAttributeValue( AttributeKey.InactiveGiverCutoff ).AsInteger() ).Date;
                pnlGiving.Visible = true;
                var hasGiftsAfterCutoff = threeYearsGiftData
                    .Any( h =>
                        h.Amount > 0 && (
                            h.Year >= inactiveGiverCutOffDate.Year ||
                            ( h.Year == inactiveGiverCutOffDate.Year && h.Month >= inactiveGiverCutOffDate.Month )
                        )
                    );

                if ( !hasGiftsAfterCutoff )
                {
                    var lastGaveObject = historyObjects.FirstOrDefault( h => h.Amount > 0 );
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

            MaxGiftAmount = contributionByMonths.Max( a => a.Value );

            rptGivingByMonth.DataSource = contributionByMonths.OrderBy( a => a.Key );
            rptGivingByMonth.DataBind();

            var givingPercentileAttribute = AttributeCache.Get( Rock.SystemGuid.Attribute.PERSON_GIVING_PERCENTILE );
            if ( givingPercentileAttribute != null )
            {
                var givingPercentile = Person.GetAttributeValue( givingPercentileAttribute.Key ).AsInteger();
                var percentileStage = 10 - givingPercentile / 10;
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

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
            var last12MonthTotal = Person.GetAttributeValue( Rock.SystemGuid.Attribute.PERSON_GIVING_12_MONTHS.AsGuid() ).AsDecimal();
            var last12MonthCount = Person.GetAttributeValue( Rock.SystemGuid.Attribute.PERSON_GIVING_12_MONTHS_COUNT.AsGuid() ).AsInteger();
            var last90DayCount = Person.GetAttributeValue( Rock.SystemGuid.Attribute.PERSON_GIVING_90_DAYS_COUNT.AsGuid() ).AsInteger();
            var baseGrowthContribution = Person.GetAttributeValue( Rock.SystemGuid.Attribute.PERSON_GIVING_PRIOR_90_DAYS.AsGuid() ).AsDecimal();
            var last90DaysContribution = Person.GetAttributeValue( Rock.SystemGuid.Attribute.PERSON_GIVING_90_DAYS.AsGuid() ).AsDecimal();
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
            var growthTitle = Math.Abs( growthPercent ).ToString( "N1" ) + "%";
            var growthText = growthPercent > 1000 ?
                "HIGH" :
                growthPercent < -1000 ?
                    "LOW" :
                    growthTitle;

            string kpi = GetKpiShortCode(
                "Last 12 Months",
                FormatAsCurrency( last12MonthTotal ),
                subValue: string.Format( "<div class=\"d-block mt-2\"><span class=\"badge badge-warning \">First Gift: {0}</span></div>", Person.GetAttributeValue( "core_EraFirstGave" ).AsDateTime().ToShortDateString() ) );

            kpi += GetKpiShortCode(
                "Last 90 Days",
                FormatAsCurrency( last90DaysContribution ),
                string.Format(
                    "<span title=\"{3}\" class=\"small text-{2}\"><i class=\"fa {1}\"></i> {0}</span>",
                    growthText, // 0
                    isGrowthPositive ? "fa-arrow-up" : "fa-arrow-down", // 1
                    isGrowthPositive ? "success" : "danger", // 2
                    growthTitle ) ); // 3

            kpi += GetKpiShortCode( "Gifts Last 12 Months", last12MonthCount.ToStringSafe() );
            kpi += GetKpiShortCode( "Gifts Last 90 Days", last90DayCount.ToStringSafe() );
            lLastGiving.Text = string.Format( @"{{[kpis size:'lg' columnmin:'200px' columncount:'4' columncountmd:'3' columncountsm:'2']}}{0}{{[endkpis]}}", kpi ).ResolveMergeFields( mergeFields );

            GetGivingAnalyticsKPI( rockContext );

            BindYearlySummary();
        }

        protected string GetGivingByMonthPercent( decimal amount )
        {
            return string.Format( "height: {0}%", MaxGiftAmount == 0 ? 0 : amount / MaxGiftAmount * 100 );
        }

        private void GetGivingAnalyticsKPI( RockContext rockContext )
        {
            var stringBuilder = new StringBuilder();
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );

            // Typical gift KPI
            var giftAmountMedian = FormatAsCurrency( Person.GetAttributeValue( "GiftAmountMedian" ).AsDecimal() );
            var giftAmountIqr = FormatAsCurrency( Person.GetAttributeValue( "GiftAmountIQR" ).AsDecimal() );

            var typicalGiftKpi = GetKpiShortCode(
                "Typical Gift",
                giftAmountMedian,
                $"{PlusOrMinus} {giftAmountIqr}",
                "fa-fw fa-money-bill",
                "left",
                $"A typical gift amount has a median value of {giftAmountMedian} with an IQR variance of {giftAmountIqr}." );

            stringBuilder.Append( typicalGiftKpi );

            // Add KPI for the average days between gifts and the standard deviation of days between gifts.
            var giftFrequencyDaysMean = Person.GetAttributeValue( Rock.SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS.AsGuid() ).AsDecimal().ToString( "N0" );
            var giftFrequencyDaysMeanUnits = giftFrequencyDaysMean == "1" ? "day" : "days";
            var giftFrequencyDaysStdDev = Person.GetAttributeValue( Rock.SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS.AsGuid() ).AsDecimal().ToString( "N1" );
            var giftFrequencyDaysStdDevUnits = giftFrequencyDaysStdDev == "1.0" ? "day" : "days";

            var typicalFrequencyKpi = GetKpiShortCode(
                "Typical Frequency",
                giftFrequencyDaysMean + "d",
                $"{PlusOrMinus} {giftFrequencyDaysStdDev}d",
                "fa-fw fa-clock",
                description: $"A typical gift frequency has a mean value of {giftFrequencyDaysMean} {giftFrequencyDaysMeanUnits} with a standard deviation variance of {giftFrequencyDaysStdDev} {giftFrequencyDaysStdDevUnits}." );

            stringBuilder.Append( typicalFrequencyKpi );

            // Percent of gifts that are scheduled KPI
            stringBuilder.Append( GetKpiShortCode( "Percent Scheduled", Person.GetAttributeValue( "PercentofGiftsScheduled" ).AsInteger() + "%", icon: "fa-fw fa-percent" ) );

            // Gives as family / individual KPI
            var givesAs = Person.GivingGroupId.HasValue ? "Family" : "Individual";
            var givesAsIcon = Person.GivingGroupId.HasValue ? "fa-fw fa-users" : "fa-fw fa-user";
            stringBuilder.Append( GetKpiShortCode( "Gives As", givesAs, icon: givesAsIcon ) );

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

            // Frequency label KPI
            var frequencyLabelAttribute = AttributeCache.Get( Rock.SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_LABEL );
            if ( frequencyLabelAttribute != null )
            {
                var frequencyLabel = frequencyLabelAttribute.FieldType.Field.FormatValue( null, Person.GetAttributeValue( "FrequencyLabel" ), frequencyLabelAttribute.QualifierValues, false );
                stringBuilder.Append( GetKpiShortCode( "Frequency", frequencyLabel, icon: "fa-fw fa-calendar-alt", textAlign: "left" ) );
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

            var financialTransactionAlertService = new FinancialTransactionAlertService( rockContext );
            var financialTransactionAlertQry = financialTransactionAlertService.Queryable().AsNoTracking()
                .Where( a =>
                    a.PersonAlias.PersonId == Person.Id );
            var financialTransactionGratitudeCount = financialTransactionAlertQry.Where( a => a.FinancialTransactionAlertType.AlertType == AlertType.Gratitude ).Count();
            var financialTransactionFollowupCount = financialTransactionAlertQry.Where( a => a.FinancialTransactionAlertType.AlertType == AlertType.FollowUp ).Count();

            var alertListUrl = LinkedPageUrl( AttributeKey.AlertListPage, new Dictionary<string, string> {
                {  "PersonGuid", Person.Guid.ToString() }
            } );
            var hasAlertListLink = !alertListUrl.IsNullOrWhiteSpace();

            stringBuilder.Append( GetKpiShortCode(
                "Giving Alerts",
                string.Format(
                    "{2}<span class=\"badge bg-success\">{0}</span> <span class=\"badge bg-warning\">{1}</span>{3}",
                    financialTransactionGratitudeCount, // 0
                    financialTransactionFollowupCount, // 1
                    hasAlertListLink ? string.Format( "<a href=\"{0}\">", alertListUrl ) : string.Empty, // 2
                    hasAlertListLink ? "</a>" : string.Empty ), // 3
                icon: "fa-fw fa-comment-alt" ) );

            lGivingAnalytics.Text = string.Format( @"{{[kpis columnmin:'200px' iconbackground:'false' columncount:'4' columncountmd:'3' columncountsm:'2']}}{0}{{[endkpis]}}", stringBuilder ).ResolveMergeFields( mergeFields );
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

            var kpi = string.Format(
                "[[ kpi {3} labellocation:'top' value:'{0}' {2} label:'{1}' {4} description:'{5}' ]][[ endkpi ]]",
                value, // 0
                label, // 1
                subValue, // 2
                icon, // 3
                textAlign, //4
                description ); //5
            return kpi;
        }

        /// <summary>
        /// Binds the yearly summary.
        /// </summary>
        private void BindYearlySummary()
        {
            var historyJson = Person.GetAttributeValue( Rock.SystemGuid.Attribute.PERSON_GIVING_HISTORY_JSON.AsGuid() );
            var historyObjects = FinancialTransactionService.GetGivingAnalyticsMonthlyAccountGivingHistoryFromJson( historyJson );

            if ( !IsYearlySummaryExpanded )
            {
                // Only show this current year and last year
                historyObjects = historyObjects
                    .Where( h => h.Year >= ( RockDateTime.Now.Year - 1 ) )
                    .ToList();
            }

            using ( var rockContext = new RockContext() )
            {
                var financialAccounts = new FinancialAccountService( rockContext ).Queryable()
                    .AsNoTracking()
                    .ToDictionary( k => k.Id, v => v.Name );

                var summaryList = historyObjects
                    .GroupBy( a => new { a.Year, a.AccountId } )
                    .Select( t => new SummaryRecord
                    {
                        Year = t.Key.Year,
                        AccountId = t.Key.AccountId,
                        TotalAmount = t.Sum( d => d.Amount )
                    } )
                    .OrderByDescending( a => a.Year )
                    .ToList();

                var contributionSummaries = new List<ContributionSummary>();
                foreach ( var item in summaryList.GroupBy( a => a.Year ) )
                {
                    var contributionSummary = new ContributionSummary();
                    contributionSummary.Year = item.Key;
                    contributionSummary.SummaryRecords = new List<SummaryRecord>();
                    foreach ( var a in item )
                    {
                        a.AccountName = financialAccounts.ContainsKey( a.AccountId ) ? financialAccounts[a.AccountId] : string.Empty;
                        contributionSummary.SummaryRecords.Add( a );
                    }

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
            return value.FormatAsCurrencyWithDecimalPlaces( 0 );
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
    }
}