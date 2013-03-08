//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class KeyValueList : HiddenField
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string script = @"
        function updateKeyValues( e ) {{
            var $span = e.closest('span.key-value-list');
            var newValue = '';
            $span.children('span.key-value-rows:first').children('div.controls-row').each(function( index ) {
                newValue += $(this).children('input.key-value-key:first').val() + '^' +
                    $(this).children('input.key-value-value:first').val() + '|'
            });
            $span.children('input:first').val(newValue);            
        }}

        $('a.key-value-add').click(function (e) {{
            e.preventDefault();
            var newKeyValue = '<div class=""controls controls-row""><input class=""span3 key-value-key"" type=""text"" placeholder=""Key""></input><input class=""span4 key-value-value"" type=""text"" placeholder=""Value""></input><a href=""#"" class=""btn key-value-remove""><i class=""icon-minus-sign""></i></a></div>';
            $(this).prev().append(newKeyValue);
        }});

        $('a.key-value-remove').live('click', function (e) {{
            e.preventDefault();
            var $rows = $(this).closest('span.key-value-rows');
            $(this).closest('div.controls-row').remove();
            updateKeyValues($rows);            
        }});

        $('span.key-value-rows > div.controls-row > input').live('blur', function (e) {{
            updateKeyValues($(this));            
        }});
";
            ScriptManager.RegisterStartupScript( this, this.GetType(), "key-value-list", script, true );
        }

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

            string[] nameValues = this.Value.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
            foreach ( string nameValue in nameValues )
            {
                string[] nameAndValue = nameValue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                if ( nameAndValue.Length == 2 )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "controls controls-row" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    writer.WriteLine();

                    // Write Name
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "span3 key-value-key" );
                    writer.AddAttribute( HtmlTextWriterAttribute.Type, "text" );
                    writer.AddAttribute( HtmlTextWriterAttribute.Value, nameAndValue[0] );
                    writer.AddAttribute( "placeholder", "Key" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Input );
                    writer.RenderEndTag();
                    writer.WriteLine();

                    // Write Value
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "span4 key-value-value" );
                    writer.AddAttribute( HtmlTextWriterAttribute.Type, "text" );
                    writer.AddAttribute( "placeholder", "Value" );
                    writer.AddAttribute( HtmlTextWriterAttribute.Value, nameAndValue[1] );
                    writer.RenderBeginTag( HtmlTextWriterTag.Input );
                    writer.RenderEndTag();
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
        }

    }
}