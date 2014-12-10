// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
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
    public class RockBoundField : BoundField, IPriorityColumn
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
        /// Gets or sets the column priority.
        /// </summary>
        /// <value>
        /// The priority of the column.
        /// </value>
        public ColumnPriority ColumnPriority
        {
            get {
                object t = ViewState["ColumnPriority"];
                return (t == null) ? ColumnPriority.AlwaysVisible : (ColumnPriority)t; 
            }
            set { ViewState["ColumnPriority"] = value; }
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

        /// <summary>
        /// Performs basic instance initialization for a data control field.
        /// </summary>
        /// <param name="sortingEnabled">A value that indicates whether the control supports the sorting of columns of data.</param>
        /// <param name="control">The data control that owns the <see cref="T:System.Web.UI.WebControls.DataControlField"/>.</param>
        /// <returns>
        /// Always returns false.
        /// </returns>
        public override bool Initialize( bool sortingEnabled, Control control )
        {
            
            return base.Initialize( sortingEnabled, control );
        }
    }
}