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
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Reporting
{
    /// <summary>
    /// Parameters for the GetQuery actions.
    /// </summary>
    public sealed class GetQueryableOptions
    {
        /// <summary>
        /// Gets or sets the database context.
        /// </summary>
        /// <value>
        /// The database context.
        /// </value>
        public System.Data.Entity.DbContext DbContext { get; set; }

        /// <summary>
        /// Gets or sets the sort property.
        /// </summary>
        /// <value>
        /// The sort property.
        /// </value>
        public SortProperty SortProperty { get; set; }

        /// <summary>
        /// Gets or sets the data view filter overrides.
        /// </summary>
        /// <value>
        /// The data view filter overrides.
        /// </value>
        public DataViewFilterOverrides DataViewFilterOverrides { get; set; }

        /// <summary>
        /// Gets or sets the database timeout seconds.
        /// </summary>
        /// <value>
        /// The database timeout seconds.
        /// </value>
        public int? DatabaseTimeoutSeconds { get; set; }
    }

}
