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

using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Volume / Page / Entry Control that is useful for sacraments or steps (<see cref="Rock.Model.Step"/>)
    /// </summary>
    /// <seealso cref="System.Web.UI.WebControls.CompositeControl" />
    /// <seealso cref="Rock.Web.UI.Controls.IRockControl" />
    public class RegistryEntry : CompositeControl, IRockControl
    {
        private TextBox tbVolume;
        private TextBox tbPage;
        private TextBox tbLine;

        /// <summary>
        /// Gets or sets the text for the control.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        public string Text
        {
            get
            {
                EnsureChildControls();
                string volume = tbVolume.Text.IsNullOrWhiteSpace() ? string.Empty : tbVolume.Text;
                string page = tbPage.Text.IsNullOrWhiteSpace() ? string.Empty : tbPage.Text;
                string line = tbLine.Text.IsNullOrWhiteSpace() ? string.Empty : tbLine.Text;

                return $"{volume},{page},{line}";
            }
            set
            {
                EnsureChildControls();
                string[] values = ( value ?? string.Empty ).Split( ',' );
                if ( values.Length != 3 )
                {
                    // Need three numbers and only three numbers.
                    return;
                }

                tbVolume.Text = values[0].AsIntegerOrNull() != null ? values[0] : string.Empty;
                tbPage.Text = values[1].AsIntegerOrNull() != null ? values[1] : string.Empty;
                tbLine.Text = values[2].AsIntegerOrNull() != null ? values[2] : string.Empty;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistryEntry"/> class.
        /// </summary>
        public RegistryEntry() : base()
        {
            RequiredFieldValidator = new HiddenFieldValidator();
            HelpBlock = new HelpBlock();
            WarningBlock = new WarningBlock();
        }

        #region IRockControl Implementation

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label text
        /// </value>
        [Bindable( true )]
        [Category( "Appearance" )]
        [DefaultValue( "" )]
        [Description( "The text for the label.")]
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
        /// Gets or sets the warning text.
        /// </summary>
        /// <value>
        /// The warning text.
        /// </value>
        [Bindable( true )]
        [Category( "Appearance" )]
        [DefaultValue( "" )]
        [Description( "The warning block." )]
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
        /// Gets or sets the required error message.  If blank, the LabelName name will be used
        /// </summary>
        /// <value>
        /// The required error message.
        /// </value>
        public string RequiredErrorMessage
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
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsValid
        {
            get
            {
                return !Required || RequiredFieldValidator == null || RequiredFieldValidator.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets the form group class.
        /// </summary>
        /// <value>
        /// The form group class.
        /// </value>
        [Bindable( true )]
        [Category( "Appearance" )]
        [Description( "The CSS class to add to the form-group div." )]
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
        /// Gets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        public HelpBlock HelpBlock { get; set; }

        /// <summary>
        /// Gets the warning block.
        /// </summary>
        /// <value>
        /// The warning block.
        /// </value>
        public WarningBlock WarningBlock { get; set; }

        /// <summary>
        /// Gets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

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
                return RequiredFieldValidator.ValidationGroup;
            }
            set
            {
                RequiredFieldValidator.ValidationGroup = value;
            }
        }

        /// <summary>
        /// Renders the base control.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( "class", "form-control-group row form-row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // Bootstrap this to horizontal
            writer.AddAttribute( "class", "col-sm-4" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            tbVolume.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-sm-4" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            tbPage.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-sm-4" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            tbLine.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();
        }

        #endregion IRockControl Implementation

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
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            tbVolume = new TextBox
            {
                ID = $"tbVolume_{this.ID}",
                TextMode = TextBoxMode.Number,
                CssClass = "form-control"
            };

            tbVolume.Attributes.Add( "placeholder", "Volume" );

            tbPage = new TextBox
            {
                ID = $"",
                TextMode = TextBoxMode.Number,
                CssClass = "form-control"
            };

            tbPage.Attributes.Add( "placeholder", "Page" );

            tbLine = new TextBox
            {
                ID = $"",
                TextMode = TextBoxMode.Number,
                CssClass = "form-control"
            };

            tbLine.Attributes.Add( "placeholder", "Line" );

            Controls.Add( tbVolume );
            Controls.Add( tbPage );
            Controls.Add( tbLine );

        }
    }
}
