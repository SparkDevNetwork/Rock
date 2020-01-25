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
using System.Collections.Generic;
using System.Web.UI.WebControls;

using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.BulkExport
{
    /// <summary>
    /// Options for BulkExports
    /// </summary>
    public class ExportOptions
    {
        /// <summary>
        /// Optional field to sort by. This must be a mapped property on the Person model.
        /// </summary>
        /// <value>
        /// The sort by.
        /// </value>
        public string SortBy { get; set; }

        /// <summary>
        /// The sort direction (1 = Ascending, 0 = Descending). Default is 1 (Ascending).
        /// </summary>
        /// <value>
        /// The sort direction.
        /// </value>
        public SortDirection SortDirection { get; set; }

        /// <summary>
        /// The optional data view to use for filtering.
        /// </summary>
        /// <value>
        /// The data view identifier.
        /// </value>
        public int? DataViewId { get; set; }

        /// <summary>
        /// The optional date/time to filter to only get newly updated items.
        /// </summary>
        /// <value>
        /// The modified since.
        /// </value>
        public DateTime? ModifiedSince { get; set; }

        /// <summary>
        /// Optional list of attributes for the attribute values that should be included with each Export record.
        /// </summary>
        /// <value>
        /// The attribute keys.
        /// </value>
        public List<AttributeCache> AttributeList { get; set; }

        /// <summary>
        /// Raw/Formatted (default is Raw)
        /// </summary>
        /// <value>
        /// The type of the attribute return.
        /// </value>
        public AttributeReturnType AttributeReturnType { get; set; }

        /// <summary>
        /// Gets the sort property from <see cref="SortBy"/> and <see cref="SortDirection"/>
        /// </summary>
        /// <value>
        /// The sort property.
        /// </value>
        internal SortProperty SortProperty
        {
            get
            {
                if ( this.SortBy.IsNotNullOrWhiteSpace() )
                {
                    return new SortProperty { Direction = this.SortDirection, Property = this.SortBy };
                }
                else
                {
                    return new SortProperty { Direction = this.SortDirection, Property = "Id" };
                }
            }
        }
    }
}
