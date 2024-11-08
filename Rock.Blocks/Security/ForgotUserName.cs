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

namespace Rock.Blocks.Security
{
    /// <summary>
    /// Displays the details of a particular user login.
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
        EditorTheme = Rock.Web.UI.Controls.CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = "<h5 class='text-center'>Can't log in?</h5>",
        Category = "Captions",
        Order = 0 )]

    [CodeEditorField( "Invalid Email Caption",
        Key = AttributeKey.InvalidEmailCaption,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Html,
        EditorTheme = Rock.Web.UI.Controls.CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = "Sorry, we didn't recognize that email address. Want to try another?",
        Category = "Captions",
        Order = 1 )]

    [CodeEditorField( "Success Caption",
        Key = AttributeKey.SuccessCaption,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Html,
        EditorTheme = Rock.Web.UI.Controls.CodeEditorTheme.Rock,
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

    [Rock.SystemGuid.EntityTypeGuid( "5BBEE600-781E-4480-8144-36F8D01C7F09" )]
    [Rock.SystemGuid.BlockTypeGuid( "16CD7562-BE31-4823-9C4D-F365AB0AA5C4" )]
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

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion

        #region Properties

        private string ConfirmationPageUrl => this.GetLinkedPageUrl( AttributeKey.ConfirmationPage );

        private bool CreateCommunicationRecord => GetAttributeValue( AttributeKey.CreateCommunicationRecord ).AsBoolean();

        private Guid EmailTemplateGuid => GetAttributeValue( AttributeKey.EmailTemplate ).AsGuid();

        private string HeadingCaption => GetAttributeValue( AttributeKey.HeadingCaption );

        private string InvalidEmailCaption => GetAttributeValue( AttributeKey.InvalidEmailCaption );

        private string SuccessCaption => GetAttributeValue( AttributeKey.SuccessCaption );

        private bool DisableCaptchaSupport => GetAttributeValue( AttributeKey.DisableCaptchaSupport ).AsBoolean();

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
                NavigationUrls = GetBoxNavigationUrls(),
                SecurityGrantToken = null,
                DisableCaptchaSupport = this.DisableCaptchaSupport
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
            var disableCaptcha = GetAttributeValue( AttributeKey.DisableCaptchaSupport ).AsBoolean();

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
                throw new Exception( "Invalid request." );
            }

            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var userLoginService = new UserLoginService( rockContext );
            var accountTypes = new List<string>();
            var usernamesSupportingPasswordChange = new List<string>();
            var results = new List<IDictionary<string, object>>();

            foreach ( var person in personService.GetByEmail( bag.Email )
                .AsNoTracking()
                .Include( p => p.Users )
                .Include( p => p.Users.Select( u => u.EntityType ) )
                .Where( p => p.Users.Any() ) )
            {
                var users = new List<UserLogin>();
                foreach ( var user in person.Users )
                {
                    if ( user.EntityType != null )
                    {
                        var component = AuthenticationContainer.GetComponent( user.EntityType.Name );

                        if ( component != null && !component.RequiresRemoteAuthentication )
                        {
                            if ( component.SupportsChangePassword )
                            {
                                usernamesSupportingPasswordChange.Add( user.UserName );
                            }

                            users.Add( user );
                        }

                        accountTypes.Add( user.EntityType.FriendlyName );
                    }
                }

                results.Add( new Dictionary<string, object>
                {
                    { "Person", person },
                    { "Users", users },
                    { "SupportsChangePassword", usernamesSupportingPasswordChange }
                } );
            }

            if ( results.Count > 0 && usernamesSupportingPasswordChange.Any() )
            {
                var mergeFields = this.RequestContext.GetCommonMergeFields( this.GetCurrentPerson() );
                mergeFields.Add( "ConfirmAccountUrl", RequestContext.RootUrlPath + url);
                mergeFields.Add( "Results", results.ToArray() );

                var emailMessage = new RockEmailMessage( this.EmailTemplateGuid );
                emailMessage.AddRecipient( RockEmailMessageRecipient.CreateAnonymous( bag.Email, mergeFields ) );
                emailMessage.AppRoot = "/";
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
        private static bool CheckHostConfiguration( string hostName )
        {
            using ( var rockContext = new RockContext() )
            {
                var siteService = new SiteService( rockContext );
                var siteHostNames = siteService.Queryable().AsNoTracking()
                    .SelectMany( s => s.SiteDomains )
                    .Select( d => d.Domain )
                    .ToList();

                return siteHostNames.Contains( hostName );
            }
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
            };
        }

        #endregion
    }
}
