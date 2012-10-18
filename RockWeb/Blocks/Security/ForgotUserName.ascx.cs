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
            PersonService personService = new PersonService();

            var mergeObjects = new List<object>();

            var values = new Dictionary<string, string>();
            values.Add( "ConfirmAccountUrl", RootPath + "ConfirmAccount" );
            mergeObjects.Add( values );

            Dictionary<object, List<object>> personObjects = new Dictionary<object, List<object>>();

            foreach(Person person in personService.GetByEmail(tbEmail.Text))
            {
                var userObjects = new List<object>();

                UserService userService = new UserService();
                foreach ( User user in userService.GetByPersonId( person.Id ) )
                    if ( user.ServiceType == AuthenticationServiceType.Internal )
                        userObjects.Add( user );

                if ( userObjects.Count > 0 )
                    personObjects.Add( person, userObjects );
            }

            if ( personObjects.Count > 0 )
            {
                mergeObjects.Add( personObjects );

                var recipients = new Dictionary<string, List<object>>();
                recipients.Add( tbEmail.Text, mergeObjects );

                Email email = new Email( Rock.SystemGuid.EmailTemplate.SECURITY_FORGOT_USERNAME );
                SetSMTPParameters( email );
                email.Send( recipients );

                pnlEntry.Visible = false;
                pnlSuccess.Visible = true;
            }
            else
                pnlWarning.Visible = true;
        }

        private void SetSMTPParameters( Email email )
        {
            var globalAttributes = GlobalAttributesCache.Read();

            email.Server = globalAttributes.GetValue( "SMTPServer" );

            int port = 0;
            if ( !Int32.TryParse( globalAttributes.GetValue( "SMTPPort" ), out port ) )
                port = 0;
            email.Port = port;

            bool useSSL = false;
            if ( !bool.TryParse( globalAttributes.GetValue( "SMTPUseSSL" ), out useSSL ) )
                useSSL = false;
            email.UseSSL = useSSL;

            email.UserName = globalAttributes.GetValue( "SMTPUserName" );
            email.Password = globalAttributes.GetValue( "SMTPPassword" );
        }


        #endregion

    }
}