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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using RestSharp;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Provides a Captcha control that verifies the user is a real person.
    /// </summary>
    public class Captcha : WebControl, IRockControl, IPostBackEventHandler
    {
        #region Fields

        private HiddenField _hfToken;

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets or sets the custom validator.
        /// </summary>
        /// <value>
        /// The custom validator.
        /// </value>
        protected CustomValidator CustomValidator { get; set; }

        /// <summary>
        /// Cached value to contain if the user response is valid.
        /// </summary>
        [Obsolete( "Use ValidatedResult instead." )]
        [RockObsolete( "1.12.5" )]
        protected bool? _isResponseValid { get; set; }

        /// <summary>
        /// Gets or sets the cached response result.
        /// </summary>
        /// <value>
        /// The cached response result.
        /// </value>
        protected bool? ValidatedResult
        {
#pragma warning disable CS0618 // Type or member is obsolete
            // When _isResponseValid is removed, this can be changed to a simple get;set;
            get => _isResponseValid;
            set => _isResponseValid = value;
#pragma warning restore CS0618 // Type or member is obsolete
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The public site key to use when obtaining user responses.
        /// </summary>
        public string SiteKey
        {
            get
            {
                return ( string ) ViewState["SiteKey"];
            }
            set
            {
                ViewState["SiteKey"] = value;
            }
        }

        /// <summary>
        /// The secret key to use when verifying user responses.
        /// </summary>
        public string SecretKey
        {
            get
            {
                return ( string ) ViewState["SecretKey"];
            }
            set
            {
                ViewState["SecretKey"] = value;
            }
        }

        /// <summary>
        /// Returns true if the captcha control is available for use, meaning its settings have been
        /// configured by the administrator.
        /// </summary>
        public bool IsAvailable
        {
            get
            {
                return !string.IsNullOrWhiteSpace( SiteKey ) && !string.IsNullOrWhiteSpace( SecretKey );
            }
        }

        /// <summary>
        /// Occurs when a file is uploaded.
        /// </summary>
        public event EventHandler<TokenReceivedEventArgs> TokenReceived;

        #endregion

        #region IRockControl implementation

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the label." )
        ]
        public string Label
        {
            get { return ViewState["Label"] as string ?? string.Empty; }
            set { ViewState["Label"] = value; }
        }

        /// <summary>
        /// Gets or sets the form group class.
        /// </summary>
        /// <value>
        /// The form group class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        Description( "The CSS class to add to the form-group div." )
        ]
        public string FormGroupCssClass
        {
            get { return ViewState["FormGroupCssClass"] as string ?? string.Empty; }
            set { ViewState["FormGroupCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The help block." )
        ]
        public string Help
        {
            get
            {
                return HelpBlock != null ? HelpBlock.Text : string.Empty;
            }

            set
            {
                if ( HelpBlock != null )
                {
                    HelpBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the warning text.
        /// </summary>
        /// <value>
        /// The warning text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The warning block." )
        ]
        public string Warning
        {
            get
            {
                return WarningBlock != null ? WarningBlock.Text : string.Empty;
            }

            set
            {
                if ( WarningBlock != null )
                {
                    WarningBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RockTextBox"/> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "false" ),
        Description( "Is the value required?" )
        ]
        public bool Required
        {
            get { return ViewState["Required"] as bool? ?? false; }
            set { ViewState["Required"] = value; }
        }

        /// <summary>
        /// Gets or sets the required error message.  If blank, the LabelName name will be used
        /// </summary>
        /// <value>
        /// The required error message.
        /// </value>
        public string RequiredErrorMessage
        {
            get
            {
                var errorMessage = ViewState["RequiredErrorMessage"] as string ?? string.Empty;
                if ( errorMessage.IsNotNullOrWhiteSpace() )
                {
                    return errorMessage;
                }

                if ( Label.IsNotNullOrWhiteSpace() )
                {
                    return $"{Label} is required.";
                }

                return "Please complete the captcha.";
            }

            set
            {
                ViewState["RequiredErrorMessage"] = value;

                if ( CustomValidator != null )
                {
                    CustomValidator.ErrorMessage = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets an optional validation group to use.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get
            {
                return ViewState["ValidationGroup"] as string ?? string.Empty;
            }
            set
            {
                ViewState["ValidationGroup"] = value;

                if ( CustomValidator != null )
                {
                    CustomValidator.ValidationGroup = value;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsValid
        {
            get
            {
                return !Required || CustomValidator.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        public HelpBlock HelpBlock { get; set; }

        /// <summary>
        /// Gets or sets the warning block.
        /// </summary>
        /// <value>
        /// The warning block.
        /// </value>
        public WarningBlock WarningBlock { get; set; }

        /// <summary>
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the Captcha control.
        /// </summary>
        public Captcha() : base()
        {
            CustomValidator = new CustomValidator();
            SiteKey = SystemSettings.GetValue( SystemKey.SystemSetting.CAPTCHA_SITE_KEY );
            SecretKey = SystemSettings.GetValue( SystemKey.SystemSetting.CAPTCHA_SECRET_KEY );
            _hfToken = new HiddenField();
        }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Create all our child controls and add them to the DOM.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            CustomValidator.ID = $"{ID}_cfv";
            CustomValidator.CssClass = "validation-error help-inline js-captcha-validator";
            CustomValidator.ClientValidationFunction = "Rock.controls.captcha.clientValidate";
            CustomValidator.ErrorMessage = RequiredErrorMessage;
            CustomValidator.Enabled = true;
            CustomValidator.Display = ValidatorDisplay.Dynamic;
            CustomValidator.ValidationGroup = ValidationGroup;

            CustomValidator.Attributes.Add( "data-required", Required.ToString().ToLower() );
            CustomValidator.Attributes.Add( "data-captcha-id", ClientID );

            Controls.Add( CustomValidator );

            _hfToken.ID = $"{ID}_hfToken";
            Controls.Add( _hfToken );
        }

        /// <summary>
        /// Called just before rendering begins on the page.
        /// </summary>
        /// <param name="e">The EventArgs that describe this event.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            var postBackScript = this.TokenReceived != null
                ? this.Page.ClientScript.GetPostBackEventReference( new PostBackOptions( this, "TokenReceived" ), false ).Replace( '\'', '"' )
                : "";

            if ( SiteKey.IsNotNullOrWhiteSpace() )
            {
                // Add the cloudflare script tag to head.
                var additionalAttributes = new Dictionary<string, string> { { "defer", null } };
                RockPage.AddScriptSrcToHead(
                    this.Page,
                    "rockCloudflareTurnstile",
                    "https://challenges.cloudflare.com/turnstile/v0/api.js?render=explicit",
                    additionalAttributes
                );

                string script = $@"
;(function () {{
    Rock.controls.captcha.initialize({{
        id: '{ClientID}',
        key: '{SiteKey}',
        postBackScript: '{postBackScript}'
    }});
}})();
";

                ScriptManager.RegisterStartupScript( this, GetType(), $"captcha-{ClientID}", script, true );
            }
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                RockControlHelper.RenderControl( this, writer );
            }
        }

        /// <summary>
        /// When implemented by a class, enables a server control to process an event raised when a form is posted to the server.
        /// </summary>
        /// <param name="eventArgument">A <see cref="T:System.String" /> that represents an optional event argument to be passed to the event handler.</param>
        public void RaisePostBackEvent( string eventArgument )
        {
            if ( eventArgument == "TokenReceived" && TokenReceived != null )
            {
                var token = HttpContext.Current.Request.Form[$"{UniqueID}_hfToken"];
                TokenReceived( this, new TokenReceivedEventArgs { Token = token, IsValid = IsResponseValid() } );
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Check if the Captcha response is valid. This bypasses the Required check and
        /// simply checks if the user response is valid. An example of such use would be
        /// to have the prayer entry block have a non-required Captcha. If the captcha
        /// is valid then it auto-approves, otherwise it requires admin approval.
        /// </summary>
        /// <returns>True if the user response to the captcha is valid.</returns>
        public bool IsResponseValid()
        {
            var userResponse = HttpContext.Current.Request.Form[$"{UniqueID}_hfToken"];
            string remoteIp = HttpContext.Current.Request.UserHostAddress;

            if ( string.IsNullOrWhiteSpace( SiteKey ) || string.IsNullOrWhiteSpace( SecretKey ) )
            {
                return true;
            }

            if ( ValidatedResult.HasValue )
            {
                return ValidatedResult.Value;
            }

            if ( string.IsNullOrWhiteSpace( userResponse ) )
            {
                return false;
            }

            if ( !string.IsNullOrWhiteSpace( HttpContext.Current.Request.Headers["HTTP_X_FORWARDED_FOR"] ) )
            {
                remoteIp = HttpContext.Current.Request.Headers["HTTP_X_FORWARDED_FOR"];
            }

            try
            {
                var client = new RestClient( "https://challenges.cloudflare.com/turnstile/v0/siteverify" );
                var request = new RestRequest( Method.POST );

                request.AddParameter( "secret", SecretKey );
                request.AddParameter( "response", userResponse );
                request.AddParameter( "remoteip", remoteIp );
                request.Timeout = 5000;

                var response = client.Execute<CloudFlareCaptchaResponse>( request );

                ValidatedResult = response.Data.Success;
            }
            catch ( Exception e )
            {
                Rock.Model.ExceptionLogService.LogException( e );
                ValidatedResult = false;
            }

            return ValidatedResult.Value;
        }

        /// <summary>
        /// This is where you implement the simple aspects of rendering your control.  The rest
        /// will be handled by calling RenderControlHelper's RenderControl() method.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( HtmlTextWriterAttribute.Id, ClientID );
            writer.AddAttribute( HtmlTextWriterAttribute.Class, $"cf-turnstile js-captcha {CssClass}" );
            writer.AddAttribute( "data-sitekey", SiteKey );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.RenderEndTag();

            CustomValidator.RenderControl( writer );

            _hfToken.RenderControl( writer );
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// Support class to handle the response Cloudflares captcha reponse
        /// </summary>
        private class CloudFlareCaptchaResponse
        {
            [JsonProperty( "success" )]
            public bool Success { get; set; }

            [JsonProperty( "challenge_ts" )]
            public DateTime ChallengeTimeStamp { get; set; }

            [JsonProperty( "hostname" )]
            public string HostName { get; set; }

            [JsonProperty( "error-codes" )]
            public List<string> ErrorCodes { get; set; }

            [JsonProperty( "action" )]
            public string Action { get; set; }

            [JsonProperty( "cdata" )]
            public string CustomerData { get; set; }
        }

        /// <summary>
        /// Event argument for the TokenReceivedEvent
        /// </summary>
        /// <seealso cref="System.EventArgs" />
        public class TokenReceivedEventArgs : EventArgs
        {
            /// <summary>
            /// Gets or sets the token.
            /// </summary>
            /// <value>
            /// The token.
            /// </value>
            public string Token { get; set; }

            /// <summary>
            /// Returns true if ... is valid.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
            /// </value>
            public bool IsValid { get; set; }
        }

        #endregion
    }
}
