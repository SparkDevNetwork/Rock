// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A <see cref="T:System.Web.UI.WebControls.TextBox"/> control with an associated label.
    /// </summary>
    [ToolboxData( "<{0}:Toggle runat=server></{0}:Toggle>" )]
    public class Toggle : CompositeControl, IRockControl
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
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Toggle"/> class.
        /// </summary>
        public Toggle()
        {
            HelpBlock = new HelpBlock();
        }

        #region Controls

        /// <summary>
        /// The "On" button 
        /// </summary>
        private HtmlAnchor _btnOn;

        /// <summary>
        /// The "Off" button 
        /// </summary>
        private HtmlAnchor _btnOff;

        /// <summary>
        /// The hiddenfield for storing if checked (toggled On)
        /// </summary>
        private HiddenFieldWithClass _hfChecked;

        #endregion

        /// <summary>
        /// Gets or sets the on text.
        /// </summary>
        /// <value>
        /// The on text.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "On" ),
        Description( "The text to display for the On button." )
        ]
        public string OnText
        {
            get
            {
                return ViewState["OnText"] as string ?? "On";
            }

            set
            {
                ViewState["OnText"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the active button CSS class.
        /// </summary>
        /// <value>
        /// The active button CSS class.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "" ),
        Description( "The CssClass to apply to the active button." )
        ]
        public string ActiveButtonCssClass
        {
            get
            {
                return ViewState["ActiveButtonCssClass"] as string ?? "";
            }

            set
            {
                ViewState["ActiveButtonCssClass"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the button size CSS class.
        /// </summary>
        /// <value>
        /// The button size CSS class.
        /// </value>
        /// <example>
        /// btn-lg, btn-sm, btn-xs, or leave blank (default size)
        /// </example>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "" ),
        Description( "The CssClass to apply to both the on and off buttons." )
        ]
        public string ButtonSizeCssClass
        {
            get
            {
                return ViewState["ButtonCssClass"] as string ?? "";
            }

            set
            {
                ViewState["ButtonCssClass"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the off text.
        /// </summary>
        /// <value>
        /// The off text.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "Off" ),
        Description( "The text to display for the Off button." )
        ]
        public string OffText
        {
            get
            {
                return ViewState["OffText"] as string ?? "Off";
            }

            set
            {
                ViewState["OffText"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the on CSS class.
        /// </summary>
        /// <value>
        /// The on Css Class.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "" ),
        Description( "The optional CSS class to apply to the on state when active." )
        ]
        public string OnCssClass
        {
            get
            {
                return ViewState["OnCssClass"] as string ?? "";
            }

            set
            {
                ViewState["OnCssClass"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the off CSS class.
        /// </summary>
        /// <value>
        /// The off Css Class.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "" ),
        Description( "The optional CSS class to apply to the off state when active." )
        ]
        public string OffCssClass
        {
            get
            {
                return ViewState["OffCssClass"] as string ?? "";
            }

            set
            {
                ViewState["OffCssClass"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Toggle"/> is checked.
        /// </summary>
        /// <value>
        ///   <c>true</c> if checked; otherwise, <c>false</c>.
        /// </value>
        public bool Checked
        {
            get
            {
                EnsureChildControls();
                return _hfChecked.Value.AsBooleanOrNull() ?? false;
            }

            set
            {
                EnsureChildControls();
                _hfChecked.Value = value.ToString();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( CheckedChanged != null )
            {
                EnsureChildControls();
                _btnOn.ServerClick += btnOnOff_ServerClick;
                _btnOff.ServerClick += btnOnOff_ServerClick;
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Registers the javascript.
        /// </summary>
        private void RegisterJavascript()
        {
            var script = string.Format( @"Rock.controls.toggleButton.initialize({{ id: '{0}', activeButtonCssClass: '{1}', onButtonCssClass: '{2}', offButtonCssClass: '{3}' }});", this.ClientID, this.ActiveButtonCssClass, this.OnCssClass, this.OffCssClass );
            ScriptManager.RegisterStartupScript( this, this.GetType(), "toggle-script" + this.ClientID, script, true );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            _hfChecked = new HiddenFieldWithClass();
            _hfChecked.CssClass = "js-toggle-checked";
            _hfChecked.ID = this.ID + "_hfChecked";
            
            _btnOn = new HtmlAnchor();
            _btnOn.ID = this.ID + "_btnOn";

            _btnOff = new HtmlAnchor();
            _btnOff.ID = this.ID + "_btnOff";

            Controls.Add( _hfChecked );
            Controls.Add( _btnOn );
            Controls.Add( _btnOff );
        }

        /// <summary>
        /// Handles the ServerClick event of the btnOnOff control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnOnOff_ServerClick( object sender, EventArgs e )
        {
            if ( CheckedChanged != null )
            {
                this.Checked = sender == _btnOn;
                CheckedChanged( this, new EventArgs() );
            }
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( "id", this.ClientID.ToString() );
            writer.AddAttribute( "class", "toggle-container " + this.CssClass );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "btn-group btn-toggle " + this.CssClass );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            _btnOn.Attributes["class"] = "btn btn-default js-toggle-on " + this.ButtonSizeCssClass;
            _btnOn.InnerText = this.OnText;
            _btnOff.Attributes["class"] = "btn btn-default js-toggle-off " + this.ButtonSizeCssClass;
            _btnOff.InnerText = this.OffText;
            
            if ( this.Checked )
            {
                _btnOn.AddCssClass( this.ActiveButtonCssClass + " " + this.OnCssClass + " active" );
                //_btnOff.RemoveCssClass( this.OffCssClass );
            }
            else
            {
                _btnOff.AddCssClass( this.ActiveButtonCssClass + " " + this.OffCssClass + " active" );
                //_btnOn.RemoveCssClass( this.OnCssClass );
            }

            _btnOn.RenderControl( writer );
            _btnOff.RenderControl( writer );

            writer.RenderEndTag();

            _hfChecked.RenderControl( writer );

            writer.RenderEndTag();

            RegisterJavascript();
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
                return string.Empty;
            }

            set
            {
                // intentionally blank
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
        /// Occurs when [checked changed].
        /// </summary>
        public event EventHandler CheckedChanged;
    }
}