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
    [CustomCheckboxListField( "Credit Card Provider", "Which payment processor should be used for credit cards?",
        "SELECT [Name] AS [Text], [Id] AS [Value] FROM [FinancialGateway]", true, "", "Payments", 0 )]
    [CustomCheckboxListField( "Checking/ACH Provider", "Which payment processor should be used for checking/ACH?",
        "SELECT [Name] AS [Text], [Id] AS [Value] FROM [FinancialGateway]", true, "", "Payments", 1 )]
    [AccountsField("Default Accounts", "Which accounts should be displayed by default?", true, "", "Payments", 2)]
    [BooleanField( "Show Vertical Layout", "Should the giving page display vertically or horizontally?", true, "UI Options", 0 )]
    [BooleanField( "Show Campuses", "Should giving be associated with a specific campus?", false, "UI Options", 1 )]
    [BooleanField( "Show Credit Card", "Allow users to give using a credit card?", true, "UI Options", 2 )]
    [BooleanField( "Show Checking/ACH", "Allow users to give using a checking account?", true, "UI Options", 3 )]
    [BooleanField( "Show Recurrence", "Allow users to give recurring gifts?", true, "UI Options", 4 )]
    [BooleanField( "Require Phone", "Should financial contributions require a user's phone number?", true, "Data Requirements", 0 )]    
    public partial class GivingProfileDetail : RockBlock
    {
        #region Fields

        protected bool _ShowCreditCard = false;
        protected bool _ShowChecking = false;
        protected string _spanClass = "";

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
                object currentTab = ViewState["CurrentTab"];
                return currentTab != null ? currentTab.ToString() : "Credit Card";
            }

            set
            {
                ViewState["CurrentTab"] = value;
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
            _spanClass = ( Convert.ToBoolean( GetAttributeValue( "ShowVerticalLayout" ) ) ) ? "span12" : "span6";

            
            _ShowCreditCard = Convert.ToBoolean( GetAttributeValue( "ShowCreditCard" ) );
            _ShowChecking = Convert.ToBoolean( GetAttributeValue( "ShowChecking/ACH" ) );
            
            if ( !IsPostBack )
            {
                // Load account information
                if ( CurrentPerson != null )
                {
                    BindPersonDetails();
                    divSavePayment.Visible = true;
                    divCreateAccount.Visible = false;
                }

                // Show Campus?
                if ( Convert.ToBoolean( GetAttributeValue( "ShowCampuses" ) ) )
                {
                    BindCampuses();
                }

                // Require phone number?
                if ( Convert.ToBoolean( GetAttributeValue( "RequirePhone" ) ) )
                {
                    txtPhone.Required = true;
                }
            
                BindAccounts();
                BindOptions();                
            }
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
            FinancialAccountService accountService = new FinancialAccountService(); 
            FinancialTransactionDetail account;
            
            var lookupAccounts = accountService.Queryable().Where(f => f.IsActive)
                .Distinct().OrderBy(f => f.Order).ToList();

            foreach ( RepeaterItem item in rptAccountList.Items )
            {
                account = new FinancialTransactionDetail();                
                
                // TODO rewrite account lookup to use ID instead of name
                string accountName = ( (LabeledTextBox)item.FindControl("lblAccountName") ).LabelText;
                account.Account = lookupAccounts.Where(f => f.PublicName == accountName).FirstOrDefault();
                decimal amount = Decimal.Parse( ( (LabeledTextBox)item.FindControl( "txtAccountAmount" ) ).Text );
                account.Amount = amount;                
                //account.TransactionId = _transaction.Id;
                
                //detailList.Add(account);
            }
            
            account = new FinancialTransactionDetail();
            account.Account = lookupAccounts.Where( f => f.PublicName == btnAddAccount.SelectedValue ).FirstOrDefault();
            account.Amount = 0M;
            //detailList.Add( account );

            if ( btnAddAccount.Items.Count > 1 )
            {
                btnAddAccount.Items.Remove( btnAddAccount.SelectedValue );
                btnAddAccount.Title = "Add Another Gift";
            }
            else
            {
                divAddAccount.Visible = false;
            }

            //rptAccountList.DataSource = detailList.ToDictionary(f => (string)f.Account.PublicName, f => (decimal)f.Amount);
            //rptAccountList.DataBind();
        }

        /// <summary>
        /// Handles the SelectionChanged event of the btnFrequency control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnFrequency_SelectionChanged( object sender, EventArgs e )
        {
            divFrequency.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnBack_Click( object sender, EventArgs e )
        {
            pnlConfirm.Visible = false;
            pnlDetails.Visible = true;
        }

        /// <summary>
        /// Handles the CheckedChanged event of the chkLimitGifts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void chkLimitGifts_CheckedChanged( object sender, EventArgs e )
        {
            divLimitGifts.Visible = !divLimitGifts.Visible;
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
            LinkButton lb = sender as LinkButton;
            if ( lb != null )
            {
                CurrentTab = lb.Text;

                var paymentTypeGuid = new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_PAYMENT_TYPE );
                rptPaymentType.DataSource = DefinedTypeCache.Read( paymentTypeGuid ).DefinedValues;
                rptPaymentType.DataBind();
            }

            ShowSelectedTab();
        }

        /// <summary>
        /// Handles the Click event of the btnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnNext_Click( object sender, EventArgs e )
        {
            FinancialTransactionDetailService detailService = new FinancialTransactionDetailService();
            FinancialTransactionService transactionService = new FinancialTransactionService();
            FinancialAccountService accountService = new FinancialAccountService();            
            PersonService personService = new PersonService();
            FinancialTransaction transaction = new FinancialTransaction();
            Person person;

            // process person details          
            var personGroup = personService.GetByEmail(Request.Form["txtEmail"]);

            if ( personGroup.Count() > 0 )
            {
                person = personGroup.Where(p => p.FirstName == Request.Form["txtFirstName"]
                    && p.LastName == Request.Form["txtLastName"]).Distinct().FirstOrDefault();
                // TODO duplicate person handling?  ex NewAccount.ascx DisplayDuplicates()
            }
            else
            {
                person = new Person();
                personService.Add(person, CurrentPersonId);
            }

            person.Email = txtEmail.Text;
            person.GivenName = txtFirstName.Text;
            person.LastName = txtLastName.Text;
            // TODO get address

            personService.Save(person, CurrentPersonId);
            cfrmName.Text = person.FullName;

            // process gift details
            var lookupAccounts = accountService.Queryable().Where(f => f.IsActive)
                .Distinct().OrderBy(f => f.Order).ToList();

            //transaction = transactionService.Get
            if ( transaction == null )
            {
                transaction = new FinancialTransaction();
                transaction.TransactionTypeValueId = Rock.Web.Cache.DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION ).Id;
                transactionService.Add( transaction, person.Id );
            }

            foreach ( RepeaterItem item in rptAccountList.Items )
            {
                FinancialTransactionDetail account = new FinancialTransactionDetail();
                FinancialTransactionDetail detail = new FinancialTransactionDetail();

                // TODO rewrite account lookup to use ID instead of name
                var accountItem = ( (NumberBox)item.FindControl("txtAccountAmount") );
                string accountName = accountItem.LabelText;
                decimal amount = Decimal.Parse( accountItem.Text );
                detail.Amount = amount;
                account.Amount = amount;
                detail.TransactionId = transaction.Id;
                account.TransactionId = transaction.Id;
                detail.Summary = "$" + amount + " contribution to " + account.Account + " by " + person.FullName;

                detailService.Add(detail, person.Id);
                //detailList.Add(account);
            }

            transaction.AuthorizedPersonId = person.Id;
            //transaction.Amount = detailList.Sum(g => (decimal)g.Amount);
            transactionService.Save( transaction, CurrentPersonId );

            // process payment type
            if ( !string.IsNullOrEmpty(hfCardType.Value) )
            {
                litPaymentType.Text = hfCardType.Value.Split(' ').Reverse().FirstOrDefault().Substring(3);
                litPaymentType.Text = char.ToUpper(litPaymentType.Text[0]) + litPaymentType.Text.Substring(1);
                litAccountType.Text = " credit card ";
            }
            else
            {
                litPaymentType.Text = rblAccountType.SelectedValue;
                litAccountType.Text = " account ";
            }

            string lastFour = !string.IsNullOrEmpty(txtCreditCard.Text)
                ? txtCreditCard.Text
                : txtAccount.Text;

            if ( !string.IsNullOrEmpty(lastFour) )
            {
                lblPaymentLastFour.Text = lastFour.Substring(lastFour.Length - 4, 4);
            }
            else
            {
                divPaymentConfirmation.Visible = false;                
            }

            litGiftTotal.Text = transaction.Amount.ToString();

            //if ( detailList.Count == 1 )
            //{
            //    litMultiGift.Visible = false;
            //    litGiftTotal.Visible = false;
            //}
            
            //rptGiftConfirmation.DataSource = detailList.ToDictionary(f => (string)f.Account.PublicName, f => (decimal)f.Amount);
            //rptGiftConfirmation.DataBind();

            pnlDetails.Visible = false;
            pnlConfirm.Visible = true;
        }

        protected void btnGive_Click( object sender, EventArgs e )
        {
            FinancialTransactionService transactionService = new FinancialTransactionService();
            FinancialTransaction transaction = new FinancialTransaction();

            //foreach ( transaction detail) 
            //{

            //}

            transactionService.Save( transaction, CurrentPersonId );

            litDateGift.Text = DateTime.Now.ToString( "f" );
            litGiftTotal2.Text = litGiftTotal.Text;
            litPaymentType2.Text = litPaymentType.Text;
            
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
            var items = campusService.Queryable().OrderBy( a => a.Name )
                .Select( a => a.Name ).Distinct();

            if ( items.Any() )
            {
                btnCampusList.DataSource = items.ToList();
                divCampus.Visible = true;
                btnCampusList.Title = "Select Campus";
            }
        }

        /// <summary>
        /// Binds the accounts.
        /// </summary>
        protected void BindAccounts()
        {
            List<FinancialTransactionDetail> transactionList = new List<FinancialTransactionDetail>();
            FinancialAccountService accountService = new FinancialAccountService();            
            
            var selectedAccounts = accountService.Queryable().Where( f => f.IsActive );

            if ( GetAttributeValue( "DefaultAccounts" ).Any() )
            {
                var accountGuids = GetAttributeValue( "DefaultAccounts" ).Split( ',' ).Select( a => new Guid(a) );

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
                detail.Amount = 0;
                transactionList.Add( detail );
            }

            rptAccountList.DataSource = transactionList;
            rptAccountList.DataBind();
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
            
            if ( personLocation != null )
            {
                txtFirstName.Text = CurrentPerson.FirstName.ToString();
                txtLastName.Text = CurrentPerson.LastName.ToString();
                txtAddress.Text = personLocation.Street1.ToString();
                txtCity.Text = personLocation.City.ToString();
                ddlState.Text = personLocation.State.ToString();
                txtZipcode.Text = personLocation.Zip.ToString();
                txtEmail.Text = CurrentPerson.Email.ToString();
            }
        }

        /// <summary>
        /// Gets the tab class.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        protected string GetTabClass( object property )
        {
            if ( property.ToString() == CurrentTab )
            {
                return "active";
            }

            return string.Empty;
        }

        /// <summary>
        /// Binds the page control options.
        /// </summary>
        protected void BindOptions()
        {
            // bind frequency options
            var frequencyTypeGuid = new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_FREQUENCY );
            btnFrequency.BindToDefinedType( DefinedTypeCache.Read( frequencyTypeGuid ) );
            
            // bind payment types
            var paymentTypeGuid = new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_PAYMENT_TYPE );
            rptPaymentType.DataSource = DefinedTypeCache.Read( paymentTypeGuid ).DefinedValues;
            rptPaymentType.DataBind();

            // bind credit card options
            btnMonthExpiration.Items.Clear();
            btnYearExpiration.Items.Clear();

            btnMonthExpiration.DataSource = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames.ToList().GetRange(0, 12);
            btnYearExpiration.DataSource = Enumerable.Range( (DateTime.Now.Year), 10).ToList();
            
            btnMonthExpiration.DataBind();
            btnYearExpiration.DataBind();
        }

        /// <summary>
        /// Shows the selected pane.
        /// </summary>
        private void ShowSelectedTab()
        {
            if ( CurrentTab.Equals( "Credit Card" ) )
            {
                pnlCreditCard.Visible = true;
                pnlChecking.Visible = false;
                pnlCreditCard.DataBind();
            }
            else if ( CurrentTab.Equals( "Checking/ACH" ) )
            {
                pnlCreditCard.Visible = false;
                pnlChecking.Visible = true;
                pnlChecking.DataBind();
            }

            pnlPayment.DataBind();
        }

        #endregion
    }
}