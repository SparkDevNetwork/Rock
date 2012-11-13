//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Cms;
using Rock.Communication;
using Rock.Crm;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Security
{
    [BlockProperty( 0, "Check for Duplicates", "Duplicates", "", "Should people with the same email and last name be presented as a possible pre-existing record for user to choose from.",false, "true", "Rock", "Rock.Field.Types.Boolean" )]
    [BlockProperty( 1, "Confirm Route", "The URL Route for Confirming an account", true)]
    [BlockProperty( 2, "Found Duplicate", "FoundDuplicateCaption", "Captions", "", false,"There are already one or more people in our system that have the same email address and last name as you do.  Are any of these people you?" )]
    [BlockProperty( 3, "Existing Account", "ExistingAccountCaption", "Captions", "", false,"{0}, you already have an existing account.  Would you like us to email you the username?" )]
    [BlockProperty( 4, "Sent Login", "SentLoginCaption", "Captions", "", false,"Your username has been emailed to you.  If you've forgotten your password, the email includes a link to reset your password." )]
    [BlockProperty( 5, "Confirm", "ConfirmCaption", "Captions", "", false,"Because you've selected an existing person, we need to have you confirm the email address you entered belongs to you. We've sent you an email that contains a link for confirming.  Please click the link in your email to continue." )]
    [BlockProperty( 6, "Success", "SuccessCaption", "Captions", "", false,"{0}, Your account has been created" )]
    public partial class NewAccount : Rock.Web.UI.RockBlock
    {
        PlaceHolder[] PagePanels = new PlaceHolder[6];

        #region Properties

        protected string Password
        {
            get
            {
                string password = ViewState["Password"] as string;
                return password ?? "";
            }
            set
            {
                ViewState["Password"] = value;
            }
        }

        protected string PasswordConfirm
        {
            get
            {
                string password = ViewState["PasswordConfirm"] as string;
                return password ?? "";
            }
            set
            {
                ViewState["PasswordConfirm"] = value;
            }
        }

        #endregion

        #region Overridden RockPage Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            
            lFoundDuplicateCaption.Text = AttributeValue( "FoundDuplicateCaption" );
            lSentLoginCaption.Text = AttributeValue( "SentLoginCaption" );
            lConfirmCaption.Text = AttributeValue( "ConfirmCaption" );
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

                //tbPassword.TextBox.TextMode = TextBoxMode.Password;
                //tbPasswordConfirm.TextBox.TextMode = TextBoxMode.Password;
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

        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            if ( tbPassword.Text == string.Empty && Password != string.Empty )
                tbPassword.Text = Password;
            if ( tbPasswordConfirm.Text == string.Empty && PasswordConfirm != string.Empty )
                tbPasswordConfirm.Text = PasswordConfirm;
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
            Password = tbPassword.Text;
            PasswordConfirm = tbPasswordConfirm.Text;

            if ( Page.IsValid )
            {
                Rock.Cms.UserService userService = new Rock.Cms.UserService();
                Rock.Cms.User user = userService.GetByUserName( tbUserName.Text );
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
                Rock.Cms.UserService userService = new Rock.Cms.UserService();
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
            Response.Redirect( "~/Login", false );
            Context.ApplicationInstance.CompleteRequest();
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
                Person person = personService.Get( Int32.Parse( hfSendPersonId.Value ) );
                if ( person != null )
                {
                    var mergeObjects = new Dictionary<string, object>();
                    mergeObjects.Add( "ConfirmAccountUrl", RootPath + "ConfirmAccount" );

                    var personDictionaries = new List<IDictionary<string, object>>();

                    var users = new List<IDictionary<string, object>>();
                    UserService userService = new UserService();
                    foreach ( User user in userService.GetByPersonId( person.Id ) )
                    {
                        if ( user.ServiceType == AuthenticationServiceType.Internal )
                        {
                            var userDictionary = new UserDto( user ).ToDictionary();
                            userDictionary.Add( "ConfirmationCodeEncoded", user.ConfirmationCodeEncoded );
                            users.Add( userDictionary );
                        }
                    }

                    if ( users.Count > 0 )
                    {
                        IDictionary<string, object> personDictionary = new PersonDto( person ).ToDictionary();
                        personDictionary.Add( "FirstName", person.FirstName );
                        personDictionary.Add( "Users", users.ToArray() );
                        personDictionaries.Add( personDictionary );
                    }

                    mergeObjects.Add( "Persons", personDictionaries.ToArray() );

                    var recipients = new Dictionary<string, Dictionary<string, object>>();
                    recipients.Add( person.Email, mergeObjects );

                    Email email = new Email( Rock.SystemGuid.EmailTemplate.SECURITY_FORGOT_USERNAME );
                    email.Send( recipients );
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
                Rock.Cms.User user = CreateUser( person, false );

                var mergeObjects = new Dictionary<string, object>();
                mergeObjects.Add( "ConfirmAccountUrl", RootPath + "ConfirmAccount" );

                var personDictionary = new PersonDto( person ).ToDictionary();
                personDictionary.Add( "FirstName", person.FirstName );
                mergeObjects.Add( "Person", personDictionary );

                mergeObjects.Add( "User", new UserDto( user ).ToDictionary() );

                var recipients = new Dictionary<string, Dictionary<string, object>>();
                recipients.Add( person.Email, mergeObjects );

                Email email = new Email( Rock.SystemGuid.EmailTemplate.SECURITY_CONFIRM_ACCOUNT );
                email.Send( recipients );

                ShowPanel( 4 );
            }
            else
                ShowErrorMessage("Invalid Person");
        }

        private void DisplaySuccess( Rock.Cms.User user )
        {
            FormsAuthentication.SignOut();
            Rock.Security.Authorization.SetAuthCookie( tbUserName.Text, false, false );

            if ( user != null && user.PersonId.HasValue )
            {
                PersonService personService = new PersonService();
                Person person = personService.Get( user.PersonId.Value );

                if ( person != null )
                {
                    var mergeObjects = new Dictionary<string, object>();
                    mergeObjects.Add( "ConfirmAccountUrl", RootPath + "ConfirmAccount" );

                    var personDictionary = new PersonDto( person ).ToDictionary();
                    personDictionary.Add("FirstName", person.FirstName);
                    mergeObjects.Add( "Person", personDictionary );

                    mergeObjects.Add( "User", new UserDto( user ).ToDictionary() );

                    var recipients = new Dictionary<string, Dictionary<string, object>>();
                    recipients.Add( person.Email, mergeObjects );

                    Email email = new Email( Rock.SystemGuid.EmailTemplate.SECURITY_ACCOUNT_CREATED );
                    email.Send( recipients );

                    lSuccessCaption.Text = AttributeValue( "SuccessCaption" );
                    if ( lSuccessCaption.Text.Contains( "{0}" ) )
                        lSuccessCaption.Text = string.Format( lSuccessCaption.Text, person.FirstName );

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

        private Person CreatePerson()
        {
            Rock.Crm.PersonService personService = new PersonService();

            Person person = new Person();
            person.GivenName = tbFirstName.Text;
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

            return person;
        }

        private Rock.Cms.User CreateUser( Person person, bool confirmed )
        {
            Rock.Cms.UserService userService = new Rock.Cms.UserService();
            return userService.Create( person, Rock.Cms.AuthenticationServiceType.Internal, "Rock.Security.Authentication.Database", tbUserName.Text, Password, confirmed, CurrentPersonId );
        }

        #endregion
    }

    enum Direction {
        Forward,
        Back
    }
}