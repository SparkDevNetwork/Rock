//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Event argument used by the <see cref="Grid"/> events
    /// </summary>
    public class RowEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the index of the row that fired the event
        /// </summary>
        /// <value>
        /// The index of the row.
        /// </value>
        public int RowIndex { get; private set; }

        /// <summary>
        /// Gets the row key value.
        /// </summary>
        /// <value>
        /// The row key value.
        /// </value>
        public object RowKeyValue { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RowEventArgs" /> class.
        /// </summary>
        /// <param name="row">The row.</param>
        public RowEventArgs( GridViewRow row )
        {
            if ( row != null )
            {
                RowIndex = row.RowIndex;

                Grid grid = ( row.Parent.Parent as Grid );
                if ( grid.DataKeyNames.Length > 0 )
                {
                    RowKeyValue = grid.DataKeys[row.RowIndex].Value;
                }
            }
            else
            {
                RowIndex = -1;
            }
        }
    }
}