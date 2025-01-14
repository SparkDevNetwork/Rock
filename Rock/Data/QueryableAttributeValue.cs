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

namespace Rock.Data
{
    /// <summary>
    /// Defines the structure of queryable attribute values. These are represented
    /// in the database by custom views created by Rock automatically.
    /// </summary>
    public abstract class QueryableAttributeValue
    {
        /// <summary>
        /// Gets the identifier of the attribute value. This will not always match
        /// the <see cref="IEntity.Id"/> property of the <see cref="Model.AttributeValue"/>.
        /// </summary>
        /// <value>The identifier of the attribute value.</value>
        public long Id { get; private set; }

        /// <summary>
        /// Gets the identifier of the entity this value belongs to.
        /// </summary>
        /// <value>The identifier of the entity this value belongs to.</value>
        public int EntityId { get; private set; }

        /// <summary>
        /// Gets the identifier of the attribute this value represents.
        /// </summary>
        /// <value>The identifier of the attribute this value represents.</value>
        public int AttributeId { get; private set; }

        /// <summary>
        /// Gets the key of the attribute.
        /// </summary>
        /// <value>The key of the attribute.</value>
        public string Key { get; private set; }

        /// <summary>
        /// Gets the raw attribute value.
        /// </summary>
        /// <value>The raw attribute value.</value>
        public string Value { get; private set; }

        /// <summary>
        /// Gets the persisted text value.
        /// </summary>
        /// <value>The persisted text value.</value>
        public string PersistedTextValue { get; private set; }

        /// <summary>
        /// Gets the persisted HTML value.
        /// </summary>
        /// <value>The persisted HTML value.</value>
        public string PersistedHtmlValue { get; private set; }

        /// <summary>
        /// Gets the persisted condensed text value.
        /// </summary>
        /// <value>The persisted condensed text value.</value>
        public string PersistedCondensedTextValue { get; private set; }

        /// <summary>
        /// Gets the persisted condensed HTML value.
        /// </summary>
        /// <value>The persisted condensed HTML value.</value>
        public string PersistedCondensedHtmlValue { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the persisted values are dirty.
        /// </summary>
        /// <value><c>true</c> if the peristed values are dirty; otherwise, <c>false</c>.</value>
        public bool IsPersistedValueDirty { get; private set; }

        /// <summary>
        /// Gets the value checksum. This is a hash of <see cref="Value"/> that
        /// is automatically calculated by the database.
        /// </summary>
        /// <value>The value checksum.</value>
        public int? ValueChecksum { get; private set; }
    }
}
