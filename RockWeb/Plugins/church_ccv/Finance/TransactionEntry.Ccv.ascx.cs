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
using Rock.Security;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Communication;

namespace RockWeb.Plugins.church_ccv.Finance
{
    [DisplayName( "Transaction Entry (CCV)" )]
    [Category( "CCV > Finance" )]
    [Description( "Custom CCV block that provides an optimized experience for giving" )]

    [FinancialGatewayField( "Credit Card Gateway", "The payment gateway to use for Credit Card transactions", false, "", "", 0, "CCGateway" )]
    [TextField( "Batch Name Prefix", "The batch prefix name to use when creating a new batch", false, "Online Giving", "", 2 )]
    [AccountsField( "Accounts", "The accounts to display. By default all active accounts with a Public Name will be displayed", false, "", "", 6 )]

    [DefinedValueField( "2E6540EA-63F0-40FE-BE50-F2A84735E600", "Connection Status", "The connection status to use for new individuals (default: 'Web Prospect'.)", true, false, "368DD475-242C-49C4-A42C-7278BE690CC2", "", 25 )]
    [DefinedValueField( "8522BADD-2871-45A5-81DD-C76DA07E2E7E", "Record Status", "The record status to use for new individuals (default: 'Pending'.)", true, false, "283999EC-7346-42E3-B807-BCE9B2BABB49", "", 26 )]
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
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
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
                }
             );

            // these are needed for the Location Detector and FundSetter js to work
            this.GivingFundsJSON = accounts.Select( a => new { value = a.AccountId, text = a.Name } ).ToJson();
            this.CampusFundLocationsJSON = campusFundLocations.ToJson();

            ddlAccounts.Items.Clear();
            foreach ( var account in accounts )
            {
                ddlAccounts.Items.Add( new ListItem( account.Name, account.AccountId.ToString() ) );
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
            var errorMessage = string.Empty;
            if ( !ValidateEntries( out errorMessage ) )
            {
                nbMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbMessage.Title = "Before we finish...";
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

            // TODO Something to prevent duplicate transactions

            
            // set up the CreditCard charge
            var cardMMYY = tbCardExpiry.Value.Split( new char[] { '/' } ).Select( a => a.Trim().AsInteger() ).ToArray();
            var expDateTime = new DateTime( 2000 + cardMMYY[1], cardMMYY[0], 1 );

            // TODO: use Saved Account if selected

            CreditCardPaymentInfo cc = new CreditCardPaymentInfo( tbCardNumber.Value.Replace(" ", ""), tbCardCvc.Value, expDateTime );
            cc.NameOnCard = gateway.SplitNameOnCard ? tbFirstName.Value : hfFullName.Value;
            cc.LastName = tbLastName.Value;

            if ( !string.IsNullOrEmpty( tbStreet.Value ) )
            {
                cc.BillingStreet1 = tbStreet.Value;
                cc.BillingStreet2 = string.Empty;
                cc.BillingCity = tbCity.Value;
                cc.BillingState = tbState.Value;
                cc.BillingPostalCode = tbZip.Value;
                cc.BillingCountry = GlobalAttributesCache.Read().OrganizationCountry;

                cc.Street1 = tbStreet.Value;
                cc.Street2 = string.Empty;
                cc.City = tbCity.Value;
                cc.State = tbState.Value;
                cc.PostalCode = tbZip.Value;
                cc.Country = GlobalAttributesCache.Read().OrganizationCountry;
            }

            cc.Amount = tbAmount.Text.AsDecimal();
            cc.Email = tbEmail.Value;
            cc.Phone = PhoneNumber.FormattedNumber( PhoneNumber.DefaultCountryCode(), tbPhone.Value, true );
            

            // TODO Setup a Scheduled Payment

            // Charge CC
            var transaction = gateway.Charge( financialGateway, cc, out errorMessage );
            if (transaction == null)
            {
                nbMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbMessage.Title = "Payment Error";
                nbMessage.Text = errorMessage;
                nbMessage.Visible = true;
                return;
            }
            else
            {
                SaveTransaction( financialGateway, gateway, person, cc, transaction, rockContext );
                // show success page
            }

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
            if ( amount <= 0 )
            {
                errorMessages.Add( "Make sure you've entered an amount." );
            }

            // Validate that no negative amounts were entered
            if ( amount < 0 )
            {
                errorMessages.Add( "Make sure the amount you've entered is a positive amount" );
            }

            // validate the payment schedule
            if ( cbRepeating.Checked )
            {
                var startDate = dpStartDate.Value.AsDateTime();

                if ( startDate == null )
                {
                    errorMessages.Add( "When scheduling a repeating payment, make sure the First Gift date is specified" );
                }

                // Make sure a repeating payment starts in the future
                if ( startDate <= RockDateTime.Today )
                {
                    errorMessages.Add( "When scheduling a repeating payment, make sure the First Gift date is in the future (after today)" );
                }
            }

            if ( string.IsNullOrWhiteSpace( tbFirstName.Value ) || string.IsNullOrWhiteSpace( tbLastName.Value ) )
            {
                errorMessages.Add( "Make sure to enter both a first and last name" );
            }

            if ( string.IsNullOrWhiteSpace( tbEmail.Value ) )
            {
                errorMessages.Add( "Make sure to enter a valid email address.  An email address is required for us to send you a payment confirmation" );
            }

            if ( string.IsNullOrWhiteSpace( tbCardNumber.Value ) )
            {
                errorMessages.Add( "Make sure to enter a valid credit card number" );
            }

            var currentMonth = RockDateTime.Today;
            currentMonth = new DateTime( currentMonth.Year, currentMonth.Month, 1 );

            var cardMMYY = tbCardExpiry.Value.Split( new char[] { '/' } ).Select( a => a.Trim().AsInteger() ).ToArray();
            if ( cardMMYY.Length != 2 )
            {
                errorMessages.Add( "Make sure to enter a valid credit card expiration date" );
            }
            else
            {
                var expDateTime = new DateTime( 2000 + cardMMYY[1], cardMMYY[0], 1 );
                if ( expDateTime < currentMonth )
                {
                    //errorMessages.Add( "The Credit card expiration date is expired" );
                }
            }

            if ( string.IsNullOrWhiteSpace( tbCardCvc.Value ) )
            {
                errorMessages.Add( "Make sure to enter a valid credit card security code" );
            }

            if ( errorMessages.Any() )
            {
                errorMessage = errorMessages.AsDelimited( "<br/>" );

                return false;
            }

            return true;
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
                        var personMatches = personService.GetByMatch( tbFirstName.Value, tbLastName.Value, tbEmail.Value );
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
                        person.FirstName = tbFirstName.Value;
                        person.LastName = tbLastName.Value;
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

            if ( create && person != null ) // person should never be null at this point
            {
                person.Email = tbEmail.Value;

                if ( GetAttributeValue( "DisplayPhone" ).AsBooleanOrNull() ?? false )
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
                    phone.Number = PhoneNumber.CleanNumber( tbPhone.Value );
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
                        tbStreet.Value, null, tbCity.Value, tbState.Value, tbZip.Value, GlobalAttributesCache.Read().OrganizationCountry,
                        true );
                }
            }

            return person;
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
                batchChanges
            );

            HistoryService.SaveChanges(
                rockContext,
                typeof( FinancialBatch ),
                Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(),
                batch.Id,
                txnChanges,
                person.FullName,
                typeof( FinancialTransaction ),
                transaction.Id
            );

            //SendReceipt( transaction.Id );
        }

        #endregion
    }
}
