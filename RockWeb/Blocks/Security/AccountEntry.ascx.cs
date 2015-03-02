// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Security
{
    /// <summary>
    /// Block for user to create a new login account.
    /// </summary>
    [DisplayName( "Account Entry" )]
    [Category( "Security" )]
    [Description( "Block allows users to create a new login account." )]

    [BooleanField( "Check for Duplicates", "Should people with the same email and last name be presented as a possible pre-existing record for user to choose from.", true, "", 0, "Duplicates" )]
    [TextField( "Found Duplicate Caption", "", false, "There are already one or more people in our system that have the same email address and last name as you do.  Are any of these people you?", "Captions", 1 )]
    [TextField( "Existing Account Caption", "", false, "{0}, you already have an existing account.  Would you like us to email you the username?", "Captions", 2 )]
    [TextField( "Sent Login Caption", "", false, "Your username has been emailed to you.  If you've forgotten your password, the email includes a link to reset your password.", "Captions", 3 )]
    [TextField( "Confirm Caption", "", false, "Because you've selected an existing person, we need to have you confirm the email address you entered belongs to you. We've sent you an email that contains a link for confirming.  Please click the link in your email to continue.", "Captions", 4 )]
    [TextField( "Success Caption", "", false, "{0}, Your account has been created", "Captions", 5 )]
    [LinkedPage( "Confirmation Page", "Page for user to confirm their account (if blank will use 'ConfirmAccount' page route)", true, "", "Pages", 6 )]
    [LinkedPage( "Login Page", "Page to navigate to when user elects to login (if blank will use 'Login' page route)", true, "", "Pages", 7 )]
    [SystemEmailField( "Forgot Username", "Forgot Username Email Template", false, Rock.SystemGuid.SystemEmail.SECURITY_FORGOT_USERNAME, "Email Templates", 8, "ForgotUsernameTemplate" )]
    [SystemEmailField( "Confirm Account", "Confirm Account Email Template", false, Rock.SystemGuid.SystemEmail.SECURITY_CONFIRM_ACCOUNT, "Email Templates", 9, "ConfirmAccountTemplate" )]
    [SystemEmailField( "Account Created", "Account Created Email Template", false, Rock.SystemGuid.SystemEmail.SECURITY_ACCOUNT_CREATED, "Email Templates", 10, "AccountCreatedTemplate" )]
    [DefinedValueField( "2E6540EA-63F0-40FE-BE50-F2A84735E600", "Connection Status", "The connection status to use for new individuals (default: 'Web Prospect'.)", true, false, "368DD475-242C-49C4-A42C-7278BE690CC2" )]
    [DefinedValueField( "8522BADD-2871-45A5-81DD-C76DA07E2E7E", "Record Status", "The record status to use for new individuals (default: 'Pending'.)", true, false, "283999EC-7346-42E3-B807-BCE9B2BABB49" )]
    public partial class AccountEntry : Rock.Web.UI.RockBlock
    {
        #region Fields

        private PlaceHolder[] PagePanels = new PlaceHolder[6];

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            lFoundDuplicateCaption.Text = GetAttributeValue( "FoundDuplicateCaption" );
            lSentLoginCaption.Text = GetAttributeValue( "SentLoginCaption" );
            lConfirmCaption.Text = GetAttributeValue( "ConfirmCaption" );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
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

        #endregion

        #region Events

        #region User Info Panel

        /// <summary>
        /// Handles the Click event of the btnUserInfoNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnUserInfoNext_Click( object sender, EventArgs e )
        {

            if ( Page.IsValid )
            {
                if ( UserLoginService.IsPasswordValid( tbPassword.Text ) )
                {
                    var userLoginService = new Rock.Model.UserLoginService( new RockContext() );
                    var userLogin = userLoginService.GetByUserName( tbUserName.Text );

                    if ( userLogin == null )
                    {
                        DisplayDuplicates( Direction.Forward );
                    }
                    else
                    {
                        ShowErrorMessage( "Username already exists" );
                    }
                }
                else
                {
                    ShowErrorMessage( UserLoginService.FriendlyPasswordRules() );
                }
            }
        }

        #endregion

        #region Duplicates Panel

        /// <summary>
        /// Handles the Click event of the btnDuplicatesPrev control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDuplicatesPrev_Click( object sender, EventArgs e )
        {
            DisplayUserInfo( Direction.Back );
        }

        /// <summary>
        /// Handles the Click event of the btnDuplicatesNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDuplicatesNext_Click( object sender, EventArgs e )
        {
            int personId = Request.Form["DuplicatePerson"].AsInteger();
            if ( personId > 0 )
            {
                var userLoginService = new Rock.Model.UserLoginService( new RockContext() );
                var userLogins = userLoginService.GetByPersonId( personId ).ToList();
                if ( userLogins.Count > 0 )
                {
                    DisplaySendLogin( personId, Direction.Forward );
                }
                else
                {
                    DisplayConfirmation( personId );
                }
            }
            else
            {
                DisplaySuccess( CreateUser( CreatePerson(), true ) );
            }
        }

        #endregion

        #region Send Login Panel

        /// <summary>
        /// Handles the Click event of the btnSendPrev control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSendPrev_Click( object sender, EventArgs e )
        {
            DisplayDuplicates( Direction.Back );
        }

        /// <summary>
        /// Handles the Click event of the btnSendYes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSendYes_Click( object sender, EventArgs e )
        {
            DisplaySentLogin( Direction.Forward );
        }

        /// <summary>
        /// Handles the Click event of the btnSendLogin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSendLogin_Click( object sender, EventArgs e )
        {
            string loginUrl = LinkedPageUrl( "LoginPage" );
            if ( string.IsNullOrWhiteSpace( loginUrl ) )
            {
                loginUrl = ResolveRockUrl( "~/Login" );
            }

            string returnUrl = Request.QueryString["returnurl"];
            if ( !string.IsNullOrWhiteSpace( returnUrl ) && !loginUrl.Contains( "returnurl" ) )
            {
                string delimiter = "?";
                if ( loginUrl.Contains( '?' ) )
                {
                    delimiter = "&";
                }

                loginUrl += delimiter + "returnurl=" + returnUrl;
            }

            Response.Redirect( loginUrl, false );
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void btnContinue_Click( object sender, EventArgs e )
        {
            string returnUrl = Request.QueryString["returnurl"];
            if ( !string.IsNullOrWhiteSpace( returnUrl ) )
            {
                Response.Redirect( Server.UrlDecode( returnUrl ), false );
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Shows the error message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void ShowErrorMessage( string message )
        {
            pnlMessage.Controls.Add( new LiteralControl( message ) );
            pnlMessage.Visible = true;
        }

        /// <summary>
        /// Displays the user information.
        /// </summary>
        /// <param name="direction">The direction.</param>
        private void DisplayUserInfo( Direction direction )
        {
            ShowPanel( 0 );
        }

        /// <summary>
        /// Displays the duplicates.
        /// </summary>
        /// <param name="direction">The direction.</param>
        private void DisplayDuplicates( Direction direction )
        {
            bool displayed = false;

            if ( Convert.ToBoolean( GetAttributeValue( "Duplicates" ) ) )
            {
                PersonService personService = new PersonService( new RockContext() );
                var matches = personService.Queryable().Where( p =>
                        p.Email.ToLower() == tbEmail.Text.ToLower() && p.LastName.ToLower() == tbLastName.Text.ToLower() ).ToList();

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
                {
                    displayed = false;
                }
            }

            if ( !displayed )
            {
                if ( direction == Direction.Forward )
                {
                    DisplaySuccess( CreateUser( CreatePerson(), true ) );
                }
                else
                {
                    DisplayUserInfo( direction );
                }
            }
        }

        /// <summary>
        /// Displays the send login.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="direction">The direction.</param>
        private void DisplaySendLogin( int personId, Direction direction )
        {
            hfSendPersonId.Value = personId.ToString();

            lExistingAccountCaption.Text = GetAttributeValue( "ExistingAccountCaption" );
            if ( lExistingAccountCaption.Text.Contains( "{0}" ) )
            {
                PersonService personService = new PersonService( new RockContext() );
                Person person = personService.Get( personId );
                if ( person != null )
                {
                    lExistingAccountCaption.Text = string.Format( lExistingAccountCaption.Text, person.FirstName );
                }
            }

            ShowPanel( 2 );
        }

        /// <summary>
        /// Displays the sent login.
        /// </summary>
        /// <param name="direction">The direction.</param>
        private void DisplaySentLogin( Direction direction )
        {
            var rockContext = new RockContext();
            PersonService personService = new PersonService( rockContext );
            Person person = personService.Get( hfSendPersonId.Value.AsInteger() );
            if ( person != null )
            {
                string url = LinkedPageUrl( "ConfirmationPage" );
                if ( string.IsNullOrWhiteSpace( url ) )
                {
                    url = ResolveRockUrl( "~/ConfirmAccount" );
                }

                var mergeObjects = GlobalAttributesCache.GetMergeFields( CurrentPerson );
                mergeObjects.Add( "ConfirmAccountUrl", RootPath + url.TrimStart( new char[] { '/' } ) );
                var results = new List<IDictionary<string, object>>();

                var users = new List<UserLogin>();
                var userLoginService = new UserLoginService( rockContext );
                foreach ( UserLogin user in userLoginService.GetByPersonId( person.Id ) )
                {
                    if ( user.EntityType != null )
                    {
                        var component = AuthenticationContainer.GetComponent( user.EntityType.Name );
                        if ( component.ServiceType == AuthenticationServiceType.Internal )
                        {
                            users.Add( user );
                        }
                    }
                }

                var resultsDictionary = new Dictionary<string, object>();
                resultsDictionary.Add( "Person", person );
                resultsDictionary.Add( "Users", users );
                results.Add( resultsDictionary );

                mergeObjects.Add( "Results", results.ToArray() );

                var recipients = new List<RecipientData>();
                recipients.Add( new RecipientData( person.Email, mergeObjects ) );

                Email.Send( GetAttributeValue( "ForgotUsernameTemplate" ).AsGuid(), recipients, ResolveRockUrl( "~/" ), ResolveRockUrl( "~~/" ) );
            }
            else
            {
                ShowErrorMessage( "Invalid Person" );
            }

            ShowPanel( 3 );
        }

        /// <summary>
        /// Displays the confirmation.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        private void DisplayConfirmation( int personId )
        {
            PersonService personService = new PersonService( new RockContext() );
            Person person = personService.Get( personId );

            if ( person != null )
            {
                Rock.Model.UserLogin user = CreateUser( person, false );

                string url = LinkedPageUrl( "ConfirmationPage" );
                if ( string.IsNullOrWhiteSpace( url ) )
                {
                    url = ResolveRockUrl( "~/ConfirmAccount" );
                }

                var mergeObjects = GlobalAttributesCache.GetMergeFields( CurrentPerson );
                mergeObjects.Add( "ConfirmAccountUrl", RootPath + url.TrimStart( new char[] { '/' } ) );
                mergeObjects.Add( "Person", person );
                mergeObjects.Add( "User", user );

                var recipients = new List<RecipientData>();
                recipients.Add( new RecipientData( person.Email, mergeObjects ) );

                Email.Send( GetAttributeValue( "ConfirmAccountTemplate" ).AsGuid(), recipients, ResolveRockUrl( "~/" ), ResolveRockUrl( "~~/" ) );

                ShowPanel( 4 );
            }
            else
            {
                ShowErrorMessage( "Invalid Person" );
            }
        }

        /// <summary>
        /// Displays the success.
        /// </summary>
        /// <param name="user">The user.</param>
        private void DisplaySuccess( Rock.Model.UserLogin user )
        {
            FormsAuthentication.SignOut();
            Rock.Security.Authorization.SetAuthCookie( tbUserName.Text, false, false );

            if ( user != null && user.PersonId.HasValue )
            {
                PersonService personService = new PersonService( new RockContext() );
                Person person = personService.Get( user.PersonId.Value );

                if ( person != null )
                {
                    try
                    {
                        string url = LinkedPageUrl( "ConfirmationPage" );
                        if ( string.IsNullOrWhiteSpace( url ) )
                        {
                            url = ResolveRockUrl( "~/ConfirmAccount" );
                        }

                        var mergeObjects = GlobalAttributesCache.GetMergeFields( CurrentPerson );
                        mergeObjects.Add( "ConfirmAccountUrl", RootPath + url.TrimStart( new char[] { '/' } ) );
                        mergeObjects.Add( "Person", person );
                        mergeObjects.Add( "User", user );

                        var recipients = new List<RecipientData>();
                        recipients.Add( new RecipientData( person.Email, mergeObjects ) );

                        Email.Send( GetAttributeValue( "AccountCreatedTemplate" ).AsGuid(), recipients, ResolveRockUrl( "~/" ), ResolveRockUrl( "~~/" ) );
                    }
                    catch ( SystemException ex )
                    {
                        ExceptionLogService.LogException( ex, Context, RockPage.PageId, RockPage.Site.Id, CurrentPersonAlias );
                    }

                    string returnUrl = Request.QueryString["returnurl"];
                    btnContinue.Visible = !string.IsNullOrWhiteSpace( returnUrl );

                    lSuccessCaption.Text = GetAttributeValue( "SuccessCaption" );
                    if ( lSuccessCaption.Text.Contains( "{0}" ) )
                    {
                        lSuccessCaption.Text = string.Format( lSuccessCaption.Text, person.FirstName );
                    }

                    ShowPanel( 5 );
                }
                else
                {
                    ShowErrorMessage( "Invalid Person" );
                }
            }
            else
            {
                ShowErrorMessage( "Invalid User" );
            }
        }

        /// <summary>
        /// Shows the panel.
        /// </summary>
        /// <param name="panel">The panel.</param>
        private void ShowPanel( int panel )
        {
            for ( int i = 0; i < PagePanels.Length; i++ )
            {
                PagePanels[i].Visible = i == panel;
            }
        }

        /// <summary>
        /// Creates the person.
        /// </summary>
        /// <returns></returns>
        private Person CreatePerson()
        {
            var rockContext = new RockContext();

            DefinedValueCache dvcConnectionStatus = DefinedValueCache.Read( GetAttributeValue( "ConnectionStatus" ).AsGuid() );
            DefinedValueCache dvcRecordStatus = DefinedValueCache.Read( GetAttributeValue( "RecordStatus" ).AsGuid() );

            Person person = new Person();
            person.FirstName = tbFirstName.Text;
            person.LastName = tbLastName.Text;
            person.Email = tbEmail.Text;
            person.IsEmailActive = true;
            person.EmailPreference = EmailPreference.EmailAllowed;
            person.RecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
            if ( dvcConnectionStatus != null )
            {
                person.ConnectionStatusValueId = dvcConnectionStatus.Id;
            }

            if ( dvcRecordStatus != null )
            {
                person.RecordStatusValueId = dvcRecordStatus.Id;
            }

            switch ( ddlGender.SelectedValue )
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

            var birthday = bdaypBirthDay.SelectedDate;
            if ( birthday.HasValue )
            {
                person.BirthMonth = birthday.Value.Month;
                person.BirthDay = birthday.Value.Day;
                if ( birthday.Value.Year != DateTime.MinValue.Year )
                {
                    person.BirthYear = birthday.Value.Year;
                }
            }

            PersonService.SaveNewPerson( person, rockContext, null, false );
            return person;
        }

        /// <summary>
        /// Creates the user.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="confirmed">if set to <c>true</c> [confirmed].</param>
        /// <returns></returns>
        private Rock.Model.UserLogin CreateUser( Person person, bool confirmed )
        {
            var rockContext = new RockContext();
            var userLoginService = new Rock.Model.UserLoginService( rockContext );
            return UserLoginService.Create( 
                rockContext, 
                person, 
                Rock.Model.AuthenticationServiceType.Internal,
                EntityTypeCache.Read( Rock.SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id,
                tbUserName.Text, 
                tbPassword.Text,
                confirmed );
        }

        #endregion
        
        #region Enumerations

        private enum Direction
        {
            Forward,
            Back
        }

        #endregion
    }
}