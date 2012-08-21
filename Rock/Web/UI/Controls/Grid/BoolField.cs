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
    /// <see cref="Grid"/> Column to display a boolean value.
    /// </summary>
    [ToolboxData( "<{0}:BoolField runat=server></{0}:BoolField>" )]
    public class BoolField : BoundField
    {
        /// <summary>
        /// Initializes the <see cref="T:System.Web.UI.WebControls.BoundField"/> object.
        /// </summary>
        /// <param name="enableSorting">true if sorting is supported; otherwise, false.</param>
        /// <param name="control">The data control that owns the <see cref="T:System.Web.UI.WebControls.BoundField"/>.</param>
        /// <returns>
        /// false in all cases.
        /// </returns>
        public override bool Initialize( bool enableSorting, Control control )
        {
            this.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            this.ItemStyle.CssClass = "grid-icon-cell bool";

            return base.Initialize( enableSorting, control );
        }

        /// <summary>
        /// Formats the specified field value for a cell in the <see cref="T:System.Web.UI.WebControls.BoundField"/> object.
        /// </summary>
        /// <param name="dataValue">The field value to format.</param>
        /// <param name="encode">true to encode the value; otherwise, false.</param>
        /// <returns>
        /// The field value converted to the format specified by <see cref="P:System.Web.UI.WebControls.BoundField.DataFormatString"/>.
        /// </returns>
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

            return string.Format( "<span class=\"{0}\">{1}</span>", 
                boolValue.ToString().ToLower(), ( boolValue ? " " : "" ) );
        }
    }
}