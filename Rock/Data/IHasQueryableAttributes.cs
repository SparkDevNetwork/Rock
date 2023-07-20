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

namespace Rock.Data
{
    /// <summary>
    /// Specifies that the entity supports the fast queryable entity attributes
    /// backed by SQL views.
    /// </summary>
    /// <typeparam name="T">The <see cref="QueryableAttributeValue"/> subclass that represents the attribute values for this entity.</typeparam>
    internal interface IHasQueryableAttributes<T>
        where T : QueryableAttributeValue
    {
        /// <summary>
        /// Gets the entity attribute values. This should only be used inside
        /// LINQ statements when building a where clause for the query. This
        /// property should only be used inside LINQ statements for filtering
        /// or selecting values. Do <b>not</b> use it for accessing the
        /// attributes after the entity has been loaded.
        /// </summary>
        /// <value>The entity attribute values.</value>
        ICollection<T> EntityAttributeValues { get; }
    }
}
