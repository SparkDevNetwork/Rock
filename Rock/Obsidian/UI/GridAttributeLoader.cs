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
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Obsidian.UI
{
    /// <summary>
    /// Centralized helper for loading filtered attribute values for entities
    /// referenced by grid rows.
    /// </summary>
    public static class GridAttributeLoader
    {
        /// <summary>
        /// Loads attribute values for the set of entities selected from items limited to the provided grid attributes.
        /// </summary>
        /// <typeparam name="TItem">The row type.</typeparam>
        /// <typeparam name="TEntity">The entity type that has attributes.</typeparam>
        /// <param name="items">The materialized list of items (rows).</param>
        /// <param name="entitySelector">Selector that returns the attributed entity from the row.</param>
        /// <param name="gridAttributes">The grid attributes to load.</param>
        /// <param name="rockContext">The database context.</param>
        public static void LoadFor<TItem, TEntity>( IEnumerable<TItem> items, Func<TItem, TEntity> entitySelector, IEnumerable<AttributeCache> gridAttributes, RockContext rockContext )
            where TEntity : class, IHasAttributes, IEntity, new()
        {
            var attributes = gridAttributes?.ToList();

            if ( attributes == null || attributes.Count == 0 )
            {
                return;
            }

            var entities = items?.Select( entitySelector ).Where( e => e != null ).ToList();

            if ( entities == null || entities.Count == 0 )
            {
                return;
            }

            var attributeIds = attributes.Select( a => a.Id ).ToList();

            Helper.LoadFilteredAttributes<TEntity>( entities, rockContext, a => attributeIds.Contains( a.Id ) );
        }
    }
}

