using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Rock.Controls
{
    public class GridIntegerColumn : GridColumn
    {
        internal override string Editor
        {
            get
            {
                return "IntegerCellEditor";
            }
        }

        internal override string RowParameter( object dataItem, string keyName )
        {
            return string.Format( "{0}:{1}", keyName, DataBinder.GetPropertyValue( dataItem, DataField, null ).ToLower() );
        }

        internal override void AddScriptFunctions( Page page )
        {
            if ( CanEdit )
            {
                ClientScriptManager cs = page.ClientScript;
                Type baseType = base.GetType();

                if ( !cs.IsClientScriptBlockRegistered( baseType, "IntegerCellEditor" ) )
                    cs.RegisterClientScriptBlock( baseType, "IntegerCellEditor", @"
    function IntegerCellEditor(args) {
        var $input;
        var defaultValue;
        var scope = this;

        this.init = function() {
            $input = $(""<INPUT type=text class='editor-text' />"");

            $input.bind(""keydown.nav"", function(e) {
                if (e.keyCode === $.ui.keyCode.LEFT || e.keyCode === $.ui.keyCode.RIGHT) {
                    e.stopImmediatePropagation();
                }
            });

            $input.appendTo(args.container);
            $input.focus().select();
        };

        this.destroy = function() {
            $input.remove();
        };

        this.focus = function() {
            $input.focus();
        };

        this.loadValue = function(item) {
            defaultValue = item[args.column.field];
            $input.val(defaultValue);
            $input[0].defaultValue = defaultValue;
            $input.select();
        };

        this.serializeValue = function() {
            return parseInt($input.val(),10) || 0;
        };

        this.applyValue = function(item,state) {
            item[args.column.field] = state;
        };

        this.isValueChanged = function() {
            return (!($input.val() == """" && defaultValue == null)) && ($input.val() != defaultValue);
        };

        this.validate = function() {
            if (isNaN($input.val()))
                return {
                    valid: false,
                    msg: ""Please enter a valid integer""
                };

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