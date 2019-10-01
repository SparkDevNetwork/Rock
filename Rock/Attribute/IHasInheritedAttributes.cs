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

namespace Rock.Attribute
{
    /// <summary>
    /// Represents any class that supports having inherited attributes
    /// </summary>
    public interface IHasInheritedAttributes
    {
        /// <summary>
        /// Get a list of all inherited Attributes that should be applied to this entity.
        /// </summary>
        /// <returns>A list of all inherited AttributeCache objects.</returns>
        List<Rock.Web.Cache.AttributeCache> GetInheritedAttributes( Data.RockContext rockContext );

        /// <summary>
        /// Get any alternate Ids that should be used when loading attribute value for this entity.
        /// </summary>
        /// <returns>A list of any alternate entity Ids that should be used when loading attribute values.</returns>
        List<int> GetAlternateEntityIds( Data.RockContext rockContext );
    }
}
