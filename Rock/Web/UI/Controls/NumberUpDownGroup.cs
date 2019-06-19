﻿// <copyright>
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
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( System.EventArgs e )
        {
            EnsureChildControls();
            base.OnInit( e );
        }

        /// <summary>
        /// Gets or sets the group custom validator.
        /// </summary>
        /// <value>
        /// The group custom validator.
        /// </value>
        public CustomValidator GroupCustomValidator { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="NumberUpDown"/> controls in this NumberUpDownGroup
        /// </summary>
        /// <value>
        /// The number up down controls.
        /// </value>
        public List<NumberUpDown> NumberUpDownControls { get; set; } = new List<NumberUpDown>();

        /// <summary>
        /// Gets or sets the collection of NumberUpDown objects.
        /// </summary>
        /// <value>
        /// The control group.
        /// </value>
        [System.Obsolete( "Use NumberUpDownControls Instead" )]
        [RockObsolete( "1.9" )]
        public List<NumberUpDown> ControlGroup
        {
            get
            {
                return NumberUpDownControls;
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
                EnsureChildControls();
                return GroupCustomValidator.ErrorMessage;
            }

            set
            {
                EnsureChildControls();
                GroupCustomValidator.ErrorMessage = value;
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
            get
            {
                EnsureChildControls();
                return ViewState["ValidationGroup"] as string;
            }

            set
            {
                EnsureChildControls();
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
            GroupCustomValidator = new CustomValidator();

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
            foreach ( NumberUpDown numberUpDown in NumberUpDownControls )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "margin-l-sm margin-b-sm" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                // control label
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "margin-b-sm" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.Write( numberUpDown.Label );
                writer.RenderEndTag();

                // control
                numberUpDown.RenderBaseControl( writer );

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
                string required = Required ? " required" : string.Empty;
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-group margin-b-md js-number-up-down-group " + required );
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

            foreach( var numberUpDown in this.NumberUpDownControls)
            {
                Controls.Add( numberUpDown );
            }

            GroupCustomValidator.ID = this.ID + "_cfv";
            GroupCustomValidator.ClientValidationFunction = "Rock.controls.numberUpDownGroup.clientValidate";
            GroupCustomValidator.ErrorMessage = this.RequiredErrorMessage;
            GroupCustomValidator.CssClass = "validation-error help-inline";
            GroupCustomValidator.Enabled = true;
            GroupCustomValidator.Display = ValidatorDisplay.Dynamic;
            GroupCustomValidator.ValidationGroup = ValidationGroup;
            Controls.Add( GroupCustomValidator );
        }
    }
}
