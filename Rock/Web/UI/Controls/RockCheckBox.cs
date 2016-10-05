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
//
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A <see cref="T:System.Web.UI.WebControls.TextBox"/> control with an associated label.
    /// </summary>
    [ToolboxData( "<{0}:RockCheckBox runat=server></{0}:RockTextBox>" )]
    public class RockCheckBox : CheckBox, IRockControl
    {
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

        /// <summary>
        /// Gets or sets the group of controls for which the <see cref="T:System.Web.UI.WebControls.CheckBox" /> control causes validation when it posts back to the server.
        /// </summary>
        /// <returns>The group of controls for which the <see cref="T:System.Web.UI.WebControls.CheckBox" /> causes validation when it posts back to the server. The default is an empty string ("").</returns>
        public override string ValidationGroup
        {
            get
            {
                return base.ValidationGroup;
            }
            set
            {
                base.ValidationGroup = value;
                if ( RequiredFieldValidator != null )
                {
                    RequiredFieldValidator.ValidationGroup = value;
                }
            }
        }

        #endregion

        /// <summary>
        /// Gets or sets a value indicating whether [display inline].
        /// Defaults to False
        /// True will render the label with class="checkbox-inline"
        /// </summary>
        /// <value>
        ///   <c>true</c> if [display inline]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayInline
        {
            get
            {
                return this.ViewState["DisplayInline"] as bool? ?? false;
            }

            set
            {
                this.ViewState["DisplayInline"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the CSS class for the checkbox "div" (see Rock.Web.UI.Adapters.CheckboxAdaptor)
        /// </summary>
        /// <value>
        /// The container CSS class.
        /// </value>
        public string ContainerCssClass
        {
            get
            {
                return this.ViewState["ContainerCssClass"] as string ?? string.Empty;
            }

            set
            {
                this.ViewState["ContainerCssClass"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the CSS class for the checkbox &lt;input&gt;
        /// </summary>
        /// <value>
        /// The checkbox &lt;input&gt; CSS class.
        /// </value>
        public override string CssClass
        {
            get
            {
                return base.CssClass;
            }
            set
            {
                base.CssClass = value;
            }
        }

        /// <summary>
        /// Gets or sets the selected icon CSS class. If specified along with UnSelectedIcon, the default checkbox is hidden, and an icon is displayed instead
        /// </summary>
        public string SelectedIconCssClass
        {
            get { return ViewState["SelectedIconCssClass"] as string ?? string.Empty; }
            set { ViewState["SelectedIconCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the un-selected icon CSS class. If specified along with SelectedIcon, the default checkbox is hidden, and an icon is displayed instead
        /// </summary>
        public string UnSelectedIconCssClass
        {
            get { return ViewState["UnSelectedIconCssClass"] as string ?? string.Empty; }
            set { ViewState["UnSelectedIconCssClass"] = value; }
        }

        // Needed for rendering help block with no label value
        private string TemporaryHelpValue = string.Empty;

        // Needed for rendering warning block with no label value
        private string TemporaryWarningValue = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="RockCheckBox"/> class.
        /// </summary>
        public RockCheckBox()
            : base()
        {
            HelpBlock = new HelpBlock();
            WarningBlock = new WarningBlock();
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );
        }

        /// <summary>
        /// Registers client script for generating postback prior to rendering on the client if <see cref="P:System.Web.UI.WebControls.CheckBox.AutoPostBack" /> is true.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnPreRender( System.EventArgs e )
        {
            base.OnPreRender( e );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                bool renderLabel = ( !string.IsNullOrEmpty( Label ) );
                bool renderHelp = ( HelpBlock != null && !string.IsNullOrWhiteSpace( Help ) );
                bool renderWarning = ( WarningBlock != null && !string.IsNullOrWhiteSpace( Warning ) );

                // If rendering help text with no label, the CheckBoxAdapter will need to render the help, so it needs to be temporarily 
                // blanked out so that the RockControlHelper does not render it
                TemporaryHelpValue = Help;
                if (!renderLabel && renderHelp)
                {
                    Help = string.Empty;
                }

                // If rendering warning text with no label, the CheckBoxAdapter will need to render the warning, so it needs to be temporarily 
                // blanked out so that the RockControlHelper does not render it
                TemporaryWarningValue = Warning;
                if ( !renderLabel && renderWarning )
                {
                    Warning = string.Empty;
                }

                RockControlHelper.RenderControl( this, writer );
            }
        }

        /// <summary>
        /// Renders the base control.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public virtual void RenderBaseControl( HtmlTextWriter writer )
        {
            if (Enabled)
            {
                Help = TemporaryHelpValue;
                Warning = TemporaryWarningValue;

                if ( !string.IsNullOrWhiteSpace( SelectedIconCssClass ) && !string.IsNullOrWhiteSpace( UnSelectedIconCssClass ) )
                {
                    string postbackJS = string.Empty;
                    if ( this.AutoPostBack  )
                    {
                        postbackJS = this.Page.ClientScript.GetPostBackEventReference( new PostBackOptions( this ) );
                    }

                    base.Style.Add( HtmlTextWriterStyle.Display, "none" );
                    writer.WriteLine( string.Format(
                        "<div class='rock-checkbox-icon {5}' onclick=\"$('#{0}').prop('checked', !$('#{0}').prop('checked')); $(this).find('i').toggleClass('{1}').toggleClass('{2}'); {6} \" ><i class=\"{3}\"></i> {4}</div>", 
                            this.ClientID, // {0}
                            SelectedIconCssClass, // {1}
                            UnSelectedIconCssClass, // {2}
                            this.Checked ? SelectedIconCssClass : UnSelectedIconCssClass, // {3}
                            this.Text, // {4}
                            this.ContainerCssClass, // {5}
                            postbackJS // {6}
                            ) );
                }
                else
                {
                    base.Style.Remove( HtmlTextWriterStyle.Display );
                }

                base.RenderControl( writer );
            }
            else
            {
                string selectedCss = string.IsNullOrWhiteSpace( SelectedIconCssClass ) ? "fa fa-check-square-o" : SelectedIconCssClass;
                string unselectedCss = string.IsNullOrWhiteSpace( UnSelectedIconCssClass ) ? "fa fa-square-o" : UnSelectedIconCssClass;
                writer.WriteLine( string.Format( "<div class='rock-checkbox-icon text-muted'><i class=\"{0}\"></i> {1}</div>", this.Checked ? selectedCss : unselectedCss, this.Text ) );
            }
        }
    }
}