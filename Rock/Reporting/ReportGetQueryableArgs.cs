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
using Rock.Model;
using Rock.Reporting;
using Rock.Web.Cache;

namespace Rock.Reporting
{

    /// <summary>
    /// 
    /// </summary>
    public sealed class ReportGetQueryableArgs
    {
        /// <summary>
        /// Gets or sets the report database context.
        /// </summary>
        /// <value>
        /// The report database context.
        /// </value>
        public Rock.Data.DbContext ReportDbContext { get; set; }

        /// <summary>
        /// Gets or sets the entity fields.
        /// </summary>
        /// <value>
        /// The entity fields.
        /// </value>
        public Dictionary<int, EntityField> EntityFields { get; set; }

        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public Dictionary<int, AttributeCache> Attributes { get; set; }

        /// <summary>
        /// Gets or sets the select components.
        /// </summary>
        /// <value>
        /// The select components.
        /// </value>
        public Dictionary<int, ReportField> SelectComponents { get; set; }

        /// <summary>
        /// Gets or sets the sort property.
        /// </summary>
        /// <value>
        /// The sort property.
        /// </value>
        public Rock.Web.UI.Controls.SortProperty SortProperty { get; set; }

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

        /// <summary>
        /// Gets or sets a value indicating whether this instance is communication.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is communication; otherwise, <c>false</c>.
        /// </value>
        public bool IsCommunication { get; set; }
    }
}
