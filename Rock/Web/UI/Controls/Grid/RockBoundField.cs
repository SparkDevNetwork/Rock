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
    [ToolboxData( "<{0}:RockBoundField runat=server></{0}:RockBoundField>" )]
    public class RockBoundField : BoundField
    {
        /// <summary>
        /// Gets or sets the length of the truncate.
        /// </summary>
        /// <value>
        /// The length of the truncate.
        /// </value>
        public int TruncateLength
        {
            get { return ViewState["TruncateLength"] as int? ?? 0; }
            set { ViewState["TruncateLength"] = value; }
        }

        /// <summary>
        /// Formats the specified field value for a cell in the <see cref="T:System.Web.UI.WebControls.BoundField" /> object.
        /// </summary>
        /// <param name="dataValue">The field value to format.</param>
        /// <param name="encode">true to encode the value; otherwise, false.</param>
        /// <returns>
        /// The field value converted to the format specified by <see cref="P:System.Web.UI.WebControls.BoundField.DataFormatString" />.
        /// </returns>
        protected override string FormatDataValue( object dataValue, bool encode )
        {
            if (dataValue is string && TruncateLength > 0)
            {
                return base.FormatDataValue( ( (string)dataValue ).Truncate( TruncateLength ), encode );
            }

            return base.FormatDataValue( dataValue, encode );
        }
    }
}