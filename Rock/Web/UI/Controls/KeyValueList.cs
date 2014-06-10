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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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
    public class KeyValueList : CompositeControl, IRockControl
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

        /// <summary>
        /// Gets or sets the group of controls for which the <see cref="T:System.Web.UI.WebControls.TextBox" /> control causes validation when it posts back to the server.
        /// </summary>
        /// <returns>The group of controls for which the <see cref="T:System.Web.UI.WebControls.TextBox" /> control causes validation when it posts back to the server. The default value is an empty string ("").</returns>
        public string ValidationGroup { get; set; }

        #endregion

        #region Controls

        HiddenField _hfValue;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the key prompt.
        /// </summary>
        /// <value>
        /// The key prompt.
        /// </value>
        public string KeyPrompt
        {
            get { return ViewState["KeyPrompt"] as string ?? "Key"; }
            set { ViewState["KeyPrompt"] = value; }
        }

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
        /// Gets or sets custom values.  If custom values are used, the value portion of this control will
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
        /// Gets or sets the defined type id.  If a defined type id is used, the value portion of this control
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
        /// Initializes a new instance of the <see cref="KeyValueList"/> class.
        /// </summary>
        public KeyValueList() : base()
        {
            this.HelpBlock = new HelpBlock();
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
            Dictionary<string, string> values = null;
            if ( DefinedTypeId.HasValue )
            {
                values = new Dictionary<string, string>();
                new DefinedValueService( new RockContext() )
                    .GetByDefinedTypeId( DefinedTypeId.Value )
                    .ToList()
                    .ForEach( v => values.Add( v.Id.ToString(), v.Name ) );
            } 
            else if ( CustomValues != null )
            {
                values = CustomValues;
            }

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "key-value-list" );
            writer.RenderBeginTag( HtmlTextWriterTag.Span );
            writer.WriteLine();

            _hfValue.RenderControl( writer );
            writer.WriteLine();

            StringBuilder valueHtml = new StringBuilder();
            valueHtml.AppendFormat( @"<div class=""controls controls-row form-control-group""><input class=""key-value-key form-control input-width-md js-key-value-input"" type=""text"" placeholder=""{0}""></input> ", KeyPrompt);
            if ( values != null )
            {
                valueHtml.Append( @"<select class=""key-value-value form-control input-width-lg js-key-value-input""><option value=""""></option>" );
                foreach ( var value in values )
                {
                    valueHtml.AppendFormat( @"<option value=""{0}"">{1}</option>", value.Key, value.Value );
                }
                valueHtml.Append( @"</select>" );
            }
            else
            {
                valueHtml.AppendFormat( @"<input class=""key-value-value input-width-lg form-control js-key-value-input"" type=""text"" placeholder=""{0}""></input>", ValuePrompt );
            }
            valueHtml.Append( @"<a href=""#"" class=""btn btn-sm btn-danger key-value-remove""><i class=""fa fa-minus-circle""></i></a></div>" );

            var hfValueHtml = new HtmlInputHidden();
            hfValueHtml.AddCssClass( "js-value-html" );
            hfValueHtml.Value = valueHtml.ToString();
            hfValueHtml.RenderControl( writer );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "key-value-rows" );
            writer.RenderBeginTag( HtmlTextWriterTag.Span );
            writer.WriteLine();


            string[] nameValues = this.Value.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
            foreach ( string nameValue in nameValues )
            {
                string[] nameAndValue = nameValue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "controls controls-row form-control-group" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.WriteLine();

                // Write Name
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "key-value-key form-control input-width-md js-key-value-input" );
                writer.AddAttribute( HtmlTextWriterAttribute.Type, "text" );
                writer.AddAttribute( HtmlTextWriterAttribute.Value, nameAndValue.Length >= 1 ? nameAndValue[0] : string.Empty );
                writer.AddAttribute( "placeholder", KeyPrompt );
                writer.RenderBeginTag( HtmlTextWriterTag.Input );
                writer.RenderEndTag();
                writer.Write( " " );
                writer.WriteLine();

                // Write Value
                if ( values != null )
                {
                    DropDownList ddl = new DropDownList();
                    ddl.AddCssClass( "key-value-value form-control input-width-lg js-key-value-input" );
                    ddl.DataTextField = "Value";
                    ddl.DataValueField = "Key";
                    ddl.DataSource = values;
                    ddl.DataBind();
                    ddl.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );
                    if ( nameAndValue.Length >= 2 )
                    {
                        ddl.SelectedValue = nameAndValue[1];
                    }
                    ddl.RenderControl( writer );
                }
                else
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "key-value-value form-control input-width-lg js-key-value-input" );
                    writer.AddAttribute( HtmlTextWriterAttribute.Type, "text" );
                    writer.AddAttribute( "placeholder", ValuePrompt );
                    writer.AddAttribute( HtmlTextWriterAttribute.Value, nameAndValue.Length >= 2 ? nameAndValue[1] : string.Empty );
                    writer.RenderBeginTag( HtmlTextWriterTag.Input );
                    writer.RenderEndTag();
                }

                writer.Write( " " );
                writer.WriteLine();

                // Write Remove Button
                writer.AddAttribute( HtmlTextWriterAttribute.Href, "#" );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "btn btn-sm btn-danger key-value-remove" );
                writer.RenderBeginTag( HtmlTextWriterTag.A );
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "fa fa-minus-circle");
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

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "btn btn-action btn-xs key-value-add" );
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

            RegisterClientScript();
        }

        private void RegisterClientScript()
        {
            string script = @"
    function updateKeyValues( e ) {
        var $span = e.closest('span.key-value-list');
        var newValue = '';
        $span.children('span.key-value-rows:first').children('div.controls-row').each(function( index ) {
            newValue += $(this).children('.key-value-key:first').val() + '^' + $(this).children('.key-value-value:first').val() + '|'
        });
        $span.children('input:first').val(newValue);            
    }

    $('a.key-value-add').click(function (e) {
        e.preventDefault();
        var $keyValueList = $(this).closest('.key-value-list');
        $keyValueList.find('.key-value-rows').append($keyValueList.find('.js-value-html').val());
    });

    $(document).on('click', 'a.key-value-remove', function (e) {
        e.preventDefault();
        var $rows = $(this).closest('span.key-value-rows');
        $(this).closest('div.controls-row').remove();
        updateKeyValues($rows);            
    });

    $(document).on('focusout', '.js-key-value-input', function (e) {
        updateKeyValues($(this));            
    });
";

            ScriptManager.RegisterStartupScript( this, this.GetType(), "key-value-list", script, true );
        }

    }
}