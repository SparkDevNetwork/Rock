using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Humanizer;

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_bemaservices.Event
{
    /// <summary>
    /// Block used to show Registration that still need to be paid for
    /// </summary>
    [DisplayName( "Registration Cart" )]
    [Category( "BEMA Services > Event" )]
    [Description( "Block used to register for a registration instance." )]
    [BooleanField( "Default ACH", "With this enable, the payment method will default to ACH", false, "", 0 )]
    [TextField( "Batch Name Prefix", "The batch prefix name to use when creating a new batch", false, "Event Registration", "",3 )]

    public partial class RegistrationCart : RockBlock
    {
        private List<RegistrationItem> _registrations = new List<RegistrationItem>();

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
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
                SetDefaultSettings();

                if ( _registrations.Count == 0 )
                {
                    _registrations = LoadEventRegistrations();
                }

                if ( !_registrations.Any() )
                {
                    pnlMain.Visible = false;
                    pnlNoEvents.Visible = true;
                }

                cTotal.Text = _registrations.Select( x => x.PayingAmount ).Sum().ToString();
            }
            else
            {

            }
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["REGISTRATIONS"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                _registrations = LoadEventRegistrations();
            }
            else
            {
                _registrations = JsonConvert.DeserializeObject<List<RegistrationItem>>( json );
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["REGISTRATIONS"] = JsonConvert.SerializeObject( _registrations, Formatting.None, jsonSetting );
            return base.SaveViewState();
        }


        private void SetDefaultSettings()
        {
            if ( GetAttributeValue( "DefaultACH" ).AsBoolean() )
            {
                liCreditCard.AddCssClass( "active" );
                liACH.RemoveCssClass( "active" );
                divNewCard.Visible = false;
                divACHPaymentInfo.Visible = true;
                hfPaymentTab.Value = "ACH";
            }
            else
            {
                liACH.RemoveCssClass( "active" );
                liCreditCard.AddCssClass( "active" );
                divNewCard.Visible = true;
                divACHPaymentInfo.Visible = false;
                hfPaymentTab.Value = "Card";
            }
        }

        private List<RegistrationItem> LoadEventRegistrations()
        {
            if ( CurrentPerson == null )
            {
                // throw error here
                throw new Exception( "You must be logged in to use this block" );
            }

            RockContext rockContext = new RockContext();
            RegistrationService registrationService = new RegistrationService( rockContext );

            List<RegistrationItem> registrations = new List<RegistrationItem>();

            var person = CurrentPerson;
            var family = person.PrimaryFamily;

            // Getting event registrations for self
            // Need to ask David about WaitList. Should this only pull registations where the person is not on a waitlist?
            var selfRegistrations = registrationService.Queryable().AsNoTracking().Where( x => x.PersonAlias.PersonId == person.Id ).ToList();
            selfRegistrations = selfRegistrations.Where( x => x.TotalCost > x.TotalPaid ).ToList();
            if ( selfRegistrations.Count > 0 )
            {
                foreach ( var selfRegistration in selfRegistrations )
                {
                    registrations.Add( new RegistrationItem
                    {
                        Id = selfRegistration.Id,
                        RegistrationId = selfRegistration.Id,
                        Registrants = string.Join( ", ", selfRegistration.Registrants.Select( x => x.FirstName + " " + x.LastName ) ),
                        EventName = selfRegistration.RegistrationInstance.Name,
                        TotalCost = selfRegistration.TotalCost,
                        TotalPaid = selfRegistration.TotalPaid,
                        TotalRemaining = selfRegistration.TotalCost - selfRegistration.TotalPaid,
                        PayingAmount = selfRegistration.TotalCost - selfRegistration.TotalPaid,
                        AllowUpdates = selfRegistration.RegistrationInstance.RegistrationTemplate.AllowExternalRegistrationUpdates
                    } );
                }
            }

            // Checking if current person is an adult
            if ( person.GetFamilyRole().Guid == Guid.Parse( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) )
            {
                if ( family != null )
                {
                    var familyMembers = family.Members.Select( x => x.Person.Id ).ToList();
                    var familyRegistrations = registrationService.Queryable().AsNoTracking().Where( x => familyMembers.Contains( x.PersonAlias.Person.Id ) ).ToList();
                    familyRegistrations = familyRegistrations.Where( x => x.TotalCost > x.TotalPaid ).ToList();
                    if ( familyRegistrations.Count > 0 )
                    {
                        foreach ( var selfRegistration in familyRegistrations )
                        {
                            // Checking if item is already in list
                            if ( !registrations.Select( x => x.Id ).ToList().Contains( selfRegistration.Id ) )
                            {
                                var registrationItem = new RegistrationItem
                                {
                                    Id = selfRegistration.Id,
                                    RegistrationId = selfRegistration.Id,
                                    Registrants = string.Join( ", ", selfRegistration.Registrants.Select( x => x.FirstName + " " + x.LastName ) ),
                                    EventName = selfRegistration.RegistrationInstance.Name,
                                    TotalCost = selfRegistration.TotalCost,
                                    TotalPaid = selfRegistration.TotalPaid,
                                    TotalRemaining = selfRegistration.TotalCost - selfRegistration.TotalPaid,
                                    PayingAmount = selfRegistration.TotalCost - selfRegistration.TotalPaid,
                                    AllowUpdates = selfRegistration.RegistrationInstance.RegistrationTemplate.AllowExternalRegistrationUpdates
                                };

                                registrations.Add( registrationItem );
                            }
                        }
                    }
                }
            }

            rData.DataSource = registrations;
            rData.DataBind();

            return registrations;
        }

        protected void btnCreditCard_Click( object sender, EventArgs e )
        {
            divNewCard.Visible = true;
            divACHPaymentInfo.Visible = false;
            liCreditCard.AddCssClass( "active" );
            liACH.RemoveCssClass( "active" );
            hfPaymentTab.Value = "Card";
        }

        protected void btnACH_Click( object sender, EventArgs e )
        {
            divACHPaymentInfo.Visible = true;
            divNewCard.Visible = false;
            liACH.AddCssClass( "active" );
            liCreditCard.RemoveCssClass( "active" );
            hfPaymentTab.Value = "ACH";
        }

        protected void btnProcess_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            RegistrationService registrationService = new RegistrationService( rockContext );

            // Making sure payment information is provided
            var validationErrors = ValidateFields();
            if ( validationErrors.Any() )
            {
                lErrorMessages.Text = string.Format( "<div class='alert alert-danger'><ul><li>{0}</li></ul></div>", validationErrors.AsDelimited( "</li><li>" ) );
            }
            else
            {
                List<string> errorMessages = new List<string>();
                foreach ( var registrationItem in _registrations )
                {
                    string errorMessage = null;
                    if ( registrationItem.PayingAmount > 0 )
                    {
                        var registration = registrationService.Get( registrationItem.RegistrationId );

                        if ( registration != null )
                        {
                            var processSuccess = ProcessPayment( rockContext, registration, registrationItem, out errorMessage );
                            if ( !string.IsNullOrWhiteSpace( errorMessage ) || processSuccess == false )
                            {
                                if( processSuccess == false )
                                {
                                    errorMessages.Add( "An error occurred while completing transactions." );
                                }
                                else
                                {
                                    errorMessages.Add( errorMessage );
                                }
                            }
                        }
                    }
                }

                // Checking if an error messages were cause during process
                if ( errorMessages.Any() )
                {
                    ShowError( errorMessages );
                }
                else
                {
                    ShowSuccess( _registrations );
                }
            }
        }

        /// <summary>
        /// Validates the fields.
        /// </summary>
        /// <returns></returns>
        private List<string> ValidateFields()
        {
            var errorMessages = new List<string>();

            // Checking if this is an ACH or a Card
            if ( hfPaymentTab.Value == "Card" )
            {
                // Processing as card
                var rgx = new System.Text.RegularExpressions.Regex( @"[^\d]" );
                string ccNum = rgx.Replace( txtCreditCard.Text, string.Empty );

                if ( string.IsNullOrWhiteSpace( txtCardFirstName.Text ) )
                {
                    errorMessages.Add( "Card First Name is required" );
                }

                if ( string.IsNullOrWhiteSpace( txtCardLastName.Text ) )
                {
                    errorMessages.Add( "Card Last Name is required" );
                }

                if ( string.IsNullOrWhiteSpace( ccNum ) )
                {
                    errorMessages.Add( "Card Number is required" );
                }

                if ( !mypExpiration.SelectedDate.HasValue )
                {
                    errorMessages.Add( "Card Expiration Date is required " );
                }

                if ( string.IsNullOrWhiteSpace( txtCVV.Text ) )
                {
                    errorMessages.Add( "Card Security Code is required" );
                }
            }
            else
            {
                if ( string.IsNullOrWhiteSpace( txtAccountName.Text ) )
                {
                    errorMessages.Add( "Account Name is required" );
                }

                if ( string.IsNullOrWhiteSpace( txtRoutingNumber.Text ) )
                {
                    errorMessages.Add( "Routing Number is required" );
                }

                if ( string.IsNullOrWhiteSpace( txtAccountNumber.Text ) )
                {
                    errorMessages.Add( "Account Number is required" );
                }
            }

            return errorMessages;
        }

        /// <summary>
        /// Processes the payment.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="registration">The registration.</param>
        /// <param name="registrationItem">The registration.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private bool ProcessPayment( RockContext rockContext, Registration registration, RegistrationItem registrationItem, out string errorMessage )
        {
            GatewayComponent gateway = null;
            if (
                registration != null &&
                registration.RegistrationInstance != null &&
                registration.RegistrationInstance.RegistrationTemplate != null &&
                registration.RegistrationInstance.RegistrationTemplate.FinancialGateway != null
            )
            {
                gateway = registration.RegistrationInstance.RegistrationTemplate.FinancialGateway.GetGatewayComponent();
            }

            if ( gateway == null )
            {
                errorMessage = "There was a problem creating the payment gateway information";
                return false;
            }

            if ( !registration.RegistrationInstance.AccountId.HasValue || registration.RegistrationInstance.Account == null )
            {
                errorMessage = "There was a problem with the account configuration for this " + registration.RegistrationInstance.RegistrationTemplate.RegistrationTerm.ToLower();
                return false;
            }

            PaymentInfo paymentInfo = null;

            // Checking if this is an ACH or a Card
            if ( hfPaymentTab.Value == "Card" )
            {
                paymentInfo = GetPaymentInfo( gateway, registration, registrationItem, Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD );
            }
            else
            {
                paymentInfo = GetPaymentInfo( gateway, registration, registrationItem, Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH );
            }

            paymentInfo.Comment1 = string.Format( "{0} ({1})", registration.RegistrationInstance.Name, registration.RegistrationInstance.Account.GlCode );

            var transaction = gateway.Charge( registration.RegistrationInstance.RegistrationTemplate.FinancialGateway, paymentInfo, out errorMessage );

            return SaveTransaction( gateway, registration, transaction, paymentInfo, rockContext );
        }

        /// <summary>
        /// Gets the payment information.
        /// </summary>
        /// <param name="gateway">The gateway.</param>
        /// <param name="registration">The registration.</param>
        /// <param name="registrationItem">The registration item.</param>
        /// <param name="currencyType">Type of the currency.</param>
        /// <returns></returns>
        private PaymentInfo GetPaymentInfo( GatewayComponent gateway, Registration registration, RegistrationItem registrationItem, string currencyType )
        {
            PaymentInfo paymentInfo;
            if ( currencyType == Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD )
            {
                CreditCardPaymentInfo ccPaymentInfo = GetCCPaymentInfo();
                ccPaymentInfo.NameOnCard = gateway != null ? txtCardFirstName.Text : "";
                ccPaymentInfo.LastNameOnCard = txtCardLastName.Text;

                ccPaymentInfo.BillingStreet1 = acBillingAddress.Street1;
                ccPaymentInfo.BillingStreet2 = acBillingAddress.Street2;
                ccPaymentInfo.BillingCity = acBillingAddress.City;
                ccPaymentInfo.BillingState = acBillingAddress.State;
                ccPaymentInfo.BillingPostalCode = acBillingAddress.PostalCode;
                ccPaymentInfo.BillingCountry = acBillingAddress.Country;

                paymentInfo = ccPaymentInfo;
            }
            else
            {
                paymentInfo = GetACHInfo();
            }

            paymentInfo.Amount = registrationItem.PayingAmount;
            paymentInfo.Email = registration.ConfirmationEmail;

            paymentInfo.Street1 = acBillingAddress.Street1;
            paymentInfo.Street2 = acBillingAddress.Street2;
            paymentInfo.City = acBillingAddress.City;
            paymentInfo.State = acBillingAddress.State;
            paymentInfo.PostalCode = acBillingAddress.PostalCode;
            paymentInfo.Country = acBillingAddress.Country;

            paymentInfo.FirstName = registration.FirstName;
            paymentInfo.LastName = registration.LastName;

            return paymentInfo;
        }

        /// <summary>
        /// Creates a CreditCardPaymentInfo obj using data in the UI and RegistrationState
        /// </summary>
        /// <param name="gateway">The gateway.</param>
        /// <param name="gateway">The gateway.</param>
        /// <returns></returns>
        private CreditCardPaymentInfo GetCCPaymentInfo()
        {
            return new CreditCardPaymentInfo( txtCreditCard.Text, txtCVV.Text, mypExpiration.SelectedDate != null ? mypExpiration.SelectedDate.Value : new DateTime() );
        }


        /// <summary>
        /// Gets the ACH information.
        /// </summary>
        /// <returns></returns>
        private ACHPaymentInfo GetACHInfo()
        {
            return new ACHPaymentInfo( txtAccountNumber.Text, txtRoutingNumber.Text, rblAccountType.SelectedValue == "Savings" ? BankAccountType.Savings : BankAccountType.Checking );
        }


        /// <summary>
        /// Saves the transaction.
        /// </summary>
        /// <param name="gateway">The gateway.</param>
        /// <param name="registration">The registration.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private bool SaveTransaction( GatewayComponent gateway, Registration registration, FinancialTransaction transaction, PaymentInfo paymentInfo, RockContext rockContext )
        {
            if ( transaction != null )
            {
                transaction.AuthorizedPersonAliasId = registration.PersonAliasId;
                transaction.TransactionDateTime = RockDateTime.Now;
                transaction.FinancialGatewayId = registration.RegistrationInstance.RegistrationTemplate.FinancialGatewayId;

                var txnType = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_EVENT_REGISTRATION ) );
                transaction.TransactionTypeValueId = txnType.Id;

                if ( transaction.FinancialPaymentDetail == null )
                {
                    transaction.FinancialPaymentDetail = new FinancialPaymentDetail();
                }

                DefinedValueCache currencyType = null;
                DefinedValueCache creditCardType = null;

                if ( paymentInfo != null )
                {
                    transaction.FinancialPaymentDetail.SetFromPaymentInfo( paymentInfo, gateway, rockContext );
                    currencyType = paymentInfo.CurrencyTypeValue;
                    creditCardType = paymentInfo.CreditCardTypeValue;
                }

                Guid sourceGuid = Guid.Empty;
                if ( Guid.TryParse( GetAttributeValue( "Source" ), out sourceGuid ) )
                {
                    var source = DefinedValueCache.Get( sourceGuid );
                    if ( source != null )
                    {
                        transaction.SourceTypeValueId = source.Id;
                    }
                }

                transaction.Summary = registration.GetSummary( registration.RegistrationInstance );

                var transactionDetail = new FinancialTransactionDetail();
                transactionDetail.Amount = paymentInfo.Amount;
                transactionDetail.AccountId = registration.RegistrationInstance.AccountId.Value;
                transactionDetail.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Registration ) ).Id;
                transactionDetail.EntityId = registration.Id;
                transaction.TransactionDetails.Add( transactionDetail );

                var batchChanges = new History.HistoryChangeList();

                rockContext.WrapTransaction( () =>
                {
                    var batchService = new FinancialBatchService( rockContext );

                // determine batch prefix
                string batchPrefix = string.Empty;
                    if ( !string.IsNullOrWhiteSpace( registration.RegistrationInstance.RegistrationTemplate.BatchNamePrefix ) )
                    {
                        batchPrefix = registration.RegistrationInstance.RegistrationTemplate.BatchNamePrefix;
                    }
                    else
                    {
                        batchPrefix = GetAttributeValue( "BatchNamePrefix" );
                    }

                // Get the batch
                var batch = batchService.Get(
                        batchPrefix,
                        currencyType,
                        creditCardType,
                        transaction.TransactionDateTime.Value,
                            registration.RegistrationInstance.RegistrationTemplate.FinancialGateway.GetBatchTimeOffset() );

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
                    batch.Transactions.Add( transaction );

                    rockContext.SaveChanges();
                } );

                if ( transaction.BatchId.HasValue )
                {
                    Task.Run( () =>
                        HistoryService.SaveChanges(
                            new RockContext(),
                            typeof( FinancialBatch ),
                            Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                            transaction.BatchId.Value,
                            batchChanges,
                            true,
                            CurrentPersonAliasId ) );
                }

                var registrationChanges = new History.HistoryChangeList();
                registrationChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Payment" ).SetNewValue( string.Format( "{0} payment", transaction.TotalAmount.FormatAsCurrency() ) );
                Task.Run( () =>
                    HistoryService.SaveChanges(
                        new RockContext(),
                        typeof( Registration ),
                        Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                        registration.Id,
                        registrationChanges,
                        true,
                        CurrentPersonAliasId ) );

                return true;
            }
            else
            {
                return false;
            }
        }

        private void ShowError( List<string> errorMessages )
        {
            rptErrors.DataSource = errorMessages.ToList();
            rptErrors.DataBind();
            pnlMain.Visible = false;
            pnlError.Visible = true;

            // Need to clear Viewstate Here.
            ViewState["REGISTRATIONS"] = null;
        }

        private void ShowSuccess( List<RegistrationItem> registrations )
        {
            rptSuccessItems.DataSource = registrations.Where( x => x.PayingAmount > 0 ).ToList();
            rptSuccessItems.DataBind();
            pnlMain.Visible = false;
            pnlSuccess.Visible = true;

            // Need to clear Viewstate Here.
            ViewState["REGISTRATIONS"] = null;
        }


        protected void cPaymentAmount_TextChanged( object sender, EventArgs e )
        {
            CurrencyBox currencyBox = ( CurrencyBox ) sender;
            RepeaterItem dataItem = ( RepeaterItem ) currencyBox.DataItemContainer;
            var itemIndex = dataItem.ItemIndex;
            _registrations[itemIndex].PayingAmount = currencyBox.Text.AsDecimal();

            // Updating total
            cTotal.Text = _registrations.Select( x => x.PayingAmount ).Sum().ToString();
        }
    }

    public class RegistrationItem
    {
        public int Id { get; set; }
        public int RegistrationId { get; set; }
        public string ImageURL { get; set; }
        public string EventName { get; set; }
        public string Registrants { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalRemaining { get; set; }
        public decimal PayingAmount { get; set; }
        public bool AllowUpdates { get; set; }
    }
}