//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// 
    /// </summary>
    [AccountsField("Accounts", "The accounts that new pledges will be allocated toward", true, "", "", 0, "DefaultAccounts")]
    [TextField( "Legend Text", "Custom heading at the top of the form.", key: "LegendText", defaultValue: "Create a new pledge", order: 1 )]
    [LinkedPage( "Giving Page", "The page used to set up a person's giving profile.", key: "GivingPage", order: 2 )]
    [DateField( "Start Date", "Date all pledges will begin on.", key: "DefaultStartDate", order: 3 )]
    [DateField( "End Date", "Date all pledges will end.", key: "DefaultEndDate", order: 4 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_STATUS, "New User Status", "Person status to assign to a new user.", key: "DefaultPersonStatus", order: 5 )]
    public partial class CreatePledge : RockBlock
    {
        /// <summary>
        /// Gets the accounts.
        /// </summary>
        /// <value>
        /// The accounts.
        /// </value>
        private List<FinancialAccount> Accounts
        {
            get
            {
                var cacheKey = string.Format( "FinancialAccountsForPage_{0}", CurrentPage.Id );
                var accounts = GetCacheItem( cacheKey ) as List<FinancialAccount>;

                if ( accounts == null )
                {
                    int cacheDuration;
                    int.TryParse( GetAttributeValue( "CacheDuration" ), out cacheDuration );
                    var accountService = new FinancialAccountService();
                    accounts = accountService.Queryable().Where( a => AccountGuids.Contains( a.Guid ) ).ToList();
                    AddCacheItem( cacheKey, accounts, cacheDuration );
                }

                return accounts;
            }
        }

        /// <summary>
        /// Gets the account guids.
        /// </summary>
        /// <value>
        /// The account guids.
        /// </value>
        private List<Guid> AccountGuids
        {
            get
            {
                return GetAttributeValue( "DefaultAccounts" ).Split( new[] { ',' } ).Select( Guid.Parse ).ToList();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !IsPostBack )
            {
                Session.Remove( "CachedPledge" );
                lLegendText.Text = GetAttributeValue( "LegendText" );
                ShowForm();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            using ( new UnitOfWorkScope() )
            {
                RockTransactionScope.WrapTransaction( () =>
                    {
                        var pledgeService = new FinancialPledgeService();
                        var person = FindPerson();
                        var pledges = FindAndUpdatePledge( person );

                        // Does this person already have a pledge for these accounts?
                        // If so, give them the option to create a new one?
                        if ( pledgeService.Queryable().Any( p => p.PersonId == person.Id && AccountGuids.Contains( p.Guid ) ) )
                        {
                            pnlConfirm.Visible = true;
                            Session.Add( "CachedPledges", pledges );
                            return;
                        }

                        foreach ( var pledge in pledges )
                        {
                            if ( !pledge.IsValid ) 
                                continue;

                            pledgeService.Add( pledge, person.Id );
                            pledgeService.Save( pledge, person.Id );
                        }

                        // TODO: Queue up email copy of receipt to send to user
                        ShowReceipt();
                    });
            }
        }

        protected void btnConfirmYes_Click( object sender, EventArgs e )
        {
            var pledge = Session["CachedPledges"] as FinancialPledge;
            
            if ( pledge != null && pledge.IsValid )
            {
                var pledgeService = new FinancialPledgeService();
                pledgeService.Add( pledge, CurrentPersonId );
                pledgeService.Save( pledge, CurrentPersonId );
                Session.Remove( "CachedPledges" );
                ShowReceipt();
            }
        }

        protected void btnConfirmNo_Click( object sender, EventArgs e )
        {
            pnlConfirm.Visible = false;
        }

        /// <summary>
        /// Shows the view.
        /// </summary>
        private void ShowForm()
        {
            var frequencyTypeGuid = new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_PLEDGE_FREQUENCY );
            ddlFrequencyType.BindToDefinedType( DefinedTypeCache.Read( frequencyTypeGuid ) );

            if ( AccountGuids.Any() )
            {
                rptAccounts.DataSource = Accounts;
                rptAccounts.DataBind();
            }

            if ( CurrentPerson != null )
            {
                tbFirstName.Text = CurrentPerson.FirstName;
                tbLastName.Text = CurrentPerson.LastName;
                tbEmail.Text = CurrentPerson.Email;
            }

            var start = GetAttributeValue( "DefaultStartDate" );
            var end = GetAttributeValue( "DefaultEndDate" );

            if ( string.IsNullOrWhiteSpace( start ) )
                dtpStartDate.Visible = true;

            if ( string.IsNullOrWhiteSpace( end ) )
                dtpEndDate.Visible = true;

            var pledge = Session["CachedPledge"] as FinancialPledge;

            if ( pledge != null )
                pnlConfirm.Visible = true;
        }

        private void ShowReceipt()
        {
            pnlForm.Visible = false;
            pnlReceipt.Visible = true;
            btnGivingProfile.NavigateUrl = string.Format( "~/Page/{0}", GetAttributeValue( "GivingPage" ) );
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
                person = personService.GetByEmail( tbEmail.Text )
                    .FirstOrDefault( p => p.FirstName == tbFirstName.Text && p.LastName == tbLastName.Text );
            }

            if ( person == null )
            {
                var definedValue = new DefinedValueService().Get( new Guid( GetAttributeValue( "DefaultPersonStatus" ) ) );
                person = new Person
                {
                    GivenName = tbFirstName.Text,
                    LastName = tbLastName.Text,
                    Email = tbEmail.Text,
                    PersonStatusValueId = definedValue.Id,
                };

                personService.Add( person, CurrentPersonId );
                personService.Save( person, CurrentPersonId );
            }

            return person;
        }

        /// <summary>
        /// Finds the pledge.
        /// </summary>
        /// <param name="person">The Person.</param>
        /// <returns></returns>
        private IEnumerable<FinancialPledge> FindAndUpdatePledge( Person person )
        {
            var pledges = Session["CachedPledges"] as List<FinancialPledge> ?? new List<FinancialPledge>();
            
            if ( pledges.Any() )
                return pledges;

            var startSetting = GetAttributeValue( "DefaultStartDate" );
            var startDate = !string.IsNullOrWhiteSpace( startSetting ) ? DateTime.Parse( startSetting ) : DateTime.Parse( dtpStartDate.Text );
            var endSetting = GetAttributeValue( "DefaultEndDate" );
            var endDate = !string.IsNullOrWhiteSpace( endSetting ) ? DateTime.Parse( endSetting ) : DateTime.Parse( dtpEndDate.Text );
            var frequencyId = int.Parse( ddlFrequencyType.SelectedValue );

            // For some reason, this approach is not working. Account is not being found from the repeater data items...
            pledges.AddRange( from RepeaterItem item in rptAccounts.Items
                              where item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem
                              let textBox = item.FindControl( "tbAmount" ) as LabeledTextBox
                              let hiddenField = item.FindControl( "hfId" ) as HiddenField
                              where !string.IsNullOrWhiteSpace( hiddenField.Value ) && !string.IsNullOrWhiteSpace( textBox.Text )
                              select new FinancialPledge
                                  {
                                      PersonId = person.Id,
                                      AccountId = int.Parse( hiddenField.Value ),
                                      TotalAmount = decimal.Parse( textBox.Text ),
                                      StartDate = startDate,
                                      EndDate = endDate,
                                      PledgeFrequencyValueId = frequencyId
                                  } );

            return pledges;
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptAccounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptAccounts_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var account = e.Item.DataItem as FinancialAccount;
            var textbox = e.Item.FindControl( "tbAmount" ) as LabeledTextBox;
            var hiddenField = e.Item.FindControl( "hfId" ) as HiddenField;

            if ( textbox == null || hiddenField == null || account == null )
                return;

            textbox.LabelText = account.PublicName;
            hiddenField.Value = account.Id.ToString();
        }
    }
}
