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
    [ToolboxData("<{0}:DefinedValueField runat=server></{0}:DefinedValueField>")]
    public class DefinedValueField : BoundField
    {
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
