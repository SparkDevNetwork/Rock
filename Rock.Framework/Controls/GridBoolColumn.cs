using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Rock.Controls
{
    public class GridBoolColumn : GridColumn
    {
        internal override string Formatter
        {
            get
            {
                return "BoolCellFormatter";
            }
        }

        internal override string Editor
        {
            get
            {
                return "YesNoCheckboxCellEditor";
            }
        }

        internal override string RowParameter( object dataItem )
        {
            return string.Format( "{0}:{1}", DataField, DataBinder.GetPropertyValue( dataItem, DataField, null ).ToLower() );
        }

        internal override void AddScriptFunctions( Dictionary<string, string> functions, Page page )
        {
            if ( !functions.ContainsKey( "BoolCellFormatter" ) )
                functions.Add( "BoolCellFormatter", string.Format( @"
    function BoolCellFormatter(row, cell, value, columnDef, dataContext) {{
        return value ? ""<img src='{0}'>"" : """";
    }}
",
                    page.ResolveUrl( "~/Assets/Icons/tick.png" ) ) );

            if ( !functions.ContainsKey( "YesNoCheckboxCellEditor" ) )
                functions.Add( "YesNoCheckboxCellEditor", @"
    function YesNoCheckboxCellEditor(args) {
        var $select;
        var defaultValue;
        var scope = this;

        this.init = function() {
            $select = $(""<INPUT type=checkbox value='true' class='editor-checkbox' hideFocus>"");
            $select.appendTo(args.container);
            $select.focus();
        };

        this.destroy = function() {
            $select.remove();
        };

        this.focus = function() {
            $select.focus();
        };

        this.loadValue = function(item) {
            defaultValue = item[args.column.field];
            if (defaultValue)
                $select.attr(""checked"", ""checked"");
            else
                $select.removeAttr(""checked"");
        };

        this.serializeValue = function() {
            return $select.attr(""checked"");
        };

        this.applyValue = function(item,state) {
            item[args.column.field] = state;
        };

        this.isValueChanged = function() {
            return ($select.attr(""checked"") != defaultValue);
        };

        this.validate = function() {
            return {
                valid: true,
                msg: null
            };
        };

        this.init();
    }
" );
        }
    }
}