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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A <see cref="T:Rock.Web.UI.Controls.RockTextBox"/> control with an associated label.
    /// </summary>
    [ToolboxData( "<{0}:UrlLinkBox runat=server></{0}:UrlLinkBox>" )]
    public class UrlLinkBox : RockTextBox
    {
        // Client-side validator for well-formed URL parts.
        private RegularExpressionValidator _regexValidator;
        // https://www.regextester.com/1965
        // Modified from link above to support urls like "http://localhost:6229/Person/1/Edit" (Url does not have a period)
        private readonly string _regexUrl = @"^(http[s]?:\/\/)?[^\s([" + '"' + @" <,>]*\.?[^\s[" + '"' + @",><]*$";
        private readonly string _regexUrlWithTrailingForwardSlash = @"^(http[s]?:\/\/)?[^\s([" + '"' + @" <,>]*\.?[^\s[" + '"' + @",><]*\/$";
        private readonly string _validationErrorMessage = "The link provided is not valid.";

        // Server-side validator for base URL.
        private CustomValidator _baseUrlValidator = null;
        private List<string> _baseUrls = new List<string>();

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlLinkBox"/> class.
        /// </summary>
        public UrlLinkBox() : base()
        {
            // Prevent ViewState from being disabled by the container, because it is necessary for this control to operate correctly.
            this.ViewStateMode = ViewStateMode.Enabled;

            // Default validation display mode to use the ValidationSummary control rather than inline.
            this.ValidationDisplay = ValidatorDisplay.None;
        }

        #endregion

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( System.EventArgs e )
        {
            base.OnInit( e );

            // Initialize the Base URL and aliases.
            _baseUrls = this.BaseUrlAliases.SplitDelimitedValues( "," ).ToList();

            if ( !string.IsNullOrWhiteSpace( this.BaseUrl ) )
            {
                // Display the default BaseURL as a tooltip for the text entry field.
                this.PrependText = $"<i class='fa fa-link' data-toggle='tooltip' title=\"{ this.BaseUrl }...\"'></i>";

                _baseUrls.Add( this.BaseUrl );
            }
            else
            {
                this.PrependText = "<i class='fa fa-link'></i>";
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public override bool IsValid
        {
            get
            {
                EnsureChildControls();
                return base.IsValid && _regexValidator.IsValid && _baseUrlValidator.IsValid;
            }
        }

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether a trailing forward slash should be required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [should require forward slash]; otherwise, <c>false</c>.
        /// </value>
        public bool ShouldRequireTrailingForwardSlash { get; set; }

        /// <summary>
        /// Gets or sets a base URL that will be prepended if it does not exist.
        /// </summary>
        public string BaseUrl
        {
            get { return ViewState["BaseUrl"] as string; }
            set { ViewState["BaseUrl"] = value; }
        }

        /// <summary>
        /// A comma-delimited list of valid Base URLs.
        /// If an absolute URL is entered, it must start with one of these bases.
        /// If it exists, an alias will be stripped from the entry and replaced with the Base URL.
        /// </summary>
        public string BaseUrlAliases
        {
            get { return ViewState["BaseUrlAliases"] as string; }
            set { ViewState["BaseUrlAliases"] = value; }
        }

        /// <summary>
        /// Sets the validation message display mode for the control.
        /// </summary>
        public ValidatorDisplay ValidationDisplay
        {
            get { return ViewState["ValidationDisplay"].ToStringSafe().ConvertToEnum<ValidatorDisplay>( ValidatorDisplay.None ); }
            set { ViewState["ValidationDisplay"] = value; }
        }

        /// <summary>
        /// Gets or sets the URL, adding or removing the Base URL as necessary.
        /// </summary>
        public string Url
        {
            get
            {
                var url = ReplaceBaseUrlAliases( this.Text, this.BaseUrl );
                return url;
            }
            set
            {
                this.Text = ReplaceBaseUrlAliases( value, string.Empty );
            }
        }

        #endregion

        /// <summary>
        /// Gets the regular expression for validating the URL depending on whether or not
        /// the trailing forward slash is required.
        /// </summary>
        /// <returns></returns>
        private string GetUrlRegEx()
        {
            if ( ShouldRequireTrailingForwardSlash )
            {
                return _regexUrlWithTrailingForwardSlash;
            }
            return _regexUrl;
        }

        /// <summary>
        /// Gets the validation error message for value depending on whether or not
        /// the trailing forward slash is required.
        /// </summary>
        /// <returns></returns>
        private string GetValidationErrorMessage()
        {
            if ( ShouldRequireTrailingForwardSlash )
            {
                return _validationErrorMessage + " Please ensure the URL ends with a forward slash.";
            }
            return _validationErrorMessage;
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            _regexValidator = new RegularExpressionValidator();
            _regexValidator.ID = this.ID + "_RE";
            _regexValidator.ControlToValidate = this.ID;
            _regexValidator.Display = ValidatorDisplay.Dynamic;
            _regexValidator.CssClass = "validation-error help-inline";
            _regexValidator.ValidationExpression = GetUrlRegEx();
            _regexValidator.ErrorMessage = GetValidationErrorMessage();
            Controls.Add( _regexValidator );

            _baseUrlValidator = new CustomValidator();
            _baseUrlValidator.ID = this.ID + "_BV";
            _baseUrlValidator.ControlToValidate = this.ID;
            _baseUrlValidator.Display = ValidatorDisplay.Dynamic;
            _baseUrlValidator.CssClass = "validation-error help-inline";
            _baseUrlValidator.EnableClientScript = false;
            _baseUrlValidator.ServerValidate += _baseUrlValidator_ServerValidate;
            Controls.Add( _baseUrlValidator );
        }

        /// <summary>
        /// Gets or sets the group of controls for which the <see cref="T:System.Web.UI.WebControls.TextBox" /> control causes validation when it posts back to the server.
        /// </summary>
        /// <returns>The group of controls for which the <see cref="T:System.Web.UI.WebControls.TextBox" /> control causes validation when it posts back to the server. The default value is an empty string ("").</returns>
        public override string ValidationGroup
        {
            get
            {
                return base.ValidationGroup;
            }
            set
            {
                base.ValidationGroup = value;

                EnsureChildControls();
                _regexValidator.ValidationGroup = value;
                _baseUrlValidator.ValidationGroup = value;
            }
        }

        /// <summary>
        /// Renders any data validator.
        /// </summary>
        /// <param name="writer">The writer.</param>
        protected override void RenderDataValidator( HtmlTextWriter writer )
        {
            if ( this.RequiredFieldValidator != null )
            {
                this.RequiredFieldValidator.Display = this.ValidationDisplay;
            }

            base.RenderDataValidator( writer );

            _regexValidator.ValidationExpression = GetUrlRegEx();
            _regexValidator.ErrorMessage = GetValidationErrorMessage();
            _regexValidator.Display = this.ValidationDisplay;
            _regexValidator.ValidationGroup = this.ValidationGroup;
            _regexValidator.RenderControl( writer );

            _baseUrlValidator.Display = this.ValidationDisplay;
            _baseUrlValidator.RenderControl( writer );
        }

        /// <summary>
        /// Renders the base control.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public override void RenderBaseControl( HtmlTextWriter writer )
        {
            this.Attributes["type"] = "url";

            base.RenderBaseControl( writer );
        }

        #region Internal methods

        /// <summary>
        /// Handles the ServerValidate event for the baseUrlValidator.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        private void _baseUrlValidator_ServerValidate( object source, ServerValidateEventArgs args )
        {
            // If entry is empty or no base URL to validate, exit.
            if ( string.IsNullOrWhiteSpace( args.Value ) || string.IsNullOrWhiteSpace( this.BaseUrl ) )
            {
                args.IsValid = true;
                return;
            }

            // Verify that the base URL matches one of the valid bases.
            var url = this.Url;
            foreach ( var baseUrl in _baseUrls )
            {
                if ( url.StartsWith( baseUrl, StringComparison.OrdinalIgnoreCase ) )
                {
                    args.IsValid = true;
                    return;
                }
            }

            args.IsValid = false;

            if ( !string.IsNullOrWhiteSpace( this.BaseUrl ) )
            {
                _baseUrlValidator.Text = $"{this.Label} URL must match the pattern \"{this.BaseUrl}...\"";
                _baseUrlValidator.ErrorMessage = $"{this.Label} URL must match the pattern \"{this.BaseUrl}...\"";
            }
        }

        private string ReplaceBaseUrlAliases( string input, string newBaseUrl )
        {
            var output = input;
            if ( string.IsNullOrWhiteSpace( output ) )
            {
                return output;
            }

            // Recursively remove the base URL and any aliases from the input.
            bool doReplace;
            do
            {
                output = output.Trim();

                // Strip recognized Base Urls from the input text.
                doReplace = false;
                foreach ( var baseUrl in _baseUrls )
                {
                    if ( !string.IsNullOrWhiteSpace( baseUrl ) )
                    {
                        if ( output.StartsWith( baseUrl, StringComparison.OrdinalIgnoreCase ) )
                        {
                            output = output.Substring( baseUrl.Length );
                            doReplace = true;
                        }
                    }
                }
            } while ( doReplace );

            // If the remaining text represents a relative URL, add the new base URL.
            if ( !string.IsNullOrWhiteSpace( newBaseUrl )
                 && Uri.IsWellFormedUriString( output, UriKind.Relative ) )
            {
                output = newBaseUrl.TrimEnd( '/' ) + "/" + output.TrimStart( '/' );
            }

            return output;
        }

        #endregion
    }
}