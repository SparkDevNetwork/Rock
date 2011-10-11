using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Rock.Controls.Grid
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Column
    {
        private string headerTextValue;
        private string dataFieldValue;
        private string dataFormatStringValue;
        private int widthValue;
        private int minWidthValue;
        private bool sortableValue;
        private string classNameValue;

        public Column()
            : this( string.Empty, string.Empty, string.Empty, 0, 0, true, string.Empty)
        {
        }

        public Column( string headerText, string dataField, string dataFormatString,
            int width, int minWidth, bool sortable, string className )
        {
            headerTextValue = headerText;
            dataFieldValue = dataField;
            dataFormatStringValue = dataFormatString;
            widthValue = width;
            minWidthValue = minWidth;
            sortableValue = sortable;
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
        DefaultValue( false ),
        Description( "Can column be sorted" ),
        NotifyParentProperty( true ),
        ]
        public bool Sortable
        {
            get { return sortableValue; }
            set { sortableValue = value; }
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

        internal virtual string FormatCell( object dataItem, string identityColumn )
        {
            return DataBinder.GetPropertyValue( dataItem, DataField, DataFormatString );
        }

        internal string JsFriendlyClientId { get; set; }

        internal virtual void AddScriptFunctions( Page page )
        {
        }
    }
}