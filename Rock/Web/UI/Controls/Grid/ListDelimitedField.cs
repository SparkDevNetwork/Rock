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
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Displays a BoundField as a Delimited List when the underlying datatype is enumerable
    /// </summary>
    [ToolboxData( "<{0}:ListDelimitedField runat=server></{0}:ListDelimitedField>" )]
    public class ListDelimitedField : RockBoundField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListDelimitedField" /> class.
        /// </summary>
        public ListDelimitedField()
            : base()
        {
            //
        }

        /// <summary>
        /// Formats the specified field value for a cell in the <see cref="T:System.Web.UI.WebControls.BoundField"/> object.
        /// </summary>
        /// <param name="dataValue">The field value to format.</param>
        /// <param name="encode">true to encode the value; otherwise, false.</param>
        /// <returns>
        /// The field value converted to the format specified by <see cref="P:System.Web.UI.WebControls.BoundField.DataFormatString"/>.
        /// </returns>
        protected override string FormatDataValue( object dataValue, bool encode )
        {
            if (dataValue is IEnumerable<object>)
            {
                var list = ( dataValue as IEnumerable<object> );
                return list.ToList().AsDelimited( Delimiter ?? ", " );
            }
            else
            {
                return base.FormatDataValue( dataValue, encode );
            }
        }

        /// <summary>
        /// Gets or sets the delimiter.
        /// </summary>
        /// <value>
        /// The delimiter.
        /// </value>
        public string Delimiter { get; set; }
    }
}