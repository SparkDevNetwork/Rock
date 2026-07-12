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

using Rock.Attribute;
using Rock.Lava;
using Rock.Model;
using Rock.ViewModels.Blocks.Finance.ScheduledTransactionSummary;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Block that shows a summary of the scheduled transactions for the currently logged in person.
    /// </summary>
    [DisplayName( "Scheduled Transaction Summary" )]
    [Category( "Finance" )]
    [Description( "Block that shows a summary of the scheduled transactions for the currently logged in user." )]

    #region Block Attributes

    [CodeEditorField( "Template",
        Description = "Lava template for the content to be placed on the page.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = @"{% include '~~/Assets/Lava/ScheduledTransactionSummary.lava' %}",
        Key = AttributeKey.Template,
        Order = 0 )]

    [LinkedPage( "Manage Scheduled Transactions Page",
        Description = "Link to be used for managing an individual's scheduled transactions.",
        IsRequired = false,
        Key = AttributeKey.ManageScheduledTransactionsPage,
        Order = 1 )]

    [LinkedPage( "Transaction History Page",
        Description = "Link to use for viewing an individual's transaction history.",
        IsRequired = false,
        Key = AttributeKey.TransactionHistoryPage,
        Order = 2 )]

    [LinkedPage( "Transaction Entry Page",
        Description = "Link to use when adding new transactions.",
        IsRequired = false,
        Key = AttributeKey.TransactionEntryPage,
        Order = 3 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "FCD80A4F-3AE6-483D-B146-FFF4D449401A" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "5083BD6E-E707-4F52-9267-B4C138FBCA9D" )]
    [Rock.SystemGuid.BlockTypeGuid( "3FC83F0E-8BAA-4CB3-BAD0-0CFBE0E621AA" )]
    public class ScheduledTransactionSummary : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string Template = "Template";
            public const string ManageScheduledTransactionsPage = "ManageScheduledTransactionsPage";
            public const string TransactionHistoryPage = "TransactionHistoryPage";
            public const string TransactionEntryPage = "TransactionEntryPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return new ScheduledTransactionSummaryOptionsBag();
        }

        /// <inheritdoc/>
        protected override string GetInitialHtmlContent()
        {
            return RenderContent();
        }

        #endregion Methods

        #region Private Methods

        /// <summary>
        /// Resolves the configured Lava template against the current person's scheduled-giving summary.
        /// </summary>
        /// <returns>The rendered HTML for the block.</returns>
        private string RenderContent()
        {
            var mergeFields = RequestContext.GetCommonMergeFields();

            mergeFields["ScheduledTransactions"] = GetScheduledTransactionSummaries();
            mergeFields["LinkedPages"] = GetLinkedPages();

            // Expose the current person under the legacy "Person" key in addition to the
            // "CurrentPerson" key from the common merge fields so older templates keep working.
            mergeFields["Person"] = RequestContext.CurrentPerson;

            return GetAttributeValue( AttributeKey.Template ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Builds the linked-page URLs exposed to the Lava template.
        /// </summary>
        /// <returns>A dictionary of linked-page URLs keyed by their template merge-field name.</returns>
        private Dictionary<string, object> GetLinkedPages()
        {
            return new Dictionary<string, object>
            {
                ["ManageScheduledTransactionsPage"] = this.GetLinkedPageUrl( AttributeKey.ManageScheduledTransactionsPage ),
                ["TransactionHistoryPage"] = this.GetLinkedPageUrl( AttributeKey.TransactionHistoryPage ),
                ["TransactionEntryPage"] = this.GetLinkedPageUrl( AttributeKey.TransactionEntryPage )
            };
        }

        /// <summary>
        /// Gets the active scheduled-transaction summaries for the current person's giving unit.
        /// </summary>
        /// <returns>The list of schedule summaries, or an empty list when there is no current person or no active schedules.</returns>
        private List<ScheduledTransactionSummaryInfo> GetScheduledTransactionSummaries()
        {
            var currentPerson = RequestContext.CurrentPerson;

            if ( currentPerson == null )
            {
                return new List<ScheduledTransactionSummaryInfo>();
            }

            var transactionService = new FinancialScheduledTransactionService( RockContext );

            var schedules = transactionService
                .Queryable()
                .Include( s => s.ScheduledTransactionDetails.Select( d => d.Account ) )
                .Include( s => s.FinancialPaymentDetail )
                .Include( s => s.FinancialGateway )
                .Where( s => s.AuthorizedPersonAlias.Person.GivingId == currentPerson.GivingId && s.IsActive )
                .ToList();

            if ( schedules.Count == 0 )
            {
                return new List<ScheduledTransactionSummaryInfo>();
            }

            /*
                6/29/26 - MSE

                Each active schedule's status and next-payment date are refreshed directly from the
                payment gateway so the summary reflects the gateway's current state (for example, a
                one-time gift that has already processed is reported back as inactive). The gateway
                reports are then persisted. This is a per-schedule gateway round-trip, so it is run
                once over the already-materialized list rather than re-querying per item.

                Reason: Refresh schedule status/next-payment from the gateway on render, then persist.
            */
            transactionService.GetStatus( schedules, activeOnly: true );
            RockContext.SaveChanges();

            var lastPaymentDates = GetLastPaymentDates( schedules.Select( s => s.Id ).ToList() );
            var now = RockDateTime.Now;

            return schedules
                .Select( schedule => BuildSummary( schedule, lastPaymentDates, now ) )
                .ToList();
        }

        /// <summary>
        /// Gets the most recent transaction date for each of the supplied schedules in a single query.
        /// </summary>
        /// <param name="scheduleIds">The scheduled-transaction identifiers to look up.</param>
        /// <returns>A dictionary of the last payment date keyed by scheduled-transaction identifier.</returns>
        private Dictionary<int, DateTime?> GetLastPaymentDates( List<int> scheduleIds )
        {
            return new FinancialTransactionService( RockContext )
                .Queryable()
                .Where( t => t.ScheduledTransactionId.HasValue
                    && scheduleIds.Contains( t.ScheduledTransactionId.Value )
                    && t.TransactionDateTime.HasValue )
                .GroupBy( t => t.ScheduledTransactionId.Value )
                .Select( g => new { ScheduledTransactionId = g.Key, LastPaymentDate = g.Max( t => t.TransactionDateTime ) } )
                .ToDictionary( x => x.ScheduledTransactionId, x => x.LastPaymentDate );
        }

        /// <summary>
        /// Builds the Lava summary object for a single scheduled transaction.
        /// </summary>
        /// <param name="schedule">The scheduled transaction.</param>
        /// <param name="lastPaymentDates">The pre-fetched last payment dates keyed by schedule Id.</param>
        /// <param name="now">The current Rock date/time used for the day calculations.</param>
        /// <returns>The populated summary object.</returns>
        private ScheduledTransactionSummaryInfo BuildSummary( FinancialScheduledTransaction schedule, IReadOnlyDictionary<int, DateTime?> lastPaymentDates, DateTime now )
        {
            var frequency = DefinedValueCache.Get( schedule.TransactionFrequencyValueId );
            var paymentDetail = schedule.FinancialPaymentDetail;

            lastPaymentDates.TryGetValue( schedule.Id, out var lastPaymentDate );

            return new ScheduledTransactionSummaryInfo
            {
                Id = schedule.Id,
                Guid = schedule.Guid,
                StartDate = schedule.StartDate,
                EndDate = schedule.EndDate,
                NextPaymentDate = schedule.NextPaymentDate,
                DaysTillNextPayment = schedule.NextPaymentDate.HasValue
                    ? ( int? ) ( schedule.NextPaymentDate.Value - now ).Days
                    : null,
                LastPaymentDate = lastPaymentDate,
                DaysSinceLastPayment = lastPaymentDate.HasValue
                    ? ( int? ) ( now - lastPaymentDate.Value ).Days
                    : null,
                CurrencyType = GetDefinedValueText( paymentDetail?.CurrencyTypeValueId ),
                CreditCardType = GetDefinedValueText( paymentDetail?.CreditCardTypeValueId ),
                UrlEncryptedKey = schedule.UrlEncodedKey,
                Frequency = frequency?.Value,
                FrequencyDescription = frequency?.Description,
                ScheduledAmount = schedule.ScheduledTransactionDetails.Sum( d => d.Amount ),
                TransactionDetails = schedule.ScheduledTransactionDetails
                    .Select( d => new ScheduledTransactionDetailInfo
                    {
                        /*
                            6/29/26 - MSE

                            The legacy WebForms block stored the detail row's own Id ( detail.Id )
                            under the "AccountId" merge field, which was inconsistent with the
                            sibling "AccountName" that held the actual account's name. We now expose
                            the real FinancialAccount Id ( d.AccountId ) that the field name implies.
                            This is an intentional divergence from WebForms; any custom Lava template
                            that read the old "AccountId" will now receive the corrected account Id.

                            Reason: Fixed "AccountId" to return the account Id rather than the detail Id.
                        */
                        AccountId = d.AccountId,
                        AccountName = d.Account?.Name,
                        Amount = d.Amount,
                        Summary = d.Summary
                    } )
                    .ToList()
            };
        }

        /// <summary>
        /// Gets the text value of a defined value, or an empty string when the identifier is null or unmatched.
        /// </summary>
        /// <param name="definedValueId">The defined value identifier.</param>
        /// <returns>The defined value's text, or an empty string.</returns>
        private static string GetDefinedValueText( int? definedValueId )
        {
            if ( !definedValueId.HasValue )
            {
                return string.Empty;
            }

            return DefinedValueCache.Get( definedValueId.Value )?.Value ?? string.Empty;
        }

        #endregion Private Methods

        #region Helper Types

        /// <summary>
        /// A Lava-friendly summary of a single scheduled transaction.
        /// </summary>
        private class ScheduledTransactionSummaryInfo : LavaDataObject
        {
            public int Id { get; set; }

            public Guid Guid { get; set; }

            public DateTime StartDate { get; set; }

            public DateTime? EndDate { get; set; }

            public DateTime? NextPaymentDate { get; set; }

            public int? DaysTillNextPayment { get; set; }

            public DateTime? LastPaymentDate { get; set; }

            public int? DaysSinceLastPayment { get; set; }

            public string CurrencyType { get; set; }

            public string CreditCardType { get; set; }

            public string UrlEncryptedKey { get; set; }

            public string Frequency { get; set; }

            public string FrequencyDescription { get; set; }

            public decimal ScheduledAmount { get; set; }

            public List<ScheduledTransactionDetailInfo> TransactionDetails { get; set; }
        }

        /// <summary>
        /// A Lava-friendly summary of a single scheduled-transaction account detail.
        /// </summary>
        private class ScheduledTransactionDetailInfo : LavaDataObject
        {
            public int AccountId { get; set; }

            public string AccountName { get; set; }

            public decimal Amount { get; set; }

            public string Summary { get; set; }
        }

        #endregion Helper Types
    }
}
