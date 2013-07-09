//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Collections.Generic;

using DotLiquid;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Front end block for giving: gift detail, user detail, and payment detail
    /// </summary>    
    [Description("Giving profile details UI")]
    [LinkedPage( "New Accounts", "What page should users redirect to when creating a new account?", true, "7D4E2142-D24E-4DD2-84BC-B34C5C3D0D46", "Financial")]    
    [CustomCheckboxListField( "Credit Card Provider", "Which payment processor should be used for credit cards?",
        "SELECT [Name] AS [Text], [Id] AS [Value] FROM [FinancialGateway]", true, "", "Payment Options", 0 )]
    [CustomCheckboxListField( "Checking/ACH Provider", "Which payment processor should be used for checking/ACH?",
        "SELECT [Name] AS [Text], [Id] AS [Value] FROM [FinancialGateway]", true, "", "Payment Options", 1 )]
    [AccountsField("Default Accounts", "Which accounts should be displayed by default?", true, "", "Payment Options", 2)]
    [BooleanField( "Show Vertical Layout", "Should the giving page display vertically or horizontally?", true, "UI Options", 0 )]
    [BooleanField( "Show Campuses", "Should giving be associated with a specific campus?", false, "UI Options", 1 )]
    [BooleanField( "Show Additional Accounts", "Should users be allowed to give to additional accounts?", true, "UI Options", 2 )]
    [BooleanField( "Show Credit Card", "Allow users to give using a credit card?", true, "UI Options", 3 )]
    [BooleanField( "Show Checking/ACH", "Allow users to give using a checking account?", true, "UI Options", 4 )]
    [BooleanField( "Show Frequencies", "Allow users to give recurring gifts?", true, "UI Options", 5 )]
    [BooleanField( "Show State Name", "Should the address state show the full name (Arizona) or the abbreviation (AZ)?", true, "UI Options", 6 )]
    [BooleanField( "Require Phone", "Should financial contributions require a user's phone number?", true, "UI Options", 7 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.LOCATION_ADDRESS_STATE, "Default State", "Which state should be selected by default?", false, "", "UI Options", 8)]    
    [MemoField( "Confirmation Message", "What text should be displayed on the confirmation page?", true,  @"{{ ContributionConfirmationHeader }}<br/><br/>{{ Person.FullName }},<br/><br/>
        You are about to give a total of <strong>{{ TotalContribution }}</strong> using your {{ PaymentType }} ending in {{ PaymentLastFour }}.<br/><br/>
        If this is correct, please press Give.  Otherwise, click Back to edit.<br/>Thank you,<br/><br/>{{ OrganizationName }}<br/>{{ ContributionConfirmFooter }}", "Message Options", 0)]
    [MemoField( "Receipt Message", "What text should be displayed on the receipt page?", true, @"{{ ContributionReceiptHeader }}<br/>{{ Person.FullName }},<br/><br/>
        Thank you for your generosity! You just gave a total of {{ TotalContribution }} to {{ OrganizationName }}.<br/><br/>{{ ContributionReceiptFooter }}", "Message Options", 1 )]
    [MemoField( "Summary Message", "What text should be displayed on the transaction summary?", true, @"{{ Date }}: {{ TotalContribution }} given by {{ Person.FullName }} using a 
        {{ PaymentType }} ending in {{ PaymentLastFour }}.", "Message Options", 2 )]
    public partial class GivingProfileDetail : RockBlock
    {
        #region Fields

        protected string _spanClass;
        
        /// <summary>
        /// Gets or sets the current tab.
        /// </summary>
        /// <value>
        /// The current tab.
        /// </value>
        protected string CurrentTab
        {
            get
            {
                object currentTab = Session["CurrentTab"];
                return currentTab != null ? currentTab.ToString() : "Credit Card";
            }

            set
            {
                Session["CurrentTab"] = value;
            }
        }

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !IsPostBack )
            {
                // Change Layout vertically or horizontally
                if ( Convert.ToBoolean( GetAttributeValue( "ShowVerticalLayout" ) ) )
                {
                    _spanClass = "span9 offset2";

                    divCity.AddCssClass( "span7" );
                    divState.AddCssClass( "span3" );
                    divZip.AddCssClass( "span2" );
                    divPayment.AddCssClass( "form-horizontal" );
                    divCardNumber.AddCssClass( "span7" );
                    divCardType.AddCssClass( "span5" );
                    divCheckDetail.AddCssClass( "span7" );
                    divCheckImage.AddCssClass( "span5" );
                    divDefaultAddress.AddCssClass( "align-middle" );
                    divNewCity.AddCssClass( "span7" );
                    divNewState.AddCssClass( "span3" );
                    divNewZip.AddCssClass( "span2" );
                }
                else
                {
                    _spanClass = "span6";

                    divCity.AddCssClass( "span5" );
                    divState.AddCssClass( "span5" );
                    divZip.AddCssClass( "span2" );
                    divCardNumber.AddCssClass( "span6" );
                    divCardType.AddCssClass( "span6 label-padding" );
                    divExpiration.AddCssClass( "span6" );
                    divCVV.AddCssClass( "span6" );
                    divCheckDetail.AddCssClass( "span6" );
                    divCheckImage.AddCssClass( "span6" );
                    divNewCity.AddCssClass( "span5" );
                    divNewState.AddCssClass( "span5" );
                    divNewZip.AddCssClass( "span2" );
                }

                divDetails.AddCssClass( _spanClass );
                divAddress.AddCssClass( _spanClass );
                divPayment.AddCssClass( _spanClass );
                divNext.AddCssClass( _spanClass );
                divConfirm.AddCssClass( _spanClass );
                pnlComplete.AddCssClass( _spanClass );
                divGiveBack.AddCssClass( _spanClass );

                chkLimitGifts.InputAttributes.Add( "class", "toggle-input" );
                chkNewAddress.InputAttributes.Add( "class", "toggle-input" );
                chkSavePayment.InputAttributes.Add( "class", "toggle-input" );
                chkCreateAccount.InputAttributes.Add( "class", "toggle-input" );
                
                // Show Campus
                if ( Convert.ToBoolean( GetAttributeValue( "ShowCampuses" ) ) )
                {
                    BindCampuses();                    
                }
                else
                {
                    divCampus.Visible = false;
                }
                                
                // Show Frequencies
                if ( Convert.ToBoolean( GetAttributeValue( "ShowFrequencies" ) ) )
                {
                    BindFrequencies();
                    divFrequency.Visible = true;
                }

                // Show Payment types
                bool showCredit = Convert.ToBoolean( GetAttributeValue( "ShowCreditCard" ) );
                bool showChecking = Convert.ToBoolean( GetAttributeValue( "ShowChecking/ACH" ) );
                BindPaymentTypes( showCredit, showChecking );

                // Customize State picker
                if ( !Convert.ToBoolean( GetAttributeValue( "ShowStateName" ) ) )
                {
                    ddlState.UseAbbreviation = true;
                }

                var defaultState = GetAttributeValue( "DefaultState" );
                if ( !string.IsNullOrWhiteSpace( defaultState ) )
                {
                    Guid stateGuid = Guid.Empty;
                    if (Guid.TryParse(defaultState, out stateGuid))
                    {
                        var definedValueState = DefinedValueCache.Read( stateGuid );
                        if ( definedValueState != null )
                        {
                            ddlState.SelectedValue = definedValueState.Name;
                        }
                    }
                }

                // Require Phone
                if ( Convert.ToBoolean( GetAttributeValue( "RequirePhone" ) ) )
                {
                    txtPhone.Required = true;
                }  
                                
                // Load Profile
                string profileId = PageParameter( "GivingProfileId" );
                if ( !string.IsNullOrWhiteSpace( profileId ) )
                {
                    BindGivingProfile( Convert.ToInt32( profileId ) );
                }
                else
                {
                    BindGivingProfile( 0 );
                }

                if ( CurrentPerson != null )
                {
                    BindPersonDetails();
                    BindSavedPayments();
                    pnlSavePayment.Visible = true;
                    pnlCreateAccount.Visible = false;
                }

                ShowSelectedTab();
            }

            // postback layout changes
            if ( _spanClass != "span6" )
            {
                txtCity.LabelText = "City, State, Zip";
                txtNewCity.LabelText = "City, State, Zip";
                ddlState.LabelText = string.Empty;
                ddlNewState.LabelText = string.Empty;
                txtZip.LabelText = string.Empty;
                txtNewZip.LabelText = string.Empty;                                
            }            
        }
       
        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnAddAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddAccount_SelectionChanged( object sender, EventArgs e )
        {
            SaveAmounts();
            var amountList = (Dictionary<FinancialAccount, Decimal>) Session["CachedAmounts"];
            var accountService = new FinancialAccountService();

            FinancialAccount account = accountService.Get( (int)btnAddAccount.SelectedValueAsInt() );
            amountList.Add( account, 0M );

            if ( btnAddAccount.Items.Count > 1 )
            {
                btnAddAccount.Items.Remove( btnAddAccount.SelectedItem );
                btnAddAccount.Title = "Add Another Gift";
            }
            else
            {
                btnAddAccount.Visible = false;
                divAddAccount.Visible = false;
            }
                        
            RebindAmounts( amountList );
        }

        /// <summary>
        /// Handles the SelectionChanged event of the btnFrequency control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnFrequency_SelectionChanged( object sender, EventArgs e )
        {
            SaveAmounts();
            
            if ( btnFrequency.SelectedValueAsInt() != DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_TYPE_ONE_TIME_FUTURE ).Id
                && btnFrequency.SelectedValueAsInt() != DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_TYPE_ONE_TIME ).Id )
            {
                if ( divRecurrence.Visible != true )
                {
                    divRecurrence.Visible = true;
                    dtpStartDate.Required = true;
                }

                if ( divLimitGifts.Visible != true )
                {
                    divLimitGifts.Visible = true;
                }
            }
            else if ( btnFrequency.SelectedValueAsInt() == DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_TYPE_ONE_TIME_FUTURE ).Id )
            {
                if ( divRecurrence.Visible != true )
                {
                    divRecurrence.Visible = true;
                    dtpStartDate.Required = true;
                }

                if ( divLimitGifts.Visible != false )
                {
                    divLimitGifts.Visible = false;
                }                
            }
            else
            {
                if ( divRecurrence.Visible != false )
                {
                    divRecurrence.Visible = false;
                    dtpStartDate.Required = false;
                }

                if ( divLimitGifts.Visible != false )
                {
                    divLimitGifts.Visible = false;
                }
            }

            RebindAmounts();
        }
        
        /// <summary>
        /// Handles the Click1 event of the lbPaymentType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbPaymentType_Click( object sender, EventArgs e )
        {
            //SaveAmounts();
            LinkButton lb = sender as LinkButton;
            if ( lb != null )
            {
                foreach ( RepeaterItem item in rptPaymentType.Items )
                {
                    ( (HtmlGenericControl)item.FindControl( "liSelectedTab" ) ).RemoveCssClass( "active" );
                }

                CurrentTab = lb.Text;
                ( (HtmlGenericControl)lb.Parent ).AddCssClass( "active" );
            }

            //RebindAmounts();
            ShowSelectedTab();
        }

        /// <summary>
        /// Handles the Click event of the btnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnNext_Click( object sender, EventArgs e )
        {
            Person person = FindPerson();
            SaveAmounts();            

            var amountList = (Dictionary<FinancialAccount, Decimal>)Session["CachedAmounts"];
            var configValues = new Dictionary<string,object>();            
            Rock.Web.Cache.GlobalAttributesCache.Read().AttributeValues
                .Where( v => 
                    v.Key.StartsWith( "Organization", StringComparison.CurrentCultureIgnoreCase ) || 
                    v.Key.StartsWith( "Contribution", StringComparison.CurrentCultureIgnoreCase ) )
                .ToList()
                .ForEach( v => configValues.Add( v.Key, v.Value.Value ) );
            
            if ( !string.IsNullOrEmpty( txtCreditCard.Text ) )
            {
                string visa = "^4[0-9]{12}(?:[0-9]{3})?$";
                string mastercard = "^5[1-5][0-9]{14}$";
                string amex = "^3[47][0-9]{13}$";
                string discover = "^6(?:011|5[0-9]{2})[0-9]{12}$";
                                
                if ( System.Text.RegularExpressions.Regex.IsMatch( txtCreditCard.Text, visa ) )
                {
                    configValues.Add( "PaymentType", "Visa" );
                }
                else if ( System.Text.RegularExpressions.Regex.IsMatch( txtCreditCard.Text, mastercard ) )
                {
                    configValues.Add( "PaymentType", "MasterCard" );
                }
                else if ( System.Text.RegularExpressions.Regex.IsMatch( txtCreditCard.Text, amex ) )
                {
                    configValues.Add( "PaymentType", "American Express" );
                }
                else if ( System.Text.RegularExpressions.Regex.IsMatch( txtCreditCard.Text, discover ) )
                {
                    configValues.Add( "PaymentType", "Discover" );
                }
                else
                {
                    configValues.Add( "PaymentType", "credit card" );
                }

                configValues.Add( "PaymentLastFour", txtCreditCard.Text.Substring( txtCreditCard.Text.Length - 4, 4 ) );
            }
            else if ( !string.IsNullOrEmpty( txtAccountNumber.Text ) )
            {
                configValues.Add( "PaymentType", rblAccountType.SelectedValue + " account" );
                configValues.Add( "PaymentLastFour", txtAccountNumber.Text.Substring( txtAccountNumber.Text.Length - 4, 4 ) );
            }
            
            configValues.Add( "Person", person );
            configValues.Add( "TotalContribution", amountList.Sum( td => td.Value ).ToString() );
            configValues.Add( "Transactions", amountList.Where( td => td.Value > 0 ).ToArray() );
            
            var confirmationTemplate = GetAttributeValue( "ConfirmationMessage" );
            lPaymentConfirmation.Text = confirmationTemplate.ResolveMergeFields( configValues );
            Session["CachedMergeFields"] = configValues;

            pnlDetailsAndAddress.Visible = false;
            pnlPayment.Visible = false;
            pnlConfirm.Visible = true;
            pnlContribution.Update();
        }

        /// <summary>
        /// Handles the Click event of the btnBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnBack_Click( object sender, EventArgs e )
        {
            pnlConfirm.Visible = false;
            RebindAmounts();
            pnlDetailsAndAddress.Visible = true;
            pnlPayment.Visible = true;
            pnlContribution.Update();
        }

        /// <summary>
        /// Handles the Click event of the btnGive control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnGive_Click( object sender, EventArgs e )
        {
            Person person = FindPerson();

            using ( new UnitOfWorkScope() )
            {
                RockTransactionScope.WrapTransaction( () =>
                {
                    var groupLocationService = new GroupLocationService();
                    var groupMemberService = new GroupMemberService();
                    var phoneService = new PhoneNumberService();
                    var locationService = new LocationService();
                    var groupService = new GroupService();
                    GroupLocation groupLocation;
                    Location homeAddress;
                    Group familyGroup;

                    var homeLocationType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.LOCATION_TYPE_HOME );
                    var addressList = locationService.Queryable().Where( l => l.Street1 == txtStreet.Text
                        && l.City == txtCity.Text && l.State == ddlState.SelectedValue && l.Zip == txtZip.Text
                        && l.LocationTypeValueId == homeLocationType.Id ).ToList();

                    if ( !addressList.Any() )
                    {
                        homeAddress = new Location();
                        locationService.Add( homeAddress, person.Id );
                    }
                    else
                    {
                        homeAddress = addressList.FirstOrDefault();
                    }

                    homeAddress.Street1 = txtStreet.Text ?? homeAddress.Street1;
                    homeAddress.City = txtCity.Text ?? homeAddress.City;
                    homeAddress.State = ddlState.SelectedValue ?? homeAddress.State;
                    homeAddress.Zip = txtZip.Text ?? homeAddress.Zip;
                    homeAddress.IsActive = true;
                    homeAddress.IsLocation = true;
                    homeAddress.Country = "US";
                    homeAddress.LocationTypeValueId = homeLocationType.Id;
                    locationService.Save( homeAddress, person.Id );

                    GroupType familyGroupType = new GroupTypeService().Get( new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ) );
                    var familyGroupList = groupMemberService.Queryable().Where( g => g.PersonId == person.Id
                        && g.Group.GroupType.Guid == familyGroupType.Guid ).Select( g => g.Group ).ToList();

                    if ( !familyGroupList.Any() )
                    {
                        familyGroup = new Group();
                        familyGroup.IsActive = true;
                        familyGroup.IsSystem = false;
                        familyGroup.IsSecurityRole = false;
                        familyGroup.Name = "The " + txtLastName.Text + " Family";
                        familyGroup.GroupTypeId = familyGroupType.Id;
                        groupService.Add( familyGroup, person.Id );
                        groupService.Save( familyGroup, person.Id );

                        var familyMember = new GroupMember();
                        familyMember.IsSystem = false;
                        familyMember.GroupId = familyGroup.Id;
                        familyMember.PersonId = person.Id;
                        familyMember.GroupRoleId = new GroupRoleService().Get( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ).Id;
                        groupMemberService.Add( familyMember, person.Id );
                        groupMemberService.Save( familyMember, person.Id );
                    }
                    else
                    {
                        familyGroup = familyGroupList.FirstOrDefault();
                    }

                    var groupLocationList = groupLocationService.Queryable().Where( g => g.GroupLocationTypeValueId == familyGroupType.Id
                        && g.GroupId == familyGroup.Id ).ToList();

                    if ( !groupLocationList.Any() )
                    {
                        groupLocation = new GroupLocation();
                        groupLocation.GroupId = familyGroup.Id;
                        groupLocation.LocationId = homeAddress.Id;
                        groupLocation.IsMailing = true;
                        groupLocation.IsLocation = true;
                        groupLocation.GroupLocationTypeValueId = homeLocationType.Id;
                        groupLocationService.Add( groupLocation, person.Id );
                        groupLocationService.Save( groupLocation, person.Id );
                    }
                    else
                    {
                        groupLocation = groupLocationList.FirstOrDefault();
                    }

                    groupLocation.LocationId = homeAddress.Id;
                    groupLocationService.Save( groupLocation, person.Id );

                    var homePhoneType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME );
                    string phoneNumeric = txtPhone.Text.AsNumeric();
                    if ( !phoneService.Queryable().Where( n => n.PersonId == person.Id
                        && n.NumberTypeValueId == homePhoneType.Id && n.Number == phoneNumeric ).Any() )
                    {
                        var homePhone = new PhoneNumber();
                        homePhone.Number = phoneNumeric;
                        homePhone.PersonId = person.Id;
                        homePhone.IsSystem = false;
                        homePhone.IsMessagingEnabled = false;
                        homePhone.IsUnlisted = false;
                        homePhone.NumberTypeValueId = homePhoneType.Id;
                        phoneService.Add( homePhone, person.Id );
                        phoneService.Save( homePhone, person.Id );
                    }
                } );
            }

            var amountList = (Dictionary<FinancialAccount, Decimal>)Session["CachedAmounts"];
            var profileId = (int)Session["CachedProfileId"];
            Location giftLocation = new Location();

            var configValues = (Dictionary<string, object>)Session["CachedMergeFields"];
            configValues.Add( "Date", DateTimeOffset.Now.ToString( "MM/dd/yyyy hh:mm tt" ) );
            
            var receiptTemplate = GetAttributeValue( "ReceiptMessage" );
            lReceipt.Text = receiptTemplate.ResolveMergeFields( configValues );
            var summaryTemplate = GetAttributeValue( "SummaryMessage" );
            string summaryMessage = summaryTemplate.ResolveMergeFields( configValues );

            var creditProcessorId = GetAttributeValue( "CreditCardProvider" );
            var achProcessorId = GetAttributeValue( "Checking/ACHProvider" );
            var gatewayService = new FinancialGatewayService();
            FinancialGateway gateway;

            if ( !string.IsNullOrEmpty( txtCreditCard.Text ) && !string.IsNullOrWhiteSpace( creditProcessorId ) )
            {
                int creditId = Convert.ToInt32( creditProcessorId );                
                gateway = new FinancialGatewayService().Get( creditId );
            }
            else if ( !string.IsNullOrEmpty( txtAccountNumber.Text ) && !string.IsNullOrWhiteSpace( achProcessorId ) )
            {
                int achId = Convert.ToInt32( achProcessorId );
                gateway = new FinancialGatewayService().Get( achId );
            }
            else 
            {
                gateway = gatewayService.Queryable().FirstOrDefault();
            }

            // #TODO test card through gateway 

            if ( btnFrequency.SelectedIndex > -1 && btnFrequency.SelectedValueAsInt() != DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_TYPE_ONE_TIME ).Id )
            {
                using ( new UnitOfWorkScope() )
                {
                    RockTransactionScope.WrapTransaction( () =>
                    {
                        var scheduledTransactionDetailService = new FinancialScheduledTransactionDetailService();
                        var scheduledTransactionService = new FinancialScheduledTransactionService();
                        FinancialScheduledTransaction scheduledTransaction;
                        var detailList = amountList.ToList();

                        if ( profileId > 0 )
                        {
                            scheduledTransaction = scheduledTransactionService.Get( profileId );
                        }
                        else
                        {
                            scheduledTransaction = new FinancialScheduledTransaction();
                            scheduledTransactionService.Add( scheduledTransaction, person.Id );
                        }

                        DateTime startDate = (DateTime)dtpStartDate.SelectedDate;
                        if ( startDate != null )
                        {
                            scheduledTransaction.StartDate = startDate;                                                     
                        }

                        scheduledTransaction.TransactionFrequencyValueId = (int)btnFrequency.SelectedValueAsInt();
                        scheduledTransaction.AuthorizedPersonId = person.Id;
                        scheduledTransaction.IsActive = true;

                        if ( !string.IsNullOrEmpty( txtCreditCard.Text ) )
                        {
                            scheduledTransaction.CardReminderDate = mypExpiration.SelectedDate;
                        }                        

                        if ( chkLimitGifts.Checked && !string.IsNullOrWhiteSpace( txtLimitNumber.Text ) )
                        {
                            scheduledTransaction.NumberOfPayments = Convert.ToInt32( txtLimitNumber.Text );
                        }                                                
                        
                        foreach ( var detail in amountList.ToList() )
                        {
                            var scheduledTransactionDetail = new FinancialScheduledTransactionDetail();
                            scheduledTransactionDetail.AccountId = detail.Key.Id;
                            scheduledTransactionDetail.Amount = detail.Value;
                            scheduledTransactionDetail.ScheduledTransactionId = scheduledTransaction.Id;
                            scheduledTransactionDetailService.Add( scheduledTransactionDetail, person.Id );
                            scheduledTransactionDetailService.Save( scheduledTransactionDetail, person.Id );
                        }

                        // implement gateway charge()

                        scheduledTransactionService.Save( scheduledTransaction, person.Id );
                    });
                }
            }
            else
            {
                using ( new UnitOfWorkScope() )
                {
                    RockTransactionScope.WrapTransaction( () =>
                    {
                        var transactionService = new FinancialTransactionService();
                        var tdService = new FinancialTransactionDetailService();
                        var transaction = new FinancialTransaction();
                        var detailList = amountList.ToList();

                        transaction.Summary = summaryMessage;
                        transaction.Amount = detailList.Sum( d => d.Value );
                        transaction.TransactionTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION ).Id;
                        transaction.TransactionDateTime = DateTimeOffset.Now.DateTime;
                        transaction.AuthorizedPersonId = person.Id;
                        transactionService.Add( transaction, person.Id );
                        
                        foreach ( var detail in detailList )
                        {
                            var td = new FinancialTransactionDetail();
                            td.TransactionId = transaction.Id;
                            td.AccountId = detail.Key.Id;
                            td.Amount = detail.Value;
                            td.TransactionId = transaction.Id;
                            tdService.Add( td, person.Id );
                            tdService.Save( td, person.Id );
                        }

                        // #TODO implement gateway.charge()

                        transactionService.Save( transaction, person.Id );
                    } );
                }
            }

            Session["CachedMergeFields"] = configValues;
            pnlConfirm.Visible = false;
            pnlComplete.Visible = true;
            pnlContribution.Update();
        }

        /// <summary>
        /// Handles the Click event of the btnSavePaymentInfo control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSavePaymentInfo_Click( object sender, EventArgs e )
        {
            var accountService = new FinancialPersonSavedAccountService();
            var configValues = (Dictionary<string, object>)Session["CachedMergeFields"];
            string accountNickname = txtPaymentNick.Text;
            Person person = FindPerson();

            var account = accountService.Queryable().Where( a => a.Name == accountNickname
                && a.PersonId == person.Id ).FirstOrDefault();

            if ( account == null )
            {
                account = new FinancialPersonSavedAccount();
                accountService.Add( account, person.Id );
            }

            account.Name = accountNickname;
            // #TODO WITH GATEWAY CALL
            account.TransactionCode = "Unknown";
            
            account.PersonId = person.Id;
            account.MaskedAccountNumber = configValues["PaymentLastFour"].ToString();

            if ( !string.IsNullOrEmpty( txtCreditCard.Text ) )
            {
                account.PaymentMethod = PaymentMethod.CreditCard;
            }
            else if ( !string.IsNullOrEmpty( txtAccountNumber.Text ) )
            {
                account.PaymentMethod = PaymentMethod.ACH;
            }            
            
            accountService.Save( account, person.Id );
            divPaymentNick.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the btnCreateAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCreateAccount_Click( object sender, EventArgs e )
        {
            Dictionary<string, string> userValues = new Dictionary<string, string>();
            ViewState["Password"] = txtPassword.Text;            
            userValues.Add( "FirstName", txtFirstName.Text );
            userValues.Add( "LastName", txtLastName.Text );
            userValues.Add( "Email", txtEmail.Text );
            NavigateToLinkedPage( "NewAccount", userValues );
        }
        
        #endregion

        #region Internal Methods

        /// <summary>
        /// Saves the account values.
        /// </summary>
        protected void SaveAmounts()
        {
            var amountList = (Dictionary<FinancialAccount, Decimal>)Session["CachedAmounts"] ?? 
                    new Dictionary<FinancialAccount, Decimal>();   

            foreach ( RepeaterItem item in rptAccountList.Items )
            {
                var accountId = Convert.ToInt32( ( (HiddenField)item.FindControl( "hfAccountId" ) ).Value );
                var control = (NumberBox)item.FindControl( "txtAccountAmount" );
                var accountAmount = ( (NumberBox)item.FindControl( "txtAccountAmount" ) ).Text;
                
                if ( !string.IsNullOrWhiteSpace( accountAmount ) && Decimal.Parse(accountAmount) > 0 )
                {
                    var key = amountList.Keys.Where( d => d.Id == accountId ).FirstOrDefault();
                    amountList[key] = Decimal.Parse( accountAmount );
                }                                
            }

            Session["CachedAmounts"] = amountList;
        }

        /// <summary>
        /// Rebinds the amounts.
        /// </summary>
        protected void RebindAmounts( Dictionary<FinancialAccount, Decimal> amountList = null )
        {
            if ( amountList == null )
            {
                amountList = (Dictionary<FinancialAccount, Decimal>)Session["CachedAmounts"] ?? 
                    new Dictionary<FinancialAccount, Decimal>();   
            }
            rptAccountList.DataSource = amountList;
            rptAccountList.DataBind();
            spnTotal.InnerText = amountList.Sum( d => d.Value ).ToString();
            Session["CachedAmounts"] = amountList;
        }

        /// <summary>
        /// Binds the campuses.
        /// </summary>
        protected void BindCampuses()
        {
            btnCampusList.Items.Clear();
            CampusService campusService = new CampusService();
            var items = campusService.Queryable().OrderBy( a => a.Name ).Distinct();

            if ( items.Any() )
            {
                btnCampusList.DataSource = items.ToList();
                btnCampusList.DataTextField = "Name";
                btnCampusList.DataValueField = "Id";
                btnCampusList.DataBind();
                btnCampusList.SelectedValue = btnCampusList.Items[0].Value;
            }
            else
            {
                divCampus.Visible = false;
            }
        }

        /// <summary>
        /// Binds the frequencies.
        /// </summary>
        protected void BindFrequencies()
        {
            var frequencyTypes = DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_FREQUENCY ) );
            if ( frequencyTypes != null )
            {
                btnFrequency.BindToDefinedType( frequencyTypes );
                btnFrequency.SelectedValue = btnFrequency.Items[0].Value;
            }            
        }
        
        /// <summary>
        /// Binds the profile.
        /// </summary>
        /// <param name="profileId">The profile id.</param>
        protected void BindGivingProfile( int profileId )
        {
            DateTime currentDate = DateTimeOffset.Now.DateTime;
            var accountService = new FinancialAccountService();
            var activeAccounts = accountService.Queryable().Where( f => f.IsActive
                && ( f.StartDate == null || f.StartDate <= currentDate )
                && ( f.EndDate == null || f.EndDate <= currentDate )
                && f.PublicName != null && f.PublicName != "" );
            var accountGuids = GetAttributeValues( "DefaultAccounts" ).Select( Guid.Parse ).ToList();
            var scheduledTransactionService = new FinancialScheduledTransactionService();
            var transactionList = new Dictionary<FinancialAccount, decimal>();
            FinancialScheduledTransaction scheduledTransaction;

            if ( profileId != 0 && scheduledTransactionService.TryGet( profileId, out scheduledTransaction ) )
            {   // Retrieve Transaction
                btnFrequency.SelectedValue = scheduledTransaction.TransactionFrequencyValue.Id.ToString();
                divFrequency.Visible = true;
                dtpStartDate.SelectedDate = scheduledTransaction.StartDate;                
                divRecurrence.Visible = true;
                divLimitGifts.Visible = true;

                if ( scheduledTransaction.NumberOfPayments != null )
                {
                    chkLimitGifts.Checked = true;
                    txtLimitNumber.Text = scheduledTransaction.NumberOfPayments.ToString();
                    divLimitNumber.Visible = true;
                }

                foreach ( var details in scheduledTransaction.ScheduledTransactionDetails)
                {
                    transactionList.Add( details.Account, details.Amount );
                }               
            }     
            else 
            {   // New Transaction
                IQueryable<FinancialAccount> selectedAccounts = activeAccounts;

                if ( accountGuids.Any() )
                {
                    selectedAccounts = selectedAccounts.Where( a => accountGuids.Contains( a.Guid ) );
                }

                var campusId = btnCampusList.SelectedValueAsInt();
                selectedAccounts = selectedAccounts.Where( f => f.CampusId == campusId 
                    || ( f.CampusId == null && f.ChildAccounts.Count() == 0 ) );

                foreach ( var account in selectedAccounts )
                {
                    transactionList.Add( account, 0M );
                }                  
            }

            if ( activeAccounts.Count() > transactionList.Count() && Convert.ToBoolean( GetAttributeValue( "ShowAdditionalAccounts" ) ) )
            {
                var unselectedAccounts = activeAccounts.Where( a => !accountGuids.Contains( a.Guid ) ).ToList();

                if ( unselectedAccounts.Any() )
                {
                    btnAddAccount.DataTextField = "PublicName";
                    btnAddAccount.DataValueField = "Id";
                    btnAddAccount.DataSource = unselectedAccounts.ToList();
                    btnAddAccount.DataBind();
                }
            }
            else
            {
                divAddAccount.Visible = false;
            }

            Session["CachedAmounts"] = transactionList;
            Session["CachedProfileId"] = profileId;
            rptAccountList.DataSource = transactionList;
            rptAccountList.DataBind();
            spnTotal.InnerText = transactionList.Sum( d => d.Value ).ToString( "f2" );
        }

        /// <summary>
        /// Binds the payment types.
        /// </summary>
        protected void BindPaymentTypes( bool showCredit, bool showChecking )
        {
            var queryable = DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_PAYMENT_TYPE ) )
                .DefinedValues.AsQueryable();

            if ( !showCredit )
            {
                queryable = queryable.Where( dv => dv.Guid != new Guid( Rock.SystemGuid.DefinedValue.TRANSACTION_PAYMENT_TYPE_CREDIT_CARD ) );
                divCreditCard.Visible = false;
                mypExpiration.SelectedDate = DateTime.Now.AddYears( 2 );
            }
            if ( !showChecking )
            {
                queryable = queryable.Where( dv => dv.Guid != new Guid( Rock.SystemGuid.DefinedValue.TRANSACTION_PAYMENT_TYPE_CHECKING ) );
                divChecking.Visible = false;
            }

            rptPaymentType.DataSource = queryable.ToList();
            rptPaymentType.DataBind();

            ( (HtmlGenericControl)rptPaymentType.Items[0].FindControl( "liSelectedTab" ) ).AddCssClass( "active" );
        }
        
        /// <summary>
        /// Binds the person details if they're logged in.
        /// </summary>
        protected void BindPersonDetails()
        {
            var groupLocationService = new GroupLocationService();            
            var groupMemberService = new GroupMemberService();
            var phoneNumberService = new PhoneNumberService();
            var person = FindPerson();


            var personGroups = groupMemberService.Queryable().Where( gm => gm.PersonId == person.Id ).Select( gm => gm.GroupId ).ToList();
            var homePhoneType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME );
            var homeLocationType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.LOCATION_TYPE_HOME );

            Location personLocation = groupLocationService.Queryable().Where( g => personGroups.Contains( g.GroupId ) 
                && g.GroupLocationTypeValueId == homeLocationType.Id)
                .Select( g => g.Location).FirstOrDefault();

            txtFirstName.Text = CurrentPerson.GivenName.ToString();
            txtLastName.Text = CurrentPerson.LastName.ToString();
            txtEmail.Text = CurrentPerson.Email.ToString();
            txtCardName.Text = CurrentPerson.FullName;

            if ( personLocation != null )
            {                
                txtStreet.Text = personLocation.Street1.ToString() ?? string.Empty;
                txtCity.Text = personLocation.City.ToString() ?? string.Empty;
                ddlState.SelectedValue = personLocation.State.ToUpper() ?? string.Empty;
                txtZip.Text = personLocation.Zip.ToString() ?? string.Empty;                
            }

            var homePhone = phoneNumberService.Queryable().Where( n => n.PersonId == person.Id
                && n.NumberTypeValueId == homePhoneType.Id ).FirstOrDefault();            
            if ( homePhone != null )
            {
                txtPhone.Text = homePhone.Number;
            }
        }

        /// <summary>
        /// Binds the saved payments if there are any.
        /// </summary>
        protected void BindSavedPayments()
        {
            var savedAccountService = new FinancialPersonSavedAccountService();
            var person = FindPerson();

            var accountsQueryable = savedAccountService.Queryable().Where( a => a.PersonId == person.Id );

            if ( accountsQueryable.Count() > 0 )
            {
                if ( accountsQueryable.Where( a => a.PaymentMethod == PaymentMethod.CreditCard ).Any() )
                {
                    var savedCreditCard = accountsQueryable.Where( a => a.PaymentMethod == PaymentMethod.CreditCard )
                        .ToDictionary( a => "Use " + a.Name + " ending in *************" + a.MaskedAccountNumber, a => a.Id );
                    savedCreditCard.Add( "Use a different card", 0 );
                    rblSavedCard.DataSource = savedCreditCard;
                    rblSavedCard.DataValueField = "Value";
                    rblSavedCard.DataTextField = "Key";
                    rblSavedCard.DataBind();
                    divSavedCard.Visible = true;                 
                    divNewCard.Style["display"] = "none";
                }

                if ( accountsQueryable.Where( a => a.PaymentMethod == PaymentMethod.ACH ).Any() )
                {
                    var savedACH = accountsQueryable.Where( a => a.PaymentMethod == PaymentMethod.ACH )
                        .ToDictionary( a => "Use " + a.Name + " account ending in *************" + a.MaskedAccountNumber, a => a.Id );
                    savedACH.Add( "Use a different account", 0 );
                    rblSavedCheck.DataSource = savedACH;
                    rblSavedCheck.DataValueField = "Value";
                    rblSavedCheck.DataTextField = "Key";
                    rblSavedCheck.DataBind();
                    divSavedCheck.Visible = true;
                    divNewCheck.Style["display"] = "none";
                }
            }
            else
            {
                divSavedCard.Visible = false;
                divSavedCheck.Visible = false;
                txtCreditCard.Required = true;
                mypExpiration.Required = true;
                txtCVV.Required = true;
                txtCardName.Required = true;
                txtBankName.Required = true;
                txtRoutingNumber.Required = true;
                txtAccountNumber.Required = true;                
            }           
        }

        /// <summary>
        /// Shows the selected pane.
        /// </summary>
        private void ShowSelectedTab()
        {
            var currentTab = CurrentTab;
            if ( currentTab.Equals( "Credit Card" ) )
            {
                divCreditCard.Visible = true;
                divChecking.Visible = false;
            }
            else if ( CurrentTab.Equals( "Checking/ACH" ) )
            {
                divChecking.Visible = true;
                divCreditCard.Visible = false;
            }
            else
            {
                divChecking.Visible = false;
                divCreditCard.Visible = false;
            }

            pnlPaymentTabs.Update();
            pnlPaymentContent.Update();
        }

        /// <summary>
        /// Handles the ServerValidate event of the AccountValidator control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="ServerValidateEventArgs"/> instance containing the event data.</param>
        protected void cvAccountValidator_ServerValidate( object source, ServerValidateEventArgs args )
        {
            foreach ( RepeaterItem item in rptAccountList.Items )
            {
                args.IsValid = false;
                decimal amount;
                Decimal.TryParse( ((NumberBox)item.FindControl( "txtAccountAmount" )).Text, out amount );
                if ( amount > 0 )
                {
                    args.IsValid = true;
                    return;
                }
            }
        }

        /// <summary>
        /// Finds the person if they're logged in, or by email and name. If not found, creates a new person.
        /// </summary>
        /// <returns></returns>
        private Person FindPerson()
        {
            Person person;
            var personService = new PersonService();

            if ( CurrentPerson != null )
            {
                person = CurrentPerson;
            }
            else
            {
                person = personService.GetByEmail( txtEmail.Text )
                    .FirstOrDefault( p => p.FirstName == txtFirstName.Text && p.LastName == txtLastName.Text );
            }

            if ( person == null )
            {
                person = new Person
                {
                    GivenName = txtFirstName.Text,
                    LastName = txtLastName.Text,
                    Email = txtEmail.Text,                    
                };

                personService.Add( person, CurrentPersonId );
                personService.Save( person, CurrentPersonId );
            }

            return person;
        }

        #endregion                       
    }
}