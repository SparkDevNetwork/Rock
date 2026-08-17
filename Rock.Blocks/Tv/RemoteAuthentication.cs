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

using System.ComponentModel;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.ViewModels.Blocks.Tv.RemoteAuthentication;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Tv
{
    /// <summary>
    /// Authenticates an individual for a remote system (for example, a TV app)
    /// using a short-lived security code.
    /// </summary>
    [DisplayName( "Remote Authentication" )]
    [Category( "TV > TV Apps" )]
    [Description( "Authenticates an individual for a remote system." )]
    [IconCssClass( "ti ti-device-tv" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [SiteField( "Site",
        Description = "The optional site that the remote authentication is tied to.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.Site )]

    [IntegerField( "Code Expiration Duration",
        Description = "The length of time in minutes that a code is good for.",
        IsRequired = true,
        DefaultIntegerValue = 10,
        Order = 1,
        Key = AttributeKey.CodeExpirationDuration )]

    [CodeEditorField( "Header Content",
        Description = "Lava template to create the header.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 300,
        IsRequired = false,
        DefaultValue = DefaultHeaderContent,
        Order = 2,
        Key = AttributeKey.HeaderContent )]

    [CodeEditorField( "Footer Content",
        Description = "Lava template to create the footer.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 300,
        IsRequired = false,
        DefaultValue = "",
        Order = 3,
        Key = AttributeKey.FooterContent )]

    [CodeEditorField( "Success Message",
        Description = "Lava template that will be displayed after a successful authentication.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 300,
        IsRequired = false,
        DefaultValue = DefaultSuccessMessage,
        Order = 4,
        Key = AttributeKey.SuccessMessage )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "5A74867D-6BB7-4461-95C0-A568C6ADAA8B" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "7DEFD622-FD5B-4D55-8774-EC2E77AF09C6" )]
    [Rock.SystemGuid.BlockTypeGuid( "3080C707-4594-4DDD-95B5-DEF82141DE6A" )]
    public class RemoteAuthentication : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string Site = "Site";
            public const string HeaderContent = "HeaderContent";
            public const string FooterContent = "FooterContent";
            public const string CodeExpirationDuration = "CodeExpirationDuration";
            public const string SuccessMessage = "SuccessMessage";
        }

        private static class PageParameterKey
        {
            public const string AuthCode = "AuthCode";
        }

        #endregion Keys

        #region Attribute Strings

        private const string DefaultHeaderContent = @"<div class=""mb-4"">
    <h1>Hello
    {{ CurrentPerson.NickName }}</h1>
    <span>Enter your security code below to authenticate your application.</span>
</div>";

        private const string DefaultSuccessMessage = @"<div>
    <h1>Success!</h1>
    <span>{{ CurrentPerson.NickName }}, you have successfully authenticated to your application.</span>
</div>";

        #endregion Attribute Strings

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var options = new RemoteAuthenticationOptionsBag();

            if ( RequestContext.CurrentPerson == null )
            {
                options.WarningMessage = "This page requires that a person be authenticated to use.";
                return options;
            }

            var mergeFields = RequestContext.GetCommonMergeFields();
            options.HeaderHtml = GetAttributeValue( AttributeKey.HeaderContent ).ResolveMergeFields( mergeFields );
            options.FooterHtml = GetAttributeValue( AttributeKey.FooterContent ).ResolveMergeFields( mergeFields );

            var authCode = PageParameter( PageParameterKey.AuthCode );
            if ( authCode.IsNotNullOrWhiteSpace() )
            {
                var result = AttemptAuthentication( authCode );
                options.IsSuccess = result.IsSuccess;
                options.SuccessHtml = result.SuccessHtml;
                options.ErrorMessage = result.ErrorMessage;
            }

            return options;
        }

        /// <summary>
        /// Attempts to authenticate the current person against a remote authentication
        /// session for the supplied security code.
        /// </summary>
        /// <param name="authCode">The security code entered by the person or supplied via page parameter.</param>
        /// <returns>A bag describing success or failure of the attempt.</returns>
        [BlockAction]
        public BlockActionResult Authenticate( string authCode )
        {
            if ( RequestContext.CurrentPerson == null )
            {
                return ActionUnauthorized( "This page requires that a person be authenticated to use." );
            }

            var result = AttemptAuthentication( authCode );
            return ActionOk( result );
        }

        /// <summary>
        /// Attempts to match and complete a remote authentication session for the
        /// current person using the given security code.
        /// </summary>
        /// <param name="authCode">The security code to validate.</param>
        /// <returns>The authentication result bag.</returns>
        private RemoteAuthenticationAuthenticateResultBag AttemptAuthentication( string authCode )
        {
            var result = new RemoteAuthenticationAuthenticateResultBag();

            var normalizedCode = authCode?.Trim().ToUpperInvariant() ?? string.Empty;
            if ( normalizedCode.IsNullOrWhiteSpace() )
            {
                result.ErrorMessage = "The code provided is not valid. Please confirm that you have correctly entered the code.";
                return result;
            }

            var codeExpirationDuration = GetAttributeValue( AttributeKey.CodeExpirationDuration ).AsInteger();
            var codeExpirationDateTime = RockDateTime.Now.AddMinutes( codeExpirationDuration * -1 );

            // Fallback window so expired codes can be distinguished from codes that never existed.
            var expirationWindowDate = codeExpirationDateTime.AddHours( -2 );

            var siteId = ResolveConfiguredSiteId();

            var remoteAuthenticationService = new RemoteAuthenticationSessionService( RockContext );

            var authSession = remoteAuthenticationService.Queryable()
                .Where( s => s.SiteId == siteId
                    && s.Code != null
                    && s.Code.ToUpper() == normalizedCode
                    && s.AuthorizedPersonAliasId == null
                    && s.SessionStartDateTime > expirationWindowDate )
                .FirstOrDefault();

            if ( authSession == null )
            {
                result.ErrorMessage = "The code provided is not valid. Please confirm that you have correctly entered the code.";
                return result;
            }

            if ( authSession.SessionStartDateTime < codeExpirationDateTime )
            {
                result.ErrorMessage = "The code you provided has expired. Please create a new code and try again.";
                return result;
            }

            authSession.AuthorizedPersonAliasId = RequestContext.CurrentPerson.PrimaryAliasId;
            authSession.SessionAuthenticatedDateTime = RockDateTime.Now;
            authSession.AuthenticationIpAddress = RequestContext.ClientInformation.IpAddress;

            RockContext.SaveChanges();

            var mergeFields = RequestContext.GetCommonMergeFields();
            result.IsSuccess = true;
            result.SuccessHtml = GetAttributeValue( AttributeKey.SuccessMessage ).ResolveMergeFields( mergeFields );

            return result;
        }

        /// <summary>
        /// Resolves the optional site block setting to a site identifier for session matching.
        /// </summary>
        /// <returns>The configured site identifier, or <c>null</c> when no site is configured.</returns>
        private int? ResolveConfiguredSiteId()
        {
            /*
                7/13/26 - MSE

                Return the configured Id directly instead of resolving through SiteCache. A cache lookup
                returned the same Id but fell back to null when the site was missing, which would broaden
                the session query to match site-agnostic sessions on an auth block.

                Reason: Avoid a null-site fallback that widens which sessions an auth code can match.
            */
            return GetAttributeValue( AttributeKey.Site ).AsIntegerOrNull();
        }

        #endregion Methods
    }
}
