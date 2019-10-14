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

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// NumberUpDown control
    /// </summary>
    [ToolboxData( "<{0}:NumberUpDown runat=server></{0}:NumberUpDown>" )]
    public class NumberUpDown : CompositeControl, IRockControl, IDisplayRequiredIndicator, IPostBackEventHandler
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
        public virtual bool Required
        {
            get { return ViewState["Required"] as bool? ?? false; }
            set { ViewState["Required"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the Required indicator when Required=true
        /// </summary>
        /// <value>
        /// <c>true</c> if [display required indicator]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayRequiredIndicator
        {
            get { return ViewState["DisplayRequiredIndicator"] as bool? ?? true; }
            set { ViewState["DisplayRequiredIndicator"] = value; }
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
        /// Gets or sets the group of controls for which the <see cref="T:System.Web.UI.WebControls.TextBox" /> control causes validation when it posts back to the server.
        /// </summary>
        /// <returns>The group of controls for which the <see cref="T:System.Web.UI.WebControls.TextBox" /> control causes validation when it posts back to the server. The default value is an empty string ("").</returns>
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

        #endregion

        #region Controls

        HiddenFieldWithClass _hfMin;
        HiddenFieldWithClass _hfMax;
        HiddenFieldWithClass _hfNumber;
        Label _lblNumber;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the minimum.
        /// </summary>
        /// <value>
        /// The minimum.
        /// </value>
        public int Minimum
        {
            get
            {
                EnsureChildControls();
                return _hfMin.Value.AsIntegerOrNull() ?? int.MinValue;
            }
            set
            {
                EnsureChildControls();
                _hfMin.Value = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the maximum.
        /// </summary>
        /// <value>
        /// The maximum.
        /// </value>
        public int Maximum
        {
            get
            {
                EnsureChildControls();
                return _hfMax.Value.AsIntegerOrNull() ?? int.MaxValue;
            }
            set
            {
                EnsureChildControls();
                _hfMax.Value = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the number.
        /// </summary>
        /// <value>
        /// The number.
        /// </value>
        public int Value
        {
            get
            {
                EnsureChildControls();
                return _hfNumber.ValueAsInt();
            }
            set
            {
                EnsureChildControls();
                _hfNumber.Value = value.ToString();
                _lblNumber.Text = value.ToString();
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberUpDown"/> class.
        /// </summary>
        public NumberUpDown() : base()
        {
            RequiredFieldValidator = new HiddenFieldValidator();
            RequiredFieldValidator.ValidationGroup = this.ValidationGroup;
            HelpBlock = new HelpBlock();
            WarningBlock = new WarningBlock();
        }

        #endregion

        #region Control Methods

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            _hfMin = new HiddenFieldWithClass();
            _hfMin.ID = string.Format( "{0}_hfMin", this.ID );
            _hfMin.CssClass = "js-number-up-down-min";
            _hfMin.Value = int.MinValue.ToString();
            Controls.Add( _hfMin );

            _hfMax = new HiddenFieldWithClass();
            _hfMax.ID = string.Format( "{0}_hfMax", this.ID );
            _hfMax.CssClass = "js-number-up-down-max";
            _hfMax.Value = int.MaxValue.ToString();
            Controls.Add( _hfMax );

            _hfNumber = new HiddenFieldWithClass();
            _hfNumber.ID = string.Format( "{0}_hfNumber", this.ID );
            _hfNumber.CssClass = "js-number-up-down-value";
            _hfNumber.Value = "0";
            Controls.Add( _hfNumber );

            _lblNumber = new Label();
            _lblNumber.ID = string.Format( "{0}_lblNumber", this.ID );
            Controls.Add( _lblNumber );

            RequiredFieldValidator.InitialValue = string.Empty;
            RequiredFieldValidator.ControlToValidate = _hfNumber.ID;
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
        /// Renders the base control.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            // Ensure valid values first
            Minimum = Minimum > Maximum ? Maximum : Minimum;
            Value = Value > Maximum ? Maximum : Value;
            Value = Value < Minimum ? Minimum : Value;

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "numberincrement margin-t-sm " + CssClass );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            _hfMin.RenderControl( writer );
            _hfMax.RenderControl( writer );
            _hfNumber.RenderControl( writer );

            string postBackScript = string.Empty;
            if ( this.NumberUpdated != null )
            {
                ScriptManager.GetCurrent( this.Page ).RegisterAsyncPostBackControl( this );
                postBackScript = this.Page.ClientScript.GetPostBackEventReference( new PostBackOptions( this, "NumberUpdated" ), true );
                postBackScript = postBackScript.Replace( '\'', '"' );
            }

            string disabledMinCss;
            if ( this.Enabled )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Onclick, string.Format( "Rock.controls.numberUpDown.adjust( this, -1, '{0}' );", postBackScript ) );
                disabledMinCss = Value <= Minimum ? "disabled " : "";
            }
            else
            {
                disabledMinCss = "disabled ";
            }

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "js-number-down numberincrement-down " + disabledMinCss );
            writer.RenderBeginTag( HtmlTextWriterTag.A );
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "fa fa-minus " );
            writer.RenderBeginTag( HtmlTextWriterTag.I );
            writer.RenderEndTag();
            writer.RenderEndTag();

            _lblNumber.CssClass = "js-number-up-down-lbl numberincrement-value ";
            _lblNumber.RenderControl( writer );

            string disabledMaxCss =  Value >= Maximum ? "disabled " : "";
            if ( this.Enabled )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Onclick, string.Format( "Rock.controls.numberUpDown.adjust( this, 1, '{0}' );", postBackScript ) );
                disabledMaxCss = Value >= Maximum ? "disabled " : "";
            }
            else
            {
                disabledMaxCss = "disabled ";
            }

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "js-number-up numberincrement-up " + disabledMaxCss );
            writer.RenderBeginTag( HtmlTextWriterTag.A );
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "fa fa-plus " );
            writer.RenderBeginTag( HtmlTextWriterTag.I );
            writer.RenderEndTag();
            writer.RenderEndTag();

            writer.RenderEndTag();  // Div.input-group
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when [number updated].
        /// </summary>
        public event EventHandler<EventArgs> NumberUpdated;

        /// <summary>
        /// When implemented by a class, enables a server control to process an event raised when a form is posted to the server.
        /// </summary>
        /// <param name="eventArgument">A <see cref="T:System.String" /> that represents an optional event argument to be passed to the event handler.</param>
        public void RaisePostBackEvent( string eventArgument )
        {
            if ( eventArgument == "NumberUpdated" && NumberUpdated != null )
            {
                NumberUpdated( this, new EventArgs() );
            }
        }

        #endregion
    }
}