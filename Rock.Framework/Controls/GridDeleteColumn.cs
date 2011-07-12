using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;

namespace Rock.Controls
{
    public class GridDeleteColumn : GridColumn
    {
        public GridDeleteColumn()
            : base()
        {
        }

        public GridDeleteColumn( string headerText, string dataField)
            : base()
        {
            base.HeaderText = headerText;
            base.DataField = dataField;
        }

        internal override List<string> ColumnParameters
        {
            get
            {
                List<string> columnParameters = new List<string>();

                columnParameters.Add( "id:\"#\"" );
                columnParameters.Add( "width:40" );
                columnParameters.Add( "name:\"\"" );
                columnParameters.Add( string.Format( "field:\"{0}\"", DataField.Replace( ".", "" ) ) );
                columnParameters.Add( "resizable:false" );
                columnParameters.Add( "selectable:false" );
                columnParameters.Add( "cssClass:\"data-grid-cell-delete dnd\"" );
                if ( Formatter != string.Empty )
                    columnParameters.Add( string.Format( "formatter:{0}", Formatter ) );

                return columnParameters;
            }
        }

        internal override string RowParameter( object dataItem )
        {
            return string.Empty;
        }

        internal override string RowParameter( object dataItem, string keyName )
        {
            return string.Empty;
        }

        internal override string Formatter
        {
            get
            {
                return "DeleteCellFormatter";
            }
        }

        internal override void AddScriptFunctions( Page page )
        {
            ClientScriptManager cs = page.ClientScript;
            Type baseType = base.GetType();

            if ( !cs.IsClientScriptBlockRegistered( baseType, "DeleteCellFormatter" ) )
                cs.RegisterClientScriptBlock( baseType, "DeleteCellFormatter", string.Format( @"
    function DeleteCellFormatter(row, cell, value, columnDef, dataContext) {{
        return ""<a href='#' onclick='{0}_DeleteRow("" + row + "","" + value + "")'></a>"";
    }}
",
                    JsFriendlyClientId), true);
        }
    }
}