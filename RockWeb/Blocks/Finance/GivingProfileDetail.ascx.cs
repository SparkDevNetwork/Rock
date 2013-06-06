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
    [LinkedPage( "New Account" )]
    [BooleanField( "Require Phone", "Should financial contributions require a user's phone number?", true, "Data Requirements", 0 )]   
    [CustomCheckboxListField( "Credit Card Provider", "Which payment processor should be used for credit cards?",
        "SELECT [Name] AS [Text], [Id] AS [Value] FROM [FinancialGateway]", true, "", "Payments", 0 )]
    [CustomCheckboxListField( "Checking/ACH Provider", "Which payment processor should be used for checking/ACH?",
        "SELECT [Name] AS [Text], [Id] AS [Value] FROM [FinancialGateway]", true, "", "Payments", 1 )]
    [AccountsField("Default Accounts", "Which accounts should be displayed by default?", true, "", "Payments", 2)]
    [BooleanField( "Show Vertical Layout", "Should the giving page display vertically or horizontally?", true, "UI Options", 0 )]
    [BooleanField( "Show Campuses", "Should giving be associated with a specific campus?", false, "UI Options", 1 )]
    [BooleanField( "Show Credit Card", "Allow users to give using a credit card?", true, "UI Options", 2 )]
    [BooleanField( "Show Checking/ACH", "Allow users to give using a checking account?", true, "UI Options", 3 )]
    [BooleanField( "Show Frequencies", "Allow users to give recurring gifts?", true, "UI Options", 4 )]
    [TextField( "Confirmation Message", "What text should be displayed on the confirmation page?", true,
        @"{{ ContributionHeader }}<br/><br/>
        {{ Person.FullName }},<br/><br/>
        You're about to give a total of <strong>{{ TotalContribution }}</strong> using your {{ PaymentType }} ending in {{ PaymentLastFour }}.<br/><br/>
        If this is correct, please press Give.  Otherwise, click Back to edit.<br/>
        Thank you,<br/>
        {{ OrganizationName }}<br/>  
        {{ ContributionFooter }}"
    , "UI Options", 5)]
    [TextField( "Receipt Message", "What text should be displayed on the receipt page?", true,
        @"{{ ReceiptHeader }}<br/><br/>
        {{ Person.FullName }},<br/><br/>
        Thank you for your generosity! You just gave a total of {{ TotalContribution }} to {{ OrganizationName }}.<br/><br/>        
        {{ ReceiptFooter }}"
    , "UI Options", 5 )]
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
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( !IsPostBack )
            {
                // Change Layout vertically or horizontally
                if ( Convert.ToBoolean( GetAttributeValue( "ShowVerticalLayout" ) ) )
                {
                    _spanClass = "span9 offset2";

                    txtCity.LabelText = "City, State, Zip";
                    txtNewCity.LabelText = "City, State, Zip";
                    ddlState.LabelText = string.Empty;
                    ddlNewState.LabelText = string.Empty;
                    txtZip.LabelText = string.Empty;
                    txtNewZip.LabelText = string.Empty;

                    divCity.AddCssClass( "span7" );
                    divState.AddCssClass( "span3" );
                    divZip.AddCssClass( "span2" );
                    divPayment.AddCssClass( "form-horizontal" );
                    divCreditCard.AddCssClass( "span7" );
                    divCardType.AddCssClass( "span5" );
                    divChecking.AddCssClass( "span7" );
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
                    divCreditCard.AddCssClass( "span6" );
                    divCardType.AddCssClass( "span6 label-padding" );
                    divExpiration.AddCssClass( "span6" );
                    divCVV.AddCssClass( "span6" );
                    divChecking.AddCssClass( "span6" );
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
                divGiveBack.AddCssClass( _spanClass );
                divReceipt.AddCssClass( _spanClass );
                divPrint.AddCssClass( _spanClass );

                // Show Campus
                if ( Convert.ToBoolean( GetAttributeValue( "ShowCampuses" ) ) )
                {
                    BindCampuses();
                    divCampus.Visible = true;
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
                else
                {
                    divFrequency.Visible = false;
                }

                // Show Payment types
                bool showCredit = Convert.ToBoolean( GetAttributeValue( "ShowCreditCard" ) );
                bool showChecking = Convert.ToBoolean( GetAttributeValue( "ShowChecking/ACH" ) );
                BindPaymentTypes( showCredit, showChecking );

                // Require Phone
                if ( Convert.ToBoolean( GetAttributeValue( "RequirePhone" ) ) )
                {
                    txtPhone.Required = true;
                }  
                                
                // Load Profile
                string profileId = PageParameter( "GivingProfileId" );
                if ( !string.IsNullOrWhiteSpace( profileId ) )
                {
                    BindProfile ( Convert.ToInt32( profileId ) );
                }
                else
                {
                    BindProfile( 0 );
                }

                if ( CurrentPerson != null )
                {
                    BindPersonDetails();
                    divSavePayment.Visible = true;
                    divCreateAccount.Visible = false;
                }                                              
            }                                 
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        //protected override void OnLoad( EventArgs e )
        //{
        //    base.OnLoad( e );            
        //}
        
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

            RebindAmounts( (Dictionary<FinancialAccount, Decimal>)Session["CachedAmounts"] );
        }

        /// <summary>
        /// Handles the Click event of the btnBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnBack_Click( object sender, EventArgs e )
        {
            pnlConfirm.Visible = false;
            RebindAmounts( (Dictionary<FinancialAccount, Decimal>)Session["CachedAmounts"] );
            pnlDetails.Visible = true;
            pnlPayment.Visible = true;
        }

        /// <summary>
        /// Handles the CheckedChanged event of the chkLimitGifts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void chkLimitGifts_CheckedChanged( object sender, EventArgs e )
        {
            SaveAmounts();
            divLimitNumber.Visible = !divLimitNumber.Visible;
            RebindAmounts( (Dictionary<FinancialAccount, Decimal>)Session["CachedAmounts"] );
        }
            
        /// <summary>
        /// Handles the CheckedChanged event of the chkSaveCheck control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void chkSavePayment_CheckedChanged( object sender, EventArgs e )
        {
            divPaymentNick.Visible = !divPaymentNick.Visible;
        }
        
        /// <summary>
        /// Handles the CheckedChanged event of the chkCreateAcct control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void chkCreateAccount_CheckedChanged( object sender, EventArgs e )
        {
            divCredentials.Visible = !divCredentials.Visible;
        }

        /// <summary>
        /// Handles the CheckedChanged event of the chkDefaultAddress control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void chkDefaultAddress_CheckedChanged( object sender, EventArgs e )
        {
            SaveAmounts();
            divNewAddress.Visible = !divNewAddress.Visible;
            txtNewCity.Required = !txtNewCity.Required;
            ddlNewState.Required = !ddlNewState.Required;
            txtNewZip.Required = !txtNewZip.Required;
            RebindAmounts( (Dictionary<FinancialAccount, Decimal>)Session["CachedAmounts"] );
        }

        /// <summary>
        /// Handles the Click1 event of the lbPaymentType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbPaymentType_Click( object sender, EventArgs e )
        {
            SaveAmounts();
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

            RebindAmounts( (Dictionary<FinancialAccount, Decimal>)Session["CachedAmounts"] );
            ShowSelectedTab();
        }

        /// <summary>
        /// Handles the Click event of the btnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnNext_Click( object sender, EventArgs e )
        {
            if ( !Page.IsValid )
            {
                return;
            }

            SaveAmounts();

            var amountList = (Dictionary<FinancialAccount, Decimal>)Session["CachedAmounts"];
            Person person = FindPerson();        

            // Set up confirmation message
            var configValues = new Dictionary<string,object>();
            
            Rock.Web.Cache.GlobalAttributesCache.Read().AttributeValues
                .Where( v => 
                    v.Key.StartsWith( "Organization", StringComparison.CurrentCultureIgnoreCase ) || 
                    v.Key.StartsWith( "Confirmation", StringComparison.CurrentCultureIgnoreCase ) )
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
                    // type visa
                    configValues.Add( "PaymentType", "Visa" );
                }
                else if ( System.Text.RegularExpressions.Regex.IsMatch( txtCreditCard.Text, mastercard ) )
                {
                    // type mastercard
                    configValues.Add( "PaymentType", "MasterCard" );
                }
                else if ( System.Text.RegularExpressions.Regex.IsMatch( txtCreditCard.Text, amex ) )
                {
                    // type amex
                    configValues.Add( "PaymentType", "American Express" );
                }
                else if ( System.Text.RegularExpressions.Regex.IsMatch( txtCreditCard.Text, discover ) )
                {
                    // type discover
                    configValues.Add( "PaymentType", "Discover" );
                }

                configValues.Add( "PaymentLastFour", txtCreditCard.Text.Substring( txtCreditCard.Text.Length - 4, 4 ) );
            }
            else if ( !string.IsNullOrEmpty( txtAccount.Text ) )
            {
                configValues.Add( "PaymentType", rblAccountType.SelectedValue + " account" );
                configValues.Add( "PaymentLastFour", txtAccount.Text.Substring( txtAccount.Text.Length - 4, 4 ) );
            }

            
            configValues.Add( "Person", person );
            configValues.Add( "TotalContribution", amountList.Sum( td => td.Value ).ToString() );
            configValues.Add( "Transactions", amountList.Where( td => td.Value > 0 ).ToArray() );
            
            var confirmationTemplate = GetAttributeValue( "ConfirmationMessage" );
            lPaymentConfirmation.Text = confirmationTemplate.ResolveMergeFields( configValues );
                        
            pnlDetails.Visible = false;
            pnlPayment.Visible = false;
            pnlConfirm.Visible = true;            
        }

        /// <summary>
        /// Handles the Click event of the btnGive control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnGive_Click( object sender, EventArgs e )
        {
            var amountList = (Dictionary<FinancialAccount, Decimal>)Session["CachedAmounts"];
            Location giftLocation = new Location();     
            Person person = FindPerson();

            if ( chkDefaultAddress.Checked )
            {
                giftLocation.Street1 = txtStreet.Text;
                giftLocation.City = txtCity.Text;
                giftLocation.State = ddlState.SelectedValue;
                giftLocation.Zip = txtZip.Text;
            }
            else
            {
                giftLocation.Street1 = diffStreet.Text;
                giftLocation.City = txtNewCity.Text;
                giftLocation.State = ddlNewState.SelectedValue;
                giftLocation.Zip = txtNewZip.Text;
            }

            ////////////// #TODO ///////////////////
            ///////////// Process //////////////////
            ///////////// payment //////////////////
            ///////////// through //////////////////
            ///////////// gateway //////////////////
            ///////////// service //////////////////
            ////////////// #TODO ///////////////////

            if ( divFrequency.Visible && btnFrequency.SelectedValueAsInt() != DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_TYPE_ONE_TIME ).Id )
            {
                using ( new UnitOfWorkScope() )
                {
                    RockTransactionScope.WrapTransaction( () =>
                    {
                        var scheduledTransactionDetailService = new FinancialScheduledTransactionDetailService();
                        var scheduledTransactionService = new FinancialScheduledTransactionService();                        
                        var scheduledTransaction = new FinancialScheduledTransaction();
                        var detailList = amountList.ToList();

                        scheduledTransaction.TransactionFrequencyValueId = (int)btnFrequency.SelectedValueAsInt();
                        scheduledTransaction.StartDate = (DateTime)dtpStartDate.SelectedDate;
                        scheduledTransaction.AuthorizedPersonId = person.Id;
                        scheduledTransaction.IsActive = true;

                        if ( chkLimitGifts.Checked && !string.IsNullOrWhiteSpace( txtLimitNumber.Text ) )
                        {
                            scheduledTransaction.NumberOfPayments = Convert.ToInt32( txtLimitNumber.Text );
                        }                                                
                        
                        scheduledTransactionService.Add( scheduledTransaction, person.Id );
                        scheduledTransactionService.Save( scheduledTransaction, person.Id );

                        foreach ( var detail in amountList.ToList() )
                        {
                            var scheduledTransactionDetail = new FinancialScheduledTransactionDetail();
                            scheduledTransactionDetail.AccountId = detail.Key.Id;
                            scheduledTransactionDetail.Amount = detail.Value;
                            scheduledTransactionDetail.ScheduledTransactionId = scheduledTransaction.Id;
                            scheduledTransactionDetailService.Add( scheduledTransactionDetail, person.Id );
                            scheduledTransactionDetailService.Save( scheduledTransactionDetail, person.Id );
                        }                        
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

                        transaction.Amount = detailList.Sum( d => d.Value );
                        transaction.TransactionTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION ).Id;
                        transaction.AuthorizedPersonId = person.Id;
                        transactionService.Add( transaction, person.Id );
                        transactionService.Save( transaction, person.Id );

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
                    } );
                }
            }
            
            // display receipt
            var configValues = new Dictionary<string, object>();
            Rock.Web.Cache.GlobalAttributesCache.Read().AttributeValues
                .Where( v =>
                    v.Key.StartsWith( "Organization", StringComparison.CurrentCultureIgnoreCase ) ||
                    v.Key.StartsWith( "Receipt", StringComparison.CurrentCultureIgnoreCase ) )
                .ToList()
                .ForEach( v => configValues.Add( v.Key, v.Value.Value ) );

            configValues.Add( "Person", person );
            configValues.Add( "TotalContribution", amountList.Sum( d => d.Value ).ToString() );

            var receiptTemplate = GetAttributeValue( "ReceiptMessage" );
            lReceipt.Text = receiptTemplate.ResolveMergeFields( configValues );

            pnlConfirm.Visible = false;
            pnlComplete.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnCreateAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCreateAccount_Click( object sender, EventArgs e )
        {
            ViewState["Password"] = txtPassword.Text;
            Dictionary<string, string> userValues = new Dictionary<string, string>();
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
            var amountList = (Dictionary<FinancialAccount, Decimal>)Session["CachedAmounts"];

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
            amountList = amountList ?? new Dictionary<FinancialAccount, Decimal>();
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
        protected void BindProfile( int profileId )
        {
            var accountService = new FinancialAccountService();
            var selectedAccounts = accountService.Queryable().Where( f => f.IsActive );
            var accountGuids = GetAttributeValues( "DefaultAccounts" ).Select( Guid.Parse ).ToList();
            var scheduledTransactionService = new FinancialScheduledTransactionService();
            var amountList = new Dictionary<FinancialAccount, decimal>();
            FinancialScheduledTransaction scheduledTransaction;

            if ( profileId != 0 && scheduledTransactionService.TryGet( profileId, out scheduledTransaction ) )
            {
                btnFrequency.SelectedValue = scheduledTransaction.TransactionFrequencyValue.ToString();
                dtpStartDate.SelectedDate = scheduledTransaction.StartDate;
                divFrequency.Visible = true;

                if ( scheduledTransaction.NumberOfPayments != null )
                {
                    chkLimitGifts.Checked = true;
                    txtLimitNumber.Text = scheduledTransaction.NumberOfPayments.ToString();
                    divLimitGifts.Visible = true;
                    divLimitNumber.Visible = true;
                }

                // set btnCampus.SelectedValue
                foreach ( var details in scheduledTransaction.ScheduledTransactionDetails)
                {
                    amountList.Add( details.Account, details.Amount );
                }               

            }     
            else 
            {
                if ( btnCampusList.SelectedIndex > -1 )
                {
                    var campusId = btnCampusList.SelectedValueAsInt();
                    selectedAccounts = selectedAccounts.Where( f => f.CampusId == campusId );
                }

                foreach ( var account in selectedAccounts.Where( a => accountGuids.Contains( a.Guid ) ) )
                {
                    amountList.Add( account, 0M );
                }                  
            }

            if ( accountGuids.Count > selectedAccounts.Count() )
            {
                var unselectedAccounts = selectedAccounts.Where( a => !accountGuids.Contains( a.Guid ) ).ToList();

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
                btnAddAccount.Visible = false;
            }

            Session["CachedAmounts"] = amountList;
            rptAccountList.DataSource = amountList;
            rptAccountList.DataBind();
        }

        /// <summary>
        /// Binds the payment types.
        /// </summary>
        protected void BindPaymentTypes( bool showCredit, bool showChecking )
        {
            // get all available payment types
            var queryable = DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_PAYMENT_TYPE ) )
                .DefinedValues.AsQueryable();

            if ( !showCredit )
            {
                queryable = queryable.Where( dv => dv.Guid != new Guid( Rock.SystemGuid.DefinedValue.TRANSACTION_PAYMENT_TYPE_CREDIT_CARD ) );
                pnlCreditCard.Visible = false;
                dtpExpiration.SelectedDate = DateTime.Now.AddYears(2);
            }
            if ( !showChecking )
            {
                queryable = queryable.Where( dv => dv.Guid != new Guid( Rock.SystemGuid.DefinedValue.TRANSACTION_PAYMENT_TYPE_CHECKING ) );
                pnlChecking.Visible = false;
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
            GroupLocationService groupLocationService = new GroupLocationService();
            GroupMemberService groupService = new GroupMemberService();
            LocationService locationService = new LocationService();            
            var person = FindPerson();

            List<int> personGroups = groupService.Queryable()
                .Where( g => g.PersonId == person.Id )
                .Select( g => g.GroupId ).ToList();

            Location personLocation = groupLocationService.Queryable()
                .Where( g => personGroups.Contains( g.GroupId ) )
                .Select( g => g.Location )
                .ToList().FirstOrDefault();

            txtFirstName.Text = CurrentPerson.GivenName.ToString();
            txtLastName.Text = CurrentPerson.LastName.ToString();
            txtEmail.Text = CurrentPerson.Email.ToString();

            if ( personLocation != null )
            {                
                txtStreet.Text = personLocation.Street1.ToString();
                txtCity.Text = personLocation.City.ToString();
                ddlState.Text = personLocation.State.ToString();
                txtZip.Text = personLocation.Zip.ToString();
                txtEmail.Text = CurrentPerson.Email.ToString();
                txtCardName.Text = CurrentPerson.FullName;
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
                pnlCreditCard.Visible = true;
                txtCreditCard.Required = true;
                dtpExpiration.Required = true;
                txtCVV.Required = true;
                txtCardName.Required = true;
                pnlChecking.Visible = false;
                txtBankName.Required = false;
                txtRouting.Required = false;
                txtAccount.Required = false;
            }
            else if ( CurrentTab.Equals( "Checking/ACH" ) )
            {
                pnlChecking.Visible = true;
                txtBankName.Required = true;
                txtRouting.Required = true;
                txtAccount.Required = true;
                pnlCreditCard.Visible = false;
                txtCreditCard.Required = false;
                dtpExpiration.Required = false;
                txtCVV.Required = false;
                txtCardName.Required = false;
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