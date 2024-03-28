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
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Enums.Blocks.Security.AccountEntry;
using Rock.Model;
using Rock.Security;
using Rock.Security.Authentication;
using Rock.Security.Authentication.Passwordless;
using Rock.ViewModels.Blocks.Security.AccountEntry;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Security
{
    /// <summary>
    /// Allows the user to register.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />
    [DisplayName( "Account Entry" )]
    [Category( "Security" )]
    [Description( "Allows the user to register." )]
    [IconCssClass( "fa fa-user-lock" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [BooleanField(
        "Require Email For Username",
        Key = AttributeKey.RequireEmailForUsername,
        Description = "When enabled the label on the Username will be changed to Email and the field will validate to ensure that the input is formatted as an email.",
        DefaultBooleanValue = false,
        Order = 0 )]

    [TextField(
        "Username Field Label",
        Key = AttributeKey.UsernameFieldLabel,
        Description = "The label to use for the username field.  For example, this allows an organization to customize it to 'Username / Email' in cases where both are supported.",
        IsRequired = false,
        DefaultValue = "Username",
        Order = 1 )]

    [BooleanField(
        "Check For Duplicates",
        Key = AttributeKey.Duplicates,
        Description = "Should people with the same email and last name be presented as a possible pre-existing record for user to choose from.",
        DefaultBooleanValue = true,
        Order = 2 )]

    [TextField(
        "Found Duplicate Caption",
        Key = AttributeKey.FoundDuplicateCaption,
        IsRequired = false,
        DefaultValue = "There are already one or more people in our system that have the same email address and last name as you do.  Are any of these people you?",
        Category = "Captions",
        Order = 3 )]

    [TextField(
        "Existing Account Caption",
        Key = AttributeKey.ExistingAccountCaption,
        IsRequired = false,
        DefaultValue = "{0}, you already have an existing account.  Would you like us to email you the username?",
        Category = "Captions",
        Order = 4 )]

    [TextField(
        "Sent Login Caption",
        IsRequired = false,
        DefaultValue = "Your username has been emailed to you.  If you've forgotten your password, the email includes a link to reset your password.",
        Category = "Captions",
        Order = 5,
        Key = AttributeKey.SentLoginCaption )]

    [TextField(
        "Confirm Caption",
        Key = AttributeKey.ConfirmCaption,
        IsRequired = false,
        DefaultValue = "Because you've selected an existing person, we need to have you confirm the email address you entered belongs to you. We've sent you an email that contains a link for confirming.  Please click the link in your email to continue.",
        Category = "Captions",
        Order = 6 )]

    [TextField(
        "Success Caption",
        Key = AttributeKey.SuccessCaption,
        IsRequired = false,
        DefaultValue = "{0}, Your account has been created",
        Category = "Captions",
        Order = 7 )]

    [LinkedPage(
        "Confirmation Page",
        Key = AttributeKey.ConfirmationPage,
        Description = "Page for user to confirm their account (if blank will use 'ConfirmAccount' page route)",
        IsRequired = false,
        Category = "Pages",
        Order = 8 )]

    [LinkedPage(
        "Login Page",
        Key = AttributeKey.LoginPage,
        Description = "Page to navigate to when a user elects to log in (if blank will use 'Login' page route)",
        IsRequired = false,
        Category = "Pages",
        Order = 9 )]

    [SystemCommunicationField(
        "Forgot Username",
        Key = AttributeKey.ForgotUsernameTemplate,
        Description = "Forgot Username Email Template",
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.SystemCommunication.SECURITY_FORGOT_USERNAME,
        Category = "Email Templates",
        Order = 10 )]

    [SystemCommunicationField(
        "Confirm Account",
        Key = AttributeKey.ConfirmAccountTemplate,
        Description = "Confirm Account Email Template",
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.SystemCommunication.SECURITY_CONFIRM_ACCOUNT,
        Category = "Email Templates",
        Order = 11 )]

    [SystemCommunicationField(
        "Account Created",
        Key = AttributeKey.AccountCreatedTemplate,
        Description = "Account Created Email Template",
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.SystemCommunication.SECURITY_ACCOUNT_CREATED,
        Category = "Email Templates",
        Order = 12 )]

    [DefinedValueField(
        "Connection Status",
        Key = AttributeKey.ConnectionStatus,
        Description = "The connection status to use for new individuals (default = 'Prospect'.)",
        DefinedTypeGuid = "2E6540EA-63F0-40FE-BE50-F2A84735E600",
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PROSPECT,
        Order = 13 )]

    [DefinedValueField(
        "Record Status",
        Key = AttributeKey.RecordStatus,
        Description = "The record status to use for new individuals (default = 'Pending'.)",
        DefinedTypeGuid = "8522BADD-2871-45A5-81DD-C76DA07E2E7E",
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = "283999EC-7346-42E3-B807-BCE9B2BABB49",
        Order = 14 )]

    [BooleanField(
        "Show Address",
        Key = AttributeKey.ShowAddress,
        Description = "Allows showing the address field.",
        DefaultBooleanValue = false,
        Order = 15 )]

    [GroupLocationTypeField(
        "Location Type",
        Key = AttributeKey.LocationType,
        Description = "The type of location that address should use.",
        GroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY,
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME,
        Order = 16 )]

    [BooleanField(
        "Address Required",
        Key = AttributeKey.AddressRequired,
        Description = "Whether the address is required.",
        DefaultBooleanValue = false,
        Order = 17 )]

    [BooleanField(
        "Show Phone Numbers",
        Key = AttributeKey.ShowPhoneNumbers,
        Description = "Allows showing the phone numbers.",
        DefaultBooleanValue = false,
        Order = 18 )]

    [IntegerField(
        "Minimum Age",
        Key = AttributeKey.MinimumAge,
        Description = "The minimum age allowed to create an account. Warning = The Children's Online Privacy Protection Act disallows children under the age of 13 from giving out personal information without their parents' permission.",
        IsRequired = false,
        DefaultIntegerValue = 13,
        Order = 19 )]

    [DefinedValueField(
        "Phone Types",
        Key = AttributeKey.PhoneTypes,
        Description = "The phone numbers to display for editing.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE,
        IsRequired = false,
        AllowMultiple = true,
        Order = 20 )]

    [DefinedValueField(
        "Phone Types Required",
        Key = AttributeKey.PhoneTypesRequired,
        Description = "The phone numbers that are required.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE,
        IsRequired = false,
        AllowMultiple = true,
        Order = 21 )]

    [BooleanField(
        "Show Campus",
        Key = AttributeKey.ShowCampusSelector,
        Description = "Allows selection of primary a campus. If there is only one active campus then the campus field will not show.",
        DefaultBooleanValue = false,
        Order = 22 )]

    [TextField(
        "Campus Selector Label",
        Key = AttributeKey.CampusSelectorLabel,
        Description = "The label for the campus selector (only effective when \"Show Campus Selector\" is enabled).",
        IsRequired = false,
        DefaultValue = "Campus",
        Order = 23 )]

    [BooleanField( "Save Communication History",
        Key = AttributeKey.CreateCommunicationRecord,
        Description = "Should a record of communication from this block be saved to the recipient's profile?",
        DefaultBooleanValue = false,
        ControlType = Rock.Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Order = 24 )]

    [BooleanField(
        "Show Gender",
        Key = AttributeKey.ShowGender,
        Description = "Determines if the gender selection field should be shown.",
        DefaultBooleanValue = true,
        Order = 25 )]

    [AttributeCategoryField(
        "Attribute Categories",
        Key = AttributeKey.AttributeCategories,
        Description = "The Attribute Categories to display attributes from.",
        AllowMultiple = true,
        EntityTypeName = "Rock.Model.Person",
        IsRequired = false,
        Order = 26 )]

    [BooleanField(
        "Disable Username Availability Checking",
        Key = AttributeKey.DisableUsernameAvailabilityCheck,
        Description = "Disables username availability checking.",
        DefaultBooleanValue = false,
        Order = 27 )]

    [SystemCommunicationField(
        "Confirm Account (Passwordless)",
        Key = AttributeKey.ConfirmAccountPasswordlessTemplate,
        Description = "Confirm Account (Passwordless) Email Template",
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.SystemCommunication.SECURITY_CONFIRM_ACCOUNT_PASSWORDLESS,
        Category = "Email Templates",
        Order = 28 )]

    [TextField(
        "Confirm Caption (Passwordless)",
        Key = AttributeKey.ConfirmCaptionPasswordless,
        IsRequired = false,
        DefaultValue = "Because you've selected an existing person, we need to have you confirm the email address you entered belongs to you. We’ve sent you an email that contains a code for confirming.  Please enter the code from your email to continue.",
        Category = "Captions",
        Order = 29 )]

    [BooleanField(
        "Disable Captcha Support",
        Key = AttributeKey.DisableCaptchaSupport,
        Description = "If set to 'Yes' the CAPTCHA verification step will not be performed.",
        DefaultBooleanValue = false,
        Order = 30 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "75704274-FDB8-4A0C-AE0E-510F1977BE0A" )]
    [Rock.SystemGuid.BlockTypeGuid( "E5C34503-DDAD-4881-8463-0E1E20B1675D" )]
    public class AccountEntry : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string RequireEmailForUsername = "RequireEmailForUsername";
            public const string UsernameFieldLabel = "UsernameFieldLabel";
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
            public const string CreateCommunicationRecord = "CreateCommunicationRecord";
            public const string ShowGender = "ShowGender";
            public const string AttributeCategories = "AttributeCategories";
            public const string DisableUsernameAvailabilityCheck = "DisableUsernameAvailabilityCheck";
            public const string ConfirmAccountPasswordlessTemplate = "ConfirmAccountPasswordlessTemplate";
            public const string ConfirmCaptionPasswordless = "ConfirmCaptionPasswordless";
            public const string DisableCaptchaSupport = "DisableCaptchaSupport";
        }

        private static class PageParameterKey
        {
            public const string Status = "status";
            public const string State = "State";
            public const string AreUsernameAndPasswordRequired = "AreUsernameAndPasswordRequired";
            public const string ReturnUrl = "returnurl";
        }

        #endregion

        #region IRockObsidianBlockType Implementation

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return GetInitializationBox();
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Sends the individual a forgot username email with database authentication usernames if they select a duplicate person who already has a database login.
        /// </summary>
        /// <param name="bag">The forgot username request bag.</param>
        [BlockAction]
        public BlockActionResult ForgotUsername( AccountEntryForgotUsernameRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var person = GetSelectedDuplicatePerson( bag.PersonId, bag.Email, bag.LastName, rockContext );
                if ( person == null )
                {
                    return ActionOk();
                }

                var users = new List<UserLogin>();
                var userLoginService = new UserLoginService( rockContext );
                foreach ( UserLogin user in userLoginService.GetByPersonId( person.Id ).Where( u => u.EntityType != null ) )
                {
                    var component = AuthenticationContainer.GetComponent( user.EntityType.Name );
                    if ( component.ServiceType == AuthenticationServiceType.Internal )
                    {
                        users.Add( user );
                    }
                }

                var url = this.GetLinkedPageUrl( AttributeKey.ConfirmationPage );
                if ( string.IsNullOrWhiteSpace( url ) )
                {
                    url = "/ConfirmAccount";
                }

                var mergeObjects = GetMergeFields(
                    null,
                    new Dictionary<string, object>
                    {
                        { "ConfirmAccountUrl", this.RequestContext.RootUrlPath + url },
                        { "Results",
                            new Dictionary<string, object>[]
                            {
                                new Dictionary<string, object>
                                {
                                    { "Person", person },
                                    { "Users", users },
                                }
                            }
                        }
                    } );

                var emailMessage = new RockEmailMessage( GetAttributeValue( AttributeKey.ForgotUsernameTemplate ).AsGuid() )
                {
                    AppRoot = "/",
                    ThemeRoot = this.RequestContext.ResolveRockUrl( "~~/" ),
                    CreateCommunicationRecord = GetAttributeValue( AttributeKey.CreateCommunicationRecord ).AsBoolean()
                };
                emailMessage.AddRecipient( new RockEmailMessageRecipient( person, mergeObjects ) );
                emailMessage.Send();

                return ActionOk();
            }
        }

        /// <summary>
        /// Registers a new account.
        /// </summary>
        /// <param name="box">The register request box.</param>
        [BlockAction]
        public BlockActionResult Register( AccountEntryRegisterRequestBox box )
        {
            var disableCaptcha = GetAttributeValue( AttributeKey.DisableCaptchaSupport ).AsBoolean();

            if ( !disableCaptcha && !RequestContext.IsCaptchaValid )
            {
                return ActionBadRequest( "Captcha was not valid." );
            }

            using ( var rockContext = new RockContext() )
            {
                var config = GetInitializationBox( box.State );

                if ( !IsRequestValid( box, config, rockContext, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                if ( box.SelectedPersonId > 0 )
                {
                    // The individual elected to create new account for existing person.
                    var person = GetSelectedDuplicatePerson( box.SelectedPersonId, box.PersonInfo.Email, box.PersonInfo.LastName, rockContext );
                    return RegisterExistingPerson( person, box, config, rockContext );
                }

                if ( box.SelectedPersonId == 0 )
                {
                    // The individual elected to create new account even though there are people who match the supplied person info.
                    return RegisterNewPerson( box, config, rockContext );
                }

                var duplicatePeople = GetDuplicatePeople( box.SelectedPersonId, box.PersonInfo.Email, box.PersonInfo.LastName, rockContext );

                if ( !duplicatePeople.Any() )
                {
                    // The supplied person info does not match any existing people so create a new account.
                    return RegisterNewPerson( box, config, rockContext );
                }

                // Show the duplicates to the individual to select from.
                return ActionOk( new AccountEntryRegisterResponseBox
                {
                    Step = AccountEntryStep.DuplicatePersonSelection,
                    DuplicatePersonSelectionStepBag = new AccountEntryDuplicatePersonSelectionStepBag
                    {
                        DuplicatePeople = GetDuplicatePersonItemBags( box, duplicatePeople ),
                        Caption = GetAttributeValue( AttributeKey.FoundDuplicateCaption )
                    }
                } );
            }
        }

        #endregion

        #region Private Methods

        private List<AccountEntryDuplicatePersonItemBag> GetDuplicatePersonItemBags( AccountEntryRegisterRequestBox box, List<Person> duplicatePeople )
        {
            var items = new List<AccountEntryDuplicatePersonItemBag>();

            var enteredFirstName = box.PersonInfo?.FirstName;
            var enteredLastName = box.PersonInfo?.LastName;

            foreach ( var duplicatePerson in duplicatePeople )
            {
                var isFirstNameMatching = string.Equals( enteredFirstName, duplicatePerson.FirstName, StringComparison.InvariantCultureIgnoreCase );
                var isLastNameMatching = string.Equals( enteredLastName, duplicatePerson.LastName, StringComparison.InvariantCultureIgnoreCase );
                var title = duplicatePerson.TitleValueId.HasValue ? $"{DefinedValueCache.GetValue( duplicatePerson.TitleValueId )} " : string.Empty;
                var suffix = duplicatePerson.SuffixValueId.HasValue ? $" {DefinedValueCache.GetValue( duplicatePerson.SuffixValueId )}" : string.Empty;

                string safeFirstName;
                string safeLastName;
                if ( duplicatePerson.TitleValueId.HasValue || duplicatePerson.SuffixValueId.HasValue )
                {
                    // Since the registering person doesn't set a title or suffix during registration,
                    // and the matched person has at least one of those values,
                    // the matched person's first name should be obscured and the last name should be shown only if it matches.
                    safeFirstName = Obscure( duplicatePerson.FirstName );
                    safeLastName = isLastNameMatching ? duplicatePerson.LastName : Obscure( duplicatePerson.LastName );
                }
                else
                {
                    safeFirstName = isFirstNameMatching ? duplicatePerson.FirstName : Obscure( duplicatePerson.FirstName );
                    safeLastName = isLastNameMatching ? duplicatePerson.LastName : Obscure( duplicatePerson.LastName );
                }

                items.Add( new AccountEntryDuplicatePersonItemBag
                {
                    Id = duplicatePerson.Id,
                    FullName = $"{title}{safeFirstName} {safeLastName}{suffix}"
                } );
            }

            return items;
        }

        /// <summary>
        /// Authenticates the individual associated with the <paramref name="userLogin"/>.
        /// </summary>
        /// <param name="userLogin">The user login to authenticate.</param>
        private void AuthenticateUser( UserLogin userLogin )
        {
            UserLoginService.UpdateLastLogin( userLogin.UserName );
            var securitySettings = new SecuritySettingsService().SecuritySettings;

            // 2FA: An individual is authenticated after registering for a new person
            // or an existing person with a user confirmed account
            // or a brand new account. Mark the auth ticket as two-factor authenticated.
            Authorization.SetAuthCookie(
                userLogin.UserName,
                isPersisted: true,
                isImpersonated: false,
                isTwoFactorAuthenticated: true,
                TimeSpan.FromMinutes( securitySettings.PasswordlessSignInSessionDuration ) );
        }

        /// <summary>
        /// Determines if a <paramref name="person"/> can use database authentication.
        /// </summary>
        /// <param name="person">The person to check.</param>
        /// <param name="rockContext">The Rock context.</param>
        /// <returns><c>true</c> if the person can use database authentication; otherwise, <c>false</c>.</returns>
        private bool CanPersonAuthenticateWithExistingUserLogin( Person person, RockContext rockContext )
        {
            var userLoginService = new UserLoginService( rockContext );
            var userLogins = userLoginService.GetByPersonId( person.Id )
                .Include( p => p.EntityType )
                .Where( l => l.IsLockedOut != true )
                .ToList();

            var passwordlessGuid = SystemGuid.EntityType.AUTHENTICATION_PASSWORDLESS.AsGuid();
            return userLogins.Any( ul =>
            {
                var component = AuthenticationContainer.GetComponent( ul.EntityType.Name );
                return !component.RequiresRemoteAuthentication && component.EntityType.Guid != passwordlessGuid;
            } );
        }

        /// <summary>
        /// Creates a passwordless user login.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="username">The user login username.</param>
        /// <param name="rockContext">The context.</param>
        /// <returns>The created user login.</returns>
        private UserLogin CreatePasswordlessUserLogin( Person person, string username, RockContext rockContext )
        {
            /*
                10/19/2023 - JMH

                The individual is registering as a result of a passwordless login for a new email/mobile phone.
                Since their email/mobile phone was already confirmed by using passwordless login,
                the new UserLogin should be marked as confirmed.

                Reason: Passwordless Login
             */
            return UserLoginService.Create(
                rockContext,
                person,
                AuthenticationServiceType.External,
                EntityTypeCache.Get( typeof( PasswordlessAuthentication ) ).Id,
                username,
                null,
                isConfirmed: true );
        }

        /// <summary>
        /// Creates a <see cref="Person"/>.
        /// </summary>
        /// <param name="box">The register request box.</param>
        /// <param name="config">The block initialization box.</param>
        /// <param name="rockContext">The Rock context.</param>
        /// <returns>The created person.</returns>
        private Person CreatePerson( AccountEntryRegisterRequestBox box, AccountEntryInitializationBox config, RockContext rockContext )
        {
            var person = new Person
            {
                FirstName = box.PersonInfo.FirstName,
                LastName = box.PersonInfo.LastName,
                Email = box.PersonInfo.Email,
                IsEmailActive = true,
                EmailPreference = EmailPreference.EmailAllowed,
                RecordTypeValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id,
                Gender = box.PersonInfo.Gender ?? Gender.Unknown,
                ConnectionStatusValueId = DefinedValueCache.Get( GetAttributeValue( AttributeKey.ConnectionStatus ).AsGuid() )?.Id,
                RecordStatusValueId = DefinedValueCache.Get( GetAttributeValue( AttributeKey.RecordStatus ).AsGuid() )?.Id
            };

            var birthday = box.PersonInfo.Birthday;
            if ( birthday != null )
            {
                person.BirthMonth = birthday.Month;
                person.BirthDay = birthday.Day;
                if ( birthday.Year != DateTime.MinValue.Year )
                {
                    person.BirthYear = birthday.Year;
                }
            }

            var isSmsNumberAssigned = false;

            if ( box.PersonInfo?.PhoneNumbers != null )
            {
                foreach ( var item in box.PersonInfo.PhoneNumbers )
                {
                    var cleanNumber = PhoneNumber.CleanNumber( item.PhoneNumber );

                    if ( cleanNumber.IsNullOrWhiteSpace() )
                    {
                        continue;
                    }

                    var phoneNumber = new PhoneNumber
                    {
                        NumberTypeValueId = DefinedValueCache.Get( item.Guid ).Id,
                        Number = cleanNumber,
                        IsUnlisted = item.IsUnlisted,
                        IsMessagingEnabled = item.IsSmsEnabled && !isSmsNumberAssigned,
                        CountryCode = PhoneNumber.CleanNumber( item.CountryCode )
                    };

                    // Only allow one number to have SMS enabled.
                    isSmsNumberAssigned = isSmsNumberAssigned || phoneNumber.IsMessagingEnabled;

                    person.PhoneNumbers.Add( phoneNumber );
                }
            }

            int? campusId = null;
            if ( config.IsCampusPickerShown && box.PersonInfo.Campus.HasValue )
            {
                campusId = CampusCache.GetId( box.PersonInfo.Campus.Value );
            }

            PersonService.SaveNewPerson( person, rockContext, campusId, true );

            // Save address
            var address = box.PersonInfo.Address;
            if ( config.IsAddressShown
                 && address != null
                 && address.Street1.IsNotNullOrWhiteSpace()
                 && address.City.IsNotNullOrWhiteSpace()
                 && address.PostalCode.IsNotNullOrWhiteSpace() )
            {
                var locationTypeGuid = GetAttributeValue( AttributeKey.LocationType ).AsGuid();
                if ( locationTypeGuid != Guid.Empty )
                {
                    var familyGroupTypeGuid = SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
                    var groupService = new GroupService( rockContext );
                    var groupLocationService = new GroupLocationService( rockContext );
                    var locationService = new LocationService( rockContext );

                    var family = groupService.Queryable()
                        .Where( g => g.GroupType.Guid == familyGroupTypeGuid )
                        .Where( g => g.Members.Any( m => m.PersonId == person.Id ) )
                        .FirstOrDefault();

                    var location = locationService.Get( address.Street1, address.Street2, address.City, address.State, address.PostalCode, address.Country );
                    var groupLocation = new GroupLocation
                    {
                        GroupId = family.Id,
                        Location = location,
                        GroupLocationTypeValueId = DefinedValueCache.Get( locationTypeGuid ).Id,
                        IsMailingLocation = true,
                        IsMappedLocation = true
                    };
                    groupLocationService.Add( groupLocation );

                    rockContext.SaveChanges();
                }
            }

            // Save any attribute values
            person.LoadAttributes( rockContext );
            var personAttributes = GetAttributeCategoryAttributes( rockContext );
            person.SetPublicAttributeValues(
                box.PersonInfo.AttributeValues,
                person,
                // Do not enforce security; otherwise, some attribute values may not be set for unauthenticated users.
                enforceSecurity: false,
                attributeFilter: a1 => personAttributes.Any( a => a.Guid == a1.Guid ) );
            person.SaveAttributeValues( rockContext );

            return person;
        }

        /// <summary>
        /// Creates a user login.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="isConfirmed">Whether the user login is confirmed.</param>
        /// <param name="username">The user login username.</param>
        /// <param name="password">The user login password.</param>
        /// <param name="rockContext">The context.</param>
        /// <returns>The created user login.</returns>
        private UserLogin CreateDatabaseUserLogin( Person person, bool isConfirmed, string username, string password, RockContext rockContext )
        {
            return UserLoginService.Create(
                rockContext,
                person,
                AuthenticationServiceType.Internal,
                EntityTypeCache.Get( SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id,
                username,
                password,
                isConfirmed );
        }

        /// <summary>
        /// Tries to finalize a passwordless authentication session if one exists.
        /// </summary>
        /// <param name="person">The registered person.</param>
        /// <param name="remoteAuthenticationSession">The remote authentication session.</param>
        /// <param name="rockContext">The context.</param>
        /// <returns><c>true</c> if a passwordless authentication session was finalized; otherwise, <c>false</c>.</returns>
        private void FinalizePasswordlessAuthentication( Person person, RemoteAuthenticationSession remoteAuthenticationSession, RockContext rockContext )
        {
            var remoteAuthenticationSessionService = new RemoteAuthenticationSessionService( rockContext );
            remoteAuthenticationSessionService.CompleteRemoteAuthenticationSession( remoteAuthenticationSession, person.PrimaryAliasId.Value );
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Gets the attributes for the specified attribute categories.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The attributes for the specified attribute categories.</returns>
        private List<AttributeCache> GetAttributeCategoryAttributes( RockContext rockContext )
        {
            var attributeService = new AttributeService( rockContext );
            var attributes = new List<AttributeCache>();

            foreach ( var categoryGuid in this.GetAttributeValues( AttributeKey.AttributeCategories ).AsGuidList() )
            {
                var category = CategoryCache.Get( categoryGuid );

                if ( category != null )
                {
                    foreach ( var attribute in attributeService.GetByCategoryId( category.Id, false ) )
                    {
                        if ( !attributes.Any( a => a.Guid == attribute.Guid ) )
                        {
                            attributes.Add( AttributeCache.Get( attribute ) );
                        }
                    }
                }
            }

            return attributes;
        }

        /// <summary>
        /// Gets the list of <see cref="Person"/> records that match the specified criteria.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="email">The person's email address.</param>
        /// <param name="lastName">The person's last name.</param>
        /// <param name="rockContext">The Rock context</param>
        /// <returns>The list of matching people.</returns>
        private List<Person> GetDuplicatePeople( int? personId, string email, string lastName, RockContext rockContext )
        {
            var personService = new PersonService( rockContext );
            var people = GetDuplicatePeopleQuery( personId, email, lastName, personService )
                .AsNoTracking()
                .ToList();

            return people
                // Remove duplicates that share the same name, age, and family role.
                .GroupBy( p => new
                {
                    p.TitleValueId,
                    FirstishName = p.NickName + p.FirstName,
                    p.LastName,
                    p.SuffixValueId,
                    p.Age,
                    Role = p.GetFamilyRole()?.Id
                } )
                .Select( g => g.OrderBy( p => p.Id ).First() )

                // Remove duplicates that have the same title, nickname, and suffix.
                .GroupBy( p => new
                {
                    p.TitleValueId,
                    p.NickName,
                    p.SuffixValueId,
                } )
                .Select( g => g.OrderBy( p => p.Id ).First() )
                // Remove duplicates that have the same title, first name, and suffix.
                .GroupBy( p => new
                {
                    p.TitleValueId,
                    p.FirstName,
                    p.SuffixValueId,
                } )
                .Select( g => g.OrderBy( p => p.Id ).First() )
                .ToList();
        }

        /// <summary>
        /// Gets the queryable of <see cref="Person"/> records that match the specified criteria.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="email">The person's email address.</param>
        /// <param name="lastName">The person's last name.</param>
        /// <param name="personService">The person service.</param>
        /// <returns>The queryable or matching people.</returns>
        private IQueryable<Person> GetDuplicatePeopleQuery( int? personId, string email, string lastName, PersonService personService )
        {
            if ( !GetAttributeValue( AttributeKey.Duplicates ).AsBoolean() )
            {
                return Enumerable.Empty<Person>().AsQueryable();
            }

            // Skip duplicate check if user opted to create new account.
            if ( personId == 0 )
            {
                return Enumerable.Empty<Person>().AsQueryable();
            }

            return personService.Queryable()
                .Where( p => p.Email.ToLower() == email.ToLower() )
                .Where( p => p.LastName.ToLower() == lastName.ToLower() );
        }

        /// <summary>
        /// Gets the existing account step caption.
        /// </summary>
        /// <param name="person">The person for whom the caption will be personalized.</param>
        /// <returns>The existing account step caption.</returns>
        private string GetExistingAccountCaption( Person person )
        {
            var caption = GetAttributeValue( AttributeKey.ExistingAccountCaption );

            if ( caption.Contains( "{0}" ) )
            {
                return string.Format( caption, person.FirstName );
            }

            return caption;
        }

        /// <summary>
        /// Gets the block's initialization box.
        /// </summary>
        /// <param name="encryptedStateOverride">The encrypted passwordless state override. If not specified, the encrypted passwordless state is retrieved from page parameters.</param>
        /// <returns>The initialization box.</returns>
        private AccountEntryInitializationBox GetInitializationBox( string encryptedStateOverride = null )
        {
            // Automatically set the phone number or email if this user is coming from the passwordless login flow.
            var passwordlessLoginStateString = encryptedStateOverride ?? Uri.UnescapeDataString( PageParameter( PageParameterKey.State ) );
            var passwordlessLoginState = PasswordlessAuthentication.GetDecryptedAuthenticationState( passwordlessLoginStateString );
            var currentPerson = GetCurrentPerson();

            var showPhoneNumbers = GetAttributeValue( AttributeKey.ShowPhoneNumbers ).AsBoolean();
            var requiredPhoneTypes = GetAttributeValue( AttributeKey.PhoneTypesRequired )
                .Split( ',' )
                .Where( guidString => guidString.IsNotNullOrWhiteSpace() )
                .Select( Guid.Parse )
                .ToList();

            var selectedPhoneTypeGuids = GetAttributeValue( AttributeKey.PhoneTypes )
                .Split( ',' )
                .Where( guidString => guidString.IsNotNullOrWhiteSpace() )
                .Select( Guid.Parse )
                .ToList();

            var knownNumbers = new Dictionary<Guid, string>();
            if ( passwordlessLoginState != null )
            {
                knownNumbers.Add( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid(), passwordlessLoginState.PhoneNumber );
            }
            else if ( currentPerson != null && showPhoneNumbers )
            {
                foreach ( var phoneNumber in currentPerson.PhoneNumbers )
                {
                    knownNumbers.Add( phoneNumber.NumberTypeValue.Guid, phoneNumber.Number );
                }
            }

            var phoneNumberTypeDefinedType = DefinedTypeCache.Get( SystemGuid.DefinedType.PERSON_PHONE_TYPE.AsGuid() );

            var phoneNumberBags = phoneNumberTypeDefinedType.DefinedValues
                .Where( v => selectedPhoneTypeGuids.Contains( v.Guid ) || knownNumbers.ContainsKey( v.Guid ) )
                .Select( v => new AccountEntryPhoneNumberBag
                {
                    Guid = v.Guid,
                    IsHidden = passwordlessLoginState != null && knownNumbers.ContainsKey( v.Guid ) && knownNumbers[v.Guid].IsNotNullOrWhiteSpace(),
                    IsRequired = requiredPhoneTypes.Contains( v.Guid ) || ( passwordlessLoginState != null && knownNumbers.ContainsKey( v.Guid ) ),
                    IsSmsEnabled = false,
                    IsUnlisted = false,
                    Label = v.Value,
                    PhoneNumber = knownNumbers.GetValueOrDefault( v.Guid, null )
                } )
                .ToList();

            var isEmailRequiredForUsername = GetAttributeValue( AttributeKey.RequireEmailForUsername ).AsBoolean();

            var accountEntryRegisterStepBox = new AccountEntryRegisterResponseBox
            {
                Step = AccountEntryStep.Registration
            };

            if ( PageParameter( PageParameterKey.Status ).ToLower() == "success" && currentPerson != null )
            {
                accountEntryRegisterStepBox = new AccountEntryRegisterResponseBox()
                {
                    Step = AccountEntryStep.Completed,
                    CompletedStepBag = new AccountEntryCompletedStepBag()
                    {
                        Caption = GetSuccessCaption( currentPerson ),
                        IsPlainCaption = true,
                        IsRedirectAutomatic = true,
                    }
                };
            }

            var areUsernameAndPasswordRequired = PageParameter( PageParameterKey.AreUsernameAndPasswordRequired ).AsBoolean();

            // Use an empty Person if none is available.
            // We should always include an AccountEntryPersonInfoBag.
            // The Obsidian block expects any configured attributes
            // (in addition to other config values like phones & addresses)
            // to be set in the AccountEntryPersonInfoBag.
            if ( currentPerson == null )
            {
                currentPerson = new Person();
            }

            if ( showPhoneNumbers )
            {
                foreach ( var bag in phoneNumberBags )
                {
                    var phoneNumber = currentPerson.PhoneNumbers.FirstOrDefault( x => x.Number == bag.PhoneNumber );

                    if ( phoneNumber != null )
                    {
                        bag.PhoneNumber = phoneNumber.Number;
                        bag.IsSmsEnabled = phoneNumber.IsMessagingEnabled;
                        bag.IsUnlisted = phoneNumber.IsUnlisted;
                    }
                }
            }

            var accountEntryPersonInfoBag = new AccountEntryPersonInfoBag
            {
                FirstName = currentPerson.FirstName,
                Gender = currentPerson.Gender,
                Campus = currentPerson.PrimaryCampus?.Guid,
                Email = currentPerson.Email,
                LastName = currentPerson.LastName,
                PhoneNumbers = phoneNumberBags
            };

            if ( currentPerson.BirthDate.HasValue )
            {
                accountEntryPersonInfoBag.Birthday = new ViewModels.Controls.BirthdayPickerBag()
                {
                    Day = currentPerson.BirthDate.Value.Day,
                    Month = currentPerson.BirthDate.Value.Month,
                    Year = currentPerson.BirthDate.Value.Year,
                };
            }

            var homeAddress = currentPerson.GetHomeLocation();
            if ( homeAddress != null )
            {
                accountEntryPersonInfoBag.Address = new ViewModels.Controls.AddressControlBag
                {
                    Street1 = homeAddress.Street1,
                    Street2 = homeAddress.Street2,
                    City = homeAddress.City,
                    State = homeAddress.State,
                    PostalCode = homeAddress.PostalCode,
                    Country = homeAddress.Country
                };
            }

            using ( var rockContext = new RockContext() )
            {
                var personAttributes = GetAttributeCategoryAttributes( rockContext );

                // Load the attributes for the current person if possible.
                currentPerson.LoadAttributes( rockContext );

                accountEntryPersonInfoBag = accountEntryPersonInfoBag ?? new AccountEntryPersonInfoBag();
                accountEntryPersonInfoBag.Attributes = currentPerson.GetPublicAttributesForEdit( currentPerson, attributeFilter: a1 => personAttributes.Any( a => a.Guid == a1.Guid ), enforceSecurity: false );
                accountEntryPersonInfoBag.AttributeValues = currentPerson.GetPublicAttributeValuesForEdit( currentPerson, attributeFilter: a1 => personAttributes.Any( a => a.Guid == a1.Guid ), enforceSecurity: false );
            }

            return new AccountEntryInitializationBox
            {
                ArePhoneNumbersShown = GetAttributeValue( AttributeKey.ShowPhoneNumbers ).AsBoolean(),
                CampusPickerLabel = GetAttributeValue( AttributeKey.CampusSelectorLabel ),
                ConfirmationSentCaption = GetAttributeValue( AttributeKey.ConfirmCaption ),
                Email = passwordlessLoginState?.Email,
                ExistingAccountCaption = GetAttributeValue( AttributeKey.ExistingAccountCaption ),
                // Account info (username and password) should only be hidden if registering through the passwordless
                // authentication flow AND if username and password are not required.
                IsAccountInfoHidden = passwordlessLoginState != null && !areUsernameAndPasswordRequired,
                IsAddressRequired = GetAttributeValue( AttributeKey.AddressRequired ).AsBoolean(),
                IsAddressShown = GetAttributeValue( AttributeKey.ShowAddress ).AsBoolean(),
                IsCampusPickerShown = GetAttributeValue( AttributeKey.ShowCampusSelector ).AsBoolean(),
                IsEmailHidden = ( passwordlessLoginState?.Email.IsNotNullOrWhiteSpace() ?? false ) || isEmailRequiredForUsername,
                IsEmailRequiredForUsername = isEmailRequiredForUsername,
                IsUsernameAvailabilityCheckDisabled = GetAttributeValue( AttributeKey.DisableUsernameAvailabilityCheck ).AsBoolean(),
                LoginPageUrl = this.GetLinkedPageUrl( AttributeKey.LoginPage ) ?? "/Login",
                MinimumAge = GetAttributeValue( AttributeKey.MinimumAge ).AsInteger(),
                PhoneNumbers = phoneNumberBags,
                SentLoginCaption = GetAttributeValue( AttributeKey.SentLoginCaption ),
                State = passwordlessLoginStateString,
                SuccessCaption = GetAttributeValue( AttributeKey.SuccessCaption ),
                UsernameFieldLabel = GetAttributeValue( AttributeKey.UsernameFieldLabel ),
                UsernameRegex = isEmailRequiredForUsername ? @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" : Rock.Web.Cache.GlobalAttributesCache.Get().GetValue( "core.ValidUsernameRegularExpression" ),
                UsernameRegexDescription = isEmailRequiredForUsername ? string.Empty : GlobalAttributesCache.Get().GetValue( "core.ValidUsernameCaption" ),
                AccountEntryRegisterStepBox = accountEntryRegisterStepBox,
                IsGenderPickerShown = GetAttributeValue( AttributeKey.ShowGender ).AsBoolean(),
                AccountEntryPersonInfoBag = accountEntryPersonInfoBag,
                DisableCaptchaSupport = GetAttributeValue( AttributeKey.DisableCaptchaSupport ).AsBoolean(),
            };
        }

        /// <summary>
        /// Gets the merge fields.
        /// </summary>
        /// <param name="currentPersonOverride">The current person override if not already on the request.</param>
        /// <param name="additionalMergeFields">Additional merge fields to add to the result.</param>
        /// <returns>The merge fields.</returns>
        private Dictionary<string, object> GetMergeFields( Person currentPersonOverride = null, IDictionary<string, object> additionalMergeFields = null )
        {
            var mergeFields = RequestContext.GetCommonMergeFields( currentPersonOverride );

            if ( additionalMergeFields?.Any() == true )
            {
                foreach ( var mergeField in additionalMergeFields )
                {
                    mergeFields.Add( mergeField.Key, mergeField.Value );
                }
            }

            return mergeFields;
        }

        /// <summary>
        /// Gets the redirect URL after login.
        /// </summary>
        /// <returns>The redirect URL after login.</returns>
        private string GetRedirectUrlAfterRegistration()
        {
            var returnUrl = GetSafeDecodedUrl( PageParameter( PageParameterKey.ReturnUrl ) );

            if ( returnUrl.IsNotNullOrWhiteSpace() )
            {
                return returnUrl;
            }

            return $"{this.RequestContext.RootUrlPath}/page/{PageCache.Id}?status=success";
        }

        /// <summary>
        /// Returns the decoded URL if it is safe or <c>null</c> if it is not.
        /// </summary>
        /// <returns>The <paramref name="url"/> if it is safe; otherwise <c>null</c>.</returns>
        private string GetSafeDecodedUrl( string url )
        {
            if ( url.IsNullOrWhiteSpace() )
            {
                return url;
            }

            var decodedUrl = url.GetFullyUrlDecodedValue();

            // Remove the http and https schemes before checking if URL contains XSS objects.
            if ( decodedUrl.Replace( "https://", string.Empty )
                .Replace( "http://", string.Empty )
                .RedirectUrlContainsXss() )
            {
                return null;
            }

            return decodedUrl;
        }

        /// <summary>
        /// Gets the <see cref="Person"/> record that matches the specified criteria.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="email">The person's email address.</param>
        /// <param name="lastName">The person's last name.</param>
        /// <param name="rockContext">The Rock context.</param>
        /// <returns>The matching person.</returns>
        private Person GetSelectedDuplicatePerson( int? personId, string email, string lastName, RockContext rockContext )
        {
            // Make sure the user picked a valid, matching person and not a random person ID.
            var personService = new PersonService( rockContext );
            return GetDuplicatePeopleQuery( personId, email, lastName, personService )
                .Where( p => p.Id == personId.Value )
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the success caption.
        /// </summary>
        /// <param name="person">The person for whom the caption will be personalized.</param>
        /// <returns>The success step caption.</returns>
        private string GetSuccessCaption( Person person )
        {
            var caption = GetAttributeValue( AttributeKey.SuccessCaption );

            if ( caption.Contains( "{0}" ) )
            {
                return string.Format( caption, person.FirstName );
            }

            return caption;
        }

        /// <summary>
        /// Determines if an address is valid.
        /// </summary>
        /// <param name="box">The register request box.</param>
        /// <param name="config">The block initialization box.</param>
        /// <returns><c>true</c> if the address is not required or if it is valid; otherwise, <c>false</c>.</returns>
        private bool IsAddressValidIfRequired( AccountEntryRegisterRequestBox box, AccountEntryInitializationBox config )
        {
            if ( !config.IsAddressShown || !config.IsAddressRequired )
            {
                return true;
            }

            var address = box.PersonInfo.Address;

            if ( address == null
                 || address.Street1.IsNullOrWhiteSpace()
                 || address.City.IsNullOrWhiteSpace()
                 || address.PostalCode.IsNullOrWhiteSpace() )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the account entry is from passwordless authentication.
        /// </summary>
        /// <param name="bag">The bag.</param>
        private bool IsFromPasswordlessAuthentication( AccountEntryRegisterRequestBox box, out PasswordlessAuthenticationState state )
        {
            if ( box.State.IsNullOrWhiteSpace() )
            {
                state = null;
                return false;
            }

            state = PasswordlessAuthentication.GetDecryptedAuthenticationState( box?.State );
            return state != null;
        }

        /// <summary>
        /// Determines if the full name is valid.
        /// </summary>
        /// <param name="box">The register request box.</param>
        /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
        private static bool IsFullNameValid( AccountEntryRegisterRequestBox box )
        {
            /*
                12/28/2022 - JMH
             
                See https://app.asana.com/0/1121505495628584/1200018171012738/f on why this is done

                Reason: Passwordless Authentication
             */
            if ( box.FullName.IsNotNullOrWhiteSpace() )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if the new person is old enough for a new account.
        /// </summary>
        /// <param name="box">The register request box.</param>
        /// <param name="config">The block initialization box.</param>
        /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
        private static bool IsOldEnough( AccountEntryRegisterRequestBox box, AccountEntryInitializationBox config )
        {
            if ( config.MinimumAge < 1 )
            {
                return true;
            }

            var birthdayParts = box.PersonInfo?.Birthday;
            if ( birthdayParts == null )
            {
                return false;
            }

            var birthdate = RockDateTime.New( birthdayParts.Year, birthdayParts.Month, birthdayParts.Day );
            var youngestAllowedBirthdate = RockDateTime.Now.AddYears( -config.MinimumAge );
            if ( birthdate > youngestAllowedBirthdate )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if the password format is valid.
        /// </summary>
        /// <param name="box">The register request box.</param>
        /// <param name="config">The block initialization box.</param>
        /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
        private static bool IsPasswordFormatValid( AccountEntryRegisterRequestBox box, AccountEntryInitializationBox config )
        {
            // Skip password validation if account info is hidden from the user.
            if ( config.IsAccountInfoHidden )
            {
                return true;
            }

            if ( !UserLoginService.IsPasswordValid( box.AccountInfo?.Password ) )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Tries to finalize a passwordless authentication session if one exists.
        /// </summary>
        /// <param name="rockContext">The context.</param>
        /// <returns><c>true</c> if a passwordless authentication session was finalized; otherwise, <c>false</c>.</returns>
        private bool IsPasswordlessAuthenticationValid( PasswordlessAuthenticationState passwordlessAuthenticationState, RockContext rockContext, out RemoteAuthenticationSession remoteAuthenticationSession )
        {
            var remoteAuthenticationSessionService = new RemoteAuthenticationSessionService( rockContext );
            remoteAuthenticationSession = remoteAuthenticationSessionService.VerifyRemoteAuthenticationSession( passwordlessAuthenticationState.UniqueIdentifier, passwordlessAuthenticationState.Code, passwordlessAuthenticationState.CodeIssueDate, passwordlessAuthenticationState.CodeLifetime );

            if ( remoteAuthenticationSession == null )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if the register request is valid.
        /// </summary>
        /// <param name="box">The register request box.</param>
        /// <param name="config">The block initialization box.</param>
        /// <param name="rockContext">The Rock context.</param>
        /// <param name="errorMessage">The error message that will be set if the request is invalid.</param>
        /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
        private bool IsRequestValid( AccountEntryRegisterRequestBox box, AccountEntryInitializationBox config, RockContext rockContext, out string errorMessage )
        {
            if ( box == null )
            {
                // The API consumer isn't sending the request properly.
                errorMessage = "Request missing";
                return false;
            }

            if ( !IsFullNameValid( box ) )
            {
                errorMessage = "Invalid Form Value";
                return false;
            }

            if ( !IsOldEnough( box, config ) )
            {
                errorMessage = $"We are sorry, you must be at least {( config.MinimumAge == 1 ? "year" : "years" )} old to create an account.";
                return false;
            }

            if ( !IsUsernameValidIfEmailAddressFormatRequired( box, config ) )
            {
                errorMessage = "Username must be a valid email address.";
                return false;
            }

            if ( !IsUsernameValidIfRegexFormatRequired( box, config ) )
            {
                errorMessage = GetAttributeValue( AttributeKey.UsernameFieldLabel ) + " is not valid. " + GlobalAttributesCache.Get().GetValue( "core.ValidUsernameCaption" );
                return false;
            }

            if ( !IsPasswordFormatValid( box, config ) )
            {
                errorMessage = UserLoginService.FriendlyPasswordRules();
                return false;
            }

            if ( !IsUsernameAvailable( box, config, rockContext ) )
            {
                errorMessage = "The " + GetAttributeValue( AttributeKey.UsernameFieldLabel ).ToLower() + " you selected is already in use.";
                return false;
            }

            if ( !IsAddressValidIfRequired( box, config ) )
            {
                errorMessage = "Address is required";
                return false;
            }

            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Determines if the username is available.
        /// </summary>
        /// <param name="box">The register request box.</param>
        /// <param name="config">The block initialization box.</param>
        /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
        private static bool IsUsernameAvailable( AccountEntryRegisterRequestBox box, AccountEntryInitializationBox config, RockContext rockContext )
        {
            // Skip username check if account info is hidden from the user.
            if ( config.IsAccountInfoHidden )
            {
                return true;
            }

            var userLoginService = new UserLoginService( rockContext );
            var userLogin = userLoginService.GetByUserName( box.AccountInfo?.Username );

            if ( userLogin != null )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if the username is in email format, if required.
        /// </summary>
        /// <param name="box">The register request box.</param>
        /// <param name="config">The block initialization box.</param>
        /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
        private static bool IsUsernameValidIfEmailAddressFormatRequired( AccountEntryRegisterRequestBox box, AccountEntryInitializationBox config )
        {
            // Skip username validation if account info was hidden from the user or if email format is not required.
            if ( config.IsAccountInfoHidden || !config.IsEmailRequiredForUsername )
            {
                return true;
            }

            var match = Regex.Match( box.AccountInfo?.Username, @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" );
            if ( !match.Success )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if the username is in the correct regex format, if required.
        /// </summary>
        /// <param name="box">The register request box.</param>
        /// <param name="config">The block initialization box.</param>
        /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
        private static bool IsUsernameValidIfRegexFormatRequired( AccountEntryRegisterRequestBox box, AccountEntryInitializationBox config )
        {
            // Skip username validation if account info was hidden from the user or if email format is required.
            if ( config.IsAccountInfoHidden || config.IsEmailRequiredForUsername )
            {
                return true;
            }

            var regexString = GlobalAttributesCache.Get().GetValue( "core.ValidUsernameRegularExpression" );
            var match = Regex.Match( box.AccountInfo?.Username, regexString );
            if ( !match.Success )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Obscures the <paramref name="value"/> by concatenating its first character and 5 asterisks (*).
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The obscured value.</returns>
        private static string Obscure( string value )
        {
            return $"{value.SubstringSafe( 0, 1 )}*****";
        }

        /// <summary>
        /// Registers a new <see cref="UserLogin"/> for an existing <see cref="Person"/>.
        /// </summary>
        /// <param name="person">The person for which the account will be registered.</param>
        /// <param name="box">The register request box.</param>
        /// <param name="config">The block configuration box.</param>
        /// <param name="rockContext">The Rock context.</param>
        private BlockActionResult RegisterExistingPerson( Person person, AccountEntryRegisterRequestBox box, AccountEntryInitializationBox config, RockContext rockContext )
        {
            if ( person == null )
            {
                return ActionBadRequest( "Invalid Person" );
            }

            UpdatePerson( person, box.PersonInfo, rockContext );

            var isFromPasswordlessAuthentication = IsFromPasswordlessAuthentication( box, out var passwordlessAuthenticationState );
            if ( !isFromPasswordlessAuthentication && CanPersonAuthenticateWithExistingUserLogin( person, rockContext ) )
            {
                return ActionOk( new AccountEntryRegisterResponseBox
                {
                    Step = AccountEntryStep.ExistingAccount,
                    ExistingAccountStepBag = new AccountEntryExistingAccountStepBag
                    {
                        Caption = GetExistingAccountCaption( person )
                    }
                } );
            }

            UserLogin userLogin;
            var isAccountCreated = false;
            if ( isFromPasswordlessAuthentication )
            {
                var remoteAuthenticationSessionService = new RemoteAuthenticationSessionService( rockContext );
                RemoteAuthenticationSession remoteAuthenticationSession;

                /*
                     1/23/2023 - JMH

                     The individual used passwordless authentication
                     and entered a mobile number that didn't match an existing Person.

                     (mobile number verified at this point)

                     They were redirected to the registration page.

                     The registration data they added matches an existing Person's email
                     and the individual opted to authenticate as the existing Person.

                     Now we are here.

                     We need to send a new one-time passcode (OTP) to the existing Person's email
                     to verify that the individual has access to it before we can authenticate them.

                     The Code field is what holds this second OTP value.
                     If box.Code == null, then we need to email the OTP
                     so the individual can supply the emailed code in another registration request.

                     Reason: Passwordless Authentication
                 */
                if ( box.Code.IsNullOrWhiteSpace() )
                {
                    remoteAuthenticationSession = remoteAuthenticationSessionService.VerifyRemoteAuthenticationSession(
                        passwordlessAuthenticationState.UniqueIdentifier,
                        passwordlessAuthenticationState.Code,
                        passwordlessAuthenticationState.CodeIssueDate,
                        passwordlessAuthenticationState.CodeLifetime );

                    if ( remoteAuthenticationSession == null )
                    {
                        return ActionBadRequest( "Code invalid or expired" );
                    }

                    // Overwrite the OTP since we are going to have the individual verify they have access to the existing person's email.
                    remoteAuthenticationSession.Code = remoteAuthenticationSessionService.GenerateUsableCode( passwordlessAuthenticationState.CodeIssueDate, passwordlessAuthenticationState.CodeLifetime );
                    passwordlessAuthenticationState.Code = remoteAuthenticationSession.Code;
                    rockContext.SaveChanges();

                    SendPasswordlessAccountConfirmationEmail( person, remoteAuthenticationSession.Code );

                    return ActionOk( new AccountEntryRegisterResponseBox
                    {
                        Step = AccountEntryStep.PasswordlessConfirmationSent,
                        PasswordlessConfirmationSentStepBag = new AccountEntryPasswordlessConfirmationSentStepBag
                        {
                            Caption = GetAttributeValue( AttributeKey.ConfirmCaptionPasswordless ),
                            State = PasswordlessAuthentication.GetEncryptedAuthenticationState( passwordlessAuthenticationState )
                        }
                    } );
                }

                remoteAuthenticationSession = remoteAuthenticationSessionService.VerifyRemoteAuthenticationSession(
                    passwordlessAuthenticationState.UniqueIdentifier,
                    box.Code,
                    passwordlessAuthenticationState.CodeIssueDate,
                    passwordlessAuthenticationState.CodeLifetime );

                if ( remoteAuthenticationSession == null )
                {
                    return ActionBadRequest( "Code invalid or expired" );
                }

                var userLoginService = new UserLoginService( rockContext );
                var username = PasswordlessAuthentication.GetUsername( passwordlessAuthenticationState.UniqueIdentifier );
                userLogin = userLoginService.GetByUserName( username );

                if ( userLogin == null )
                {
                    // Create new UserLogin for existing person.
                    userLogin = CreatePasswordlessUserLogin( person, username, rockContext );

                    // Add the phone number used for passwordless to the person.
                    if ( passwordlessAuthenticationState.PhoneNumber.IsNotNullOrWhiteSpace() )
                    {
                        person.UpdatePhoneNumber(
                            DefinedValueCache.GetId( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() ).Value,
                            null,
                            passwordlessAuthenticationState.PhoneNumber,
                            true,
                            false,
                            rockContext );
                        rockContext.SaveChanges();
                    }

                    isAccountCreated = true;
                }

                FinalizePasswordlessAuthentication( person, remoteAuthenticationSession, rockContext );
            }
            else
            {
                userLogin = CreateDatabaseUserLogin( person, false, box.AccountInfo.Username, box.AccountInfo.Password, rockContext );
                isAccountCreated = true;
            }

            if ( userLogin.IsConfirmed == false )
            {
                SendAccountConfirmationEmail( userLogin );

                return ActionOk( new AccountEntryRegisterResponseBox
                {
                    Step = AccountEntryStep.ConfirmationSent,
                    ConfirmationSentStepBag = new AccountEntryConfirmationSentStepBag
                    {
                        Caption = GetAttributeValue( AttributeKey.ConfirmCaption )
                    }
                } );
            }
            else
            {
                AuthenticateUser( userLogin );

                if ( isAccountCreated )
                {
                    SendAccountCreatedEmail( userLogin, person );
                }

                return ActionOk( new AccountEntryRegisterResponseBox
                {
                    Step = AccountEntryStep.Completed,
                    CompletedStepBag = new AccountEntryCompletedStepBag
                    {
                        Caption = GetSuccessCaption( person ),
                        RedirectUrl = GetRedirectUrlAfterRegistration(),
                        IsRedirectAutomatic = isFromPasswordlessAuthentication
                    }
                } );
            }
        }

        private void UpdatePerson( Person person, AccountEntryPersonInfoBag bag, RockContext rockContext )
        {

            // Save any attribute values
            person.LoadAttributes( rockContext );
            var personAttributes = GetAttributeCategoryAttributes( rockContext );
            person.SetPublicAttributeValues(
                bag.AttributeValues,
                this.GetCurrentPerson(),
                // Do not enforce security; otherwise, some attribute values may not be set for unauthenticated users.
                enforceSecurity: false,
                attributeFilter: a1 => personAttributes.Any( a => a.Guid == a1.Guid ) );
            person.SaveAttributeValues( rockContext );
        }

        /// <summary>
        /// Registers a new <see cref="Person"/> and a new <see cref="UserLogin"/> for that <see cref="Person"/>.
        /// </summary>
        /// <param name="box">The register request box.</param>
        /// <param name="config">The block configuration box.</param>
        /// <param name="rockContext">The Rock context.</param>
        private BlockActionResult RegisterNewPerson( AccountEntryRegisterRequestBox box, AccountEntryInitializationBox config, RockContext rockContext )
        {
            var person = CreatePerson( box, config, rockContext );

            UserLogin userLogin;
            var isFromPasswordlessAuthentication = IsFromPasswordlessAuthentication( box, out var passwordlessAuthenticationState );

            // Create confirmed UserLogin.
            if ( isFromPasswordlessAuthentication )
            {
                if ( !IsPasswordlessAuthenticationValid( passwordlessAuthenticationState, rockContext, out var remoteAuthenticationSession ) )
                {
                    return ActionBadRequest( "Code invalid or expired" );
                }

                userLogin = CreatePasswordlessUserLogin( person, PasswordlessAuthentication.GetUsername( passwordlessAuthenticationState.UniqueIdentifier ), rockContext );

                /*  
                    10/19/2023 - JMH

                    Also create a Database login if the username and password were provided.
                    This will happen when the individual uses passwordless login for a new email/mobile phone,
                    and if 2FA is enabled in Security Settings for the individual's protection profile,
                    requiring that Rock also gather username & password for their next 2FA login.

                    Reason: Two-Factor Authentication
                 */
                if ( box.AccountInfo?.Username?.IsNotNullOrWhiteSpace() == true && box.AccountInfo.Password.IsNotNullOrWhiteSpace() )
                {
                    CreateDatabaseUserLogin( person, true, box.AccountInfo.Username, box.AccountInfo.Password, rockContext );
                }
            }
            else
            {
                userLogin = CreateDatabaseUserLogin( person, true, box.AccountInfo.Username, box.AccountInfo.Password, rockContext );
            }

            AuthenticateUser( userLogin );

            SendAccountCreatedEmail( userLogin, person );

            return ActionOk( new AccountEntryRegisterResponseBox
            {
                Step = AccountEntryStep.Completed,
                CompletedStepBag = new AccountEntryCompletedStepBag
                {
                    Caption = GetSuccessCaption( person ),
                    RedirectUrl = GetRedirectUrlAfterRegistration(),
                    IsRedirectAutomatic = true
                }
            } );
        }

        /// <summary>
        /// Sends the account confirmation email.
        /// </summary>
        /// <param name="userLogin">The user login.</param>
        private void SendAccountConfirmationEmail( UserLogin userLogin )
        {
            try
            {
                var url = this.GetLinkedPageUrl( AttributeKey.ConfirmationPage );
                if ( url.IsNullOrWhiteSpace() )
                {
                    url = "/ConfirmAccount";
                }

                var mergeFields = GetMergeFields(
                    null,
                    new Dictionary<string, object>
                    {
                        { "ConfirmAccountUrl", this.RequestContext.RootUrlPath + url },
                        { "Person", userLogin.Person },
                        { "User", userLogin }
                    } );

                var message = new RockEmailMessage( GetAttributeValue( AttributeKey.ConfirmAccountTemplate ).AsGuid() );
                message.SetRecipients( new List<RockEmailMessageRecipient>
                {
                    new RockEmailMessageRecipient( userLogin.Person, mergeFields )
                } );
                message.AppRoot = "/";
                message.ThemeRoot = this.RequestContext.ResolveRockUrl( "~~/" );
                message.CreateCommunicationRecord = false;
                message.Send();
            }
            catch ( SystemException ex )
            {
                ExceptionLogService.LogException( ex );
            }
        }

        /// <summary>
        /// Sends the account created email.
        /// </summary>
        /// <param name="userLogin">The user login that was created.</param>
        /// <param name="person">The person associated with the user login.</param>
        private void SendAccountCreatedEmail( UserLogin userLogin, Person person )
        {
            try
            {
                var url = this.GetLinkedPageUrl( AttributeKey.ConfirmationPage );
                if ( url.IsNullOrWhiteSpace() )
                {
                    url = "/ConfirmAccount";
                }

                var mergeObjects = GetMergeFields(
                    person,
                    new Dictionary<string, object>
                    {
                        { "ConfirmAccountUrl", this.RequestContext.RootUrlPath + url },
                        { "Person", person },
                        { "User", userLogin }
                    } );

                var emailMessage = new RockEmailMessage( GetAttributeValue( AttributeKey.AccountCreatedTemplate ).AsGuid() );
                emailMessage.AddRecipient( new RockEmailMessageRecipient( person, mergeObjects ) );
                emailMessage.AppRoot = "/";
                emailMessage.ThemeRoot = this.RequestContext.ResolveRockUrl( "~~/" );
                emailMessage.CreateCommunicationRecord = GetAttributeValue( AttributeKey.CreateCommunicationRecord ).AsBoolean();
                emailMessage.Send();
            }
            catch ( SystemException ex )
            {
                ExceptionLogService.LogException( ex );
            }
        }

        /// <summary>
        /// Sends the account confirmation (passwordless) email.
        /// </summary>
        /// <param name="person">The person.</param>
        private void SendPasswordlessAccountConfirmationEmail( Person person, string code )
        {
            try
            {
                var mergeFields = GetMergeFields(
                    null,
                    new Dictionary<string, object>
                    {
                        { "Person", person },
                        { "Code", code }
                    } );

                var message = new RockEmailMessage( GetAttributeValue( AttributeKey.ConfirmAccountPasswordlessTemplate ).AsGuid() );
                message.SetRecipients( new List<RockEmailMessageRecipient>
                {
                    new RockEmailMessageRecipient( person, mergeFields )
                } );
                message.AppRoot = "/";
                message.ThemeRoot = this.RequestContext.ResolveRockUrl( "~~/" );
                message.CreateCommunicationRecord = false;
                message.Send();
            }
            catch ( SystemException ex )
            {
                ExceptionLogService.LogException( ex );
            }
        }

        #endregion
    }
}
