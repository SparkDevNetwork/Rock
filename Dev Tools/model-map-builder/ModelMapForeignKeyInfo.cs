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

namespace Rock.ModelMapBuilder
{
    /// <summary>
    /// A foreign key relationship from a model's table to another table.
    /// </summary>
    internal class ModelMapForeignKeyInfo
    {
        /// <summary>
        /// Gets or sets the column on this model's table that holds the reference.
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// Gets or sets the referenced (target) table name.
        /// </summary>
        public string ReferenceTableName { get; set; }

        /// <summary>
        /// Gets or sets the referenced (target) column name.
        /// </summary>
        public string ReferenceColumnName { get; set; }
    }
}
