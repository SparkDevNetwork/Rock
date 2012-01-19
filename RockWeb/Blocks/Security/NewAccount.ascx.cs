//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;

using Rock.Communication;
using Rock.CRM;

namespace RockWeb.Blocks.Security
{
    [Rock.Attribute.Property( 0, "Check for Duplicates", "Duplicates", "", 
        "Should people with the same email and last name be presented as a possible pre-existing record for user to choose from.",
        false, "true", "Rock", "Rock.FieldTypes.Boolean" )]
    [Rock.Attribute.Property( 1, "Found Duplicate", "FoundDuplicateCaption", "Captions", "", false,
        "There are already one or more people in our system that have the same email address and last name as you do.  Are any of these people you?" )]
    [Rock.Attribute.Property( 2, "Existing Account", "ExistingAccountCaption", "Captions", "", false,
        "{0}, you already have an existing account.  Would you like us to email you the username?" )]
    [Rock.Attribute.Property( 3, "Sent Login", "SentLoginCaption", "Captions", "", false,
        "Your username has been emailed to you.  If you've forgotten your password, the email includes a link to reset your password." )]
    [Rock.Attribute.Property( 4, "Confirm", "ConfirmCaption", "Captions", "", false,
        "Because you've selected an existing person, we need to have you confirm the email address you entered belongs to you. We've sent you an email that contains a link for confirming.  Please click the link in your email to continue." )]
    [Rock.Attribute.Property( 5, "Success", "SuccessCaption", "Captions", "", false,
        "Account has been created" )]
    public partial class NewAccount : Rock.Web.UI.Block
    {
        PlaceHolder[] PagePanels = new PlaceHolder[6];

        #region Overridden Page Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            lFoundDuplicateCaption.Text = AttributeValue( "FoundDuplicateCaption" );
            lSentLoginCaption.Text = AttributeValue( "SentLoginCaption" );
            lConfirmCaption.Text = AttributeValue( "ConfirmCaption" );
            lSuccessCaption.Text = AttributeValue( "SuccessCaption" );
        }

        protected override void OnLoad( System.EventArgs e )
        {
            base.OnLoad( e );

            pnlMessage.Controls.Clear();
            pnlMessage.Visible = false;

            PagePanels[0] = phUserInfo;
            PagePanels[1] = phDuplicates;
            PagePanels[2] = phSendLoginInfo;
            PagePanels[3] = phSentLoginInfo;
            PagePanels[4] = phConfirmation;
            PagePanels[5] = phSuccess;

            if ( !Page.IsPostBack )
            {

                tbPassword.TextBox.TextMode = TextBoxMode.Password;
                tbPasswordConfirm.TextBox.TextMode = TextBoxMode.Password;
                LoadBirthDays();

                int year = DateTime.Now.Year;
                for ( int i = 0; i <= 110; i++ )
                {
                    string yearStr = ( year - i ).ToString();
                    ddlBirthYear.Items.Add( new ListItem( yearStr, yearStr ) );
                }

                DisplayUserInfo( Direction.Forward );
            }
        }

        #endregion

        #region Events

        #region User Info Panel

        protected void ddlBirthMonth_IndexChanged( object sender, EventArgs e )
        {
            LoadBirthDays();
        }

        protected void btnUserInfoNext_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                Rock.CMS.UserService userService = new Rock.CMS.UserService();
                Rock.CMS.User user = userService.GetByApplicationNameAndUsername( "RockChMS", tbUserName.Text );
                if ( user == null )
                    DisplayDuplicates( Direction.Forward );
                else
                    ShowErrorMessage( "Username already exists" );
            }
        }

        #endregion

        #region Duplicates Panel

        protected void btnDuplicatesPrev_Click( object sender, EventArgs e )
        {
            DisplayUserInfo( Direction.Back );
        }

        protected void btnDuplicatesNext_Click( object sender, EventArgs e )
        {
            int personId = Int32.Parse( Request.Form["DuplicatePerson"] );
            if ( personId > 0 )
            {
                Rock.CMS.UserService userService = new Rock.CMS.UserService();
                var users = userService.GetByPersonId(personId).ToList();
                if (users.Count > 0)
                    DisplaySendLogin( personId, Direction.Forward );
                else
                    DisplayConfirmation( personId );
            }                        
            else
            {
                DisplaySuccess( CreateUser( CreatePerson(), true ) );
            }
        }

        #endregion

        #region Send Login Panel

        protected void btnSendPrev_Click( object sender, EventArgs e )
        {
            DisplayDuplicates( Direction.Back );
        }

        protected void btnSendYes_Click( object sender, EventArgs e )
        {
            DisplaySentLogin( Direction.Forward );
        }

        protected void btnSendLogin_Click( object sender, EventArgs e )
        {
            Response.Redirect( "~/Login", true );
        }

        #endregion

        #endregion

        #region Private Methods

        private void ShowErrorMessage( string message )
        {
            pnlMessage.Controls.Add( new LiteralControl( message ) );
            pnlMessage.Visible = true;
        }

        private void DisplayUserInfo( Direction direction )
        {
            ShowPanel( 0 );
        }

        private void DisplayDuplicates( Direction direction )
        {
            bool displayed = false;

            if ( Convert.ToBoolean( AttributeValue( "Duplicates" ) ) )
            {
                PersonService personService = new PersonService();
                var matches = personService.
                    Queryable().
                    Where( p =>
                        p.Email.ToLower() == tbEmail.Text.ToLower() &&
                        p.LastName.ToLower() == tbLastName.Text.ToLower() ).
                    ToList();

                if ( matches.Count > 0 )
                {
                    gDuplicates.AllowPaging = false;
                    gDuplicates.ShowActionRow = false;

                    gDuplicates.DataSource = matches;
                    gDuplicates.DataBind();

                    ShowPanel( 1 );

                    displayed = true;
                }
                else
                    displayed = false;

            }

            if ( !displayed )
            {
                if ( direction == Direction.Forward )
                    DisplaySuccess( CreateUser (CreatePerson(), true));
                else
                    DisplayUserInfo( direction );
            }
        }

        private void DisplaySendLogin( int personId, Direction direction )
        {
            hfSendPersonId.Value = personId.ToString();

            lExistingAccountCaption.Text = AttributeValue( "ExistingAccountCaption" );
            if ( lExistingAccountCaption.Text.Contains( "{0}" ) )
            {
                PersonService personService = new PersonService();
                Person person = personService.Get( personId );
                if ( person != null )
                    lExistingAccountCaption.Text = string.Format( lExistingAccountCaption.Text, person.FirstName );
            }

            ShowPanel( 2 );
        }

        private void DisplaySentLogin( Direction direction )
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                PersonService personService = new PersonService();
                Rock.CMS.UserService userService = new Rock.CMS.UserService();

                Person person = personService.Get( Int32.Parse( hfSendPersonId.Value ) );
                if ( person != null )
                {
                    StringBuilder sb = new StringBuilder();
                    foreach ( var user in userService.GetByPersonId( person.Id ) )
                        sb.AppendLine( user.Username );

                    Dictionary<string, string> mergeValues = new Dictionary<string,string>();
                    mergeValues.Add( "FirstName", person.FirstName );
                    mergeValues.Add( "LastName", person.LastName );
                    mergeValues.Add( "Usernames", sb.ToString() );

                    Dictionary<string, Dictionary<string, string>> recipientInfo = new Dictionary<string,Dictionary<string,string>>();
                    recipientInfo.Add(person.Email, mergeValues);

                    Email email = new Email( SystemEmailTemplate.SECURITY_FORGOT_USERNAME );
                    email.Send( recipientInfo );
                }
                else
                    ShowErrorMessage( "Invalid Person" );
            }

            ShowPanel( 3 );
        }

        private void DisplayConfirmation( int personId )
        {
            PersonService personService = new PersonService();
            Person person = personService.Get(personId);

            if (person != null)
            {
                Rock.CMS.User user = CreateUser( personId, false );

                string identifier = string.Format( "ROCK|{0}|{1}",
                    user.Guid.ToString(), user.Username );

                string encryptionPhrase = ConfigurationManager.AppSettings["EncryptionPhrase"];
                if ( String.IsNullOrWhiteSpace( encryptionPhrase ) )
                    encryptionPhrase = "Rock Rocks!";

                string confirmationCode = Rock.Security.Encryption.EncryptString( identifier, encryptionPhrase );

                Dictionary<string, string> mergeValues = new Dictionary<string, string>();
                mergeValues.Add( "FirstName", person.FirstName );
                mergeValues.Add( "LastName", person.LastName );
                mergeValues.Add( "ConfirmationCode", confirmationCode );

                Dictionary<string, Dictionary<string, string>> recipientInfo = new Dictionary<string, Dictionary<string, string>>();
                recipientInfo.Add( person.Email, mergeValues );

                Email email = new Email( SystemEmailTemplate.SECURITY_CONFIRM_ACCOUNT );
                email.Send( recipientInfo );

                ShowPanel( 4 );
            }
            else
                ShowErrorMessage("Invalid Person");
        }

        private void DisplaySuccess( Rock.CMS.User user )
        {
            FormsAuthenticationTicket tkt;
            string cookiestr;
            HttpCookie ck;
            tkt = new FormsAuthenticationTicket( 1, tbUserName.Text, DateTime.Now, DateTime.Now.AddMinutes( 30 ), false, "your custom data" );
            cookiestr = FormsAuthentication.Encrypt( tkt );
            ck = new HttpCookie( FormsAuthentication.FormsCookieName, cookiestr );
            ck.Path = FormsAuthentication.FormsCookiePath;
            Response.Cookies.Add( ck );

            if ( user != null && user.PersonId.HasValue )
            {
                PersonService personService = new PersonService();
                Person person = personService.Get( user.PersonId.Value );

                if ( person != null )
                {
                    Dictionary<string, string> mergeValues = new Dictionary<string, string>();
                    mergeValues.Add( "FirstName", person.FirstName );
                    mergeValues.Add( "LastName", person.LastName );

                    Dictionary<string, Dictionary<string, string>> recipientInfo = new Dictionary<string, Dictionary<string, string>>();
                    recipientInfo.Add( person.Email, mergeValues );

                    Email email = new Email( SystemEmailTemplate.SECURITY_ACCOUNT_CREATED );
                    email.Send( recipientInfo );

                    ShowPanel( 5 );
                }
                else
                    ShowErrorMessage( "Invalid Person" );
            }
            else
                ShowErrorMessage( "Invalid User" );
        }

        private void LoadBirthDays()
        {
            int currentMonth = 0;
            if ( ddlBirthMonth.SelectedIndex >= 0 )
                currentMonth = Int32.Parse( ddlBirthMonth.SelectedValue);

            int currentDay = 0;
            if ( ddlBirthDay.SelectedIndex >= 0 )
                currentDay = Int32.Parse( ddlBirthDay.SelectedValue );

            int[] days = new int[13] { 31, 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

            ddlBirthDay.Items.Clear();
            ddlBirthDay.Items.Add( new ListItem( "Day", "0" ) );
            for ( int i = 1; i <= days[currentMonth]; i++ )
                ddlBirthDay.Items.Add( new ListItem( i.ToString(), i.ToString() ) );

            if ( currentDay <= days[currentMonth] )
                ddlBirthDay.SelectedValue = currentDay.ToString();
        }

        private void ShowPanel( int panel )
        {
            for ( int i = 0; i < PagePanels.Length; i++ )
                PagePanels[i].Visible = i == panel;
        }

        private int CreatePerson()
        {
            Rock.CRM.PersonService personService = new PersonService();

            Person person = new Person();
            person.FirstName = tbFirstName.Text;
            person.LastName = tbLastName.Text;
            person.Email = tbEmail.Text;
            switch(ddlGender.SelectedValue)
            {
                case "M":
                    person.Gender = Gender.Male;
                    break;
                case "F":
                    person.Gender = Gender.Female;
                    break;
                default:
                    person.Gender = Gender.Unknown;
                    break;
            }

            if (ddlBirthMonth.SelectedValue != "0")
                person.BirthMonth = Int32.Parse(ddlBirthMonth.SelectedValue);

            if (ddlBirthDay.SelectedValue != "0")
                person.BirthDay = Int32.Parse(ddlBirthDay.SelectedValue);

            if (ddlBirthYear.SelectedValue != "0")
                person.BirthYear = Int32.Parse(ddlBirthYear.SelectedValue);

            personService.Add(person, CurrentPersonId);
            personService.Save(person, CurrentPersonId);

            return person.Id;
        }

        private Rock.CMS.User CreateUser( int personId, bool confirmed )
        {
            Rock.CMS.UserService userService = new Rock.CMS.UserService();

            Rock.CMS.User user = new Rock.CMS.User();
            user.PersonId = personId;
            user.AuthenticationType = 1;
            user.Username = tbUserName.Text;
            user.Password = tbPassword.Text;
            user.ApplicationName = "RockChMS";
            user.IsLockedOut = !confirmed;

            userService.Add( user, CurrentPersonId );
            userService.Save( user, CurrentPersonId );
            
            return user;
        }

        #endregion
    }

    enum Direction {
        Forward,
        Back
    }
}