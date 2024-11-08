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
        "Lava template to create the header.",
        CodeEditorMode.Lava,
        CodeEditorTheme.Rock,
        300,
        false,
        @"<div class=""mb-4"">
    <h1>Hello
    {{ CurrentPerson.NickName }}</h1>
    <span>Enter your security code below to authenticate your application.</span>
</div>",
        "default",
        1,
        AttributeKey.HeaderContent)]

    [CodeEditorField( "Footer Content",
        "Lava template to create the footer.",
        CodeEditorMode.Lava,
        CodeEditorTheme.Rock,
        300,
        false,
        "",
        "default",
        2,
        AttributeKey.FooterContent )]

    [CodeEditorField( "Success Message",
        "Lava template that will be displayed after a successful authentication.",
        CodeEditorMode.Lava,
        CodeEditorTheme.Rock,
        300,
        false,
        @"<div>
    <h1>Success!</h1>
    <span>{{ CurrentPerson.NickName }}, you have successfully authenticated to your application.</span>
</div>",
        "default",
        3,
        AttributeKey.SuccessMessage )]

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

                // If passed an authentication token we will attempt to complete the authentication
                var passScanCode = PageParameter( "AuthCode" );

                if ( passScanCode.IsNotNullOrWhiteSpace() )
                {
                    AttemptAuthentication( passScanCode );
                    base.OnLoad( e );
                    return;
                }

                SetupPage();
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
            AttemptAuthentication( txtSecurityCode.Text );
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
        /// Attempts the authentication.
        /// </summary>
        private void AttemptAuthentication( string authCode )
        {
            pnlAuthenticate.Visible = false;
            pnlSuccess.Visible = true;

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
            var rockContext = new RockContext();
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
                nbAuthenticationMessages.Text = "The code provided is not valid. Please confirm that you have correctly entered the code.";
                nbAuthenticationMessages.NotificationBoxType = NotificationBoxType.Warning;
                nbAuthenticationMessages.Visible = true;
                return;
            }

            // Check for expired session
            if ( authSession.SessionStartDateTime < codeExpirationDateTime )
            {
                nbAuthenticationMessages.Text = "The code you provided has expired. Please create a new new code and try again.";
                nbAuthenticationMessages.NotificationBoxType = NotificationBoxType.Warning;
                nbAuthenticationMessages.Visible = true;
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

        #endregion
    }
}