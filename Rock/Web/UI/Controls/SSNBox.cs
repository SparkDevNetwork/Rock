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

using Rock.Security;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control for entering a Social Security Number (SSN). Note: This control should only be used on a page that is using SSL as the SSN number is passed from client
    /// to server in a hidden field in plain text. If the SSN is being persisted, make sure to use the TextEncrypted property instead of the Text property so that an 
    /// encrypted version of the SSN number is stored.
    /// </summary>
    public class SSNBox : CompositeControl, IRockControl
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

        #endregion

        private HiddenFieldWithClass hfSSN;
        private TextBox ssnArea;
        private TextBox ssnGroup;
        private TextBox ssnSerial;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( System.EventArgs e )
        {
            base.OnInit( e );


            string script = @" 
    $('input.ssn-part').on( 'keydown', function (e) {
        if ($.inArray( e.keyCode, [46, 8, 9, 27, 13 ] ) !== -1 ||              // Allow: Ctrl/cmd+A
            ( e.keyCode == 65 && ( e.ctrlKey === true || e.metaKey === true ) ) ||      // Allow: Ctrl/cmd+C
            ( e.keyCode == 67 && ( e.ctrlKey === true || e.metaKey === true ) ) ||      // Allow: Ctrl/cmd+X
            ( e.keyCode == 88 && ( e.ctrlKey === true || e.metaKey === true ) ) ||      // Allow: home, end, left, right
            ( e.keyCode >= 35 && e.keyCode <= 39 )) {                                   // let it happen, don't do anything
                return;
        }
        // Ensure that it is a number and stop the keypress
        if ( ( e.shiftKey || ( e.keyCode < 48 || e.keyCode > 57 ) ) && ( e.keyCode < 96 || e.keyCode > 105 ) ) {
            e.preventDefault();
        }
    });

    $('input.ssn-part').on( 'keyup', function (e) {
        if ( ( e.keyCode >= 48 && e.keyCode <= 57 ) ||      // 0-9
            ( e.keyCode >= 96 && e.keyCode <= 105 ) ) {     // numpad keys
            if ( this.value.length >= this.maxLength ) {
                $(this).nextAll('.ssn-part:first').focus();
            }
        }
    });

    $('input.ssn-part').on( 'change', function(e) {   
        var $ctrlGroup = $(this).closest('.form-control-group');
        var ssnArea = $ctrlGroup.find('.ssn-area:first').val();
        var ssnGroup = $ctrlGroup.find('.ssn-group:first').val();
        var ssnSerial = $ctrlGroup.find('.ssn-serial:first').val();
        var $ssn = $ctrlGroup.find('.js-ssn:first')
        $ssn.val( ( ssnArea.length == 3 ? ssnArea + '-' : '' ) + ( ssnGroup.length == 2 ? ssnGroup + '-' : '' ) + ssnSerial );
    });
";
            ScriptManager.RegisterStartupScript( this, this.GetType(), "ssn-box", script, true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( Page.IsPostBack )
            {
                EnsureChildControls();
                ssnArea.Attributes["value"] = ssnArea.Text;
                ssnGroup.Attributes["value"] = ssnGroup.Text;
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Gets or sets the selected value.
        /// </summary>
        /// <value>
        /// The selected value.
        /// </value>
        public string Text
        {
            get
            {
                EnsureChildControls();
                return hfSSN.Value;
            }

            set
            {
                EnsureChildControls();
                hfSSN.Value = value;

                if ( !string.IsNullOrEmpty( value ) )
                {
                    string ssn = value.AsNumeric();
                    ssnArea.Attributes["value"] = ssn.SafeSubstring( 0, 3 );
                    ssnGroup.Attributes["value"] = ssn.SafeSubstring( 3, 2 );
                    ssnSerial.Text = ssn.SafeSubstring( 5, 4 );
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected value encrypted.
        /// </summary>
        /// <value>
        /// The selected value encrypted.
        /// </value>
        public string TextEncrypted
        {
            get
            {
                return Encryption.EncryptString( Text );
            }

            set
            {
                Text = Encryption.DecryptString( value );
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SSNBox"/> class.
        /// </summary>
        public SSNBox()
            : base()
        {
            RequiredFieldValidator = new HiddenFieldValidator();
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

            hfSSN = new HiddenFieldWithClass();
            hfSSN.ID = string.Format( "hfSSN_{0}", this.ID );
            hfSSN.CssClass = "js-ssn";

            this.RequiredFieldValidator.InitialValue = string.Empty;
            this.RequiredFieldValidator.ControlToValidate = hfSSN.ID;

            ssnArea =  new TextBox();
            ssnArea.CssClass = "form-control ssn-part ssn-area";
            ssnArea.ID = "ssnArea_" + this.ID;
            ssnArea.TextMode = TextBoxMode.Password;
            ssnArea.Attributes["pattern"] = "[0-9]*";
            ssnArea.MaxLength = 3;

            ssnGroup = new TextBox();
            ssnGroup.CssClass = "form-control ssn-part ssn-group";
            ssnGroup.ID = "ssnGroup_" + this.ID;
            ssnGroup.TextMode = TextBoxMode.Password;
            ssnGroup.Attributes["pattern"] = "[0-9]*";
            ssnGroup.MaxLength = 2;

            ssnSerial = new TextBox();
            ssnSerial.CssClass = "form-control ssn-part ssn-serial";
            ssnSerial.ID = "ssnSerial_" + this.ID;
            ssnGroup.Attributes["pattern"] = "[0-9]*";
            ssnSerial.MaxLength = 4;

            Controls.Add( hfSSN );
            Controls.Add( ssnArea );
            Controls.Add( ssnGroup );
            Controls.Add( ssnSerial );
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
            writer.AddAttribute( "class", "form-control-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            string separatorHtml = " <span class='separator'>-</span> ";

            hfSSN.RenderControl( writer );
            ssnArea.RenderControl( writer );
            writer.Write( separatorHtml );
            ssnGroup.RenderControl( writer );
            writer.Write( separatorHtml );
            ssnSerial.RenderControl( writer );

            writer.RenderEndTag();
        }

    }
}