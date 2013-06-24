//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Specialized;
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
        /// Usually the Id value of the data in the row
        /// Cast this to an Int (or use RowKeyId) if your datakey is an integer
        /// </summary>
        /// <value>
        /// The row key value.
        /// </value>
        public object RowKeyValue { get; private set; }

        /// <summary>
        /// Gets the row key values
        /// Use this if your datarow has multiple keys (rare)
        /// </summary>
        /// <value>
        /// The row key values.
        /// </value>
        public IOrderedDictionary RowKeyValues { get; private set; }

        /// <summary>
        /// Gets the row key id
        /// Usually the Id value of the data in the row
        /// </summary>
        /// <value>
        /// The row key id.
        /// </value>
        public int RowKeyId {
            get
            {
                return (int)RowKeyValue;
            }
        }

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
                    RowKeyValues = grid.DataKeys[row.RowIndex].Values;
                }
            }
            else
            {
                RowIndex = -1;
            }
        }
    }
}