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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    ///
    /// </summary>
    public class ValueList : CompositeControl, IRockControl
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
        public string ValidationGroup { get; set; }

        #endregion

        #region Controls

        /// <summary>
        /// The _HF value
        /// </summary>
        public HiddenField _hfValue;

        /// <summary>
        /// The hf disable VRM
        /// </summary>
        public HiddenField _hfValueDisableVrm;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the value prompt.
        /// </summary>
        /// <value>
        /// The value prompt.
        /// </value>
        public string ValuePrompt
        {
            get { return ViewState["ValuePrompt"] as string ?? "Value"; }
            set { ViewState["ValuePrompt"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to allow HTML content in the value
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow HTML value]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowHtmlValue
        {
            get
            {
                EnsureChildControls();
                return _hfValueDisableVrm.Value.AsBoolean();
            }
            set
            {
                EnsureChildControls();
                _hfValueDisableVrm.Value = value.ToTrueFalse();
            }
        }

        /// <summary>
        /// Gets or sets custom values.  If custom values are used, the value control will
        /// render as a DropDownList with the selected values.
        /// </summary>
        /// <value>
        /// The custom values.
        /// </value>
        public Dictionary<string, string> CustomValues
        {
            get { return ViewState["CustomValues"] as Dictionary<string, string>; }
            set { ViewState["CustomValues"] = value; }
        }

        /// <summary>
        /// Gets or sets the defined type id.  If a defined type id is used, the value control
        /// will render as a DropDownList of values from that defined type.  If a DefinedTypeId is not specified
        /// the values will be rendered as free-form text fields.
        /// </summary>
        /// <value>
        /// The defined type id.
        /// </value>
        public int? DefinedTypeId
        {
            get { return ViewState["DefinedTypeId"] as int?; }
            set { ViewState["DefinedTypeId"] = value; }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value
        {
            get
            {
                EnsureChildControls();
                return _hfValue.Value;
            }
            set
            {
                EnsureChildControls();
                _hfValue.Value = value;

            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueList"/> class.
        /// </summary>
        public ValueList() : base()
        {
            this.HelpBlock = new HelpBlock();
            this.WarningBlock = new WarningBlock();
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            _hfValue = new HiddenField();
            _hfValue.ID = this.ID + "_hfValue";
            Controls.Add( _hfValue );

            _hfValueDisableVrm = new HiddenField();
            _hfValueDisableVrm.ID = _hfValue.ID + "_dvrm";
            Controls.Add( _hfValueDisableVrm );
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
        public virtual void RenderBaseControl( HtmlTextWriter writer )
        {
            Dictionary<string, string> definedValues = null;
            if ( DefinedTypeId.HasValue )
            {
                definedValues = new Dictionary<string, string>();
                new DefinedValueService( new RockContext() )
                    .GetByDefinedTypeId( DefinedTypeId.Value )
                    .ToList()
                    .ForEach( v => definedValues.Add( v.Id.ToString(), v.Value ) );
            }
            else if ( CustomValues != null )
            {
                definedValues = CustomValues;
            }

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "value-list" );
            writer.AddAttribute( HtmlTextWriterAttribute.Id, this.ClientID );
            writer.RenderBeginTag( HtmlTextWriterTag.Span );
            writer.WriteLine();

            _hfValue.RenderControl( writer );
            _hfValueDisableVrm.RenderControl( writer );

            writer.WriteLine();

            StringBuilder valueHtml = new StringBuilder();
            valueHtml.Append( @"<div class=""controls controls-row form-control-group"">");
            if ( definedValues != null && definedValues.Any() )
            {
                valueHtml.Append( @"<select class=""form-control input-width-lg js-value-list-input"">" );
                foreach ( var definedValue in definedValues )
                {
                    valueHtml.AppendFormat( @"<option value=""{0}"">{1}</option>", definedValue.Key, definedValue.Value );
                }
                valueHtml.Append( @"</select>" );
            }
            else
            {
                valueHtml.AppendFormat( @"<input class=""form-control input-width-lg js-value-list-input"" type=""text"" placeholder=""{0}""></input>", ValuePrompt );
            }
            valueHtml.Append( @"<a href=""#"" class=""btn btn-danger btn-square value-list-remove""><i class=""fa fa-times""></i></a></div>" );

            var hfValueHtml = new HtmlInputHidden();
            hfValueHtml.AddCssClass( "js-value-list-html" );
            hfValueHtml.Value = valueHtml.ToString();
            hfValueHtml.RenderControl( writer );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "value-list-rows" );
            writer.RenderBeginTag( HtmlTextWriterTag.Span );
            writer.WriteLine();


            var values = this.Value.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).AsEnumerable();
            values = values.Select( s => HttpUtility.UrlDecode( s ) );

            foreach ( string value in values )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "controls controls-row form-control-group" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.WriteLine();

                if ( definedValues != null && definedValues.Any() )
                {
                    DropDownList ddl = new DropDownList();
                    ddl.AddCssClass( "form-control input-width-lg js-value-list-input" );
                    ddl.DataTextField = "Value";
                    ddl.DataValueField = "Key";
                    ddl.DataSource = definedValues;
                    ddl.DataBind();
                    ddl.SelectedValue = value;
                    ddl.RenderControl( writer );
                }
                else
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-control input-width-lg js-value-list-input" );
                    writer.AddAttribute( HtmlTextWriterAttribute.Type, "text" );
                    writer.AddAttribute( "placeholder", ValuePrompt );
                    writer.AddAttribute( HtmlTextWriterAttribute.Value, value );
                    writer.RenderBeginTag( HtmlTextWriterTag.Input );
                    writer.RenderEndTag();
                }

                writer.Write( " " );
                writer.WriteLine();

                // Write Remove Button
                writer.AddAttribute( HtmlTextWriterAttribute.Href, "#" );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "btn btn-sm btn-danger value-list-remove" );
                writer.RenderBeginTag( HtmlTextWriterTag.A );
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "fa fa-times" );
                writer.RenderBeginTag( HtmlTextWriterTag.I );
                writer.RenderEndTag();
                writer.RenderEndTag();
                writer.WriteLine();

                writer.RenderEndTag();
                writer.WriteLine();

            }

            writer.RenderEndTag();
            writer.WriteLine();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "actions" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            var addButtonCssClass = "btn btn-action btn-xs btn-square value-list-add";
            if ( !this.Enabled )
            {
                addButtonCssClass += " aspNetDisabled disabled";
            }

            writer.AddAttribute( HtmlTextWriterAttribute.Class, addButtonCssClass );

            writer.AddAttribute( HtmlTextWriterAttribute.Href, "#" );
            writer.RenderBeginTag( HtmlTextWriterTag.A );
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "fa fa-plus-circle");
            writer.RenderBeginTag( HtmlTextWriterTag.I );

            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.WriteLine();

            writer.RenderEndTag();
            writer.WriteLine();
        }
    }
}