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

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Model;
using Rock.Security;
using Rock.Utility.Enums;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Security.PhoneNumberIdentification;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

using Authorization = Rock.Security.Authorization;

namespace Rock.Blocks.Security
{
    /// <summary>
    /// Allows an individual to identify themselves by verifying ownership of a phone number.
    /// </summary>
    [DisplayName( "Phone Number Lookup" )]
    [Category( "Security" )]
    [Description( "Log in via phone number." )]
    [IconCssClass( "ti ti-phone" )]

    #region Block Attributes

    [CustomDropdownListField(
        "Authentication Level",
        Description = "This determines what level of authentication that the lookup would do.",
        ListSource = "10^Trusted Login,30^Identified",
        DefaultValue = "30",
        IsRequired = true,
        Order = 1,
        Key = AttributeKey.AuthenticationLevel )]

    [TextField(
        "Title",
        Description = "The title for the block text.",
        IsRequired = false,
        DefaultValue = "Individual Lookup",
        Order = 2,
        Key = AttributeKey.Title )]

    [CodeEditorField(
        "Initial Instructions",
        Description = "The instructions to show on the initial screen.<span class='tip tip-lava'></span><span class='tip tip-html'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 100,
        DefaultValue = "Please enter your mobile phone number below. We’ll use this number for verification.",
        IsRequired = false,
        Order = 3,
        Key = AttributeKey.InitialInstructions )]

    [CodeEditorField(
        "Verification Instructions",
        Description = "The instructions to show on the Verification screen.<span class='tip tip-lava'></span><span class='tip tip-html'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 100,
        DefaultValue = "Please enter the six digit confirmation code below.",
        IsRequired = false,
        Order = 4,
        Key = AttributeKey.VerificationInstructions )]

    [CodeEditorField(
        "Individual Selection Instructions",
        Description = "The instructions to show on the Individual Selection screen.<span class='tip tip-lava'></span><span class='tip tip-html'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 100,
        DefaultValue = "The phone number provided matches several individuals in our records. Please select yourself from the list.",
        IsRequired = false,
        Order = 5,
        Key = AttributeKey.IndividualSelectionInstructions )]

    [CodeEditorField(
        "Phone Number Not Found Message",
        Description = "The instructions to show when the phone number is not found in Rock after the phone number has been verified.<span class='tip tip-lava'></span><span class='tip tip-html'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 100,
        DefaultValue = "We did not find the phone number you provided in our records.",
        IsRequired = false,
        Order = 6,
        Key = AttributeKey.PhoneNumberNotFoundMessage )]

    [CodeEditorField(
        "Text Message Template",
        Description = "The template to use for the SMS message.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = "Your {{ 'Global' | Attribute:'OrganizationName' }} verification code is {{ ConfirmationCode }}",
        Order = 7,
        Key = AttributeKey.TextMessageTemplate )]

    [SystemPhoneNumberField(
        "SMS Number",
        Description = "The phone number SMS messages should be sent from.",
        IsRequired = true,
        Order = 8,
        Key = AttributeKey.SmsNumber )]

    [IntegerField(
        "Verification Time Limit",
        Description = "The number of minutes that the user has to verify their phone number.",
        DefaultIntegerValue = 5,
        Order = 9,
        Key = AttributeKey.VerificationTimeLimit )]

    [IntegerField(
        "Validation Code Attempts",
        Description = "The number of times a validation code verification can be re-tried before failing permanently.",
        DefaultIntegerValue = IdentityVerification.DefaultMaxFailedMatchAttemptCount,
        Order = 10,
        Key = AttributeKey.ValidationCodeAttempts )]

    [IntegerField(
        "IP Throttle Limit",
        Description = "The number of times a single IP address can submit phone numbers for verification per day.",
        DefaultIntegerValue = 5000,
        Order = 11,
        Key = AttributeKey.IpThrottleLimit )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "3A93E616-640B-4054-85FC-267F36DF06B5" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "B2894BC5-CDF9-4A9E-A7E8-B4AE64B7F16B" )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.PHONE_NUMBER_LOOKUP )]
    public class PhoneNumberIdentification : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string AuthenticationLevel = "AuthenticationLevel";
            public const string TextMessageTemplate = "TextMessageTemplate";
            public const string Title = "Title";
            public const string InitialInstructions = "InitialInstructions";
            public const string VerificationInstructions = "VerificationInstructions";
            public const string IndividualSelectionInstructions = "IndividualSelectionInstructions";
            public const string PhoneNumberNotFoundMessage = "PhoneNumberNotFoundMessage";
            public const string VerificationTimeLimit = "VerificationTimeLimit";
            public const string IpThrottleLimit = "IPThrottleLimit";
            public const string SmsNumber = "SMSNumber";
            public const string ValidationCodeAttempts = "ValidationCodeAttempts";
        }

        private static class PageParameterKey
        {
            public const string ReturnUrl = "returnUrl";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<PhoneNumberIdentificationBag, PhoneNumberIdentificationOptionsBag>();
            var mergeFields = RequestContext.GetCommonMergeFields();

            box.Bag = new PhoneNumberIdentificationBag
            {
                Title = GetAttributeValue( AttributeKey.Title ),
                InitialInstructions = GetAttributeValue( AttributeKey.InitialInstructions ).ResolveMergeFields( mergeFields ),
                VerificationInstructions = GetAttributeValue( AttributeKey.VerificationInstructions ).ResolveMergeFields( mergeFields ),
                IndividualSelectionInstructions = GetAttributeValue( AttributeKey.IndividualSelectionInstructions ).ResolveMergeFields( mergeFields ),
                PhoneNumberNotFoundMessage = GetAttributeValue( AttributeKey.PhoneNumberNotFoundMessage ).ResolveMergeFields( mergeFields ),
                IsConfigured = GetAttributeValue( AttributeKey.SmsNumber ).IsNotNullOrWhiteSpace()
            };

            return box;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Creates a verification record for the supplied phone number and texts a confirmation code to it.
        /// </summary>
        /// <param name="bag">The request containing the phone number to verify.</param>
        /// <returns>An encrypted token that identifies the verification record for the follow-up verify call.</returns>
        [BlockAction]
        public BlockActionResult SendVerificationCode( PhoneNumberIdentificationSendCodeRequestBag bag )
        {
            var smsNumberGuid = GetAttributeValue( AttributeKey.SmsNumber );
            if ( smsNumberGuid.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "This block is not yet configured for use." );
            }

            var phoneNumber = bag?.PhoneNumber;
            if ( phoneNumber.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Please enter a phone number." );
            }

            var countryCode = bag.CountryCode;
            var smsRecipientNumber = PhoneNumber.CleanNumber( phoneNumber );
            if ( countryCode.IsNotNullOrWhiteSpace() && countryCode != PhoneNumber.DefaultCountryCode() )
            {
                smsRecipientNumber = $"+{countryCode}{smsRecipientNumber}";
            }

            var ipLimit = GetAttributeValue( AttributeKey.IpThrottleLimit ).AsInteger();
            var messageTemplate = GetAttributeValue( AttributeKey.TextMessageTemplate );

            try
            {
                var identityVerificationService = new IdentityVerificationService( RockContext );
                var identityVerification = identityVerificationService.CreateIdentityVerificationRecord( RequestContext.ClientInformation.IpAddress, ipLimit, phoneNumber );

                if ( identityVerification == null )
                {
                    return ActionBadRequest( "Unable to create a verification code. Please try again." );
                }

                var mergeFields = RequestContext.GetCommonMergeFields();
                mergeFields.Add( "ConfirmationCode", identityVerification.IdentityVerificationCode.Code );

                var smsMessage = new RockSMSMessage
                {
                    FromSystemPhoneNumber = SystemPhoneNumberCache.Get( smsNumberGuid.AsGuid() ),
                    Message = messageTemplate
                };
                smsMessage.SetRecipients( new List<RockSMSMessageRecipient>
                {
                    RockSMSMessageRecipient.CreateAnonymous( smsRecipientNumber, mergeFields )
                } );

                if ( !smsMessage.Send( out _ ) )
                {
                    return ActionBadRequest( "Verification text message failed to send." );
                }

                var verificationToken = Encryption.EncryptString( identityVerification.Id.ToString() );
                return ActionOk( verificationToken );
            }
            catch ( IdentityVerificationIpLimitReachedException )
            {
                // The IP throttle limit was reached. Return a fixed, user-actionable message rather than the
                // exception's text so that nothing internal can ever surface to the caller.
                return ActionBadRequest( "You have requested too many verification codes today. Please contact your organization's administrator for assistance." );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                return ActionBadRequest( "Something went wrong while sending your verification code. Please try again." );
            }
        }

        /// <summary>
        /// Verifies a confirmation code and resolves the people associated with the verified phone number.
        /// </summary>
        /// <param name="bag">The request containing the verification token and the entered code.</param>
        /// <returns>
        /// A redirect URL when a single person was matched and authenticated, the list of people to choose from
        /// when multiple were matched, or a flag indicating the phone number was not found.
        /// </returns>
        [BlockAction]
        public BlockActionResult VerifyCode( PhoneNumberIdentificationVerifyRequestBag bag )
        {
            var identityVerificationId = DecryptVerificationId( bag?.VerificationToken );
            if ( !identityVerificationId.HasValue )
            {
                return ActionBadRequest( "Your session has expired. Please start over." );
            }

            var identityVerificationService = new IdentityVerificationService( RockContext );
            if ( !IsVerificationCodeValid( identityVerificationService, identityVerificationId.Value, bag.Code ) )
            {
                return ActionBadRequest( "The verification code information entered is either incorrect or expired." );
            }

            // Re-read the phone number from the verification record rather than trusting a client-supplied value.
            var phoneNumber = identityVerificationService.Get( identityVerificationId.Value )?.ReferenceNumber;
            if ( phoneNumber.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Your session has expired. Please start over." );
            }

            var personIds = new PhoneNumberService( RockContext ).GetPersonIdsByNumber( phoneNumber ).ToList();

            if ( personIds.Count == 0 )
            {
                return ActionOk( new PhoneNumberIdentificationVerifyResponseBag { IsPhoneNumberNotFound = true } );
            }

            if ( personIds.Count == 1 )
            {
                return ActionOk( new PhoneNumberIdentificationVerifyResponseBag
                {
                    RedirectUrl = AuthenticatePerson( personIds[0], phoneNumber )
                } );
            }

            var people = new PersonService( RockContext ).Queryable()
                .Where( p => personIds.Contains( p.Id ) )
                .ToList()
                .Select( p => new ListItemBag { Value = p.IdKey, Text = p.FullName } )
                .ToList();

            return ActionOk( new PhoneNumberIdentificationVerifyResponseBag
            {
                IsPersonSelectionRequired = true,
                People = people
            } );
        }

        /// <summary>
        /// Authenticates the person the individual selected from multiple phone number matches.
        /// </summary>
        /// <param name="bag">The request containing the verification token, the entered code, and the selected person.</param>
        /// <returns>The URL to redirect to after authentication.</returns>
        [BlockAction]
        public BlockActionResult AuthenticateSelectedPerson( PhoneNumberIdentificationAuthenticateRequestBag bag )
        {
            var identityVerificationId = DecryptVerificationId( bag?.VerificationToken );
            if ( !identityVerificationId.HasValue )
            {
                return ActionBadRequest( "Your session has expired. Please start over." );
            }

            var identityVerificationService = new IdentityVerificationService( RockContext );
            if ( !IsVerificationCodeValid( identityVerificationService, identityVerificationId.Value, bag.Code ) )
            {
                return ActionBadRequest( "The verification code information entered is either incorrect or expired." );
            }

            var phoneNumber = identityVerificationService.Get( identityVerificationId.Value )?.ReferenceNumber;
            if ( phoneNumber.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Your session has expired. Please start over." );
            }

            var selectedPerson = new PersonService( RockContext ).Get( bag.PersonValue, !PageCache.Layout.Site.DisablePredictableIds );
            if ( selectedPerson == null )
            {
                return ActionBadRequest( "We could not complete your request." );
            }

            // Only allow authenticating as a person that actually matches the verified phone number.
            var matchedPersonIds = new PhoneNumberService( RockContext ).GetPersonIdsByNumber( phoneNumber ).ToList();
            if ( !matchedPersonIds.Contains( selectedPerson.Id ) )
            {
                return ActionBadRequest( "We could not complete your request." );
            }

            return ActionOk( AuthenticatePerson( selectedPerson.Id, phoneNumber ) );
        }

        #endregion Block Actions

        #region Private Methods

        /// <summary>
        /// Checks whether the entered code is valid for the verification record, honoring the configured time
        /// limit and maximum number of attempts.
        /// </summary>
        /// <param name="identityVerificationService">The service used to verify the code.</param>
        /// <param name="identityVerificationId">The identifier of the verification record.</param>
        /// <param name="code">The code entered by the individual.</param>
        /// <returns><c>true</c> if the code is valid; otherwise <c>false</c>.</returns>
        private bool IsVerificationCodeValid( IdentityVerificationService identityVerificationService, int identityVerificationId, string code )
        {
            var timeLimit = GetAttributeValue( AttributeKey.VerificationTimeLimit ).AsInteger();
            var validationAttempts = GetAttributeValue( AttributeKey.ValidationCodeAttempts ).AsInteger();

            return identityVerificationService.VerifyIdentityVerificationCode( identityVerificationId, timeLimit, code, validationAttempts );
        }

        /// <summary>
        /// Authenticates the specified person according to the configured authentication level and returns the URL
        /// the browser should be redirected to. The redirect itself is performed by the client.
        /// </summary>
        /// <param name="personId">The identifier of the person to authenticate.</param>
        /// <param name="phoneNumber">The verified phone number, recorded against the login history.</param>
        /// <returns>The safe URL to redirect to after authentication.</returns>
        private string AuthenticatePerson( int personId, string phoneNumber )
        {
            var authenticationLevel = GetAttributeValue( AttributeKey.AuthenticationLevel ).AsInteger();
            var person = new PersonService( RockContext ).Get( personId );
            var impersonationQueryParam = string.Empty;

            if ( person != null )
            {
                if ( authenticationLevel == ( int ) Authorization.AuthenticationLevel.TrustedLogin )
                {
                    var userLogin = person.Users
                        .Where( u => u.IsConfirmed ?? true )
                        .Where( u => !( u.IsLockedOut ?? false ) )
                        .FirstOrDefault();

                    if ( userLogin != null )
                    {
                        /*
                            6/3/2026 - MSE

                            When a protection profile requires two-factor authentication, this phone-verified login
                            satisfies the second factor, so the auth cookie is created with the two-factor flag set
                            to avoid prompting again. When 2FA is not required the flag is simply false.

                            Reason: Two-Factor Authentication
                        */
                        var isTwoFactorAuthenticated = IsTwoFactorAuthenticationRequired( person.AccountProtectionProfile );

                        Authorization.SetAuthCookie( userLogin.UserName, isPersisted: false, isImpersonated: false, isTwoFactorAuthenticated );

                        new HistoryLogin
                        {
                            UserName = phoneNumber,
                            PersonAliasId = person.PrimaryAliasId,
                            SourceSiteId = PageCache?.SiteId,
                            WasLoginSuccessful = true
                        }
                        .WithContext( "Phone Number Lookup" )
                        .SaveAfterDelay();
                    }
                    else
                    {
                        var impersonationToken = person.GetImpersonationToken( RockDateTime.Now.AddMinutes( 5 ), 1, null );
                        impersonationQueryParam = $"rckipid={impersonationToken}";
                    }
                }
                else if ( authenticationLevel == ( int ) Authorization.AuthenticationLevel.Identified )
                {
                    if ( person.PrimaryAlias != null )
                    {
                        Authorization.SetUnsecurePersonIdentifier( person.PrimaryAlias.Guid );
                    }
                }
            }

            var returnUrl = GetSafeDecodedUrl( PageParameter( PageParameterKey.ReturnUrl ) );
            if ( returnUrl.IsNullOrWhiteSpace() )
            {
                returnUrl = "/";
            }

            if ( impersonationQueryParam.IsNotNullOrWhiteSpace() )
            {
                returnUrl += returnUrl.Contains( "?" ) ? $"&{impersonationQueryParam}" : $"?{impersonationQueryParam}";
            }

            return returnUrl;
        }

        /// <summary>
        /// Determines whether two-factor authentication is required for the specified protection profile.
        /// </summary>
        /// <param name="protectionProfile">The account protection profile to check.</param>
        /// <returns><c>true</c> if two-factor authentication is required; otherwise <c>false</c>.</returns>
        private static bool IsTwoFactorAuthenticationRequired( AccountProtectionProfile protectionProfile )
        {
            var securitySettings = new SecuritySettingsService().SecuritySettings;
            return securitySettings?.RequireTwoFactorAuthenticationForAccountProtectionProfiles?.Contains( protectionProfile ) == true;
        }

        /// <summary>
        /// Returns the decoded URL if it is a safe local path, or <c>null</c> if it is missing, not a
        /// local same-origin path, or contains XSS.
        /// </summary>
        /// <param name="url">The raw return URL from the page parameter.</param>
        /// <returns>The decoded, safe local URL; otherwise <c>null</c>.</returns>
        private string GetSafeDecodedUrl( string url )
        {
            if ( url.IsNullOrWhiteSpace() )
            {
                return url;
            }

            var decodedUrl = url.GetFullyUrlDecodedValue();

            if ( decodedUrl.Any( char.IsControl ) )
            {
                return null;
            }

            /*
                6/3/2026 - MSE

                The return URL must be a local (same-origin) path. When authenticating a person who has no
                user login, this block appends a short-lived impersonation token (rckipid) to the return URL,
                so allowing an absolute or protocol-relative URL here would let a crafted "returnUrl" parameter
                redirect the individual off-site and leak that token to an external host. A leading "/" that is
                not "//" or "/\" identifies a local path. The sibling Login block does not append a credential
                to its return URL, which is why this block validates the URL more strictly.

                Reason: Prevent open redirect and impersonation-token leakage.
            */
            var isLocalPath = decodedUrl.StartsWith( "/" )
                && !decodedUrl.StartsWith( "//" )
                && !decodedUrl.StartsWith( "/\\" );
            if ( !isLocalPath )
            {
                return null;
            }

            // Reject a local path that still carries an XSS payload, for example in the query string.
            if ( decodedUrl.RedirectUrlContainsXss() )
            {
                return null;
            }

            return decodedUrl;
        }

        /// <summary>
        /// Decrypts the verification token back into the verification record identifier.
        /// </summary>
        /// <param name="verificationToken">The encrypted token previously issued to the client.</param>
        /// <returns>The verification record identifier, or <c>null</c> if the token is missing or invalid.</returns>
        private static int? DecryptVerificationId( string verificationToken )
        {
            if ( verificationToken.IsNullOrWhiteSpace() )
            {
                return null;
            }

            try
            {
                return Encryption.DecryptString( verificationToken ).AsIntegerOrNull();
            }
            catch
            {
                // Intentionally ignored: a malformed or tampered token cannot be resolved and is treated as an expired session.
                return null;
            }
        }

        #endregion Private Methods
    }
}
