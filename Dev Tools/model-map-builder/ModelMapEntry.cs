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

namespace Rock.ModelMapBuilder
{
    /// <summary>
    /// The detailed information about a single model class.
    /// </summary>
    internal class ModelMapEntry
    {
        /// <summary>
        /// Gets or sets the model name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the database table name, when it differs from the model name.
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets the model's XML documentation comment.
        /// </summary>
        public ModelMapComment Comment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this model is obsolete.
        /// </summary>
        public bool IsObsolete { get; set; }

        /// <summary>
        /// Gets or sets the message explaining why the model is obsolete, if applicable.
        /// </summary>
        public string ObsoleteMessage { get; set; }

        /// <summary>
        /// Gets or sets the model's properties.
        /// </summary>
        public List<ModelMapPropertyEntry> Properties { get; set; }

        /// <summary>
        /// Gets or sets the indexes on the model's table.
        /// </summary>
        public List<ModelMapIndexInfo> Indexes { get; set; }

        /// <summary>
        /// Gets or sets the foreign keys on the model's table.
        /// </summary>
        public List<ModelMapForeignKeyInfo> ForeignKeys { get; set; }

        /// <summary>
        /// Gets or sets the model's methods. This is <see langword="null"/> (and
        /// omitted from the output) unless methods were explicitly requested.
        /// </summary>
        public List<ModelMapMethodEntry> Methods { get; set; }

        /// <summary>
        /// Determines whether <see cref="Comment"/> should be serialized.
        /// </summary>
        public bool ShouldSerializeComment()
        {
            return Comment != null && !Comment.IsEmpty;
        }

        /// <summary>
        /// Determines whether <see cref="Indexes"/> should be serialized.
        /// </summary>
        public bool ShouldSerializeIndexes()
        {
            return Indexes != null && Indexes.Count > 0;
        }

        /// <summary>
        /// Determines whether <see cref="ForeignKeys"/> should be serialized.
        /// </summary>
        public bool ShouldSerializeForeignKeys()
        {
            return ForeignKeys != null && ForeignKeys.Count > 0;
        }

        /// <summary>
        /// Determines whether <see cref="Methods"/> should be serialized
        /// (omitted entirely when methods were not requested).
        /// </summary>
        public bool ShouldSerializeMethods()
        {
            return Methods != null;
        }
    }
}
