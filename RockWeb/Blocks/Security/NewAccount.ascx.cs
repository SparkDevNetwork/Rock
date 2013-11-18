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
using Rock.Constants;
using Rock.Attribute;
using Rock.Communication;
using Rock.Model;

namespace RockWeb.Blocks.Security
{
    [BooleanField( "Check for Duplicates", "Should people with the same email and last name be presented as a possible pre-existing record for user to choose from.", true, "", 0, "Duplicates")]
    [TextField( "Confirm Route", "The URL Route for Confirming an account", true)]
    [TextField( "Found Duplicate Caption", "", false,"There are already one or more people in our system that have the same email address and last name as you do.  Are any of these people you?", "Captions", 0 )]
    [TextField( "Existing Account Caption", "", false, "{0}, you already have an existing account.  Would you like us to email you the username?", "Captions", 1 )]
    [TextField( "Sent Login Caption", "", false, "Your username has been emailed to you.  If you've forgotten your password, the email includes a link to reset your password.", "Captions", 2 )]
    [TextField( "Confirm Caption", "", false, "Because you've selected an existing person, we need to have you confirm the email address you entered belongs to you. We've sent you an email that contains a link for confirming.  Please click the link in your email to continue.", "Captions", 3 )]
    [TextField( "Success Caption", "", false, "{0}, Your account has been created", "Captions", 4 )]
    [LinkedPage("Login Page")]
    public partial class NewAccount : Rock.Web.UI.RockBlock
    {
        PlaceHolder[] PagePanels = new PlaceHolder[6];
        string loginUrl = string.Empty;

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

            loginUrl = GetAttributeValue( "LoginPage" );
            loginUrl = string.IsNullOrWhiteSpace( loginUrl ) ? "Login" : "Page/" + loginUrl.Trim();

            lFoundDuplicateCaption.Text = GetAttributeValue( "FoundDuplicateCaption" );
            lSentLoginCaption.Text = GetAttributeValue( "SentLoginCaption" );
            lConfirmCaption.Text = GetAttributeValue( "ConfirmCaption" );
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

        protected void btnUserInfoNext_Click( object sender, EventArgs e )
        {
            Password = tbPassword.Text;
            PasswordConfirm = tbPasswordConfirm.Text;

            if ( Page.IsValid )
            {
                var userLoginService = new Rock.Model.UserLoginService();
                var userLogin = userLoginService.GetByUserName( tbUserName.Text );
                if ( userLogin == null )
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
                var userLoginService = new Rock.Model.UserLoginService();
                var userLogins = userLoginService.GetByPersonId(personId).ToList();
                if (userLogins.Count > 0)
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
            Response.Redirect( "~/" + loginUrl, false );
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

            if ( Convert.ToBoolean( GetAttributeValue( "Duplicates" ) ) )
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

            lExistingAccountCaption.Text = GetAttributeValue( "ExistingAccountCaption" );
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
                    var userLoginService = new UserLoginService();
                    foreach ( UserLogin user in userLoginService.GetByPersonId( person.Id ) )
                    {
                        if ( user.ServiceType == AuthenticationServiceType.Internal )
                        {
                            var userDictionary = user.ToDictionary();
                            users.Add( userDictionary );
                        }
                    }

                    if ( users.Count > 0 )
                    {
                        IDictionary<string, object> personDictionary = person.ToDictionary();
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
                Rock.Model.UserLogin user = CreateUser( person, false );

                var mergeObjects = new Dictionary<string, object>();
                mergeObjects.Add( "ConfirmAccountUrl", RootPath + "ConfirmAccount" );

                var personDictionary = person.ToDictionary();
                mergeObjects.Add( "Person", personDictionary );

                mergeObjects.Add( "User", user.ToDictionary() );

                var recipients = new Dictionary<string, Dictionary<string, object>>();
                recipients.Add( person.Email, mergeObjects );

                Email email = new Email( Rock.SystemGuid.EmailTemplate.SECURITY_CONFIRM_ACCOUNT );
                email.Send( recipients );

                ShowPanel( 4 );
            }
            else
                ShowErrorMessage("Invalid Person");
        }

        private void DisplaySuccess( Rock.Model.UserLogin user )
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

                    var personDictionary = person.ToDictionary();
                    mergeObjects.Add( "Person", personDictionary );

                    mergeObjects.Add( "User", user.ToDictionary() );

                    var recipients = new Dictionary<string, Dictionary<string, object>>();
                    recipients.Add( person.Email, mergeObjects );

                    Email email = new Email( Rock.SystemGuid.EmailTemplate.SECURITY_ACCOUNT_CREATED );
                    email.Send( recipients );

                    lSuccessCaption.Text = GetAttributeValue( "SuccessCaption" );
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

        private void ShowPanel( int panel )
        {
            for ( int i = 0; i < PagePanels.Length; i++ )
                PagePanels[i].Visible = i == panel;
        }

        private Person CreatePerson()
        {
            Rock.Model.PersonService personService = new PersonService();

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

            var birthday = bdpBirthDay.SelectedDate;
            if ( birthday.HasValue )
            {
                person.BirthMonth = birthday.Value.Month;
                person.BirthDay = birthday.Value.Day;
                if ( birthday.Value.Year != DateTime.MinValue.Year )
                {
                    person.BirthYear = birthday.Value.Year;
                }
            }

            personService.Add(person, CurrentPersonId);
            personService.Save(person, CurrentPersonId);

            return person;
        }

        private Rock.Model.UserLogin CreateUser( Person person, bool confirmed )
        {
            var userLoginService = new Rock.Model.UserLoginService();
            return userLoginService.Create( person, Rock.Model.AuthenticationServiceType.Internal, "Rock.Security.Authentication.Database", tbUserName.Text, Password, confirmed, CurrentPersonId );
        }

        #endregion
    }

    enum Direction {
        Forward,
        Back
    }
}