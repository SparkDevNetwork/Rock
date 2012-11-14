//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    [ToolboxData( "<{0}:DateTimeField runat=server></{0}:DateTimeField>" )]
    public class DateTimeField : BoundField
    {
        /// <summary>
        /// Initializes the <see cref="T:System.Web.UI.WebControls.BoundField" /> object.
        /// </summary>
        /// <param name="enableSorting">true if sorting is supported; otherwise, false.</param>
        /// <param name="control">The data control that owns the <see cref="T:System.Web.UI.WebControls.BoundField" />.</param>
        /// <returns>
        /// false in all cases.
        /// </returns>
        public override bool Initialize( bool enableSorting, System.Web.UI.Control control )
        {
            this.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
            this.DataFormatString = "{0:g}";
            return base.Initialize( enableSorting, control );
        }
    }
}