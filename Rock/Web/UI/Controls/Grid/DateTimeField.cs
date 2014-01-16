//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    [ToolboxData( "<{0}:DateTimeField runat=server></{0}:DateTimeField>" )]
    public class DateTimeField : BoundField
    {
        /// <summary>
        /// Gets or sets a value indicating whether value should be displayed as an elapsed time (i.e. "3 days ago").
        /// </summary>
        /// <value>
        /// <c>true</c> if [format as elapsed time]; otherwise, <c>false</c>.
        /// </value>
        public bool FormatAsElapsedTime
        {
            get { return ViewState["FormatAsElapsedTime"] as bool? ?? false; }
            set { ViewState["FormatAsElapsedTime"] = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeField" /> class.
        /// </summary>
        public DateTimeField()
            : base()
        {
            // Let the Header be left aligned (that's how Bootstrap wants it), but have the item be right-aligned
            this.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
            this.DataFormatString = "{0:g}";
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
            if ( FormatAsElapsedTime )
            {
                if ( dataValue is DateTime )
                {
                    return ( (DateTime)dataValue ).ToElapsedString();
                }

                if ( dataValue is DateTime? )
                {
                    return ( (DateTime)dataValue ).ToElapsedString();
                }
            }

            return base.FormatDataValue( dataValue, encode );
        }
    }
}