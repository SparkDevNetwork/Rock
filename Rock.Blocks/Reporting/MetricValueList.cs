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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Field;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Reporting.MetricValueList;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Reporting
{
    /// <summary>
    /// Displays a list of metric values for a single metric, with optional
    /// per-partition entity filters and links to the metric value detail block
    /// for create / edit.
    /// </summary>
    [DisplayName( "Metric Value List" )]
    [Category( "Reporting" )]
    [Description( "Displays a list of metric values." )]
    [IconCssClass( "ti ti-list" )]
    [SupportedSiteTypes( SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the metric value details.",
        Key = AttributeKey.DetailPage )]

    [CustomizedGrid]

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Secondary )]

    [Rock.SystemGuid.EntityTypeGuid( "2226E624-72E5-4D54-8A26-A0CDEA67630D" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "100E5FE9-BE54-4C1A-B7E6-E8145B8E9257" )]
    [Rock.SystemGuid.BlockTypeGuid( "E40A1526-04D0-42A0-B275-D1AE161E2E57" )]
    public class MetricValueList : RockEntityListBlockType<MetricValue>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PageParameterKey
        {
            public const string MetricId = "MetricId";
            public const string MetricValueId = "MetricValueId";
            public const string MetricCategoryId = "MetricCategoryId";
            public const string ExpandedIds = "ExpandedIds";
        }

        private static class PreferenceKey
        {
            public const string FilterDateRange = "filter-date-range";
            public const string FilterGoalMeasure = "filter-goal-measure";

            /// <summary>
            /// Prefix for the per-metric partition filter preference. The metric
            /// IdKey is appended so two metrics on the same block don't share state.
            /// </summary>
            public const string FilterPartitionValuesPrefix = "filter-partition-values-";
        }

        #endregion Keys

        #region Fields

        private Metric _metric;
        private PersonPreferenceCollection _personPreferences;
        private bool? _canEdit;
        private bool? _canView;
        private Dictionary<int, Dictionary<int, string>> _entityNameLookup;

        // Set in GetListQueryable when a saved filter actually narrows the
        // query. Authoritative source for the client's gear-icon state.
        private bool _hasActiveFilters;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the lazily-resolved block-scoped Person Preferences — the single
        /// source of truth for filter state. Resolved once per request.
        /// </summary>
        private PersonPreferenceCollection PersonPreferences => _personPreferences ??= this.GetBlockPersonPreferences();

        /// <summary>
        /// Gets the metric resolved from the page parameters, or <c>null</c> if no
        /// metric is in scope. Falls back through
        /// <see cref="PageParameterKey.MetricCategoryId"/> first (so the block
        /// works when invoked from CategoryTreeView) and then
        /// <see cref="PageParameterKey.MetricId"/>. Cached after the first read.
        /// </summary>
        private Metric ResolvedMetric
        {
            get
            {
                if ( _metric != null )
                {
                    return _metric;
                }

                var disablePredictableIds = PageCache.Layout.Site.DisablePredictableIds;

                var categoryKey = PageParameter( PageParameterKey.MetricCategoryId );
                if ( categoryKey.IsNotNullOrWhiteSpace() )
                {
                    var metricId = new MetricCategoryService( RockContext ).Get( categoryKey, !disablePredictableIds )?.MetricId;
                    if ( metricId.HasValue )
                    {
                        return _metric = new MetricService( RockContext ).Get( metricId.Value );
                    }
                }

                var metricKey = PageParameter( PageParameterKey.MetricId );
                if ( metricKey.IsNotNullOrWhiteSpace() )
                {
                    return _metric = new MetricService( RockContext ).Get( metricKey, !disablePredictableIds );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current person can edit metric
        /// values (drives Add and Delete availability). Either block-level Edit
        /// or metric-level Edit grants the permission.
        /// </summary>
        private bool CanEdit => _canEdit ??= BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson )
            || ( ResolvedMetric?.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) == true );

        /// <summary>
        /// Gets a value indicating whether the current person can view metric
        /// values for the resolved metric (drives whether the grid is shown).
        /// Block-level Edit (block admin) overrides; otherwise the metric's own
        /// View authorization is enforced. Metric-level Edit does not imply
        /// View, so an explicit View-Deny on the metric is respected even when
        /// the user has inherited Edit (e.g. from the parent category).
        /// </summary>
        private bool CanView => _canView ??= ResolvedMetric != null
            && ( BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson )
                || ResolvedMetric.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) );

        /// <summary>
        /// Gets the sliding date range filter currently saved in person preferences,
        /// or <c>null</c> if no range is selected.
        /// </summary>
        private SlidingDateRangeBag FilterDateRange => PersonPreferences
            .GetValue( PreferenceKey.FilterDateRange )
            .ToSlidingDateRangeBagOrNull();

        /// <summary>
        /// Gets the Goal/Measure filter currently saved in person preferences, or
        /// <c>null</c> if no value is selected.
        /// </summary>
        private MetricValueType? FilterGoalMeasure => PersonPreferences
            .GetValue( PreferenceKey.FilterGoalMeasure )
            .ConvertToEnumOrNull<MetricValueType>();

        /// <summary>
        /// Gets the per-partition entity filters from the metric-scoped person
        /// preference, parsed as a dictionary keyed by metric partition Guid.
        /// </summary>
        private Dictionary<Guid, string> FilterPartitionValues
        {
            get
            {
                var metricKey = ResolvedMetric?.IdKey;
                if ( metricKey.IsNullOrWhiteSpace() )
                {
                    return new Dictionary<Guid, string>();
                }

                var raw = PersonPreferences.GetValue( PreferenceKey.FilterPartitionValuesPrefix + metricKey );
                if ( raw.IsNullOrWhiteSpace() )
                {
                    return new Dictionary<Guid, string>();
                }

                return raw.FromJsonOrNull<Dictionary<Guid, string>>() ?? new Dictionary<Guid, string>();
            }
        }

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<MetricValueListOptionsBag>();

            if ( ResolvedMetric == null )
            {
                // Match WebForms: when no metric is in scope (e.g. user is on a
                // category node or about to add a new metric), the block renders
                // nothing rather than showing a notification.
                return box;
            }

            if ( !CanView )
            {
                // Match WebForms: when the user lacks View access on the metric,
                // the block hides itself entirely rather than surfacing a message.
                return box;
            }

            var builder = GetGridBuilder();

            box.IsAddEnabled = CanEdit;
            box.IsDeleteEnabled = CanEdit;
            box.ExpectedRowCount = null;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();
            box.NavigationUrls = GetBoxNavigationUrls();

            return box;
        }

        /// <summary>
        /// Builds the options bag sent to the client.
        /// </summary>
        /// <returns>The options bag.</returns>
        private MetricValueListOptionsBag GetBoxOptions()
        {
            var metric = ResolvedMetric;

            return new MetricValueListOptionsBag
            {
                IsBlockVisible = true,
                MetricIdKey = metric?.IdKey,
                MetricValueTypeItems = typeof( MetricValueType ).ToEnumListItemBag(),
                PartitionFilters = BuildPartitionFilters( metric ),
                IsPartitionsColumnVisible = metric?.MetricPartitions?.Any( p => p.EntityTypeId.HasValue ) == true
            };
        }

        /// <summary>
        /// Builds the per-partition filter descriptors used to render entity
        /// pickers in the grid settings modal.
        /// </summary>
        /// <param name="metric">The metric whose partitions should be inspected.</param>
        /// <returns>One filter descriptor per entity-typed partition.</returns>
        private List<MetricPartitionFilterBag> BuildPartitionFilters( Metric metric )
        {
            var filters = new List<MetricPartitionFilterBag>();

            if ( metric?.MetricPartitions == null )
            {
                return filters;
            }

            foreach ( var partition in metric.MetricPartitions.Where( p => p.EntityTypeId.HasValue ).OrderBy( p => p.Order ) )
            {
                var entityTypeCache = EntityTypeCache.Get( partition.EntityTypeId.Value );
                if ( entityTypeCache?.SingleValueFieldType == null )
                {
                    continue;
                }

                var fieldType = entityTypeCache.SingleValueFieldType;
                var privateConfigurationValues = GetPartitionPrivateConfigurationValues( fieldType.Field, partition );
                var publicConfigurationValues = fieldType.Field.GetPublicConfigurationValues( privateConfigurationValues, ConfigurationValueUsage.Edit, null );

                var label = partition.Label.IsNotNullOrWhiteSpace() ? partition.Label : entityTypeCache.FriendlyName;

                filters.Add( new MetricPartitionFilterBag
                {
                    Attribute = new PublicAttributeBag
                    {
                        Name = label,
                        FieldTypeGuid = fieldType.Guid,
                        ConfigurationValues = publicConfigurationValues
                    },
                    MetricPartitionGuid = partition.Guid
                } );
            }

            return filters;
        }

        /// <summary>
        /// Builds the navigation URL dictionary sent to the client.
        /// </summary>
        /// <returns>A dictionary of navigation key to URL.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var qryParams = new Dictionary<string, string>
            {
                [PageParameterKey.MetricValueId] = "((Key))"
            };

            var metricKey = ResolvedMetric?.IdKey;
            if ( metricKey.IsNotNullOrWhiteSpace() )
            {
                qryParams[PageParameterKey.MetricId] = metricKey;
            }

            var metricCategoryKey = PageParameter( PageParameterKey.MetricCategoryId );
            if ( metricCategoryKey.IsNotNullOrWhiteSpace() )
            {
                qryParams[PageParameterKey.MetricCategoryId] = metricCategoryKey;
            }

            var expandedIds = PageParameter( PageParameterKey.ExpandedIds );
            if ( expandedIds.IsNotNullOrWhiteSpace() )
            {
                qryParams[PageParameterKey.ExpandedIds] = expandedIds;
            }

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, qryParams )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<MetricValue> GetListQueryable( RockContext rockContext )
        {
            var metric = ResolvedMetric;
            if ( metric == null || !CanView )
            {
                return Enumerable.Empty<MetricValue>().AsQueryable();
            }

            var qry = new MetricValueService( rockContext )
                .Queryable()
                .Include( a => a.MetricValuePartitions.Select( p => p.MetricPartition ) )
                .Where( a => a.MetricId == metric.Id );

            var dateRange = FilterDateRange?.ToActualDateRange();
            if ( dateRange?.Start.HasValue == true )
            {
                _hasActiveFilters = true;
                var start = dateRange.Start.Value;
                qry = qry.Where( a => a.MetricValueDateTime >= start );
            }

            if ( dateRange?.End.HasValue == true )
            {
                _hasActiveFilters = true;
                var end = dateRange.End.Value;
                qry = qry.Where( a => a.MetricValueDateTime < end );
            }

            var goalMeasure = FilterGoalMeasure;
            if ( goalMeasure.HasValue )
            {
                _hasActiveFilters = true;
                qry = qry.Where( a => a.MetricValueType == goalMeasure.Value );
            }

            // Apply each saved per-partition filter that resolves to a known
            // entity. Each filter narrows the query to values whose
            // MetricValuePartitions include a row matching both the partition
            // and the picked entity.
            foreach ( var kvp in FilterPartitionValues )
            {
                if ( kvp.Value.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                var partition = metric.MetricPartitions.FirstOrDefault( p => p.Guid == kvp.Key );
                if ( partition?.EntityTypeId == null )
                {
                    continue;
                }

                var entityId = ResolveFilterEntityId( partition, kvp.Value );
                if ( !entityId.HasValue )
                {
                    continue;
                }

                _hasActiveFilters = true;

                var partitionId = partition.Id;
                var resolvedEntityId = entityId.Value;

                qry = qry.Where( a => a.MetricValuePartitions.Any(
                    x => x.MetricPartitionId == partitionId && x.EntityId == resolvedEntityId ) );
            }

            return qry;
        }

        /// <inheritdoc/>
        protected override IQueryable<MetricValue> GetOrderedListQueryable( IQueryable<MetricValue> queryable, RockContext rockContext )
        {
            return queryable
                .OrderByDescending( a => a.MetricValueDateTime )
                .ThenBy( a => a.YValue )
                .ThenBy( a => a.XValue )
                .ThenByDescending( a => a.ModifiedDateTime );
        }

        /// <inheritdoc/>
        protected override List<MetricValue> GetListItems( IQueryable<MetricValue> queryable, RockContext rockContext )
        {
            var items = queryable.ToList();

            // Pre-fetch entity names for every partition reference in the loaded
            // rows. Replaces the per-row lazy-loaded lookup the legacy block did,
            // which was an N+1 against arbitrary entity types.
            _entityNameLookup = BuildEntityNameLookup( items, rockContext );

            return items;
        }

        /// <inheritdoc/>
        protected override GridBuilder<MetricValue> GetGridBuilder()
        {
            return new GridBuilder<MetricValue>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddDateTimeField( "metricValueDateTime", a => a.MetricValueDateTime )
                .AddTextField( "metricValueType", a => a.MetricValueType.ConvertToString() )
                .AddField( "yValue", a => a.YValue )
                .AddTextField( "xValue", a => a.XValue )
                .AddTextField( "partitions", a => BuildPartitionsString( a ) );
        }

        /// <summary>
        /// Composes the comma-joined entity-name string shown in the partitions
        /// column for a single metric value.
        /// </summary>
        /// <param name="metricValue">The metric value whose partitions should be rendered.</param>
        /// <returns>A comma-and-and joined string of partition entity names.</returns>
        private string BuildPartitionsString( MetricValue metricValue )
        {
            if ( _entityNameLookup == null || metricValue?.MetricValuePartitions == null )
            {
                return null;
            }

            var names = new List<string>();

            foreach ( var partition in metricValue.MetricValuePartitions
                .Where( p => p.EntityId.HasValue && p.MetricPartition?.EntityTypeId.HasValue == true ) )
            {
                if ( _entityNameLookup.TryGetValue( partition.MetricPartition.EntityTypeId.Value, out var byEntityId )
                    && byEntityId.TryGetValue( partition.EntityId.Value, out var name )
                    && name.IsNotNullOrWhiteSpace() )
                {
                    names.Add( name );
                }
            }

            return names.AsDelimited( ", ", " and " );
        }

        /// <summary>
        /// Pre-fetches a lookup of entity-type id → entity id → display name for
        /// every partition entity reference in the supplied rows. Performs one
        /// query per distinct entity type rather than one per row.
        /// </summary>
        /// <param name="items">The metric values whose partitions should be inspected.</param>
        /// <param name="rockContext">The data context to use for the lookup queries.</param>
        /// <returns>The populated lookup dictionary.</returns>
        private Dictionary<int, Dictionary<int, string>> BuildEntityNameLookup( IList<MetricValue> items, RockContext rockContext )
        {
            var lookup = new Dictionary<int, Dictionary<int, string>>();

            var idsByType = items
                .SelectMany( i => i.MetricValuePartitions ?? new List<MetricValuePartition>() )
                .Where( p => p.EntityId.HasValue && p.MetricPartition?.EntityTypeId.HasValue == true )
                .GroupBy( p => p.MetricPartition.EntityTypeId.Value )
                .ToDictionary( g => g.Key, g => g.Select( p => p.EntityId.Value ).Distinct().ToList() );

            foreach ( var kvp in idsByType )
            {
                var entityTypeCache = EntityTypeCache.Get( kvp.Key );
                if ( entityTypeCache == null )
                {
                    continue;
                }

                var entityType = entityTypeCache.GetEntityType();
                if ( entityType == null )
                {
                    continue;
                }

                var entityIds = kvp.Value;
                var byId = new Dictionary<int, string>();

                var entities = Reflection.GetQueryableForEntityType( entityType, rockContext )
                    ?.Where( e => entityIds.Contains( e.Id ) )
                    .ToList();

                if ( entities != null )
                {
                    foreach ( var entity in entities )
                    {
                        byId[entity.Id] = entity.ToString();
                    }
                }

                lookup[kvp.Key] = byId;
            }

            return lookup;
        }

        /// <summary>
        /// Resolves the saved partition filter value (from the field type's public
        /// edit value, typically an entity Guid string) to the underlying
        /// integer entity id used in the database.
        /// </summary>
        /// <param name="partition">The partition the filter targets.</param>
        /// <param name="publicValue">The saved public edit value.</param>
        /// <returns>The integer entity id, or <c>null</c> if it could not be resolved.</returns>
        private int? ResolveFilterEntityId( MetricPartition partition, string publicValue )
        {
            var entityTypeCache = EntityTypeCache.Get( partition.EntityTypeId.Value );
            if ( entityTypeCache?.SingleValueFieldType?.Field is not IEntityFieldType entityFieldType )
            {
                return null;
            }

            var fieldType = entityTypeCache.SingleValueFieldType;
            var privateConfigurationValues = GetPartitionPrivateConfigurationValues( fieldType.Field, partition );
            var privateValue = fieldType.Field.GetPrivateEditValue( publicValue, privateConfigurationValues );
            var entity = entityFieldType.GetEntity( privateValue, RockContext );
            return entity?.Id;
        }

        /// <summary>
        /// Builds the private (storage-format) configuration-value dictionary
        /// for the supplied partition's field type. Honors any entity qualifier
        /// the field type supports.
        /// </summary>
        /// <param name="fieldType">The partition's single-value field type.</param>
        /// <param name="partition">The partition whose qualifier columns/values should drive the configuration.</param>
        /// <returns>The private configuration values dictionary expected by the field type's edit-value APIs.</returns>
        private static Dictionary<string, string> GetPartitionPrivateConfigurationValues( IFieldType fieldType, MetricPartition partition )
        {
            Dictionary<string, ConfigurationValue> configurationValues;
            if ( fieldType is IEntityQualifierFieldType qualifier )
            {
                configurationValues = qualifier.GetConfigurationValuesFromEntityQualifier( partition.EntityTypeQualifierColumn, partition.EntityTypeQualifierValue );
            }
            else
            {
                configurationValues = new Dictionary<string, ConfigurationValue>();
            }

            return configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Returns the grid data wrapped with a server-computed
        /// HasActiveFilters flag for the grid settings gear icon.
        /// </summary>
        /// <returns>An action result containing the grid data response bag.</returns>
        public override BlockActionResult GetGridData()
        {
            _hasActiveFilters = false;

            var gridData = GetGridDataBag( RockContext );

            return ActionOk( new MetricValueListGetGridDataResponseBag
            {
                GridData = gridData,
                HasActiveFilters = _hasActiveFilters
            } );
        }

        /// <summary>
        /// Deletes the specified metric value and its partition rows.
        /// </summary>
        /// <param name="key">The identifier of the metric value to delete.</param>
        /// <returns>An empty result indicating success or an error message.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var metricValueService = new MetricValueService( RockContext );
            var metricValuePartitionService = new MetricValuePartitionService( RockContext );

            var metricValue = metricValueService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );
            if ( metricValue == null )
            {
                return ActionBadRequest( $"{MetricValue.FriendlyTypeName} not found." );
            }

            if ( !CanEdit )
            {
                return ActionBadRequest( $"Not authorized to delete {MetricValue.FriendlyTypeName}." );
            }

            if ( !metricValueService.CanDelete( metricValue, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            // MetricValuePartition has WillCascadeOnDelete(false) on its FK to
            // MetricValue, so the children must be removed explicitly before the
            // parent. Wrap both deletes so a failure can't leave orphan rows.
            RockContext.WrapTransaction( () =>
            {
                metricValuePartitionService.DeleteRange( metricValue.MetricValuePartitions );
                metricValueService.Delete( metricValue );
                RockContext.SaveChanges();
            } );

            return ActionOk();
        }

        #endregion Block Actions
    }
}
