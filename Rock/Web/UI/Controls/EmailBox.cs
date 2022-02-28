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
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Communication;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A <see cref="T:Rock.Web.UI.Controls.RockTextBox" /> control with an associated label.
    /// </summary>
    [ToolboxData( "<{0}:EmailBox runat=server></{0}:EmailBox>" )]
    public class EmailBox : RockTextBox
    {
        private RegularExpressionValidator _regexValidator;
        
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( System.EventArgs e )
        {
            base.OnInit( e );

            var globalAttributes = GlobalAttributesCache.Get();
            if (globalAttributes != null)
            {
                this.PrependText = "<i class='fa fa-envelope'></i>";
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
                return base.IsValid && _regexValidator.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to allow multiple email addresses (comma-delimited)
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow multiple]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowMultiple
        {
            get
            {
                return this.ViewState["AllowMultiple"] as bool? ?? false;
            }
            
            set
            {
                this.ViewState["AllowMultiple"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to allow lava
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow lava]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowLava
        {
            get
            {
                return this.ViewState["AllowLava"] as bool? ?? false;
            }

            set
            {
                this.ViewState["AllowLava"] = value;
            }
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
            _regexValidator.ErrorMessage = "Email address is not valid";

            _regexValidator.ValidationExpression = EmailAddressFieldValidator.GetRegularExpression( this.AllowMultiple, this.AllowLava );

            Controls.Add( _regexValidator );
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
            }
        }

        /// <summary>
        /// Renders any data validator.
        /// </summary>
        /// <param name="writer">The writer.</param>
        protected override void RenderDataValidator( HtmlTextWriter writer )
        {
            base.RenderDataValidator( writer );

            _regexValidator.ValidationGroup = this.ValidationGroup;
            _regexValidator.RenderControl( writer );
        }

        /// <summary>
        /// Renders the base control.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public override void RenderBaseControl( HtmlTextWriter writer )
        {
            if ( this.AllowLava )
            {
                // Avoid input type='email' because it disallows valid Lava input.
                this.Attributes["type"] = "text";
            }
            else
            {
                this.Attributes["type"] = "email";
            }

            base.RenderBaseControl( writer );
        }
    }
}