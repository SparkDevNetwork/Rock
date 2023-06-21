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
using System.ComponentModel;
using System.Net;

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.RegisterAccount;
using Rock.Data;
using Rock.Mobile;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Cms
{
    /// <summary>
    /// Allows the user to register a new account on a mobile application.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Register" )]
    [Category( "Mobile > Cms" )]
    [Description( "Allows the user to register a new account on a mobile application." )]
    [IconCssClass( "fa fa-user-plus" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [BooleanField(
        "Check For Duplicates",
        Key = AttributeKeys.CheckForDuplicates,
        Description = "If enabled and a duplicate is found then it will be used instead of creating a new person record. You must also configure the Confirmation Page and Confirm Account Template settings.",
        DefaultBooleanValue = true,
        Order = 0 )]

    [LinkedPage(
        "Confirmation Page",
        Key = AttributeKeys.ConfirmationWebPage,
        Description = "Web page on a public site for user to confirm their account (if not set then no confirmation e-mail will be sent).",
        IsRequired = false,
        Order = 1 )]

    [SystemCommunicationField(
        "Confirm Account Template",
        Key = AttributeKeys.ConfirmAccountTemplate,
        Description = "The system communication to use when generating the confirm account e-mail.",
        IsRequired = false,
        DefaultSystemCommunicationGuid = Rock.SystemGuid.SystemCommunication.SECURITY_CONFIRM_ACCOUNT,
        Order = 2 )]

    [DefinedValueField(
        "Connection Status",
        Key = AttributeKeys.ConnectionStatus,
        Description = "The connection status to use for new individuals (default = 'Prospect'.)",
        DefinedTypeGuid = "2E6540EA-63F0-40FE-BE50-F2A84735E600",
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PROSPECT,
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
        "Birthdate Show",
        Key = AttributeKeys.BirthDateShow,
        Description = "Determines whether the birthdate field will be available for input.",
        IsRequired = true,
        DefaultBooleanValue = true,
        Category = "custommobile",
        Order = 0 )]

    [BooleanField(
        "BirthDate Required",
        Key = AttributeKeys.BirthDateRequired,
        Description = "Requires that a birthdate value be entered before allowing the user to register.",
        IsRequired = true,
        DefaultBooleanValue = true,
        Category = "custommobile",
        Order = 1 )]

    [BooleanField(
        "Campus Show",
        Key = AttributeKeys.CampusShow,
        Description = "Determines whether the campus field will be available for input.",
        IsRequired = true,
        DefaultBooleanValue = true,
        Category = "custommobile",
        Order = 2 )]

    [BooleanField(
        "Campus Required",
        Key = AttributeKeys.CampusRequired,
        Description = "Requires that a campus value be entered before allowing the user to register.",
        IsRequired = true,
        DefaultBooleanValue = true,
        Category = "custommobile",
        Order = 3 )]

    [BooleanField(
        "Email Show",
        Key = AttributeKeys.EmailShow,
        Description = "Determines whether the email field will be available for input.",
        IsRequired = true,
        DefaultBooleanValue = true,
        Category = "custommobile",
        Order = 4 )]

    [BooleanField(
        "Email Required",
        Key = AttributeKeys.EmailRequired,
        Description = "Requires that a email value be entered before allowing the user to register.",
        IsRequired = true,
        DefaultBooleanValue = true,
        Category = "custommobile",
        Order = 5 )]

    [BooleanField(
        "Mobile Phone Show",
        Key = AttributeKeys.MobilePhoneShow,
        Description = "Determines whether the mobile phone field will be available for input.",
        IsRequired = true,
        DefaultBooleanValue = true,
        Category = "custommobile",
        Order = 6 )]

    [BooleanField(
        "Mobile Phone Required",
        Key = AttributeKeys.MobilePhoneRequired,
        Description = "Requires that a mobile phone value be entered before allowing the user to register.",
        IsRequired = true,
        DefaultBooleanValue = true,
        Category = "custommobile",
        Order = 7 )]

    [CustomDropdownListField( "Gender",
        Key = AttributeKeys.Gender,
        Description = "Determines the visibility and requirement of the Gender field.",
        IsRequired = true,
        DefaultValue = "2",
        ListSource = "0^Hide,1^Optional,2^Required",
        Category = "custommobile",
        Order = 8 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_REGISTER_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( "2A71FDA2-5204-418F-858E-693A1F4E9A49")]
    public class Register : RockBlockType
    {
        /// <summary>
        /// The block setting attribute keys for the MobileRegister block.
        /// </summary>
        public static class AttributeKeys
        {
            /// <summary>
            /// The check for duplicates key.
            /// </summary>
            public const string CheckForDuplicates = "CheckForDuplicates";

            /// <summary>
            /// The confirmation web page key.
            /// </summary>
            public const string ConfirmationWebPage = "ConfirmationWebPage";

            /// <summary>
            /// The confirm account template key.
            /// </summary>
            public const string ConfirmAccountTemplate = "ConfirmAccountTemplate";

            /// <summary>
            /// The connection status key.
            /// </summary>
            public const string ConnectionStatus = "ConnectionStatus";

            /// <summary>
            /// The record status key.
            /// </summary>
            public const string RecordStatus = "RecordStatus";

            /// <summary>
            /// The birth date show key.
            /// </summary>
            public const string BirthDateShow = "BirthDateShow";

            /// <summary>
            /// The birth date required key.
            /// </summary>
            public const string BirthDateRequired = "BirthDateRequired";

            /// <summary>
            /// The campus show key.
            /// </summary>
            public const string CampusShow = "CampusShow";

            /// <summary>
            /// The campus required key.
            /// </summary>
            public const string CampusRequired = "CampusRequired";

            /// <summary>
            /// The email show key.
            /// </summary>
            public const string EmailShow = "EmailShow";

            /// <summary>
            /// The email required key.
            /// </summary>
            public const string EmailRequired = "EmailRequired";

            /// <summary>
            /// The gender key.
            /// </summary>
            public const string Gender = "Gender";

            /// <summary>
            /// The mobile phone show key.
            /// </summary>
            public const string MobilePhoneShow = "MobilePhoneShow";

            /// <summary>
            /// The mobile phone required key.
            /// </summary>
            public const string MobilePhoneRequired = "MobilePhoneRequired";
        }

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 1 );

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            return new
            {
            };
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Registers the user.
        /// </summary>
        /// <param name="account">The account data.</param>
        /// <param name="supportsLogin">If <c>true</c> then the caller supports immediate login responses.</param>
        /// <param name="personalDeviceGuid">The personal device unique identifier that represents the device being used to login from.</param>
        /// <returns>The result of the registration request.</returns>
        [BlockAction]
        public object RegisterUser( AccountData account, bool supportsLogin = false, Guid? personalDeviceGuid = null )
        {
            if ( account.Username.IsNullOrWhiteSpace() || account.FirstName.IsNullOrWhiteSpace() || account.LastName.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Missing required information." );
            }

            // Verify that the username and password are valid.
            if ( !UserLoginService.IsValidNewUserLogin( account.Username, account.Password, account.Password, out _, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            using ( var rockContext = new RockContext() )
            {
                var checkForDuplicates = GetAttributeValue( AttributeKeys.CheckForDuplicates ).AsBoolean();
                Person person = null;
                UserLogin userLogin;

                // Determine if we can perform duplicate matching. This requires:
                // 1) That it be enabled on the block settings.
                // 2) An e-mail address was provided which triggers stricter matching.
                // 3) V3 shell or later that supports the new response type.
                // 4) Valid confirmation web page configured.
                // 5) Valid confirmation e-mail configured.
                var canSearchForDuplicates = checkForDuplicates
                    && account.Email.IsNotNullOrWhiteSpace()
                    && supportsLogin
                    && GetAttributeValue( AttributeKeys.ConfirmationWebPage ).IsNotNullOrWhiteSpace()
                    && GetAttributeValue( AttributeKeys.ConfirmAccountTemplate ).IsNotNullOrWhiteSpace();

                if ( canSearchForDuplicates )
                {
                    person = FindMatchingPerson( account, rockContext );
                }

                if ( person == null || ( person.Email.IsNullOrWhiteSpace() && account.Email.IsNullOrWhiteSpace() ) )
                {
                    // If no person was found, or we don't have any e-mail address
                    // to work with then create a new account. We need at least an
                    // e-mail address so we can send the confirmation e-mail.
                    person = CreatePerson( account, rockContext );

                    var isConfirmed = GetAttributeValue( AttributeKeys.ConfirmationWebPage ).IsNullOrWhiteSpace();
                    userLogin = CreateUser( person, account, isConfirmed, rockContext );
                }
                else
                {
                    UpdatePerson( person, account, rockContext );
                    userLogin = CreateUser( person, account, false, rockContext );
                }

                // Older versions of the mobile shell do not support automatic
                // login. At this point we can be assured that we have not done
                // person matching because that also requires supportsLogin.
                if ( !supportsLogin )
                {
                    return ActionOk();
                }

                // If the account isn't confirmed yet, then return a special
                // code to the shell so it knows to update the UI.
                if ( userLogin.IsConfirmed != true )
                {
                    SendConfirmation( userLogin );

                    return ActionContent( HttpStatusCode.Unauthorized, new
                    {
                        Code = 1,
                        Message = GetConfirmEmailMessage()
                    } );
                }

                // Update the last login details and log the user in.
                UpdateLastLoginDetails( userLogin, personalDeviceGuid, rockContext );

                return GetMobileResponse( userLogin, true );
            }
        }

        /// <summary>
        /// Validates the username and password and returns a response that
        /// can by used by the mobile shell.
        /// </summary>
        /// <param name="username">The username to log in with.</param>
        /// <param name="password">The password to log in with.</param>
        /// <param name="rememberMe">If <c>true</c> then the cookie will persist across sessions.</param>
        /// <param name="personalDeviceGuid">The personal device unique identifier making the request.</param>
        /// <returns>The result of the block action.</returns>
        [BlockAction]
        public BlockActionResult MobileLogin( string username, string password, bool rememberMe, Guid? personalDeviceGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var userLoginService = new UserLoginService( rockContext );
                var (state, userLogin) = userLoginService.GetAuthenticatedUserLogin( username, password );

                if ( state == UserLoginValidationState.Valid )
                {
                    UpdateLastLoginDetails( userLogin, personalDeviceGuid, rockContext );

                    return GetMobileResponse( userLogin, rememberMe );
                }
                else
                {
                    return ActionUnauthorized( "It looks like your account isn't ready to be logged into just yet, please try again after confirming your e-mail address." );
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Finds the best matching person for the given account data. This will
        /// handle
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private Person FindMatchingPerson( AccountData account, RockContext rockContext )
        {
            var personService = new PersonService( rockContext );
            Gender? gender = ( Gender ) account.Gender;

            // For matching purposes, treat "unknown" as not set.
            if ( gender == Gender.Unknown )
            {
                gender = null;
            }

            // Try to find a matching person based on name, email address,
            // mobile phone, and birthday. If these were not provided they
            // are not considered.
            var personQuery = new PersonService.PersonMatchQuery( account.FirstName, account.LastName, account.Email, account.MobilePhone, gender: gender, birthDate: account.BirthDate?.DateTime );

            return personService.FindPerson( personQuery, true );
        }

        /// <summary>
        /// Creates the person.
        /// </summary>
        /// <param name="account">The account details.</param>
        /// <param name="rockContext">The database context to operate in.</param>
        /// <returns></returns>
        private Person CreatePerson( AccountData account, RockContext rockContext )
        {
            DefinedValueCache dvcConnectionStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKeys.ConnectionStatus ).AsGuid() );
            DefinedValueCache dvcRecordStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKeys.RecordStatus ).AsGuid() );

            Person person = new Person
            {
                FirstName = account.FirstName,
                LastName = account.LastName,
                Email = account.Email,
                Gender = ( Gender ) account.Gender,
                IsEmailActive = true,
                EmailPreference = EmailPreference.EmailAllowed,
                RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id
            };

            if ( dvcConnectionStatus != null )
            {
                person.ConnectionStatusValueId = dvcConnectionStatus.Id;
            }

            if ( dvcRecordStatus != null )
            {
                person.RecordStatusValueId = dvcRecordStatus.Id;
            }

            if ( account.BirthDate.HasValue )
            {
                person.BirthMonth = account.BirthDate.Value.Month;
                person.BirthDay = account.BirthDate.Value.Day;
                if ( account.BirthDate.Value.Year != DateTime.MinValue.Year )
                {
                    person.BirthYear = account.BirthDate.Value.Year;
                }
            }

            if ( !string.IsNullOrWhiteSpace( PhoneNumber.CleanNumber( account.MobilePhone ) ) )
            {
                int phoneNumberTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() ).Id;

                var phoneNumber = new PhoneNumber
                {
                    NumberTypeValueId = phoneNumberTypeId,
                    Number = PhoneNumber.CleanNumber( account.MobilePhone )
                };
                person.PhoneNumbers.Add( phoneNumber );

                // TODO: Do we need to deal with this? -dsh
                //phoneNumber.CountryCode = PhoneNumber.CleanNumber( pnbPhone.CountryCode );

                // TODO: How to deal with SMS enabled option? -dsh
                phoneNumber.IsMessagingEnabled = false;
            }

            int? campusId = null;

            if ( account.Campus.HasValue )
            {
                campusId = CampusCache.Get( account.Campus.Value )?.Id;
            }

            PersonService.SaveNewPerson( person, rockContext, campusId, false );

            return person;
        }

        /// <summary>
        /// Creates the user.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="account">The account details.</param>
        /// <param name="confirmed">If set to <c>true</c> then the account is marked as confirmed.</param>
        /// <param name="rockContext">The database context to operate in.</param>
        /// <returns>The <see cref="UserLogin"/> that was created.</returns>
        private UserLogin CreateUser( Person person, AccountData account, bool confirmed, RockContext rockContext )
        {
            return UserLoginService.Create(
                rockContext,
                person,
                AuthenticationServiceType.Internal,
                EntityTypeCache.Get( Rock.SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id,
                account.Username,
                account.Password,
                confirmed );
        }

        /// <summary>
        /// Updates the person record with the provided account information.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="account">The account.</param>
        /// <param name="rockContext">The rock context.</param>
        private void UpdatePerson( Person person, AccountData account, RockContext rockContext )
        {
            person.FirstName = account.FirstName;
            person.LastName = account.LastName;

            if ( account.Gender != ( int ) Gender.Unknown )
            {
                person.Gender = ( Gender ) account.Gender;
            }

            if ( account.BirthDate.HasValue )
            {
                person.BirthMonth = account.BirthDate.Value.Month;
                person.BirthDay = account.BirthDate.Value.Day;
                if ( account.BirthDate.Value.Year != DateTime.MinValue.Year )
                {
                    person.BirthYear = account.BirthDate.Value.Year;
                }
            }

            if ( account.Campus.HasValue )
            {
                var campusId = CampusCache.Get( account.Campus.Value )?.Id;

                if ( campusId.HasValue )
                {
                    person.GetFamily( rockContext ).CampusId = campusId;
                }
            }

            if ( account.Email.IsNotNullOrWhiteSpace() )
            {
                person.Email = account.Email;
            }

            if ( !string.IsNullOrWhiteSpace( PhoneNumber.CleanNumber( account.MobilePhone ) ) )
            {
                int phoneNumberTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() ).Id;

                person.UpdatePhoneNumber( phoneNumberTypeId, string.Empty, account.MobilePhone, null, null, rockContext );
            }
        }

        /// <summary>
        /// Gets the confirm e-mail address message.
        /// </summary>
        /// <returns>The message to display.</returns>
        protected virtual string GetConfirmEmailMessage()
        {
            return "Your account was created, however, we need to confirm the email associated with this account belongs to you. We've sent you an email that contains a link for confirming. Please click the link in your email and then try logging in again.";
        }

        /// <summary>
        /// Updates the last login details and any related login facts.
        /// </summary>
        /// <param name="userLogin">The user login.</param>
        /// <param name="personalDeviceGuid">The personal device unique identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        private void UpdateLastLoginDetails( UserLogin userLogin, Guid? personalDeviceGuid, RockContext rockContext )
        {
            // If we have a personal device, then attempt to update it
            // to point at this person unless it already points there.
            if ( personalDeviceGuid.HasValue )
            {
                var personalDevice = new PersonalDeviceService( rockContext ).Get( personalDeviceGuid.Value );

                if ( personalDevice != null && personalDevice.PersonAliasId != userLogin.Person.PrimaryAliasId )
                {
                    personalDevice.PersonAliasId = userLogin.Person.PrimaryAliasId;
                }
            }

            userLogin.LastLoginDateTime = RockDateTime.Now;

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Gets the response to send for a valid log in on mobile.
        /// </summary>
        /// <param name="userLogin">The user login.</param>
        /// <param name="rememberMe">if set to <c>true</c> then the login should persist beyond this session.</param>
        /// <returns>The result of the action.</returns>
        private BlockActionResult GetMobileResponse( UserLogin userLogin, bool rememberMe )
        {
            var site = MobileHelper.GetCurrentApplicationSite();

            if ( site == null )
            {
                return ActionStatusCode( HttpStatusCode.Unauthorized );
            }

            var authCookie = Rock.Security.Authorization.GetSimpleAuthCookie( userLogin.UserName, rememberMe, false );

            var mobilePerson = MobileHelper.GetMobilePerson( userLogin.Person, site );
            mobilePerson.AuthToken = authCookie.Value;

            return ActionOk( new
            {
                Person = mobilePerson
            } );
        }

        /// <summary>
        /// Sends the confirmation e-mail for the login.
        /// </summary>
        /// <param name="userLogin">The user login that should receive the confirmation e-mail.</param>
        /// <returns><c>true</c> if the e-mail was sent; otherwise <c>false</c>.</returns>
        private bool SendConfirmation( UserLogin userLogin )
        {
            var systemEmailGuid = GetAttributeValue( AttributeKeys.ConfirmAccountTemplate ).AsGuidOrNull();
            var confirmationWebPage = GetAttributeValue( AttributeKeys.ConfirmationWebPage );
            var confirmationPageGuid = confirmationWebPage.Split( ',' )[0].AsGuidOrNull();
            var confirmationPage = confirmationPageGuid.HasValue ? PageCache.Get( confirmationPageGuid.Value ) : null;

            // Make sure we have the required information.
            if ( !systemEmailGuid.HasValue || confirmationPage == null )
            {
                return false;
            }

            var mergeFields = RequestContext.GetCommonMergeFields();

            UserLoginService.SendConfirmationEmail( userLogin, systemEmailGuid.Value, confirmationPage, null, mergeFields );

            return true;
        }

        #endregion
    }
}
