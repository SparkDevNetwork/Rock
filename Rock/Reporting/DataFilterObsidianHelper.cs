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
            return ToBag( new DataViewFilterEntityAdapter( filter ), filteredEntityType, rockContext, requestContext );
        }

        /// <summary>
        /// Converts a <see cref="DataViewFilter"/> entity tree into an Obsidian bag tree.
        /// </summary>
        public static DataViewFilterBag ToBag( DataViewFilterCache filter, Type filteredEntityType, RockContext rockContext, RockRequestContext requestContext )
        {
            return ToBag( new DataViewFilterCacheAdapter( filter ), filteredEntityType, rockContext, requestContext );
        }

        /// <summary>
        /// Converts a <see cref="DataViewFilter"/> entity tree into an Obsidian bag tree.
        /// </summary>
        private static DataViewFilterBag ToBag<TSource, TAdapter>( IDataViewFilterAdapter<TSource, TAdapter> filter, Type filteredEntityType, RockContext rockContext, RockRequestContext requestContext )
            where TAdapter : IDataViewFilterAdapter<TSource, TAdapter>
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
        /// <param name="bag">The filter tree posted by the client.</param>
        /// <param name="filteredEntityType">The type of entity being filtered.</param>
        /// <param name="existingFilter">The filter tree currently saved for the item being edited, or <c>null</c> if there is none. Only pass the item's own filter, never one identified by the client.</param>
        /// <param name="rockContext">The context to use for database access.</param>
        /// <param name="requestContext">The context that describes the current request.</param>
        /// <returns>The new filter tree.</returns>
        public static DataViewFilter ToEntity( DataViewFilterBag bag, Type filteredEntityType, DataViewFilter existingFilter, RockContext rockContext, RockRequestContext requestContext )
        {
            var existingFilters = new Dictionary<Guid, DataViewFilter>();
            var pendingFilters = new Queue<DataViewFilter>();

            if ( existingFilter != null )
            {
                pendingFilters.Enqueue( existingFilter );
            }

            while ( pendingFilters.Count > 0 )
            {
                var pendingFilter = pendingFilters.Dequeue();

                existingFilters[pendingFilter.Guid] = pendingFilter;

                foreach ( var childFilter in pendingFilter.ChildFilters ?? new List<DataViewFilter>() )
                {
                    pendingFilters.Enqueue( childFilter );
                }
            }

            return ToEntityInternal( bag, filteredEntityType, null, existingFilters, rockContext, requestContext );
        }

        /// <summary>
        /// Recursively converts a bag to an entity and assigns parent relationships.
        /// </summary>
        private static DataViewFilter ToEntityInternal( DataViewFilterBag bag, Type filteredEntityType, DataViewFilter parentFilter, Dictionary<Guid, DataViewFilter> existingFilters, RockContext rockContext, RockRequestContext requestContext )
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
                    var postedSelection = componentData.Any()
                        ? component.GetSelectionFromObsidianComponentData( filteredEntityType, componentData, rockContext, requestContext )
                        : bag.Selection;
                    var existingSelection = existingFilters.TryGetValue( filter.Guid, out var existingFilter ) && existingFilter.EntityTypeId == filter.EntityTypeId
                        ? existingFilter.Selection
                        : null;

                    filter.Selection = GetAuthorizedSelection( component, filteredEntityType, existingSelection, postedSelection, requestContext?.CurrentPerson );
                    filter.RelatedDataViewId = component.GetRelatedDataViewId( filteredEntityType, filter.Selection, rockContext );
                }
                else
                {
                    filter.Selection = bag.Selection;
                }
            }

            foreach ( var childBag in bag.ChildFilters ?? new List<DataViewFilterBag>() )
            {
                var childFilter = ToEntityInternal( childBag, filteredEntityType, filter, existingFilters, rockContext, requestContext );

                if ( childFilter != null )
                {
                    filter.ChildFilters.Add( childFilter );
                }
            }

            return filter;
        }

        /// <summary>
        /// Gets the selection to save for a filter, keeping the existing
        /// selection when the person is not authorized for it and rejecting
        /// a posted selection the person is not authorized for.
        /// </summary>
        /// <param name="component">The filter component.</param>
        /// <param name="filteredEntityType">The type of entity being filtered.</param>
        /// <param name="existingSelection">The saved selection for this filter, or <c>null</c> if it is new.</param>
        /// <param name="postedSelection">The selection built from the posted data.</param>
        /// <param name="person">The person saving the filter.</param>
        /// <returns>The selection to save.</returns>
        private static string GetAuthorizedSelection( DataFilterComponent component, Type filteredEntityType, string existingSelection, string postedSelection, Person person )
        {
            /*
                10/19/2026 - DSH

                Same rule FilterField applies in WebForms (TH173). The existing
                selection comes from the database, matched by Guid within the
                item's own saved filter tree, so the client cannot supply it.

                Reason: A crafted post could filter on a secured attribute, and
                the editor could silently clear a filter the person cannot view.
            */
            if ( existingSelection != null && !component.IsSelectionAuthorizedForEdit( filteredEntityType, existingSelection, person ) )
            {
                return existingSelection;
            }

            if ( !component.IsSelectionAuthorizedForEdit( filteredEntityType, postedSelection, person ) )
            {
                return existingSelection ?? string.Empty;
            }

            return postedSelection;
        }

        private interface IDataViewFilterAdapter<TSource, TAdapter> where TAdapter : IDataViewFilterAdapter<TSource, TAdapter>
        {
            TSource Source { get; }
            Guid Guid { get; }
            FilterExpressionType ExpressionType { get; }
            string Selection { get; }
            List<TAdapter> ChildFilters { get; }
            int? EntityTypeId { get; }
        }

        private class DataViewFilterEntityAdapter : IDataViewFilterAdapter<DataViewFilter, DataViewFilterEntityAdapter>
        {
            public DataViewFilterEntityAdapter( DataViewFilter entity )
            {
                Source = entity;
            }

            public DataViewFilter Source { get; }

            public Guid Guid => Source.Guid;

            public FilterExpressionType ExpressionType => Source.ExpressionType;

            public string Selection => Source.Selection;

            public List<DataViewFilterEntityAdapter> ChildFilters => Source.ChildFilters?.Select( f => new DataViewFilterEntityAdapter( f ) ).ToList();

            public int? EntityTypeId => Source.EntityTypeId;
        }

        private class DataViewFilterCacheAdapter : IDataViewFilterAdapter<DataViewFilterCache, DataViewFilterCacheAdapter>
        {
            public DataViewFilterCacheAdapter( DataViewFilterCache cache )
            {
                Source = cache;
            }

            public DataViewFilterCache Source { get; }

            public Guid Guid => Source.Guid;

            public FilterExpressionType ExpressionType => Source.ExpressionType;

            public string Selection => Source.Selection;

            public List<DataViewFilterCacheAdapter> ChildFilters => Source.ChildFilters?.Select( f => new DataViewFilterCacheAdapter( f ) ).ToList();

            public int? EntityTypeId => Source.EntityTypeId;
        }
    }
}
