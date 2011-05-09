using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Rock.Controls
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class GridColumn
    {
        private string headerTextValue;
        private string dataFieldValue;
        private string dataFormatStringValue;
        private int widthValue;
        private int minWidthValue;
        private bool canEdit;
        private bool sortable;
        private string classNameValue;

        public GridColumn()
            : this( string.Empty, string.Empty, string.Empty, 0, 0, string.Empty )
        {
        }

        public GridColumn( string headerText, string dataField, string dataFormatString,
            int width, int minWidth, string className )
        {
            headerTextValue = headerText;
            dataFieldValue = dataField;
            dataFormatStringValue = dataFormatString;
            widthValue = width;
            minWidthValue = minWidth;
            classNameValue = className;
        }

        [
        Category( "Behavior" ),
        DefaultValue( "" ),
        Description( "Column Header Text" ),
        NotifyParentProperty( true ),
        ]
        public String HeaderText
        {
            get { return headerTextValue; }
            set { headerTextValue = value; }
        }

        [
        Category( "Behavior" ),
        DefaultValue( "" ),
        Description( "Data Field to use in datasource" ),
        NotifyParentProperty( true ),
        ]
        public String DataField
        {
            get { return dataFieldValue; }
            set { dataFieldValue = value; }
        }

        [
        Category( "Behavior" ),
        DefaultValue( "" ),
        Description( "Format string to apply to the datafield" ),
        NotifyParentProperty( true ),
        ]
        public String DataFormatString
        {
            get { return dataFormatStringValue; }
            set { dataFormatStringValue = value; }
        }

        [
        Category( "Behavior" ),
        DefaultValue( 0 ),
        Description( "Column width" ),
        NotifyParentProperty( true ),
        ]
        public int Width
        {
            get { return widthValue; }
            set { widthValue = value; }
        }

        [
        Category( "Behavior" ),
        DefaultValue( 0 ),
        Description( "Minimum column width" ),
        NotifyParentProperty( true ),
        ]
        public int MinWidth
        {
            get { return minWidthValue; }
            set { minWidthValue = value; }
        }

        [
        Category( "Behavior" ),
        DefaultValue( 0 ),
        Description( "Can values be edited" ),
        NotifyParentProperty( true ),
        ]
        public bool CanEdit
        {
            get { return canEdit; }
            set { canEdit = value; }
        }

        [
        Category( "Behavior" ),
        DefaultValue( 0 ),
        Description( "Can column be sorted" ),
        NotifyParentProperty( true ),
        ]
        public bool Sortable
        {
            get { return sortable; }
            set { sortable = value; }
        }

        [
        Category( "Behavior" ),
        DefaultValue( "" ),
        Description( "CSS class name to associate with column values" ),
        NotifyParentProperty( true ),
        ]
        public string ClassName
        {
            get { return classNameValue; }
            set { classNameValue = value; }
        }

        internal virtual List<string> ColumnParameters
        {
            get
            {
                List<string> columnParameters = new List<string>();

                columnParameters.Add( string.Format( "id:\"{0}\"", DataField ) );
                columnParameters.Add( string.Format( "field:\"{0}\"", DataField ) );
                columnParameters.Add( string.Format( "name:\"{0}\"", HeaderText ) );
                if ( Width > 0 )
                    columnParameters.Add( string.Format( "width:{0}", Width.ToString() ) );
                if ( MinWidth > 0 )
                    columnParameters.Add( string.Format( "minWidth:{0}", MinWidth.ToString() ) );
                if ( CanEdit )
                    columnParameters.Add( string.Format( "editor:{0}", Editor ) );
                if ( Formatter != string.Empty )
                    columnParameters.Add( string.Format( "formatter:{0}", Formatter ) );
                if ( Sortable )
                    columnParameters.Add( "sortable:true" );

                return columnParameters;
            }

        }

        internal virtual string Formatter
        {
            get { return string.Empty; }
        }

        internal virtual string Editor
        {
            get { return "TextCellEditor"; }
        }

        internal virtual string RowParameter ( object dataItem )
        {
            return string.Format( "{0}:\"{1}\"", DataField, DataBinder.GetPropertyValue( dataItem, DataField, null ) );
        }

        internal virtual void AddScriptFunctions( Dictionary<string, string> functions, Page page )
        {
            if (!functions.ContainsKey("TextCellEditor"))
                functions.Add( "TextCellEditor", @"
    function TextCellEditor(args) {

        var $input;
        var defaultValue;
        var scope = this;

        this.init = function() {
            $input = $(""<INPUT type=text class='editor-text' />"")
                .appendTo(args.container)
                .bind(""keydown.nav"", function(e) {
                    if (e.keyCode === $.ui.keyCode.LEFT || e.keyCode === $.ui.keyCode.RIGHT) {
                        e.stopImmediatePropagation();
                    }
                })
                .focus()
                .select();
        };

        this.destroy = function() {
            $input.remove();
        };

        this.focus = function() {
            $input.focus();
        };

        this.getValue = function() {
            return $input.val();
        };

        this.setValue = function(val) {
            $input.val(val);
        };

        this.loadValue = function(item) {
            defaultValue = item[args.column.field] || """";
            $input.val(defaultValue);
            $input[0].defaultValue = defaultValue;
            $input.select();
        };

        this.serializeValue = function() {
            return $input.val();
        };

        this.applyValue = function(item,state) {
            item[args.column.field] = state;
        };

        this.isValueChanged = function() {
            return (!($input.val() == """" && defaultValue == null)) && ($input.val() != defaultValue);
        };

        this.validate = function() {
            if (args.column.validator) {
                var validationResults = args.column.validator($input.val());
                if (!validationResults.valid)
                    return validationResults;
            }

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