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

using Rock.Web.Cache;

namespace Rock.Blocks
{
    /// <summary>
    /// Client Block Type
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />
    /// <seealso cref="IRockObsidianBlockType" />
    [Obsolete( "Use RockBlockType instead." )]
    [RockObsolete( "1.16" )]
    public abstract class RockObsidianBlockType : RockBlockType, IRockObsidianBlockType
    {
        #region Methods

        /// <summary>
        /// Gets any non-empty EntityTypeQualifiedColumn values for the entity
        /// specified by the generic type. If this entity type has no attributes
        /// with qualified columns then an empty list will be returned.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity whose attributes will be inspected.</typeparam>
        /// <returns>A list of distinct EntityTypeQualifiedColumn values for <typeparamref name="TEntity"/>.</returns>
        [RockObsolete( "1.16" )]
        [Obsolete( "Use the GetAttributeQualifiedColumns method on AttributeCache instead.")]
        protected List<string> GetAttributeQualifiedColumns<TEntity>()
        {
            return AttributeCache.GetAttributeQualifiedColumns<TEntity>();
        }

        #endregion Methods
    }
}
