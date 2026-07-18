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
using System.Net;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Finance.PublicScheduledTransactionList;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Public-facing block that lists the current person's (and their related
    /// businesses') active scheduled-transaction records as DisplayCards.
    /// Provides Edit / Transfer / Delete actions on each row.
    /// </summary>
    [DisplayName( "Public Scheduled Transaction List" )]
    [Category( "Finance" )]
    [Description( "Shows the current person's scheduled giving profiles, with the ability to edit, transfer, or cancel each one." )]
    [IconCssClass( "ti ti-cash" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    // --- General Settings ---

    [BooleanField( "Show Block Header",
        Key = AttributeKey.ShowBlockHeader,
        Description = "When enabled, displays a title and description at the top of the block.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.GeneralSettings,
        Order = 0 )]

    [LinkedPage(
        "New Scheduled Transaction Page",
        Key = AttributeKey.ScheduledTransactionEntryPage,
        Description = "The page users navigate to when creating a new transaction.",
        IsRequired = false,
        Category = AttributeCategory.GeneralSettings,
        Order = 1 )]

    [FinancialGatewayField(
        "Gateway Filter",
        Key = AttributeKey.GatewayFilter,
        Description = "Displays only transactions from the selected gateway; leave empty to show all.",
        IsRequired = false,
        Category = AttributeCategory.GeneralSettings,
        Order = 2 )]

    [FinancialGatewayField(
        "Gateway for Transfers",
        Key = AttributeKey.TransferToGateway,
        Description = "When set, transactions using a different gateway will show a Transfer button instead of Edit. Combines with Transfer Button Label to customize the button label.",
        IsRequired = false,
        Category = AttributeCategory.GeneralSettings,
        Order = 3 )]

    [LinkedPage(
        "Scheduled Transaction Edit Page",
        Key = AttributeKey.ScheduledTransactionEditPage,
        Description = "The page users navigate to when editing a transaction (for gateways that do not support hosted payment updates).",
        IsRequired = false,
        Category = AttributeCategory.GeneralSettings,
        Order = 4 )]

    [LinkedPage(
        "Scheduled Transaction Edit Page (Hosted)",
        Key = AttributeKey.ScheduledTransactionEditPageHosted,
        Description = "The page users navigate to when editing a transaction (for gateways that support hosted payment updates).",
        IsRequired = false,
        Category = AttributeCategory.GeneralSettings,
        Order = 5 )]

    [DefinedValueField(
        "Transaction Type Filters",
        Key = AttributeKey.TransactionTypes,
        Description = "Show only transactions of the selected type(s); leave empty to show all types.",
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE,
        DefaultValue = Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION,
        AllowMultiple = true,
        Category = AttributeCategory.GeneralSettings,
        Order = 6 )]

    // --- Customize Text ---

    [TextField( "Block Header Title",
        Key = AttributeKey.BlockHeaderTitle,
        Description = "The title displayed at the top of the block.",
        IsRequired = true,
        DefaultValue = "Manage Giving Profiles",
        Category = AttributeCategory.CustomizeText,
        Order = 0 )]

    [TextField( "Block Header Description",
        Key = AttributeKey.BlockHeaderDescription,
        Description = "The supporting text displayed below the header title.",
        IsRequired = false,
        DefaultValue = "Your giving profiles are listed below. Edit a profile to change its frequency, start date, or amount. Delete a profile to stop automated giving, or create a new one anytime.",
        Category = AttributeCategory.CustomizeText,
        Order = 1 )]

    [TextField( "Block Header Icon",
        Key = AttributeKey.BlockHeaderIcon,
        Description = "The CSS class of the icon displayed in the block header (e.g. 'ti ti-cash').",
        IsRequired = false,
        DefaultValue = "ti ti-cash",
        Category = AttributeCategory.CustomizeText,
        Order = 2 )]

    [TextField( "Transaction Label",
        Key = AttributeKey.TransactionLabel,
        Description = "The text that appears as the transaction type label to users (e.g., Gift, Donation).",
        IsRequired = true,
        DefaultValue = "Gift",
        Category = AttributeCategory.CustomizeText,
        Order = 3 )]

    [TextField(
        "Transfer Button Label",
        Key = AttributeKey.TransferButtonText,
        Description = "The button label shown when a transaction requires transfer to a different gateway.",
        IsRequired = true,
        DefaultValue = "Transfer Gateway",
        Category = AttributeCategory.CustomizeText,
        Order = 4 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "769FDD5D-73D2-4C63-BBA3-050BA1E1CD3E" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "A51CA42A-80E0-46B7-BD61-5B33F050856A" )]
    [Rock.SystemGuid.BlockTypeGuid( "081FF29F-0A9F-4EC3-95AD-708FA0E6132D" )]
    public class PublicScheduledTransactionList : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string ShowBlockHeader = "ShowBlockHeader";
            public const string BlockHeaderTitle = "BlockHeaderTitle";
            public const string BlockHeaderDescription = "BlockHeaderDescription";
            public const string BlockHeaderIcon = "BlockHeaderIcon";
            public const string ScheduledTransactionEditPage = "ScheduledTransactionEditPage";
            public const string ScheduledTransactionEditPageHosted = "ScheduledTransactionEditPageHosted";
            public const string ScheduledTransactionEntryPage = "ScheduledTransactionEntryPage";
            public const string TransactionLabel = "TransactionLabel";
            public const string GatewayFilter = "GatewayFilter";
            public const string TransferToGateway = "TransferToGateway";
            public const string TransferButtonText = "TransferButtonText";
            public const string TransactionTypes = "TransactionTypes";
        }

        private static class AttributeCategory
        {
            public const string GeneralSettings = "General Settings";
            public const string CustomizeText = "Customize Text";
        }

        private static class PageParameterKey
        {
            public const string ScheduledTransactionGuid = "ScheduledTransactionGuid";
            public const string Transfer = "transfer";
        }

        private static class NavigationUrlKey
        {
            public const string EntryPage = "EntryPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<PublicScheduledTransactionListBag, PublicScheduledTransactionListOptionsBag>
            {
                Bag = BuildBag(),
                NavigationUrls = GetBoxNavigationUrls()
            };

            return box;
        }

        /// <summary>
        /// Resolves the Add-button target URL from the Scheduled Transaction
        /// Entry Page attribute so the .obs side can navigate on click.
        /// </summary>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.EntryPage] = this.GetLinkedPageUrl( AttributeKey.ScheduledTransactionEntryPage )
            };
        }

        /// <summary>
        /// Builds the top-level bag with the per-row item list, empty-state
        /// flag, and Add-button visibility. Returns an empty bag for anonymous
        /// viewers so the block renders nothing (matching the original behavior).
        /// </summary>
        private PublicScheduledTransactionListBag BuildBag()
        {
            var transactionLabel = GetAttributeValue( AttributeKey.TransactionLabel );
            var entryPageConfigured = GetAttributeValue( AttributeKey.ScheduledTransactionEntryPage ).IsNotNullOrWhiteSpace();

            var bag = new PublicScheduledTransactionListBag
            {
                Items = new List<ScheduledTransactionItemBag>(),
                AddButtonText = $"Create New {transactionLabel}",
                ShowAddButton = entryPageConfigured,
                ShowBlockHeader = GetAttributeValue( AttributeKey.ShowBlockHeader ).AsBoolean(),
                BlockHeaderTitle = GetAttributeValue( AttributeKey.BlockHeaderTitle ),
                BlockHeaderDescription = GetAttributeValue( AttributeKey.BlockHeaderDescription ),
                BlockHeaderIconCssClass = GetAttributeValue( AttributeKey.BlockHeaderIcon )
            };

            if ( RequestContext.CurrentPerson == null )
            {
                // Anonymous viewers see nothing: no items, no empty-state alert,
                // no Add button. Matches the original WebForms behavior.
                bag.ShowAddButton = false;
                bag.ShowBlockHeader = false;
                return bag;
            }

            var schedules = LoadSchedulesForCurrentPerson();

            if ( schedules.Count == 0 )
            {
                bag.IsEmpty = true;
                bag.EmptyMessage = $"No {transactionLabel.Pluralize().ToLower()} currently exist.";
                return bag;
            }

            var lastPaymentDates = GetLastPaymentDates( schedules.Select( s => s.Id ) );

            var transferToGatewayGuid = GetAttributeValue( AttributeKey.TransferToGateway ).AsGuidOrNull();
            var eventRegistrationTransactionTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_EVENT_REGISTRATION.AsGuid() );

            foreach ( var transactionSchedule in schedules )
            {
                bag.Items.Add( BuildItemBag(
                    transactionSchedule,
                    lastPaymentDates,
                    transferToGatewayGuid,
                    eventRegistrationTransactionTypeValueId ) );
            }

            return bag;
        }

        /// <summary>
        /// Loads the active scheduled transactions belonging to the current
        /// person (and any businesses they own), applying the optional gateway
        /// and transaction-type filters. Calls GetStatus to refresh gateway
        /// state for each schedule before returning.
        /// </summary>
        private List<FinancialScheduledTransaction> LoadSchedulesForCurrentPerson()
        {
            var transactionService = new FinancialScheduledTransactionService( RockContext );
            var personService = new PersonService( RockContext );

            // Collect the giving IDs we want to match: the current person's
            // own giving ID plus the giving ID of every business they own.
            // Matches historical schedules attached to either form (G{n} or P{n}).
            var givingIds = personService.GetBusinesses( RequestContext.CurrentPerson.Id )
                .Select( g => g.GivingId )
                .ToList();
            givingIds.Add( RequestContext.CurrentPerson.GivingId );

            var schedules = transactionService.Queryable()
                .Include( a => a.TransactionTypeValue )
                .Include( a => a.TransactionFrequencyValue )
                .Include( a => a.FinancialGateway )
                .Include( a => a.FinancialPaymentDetail.CurrencyTypeValue )
                .Include( a => a.FinancialPaymentDetail.CreditCardTypeValue )
                .Include( a => a.AuthorizedPersonAlias.Person )
                .Include( a => a.ScheduledTransactionDetails.Select( s => s.Account ) )
                .Where( s => givingIds.Contains( s.AuthorizedPersonAlias.Person.GivingId ) && s.IsActive == true );

            var gatewayFilterGuid = GetAttributeValue( AttributeKey.GatewayFilter ).AsGuidOrNull();
            if ( gatewayFilterGuid.HasValue )
            {
                schedules = schedules.Where( s => s.FinancialGateway.Guid == gatewayFilterGuid.Value );
            }

            // Transaction-type filter intentionally includes schedules whose
            // TransactionTypeValueId is null so legacy rows keep rendering.
            var transactionTypeGuids = GetTransactionTypesFilter();
            schedules = schedules.Where( s => !s.TransactionTypeValueId.HasValue || transactionTypeGuids.Contains( s.TransactionTypeValue.Guid ) );

            var scheduleList = schedules.ToList();

            // Refresh gateway status (NextPaymentDate / IsActive) for each
            // active schedule. Hits the gateway once per row; preserved from
            // the original block to keep the card-expired / status badges
            // current with what the gateway shows.
            transactionService.GetStatus( scheduleList, true );

            return scheduleList;
        }

        /// <summary>
        /// Computes the most-recent transaction date for each scheduled
        /// transaction in a single batched query rather than a per-row lazy load.
        /// </summary>
        private Dictionary<int, DateTime?> GetLastPaymentDates( IEnumerable<int> scheduleIds )
        {
            var ids = scheduleIds.ToList();

            if ( ids.Count == 0 )
            {
                return new Dictionary<int, DateTime?>();
            }

            return new FinancialTransactionService( RockContext ).Queryable()
                .Where( t => t.ScheduledTransactionId.HasValue
                    && ids.Contains( t.ScheduledTransactionId.Value )
                    && t.TransactionDateTime.HasValue )
                .GroupBy( t => t.ScheduledTransactionId.Value )
                .Select( g => new
                {
                    ScheduledTransactionId = g.Key,
                    LastPaymentDate = g.Max( t => t.TransactionDateTime )
                } )
                .ToDictionary( x => x.ScheduledTransactionId, x => x.LastPaymentDate );
        }

        /// <summary>
        /// Returns the configured transaction-type defined-value guids,
        /// filtered to those still present in the DefinedValueCache.
        /// </summary>
        private List<Guid> GetTransactionTypesFilter()
        {
            return this.GetAttributeValues( AttributeKey.TransactionTypes )
                .AsGuidList()
                .Where( guid => DefinedValueCache.Get( guid ) != null )
                .ToList();
        }

        /// <summary>
        /// Builds the DisplayCard-friendly per-row bag: icon, title,
        /// description, next-payment line, person name, frequency pill,
        /// edit-button visibility + resolved URL.
        /// </summary>
        private ScheduledTransactionItemBag BuildItemBag(
            FinancialScheduledTransaction transactionSchedule,
            Dictionary<int, DateTime?> lastPaymentDates,
            Guid? transferToGatewayGuid,
            int? eventRegistrationTransactionTypeValueId )
        {
            // Determine whether this row should route through the Transfer
            // flow (i.e., the configured transfer-to gateway differs from
            // the schedule's current gateway).
            var isTransfer = transferToGatewayGuid.HasValue
                && transactionSchedule.FinancialGateway != null
                && transactionSchedule.FinancialGateway.Guid != transferToGatewayGuid.Value;

            // Whether the schedule's gateway supports a hosted edit page.
            var hostedGatewayComponent = transactionSchedule.FinancialGateway?.GetGatewayComponent() as IHostedGatewayComponent;
            var useHostedGatewayEditPage = hostedGatewayComponent != null
                && hostedGatewayComponent.GetSupportedHostedGatewayModes( transactionSchedule.FinancialGateway ).Contains( HostedGatewayMode.Hosted );

            // Event-registration schedules are read-only from this block.
            var isEventRegistration = eventRegistrationTransactionTypeValueId.HasValue
                && eventRegistrationTransactionTypeValueId == transactionSchedule.TransactionTypeValueId;

            bool showEditButton;
            if ( isEventRegistration )
            {
                showEditButton = false;
            }
            else if ( useHostedGatewayEditPage )
            {
                showEditButton = GetAttributeValue( AttributeKey.ScheduledTransactionEditPageHosted ).IsNotNullOrWhiteSpace();
            }
            else
            {
                showEditButton = GetAttributeValue( AttributeKey.ScheduledTransactionEditPage ).IsNotNullOrWhiteSpace();
            }

            var editButtonText = isTransfer
                ? GetAttributeValue( AttributeKey.TransferButtonText )
                : "Edit";

            // Transfer rows swap the pencil icon for the left-right arrows
            // icon per the Figma design, so users can tell the button will
            // move the schedule to a different gateway rather than open the
            // standard edit page.
            var editIconCssClass = isTransfer
                ? "ti ti-arrows-left-right"
                : "ti ti-pencil";

            var editUrl = ResolveEditUrl( transactionSchedule, isTransfer, useHostedGatewayEditPage );

            var totalAmount = transactionSchedule.ScheduledTransactionDetails?.Sum( d => d.Amount ) ?? 0m;
            var transactionType = transactionSchedule.TransactionTypeValue?.Value;
            var title = transactionType.IsNotNullOrWhiteSpace()
                ? $"{totalAmount:C} ({transactionType})"
                : $"{totalAmount:C}";

            var ( frequencyIconCssClass, frequencyLabel ) = ResolveFrequency( transactionSchedule );

            return new ScheduledTransactionItemBag
            {
                IdKey = transactionSchedule.IdKey,
                IconCssClass = ResolvePaymentIconCssClass( transactionSchedule ),
                Title = title,
                Description = BuildPaymentDescription( transactionSchedule ),
                NextPaymentText = BuildNextPaymentText( transactionSchedule ),
                PersonName = transactionSchedule.AuthorizedPersonAlias?.Person?.FullName ?? string.Empty,
                FrequencyIconCssClass = frequencyIconCssClass,
                FrequencyLabel = frequencyLabel,
                ShowEditButton = showEditButton,
                EditButtonText = editButtonText,
                EditIconCssClass = editIconCssClass,
                EditUrl = editUrl
            };
        }

        /// <summary>
        /// Picks the DisplayCard icon based on payment currency type: credit
        /// card gets a card icon, everything else (ACH, check, cash) gets a
        /// bank icon.
        /// </summary>
        private static string ResolvePaymentIconCssClass( FinancialScheduledTransaction transactionSchedule )
        {
            var currencyTypeGuid = transactionSchedule.FinancialPaymentDetail?.CurrencyTypeValue?.Guid;
            var creditCardGuid = Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid();

            return currencyTypeGuid.HasValue && currencyTypeGuid.Value == creditCardGuid
                ? "ti ti-credit-card"
                : "ti ti-building-bank";
        }

        /// <summary>
        /// Builds the DisplayCard description line: e.g.
        /// "Visa Ending in 6789 • Expires 11/28" for credit cards,
        /// "Checking Account Ending in 2121" for ACH.
        /// </summary>
        private static string BuildPaymentDescription( FinancialScheduledTransaction transactionSchedule )
        {
            var paymentDetail = transactionSchedule.FinancialPaymentDetail;
            if ( paymentDetail == null )
            {
                return string.Empty;
            }

            var parts = new List<string>();
            var currencyType = paymentDetail.CurrencyTypeValue?.Value;
            var creditCardType = paymentDetail.CreditCardTypeValue?.Value;
            var accountMasked = paymentDetail.AccountNumberMasked;
            var lastFour = ExtractLastFour( accountMasked );

            if ( string.Equals( currencyType, "Credit Card", StringComparison.OrdinalIgnoreCase ) )
            {
                var cardBrand = creditCardType.IsNotNullOrWhiteSpace() ? creditCardType : "Credit Card";
                parts.Add( lastFour.IsNotNullOrWhiteSpace() ? $"{cardBrand} Ending in {lastFour}" : cardBrand );
            }
            else if ( currencyType.IsNotNullOrWhiteSpace() )
            {
                parts.Add( lastFour.IsNotNullOrWhiteSpace() ? $"{currencyType} Ending in {lastFour}" : currencyType );
            }

            // Only credit cards carry expiration; skip for ACH / cash / check.
            if ( string.Equals( currencyType, "Credit Card", StringComparison.OrdinalIgnoreCase ) )
            {
                var expirationDate = paymentDetail.CardExpirationDate;
                if ( expirationDate.HasValue )
                {
                    parts.Add( $"Expires {expirationDate.Value:MM/yy}" );

                    if ( expirationDate.Value < RockDateTime.Now )
                    {
                        parts.Add( "Card Expired" );
                    }
                }
            }

            return string.Join( " • ", parts );
        }

        /// <summary>
        /// Returns the trailing digit sequence from a masked account number
        /// (e.g. "************1234" or "411111******1111" → "1234" or "1111").
        /// Returns empty when no trailing digits are present.
        /// </summary>
        private static string ExtractLastFour( string accountMasked )
        {
            if ( accountMasked.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            var trailing = new string( accountMasked
                .Reverse()
                .TakeWhile( c => c != '*' )
                .Reverse()
                .ToArray() );

            return trailing;
        }

        /// <summary>
        /// Formats the next-payment date as "Next on {MMMM d, yyyy}", or null
        /// when the schedule has no NextPaymentDate.
        /// </summary>
        private static string BuildNextPaymentText( FinancialScheduledTransaction transactionSchedule )
        {
            return transactionSchedule.NextPaymentDate.HasValue
                ? $"Next on {transactionSchedule.NextPaymentDate.Value:MMMM d, yyyy}"
                : null;
        }

        /// <summary>
        /// Chooses an icon + label pair for the frequency pill. One-time
        /// transactions get a gift icon; recurring gets a refresh icon.
        /// </summary>
        private static (string iconCssClass, string label) ResolveFrequency( FinancialScheduledTransaction transactionSchedule )
        {
            var label = transactionSchedule.TransactionFrequencyValue?.Value ?? string.Empty;

            if ( string.Equals( label, "One-Time", StringComparison.OrdinalIgnoreCase ) )
            {
                return ( "ti ti-gift", "One-Time" );
            }

            return ( "ti ti-refresh", label );
        }

        /// <summary>
        /// Resolves the Edit-button URL for a row, applying the same routing
        /// rules the original block used at click time: a Transfer routes to
        /// the entry page with transfer=true; otherwise hosted-gateway support
        /// picks between the hosted and classic edit pages.
        /// </summary>
        private string ResolveEditUrl( FinancialScheduledTransaction transactionSchedule, bool isTransfer, bool useHostedGatewayEditPage )
        {
            var queryParams = new Dictionary<string, string>
            {
                [PageParameterKey.ScheduledTransactionGuid] = transactionSchedule.Guid.ToString()
            };

            if ( isTransfer && GetAttributeValue( AttributeKey.ScheduledTransactionEntryPage ).IsNotNullOrWhiteSpace() )
            {
                queryParams[PageParameterKey.Transfer] = "true";
                return this.GetLinkedPageUrl( AttributeKey.ScheduledTransactionEntryPage, queryParams );
            }

            return useHostedGatewayEditPage
                ? this.GetLinkedPageUrl( AttributeKey.ScheduledTransactionEditPageHosted, queryParams )
                : this.GetLinkedPageUrl( AttributeKey.ScheduledTransactionEditPage, queryParams );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Inactivates the specified scheduled transaction at the gateway and
        /// returns a replacement bag whose AlertMessage / AlertType are set so
        /// the client can swap the row's DisplayCard for an inline alert.
        /// </summary>
        /// <param name="idKey">The hashed identifier of the scheduled transaction to cancel.</param>
        [BlockAction]
        public BlockActionResult CancelScheduledTransaction( string idKey )
        {
            /* 2021-08-27 MDP

            We really don't want to actually delete a FinancialScheduledTransaction.
            Just inactivate it, even if there aren't FinancialTransactions associated with it.
            It is possible the the Gateway has processed a transaction on it that Rock doesn't know about yet.
            If that happens, Rock won't be able to match a record for that downloaded transaction!
            We also might want to match inactive or "deleted" schedules on the Gateway to a person in Rock,
            so we'll need the ScheduledTransaction to do that.

            So, don't delete ScheduledTransactions.

            */

            var scheduledTransactionId = IdHasher.Instance.GetId( idKey ) ?? 0;

            var fstService = new FinancialScheduledTransactionService( RockContext );
            var currentTransaction = fstService.Get( scheduledTransactionId );
            if ( currentTransaction != null && currentTransaction.FinancialGateway != null )
            {
                currentTransaction.FinancialGateway.LoadAttributes( RockContext );
            }

            var transactionLabel = GetAttributeValue( AttributeKey.TransactionLabel );

            string errorMessage = string.Empty;
            var replacement = new ScheduledTransactionItemBag
            {
                IdKey = idKey
            };

            if ( fstService.Cancel( currentTransaction, out errorMessage ) )
            {
                try
                {
                    fstService.GetStatus( currentTransaction, out errorMessage );
                }
                catch
                {
                    // Ignore
                }

                RockContext.SaveChanges();

                replacement.AlertMessage = $"Your scheduled {transactionLabel.ToLower()} has been deleted.";
                replacement.AlertType = "success";
            }
            else
            {
                replacement.AlertMessage = $"An error occurred while deleting your scheduled transaction. Message: {errorMessage}";
                replacement.AlertType = "danger";
            }

            return ActionOk( replacement );
        }

        #endregion Block Actions
    }
}
