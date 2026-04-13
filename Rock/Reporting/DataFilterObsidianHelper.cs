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

using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.Security;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Reporting;
using Rock.Web.Cache;

namespace Rock.Reporting
{
    /// <summary>
    /// Helper methods for converting <see cref="DataViewFilter"/> values to and from
    /// Obsidian-friendly bags and discovering available data filter types.
    /// </summary>
    internal static class DataFilterObsidianHelper
    {
        /// <summary>
        /// Gets the list of authorized data filter types for the specified entity type.
        /// </summary>
        internal static List<DataFilterTypeItemBag> GetAvailableFilterTypes( Type filteredEntityType, Person currentPerson, IEnumerable<Guid> excludedFilterTypeGuids = null, bool isObsidianSupported = false )
        {
            if ( filteredEntityType == null )
            {
                return new List<DataFilterTypeItemBag>();
            }

            var excludedGuids = excludedFilterTypeGuids?.ToHashSet() ?? new HashSet<Guid>();

            return DataFilterContainer.GetComponentsByFilteredEntityName( filteredEntityType.FullName )
                .Where( c => c.IsAuthorized( Authorization.VIEW, currentPerson ) )
                .Where( c => !isObsidianSupported || IsObsidianSupported( filteredEntityType, c ) )
                .Select( c => new
                {
                    Component = c,
                    EntityType = EntityTypeCache.Get( c.TypeName )
                } )
                .Where( x => x.EntityType != null && !excludedGuids.Contains( x.EntityType.Guid ) )
                .OrderBy( x => x.Component.Section.IsNullOrWhiteSpace() ? 0 : 1 )
                .ThenBy( x => x.Component.Section )
                .ThenBy( x => x.Component.Order )
                .ThenBy( x => x.Component.GetTitle( filteredEntityType ) )
                .Select( x => new DataFilterTypeItemBag
                {
                    FilterTypeGuid = x.EntityType.Guid,
                    Title = x.Component.GetTitle( filteredEntityType ),
                    Section = x.Component.Section,
                    Description = x.Component.Description
                } )
                .ToList();
        }

        private static bool IsObsidianSupported( Type filteredEntityType, DataFilterComponent component )
        {
            try
            {
                var methodInfo = component
                    .GetType()
                    .GetMethod( nameof( DataFilterComponent.GetSelectionFromObsidianComponentData ) );

                return methodInfo.GetBaseDefinition().DeclaringType != methodInfo.DeclaringType;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Converts a <see cref="DataViewFilter"/> entity tree into an Obsidian bag tree.
        /// </summary>
        public static DataViewFilterBag ToBag( DataViewFilter filter, Type filteredEntityType, RockContext rockContext, RockRequestContext requestContext )
        {
            if ( filter == null )
            {
                return null;
            }

            var bag = new DataViewFilterBag
            {
                Guid = filter.Guid,
                ExpressionType = filter.ExpressionType,
                Selection = filter.Selection,
                ChildFilters = filter.ChildFilters?
                    .Select( child => ToBag( child, filteredEntityType, rockContext, requestContext ) )
                    .ToList()
                    ?? new List<DataViewFilterBag>()
            };

            if ( filter.ExpressionType == FilterExpressionType.Filter && filter.EntityTypeId.HasValue )
            {
                var filterEntityType = EntityTypeCache.Get( filter.EntityTypeId.Value, rockContext );
                var component = filterEntityType != null
                    ? DataFilterContainer.GetComponent( filterEntityType.Name )
                    : null;

                bag.FilterTypeGuid = filterEntityType?.Guid;

                if ( component != null )
                {
                    bag.ComponentData = component.GetObsidianComponentData( filteredEntityType, filter.Selection, rockContext, requestContext );
                }
            }

            return bag;
        }

        /// <summary>
        /// Converts an Obsidian bag tree into a <see cref="DataViewFilter"/> entity tree.
        /// </summary>
        public static DataViewFilter ToEntity( DataViewFilterBag bag, Type filteredEntityType, RockContext rockContext, RockRequestContext requestContext )
        {
            return ToEntityInternal( bag, filteredEntityType, null, rockContext, requestContext );
        }

        /// <summary>
        /// Recursively converts a bag to an entity and assigns parent relationships.
        /// </summary>
        private static DataViewFilter ToEntityInternal( DataViewFilterBag bag, Type filteredEntityType, DataViewFilter parentFilter, RockContext rockContext, RockRequestContext requestContext )
        {
            if ( bag == null )
            {
                return null;
            }

            var filter = new DataViewFilter
            {
                Guid = bag.Guid == Guid.Empty ? Guid.NewGuid() : bag.Guid,
                ExpressionType = bag.ExpressionType,
                Parent = parentFilter,
                ChildFilters = new List<DataViewFilter>()
            };

            if ( bag.ExpressionType == FilterExpressionType.Filter && bag.FilterTypeGuid.HasValue )
            {
                var filterEntityType = EntityTypeCache.Get( bag.FilterTypeGuid.Value, rockContext );
                var component = filterEntityType != null
                    ? DataFilterContainer.GetComponent( filterEntityType.Name )
                    : null;

                filter.EntityTypeId = filterEntityType?.Id;

                if ( component != null )
                {
                    var componentData = bag.ComponentData ?? new Dictionary<string, string>();
                    filter.Selection = componentData.Any()
                        ? component.GetSelectionFromObsidianComponentData( filteredEntityType, componentData, rockContext, requestContext )
                        : bag.Selection;
                    filter.RelatedDataViewId = component.GetRelatedDataViewId( filteredEntityType, filter.Selection, rockContext );
                }
                else
                {
                    filter.Selection = bag.Selection;
                }
            }

            foreach ( var childBag in bag.ChildFilters ?? new List<DataViewFilterBag>() )
            {
                var childFilter = ToEntityInternal( childBag, filteredEntityType, filter, rockContext, requestContext );

                if ( childFilter != null )
                {
                    filter.ChildFilters.Add( childFilter );
                }
            }

            return filter;
        }
    }
}
