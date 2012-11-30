//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Web.UI;

using Rock.Cms;
using Rock.Communication;
using Rock.Crm;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Security
{
    [BlockProperty( 0, "Heading", "HeadingCaption", "Captions", "", false,"Enter your email address below and we'll send you your account user name" )]
    [BlockProperty( 1, "Invalid Email", "InvalidEmailCaption", "Captions", "", false,"There are not any accounts for the email address you entered" )]
    [BlockProperty( 2, "Success", "SuccessCaption", "Captions", "", false,"Your user name has been sent to the email address you entered" )]
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
                lCaption.Text = AttributeValue( "HeadingCaption" );
                lWarning.Text = AttributeValue( "InvalidEmailCaption" );
                lSuccess.Text = AttributeValue( "SuccessCaption" );
            }
        }

        #endregion

        #region Events

        protected void btnSend_Click( object sender, EventArgs e )
        {
            var mergeObjects = new Dictionary<string, object>();
            mergeObjects.Add( "ConfirmAccountUrl", RootPath + "ConfirmAccount" );

            var personDictionaries = new List<IDictionary<string, object>>();

            PersonService personService = new PersonService();
            UserService userService = new UserService();

            foreach ( Person person in personService.GetByEmail( tbEmail.Text ) )
            {
                var users = new List<IDictionary<string,object>>();
                foreach ( UserLogin user in userService.GetByPersonId( person.Id ) )
                {
                    if ( user.ServiceType == AuthenticationServiceType.Internal )
                    {
                        var userDictionary = new UserLoginDto( user ).ToDictionary();
                        userDictionary.Add("ConfirmationCodeEncoded", user.ConfirmationCodeEncoded);
                        users.Add(userDictionary);
                    }
                }

                if (users.Count > 0)
                {
                    IDictionary<string,object> personDictionary = new PersonDto(person).ToDictionary();
                    personDictionary.Add("FirstName", person.FirstName);
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