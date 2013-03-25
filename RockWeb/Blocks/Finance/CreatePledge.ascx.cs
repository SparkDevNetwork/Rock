//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// 
    /// </summary>
    [CustomDropdownListField( "Fund", "The fund that new pledges will be allocated toward.",
        listSource: "SELECT [Id] AS 'Value', [PublicName] AS 'Text' FROM [Fund] WHERE [IsPledgable] = 1 ORDER BY [SortOrder]",
        key: "DefaultFund", required: true )]
    [TextField( "Legend Text", "Custom heading at the top of the form.", key: "LegendText", defaultValue: "Create a new pledge" )]
    [LinkedPage( "Giving Page", "The page used to set up a person's giving profile." )]
    [DateField( "Start Date", "Date all pledges will begin on.", key: "DefaultStartDate" )]
    [DateField( "End Date", "Date all pledges will end.", key: "DefaultEndDate" )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_STATUS, "New User Status", "Person status to assign to a new user.", key: "DefaultPersonStatus" )]
    public partial class CreatePledge : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !IsPostBack )
            {
                lLegendText.Text = GetAttributeValue( "LegendText" );
                ShowView();
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
                        var pledgeService = new PledgeService();
                        var person = FindPerson();
                        var pledge = FindPledge( pledgeService, person );

                        // Does this person already have a pledge for this fund?
                        // If so, give them the option to create a new one?
                        if ( pledge.Id != 0 )
                        {
                            // Show UI control for creating a pledge...
                            // Consider caching newly created pledge?
                            return;
                        }

                        if ( pledge.IsValid )
                        {
                            if ( pledge.Id == 0 )
                            {
                                pledgeService.Add( pledge, person.Id );
                            }

                            pledgeService.Save( pledge, person.Id );
                        }
                    });
            }

            // Redirect to linked page?
        }

        /// <summary>
        /// Shows the view.
        /// </summary>
        private void ShowView()
        {
            var frequencyTypeGuid = new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_FREQUENCY_TYPE );
            ddlFrequencyType.BindToDefinedType( DefinedTypeCache.Read( frequencyTypeGuid ) );

            if ( CurrentPerson != null )
            {
                tbFirstName.Text = !string.IsNullOrWhiteSpace( CurrentPerson.NickName ) ? CurrentPerson.NickName : CurrentPerson.FirstName;
                tbLastName.Text = CurrentPerson.LastName;
                tbEmail.Text = CurrentPerson.Email;
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
                    Pledges = new List<Pledge>()
                };

                personService.Add( person, CurrentPersonId );
                personService.Save( person, CurrentPersonId );
            }

            return person;
        }

        /// <summary>
        /// Finds the pledge.
        /// </summary>
        /// <param name="pledgeService">The pledge service.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        private Pledge FindPledge( PledgeService pledgeService, Person person )
        {
            var defaultFundId = int.Parse( GetAttributeValue( "DefaultFund" ) );
            var pledge = person.Pledges.FirstOrDefault( p => p.FundId == defaultFundId );

            if ( pledge != null )
            {
                return pledge;
            }
            
            pledge = new Pledge
            {
                PersonId = person.Id,
                FundId = defaultFundId,
                Amount = decimal.Parse( tbAmount.Text ),
                StartDate = DateTime.Parse( GetAttributeValue( "DefaultStartDate" ) ),
                EndDate = DateTime.Parse( GetAttributeValue( "DefaultEndDate" ) ),
                FrequencyTypeValueId = int.Parse( ddlFrequencyType.SelectedValue ),
                FrequencyAmount = decimal.Parse( tbFrequencyAmount.Text )
            };

            return pledge;
        }
    }
}
