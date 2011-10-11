using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Controls
{
    [ToolboxData( "<{0}:BoolField runat=server></{0}:BoolField>" )]
    public class BoolField : BoundField
    {
        public override void InitializeCell( DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex )
        {
            base.InitializeCell( cell, cellType, rowState, rowIndex );

            this.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        }
        protected override string FormatDataValue( object dataValue, bool encode )
        {
            bool boolValue = false;

            string value = base.FormatDataValue( dataValue, encode );
            if ( !bool.TryParse( value, out boolValue ) )
            {
                int intValue = 0;
                if ( Int32.TryParse( value, out intValue ) )
                    boolValue = intValue != 0;
            }

            return string.Format( "<span class=\"col-bool {0}\">{1}</span>", 
                boolValue.ToString().ToLower(), ( boolValue ? "X" : "" ) );
        }
    }
}