//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using Rock;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// <see cref="Grid"/> Column for displaying a badge
    /// </summary>
    [ToolboxData( "<{0}:BadgeField runat=server></{0}:BadgeField>" )]
    public class BadgeField : BoundField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BadgeField" /> class.
        /// </summary>
        public BadgeField()
            : base()
        {
            this.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            this.HeaderStyle.CssClass = "span1";
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
            BadgeRowEventArgs eventArg = new BadgeRowEventArgs( dataValue );
            if ( SetBadgeType != null )
            {
                SetBadgeType( this, eventArg );
            }

            string css = "badge";
            if ( eventArg.BadgeType != BadgeType.None )
            {
                css += " badge-" + eventArg.BadgeType.ConvertToString().ToLower();
            }

            string fieldValue = base.FormatDataValue( eventArg.FieldValue, encode );

            return string.Format( "<span class='{0}'>{1}</span>", css, fieldValue );
        }

        /// <summary>
        /// Occurs when badge field is being formatted.  Use to set the badge type
        /// based on the current row's field value.
        /// </summary>
        public event EventHandler<BadgeRowEventArgs> SetBadgeType;
    }


    /// <summary>
    /// 
    /// </summary>
    public class BadgeRowEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the field value.
        /// </summary>
        /// <value>
        /// The field value.
        /// </value>
        public object FieldValue { get; private set; }

        /// <summary>
        /// Gets or sets the type of the badge.
        /// </summary>
        /// <value>
        /// The type of the badge.
        /// </value>
        public BadgeType BadgeType { get; set; }

        public BadgeRowEventArgs(object fieldValue)
        {
            FieldValue = fieldValue;
            BadgeType = BadgeType.None;
        }
    }
}