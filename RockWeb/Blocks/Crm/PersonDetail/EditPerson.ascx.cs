//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// The main Person Profile blockthe main information about a peron 
    /// </summary>
    public partial class EditPerson : Rock.Web.UI.PersonBlock
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rblMaritalStatus.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS ) ) );
            rblStatus.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_STATUS ) ) );
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack && Person != null )
            {
                SetValues();
            }
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            SaveValues();
            Response.Redirect( string.Format( "~/Person/{0}" + Person.Id ), false );
        }

        protected void btnCancel_Click( object sender, EventArgs e )
        {
            Response.Redirect( string.Format( "~/Person/{0}" + Person.Id ), false );
        }

        private void SetValues()
        {
            tbGivenName.Text = Person.GivenName;
            tbNickName.Text = Person.NickName;
            tbMiddleName.Text = Person.MiddleName;
            tbLastName.Text = Person.LastName;
            dpBirthDate.SelectedDate = Person.BirthDate;
            dpAnniversaryDate.SelectedDate = Person.AnniversaryDate;
            rblGender.SelectedValue = Person.Gender.ConvertToString();
            rblMaritalStatus.SelectedValue = Person.MaritalStatusValueId.ToString();
            rblStatus.SelectedValue = Person.PersonStatusValueId.ToString();
            tbEmail.Text = Person.Email;
        }

        private void SaveValues()
        {
            var service = new PersonService();
            var person = service.Get( Person.Id );

            person.GivenName = tbGivenName.Text;
            person.NickName = tbNickName.Text;
            person.MiddleName = tbMiddleName.Text;
            person.LastName = tbLastName.Text;
            person.BirthDate = dpBirthDate.SelectedDate;
            person.AnniversaryDate = dpAnniversaryDate.SelectedDate;
            person.Gender = rblMaritalStatus.SelectedValue.ConvertToEnum<Gender>();
            person.MaritalStatusValueId = int.Parse( rblMaritalStatus.SelectedValue );
            person.PersonStatusValueId = int.Parse( rblStatus.SelectedValue );
            person.Email = tbEmail.Text;

            service.Save( person, CurrentPersonId );
        }
    }
}