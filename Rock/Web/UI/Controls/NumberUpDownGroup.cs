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

using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;

using Rock;

namespace Rock.Web.UI.Controls
{
    [ToolboxData( "<{0}:NumberUpDownGroup runat=server></{0}:NumberUpDown>" )]
    public class NumberUpDownGroup : CompositeControl, IRockControl, IDisplayRequiredIndicator
    {
        public CustomValidator GroupCustomValidator { get; set; }

        private List<NumberUpDown> _controlGroup = new List<NumberUpDown>();
        
        public List<NumberUpDown> ControlGroup
        {
            get
            {
                return _controlGroup;
            }
            set
            {
                _controlGroup = value;
            }
        }

        public string requiredLabel { get; set; }

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

        public HelpBlock HelpBlock { get; set; }

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

        public WarningBlock WarningBlock { get; set; }

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

        public RequiredFieldValidator RequiredFieldValidator { get; set; }

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
        
        public string ValidationGroup
        {
            get { return ViewState["ValidationGroup"] as string; }
            set
            {
                ViewState["ValidationGroup"] = value;

                if ( GroupCustomValidator != null )
                {
                    GroupCustomValidator.ValidationGroup = value;
                }
            }

            //get
            //{
            //    return GroupCustomValidator.ValidationGroup;
            //}
            //set
            //{
            //    GroupCustomValidator.ValidationGroup = value;
            //}
        }

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

        public bool IsValid
        {
            get
            {
                return !Required || GroupCustomValidator == null || GroupCustomValidator.IsValid;
            }
        }

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

        public NumberUpDownGroup() : base()
        {
            GroupCustomValidator = new CustomValidator
            {
                ValidationGroup = this.ValidationGroup
            };

            HelpBlock = new HelpBlock();
            WarningBlock = new WarningBlock();
        }

        public void RenderBaseControl( HtmlTextWriter writer )
        {
            foreach ( var control in ControlGroup )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Style, "margin: 10px;" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                // control label
                writer.AddAttribute( HtmlTextWriterAttribute.Style, "margin-bottom: 10px;" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.Write( control.Label );
                writer.RenderEndTag();

                // control
                control.RenderBaseControl( writer );

                writer.RenderEndTag();
            }

            GroupCustomValidator.RenderControl( writer );
        }

        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Style, "margin-bottom: 25px;" );
                RockControlHelper.RenderControl( this, writer );
            }
        }

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
