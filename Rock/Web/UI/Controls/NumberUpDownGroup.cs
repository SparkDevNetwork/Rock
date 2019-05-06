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

using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A wrapper class for a collection of NumberUpDown controls
    /// </summary>
    /// <seealso cref="System.Web.UI.WebControls.CompositeControl" />
    /// <seealso cref="Rock.Web.UI.Controls.IRockControl" />
    /// <seealso cref="Rock.Web.UI.Controls.IDisplayRequiredIndicator" />
    [ToolboxData( "<{0}:NumberUpDownGroup runat=server></{0}:NumberUpDown>" )]
    public class NumberUpDownGroup : CompositeControl, IRockControl, IDisplayRequiredIndicator
    {
        /// <summary>
        /// Gets or sets the group custom validator.
        /// </summary>
        /// <value>
        /// The group custom validator.
        /// </value>
        public CustomValidator GroupCustomValidator { get; set; }

        /// <summary>
        /// Gets or sets the collection of NumberUpDown objects.
        /// </summary>
        /// <value>
        /// The control group.
        /// </value>
        public List<NumberUpDown> ControlGroup
        {
            get
            {
                var controlGroup = new List<NumberUpDown>();

                foreach ( Control control in Controls )
                {
                    if ( control is NumberUpDown )
                    {
                        var numberUpDown = control as NumberUpDown;
                        if (numberUpDown != null )
                        {
                            controlGroup.Add( numberUpDown );
                        }
                    }
                }

                return controlGroup;
            }
        }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label text
        /// </value>
        [Bindable( true )]
        [Category( "Appearance" )]
        [DefaultValue( "" )]
        [Description( "The text for the label." )]
        public string Label
        {
            get
            {
                return ViewState["Label"] as string ?? string.Empty;
            }

            set
            {
                ViewState["Label"] = value;
            }
        }

        /// <summary>
        /// Gets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        public HelpBlock HelpBlock { get; set; }

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
        /// Gets the warning block.
        /// </summary>
        /// <value>
        /// The warning block.
        /// </value>
        public WarningBlock WarningBlock { get; set; }

        /// <summary>
        /// Gets or sets the warning text.
        /// </summary>
        /// <value>
        /// The warning text.
        /// </value>
        [Bindable( true )]
        [Category( "Appearance" )]
        [DefaultValue( "" )]
        [Description( "The help block." )]
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
        /// Gets or sets a value indicating whether this <see cref="IRockControl" /> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        [Bindable( true )]
        [Category( "Behavior" )]
        [DefaultValue( "false" )]
        [Description( "Is the value required?" )]
        public bool Required
        {
            get
            {
                return ViewState["Required"] as bool? ?? false;
            }

            set
            {
                ViewState["Required"] = value;
            }
        }

        /// <summary>
        /// Gets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

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
                return GroupCustomValidator != null ? GroupCustomValidator.ErrorMessage : string.Empty;
            }

            set
            {
                if ( GroupCustomValidator != null )
                {
                    GroupCustomValidator.ErrorMessage = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get {
                return ViewState["ValidationGroup"] as string;
            }

            set
            {
                ViewState["ValidationGroup"] = value;

                if ( GroupCustomValidator != null )
                {
                    GroupCustomValidator.ValidationGroup = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the Required indicator when Required=true
        /// </summary>
        /// <value>
        ///   <c>true</c> if [display required indicator]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayRequiredIndicator
        {
            get
            {
                return ViewState["DisplayRequiredIndicator"] as bool? ?? true;
            }

            set
            {
                ViewState["DisplayRequiredIndicator"] = value;
            }
        }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid
        {
            get
            {
                return !Required || GroupCustomValidator == null || GroupCustomValidator.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets the form group class.
        /// </summary>
        /// <value>
        /// The form group class.
        /// </value>
        public string FormGroupCssClass
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
        /// Initializes a new instance of the <see cref="NumberUpDownGroup"/> class.
        /// </summary>
        public NumberUpDownGroup() : base()
        {
            GroupCustomValidator = new CustomValidator
            {
                ValidationGroup = this.ValidationGroup
            };

           // ControlGroup = new List<NumberUpDown>();
            HelpBlock = new HelpBlock();
            WarningBlock = new WarningBlock();
        }

        /// <summary>
        /// This is where you implement the simple aspects of rendering your control.  The rest
        /// will be handled by calling RenderControlHelper's RenderControl() method.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            foreach ( var control in ControlGroup )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "margin-l-sm margin-b-sm" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                // control label
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "margin-b-sm" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.Write( control.Label );
                writer.RenderEndTag();

                // control
                control.RenderBaseControl( writer );

                writer.RenderEndTag();
            }

            GroupCustomValidator.RenderControl( writer );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "margin-b-md" );
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

            GroupCustomValidator.ID = this.ID + "_cfv";
            GroupCustomValidator.ControlToValidate = this.ID;
            GroupCustomValidator.ErrorMessage = this.RequiredErrorMessage;
            GroupCustomValidator.CssClass = "validation-error help-inline";
            GroupCustomValidator.Enabled = true;
            GroupCustomValidator.Display = ValidatorDisplay.Dynamic;
            GroupCustomValidator.ValidationGroup = ValidationGroup;

            // Need custom script to ensure at least one of the controls has a value > 0
            GroupCustomValidator.ClientValidationFunction = "Rock.controls.numberUpDownGroup.clientValidate";

            Controls.Add( GroupCustomValidator );
        }
    }
}
