//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Web.UI;

using Rock.Crm;

namespace RockWeb.Blocks
{
    public partial class PersonEdit : Rock.Web.UI.RockBlock
    {
        private Person person = null;

        /// <summary>
        /// Gets a list of any context entities that the block requires.
        /// </summary>
        public override List<string> RequiredContext
        {
            get { return new List<string>() { "Rock.Crm.Person" }; }
        }

        protected void Page_Load( object sender, EventArgs e )
        {
            Person person = CurrentPage.GetCurrentContext( "Rock.Crm.Person" ) as Rock.Crm.Person;
            if (person == null)
            {
                PersonService personService = new PersonService();
                person = new Person();
                personService.Add( person, CurrentPersonId );
            }

            if ( !IsPostBack )
            {
                txtFirstName.Text = person.FirstName;
                txtNickName.Text = person.NickName;
                txtLastName.Text = person.LastName;
            }
        }

        protected void btnUpdate_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid && person != null)
            {
                PersonService personService = new PersonService();

                person.GivenName = txtFirstName.Text;
                person.NickName = txtNickName.Text;
                person.LastName = txtLastName.Text;
                
                if ( person.Guid == Guid.Empty )
                    personService.Add( person, CurrentPersonId );
                
                personService.Save( person, CurrentPersonId );
            }
        }
    }
}