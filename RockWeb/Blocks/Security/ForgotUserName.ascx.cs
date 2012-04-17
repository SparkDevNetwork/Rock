//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Communication;
using Rock.CMS;
using Rock.CRM;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Security
{
    [Rock.Attribute.Property( 0, "Heading", "HeadingCaption", "Captions", "", false,
        "Enter your email address below and we'll send you your account user name" )]
    [Rock.Attribute.Property( 1, "Invalid Email", "InvalidEmailCaption", "Captions", "", false,
        "There are not any accounts for the email address you entered" )]
    [Rock.Attribute.Property( 2, "Success", "SuccessCaption", "Captions", "", false,
        "Your user name has been sent to the email address you entered" )]
    public partial class ForgotUserName : Rock.Web.UI.Block
    {
        #region Overridden Page Methods

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
            PersonRepository personRepository = new PersonRepository();

            var mergeObjects = new List<object>();

            var values = new Dictionary<string, string>();
            values.Add( "ConfirmAccountUrl", RootPath + "ConfirmAccount" );
            mergeObjects.Add( values );

            Dictionary<object, List<object>> personObjects = new Dictionary<object, List<object>>();

            foreach(Person person in personRepository.GetByEmail(tbEmail.Text))
            {
                var userObjects = new List<object>();

                UserRepository userRepository = new UserRepository();
                foreach ( User user in userRepository.GetByPersonId( person.Id ) )
                    if ( user.AuthenticationType != AuthenticationType.Facebook )
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
            email.Server = GlobalAttributes.Value( "SMTPServer" );

            int port = 0;
            if ( !Int32.TryParse( GlobalAttributes.Value( "SMTPPort" ), out port ) )
                port = 0;
            email.Port = port;

            bool useSSL = false;
            if ( !bool.TryParse( GlobalAttributes.Value( "SMTPUseSSL" ), out useSSL ) )
                useSSL = false;
            email.UseSSL = useSSL;

            email.UserName = GlobalAttributes.Value( "SMTPUserName" );
            email.Password = GlobalAttributes.Value( "SMTPPassword" );
        }


        #endregion

    }
}