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
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;

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
    public partial class GivingOverview : Rock.Web.UI.PersonBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string InactiveGiverCutoff = "InactiveGiverCutoff";
        }

        #endregion Attribute Keys

        #region Properties

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
                ShowDetail();
            }
        }

        #endregion Base Control Methods

        #region Events

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
                @"<tr><td>{0}</td><td class='text-right'>{1}</td></tr>",
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
            var transactionDetailService = new FinancialTransactionDetailService( rockContext );
            var qry = transactionDetailService.Queryable().AsNoTracking()
                .Where( a =>
                    a.Transaction.TransactionTypeValueId == contributionType.Id &&
                    a.Transaction.TransactionDateTime.HasValue );

            qry = qry.Where( t => t.Transaction.AuthorizedPersonAlias.Person.GivingId == Person.GivingId );


            if ( qry.Any() )
            {
                var inactiveGiverCutOffDate = RockDateTime.Now.AddDays( -GetAttributeValue( AttributeKey.InactiveGiverCutoff ).AsInteger() ).Date;
                pnlGiving.Visible = true;
                if ( qry.Where( a => a.Transaction.TransactionDateTime.Value >= inactiveGiverCutOffDate ).Count() == default( int ) )
                {
                    pnlInactiveGiver.Visible = true;
                    lLastGiver.Text = qry
                        .OrderByDescending( a => a.Transaction.TransactionDateTime.Value )
                        .Select( a => a.Transaction.TransactionDateTime.Value )
                        .FirstOrDefault()
                        .ToShortDateString();
                }

            }
            else
            {
                pnlNoGiving.Visible = true;
                return;
            }

            var contributionByMonths = new Dictionary<DateTime, decimal>();
            for ( var i = 0; i <= 35; i++ )
            {
                var startDate = RockDateTime.Now.AddMonths( -i ).StartOfMonth();
                var amt = GetContributionByMonth( startDate );
                contributionByMonths.AddOrReplace( startDate, amt );
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
            }
            

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
            var last12MonthStartDate = RockDateTime.Now.AddMonths( -12 ).StartOfMonth();
            var last90DaysStartDate = RockDateTime.Now.AddDays( -90 ).Date;
            var last180DaysStartDate = RockDateTime.Now.AddDays( -180 ).Date;
            var last12MonthQry = qry.Where( a => a.Transaction.TransactionDateTime.Value >= last12MonthStartDate );
            var last90DaysQry = qry.Where( a => a.Transaction.TransactionDateTime.Value >= last90DaysStartDate );
            var baseGrowthQry = qry.Where( a => a.Transaction.TransactionDateTime.Value >= last180DaysStartDate && a.Transaction.TransactionDateTime.Value < last90DaysStartDate );
            var baseGrowthContribution = baseGrowthQry.Select( a => a.Amount ).DefaultIfEmpty().Sum();
            var last90DaysContribution = last90DaysQry.Select( a => a.Amount ).DefaultIfEmpty().Sum();
            decimal growthPercent = 0;
            if ( last90DaysContribution == 0 )
            {
                growthPercent = 0;
            }
            else if ( baseGrowthContribution == 0 )
            {
                growthPercent = 100;
            }
            else
            {
                growthPercent = ( baseGrowthContribution - last90DaysContribution ) / baseGrowthContribution * 100;
            }

            var isGrowthPositive = growthPercent >= 0;

            string kpi = GetKpiShortCode(
                "$ Last 12 Months",
                FormatAsCurrency( contributionByMonths.Where( a => a.Key >= last12MonthStartDate ).Sum( a => a.Value ) ),
                subValue: string.Format( "<span class=\"label label-warning \">{0}</span>", Person.GetAttributeValue( "core_EraFirstGave" ).AsDateTime().ToShortDateString() ) );
            kpi += GetKpiShortCode(
                "$ Last 90 Days",
                FormatAsCurrency( last90DaysContribution ),
                string.Format( "<span class=\"small text-{2}\"><i class=\"fa {1}\"></i> {0}%</span>", Math.Round( Math.Abs( growthPercent ), 2 ), isGrowthPositive ? "fa-arrow-up" : "fa-arrow-down", isGrowthPositive ? "success" : "danger" ) );
            kpi += GetKpiShortCode( "Gifts Last 12 Months", last12MonthQry.Select( a => a.TransactionId ).Distinct().Count().ToStringSafe() );
            kpi += GetKpiShortCode( "Gifts Last 90 Days", last90DaysQry.Select( a => a.TransactionId ).Distinct().Count().ToStringSafe() );
            lLastGiving.Text = string.Format( @"{{[kpis size:'xl' columnmin:'220px' columnminmd:'220px' columncount:'4' columncountmd:'3' columncountsm:'2']}}{0}{{[endkpis]}}", kpi ).ResolveMergeFields( mergeFields );

            GetGivingAnalyticsKPI( rockContext );

            BindYearlySummary();
        }

        protected string GetGivingByMonthPercent( decimal amount )
        {
            return string.Format( "height: {0}%", MaxGiftAmount == 0 ? 0 : amount / MaxGiftAmount * 100 );
        }

        private void GetGivingAnalyticsKPI( RockContext rockContext )
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
            var givingAnalytics = GetKpiShortCode( "Typical Gift", FormatAsCurrency( Person.GetAttributeValue( "GiftAmountMedian" ).AsDecimal() ), FormatAsCurrency( Person.GetAttributeValue( "GiftAmountIQR" ).AsDecimal() ) + " σ", "fa-fw fa-money-bill", "left" );
            givingAnalytics += GetKpiShortCode( "Typical Frequency", Person.GetAttributeValue( "GiftFrequencyDaysMean" ).AsInteger() + "d", Person.GetAttributeValue( "GiftFrequencyDaysStandardDeviation" ).AsInteger() + "d σ", "fa-fw fa-clock" );
            givingAnalytics += GetKpiShortCode( "% Scheduled", Person.GetAttributeValue( "PercentofGiftsScheduled" ).AsInteger() + "%", icon: "fa-fw fa-percent" );
            var givesAs = "Individual";
            var givesAsIcon = "fa-fw fa-user";
            if ( Person.GivingGroupId.HasValue )
            {
                givesAs = "Family";
                givesAsIcon = "fa-fw fa-users";
            }

            givingAnalytics += GetKpiShortCode( "Gives As", givesAs, icon: givesAsIcon );

            var frequencyLabelAttribute = AttributeCache.Get( Rock.SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_LABEL );
            if ( frequencyLabelAttribute != null )
            {
                var frequencyLabel = frequencyLabelAttribute.FieldType.Field.FormatValue( null, Person.GetAttributeValue( "FrequencyLabel" ), frequencyLabelAttribute.QualifierValues, false );
                givingAnalytics += GetKpiShortCode( "Frequency", frequencyLabel, icon: "fa-fw fa-calendar-alt", textAlign: "left" );
            }

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

                    givingAnalytics += GetKpiShortCode( "Preferred Currency", preferredCurrencyValue.Value, icon: "fa-fw " + iconCssClass );
                }
                else
                {
                    givingAnalytics += GetKpiShortCode( "Preferred Currency", string.Empty, icon: "fa-fw " + iconCssClass );
                }
            }


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

                    givingAnalytics += GetKpiShortCode( "Preferred Source", preferredSourceValue.Value, icon: "fa-fw " + iconCssClass );
                }
                else
                {
                    givingAnalytics += GetKpiShortCode( "Preferred Source", string.Empty, icon: "fa-fw " + iconCssClass );
                }
            }

            var financialTransactionAlertService = new FinancialTransactionAlertService( rockContext );
            var financialTransactionAlertQry = financialTransactionAlertService.Queryable().AsNoTracking()
                .Where( a =>
                    a.PersonAlias.PersonId == Person.Id );
            var financialTransactionGratitudeCount = financialTransactionAlertQry.Where( a => a.FinancialTransactionAlertType.AlertType == AlertType.Gratitude ).Count();
            var financialTransactionFollowupCount = financialTransactionAlertQry.Where( a => a.FinancialTransactionAlertType.AlertType == AlertType.FollowUp ).Count();
            givingAnalytics += GetKpiShortCode(
                "Giving Alerts",
                string.Format( "<span class=\"badge bg-success\">{0}</span><span class=\"badge bg-warning\">{1}</span>", financialTransactionGratitudeCount, financialTransactionFollowupCount ),
                icon: "fa-fw fa-comment-alt" );

            lGivingAnalytics.Text = string.Format( @"{{[kpis size:'lg' columnmin:'220px' iconbackground:'false' columnminmd:'220px' columncount:'4' columncountmd:'3' columncountsm:'2']}}{0}{{[endkpis]}}", givingAnalytics ).ResolveMergeFields( mergeFields );
        }

        private decimal GetContributionByMonth( DateTime date )
        {
            var contributionType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );
            var rockContext = new RockContext();
            var transactionDetailService = new FinancialTransactionDetailService( rockContext );
            var qry = transactionDetailService.Queryable().AsNoTracking()
                .Where( a =>
                    a.Transaction.TransactionTypeValueId == contributionType.Id &&
                    a.Transaction.TransactionDateTime.HasValue );

            qry = qry.Where( t => t.Transaction.AuthorizedPersonAlias.Person.GivingId == Person.GivingId );
            var startDate = date.StartOfMonth();
            var endDate = startDate.AddMonths( 1 );
            return qry
                .Where( a => a.Transaction.TransactionDateTime.Value >= startDate && a.Transaction.TransactionDateTime < endDate )
                .Select( l => l.Amount )
                .DefaultIfEmpty( 0 )
                .Sum();
        }

        private string GetKpiShortCode( string label, string value, string subValue = "", string icon = "", string textAlign = "" )
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
                textAlign = string.Format( "textAlign:'{0}'", textAlign );
            }

            var kpi = string.Format(
                "[[ kpi {3} labellocation:'top' value:'{0}' {2} label:'{1}' {4}]][[ endkpi ]]",
                value,
                label,
                subValue,
                icon,
                textAlign );
            return kpi;
        }

        private void BindYearlySummary()
        {
            var contributionType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );
            if ( contributionType != null )
            {
                var rockContext = new RockContext();
                var transactionDetailService = new FinancialTransactionDetailService( rockContext );
                var qry = transactionDetailService.Queryable().AsNoTracking()
                    .Where( a =>
                        a.Transaction.TransactionTypeValueId == contributionType.Id &&
                        a.Transaction.TransactionDateTime.HasValue );

                qry = qry.Where( t => t.Transaction.AuthorizedPersonAlias.Person.GivingId == Person.GivingId );

                var financialAccounts = new FinancialAccountService( rockContext ).Queryable().Select( a => new { a.Id, a.Name } ).ToDictionary( k => k.Id, v => v.Name );
                List<SummaryRecord> summaryList;
                using ( new Rock.Data.QueryHintScope( rockContext, QueryHintType.RECOMPILE ) )
                {
                    summaryList = qry
                        .GroupBy( a => new { a.Transaction.TransactionDateTime.Value.Year, a.AccountId } )
                        .Select( t => new SummaryRecord
                        {
                            Year = t.Key.Year,
                            AccountId = t.Key.AccountId,
                            TotalAmount = t.Sum( d => d.Amount )
                        } ).OrderByDescending( a => a.Year )
                        .ToList();
                }

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
            }
        }

        private string FormatAsCurrency( decimal value )
        {
            var currencySymbol = GlobalAttributesCache.Value( "CurrencySymbol" );
            return string.Format( "{0}{1:N0}", currencySymbol, value );
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