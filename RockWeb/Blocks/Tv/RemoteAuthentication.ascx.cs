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
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Tv
{
    /*
    10/15/2021 - JME
    The code from this block was taken primarily from the MobileApplicationDetail.ascx block.
    */
    [DisplayName( "Remote Authentication" )]
    [Category( "TV > TV Apps" )]
    [Description( "Authenticates an individual for a remote system." )]

    #region Block Attributes

    [SiteField( "Site",
                "The optional site that the remote authentication is tied to.",
                false, "", "", 0,
                AttributeKey.Site)]

    [CodeEditorField( "Header Content",
        Description = "Lava template to create the header.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 300,
        IsRequired = false,
        DefaultValue = @"<div class=""mb-4"">
    <h1>Hello
    {{ CurrentPerson.NickName }}</h1>
    <span>Enter your security code below to authenticate your application.</span>
</div>",
        Category = "default",
        Order = 1,
        Key = AttributeKey.HeaderContent )]

    [CodeEditorField( "Footer Content",
        Description = "Lava template to create the footer.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 300,
        IsRequired = false,
        DefaultValue = "",
        Category = "default",
        Order = 2,
        Key = AttributeKey.FooterContent )]

    [CodeEditorField( "Success Message",
        Description = "Lava template that will be displayed after a successful authentication.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 300,
        IsRequired = false,
        DefaultValue = @"<div>
    <h1>Success!</h1>
    <span>{{ CurrentPerson.NickName }}, you have successfully authenticated to your application.</span>
</div>",
        Category = "default",
        Order = 3,
        Key = AttributeKey.SuccessMessage )]

    [IntegerField(  "Code Expiration Duration",
        "The length of time in minutes that a code is good for.",
        true,
        10,
        "",
        4,
        AttributeKey.CodeExpirationDuration)]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "3080C707-4594-4DDD-95B5-DEF82141DE6A" )]
    public partial class RemoteAuthentication : Rock.Web.UI.RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string Site = "Site";
            public const string HeaderContent = "HeaderContent";
            public const string FooterContent = "FooterContent";
            public const string CodeExpirationDuration = "CodeExpirationDuration";
            public const string SuccessMessage = "SuccessMessage";
        }

        #endregion Attribute Keys

        #region Base Control Methods

        // Overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // This event gets fired after block settings are updated. It's nice to repaint the screen if these settings would alter it.
            this.BlockUpdated += Block_BlockUpdated;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                // Check to ensure that the person is logged in.
                if ( CurrentPerson == null )
                {
                    nbWarningMessages.Text = "This page requires that a person be authenticated to use.";
                    nbWarningMessages.Visible = true;

                    pnlAuthenticate.Visible = false;
                    base.OnLoad( e );
                    return;
                }

                SetupPage();

                // If passed an authentication code, prefill it and ask the person to confirm.
                // Nothing is written until they confirm with a postback.
                int? pendingSessionId = null;
                var passScanCode = PageParameter( "AuthCode" )?.Trim().ToUpperInvariant();

                if ( passScanCode.IsNotNullOrWhiteSpace() )
                {
                    txtSecurityCode.Text = passScanCode;

                    var authSession = FindPendingSession( new RockContext(), passScanCode, out var errorMessage );

                    if ( authSession == null )
                    {
                        ShowAuthenticationMessage( errorMessage, NotificationBoxType.Warning );
                    }
                    else
                    {
                        pendingSessionId = authSession.Id;
                        txtSecurityCode.ReadOnly = true;
                        btnSubmit.Text = "Confirm";

                        var appName = SiteCache.Get( authSession.SiteId ?? 0 )?.Name ?? "your TV application";
                        ShowAuthenticationMessage( $"Sign in to {appName.EncodeHtml()}? This code was requested at {authSession.SessionStartDateTime?.ToShortTimeString()}. Only continue if this code is showing on a TV in front of you.", NotificationBoxType.Info );
                    }
                }

                hfConfirmationToken.Value = CreateConfirmationToken( pendingSessionId );
            }
        }

        #endregion

        #region Events

        // Handlers called by the controls on your block.

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Block_BlockUpdated( object sender, EventArgs e )
        {
            SetupPage();
        }

        /// <summary>
        /// Handles the Click event of the btnSubmit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSubmit_Click( object sender, EventArgs e )
        {
            // A postback event can be raised from a GET request that carries the
            // postback fields in the query string, so require an actual POST.
            if ( Request.HttpMethod != "POST" )
            {
                ShowAuthenticationMessage( "This action must be submitted from the page.", NotificationBoxType.Warning );
                return;
            }

            var token = TryDecryptConfirmationToken( hfConfirmationToken.Value );

            if ( token == null )
            {
                ShowAuthenticationMessage( "This page has expired. Please reload the page and try again.", NotificationBoxType.Warning );
                return;
            }

            AttemptAuthentication( txtSecurityCode.Text, token.RemoteAuthenticationSessionId );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Setups the page.
        /// </summary>
        private void SetupPage()
        {
            pnlAuthenticate.Visible = true;
            pnlSuccess.Visible = false;

            nbWarningMessages.Visible = false;
            nbAuthenticationMessages.Visible = false;

            // Configure header and footer
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, null, new Rock.Lava.CommonMergeFieldsOptions() );

            lHeaderInfo.Text = GetAttributeValue(AttributeKey.HeaderContent).ResolveMergeFields( mergeFields );
            lFooterInfo.Text = GetAttributeValue( AttributeKey.FooterContent ).ResolveMergeFields( mergeFields );

        }

        /// <summary>
        /// Shows a message in the authentication notification box.
        /// </summary>
        /// <param name="text">The message text.</param>
        /// <param name="type">The type of notification box to display.</param>
        private void ShowAuthenticationMessage( string text, NotificationBoxType type )
        {
            nbAuthenticationMessages.Text = text;
            nbAuthenticationMessages.NotificationBoxType = type;
            nbAuthenticationMessages.Visible = true;
        }

        /// <summary>
        /// Finds the pending remote authentication session for the code. This does not
        /// make any changes to the session.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="authCode">The authentication code.</param>
        /// <param name="errorMessage">The message to display if no valid session was found.</param>
        /// <returns>The pending session or <c>null</c> if the code is not valid or has expired.</returns>
        private RemoteAuthenticationSession FindPendingSession( RockContext rockContext, string authCode, out string errorMessage )
        {
            authCode = authCode?.Trim().ToUpperInvariant();

            if ( authCode.IsNullOrWhiteSpace() )
            {
                errorMessage = "The code provided is not valid. Please confirm that you have correctly entered the code.";
                return null;
            }

            var codeExpirationDuration = GetAttributeValue( AttributeKey.CodeExpirationDuration ).AsInteger();
            var codeExpirationDateTime = RockDateTime.Now.AddMinutes( codeExpirationDuration * -1 );

            // Get site
            var siteId = GetAttributeValue( AttributeKey.Site ).AsIntegerOrNull();

            if ( siteId.HasValue )
            {
                var site = SiteCache.Get( siteId.Value );
                siteId = site.Id;
            }

            // Get matching remote authentication record
            var remoteAuthenticationService = new RemoteAuthenticationSessionService( rockContext );

            // Create a fallback date to eliminate very old sessions. We want to be able to tell someone
            // that the code they have has expired so we can't use the expiration date, we need a date older
            // than that.
            var expirationWindowDate = codeExpirationDateTime.AddHours( -2 );

            var authSession = remoteAuthenticationService.Queryable()
                                .Where( s => s.SiteId == siteId
                                            && s.Code == authCode
                                            && s.AuthorizedPersonAliasId == null
                                            && s.SessionStartDateTime > expirationWindowDate )
                                .FirstOrDefault();

            // Check for code that does not exist
            if ( authSession == null )
            {
                errorMessage = "The code provided is not valid. Please confirm that you have correctly entered the code.";
                return null;
            }

            // Check for expired session
            if ( authSession.SessionStartDateTime < codeExpirationDateTime )
            {
                errorMessage = "The code you provided has expired. Please create a new new code and try again.";
                return null;
            }

            errorMessage = null;
            return authSession;
        }

        /// <summary>
        /// Attempts the authentication.
        /// </summary>
        /// <param name="authCode">The authentication code.</param>
        /// <param name="expectedSessionId">The identifier of the session that was shown to the person when the page loaded, if any.</param>
        private void AttemptAuthentication( string authCode, int? expectedSessionId )
        {
            var rockContext = new RockContext();
            var authSession = FindPendingSession( rockContext, authCode, out var errorMessage );

            if ( authSession == null )
            {
                ShowAuthenticationMessage( errorMessage, NotificationBoxType.Warning );
                return;
            }

            if ( expectedSessionId.HasValue && authSession.Id != expectedSessionId.Value )
            {
                ShowAuthenticationMessage( "This code does not match the sign-in request shown on this page. Please reload the page and try again.", NotificationBoxType.Warning );
                return;
            }

            // Process authentication
            authSession.AuthorizedPersonAliasId = CurrentPersonAliasId;
            authSession.SessionAuthenticatedDateTime = RockDateTime.Now;
            authSession.AuthenticationIpAddress = RockPage.GetClientIpAddress();

            rockContext.SaveChanges();

            // Show success message
            pnlAuthenticate.Visible = false;
            pnlSuccess.Visible = true;

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, null, new Rock.Lava.CommonMergeFieldsOptions() );
            lSuccessContent.Text = GetAttributeValue( AttributeKey.SuccessMessage ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Creates the encrypted confirmation token that must be posted back
        /// with the request to authenticate a session.
        /// </summary>
        /// <param name="pendingSessionId">The identifier of the session shown to the person, if any.</param>
        /// <returns>The encrypted token.</returns>
        private string CreateConfirmationToken( int? pendingSessionId )
        {
            var payload = new ConfirmationTokenPayload
            {
                BlockId = BlockId,
                PersonAliasId = CurrentPersonAliasId.Value,
                RemoteAuthenticationSessionId = pendingSessionId,
                ExpiresAt = RockDateTime.Now.Add( ConfirmationTokenLifetime )
            };

            return Rock.Security.Encryption.EncryptString( payload.ToJson() );
        }

        /// <summary>
        /// Decrypts and validates the confirmation token. The token must have been
        /// issued by this block to the current person and must not have expired.
        /// </summary>
        /// <param name="token">The encrypted token.</param>
        /// <returns>The token payload or <c>null</c> if the token is not valid.</returns>
        private ConfirmationTokenPayload TryDecryptConfirmationToken( string token )
        {
            if ( token.IsNullOrWhiteSpace() )
            {
                return null;
            }

            ConfirmationTokenPayload payload;

            try
            {
                payload = Rock.Security.Encryption.DecryptString( token ).FromJsonOrNull<ConfirmationTokenPayload>();
            }
            catch
            {
                return null;
            }

            if ( payload == null
                || payload.BlockId != BlockId
                || payload.PersonAliasId != CurrentPersonAliasId
                || payload.ExpiresAt < RockDateTime.Now )
            {
                return null;
            }

            return payload;
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// How long a confirmation token is valid after the page is loaded.
        /// </summary>
        private static readonly TimeSpan ConfirmationTokenLifetime = TimeSpan.FromMinutes( 30 );

        /// <summary>
        /// The data stored in the encrypted confirmation token.
        /// </summary>
        private class ConfirmationTokenPayload
        {
            public int BlockId { get; set; }

            public int PersonAliasId { get; set; }

            public int? RemoteAuthenticationSessionId { get; set; }

            public DateTime ExpiresAt { get; set; }
        }

        #endregion
    }
}