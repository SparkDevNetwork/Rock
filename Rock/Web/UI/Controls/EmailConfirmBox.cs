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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 2 email fields that must match to be valid.
    /// </summary>
    /// <seealso cref="System.Web.UI.WebControls.CompositeControl" />
    /// <seealso cref="Rock.Web.UI.Controls.IRockControl" />
    [ToolboxData( "<{0}:EmailConfirmBox runat=server></{0}:EmailConfirmBox>" )]
    public class EmailConfirmBox : CompositeControl, IRockControl
    {
        #region IRockControl Implementation

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        [Bindable( true )]
        [Category( "Appearance" )]
        [DefaultValue( "" )]
        [Description( "The text for the label." )]
        public virtual string Label
        {
            get
            {
                // Suppress this control's label,
                // since the EmailBoxes have their own labels.
                return string.Empty;
            }

            set
            {
                EnsureChildControls();
                _ebPrimary.Label = value;
                _ebConfirm.Label = "Confirm " + value;
            }
        }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        [Bindable( true )]
        [Category( "Appearance" )]
        [DefaultValue( "" )]
        [Description( "The help block." )]
        public virtual string Help
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
        [Bindable( true )]
        [Category( "Appearance" )]
        [DefaultValue( "" )]
        [Description( "The warning block." )]
        public virtual string Warning
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
        /// Gets or sets a value indicating whether this is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        [Bindable( true )]
        [Category( "Behavior" )]
        [DefaultValue( "false" )]
        [Description( "Is the value required?" )]
        public virtual bool Required
        {
            get
            {
                return ViewState["Required"] as bool? ?? false;
            }

            set
            {
                ViewState["Required"] = value;
                EnsureChildControls();
                _ebPrimary.Required = value;
                _ebConfirm.Required = value;
            }
        }

        /// <summary>
        /// Gets or sets the required error message.  If blank, the LabelName name will be used
        /// </summary>
        /// <value>The required error message.</value>
        public virtual string RequiredErrorMessage
        {
            get
            {
                return RequiredFieldValidator != null ? RequiredFieldValidator.ErrorMessage : string.Empty;
            }

            set
            {
                if ( RequiredFieldValidator != null )
                {
                    RequiredFieldValidator.ErrorMessage = value;
                }
            }
        }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public virtual bool IsValid
        {
            get
            {
                EnsureChildControls();
                return ( !Required || RequiredFieldValidator == null || RequiredFieldValidator.IsValid ) &&
                       ( CustomValidator == null || CustomValidator.IsValid ) &&
                       _ebPrimary.IsValid && _ebConfirm.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets the form group class.
        /// </summary>
        /// <value>
        /// The form group class.
        /// </value>
        public virtual string FormGroupCssClass
        {
            get
            {
                return ViewState["FormGroupCssClass"] as string ?? string.Empty;
            }

            set
            {
                ViewState["FormGroupCssClass"] = value;
            }
        }

        /// <summary>
        /// Gets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        public virtual HelpBlock HelpBlock { get; set; }

        /// <summary>
        /// Gets the warning block.
        /// </summary>
        /// <value>
        /// The warning block.
        /// </value>
        public virtual WarningBlock WarningBlock { get; set; }

        /// <summary>
        /// Gets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public virtual RequiredFieldValidator RequiredFieldValidator { get; set; }

        /// <summary>
        /// Gets the compare validator.
        /// </summary>
        /// <value>
        /// The compare validator.
        /// </value>
        public virtual CustomValidator CustomValidator { get; set; }

        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public virtual string ValidationGroup
        {
            get
            {
                return ViewState["ValidationGroup"] as string;
            }

            set
            {
                ViewState["ValidationGroup"] = value;
                EnsureChildControls();
                _ebPrimary.ValidationGroup = value;
                _ebConfirm.ValidationGroup = value;
                CustomValidator.ValidationGroup = value;
            }
        }

        /// <summary>
        /// This is where you implement the simple aspects of rendering your control.  The rest
        /// will be handled by calling RenderControlHelper's RenderControl() method.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public virtual void RenderBaseControl(HtmlTextWriter writer)
        {
            writer.AddAttribute( "id", this.ClientID );
            writer.AddAttribute( "class", "js-emailConfirmControl " + this.CssClass );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            _ebPrimary.RenderControl( writer );
            CustomValidator.RenderControl( writer );
            _ebConfirm.RenderControl( writer );

            writer.RenderEndTag();
        }

        #endregion IRockControl Implementation

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="EmailConfirmBox"/> class.
        /// </summary>
        public EmailConfirmBox() : base()
        {
            CustomValidator = new CustomValidator();
            CustomValidator.ValidationGroup = this.ValidationGroup;

            HelpBlock = new HelpBlock();
            WarningBlock = new WarningBlock();
        }

        #endregion

        /// <summary>
        /// The message to show if validation fails
        /// </summary>
        public string ValidationMessage { get; set; }

        /// <summary>
        /// The value of the text 
        /// </summary>
        public string Text {
            get
            {
                EnsureChildControls();
                return _ebPrimary.Text ?? string.Empty;
            }

            set
            {
                EnsureChildControls();
                _ebPrimary.Text = value;
                _ebConfirm.Text = value;
            }
        }

        private EmailBox _ebPrimary;
        private EmailBox _ebConfirm;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad( e );

            EnsureChildControls();
        }

        /// <summary>
        /// Called just before rendering begins on the page.
        /// </summary>
        /// <param name="e">The EventArgs that describe this event.</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender( e );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl(HtmlTextWriter writer)
        {
            if ( this.Visible )
            {
                RockControlHelper.RenderControl( this, writer );
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            _ebPrimary = new EmailBox
            {
                ID = "ebPrimary",
                AllowLava = false,
                AllowMultiple = false,
                CssClass = "js-primary",
                Required = Required,
                ValidationGroup = ValidationGroup
            };
            Controls.Add( _ebPrimary );

            _ebConfirm = new EmailBox
            {
                ID = "ebConfirm",
                AllowLava = false,
                AllowMultiple = false,
                CssClass = "js-confirm",
                Required = Required,
                ValidationGroup = ValidationGroup
            };
            Controls.Add( _ebConfirm );

            CustomValidator = new CustomValidator
            {
                ID = "cv",
                ClientValidationFunction = "Rock.controls.emailConfirmControl.clientValidate",
                ErrorMessage = "Email and confirmation do not match.",
                CssClass = "validation-error help-inline",
                Enabled = true,
                Display = ValidatorDisplay.Dynamic,
                ValidationGroup = ValidationGroup
            };
            Controls.Add( CustomValidator );
        }
    }
}