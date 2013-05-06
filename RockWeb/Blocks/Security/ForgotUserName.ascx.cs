//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Web.UI;
using Rock.Attribute;
using Rock.Communication;
using Rock.Model;

namespace RockWeb.Blocks.Security
{
    [TextField( "Heading Caption", "", false, "Enter your email address below and we'll send you your account user name", "Captions", 0 )]
    [TextField( "Invalid Email Caption", "", false, "There are not any accounts for the email address you entered", "Captions", 1 )]
    [TextField( "Success Caption", "", false, "Your user name has been sent to the email address you entered", "Captions", 2 )]
    public partial class ForgotUserName : Rock.Web.UI.RockBlock
    {
        #region Overridden RockPage Methods

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            pnlEntry.Visible = true;
            pnlWarning.Visible = false;
            pnlSuccess.Visible = false;

            if ( !Page.IsPostBack )
            {
                lCaption.Text = GetAttributeValue( "HeadingCaption" );
                lWarning.Text = GetAttributeValue( "InvalidEmailCaption" );
                lSuccess.Text = GetAttributeValue( "SuccessCaption" );
            }
        }

        #endregion

        #region Events

        protected void btnSend_Click( object sender, EventArgs e )
        {
            var mergeObjects = new Dictionary<string, object>();
            mergeObjects.Add( "ConfirmAccountUrl", RootPath + "ConfirmAccount" );

            var personDictionaries = new List<IDictionary<string, object>>();

            var personService = new PersonService();
            var userLoginService = new UserLoginService();

            foreach ( Person person in personService.GetByEmail( tbEmail.Text ) )
            {
                var users = new List<IDictionary<string,object>>();
                foreach ( UserLogin user in userLoginService.GetByPersonId( person.Id ) )
                {
                    if ( user.ServiceType == AuthenticationServiceType.Internal )
                    {
                        users.Add( user.ToDictionary() );
                    }
                }

                if (users.Count > 0)
                {
                    IDictionary<string,object> personDictionary = person.ToDictionary();
                    personDictionary.Add("Users", users.ToArray());
                    personDictionaries.Add( personDictionary );
                }
            }

            if ( personDictionaries.Count > 0 )
            {
                mergeObjects.Add( "Persons", personDictionaries.ToArray() );

                var recipients = new Dictionary<string, Dictionary<string, object>>();
                recipients.Add( tbEmail.Text, mergeObjects );

                Email email = new Email( Rock.SystemGuid.EmailTemplate.SECURITY_FORGOT_USERNAME );
                email.Send( recipients );

                pnlEntry.Visible = false;
                pnlSuccess.Visible = true;
            }
            else
                pnlWarning.Visible = true;
        }

        #endregion

    }
}