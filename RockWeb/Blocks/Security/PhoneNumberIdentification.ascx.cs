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
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Lava;
using Rock.Logging;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Security
{
    /// <summary>
    /// Allows user to verify themselves via their phone number.
    /// </summary>
    [DisplayName( "Phone Number Lookup" )]
    [Category( "Security" )]
    [Description( "Log in via phone number" )]

    [CustomDropdownListField( "Authentication Level",
        Description = "This determines what level of authentication that the lookup would do.",
        ListSource = "10^Trusted Login,30^Identified",
        DefaultValue = "30",
        IsRequired = true,
        Order = 4,
        Key = AttributeKey.AuthenticationLevel )]

    [CodeEditorField(
        "Text Message Template",
        Description = "The template to use for the SMS message.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = "Your {{ 'Global' | Attribute:'OrganizationName' }} verification code is {{ ConfirmationCode }}",
        Order = 2,
        Key = AttributeKey.TextMessageTemplate )]

    [TextField(
        "Title",
        Description = "The title for the block text.",
        IsRequired = false,
        DefaultValue = "Individual Lookup",
        Order = 3,
        Key = AttributeKey.Title )]

    [CodeEditorField(
        "Initial Instructions",
        Description = "The instructions to show on the initial screen.<span class='tip tip-lava'></span><span class='tip tip-html'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        DefaultValue = "Please enter your mobile phone number below. We’ll use this number for verification.",
        IsRequired = false,
        Order = 4,
        Key = AttributeKey.InitialInstructions )]

    [CodeEditorField(
        "Verification Instructions",
        Description = "The instructions to show on the Verification screen.<span class='tip tip-lava'></span><span class='tip tip-html'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        DefaultValue = "Please enter the six digit confirmation code below.",
        IsRequired = false,
        Order = 5,
        Key = AttributeKey.VerificationInstructions )]

    [CodeEditorField(
        "Individual Selection Instructions",
        Description = "The instructions to show on the Individual Selection screen.<span class='tip tip-lava'></span><span class='tip tip-html'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        DefaultValue = "The phone number provided matches several individuals in our records. Please select yourself from the list.",
        IsRequired = false,
        Order = 6,
        Key = AttributeKey.IndividualSelectionInstructions )]

    [CodeEditorField(
        "Phone Number Not Found Message",
        Description = "The instructions to show when the phone number is not found in Rock after the phone number has been verified.<span class='tip tip-lava'></span><span class='tip tip-html'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        DefaultValue = "We did not find the phone number you provided in our records.",
        IsRequired = false,
        Order = 7,
        Key = AttributeKey.PhoneNumberNotFoundMessage )]

    [IntegerField(
        "Verification Time Limit",
        Description = "The number of minutes that the user has to verify their phone number.",
        DefaultIntegerValue = 5,
        Order = 8,
        Key = AttributeKey.VerificationTimeLimit )]

    [IntegerField(
        "IP Throttle Limit",
        Description = "The number of times a single IP address can submit phone numbers for verification per day.",
        DefaultIntegerValue = 5000,
        Order = 9,
        Key = AttributeKey.IpThrottleLimit )]

    [SystemPhoneNumberField(
        "SMS Number",
        Key = AttributeKey.SmsNumber,
        Description = "The phone number SMS messages should be sent from",
        Order = 10 )]

    [IntegerField(
        "Validation Code Attempts",
        Description = "The number of times a validation code verification can be re-tried before failing permanently.",
        DefaultIntegerValue = IdentityVerification.DefaultMaxFailedMatchAttemptCount,
        Order = 11,
        Key = AttributeKey.ValidationCodeAttempts )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.PHONE_NUMBER_LOOKUP )]
    public partial class PhoneNumberIdentification : Rock.Web.UI.RockBlock
    {
        private int IdentityVerificationId
        {
            get
            {
                return Encryption.DecryptString( ViewState["IdentityVerificationId"].ToString() ).AsInteger();
            }
            set
            {
                ViewState["IdentityVerificationId"] = Encryption.EncryptString( value.ToString() );
            }
        }

        private class AttributeKey
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

        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.BlockUpdated += PhoneNumberIdentification_BlockUpdated;

            ApplyBlockSettings();
        }

        protected override void OnLoad( EventArgs e )
        {
            ValidateBlockConfigured();

            if ( Page.IsPostBack )
            {
                nbWarningMessage.Visible = false;
            }

            base.OnLoad( e );
        }

        private void ValidateBlockConfigured()
        {
            if ( GetAttributeValue( AttributeKey.SmsNumber ).IsNullOrWhiteSpace() )
            {
                nbConfigurationError.Visible = true;
                pbPhoneNumberLookup.Enabled = false;
                btnLookup.Enabled = false;
            }
        }

        #endregion

        #region Events

        protected void btnLookup_Click( object sender, EventArgs e )
        {
            if ( !Page.IsValid )
            {
                return;
            }

            var ipLimit = GetAttributeValue( AttributeKey.IpThrottleLimit ).AsInteger();
            var messageTemplate = GetAttributeValue( AttributeKey.TextMessageTemplate );
            var fromNumber = GetAttributeValue( AttributeKey.SmsNumber );
            var phoneNumber = pbPhoneNumberLookup.Number;

            try
            {
                using ( var rockContext = new RockContext() )
                {
                    var identityVerificationService = new IdentityVerificationService( rockContext );

                    var identityVerification = identityVerificationService.CreateIdentityVerificationRecord( Request.UserHostAddress, ipLimit, phoneNumber );

                    var smsMessage = new RockSMSMessage
                    {
                        FromSystemPhoneNumber = SystemPhoneNumberCache.Get( fromNumber ),
                        Message = messageTemplate,
                    };
                    var mergeObjects = LavaHelper.GetCommonMergeFields( this.RockPage );
                    mergeObjects.Add( "ConfirmationCode", identityVerification.IdentityVerificationCode.Code );

                    smsMessage.SetRecipients( new List<RockSMSMessageRecipient> {
                        RockSMSMessageRecipient.CreateAnonymous( phoneNumber, mergeObjects )
                    } );

                    var errorList = new List<string>();
                    if ( smsMessage.Send( out errorList ) )
                    {
                        IdentityVerificationId = identityVerification.Id;
                        ShowVerificationPage();
                    }
                    else
                    {
                        ShowWarningMessage( "Verification text message failed to send." );
                    }
                }
            }
            catch ( Exception ex )
            {
                ShowWarningMessage( ex.Message );
                RockLogger.Log.Error( RockLogDomains.Core, ex );
                ExceptionLogService.LogException( ex );
            }
        }

        protected void btnPersonChooser_Command( object sender, CommandEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( IsVerificationCodeValid( rockContext ) )
                {
                    var personId = e.CommandArgument.ToString().AsInteger();
                    if ( personId > 0 )
                    {
                        AuthenticatePerson( personId );
                    }
                }

            }
        }

        protected void btnVerify_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !IsVerificationCodeValid( rockContext ) )
                {
                    ShowWarningMessage( "The verification code information entered is either incorrect or expired." );
                    ShowVerificationPage();
                    return;
                }

                var phoneNumberService = new PhoneNumberService( rockContext );
                var phoneNumber = pbPhoneNumberLookup.Number;
                var personIds = phoneNumberService.GetPersonIdsByNumber( phoneNumber ).ToList();

                if ( personIds.Count == 0 )
                {
                    ShowPersonNotFound();
                    return;
                }

                if ( personIds.Count == 1 )
                {
                    AuthenticatePerson( personIds[0] );
                    return;
                }

                if ( personIds.Count > 1 )
                {
                    ShowPersonChooser( personIds );
                    return;
                }
            }
        }

        protected void btnResend_Click( object sender, EventArgs e )
        {
            ShowPhoneNumberEntryPage();
        }

        private void PhoneNumberIdentification_BlockUpdated( object sender, EventArgs e )
        {
            ApplyBlockSettings();
            ValidateBlockConfigured();
        }

        #endregion

        #region Private Helper Methods

        private bool IsVerificationCodeValid( RockContext rockContext )
        {
            var timeLimit = GetAttributeValue( AttributeKey.VerificationTimeLimit ).AsInteger();
            var validationAttempts = GetAttributeValue( AttributeKey.ValidationCodeAttempts ).AsInteger();

            var verificationCode = string.Format( "{0}{1}{2}{3}{4}{5}",
                nbVerificationCodeBox1.Text,
                nbVerificationCodeBox2.Text,
                nbVerificationCodeBox3.Text,
                nbVerificationCodeBox4.Text,
                nbVerificationCodeBox5.Text,
                nbVerificationCodeBox6.Text );

            var identityVerificationService = new IdentityVerificationService( rockContext );
            return identityVerificationService.VerifyIdentityVerificationCode( IdentityVerificationId, timeLimit, verificationCode, validationAttempts );
        }

        private void RegisterVerificationCodeScript()
        {
            var androidScript = @"
                $(function () {
                    const inputElements = [...document.querySelectorAll('input.js-verification-code')];
                    inputElements.forEach((ele, index) => {
                        ele.addEventListener('keydown', (e) => {
                            // if the keycode is backspace & the current field is empty
                            // focus the input before the current. Then the event happens
                            // which will clear the 'before' input box.
                            if (e.keyCode === 8 && e.target.value === '') inputElements[Math.max(0, index - 1)].focus()
                        });
                        ele.addEventListener('input', (e) => {
                            // take the first character of the input
                            const [first, ...rest] = e.target.value
                            e.target.value = first ?? '' // first will be undefined when backspace was entered, so set the input to ""
                            const lastInputBox = index === inputElements.length - 1
                            const didInsertContent = first !== undefined
                            if (didInsertContent && !lastInputBox) {
                                // continue to input the rest of the string
                                inputElements[index + 1].focus()
                                inputElements[index + 1].value = rest.join('') // set the rest of the values as the value for the next input and trigger the input event so the cycle is repeated
                                inputElements[index + 1].dispatchEvent(new Event('input'))
                            }
                            if(lastInputBox){
                                $('.js-verify-button').focus();
                            }
                        });
                    });
                });";

            var nonAndroidScript = @"
                $(function () {
                    $('.js-code-1').focus();

                    var codeInputs = $('.js-verification-code');

                    codeInputs.on('paste', function () {
                        var self = $(this);
                        var originalValue = self.val();

                        self.val('');

                        self.one('input', function () {
                            var intRegex = /^\d+$/;
                            $currentInputBox = $(this);

                            var pastedValue = $currentInputBox.val();

                            if (pastedValue.length == 6 && intRegex.test(pastedValue)) {
                                pasteValues(pastedValue);
                                $('.js-verify-button').focus();
                            } else {
                                self.val(originalValue);
                            }
                        });
                    });

                    codeInputs.on('keydown', function (event) {
                        var self = $(this);
                        var value = self.val();
                        var key = event.keyCode || event.charCode;

                        if ((key === 8 || key === 46) && value.length >= 1) {
                            return;
                        }

                        if (key === 8) {
                            var boxNumber = parseInt(self.attr('data-box-number'));
                            if (boxNumber && boxNumber > 1 && boxNumber <= 6) {
                                var prevBox = $('.js-code-' + (boxNumber - 1));
                                prevBox.val('');
                                prevBox.focus();
                            }
                            event.preventDefault();
                            return;
                        }

                        if (value.length >= 1 && !event.ctrlKey) {
                            event.preventDefault();
                            return;
                        }

                        if (!event.ctrlKey) {
                            self.one('input', function () {
                                var self = $(this);
                                var boxNumber = parseInt(self.attr('data-box-number'));
                                if (boxNumber && boxNumber < 6) {
                                    $('.js-code-' + (boxNumber + 1)).focus();
                                }
                                if (boxNumber === 6) {
                                    $('.js-verify-button').focus();
                                }
                            });
                        }
                    });
                });

                function pasteValues(element) {
                    var values = element.split('');

                    $(values).each(function (index) {
                        var $inputBox = $('.js-code-' + (index + 1));
                        $inputBox.val(values[index])
                    });
                }
            ";

            var script = Request.UserAgent.IndexOf( "android", StringComparison.OrdinalIgnoreCase ) >= 0 ? androidScript : nonAndroidScript;
            ScriptManager.RegisterStartupScript( pnlVerificationCodeEntry, pnlVerificationCodeEntry.GetType(), "verificationCode", script, true );
        }

        private void ShowPhoneNumberEntryPage()
        {
            pnlPhoneNumberEntry.Visible = true;
            pnlPersonChooser.Visible = false;
            pnlNotFound.Visible = false;
            pnlVerificationCodeEntry.Visible = false;
        }

        private void ShowVerificationPage()
        {
            pnlPhoneNumberEntry.Visible = false;
            pnlPersonChooser.Visible = false;
            pnlNotFound.Visible = false;
            pnlVerificationCodeEntry.Visible = true;
            RegisterVerificationCodeScript();
        }

        private void ShowPersonNotFound()
        {
            pnlPhoneNumberEntry.Visible = false;
            pnlPersonChooser.Visible = false;
            pnlNotFound.Visible = true;
            pnlVerificationCodeEntry.Visible = false;
        }

        private void ShowPersonChooser( List<int> personIds )
        {
            pnlPhoneNumberEntry.Visible = false;
            pnlPersonChooser.Visible = true;
            pnlNotFound.Visible = false;
            pnlVerificationCodeEntry.Visible = false;

            var personService = new PersonService( new RockContext() );
            var people = personService.Queryable().Where( p => personIds.Contains( p.Id ) );
            rptPeople.DataSource = people.ToList();
            rptPeople.DataBind();
        }

        private void AuthenticatePerson( int personId )
        {
            var authenticationLevel = GetAttributeValue( AttributeKey.AuthenticationLevel ).AsInteger();
            var person = new PersonService( new RockContext() ).Get( personId );
            var user = person.Users
                .Where( u => u.IsConfirmed ?? true )
                .Where( u => !( u.IsLockedOut ?? false ) )
                .FirstOrDefault();
            var qryParams = string.Empty;

            switch ( authenticationLevel )
            {
                case ( int ) Rock.Security.Authorization.AuthenticationLevel.TrustedLogin:
                    if ( user != null )
                    {
                        var userName = user.UserName;
                        UserLoginService.UpdateLastLogin( userName );

                        /*
                            10/20/2023 - JMH

                            If 2FA is required for the person's protection profile,
                            then 2FA will need to be bypassed here by hard-coding a true value in their auth cookie.

                            If 2FA is not required, then the auth cookie will be created without bypassing 2FA
                            since there is no need to bypass it.

                            Reason: Two-Factor Authentication
                         */
                        var isTwoFactorAuthenticated = false;
                        var securitySettings = new SecuritySettingsService().SecuritySettings;

                        if ( securitySettings.RequireTwoFactorAuthenticationForAccountProtectionProfiles?.Contains( person.AccountProtectionProfile ) == true )
                        {
                            isTwoFactorAuthenticated = true;
                        }

                        Authorization.SetAuthCookie(
                            userName,
                            isPersisted: false,
                            isImpersonated: false,
                            isTwoFactorAuthenticated );
                    }
                    else
                    {
                        var impersonationToken = person.GetImpersonationToken( RockDateTime.Now.AddMinutes( 5 ), 1, null );
                        qryParams = string.Format( "{0}={1}", "rckipid", impersonationToken );
                    }

                    break;
                case ( int ) Rock.Security.Authorization.AuthenticationLevel.Identified:
                    Authorization.SetUnsecurePersonIdentifier( person.PrimaryAlias.Guid );
                    break;
            }

            var returnUrl = PageParameter( "returnUrl" );
            if ( returnUrl.IsNullOrWhiteSpace() || returnUrl.RedirectUrlContainsXss() )
            {
                returnUrl = "/";
            }
            else
            {
                returnUrl = Server.UrlDecode( returnUrl );
            }

            if ( qryParams.IsNotNullOrWhiteSpace() )
            {
                if ( returnUrl.Contains( "?" ) )
                {
                    returnUrl = string.Format( "{0}&{1}", returnUrl, qryParams );
                }
                else
                {
                    returnUrl = string.Format( "{0}?{1}", returnUrl, qryParams );
                }
            }

            Response.Redirect( returnUrl, false );
            Context.ApplicationInstance.CompleteRequest();
        }

        private void ShowWarningMessage( string warningMessage )
        {
            nbWarningMessage.Text = warningMessage;
            nbWarningMessage.Visible = true;
        }

        private void ApplyBlockSettings()
        {
            var mergeFields = LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson );
            litTitle.Text = GetAttributeValue( AttributeKey.Title );
            litInitialInstructions.Text = GetAttributeValue( AttributeKey.InitialInstructions ).ResolveMergeFields( mergeFields );
            litIndividualSelectionInstructions.Text = GetAttributeValue( AttributeKey.IndividualSelectionInstructions ).ResolveMergeFields( mergeFields );
            litNotFoundInstructions.Text = GetAttributeValue( AttributeKey.PhoneNumberNotFoundMessage ).ResolveMergeFields( mergeFields );
            litVerificationInstructions.Text = GetAttributeValue( AttributeKey.VerificationInstructions ).ResolveMergeFields( mergeFields );
        }
        #endregion
    }
}