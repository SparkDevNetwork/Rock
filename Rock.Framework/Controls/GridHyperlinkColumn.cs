using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;

namespace Rock.Controls
{
    public class GridHyperlinkColumn : GridColumn
    {
        private string dataNavigateUrlFieldValue;
        private string dataNavigateUrlFormatStringValue;

        public GridHyperlinkColumn()
            : base()
        {
        }

        public GridHyperlinkColumn( string headerText, string dataNavigateUrlField, string dataNavigateUrlFormatString,
            string dataField, string dataFormatString, int width, int minWidth, bool canEdit, bool sortable,
            bool uniqueIdentifier, string className, bool visible )
            : base( headerText, dataField, dataFormatString, width, minWidth, canEdit, sortable, uniqueIdentifier, 
            className, visible )
        {
            dataNavigateUrlFieldValue = dataNavigateUrlField;
            dataNavigateUrlFormatStringValue = dataNavigateUrlFormatString;
        }

        [
        Category( "Behavior" ),
        DefaultValue( "" ),
        Description( "Data Field to use in DataNavigateUrlFormatString" ),
        NotifyParentProperty( true ),
        ]
        public String DataNavigateUrlField
        {
            get { return dataNavigateUrlFieldValue; }
            set { dataNavigateUrlFieldValue = value; }
        }

        [
        Category( "Behavior" ),
        DefaultValue( "" ),
        Description( "Format string to apply to the hyperlink url" ),
        NotifyParentProperty( true ),
        ]
        public String DataNavigateUrlFormatString
        {
            get { return dataNavigateUrlFormatStringValue; }
            set { dataNavigateUrlFormatStringValue = value; }
        }

        internal override string Formatter
        {
            get
            {
                return "HyperlinkCellFormatter";
            }
        }

        internal override void AddScriptFunctions( Page page )
        {
            ClientScriptManager cs = page.ClientScript;
            Type baseType = base.GetType();

            string dataFormatString = DataFormatString == string.Empty ? "{0}" : DataFormatString;
            string dataNavigateUrlFormatString = page.ResolveUrl( DataNavigateUrlFormatString == string.Empty ? "{0}" : DataNavigateUrlFormatString );

            if ( !cs.IsClientScriptBlockRegistered( baseType, "HyperlinkCellFormatter" ) )
                cs.RegisterClientScriptBlock( baseType, "HyperlinkCellFormatter", string.Format( @"
    function HyperlinkCellFormatter(row, cell, value, columnDef, dataContext) {{

        var formattedValue = '{0}';
        formattedValue = formattedValue.replace('{{0}}', value);
        
        var hyperlinkValue = '{1}';
        hyperlinkValue = hyperlinkValue.replace('{{0}}', dataContext.{2});

        return ""<a href='"" + hyperlinkValue + ""'>"" + formattedValue + ""</a>"";

    }}
",
                    dataFormatString,
                    dataNavigateUrlFormatString,
                    DataNavigateUrlField), true );

        }
    }
}