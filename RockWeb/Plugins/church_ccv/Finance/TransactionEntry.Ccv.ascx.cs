using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.church_ccv.Finance
{
    [DisplayName( "Transaction Entry (CCV)" )]
    [Category( "CCV > Finance" )]
    [Description( "Custom CCV block that provides an optimized experience for giving" )]

    [FinancialGatewayField( "Credit Card Gateway", "The payment gateway to use for Credit Card transactions", false, "", "", 0, "CCGateway" )]
    [TextField( "Batch Name Prefix", "The batch prefix name to use when creating a new batch", false, "Online Giving", "", 2 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE, "Source", "The Financial Source Type to use when creating transactions", false, false,
        Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_WEBSITE, "", 3 )]
    [AccountsField( "Accounts", "The accounts to display. By default all active accounts with a Public Name will be displayed", false, "", "", 6 )]

    [BooleanField( "Scheduled Transactions", "Allow", "Don't Allow",
        "If the selected gateway(s) allow scheduled transactions, should that option be provided to user", true, "", 8, "AllowScheduled" )]

    [GroupLocationTypeField( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY, "Address Type", "The location type to use for the person's address", false,
        Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME, "", 11 )]
    [SystemEmailField( "Confirm Account", "Confirm Account Email Template", false, Rock.SystemGuid.SystemEmail.SECURITY_CONFIRM_ACCOUNT, "Email Templates", 12, "ConfirmAccountTemplate" )]

    [DefinedValueField( "2E6540EA-63F0-40FE-BE50-F2A84735E600", "Connection Status", "The connection status to use for new individuals (default: 'Web Prospect'.)", true, false, "368DD475-242C-49C4-A42C-7278BE690CC2", "", 25 )]
    [DefinedValueField( "8522BADD-2871-45A5-81DD-C76DA07E2E7E", "Record Status", "The record status to use for new individuals (default: 'Pending'.)", true, false, "283999EC-7346-42E3-B807-BCE9B2BABB49", "", 26 )]

    [CodeEditorField( "Success Template", "The text (HTML) to display when a transaction is successfully submitted. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",
        CodeEditorMode.Html, CodeEditorTheme.Rock, 200, true, @"
<div class='well'>
    <legend>Gift Information</legend>
    <p>
        Thank you for your generous contribution.  Your support is helping {{ 'Global' | Attribute:'OrganizationName' }} actively
        achieve our mission.  We are so grateful for your commitment.
    </p>
    <dl class='dl-horizontal gift-success'>
        {% if PaymentSchedule.Id %}<dt>Payment Schedule ID</dt><dd>{{ PaymentSchedule.Id }}</dd>{% endif %}
        <dt>Confirmation Code</dt><dd>{{ TransactionCode }}</dd>
        <dt> </dt><dd> </dd>
        <dt>Name</dt><dd>{{ Person.FullName }}</dd>
        {% if PaymentInfo.Phone %}<dt>Phone</dt><dd>{{ PaymentInfo.Phone }}</dd>{% endif %}
        <dt>Email</dt><dd>{{ PaymentInfo.Email }}</dd>
        {% if Address %}<dt>Address</dt><dd>{{ Address }}</dd>{% endif %}
        <dt>Account</dt><dd>{{ Account.Name }}</dd>
        <dt>Amount</dt><dd>{{ 'Global' | Attribute:'CurrencySymbol' }}{{ PaymentInfo.Amount | Format:'#,##0.00' }}</dd>
        <dt> </dt><dd> </dd>
        <dt>Payment Method</dt><dd>Credit Card</dd>
        <dt>Account Number</dt><dd>{% if PaymentInfo.MaskedNumber %>{{ PaymentInfo.MaskedNumber }}{% else %}{{ PaymentDetail.AccountNumberMasked }}{% endif %}</dd>
        {% if PaymentSchedule %}<dt>When</dt><dd>{{ PaymentSchedule }}</dd>{% endif %}
    </dl>
</div>
", "Text Options", 22 )]

    [TextField( "Save Account Title", "The text to display as heading of section for saving payment information.", false, "Make Giving Even Easier", "Text Options", 24 )]
    [SystemEmailField( "Receipt Email", "The system email to use to send the receipt.", false, "", "Email Templates", 27 )]
    public partial class TransactionEntryCcv : Rock.Web.UI.RockBlock
    {
        /// <summary>
        /// Gets or sets the giving funds json.
        /// </summary>
        /// <value>
        /// The giving funds json.
        /// </value>
        public string GivingFundsJSON { get; set; }

        /// <summary>
        /// Gets or sets the campus fund locations json.
        /// </summary>
        /// <value>
        /// The campus fund locations json.
        /// </value>
        public string CampusFundLocationsJSON { get; set; }

        public string CurrentCampusId { get; set; }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            this.RockPage.AddCSSLink( ResolveUrl( "~/Plugins/church_ccv/Finance/styles/main.css" ) );
            this.RockPage.AddCSSLink( ResolveUrl( "~/Plugins/church_ccv/Finance/styles/vendor.css" ) );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += TransactionEntryCcv_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upPayment );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the TransactionEntryCcv control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void TransactionEntryCcv_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !this.IsPostBack )
            {
                ShowDetail();
            }
            else
            {

            }
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            hfTransactionGuid.Value = Guid.NewGuid().ToString();
            if ( CurrentPerson != null )
            {
                hfFullName.Value = CurrentPerson.FullName;
                hfHideFullNameInput.Value = "1";
                lFullName.Text = string.Format( "<label class='form-control-static js-static-fullname'>{0}</label>", CurrentPerson.FullName );
                tbFirstName.Value = CurrentPerson.FirstName;
                tbLastName.Value = CurrentPerson.LastName;
                tbEmail.Value = CurrentPerson.Email;
            }
            else
            {
                hfHideFullNameInput.Value = "0";
                lFullName.Visible = false;
            }

            LoadDropDowns();
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        protected void LoadDropDowns()
        {
            var rockContext = new RockContext();

            var selectedAccountGuids = GetAttributeValue( "Accounts" ).SplitDelimitedValues().ToList().AsGuidList();
            bool showAll = !selectedAccountGuids.Any();
            var today = RockDateTime.Today;

            var accountsQry = new FinancialAccountService( rockContext ).Queryable()
                    .Where( f =>
                        f.IsActive &&
                        f.IsPublic.HasValue &&
                        f.IsPublic.Value &&
                        ( f.StartDate == null || f.StartDate <= today ) &&
                        ( f.EndDate == null || f.EndDate >= today ) );

            if ( selectedAccountGuids.Any() )
            {
                accountsQry = accountsQry.Where( a => selectedAccountGuids.Contains( a.Guid ) );
            }

            var accounts = accountsQry
                    .OrderBy( f => f.Order )
                    .ThenBy( f => f.Name )
                    .Select( a => new
                    {
                        AccountId = a.Id.ToString(),
                        Name = a.Name,
                        CampusId = a.CampusId
                    } ).ToList();

            var campusFundLocations = CampusCache.All().OrderBy( a => a.Name ).Select( a =>
                new
                {
                    campusId = a.Id,
                    account = accounts.Where( c => c.CampusId == a.Id ).Select( x => new
                    {
                        Id = x.AccountId,
                        Text = x.Name
                    } ).FirstOrDefault(),

                    longitude = a.LocationId.HasValue ? a.Location.Longitude : (double?)null,
                    latitude = a.LocationId.HasValue ? a.Location.Latitude : (double?)null,
                } );

            // these are needed for the Location Detector and FundSetter js to work
            this.GivingFundsJSON = accounts.Select( a => new { value = a.AccountId, text = a.Name, campus = a.CampusId } ).ToJson();
            this.CampusFundLocationsJSON = campusFundLocations.ToJson();
            var campusEntityType = EntityTypeCache.Read(typeof(Campus));
            var currentCampus = RockPage.GetCurrentContext(campusEntityType) as Campus;
            if (currentCampus != null)
                this.CurrentCampusId = currentCampus.Id.ToString();

            ddlAccounts.Items.Clear();
            foreach ( var account in accounts )
            {
                ddlAccounts.Items.Add( new ListItem( account.Name, account.AccountId.ToString() ) );
            }

            if ( CurrentPersonId.HasValue )
            {
                FinancialGateway financialGateway = this.GetGateway( rockContext, "CCGateway" );
                var ccCurrencyType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD ) );
                if ( financialGateway != null && ccCurrencyType != null )
                {
                    // Get the saved accounts for the currently logged in user
                    var savedAccountList = new FinancialPersonSavedAccountService( rockContext )
                        .GetByPersonId( CurrentPersonId.Value )
                        .Where( a =>
                            a.FinancialGatewayId == financialGateway.Id &&
                            a.FinancialPaymentDetail != null &&
                            a.FinancialPaymentDetail.CurrencyTypeValueId == ccCurrencyType.Id )
                        .OrderBy( a => a.Name )
                        .Select( a => new
                        {
                            a.Id,
                            Name = "Use " + a.Name + " (" + a.FinancialPaymentDetail.AccountNumberMasked + ")"
                        } ).ToList();

                    rblSavedAccount.DataSource = savedAccountList;
                    rblSavedAccount.DataBind();
                    if ( rblSavedAccount.Items.Count > 0 )
                    {
                        rblSavedAccount.Items.Add( new ListItem( "Use a new card", "0" ) );
                        if ( rblSavedAccount.SelectedValue == string.Empty )
                        {
                            rblSavedAccount.Items[0].Selected = true;
                        }

                        rblSavedAccount.Visible = true;
                    }
                    else
                    {
                        rblSavedAccount.Visible = false;
                    }

                    if ( rblSavedAccount.Visible && rblSavedAccount.SelectedValue != "0" )
                    {
                        pnlCardInput.Style[HtmlTextWriterStyle.Display] = "none";
                        pnlCardGraphicHolder.Style[HtmlTextWriterStyle.Display] = "none";
                    }
                }
            }
        }

        #region Similar code from TransactionEntry.ascx.cs

        /// <summary>
        /// Handles the Click event of the btnSubmit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSubmit_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var transactionGuid = hfTransactionGuid.Value.AsGuid();

            // validate configuration
            var financialGateway = GetGateway( rockContext, "CCGateway" );
            var gateway = GetGatewayComponent( rockContext, financialGateway );

            if ( gateway == null )
            {
                nbMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbMessage.Title = "ERROR";
                nbMessage.Text = "There was a problem creating the payment gateway information";
                nbMessage.Visible = true;
                return;
            }

            // valid UI inputs
            // NOTE: The Client Javascript should have caught most of these, but just in case
            var errorMessage = string.Empty;
            if ( !ValidateEntries( out errorMessage ) )
            {
                nbMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbMessage.Text = errorMessage;
                nbMessage.Visible = true;
                return;
            }

            // validate/create person
            Person person = GetPerson( true );
            if ( person == null )
            {
                nbMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbMessage.Title = "ERROR";
                nbMessage.Text = "There was a problem creating the person information";
                nbMessage.Visible = true;
                return;
            }

            if ( !person.PrimaryAliasId.HasValue )
            {
                nbMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbMessage.Title = "ERROR";
                nbMessage.Text = "There was a problem creating the person's primary alias";
                nbMessage.Visible = true;
                return;
            }

            // set up the CreditCard charge
            PaymentInfo paymentInfo = null;

            var savedAccountId = rblSavedAccount.SelectedValue.AsInteger();
            if ( savedAccountId != 0 )
            {
                // used a saved account
                var savedAccount = new FinancialPersonSavedAccountService( new RockContext() ).Get( savedAccountId );
                if ( savedAccount != null )
                {
                    paymentInfo = savedAccount.GetReferencePayment();
                }
            }
            else
            {
                // used a new card
                var cardMMYY = tbCardExpiry.Value.Split( new char[] { '/' } ).Select( a => a.Trim().AsInteger() ).ToArray();
                var expDateTime = new DateTime( 2000 + cardMMYY[1], cardMMYY[0], 1 );

                var ccPaymentInfo = new CreditCardPaymentInfo( tbCardNumber.Value.Replace( " ", string.Empty ), tbCardCvc.Value, expDateTime );
                ccPaymentInfo.NameOnCard = gateway.SplitNameOnCard ? tbFirstName.Value.Trim() : hfFullName.Value.Trim();
                ccPaymentInfo.LastName = tbLastName.Value.Trim();

                paymentInfo = ccPaymentInfo;
            }

            if ( !string.IsNullOrWhiteSpace( tbStreet.Value ) )
            {
                if ( paymentInfo is CreditCardPaymentInfo )
                {
                    var ccPaymentInfo = paymentInfo as CreditCardPaymentInfo;
                    ccPaymentInfo.BillingStreet1 = tbStreet.Value.Trim();
                    ccPaymentInfo.BillingStreet2 = string.Empty;
                    ccPaymentInfo.BillingCity = tbCity.Value.Trim();
                    ccPaymentInfo.BillingState = tbState.Value.Trim();
                    ccPaymentInfo.BillingPostalCode = tbZip.Value.Trim();
                    ccPaymentInfo.BillingCountry = GlobalAttributesCache.Read().OrganizationCountry;
                }

                paymentInfo.Street1 = tbStreet.Value.Trim();
                paymentInfo.Street2 = string.Empty;
                paymentInfo.City = tbCity.Value.Trim();
                paymentInfo.State = tbState.Value.Trim();
                paymentInfo.PostalCode = tbZip.Value.Trim();
                paymentInfo.Country = GlobalAttributesCache.Read().OrganizationCountry;
            }

            paymentInfo.Amount = tbAmount.Text.AsDecimal();
            paymentInfo.Email = tbEmail.Value.Trim();

            if ( !string.IsNullOrWhiteSpace( tbPhone.Value ) )
            {
                paymentInfo.Phone = PhoneNumber.FormattedNumber( PhoneNumber.DefaultCountryCode(), tbPhone.Value.Trim(), true );
            }

            PaymentSchedule schedule = GetSchedule();

            FinancialPaymentDetail paymentDetail = null;
            if ( schedule != null )
            {
                schedule.PersonId = person.Id;

                var scheduledTransactionAlreadyExists = new FinancialScheduledTransactionService( rockContext ).Queryable().FirstOrDefault( a => a.Guid == transactionGuid );
                if ( scheduledTransactionAlreadyExists != null )
                {
                    // hopefully shouldn't happen, but just in case the scheduledtransaction already went thru, show the success screen
                    ShowSuccess( person, paymentInfo, schedule, scheduledTransactionAlreadyExists.FinancialPaymentDetail, scheduledTransactionAlreadyExists.TransactionCode, scheduledTransactionAlreadyExists.GatewayScheduleId );
                    return;
                }

                var scheduledTransaction = gateway.AddScheduledPayment( financialGateway, schedule, paymentInfo, out errorMessage );
                if ( scheduledTransaction == null )
                {
                    nbMessage.NotificationBoxType = NotificationBoxType.Danger;
                    nbMessage.Title = "Payment Error";
                    nbMessage.Text = errorMessage;
                    nbMessage.Visible = true;
                    return;
                }

                // manually assign the Guid that we generated at the beginning of the transaction UI entry to help make duplicate scheduledTransactions impossible
                scheduledTransaction.Guid = transactionGuid;

                SaveScheduledTransaction( financialGateway, gateway, person, paymentInfo, schedule, scheduledTransaction, rockContext );
                paymentDetail = scheduledTransaction.FinancialPaymentDetail.Clone( false );

                ShowSuccess( person, paymentInfo, schedule, paymentDetail, scheduledTransaction.TransactionCode, scheduledTransaction.GatewayScheduleId );
            }
            else
            {
                var transactionAlreadyExists = new FinancialTransactionService( rockContext ).Queryable().FirstOrDefault( a => a.Guid == transactionGuid );
                if ( transactionAlreadyExists != null )
                {
                    // hopefully shouldn't happen, but just in case the transaction already went thru, show the success screen
                    ShowSuccess( person, paymentInfo, null, transactionAlreadyExists.FinancialPaymentDetail, transactionAlreadyExists.TransactionCode, null );
                    return;
                }

                // Charge CC
                var transaction = gateway.Charge( financialGateway, paymentInfo, out errorMessage );
                if ( transaction == null )
                {
                    nbMessage.NotificationBoxType = NotificationBoxType.Danger;
                    nbMessage.Title = "Payment Error";
                    nbMessage.Text = errorMessage;
                    nbMessage.Visible = true;
                    return;
                }

                // manually assign the Guid that we generated at the beginning of the transaction UI entry to help make duplicate transactions impossible
                transaction.Guid = transactionGuid;

                SaveTransaction( financialGateway, gateway, person, paymentInfo, transaction, rockContext );
                paymentDetail = transaction.FinancialPaymentDetail.Clone( false );

                ShowSuccess( person, paymentInfo, schedule, paymentDetail, transaction.TransactionCode, null );
            }
        }

        /// <summary>
        /// Shows the success.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="paymentSchedule">The payment schedule.</param>
        /// <param name="financialPaymentDetail">The financial payment detail.</param>
        /// <param name="transactionCode">The transaction code.</param>
        private void ShowSuccess( Person person, PaymentInfo paymentInfo, PaymentSchedule paymentSchedule, FinancialPaymentDetail financialPaymentDetail, string transactionCode, string gatewayScheduleId )
        {
            upPayment.Update();
            var accountId = ddlAccounts.SelectedValue.AsInteger();
            var rockContext = new RockContext();
            var account = new FinancialAccountService( rockContext ).Get( accountId );

            givingForm.Visible = false;
            pnlSuccess.Visible = true;
            var successTemplate = this.GetAttributeValue( "SuccessTemplate" );
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage );
            mergeFields.Add( "TransactionCode", transactionCode );
            mergeFields.Add( "PaymentInfo", DotLiquid.Hash.FromAnonymousObject( paymentInfo ) );
            mergeFields.Add( "PaymentDetail", financialPaymentDetail );
            if ( paymentSchedule != null )
            {
                mergeFields.Add( "PaymentSchedule", paymentSchedule.ToString() );
            }

            mergeFields.Add( "Account", account );
            mergeFields.AddOrReplace( "Person", person );
            lSuccessContent.Text = successTemplate.ResolveMergeFields( mergeFields );

            var financialGateway = GetGateway( rockContext, "CCGateway" );
            var gatewayComponent = GetGatewayComponent( rockContext, financialGateway );

            // If there was a transaction code returned and this was not already created from a previous saved account,
            // show the option to save the account.
            if ( !( paymentInfo is ReferencePaymentInfo ) && !string.IsNullOrWhiteSpace( transactionCode ) && gatewayComponent.SupportsSavedAccount( paymentInfo.CurrencyTypeValue ) )
            {
                hfSaveAccountScheduleId.Value = gatewayScheduleId;
                hfSaveAccountTransactionCode.Value = transactionCode;
                cbSaveAccount.Visible = true;
                pnlSaveAccount.Visible = true;
                txtSaveAccount.Visible = true;

                // If current person does not have a login, have them create a username and password
                phCreateLogin.Visible = !new UserLoginService( rockContext ).GetByPersonId( person.Id ).Any();
            }
            else
            {
                pnlSaveAccount.Visible = false;
            }
        }

        /// <summary>
        /// Gets the schedule.
        /// </summary>
        /// <returns></returns>
        private PaymentSchedule GetSchedule()
        {
            // Figure out if this is a one-time transaction or a future scheduled transaction
            if ( GetAttributeValue( "AllowScheduled" ).AsBoolean() )
            {
                if ( cbRepeating.Checked == false )
                {
                    // one-time immediate payment
                    return null;
                }

                var schedule = new PaymentSchedule();
                if ( rbBiWeekly.Checked )
                {
                    schedule.TransactionFrequencyValue = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_BIWEEKLY.AsGuid() );
                }
                else if ( rbMonthly.Checked )
                {
                    schedule.TransactionFrequencyValue = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_MONTHLY.AsGuid() );
                }
                else if ( rbTwiceMonthly.Checked )
                {
                    schedule.TransactionFrequencyValue = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_TWICEMONTHLY.AsGuid() );
                }
                else if ( rbWeekly.Checked )
                {
                    schedule.TransactionFrequencyValue = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_WEEKLY.AsGuid() );
                }

                var startDate = dpStartDate.Value.AsDateTime();

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
        /// Validates the entries.
        /// </summary>
        /// <returns></returns>
        private bool ValidateEntries( out string errorMessage )
        {
            var amount = tbAmount.Text.AsDecimalOrNull();

            errorMessage = string.Empty;

            var errorMessages = new List<string>();

            // Validate that an amount was entered
            if ( (amount ?? 0) == 0 )
            {
                errorMessages.Add( "Make sure you've entered an amount." );
            }

            // Validate that no negative amounts were entered
            if ( amount < 0 )
            {
                errorMessages.Add( "Make sure the amount you've entered is a positive amount." );
            }

            // validate the payment schedule
            if ( cbRepeating.Checked )
            {
                var startDate = dpStartDate.Value.AsDateTime();

                if ( startDate == null )
                {
                    errorMessages.Add( "When scheduling a repeating payment, make sure the First Gift date is specified." );
                }

                // Make sure a repeating payment starts in the future
                if ( startDate <= RockDateTime.Today )
                {
                    errorMessages.Add( "When scheduling a repeating payment, make sure the First Gift date is in the future (after today)." );
                }
            }

            if ( CurrentPerson == null && ( string.IsNullOrWhiteSpace( tbFirstName.Value ) || string.IsNullOrWhiteSpace( tbLastName.Value ) ) )
            {
                errorMessages.Add( "Make sure to enter both a first and last name." );
            }

            if ( string.IsNullOrWhiteSpace( tbEmail.Value ) )
            {
                errorMessages.Add( "Make sure to enter a valid email address.  An email address is required for us to send you a payment confirmation." );
            }

            if ( rblSavedAccount.SelectedValue.AsInteger() == 0 )
            {
                if ( string.IsNullOrWhiteSpace( tbCardNumber.Value ) )
                {
                    errorMessages.Add( "Make sure to enter a valid credit card number." );
                }

                var currentMonth = RockDateTime.Today;
                currentMonth = new DateTime( currentMonth.Year, currentMonth.Month, 1 );

                var cardMMYY = tbCardExpiry.Value.Split( new char[] { '/' } ).Select( a => a.Trim().AsInteger() ).ToArray();
                if ( cardMMYY.Length != 2 )
                {
                    errorMessages.Add( "Make sure to enter a valid credit card expiration date." );
                }
                else
                {
                    try
                    {
                        var expDateTime = new DateTime( 2000 + cardMMYY[1], cardMMYY[0], 1 );
                        if ( expDateTime < currentMonth )
                        {
                            // let the gateway complain if it doesn't like the Expiration Date
                        }
                    }
                    catch
                    {
                        errorMessages.Add( "Invalid Expiration Date." );
                    }
                }

                if ( string.IsNullOrWhiteSpace( tbCardCvc.Value ) )
                {
                    errorMessages.Add( "Make sure to enter a valid credit card security code." );
                }
            }

            if ( errorMessages.Any() )
            {
                errorMessage = errorMessages.AsDelimited( "<br/>" );

                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the gateway.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <returns></returns>
        private FinancialGateway GetGateway( RockContext rockContext, string attributeName )
        {
            var financialGatewayService = new FinancialGatewayService( rockContext );
            Guid? ccGatewayGuid = GetAttributeValue( attributeName ).AsGuidOrNull();
            if ( ccGatewayGuid.HasValue )
            {
                return financialGatewayService.Get( ccGatewayGuid.Value );
            }

            return null;
        }

        /// <summary>
        /// Gets the gateway component.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="gateway">The gateway.</param>
        /// <returns></returns>
        private GatewayComponent GetGatewayComponent( RockContext rockContext, FinancialGateway gateway )
        {
            if ( gateway != null )
            {
                gateway.LoadAttributes( rockContext );
                var gatewayComponent = gateway.GetGatewayComponent();
                if ( gatewayComponent != null )
                {
                    var threeStepGateway = gatewayComponent as ThreeStepGatewayComponent;
                    if ( threeStepGateway != null )
                    {
                        // NOT SUPPORTED.  If CCV needs to support this, we'll have to update this block
                        return null;
                    }
                }

                return gatewayComponent;
            }

            return null;
        }

        #endregion

        #region Exact Copy/Pasted code from TransactionEntry.ascx.cs
        // Except where noted, the code in this region is nearly an exact copy/paste from Core TransactionEntry.ascx.cs

        /// <summary>
        /// Saves the transaction.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="gateway">The gateway.</param>
        /// <param name="person">The person.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SaveTransaction( FinancialGateway financialGateway, GatewayComponent gateway, Person person, PaymentInfo paymentInfo, FinancialTransaction transaction, RockContext rockContext )
        {
            var txnChanges = new List<string>();
            txnChanges.Add( "Created Transaction" );

            History.EvaluateChange( txnChanges, "Transaction Code", string.Empty, transaction.TransactionCode );

            transaction.AuthorizedPersonAliasId = person.PrimaryAliasId;
            History.EvaluateChange( txnChanges, "Person", string.Empty, person.FullName );

            transaction.TransactionDateTime = RockDateTime.Now;
            History.EvaluateChange( txnChanges, "Date/Time", null, transaction.TransactionDateTime );

            transaction.FinancialGatewayId = financialGateway.Id;
            History.EvaluateChange( txnChanges, "Gateway", string.Empty, financialGateway.Name );

            var txnType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION ) );
            transaction.TransactionTypeValueId = txnType.Id;
            History.EvaluateChange( txnChanges, "Type", string.Empty, txnType.Value );

            transaction.Summary = paymentInfo.Comment1;
            History.EvaluateChange( txnChanges, "Summary", string.Empty, transaction.Summary );

            if ( transaction.FinancialPaymentDetail == null )
            {
                transaction.FinancialPaymentDetail = new FinancialPaymentDetail();
            }

            transaction.FinancialPaymentDetail.SetFromPaymentInfo( paymentInfo, gateway, rockContext, txnChanges );

            Guid sourceGuid = Guid.Empty;
            if ( Guid.TryParse( GetAttributeValue( "Source" ), out sourceGuid ) )
            {
                var source = DefinedValueCache.Read( sourceGuid );
                if ( source != null )
                {
                    transaction.SourceTypeValueId = source.Id;
                    History.EvaluateChange( txnChanges, "Source", string.Empty, source.Value );
                }
            }

            // This chunk of Account logic is specific to TransactionEntry.ccv
            var accountId = ddlAccounts.SelectedValue.AsInteger();
            var account = new FinancialAccountService( rockContext ).Get( accountId );

            var transactionDetail = new FinancialTransactionDetail();
            transactionDetail.Amount = paymentInfo.Amount;
            transactionDetail.AccountId = account.Id;
            transaction.TransactionDetails.Add( transactionDetail );
            History.EvaluateChange( txnChanges, account.Name, 0.0M.FormatAsCurrency(), transactionDetail.Amount.FormatAsCurrency() );

            var batchService = new FinancialBatchService( rockContext );

            // Get the batch
            var batch = batchService.Get(
                GetAttributeValue( "BatchNamePrefix" ),
                paymentInfo.CurrencyTypeValue,
                paymentInfo.CreditCardTypeValue,
                transaction.TransactionDateTime.Value,
                financialGateway.GetBatchTimeOffset() );

            var batchChanges = new List<string>();

            if ( batch.Id == 0 )
            {
                batchChanges.Add( "Generated the batch" );
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

            HistoryService.SaveChanges(
                rockContext,
                typeof( FinancialBatch ),
                Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                batch.Id,
                batchChanges );

            HistoryService.SaveChanges(
                rockContext,
                typeof( FinancialBatch ),
                Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(),
                batch.Id,
                txnChanges,
                person.FullName,
                typeof( FinancialTransaction ),
                transaction.Id );

            SendReceipt( transaction.Id );
        }

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <param name="create">if set to <c>true</c> [create].</param>
        /// <returns></returns>
        private Person GetPerson( bool create )
        {
            Person person = null;
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );

            Group familyGroup = null;

            int personId = ViewState["PersonId"] as int? ?? 0;
            if ( personId == 0 && this.CurrentPerson != null )
            {
                personId = this.CurrentPerson.Id;
            }

            if ( personId != 0 )
            {
                person = personService.Get( personId );
            }

            if ( create )
            {
                if ( person == null )
                {
                    // Check to see if there's only one person with same email, first name, and last name
                    if ( !string.IsNullOrWhiteSpace( tbEmail.Value ) &&
                        !string.IsNullOrWhiteSpace( tbFirstName.Value ) &&
                        !string.IsNullOrWhiteSpace( tbLastName.Value ) )
                    {
                        // Same logic as CreatePledge.ascx.cs
                        var personMatches = personService.GetByMatch( tbFirstName.Value.Trim(), tbLastName.Value.Trim(), tbEmail.Value.Trim() );
                        if ( personMatches.Count() == 1 )
                        {
                            person = personMatches.FirstOrDefault();
                        }
                        else
                        {
                            person = null;
                        }
                    }

                    if ( person == null )
                    {
                        DefinedValueCache dvcConnectionStatus = DefinedValueCache.Read( GetAttributeValue( "ConnectionStatus" ).AsGuid() );
                        DefinedValueCache dvcRecordStatus = DefinedValueCache.Read( GetAttributeValue( "RecordStatus" ).AsGuid() );

                        // Create Person
                        person = new Person();
                        person.FirstName = tbFirstName.Value.Trim();
                        person.LastName = tbLastName.Value.Trim();
                        person.IsEmailActive = true;
                        person.EmailPreference = EmailPreference.EmailAllowed;
                        person.RecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                        if ( dvcConnectionStatus != null )
                        {
                            person.ConnectionStatusValueId = dvcConnectionStatus.Id;
                        }

                        if ( dvcRecordStatus != null )
                        {
                            person.RecordStatusValueId = dvcRecordStatus.Id;
                        }

                        // Create Person/Family
                        familyGroup = PersonService.SaveNewPerson( person, rockContext, null, false );
                    }

                    ViewState["PersonId"] = person != null ? person.Id : 0;
                }
            }

            // person should never be null at this point
            if ( create && person != null )
            {
                person.Email = tbEmail.Value.Trim();

                string phoneNumberValue = tbPhone.Value.Trim();
                if ( !string.IsNullOrEmpty( phoneNumberValue ) )
                {
                    var numberTypeId = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME ) ).Id;

                    var phone = person.PhoneNumbers.FirstOrDefault( p => p.NumberTypeValueId == numberTypeId );
                    if ( phone == null )
                    {
                        phone = new PhoneNumber();
                        person.PhoneNumbers.Add( phone );
                        phone.NumberTypeValueId = numberTypeId;
                    }

                    phone.CountryCode = PhoneNumber.CleanNumber( PhoneNumber.DefaultCountryCode() );
                    phone.Number = PhoneNumber.CleanNumber( phoneNumberValue );
                }

                if ( familyGroup == null )
                {
                    var groupLocationService = new GroupLocationService( rockContext );
                    var groupLocationValue = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );
                    if ( groupLocationValue != null )
                    {
                        familyGroup = groupLocationService.Queryable()
                            .Where( gl => gl.Id == groupLocationValue.Id )
                            .Select( gl => gl.Group )
                            .FirstOrDefault();
                    }
                    else
                    {
                        familyGroup = personService.GetFamilies( person.Id ).FirstOrDefault();
                    }
                }

                rockContext.SaveChanges();

                if ( familyGroup != null )
                {
                    GroupService.AddNewGroupAddress(
                        rockContext,
                        familyGroup,
                        GetAttributeValue( "AddressType" ),
                        tbStreet.Value.Trim(),
                        null,
                        tbCity.Value.Trim(),
                        tbState.Value.Trim(),
                        tbZip.Value.Trim(),
                        GlobalAttributesCache.Read().OrganizationCountry,
                        true );
                }
            }

            return person;
        }

        /// <summary>
        /// Saves the scheduled transaction.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="gateway">The gateway.</param>
        /// <param name="person">The person.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="schedule">The schedule.</param>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SaveScheduledTransaction( FinancialGateway financialGateway, GatewayComponent gateway, Person person, PaymentInfo paymentInfo, PaymentSchedule schedule, FinancialScheduledTransaction scheduledTransaction, RockContext rockContext )
        {
            scheduledTransaction.TransactionFrequencyValueId = schedule.TransactionFrequencyValue.Id;
            scheduledTransaction.StartDate = schedule.StartDate;
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
                var source = DefinedValueCache.Read( sourceGuid );
                if ( source != null )
                {
                    scheduledTransaction.SourceTypeValueId = source.Id;
                }
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

            var accountId = ddlAccounts.SelectedValue.AsInteger();

            var account = new FinancialAccountService( rockContext ).Get( accountId );

            var transactionDetail = new FinancialScheduledTransactionDetail();
            transactionDetail.Amount = paymentInfo.Amount;
            transactionDetail.AccountId = account.Id;
            scheduledTransaction.ScheduledTransactionDetails.Add( transactionDetail );
            changeSummary.AppendFormat( "{0}: {1}", account.Name, paymentInfo.Amount.FormatAsCurrency() );
            changeSummary.AppendLine();

            if ( !string.IsNullOrWhiteSpace( paymentInfo.Comment1 ) )
            {
                changeSummary.Append( paymentInfo.Comment1 );
                changeSummary.AppendLine();
            }

            var transactionService = new FinancialScheduledTransactionService( rockContext );
            transactionService.Add( scheduledTransaction );
            rockContext.SaveChanges();

            // Add a note about the change
            var noteType = NoteTypeCache.Read( Rock.SystemGuid.NoteType.SCHEDULED_TRANSACTION_NOTE.AsGuid() );
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
        }

        /// <summary>
        /// Sends the receipt.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        private void SendReceipt( int transactionId )
        {
            Guid? recieptEmail = GetAttributeValue( "ReceiptEmail" ).AsGuidOrNull();
            if ( recieptEmail.HasValue )
            {
                // Queue a transaction to send reciepts
                var newTransactionIds = new List<int> { transactionId };
                var sendPaymentRecieptsTxn = new Rock.Transactions.SendPaymentReciepts( recieptEmail.Value, newTransactionIds );
                Rock.Transactions.RockQueue.TransactionQueue.Enqueue( sendPaymentRecieptsTxn );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbSaveAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSaveAccount_Click( object sender, EventArgs e )
        {
            upPayment.Update();
            var transactionCode = hfSaveAccountTransactionCode.Value;
            var scheduleId = hfSaveAccountScheduleId.Value;

            if ( string.IsNullOrWhiteSpace( transactionCode ) )
            {
                nbSaveAccount.Text = "Sorry, the account information cannot be saved as there's not a valid transaction code to reference";
                nbSaveAccount.Visible = true;
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                if ( phCreateLogin.Visible )
                {
                    if ( string.IsNullOrWhiteSpace( txtUserName.Text ) || string.IsNullOrWhiteSpace( txtPassword.Text ) )
                    {
                        nbSaveAccount.Title = "Missing Informaton";
                        nbSaveAccount.Text = "A username and password are required when saving an account";
                        nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                        nbSaveAccount.Visible = true;
                        return;
                    }

                    if ( new UserLoginService( rockContext ).GetByUserName( txtUserName.Text ) != null )
                    {
                        nbSaveAccount.Title = "Invalid Username";
                        nbSaveAccount.Text = "The selected Username is already being used.  Please select a different Username";
                        nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                        nbSaveAccount.Visible = true;
                        return;
                    }

                    if ( !UserLoginService.IsPasswordValid( txtPassword.Text ) )
                    {
                        nbSaveAccount.Title = string.Empty;
                        nbSaveAccount.Text = UserLoginService.FriendlyPasswordRules();
                        nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                        nbSaveAccount.Visible = true;
                        return;
                    }

                    if ( txtPasswordConfirm.Text != txtPassword.Text )
                    {
                        nbSaveAccount.Title = "Invalid Password";
                        nbSaveAccount.Text = "The password and password confirmation do not match";
                        nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                        nbSaveAccount.Visible = true;
                        return;
                    }
                }

                if ( !string.IsNullOrWhiteSpace( txtSaveAccount.Text ) )
                {
                    var financialGateway = GetGateway( rockContext, "CCGateway" );
                    var gateway = GetGatewayComponent( rockContext, financialGateway );

                    if ( gateway != null )
                    {
                        var ccCurrencyType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD ) );

                        string errorMessage = string.Empty;

                        PersonAlias authorizedPersonAlias = null;
                        string referenceNumber = string.Empty;
                        FinancialPaymentDetail paymentDetail = null;
                        int? currencyTypeValueId = ccCurrencyType.Id;

                        if ( string.IsNullOrWhiteSpace( scheduleId ) )
                        {
                            var transaction = new FinancialTransactionService( rockContext ).GetByTransactionCode( transactionCode );
                            if ( transaction != null && transaction.AuthorizedPersonAlias != null )
                            {
                                authorizedPersonAlias = transaction.AuthorizedPersonAlias;
                                if ( transaction.FinancialGateway != null )
                                {
                                    transaction.FinancialGateway.LoadAttributes( rockContext );
                                }

                                referenceNumber = gateway.GetReferenceNumber( transaction, out errorMessage );
                                paymentDetail = transaction.FinancialPaymentDetail;
                            }
                        }
                        else
                        {
                            var scheduledTransaction = new FinancialScheduledTransactionService( rockContext ).GetByScheduleId( scheduleId );
                            if ( scheduledTransaction != null )
                            {
                                authorizedPersonAlias = scheduledTransaction.AuthorizedPersonAlias;
                                if ( scheduledTransaction.FinancialGateway != null )
                                {
                                    scheduledTransaction.FinancialGateway.LoadAttributes( rockContext );
                                }

                                referenceNumber = gateway.GetReferenceNumber( scheduledTransaction, out errorMessage );
                                paymentDetail = scheduledTransaction.FinancialPaymentDetail;
                            }
                        }

                        if ( authorizedPersonAlias != null && authorizedPersonAlias.Person != null && paymentDetail != null )
                        {
                            if ( phCreateLogin.Visible )
                            {
                                var user = UserLoginService.Create(
                                    rockContext,
                                    authorizedPersonAlias.Person,
                                    Rock.Model.AuthenticationServiceType.Internal,
                                    EntityTypeCache.Read( Rock.SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id,
                                    txtUserName.Text,
                                    txtPassword.Text,
                                    false );

                                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                                mergeFields.Add( "ConfirmAccountUrl", RootPath + "ConfirmAccount" );

                                var personDictionary = authorizedPersonAlias.Person.ToLiquid() as Dictionary<string, object>;
                                mergeFields.Add( "Person", personDictionary );

                                mergeFields.Add( "User", user );

                                var recipients = new List<Rock.Communication.RecipientData>();
                                recipients.Add( new Rock.Communication.RecipientData( authorizedPersonAlias.Person.Email, mergeFields ) );

                                Rock.Communication.Email.Send( GetAttributeValue( "ConfirmAccountTemplate" ).AsGuid(), recipients, ResolveRockUrl( "~/" ), ResolveRockUrl( "~~/" ), false );
                            }

                            if ( errorMessage.Any() )
                            {
                                nbSaveAccount.Title = "Invalid Transaction";
                                nbSaveAccount.Text = "Sorry, the account information cannot be saved. " + errorMessage;
                                nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                                nbSaveAccount.Visible = true;
                            }
                            else
                            {
                                if ( authorizedPersonAlias != null )
                                {
                                    var savedAccount = new FinancialPersonSavedAccount();
                                    savedAccount.PersonAliasId = authorizedPersonAlias.Id;
                                    savedAccount.ReferenceNumber = referenceNumber;
                                    savedAccount.Name = txtSaveAccount.Text;
                                    savedAccount.TransactionCode = transactionCode;
                                    savedAccount.FinancialGatewayId = financialGateway.Id;
                                    savedAccount.FinancialPaymentDetail = new FinancialPaymentDetail();
                                    savedAccount.FinancialPaymentDetail.AccountNumberMasked = paymentDetail.AccountNumberMasked;
                                    savedAccount.FinancialPaymentDetail.CurrencyTypeValueId = paymentDetail.CurrencyTypeValueId;
                                    savedAccount.FinancialPaymentDetail.CreditCardTypeValueId = paymentDetail.CreditCardTypeValueId;
                                    savedAccount.FinancialPaymentDetail.NameOnCardEncrypted = paymentDetail.NameOnCardEncrypted;
                                    savedAccount.FinancialPaymentDetail.ExpirationMonthEncrypted = paymentDetail.ExpirationMonthEncrypted;
                                    savedAccount.FinancialPaymentDetail.ExpirationYearEncrypted = paymentDetail.ExpirationYearEncrypted;
                                    savedAccount.FinancialPaymentDetail.BillingLocationId = paymentDetail.BillingLocationId;

                                    var savedAccountService = new FinancialPersonSavedAccountService( rockContext );
                                    savedAccountService.Add( savedAccount );
                                    rockContext.SaveChanges();

                                    cbSaveAccount.Visible = false;
                                    txtSaveAccount.Visible = false;
                                    phCreateLogin.Visible = false;
                                    divSaveActions.Visible = false;

                                    nbSaveAccount.Title = "Success";
                                    nbSaveAccount.Text = "The account has been saved for future use";
                                    nbSaveAccount.NotificationBoxType = NotificationBoxType.Success;
                                    nbSaveAccount.Visible = true;
                                }
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

        #endregion
    }
}
