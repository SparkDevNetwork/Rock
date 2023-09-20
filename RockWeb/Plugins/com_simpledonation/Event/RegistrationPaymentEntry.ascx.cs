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
using System.Web.UI;
using System.Web.UI.WebControls;
using com.SimpleDonation.Model;
using com.SimpleDonation.Services;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Lava;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_simpledonation.Event
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Registration Payment Entry" )]
    [Category( "Simple Donation" )]
    [Description( "Block for users to make payments to their registrations." )]

    #region Block Attributes

    [BooleanField(
        "Show Inactive Registrations",
        Key = AttributeKey.ShowInactiveRegistrations,
        DefaultBooleanValue = true,
        Order = 0 )]

    [BooleanField(
        "Show Paid Registrations",
        Key = AttributeKey.ShowPaidRegistrations,
        DefaultBooleanValue = true,
        Order = 1 )]

    [BooleanField(
        "Impersonation",
        Key = AttributeKey.Impersonation,
        TrueText = "Allow (only use on an internal page used by staff)",
        FalseText = "Don't Allow",
        Description = "Should the current user be able to view and edit other people's transactions?  IMPORTANT: This should only be enabled on an internal page that is secured to trusted users",
        DefaultBooleanValue = false,
        Order = 2 )]

    [TextField( "Registration Parameter",
        Key = AttributeKey.RegistrationParameter,
        Description = "The Page Parameter that will be used to prefill the Registration field",
        IsRequired = true,
        DefaultValue = "RegistrationId",
        Order = 3 )]

    [TextField( "Payment Date Parameter",
        Key = AttributeKey.PaymentDateParameter,
        Description = "The Page Parameter that will be used to prefill the Payment Date field",
        IsRequired = true,
        DefaultValue = "PaymentDate",
        Order = 4 )]

    [TextField( "Frequency Parameter",
        Key = AttributeKey.FrequencyParameter,
        Description = "The Page Parameter that will be used to prefill the Frequency field",
        IsRequired = true,
        DefaultValue = "Frequency",
        Order = 5 )]

    [BooleanField( "Cover Fees Checked",
        Key = AttributeKey.AreCoverFeesChecked,
        Description = "Determines whether the 'cover fees' checkbox is checked by default",
        DefaultBooleanValue = false,
        IsRequired = true,
        Order = 6 )]

    [BooleanField( "Cover Fees Visible",
        Key = AttributeKey.AreCoverFeesVisible,
        Description = "Determines whether the 'cover fees' checkbox will be displayed",
        DefaultBooleanValue = true,
        IsRequired = true,
        Order = 7 )]

    [BooleanField( "Show Credit Card as Default",
        Key = AttributeKey.ShowCCDefault,
        Description = "If set to 'Yes', Credit Cards will be selected as the default payment type in the giving form",
        DefaultBooleanValue = true,
        IsRequired = true,
        Order = 8 )]

    [BooleanField( "Enable ACH",
        Key = AttributeKey.EnableACH,
        Description = "If true, Event Registrations can be paid via ACH",
        DefaultBooleanValue = false,
        IsRequired = true,
        Order = 9 )]

    [WorkflowTypeField( "Transaction Error Workflow",
        Key = AttributeKey.TransactionErrorWorkflow,
        Description = "An optional workflow to launch if saving the transaction errors out after the transaction was sent to the financial provider.",
        AllowMultiple = false,
        IsRequired = false,
        DefaultValue = "",
        Order = 10 )]

    [TextField( "No Registrations Found Warning",
        Key = AttributeKey.NoRegistrationsWarning,
        Description = "The warning text displayed when no registrations are found for a person.",
        IsRequired = true,
        DefaultValue = "No applicable registrations could be found for your account.",
        Order = 11 )]

    #endregion Block Attributes
    public partial class RegistrationPaymentEntry : Rock.Web.UI.RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string CCGateway = "CCGateway";
            public const string ACHGateway = "ACHGateway";
            public const string ShowInactiveRegistrations = "ShowInactiveRegistrations";
            public const string ShowPaidRegistrations = "ShowPaidRegistrations";
            public const string Impersonation = "Impersonation";
            public const string RegistrationParameter = "RegistrationParameter";
            public const string PaymentDateParameter = "PaymentDateParameter";
            public const string FrequencyParameter = "FrequencyParameter";
            public const string AreCoverFeesChecked = "AreCoverFeesChecked";
            public const string AreCoverFeesVisible = "AreCoverFeesVisible";
            public const string ShowCCDefault = "ShowCCDefault";
            public const string EnableACH = "EnableACH";
            public const string TransactionErrorWorkflow = "TransactionErrorWorkflow";
            public const string NoRegistrationsWarning = "NoRegistrationsWarning";
        }

        #endregion Attribute Keys

        #region Fields

        private Person _targetPerson = null;

        private IFeeCalculationService _feeCalculationService = new ProRataFeeCalculationService();
        private IAccountService _accountService = new SimpleDonationAccountService();
        private Guid _firstFifteenthGuid = com.SimpleDonation.Constants.SystemGuid.FIRST_AND_FIFTEENTH_DEFINED_VALUE_GUID.AsGuid();
        private Guid _firstOfTheMonthGuid = com.SimpleDonation.Constants.SystemGuid.FIRST_OF_THE_MONTH_DEFINED_VALUE_GUID.AsGuid();

        protected string _organizationName = null;
        protected bool _coverFeesVisible = true;
        public const string ShowCCdefault = "ShowCCdefault";

        private Registration _registration;

        #endregion

        #region Properties

        protected string TransactionCode
        {
            get { return ViewState["TransactionCode"] as string ?? string.Empty; }
            set { ViewState["TransactionCode"] = value; }
        }

        protected int? ScheduleId
        {
            get { return ViewState["ScheduleId"] as int?; }
            set { ViewState["ScheduleId"] = value; }
        }

        protected List<AccountItem> SelectedAccounts
        {
            get
            {
                var accounts = ViewState["SelectedAccounts"] as List<AccountItem>;
                if ( accounts == null )
                {
                    accounts = new List<AccountItem>();
                }

                return accounts;
            }

            set
            {
                ViewState["SelectedAccounts"] = value;
            }
        }

        #endregion

        #region Base Control Methods


        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // Add handler for page navigation
            RockPage page = Page as RockPage;
            if ( page != null )
            {
                page.PageNavigate += page_PageNavigate;
            }

            using ( var rockContext = new RockContext() )
            {
                SetTargetPerson( rockContext );
                LoadRegistrationDropdown( rockContext );
            }

            RegisterScript();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            // Hide the messages on every postback
            nbMessage.Visible = false;
            nbSelectionMessage.Visible = false;
            _organizationName = GlobalAttributesCache.Value( "OrganizationName" ).EscapeQuotes();
            hfOrganizationName.Value = GlobalAttributesCache.Value( "OrganizationName" ).EscapeQuotes();

            if ( !Page.IsPostBack )
            {
                SetScheduleOptions();

                var registration = GetRegistration();
                if ( registration != null )
                {
                    RegisterFeeRates( registration.RegistrationInstance.RegistrationTemplate.FinancialGatewayId );
                }
                SetPaymentOptions();

                SetPage( 1 );

                // If an invalid PersonToken was specified, hide everything except for the error message
                if ( nbInvalidPersonWarning.Visible )
                {
                    pnlSelection.Visible = false;
                }

                if ( registration != null )
                {
                    string title = string.Format( "Configure Scheduled Payments for {0}", registration.RegistrationInstance.Name );

                    RockPage.BrowserTitle = title;
                    RockPage.PageTitle = title;
                    RockPage.Header.Title = title;
                    var pageBreadCrumb = RockPage.PageReference.BreadCrumbs.FirstOrDefault();
                    if ( pageBreadCrumb != null )
                    {
                        pageBreadCrumb.Name = RockPage.PageTitle;
                    }
                }
                else
                {
                    pnlSelection.Visible = false;
                    nbMessage.Heading = "Sorry";
                    nbMessage.Text = string.Format( "<p>{0}</p>", GetAttributeValue( AttributeKey.NoRegistrationsWarning ) );
                    nbMessage.NotificationBoxType = NotificationBoxType.Warning;
                    nbMessage.Visible = true;
                }
            }
            else
            {
                if ( Request.Form["__EVENTARGUMENT"] != null )
                {
                    if ( Request.Form["__EVENTARGUMENT"] == "Token_Complete" )
                    {
                        btnPaymentInfoNext_Click( null, e );
                    }
                }
            }

            // Show or Hide the Credit card entry panel based on if a saved account exists and it's selected or not.
            divNewPayment.Style[HtmlTextWriterStyle.Display] = ( rblSavedAccount.Items.Count == 0 || rblSavedAccount.Items[rblSavedAccount.Items.Count - 1].Selected ) ? "block" : "none";

            if ( hfPaymentTab.Value == "ACH" )
            {
                liCreditCard.RemoveCssClass( "active" );
                liACH.AddCssClass( "active" );
                divCCPaymentInfo.RemoveCssClass( "active" );
                divACHPaymentInfo.AddCssClass( "active" );
            }
            else
            {
                liCreditCard.AddCssClass( "active" );
                liACH.RemoveCssClass( "active" );
                divCCPaymentInfo.AddCssClass( "active" );
                divACHPaymentInfo.RemoveCssClass( "active" );
            }

            // Show billing address based on if billing address checkbox is checked
            divBillingAddress.Style[HtmlTextWriterStyle.Display] = cbBillingAddress.Checked ? "block" : "none";

            // Show save account info based on if checkbox is checked
            divSaveAccount.Style[HtmlTextWriterStyle.Display] = cbSaveAccount.Checked ? "block" : "none";
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

        }

        protected void page_PageNavigate( object sender, HistoryEventArgs e )
        {
            int pageId = e.State["GivingDetail"].AsInteger();
            if ( pageId > 0 )
            {
                SetPage( pageId );
            }
        }

        protected void dtpStartDate_TextChanged( object sender, EventArgs e )
        {
            LoadFrequencyDropdown();
            SetAmountFromFrequencyDropdown();
        }

        protected void btnFrequency_SelectionChanged( object sender, EventArgs e )
        {
            SetAmountFromFrequencyDropdown();
        }

        protected void ddlRegistrations_SelectedIndexChanged( object sender, EventArgs e )
        {
            GetRegistration( true );
            BindSavedAccounts();
            LoadFrequencyDropdown();
            SetAmountFromFrequencyDropdown();
            DisplayExistingPayments();
        }

        protected void lbSaveAccount_Click( object sender, EventArgs e )
        {
            if ( string.IsNullOrWhiteSpace( TransactionCode ) )
            {
                nbSaveAccount.Text = "Sorry, the account information cannot be saved as there's not a valid transaction code to reference";
                nbSaveAccount.Visible = true;
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var registration = GetRegistration( false, rockContext );
                if ( !string.IsNullOrWhiteSpace( txtSaveAccount.Text ) )
                {
                    GatewayComponent gateway = null;
                    if ( registration.RegistrationInstance.RegistrationTemplate != null && registration.RegistrationInstance.RegistrationTemplate.FinancialGateway != null )
                    {
                        gateway = registration.RegistrationInstance.RegistrationTemplate.FinancialGateway.GetGatewayComponent();
                    }

                    if ( gateway != null )
                    {
                        var ccCurrencyType = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD ) );
                        string errorMessage = string.Empty;

                        var person = _targetPerson;
                        string referenceNumber = string.Empty;
                        FinancialPaymentDetail paymentDetail = null;

                        bool isACHTxn = hfPaymentTab.Value == "ACH";
                        var achCurrencyType = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH ) );
                        int? currencyTypeValueId = isACHTxn ? achCurrencyType.Id : ccCurrencyType.Id;
                        gateway.LoadAttributes( rockContext );

                        if ( !ScheduleId.HasValue )
                        {
                            var transaction = new FinancialTransactionService( rockContext ).GetByTransactionCode( registration.RegistrationInstance.RegistrationTemplate.FinancialGateway.Id, TransactionCode );
                            if ( transaction != null && transaction.AuthorizedPersonAlias != null )
                            {
                                referenceNumber = gateway.GetReferenceNumber( transaction, out errorMessage );
                                paymentDetail = transaction.FinancialPaymentDetail;
                            }
                        }
                        else
                        {
                            var scheduledTransaction = new FinancialScheduledTransactionService( rockContext ).Get( ScheduleId.Value );
                            if ( scheduledTransaction != null )
                            {
                                referenceNumber = gateway.GetReferenceNumber( scheduledTransaction, out errorMessage );
                                paymentDetail = scheduledTransaction.FinancialPaymentDetail;
                            }
                        }

                        if ( person != null && paymentDetail != null )
                        {
                            if ( errorMessage.Any() )
                            {
                                nbSaveAccount.Title = "Invalid Transaction";
                                nbSaveAccount.Text = "Sorry, the account information cannot be saved. " + errorMessage;
                                nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                                nbSaveAccount.Visible = true;
                            }
                            else
                            {
                                var savedAccount = new FinancialPersonSavedAccount();
                                savedAccount.PersonAliasId = person.PrimaryAliasId;
                                savedAccount.ReferenceNumber = referenceNumber;
                                savedAccount.Name = txtSaveAccount.Text;
                                savedAccount.TransactionCode = TransactionCode;
                                savedAccount.FinancialGatewayId = registration.RegistrationInstance.RegistrationTemplate.FinancialGatewayId;
                                savedAccount.FinancialPaymentDetail = new FinancialPaymentDetail();
                                savedAccount.FinancialPaymentDetail.AccountNumberMasked = paymentDetail.AccountNumberMasked;
                                savedAccount.FinancialPaymentDetail.CurrencyTypeValueId = paymentDetail.CurrencyTypeValueId;
                                savedAccount.FinancialPaymentDetail.CreditCardTypeValueId = paymentDetail.CreditCardTypeValueId;
                                savedAccount.FinancialPaymentDetail.NameOnCard = paymentDetail.NameOnCard;
                                savedAccount.FinancialPaymentDetail.ExpirationMonth = paymentDetail.ExpirationMonth;
                                savedAccount.FinancialPaymentDetail.ExpirationYear = paymentDetail.ExpirationYear;
                                savedAccount.FinancialPaymentDetail.BillingLocationId = paymentDetail.BillingLocationId;

                                var savedAccountService = new FinancialPersonSavedAccountService( rockContext );
                                savedAccountService.Add( savedAccount );
                                rockContext.SaveChanges();

                                cbSaveAccount.Visible = false;
                                txtSaveAccount.Visible = false;
                                divSaveActions.Visible = false;

                                nbSaveAccount.Title = "Success";
                                nbSaveAccount.Text = "The account has been saved for future use";
                                nbSaveAccount.NotificationBoxType = NotificationBoxType.Success;
                                nbSaveAccount.Visible = true;
                            }
                        }
                        else
                        {
                            nbSaveAccount.Title = "Invalid Transaction";
                            nbSaveAccount.Text = "Sorry, the account information cannot be saved as there's not a valid transaction code to reference.";
                            nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                            nbSaveAccount.Visible = true;
                        }
                    }
                    else
                    {
                        nbSaveAccount.Title = "Invalid Gateway";
                        nbSaveAccount.Text = "Sorry, the financial gateway information for this type of transaction is not valid.";
                        nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                        nbSaveAccount.Visible = true;
                    }
                }
                else
                {
                    nbSaveAccount.Title = "Missing Account Name";
                    nbSaveAccount.Text = "Please enter a name to use for this account.";
                    nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                    nbSaveAccount.Visible = true;
                }
            }
        }

        protected void btnPaymentInfoNext_Click( object sender, EventArgs e )
        {
            string errorMessage = string.Empty;
            if ( ProcessConfirmation( out errorMessage ) )
            {
                SetPage( 2 );
            }
            else
            {
                ShowMessage( NotificationBoxType.Danger, "Payment Error", errorMessage );
            }
        }

        #endregion

        #region Methods
        private bool ProcessConfirmation( out string errorMessage )
        {
            var rockContext = new RockContext();
            var registration = GetRegistration();
            if ( registration != null )
            {
                var financialGateway = registration.RegistrationInstance.RegistrationTemplate.FinancialGateway;
                if ( financialGateway != null )
                {
                    var gatewayComponent = financialGateway.GetGatewayComponent();
                    if ( gatewayComponent != null )
                    {
                        if ( string.IsNullOrWhiteSpace( TransactionCode ) )
                        {
                            var transactionGuid = hfTransactionGuid.Value.AsGuid();

                            bool isACHTxn = hfPaymentTab.Value == "ACH";

                            // only create/update the person if they are giving as a person. If they are giving as a Business, the person shouldn't be created this way
                            Person person = _targetPerson;

                            if ( person == null )
                            {
                                errorMessage = "There was a problem creating the person information";
                                return false;
                            }

                            if ( !person.PrimaryAliasId.HasValue )
                            {
                                errorMessage = "There was a problem creating the person's primary alias";
                                return false;
                            }

                            PaymentInfo paymentInfo = GetPaymentInfo( rockContext, person, out errorMessage );
                            if ( paymentInfo == null )
                            {
                                return false;
                            }

                            paymentInfo.Email = person.Email;

                            PaymentSchedule schedule = GetSchedule();
                            FinancialPaymentDetail paymentDetail = null;

                            FinancialAccount feeAccount = null;
                            registration.RegistrationInstance.LoadAttributes();
                            var feeAccountGuid = registration.RegistrationInstance.GetAttributeValue( "CoverFeeAccount" ).AsGuidOrNull();
                            if ( feeAccountGuid != null )
                            {
                                feeAccount = new FinancialAccountService( rockContext ).Get( feeAccountGuid.Value );
                            }

                            if ( schedule != null )
                            {
                                schedule.PersonId = person.Id;

                                var scheduledTransactionAlreadyExists = new FinancialScheduledTransactionService( rockContext ).Queryable().FirstOrDefault( a => a.Guid == transactionGuid );
                                if ( scheduledTransactionAlreadyExists != null )
                                {
                                    // hopefully shouldn't happen, but just in case the scheduledtransaction already went thru, show the success screen
                                    ShowSuccess( gatewayComponent, person, paymentInfo, schedule, scheduledTransactionAlreadyExists.FinancialPaymentDetail, rockContext );
                                    return true;
                                }

                                var scheduledTransaction = gatewayComponent.AddScheduledPayment( financialGateway, schedule, paymentInfo, out errorMessage );
                                if ( scheduledTransaction == null )
                                {
                                    return false;
                                }

                                // manually assign the Guid that we generated at the beginning of the transaction UI entry to help make duplicate scheduled transactions impossible
                                scheduledTransaction.Guid = transactionGuid;

                                SaveScheduledTransaction( financialGateway, gatewayComponent, person, paymentInfo, schedule, scheduledTransaction, feeAccount, rockContext );
                                paymentDetail = scheduledTransaction.FinancialPaymentDetail.Clone( false );
                            }
                            else
                            {
                                var transactionAlreadyExists = new FinancialTransactionService( rockContext ).Queryable().FirstOrDefault( a => a.Guid == transactionGuid );
                                if ( transactionAlreadyExists != null )
                                {
                                    // hopefully shouldn't happen, but just in case the transaction already went thru, show the success screen
                                    ShowSuccess( gatewayComponent, person, paymentInfo, null, transactionAlreadyExists.FinancialPaymentDetail, rockContext );
                                    return true;
                                }

                                var transaction = gatewayComponent.Charge( financialGateway, paymentInfo, out errorMessage );
                                if ( transaction == null )
                                {
                                    return false;
                                }

                                // manually assign the Guid that we generated at the beginning of the transaction UI entry to help make duplicate transactions impossible
                                transaction.Guid = transactionGuid;

                                try
                                {
                                    SaveTransaction( financialGateway, gatewayComponent, person, paymentInfo, transaction, feeAccount, rockContext );
                                    paymentDetail = transaction.FinancialPaymentDetail.Clone( false );
                                }
                                catch ( Exception ex )
                                {
                                    var transactionErrorWorkflowGuid = GetAttributeValue( AttributeKey.TransactionErrorWorkflow ).AsGuidOrNull();
                                    if ( transactionErrorWorkflowGuid.HasValue )
                                    {
                                        var workflowType = WorkflowTypeCache.Get( transactionErrorWorkflowGuid.Value );
                                        if ( workflowType != null )
                                        {
                                            var workflowTransaction = new Rock.Transactions.LaunchWorkflowTransaction( workflowType.Id );
                                            Dictionary<string, string> workflowAttributeValues = new Dictionary<string, string>();
                                            workflowAttributeValues.Add( "Person", person.PrimaryAlias.Guid.ToString() );
                                            workflowAttributeValues.Add( "Exception", ex.Message );
                                            if ( workflowAttributeValues != null )
                                            {
                                                workflowTransaction.WorkflowAttributeValues = workflowAttributeValues;
                                            }
                                            Rock.Transactions.RockQueue.TransactionQueue.Enqueue( workflowTransaction );

                                            errorMessage = "Your contribution has successfully been charged, but there was a problem saving your transaction to your profile. Church administrators have been notified of the issue and will resolve it shortly. ";
                                            return false;
                                        }
                                    }
                                }
                            }

                            ShowSuccess( gatewayComponent, person, paymentInfo, schedule, paymentDetail, rockContext );

                            return true;
                        }
                        else
                        {
                            errorMessage = string.Empty;
                            return false;
                        }
                    }
                }
            }

            errorMessage = "There was a problem creating the payment gateway information";
            return false;
        }

        private void SaveScheduledTransaction( FinancialGateway financialGateway, GatewayComponent gateway, Person person, PaymentInfo paymentInfo, PaymentSchedule schedule, FinancialScheduledTransaction scheduledTransaction, FinancialAccount feeAccount, RockContext rockContext )
        {
            scheduledTransaction.TransactionFrequencyValueId = schedule.TransactionFrequencyValue.Id;
            scheduledTransaction.StartDate = schedule.StartDate;
            scheduledTransaction.EndDate = schedule.EndDate;
            scheduledTransaction.AuthorizedPersonAliasId = person.PrimaryAliasId.Value;
            scheduledTransaction.FinancialGatewayId = financialGateway.Id;

            if ( scheduledTransaction.FinancialPaymentDetail == null )
            {
                scheduledTransaction.FinancialPaymentDetail = new FinancialPaymentDetail();
            }
            scheduledTransaction.FinancialPaymentDetail.SetFromPaymentInfo( paymentInfo, gateway, rockContext );

            Guid sourceGuid = Guid.Empty;
            if ( Guid.TryParse( GetAttributeValue( "Source" ), out sourceGuid ) )
            {
                var source = DefinedValueCache.Get( sourceGuid );
                if ( source != null )
                {
                    scheduledTransaction.SourceTypeValueId = source.Id;
                }
            }

            var txnType = DefinedValueCache.Get( this.GetAttributeValue( "TransactionType" ).AsGuidOrNull() ?? Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );
            scheduledTransaction.TransactionTypeValueId = txnType.Id;
            scheduledTransaction.Summary = paymentInfo.Comment1;

            var registration = GetRegistration();
            if ( registration != null )
            {
                var contributionTxnType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );
                var registrationTxnType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_EVENT_REGISTRATION.AsGuid() );

                scheduledTransaction.TransactionTypeValueId = registration.RegistrationInstance.Account.IsTaxDeductible ? contributionTxnType.Id : registrationTxnType.Id;
            }

            var changeSummary = new StringBuilder();
            changeSummary.AppendFormat( "{0} starting {1}", schedule.TransactionFrequencyValue.Value, schedule.StartDate.ToShortDateString() );
            changeSummary.AppendLine();
            changeSummary.Append( paymentInfo.CurrencyTypeValue.Value );
            if ( paymentInfo.CreditCardTypeValue != null )
            {
                changeSummary.AppendFormat( " - {0}", paymentInfo.CreditCardTypeValue.Value );
            }
            changeSummary.AppendFormat( " {0}", paymentInfo.MaskedNumber );
            changeSummary.AppendLine();

            decimal feeAmount;
            FeeCalculationResult feeResult = null;

            if ( cbCoverFees.Checked && decimal.TryParse( hfFeeAmount.Value, out feeAmount ) )
            {
                feeResult = _feeCalculationService
                    .CalculateFees( SelectedAccounts.ToDictionary( account => account.Id, account => account.Amount ), feeAmount );
            }

            decimal totalFeeAmount = 0m;

            foreach ( var account in SelectedAccounts.Where( a => a.Amount > 0 ) )
            {
                var transactionDetail = new FinancialScheduledTransactionDetail();
                transactionDetail.Amount = account.Amount;

                if ( feeResult != null )
                {
                    Fee fee = feeResult.Fees.FirstOrDefault( f => f.Id == account.Id );
                    decimal offsetAmount = fee != null ? fee.Amount : 0m;
                    if ( feeAccount != null && feeAccount.Id != account.Id )
                    {
                        totalFeeAmount = totalFeeAmount + offsetAmount;
                    }
                    else
                    {
                        transactionDetail.Amount = transactionDetail.Amount + offsetAmount;
                    }
                }

                transactionDetail.AccountId = account.Id;
                if ( registration != null )
                {
                    transactionDetail.EntityTypeId = registration.TypeId;
                    transactionDetail.EntityId = registration.Id;
                }

                scheduledTransaction.ScheduledTransactionDetails.Add( transactionDetail );
                changeSummary.AppendFormat( "{0}: {1}", account.Name, account.Amount.FormatAsCurrency() );
                changeSummary.AppendLine();
            }

            if ( feeAccount != null && cbCoverFees.Checked && totalFeeAmount > 0 )
            {
                var feeTransactionDetail = new FinancialScheduledTransactionDetail();
                feeTransactionDetail.Amount = totalFeeAmount;
                feeTransactionDetail.AccountId = feeAccount.Id;

                scheduledTransaction.ScheduledTransactionDetails.Add( feeTransactionDetail );
            }

            if ( !string.IsNullOrWhiteSpace( paymentInfo.Comment1 ) )
            {
                changeSummary.Append( paymentInfo.Comment1 );
                changeSummary.AppendLine();
            }

            var transactionService = new FinancialScheduledTransactionService( rockContext );
            transactionService.Add( scheduledTransaction );
            rockContext.SaveChanges();

            // Add a note about the change
            var noteType = NoteTypeCache.Get( Rock.SystemGuid.NoteType.SCHEDULED_TRANSACTION_NOTE.AsGuid() );
            if ( noteType != null )
            {
                var noteService = new NoteService( rockContext );
                var note = new Note();
                note.NoteTypeId = noteType.Id;
                note.EntityId = scheduledTransaction.Id;
                note.Caption = "Created Transaction";
                note.Text = changeSummary.ToString();
                noteService.Add( note );
            }
            rockContext.SaveChanges();

            ScheduleId = scheduledTransaction.Id;
            TransactionCode = scheduledTransaction.TransactionCode;
        }
        private void SaveTransaction( FinancialGateway financialGateway, GatewayComponent gateway, Person person, PaymentInfo paymentInfo, FinancialTransaction transaction, FinancialAccount feeAccount, RockContext rockContext )
        {

            transaction.AuthorizedPersonAliasId = person.PrimaryAliasId;
            transaction.TransactionDateTime = RockDateTime.Now;
            transaction.FinancialGatewayId = financialGateway.Id;

            var txnType = DefinedValueCache.Get( this.GetAttributeValue( "TransactionType" ).AsGuidOrNull() ?? Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );
            transaction.TransactionTypeValueId = txnType.Id;

            var registration = GetRegistration();
            if ( registration != null )
            {
                var contributionTxnType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );
                var registrationTxnType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_EVENT_REGISTRATION.AsGuid() );

                transaction.TransactionTypeValueId = registration.RegistrationInstance.Account.IsTaxDeductible ? contributionTxnType.Id : registrationTxnType.Id;
            }

            transaction.Summary = paymentInfo.Comment1;

            if ( transaction.FinancialPaymentDetail == null )
            {
                transaction.FinancialPaymentDetail = new FinancialPaymentDetail();
            }
            transaction.FinancialPaymentDetail.SetFromPaymentInfo( paymentInfo, gateway, rockContext );

            Guid sourceGuid = Guid.Empty;
            if ( Guid.TryParse( GetAttributeValue( "Source" ), out sourceGuid ) )
            {
                var source = DefinedValueCache.Get( sourceGuid );
                if ( source != null )
                {
                    transaction.SourceTypeValueId = source.Id;
                }
            }

            decimal feeAmount;
            FeeCalculationResult feeResult = null;

            if ( cbCoverFees.Checked && decimal.TryParse( hfFeeAmount.Value, out feeAmount ) )
            {
                feeResult = _feeCalculationService
                    .CalculateFees( SelectedAccounts.ToDictionary( account => account.Id, account => account.Amount ), feeAmount );
            }

            decimal totalFeeAmount = 0m;

            foreach ( var account in SelectedAccounts.Where( a => a.Amount > 0 ) )
            {
                var transactionDetail = new FinancialTransactionDetail();
                transactionDetail.Amount = account.Amount;

                if ( feeResult != null )
                {
                    Fee fee = feeResult.Fees.FirstOrDefault( f => f.Id == account.Id );
                    decimal offsetAmount = fee != null ? fee.Amount : 0m;

                    if ( feeAccount != null && feeAccount.Id != account.Id )
                    {
                        totalFeeAmount = totalFeeAmount + offsetAmount;
                    }
                    else
                    {
                        transactionDetail.Amount = transactionDetail.Amount + offsetAmount;
                    }
                }

                transactionDetail.AccountId = account.Id;
                if ( registration != null )
                {
                    transactionDetail.EntityTypeId = registration.TypeId;
                    transactionDetail.EntityId = registration.Id;
                }

                transaction.TransactionDetails.Add( transactionDetail );
            }

            /* SimpleDonation.Start */
            if ( feeAccount != null && cbCoverFees.Checked && totalFeeAmount > 0 )
            {
                var feeTransactionDetail = new FinancialTransactionDetail();
                feeTransactionDetail.Amount = totalFeeAmount;
                feeTransactionDetail.AccountId = feeAccount.Id;

                transaction.TransactionDetails.Add( feeTransactionDetail );
            }
            /* SimpleDonation.End */

            var batchService = new FinancialBatchService( rockContext );

            // Get the batch
            var batch = batchService.Get(
                "Registration Payment",
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

            transaction.BatchId = batch.Id;
            transaction.LoadAttributes( rockContext );

            batch.Transactions.Add( transaction );

            rockContext.SaveChanges();
            transaction.SaveAttributeValues();

            HistoryService.SaveChanges(
                rockContext,
                typeof( FinancialBatch ),
                Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                batch.Id,
                batchChanges
            );

            SendReceipt( transaction.Id );

            TransactionCode = transaction.TransactionCode;
        }

        private void SendReceipt( int transactionId )
        {
            Guid? receiptEmail = GetAttributeValue( "ReceiptEmail" ).AsGuidOrNull();
            if ( receiptEmail.HasValue )
            {
                // Queue a transaction to send receipts
                var newTransactionIds = new List<int> { transactionId };
                var sendPaymentReceiptsTxn = new Rock.Transactions.SendPaymentReceipts( receiptEmail.Value, newTransactionIds );
                Rock.Transactions.RockQueue.TransactionQueue.Enqueue( sendPaymentReceiptsTxn );
            }
        }


        private void ShowSuccess( GatewayComponent gatewayComponent, Person person, PaymentInfo paymentInfo, PaymentSchedule schedule, FinancialPaymentDetail paymentDetail, RockContext rockContext )
        {
            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "CurrentPerson", CurrentPerson );

            var registration = GetRegistration();
            mergeFields.Add( "RegistrationInstance", registration.RegistrationInstance );
            mergeFields.Add( "Registration", registration );
            mergeFields.Add( "TransactionCode", TransactionCode );

            mergeFields.Add( "FullName", paymentInfo.FullName );
            mergeFields.Add( "Email", paymentInfo.Email );
            mergeFields.Add( "Accounts", SelectedAccounts.Where( a => a.Amount != 0 ) );
            mergeFields.Add( "TotalPaid", paymentInfo.Amount );
            mergeFields.Add( "PaymentMethod", paymentInfo.CurrencyTypeValue.Description );

            if ( ScheduleId.HasValue )
            {
                var scheduledTxn = new FinancialScheduledTransactionService( rockContext ).Get( ScheduleId.Value );
                if ( scheduledTxn != null && !string.IsNullOrWhiteSpace( scheduledTxn.GatewayScheduleId ) )
                {
                    mergeFields.Add( "GatewayScheduleId", scheduledTxn.GatewayScheduleId );
                }
            }

            decimal feeAmount;
            if ( cbCoverFees.Checked && decimal.TryParse( hfFeeAmount.Value, out feeAmount ) )
            {
                FeeCalculationResult feeResult = _feeCalculationService
                    .CalculateFees( SelectedAccounts.ToDictionary( account => account.Id, account => account.Amount ), feeAmount );
                mergeFields.Add( "FeeTotal", feeResult.Total );
            }

            string acctNumber = paymentInfo.MaskedNumber;
            if ( string.IsNullOrWhiteSpace( acctNumber ) && paymentDetail != null && !string.IsNullOrWhiteSpace( paymentDetail.AccountNumberMasked ) )
            {
                acctNumber = paymentDetail.AccountNumberMasked;
            }
            mergeFields.Add( "AccountNumberMasked", acctNumber );

            mergeFields.Add( "When", schedule != null ? schedule.ToString() : "Today" );

            var registrationInstance = registration.RegistrationInstance;
            registrationInstance.LoadAttributes();
            var successText = registrationInstance.GetAttributeValue( "PaymentEntrySuccessPage" );
            lSuccess.Text = successText.ResolveMergeFields( mergeFields );

            // If there was a transaction code returned and this was not already created from a previous saved account,
            // show the option to save the account.
            if ( !( paymentInfo is ReferencePaymentInfo ) && !string.IsNullOrWhiteSpace( TransactionCode ) && gatewayComponent.SupportsSavedAccount( paymentInfo.CurrencyTypeValue ) )
            {
                cbSaveAccount.Visible = true;
                pnlSaveAccount.Visible = true;
                txtSaveAccount.Visible = true;
            }
            else
            {
                pnlSaveAccount.Visible = false;
            }
        }

        private PaymentInfo GetPaymentInfo( RockContext rockContext, Person person, out string errorMessage )
        {
            PaymentInfo paymentInfo = null;

            if ( rblSavedAccount.Items.Count > 0 && ( rblSavedAccount.SelectedValueAsId() ?? 0 ) > 0 )
            {
                var savedAccount = new FinancialPersonSavedAccountService( rockContext ).Get( rblSavedAccount.SelectedValueAsId().Value );
                if ( savedAccount != null )
                {
                    var referencePaymentInfo = savedAccount.GetReferencePayment();

                    if ( referencePaymentInfo != null )
                    {
                        paymentInfo = new SimpleDonationReferencePaymentInfo(
                           cbCoverFees.Checked,
                           referencePaymentInfo );
                    }
                }
                else
                {
                    errorMessage = "There was a problem retrieving the saved account";
                }
            }
            else
            {
                bool isAch = hfPaymentTab.Value == "ACH";
                Guid currencyTypeGuid = isAch
                    ? Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH.AsGuid()
                    : Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid();

                string firstName = null;
                string lastName = null;

                if ( isAch )
                {
                    firstName = person.FirstName;
                    lastName = person.LastName;
                }
                else
                {
                    firstName = txtCardFirstName.Text;
                    lastName = txtCardLastName.Text;
                }

                paymentInfo = new SimpleDonationPaymentInfo(
                    hfStripeToken.Value,
                    firstName,
                    lastName,
                    cbCoverFees.Checked,
                    currencyTypeGuid );

            }

            paymentInfo.Amount = SelectedAccounts.Sum( a => a.Amount );
            decimal feeAmount;
            decimal? feeResultTotal = null;
            if ( cbCoverFees.Checked && decimal.TryParse( hfFeeAmount.Value, out feeAmount ) )
            {
                FeeCalculationResult feeResult = _feeCalculationService
                    .CalculateFees( SelectedAccounts.ToDictionary( account => account.Id, account => account.Amount ), feeAmount );
                paymentInfo.Amount = paymentInfo.Amount + feeResult.Total;
                feeResultTotal = feeResult.Total;
            }

            if ( paymentInfo == null )
            {
                errorMessage = "There was a problem creating the payment information";
                return null;
            }
            else
            {
                paymentInfo.FirstName = person.FirstName;
                paymentInfo.LastName = person.LastName;
            }

            // get the payment comment
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "TransactionDateTime", RockDateTime.Now );

            if ( paymentInfo != null )
            {
                mergeFields.Add( "CurrencyType", paymentInfo.CurrencyTypeValue );
            }

            if ( SelectedAccounts != null )
            {
                mergeFields.Add( "TransactionAccountDetails", SelectedAccounts.Where( a => a.Amount != 0 ).ToList() );
            }

            var registration = GetRegistration();
            var startDate = dtpStartDate.SelectedDate ?? RockDateTime.Today;
            if ( registration != null )
            {
                registration.RegistrationInstance.LoadAttributes();
                var paymentEndDate = registration.RegistrationInstance.GetAttributeValue( "ScheduledPaymentEndDate" ).AsDateTime();
                var enforceNoScheduledPaymentEndDate = registration.RegistrationInstance.GetAttributeValue( "EnforceNoScheduledPaymentEndDate" ).AsBoolean();
                var registrationAccount = registration.RegistrationInstance.Account;
                int? numberOfPayments = 0;
                decimal? paymentAmount = null;
                DateTime? endDate;
                List<DateTime> paymentDates = new List<DateTime>();
                if ( registrationAccount != null && ( paymentEndDate != null || enforceNoScheduledPaymentEndDate ) )
                {
                    var frequency = DefinedValueCache.Get( btnFrequency.SelectedValue.AsInteger() );


                    GetScheduleInfo( startDate, paymentEndDate, frequency.Value, out numberOfPayments, out endDate, out paymentDates );

                    if ( numberOfPayments != 0 )
                    {
                        if ( enforceNoScheduledPaymentEndDate )
                        {
                            paymentAmount = registration.DiscountedCost / numberOfPayments;
                        }
                        else
                        {
                            paymentAmount = registration.BalanceDue / numberOfPayments;
                        }

                        if ( feeResultTotal.HasValue && feeResultTotal.Value > 0 )
                        {
                            paymentAmount += feeResultTotal.Value;
                        }
                    }
                }

                var sb = new StringBuilder();
                sb.AppendLine( "</br></br><b>Payment Schedule</b>" );
                foreach ( var paymentDate in paymentDates )
                {
                    sb.AppendLine( String.Format( "{0} on {1}", paymentAmount.FormatAsCurrency(), paymentDate.ToShortDateString() ) );
                }

                paymentInfo.Comment1 = String.Format( "{0}[{1}]: {2}{3}{4}"
                    , registration.RegistrationInstance.Name
                    , registration.RegistrationInstance.Id
                    , registration.Registrants.Where( r => r.OnWaitList != true ).Select( r => r.Person.FullName ).JoinStringsWithCommaAnd()
                    , paymentDates.Any() ? sb.ToString() : ""
                    , !string.IsNullOrWhiteSpace( paymentInfo.Comment1 ) ? string.Format( "</br>{0}", paymentInfo.Comment1 ) : "" );
            }

            errorMessage = string.Empty;
            return paymentInfo;
        }

        private PaymentSchedule GetSchedule()
        {
            var registrationPaymentEndDate = GetFrequencyEndDate();
            var numberOfPayments = GetFrequencyNumberOfPayments();

            // If a one-time gift was selected for today's date, then treat as a onetime immediate transaction (not scheduled)
            int oneTimeFrequencyId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME ).Id;
            if ( btnFrequency.SelectedValue == oneTimeFrequencyId.ToString() && dtpStartDate.SelectedDate <= RockDateTime.Today )
            {
                // one-time immediate payment
                return null;
            }

            var schedule = new PaymentSchedule();
            schedule.TransactionFrequencyValue = DefinedValueCache.Get( btnFrequency.SelectedValueAsId().Value );
            DateTime today = RockDateTime.Today;

            if ( dtpStartDate.SelectedDate.HasValue && dtpStartDate.SelectedDate > today )
            {
                schedule.StartDate = dtpStartDate.SelectedDate.Value;
            }
            else
            {
                schedule.StartDate = today;
            }

            if ( schedule.TransactionFrequencyValue.Guid == _firstFifteenthGuid )
            {
                schedule.StartDate = GetNextFirstFifteenthDate( schedule.StartDate ) ?? schedule.StartDate;
            }

            if ( schedule.TransactionFrequencyValue.Guid == _firstOfTheMonthGuid )
            {
                schedule.StartDate = GetNext1stOfTheMonthDate( schedule.StartDate ) ?? schedule.StartDate;
            }

            if ( registrationPaymentEndDate.HasValue )
            {
                schedule.EndDate = registrationPaymentEndDate;
            }

            schedule.NumberOfPayments = numberOfPayments;

            return schedule;
        }

        private void BindSavedAccounts()
        {
            var rockContext = new RockContext();
            var registration = GetRegistration();
            if ( registration != null )
            {
                var financialGateway = registration.RegistrationInstance.RegistrationTemplate.FinancialGateway;
                if ( financialGateway != null )
                {
                    var gatewayComponent = financialGateway.GetGatewayComponent();
                    if ( gatewayComponent != null )
                    {
                        rblSavedAccount.Items.Clear();

                        if ( _targetPerson != null )
                        {
                            // Get the saved accounts for the currently logged in user
                            var savedAccounts = new FinancialPersonSavedAccountService( rockContext )
                                .GetByPersonId( _targetPerson.Id )
                                .ToList();

                            // Find the saved accounts that are valid for the selected CC gateway
                            var ccSavedAccountIds = new List<int>();
                            var ccCurrencyType = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD ) );
                            if ( financialGateway != null &&
                                gatewayComponent != null &&
                                gatewayComponent.SupportsSavedAccount( ccCurrencyType ) )
                            {
                                ccSavedAccountIds = savedAccounts
                                    .Where( a =>
                                        a.FinancialGatewayId == financialGateway.Id &&
                                        a.FinancialPaymentDetail != null &&
                                        a.FinancialPaymentDetail.CurrencyTypeValueId == ccCurrencyType.Id )
                                    .Select( a => a.Id )
                                    .ToList();
                            }

                            // Find the saved accounts that are valid for the selected ACH gateway
                            var achSavedAccountIds = new List<int>();
                            var achCurrencyType = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH ) );
                            if ( financialGateway != null &&
                                gatewayComponent != null &&
                                gatewayComponent.SupportsSavedAccount( achCurrencyType ) &&
                                GetAttributeValue( AttributeKey.EnableACH ).AsBoolean() )
                            {
                                achSavedAccountIds = savedAccounts
                                    .Where( a =>
                                        a.FinancialGatewayId == financialGateway.Id &&
                                        a.FinancialPaymentDetail != null &&
                                        a.FinancialPaymentDetail.CurrencyTypeValueId == achCurrencyType.Id )
                                    .Select( a => a.Id )
                                    .ToList();
                            }

                            var savedAccountsById = savedAccounts
                                .Where( a =>
                                    ccSavedAccountIds.Contains( a.Id ) ||
                                    achSavedAccountIds.Contains( a.Id ) )
                                .OrderBy( a => a.Name )
                                .Select( a => new
                                {
                                    Id = a.Id,
                                    Name = "Use " + a.Name + " (" + a.FinancialPaymentDetail.AccountNumberMasked + ")",
                                    DataCurrency = a.FinancialPaymentDetail != null && a.FinancialPaymentDetail.CurrencyTypeValueId == ccCurrencyType.Id ? "CC" : "ACH",
                                } ).ToList();

                            // Bind the accounts
                            rblSavedAccount.DataSource = savedAccountsById;
                            rblSavedAccount.DataBind();
                            if ( rblSavedAccount.Items.Count > 0 )
                            {
                                hfSavedAccounts.Value = savedAccountsById.ToJson();
                                rblSavedAccount.Items.Add( new ListItem( "Use a different payment method", "0" ) );
                                if ( rblSavedAccount.SelectedValue == "" )
                                {
                                    rblSavedAccount.Items[0].Selected = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ShowMessage( NotificationBoxType type, string title, string text )
        {
            if ( !string.IsNullOrWhiteSpace( text ) )
            {
                NotificationBox nb = nbMessage;
                switch ( hfCurrentPage.Value.AsInteger() )
                {
                    case 1:
                        nb = nbSelectionMessage;
                        break;
                    case 2:
                        nb = nbSelectionMessage;
                        break;
                    case 4:
                        nb = nbSuccessMessage;
                        break;
                }

                nb.Text = text;
                nb.Title = string.IsNullOrWhiteSpace( title ) ? "" : string.Format( "<p>{0}</p>", title );
                nb.NotificationBoxType = type;
                nb.Visible = true;
            }
        }

        private void RegisterFeeRates( int? financialGatewayId )
        {
            const string accountInfoKey = "com.SimpleDonation.AccountInfo";
            AccountInfo accountInfo = ( AccountInfo ) Cache.Get( accountInfoKey );

            if ( accountInfo == null )
            {
                accountInfo = _accountService.GetAccountInfo( financialGatewayId );
                Cache.Insert( accountInfoKey, accountInfo, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes( 10 ) );
            }

            hfPublicKey.Value = accountInfo.PublicKey;
            hfAchRate.Value = accountInfo.AchRate.ToString();
            hfCardRate.Value = accountInfo.CardRate.ToString();
            hfCapAch.Value = accountInfo.CapAch.ToString();
        }

        private void SetScheduleOptions()
        {
            cbCoverFees.Checked = GetAttributeValue( AttributeKey.AreCoverFeesChecked ).AsBoolean();

            var registrationId = PageParameter( GetAttributeValue( AttributeKey.RegistrationParameter ) ).AsInteger();
            if ( registrationId > 0 )
            {
                var registrationIdString = registrationId.ToString();
                if ( ddlRegistrations.Items.FindByValue( registrationIdString ) != null )
                {
                    ddlRegistrations.SelectedValue = registrationIdString;
                }
            }
            GetRegistration( true );
            BindSavedAccounts();

            var paymentDate = PageParameter( GetAttributeValue( AttributeKey.PaymentDateParameter ) ).AsDateTime();
            dtpStartDate.SelectedDate = paymentDate ?? RockDateTime.Today;
            HideStartDatePicker();

            LoadFrequencyDropdown();
            DisplayExistingPayments();

            var frequencyParameter = PageParameter( GetAttributeValue( AttributeKey.FrequencyParameter ) ).ToStringSafe();
            if ( !string.IsNullOrWhiteSpace( frequencyParameter ) )
            {
                var frequencyValues = frequencyParameter.Split( new char[] { '^' } );

                var frequencyId = frequencyValues[0].AsIntegerOrNull();
                if ( !frequencyId.HasValue )
                {
                    var frequencyGuid = frequencyValues[0].AsGuid();
                    var frequency = DefinedValueCache.Get( frequencyGuid );
                    if ( frequency != null )
                    {
                        frequencyId = frequency.Id;
                    }
                }

                var frequencyValue = frequencyId.ToStringSafe();
                if ( !string.IsNullOrWhiteSpace( frequencyValue ) )
                {
                    if ( btnFrequency.Items.FindByValue( frequencyValue ) != null )
                    {
                        btnFrequency.SelectedValue = frequencyValue;
                        if ( frequencyValues.Length >= 2 && frequencyValues[1].AsBoolean( true ) == false )
                        {
                            btnFrequency.Visible = false;
                            txtFrequency.Visible = true;
                            txtFrequency.Text = btnFrequency.SelectedItem.Text;
                        }
                    }
                }
            }

            SetAmountFromFrequencyDropdown();
        }

        private void SetPaymentOptions()
        {
            // Set the payment method tabs
            var registration = GetRegistration();
            if ( registration != null )
            {
                var financialGateway = registration.RegistrationInstance.RegistrationTemplate.FinancialGateway;
                if ( financialGateway != null )
                {
                    var gatewayComponent = financialGateway.GetGatewayComponent();
                    if ( gatewayComponent != null )
                    {
                        bool achEnabled = GetAttributeValue( AttributeKey.EnableACH ).AsBoolean();
                        bool showCCdefault = this.GetAttributeValue( AttributeKey.ShowCCDefault ).AsBoolean( true );
                        divCCPaymentInfo.Visible = true;
                        divACHPaymentInfo.Visible = achEnabled;
                        if ( showCCdefault == true || !achEnabled )
                        {
                            hfPaymentTab.Value = "CreditCard";
                        }
                        else
                        {
                            hfPaymentTab.Value = "ACH";
                        }

                        if ( achEnabled )
                        {
                            phPills.Visible = true;
                        }


                        // Determine if and how Name on Card should be displayed
                        txtCardFirstName.Visible = gatewayComponent != null && gatewayComponent.PromptForNameOnCard( financialGateway ) && gatewayComponent.SplitNameOnCard;
                        txtCardLastName.Visible = gatewayComponent != null && gatewayComponent.PromptForNameOnCard( financialGateway ) && gatewayComponent.SplitNameOnCard;
                        txtCardName.Visible = gatewayComponent != null && gatewayComponent.PromptForNameOnCard( financialGateway ) && !gatewayComponent.SplitNameOnCard;

                        // Set cc expiration min/max
                        mypExpiration.MinimumYear = RockDateTime.Now.Year;
                        mypExpiration.MaximumYear = mypExpiration.MinimumYear + 15;

                        // Determine if account name should be displayed for bank account
                        txtAccountName.Visible = gatewayComponent != null && gatewayComponent.PromptForBankAccountName( financialGateway );

                        // Determine if billing address should be displayed
                        cbBillingAddress.Visible = gatewayComponent != null && gatewayComponent.PromptForBillingAddress( financialGateway );
                        divBillingAddress.Visible = gatewayComponent != null && gatewayComponent.PromptForBillingAddress( financialGateway );
                    }
                }
            }
        }

        private void SetTargetPerson( RockContext rockContext )
        {
            // If impersonation is allowed, and a valid person key was used, set the target to that person
            if ( GetAttributeValue( "Impersonation" ).AsBooleanOrNull() ?? false )
            {
                string personKey = PageParameter( "Person" );
                if ( !string.IsNullOrWhiteSpace( personKey ) )
                {
                    var incrementKeyUsage = !this.IsPostBack;
                    _targetPerson = new PersonService( rockContext ).GetByImpersonationToken( personKey, incrementKeyUsage, this.PageCache.Id );

                    if ( _targetPerson == null )
                    {
                        nbInvalidPersonWarning.Text = "Invalid or Expired Person Token specified";
                        nbInvalidPersonWarning.NotificationBoxType = NotificationBoxType.Danger;
                        nbInvalidPersonWarning.Visible = true;
                        return;
                    }
                }
            }

            if ( _targetPerson == null )
            {
                _targetPerson = CurrentPerson;
            }
        }

        private void RegisterScript()
        {
            RockPage.AddScriptLink( ResolveUrl( "~/Scripts/jquery.creditCardTypeDetector.js" ) );

            if ( _coverFeesVisible )
            {
                RockPage.AddScriptLink( ResolveUrl( "~/Plugins/com_simpledonation/js/cover-fees-reg-payment.js" ) );
            }

            RockPage.AddScriptLink( "https://js.stripe.com/v2/", false );
            RockPage.AddScriptLink( ResolveUrl( "~/Plugins/com_simpledonation/js/jquery.payment.min.js" ) );
            RockPage.AddScriptLink( ResolveUrl( "~/Plugins/com_simpledonation/js/stripe-token-reg-payment.js" ) );

            int oneTimeFrequencyId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME ).Id;

            string scriptFormat = @"
                Sys.Application.add_load(function () {{
                    // As amounts are entered, validate that they are numeric and recalc total
                    $('.account-amount').on('change', function() {{
                        var totalAmt = Number(0);

                        $('.account-amount .form-control').each(function (index) {{
                            var itemValue = $(this).val();
                            if (itemValue != null && itemValue != '') {{
                                if (isNaN(itemValue)) {{
                                    $(this).parents('div.input-group').addClass('has-error');
                                }}
                                else {{
                                    $(this).parents('div.input-group').removeClass('has-error');
                                    var num = Number(itemValue);
                                    $(this).val(num.toFixed(2));
                                    totalAmt = totalAmt + num;
                                }}
                            }}
                            else {{
                                $(this).parents('div.input-group').removeClass('has-error');
                            }}
                        }});
                        $('.total-amount').html('{3}' + totalAmt.toFixed(2));
                        return false;
                    }});

                    // Save the state of the selected payment type pill to a hidden field so that state can
                    // be preserved through postback
                    $('a[data-toggle=""pill""]').on('shown.bs.tab', function (e) {{
                        var tabHref = $(e.target).attr(""href"");
                        if (tabHref == '#{0}') {{
                            $('#{1}').val('CreditCard');
                        }} else {{
                            $('#{1}').val('ACH');
                        }}
                    }});

                    // Detect credit card type
                    $('.credit-card').creditCardTypeDetector({{ 'credit_card_logos': '.card-logos' }});

                    if ( typeof {17} != 'undefined' ) {{
                        //// Toggle credit card display if saved card option is available
                        $('input[name=""{18}""]').change(function () {{
                            var radioDisplay = $('#{19}').css('display');
                            var selectedVal = $('input[name=""{18}""]:checked').val();

                            if ( selectedVal == 0 && radioDisplay == 'none') {{
                                $('#{19}').slideDown();
                            }}
                            else if (selectedVal != 0 && radioDisplay != 'none') {{
                                $('#{19}').slideUp();
                            }}
                        }});
                    }}

                    // Hide or show a div based on selection of checkbox
                    $('input:checkbox.toggle-input').unbind('click').on('click', function () {{
                        $(this).parents('.checkbox').next('.toggle-content').slideToggle();
                    }});

                    // Disable the submit button as soon as it's clicked to prevent double-clicking
                    $('a[id$=""btnNext""]').click(function() {{
                        $(this).unbind('click');
                        if (typeof (Page_ClientValidate) == 'function') {{
                            if (Page_IsValid) {{
                                Page_ClientValidate();
                            }}
                        }}

                        if (Page_IsValid) {{
            			    $(this).addClass('disabled');
            			    $(this).click(function () {{
            				    return false;
            			    }});
                        }}
                    }});
                }});
            ";
            string script = string.Format(
                scriptFormat,
                divCCPaymentInfo.ClientID,      // {0}
                hfPaymentTab.ClientID,          // {1}
                oneTimeFrequencyId,             // {2}
                GlobalAttributesCache.Value( "CurrencySymbol" ), // {3)
                this.BlockValidationGroup,      // {4}
                txtCreditCard.ClientID,         // {5}
                mypExpiration.ClientID,         // {6}
                txtCVV.ClientID,                // {7}
                txtAccountName.ClientID,        // {8}
                txtAccountNumber.ClientID,      // {9}
                txtRoutingNumber.ClientID,      // {10}
                rblAccountType.ClientID,        // {11}
                cbBillingAddress.ClientID,      // {12}
                acBillingAddress.ClientID,      // {13}
                txtCardFirstName.ClientID,      // {14}
                txtCardLastName.ClientID,       // {15}
                txtCardName.ClientID,           // {16}
                rblSavedAccount.ClientID,       // {17}
                rblSavedAccount.UniqueID,       // {18}
                divNewPayment.ClientID          // {19}
            );

            ScriptManager.RegisterStartupScript( upPayment, this.GetType(), "giving-profile", script, true );
        }

        protected void LoadRegistrationDropdown( RockContext rockContext = null )
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }
            var registrationService = new RegistrationService( rockContext );

            var registrationQry = registrationService.Queryable().Where( r => r.PersonAlias.Person.PrimaryFamilyId == _targetPerson.PrimaryFamilyId );

            bool showInactiveRegistrations = GetAttributeValue( AttributeKey.ShowInactiveRegistrations ).AsBoolean();
            if ( !showInactiveRegistrations )
            {
                registrationQry = registrationQry.Where( r => r.RegistrationInstance.IsActive );
            }

            bool showPaidRegistrations = GetAttributeValue( AttributeKey.ShowPaidRegistrations ).AsBoolean();
            if ( !showPaidRegistrations )
            {
                registrationQry = registrationQry.ToList().Where( r => r.BalanceDue > 0 ).AsQueryable();
            }

            var registrationList = registrationQry
                .ToList()
                .Select( r => new
                {
                    Text = String.Format( "{0} ({1}) - {2}", r.RegistrationInstance.Name, r.Registrants.Select( rr => rr.PersonAlias.Person.NickName ).JoinStringsWithCommaAnd(), r.BalanceDue.FormatAsCurrency() ),
                    Value = r.Id
                } )
                .ToList();

            ddlRegistrations.DataSource = registrationList;
            ddlRegistrations.DataBind();
        }

        private void HideStartDatePicker()
        {
            var registration = GetRegistration();
            bool disableDatePicker = false;
            if ( registration != null )
            {
                registration.RegistrationInstance.LoadAttributes();
                disableDatePicker = registration.RegistrationInstance.GetAttributeValue( "DisablePaymentDate" ).AsBoolean();
            }
            if ( disableDatePicker )
            {
                dtpStartDate.Enabled = false;
                dtpStartDate.Visible = false;
                lStartDate.Text = String.Format( "<b>{0}</b><br/>{1}</br><br/>", dtpStartDate.Label, dtpStartDate.SelectedDate.ToShortDateString() );
            }
        }

        private List<DefinedValueCache> LoadFrequencies( RockContext rockContext = null )
        {
            var supportedFrequencies = new List<DefinedValueCache>();
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }

            var registration = GetRegistration( false, rockContext );
            if ( registration != null )
            {
                var financialGateway = registration.RegistrationInstance.RegistrationTemplate.FinancialGateway;
                financialGateway.LoadAttributes( rockContext );
                var gatewayComponent = financialGateway.GetGatewayComponent();
                if ( gatewayComponent != null )
                {
                    supportedFrequencies = gatewayComponent.SupportedPaymentSchedules;

                    // If gateway didn't specifically support one-time, add it anyway for immediate gifts
                    var oneTimeFrequency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME );
                    if ( !supportedFrequencies.Where( f => f.Id == oneTimeFrequency.Id ).Any() )
                    {
                        supportedFrequencies.Add( oneTimeFrequency );
                    }

                    registration.RegistrationInstance.LoadAttributes();
                    var availableFrequencyGuids = registration.RegistrationInstance.GetAttributeValue( "AvailableTransactionFrequencies" ).SplitDelimitedValues().AsGuidList();
                    if ( availableFrequencyGuids.Any() )
                    {
                        supportedFrequencies = supportedFrequencies.Where( f => availableFrequencyGuids.Contains( f.Guid ) ).ToList();
                    }

                }
            }
            return supportedFrequencies;
        }

        public void LoadFrequencyDropdown()
        {
            var registration = GetRegistration();
            var oneTimeFrequency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME );
            var yearlyFrequency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_YEARLY );

            var selectedValue = btnFrequency.SelectedValue;
            btnFrequency.Items.Clear();

            // Update the frequency options
            if ( registration != null )
            {
                var startDate = dtpStartDate.SelectedDate ?? RockDateTime.Today;
                registration.RegistrationInstance.LoadAttributes();
                var paymentEndDate = registration.RegistrationInstance.GetAttributeValue( "ScheduledPaymentEndDate" ).AsDateTime();
                var enforceNoScheduledPaymentEndDate = registration.RegistrationInstance.GetAttributeValue( "EnforceNoScheduledPaymentEndDate" ).AsBoolean();

                if ( paymentEndDate != null || enforceNoScheduledPaymentEndDate )
                {
                    if ( enforceNoScheduledPaymentEndDate )
                    {
                        paymentEndDate = startDate.AddYears( 1 ).AddDays( -1 ).AddSeconds( -1 );
                    }

                    foreach ( var frequency in LoadFrequencies() )
                    {
                        decimal? paymentAmount = null;
                        int? numberOfPayments;
                        DateTime? endDate;
                        List<DateTime> paymentDates = new List<DateTime>();
                        GetScheduleInfo( startDate, paymentEndDate, frequency.Value, out numberOfPayments, out endDate, out paymentDates );

                        if ( numberOfPayments > 1 || ( !enforceNoScheduledPaymentEndDate && frequency.Value == "One-Time" ) || ( enforceNoScheduledPaymentEndDate && frequency.Value == "Yearly" ) )
                        {
                            if ( enforceNoScheduledPaymentEndDate )
                            {
                                paymentAmount = registration.DiscountedCost / numberOfPayments;
                            }
                            else
                            {
                                paymentAmount = registration.BalanceDue / numberOfPayments;
                            }

                            var itemText = String.Format( "{0} {1} Payment{2} of {3} {4}"
                                , enforceNoScheduledPaymentEndDate ? "" : numberOfPayments.ToString()
                                , frequency.Value
                                , ( enforceNoScheduledPaymentEndDate || numberOfPayments > 1 ) ? "s" : ""
                                , paymentAmount.FormatAsCurrency()
                                , enforceNoScheduledPaymentEndDate ? "" : ( numberOfPayments > 1 ? string.Format( "from {0} to {1}", startDate.ToShortDateString(), endDate.ToShortDateString() ) : string.Format( "on {0}", endDate.ToShortDateString() ) ) );

                            var listItem = new ListItem( itemText, frequency.Id.ToString() );
                            if ( selectedValue == listItem.Value )
                            {
                                listItem.Selected = true;
                            }
                            btnFrequency.Items.Add( listItem );
                        }
                    }

                    if ( btnFrequency.SelectedItem == null )
                    {
                        if ( enforceNoScheduledPaymentEndDate )
                        {
                            btnFrequency.SetValue( yearlyFrequency.Id.ToString() );
                        }
                        else
                        {
                            btnFrequency.SetValue( oneTimeFrequency.Id.ToString() );
                        }
                    }
                }
            }
        }

        public void DisplayExistingPayments()
        {
            var rockContext = new RockContext();
            var registration = GetRegistration( false, rockContext );
            var scheduledPaymentDetails = new List<FinancialScheduledTransactionDetail>();
            if ( registration != null )
            {
                scheduledPaymentDetails = new FinancialScheduledTransactionDetailService( rockContext )
               .Queryable()
               .AsNoTracking()
               .Where( std =>
                         std.EntityId == registration.Id &&
                         std.EntityTypeId == registration.TypeId &&
                         std.ScheduledTransaction.IsActive == true
                   )
               .ToList();
            }

            if ( scheduledPaymentDetails.Any() )
            {
                nbExistingScheduledPayments.Visible = true;
                nbExistingScheduledPayments.Title = "Warning";
                nbExistingScheduledPayments.NotificationBoxType = NotificationBoxType.Warning;
                var sb = new StringBuilder();
                sb.Append( "</br>The following payments are already scheduled for this registration:</br><ul>" );
                foreach ( var scheduledPaymentDetail in scheduledPaymentDetails )
                {
                    var scheduledTransaction = scheduledPaymentDetail.ScheduledTransaction;
                    var startDate = scheduledTransaction.StartDate.ToShortDateString();
                    var endDate = scheduledTransaction.EndDate.ToShortDateString();

                    var dateRangeString = string.Format( "from {0} to {1}", startDate, endDate );
                    if ( startDate == endDate )
                    {
                        dateRangeString = string.Format( "on {0}", startDate );
                    }

                    sb.AppendFormat( "<li>{0} {1} {2}</li>"
                            , scheduledPaymentDetail.Amount.FormatAsCurrency()
                            , scheduledTransaction.TransactionFrequencyValue.Value
                            , dateRangeString
                        );
                }
                sb.Append( "</ul>" );

                nbExistingScheduledPayments.Text = sb.ToString();
            }
            else
            {
                nbExistingScheduledPayments.Visible = false;
            }
        }

        private void SetAmountFromFrequencyDropdown()
        {
            // Update the frequency options
            var registration = GetRegistration();
            var startDate = dtpStartDate.SelectedDate ?? RockDateTime.Today;
            if ( registration != null )
            {
                registration.RegistrationInstance.LoadAttributes();
                var paymentEndDate = registration.RegistrationInstance.GetAttributeValue( "ScheduledPaymentEndDate" ).AsDateTime();
                var enforceNoScheduledPaymentEndDate = registration.RegistrationInstance.GetAttributeValue( "EnforceNoScheduledPaymentEndDate" ).AsBoolean();

                var registrationAccount = registration.RegistrationInstance.Account;
                if ( registrationAccount != null && ( paymentEndDate != null || enforceNoScheduledPaymentEndDate ) )
                {
                    var frequency = DefinedValueCache.Get( btnFrequency.SelectedValue.AsInteger() );

                    int? numberOfPayments = 0;
                    decimal? paymentAmount = null;
                    DateTime? endDate;
                    List<DateTime> paymentDates = new List<DateTime>();

                    if ( enforceNoScheduledPaymentEndDate )
                    {
                        paymentEndDate = startDate.AddYears( 1 ).AddDays( -1 ).AddSeconds( -1 );
                    }

                    GetScheduleInfo( startDate, paymentEndDate, frequency.Value, out numberOfPayments, out endDate, out paymentDates );

                    if ( numberOfPayments != 0 )
                    {
                        if ( enforceNoScheduledPaymentEndDate )
                        {
                            paymentAmount = registration.DiscountedCost / numberOfPayments;
                        }
                        else
                        {
                            paymentAmount = registration.BalanceDue / numberOfPayments;
                        }

                        if ( paymentAmount.HasValue )
                        {
                            SelectedAccounts = new List<AccountItem>();
                            var accountItem = new AccountItem( registrationAccount.Id, registrationAccount.Order, registrationAccount.Name, registrationAccount.CampusId, registrationAccount.PublicName );
                            accountItem.Amount = paymentAmount.Value;
                            hfPaymentAmount.Value = paymentAmount.Value.ToString();
                            accountItem.Enabled = false;
                            SelectedAccounts.Add( accountItem );
                        }
                    }

                    var sb = new StringBuilder();
                    sb.AppendLine( "<b>Payment Schedule</b></br>" );
                    foreach ( var paymentDate in paymentDates )
                    {
                        sb.AppendLine( String.Format( "{0} on {1}</br>", paymentAmount.FormatAsCurrency(), paymentDate.ToShortDateString() ) );
                    }

                    if ( enforceNoScheduledPaymentEndDate )
                    {
                        if ( frequency.Value == "Yearly" )
                        {
                            for ( var i = 1; i <= 5; i++ )
                            {
                                sb.AppendLine( String.Format( "{0} on {1}</br>", paymentAmount.FormatAsCurrency(), paymentDates.First().AddYears( i ).ToShortDateString() ) );
                            }
                        }
                        sb.AppendLine( "..." );
                    }

                    lPaymentDates.Text = sb.ToString();
                }
            }
        }

        /// <summary>
        /// Gets the frequency end date.
        /// </summary>
        /// <returns>System.Nullable&lt;DateTime&gt;.</returns>
        private DateTime? GetFrequencyEndDate()
        {
            // Update the frequency options
            var registration = GetRegistration();
            var startDate = dtpStartDate.SelectedDate ?? RockDateTime.Today;
            var frequency = DefinedValueCache.Get( btnFrequency.SelectedItem.Value.AsInteger() );

            int? numberOfPayments = 0;
            DateTime? paymentEndDate = null;
            DateTime? endDate = null;
            List<DateTime> paymentDates = new List<DateTime>();
            if ( registration != null )
            {
                registration.RegistrationInstance.LoadAttributes();
                paymentEndDate = registration.RegistrationInstance.GetAttributeValue( "ScheduledPaymentEndDate" ).AsDateTime();
                var enforceNoScheduledPaymentEndDate = registration.RegistrationInstance.GetAttributeValue( "EnforceNoScheduledPaymentEndDate" ).AsBoolean();

                var registrationAccount = registration.RegistrationInstance.Account;
                if ( registrationAccount != null && ( paymentEndDate != null || enforceNoScheduledPaymentEndDate ) )
                {
                    GetScheduleInfo( startDate, paymentEndDate, frequency.Value, out numberOfPayments, out endDate, out paymentDates );
                }

                if ( enforceNoScheduledPaymentEndDate )
                {
                    endDate = null;
                }
            }

            return endDate;
        }

        /// <summary>
        /// Gets the frequency start date.
        /// </summary>
        /// <returns>System.Nullable&lt;DateTime&gt;.</returns>
        private DateTime? GetFrequencyStartDate()
        {
            // Update the frequency options
            var registration = GetRegistration();
            var startDate = dtpStartDate.SelectedDate ?? RockDateTime.Today;
            var frequency = DefinedValueCache.Get( btnFrequency.SelectedItem.Value.AsInteger() );

            int? numberOfPayments = 0;
            DateTime? paymentEndDate = null;
            DateTime? endDate = null;
            List<DateTime> paymentDates = new List<DateTime>();
            if ( registration != null )
            {
                registration.RegistrationInstance.LoadAttributes();
                paymentEndDate = registration.RegistrationInstance.GetAttributeValue( "ScheduledPaymentEndDate" ).AsDateTime();
                var enforceNoScheduledPaymentEndDate = registration.RegistrationInstance.GetAttributeValue( "EnforceNoScheduledPaymentEndDate" ).AsBoolean();
                var registrationAccount = registration.RegistrationInstance.Account;
                if ( registrationAccount != null && ( paymentEndDate != null || enforceNoScheduledPaymentEndDate ) )
                {
                    GetScheduleInfo( startDate, paymentEndDate, frequency.Value, out numberOfPayments, out endDate, out paymentDates );
                }
            }

            return startDate;
        }

        /// <summary>
        /// Gets the frequency start date.
        /// </summary>
        /// <returns>System.Nullable&lt;DateTime&gt;.</returns>
        private List<DateTime> GetFrequencyPaymentDates()
        {
            // Update the frequency options
            var registration = GetRegistration();
            var startDate = dtpStartDate.SelectedDate ?? RockDateTime.Today;
            var frequency = DefinedValueCache.Get( btnFrequency.SelectedItem.Value.AsInteger() );

            int? numberOfPayments = 0;
            DateTime? paymentEndDate = null;
            DateTime? endDate = null;
            List<DateTime> paymentDates = new List<DateTime>();
            if ( registration != null )
            {
                registration.RegistrationInstance.LoadAttributes();
                paymentEndDate = registration.RegistrationInstance.GetAttributeValue( "ScheduledPaymentEndDate" ).AsDateTime();
                var enforceNoScheduledPaymentEndDate = registration.RegistrationInstance.GetAttributeValue( "EnforceNoScheduledPaymentEndDate" ).AsBoolean();
                var registrationAccount = registration.RegistrationInstance.Account;
                if ( registrationAccount != null && ( paymentEndDate != null || enforceNoScheduledPaymentEndDate ) )
                {
                    GetScheduleInfo( startDate, paymentEndDate, frequency.Value, out numberOfPayments, out endDate, out paymentDates );
                }
            }

            return paymentDates;
        }

        /// <summary>
        /// Gets the frequency start date.
        /// </summary>
        /// <returns>System.Nullable&lt;DateTime&gt;.</returns>
        private int? GetFrequencyNumberOfPayments()
        {
            // Update the frequency options
            var registration = GetRegistration();
            var startDate = dtpStartDate.SelectedDate ?? RockDateTime.Today;
            var frequency = DefinedValueCache.Get( btnFrequency.SelectedItem.Value.AsInteger() );

            int? numberOfPayments = 0;
            DateTime? paymentEndDate = null;
            DateTime? endDate = null;
            List<DateTime> paymentDates = new List<DateTime>();
            if ( registration != null )
            {
                registration.RegistrationInstance.LoadAttributes();
                paymentEndDate = registration.RegistrationInstance.GetAttributeValue( "ScheduledPaymentEndDate" ).AsDateTime();
                var enforceNoScheduledPaymentEndDate = registration.RegistrationInstance.GetAttributeValue( "EnforceNoScheduledPaymentEndDate" ).AsBoolean();
                var registrationAccount = registration.RegistrationInstance.Account;
                if ( registrationAccount != null && ( paymentEndDate != null || enforceNoScheduledPaymentEndDate ) )
                {
                    GetScheduleInfo( startDate, paymentEndDate, frequency.Value, out numberOfPayments, out endDate, out paymentDates );
                }

                if ( enforceNoScheduledPaymentEndDate )
                {
                    numberOfPayments = null;
                }
            }

            return numberOfPayments;
        }

        /// <summary>
        /// Gets the schedule information.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="paymentEndDate">The payment end date.</param>
        /// <param name="listItemText">The list item text.</param>
        /// <param name="numberOfPayments">The number of payments.</param>
        /// <param name="endDate">The end date.</param>
        private static void GetScheduleInfo( DateTime? startDate, DateTime? paymentEndDate, string listItemText, out int? numberOfPayments, out DateTime? endDate, out List<DateTime> paymentDates )
        {
            numberOfPayments = 0;
            endDate = startDate;
            var finalEndDateValue = endDate;
            paymentDates = new List<DateTime>();
            var daysToJump = 0;
            switch ( listItemText )
            {
                case "One-Time":
                    if ( startDate >= paymentEndDate )
                    {
                        endDate = paymentEndDate;
                    }
                    numberOfPayments = 1;
                    paymentDates.Add( endDate.Value );
                    break;
                case "Weekly":
                    while ( endDate <= paymentEndDate )
                    {
                        numberOfPayments++;
                        paymentDates.Add( endDate.Value );
                        endDate = endDate.Value.AddDays( 7 );
                    }
                    endDate = endDate.Value.AddDays( -7 );
                    break;
                case "Bi-Weekly":
                    while ( endDate <= paymentEndDate )
                    {
                        numberOfPayments++;
                        paymentDates.Add( endDate.Value );
                        endDate = endDate.Value.AddDays( 14 );
                    }
                    endDate = endDate.Value.AddDays( -14 );
                    break;
                case "Monthly":
                    while ( endDate <= paymentEndDate )
                    {
                        numberOfPayments++;
                        paymentDates.Add( endDate.Value );
                        endDate = endDate.Value.AddMonths( 1 );
                    }
                    endDate = endDate.Value.AddMonths( -1 );
                    break;
                case "1st of the Month":
                    if ( endDate.HasValue )
                    {
                        endDate = GetNext1stOfTheMonthDate( endDate );

                        while ( endDate <= paymentEndDate )
                        {
                            numberOfPayments++;
                            paymentDates.Add( endDate.Value );
                            finalEndDateValue = endDate.Value;
                            endDate = endDate.Value.AddMonths( 1 );
                        }
                    }
                    endDate = finalEndDateValue;
                    break;
                case "1st/15th":
                    if ( endDate.HasValue )
                    {
                        endDate = GetNextFirstFifteenthDate( endDate );

                        while ( endDate <= paymentEndDate )
                        {
                            numberOfPayments++;
                            paymentDates.Add( endDate.Value );
                            finalEndDateValue = endDate.Value;
                            endDate = GetNextFirstFifteenthDate( endDate );
                        }
                    }
                    endDate = finalEndDateValue;
                    break;
                case "Yearly":
                    while ( endDate <= paymentEndDate )
                    {
                        numberOfPayments++;
                        paymentDates.Add( endDate.Value );
                        endDate = endDate.Value.AddYears( 1 );
                    }
                    endDate = endDate.Value.AddYears( -1 );
                    break;
                default:
                    numberOfPayments = 0;
                    paymentDates.Add( endDate.Value );
                    break;
            }
        }

        private static DateTime? GetNextFirstFifteenthDate( DateTime? initialDateTime )
        {
            var newDateTime = new DateTime();

            // If it's currently after the 15th of the month, the start date should be the
            // first of next month. Otherwise, it should be the 15th of the current month.
            if ( initialDateTime.Value.Day >= 15 )
            {
                if ( initialDateTime.Value.Month == 12 )
                {
                    newDateTime = new DateTime( initialDateTime.Value.Year + 1, 1, 1 );
                }
                else
                {
                    newDateTime = new DateTime( initialDateTime.Value.Year, initialDateTime.Value.Month + 1, 1 );
                }
            }
            else
            {
                newDateTime = new DateTime( initialDateTime.Value.Year, initialDateTime.Value.Month, 15 );
            }

            return newDateTime;
        }

        private static DateTime? GetNext1stOfTheMonthDate( DateTime? initialDateTime )
        {
            var newDateTime = new DateTime();

            // If it's currently after the first of the month, the start date should be the
            // first of next month. Otherwise, it should be the first of the current month.
            if ( initialDateTime.Value.Day >= 1 )
            {
                if ( initialDateTime.Value.Month == 12 )
                {
                    newDateTime = new DateTime( initialDateTime.Value.Year + 1, 1, 1 );
                }
                else
                {
                    newDateTime = new DateTime( initialDateTime.Value.Year, initialDateTime.Value.Month + 1, 1 );
                }
            }
            else
            {
                newDateTime = new DateTime( initialDateTime.Value.Year, initialDateTime.Value.Month, 1 );
            }

            return newDateTime;
        }

        private Registration GetRegistration( bool updateRegistration = false, RockContext rockContext = null )
        {
            if ( _registration == null || updateRegistration == true )
            {
                if ( rockContext == null )
                {
                    rockContext = new RockContext();
                }

                var registrationId = ddlRegistrations.SelectedValue.AsIntegerOrNull();
                if ( registrationId != null )
                {
                    _registration = new RegistrationService( rockContext ).Get( registrationId.Value );
                }
            }

            return _registration;
        }

        private void SetPage( int page )
        {
            // Page 1 = Selection
            // Page 2 = Success

            pnlSelection.Visible = page == 1;
            pnlContributionInfo.Visible = page == 1;

            pnlPayment.Visible = true;
            rblSavedAccount.Visible = page == 1 && rblSavedAccount.Items.Count > 0;
            cbCoverFees.Visible = page == 1 && GetAttributeValue( AttributeKey.AreCoverFeesVisible ).AsBoolean();
            bool usingSavedAccount = rblSavedAccount.Items.Count > 0 && ( rblSavedAccount.SelectedValueAsId() ?? 0 ) > 0;
            divNewPayment.Visible = page == 1;
            pnlPayment.Visible = rblSavedAccount.Visible || divNewPayment.Visible;

            btnPaymentInfoNext.Visible = page == 1;
            pnlSuccess.Visible = page == 2;

            hfCurrentPage.Value = page.ToString();
        }

        #endregion

        /// <summary>
        /// Lightweight object for each contribution item
        /// </summary>
        [Serializable]
        protected class AccountItem : LavaDataObject
        {
            public int Id { get; set; }

            public int Order { get; set; }

            public string Name { get; set; }

            public int? CampusId { get; set; }

            public decimal Amount { get; set; }

            public bool Enabled { get; set; }

            public string PublicName { get; set; }

            public string AmountFormatted
            {
                get
                {
                    return Amount > 0 ? Amount.FormatAsCurrency() : string.Empty;
                }
            }

            public AccountItem( int id, int order, string name, int? campusId, string publicName )
            {
                Id = id;
                Order = order;
                Name = name;
                CampusId = campusId;
                PublicName = publicName;
                Enabled = true;
            }

            public AccountItem( int id, int order, string name, int? campusId, string publicName, decimal amount, bool enabled )
                : this( id, order, name, campusId, publicName )
            {
                Amount = amount;
                Enabled = enabled;
            }
        }
    }
}