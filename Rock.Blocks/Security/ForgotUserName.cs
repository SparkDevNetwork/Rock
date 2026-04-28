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

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Enums.Blocks.Security.ForgotUserName;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks.Security.ForgotUserName;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Security
{
    /// <summary>
    /// Allows a user to get their forgotten username information emailed to them.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Forgot Username" )]
    [Category( "Security" )]
    [Description( "Allows a user to get their forgotten username information emailed to them." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [CodeEditorField( "Heading Caption",
        Key = AttributeKey.HeadingCaption,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Html,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = "<h5 class='text-center'>Can't log in?</h5>",
        Category = "Captions",
        Order = 0 )]

    [CodeEditorField( "Invalid Email Caption",
        Key = AttributeKey.InvalidEmailCaption,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Html,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = "Sorry, we didn't recognize that email address. Want to try another?",
        Category = "Captions",
        Order = 1 )]

    [CodeEditorField( "Success Caption",
        Key = AttributeKey.SuccessCaption,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Html,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = "We've emailed you instructions for logging in.",
        Category = "Captions",
        Order = 2 )]

    [LinkedPage( "Confirmation Page",
        Description = "Page for user to confirm their account (if blank will use 'ConfirmAccount' page route).",
        Key = AttributeKey.ConfirmationPage,
        IsRequired = false,
        Order = 3 )]

    [SystemCommunicationField( "Forgot Username Email Template",
        Key = AttributeKey.EmailTemplate,
        Description = "The email template to use when sending the forgot username (and password) email.  The following merge fields are available for use in the template: Person, Users, and SupportsChangePassword (an array of the usernames that support password changes).",
        IsRequired = false,
        DefaultSystemCommunicationGuid = Rock.SystemGuid.SystemCommunication.SECURITY_FORGOT_USERNAME,
        Order = 4 )]

    [BooleanField( "Save Communication History",
        Description = "Should a record of communication from this block be saved to the recipient's profile?",
        DefaultBooleanValue = false,
        ControlType = Rock.Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Key = AttributeKey.CreateCommunicationRecord,
        Order = 5 )]

    [BooleanField(
        "Disable Captcha Support",
        Key = AttributeKey.DisableCaptchaSupport,
        Description = "If set to 'Yes' the CAPTCHA verification step will not be performed.",
        DefaultBooleanValue = false,
        Order = 6 )]

    #endregion

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Primary )]
    [Rock.SystemGuid.EntityTypeGuid( "5BBEE600-781E-4480-8144-36F8D01C7F09" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "16CD7562-BE31-4823-9C4D-F365AB0AA5C4" )]
    [Rock.SystemGuid.BlockTypeGuid( "02B3D7D1-23CE-4154-B602-F4A15B321757" )]
    public class ForgotUserName : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string HeadingCaption = "HeadingCaption";
            public const string InvalidEmailCaption = "InvalidEmailCaption";
            public const string SuccessCaption = "SuccessCaption";
            public const string ConfirmationPage = "ConfirmationPage";
            public const string EmailTemplate = "EmailTemplate";
            public const string CreateCommunicationRecord = "CreateCommunicationRecord";
            public const string DisableCaptchaSupport = "DisableCaptchaSupport";
        }

        #endregion

        #region Properties

        private string ConfirmationPageUrl => this.GetLinkedPageUrl( AttributeKey.ConfirmationPage );

        private bool CreateCommunicationRecord => GetAttributeValue( AttributeKey.CreateCommunicationRecord ).AsBoolean();

        private Guid EmailTemplateGuid => GetAttributeValue( AttributeKey.EmailTemplate ).AsGuid();

        private string HeadingCaption => GetAttributeValue( AttributeKey.HeadingCaption );

        private string InvalidEmailCaption => GetAttributeValue( AttributeKey.InvalidEmailCaption );

        private string SuccessCaption => GetAttributeValue( AttributeKey.SuccessCaption );

        #endregion

        #region Public Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return new ForgotUserNameInitializationBox
            {
                Captions = new ForgotUserNameCaptionsBag
                {
                    HeadingCaption = this.HeadingCaption,
                    InvalidEmailCaption = this.InvalidEmailCaption,
                    SuccessCaption = this.SuccessCaption
                },
                ErrorMessage = null,
                SecurityGrantToken = null,
                DisableCaptchaSupport = Captcha.CaptchaService.ShouldDisableCaptcha( GetAttributeValue( AttributeKey.DisableCaptchaSupport ).AsBoolean() )
            };
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Sends instructions to the email in the request.
        /// </summary>
        /// <param name="bag">The request bag.</param>
        [BlockAction]
        public BlockActionResult SendInstructions( ForgotUserNameSendInstructionsRequestBag bag )
        {
            var disableCaptcha = Captcha.CaptchaService.ShouldDisableCaptcha( GetAttributeValue( AttributeKey.DisableCaptchaSupport ).AsBoolean() );

            if ( !disableCaptcha && !RequestContext.IsCaptchaValid )
            {
                return ActionBadRequest( "Captcha was not valid." );
            }

            var url = this.ConfirmationPageUrl;

            if ( string.IsNullOrWhiteSpace( url ) )
            {
                url = "/ConfirmAccount";
            }

            var rootUri = new Uri( this.RequestContext.RootUrlPath );
            var hostName = rootUri.Host;
            if ( !CheckHostConfiguration( hostName ) )
            {
                return ActionBadRequest( "Invalid request." );
            }

            var personService = new PersonService( RockContext );
            var accountTypes = new List<string>();
            var hasAccountWithPasswordResetAbility = false;
            var results = new List<IDictionary<string, object>>();
            var passwordlessAuthGuid = Rock.SystemGuid.EntityType.AUTHENTICATION_PASSWORDLESS.AsGuid();

            foreach ( var person in personService.GetByEmail( bag.Email )
                .AsNoTracking()
                .Include( p => p.Users )
                .Include( p => p.Users.Select( u => u.EntityType ) )
                .Where( p => p.Users.Any() ) )
            {
                var users = new List<UserLogin>();
                var supportsChangePassword = new List<string>();

                foreach ( var user in person.Users )
                {
                    if ( user.EntityType != null )
                    {
                        var component = AuthenticationContainer.GetComponent( user.EntityType.Name );

                        if ( component != null && !component.RequiresRemoteAuthentication )
                        {
                            if ( component.SupportsChangePassword )
                            {
                                supportsChangePassword.Add( user.UserName );
                            }

                            /*
                                3/31/26 - MSE

                                Passwordless logins (e.g., email/SMS codes) don't have a traditional
                                username to display in the forgot-username email, so we replace it
                                with a friendly label.

                                They also should not count toward hasAccountWithPasswordResetAbility
                                because the person cannot reset a password that doesn't exist;
                                if only passwordless accounts are found the block should show the
                                "not supported" warning instead.

                                The UserName can be mutated directly (instead of cloned) because
                                this query uses AsNoTracking(), so there is no risk of persisting
                                the change.

                                Reason: Parity with the original WebForms passwordless handling.
                            */
                            if ( component.TypeGuid == passwordlessAuthGuid )
                            {
                                user.UserName = "(email or mobile number)";
                                users.Add( user );
                            }
                            else
                            {
                                users.Add( user );
                                hasAccountWithPasswordResetAbility = true;
                            }
                        }

                        accountTypes.Add( user.EntityType.FriendlyName );
                    }
                }

                results.Add( new Dictionary<string, object>
                {
                    { "Person", person },
                    { "Users", users },
                    { "SupportsChangePassword", supportsChangePassword }
                } );
            }

            if ( results.Count > 0 && hasAccountWithPasswordResetAbility )
            {
                var mergeFields = this.RequestContext.GetCommonMergeFields( this.GetCurrentPerson() );
                mergeFields.Add( "ConfirmAccountUrl", RequestContext.RootUrlPath + url.TrimStart( '/' ) );
                mergeFields.Add( "Results", results.ToArray() );

                var emailMessage = new RockEmailMessage( this.EmailTemplateGuid );
                emailMessage.AddRecipient( RockEmailMessageRecipient.CreateAnonymous( bag.Email, mergeFields ) );
                emailMessage.AppRoot = RequestContext.RootUrlPath.EnsureTrailingForwardslash();
                emailMessage.ThemeRoot = this.RequestContext.ResolveRockUrl( "~~/" );
                emailMessage.CreateCommunicationRecord = this.CreateCommunicationRecord;
                emailMessage.Send();

                return ActionOk( new ForgotUserNameSendInstructionsResultBag
                {
                    ResultType = SendInstructionsResultType.InstructionsSent
                } );
            }
            else if ( results.Count > 0 )
            {
                return ActionOk( new ForgotUserNameSendInstructionsResultBag
                {
                    ResultType = SendInstructionsResultType.ChangePasswordNotSupported,
                    ChangePasswordNotSupportedResult = new ChangePasswordNotSupportedResultBag
                    {
                        AccountTypes = accountTypes,
                        NewAccountUrl = "/NewAccount"
                    }
                } );
            }
            else
            {
                return ActionOk( new ForgotUserNameSendInstructionsResultBag
                {
                    ResultType = SendInstructionsResultType.EmailInvalid
                } );
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Verifies that the specified host name is configured within a Rock Site to avoid creating
        /// invalid link URLs.
        /// </summary>
        /// <param name="hostName">The host name</param>
        /// <returns>True if the specified host name is configured in Rock.</returns>
        private bool CheckHostConfiguration( string hostName )
        {
            var siteService = new SiteService( RockContext );
            var siteHostNames = siteService.Queryable().AsNoTracking()
                .SelectMany( s => s.SiteDomains )
                .Select( d => d.Domain )
                .ToList();

            return siteHostNames.Exists( s => s.Equals( hostName, StringComparison.OrdinalIgnoreCase ) );
        }

        #endregion
    }
}
