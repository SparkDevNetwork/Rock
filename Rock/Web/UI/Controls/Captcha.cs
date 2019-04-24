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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using RestSharp;

using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Provides a Captcha control that verifies the user is a real person.
    /// </summary>
    public class Captcha : WebControl, IRockControl
    {
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
        protected bool? _isResponseValid { get; set; }

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
                return ViewState["RequiredErrorMessage"] as string ?? string.Empty;
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
                return !Required || !string.IsNullOrWhiteSpace( HttpContext.Current.Request.Params["g-recaptcha-response"] );
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
            SiteKey = GlobalAttributesCache.Value( "core_GoogleReCaptchaSiteKey" );
            SecretKey = GlobalAttributesCache.Value( "core_GoogleReCaptchaSecretKey" );
        }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Create all our child controls and add them to the DOM.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            CustomValidator.ID = ID + "_cfv";
            CustomValidator.CssClass = "validation-error help-inline js-captcha-validator";
            CustomValidator.ClientValidationFunction = "Rock.controls.captcha.clientValidate";
            CustomValidator.ErrorMessage = RequiredErrorMessage;
            CustomValidator.Enabled = true;
            CustomValidator.Display = ValidatorDisplay.Dynamic;
            CustomValidator.ValidationGroup = ValidationGroup;
            Controls.Add( CustomValidator );
        }

        /// <summary>
        /// Called just before rendering begins on the page.
        /// </summary>
        /// <param name="e">The EventArgs that describe this event.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            var rockPage = Page as RockPage;

            if ( rockPage != null )
            {
                rockPage.AddScriptLink( "https://www.google.com/recaptcha/api.js?render=explicit&onload=Rock_controls_captcha_onloadInitialize", false );
            }

            string script = string.Format( @"
;(function () {{
    Rock.controls.captcha.initialize({{id: '{0}', key: '{1}'}});
}})();
", ClientID, SiteKey );

            ScriptManager.RegisterStartupScript( this, GetType(), "captcha-" + ClientID, script, true );
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
            var userResponse = HttpContext.Current.Request.Params["g-recaptcha-response"];
            string remoteIp = HttpContext.Current.Request.UserHostAddress;

            if ( _isResponseValid.HasValue )
            {
                return _isResponseValid.Value;
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
                var client = new RestClient( "https://www.google.com/recaptcha/api/siteverify" );
                var request = new RestRequest( Method.POST );

                request.AddParameter( "secret", SecretKey );
                request.AddParameter( "response", userResponse );
                request.AddParameter( "remoteip", remoteIp );
                request.Timeout = 5000;

                var response = client.Execute<ReCaptchaResponse>( request );

                _isResponseValid = JsonConvert.DeserializeObject<ReCaptchaResponse>( response.Content ).Success;
            }
            catch (Exception e)
            {
                Rock.Model.ExceptionLogService.LogException( e );
                _isResponseValid = false;
            }

            return _isResponseValid.Value;
        }

        /// <summary>
        /// This is where you implement the simple aspects of rendering your control.  The rest
        /// will be handled by calling RenderControlHelper's RenderControl() method.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            string errorMessage;

            if ( !string.IsNullOrWhiteSpace( RequiredErrorMessage ) )
            {
                errorMessage = RequiredErrorMessage;
            }
            else if ( !string.IsNullOrWhiteSpace( Label ) )
            {
                errorMessage = Label + " Is Required";
            }
            else
            {
                errorMessage = "Please complete the captcha";
            }

            writer.AddAttribute( HtmlTextWriterAttribute.Id, ClientID );
            writer.AddAttribute( "data-required", Required.ToString().ToLower() );
            writer.AddAttribute( "data-required-error-message", errorMessage );
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "js-captcha " + CssClass );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.RenderEndTag();

            CustomValidator.RenderControl( writer );
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// Support class to handle the response from the recaptcha verification.
        /// </summary>
        private class ReCaptchaResponse
        {
            [JsonProperty( "success" )]
            public bool Success { get; set; }

            [JsonProperty( "hostname" )]
            public string Hostname { get; set; }

            [JsonProperty( "error-codes" )]
            public string[] ErrorCodes { get; set; }
        }

        #endregion
    }
}
