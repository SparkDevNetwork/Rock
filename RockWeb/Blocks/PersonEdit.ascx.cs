using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Models.Crm;
using Rock.Services.Crm;

namespace RockWeb.Blocks
{
    public partial class PersonEdit : Rock.Cms.CmsBlock
    {
        protected void Page_Load( object sender, EventArgs e )
        {
            if ( !IsPostBack )
            {
                Person person;

                string personId = ( string )Page.RouteData.Values["PersonId"] ?? string.Empty;
                if ( string.IsNullOrEmpty( personId ) )
                    personId = Request.QueryString["PersonId"];

                PersonService personService = new PersonService();

                if ( !string.IsNullOrEmpty( personId ) )
                    person = personService.GetPerson( Convert.ToInt32( personId ) );
                else
                {
                    person = new Person();
                    personService.AddPerson( person );
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

                PersonService personService = new PersonService();

                if ( !string.IsNullOrEmpty( personId ) )
                    person = personService.GetPerson( Convert.ToInt32( personId ) );
                else
                {
                    person = new Person();
                    personService.AddPerson( person );
                }

                person.FirstName = txtFirstName.Text;
                person.NickName = txtNickName.Text;
                person.LastName = txtLastName.Text;
                if ( person.Guid == Guid.Empty )
                    personService.AddPerson( person );
                personService.Save( person, CurrentPersonId );
            }
        }
    }
}