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
        /// Initializes a new instance of the <see cref="DateTimeField" /> class.
        /// </summary>
        public DateTimeField()
            : base()
        {
            this.HeaderStyle.HorizontalAlign = HorizontalAlign.Right;
            this.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
            this.DataFormatString = "{0:g}";
        }
    }
}