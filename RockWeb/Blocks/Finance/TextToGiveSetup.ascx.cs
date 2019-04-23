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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Lava;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Setup block for Text-To-Give (mostly a simplified transaction entry v2 adaptation)
    /// 
    /// This block is typically reached through the SMS give action. It requires a URL person token. That person could be blank except for a mobile phone
    /// number. This block will set name, address, and email for the person.
    ///
    /// In order to give through SMS, a person must have a default saved account and a contribution account (field on the person). This block's goal
    /// is to set both of those while simulatenously allowing the person to give their first gift. 
    /// </summary>
    [DisplayName( "Text To Give Setup" )]
    [Category( "Finance" )]
    [Description( "Allow an SMS sender to configure their SMS based giving." )]

    #region Block Attributes

    [FinancialGatewayField(
        "Financial Gateway",
        Key = AttributeKey.FinancialGateway,
        Description = "The payment gateway to use for Credit Card and ACH transactions.",
        Category = AttributeCategory.None,
        Order = 0 )]

    [BooleanField(
        "Enable ACH",
        Key = AttributeKey.EnableACH,
        DefaultBooleanValue = true,
        Category = AttributeCategory.None,
        Order = 1 )]

    [BooleanField(
        "Enable Credit Card",
        Key = AttributeKey.EnableCreditCard,
        DefaultBooleanValue = true,
        Category = AttributeCategory.None,
        Order = 2 )]

    [TextField(
        "Batch Name Prefix",
        Key = AttributeKey.BatchNamePrefix,
        Description = "The batch prefix name to use when creating a new batch.",
        DefaultValue = "SMS Giving Setup",
        Category = AttributeCategory.None,
        Order = 3 )]

    [AccountsField(
        "Accounts",
        Key = AttributeKey.AccountsToDisplay,
        Description = "The accounts to display. By default all active accounts with a Public Name will be displayed. If the account has a child account for the selected campus, the child account for that campus will be used.",
        Category = AttributeCategory.None,
        Order = 5 )]

    [BooleanField(
        "Ask for Campus if Known",
        Key = AttributeKey.AskForCampusIfKnown,
        Description = "If the campus for the person is already known, should the campus still be prompted for?",
        DefaultBooleanValue = true,
        Category = AttributeCategory.None,
        Order = 10 )]

    #region Text Options

    [CodeEditorField(
        "Intro Message",
        Key = AttributeKey.IntroMessageTemplate,
        EditorMode = CodeEditorMode.Lava,
        Description = "The text to place at the top of the amount entry. <span class='tip tip-lava'></span>",
        DefaultValue = "<h2>Setup your first gift on-line, future gifts can be completed via text.</h2>",
        Category = AttributeCategory.TextOptions,
        Order = 2 )]

    [CodeEditorField(
        "Finish Lava Template",
        Key = AttributeKey.FinishLavaTemplate,
        EditorMode = CodeEditorMode.Lava,
        Description = "The text (HTML) to display on the success page. <span class='tip tip-lava'></span>",
        DefaultValue = DefaultFinishLavaTemplate,
        Category = AttributeCategory.TextOptions,
        Order = 5 )]

    #endregion

    #region Email Templates

    [SystemEmailField(
        "Receipt Email",
        Key = AttributeKey.ReceiptEmail,
        Description = "The system email to use to send the receipt.",
        IsRequired = false,
        Category = AttributeCategory.EmailTemplates,
        Order = 2 )]

    #endregion Email Templates

    #region Advanced options

    [DefinedValueField(
        "Transaction Type",
        Key = AttributeKey.TransactionType,
        Description = "",
        IsRequired = true,
        AllowMultiple = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE,
        DefaultValue = Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION,
        Category = AttributeCategory.Advanced,
        Order = 1 )]

    [EntityTypeField(
        "Transaction Entity Type",
        Key = AttributeKey.TransactionEntityType,
        Description = "The Entity Type for the Transaction Detail Record (usually left blank)",
        IsRequired = false,
        Category = AttributeCategory.Advanced,
        Order = 2 )]

    [BooleanField( "Enable Initial Back button",
        Key = AttributeKey.EnableInitialBackButton,
        Description = "Show a Back button on the initial page that will navigate to wherever the user was prior to the transaction entry",
        DefaultBooleanValue = false,
        Category = AttributeCategory.Advanced,
        Order = 8 )]

    #endregion Advanced Options

    #endregion Block Attributes
    public partial class TextToGiveSetup : RockBlock
    {
        #region constants

        protected const string DefaultFinishLavaTemplate = @"
{% assign transactionDetails = Transaction.TransactionDetails %}

<h1>Thank You!</h1>

<p>You can now support {{ 'Global' | Attribute:'OrganizationName' }} through text based giving! We are grateful for your support!</p>

<dl>
    <dt>Confirmation Code</dt>
    <dd>{{ Transaction.TransactionCode }}</dd>
    <dd></dd>
    
    <dt>Name</dt>
    <dd>{{ Person.FullName }}</dd>
    <dd></dd>
    <dd>{{ Person.Email }}</dd>
    <dd>{{ BillingLocation.Street }} {{ BillingLocation.City }}, {{ BillingLocation.State }} {{ BillingLocation.PostalCode }}</dd>
</dl>

<dl class='dl-horizontal'>
    {% for transactionDetail in transactionDetails %}
        <dt>{{ transactionDetail.Account.PublicName }}</dt>
        <dd>{{ transactionDetail.Amount }}</dd>
    {% endfor %}
    <dd></dd>
    
    <dt>Payment Method</dt>
    <dd>{{ PaymentDetail.CurrencyTypeValue.Description}}</dd>

    {% if PaymentDetail.AccountNumberMasked  != '' %}
        <dt>Account Number</dt>
        <dd>{{ PaymentDetail.AccountNumberMasked  }}</dd>
    {% endif %}
</dl>
";

        #endregion

        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        protected static class AttributeKey
        {
            public const string AccountsToDisplay = "AccountsToDisplay";
            public const string AskForCampusIfKnown = "AskForCampusIfKnown";
            public const string BatchNamePrefix = "BatchNamePrefix";
            public const string FinancialGateway = "FinancialGateway";
            public const string EnableACH = "EnableACH";
            public const string EnableCreditCard = "EnableCreditCard";
            public const string EnableInitialBackButton = "EnableInitialBackButton";
            public const string IntroMessageTemplate = "IntroMessageTemplate";
            public const string FinishLavaTemplate = "FinishLavaTemplate";
            public const string TransactionType = "Transaction Type";
            public const string TransactionEntityType = "TransactionEntityType";
            public const string ReceiptEmail = "ReceiptEmail";
        }

        #endregion Attribute Keys

        #region Attribute Categories

        public static class AttributeCategory
        {
            public const string None = "";
            public const string TextOptions = "Text Options";
            public const string Advanced = "Advanced";
            public const string EmailTemplates = "Email Templates";
        }

        #endregion Attribute Categories

        #region PageParameterKeys

        public static class PageParameterKey
        {
            public const string Person = "Person";
        }

        #endregion

        #region enums

        /// <summary>
        /// 
        /// </summary>
        private enum EntryStep
        {
            /// <summary>
            /// prompt for amounts (step 1)
            /// </summary>
            PromptForAmounts,

            /// <summary>
            /// Get payment information (step 2)
            /// </summary>
            GetPaymentInfo,

            /// <summary>
            /// Get/Update personal information (step 3)
            /// </summary>
            GetPersonalInformation,

            /// <summary>
            /// The show transaction summary (step 4)
            /// </summary>
            ShowTransactionSummary
        }

        #endregion enums

        #region fields

        private Control _hostedPaymentInfoControl;

        /// <summary>
        /// use FinancialGateway instead
        /// </summary>
        private Rock.Model.FinancialGateway _financialGateway = null;

        /// <summary>
        /// Gets the financial gateway (model) that is configured for this block
        /// </summary>
        /// <returns></returns>
        private Rock.Model.FinancialGateway FinancialGateway
        {
            get
            {
                if ( _financialGateway == null )
                {
                    RockContext rockContext = new RockContext();
                    var financialGatewayGuid = GetAttributeValue( AttributeKey.FinancialGateway ).AsGuid();
                    _financialGateway = new FinancialGatewayService( rockContext ).GetNoTracking( financialGatewayGuid );
                }

                return _financialGateway;
            }
        }

        private IHostedGatewayComponent _financialGatewayComponent = null;

        /// <summary>
        /// Gets the financial gateway component that is configured for this block
        /// </summary>
        /// <returns></returns>
        private IHostedGatewayComponent FinancialGatewayComponent
        {
            get
            {
                if ( _financialGatewayComponent == null )
                {
                    var financialGateway = FinancialGateway;
                    if ( financialGateway != null )
                    {
                        _financialGatewayComponent = financialGateway.GetGatewayComponent() as IHostedGatewayComponent;
                    }
                }

                return _financialGatewayComponent;
            }
        }

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets the host payment information submit JavaScript.
        /// </summary>
        /// <value>
        /// The host payment information submit script.
        /// </value>
        protected string HostPaymentInfoSubmitScript
        {
            get
            {
                return ViewState["HostPaymentInfoSubmitScript"] as string;
            }

            set
            {
                ViewState["HostPaymentInfoSubmitScript"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the payment transaction code.
        /// </summary>
        protected string TransactionCode
        {
            get { return ViewState["TransactionCode"] as string ?? string.Empty; }
            set { ViewState["TransactionCode"] = value; }
        }

        /// <summary>
        /// Gets or sets the Customer Token for a newly created customer token from the payment info control.
        /// NOTE: Lets encrypt this since we don't want the ViewState to have an un-encrypted customer token, even though ViewState is already encrypted.
        /// </summary>
        /// <value>
        /// The customer token (encrypted)
        /// </value>
        protected string CustomerTokenEncrypted
        {
            get { return ViewState["CustomerTokenEncrypted"] as string ?? string.Empty; }
            set { ViewState["CustomerTokenEncrypted"] = value; }
        }

        #endregion Properties

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            BlockUpdated += Block_BlockUpdated;
            AddConfigurationUpdateTrigger( upnlContent );

            bool enableACH = GetAttributeValue( AttributeKey.EnableACH ).AsBoolean();
            bool enableCreditCard = GetAttributeValue( AttributeKey.EnableCreditCard ).AsBoolean();
            if ( FinancialGatewayComponent != null && FinancialGateway != null )
            {
                _hostedPaymentInfoControl = FinancialGatewayComponent.GetHostedPaymentInfoControl( FinancialGateway, "_hostedPaymentInfoControl", new HostedPaymentInfoControlOptions { EnableACH = enableACH, EnableCreditCard = enableCreditCard } );
                phHostedPaymentControl.Controls.Add( _hostedPaymentInfoControl );
                HostPaymentInfoSubmitScript = FinancialGatewayComponent.GetHostPaymentInfoSubmitScript( FinancialGateway, _hostedPaymentInfoControl );
            }

            if ( _hostedPaymentInfoControl is IHostedGatewayPaymentControlTokenEvent )
            {
                ( _hostedPaymentInfoControl as IHostedGatewayPaymentControlTokenEvent ).TokenReceived += _hostedPaymentInfoControl_TokenReceived;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                // Ensure that there is only one transaction processed by getting a unique guid when this block loads for the first time
                // This will ensure there are no (unintended) duplicate transactions
                hfTransactionGuid.Value = Guid.NewGuid().ToString();
                ShowDetails();
            }
        }

        #endregion Base Control Methods

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            // If block options where changed, reload the whole page since changing some of the options (Gateway ACH Control options ) requires a full page reload
            NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Handles the TokenReceived event of the _hostedPaymentInfoControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _hostedPaymentInfoControl_TokenReceived( object sender, EventArgs e )
        {
            string errorMessage = null;
            string token = FinancialGatewayComponent.GetHostedPaymentInfoToken( FinancialGateway, _hostedPaymentInfoControl, out errorMessage );
            if ( errorMessage.IsNotNullOrWhiteSpace() )
            {
                nbPaymentTokenError.Text = errorMessage;
                nbPaymentTokenError.Visible = true;
            }
            else
            {
                nbPaymentTokenError.Visible = false;
                btnGetPaymentInfoNext_Click( sender, e );
            }
        }

        #endregion

        #region Gateway Help Related

        /// <summary>
        /// Handles the ItemDataBound event of the rptInstalledGateways control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptInstalledGateways_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            IHostedGatewayComponent financialGatewayComponent = e.Item.DataItem as IHostedGatewayComponent;
            if ( financialGatewayComponent == null )
            {
                return;
            }

            var gatewayEntityType = EntityTypeCache.Get( financialGatewayComponent.TypeGuid );
            var gatewayEntityTypeType = gatewayEntityType.GetEntityType();

            HiddenField hfGatewayEntityTypeId = e.Item.FindControl( "hfGatewayEntityTypeId" ) as HiddenField;
            hfGatewayEntityTypeId.Value = gatewayEntityType.Id.ToString();

            Literal lGatewayName = e.Item.FindControl( "lGatewayName" ) as Literal;
            Literal lGatewayDescription = e.Item.FindControl( "lGatewayDescription" ) as Literal;

            lGatewayName.Text = Reflection.GetDisplayName( gatewayEntityTypeType );
            lGatewayDescription.Text = Reflection.GetDescription( gatewayEntityTypeType );

            HyperLink aGatewayConfigure = e.Item.FindControl( "aGatewayConfigure" ) as HyperLink;
            HyperLink aGatewayLearnMore = e.Item.FindControl( "aGatewayLearnMore" ) as HyperLink;
            aGatewayConfigure.Visible = financialGatewayComponent.ConfigureURL.IsNotNullOrWhiteSpace();
            aGatewayLearnMore.Visible = financialGatewayComponent.LearnMoreURL.IsNotNullOrWhiteSpace();

            aGatewayConfigure.NavigateUrl = financialGatewayComponent.ConfigureURL;
            aGatewayLearnMore.NavigateUrl = financialGatewayComponent.LearnMoreURL;
        }

        /// <summary>
        /// Loads and Validates the gateways, showing a message if the gateways aren't configured correctly
        /// </summary>
        private bool LoadGatewayOptions()
        {
            if ( FinancialGateway == null )
            {
                ShowGatewayHelp();
                return false;
            }
            else
            {
                HideGatewayHelp();
            }

            // get the FinancialGateway's GatewayComponent so we can show a warning if they have an unsupported gateway.
            bool unsupportedGateway = ( FinancialGateway.GetGatewayComponent() is IHostedGatewayComponent ) == false;

            var testGatewayGuid = Rock.SystemGuid.EntityType.FINANCIAL_GATEWAY_TEST_GATEWAY.AsGuid();

            if ( unsupportedGateway )
            {
                ShowConfigurationMessage( NotificationBoxType.Warning, "Unsupported Gateway", "This block only support Gateways that have a hosted payment interface." );
                pnlTransactionEntry.Visible = false;
                return false;
            }
            else if ( FinancialGatewayComponent.TypeGuid == testGatewayGuid )
            {
                ShowConfigurationMessage( NotificationBoxType.Warning, "Testing", "You are using the Test Financial Gateway. No actual amounts will be charged to your card or bank account." );
            }
            else
            {
                HideConfigurationMessage();
            }

            return true;
        }

        /// <summary>
        /// Shows the gateway help
        /// </summary>
        private void ShowGatewayHelp()
        {
            pnlGatewayHelp.Visible = true;
            pnlTransactionEntry.Visible = false;

            var hostedGatewayComponentList = Rock.Financial.GatewayContainer.Instance.Components
                .Select( a => a.Value.Value )
                .Where( a => a is IHostedGatewayComponent )
                .Select( a => a as IHostedGatewayComponent ).ToList();

            rptInstalledGateways.DataSource = hostedGatewayComponentList;
            rptInstalledGateways.DataBind();
        }

        /// <summary>
        /// Hides the gateway help.
        /// </summary>
        private void HideGatewayHelp()
        {
            pnlGatewayHelp.Visible = false;
        }

        /// <summary>
        /// Shows the configuration message.
        /// </summary>
        /// <param name="notificationBoxType">Type of the notification box.</param>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        private void ShowConfigurationMessage( NotificationBoxType notificationBoxType, string title, string message )
        {
            nbConfigurationNotification.NotificationBoxType = notificationBoxType;
            nbConfigurationNotification.Title = title;
            nbConfigurationNotification.Text = message;

            nbConfigurationNotification.Visible = true;
        }

        /// <summary>
        /// Hides the configuration message.
        /// </summary>
        private void HideConfigurationMessage()
        {
            nbConfigurationNotification.Visible = false;
        }

        #endregion Gateway Guide Related

        #region Transaction Entry Related

        /// <summary>
        /// Handles the Click event of the btnSaveAccount control.
        /// </summary>
        protected void SaveAccount()
        {
            var rockContext = new RockContext();
            var targetPerson = GetTargetPerson( rockContext );

            if ( targetPerson == null )
            {
                return;
            }

            var savedAccountService = new FinancialPersonSavedAccountService( rockContext );
            var selectedSavedAccountId = ddlPersonSavedAccount.SelectedValue.AsIntegerOrNull();
            var usingSavedAccount = selectedSavedAccountId.HasValue && selectedSavedAccountId.Value > 0;

            if ( usingSavedAccount )
            {
                var existingSavedAccounts = savedAccountService.GetByPersonId( targetPerson.Id );

                foreach ( var existingSavedAccount in existingSavedAccounts )
                {
                    existingSavedAccount.IsDefault = existingSavedAccount.Id == selectedSavedAccountId;
                }

                rockContext.SaveChanges();
                return;
            }

            var financialGatewayComponent = FinancialGatewayComponent;
            var financialGateway = FinancialGateway;

            var financialTransaction = new FinancialTransactionService( rockContext ).Get( hfTransactionGuid.Value.AsGuid() );

            var gatewayPersonIdentifier = Rock.Security.Encryption.DecryptString( CustomerTokenEncrypted );

            var savedAccount = new FinancialPersonSavedAccount();
            var paymentDetail = financialTransaction.FinancialPaymentDetail;

            savedAccount.IsDefault = true;
            savedAccount.PersonAliasId = targetPerson.PrimaryAliasId;
            savedAccount.ReferenceNumber = gatewayPersonIdentifier;
            savedAccount.Name = tbSaveAccount.Text;
            savedAccount.TransactionCode = TransactionCode;
            savedAccount.GatewayPersonIdentifier = gatewayPersonIdentifier;
            savedAccount.FinancialGatewayId = financialGateway.Id;
            savedAccount.FinancialPaymentDetail = new FinancialPaymentDetail();
            savedAccount.FinancialPaymentDetail.AccountNumberMasked = paymentDetail.AccountNumberMasked;
            savedAccount.FinancialPaymentDetail.CurrencyTypeValueId = paymentDetail.CurrencyTypeValueId;
            savedAccount.FinancialPaymentDetail.CreditCardTypeValueId = paymentDetail.CreditCardTypeValueId;
            savedAccount.FinancialPaymentDetail.NameOnCardEncrypted = paymentDetail.NameOnCardEncrypted;
            savedAccount.FinancialPaymentDetail.ExpirationMonthEncrypted = paymentDetail.ExpirationMonthEncrypted;
            savedAccount.FinancialPaymentDetail.ExpirationYearEncrypted = paymentDetail.ExpirationYearEncrypted;
            savedAccount.FinancialPaymentDetail.BillingLocationId = paymentDetail.BillingLocationId;

            savedAccountService.Add( savedAccount );
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Shows the details.
        /// </summary>
        private void ShowDetails()
        {
            if ( !LoadGatewayOptions() )
            {
                return;
            }

            aHistoryBackButton.Visible = false;
            if ( GetAttributeValue( AttributeKey.EnableInitialBackButton ).AsBoolean() )
            {
                if ( Request.UrlReferrer != null )
                {
                    aHistoryBackButton.HRef = Request.UrlReferrer.ToString();
                    aHistoryBackButton.Visible = true;
                }
                else
                {
                    aHistoryBackButton.HRef = "#";
                }
            }

            var rockContext = new RockContext();
            List<int> selectableAccountIds = new FinancialAccountService( rockContext ).GetByGuids( GetAttributeValues( AttributeKey.AccountsToDisplay ).AsGuidList() ).Select( a => a.Id ).ToList();
            CampusAccountAmountPicker.AccountIdAmount[] accountAmounts = null;
            caapPromptForAccountAmounts.AmountEntryMode = CampusAccountAmountPicker.AccountAmountEntryMode.SingleAccount;

            caapPromptForAccountAmounts.AskForCampusIfKnown = GetAttributeValue( AttributeKey.AskForCampusIfKnown ).AsBoolean();

            caapPromptForAccountAmounts.SelectableAccountIds = selectableAccountIds.ToArray();
            if ( accountAmounts != null )
            {
                caapPromptForAccountAmounts.AccountAmounts = accountAmounts;
            }

            // if Gateways are configured, show a warning if no Accounts are configured (we don't want to show an Accounts warning if they haven't configured a gateway yet)
            if ( !caapPromptForAccountAmounts.SelectableAccountIds.Any() )
            {
                ShowConfigurationMessage( NotificationBoxType.Warning, "Configuration", "At least one Financial Account must be selected in the configuration for this block." );
                pnlTransactionEntry.Visible = false;
                return;
            }

            bool enableACH = GetAttributeValue( AttributeKey.EnableACH ).AsBoolean();
            bool enableCreditCard = GetAttributeValue( AttributeKey.EnableCreditCard ).AsBoolean();

            if ( enableACH == false && enableCreditCard == false )
            {
                ShowConfigurationMessage( NotificationBoxType.Warning, "Configuration", "Enable ACH and/or Enable Credit Card needs to be enabled." );
                pnlTransactionEntry.Visible = false;
                return;
            }

            pnlTransactionEntry.Visible = true;
            SetInitialTargetPersonControls();

            string introMessageTemplate = GetAttributeValue( AttributeKey.IntroMessageTemplate );

            Dictionary<string, object> introMessageMergeFields = null;

            introMessageMergeFields = LavaHelper.GetCommonMergeFields( RockPage );
            lIntroMessage.Text = introMessageTemplate.ResolveMergeFields( introMessageMergeFields );

            UpdateGivingControlsForSelections();
        }

        /// <summary>
        /// Initializes the UI based on the initial target person.
        /// </summary>
        private void SetInitialTargetPersonControls()
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );

            Person targetPerson = null;
            var personKey = PageParameter( PageParameterKey.Person );

            if ( personKey.IsNotNullOrWhiteSpace() )
            {
                var incrementKeyUsage = !IsPostBack;
                targetPerson = personService.GetByImpersonationToken( personKey, incrementKeyUsage, PageCache.Id );
            }

            if ( targetPerson == null )
            {
                nbInvalidPersonWarning.Text = "Invalid or Expired Person Token specified";
                nbInvalidPersonWarning.NotificationBoxType = NotificationBoxType.Danger;
                nbInvalidPersonWarning.Visible = true;
                pnlTransactionEntry.Visible = false;
                return;
            }

            hfTargetPersonId.Value = targetPerson.Id.ToString();
            SetAccountPickerCampus( targetPerson );

            lCurrentPersonFullName.Text = targetPerson.FullName;
            tbFirstName.Text = targetPerson.FirstName;
            tbLastName.Text = targetPerson.LastName;
            tbEmailIndividual.Text = targetPerson.Email;
            var addressTypeId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME );

            GroupLocation personGroupLocation = null;
            if ( addressTypeId.HasValue )
            {
                personGroupLocation = personService.GetFirstLocation( targetPerson.Id, addressTypeId.Value );
            }

            if ( personGroupLocation != null )
            {
                acAddressIndividual.SetValues( personGroupLocation.Location );
            }
            else
            {
                acAddressIndividual.SetValues( null );
            }
        }

        /// <summary>
        /// Gets the target person.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private Person GetTargetPerson( RockContext rockContext )
        {
            int? targetPersonId = hfTargetPersonId.Value.AsIntegerOrNull();
            if ( targetPersonId == null )
            {
                return null;
            }

            var targetPerson = new PersonService( rockContext ).Get( targetPersonId.Value );
            return targetPerson;
        }

        /// <summary>
        /// Updates the person from input information collected (Phone, Email, Address) and saves changes (if any) to the database..
        /// </summary>
        /// <param name="person">The person.</param>
        private void UpdatePersonFromInputInformation( Person person )
        {
            person.Email = tbEmailIndividual.Text;
            var primaryFamily = person.GetFamily();

            if ( primaryFamily != null )
            {
                var rockContext = new RockContext();

                // fetch primaryFamily using rockContext so that any changes will get saved
                primaryFamily = new GroupService( rockContext ).Get( primaryFamily.Id );

                GroupService.AddNewGroupAddress(
                    rockContext,
                    primaryFamily,
                    Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME,
                    acAddressIndividual.Street1,
                    acAddressIndividual.Street2,
                    acAddressIndividual.City,
                    acAddressIndividual.State,
                    acAddressIndividual.PostalCode,
                    acAddressIndividual.Country,
                    true );
            }
        }

        /// <summary>
        /// Binds the person saved accounts that are available for the person
        /// </summary>
        private void BindPersonSavedAccounts()
        {
            ddlPersonSavedAccount.Visible = false;
            var currentSavedAccountSelection = ddlPersonSavedAccount.SelectedValue;

            int? targetPersonId = hfTargetPersonId.Value.AsIntegerOrNull();
            if ( targetPersonId == null )
            {
                return;
            }

            var rockContext = new RockContext();
            var personSavedAccountsQuery = new FinancialPersonSavedAccountService( rockContext )
                .GetByPersonId( targetPersonId.Value )
                .Where( a => !a.IsSystem )
                .AsNoTracking();

            var financialGateway = FinancialGateway;
            var financialGatewayComponent = FinancialGatewayComponent;
            if ( financialGateway == null || financialGatewayComponent == null )
            {
                return;
            }

            bool enableACH = GetAttributeValue( AttributeKey.EnableACH ).AsBoolean();
            bool enableCreditCard = GetAttributeValue( AttributeKey.EnableCreditCard ).AsBoolean();
            var creditCardCurrency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid() );
            var achCurrency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH.AsGuid() );
            List<DefinedValueCache> allowedCurrencyTypes = new List<DefinedValueCache>();

            if ( enableCreditCard && financialGatewayComponent.SupportsSavedAccount( creditCardCurrency ) )
            {
                allowedCurrencyTypes.Add( creditCardCurrency );
            }

            if ( enableACH && financialGatewayComponent.SupportsSavedAccount( achCurrency ) )
            {
                allowedCurrencyTypes.Add( achCurrency );
            }

            int[] allowedCurrencyTypeIds = allowedCurrencyTypes.Select( a => a.Id ).ToArray();

            personSavedAccountsQuery = personSavedAccountsQuery.Where( a =>
                a.FinancialGatewayId == financialGateway.Id
                && ( a.FinancialPaymentDetail.CurrencyTypeValueId != null )
                && allowedCurrencyTypeIds.Contains( a.FinancialPaymentDetail.CurrencyTypeValueId.Value ) );

            var personSavedAccountList = personSavedAccountsQuery.OrderBy( a => a.Name ).AsNoTracking().Select( a => new
            {
                a.Id,
                a.Name,
                a.FinancialPaymentDetail.AccountNumberMasked,
            } ).ToList();

            // Only show the SavedAccount picker if there are saved accounts. If there aren't any (or if they choose 'Use a different payment method'), a later step will prompt them to enter Payment Info (CC/ACH fields)
            ddlPersonSavedAccount.Visible = personSavedAccountList.Any();

            ddlPersonSavedAccount.Items.Clear();
            foreach ( var personSavedAccount in personSavedAccountList )
            {
                var displayName = string.Format( "{0} ({1})", personSavedAccount.Name, personSavedAccount.AccountNumberMasked );
                ddlPersonSavedAccount.Items.Add( new ListItem( displayName, personSavedAccount.Id.ToString() ) );
            }

            ddlPersonSavedAccount.Items.Add( new ListItem( "Use a different payment method", 0.ToString() ) );

            if ( currentSavedAccountSelection.IsNotNullOrWhiteSpace() )
            {
                ddlPersonSavedAccount.SetValue( currentSavedAccountSelection );
            }
            else
            {
                ddlPersonSavedAccount.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Sets the account picker campus from person
        /// </summary>
        private void SetAccountPickerCampus( Person person )
        {
            int? defaultCampusId = null;

            if ( person != null )
            {
                var personCampus = person.GetCampus();
                if ( personCampus != null )
                {
                    defaultCampusId = personCampus.Id;
                }
            }

            caapPromptForAccountAmounts.CampusId = defaultCampusId;
        }

        /// <summary>
        /// Determines if a Person's Saved Account was used as the payment method
        /// </summary>
        /// <returns></returns>
        private bool UsingPersonSavedAccount()
        {
            return ddlPersonSavedAccount.SelectedValue.AsInteger() > 0;
        }

        /// <summary>
        /// Navigates to step.
        /// </summary>
        /// <param name="entryStep">The entry step.</param>
        private void NavigateToStep( EntryStep entryStep )
        {
            pnlPromptForAmounts.Visible = entryStep == EntryStep.PromptForAmounts;

            pnlAmountSummary.Visible = entryStep == EntryStep.GetPersonalInformation
                || entryStep == EntryStep.GetPaymentInfo;

            pnlPersonalInformation.Visible = entryStep == EntryStep.GetPersonalInformation;
            pnlPaymentInfo.Visible = entryStep == EntryStep.GetPaymentInfo;
            pnlTransactionSummary.Visible = entryStep == EntryStep.ShowTransactionSummary;
        }

        /// <summary>
        /// Updates the giving controls based on what options are selected in the UI
        /// </summary>
        private void UpdateGivingControlsForSelections()
        {
            nbPromptForAmountsWarning.Visible = false;
            BindPersonSavedAccounts();
        }

        /// <summary>
        /// Handles the SelectionChanged event of the ddlPersonSavedAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlPersonSavedAccount_SelectionChanged( object sender, EventArgs e )
        {
            UpdateGivingControlsForSelections();
        }

        /// <summary>
        /// Processes the transaction.
        /// </summary>
        /// <returns></returns>
        protected void ProcessTransaction()
        {
            var transactionGuid = hfTransactionGuid.Value.AsGuid();
            var rockContext = new RockContext();

            // to make duplicate transactions impossible, make sure that our Transaction hasn't already been processed
            bool transactionAlreadyExists = new FinancialTransactionService( rockContext ).Queryable().Any( a => a.Guid == transactionGuid );

            if ( transactionAlreadyExists )
            {
                ShowTransactionSummary();
            }

            var financialGatewayComponent = FinancialGatewayComponent;
            string errorMessage;
            var paymentInfo = CreatePaymentInfoFromControls();
            nbProcessTransactionError.Visible = false;

            // use the paymentToken as the reference number for creating the customer account
            var savedAccountId = ddlPersonSavedAccount.SelectedValue.AsIntegerOrNull();
            if ( savedAccountId.HasValue && savedAccountId.Value > 0 )
            {
                var financialPersonSavedAccount = new FinancialPersonSavedAccountService( rockContext ).Get( savedAccountId.Value );

                if ( financialPersonSavedAccount != null && financialPersonSavedAccount.ReferenceNumber.IsNotNullOrWhiteSpace() )
                {
                    paymentInfo.GatewayPersonIdentifier = financialPersonSavedAccount.ReferenceNumber;
                }
            }

            if ( paymentInfo.GatewayPersonIdentifier.IsNullOrWhiteSpace() )
            {
                var paymentToken = financialGatewayComponent.GetHostedPaymentInfoToken( FinancialGateway, _hostedPaymentInfoControl, out errorMessage );
                var customerToken = financialGatewayComponent.CreateCustomerAccount( FinancialGateway, paymentToken, paymentInfo, out errorMessage );
                if ( errorMessage.IsNotNullOrWhiteSpace() || customerToken.IsNullOrWhiteSpace() )
                {
                    nbProcessTransactionError.Text = errorMessage ?? "Unknown Error";
                    nbProcessTransactionError.Visible = true;
                    return;
                }

                paymentInfo.GatewayPersonIdentifier = customerToken;

                // save the customer token in view state since we'll need it in case they create a saved account
                CustomerTokenEncrypted = Rock.Security.Encryption.EncryptString( customerToken );
            }

            // determine or create the Person record that this transaction is for
            var targetPerson = GetTargetPerson( rockContext );
            int transactionPersonId;

            if ( targetPerson == null )
            {
                nbProcessTransactionError.Text = "The person token is invalid";
                nbProcessTransactionError.Visible = true;
                return;
            }

            UpdatePersonFromInputInformation( targetPerson );
            transactionPersonId = targetPerson.Id;

            nbProcessTransactionError.Visible = false;

            string transactionCode = null;
            try
            {
                FinancialTransaction financialTransaction = FinancialGatewayComponent.Charge( FinancialGateway, paymentInfo, out errorMessage );
                if ( financialTransaction == null )
                {
                    if ( errorMessage.IsNullOrWhiteSpace() )
                    {
                        errorMessage = "Unknown Error";
                    }

                    nbProcessTransactionError.Text = errorMessage;
                    nbProcessTransactionError.Visible = true;
                    return;
                }

                transactionCode = financialTransaction.TransactionCode;

                SaveTransaction( transactionPersonId, paymentInfo, financialTransaction );
            }
            catch ( Exception ex )
            {
                throw new Exception( string.Format( "Error occurred when saving financial transaction for gateway payment with a transactionCode of {0} and FinancialTransaction with Guid of {1}.", transactionCode, transactionGuid ), ex );
            }

            ShowTransactionSummary();
        }

        /// <summary>
        /// Creates a PaymentInfo object from the information collected in the UI
        /// </summary>
        /// <returns></returns>
        private ReferencePaymentInfo CreatePaymentInfoFromControls()
        {
            var paymentInfo = new ReferencePaymentInfo
            {
                Email = tbEmailIndividual.Text
            };

            paymentInfo.UpdateAddressFieldsFromAddressControl( acAddressIndividual );
            paymentInfo.FirstName = tbFirstName.Text;
            paymentInfo.LastName = tbLastName.Text;

            var commentTransactionAccountDetails = new List<FinancialTransactionDetail>();
            PopulateTransactionDetails( commentTransactionAccountDetails );

            paymentInfo.Comment1 = "Text-to-give setup";

            return paymentInfo;
        }

        /// <summary>
        /// Shows the transaction summary.
        /// </summary>
        /// <param name="financialTransaction">The financial transaction.</param>
        /// <param name="paymentInfo">The payment information.</param>
        protected void ShowTransactionSummary()
        {
            var rockContext = new RockContext();
            var transactionGuid = hfTransactionGuid.Value.AsGuid();

            var mergeFields = LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson, new CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
            var finishLavaTemplate = GetAttributeValue( AttributeKey.FinishLavaTemplate );

            int? financialPaymentDetailId = null;
            FinancialPaymentDetail financialPaymentDetail = null;
            FinancialTransaction financialTransaction = new FinancialTransactionService( rockContext ).Get( transactionGuid );

            if ( financialTransaction != null )
            {
                mergeFields.Add( "Transaction", financialTransaction );
                mergeFields.Add( "Person", financialTransaction.AuthorizedPersonAlias.Person );
                financialPaymentDetail = financialTransaction.FinancialPaymentDetail;
                financialPaymentDetailId = financialTransaction.FinancialGatewayId;
            }

            if ( financialPaymentDetail != null || financialPaymentDetailId.HasValue )
            {
                financialPaymentDetail = financialPaymentDetail ?? new FinancialPaymentDetailService( rockContext ).GetNoTracking( financialPaymentDetailId.Value );
                mergeFields.Add( "PaymentDetail", financialPaymentDetail );

                if ( financialPaymentDetail.BillingLocation != null || financialPaymentDetail.BillingLocationId.HasValue )
                {
                    var billingLocation = financialPaymentDetail.BillingLocation ?? new LocationService( rockContext ).GetNoTracking( financialPaymentDetail.BillingLocationId.Value );
                    mergeFields.Add( "BillingLocation", billingLocation );
                }
            }

            lTransactionSummaryHTML.Text = finishLavaTemplate.ResolveMergeFields( mergeFields );
            NavigateToStep( EntryStep.ShowTransactionSummary );
        }

        /// <summary>
        /// Saves the transaction.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="transaction">The transaction.</param>
        private void SaveTransaction( int personId, PaymentInfo paymentInfo, FinancialTransaction transaction )
        {
            FinancialGateway financialGateway = FinancialGateway;
            IHostedGatewayComponent gateway = FinancialGatewayComponent;
            var rockContext = new RockContext();

            // manually assign the Guid that we generated at the beginning of the transaction UI entry to help make duplicate transactions impossible
            transaction.Guid = hfTransactionGuid.Value.AsGuid();

            transaction.AuthorizedPersonAliasId = new PersonAliasService( rockContext ).GetPrimaryAliasId( personId );
            transaction.TransactionDateTime = RockDateTime.Now;
            transaction.FinancialGatewayId = financialGateway.Id;

            var txnType = DefinedValueCache.Get( GetAttributeValue( AttributeKey.TransactionType ).AsGuidOrNull() ?? Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );
            transaction.TransactionTypeValueId = txnType.Id;

            transaction.Summary = paymentInfo.Comment1;

            if ( transaction.FinancialPaymentDetail == null )
            {
                transaction.FinancialPaymentDetail = new FinancialPaymentDetail();
            }

            transaction.FinancialPaymentDetail.SetFromPaymentInfo( paymentInfo, gateway as GatewayComponent, rockContext );
            transaction.SourceTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_SMS_GIVE );

            PopulateTransactionDetails( transaction.TransactionDetails );

            var batchService = new FinancialBatchService( rockContext );

            // Get the batch
            var batch = batchService.Get(
                GetAttributeValue( AttributeKey.BatchNamePrefix ),
                paymentInfo.CurrencyTypeValue,
                paymentInfo.CreditCardTypeValue,
                transaction.TransactionDateTime.Value,
                financialGateway.GetBatchTimeOffset() );

            var batchChanges = new History.HistoryChangeList();

            if ( batch.Id == 0 )
            {
                batchChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Batch" );
                History.EvaluateChange( batchChanges, "Batch Name", string.Empty, batch.Name );
                History.EvaluateChange( batchChanges, "Status", null, batch.Status );
                History.EvaluateChange( batchChanges, "Start Date/Time", null, batch.BatchStartDateTime );
                History.EvaluateChange( batchChanges, "End Date/Time", null, batch.BatchEndDateTime );
            }

            decimal newControlAmount = batch.ControlAmount + transaction.TotalAmount;
            History.EvaluateChange( batchChanges, "Control Amount", batch.ControlAmount.FormatAsCurrency(), newControlAmount.FormatAsCurrency() );
            batch.ControlAmount = newControlAmount;

            var financialTransactionService = new FinancialTransactionService( rockContext );

            // If this is a new Batch, SaveChanges so that we can get the Batch.Id
            if ( batch.Id == 0 )
            {
                rockContext.SaveChanges();
            }

            transaction.BatchId = batch.Id;

            // use the financialTransactionService to add the transaction instead of batch.Transactions to avoid lazy-loading the transactions already associated with the batch
            financialTransactionService.Add( transaction );
            rockContext.SaveChanges();

            transaction.SaveAttributeValues();

            HistoryService.SaveChanges(
                rockContext,
                typeof( FinancialBatch ),
                Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                batch.Id,
                batchChanges );

            SendReceipt( transaction.Id );

            TransactionCode = transaction.TransactionCode;
        }

        /// <summary>
        /// Populates the transaction details
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="transactionDetails">The transaction details.</param>
        private void PopulateTransactionDetails<T>( ICollection<T> transactionDetails ) where T : ITransactionDetail, new()
        {
            var selectedAccountAmounts = caapPromptForAccountAmounts.AccountAmounts;

            foreach ( var selectedAccountAmount in selectedAccountAmounts.Where( a => a.Amount.HasValue && a.Amount != 0 ) )
            {
                var transactionDetail = new T();
                transactionDetail.Amount = selectedAccountAmount.Amount.Value;
                transactionDetail.AccountId = selectedAccountAmount.AccountId;
                transactionDetails.Add( transactionDetail );
            }
        }

        /// <summary>
        /// Sends the receipt.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        private void SendReceipt( int transactionId )
        {
            Guid? receiptEmail = GetAttributeValue( AttributeKey.ReceiptEmail ).AsGuidOrNull();
            if ( receiptEmail.HasValue )
            {
                // Queue a transaction to send receipts
                var newTransactionIds = new List<int> { transactionId };
                var sendPaymentReceiptsTxn = new Rock.Transactions.SendPaymentReceipts( receiptEmail.Value, newTransactionIds );
                Rock.Transactions.RockQueue.TransactionQueue.Enqueue( sendPaymentReceiptsTxn );
            }
        }

        #endregion Transaction Entry Related

        #region Person

        /// <summary>
        /// Update the person, indicated by the person token, with the names, email, and designated accountId
        /// </summary>
        private void SavePerson()
        {
            var rockContext = new RockContext();
            var person = GetTargetPerson( rockContext );

            if ( person == null )
            {
                return;
            }

            person.Email = tbEmailIndividual.Text;
            person.FirstName = tbFirstName.Text;
            person.LastName = tbLastName.Text;

            var selectedAccountAmounts = caapPromptForAccountAmounts.AccountAmounts;
            var firstAccountAmount = selectedAccountAmounts.FirstOrDefault( a => a.Amount.HasValue && a.Amount != 0 );
            person.ContributionFinancialAccountId = firstAccountAmount == null ? ( int? ) null : firstAccountAmount.AccountId;

            rockContext.SaveChanges();
        }

        #endregion Person

        #region Navigation

        /// <summary>
        /// Handles the Click event of the btnGiveNow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnGiveNow_Click( object sender, EventArgs e )
        {
            if ( caapPromptForAccountAmounts.IsValidAmountSelected() )
            {
                nbPromptForAmountsWarning.Visible = false;
                pnlPersonalInformation.Visible = false;
                var totalAmount = caapPromptForAccountAmounts.AccountAmounts.Sum( a => a.Amount ?? 0.00M );

                // get the accountId(s) that have an amount specified
                var amountAccountIds = caapPromptForAccountAmounts.AccountAmounts
                    .Where( a => a.Amount.HasValue && a.Amount != 0.00M ).Select( a => a.AccountId )
                    .ToList();

                var accountNames = new FinancialAccountService( new RockContext() )
                    .GetByIds( amountAccountIds )
                    .Select( a => a.PublicName )
                    .ToList().AsDelimited( ", ", " and " );

                lAmountSummaryAccounts.Text = accountNames;
                lAmountSummaryAmount.Text = totalAmount.FormatAsCurrency();
                if ( caapPromptForAccountAmounts.CampusId.HasValue )
                {
                    lAmountSummaryCampus.Text = CampusCache.Get( caapPromptForAccountAmounts.CampusId.Value ).Name;
                }

                if ( UsingPersonSavedAccount() )
                {
                    NavigateToStep( EntryStep.GetPersonalInformation );
                }
                else
                {
                    NavigateToStep( EntryStep.GetPaymentInfo );
                }
            }
            else
            {
                nbPromptForAmountsWarning.Visible = true;
                nbPromptForAmountsWarning.Text = "Please specify an amount";
            }
        }

        /// <summary>
        /// Handles the Click event of the btnGetPaymentInfoBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnGetPaymentInfoBack_Click( object sender, EventArgs e )
        {
            NavigateToStep( EntryStep.PromptForAmounts );
        }

        /// <summary>
        /// Handles the Click event of the btnGetPaymentInfoNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnGetPaymentInfoNext_Click( object sender, EventArgs e )
        {
            //// NOTE: the btnGetPaymentInfoNext button tells _hostedPaymentInfoControl to get a token via JavaScript
            //// When _hostedPaymentInfoControl gets a token response, the _hostedPaymentInfoControl_TokenReceived event will be triggered
            //// If _hostedPaymentInfoControl_TokenReceived gets a valid token, it will call btnGetPaymentInfoNext_Click

            nbProcessTransactionError.Visible = false;
            NavigateToStep( EntryStep.GetPersonalInformation );
        }

        /// <summary>
        /// Handles the Click event of the btnPersonalInformationBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnPersonalInformationBack_Click( object sender, EventArgs e )
        {
            if ( UsingPersonSavedAccount() )
            {
                NavigateToStep( EntryStep.PromptForAmounts );
            }
            else
            {
                NavigateToStep( EntryStep.GetPaymentInfo );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnPersonalInformationNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnPersonalInformationNext_Click( object sender, EventArgs e )
        {
            ProcessTransaction();
            SavePerson();
            SaveAccount();
        }

        #endregion navigation
    }
}