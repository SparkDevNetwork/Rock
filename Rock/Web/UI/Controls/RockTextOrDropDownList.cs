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
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A <see cref="T:System.Web.UI.WebControls.RockTextOrDropDownList"/> control with an associated label.
    /// </summary>
    [ToolboxData( "<{0}:RockTextOrDropDownList runat=server></{0}:RockTextOrDropDownList>" )]
    public class RockTextOrDropDownList : CompositeControl, IRockControl
    {
        #region Controls

        private HiddenField _hfDisableVrm;
        private HiddenField _hiddenField;

        /// <summary>
        /// The _text box
        /// </summary>
        public RockTextBox _textBox;

        /// <summary>
        /// The _drop down list
        /// </summary>
        public RockDropDownList _dropDownList;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the number of rows that textbox should use
        /// </summary>
        /// <value>
        /// The number of rows.
        /// </value>
        public int Rows
        {
            get { return ViewState["Rows"] as int? ?? 1; }
            set { ViewState["Rows"] = value; }
        }

        /// <summary>
        /// Gets the text box.
        /// </summary>
        /// <value>
        /// The text box.
        /// </value>
        public RockTextBox TextBox
        {
            get
            {
                EnsureChildControls();
                return _textBox;
            }
        }

        /// <summary>
        /// Gets the drop down list.
        /// </summary>
        /// <value>
        /// The drop down list.
        /// </value>
        public RockDropDownList DropDownList
        {
            get
            {
                EnsureChildControls();
                return _dropDownList;
            }
        }

        /// <summary>
        /// Gets or sets the selected value.
        /// </summary>
        /// <value>
        /// The selected value.
        /// </value>
        public string SelectedValue
        {
            get
            {
                EnsureChildControls();
                return _hiddenField.Value;
            }
            set
            {
                EnsureChildControls();
                _hiddenField.Value = value;
            }
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetValue( string value)
        {
            this.SelectedValue = value;
        }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label text
        /// </value>
        public string Label
        {
            get
            {
                EnsureChildControls();
                return _textBox.Label;
            }
            set
            {
                EnsureChildControls();
                var labels = value.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
                _textBox.Label = labels.Length > 0 ? labels[0] : "&nbsp;";
                _dropDownList.Label = labels.Length > 1 ? labels[1] : "&nbsp;";
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
            get { return ViewState["FormGroupCssClass"] as string ?? string.Empty; }
            set { ViewState["FormGroupCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        public string Help
        {
            get
            {
                EnsureChildControls();
                return _textBox.Help;
            }
            set
            {
                EnsureChildControls();
                _textBox.Help = value;
                _dropDownList.Help = string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the warning text.
        /// </summary>
        /// <value>
        /// The warning text.
        /// </value>
        public string Warning
        {
            get
            {
                EnsureChildControls();
                return _textBox.Warning;
            }
            set
            {
                EnsureChildControls();
                _textBox.Warning = value;
                _dropDownList.Warning = string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IRockControl" /> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        public bool Required { get; set; }

        /// <summary>
        /// Gets or sets the required error message.  If blank, the LabelName name will be used
        /// </summary>
        /// <value>
        /// The required error message.
        /// </value>
        public string RequiredErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid
        {
            get { return true; }
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
                return _textBox.ValidationGroup;
            }
            set
            {
                EnsureChildControls();
                _textBox.ValidationGroup = value;
                _dropDownList.ValidationGroup = value;
            }
        }

        /// <summary>
        /// Gets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        public HelpBlock HelpBlock
        {
            get
            {
                EnsureChildControls();
                return _textBox.HelpBlock;
            }
            set
            {
                EnsureChildControls();
                _textBox.HelpBlock = value;
            }
        }

        /// <summary>
        /// Gets the warning block.
        /// </summary>
        /// <value>
        /// The warning block.
        /// </value>
        public WarningBlock WarningBlock
        {
            get
            {
                EnsureChildControls();
                return _textBox.WarningBlock;
            }
            set
            {
                EnsureChildControls();
                _textBox.WarningBlock = value;
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
        /// Gets or sets a value that indicates whether the control checks client input from the browser for potentially dangerous values.
        /// </summary>
        public override ValidateRequestMode ValidateRequestMode
        {
            get
            {
                return base.ValidateRequestMode;
            }
            set
            {
                base.ValidateRequestMode = value;

                EnsureChildControls();
                _textBox.ValidateRequestMode = value;
            }
        }

        #endregion

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            _hiddenField = new HiddenField();
            _hiddenField.ID = this.ID + "_hiddenField";
            Controls.Add( _hiddenField );

            _hfDisableVrm = new HiddenField();
            _hfDisableVrm.ID = this.ID + "_hiddenField_dvrm";
            _hfDisableVrm.Value = "True";
            Controls.Add( _hfDisableVrm ); 
            
            _textBox = new RockTextBox();
            _textBox.ID = this.ID + "_textBox";
            Controls.Add( _textBox );

            _dropDownList = new RockDropDownList();
            _dropDownList.ID = this.ID + "_dropDownList";
            Controls.Add( _dropDownList );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                RenderBaseControl( writer );
            }
        }

        /// <summary>
        /// This is where you implement the simple aspects of rendering your control.  The rest
        /// will be handled by calling RenderControlHelper's RenderControl() method.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            if ( Rows > 1 )
            {
                _textBox.TextMode = TextBoxMode.MultiLine;
                _textBox.Rows = Rows;
            }
            else
            {
                _textBox.TextMode = TextBoxMode.SingleLine;
            }

            bool ddlItemSelected = false;
            foreach ( ListItem li in _dropDownList.Items )
            {
                if ( li.Value == _hiddenField.Value )
                {
                    li.Selected = true;
                    ddlItemSelected = true;
                }
                else
                {
                    li.Selected = false;
                }
            }

            if ( !ddlItemSelected )
            {
                _textBox.Text = _hiddenField.Value;
            }

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row js-text-or-ddl-row " + this.CssClass );
            writer.AddAttribute( HtmlTextWriterAttribute.Style, this.Style.Value );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            _hiddenField.RenderControl( writer );
            if ( ValidateRequestMode == System.Web.UI.ValidateRequestMode.Disabled )
            {
                _hfDisableVrm.RenderControl( writer );
            }

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-xs-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _textBox.AddCssClass( "js-text-or-ddl-input" );
            _textBox.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-xs-1" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            var lOr = new RockLiteral();
            lOr.Label = "&nbsp;";
            lOr.Text = "or";
            lOr.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-xs-5" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _dropDownList.AddCssClass( "js-text-or-ddl-input" );
            _dropDownList.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();  // row

            RegisterClientScript();
        }

        private void RegisterClientScript()
        {
            string script = @"
    function updateTextOrDdlValue( e ) {
        var $row = e.closest('div.js-text-or-ddl-row');
        var newValue = $row.find('input.js-text-or-ddl-input:first').val();
        if (!newValue || newValue == '' ) {
            newValue = $row.find('textarea.js-text-or-ddl-input:first').val();
        }
        if (!newValue || newValue == '' ) {
            newValue = $row.find('select.js-text-or-ddl-input:first').val();
        } 
        $row.find('input:first').val(newValue);
    }

    $(document).on('focusout', '.js-text-or-ddl-input', function (e) {
        updateTextOrDdlValue($(this));            
    });
";
            ScriptManager.RegisterStartupScript( this, this.GetType(), "text-or-ddl", script, true );
        }

    }
}