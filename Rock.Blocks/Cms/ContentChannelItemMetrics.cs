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
using System.Linq.Expressions;

using Rock.Attribute;
using Rock.Cms.Utm;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.ContentChannelItemMetrics;
using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays metrics for a content channel item.
    /// </summary>

    [DisplayName( "Content Channel Item Metrics" )]
    [Category( "CMS" )]
    [Description( "Displays metrics for a content channel item." )]
    [IconCssClass( "ti ti-chart-line" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [EnumsField( "UTM Metrics to Show",
        Description = "The UTM dimensions (Source, Medium, Campaign, Term, Content) to include in the metric. If none are selected, all dimensions with captured data will be shown.",
        EnumSourceType = typeof( UtmDimension ),
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.UtmMetricsToShow )]

    [Rock.SystemGuid.EntityTypeGuid( "6885E548-DE26-4967-A191-F18BE7313D9F" )]
    [Rock.SystemGuid.BlockTypeGuid( "447960A5-276E-4D5A-9AF0-133F90AA43C0" )]
    public class ContentChannelItemMetrics : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string UtmMetricsToShow = "UtmMetricsToShow";
        }

        private static class PageParameterKey
        {
            public const string ContentChannelItemId = "ContentChannelItemId";
        }

        private static class PersonPreferenceKey
        {
            /// <summary>
            /// The sliding date range, shared between the Overview panel and the Viewer Details grid.
            /// </summary>
            public const string DateRange = "DateRange";

            /// <summary>
            /// The Viewer Details campus filter, stored as a campus guid.
            /// </summary>
            public const string FilterCampus = "filter-campus";

            /// <summary>
            /// The Viewer Details connection status filter, stored as a defined value guid.
            /// </summary>
            public const string FilterConnectionStatus = "filter-connection-status";

            /// <summary>
            /// The Viewer Details original source filter, stored as the source label.
            /// </summary>
            public const string FilterSource = "filter-source";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// The device client types that get their own breakdown bucket; anything else is grouped as Unknown.
        /// </summary>
        private static readonly string[] KnownDeviceClientTypes = { "Desktop", "Mobile", "Tablet" };

        /// <summary>
        /// The label used for known people who have no connection status value.
        /// </summary>
        private const string NoStatusLabel = "No Status";

        /// <summary>
        /// The UTM dimensions in the order they should be presented.
        /// </summary>
        private static readonly UtmDimension[] AllUtmDimensions =
        {
            UtmDimension.Source,
            UtmDimension.Medium,
            UtmDimension.Campaign,
            UtmDimension.Term,
            UtmDimension.Content
        };

        /// <summary>
        /// The maximum number of rows returned for a single UTM dimension, so a flood of junk values can
        /// never overwhelm the report.
        /// </summary>
        private const int MaxUtmRowCount = 100;

        /// <summary>
        /// Substrings that indicate SQL comment syntax and never appear in a legitimate UTM tag.
        /// </summary>
        private static readonly string[] UtmSqlCommentTokens = { "--", "/*", "*/" };

        #endregion Fields

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ContentChannelItemMetricsInitializationBox();

            var contentChannelItem = GetContentChannelItem();

            if ( contentChannelItem != null )
            {
                var contentChannel = contentChannelItem.ContentChannel;

                box.ContentChannelItemIdKey = contentChannelItem.IdKey;
                box.Title = contentChannelItem.Title;
                box.ContentChannelName = contentChannel?.Name;
                box.ItemStatus = contentChannelItem.Status;
                box.IsStatusVisible = contentChannel != null
                    && contentChannel.RequiresApproval
                    && !contentChannel.ContentChannelType.DisableStatus;
                box.IsCollectingData = GetItemInteractionQuery( contentChannelItem.Id ).Any();
                box.ViewerGridDefinition = GetViewerGridBuilder().BuildDefinition();
                box.ViewerFilterOptions = new ViewerFilterOptionsBag
                {
                    Campuses = GetCampusFilterOptions(),
                    ConnectionStatuses = GetConnectionStatusFilterOptions(),
                    Sources = GetSourceFilterOptions( contentChannelItem.Id )
                };
            }

            return box;
        }

        /// <summary>
        /// Gets the content channel item identified by the page parameter.
        /// </summary>
        /// <returns>The content channel item, or <c>null</c> if not found.</returns>
        private ContentChannelItem GetContentChannelItem()
        {
            var itemKey = PageParameter( PageParameterKey.ContentChannelItemId );

            if ( itemKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return new ContentChannelItemService( RockContext )
                .Get( itemKey, !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <summary>
        /// Gets the base interaction query for a content channel item, scoped to the content channel
        /// interaction medium and not filtered by date.
        /// </summary>
        /// <param name="contentChannelItemId">The content channel item identifier.</param>
        /// <returns>The unfiltered interaction query for the item.</returns>
        private IQueryable<Interaction> GetItemInteractionQuery( int contentChannelItemId )
        {
            var contentChannelMediumValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_CONTENTCHANNEL.AsGuid() );

            return new InteractionService( RockContext ).Queryable().AsNoTracking()
                .Where( i => i.InteractionComponent.EntityId == contentChannelItemId
                    && i.InteractionComponent.InteractionChannel.ChannelTypeMediumValueId == contentChannelMediumValueId );
        }

        /// <summary>
        /// Builds the Overview KPI metrics for the content channel item over the given date range,
        /// including the percent change versus the immediately preceding period of equal length.
        /// </summary>
        /// <param name="contentChannelItemId">The content channel item identifier.</param>
        /// <param name="dateRangeDelimited">The delimited sliding date range string.</param>
        /// <returns>The Overview metrics.</returns>
        private OverviewMetricsBag BuildOverviewMetrics( int contentChannelItemId, string dateRangeDelimited )
        {
            var dateRange = RockDateTimeHelper.CalculateDateRangeFromDelimitedValues( dateRangeDelimited );

            var baseQuery = GetItemInteractionQuery( contentChannelItemId );

            var currentPeriodQuery = ApplyPeriodFilter( baseQuery, dateRange.Start, dateRange.End );

            var current = GetPeriodCounts( currentPeriodQuery );

            // Compare against the immediately preceding period of the same length, but only
            // when the range is bounded on both ends.
            PeriodCounts previous = null;
            if ( dateRange.Start.HasValue && dateRange.End.HasValue )
            {
                var periodLength = dateRange.End.Value - dateRange.Start.Value;
                var previousPeriodQuery = ApplyPeriodFilter( baseQuery, dateRange.Start.Value - periodLength, dateRange.Start.Value );
                previous = GetPeriodCounts( previousPeriodQuery );
            }

            return new OverviewMetricsBag
            {
                TotalViews = current.TotalViews,
                UniqueViews = current.UniqueViews,
                KnownPeople = current.KnownPeople,
                TotalViewsDeltaPercent = CalculateDeltaPercent( current.TotalViews, previous?.TotalViews ),
                UniqueViewsDeltaPercent = CalculateDeltaPercent( current.UniqueViews, previous?.UniqueViews ),
                KnownPeopleDeltaPercent = CalculateDeltaPercent( current.KnownPeople, previous?.KnownPeople ),
                ViewsOverTime = GetViewsOverTime( currentPeriodQuery, dateRange.Start, dateRange.End ),
                DeviceBreakdown = GetDeviceBreakdown( currentPeriodQuery ),
                ConnectionStatusBreakdown = GetConnectionStatusBreakdown( currentPeriodQuery ),
                TopReferrers = GetTopReferrers( currentPeriodQuery ),
                UtmBreakdowns = GetUtmBreakdowns( currentPeriodQuery )
            };
        }

        /// <summary>
        /// Applies the period date bounds to an interaction query. Either bound may be omitted for an
        /// open-ended range.
        /// </summary>
        /// <param name="query">The interaction query to filter.</param>
        /// <param name="startDateTime">The inclusive start of the period, or <c>null</c> for unbounded.</param>
        /// <param name="endDateTime">The exclusive end of the period, or <c>null</c> for unbounded.</param>
        /// <returns>The filtered query.</returns>
        private IQueryable<Interaction> ApplyPeriodFilter( IQueryable<Interaction> query, DateTime? startDateTime, DateTime? endDateTime )
        {
            if ( startDateTime.HasValue )
            {
                query = query.Where( i => i.InteractionDateTime >= startDateTime.Value );
            }

            if ( endDateTime.HasValue )
            {
                query = query.Where( i => i.InteractionDateTime < endDateTime.Value );
            }

            return query;
        }

        /// <summary>
        /// Gets the view counts broken down by device client type, collapsing anything that is not
        /// Desktop, Mobile, or Tablet (including sessions with no recorded device) into an Unknown bucket.
        /// </summary>
        /// <param name="query">The interaction query, already scoped to the item and period.</param>
        /// <returns>The device breakdown slices, ordered highest to lowest count, omitting empty buckets.</returns>
        private List<MetricSliceBag> GetDeviceBreakdown( IQueryable<Interaction> query )
        {
            // Pull the raw client-type counts from the database, then bucket the known types and
            // collapse everything else into Unknown in memory.
            var rawCounts = query
                .GroupBy( i => i.InteractionSession.DeviceType.ClientType )
                .Select( g => new { ClientType = g.Key, Count = g.Count() } )
                .ToList();

            var knownCounts = new Dictionary<string, int>();
            var unknownCount = 0;

            foreach ( var row in rawCounts )
            {
                if ( row.ClientType != null && KnownDeviceClientTypes.Contains( row.ClientType ) )
                {
                    knownCounts[row.ClientType] = row.Count;
                }
                else
                {
                    unknownCount += row.Count;
                }
            }

            var slices = new List<MetricSliceBag>();

            foreach ( var clientType in KnownDeviceClientTypes )
            {
                if ( knownCounts.TryGetValue( clientType, out var count ) )
                {
                    slices.Add( new MetricSliceBag { Label = clientType, Count = count } );
                }
            }

            if ( unknownCount > 0 )
            {
                slices.Add( new MetricSliceBag { Label = "Unknown", Count = unknownCount } );
            }

            // Order highest to lowest, but always keep the Unknown catch-all bucket last.
            return slices
                .OrderBy( s => s.Label == "Unknown" ? 1 : 0 )
                .ThenByDescending( s => s.Count )
                .ToList();
        }

        /// <summary>
        /// Gets the distinct known-people counts broken down by connection status, collapsing people
        /// with no connection status value into a No Status bucket.
        /// </summary>
        /// <param name="query">The interaction query, already scoped to the item and period.</param>
        /// <returns>The connection status slices, ordered highest to lowest count, omitting empty buckets.</returns>
        private List<MetricSliceBag> GetConnectionStatusBreakdown( IQueryable<Interaction> query )
        {
            // Reduce to the distinct set of known people and their connection status, then count the
            // number of people in each status.
            var statusCounts = query
                .Where( i => i.PersonAliasId.HasValue )
                .Select( i => new
                {
                    i.PersonAlias.PersonId,
                    i.PersonAlias.Person.ConnectionStatusValueId
                } )
                .Distinct()
                .GroupBy( x => x.ConnectionStatusValueId )
                .Select( g => new { ConnectionStatusValueId = g.Key, Count = g.Count() } )
                .ToList();

            // Resolve status names in memory and merge any that resolve to the same label (for
            // example, a null value and a deleted defined value both land in No Status).
            return statusCounts
                .GroupBy( row => row.ConnectionStatusValueId.HasValue
                    ? DefinedValueCache.GetValue( row.ConnectionStatusValueId.Value ) ?? NoStatusLabel
                    : NoStatusLabel )
                .Select( g => new MetricSliceBag { Label = g.Key, Count = g.Sum( x => x.Count ) } )
                // Order highest to lowest, but always keep the No Status catch-all bucket last.
                .OrderBy( s => s.Label == NoStatusLabel ? 1 : 0 )
                .ThenByDescending( s => s.Count )
                .ToList();
        }

        /// <summary>
        /// Gets the top referrer hosts by view count, excluding interactions with no captured
        /// referrer (direct traffic).
        /// </summary>
        /// <param name="query">The interaction query, already scoped to the item and period.</param>
        /// <returns>The referrer slices, ordered highest to lowest count.</returns>
        private List<MetricSliceBag> GetTopReferrers( IQueryable<Interaction> query )
        {
            return query
                .Where( i => i.ChannelCustomIndexed1 != null && i.ChannelCustomIndexed1 != "" )
                .GroupBy( i => i.ChannelCustomIndexed1 )
                .Select( g => new { Referrer = g.Key, Count = g.Count() } )
                .OrderByDescending( x => x.Count )
                .ThenBy( x => x.Referrer )
                // Cap the row count so a flood of junk referrers cannot overwhelm the report, matching the UTM breakdowns.
                .Take( MaxUtmRowCount )
                .ToList()
                .Select( x => new MetricSliceBag { Label = x.Referrer, Count = x.Count } )
                .ToList();
        }

        /// <summary>
        /// Gets the ranked UTM breakdowns for every enabled dimension, including dimensions that have no
        /// captured data so the front end can list all selected dimensions in the dropdown.
        /// </summary>
        /// <param name="query">The interaction query, already scoped to the item and period.</param>
        /// <returns>The UTM breakdowns, one per selected dimension, in canonical dimension order.</returns>
        private List<UtmDimensionMetricsBag> GetUtmBreakdowns( IQueryable<Interaction> query )
        {
            return GetSelectedUtmDimensions()
                .Select( dimension => new UtmDimensionMetricsBag
                {
                    Dimension = dimension,
                    Items = GetUtmDimensionBreakdown( query, dimension )
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the UTM dimensions enabled by the block setting, defaulting to all dimensions when
        /// none are explicitly selected.
        /// </summary>
        /// <returns>The enabled dimensions in canonical order.</returns>
        private List<UtmDimension> GetSelectedUtmDimensions()
        {
            var selectedDimensions = GetAttributeValue( AttributeKey.UtmMetricsToShow )
                .SplitDelimitedValues()
                .AsEnumList<UtmDimension>();

            if ( selectedDimensions.Count == 0 )
            {
                return AllUtmDimensions.ToList();
            }

            // Filter through AllUtmDimensions to preserve the canonical display order.
            return AllUtmDimensions
                .Where( dimension => selectedDimensions.Contains( dimension ) )
                .ToList();
        }

        /// <summary>
        /// Gets the ranked breakdown of captured values for a single UTM dimension. Source, Medium, and
        /// Campaign combine mapped defined values with unmapped raw strings; Term and Content are always
        /// raw strings.
        /// </summary>
        /// <param name="query">The interaction query, already scoped to the item and period.</param>
        /// <param name="dimension">The UTM dimension to break down.</param>
        /// <returns>The dimension slices, ordered highest to lowest count.</returns>
        private List<MetricSliceBag> GetUtmDimensionBreakdown( IQueryable<Interaction> query, UtmDimension dimension )
        {
            switch ( dimension )
            {
                case UtmDimension.Source:
                    return GetMappedUtmBreakdown( query, i => new UtmValueRow { ValueId = i.SourceValueId, RawValue = i.Source }, UtmHelper.GetUtmSourceNameFromDefinedValueOrText );
                case UtmDimension.Medium:
                    return GetMappedUtmBreakdown( query, i => new UtmValueRow { ValueId = i.MediumValueId, RawValue = i.Medium }, UtmHelper.GetUtmMediumNameFromDefinedValueOrText );
                case UtmDimension.Campaign:
                    return GetMappedUtmBreakdown( query, i => new UtmValueRow { ValueId = i.CampaignValueId, RawValue = i.Campaign }, UtmHelper.GetUtmCampaignNameFromDefinedValueOrText );
                case UtmDimension.Term:
                    return GetRawUtmBreakdown( query, i => i.Term );
                case UtmDimension.Content:
                    return GetRawUtmBreakdown( query, i => i.Content );
                default:
                    return new List<MetricSliceBag>();
            }
        }

        /// <summary>
        /// Gets the breakdown for a UTM dimension that may be stored as either a mapped defined value or an
        /// unmapped raw string. Mapped values are resolved to their defined value name, then merged with
        /// the raw strings by label.
        /// </summary>
        /// <param name="query">The interaction query, already scoped to the item and period.</param>
        /// <param name="valueSelector">Projects an interaction to its defined value id and raw string for the dimension.</param>
        /// <param name="resolveName">Resolves a (defined value id, raw string) pair to its display name for the dimension.</param>
        /// <returns>The dimension slices, ordered highest to lowest count.</returns>
        private List<MetricSliceBag> GetMappedUtmBreakdown( IQueryable<Interaction> query, Expression<Func<Interaction, UtmValueRow>> valueSelector, Func<int?, string, string> resolveName )
        {
            // Group by the (defined value id, raw string) pair in the database, ignoring interactions that
            // captured neither for this dimension.
            var rows = query
                .Select( valueSelector )
                .Where( r => r.ValueId.HasValue || ( r.RawValue != null && r.RawValue != "" ) )
                .GroupBy( r => new { r.ValueId, r.RawValue } )
                .Select( g => new { g.Key.ValueId, g.Key.RawValue, Count = g.Count() } )
                .ToList();

            // Resolve mapped ids to their defined value name in memory (falling back to the raw string),
            // then merge anything that resolves to the same label. Mapped defined values are trusted, so
            // only unmapped raw strings are run through the data-quality check. The row cap keeps a flood
            // of junk values from overwhelming the report.
            return rows
                .Select( r => new
                {
                    r.ValueId,
                    Label = resolveName( r.ValueId, r.RawValue ),
                    r.Count
                } )
                .Where( r => r.ValueId.HasValue ? !string.IsNullOrWhiteSpace( r.Label ) : IsQualityUtmValue( r.Label ) )
                .GroupBy( r => r.Label )
                .Select( g => new MetricSliceBag { Label = g.Key, Count = g.Sum( x => x.Count ) } )
                .OrderByDescending( s => s.Count )
                .ThenBy( s => s.Label )
                .Take( MaxUtmRowCount )
                .ToList();
        }

        /// <summary>
        /// Gets the breakdown for a UTM dimension that is always stored as a raw string (Term and Content).
        /// </summary>
        /// <param name="query">The interaction query, already scoped to the item and period.</param>
        /// <param name="valueSelector">Selects the raw string column for the dimension.</param>
        /// <returns>The dimension slices, ordered highest to lowest count.</returns>
        private List<MetricSliceBag> GetRawUtmBreakdown( IQueryable<Interaction> query, Expression<Func<Interaction, string>> valueSelector )
        {
            return query
                .Select( valueSelector )
                .Where( value => value != null && value != "" )
                .GroupBy( value => value )
                .Select( g => new { Value = g.Key, Count = g.Count() } )
                .ToList()
                // These values are always raw, so run every one through the data-quality check, then
                // cap the row count so a flood of junk values cannot overwhelm the report.
                .Where( x => IsQualityUtmValue( x.Value ) )
                .OrderByDescending( x => x.Count )
                .ThenBy( x => x.Value )
                .Take( MaxUtmRowCount )
                .Select( x => new MetricSliceBag { Label = x.Value, Count = x.Count } )
                .ToList();
        }

        /// <summary>
        /// Applies data-quality heuristics to a raw UTM string to keep scanner and injection junk out of
        /// the report. Only raw/unmapped values are checked; mapped defined values are trusted.
        /// </summary>
        /// <param name="value">The raw UTM string.</param>
        /// <returns><c>true</c> if the value looks like a real tag; otherwise <c>false</c>.</returns>
        private static bool IsQualityUtmValue( string value )
        {
            if ( value.IsNullOrWhiteSpace() )
            {
                return false;
            }

            // Newlines, tabs, null bytes, and other control characters never belong in a UTM tag.
            if ( value.Any( char.IsControl ) )
            {
                return false;
            }

            // SQL comment syntax is a strong injection signal.
            if ( UtmSqlCommentTokens.Any( token => value.IndexOf( token, StringComparison.Ordinal ) >= 0 ) )
            {
                return false;
            }

            // Real tags are mostly letters and digits; a value dominated by symbols is almost always an
            // encoded payload or injection fragment.
            var alphanumericCount = value.Count( char.IsLetterOrDigit );
            if ( alphanumericCount < value.Length / 2 )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the per-day view counts for a single period, emitting a zero point for days
        /// with no views so the chart renders a continuous line across a bounded range.
        /// </summary>
        /// <param name="query">The interaction query, already scoped to the item and period.</param>
        /// <param name="startDateTime">The inclusive start of the period, or <c>null</c> for unbounded; used only to zero-fill the day range.</param>
        /// <param name="endDateTime">The exclusive end of the period, or <c>null</c> for unbounded; used only to zero-fill the day range.</param>
        /// <returns>The per-day view counts ordered by date.</returns>
        private List<ViewsOverTimePointBag> GetViewsOverTime( IQueryable<Interaction> query, DateTime? startDateTime, DateTime? endDateTime )
        {
            var countsByDay = query
                .GroupBy( i => DbFunctions.TruncateTime( i.InteractionDateTime ) )
                .Select( g => new { Day = g.Key, Count = g.Count() } )
                .ToList()
                .Where( x => x.Day.HasValue )
                .ToDictionary( x => x.Day.Value, x => x.Count );

            var points = new List<ViewsOverTimePointBag>();

            // For a bounded range, emit every day so gaps render as zero. The range end is exclusive
            // (midnight of the day after the last displayed day), so step back a tick to get the last
            // day that should actually appear.
            if ( startDateTime.HasValue && endDateTime.HasValue )
            {
                var lastDay = endDateTime.Value.AddTicks( -1 ).Date;

                for ( var day = startDateTime.Value.Date; day <= lastDay; day = day.AddDays( 1 ) )
                {
                    points.Add( new ViewsOverTimePointBag
                    {
                        Date = day.ToString( "yyyy-MM-dd" ),
                        Count = countsByDay.TryGetValue( day, out var count ) ? count : 0
                    } );
                }
            }
            else
            {
                foreach ( var dayCount in countsByDay.OrderBy( x => x.Key ) )
                {
                    points.Add( new ViewsOverTimePointBag
                    {
                        Date = dayCount.Key.ToString( "yyyy-MM-dd" ),
                        Count = dayCount.Value
                    } );
                }
            }

            return points;
        }

        /// <summary>
        /// Gets the view counts for a single period from the supplied interaction query.
        /// </summary>
        /// <param name="query">The interaction query, already scoped to the item and period.</param>
        /// <returns>The period counts.</returns>
        private PeriodCounts GetPeriodCounts( IQueryable<Interaction> query )
        {
            return new PeriodCounts
            {
                // Total views: every logged interaction in the period.
                TotalViews = query.Count(),

                // Unique views: distinct browsing sessions.
                UniqueViews = query.Where( i => i.InteractionSessionId.HasValue )
                    .Select( i => i.InteractionSessionId.Value )
                    .Distinct()
                    .Count(),

                // Known people: distinct identified persons (anonymous interactions excluded).
                KnownPeople = query.Where( i => i.PersonAliasId.HasValue )
                    .Select( i => i.PersonAlias.PersonId )
                    .Distinct()
                    .Count()
            };
        }

        /// <summary>
        /// Calculates the percent change from the previous value to the current value.
        /// </summary>
        /// <param name="currentValue">The current period value.</param>
        /// <param name="previousValue">The previous period value, or <c>null</c> when unavailable.</param>
        /// <returns>The percent change, or <c>null</c> when there is no non-zero baseline.</returns>
        private double? CalculateDeltaPercent( int currentValue, int? previousValue )
        {
            if ( !previousValue.HasValue || previousValue.Value == 0 )
            {
                return null;
            }

            return ( currentValue - previousValue.Value ) / ( double ) previousValue.Value * 100.0;
        }

        /// <summary>
        /// Builds the Viewer Details grid data for the content channel item over the given date range,
        /// one row per known (identified) person who viewed the item, newest last-viewed first.
        /// </summary>
        /// <param name="contentChannelItemId">The content channel item identifier.</param>
        /// <param name="dateRangeDelimited">The delimited sliding date range string, shared with the Overview.</param>
        /// <param name="filters">The Viewer Details grid filters.</param>
        /// <returns>The grid data for the viewer list.</returns>
        private GridDataBag BuildViewerData( int contentChannelItemId, string dateRangeDelimited, ViewerFilterValues filters )
        {
            var dateRange = RockDateTimeHelper.CalculateDateRangeFromDelimitedValues( dateRangeDelimited );

            var query = ApplyPeriodFilter( GetItemInteractionQuery( contentChannelItemId ), dateRange.Start, dateRange.End )
                .Where( i => i.PersonAliasId.HasValue );

            // Original Source filter (any-touch): keep only people who viewed through this source at least
            // once in the period. A source captured as a mapped defined value is matched by its id;
            // otherwise it is matched by the raw string.
            if ( filters.Source.IsNotNullOrWhiteSpace() )
            {
                var sourceValueId = DefinedTypeCache.Get( SystemGuid.DefinedType.UTM_SOURCE.AsGuid() )
                    ?.GetDefinedValueFromValue( filters.Source )?.Id;

                query = sourceValueId.HasValue
                    ? query.Where( i => i.SourceValueId == sourceValueId.Value )
                    : query.Where( i => i.Source == filters.Source );
            }

            // Aggregate the period's interactions to one row per known person: how many times they
            // viewed and when they last viewed.
            var perPerson = query
                .GroupBy( i => i.PersonAlias.PersonId )
                .Select( g => new
                {
                    PersonId = g.Key,
                    Views = g.Count(),
                    LastViewed = g.Max( i => i.InteractionDateTime )
                } )
                .ToList();

            if ( perPerson.Count == 0 )
            {
                return GetViewerGridBuilder().Build( new List<ViewerRow>() );
            }

            var viewerPersonIdQuery = query.Select( i => i.PersonAlias.PersonId );
            var peopleQuery = new PersonService( RockContext ).Queryable()
                .Where( p => viewerPersonIdQuery.Contains( p.Id ) );

            var campusId = filters.CampusGuid.HasValue ? CampusCache.GetId( filters.CampusGuid.Value ) : null;
            if ( campusId.HasValue )
            {
                peopleQuery = peopleQuery.Where( p => p.PrimaryCampusId == campusId.Value );
            }

            var connectionStatusValueId = filters.ConnectionStatusGuid.HasValue ? DefinedValueCache.GetId( filters.ConnectionStatusGuid.Value ) : null;
            if ( connectionStatusValueId.HasValue )
            {
                peopleQuery = peopleQuery.Where( p => p.ConnectionStatusValueId == connectionStatusValueId.Value );
            }

            var people = peopleQuery.ToDictionary( p => p.Id );

            var rows = perPerson
                .Where( p => people.ContainsKey( p.PersonId ) )
                .Select( p =>
                {
                    var person = people[p.PersonId];

                    return new ViewerRow
                    {
                        Person = person,
                        Views = p.Views,
                        LastViewed = p.LastViewed,
                        CampusName = person.PrimaryCampusId.HasValue ? CampusCache.Get( person.PrimaryCampusId.Value )?.Name : null
                    };
                } )
                .OrderByDescending( r => r.LastViewed )
                .ToList();

            return GetViewerGridBuilder().Build( rows );
        }

        /// <summary>
        /// Gets the grid builder that maps a viewer row to the columns shown in the Viewer Details grid.
        /// </summary>
        /// <returns>The viewer grid builder.</returns>
        private GridBuilder<ViewerRow> GetViewerGridBuilder()
        {
            return new GridBuilder<ViewerRow>()
                .WithBlock( this )
                .AddTextField( "idKey", r => r.Person?.IdKey )
                .AddPersonField( "person", r => r.Person )
                .AddTextField( "campus", r => r.CampusName )
                .AddField( "views", r => r.Views )
                .AddDateTimeField( "lastViewed", r => r.LastViewed );
        }

        /// <summary>
        /// Gets the active campuses as options for the Viewer Details campus filter.
        /// </summary>
        /// <returns>The campus filter options, keyed by campus guid.</returns>
        private List<ListItemBag> GetCampusFilterOptions()
        {
            return CampusCache.All()
                .Where( c => c.IsActive != false )
                .OrderBy( c => c.Order )
                .Select( c => new ListItemBag { Value = c.Guid.ToString(), Text = c.Name } )
                .ToList();
        }

        /// <summary>
        /// Gets the connection status defined values as options for the Viewer Details connection status filter.
        /// </summary>
        /// <returns>The connection status filter options, keyed by defined value guid.</returns>
        private List<ListItemBag> GetConnectionStatusFilterOptions()
        {
            var definedType = DefinedTypeCache.Get( SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid() );

            if ( definedType == null )
            {
                return new List<ListItemBag>();
            }

            return definedType.DefinedValues
                .Select( v => new ListItemBag { Value = v.Guid.ToString(), Text = v.Value } )
                .ToList();
        }

        /// <summary>
        /// Gets the distinct sources ever captured for the item as options for the Viewer Details source
        /// filter. Mapped sources resolve to their defined value name; unmapped raw sources are included
        /// only when they pass the data-quality check.
        /// </summary>
        /// <param name="contentChannelItemId">The content channel item identifier.</param>
        /// <returns>The source filter options, keyed by source label.</returns>
        private List<ListItemBag> GetSourceFilterOptions( int contentChannelItemId )
        {
            var rows = GetItemInteractionQuery( contentChannelItemId )
                .Where( i => i.SourceValueId.HasValue || ( i.Source != null && i.Source != "" ) )
                .Select( i => new { i.SourceValueId, i.Source } )
                .Distinct()
                .ToList();

            return rows
                .Where( r => r.SourceValueId.HasValue || IsQualityUtmValue( r.Source ) )
                .Select( r => UtmHelper.GetUtmSourceNameFromDefinedValueOrText( r.SourceValueId, r.Source ) )
                .Where( label => label.IsNotNullOrWhiteSpace() )
                .Distinct()
                .OrderBy( label => label )
                .Select( label => new ListItemBag { Value = label, Text = label } )
                .ToList();
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the Overview KPI metrics for the content channel item. The date range is read from the
        /// block's person preferences, shared with the Viewer Details grid.
        /// </summary>
        /// <returns>A result containing the Overview metrics.</returns>
        [BlockAction]
        public BlockActionResult GetOverviewMetrics()
        {
            var contentChannelItem = GetContentChannelItem();

            if ( contentChannelItem == null )
            {
                return ActionNotFound( "Content channel item not found." );
            }

            var dateRange = GetBlockPersonPreferences().GetValue( PersonPreferenceKey.DateRange );

            return ActionOk( BuildOverviewMetrics( contentChannelItem.Id, dateRange ) );
        }

        /// <summary>
        /// Gets the Viewer Details grid data for the content channel item. The date range and filters are
        /// read from the block's person preferences, which the grid persists before requesting a reload.
        /// </summary>
        /// <returns>A result containing the viewer grid data.</returns>
        [BlockAction]
        public BlockActionResult GetViewerData()
        {
            var contentChannelItem = GetContentChannelItem();

            if ( contentChannelItem == null )
            {
                return ActionNotFound( "Content channel item not found." );
            }

            var preferences = GetBlockPersonPreferences();
            var dateRange = preferences.GetValue( PersonPreferenceKey.DateRange );

            var filters = new ViewerFilterValues
            {
                CampusGuid = preferences.GetValue( PersonPreferenceKey.FilterCampus ).AsGuidOrNull(),
                ConnectionStatusGuid = preferences.GetValue( PersonPreferenceKey.FilterConnectionStatus ).AsGuidOrNull(),
                Source = preferences.GetValue( PersonPreferenceKey.FilterSource )
            };

            return ActionOk( BuildViewerData( contentChannelItem.Id, dateRange, filters ) );
        }

        /// <summary>
        /// Creates an entity set for the subset of selected rows in the Viewer Details grid, used by the
        /// built-in grid actions (Launch Workflow, Merge Template, Export).
        /// </summary>
        /// <param name="entitySet">The entity set data from the grid.</param>
        /// <returns>A result containing the identifier of the created entity set.</returns>
        [BlockAction]
        public BlockActionResult CreateGridEntitySet( GridEntitySetBag entitySet )
        {
            if ( entitySet == null )
            {
                return ActionBadRequest( "No entity set data was provided." );
            }

            var rockEntitySet = GridHelper.CreateEntitySet( entitySet );

            if ( rockEntitySet == null )
            {
                return ActionBadRequest( "No entities were found to create the set." );
            }

            return ActionOk( rockEntitySet.Id.ToString() );
        }

        /// <summary>
        /// Creates a communication for the subset of selected rows in the Viewer Details grid, used by the
        /// built-in grid Communicate action.
        /// </summary>
        /// <param name="communication">The communication data from the grid.</param>
        /// <returns>A result containing the identifier of the created communication.</returns>
        [BlockAction]
        public BlockActionResult CreateGridCommunication( GridCommunicationBag communication )
        {
            if ( communication == null )
            {
                return ActionBadRequest( "No communication data was provided." );
            }

            var rockCommunication = GridHelper.CreateCommunication( communication, RequestContext );

            if ( rockCommunication == null )
            {
                return ActionBadRequest( "Grid has no recipients." );
            }

            return ActionOk( rockCommunication.Id.ToString() );
        }

        #endregion Block Actions

        #region Supporting Classes

        /// <summary>
        /// The view counts for a single period.
        /// </summary>
        private class PeriodCounts
        {
            /// <summary>
            /// Gets or sets the total number of views (interactions).
            /// </summary>
            public int TotalViews { get; set; }

            /// <summary>
            /// Gets or sets the number of distinct browsing sessions.
            /// </summary>
            public int UniqueViews { get; set; }

            /// <summary>
            /// Gets or sets the number of distinct identified people.
            /// </summary>
            public int KnownPeople { get; set; }
        }

        /// <summary>
        /// A single row in the Viewer Details grid: a known person and their view activity for the item.
        /// </summary>
        private class ViewerRow
        {
            /// <summary>
            /// Gets or sets the person who viewed the item.
            /// </summary>
            public Person Person { get; set; }

            /// <summary>
            /// Gets or sets the number of times the person viewed the item in the period.
            /// </summary>
            public int Views { get; set; }

            /// <summary>
            /// Gets or sets the most recent time the person viewed the item in the period.
            /// </summary>
            public DateTime LastViewed { get; set; }

            /// <summary>
            /// Gets or sets the name of the person's primary campus, or <c>null</c> if none.
            /// </summary>
            public string CampusName { get; set; }
        }

        /// <summary>
        /// The Viewer Details grid filter values read from the block's person preferences.
        /// </summary>
        private class ViewerFilterValues
        {
            /// <summary>
            /// Gets or sets the campus guid to filter viewers by, or <c>null</c> for all campuses.
            /// </summary>
            public Guid? CampusGuid { get; set; }

            /// <summary>
            /// Gets or sets the connection status defined value guid to filter viewers by, or <c>null</c> for all.
            /// </summary>
            public Guid? ConnectionStatusGuid { get; set; }

            /// <summary>
            /// Gets or sets the source label to filter viewers by, or <c>null</c>/empty for all sources.
            /// </summary>
            public string Source { get; set; }
        }

        /// <summary>
        /// The captured value of a UTM dimension on an interaction, as either a mapped defined value id or a raw string.
        /// </summary>
        private class UtmValueRow
        {
            /// <summary>
            /// Gets or sets the defined value id when the dimension mapped to a defined value.
            /// </summary>
            public int? ValueId { get; set; }

            /// <summary>
            /// Gets or sets the raw captured string when the dimension did not map to a defined value.
            /// </summary>
            public string RawValue { get; set; }
        }

        #endregion Supporting Classes
    }
}
