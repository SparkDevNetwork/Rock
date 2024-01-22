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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace RockWeb.Blocks.Security
{
    /// <summary>
    /// Block for user to request a forgotten username.
    /// </summary>
    [DisplayName( "Forgot Username" )]
    [Category( "Security" )]
    [Description( "Allows a user to get their forgotten username information emailed to them." )]

    [CodeEditorField( "Heading Caption",
        Key = AttributeKey.HeadingCaption,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Html,
        EditorTheme = Rock.Web.UI.Controls.CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = "<h5 class='text-center'>Can’t log in?</h5>",
        Category = "Captions",
        Order = 0 )]

    [CodeEditorField( "Invalid Email Caption",
        Key = AttributeKey.InvalidEmailCaption,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Html,
        EditorTheme = Rock.Web.UI.Controls.CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = "Sorry, we didn’t recognize that email address. Want to try another?",
        Category = "Captions",
        Order = 1 )]

    [CodeEditorField( "Success Caption",
        Key = AttributeKey.SuccessCaption,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Html,
        EditorTheme = Rock.Web.UI.Controls.CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = "We’ve emailed you instructions for logging in",
        Category = "Captions",
        Order = 2 )]

    [LinkedPage( "Confirmation Page",
        Description = "Page for user to confirm their account (if blank will use 'ConfirmAccount' page route)",
        Key = AttributeKey.ConfirmationPage,
        IsRequired = false,
        Order = 3 )]

    [SystemCommunicationField( "Forgot Username Email Template",
        Key = AttributeKey.EmailTemplate,
        Description = "The email template to use when sending the forgot username (and password) email.  The following merge fields are available for use in the template: Person, Users, and SupportsChangePassword (an array of the usernames that support password changes.)",
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

    [Rock.SystemGuid.BlockTypeGuid( "02B3D7D1-23CE-4154-B602-F4A15B321757" )]
    public partial class ForgotUserName : Rock.Web.UI.RockBlock
    {
        public static class AttributeKey
        {
            public const string HeadingCaption = "HeadingCaption";
            public const string InvalidEmailCaption = "InvalidEmailCaption";
            public const string SuccessCaption = "SuccessCaption";
            public const string ConfirmationPage = "ConfirmationPage";
            public const string EmailTemplate = "EmailTemplate";
            public const string CreateCommunicationRecord = "CreateCommunicationRecord"; 
            public const string DisableCaptchaSupport = "DisableCaptchaSupport";
        }
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            pnlEntry.Visible = true;
            pnlWarning.Visible = false;
            pnlSuccess.Visible = false;

            cpCaptcha.Visible = !( GetAttributeValue( AttributeKey.DisableCaptchaSupport ).AsBoolean() || !cpCaptcha.IsAvailable );

            if ( !Page.IsPostBack )
            {
                lCaption.Text = GetAttributeValue( AttributeKey.HeadingCaption );
                lWarning.Text = GetAttributeValue( AttributeKey.InvalidEmailCaption );
                lSuccess.Text = GetAttributeValue( AttributeKey.SuccessCaption );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnSend control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSend_Click( object sender, EventArgs e )
        {
            var disableCaptchaSupport = GetAttributeValue( AttributeKey.DisableCaptchaSupport ).AsBoolean() || !cpCaptcha.IsAvailable;
            if ( !disableCaptchaSupport && !cpCaptcha.IsResponseValid() )
            {
                lWarning.Text = "There was an issue processing your request. Please try again. If the issue persists please contact us.";
                pnlWarning.Visible = true;
                return;
            }

            var url = LinkedPageUrl( AttributeKey.ConfirmationPage );
            if ( string.IsNullOrWhiteSpace( url ) )
            {
                url = ResolveRockUrl( "~/ConfirmAccount" );
            }

            var rootUri = new Uri( RootPath );
            var hostName = rootUri.Host;
            if ( !CheckHostConfiguration( hostName ) )
            {
                throw new Exception( "Invalid request." );
            }

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "ConfirmAccountUrl", RootPath + url.TrimStart( new char[] { '/' } ) );
            var results = new List<IDictionary<string, object>>();

            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var userLoginService = new UserLoginService( rockContext );

            bool hasAccountWithPasswordResetAbility = false;
            List<string> accountTypes = new List<string>();

            foreach ( Person person in personService.GetByEmail( tbEmail.Text )
                .Where( p => p.Users.Any() ) )
            {
                var users = new List<UserLogin>();
                List<string> supportsChangePassword = new List<string>();
                foreach ( UserLogin user in userLoginService.GetByPersonId( person.Id ) )
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

                            if ( component.TypeGuid == Rock.SystemGuid.EntityType.AUTHENTICATION_PASSWORDLESS.AsGuid() )
                            {
                                // Clone the user so we can safely override the passwordless username without risk of DB changes.
                                var clone = user.Clone( true );
                                clone.UserName = "(email or mobile number)";
                                users.Add( clone );
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

                var resultsDictionary = new Dictionary<string, object>();
                resultsDictionary.Add( "Person", person );
                resultsDictionary.Add( "Users", users );
                resultsDictionary.Add( "SupportsChangePassword", supportsChangePassword );
                results.Add( resultsDictionary );
            }

            if ( results.Count > 0 && hasAccountWithPasswordResetAbility )
            {
                mergeFields.Add( "Results", results.ToArray() );

                var emailMessage = new RockEmailMessage( GetAttributeValue( AttributeKey.EmailTemplate ).AsGuid() );
                emailMessage.AddRecipient( RockEmailMessageRecipient.CreateAnonymous( tbEmail.Text, mergeFields ) );
                emailMessage.AppRoot = ResolveRockUrlIncludeRoot( "~/" );
                emailMessage.ThemeRoot = ResolveRockUrlIncludeRoot( "~~/" );
                emailMessage.CreateCommunicationRecord = GetAttributeValue( AttributeKey.CreateCommunicationRecord ).AsBoolean();
                emailMessage.Send();

                pnlEntry.Visible = false;
                pnlSuccess.Visible = true;
            }
            else if ( results.Count > 0 )
            {
                // The person has user accounts but none of them are allowed to have their passwords reset (Facebook/Google/etc).
                lWarning.Text = string.Format(
                                                @"<p>We were able to find the following accounts for this email, but
                                                none of them are able to be reset from this website.</p> <p>Accounts:<br /> {0}</p>
                                                <p>To create a new account with a username and password please see our <a href='{1}'>New Account</a>
                                                page.</p>",
                                    string.Join( ",", accountTypes ),
                                    ResolveRockUrl( "~/NewAccount" ) );
                pnlWarning.Visible = true;
            }
            else
            {
                pnlWarning.Visible = true;
            }
        }

        /// <summary>
        /// Verifies that the specified host name is configured within a Rock Site to avoid creating
        /// invalid link URLs.
        /// </summary>
        /// <param name="hostName">The host name</param>
        /// <returns>True if the specified host name is configured in Rock.</returns>
        private bool CheckHostConfiguration( string hostName )
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

        #endregion
    }
}