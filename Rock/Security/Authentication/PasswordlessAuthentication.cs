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
using System.ComponentModel.Composition;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Rock.Communication;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Security.Authentication.OneTimePasscode;
using Rock.Security.Authentication.Passwordless;
using Rock.Utility.Enums;

namespace Rock.Security.Authentication
{
    /// <summary>
    /// Authenticates a user using Google
    /// </summary>
    /// <seealso cref="Rock.Security.AuthenticationComponent" />
    [Description( "Passwordless Authentication Provider" )]
    [Export( typeof( AuthenticationComponent ) )]
    [ExportMetadata( "ComponentName", "Passwordless Authentication" )]

    [SystemGuid.EntityTypeGuid( SystemGuid.EntityType.AUTHENTICATION_PASSWORDLESS )]
    internal class PasswordlessAuthentication : AuthenticationComponent, IOneTimePasscodeAuthentication
    {
        /// <inheritdoc/>
        public override bool RequiresRemoteAuthentication
        {
            get
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public override AuthenticationServiceType ServiceType
        {
            get
            {
                // This must be external to allow passwordless UserLogins.
                return AuthenticationServiceType.External;
            }
        }

        /// <inheritdoc/>
        public override bool SupportsChangePassword
        {
            get
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public override bool Authenticate( UserLogin user, string password )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override bool Authenticate( HttpRequest request, out string userName, out string returnUrl )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override bool ChangePassword( UserLogin user, string oldPassword, string newPassword, out string warningMessage )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override string EncodePassword( UserLogin user, string password )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override Uri GenerateLoginUrl( HttpRequest request )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the decrypted passwordless authentication state from <paramref name="state"/>.
        /// </summary>
        /// <param name="state">The state to decrypt.</param>
        /// <returns>The decrypted state.</returns>
        internal static PasswordlessAuthenticationState GetDecryptedAuthenticationState( string state )
        {
            return Encryption.DecryptString( state )?.FromJsonOrNull<PasswordlessAuthenticationState>();
        }

        /// <summary>
        /// Gets the encrypted authentication state.
        /// </summary>
        /// <param name="state">The passwordless authentication state.</param>
        /// <returns>The encrypted authentication state.</returns>
        internal static string GetEncryptedAuthenticationState( PasswordlessAuthenticationState state )
        {
            return Encryption.EncryptString( state?.ToJson() );
        }

        /// <summary>
        /// Gets the passwordless username given a <paramref name="userIdentifier"/>.
        /// </summary>
        /// <param name="userIdentifier">The user identifier to use when generating the passwordless username.</param>
        /// <returns>The passwordless username.</returns>
        public static string GetUsername( string userIdentifier )
        {
            return $"PASSWORDLESS_{userIdentifier}";
        }

        /// <inheritdoc/>
        public override string ImageUrl()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override bool IsReturningFromAuthentication( HttpRequest request )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override void SetPassword( UserLogin user, string password )
        {
            throw new NotImplementedException();
        }

        #region IOneTimePasscodeAuthentication Implementation

        /// <inheritdoc/>
        public OneTimePasscodeAuthenticationResult Authenticate( OneTimePasscodeAuthenticationOptions options )
        {
            if ( !IsRequestValid( options, out var state ) )
            {
                return new OneTimePasscodeAuthenticationResult
                {
                    ErrorMessage = "Code invalid or expired",
                    State = options.State
                };
            }

            using ( var rockContext = new RockContext() )
            {
                if ( !IsOneTimePasscodeValid( rockContext, state, out var remoteAuthenticationSession ) )
                {
                    return new OneTimePasscodeAuthenticationResult
                    {
                        ErrorMessage = "Code is invalid",
                        State = options.State
                    };
                }

                var user = GetExistingPasswordlessUser( rockContext, state.UniqueIdentifier );

                if ( user != null )
                {
                    return AuthenticateExistingPasswordlessUser( rockContext, options, remoteAuthenticationSession, user );
                }
                else
                {
                    return AuthenticateNewPasswordlessUser( rockContext, options, state, remoteAuthenticationSession );
                }
            }
        }

        /// <summary>
        /// Sends a one time passcode (OTP) via email or SMS.
        /// </summary>
        /// <param name="sendOneTimePasscodeOptions">The OTP options.</param>
        /// <param name="rockContext">The Rock context.</param>
        /// <returns>A result containing the encrypted passwordless state, if successful.</returns>
        public SendOneTimePasscodeResult SendOneTimePasscode( SendOneTimePasscodeOptions sendOneTimePasscodeOptions, RockContext rockContext )
        {
            if ( !IsRequestValid( sendOneTimePasscodeOptions ) )
            {
                return new SendOneTimePasscodeResult
                {
                    IsSuccessful = false,
                    ErrorMessage = "Please provide Email or Phone for passwordless login.",
                    State = null
                };
            }

            var securitySettings = new SecuritySettingsService().SecuritySettings;

            var passwordlessSystemCommunication = new SystemCommunicationService( rockContext ).Get( securitySettings.PasswordlessConfirmationCommunicationTemplateGuid );

            if ( passwordlessSystemCommunication.IsActive == false )
            {
                return new SendOneTimePasscodeResult
                {
                    IsSuccessful = false,
                    ErrorMessage = "The Passwordless Login Confirmation system communication needs to be active to use passwordless login.",
                    State = null
                };
            }

            var remoteAuthenticationSessionService = new RemoteAuthenticationSessionService( rockContext );

            if ( sendOneTimePasscodeOptions.ShouldSendSmsCode )
            {
                var uniqueIdentifier = PhoneNumber.CleanNumber( sendOneTimePasscodeOptions.PhoneNumber );
                var codeIssueDate = RockDateTime.Now;
                var remoteAuthenticationSession = remoteAuthenticationSessionService.StartRemoteAuthenticationSession( sendOneTimePasscodeOptions.IpAddress, securitySettings.PasswordlessSignInDailyIpThrottle, uniqueIdentifier, codeIssueDate, sendOneTimePasscodeOptions.OtpLifetime );

                var state = new PasswordlessAuthenticationState
                {
                    Code = remoteAuthenticationSession.Code,
                    CodeIssueDate = codeIssueDate,
                    CodeLifetime = sendOneTimePasscodeOptions.OtpLifetime,
                    Email = sendOneTimePasscodeOptions.Email,
                    PhoneNumber = sendOneTimePasscodeOptions.PhoneNumber,
                    UniqueIdentifier = uniqueIdentifier,
                };

                var smsMessage = new RockSMSMessage( passwordlessSystemCommunication )
                {
                    CreateCommunicationRecord = false
                };

                smsMessage.SetRecipients(
                    new List<RockSMSMessageRecipient>
                    {
                        RockSMSMessageRecipient.CreateAnonymous(
                            sendOneTimePasscodeOptions.PhoneNumber,
                            CombineMergeFields(
                                sendOneTimePasscodeOptions.CommonMergeFields,
                                new Dictionary<string, object>
                                {
                                    { "Code", state.Code }
                                } ) )
                    } );

                List<string> errorMessages;
                if ( smsMessage.Send( out errorMessages ) )
                {
                    rockContext.SaveChanges();

                    return new SendOneTimePasscodeResult()
                    {
                        IsSuccessful = true,
                        ErrorMessage = null,
                        State = GetEncryptedAuthenticationState( state )
                    };
                }
                else
                {
                    return new SendOneTimePasscodeResult()
                    {
                        IsSuccessful = false,
                        ErrorMessage = "Unable to send confirmation code. Make sure to use a mobile phone that can receive text messages.",
                        State = null
                    };
                }
            }

            if ( sendOneTimePasscodeOptions.ShouldSendEmailCode || sendOneTimePasscodeOptions.ShouldSendEmailLink )
            {
                var uniqueIdentifier = sendOneTimePasscodeOptions.Email;
                var codeIssueDate = RockDateTime.Now;
                var remoteAuthenticationSession = remoteAuthenticationSessionService.StartRemoteAuthenticationSession( sendOneTimePasscodeOptions.IpAddress, securitySettings.PasswordlessSignInDailyIpThrottle, uniqueIdentifier, codeIssueDate, sendOneTimePasscodeOptions.OtpLifetime );

                var state = new PasswordlessAuthenticationState
                {
                    Code = remoteAuthenticationSession.Code,
                    CodeIssueDate = codeIssueDate,
                    CodeLifetime = sendOneTimePasscodeOptions.OtpLifetime,
                    Email = sendOneTimePasscodeOptions.Email,
                    PhoneNumber = sendOneTimePasscodeOptions.PhoneNumber,
                    UniqueIdentifier = uniqueIdentifier,
                };

                var encryptedState = GetEncryptedAuthenticationState( state );

                var emailMessage = new RockEmailMessage( passwordlessSystemCommunication )
                {
                    CreateCommunicationRecord = false
                };

                var isExistingPerson = GetMatchingPeopleQuery( rockContext, sendOneTimePasscodeOptions.PhoneNumber, sendOneTimePasscodeOptions.Email ).Any();

                var mergeFields = CombineMergeFields(
                    sendOneTimePasscodeOptions.CommonMergeFields,
                    new Dictionary<string, object>
                    {
                        { "IsNewPerson", !isExistingPerson },
                        { "LinkExpiration", LavaFilters.HumanizeTimeSpan(codeIssueDate, codeIssueDate + sendOneTimePasscodeOptions.OtpLifetime, 2 ) }
                    } );

                if ( sendOneTimePasscodeOptions.ShouldSendEmailCode )
                {
                    mergeFields.Add( "Code", state.Code );
                }

                if ( sendOneTimePasscodeOptions.ShouldSendEmailLink )
                {
                    var queryParams = new Dictionary<string, string>
                    {
                        { "Code", state.Code },
                        { "State", encryptedState },
                        { "IsPasswordless", true.ToString() }
                    };

                    if ( sendOneTimePasscodeOptions.PostAuthenticationRedirectUrl.IsNotNullOrWhiteSpace() )
                    {
                        queryParams.Add( "ReturnUrl", sendOneTimePasscodeOptions.PostAuthenticationRedirectUrl );
                    }

                    mergeFields.Add( "Link", sendOneTimePasscodeOptions.GetLink( queryParams ) );
                }

                emailMessage.SetRecipients( new List<RockEmailMessageRecipient>
                {
                    RockEmailMessageRecipient.CreateAnonymous( sendOneTimePasscodeOptions.Email, mergeFields )
                } );

                if ( emailMessage.Send() )
                {
                    rockContext.SaveChanges();

                    return new SendOneTimePasscodeResult()
                    {
                        IsSuccessful = true,
                        ErrorMessage = null,
                        State = encryptedState
                    };
                }
            }

            return new SendOneTimePasscodeResult
            {
                IsSuccessful = false,
                ErrorMessage = "Verification code failed to send",
                State = null
            };
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Authenticates an existing passwordless user.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="options">The verify one time passcode options.</param>
        /// <param name="remoteAuthenticationSession">The remote authentication session.</param>
        /// <param name="user">The user.</param>
        /// <returns><c>true</c> if authenticated; otherwise, <c>false</c> is returned.</returns>
        private static OneTimePasscodeAuthenticationResult AuthenticateExistingPasswordlessUser( RockContext rockContext, OneTimePasscodeAuthenticationOptions options, RemoteAuthenticationSession remoteAuthenticationSession, UserLogin user )
        {
            if ( !IsPasswordlessAuthenticationAllowedForProtectionProfile( user.Person ) )
            {
                return new OneTimePasscodeAuthenticationResult
                {
                    ErrorMessage = "Passwordless sign-in not available for your protection profile. Please request assistance from the organization administrator."
                };
            }

            CompleteRemoteAuthenticationSession( rockContext, remoteAuthenticationSession, user.Person );

            return new OneTimePasscodeAuthenticationResult
            {
                IsAuthenticated = true,
                State = options.State,
                AuthenticatedUser = user,
            };
        }

        /// <summary>
        /// Authenticates a new passwordless user.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="options">The verify one time passcode options.</param>
        /// <param name="state">The state.</param>
        /// <param name="remoteAuthenticationSession">The remote authentication session.</param>
        /// <returns><c>true</c> if authenticated; otherwise, <c>false</c> is returned.</returns>
        private static OneTimePasscodeAuthenticationResult AuthenticateNewPasswordlessUser( RockContext rockContext, OneTimePasscodeAuthenticationOptions options, PasswordlessAuthenticationState state, RemoteAuthenticationSession remoteAuthenticationSession )
        {
            /*
               12/21/2022 - JMH

               If there are no existing people who match the phone number or email provided, then redirect the individual to the registration page.
               If there is exactly one person who matches, then authenticate the individual.
               If there are multiple people who match, then return the list of people for the individual to select from.
                   Once the individual selects the intended person, the request should contain a person id and this block action should run through the authentication process.

               Reason: Passwordless Sign In
             */

            var matchingPeople = GetMatchingPeople( rockContext, state.PhoneNumber, state.Email );

            if ( !matchingPeople.Any() )
            {
                return new OneTimePasscodeAuthenticationResult
                {
                    IsRegistrationRequired = true,
                    State = options.State
                };
            }

            var personService = new PersonService( rockContext );

            Person person = null;

            // If a matching person parameter was passed in, then verify that it is one of the matched people to prevent hijacking.
            if ( options.MatchingPersonValue.IsNotNullOrWhiteSpace() )
            {
                var matchingPersonState = GetDecryptedMatchingPersonState( options.MatchingPersonValue );

                person = matchingPersonState == null
                    ? null
                    : matchingPeople.FirstOrDefault( p => p.Id == matchingPersonState.PersonId );

                if ( person == null )
                {
                    return new OneTimePasscodeAuthenticationResult
                    {
                        ErrorMessage = "The selected person is invalid"
                    };
                }
            }
            else if ( matchingPeople.Count == 1 )
            {
                person = matchingPeople[0];
            }
            else
            {
                // Multiple people match phone number or email provided.
                // Individual must select the person they want to authenticate as.
                var matchingPersonResults = matchingPeople
                    .Select( p => new PasswordlessMatchingPersonState
                    {
                        PersonId = p.Id,
                        FullName = p.FullName
                    } )
                    .Select( p => new MatchingPersonResult
                    {
                        State = GetEncryptedMatchingPersonState( p ),
                        FullName = p.FullName
                    } )
                    .ToList();

                var providedValues = new List<string>();

                if ( state.Email.IsNotNullOrWhiteSpace() )
                {
                    providedValues.Add( "email" );
                }

                if ( state.PhoneNumber.IsNotNullOrWhiteSpace() )
                {
                    providedValues.Add( "phone number" );
                }

                return new OneTimePasscodeAuthenticationResult
                {
                    IsPersonSelectionRequired = true,
                    MatchingPeopleResults = matchingPersonResults
                };
            }

            // If we made it here, we know who is attempting to authenticate.

            // Check if passwordless authentication is allowed.
            if ( !IsPasswordlessAuthenticationAllowedForProtectionProfile( person ) )
            {
                return new OneTimePasscodeAuthenticationResult
                {
                    ErrorMessage = "Passwordless sign-in not available for your protection profile. Please request assistance from the organization administrator."
                };
            }

            var username = GetUsername( state.UniqueIdentifier );
            var user = person?.Users.FirstOrDefault( u => u.UserName == username );
            if ( user == null )
            {
                user = UserLoginService.Create( rockContext, person, AuthenticationServiceType.External, EntityTypeCache.Get( typeof( PasswordlessAuthentication ) ).Id, username, null, true );
            }

            CompleteRemoteAuthenticationSession( rockContext, remoteAuthenticationSession, person );

            return new OneTimePasscodeAuthenticationResult
            {
                IsAuthenticated = true,
                State = options.State,
                AuthenticatedUser = user,
            };
        }

        /// <summary>
        /// Combines the merge field dictionaries into a new dictionary.
        /// </summary>
        /// <param name="mergeFields">The merge fields.</param>
        /// <param name="additionalMergeFields">The additional merge fields.</param>
        /// <returns>The new dictionary with combined merge fields.</returns>
        private static Dictionary<string, object> CombineMergeFields( IDictionary<string, object> mergeFields, IDictionary<string, object> additionalMergeFields )
        {
            if ( mergeFields == null && additionalMergeFields == null )
            {
                return null;
            }

            if ( mergeFields == null )
            {
                return additionalMergeFields.ToDictionary( kvp => kvp.Key, kvp => kvp.Value );
            }

            if ( additionalMergeFields == null )
            {
                return mergeFields.ToDictionary( kvp => kvp.Key, kvp => kvp.Value );
            }

            var combinedMergeFields = mergeFields.ToDictionary( kvp => kvp.Key, kvp => kvp.Value );

            foreach ( var additionalMergeField in additionalMergeFields )
            {
                combinedMergeFields.Add( additionalMergeField.Key, additionalMergeField.Value );
            }

            return combinedMergeFields;
        }

        /// <summary>
        /// Completes the remote authentication session and saves the context.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="remoteAuthenticationSession">The remote authentication session.</param>
        /// <param name="person">The person.</param>
        private static void CompleteRemoteAuthenticationSession( RockContext rockContext, RemoteAuthenticationSession remoteAuthenticationSession, Person person )
        {
            var remoteAuthenticationSessionService = new RemoteAuthenticationSessionService( rockContext );
            remoteAuthenticationSessionService.CompleteRemoteAuthenticationSession( remoteAuthenticationSession, person.PrimaryAliasId.Value );
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Gets the decrypted passwordless matching person state from <paramref name="state"/>.
        /// </summary>
        /// <param name="state">The state to decrypt.</param>
        /// <returns>The decrypted state.</returns>
        private static PasswordlessMatchingPersonState GetDecryptedMatchingPersonState( string state )
        {
            return Encryption.DecryptString( state )?.FromJsonOrNull<PasswordlessMatchingPersonState>();
        }

        /// <summary>
        /// Gets the encrypted matching person state.
        /// </summary>
        /// <param name="state">The matching person state.</param>
        /// <returns>The encrypted matching person state.</returns>
        private static string GetEncryptedMatchingPersonState( PasswordlessMatchingPersonState state )
        {
            return Encryption.EncryptString( state?.ToJson() );
        }

        /// <summary>
        /// Gets the existing passwordless user.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="uniqueIdentifier">The unique identifier (phone or email).</param>
        /// <returns>The existing passwordless user or <c>null</c>.</returns>
        private static UserLogin GetExistingPasswordlessUser( RockContext rockContext, string uniqueIdentifier )
        {
            var userLoginService = new UserLoginService( rockContext );
            var username = GetUsername( uniqueIdentifier );
            return userLoginService.GetByUserName( username );
        }

        /// <summary>
        /// Gets the people who match phone or email with duplicates removed.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="phoneNumber">The phone number.</param>
        /// <param name="email">The email.</param>
        /// <returns>The list of matching people.</returns>
        private static List<Person> GetMatchingPeople( RockContext rockContext, string phoneNumber, string email )
        {
            var peopleQuery = GetMatchingPeopleQuery( rockContext, phoneNumber, email );

            var allPeople = peopleQuery.ToList();
            var peopleWithoutDuplicates = allPeople
                .GroupBy( p => new
                {
                    FirstishName = p.NickName + p.FirstName,
                    p.LastName,
                    p.BirthDate,
                    Role = p.GetFamilyRole()?.Id
                } )
                .Select( g => g.OrderBy( p => p.Id ).First() )
                .ToList();
            return peopleWithoutDuplicates;
        }

        /// <summary>
        /// Gets the matching people query.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="phoneNumber">The phone number.</param>
        /// <param name="email">The email.</param>
        /// <returns>The matching people query.</returns>
        private static IQueryable<Person> GetMatchingPeopleQuery( RockContext rockContext, string phoneNumber, string email )
        {
            var personService = new PersonService( rockContext );
            var phoneNumberService = new PhoneNumberService( rockContext );
            IQueryable<Person> peopleQuery = null;

            if ( email.IsNotNullOrWhiteSpace() )
            {
                peopleQuery = personService.GetByEmail( email ).AsNoTracking();
            }

            if ( phoneNumber.IsNotNullOrWhiteSpace() )
            {
                if ( peopleQuery == null )
                {
                    peopleQuery = personService.Queryable().AsNoTracking();
                }

                var personIdsByPhoneNumber = phoneNumberService.GetPersonIdsByNumber( phoneNumber );

                peopleQuery = peopleQuery.Where( p => personIdsByPhoneNumber.Contains( p.Id ) );
            }

            return peopleQuery ?? Enumerable.Empty<Person>().AsQueryable();
        }

        /// <summary>
        /// Determines if the one-time passcode is valid.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="state">The state.</param>
        /// <param name="remoteAuthenticationSession">The remote authentication session set if the one-time passcode is valid.</param>
        private static bool IsOneTimePasscodeValid( RockContext rockContext, PasswordlessAuthenticationState state, out RemoteAuthenticationSession remoteAuthenticationSession )
        {
            var remoteAuthenticationSessionService = new RemoteAuthenticationSessionService( rockContext );
            remoteAuthenticationSession = remoteAuthenticationSessionService.VerifyRemoteAuthenticationSession( state.UniqueIdentifier, state.Code, state.CodeIssueDate, state.CodeLifetime );

            return remoteAuthenticationSession != null;
        }

        /// <summary>
        /// Determines whether passwordless authentication is allowed for <paramref name="person"/>.
        /// </summary>
        /// <param name="person">The person to check.</param>
        private static bool IsPasswordlessAuthenticationAllowedForProtectionProfile( Person person )
        {
            var securitySettings = new SecuritySettingsService().SecuritySettings;

            if ( securitySettings.DisablePasswordlessSignInForAccountProtectionProfiles.Contains( person.AccountProtectionProfile ) )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the passwordless login start request is valid.
        /// </summary>
        /// <param name="sendOneTimePasscodeRequest">The passwordless login request.</param>
        /// <returns>
        ///   <c>true</c> if the request is valid; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsRequestValid( SendOneTimePasscodeOptions sendOneTimePasscodeRequest )
        {
            if ( sendOneTimePasscodeRequest == null )
            {
                return false;
            }

            // Individual must opt in to sending a code or a link via SMS or Email.
            if ( !sendOneTimePasscodeRequest.ShouldSendEmailCode
                 && !sendOneTimePasscodeRequest.ShouldSendEmailLink
                 && !sendOneTimePasscodeRequest.ShouldSendSmsCode )
            {
                return false;
            }

            if ( sendOneTimePasscodeRequest.ShouldSendSmsCode && sendOneTimePasscodeRequest.PhoneNumber.IsNullOrWhiteSpace() )
            {
                return false;
            }

            if ( ( sendOneTimePasscodeRequest.ShouldSendEmailCode || sendOneTimePasscodeRequest.ShouldSendEmailLink ) && sendOneTimePasscodeRequest.Email.IsNullOrWhiteSpace() )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the passwordless login start request is valid.
        /// </summary>
        /// <param name="verifyOneTimePasscodeOptions">The verify OTP options.</param>
        /// <param name="state">The passwordless authentication state that will be set if the request is valid.</param>
        /// <returns>
        ///   <c>true</c> if the request is valid; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsRequestValid( OneTimePasscodeAuthenticationOptions verifyOneTimePasscodeOptions, out PasswordlessAuthenticationState state )
        {
            if ( verifyOneTimePasscodeOptions?.State.IsNullOrWhiteSpace() == true )
            {
                state = null;
                return false;
            }

            state = GetDecryptedAuthenticationState( verifyOneTimePasscodeOptions.State );

            if ( state == null )
            {
                return false;
            }

            if ( verifyOneTimePasscodeOptions.Code != state.Code )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether two-factor authentication is required for <paramref name="person"/>.
        /// </summary>
        /// <param name="person">The person to check.</param>
        private static bool IsTwoFactorAuthenticationRequiredForProtectionProfile( Person person )
        {
            if ( person == null )
            {
                return false;
            }

            return IsTwoFactorAuthenticationRequiredForProtectionProfile( person.AccountProtectionProfile );
        }

        /// <summary>
        /// Determines whether two-factor authentication is required for <paramref name="protectionProfile"/>.
        /// </summary>
        /// <param name="protectionProfile">The protection profile to check.</param>
        private static bool IsTwoFactorAuthenticationRequiredForProtectionProfile( AccountProtectionProfile protectionProfile )
        {
            var securitySettings = new SecuritySettingsService().SecuritySettings;

            return securitySettings?.RequireTwoFactorAuthenticationForAccountProtectionProfiles?.Contains( protectionProfile ) == true;
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// The state for a passwordless matching person.
        /// </summary>
        private class PasswordlessMatchingPersonState
        {
            /// <summary>
            /// Gets or sets the person identifier.
            /// </summary>
            /// <value>
            /// The person identifier.
            /// </value>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets the full name.
            /// </summary>
            /// <value>
            /// The full name.
            /// </value>
            public string FullName { get; set; }
        }

        #endregion
    }
}
