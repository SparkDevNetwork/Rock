//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class KeyValueList : HiddenField
    {

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
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "key-value-list" );
            writer.RenderBeginTag( HtmlTextWriterTag.Span );
            writer.WriteLine();

            base.RenderControl( writer );
            writer.WriteBreak();
            writer.WriteLine();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "key-value-rows" );
            writer.RenderBeginTag( HtmlTextWriterTag.Span );
            writer.WriteLine();

            List<DefinedValue> values = null;
            if ( DefinedTypeId.HasValue )
            {
                values = new DefinedValueService().GetByDefinedTypeId( DefinedTypeId.Value ).ToList();
            }

            string[] nameValues = this.Value.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
            foreach ( string nameValue in nameValues )
            {
                string[] nameAndValue = nameValue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "controls controls-row" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.WriteLine();

                // Write Name
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "span3 key-value-key" );
                writer.AddAttribute( HtmlTextWriterAttribute.Type, "text" );
                writer.AddAttribute( HtmlTextWriterAttribute.Value, nameAndValue.Length >= 1 ? nameAndValue[0] : string.Empty );
                writer.AddAttribute( "placeholder", "Key" );
                writer.RenderBeginTag( HtmlTextWriterTag.Input );
                writer.RenderEndTag();
                writer.Write( " " );
                writer.WriteLine();

                // Write Value
                if ( values == null )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "span4 key-value-value" );
                    writer.AddAttribute( HtmlTextWriterAttribute.Type, "text" );
                    writer.AddAttribute( "placeholder", "Value" );
                    writer.AddAttribute( HtmlTextWriterAttribute.Value, nameAndValue.Length >= 2 ? nameAndValue[1] : string.Empty );
                    writer.RenderBeginTag( HtmlTextWriterTag.Input );
                    writer.RenderEndTag();
                }
                else
                {
                    DropDownList ddl = new DropDownList();
                    ddl.AddCssClass( "key-value-value" );
                    ddl.DataTextField = "Name";
                    ddl.DataValueField = "Id";
                    ddl.DataSource = values;
                    ddl.DataBind();
                    ddl.Items.Insert(0, new ListItem( string.Empty, string.Empty ) );
                    if ( nameAndValue.Length >= 2 )
                    {
                        ddl.SelectedValue = nameAndValue[1];
                    }
                    ddl.RenderControl( writer );
                }

                writer.Write( " " );
                writer.WriteLine();

                // Write Remove Button
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "btn key-value-remove" );
                writer.AddAttribute( HtmlTextWriterAttribute.Href, "#" );
                writer.RenderBeginTag( HtmlTextWriterTag.A );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "icon-minus-sign" );
                writer.RenderBeginTag( HtmlTextWriterTag.I );
                writer.RenderEndTag();
                writer.RenderEndTag();
                writer.WriteLine();

                writer.RenderEndTag();
                writer.WriteLine();

            }

            writer.RenderEndTag();
            writer.WriteLine();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "btn key-value-add" );
            writer.AddAttribute( HtmlTextWriterAttribute.Href, "#" );
            writer.RenderBeginTag( HtmlTextWriterTag.A );
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "icon-plus-sign" );
            writer.RenderBeginTag( HtmlTextWriterTag.I );
            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.WriteLine();

            writer.RenderEndTag();
            writer.WriteLine();

            RegisterClientScript( values );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="values">The values.</param>
        private void RegisterClientScript( List<DefinedValue> values )
        {
            StringBuilder script = new StringBuilder();
            script.Append( @"
        function updateKeyValues( e ) {
            var $span = e.closest('span.key-value-list');
            var newValue = '';
            $span.children('span.key-value-rows:first').children('div.controls-row').each(function( index ) {
                newValue += $(this).children('.key-value-key:first').val() + '^' + $(this).children('.key-value-value:first').val() + '|'
            });
            $span.children('input:first').val(newValue);            
        }

        $('a.key-value-add').click(function (e) {{
            e.preventDefault();
            var newKeyValue = '<div class=""controls controls-row""><input class=""span3 key-value-key"" type=""text"" placeholder=""Key""></input> " );
            if ( values != null )
            {
                script.Append( @"<select class=""key-value-value""><option value=""""></option>" );

                foreach ( var value in values )
                {
                    script.AppendFormat( @"<option value=""{0}"">{1}</option>", value.Id, value.Name );
                }

                script.Append( @"</select>" );
            }
            else
            {
                script.Append( @"<input class=""span4 key-value-value"" type=""text"" placeholder=""Value""></input>" );
            }

            script.Append( @" <a href=""#"" class=""btn key-value-remove""><i class=""icon-minus-sign""></i></a></div>';
            $(this).prev().append(newKeyValue);
        }});

        $('a.key-value-remove').on('click', function (e) {{
            e.preventDefault();
            var $rows = $(this).closest('span.key-value-rows');
            $(this).closest('div.controls-row').remove();
            updateKeyValues($rows);            
        }});
" );

            if ( values != null )
            {
                script.Append( @"
        $('span.key-value-rows > div.controls-row > select').on('click', function (e) {{
            updateKeyValues($(this));            
        }});
" );
            }
            else
            {
                script.Append( @"
        $('span.key-value-rows > div.controls-row > input').on('blur', function (e) {{
            updateKeyValues($(this));            
        }});
" );
            }

            ScriptManager.RegisterStartupScript( this, this.GetType(), "key-value-list", script.ToString(), true );
        }

    }
}