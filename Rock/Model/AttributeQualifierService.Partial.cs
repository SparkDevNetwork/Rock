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
using System.Linq;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.AttributeQualifier"/> entity objects.
    /// </summary>
    public partial class AttributeQualifierService
    {
        /// <summary>
        /// Returns an enumerable collection containing the <see cref="Rock.Model.AttributeQualifier">AttributeQualifiers</see> by <see cref="Rock.Model.Attribute"/>.
        /// </summary>
        /// <param name="attributeId">A <see cref="System.Int32"/> that represents the Id of the <see cref="Rock.Model.Attribute"/> to retrieve <see cref="Rock.Model.AttributeQualifier"/>.</param>
        /// <returns>An enumerable collection containing the <see cref="Rock.Model.AttributeQualifier">AttributeQualifiers</see> that the specified <see cref="Rock.Model.Attribute"/> uses.</returns>
        public IQueryable<AttributeQualifier> GetByAttributeId( int attributeId )
        {
            return Queryable().Where( t => t.AttributeId == attributeId );
        }
    }
}
