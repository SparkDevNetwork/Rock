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
        Your contribution will be itemized as follows:<br/>
        <ul>
        {% for TransactionDetail in TransactionList %}
            <li><strong>{{ TransactionDetail.Amount }}</strong> to the <strong>{{ TransactionDetail.Account }}</strong></li>
        {% endfor %}
        </ul><br/>
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

            // Set vertical layout
            if ( Convert.ToBoolean( GetAttributeValue( "ShowVerticalLayout" ) ) )
            {
                _spanClass = "span8 offset2";
                txtCity.LabelText = "City, State, Zip";
                diffCity.LabelText = "City, State, Zip";
                ddlState.LabelText = string.Empty;
                diffState.LabelText = string.Empty;
                txtZip.LabelText = string.Empty;
                diffZip.LabelText = string.Empty;
            }
            else
            {
                _spanClass = "span6";
            }            
            
            if ( !IsPostBack )
            {
                // Show Campus
                if ( Convert.ToBoolean( GetAttributeValue( "ShowCampuses" ) ) )
                {
                    BindCampuses();
                    divCampus.Visible = true;
                }

                // Show Frequencies
                if ( Convert.ToBoolean( GetAttributeValue( "ShowFrequencies" ) ) )
                {
                    BindFrequencies();
                    divFrequency.Visible = true;
                }
                
                // Load account information
                if ( CurrentPerson != null )
                {
                    BindPersonDetails();
                    divSavePayment.Visible = true;
                    divCreateAccount.Visible = false;
                }                

                // Require phone number
                if ( Convert.ToBoolean( GetAttributeValue( "RequirePhone" ) ) )
                {
                    txtPhone.Required = true;
                }

                BindAccounts();
                BindPaymentTypes();
            }            

            // test data
            txtStreet.Text = "street";
            txtCity.Text = "city";
            txtZip.Text = "149471";
            txtCreditCard.Text = "42567395723957";

            txtCVV.Text = "572";
            txtCardName.Text = "dave";
            txtPhone.Text = "525225";
            dtpExpiration.SelectedDate = DateTime.Now;            
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
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
            List<FinancialTransactionDetail> transactionList = Session["TransactionList"] as List<FinancialTransactionDetail>;
            FinancialAccountService accountService = new FinancialAccountService();
            
            var newAccountId = Convert.ToInt32( btnAddAccount.SelectedValue );
            FinancialTransactionDetail detail = new FinancialTransactionDetail();
            detail.AccountId = newAccountId;
            detail.Account = accountService.Get( newAccountId );
            transactionList.Add( detail );

            if ( btnAddAccount.Items.Count > 1 )
            {
                btnAddAccount.Items.Remove( btnAddAccount.SelectedItem );
                btnAddAccount.Title = "Add Another Gift";
            }
            else
            {
                divAddAccount.Visible = false;
            }

            SaveAccountValues( transactionList );
            RebindAccounts();
        }

        /// <summary>
        /// Handles the SelectionChanged event of the btnFrequency control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnFrequency_SelectionChanged( object sender, EventArgs e )
        {
            SaveAccountValues( Session["TransactionList"] as List<FinancialTransactionDetail> );
            
            if ( btnFrequency.SelectedValue != DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_TYPE_ONE_TIME_FUTURE ).Id.ToString()
                && btnFrequency.SelectedValue != DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_TYPE_ONE_TIME ).Id.ToString() )
            {
                if ( divRecurrence.Visible != true )
                {
                    divRecurrence.Visible = true;
                }

                if ( divLimitGifts.Visible != true )
                {
                    divLimitGifts.Visible = true;
                }
            }
            else if ( btnFrequency.SelectedValue == DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_TYPE_ONE_TIME_FUTURE ).Id.ToString() )
            {
                if ( divRecurrence.Visible != true )
                {
                    divRecurrence.Visible = true;
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
                }

                if ( divLimitGifts.Visible != false )
                {
                    divLimitGifts.Visible = false;
                }
            }

            RebindAccounts();
        }

        /// <summary>
        /// Handles the Click event of the btnBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnBack_Click( object sender, EventArgs e )
        {
            pnlConfirm.Visible = false;
            RebindAccounts();
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
            SaveAccountValues( Session["TransactionList"] as List<FinancialTransactionDetail> );
            divLimitNumber.Visible = !divLimitNumber.Visible;
            RebindAccounts();
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
            divCreateAccount.Visible = !divCreateAccount.Visible;
        }

        /// <summary>
        /// Handles the CheckedChanged event of the chkDefaultAddress control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void chkDefaultAddress_CheckedChanged( object sender, EventArgs e )
        {
            divNewAddress.Visible = !divNewAddress.Visible;
        }

        /// <summary>
        /// Handles the Click1 event of the lbPaymentType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbPaymentType_Click( object sender, EventArgs e )
        {
            SaveAccountValues( Session["TransactionList"] as List<FinancialTransactionDetail> );
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

            RebindAccounts();
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

            List<FinancialTransactionDetail> transactionList = Session["TransactionList"] as List<FinancialTransactionDetail>;
            GroupLocationService groupLocationService = new GroupLocationService();
            GroupMemberService groupMemberService = new GroupMemberService();
            PersonService personService = new PersonService();
            Location giftLocation = new Location();
            Person person;
            
            SaveAccountValues( transactionList );

            var personGroup = personService.GetByEmail( txtEmail.Text );

            if ( personGroup.Count() > 0 )
            {
                person = personGroup.Where( p => p.FirstName == txtFirstName.Text
                    && p.LastName == txtLastName.Text ).Distinct().FirstOrDefault();
                // TODO duplicate person handling?  ex NewAccount.ascx DisplayDuplicates()
            }
            else
            {
                person = new Person();
                personService.Add( person, CurrentPersonId );
                person.GivenName = txtFirstName.Text;
                person.LastName = txtLastName.Text;
                person.Email = txtEmail.Text;
            }            

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
                giftLocation.City = diffCity.Text;
                giftLocation.State = diffState.SelectedValue;
                giftLocation.Zip = diffZip.Text;
            }

            List<int> personGroups = groupMemberService.Queryable()
                .Where( g => g.PersonId == CurrentPersonId )
                .Select( g => g.GroupId ).ToList();

            Location personLocation = groupLocationService.Queryable()
                .Where( g => personGroups.Contains( g.GroupId ) )
                .Select( g => g.Location )
                .ToList().FirstOrDefault();

            personService.Save( person, CurrentPersonId );

            // Set up confirmation message
            var configValues = new Dictionary<string,object>();
            
            Rock.Web.Cache.GlobalAttributesCache.Read().AttributeValues
                .Where( v => 
                    v.Key.StartsWith( "Organization", StringComparison.CurrentCultureIgnoreCase ) )
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
            configValues.Add( "TotalContribution", transactionList.Sum( ftd => ftd.Amount ).ToString() );
            configValues.Add( "TransactionList", transactionList.Where( t => t.Amount > 0 ).ToArray() );            
            
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
            FinancialTransactionService transactionService = new FinancialTransactionService();
            FinancialTransaction transaction = new FinancialTransaction();

            //foreach ( transaction detail) 
            //{

            //}

            transactionService.Save( transaction, CurrentPersonId );

            //litDateGift.Text = DateTime.Now.ToString( "f" );
            
            pnlConfirm.Visible = false;
            pnlComplete.Visible = true;
        }

        #endregion

        #region Internal Methods
                
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
        /// Binds the payment types.
        /// </summary>
        protected void BindPaymentTypes()
        {
            var queryable = DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_PAYMENT_TYPE ) )
                .DefinedValues.AsQueryable();

            if ( !Convert.ToBoolean( GetAttributeValue( "ShowCreditCard" ) ) )
            {
                queryable = queryable.Where( dv => dv.Guid != new Guid( Rock.SystemGuid.DefinedValue.TRANSACTION_PAYMENT_TYPE_CREDIT_CARD ) );
                pnlCreditCard.Visible = false;
            }
            if ( !Convert.ToBoolean( GetAttributeValue( "ShowChecking/ACH" ) ) )
            {
                queryable = queryable.Where( dv => dv.Guid != new Guid( Rock.SystemGuid.DefinedValue.TRANSACTION_PAYMENT_TYPE_CHECKING ) );
                pnlChecking.Visible = false;
            }

            rptPaymentType.DataSource = queryable.ToList();
            rptPaymentType.DataBind();
        }
        
        /// <summary>
        /// Binds the accounts.
        /// </summary>
        protected void BindAccounts()
        {
            List<FinancialTransactionDetail> transactionList = new List<FinancialTransactionDetail>();
            FinancialAccountService accountService = new FinancialAccountService();

            var selectedAccounts = accountService.Queryable().Where( f => f.IsActive );
            if ( btnCampusList.SelectedIndex != -1 )
            {
                selectedAccounts = selectedAccounts.Where( f => f.CampusId == Convert.ToInt32( btnCampusList.SelectedValue ) );
            }
            
            if ( GetAttributeValue( "DefaultAccounts" ).Any() )
            {
                var accountGuids = GetAttributeValues( "DefaultAccounts" ).Select( Guid.Parse ).ToList();

                btnAddAccount.DataTextField = "PublicName";
                btnAddAccount.DataValueField = "Id";
                btnAddAccount.DataSource = selectedAccounts.Where( a => !accountGuids.Contains( a.Guid ) ).ToList();
                btnAddAccount.DataBind();                

                selectedAccounts = selectedAccounts.Where( a => accountGuids.Contains( a.Guid ) );
            }
            else
            {
                divAddAccount.Visible = false;
            }
                        
            foreach ( var account in selectedAccounts )
            {
                FinancialTransactionDetail detail = new FinancialTransactionDetail();
                detail.AccountId = account.Id;
                detail.Account = account;
                transactionList.Add( detail );
            }

            Session["TransactionList"] = transactionList;
            rptAccountList.DataSource = transactionList;
            rptAccountList.DataBind();            
        }

        /// <summary>
        /// Saves the account values.
        /// </summary>
        protected void SaveAccountValues( List<FinancialTransactionDetail> transactionList )
        {
            foreach ( RepeaterItem item in rptAccountList.Items )
            {
                var accountId = Convert.ToInt32( ( (HiddenField)item.FindControl( "hfAccountId" ) ).Value );
                var accountAmount = ( (NumberBox)item.FindControl( "txtAccountAmount" ) ).Text;
                var index = transactionList.FindIndex( a => a.AccountId == accountId );
                if ( index >= 0 )
                {
                    transactionList[index].Amount = !string.IsNullOrWhiteSpace( accountAmount ) ? Decimal.Parse( accountAmount ) : 0M;
                }                
            }
            Session["TransactionList"] = transactionList;
        }

        /// <summary>
        /// Rebinds the accounts.
        /// </summary>
        protected void RebindAccounts()
        {
            List<FinancialTransactionDetail> transactionList = Session["TransactionList"] as List<FinancialTransactionDetail>;
            rptAccountList.DataSource = transactionList;
            rptAccountList.DataBind();
            Session["TransactionList"] = transactionList;
        }

        /// <summary>
        /// Binds the person details if they're logged in.
        /// </summary>
        protected void BindPersonDetails()
        {
            GroupMemberService groupService = new GroupMemberService();
            LocationService locationService = new LocationService();
            GroupLocationService groupLocationService = new GroupLocationService();

            List<int> personGroups = groupService.Queryable()
                .Where( g => g.PersonId == CurrentPersonId )
                .Select( g => g.GroupId ).ToList();

            Location personLocation = groupLocationService.Queryable()
                .Where( g => personGroups.Contains( g.GroupId ) )
                .Select( g => g.Location )
                .ToList().FirstOrDefault();

            txtFirstName.Text = CurrentPerson.FirstName.ToString();
            txtLastName.Text = CurrentPerson.LastName.ToString();
            txtEmail.Text = CurrentPerson.Email.ToString();

            if ( personLocation != null )
            {                
                txtStreet.Text = personLocation.Street1.ToString();
                txtCity.Text = personLocation.City.ToString();
                ddlState.Text = personLocation.State.ToString();
                txtZip.Text = personLocation.Zip.ToString();
                txtEmail.Text = CurrentPerson.Email.ToString();
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
                pnlChecking.Visible = false;
            }
            else if ( CurrentTab.Equals( "Checking/ACH" ) )
            {
                pnlCreditCard.Visible = false;
                pnlChecking.Visible = true;
            }
        }

        #endregion        
    }
}