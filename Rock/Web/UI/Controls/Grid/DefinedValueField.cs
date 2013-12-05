//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control for selecting a defined value
    /// </summary>
    [ToolboxData("<{0}:DefinedValueField runat=server></{0}:DefinedValueField>")]
    public class DefinedValueField : BoundField
    {
        /// <summary>
        /// Formats the specified field value for a cell in the <see cref="T:System.Web.UI.WebControls.BoundField" /> object.
        /// </summary>
        /// <param name="dataValue">The field value to format.</param>
        /// <param name="encode">true to encode the value; otherwise, false.</param>
        /// <returns>
        /// The field value converted to the format specified by <see cref="P:System.Web.UI.WebControls.BoundField.DataFormatString" />.
        /// </returns>
        protected override string FormatDataValue(object dataValue, bool encode)
        {
            if (dataValue is int)
            {
                dataValue = Rock.Web.Cache.DefinedValueCache.Read((int)dataValue).Name;
            }

            return base.FormatDataValue(dataValue, encode);
        }
    }
}
