using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;

namespace Rock.Controls.Grid
{
    public class HyperlinkColumn : Column
    {
        private string dataNavigateUrlFieldValue;
        private string dataNavigateUrlFormatStringValue;

        public HyperlinkColumn()
            : base()
        {
        }

        public HyperlinkColumn( string headerText, string dataNavigateUrlField, string dataNavigateUrlFormatString,
            string dataField, string dataFormatString, int width, int minWidth, bool sortable, string className )
            : base( headerText, dataField, dataFormatString, width, minWidth, sortable, className )
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

        internal override string FormatCell( object dataItem, string identityColumn )
        {
            return string.Format( "<a href=\"{0}\">{1}</a>",
                DataBinder.GetPropertyValue( dataItem, DataNavigateUrlField, DataNavigateUrlFormatString ),
                base.FormatCell( dataItem, identityColumn ) );
        }
    }
}