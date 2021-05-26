// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Class for saving sort expression
    /// </summary>
    [Serializable]
    public class SortProperty
    {
        /// <summary>
        /// Gets or sets the direction.
        /// </summary>
        /// <value>
        /// The direction.
        /// </value>
        public SortDirection Direction { get; set; }

        /// <summary>
        /// Gets the direction as an ASC or DESC string.
        /// </summary>
        /// <value>
        /// The direction string.
        /// </value>
        public string DirectionString
        {
            get
            {
                if ( Direction == SortDirection.Descending )
                {
                    return "DESC";
                }
                else
                {
                    return "ASC";
                }
            }
        }

        /// <summary>
        /// Gets or sets the Column(s) specification for Sorting.
        /// Specify multiple columns as "column1, column2, column3".
        /// To sort DESC append column(s) with a " desc", for example: "column1 desc, column2, column3 desc"
        /// </summary>
        /// <value>
        /// The property.
        /// </value>
        public string Property { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SortProperty"/> class.
        /// </summary>
        public SortProperty()
        {
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format( "{0} [{1}]", this.Property, this.Direction );
        }
    }
}