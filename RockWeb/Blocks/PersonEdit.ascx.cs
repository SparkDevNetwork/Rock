//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Web.UI;

using Rock.CRM;

namespace RockWeb.Blocks
{
    public partial class PersonEdit : Rock.Web.UI.Block
    {
        protected void Page_Load( object sender, EventArgs e )
        {
            if ( !IsPostBack )
            {
                Person person;

                string personId = ( string )Page.RouteData.Values["PersonId"] ?? string.Empty;
                if ( string.IsNullOrEmpty( personId ) )
                    personId = Request.QueryString["PersonId"];

                PersonRepository personRepository = new PersonRepository();

                if ( !string.IsNullOrEmpty( personId ) )
                    person = personRepository.Get( Convert.ToInt32( personId ) );
                else
                {
                    person = new Person();
                    personRepository.Add( person, CurrentPersonId );
                }

                txtFirstName.Text = person.FirstName;
                txtNickName.Text = person.NickName;
                txtLastName.Text = person.LastName;
            }
        }

        protected void btnUpdate_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                Person person;

                string personId = ( string )Page.RouteData.Values["PersonId"] ?? string.Empty;
                if ( string.IsNullOrEmpty( personId ) )
                    personId = Request.QueryString["PersonId"];

                PersonRepository personRepository = new PersonRepository();

                if ( !string.IsNullOrEmpty( personId ) )
                    person = personRepository.Get( Convert.ToInt32( personId ) );
                else
                {
                    person = new Person();
                    personRepository.Add( person, CurrentPersonId );
                }

                person.GivenName = txtFirstName.Text;
                person.NickName = txtNickName.Text;
                person.LastName = txtLastName.Text;
                if ( person.Guid == Guid.Empty )
                    personRepository.Add( person, CurrentPersonId );
                personRepository.Save( person, CurrentPersonId );
            }
        }
    }
}