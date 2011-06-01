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

        internal override void AddScriptFunctions( Page page )
        {
            ClientScriptManager cs = page.ClientScript;
            Type baseType = base.GetType();

            if ( !cs.IsClientScriptBlockRegistered( baseType, "BoolCellFormatter" ) )
                cs.RegisterClientScriptBlock( baseType, "BoolCellFormatter", string.Format( @"
    function BoolCellFormatter(row, cell, value, columnDef, dataContext) {{
        return value ? ""<img src='{0}'>"" : """";
    }}
",
                    page.ResolveUrl( "~/Assets/Images/Grid/tick.png" ) ), true );

            if ( CanEdit )
            {
                if ( !cs.IsClientScriptBlockRegistered( baseType, "YesNoCheckboxCellEditor" ) )
                    cs.RegisterClientScriptBlock( baseType, "YesNoCheckboxCellEditor", @"
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
",
                         true );
            }
        }
    }
}