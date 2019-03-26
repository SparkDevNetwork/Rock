// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Security
{
    /// <summary>
    /// Block for user to create a new login account.
    /// </summary>
    [DisplayName( "Account Entry" )]
    [Category( "Security" )]
    [Description( "Block allows users to create a new login account." )]

    #region "Block Attributes"
    [BooleanField(
        "Check For Duplicates",
        Key = AttributeKeys.Duplicates,
        Description = "Should people with the same email and last name be presented as a possible pre-existing record for user to choose from.",
        DefaultBooleanValue = true,
        Order = 0 )]

    [TextField(
        "Found Duplicate Caption",
        Key = AttributeKeys.FoundDuplicateCaption,
        IsRequired = false,
        DefaultValue = "There are already one or more people in our system that have the same email address and last name as you do.  Are any of these people you?",
        Category = "Captions",
        Order = 1 )]

    [TextField(
        "Existing Account Caption",
        Key = AttributeKeys.ExistingAccountCaption,
        IsRequired = false,
        DefaultValue = "{0}, you already have an existing account.  Would you like us to email you the username?",
        Category = "Captions",
        Order = 2 )]

    [TextField(
        "Sent Login Caption",
        IsRequired = false,
        DefaultValue = "Your username has been emailed to you.  If you've forgotten your password, the email includes a link to reset your password.",
        Category = "Captions",
        Order = 3,
        Key = AttributeKeys.SentLoginCaption )]

    [TextField(
        "Confirm Caption",
        Key = AttributeKeys.ConfirmCaption,
        IsRequired = false,
        DefaultValue = "Because you've selected an existing person, we need to have you confirm the email address you entered belongs to you. We’ve sent you an email that contains a link for confirming.  Please click the link in your email to continue.",
        Category = "Captions",
        Order = 4 )]

    [TextField(
        "Success Caption",
        Key = AttributeKeys.SuccessCaption,
        IsRequired = false,
        DefaultValue = "{0}, Your account has been created",
        Category = "Captions",
        Order = 5 )]

    [LinkedPage(
        "Confirmation Page",
        Key = AttributeKeys.ConfirmationPage,
        Description = "Page for user to confirm their account (if blank will use 'ConfirmAccount' page route)",
        IsRequired = false,
        Category = "Pages",
        Order = 6 )]

    [LinkedPage(
        "Login Page",
        Key = AttributeKeys.LoginPage,
        Description = "Page to navigate to when user elects to login (if blank will use 'Login' page route)",
        IsRequired = false,
        Category = "Pages",
        Order = 7 )]

    [SystemEmailField(
        "Forgot Username",
        Key = AttributeKeys.ForgotUsernameTemplate,
        Description = "Forgot Username Email Template",
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.SystemEmail.SECURITY_FORGOT_USERNAME,
        Category = "Email Templates",
        Order = 8 )]

    [SystemEmailField(
        "Confirm Account",
        Key = AttributeKeys.ConfirmAccountTemplate,
        Description = "Confirm Account Email Template",
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.SystemEmail.SECURITY_CONFIRM_ACCOUNT,
        Category = "Email Templates",
        Order = 9 )]

    [SystemEmailField(
        "Account Created",
        Key = AttributeKeys.AccountCreatedTemplate,
        Description = "Account Created Email Template",
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.SystemEmail.SECURITY_ACCOUNT_CREATED,
        Category = "Email Templates",
        Order = 10 )]

    [DefinedValueField(
        "Connection Status",
        Key = AttributeKeys.ConnectionStatus,
        Description = "The connection status to use for new individuals (default = 'Web Prospect'.)",
        DefinedTypeGuid = "2E6540EA-63F0-40FE-BE50-F2A84735E600",
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = "368DD475-242C-49C4-A42C-7278BE690CC2",
        Order = 11 )]

    [DefinedValueField(
        "Record Status",
        Key = AttributeKeys.RecordStatus,
        Description = "The record status to use for new individuals (default = 'Pending'.)",
        DefinedTypeGuid = "8522BADD-2871-45A5-81DD-C76DA07E2E7E",
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = "283999EC-7346-42E3-B807-BCE9B2BABB49",
        Order = 12 )]

    [BooleanField(
        "Show Address",
        Key = AttributeKeys.ShowAddress,
        Description = "Allows hiding the address field.",
        DefaultBooleanValue = false,
        Order = 13 )]

    [GroupLocationTypeField(
        "Location Type",
        Key = AttributeKeys.LocationType,
        Description = "The type of location that address should use.",
        GroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY,
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME,
        Order = 14 )]

    [BooleanField(
        "Address Required",
        Key = AttributeKeys.AddressRequired,
        Description = "Whether the address is required.",
        DefaultBooleanValue = false,
        Order = 15 )]

    [BooleanField(
        "Show Phone Numbers",
        Key = AttributeKeys.ShowPhoneNumbers,
        Description = "Allows hiding the phone numbers.",
        DefaultBooleanValue = false,
        Order = 16 )]

    [IntegerField(
        "Minimum Age",
        Key = AttributeKeys.MinimumAge,
        Description = "The minimum age allowed to create an account. Warning = The Children's Online Privacy Protection Act disallows children under the age of 13 from giving out personal information without their parents' permission.",
        IsRequired = false,
        DefaultIntegerValue = 13,
        Order = 17 )]

    [DefinedValueField(
        "Phone Types",
        Key = AttributeKeys.PhoneTypes,
        Description = "The phone numbers to display for editing.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE,
        IsRequired = false,
        AllowMultiple = true,
        Order = 18 )]

    [DefinedValueField(
        "Phone Types Required",
        Key = AttributeKeys.PhoneTypesRequired,
        Description = "The phone numbers that are required.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE,
        IsRequired = false,
        AllowMultiple = true,
        Order = 19 )]

    [BooleanField(
        "Show Campus Selector",
        Key = AttributeKeys.ShowCampusSelector,
        Description = "Allows selection of primary campus.",
        DefaultBooleanValue = false,
        Order = 20 )]

    [TextField(
        "Campus Selector Label",
        Key = AttributeKeys.CampusSelectorLabel,
        Description = "The label for the campus selector (only effective when \"Show Campus Selector\" is enabled).",
        IsRequired = false,
        DefaultValue = "Campus",
        Order = 21 )]
    #endregion

    public partial class AccountEntry : Rock.Web.UI.RockBlock
    {
        protected static class AttributeKeys
        {
            public const string Duplicates = "Duplicates";
            public const string FoundDuplicateCaption = "FoundDuplicateCaption";
            public const string ExistingAccountCaption = "ExistingAccountCaption";
            public const string SentLoginCaption = "SentLoginCaption";
            public const string ConfirmCaption = "ConfirmCaption";
            public const string SuccessCaption = "SuccessCaption";
            public const string ConfirmationPage = "ConfirmationPage";
            public const string LoginPage = "LoginPage";
            public const string ForgotUsernameTemplate = "ForgotUsernameTemplate";
            public const string ConfirmAccountTemplate = "ConfirmAccountTemplate";
            public const string AccountCreatedTemplate = "AccountCreatedTemplate";
            public const string ConnectionStatus = "ConnectionStatus";
            public const string RecordStatus = "RecordStatus";
            public const string ShowAddress = "ShowAddress";
            public const string LocationType = "LocationType";
            public const string AddressRequired = "AddressRequired";
            public const string ShowPhoneNumbers = "ShowPhoneNumbers";
            public const string MinimumAge = "MinimumAge";
            public const string PhoneTypes = "PhoneTypes";
            public const string PhoneTypesRequired = "PhoneTypesRequired";
            public const string ShowCampusSelector = "ShowCampusSelector";
            public const string CampusSelectorLabel = "CampusSelectorLabel";
        }

        #region Fields

        private PlaceHolder[] PagePanels = new PlaceHolder[6];
        private List<Guid> _RequiredPhoneNumberGuids = new List<Guid>();

        #endregion

        #region Properties

        protected string Password
        {
            get { return ViewState["Password"] as string ?? string.Empty; }
            set { ViewState["Password"] = value; }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            lFoundDuplicateCaption.Text = GetAttributeValue( AttributeKeys.FoundDuplicateCaption );
            lSentLoginCaption.Text = GetAttributeValue( AttributeKeys.SentLoginCaption );
            lConfirmCaption.Text = GetAttributeValue( AttributeKeys.ConfirmCaption );
            cpCampus.Label = GetAttributeValue( AttributeKeys.CampusSelectorLabel );

            rPhoneNumbers.ItemDataBound += rPhoneNumbers_ItemDataBound;
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

                // show/hide address and phone panels
                pnlAddress.Visible = GetAttributeValue( AttributeKeys.ShowAddress ).AsBoolean();
                pnlPhoneNumbers.Visible = GetAttributeValue( AttributeKeys.ShowPhoneNumbers ).AsBoolean();
                acAddress.Required = GetAttributeValue( AttributeKeys.AddressRequired ).AsBoolean();

                // show/hide campus selector
                bool showCampus = GetAttributeValue( AttributeKeys.ShowCampusSelector ).AsBoolean();
                cpCampus.Visible = showCampus;
                if ( showCampus )
                {
                    cpCampus.Campuses = CampusCache.All( false );
                }

                // set birthday picker required if minimum age > 0
                if ( GetAttributeValue( AttributeKeys.MinimumAge ).AsInteger() > 0 )
                {
                    bdaypBirthDay.Required = true;
                }

                var phoneNumbers = new List<PhoneNumber>();

                // add phone number types
                if ( pnlPhoneNumbers.Visible )
                {
                    var phoneNumberTypeDefinedType = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE ) );

                    if ( !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKeys.PhoneTypes ) ) )
                    {
                        var selectedPhoneTypeGuids = GetAttributeValue( AttributeKeys.PhoneTypes ).Split( ',' ).Select( Guid.Parse ).ToList();
                        var selectedPhoneTypes = phoneNumberTypeDefinedType.DefinedValues
                            .Where( v => selectedPhoneTypeGuids.Contains( v.Guid ) )
                            .ToList();

                        foreach ( var phoneNumberType in selectedPhoneTypes )
                        {
                            var numberType = new DefinedValue();
                            numberType.Id = phoneNumberType.Id;
                            numberType.Value = phoneNumberType.Value;
                            numberType.Guid = phoneNumberType.Guid;

                            var phoneNumber = new PhoneNumber { NumberTypeValueId = numberType.Id, NumberTypeValue = numberType };

                            phoneNumbers.Add( phoneNumber );
                        }

                        if ( !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKeys.PhoneTypesRequired ) ) )
                        {
                            _RequiredPhoneNumberGuids = GetAttributeValue( AttributeKeys.PhoneTypesRequired ).Split( ',' ).Select( Guid.Parse ).ToList();
                        }

                        rPhoneNumbers.DataSource = phoneNumbers;
                        rPhoneNumbers.DataBind();
                    }
                    SetCurrentPersonDetails();
                }
            }
        }

        #endregion

        #region Events

        #region User Info Panel

        /// <summary>
        /// Handles the ItemDataBound event of the rPhoneNumbers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        void rPhoneNumbers_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var pnbPhone = e.Item.FindControl( "pnbPhone" ) as PhoneNumberBox;
            HtmlGenericControl phoneGroup = e.Item.FindControl( "PhoneGroup" ) as HtmlGenericControl;
            if ( pnbPhone != null )
            {
                pnbPhone.ValidationGroup = BlockValidationGroup;
                var phoneNumber = e.Item.DataItem as PhoneNumber;
                if ( phoneNumber != null )
                {
                    var isRequired = _RequiredPhoneNumberGuids.Contains( phoneNumber.NumberTypeValue.Guid );
                    pnbPhone.Required = isRequired;
                    pnbPhone.RequiredErrorMessage = string.Format( "{0} phone is required", phoneNumber.NumberTypeValue.Value );
                    if ( phoneGroup != null && isRequired )
                    {
                        phoneGroup.AddCssClass( "required" );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnUserInfoNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnUserInfoNext_Click( object sender, EventArgs e )
        {
            Password = tbPassword.Text;

            if ( Page.IsValid )
            {
                if ( !IsOldEnough() )
                {
                    ShowErrorMessage(
                        string.Format( "We are sorry, you must be at least {0} years old to create an account.",
                        GetAttributeValue( AttributeKeys.MinimumAge ) )
                    );
                    return;
                }

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
                var userLogins = userLoginService.GetByPersonId( personId )
                .ToList();

                if ( userLogins.Any( ul => !AuthenticationContainer.GetComponent( ul.EntityType.Name ).RequiresRemoteAuthentication ) )
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
            string loginUrl = LinkedPageUrl( AttributeKeys.LoginPage );
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

            if ( Convert.ToBoolean( GetAttributeValue( AttributeKeys.Duplicates ) ) )
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
        /// Fills the current person's information if it's available (e.g. passed by the person impersonation parameter)
        /// </summary>
        private void SetCurrentPersonDetails()
        {
            if ( CurrentPerson == null )
            {
                return;
            }

            tbFirstName.Text = CurrentPerson.NickName;
            tbLastName.Text = CurrentPerson.LastName;
            tbEmail.Text = CurrentPerson.Email;

            switch ( CurrentPerson.Gender )
            {
                case Gender.Male:
                    ddlGender.SelectedValue = "M";
                    break;
                case Gender.Female:
                    ddlGender.SelectedValue = "F";
                    break;
                default:
                    ddlGender.SelectedValue = "U";
                    break;
            }

            bdaypBirthDay.SelectedDate = CurrentPerson.BirthDate;

            var homeGroupTypeLocation = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME );
            var familyLocation = CurrentPerson.PrimaryFamily.GroupLocations.Where( gl => gl.IsMailingLocation && gl.GroupLocationTypeValueId == homeGroupTypeLocation.Id ).OrderBy( gl => gl.Order ).FirstOrDefault();
            if ( familyLocation != null && familyLocation.Location != null )
            {
                acAddress.Street1 = familyLocation.Location.Street1;
                acAddress.Street2 = familyLocation.Location.Street2;
                acAddress.City = familyLocation.Location.City;
                acAddress.County = familyLocation.Location.County;
                acAddress.State = familyLocation.Location.State;
                acAddress.Country = familyLocation.Location.Country;
                acAddress.PostalCode = familyLocation.Location.PostalCode;
            }

            foreach ( RepeaterItem item in rPhoneNumbers.Items )
            {
                if ( item.ItemType != ListItemType.Item )
                {
                    continue;
                }
                var phoneNumberType = ( HiddenField ) item.FindControl( "hfPhoneType" );
                var phoneNumberBox = ( PhoneNumberBox ) item.FindControl( "pnbPhone" );
                var cbSms = ( CheckBox ) item.FindControl( "cbSms" );
                var cbIsUnlisted = ( CheckBox ) item.FindControl( "cbUnlisted" );

                if ( phoneNumberBox == null || phoneNumberType == null || phoneNumberType.Value.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                var phoneNumber = CurrentPerson.PhoneNumbers.FirstOrDefault( pn => pn.NumberTypeValueId.HasValue && pn.NumberTypeValueId.Value == phoneNumberType.ValueAsInt() );
                if ( phoneNumber == null )
                {
                    continue;
                }
                phoneNumberBox.Number = phoneNumber.NumberFormatted;
                phoneNumberBox.CountryCode = phoneNumberBox.CountryCode;
                cbSms.Checked = phoneNumber.IsMessagingEnabled;
                cbIsUnlisted.Checked = phoneNumber.IsUnlisted;
            }

            bool showCampus = GetAttributeValue( AttributeKeys.ShowCampusSelector ).AsBoolean();
            if ( showCampus )
            {
                cpCampus.SetValue( CurrentPerson.GetCampus() );
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

            lExistingAccountCaption.Text = GetAttributeValue( AttributeKeys.ExistingAccountCaption );
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
                string url = LinkedPageUrl( AttributeKeys.ConfirmationPage );
                if ( string.IsNullOrWhiteSpace( url ) )
                {
                    url = ResolveRockUrl( "~/ConfirmAccount" );
                }

                var mergeObjects = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
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

                var emailMessage = new RockEmailMessage( GetAttributeValue( AttributeKeys.ForgotUsernameTemplate ).AsGuid() );
                emailMessage.AddRecipient( new RecipientData( person.Email, mergeObjects ) );
                emailMessage.AppRoot = ResolveRockUrl( "~/" );
                emailMessage.ThemeRoot = ResolveRockUrl( "~~/" );
                emailMessage.CreateCommunicationRecord = false;
                emailMessage.Send();
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

                string url = LinkedPageUrl( AttributeKeys.ConfirmationPage );
                if ( string.IsNullOrWhiteSpace( url ) )
                {
                    url = ResolveRockUrl( "~/ConfirmAccount" );
                }

                var mergeObjects = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                mergeObjects.Add( "ConfirmAccountUrl", RootPath + url.TrimStart( new char[] { '/' } ) );
                mergeObjects.Add( "Person", person );
                mergeObjects.Add( "User", user );

                var emailMessage = new RockEmailMessage( GetAttributeValue( AttributeKeys.ConfirmAccountTemplate ).AsGuid() );
                emailMessage.AddRecipient( new RecipientData( person.Email, mergeObjects ) );
                emailMessage.AppRoot = ResolveRockUrl( "~/" );
                emailMessage.ThemeRoot = ResolveRockUrl( "~~/" );
                emailMessage.CreateCommunicationRecord = false;
                emailMessage.Send();

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
            Authorization.SignOut();
            Authorization.SetAuthCookie( tbUserName.Text, false, false );

            if ( user != null && user.PersonId.HasValue )
            {
                PersonService personService = new PersonService( new RockContext() );
                Person person = personService.Get( user.PersonId.Value );

                if ( person != null )
                {
                    try
                    {
                        string url = LinkedPageUrl( AttributeKeys.ConfirmationPage );
                        if ( string.IsNullOrWhiteSpace( url ) )
                        {
                            url = ResolveRockUrl( "~/ConfirmAccount" );
                        }

                        var mergeObjects = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                        mergeObjects.Add( "ConfirmAccountUrl", RootPath + url.TrimStart( new char[] { '/' } ) );
                        mergeObjects.Add( "Person", person );
                        mergeObjects.Add( "User", user );

                        var emailMessage = new RockEmailMessage( GetAttributeValue( AttributeKeys.AccountCreatedTemplate ).AsGuid() );
                        emailMessage.AddRecipient( new RecipientData( person.Email, mergeObjects ) );
                        emailMessage.AppRoot = ResolveRockUrl( "~/" );
                        emailMessage.ThemeRoot = ResolveRockUrl( "~~/" );
                        emailMessage.CreateCommunicationRecord = false;
                        emailMessage.Send();
                    }
                    catch ( SystemException ex )
                    {
                        ExceptionLogService.LogException( ex, Context, RockPage.PageId, RockPage.Site.Id, CurrentPersonAlias );
                    }

                    string returnUrl = Request.QueryString["returnurl"];
                    btnContinue.Visible = !string.IsNullOrWhiteSpace( returnUrl );

                    lSuccessCaption.Text = GetAttributeValue( AttributeKeys.SuccessCaption );
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

            DefinedValueCache dvcConnectionStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKeys.ConnectionStatus ).AsGuid() );
            DefinedValueCache dvcRecordStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKeys.RecordStatus ).AsGuid() );

            Person person = new Person();
            person.FirstName = tbFirstName.Text;
            person.LastName = tbLastName.Text;
            person.Email = tbEmail.Text;
            person.IsEmailActive = true;
            person.EmailPreference = EmailPreference.EmailAllowed;
            person.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
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

            bool smsSelected = false;

            foreach ( RepeaterItem item in rPhoneNumbers.Items )
            {
                HiddenField hfPhoneType = item.FindControl( "hfPhoneType" ) as HiddenField;
                PhoneNumberBox pnbPhone = item.FindControl( "pnbPhone" ) as PhoneNumberBox;
                CheckBox cbUnlisted = item.FindControl( "cbUnlisted" ) as CheckBox;
                CheckBox cbSms = item.FindControl( "cbSms" ) as CheckBox;

                if ( !string.IsNullOrWhiteSpace( PhoneNumber.CleanNumber( pnbPhone.Number ) ) )
                {
                    int phoneNumberTypeId;
                    if ( int.TryParse( hfPhoneType.Value, out phoneNumberTypeId ) )
                    {
                        var phoneNumber = new PhoneNumber { NumberTypeValueId = phoneNumberTypeId };
                        person.PhoneNumbers.Add( phoneNumber );
                        phoneNumber.CountryCode = PhoneNumber.CleanNumber( pnbPhone.CountryCode );
                        phoneNumber.Number = PhoneNumber.CleanNumber( pnbPhone.Number );

                        // Only allow one number to have SMS selected
                        if ( smsSelected )
                        {
                            phoneNumber.IsMessagingEnabled = false;
                        }
                        else
                        {
                            phoneNumber.IsMessagingEnabled = cbSms.Checked;
                            smsSelected = cbSms.Checked;
                        }

                        phoneNumber.IsUnlisted = cbUnlisted.Checked;
                    }
                }
            }

            bool showCampus = GetAttributeValue( AttributeKeys.ShowCampusSelector ).AsBoolean();
            int? campusId = null;
            if ( showCampus )
            {
                campusId = cpCampus.SelectedCampusId;
            }
            PersonService.SaveNewPerson( person, rockContext, campusId, false );

            // save address
            if ( pnlAddress.Visible )
            {
                if ( acAddress.IsValid && !string.IsNullOrWhiteSpace( acAddress.Street1 ) && !string.IsNullOrWhiteSpace( acAddress.City ) && !string.IsNullOrWhiteSpace( acAddress.PostalCode ) )
                {
                    Guid locationTypeGuid = GetAttributeValue( AttributeKeys.LocationType ).AsGuid();
                    if ( locationTypeGuid != Guid.Empty )
                    {
                        Guid familyGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
                        GroupService groupService = new GroupService( rockContext );
                        GroupLocationService groupLocationService = new GroupLocationService( rockContext );
                        var family = groupService.Queryable().Where( g => g.GroupType.Guid == familyGroupTypeGuid && g.Members.Any( m => m.PersonId == person.Id ) ).FirstOrDefault();

                        var groupLocation = new GroupLocation();
                        groupLocation.GroupId = family.Id;
                        groupLocationService.Add( groupLocation );

                        var location = new LocationService( rockContext ).Get( acAddress.Street1, acAddress.Street2, acAddress.City, acAddress.State, acAddress.PostalCode, acAddress.Country );
                        groupLocation.Location = location;

                        groupLocation.GroupLocationTypeValueId = DefinedValueCache.Get( locationTypeGuid ).Id;
                        groupLocation.IsMailingLocation = true;
                        groupLocation.IsMappedLocation = true;

                        rockContext.SaveChanges();
                    }
                }
            }

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
                EntityTypeCache.Get( Rock.SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id,
                tbUserName.Text,
                Password,
                confirmed );
        }

        /// <summary>
        /// Checks to see if user meets the minimum age.
        /// </summary>
        /// <returns></returns>
        private bool IsOldEnough()
        {
            var birthday = bdaypBirthDay.SelectedDate ?? Rock.RockDateTime.Today;
            var minimumAge = GetAttributeValue( AttributeKeys.MinimumAge ).AsInteger();
            if ( minimumAge == 0 )
            {
                return true;
            }
            return Rock.RockDateTime.Today.AddYears( minimumAge * -1 ) >= birthday;
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