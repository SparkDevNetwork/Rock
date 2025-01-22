using Rock.Attribute;
using Rock.Model;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using Rock.Financial;
using Rock.Web.Cache;
using Rock.Data;
using System;
using Rock.Web.UI.Controls;
using System.Threading.Tasks;
using Rock.Tasks;
using Rock.Bus.Message;
using Rock.ClientService.Finance.FinancialPersonSavedAccount.Options;
using Rock.ClientService.Finance.FinancialPersonSavedAccount;
using MassTransit;
using Rock.Common.Mobile.Blocks.Finance.Giving;
using Rock.Common.Mobile.ViewModel;
using Rock.Web.UI;

namespace Rock.Blocks.Types.Mobile.Finance
{
    /// <summary>
    /// The native giving block.
    /// </summary>
    /// <seealso cref="RockBlockType" />
    [DisplayName( "Giving" )]
    [Category( "Mobile > Finance" )]
    [Description( "Allows an individual to give. Apple and Google Pay are supported." )]
    [IconCssClass( "fa fa-hand-holding-heart" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    // Transaction Settings

    [BooleanField(
        "Enable ACH",
        Description = "Determines if adding an ACH payment method and processing a transaction with an ACH payment method is enabled.",
        Key = AttributeKey.EnableACH,
        Order = 0 )]

    [BooleanField(
        "Enable Credit Card",
        Description = "Determines if adding a credit card payment method and processing a transaction with a credit card payment method is enabled.",
        Key = AttributeKey.EnableCreditCard,
        DefaultBooleanValue = true,
        Order = 1 )]

    [BooleanField(
        "Enable Fee Coverage",
        Description = "Determines if the fee coverage feature is enabled or not.",
        Key = AttributeKey.EnableFeeCoverage,
        DefaultBooleanValue = false,
        Order = 2 )]

    [AccountsField(
        "Accounts",
        Key = AttributeKey.Accounts,
        Description = "The accounts to display. If the account has a child account for the selected campus, the child account for that campus will be used.",
        Order = 3 )]

    [BooleanField(
        "Enable Multi-Account",
        Key = AttributeKey.EnableMultiAccount,
        Description = "Should the person be able specify amounts for more than one account?",
        DefaultBooleanValue = true,
        Order = 4 )]

    [BooleanField( "Scheduled Transactions",
        Key = AttributeKey.AllowScheduled,
        Description = "If the selected gateway(s) allow scheduled transactions, should that option be provided to user. This feature is not compatible when Text-to-Give mode is enabled.",
        TrueText = "Allow",
        FalseText = "Don't Allow",
        DefaultBooleanValue = true,
        Order = 5 )]

    [LinkedPage( "Transaction List Page",
        Description = "The page to link to when an individual wants to view their transaction history.",
        Key = AttributeKey.TransactionListPage,
        IsRequired = false,
        Order = 6 )]

    [LinkedPage( "Scheduled Transaction List Page",
        Description = "The page to link to when an individual wants to view their scheduled transactions.",
        Key = AttributeKey.ScheduledTransactionListPage,
        IsRequired = false,
        Order = 7 )]

    [LinkedPage( "Saved Account List Page",
        Description = "The page to link to when an individual wants to view their payment methods.",
        Key = AttributeKey.SavedAccountListPage,
        IsRequired = false,
        Order = 8 )]

    // Person Settings

    [DefinedValueField( "Connection Status",
        Key = AttributeKey.ConnectionStatus,
        Category = AttributeCategoryKey.PersonSettings,
        Description = "The connection status to use for new individuals (default: 'Prospect'.)",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS,
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PROSPECT,
        Order = 0 )]

    [DefinedValueField( "Record Status",
        Key = AttributeKey.RecordStatus,
        Category = AttributeCategoryKey.PersonSettings,
        Description = "The record status to use for new individuals (default: 'Pending'.)",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS,
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING,
        Order = 1 )]

    [GroupLocationTypeField( "Address Type",
        Key = AttributeKey.AddressType,
        Category = AttributeCategoryKey.PersonSettings,
        Description = "The location type to use for the person's address.",
        GroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY,
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME,
        Order = 2 )]

    // Campus Settings

    [BooleanField(
        "Ask for Campus if Known",
        Key = AttributeKey.AskForCampusIfKnown,
        Category = AttributeCategoryKey.CampusSettings,
        Description = "If the campus for the person is already known, should the campus still be prompted for?",
        DefaultBooleanValue = true,
        Order = 0 )]

    [BooleanField(
        "Include Inactive Campuses",
        Key = AttributeKey.IncludeInactiveCampuses,
        Category = AttributeCategoryKey.CampusSettings,
        Description = "Set this to true to include inactive campuses",
        DefaultBooleanValue = false,
        Order = 1 )]

    [DefinedValueField(
        "Campus Types",
        Key = AttributeKey.IncludedCampusTypes,
        Category = AttributeCategoryKey.CampusSettings,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_TYPE,
        AllowMultiple = true,
        IsRequired = false,
        Description = "Set this to limit campuses by campus type.",
        Order = 2 )]

    [DefinedValueField(
        "Campus Statuses",
        Key = AttributeKey.IncludedCampusStatuses,
        Category = AttributeCategoryKey.CampusSettings,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_STATUS,
        AllowMultiple = true,
        IsRequired = false,
        Description = "Set this to limit campuses by campus status.",
        Order = 3 )]

    [BooleanField(
        "Use Account Campus Mapping Logic",
        Description = @"If enabled, the accounts will be determined as follows:
        <ul>
          <li>If the selected account is not associated with a campus, the Selected Account will be the first matching active child account that is associated with the selected campus.</li>
          <li>If the selected account is not associated with a campus, but there are no active child accounts for the selected campus, the parent account (the one the user sees) will be returned.</li>
          <li>If the selected account is associated with a campus, that account will be returned regardless of campus selection (and it won't use the child account logic)</li>
        <ul>",
        Key = AttributeKey.UseAccountCampusMappingLogic,
        DefaultBooleanValue = false,
        Order = 4 )]

    // Communication Settings

    [SystemCommunicationField( "Receipt Email",
        Key = AttributeKey.ReceiptEmail,
        Category = AttributeCategoryKey.CommunicationSettings,
        Description = "The system email to use to send the receipt.",
        IsRequired = false,
        Order = 0 )]

    [CodeEditorField( "Success Template",
        Key = AttributeKey.SuccessTemplate,
        Category = AttributeCategoryKey.CommunicationSettings,
        Description = "The template to display when a transaction is successful.",
        DefaultValue = DefaultSuccessTemplate,
        EditorHeight = 400,
        EditorMode = CodeEditorMode.Lava,
        Order = 1 )]

    // Advanced Settings

    [DefinedValueField( "Transaction Type",
        Key = AttributeKey.TransactionType,
        Description = "",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE,
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION,
        Category = AttributeCategoryKey.Advanced,
        Order = 0 )]

    [TextField( "Batch Name Prefix",
        Key = AttributeKey.BatchNamePrefix,
        Description = "The batch prefix name to use when creating a new batch.",
        IsRequired = false,
        DefaultValue = "Online Giving",
        Category = AttributeCategoryKey.Advanced,
        Order = 1 )]

    [CustomDropdownListField( "Account Campus Context",
        Key = AttributeKey.AccountCampusContext,
        Description = "Should any context be applied to the Account List",
        ListSource = "-1^No Account Campus Context Filter Applied,0^Only Accounts with Current Campus Context,1^Accounts with No Campus and Current Campus Context",
        IsRequired = false,
        DefaultValue = "-1",
        Category = AttributeCategoryKey.Advanced,
        Order = 2 )]

    #endregion

    [SystemGuid.EntityTypeGuid( "a309c830-d373-4244-a8dc-7a69f9e263be" )]
    [SystemGuid.BlockTypeGuid( "ae11559b-03c0-42ce-b8b5-ce9c1027e650" )]
    [ContextAware( typeof( Campus ) )]
    public class Giving : RockBlockType
    {
        #region Constants

        /// <summary>
        /// The attribute category keys for the block.
        /// </summary>
        private static class AttributeCategoryKey
        {
            public const string PersonSettings = "Person Records";
            public const string CampusSettings = "Campus Settings";
            public const string CommunicationSettings = "Communication Settings";
            public const string Advanced = "Advanced";
        }

        /// <summary>
        /// The attribute keys for the block.
        /// </summary>
        private static class AttributeKey
        {
            public const string Accounts = "Accounts";
            public const string EnableACH = "EnableACH";
            public const string EnableCreditCard = "EnableCreditCard";
            public const string ConnectionStatus = "ConnectionStatus";
            public const string RecordStatus = "RecordStatus";
            public const string AddressType = "AddressType";
            public const string TransactionType = "TransactionType";
            public const string BatchNamePrefix = "BatchNamePrefix";
            public const string ReceiptEmail = "ReceiptEmail";
            public const string AllowScheduled = "AllowScheduled";
            public const string EnableFeeCoverage = "EnableFeeCoverage";
            public const string AskForCampusIfKnown = "AskForCampusIfKnown";
            public const string IncludeInactiveCampuses = "IncludeInactiveCampuses";
            public const string IncludedCampusTypes = "IncludedCampusTypes";
            public const string IncludedCampusStatuses = "IncludedCampusStatuses";
            public const string UseAccountCampusMappingLogic = "UseAccountCampusMappingLogic";
            public const string SuccessTemplate = "SuccessTemplate";
            public const string EnableMultiAccount = "EnableMultiAccount";
            public const string AccountCampusContext = "AccountCampusContext";
            public const string TransactionListPage = "TransactionListPage";
            public const string ScheduledTransactionListPage = "ScheduledTransactionListPage";
            public const string SavedAccountListPage = "SavedAccountListPage";
        }

        /// <summary>
        /// The GUID of the MyWell gateway.
        /// </summary>
        private const string MyWellGatewayGuid = "c55f91ac-07f6-484b-b2ff-6ee7d82d7e93";

        #endregion

        #region Fields

        private FinancialGateway _financialGateway;
        private IHostedGatewayComponent _financialGatewayComponent;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the financial gateway.
        /// </summary>
        private FinancialGateway FinancialGateway
        {
            get
            {
                if ( _financialGateway == null )
                {
                    _financialGateway = new FinancialGatewayService( RockContext ).GetNoTracking( MyWellGatewayGuid, false );
                }

                return _financialGateway;
            }
        }

        /// <summary>
        /// Gets the financial gateway component.
        /// </summary>
        private IHostedGatewayComponent FinancialGatewayComponent
        {
            get
            {
                if ( _financialGatewayComponent == null && FinancialGateway != null )
                {
                    _financialGatewayComponent = FinancialGateway.GetGatewayComponent() as IHostedGatewayComponent;
                }

                return _financialGatewayComponent;
            }
        }

        /// <summary>
        /// Whether or not ACH is enabled.
        /// </summary>
        protected bool EnableAch => GetAttributeValue( AttributeKey.EnableACH ).AsBoolean();

        /// <summary>
        /// Whether or not credit card is enabled.
        /// </summary>
        protected bool EnableCreditCard => GetAttributeValue( AttributeKey.EnableCreditCard ).AsBoolean();

        /// <summary>
        /// Whether or not fee coverage is enabled.
        /// </summary>
        protected bool EnableFeeCoverage => GetAttributeValue( AttributeKey.EnableFeeCoverage ).AsBoolean();

        /// <summary>
        /// Gets a list of the selected accounts. The results will be filtered to this list.
        /// </summary>
        protected List<Guid> Accounts => GetAttributeValue( AttributeKey.Accounts ).SplitDelimitedValues().AsGuidList();

        /// <summary>
        /// Whether or not multiple accounts can be selected.
        /// </summary>
        protected bool EnableMultiAccount => GetAttributeValue( AttributeKey.EnableMultiAccount ).AsBoolean();

        /// <summary>
        /// Whether or not to allow scheduled transactions.
        /// </summary>
        protected bool AllowScheduled => GetAttributeValue( AttributeKey.AllowScheduled ).AsBoolean();

        /// <summary>
        /// Gets the transaction type for the block.
        /// </summary>
        protected DefinedValueCache TransactionType => DefinedValueCache.Get( GetAttributeValue( AttributeKey.TransactionType ).AsGuidOrNull() ?? Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );

        /// <summary>
        /// Gets the batch name prefix for the block.
        /// </summary>
        protected string BatchNamePrefix => GetAttributeValue( AttributeKey.BatchNamePrefix );

        /// <summary>
        /// Whether or not to ask for the campus if it is known.
        /// </summary>
        protected bool AskForCampusIfKnown => GetAttributeValue( AttributeKey.AskForCampusIfKnown ).AsBoolean();

        /// <summary>
        /// Whether or not to include inactive campuses.
        /// </summary>
        protected bool IncludeInactiveCampuses => GetAttributeValue( AttributeKey.IncludeInactiveCampuses ).AsBoolean();

        /// <summary>
        /// The campus types to include.
        /// </summary>
        protected List<Guid> IncludedCampusTypes => GetAttributeValue( AttributeKey.IncludedCampusTypes ).SplitDelimitedValues().AsGuidList();

        /// <summary>
        /// The campus statuses to include.
        /// </summary>
        protected List<Guid> IncludedCampusStatuses => GetAttributeValue( AttributeKey.IncludedCampusStatuses ).SplitDelimitedValues().AsGuidList();

        /// <summary>
        /// Whether or not to use account campus mapping logic.
        /// </summary>
        protected bool UseAccountCampusMappingLogic => GetAttributeValue( AttributeKey.UseAccountCampusMappingLogic ).AsBoolean();

        /// <summary>
        /// The connection status to use for new individuals.
        /// </summary>
        protected DefinedValueCache ConnectionStatus => DefinedValueCache.Get( GetAttributeValue( AttributeKey.ConnectionStatus ).AsGuidOrNull() ?? Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PROSPECT.AsGuid() );

        /// <summary>
        /// The record status to use for new individuals.
        /// </summary>
        protected DefinedValueCache RecordStatus => DefinedValueCache.Get( GetAttributeValue( AttributeKey.RecordStatus ).AsGuidOrNull() ?? Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING.AsGuid() );

        /// <summary>
        /// The address type to use for the person's address.
        /// </summary>
        protected DefinedValueCache AddressType => DefinedValueCache.Get( GetAttributeValue( AttributeKey.AddressType ).AsGuidOrNull() ?? Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );

        /// <summary>
        /// The receipt email to use.
        /// </summary>
        private DefinedValueCache _receiptEmail;

        /// <summary>
        /// Gets the receipt email to use.
        /// </summary>
        protected DefinedValueCache ReceiptEmail
        {
            get
            {
                if ( _receiptEmail == null )
                {
                    var receiptEmailDefinedValueGuid = GetAttributeValue( AttributeKey.ReceiptEmail ).AsGuidOrNull();

                    if ( receiptEmailDefinedValueGuid.HasValue )
                    {
                        _receiptEmail = DefinedValueCache.Get( receiptEmailDefinedValueGuid.Value );
                    }
                }

                return _receiptEmail;
            }
        }

        /// <summary>
        /// The success template to use.
        /// </summary>
        protected string SuccessTemplate => GetAttributeValue( AttributeKey.SuccessTemplate );

        /// <summary>
        /// Gets the account campus context value.
        /// </summary>
        protected int? AccountCampusContextFilter => GetAttributeValue( AttributeKey.AccountCampusContext ).AsIntegerOrNull();

        /// <summary>
        /// Gets the transaction list page identifier.
        /// </summary>
        protected Guid? TransactionListPageGuid => GetAttributeValue( AttributeKey.TransactionListPage ).AsGuidOrNull();

        /// <summary>
        /// Gets the scheduled transaction list page identifier.
        /// </summary>
        protected Guid? ScheduledTransactionListPageGuid => GetAttributeValue( AttributeKey.ScheduledTransactionListPage ).AsGuidOrNull();

        /// <summary>
        /// Gets the saved account list page identifier.
        /// </summary>
        protected Guid? SavedAccountListPageGuid => GetAttributeValue( AttributeKey.SavedAccountListPage ).AsGuidOrNull();

        #endregion

        #region Methods

        /// <summary>
        /// Gets the credit card fee coverage percentage.
        /// </summary>
        /// <returns></returns>
        private decimal? GetCreditCardFeeCoveragePercentage()
        {
            if ( FinancialGatewayComponent is IFeeCoverageGatewayComponent feeCoverageGatewayComponent )
            {
                return feeCoverageGatewayComponent.GetCreditCardFeeCoveragePercentage( FinancialGateway );
            }

            return null;
        }

        /// <summary>
        /// Gets the ACH fee coverage amount.
        /// </summary>
        /// <returns></returns>
        private decimal? GetACHFeeCoverageAmount()
        {
            if ( FinancialGatewayComponent is IFeeCoverageGatewayComponent feeCoverageGatewayComponent )
            {
                return feeCoverageGatewayComponent.GetACHFeeCoverageAmount( FinancialGateway );
            }

            return null;
        }

        /// <summary>
        /// Gets the allowed currency types supported by both the block and the
        /// financial gateway.
        /// </summary>
        /// <param name="enableACH">Whether or not ACH is enabled=</param>
        /// <param name="enableCreditCard">Whether or not credit card is enabled=</param>
        /// <param name="gatewayComponent">The gateway component that must support the currency types.</param>
        /// <returns>A list of <see cref="DefinedValueCache"/> objects that represent the currency types.</returns>
        private static List<DefinedValueCache> GetAllowedCurrencyTypes( bool enableACH, bool enableCreditCard, IHostedGatewayComponent gatewayComponent )
        {
            var creditCardCurrency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid() );
            var achCurrency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH.AsGuid() );
            var allowedCurrencyTypes = new List<DefinedValueCache>();

            // Conditionally enable credit card.
            if ( enableCreditCard && gatewayComponent.SupportsSavedAccount( creditCardCurrency ) )
            {
                allowedCurrencyTypes.Add( creditCardCurrency );
            }

            // Conditionally enable ACH.
            if ( enableACH && gatewayComponent.SupportsSavedAccount( achCurrency ) )
            {
                allowedCurrencyTypes.Add( achCurrency );
            }

            return allowedCurrencyTypes;
        }

        /// <summary>
        /// Gets a list of the supported frequencies for the current financial gateway.
        /// </summary>
        /// <returns></returns>
        private List<DefinedValueCache> GetSupportedFrequencies()
        {
            var supportedFrequencies = new List<DefinedValueCache>();

            if ( AllowScheduled && this.FinancialGatewayComponent != null )
            {
                supportedFrequencies = this.FinancialGatewayComponent.SupportedPaymentSchedules;

                // We want to remove the one-time frequency if it is supported.
                // In mobile, we are opting for the flow that if someone wants to
                // give a one-time gift, they will just enter the amount and give
                // with "Set up recurring" unchecked.
                var oneTimeFrequency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME );
                if ( supportedFrequencies.Where( f => f.Value == oneTimeFrequency.IdKey ).Any() )
                {
                    // Remove the one-time frequency.
                    supportedFrequencies = supportedFrequencies.Where( f => f.Value != oneTimeFrequency.IdKey ).ToList();
                }
            }

            return supportedFrequencies;
        }

        /// <summary>
        /// Retrieves the campuses that are configured for the block.
        /// </summary>
        /// <returns></returns>
        private List<CampusCache> GetConfiguredCampuses()
        {
            var campusList = CampusCache.All( this.IncludeInactiveCampuses );
            if ( IncludedCampusTypes?.Any() == true )
            {
                var includedCampusIds = IncludedCampusTypes.Select( a => CampusCache.GetId( a ) ).ToList();

                campusList = campusList.Where( a => a.CampusTypeValueId.HasValue && includedCampusIds.Contains( a.CampusTypeValueId.Value ) ).ToList();
            }

            if ( IncludedCampusStatuses?.Any() == true )
            {
                var includedCampusStatusIds = IncludedCampusStatuses.Select( a => CampusCache.GetId( a ) ).ToList();

                campusList = campusList.Where( a => a.CampusStatusValueId.HasValue && includedCampusStatusIds.Contains( a.CampusStatusValueId.Value ) ).ToList();
            }

            return campusList;
        }

        /// <summary>
        /// Gets the available accounts in chunks to prevent SQL complexity errors.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private List<AccountItemBag> GetAvailableAccounts( RockContext rockContext )
        {
            var financialAccountService = new FinancialAccountService( rockContext );
            var accountList = new List<AccountItemBag>();
            var availableAccounts = financialAccountService.Queryable()
            .Where( f =>
                f.IsActive
                    && f.IsPublic.HasValue
                    && f.IsPublic.Value
                    && ( f.StartDate == null || f.StartDate <= RockDateTime.Today )
                    && ( f.EndDate == null || f.EndDate >= RockDateTime.Today ) )
            .Include( f => f.ImageBinaryFile );

            if ( Accounts.Any() )
            {
                availableAccounts = availableAccounts.Where( a => Accounts.Contains( a.Guid ) );
            }

            // Filter by the campus context if configured and there is a context campus.
            var contextCampus = RequestContext.GetContextEntity<Campus>();
            if ( contextCampus != null && AccountCampusContextFilter > -1 )
            {
                availableAccounts = availableAccounts.Where( a => ( AccountCampusContextFilter == 0 && a.CampusId == contextCampus.Id )
                    || ( AccountCampusContextFilter == 1 && ( a.CampusId == null || a.CampusId == contextCampus.Id ) ) );
            }

            availableAccounts = availableAccounts.OrderBy( f => f.Order );
            var accountIds = availableAccounts.Select( f => f.Id ).ToList();

            while ( accountIds.Any() )
            {
                // Process the accounts in chunks of 1000 to prevent any potential SQL complexity errors.
                List<int> accountIdsChunk = accountIds.Take( 1000 ).ToList();
                var accountsChunk = availableAccounts.Where( a => accountIdsChunk.Contains( a.Id ) );

                var childList = accountsChunk
                    .Where( f =>
                        f.ParentAccountId.HasValue
                        && accountIds.Contains( f.ParentAccountId.Value ) )
                    .ToList();

                // Enumerate through all active accounts that are public
                foreach ( var account in accountsChunk )
                {
                    var accountItem = new AccountItemBag
                    {
                        Id = account.Id,
                        IdKey = account.IdKey,
                        PublicName = account.PublicName,
                        ParentAccountId = account.ParentAccountId,
                        PublicDescriptionHtml = account.PublicDescription
                    };

                    if ( account.ImageBinaryFile != null )
                    {
                        accountItem.ImageSource = account.ImageBinaryFile.Url;
                    }

                    accountList.Add( accountItem );
                }

                accountIds = accountIds.Where( a => !accountIdsChunk.Contains( a ) ).ToList();
            }

            return accountList;
        }

        /// <summary>
        /// Returns a list of the saved accounts for the provided person.
        /// </summary>
        /// <returns></returns>
        private List<SavedFinancialAccountListItemBag> GetCurrentPersonSavedAccounts()
        {
            if ( RequestContext.CurrentPerson == null )
            {
                return null;
            }

            var savedAccountClientService = new FinancialPersonSavedAccountClientService( RockContext, RequestContext.CurrentPerson );

            if ( FinancialGatewayComponent == null )
            {
                return null;
            }

            var accountOptions = new SavedFinancialAccountOptions
            {
                FinancialGatewayGuids = new List<Guid>
                {
                    FinancialGateway.Guid
                },
                CurrencyTypeGuids = GetAllowedCurrencyTypes( EnableAch, EnableCreditCard, FinancialGatewayComponent ).Select( a => a.Guid ).ToList()
            };

            return savedAccountClientService.GetSavedFinancialAccountsForPersonAsAccountListItems( RequestContext.CurrentPerson.Id, accountOptions )
                .Select( vm => new SavedFinancialAccountListItemBag
                {
                    AccountNumberMasked = vm.AccountNumberMasked,
                    Category = vm.Category,
                    CurrencyTypeGuid = vm.CurrencyTypeGuid,
                    Description = vm.Description,
                    Image = vm.Image,
                    Text = vm.Text,
                    Value = vm.Value
                } ).ToList();
        }

        /// <summary>
        /// Gets giving data for the block. If there is a current person, their saved accounts will be included.
        /// </summary>
        /// <returns>A bag containing basic giving information.</returns>
        private GivingInfoBag GetGivingData()
        {
            var accounts = GetAvailableAccounts( RockContext );

            var supportedFrequencies = GetSupportedFrequencies().Select( dvc => new ListItemViewModel
            {
                Text = dvc.Value,
                Value = dvc.IdKey
            } ).ToList();

            var campuses = GetConfiguredCampuses().Select( campus => new ListItemViewModel
            {
                Text = campus.Name,
                Value = campus.Guid.ToString()
            } ).ToList();

            var givingInfo = new GivingInfoBag
            {
                Accounts = GetAvailableAccounts( RockContext ),
                SupportedFrequencies = supportedFrequencies,
                Campuses = campuses,
            };

            if ( RequestContext.CurrentPerson != null )
            {
                givingInfo.PaymentMethods = GetCurrentPersonSavedAccounts();
            }

            return givingInfo;
        }


        /// <summary>
        /// Gets the transaction payment information.
        /// </summary>
        /// <param name="options">The options for the transaction.</param>
        /// <param name="person">The person processing the transaction.</param>
        /// <param name="givingAsBusiness">Whether or not the transaction is being processed for a business.</param>
        /// <param name="errorMessage">A populated error message (if one occurs).</param>
        /// <returns></returns>
        private ReferencePaymentInfo GetTxnPaymentInfo( TransactionRequestInfoBag options, Person person, bool givingAsBusiness, out string errorMessage )
        {
            ReferencePaymentInfo paymentInfo = GetPaymentInfo( options, out errorMessage );
            if ( errorMessage.IsNotNullOrWhiteSpace() )
            {
                return null;
            }
            else if ( paymentInfo == null )
            {
                errorMessage = "There was a problem creating the payment information";
                return null;
            }
            else
            {
                if ( givingAsBusiness )
                {
                    paymentInfo.BusinessName = person.LastName;
                }

                paymentInfo.FirstName = person.FirstName;
                paymentInfo.LastName = person.LastName;
            }

            if ( paymentInfo.GatewayPersonIdentifier.IsNullOrWhiteSpace() )
            {
                var financialGatewayComponent = this.FinancialGatewayComponent;

                if ( errorMessage.IsNotNullOrWhiteSpace() )
                {
                    return null;
                }

                var customerToken = financialGatewayComponent.CreateCustomerAccount( this.FinancialGateway, paymentInfo, out errorMessage );
                if ( errorMessage.IsNotNullOrWhiteSpace() || customerToken.IsNullOrWhiteSpace() )
                {
                    errorMessage = errorMessage.IsNotNullOrWhiteSpace() ? errorMessage : "Unknown Error";
                    return null;
                }

                paymentInfo.GatewayPersonIdentifier = customerToken;
            }

            errorMessage = string.Empty;
            return paymentInfo;
        }

        /// <summary>
        /// Gets the payment information.
        /// </summary>
        private ReferencePaymentInfo GetPaymentInfo( TransactionRequestInfoBag bag, out string errorMessage )
        {
            errorMessage = null;

            var isSavedAccount = bag.SavedAccountId.IsNotNullOrWhiteSpace();
            var paymentInfo = isSavedAccount ? GetSavedAccountReferenceInfo( bag.SavedAccountId ) : new ReferencePaymentInfo();
            paymentInfo.Email = bag.Email;

            var commonTransactionAccountDetails = new List<FinancialTransactionDetail>();
            PopulateTransactionDetails( commonTransactionAccountDetails, bag );

            paymentInfo.Amount = commonTransactionAccountDetails.Sum( tad => tad.Amount );

            var totalFeeCoverageAmounts = commonTransactionAccountDetails.Where( a => a.FeeCoverageAmount.HasValue ).Select( a => a.FeeCoverageAmount.Value );
            if ( totalFeeCoverageAmounts.Any() )
            {
                paymentInfo.FeeCoverageAmount = totalFeeCoverageAmounts.Sum();
            }

            if ( !isSavedAccount )
            {
                paymentInfo.Street1 = bag.Street1;
                paymentInfo.Street2 = bag.Street2;
                paymentInfo.City = bag.City;
                paymentInfo.State = bag.State;
                paymentInfo.PostalCode = bag.PostalCode;
                paymentInfo.Country = bag.Country;
                paymentInfo.AdditionalParameters = bag.AdditionalParameters;
                paymentInfo.ReferenceNumber = bag.PaymentToken;
                paymentInfo.InitialCurrencyTypeValue = DefinedValueCache.Get( bag.CurrencyTypeValue );
            }
            else
            {
                paymentInfo.AdditionalParameters.AddOrReplace( "automation", true.ToString() );
            }

            // Set the transaction type to contribution.
            paymentInfo.TransactionTypeValueId = TransactionType.Id;

            return paymentInfo;
        }

        /// <summary>
        /// Processes a transaction based on the provided information.
        /// </summary>
        /// <param name="bag">The options for this transaction.</param>
        /// <param name="errorMessage">The error message to set if an error occurs or there was an invalid configuration=</param>
        /// <returns></returns>
        private TransactionResultBag ProcessTransaction( TransactionRequestInfoBag bag, out string errorMessage )
        {
            var gateway = FinancialGatewayComponent;
            var financialGateway = FinancialGateway;

            if ( gateway == null || financialGateway == null )
            {
                errorMessage = "The financial gateway is not configured correctly.";
                return null;
            }

            Person person = GetPerson( bag, true );

            if ( person == null )
            {
                errorMessage = "There was a problem creating the person information";
                return null;
            }

            if ( !person.PrimaryAliasId.HasValue )
            {
                errorMessage = "There was a problem creating the person's primary alias";
                return null;
            }

            var paymentInfo = GetTxnPaymentInfo( bag, person, false, out errorMessage );
            if ( paymentInfo == null )
            {
                return null;
            }

            PaymentSchedule schedule = GetSchedule( bag.ProcessDate, bag.FrequencyValueId );
            var mergeFields = RequestContext.GetCommonMergeFields();

            if ( schedule != null )
            {
                schedule.PersonId = person.Id;

                var scheduledTransaction = gateway.AddScheduledPayment( financialGateway, schedule, paymentInfo, out errorMessage );
                if ( scheduledTransaction == null )
                {
                    return null;
                }

                SaveScheduledTransaction( financialGateway, gateway, person, paymentInfo, schedule, scheduledTransaction, bag );
                mergeFields.Add( "Transaction", scheduledTransaction );
            }
            else
            {
                var transaction = gateway.Charge( financialGateway, paymentInfo, out errorMessage );
                if ( transaction == null )
                {
                    return null;
                }

                SaveTransaction( financialGateway, gateway, person, paymentInfo, transaction, bag );
                mergeFields.Add( "Transaction", transaction );
            }

            return new TransactionResultBag
            {
                SuccessTemplate = SuccessTemplate.ResolveMergeFields( mergeFields )
            };
        }

        /// <summary>
        /// Saves the transaction by updating its details, associating it with a batch, and processing related operations.
        /// </summary>
        /// <param name="financialGateway">The financial gateway associated with the transaction.</param>
        /// <param name="gateway">The hosted gateway component handling the payment gateway operations.</param>
        /// <param name="person">The individual associated with the transaction.</param>
        /// <param name="paymentInfo">The payment information for the transaction.</param>
        /// <param name="transaction">The financial transaction to save.</param>
        /// <param name="options">The transaction request options containing additional details.</param>
        private void SaveTransaction( FinancialGateway financialGateway, IHostedGatewayComponent gateway, Person person, PaymentInfo paymentInfo, FinancialTransaction transaction, TransactionRequestInfoBag options )
        {
            transaction.AuthorizedPersonAliasId = person.PrimaryAliasId;
            transaction.TransactionDateTime = RockDateTime.Now;
            transaction.FinancialGatewayId = financialGateway.Id;
            transaction.TransactionTypeValueId = TransactionType.Id;

            transaction.Summary = paymentInfo.Comment1;

            if ( transaction.FinancialPaymentDetail == null )
            {
                transaction.FinancialPaymentDetail = new FinancialPaymentDetail();
            }

            transaction.FinancialPaymentDetail.SetFromPaymentInfo( paymentInfo, gateway as GatewayComponent, RockContext );

            var source = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_MOBILE_APPLICATION.AsGuid() );
            transaction.SourceTypeValueId = source.Id;

            PopulateTransactionDetails( transaction.TransactionDetails, options );

            var batchService = new FinancialBatchService( RockContext );

            // Get the batch
            var batch = batchService.GetForNewTransaction( transaction, BatchNamePrefix );

            var batchChanges = new History.HistoryChangeList();
            FinancialBatchService.EvaluateNewBatchHistory( batch, batchChanges );

            transaction.LoadAttributes( RockContext );

            var financialTransactionService = new FinancialTransactionService( RockContext );

            // If this is a new Batch, SaveChanges so that we can get the Batch.Id
            if ( batch.Id == 0 )
            {
                RockContext.SaveChanges();
            }

            transaction.BatchId = batch.Id;

            // use the financialTransactionService to add the transaction instead of batch.Transactions to avoid lazy-loading the transactions already associated with the batch
            financialTransactionService.Add( transaction );

            RockContext.SaveChanges();
            transaction.SaveAttributeValues();

            batchService.IncrementControlAmount( batch.Id, transaction.TotalAmount, batchChanges );
            RockContext.SaveChanges();

            Task.Run( () => GiftWasGivenMessage.PublishTransactionEvent( transaction.Id, GiftEventTypes.GiftSuccess ) );

            HistoryService.SaveChanges(
                RockContext,
                typeof( FinancialBatch ),
                Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                batch.Id,
                batchChanges
            );

            SendReceipt( transaction.Id );
        }

        /// <summary>
        /// Saves a scheduled transaction.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="gateway">The hosted gateway component.</param>
        /// <param name="person">The person associated with the transaction.</param>
        /// <param name="paymentInfo">The payment information for the transaction.</param>
        /// <param name="schedule">The payment schedule for the transaction.</param>
        /// <param name="scheduledTransaction">The scheduled transaction to save.</param>
        /// <param name="options">The options for the transaction.</param>
        private void SaveScheduledTransaction( FinancialGateway financialGateway, IHostedGatewayComponent gateway, Person person, PaymentInfo paymentInfo, PaymentSchedule schedule, FinancialScheduledTransaction scheduledTransaction, TransactionRequestInfoBag options )
        {
            scheduledTransaction.TransactionFrequencyValueId = schedule.TransactionFrequencyValue.Id;
            scheduledTransaction.StartDate = schedule.StartDate;
            scheduledTransaction.AuthorizedPersonAliasId = person.PrimaryAliasId.Value;
            scheduledTransaction.FinancialGatewayId = financialGateway.Id;
            scheduledTransaction.TransactionTypeValueId = TransactionType.Id;

            if ( scheduledTransaction.FinancialPaymentDetail == null )
            {
                scheduledTransaction.FinancialPaymentDetail = new FinancialPaymentDetail();
            }

            scheduledTransaction.FinancialPaymentDetail.SetFromPaymentInfo( paymentInfo, gateway as GatewayComponent, RockContext );

            var source = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_MOBILE_APPLICATION.AsGuid() );
            scheduledTransaction.SourceTypeValueId = source.Id;

            PopulateTransactionDetails( scheduledTransaction.ScheduledTransactionDetails, options );

            scheduledTransaction.Summary = paymentInfo.Comment1;

            var transactionService = new FinancialScheduledTransactionService( RockContext );
            transactionService.Add( scheduledTransaction );
            RockContext.SaveChanges();

            Task.Run( () => ScheduledGiftWasModifiedMessage.PublishScheduledTransactionEvent( scheduledTransaction.Id, ScheduledGiftEventTypes.ScheduledGiftCreated ) );
        }

        /// <summary>
        /// Gets the payment schedule.
        /// </summary>
        private PaymentSchedule GetSchedule( DateTime? startDate, string frequencyIdKey )
        {
            startDate = startDate ?? RockDateTime.Today;

            // Figure out if this is a one-time transaction or a future scheduled transaction
            if ( AllowScheduled )
            {
                // If a one-time gift was selected for today's date, then treat as a onetime immediate transaction (not scheduled)
                int oneTimeFrequencyId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME ).Id;
                var selectedFrequency = DefinedValueCache.Get( frequencyIdKey, false );
                if ( selectedFrequency?.Id == oneTimeFrequencyId && startDate <= RockDateTime.Today )
                {
                    // one-time immediate payment
                    return null;
                }

                var schedule = new PaymentSchedule();
                schedule.TransactionFrequencyValue = selectedFrequency;
                if ( startDate.HasValue && startDate > RockDateTime.Today )
                {
                    schedule.StartDate = startDate.Value;
                }
                else
                {
                    schedule.StartDate = DateTime.MinValue;
                }

                return schedule;
            }

            return null;
        }

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="create">if set to <c>true</c> [create].</param>
        /// <returns></returns>
        private Person GetPerson( TransactionRequestInfoBag options, bool create )
        {
            Person person = null;
            var personService = new PersonService( RockContext );

            Group familyGroup = null;

            int personId = RequestContext.CurrentPerson?.Id as int? ?? 0;

            if ( personId != 0 )
            {
                person = personService.Get( personId );
            }

            if ( create )
            {
                // If this is a nameless person, we need to make a new person record and merge it.
                Person namelessPerson = null;
                if ( person != null && person.IsNameless() )
                {
                    namelessPerson = person;
                    person = null;
                }

                if ( person == null )
                {
                    // Check to see if there's only one person with same email, first name, and last name
                    if ( options.Email.IsNotNullOrWhiteSpace() &&
                        options.FirstName.IsNotNullOrWhiteSpace() &&
                        options.LastName.IsNotNullOrWhiteSpace() )
                    {
                        // Same logic as PledgeEntry.ascx.cs
                        var personQuery = new PersonService.PersonMatchQuery( options.FirstName, options.LastName, options.Email, options.Phone?.Trim() ?? string.Empty );
                        person = personService.FindPerson( personQuery, true );
                    }

                    if ( person == null )
                    {

                        // Create Person
                        person = new Person
                        {
                            FirstName = options.FirstName,
                            LastName = options.LastName,
                            IsEmailActive = true,
                            EmailPreference = EmailPreference.EmailAllowed,
                            RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id
                        };
                        if ( ConnectionStatus != null )
                        {
                            person.ConnectionStatusValueId = ConnectionStatus.Id;
                        }

                        if ( RecordStatus != null )
                        {
                            person.RecordStatusValueId = RecordStatus.Id;
                        }

                        // Create Person/Family
                        familyGroup = PersonService.SaveNewPerson( person, RockContext, null, false );
                    }

                    if ( namelessPerson != null )
                    {
                        personService.MergeNamelessPersonToExistingPerson( namelessPerson, person );
                    }
                }
            }

            // Person should never be null at this point.
            if ( create && person != null )
            {
                if ( familyGroup == null )
                {
                    var groupLocationService = new GroupLocationService( RockContext );

                    Guid addressTypeGuid = Guid.Empty;
                    if ( AddressType == null )
                    {
                        addressTypeGuid = new Guid( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME );
                    }
                    else
                    {
                        addressTypeGuid = AddressType.Guid;
                    }

                    var groupLocation = personService.GetFirstLocation( person.Id, DefinedValueCache.Get( addressTypeGuid ).Id );

                    if ( groupLocation?.Id != null )
                    {
                        familyGroup = groupLocationService.Queryable()
                            .Where( gl => gl.Id == groupLocation.Id )
                            .Select( gl => gl.Group )
                            .FirstOrDefault();
                    }
                    else
                    {
                        familyGroup = person.GetFamily( RockContext );
                    }
                }

                RockContext.SaveChanges();

                if ( familyGroup != null )
                {
                    GroupService.AddNewGroupAddress(
                        RockContext,
                        familyGroup,
                        AddressType.Guid.ToString(),
                        options.Street1, options.Street2, options.City, options.State, options.PostalCode, options.Country,
                        true );
                }
            }

            return person;
        }


        /// <summary>
        /// Populates the transaction details for a FinancialTransaction or ScheduledFinancialTransaction
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="transactionDetails">The transaction details.</param>
        /// <param name="options">The options.</param>
        private void PopulateTransactionDetails<T>( ICollection<T> transactionDetails, TransactionRequestInfoBag options ) where T : ITransactionDetail, new()
        {
            var selectedAccountAmounts = options.AccountAmountSelections.Where( kvp => kvp.Amount > 0m );
            var totalSelectedAmounts = selectedAccountAmounts.Select( kvp => kvp.Amount ).Sum();
            var isAch = options.CurrencyTypeValue.AsGuid() == Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH.AsGuid();
            var enableCoverTheFees = options.EnableCoverTheFees;

            var feeCoverageGatewayComponent = FinancialGateway.GetGatewayComponent() as IFeeCoverageGatewayComponent;

            foreach ( var selectedAccountAmount in selectedAccountAmounts )
            {
                var transactionDetail = new T();
                var amount = selectedAccountAmount.Amount;

                if ( feeCoverageGatewayComponent != null && enableCoverTheFees && options.FeeCoverageAmount.HasValue )
                {
                    decimal portionOfTotalAmount = decimal.Divide( selectedAccountAmount.Amount, totalSelectedAmounts );
                    decimal feeCoverageAmountForAccount = decimal.Round( portionOfTotalAmount * options.FeeCoverageAmount.Value, 2 );

                    amount += feeCoverageAmountForAccount;
                    transactionDetail.FeeCoverageAmount = feeCoverageAmountForAccount;
                }

                // Get the account from the account id
                var account = new FinancialAccountService( RockContext ).Get( selectedAccountAmount.AccountId, !PageCache.Layout.Site.DisablePredictableIds );
                transactionDetail.AccountId = account.Id;
                transactionDetail.Amount = amount;
                transactionDetails.Add( transactionDetail );
            }
        }

        /// <summary>
        /// Gets the reference information.
        /// </summary>
        /// <param name="savedAccountId">The saved account unique identifier.</param>
        private ReferencePaymentInfo GetSavedAccountReferenceInfo( string savedAccountId )
        {
            var savedAccount = new FinancialPersonSavedAccountService( new RockContext() ).Get( savedAccountId );
            if ( savedAccount != null )
            {
                return savedAccount.GetReferencePayment();
            }

            return null;
        }

        /// <summary>
        /// Processes a new account for the hosted gateway component.
        /// </summary>
        /// <param name="hostedGatewayComponent">The gateway component.</param>
        /// <param name="options">The options to create the account.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private FinancialPersonSavedAccount ProcessNewAccount( IHostedGatewayComponent hostedGatewayComponent, AddNewSavedAccountBag options, out string errorMessage )
        {
            var currencyTypeValue = DefinedValueCache.Get( options.CurrencyTypeValueId, false );

            if ( currencyTypeValue == null )
            {
                errorMessage = "A currency type is required when creating a new account.";
                return null;
            }

            var isAch = currencyTypeValue.Guid.Equals( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH.AsGuid() );
            var isCreditCard = currencyTypeValue.Guid.Equals( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid() );

            if ( isCreditCard )
            {
                return ProcessNewCreditCardAccount( hostedGatewayComponent, options, out errorMessage );
            }
            else if ( isAch )
            {
                return ProcessNewAchAccount( hostedGatewayComponent, options, out errorMessage );
            }

            errorMessage = "Only ACH and Credit Card currency types are supported.";
            return null;
        }

        /// <summary>
        /// Processes the creation of a new saved credit card account.
        /// </summary>
        /// <param name="hostedGatewayComponent">The hosted gateway component handling the payment gateway operations.</param>
        /// <param name="options">The options containing details for creating the saved account.</param>
        /// <param name="errorMessage">An output parameter that contains an error message if the operation fails.</param>
        /// <returns>
        /// A <see cref="FinancialPersonSavedAccount"/> object representing the saved credit card account, 
        /// or null if the process fails.
        /// </returns>
        private FinancialPersonSavedAccount ProcessNewCreditCardAccount( IHostedGatewayComponent hostedGatewayComponent, AddNewSavedAccountBag options, out string errorMessage )
        {
            var paymentInfo = new ReferencePaymentInfo
            {
                Amount = 0.0M,
                Comment1 = $"Saved Account for {RequestContext.CurrentPerson.FirstName}",
                FirstName = RequestContext.CurrentPerson.FirstName,
                LastName = RequestContext.CurrentPerson.LastName,
                ReferenceNumber = options.Token,
                InitialCurrencyTypeValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD ),
                PostalCode = options.PostalCode,
                Country = "US",
                Street1 = string.Empty,
                Street2 = string.Empty,
                City = string.Empty,
                State = string.Empty,
                TransactionTypeValueId = TransactionType.Id
            };

            var customerToken = hostedGatewayComponent.CreateCustomerAccount( FinancialGateway, paymentInfo, out errorMessage );
            if ( errorMessage.IsNotNullOrWhiteSpace() || customerToken.IsNullOrWhiteSpace() )
            {
                return null;
            }

            paymentInfo.GatewayPersonIdentifier = customerToken;
            var transaction = hostedGatewayComponent.Authorize( FinancialGateway, paymentInfo, out errorMessage );
            if ( transaction == null )
            {
                return null;
            }

            var savedAccount = new FinancialPersonSavedAccount
            {
                ReferenceNumber = hostedGatewayComponent.GetReferenceNumber( transaction, out errorMessage ),
                TransactionCode = transaction.TransactionCode,
                FinancialGatewayId = FinancialGateway.Id,
                FinancialPaymentDetail = new FinancialPaymentDetail(),
                GatewayPersonIdentifier = paymentInfo.GatewayPersonIdentifier,
                PersonAliasId = RequestContext.CurrentPerson.PrimaryAliasId
            };
            savedAccount.FinancialPaymentDetail.SetFromPaymentInfo( paymentInfo, hostedGatewayComponent as GatewayComponent, RockContext );
            savedAccount.FinancialPaymentDetail.AccountNumberMasked = transaction.FinancialPaymentDetail.AccountNumberMasked;
            savedAccount.FinancialPaymentDetail.NameOnCard = transaction.FinancialPaymentDetail.NameOnCard;
            savedAccount.FinancialPaymentDetail.ExpirationMonth = transaction.FinancialPaymentDetail.ExpirationMonth;
            savedAccount.FinancialPaymentDetail.ExpirationYear = transaction.FinancialPaymentDetail.ExpirationYear;

            var creditCardTypeValue = string.Empty;

            if ( transaction.FinancialPaymentDetail.CreditCardTypeValueId.HasValue )
            {
                var creditCardTypeDefinedValue = DefinedValueCache.Get( transaction.FinancialPaymentDetail.CreditCardTypeValueId.Value );
                if ( creditCardTypeDefinedValue != null )
                {
                    creditCardTypeValue = creditCardTypeDefinedValue.Value;
                }

                savedAccount.Name = $"{creditCardTypeValue}";
            }

            if ( savedAccount.Name.IsNullOrWhiteSpace() )
            {
                savedAccount.Name = $"Card {string.Join( "", transaction.FinancialPaymentDetail.AccountNumberMasked.TakeLast( 5 ) )}";
            }

            var savedAccountService = new FinancialPersonSavedAccountService( RockContext );
            savedAccountService.Add( savedAccount );
            RockContext.SaveChanges();

            return savedAccount;
        }

        /// <summary>
        /// Processes the creation of a new saved ACH account.
        /// </summary>
        /// <param name="hostedGatewayComponent">The hosted gateway component handling the payment gateway operations.</param>
        /// <param name="options">The options containing details for creating the saved account.</param>
        /// <param name="errorMessage">An output parameter that contains an error message if the operation fails.</param>
        /// <returns>
        /// A <see cref="FinancialPersonSavedAccount"/> object representing the saved ACH account, 
        /// or null if the process fails.
        /// </returns>
        private FinancialPersonSavedAccount ProcessNewAchAccount( IHostedGatewayComponent hostedGatewayComponent, AddNewSavedAccountBag options, out string errorMessage )
        {
            if ( options.HomeAddress == null )
            {
                errorMessage = "The address is required for an ACH account.";
                return null;
            }

            var paymentInfo = new ReferencePaymentInfo
            {
                Amount = 0.0M,
                Comment1 = $"Saved Account for {RequestContext.CurrentPerson.FirstName}",
                FirstName = RequestContext.CurrentPerson.FirstName,
                LastName = RequestContext.CurrentPerson.LastName,
                ReferenceNumber = options.Token,
                InitialCurrencyTypeValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH ),
                TransactionTypeValueId = TransactionType.Id,
                Street1 = options.HomeAddress.Street1,
                Street2 = string.Empty,
                City = options.HomeAddress.City,
                State = options.HomeAddress.State,
                PostalCode = options.HomeAddress.PostalCode,
                Country = options.HomeAddress.Country
            };

            var customerToken = hostedGatewayComponent.CreateCustomerAccount( FinancialGateway, paymentInfo, out errorMessage );
            if ( errorMessage.IsNotNullOrWhiteSpace() || customerToken.IsNullOrWhiteSpace() )
            {
                return null;
            }

            paymentInfo.GatewayPersonIdentifier = customerToken;

            var savedAccount = new FinancialPersonSavedAccount
            {
                FinancialGatewayId = FinancialGateway.Id,
                FinancialPaymentDetail = new FinancialPaymentDetail(),
                GatewayPersonIdentifier = paymentInfo.GatewayPersonIdentifier,
                PersonAliasId = RequestContext.CurrentPerson.PrimaryAliasId
            };
            savedAccount.FinancialPaymentDetail.SetFromPaymentInfo( paymentInfo, hostedGatewayComponent as GatewayComponent, RockContext );

            if ( savedAccount.Name.IsNullOrWhiteSpace() )
            {
                savedAccount.Name = $"Saved Bank Account";
            }

            if( paymentInfo.AdditionalParameters.ContainsKey( "AccountNumberMasked" ) )
            {
                savedAccount.FinancialPaymentDetail.AccountNumberMasked = paymentInfo.AdditionalParameters["AccountNumberMasked"];
            }

            var savedAccountService = new FinancialPersonSavedAccountService( RockContext );
            savedAccountService.Add( savedAccount );
            RockContext.SaveChanges();

            return savedAccount;
        }

        /// <summary>
        /// Processes the account amount selections based on the campus (see <seealso cref="UseAccountCampusMappingLogic" />).
        /// </summary>
        /// <param name="options"></param>
        private void ProcessAccountAmountSelections( TransactionRequestInfoBag options )
        {
            if( options.CampusId.IsNullOrWhiteSpace() )
            {
                return;
            }

            var campusId = CampusCache.Get( options.CampusId, !this.PageCache.Layout.Site.DisablePredictableIds )?.Id;

            if ( campusId == null )
            {
                return;
            }

            var accountAmountSelectionsList = new List<AccountAmountSelectionBag>();

            foreach ( var accountAmountSelection in options.AccountAmountSelections )
            {
                var financialAccount = FinancialAccountCache.Get( accountAmountSelection.AccountId, !this.PageCache.Layout.Site.DisablePredictableIds );

                if ( financialAccount == null )
                {
                    continue;
                }

                var accountId = financialAccount.IdKey;
                if ( campusId != null )
                {
                    accountId = GetBestMatchingAccountIdForCampusFromDisplayedAccount( campusId.Value, financialAccount );
                }

                accountAmountSelectionsList.Add( new AccountAmountSelectionBag
                {
                    AccountId = accountId,
                    Amount = accountAmountSelection.Amount
                } );
            }

            options.AccountAmountSelections = accountAmountSelectionsList;
        }

        /// <summary>
        /// Sends the receipt for the transaction.
        /// </summary>
        /// <param name="transactionId">The processed transaction.</param>
        private void SendReceipt( int transactionId )
        {
            if ( ReceiptEmail != null )
            {
                // Queue a bus message to send receipts
                var sendPaymentReceiptsTask = new ProcessSendPaymentReceiptEmails.Message
                {
                    SystemEmailGuid = ReceiptEmail.Guid,
                    TransactionId = transactionId
                };

                sendPaymentReceiptsTask.Send();
            }
        }

        /// <summary>
        /// Gets the best matching AccountId for selected campus from the displayed account.
        /// </summary>
        /// <param name="campusId">The campus.</param>
        /// <param name="displayedAccount">The displayed account.</param>
        /// <returns></returns>
        private string GetBestMatchingAccountIdForCampusFromDisplayedAccount( int campusId, FinancialAccountCache displayedAccount )
        {
            if ( !UseAccountCampusMappingLogic )
            {
                return displayedAccount.IdKey;
            }

            if ( displayedAccount.CampusId.HasValue && displayedAccount.CampusId == campusId )
            {
                // displayed account is directly associated with selected campusId, so return it
                return displayedAccount.IdKey;
            }
            else
            {
                // displayed account doesn't have a campus (or belongs to another campus). Find first active matching child account
                var firstMatchingChildAccount = displayedAccount.ChildAccounts.Where( a => a.IsActive ).FirstOrDefault( a => a.CampusId.HasValue && a.CampusId == campusId );
                if ( firstMatchingChildAccount != null )
                {
                    // one of the child accounts is associated with the campus so, return the child account
                    return firstMatchingChildAccount.IdKey;
                }
                else
                {
                    // none of the child accounts is associated with the campus so, return the displayed account
                    return displayedAccount.IdKey;
                }
            }
        }

        #endregion

        #region IRockMobileBlockType

        /// <inheritdoc />
        public override object GetMobileConfigurationValues()
        {
            return new
            {
                EnableFeeCoverage = EnableFeeCoverage,
                EnableMultiAccount = EnableMultiAccount,
                EnableAch = EnableAch,
                EnableCreditCard = EnableCreditCard,
                AllowScheduled = AllowScheduled,
                CreditCardFeeCoveragePercentage = GetCreditCardFeeCoveragePercentage(),
                ACHFeeCoverageAmount = GetACHFeeCoverageAmount(),
                AskForCampusIfKnown = AskForCampusIfKnown,
                PublicKey = FinancialGateway?.GetAttributeValue( "PublicApiKey" ),
                TransactionListPageGuid = TransactionListPageGuid,
                ScheduledTransactionListPageGuid = ScheduledTransactionListPageGuid,
                SavedAccountListPageGuid = SavedAccountListPageGuid,
            };
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the initial data for the block.
        /// </summary>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetInitialData()
        {
            if( FinancialGateway == null )
            {
                return ActionBadRequest( "The Giving block is only compatible with the MyWell gateway." );
            }

            return ActionOk( GetGivingData() );
        }

        /// <summary>
        /// Gets the saved payment methods for the current person.
        /// </summary>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetPaymentMethods()
        {
            return ActionOk( GetCurrentPersonSavedAccounts() );
        }

        /// <summary>
        /// Adds a new payment method for the current person.
        /// </summary>
        /// <param name="options">The options to add the new payment method with.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult AddPaymentMethod( AddNewSavedAccountBag options )
        {
            if ( RequestContext.CurrentPerson == null )
            {
                return ActionBadRequest( "You must be logged in to add a payment method." );
            }

            if ( options == null || options.Token.IsNullOrWhiteSpace() || options.CurrencyTypeValueId.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Invalid request. Token and Currency Type are required parameters." );
            }

            var savedAccount = ProcessNewAccount( FinancialGatewayComponent, options, out var errorMessage );

            if ( savedAccount == null )
            {
                return ActionBadRequest( errorMessage );
            }

            return ActionOk( savedAccount.Guid );
        }

        /// <summary>
        /// Processes a transaction from the client.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult ProcessTransaction( TransactionRequestInfoBag options )
        {
            ProcessAccountAmountSelections( options );
            var transactionResult = ProcessTransaction( options, out var errorMessage );

            if ( errorMessage.IsNotNullOrWhiteSpace() )
            {
                return ActionBadRequest( errorMessage );
            }

            return ActionOk( transactionResult );
        }

        #endregion

        #region Default Templates

        /// <summary>
        /// The default template that is used for the <see cref="SuccessTemplate"/> property."/>
        /// </summary>
        private const string DefaultSuccessTemplate = @"<Grid>
    <StackLayout HorizontalOptions=""Center""
        StyleClass=""mt-48, px-24""
        Spacing=""24"">
        <Rock:Icon IconClass=""circle-check""
            IconFamily=""TablerIcons""
            StyleClass=""text-success-strong""
            FontSize=""80""
            HorizontalOptions=""Center"" />

        <Label Text=""Thank you for your generosity!""
            HorizontalTextAlignment=""Center""
            HorizontalOptions=""Center""
            StyleClass=""title1, text-interface-strongest, bold"" /> 

        <StackLayout Spacing=""8"">
            <Label Text=""Your gift of ${{ Transaction.TotalAmount }} has been received.""
                HorizontalTextAlignment=""Center""
                HorizontalOptions=""Center""
                StyleClass=""text-interface-strong, body"" />

            <Label Text=""We sent a confirmation email to {{ Transaction.AuthorizedPersonAlias.Person.Email }}.""
                HorizontalTextAlignment=""Center""
                HorizontalOptions=""Center""
                StyleClass=""text-interface-medium, body"" />
        </StackLayout>
    </StackLayout>
</Grid>";

        #endregion
    }
}