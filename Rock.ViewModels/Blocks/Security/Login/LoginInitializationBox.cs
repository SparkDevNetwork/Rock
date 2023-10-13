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
using System.Collections.Generic;
using Rock.Enums.Blocks.Security.Login;

namespace Rock.ViewModels.Blocks.Security.Login
{
    /// <summary>
    /// A box that contains the required information to render a login block.
    /// Implements the <see cref="Rock.ViewModels.Blocks.BlockBox" />
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Blocks.BlockBox" />
    public class LoginInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the username field label.
        /// </summary>
        /// <value>The username field label.</value>
        public string UsernameFieldLabel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to hide the new account option.
        /// </summary>
        /// <value><c>true</c> to hide new account option; otherwise, <c>false</c>.</value>
        public bool HideNewAccountOption { get; set; }

        /// <summary>
        /// Gets or sets the help page URL.
        /// </summary>
        /// <value>The help page URL.</value>
        public string HelpPageUrl { get; set; }

        /// <summary>
        /// Gets or sets the new account page URL.
        /// </summary>
        /// <value>The new account page URL.</value>
        public string NewAccountPageUrl { get; set; }

        /// <summary>
        /// Gets or sets the new account button text.
        /// </summary>
        /// <value>The new account button text.</value>
        public string NewAccountButtonText { get; set; }

        /// <summary>
        /// Gets or sets the content text.
        /// </summary>
        /// <value>The content text.</value>
        public string ContentText { get; set; }

        /// <summary>
        /// Gets or sets the prompt message.
        /// </summary>
        /// <value>The prompt message.</value>
        public string PromptMessage { get; set; }

        /// <summary>
        /// Gets or sets the external authentication buttons.
        /// </summary>
        /// <value>
        /// The external authentication buttons.
        /// </value>
        public List<ExternalAuthenticationButtonBag> ExternalAuthProviderButtons { get; set; }

        /// <summary>
        /// Page to redirect user to upon successful log in.
        /// The 'returnurl' query string will always override this setting for database authenticated logins.
        /// Redirect Page Setting will override third-party authentication 'returnurl'.
        /// </summary>
        public string RedirectUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the client should redirect to the <see cref="RedirectUrl"/>.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the client should redirect; otherwise, <c>false</c>.
        /// </value>
        public bool ShouldRedirect { get; set; }

        /// <summary>
        /// Indicates whether internal database login is supported.
        /// </summary>
        public bool IsInternalDatabaseLoginSupported { get; set; }

        /// <summary>
        /// The passwordless login auto verify options bag.
        /// </summary>
        /// <remarks>Only set when passwordless authentication automatic verification is required.</remarks>
        public PasswordlessLoginAutoVerifyOptionsBag PasswordlessAutoVerifyOptions { get; set; }

        /// <summary>
        /// The default login method when the block is loaded.
        /// </summary>
        public LoginMethod DefaultLoginMethod { get; set; }

        /// <summary>
        /// Indicates whether passwordless login is supported.
        /// </summary>
        public bool IsPasswordlessLoginSupported { get; set; }

        /// <summary>
        /// Optional text (HTML) to display above remote authorization options.
        /// </summary>
        public string RemoteAuthorizationPromptMessage { get; set; }

        /// <summary>
        /// The configuration errors.
        /// </summary>
        /// <remarks>Only set configuration has errors.</remarks>
        public List<string> ConfigurationErrors { get; set; }

        /// <summary>
        /// Gets or sets the two-factor email phone required message.
        /// </summary>
        /// <value>
        /// The two-factor email phone required message.
        /// </value>
        public string TwoFactorEmailPhoneRequiredMessage { get; set; }

        /// <summary>
        /// Gets or sets the two-factor email phone not available message.
        /// </summary>
        /// <value>
        /// The two-factor email phone not available message.
        /// </value>
        public string TwoFactorEmailPhoneNotAvailableMessage { get; set; }

        /// <summary>
        /// Gets or sets the two-factor login required message.
        /// </summary>
        /// <value>
        /// The two-factor login required message.
        /// </value>
        public string TwoFactorLoginRequiredMessage { get; set; }

        /// <summary>
        /// Gets or sets the two-factor login not available message.
        /// </summary>
        /// <value>
        /// The two-factor login not available message.
        /// </value>
        public string TwoFactorLoginNotAvailableMessage { get; set; }

        /// <summary>
        /// Gets or sets the two-factor not supported by authentication method message.
        /// </summary>
        /// <value>
        /// The two-factor not supported by authentication method message.
        /// </value>
        public string TwoFactorNotSupportedByAuthenticationMethodMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether two-factor authentication is not supported for the selected authentication factor.
        /// </summary>
        /// <value>
        ///   <c>true</c> if two-factor authentication is not supported for the selected authentication factor; otherwise, <c>false</c> or <c>null</c> if no 2FA occurred.
        /// </value>
        public bool? Is2FANotSupportedForAuthenticationFactor { get; set; }
    }
}
