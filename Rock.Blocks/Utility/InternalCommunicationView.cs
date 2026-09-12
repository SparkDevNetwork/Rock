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
using System.Runtime.Serialization;
using System.Text;

using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Cms;
using Rock.Lava;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Blocks.Utility.InternalCommunicationView;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Utility
{
    /// <summary>
    /// Block for showing the contents of internal content channels.
    /// </summary>
    [DisplayName( "Internal Communication View" )]
    [Category( "Utility" )]
    [Description( "Block for showing the contents of internal content channels." )]
    [IconCssClass( "ti ti-news" )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [ConfigurationChangedReload( BlockReloadMode.Block )]

    #region Block Attributes

    [CodeEditorField( "Block Title Template",
        Description = "Lava template for determining the title of the block. The 'Item' merge field contains the content channel item being displayed. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 100,
        IsRequired = true,
        DefaultValue = "Staff Updates <small>({{ Item.StartDateTime | Date:'sd' }})</small>",
        Order = 0,
        Key = AttributeKey.BlockTitleTemplate )]

    [TextField( "Block Title Icon CSS Class",
        Description = "The icon CSS class for use in the block title.",
        IsRequired = false,
        DefaultValue = "ti ti-news",
        Order = 1,
        Key = AttributeKey.BlockTitleIconCssClass )]

    [ContentChannelField( "Content Channel",
        Description = "The content channel to display with the template. The content channel must be of type 'Internal Communication Template'.",
        IsRequired = true,
        Order = 2,
        Key = AttributeKey.ContentChannel )]

    [MetricCategoriesField( "Metrics",
        Description = "Select the metrics you would like to display on the page.",
        IsRequired = false,
        Order = 3,
        Key = AttributeKey.Metrics )]

    [IntegerField( "Metric Value Count",
        Description = "The number of metric values to return per metric. You will always get the latest value, but if you would like to return additional values (i.e. to create a chart) you can specify that here.",
        IsRequired = false,
        DefaultIntegerValue = 0,
        Order = 4,
        Key = AttributeKey.MetricValueCount )]

    [CodeEditorField( "Body Template",
        Description = "The Lava template for rendering the body of the block.<br>Available Lava fields include:<ul><li>Item - The content channel item being displayed.</li><li>Metrics - The selected metrics, each with Title, ..., LastValue, LastRunDateTime, and MetricValues (a collection of the requested recent values, each with DateTime, Value, and Note).</li></ul>The standard common merge fields (such as CurrentPerson) are also available for personalization. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",
        EditorMode = CodeEditorMode.Html,
        EditorHeight = 600,
        IsRequired = false,
        DefaultValue = BodyTemplateDefaultValue,
        Order = 5,
        Key = AttributeKey.BodyTemplate )]

    [LavaCommandsField( "Enabled Lava Commands",
        Description = "The Lava commands that should be made available to the block.",
        IsRequired = false,
        Order = 6,
        Key = AttributeKey.EnabledLavaCommands )]

    [IntegerField( "Cache Duration",
        Description = "The time, in seconds, to cache the data for this block. The Lava template will still be run to enable personalization. Only the data for the block will be cached.",
        IsRequired = false,
        DefaultIntegerValue = 3600,
        Order = 7,
        Key = AttributeKey.CacheDuration )]

    [CustomCheckboxListField( "Cache Tags",
        Description = "Cache tags are used to link cached content so that it can be expired as a group.",
        ListSource = CacheTagListSource,
        IsRequired = false,
        Order = 10,
        Key = AttributeKey.CacheTags )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "83653A46-63E9-48C7-918E-44C9B1769308" )]
    // TODO: Will be [Rock.SystemGuid.BlockTypeGuid( "D526F4A5-19B9-410F-A663-400D93C61D3C" )]
    [Rock.SystemGuid.BlockTypeGuid( "7B886434-EE73-441F-9AFD-BC196F1B5733" )]
    public class InternalCommunicationView : RockBlockType
    {
        #region Keys and Constants

        private static class AttributeKey
        {
            public const string BlockTitleTemplate = "BlockTitleTemplate";
            public const string BlockTitleIconCssClass = "BlockTitleIconCssClass";
            public const string ContentChannel = "ContentChannel";
            public const string Metrics = "Metrics";
            public const string MetricValueCount = "MetricValueCount";
            public const string BodyTemplate = "BodyTemplate";
            public const string EnabledLavaCommands = "EnabledLavaCommands";
            public const string CacheDuration = "CacheDuration";
            public const string CacheTags = "CacheTags";
        }

        /// <summary>
        /// Alert types used for the notification message shown by the block. These must match the
        /// values of the Obsidian <c>AlertType</c> enum so the client can bind them directly.
        /// </summary>
        private static class MessageType
        {
            public const string Info = "info";
            public const string Validation = "validation";
        }

        /// <summary>
        /// SQL used to populate the "Cache Tags" checkbox list from the Cache Tags defined type.
        /// </summary>
        private const string CacheTagListSource = @"
            SELECT CAST([DefinedValue].[Value] AS VARCHAR) AS [Value], [DefinedValue].[Value] AS [Text]
            FROM [DefinedType]
            JOIN [DefinedValue] ON [DefinedType].[Id] = [DefinedValue].[DefinedTypeId]
            WHERE [DefinedType].[Guid] = 'BDF73089-9154-40C1-90E4-74518E9937DC'";

        #endregion Keys and Constants

        #region Attribute Default Values

        /// <summary>
        /// The default Lava template used to render the body of the block. This matches the template
        /// that ships with the block's current registration so a freshly added block renders a
        /// complete "staff updates" layout out of the box.
        /// </summary>
        private const string BodyTemplateDefaultValue = @"{% stylesheet id:'home-feature' %}

    .feature-image {
        width: 100%;
        height: 450px;
        background-repeat: no-repeat;
        background-size: cover;
        background-position: center;
    }


    .communicationview h1 {
        font-size: 28px;
        margin-top: 12px;
    }

    .homepage-article .photo {
        width: 100%;
        height: 140px;
        background-repeat: no-repeat;
        background-size: cover;
        background-position: center;
    }

    {% endstylesheet %}

    <div class=""communicationview"">
        {% assign featureLink = Item | Attribute:'FeatureLink','RawValue' %}

        <div class=""feature"">

            <div class=""feature-image"" style="" background-image: url('/GetImage.ashx?Guid={{ Item | Attribute:'FeatureImage','RawValue' }}&w=2400&h=2400');""></div>
            <h1 class=""feature-title"">{{ Item | Attribute:'FeatureTitle' }}</h1>
            <p>
                {{ Item | Attribute:'FeatureText' }}
            </p>

            {% if featureLink != empty -%}
                <a class=""btn btn-xs btn-link p-0"" href=""{{ featureLink | Remove:'https://' | Remove:'http://' | Prepend:'https://' }}"">More Info</a>
            {% endif -%}
        </div>

        <hr class=""margin-v-lg"" />

        <div class=""margin-b-lg"">
            {{ Item | Attribute:'Articles' }}
        </div>

        {% assign metricCount = Metrics | Size -%}
        {% if metricCount > 0 -%}
            <h1>Metrics</h1>
            {[kpis columncount:'3' size:'lg' ]}
                {% for metric in Metrics %}
                    [[ kpi icon:'{{ metric.IconCssClass  }}' value:'{{ metric.LastValue | AsInteger | Format:'N0' }}' label:'{{ metric.Title }}' secondarylabel:'{{ metric.LastRunDateTime | Date:'sd' }}' color:'#c0c0c1']][[ endkpi ]]
                {% endfor %}
            {[endkpis]}
        {% endif %}

    </div>";

        #endregion Attribute Default Values

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return new InternalCommunicationViewInitializationBox
            {
                Content = GetContentBag( 0 )
            };
        }

        /// <summary>
        /// Builds the rendered content for the requested page. Page 0 is the most recent item; each
        /// higher page index steps one item further back in time.
        /// </summary>
        /// <param name="currentPage">The zero-based page offset to render.</param>
        /// <returns>The resolved content for the requested page.</returns>
        private InternalCommunicationViewContentBag GetContentBag( int currentPage )
        {
            // Verify that the required settings have been configured before attempting to render.
            var missingConfiguration = new List<string>();

            var contentChannelGuid = GetAttributeValue( AttributeKey.ContentChannel ).AsGuidOrNull();
            if ( !contentChannelGuid.HasValue )
            {
                missingConfiguration.Add( "The content channel has not yet been configured." );
            }

            var blockTitleTemplate = GetAttributeValue( AttributeKey.BlockTitleTemplate );
            if ( blockTitleTemplate.IsNullOrWhiteSpace() )
            {
                missingConfiguration.Add( "The block title template appears to be blank." );
            }

            var bodyTemplate = GetAttributeValue( AttributeKey.BodyTemplate );
            if ( bodyTemplate.IsNullOrWhiteSpace() )
            {
                missingConfiguration.Add( "The body template appears to be blank." );
            }

            if ( missingConfiguration.Count > 0 )
            {
                var message = new StringBuilder();
                message.Append( "Currently, there are some missing configuration items. These items are summarized below: <ul>" );

                foreach ( var configurationItem in missingConfiguration )
                {
                    message.Append( $"<li>{configurationItem}</li>" );
                }

                message.Append( "</ul>" );

                return new InternalCommunicationViewContentBag
                {
                    Message = message.ToString(),
                    MessageType = MessageType.Validation,
                    PageIndex = currentPage
                };
            }

            var enabledLavaCommands = GetAttributeValue( AttributeKey.EnabledLavaCommands );
            var blockTitleIconCssClass = GetAttributeValue( AttributeKey.BlockTitleIconCssClass );
            var metricValueCount = GetAttributeValue( AttributeKey.MetricValueCount ).AsInteger();
            var cacheDuration = GetAttributeValue( AttributeKey.CacheDuration ).AsInteger();
            var cacheTags = GetAttributeValue( AttributeKey.CacheTags ) ?? string.Empty;

            ContentChannelItem contentChannelItem = null;
            List<MetricInfo> metrics = null;
            var showPrevious = false;

            // Only the most recent page is cached; paging backwards always queries fresh.
            CachedBlockData cachedItem = null;
            if ( cacheDuration > 0 && currentPage == 0 )
            {
                var serializedCachedItem = RockCache.Get( GetCacheKey(), true ) as string;
                if ( serializedCachedItem.IsNotNullOrWhiteSpace() )
                {
                    cachedItem = serializedCachedItem.FromJsonOrNull<CachedBlockData>();
                }
            }

            if ( cachedItem != null )
            {
                contentChannelItem = cachedItem.ContentChannelItem;
                metrics = cachedItem.Metrics;
                showPrevious = cachedItem.ShowPrevious;
            }
            else
            {
                var channel = ContentChannelCache.Get( contentChannelGuid.Value );

                // Get the latest content channel items. Take two so we know whether an older item
                // exists for the "Previous" paging control.
                var contentChannelItemsQry = new ContentChannelItemService( RockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( i => i.ContentChannel.Guid == contentChannelGuid
                        && i.Status == ContentChannelItemStatus.Approved
                        && i.StartDateTime <= RockDateTime.Now );

                if ( channel.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange )
                {
                    if ( channel.ContentChannelType.IncludeTime )
                    {
                        contentChannelItemsQry = contentChannelItemsQry.Where( c => !c.ExpireDateTime.HasValue || c.ExpireDateTime >= RockDateTime.Now );
                    }
                    else
                    {
                        contentChannelItemsQry = contentChannelItemsQry.Where( c => !c.ExpireDateTime.HasValue || c.ExpireDateTime > RockDateTime.Today );
                    }
                }

                var contentChannelItems = contentChannelItemsQry.OrderByDescending( i => i.StartDateTime )
                    .Skip( currentPage )
                    .Take( 2 )
                    .ToList();

                if ( contentChannelItems.Count == 0 )
                {
                    return new InternalCommunicationViewContentBag
                    {
                        Message = "It appears that there are no active communications to display for this content channel.",
                        MessageType = MessageType.Info,
                        PageIndex = currentPage
                    };
                }

                contentChannelItem = contentChannelItems.First();
                showPrevious = contentChannelItems.Count > 1;

                metrics = GetMetrics( metricValueCount );

                // Cache the resolved data for the most recent page so subsequent renders skip the
                // queries. The Lava templates are still run on every render to allow personalization.
                if ( cacheDuration > 0 && currentPage == 0 )
                {
                    var cachedData = new CachedBlockData
                    {
                        ContentChannelItem = contentChannelItem.Clone( false ),
                        ShowPrevious = showPrevious,
                        Metrics = metrics
                    };

                    var expiration = RockDateTime.Now.AddSeconds( cacheDuration );
                    RockCache.AddOrUpdate( GetCacheKey(), string.Empty, cachedData.ToJson(), expiration, cacheTags );
                }
            }

            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields["Item"] = contentChannelItem;
            mergeFields["Metrics"] = metrics;

            var titleHtml = $"<i class='{blockTitleIconCssClass}'></i> {blockTitleTemplate.ResolveMergeFields( mergeFields, enabledLavaCommands )}";
            var bodyHtml = bodyTemplate.ResolveMergeFields( mergeFields, enabledLavaCommands );

            return new InternalCommunicationViewContentBag
            {
                TitleHtml = titleHtml,
                BodyHtml = bodyHtml,

                // An older item exists to page back to.
                ShowPrevious = showPrevious,

                // A newer item exists to page forward to whenever we are not already on the most recent page.
                ShowNext = currentPage > 0,
                PageIndex = currentPage
            };
        }

        /// <summary>
        /// Gets the selected metrics along with their latest values. Partitioned metrics are summed
        /// across their partitions for the most recent run.
        /// </summary>
        /// <param name="metricValueCount">The number of recent metric values to include per metric, or zero for none.</param>
        /// <returns>The list of resolved metrics.</returns>
        private List<MetricInfo> GetMetrics( int metricValueCount )
        {
            var metricCategories = MetricCategoriesFieldAttribute.GetValueAsGuidPairs( GetAttributeValue( AttributeKey.Metrics ) );
            var metricGuids = metricCategories.Select( a => a.MetricGuid ).ToList();

            // Determine which of the selected metrics are partitioned so they can be summed separately.
            var partitionedMetricGuids = new MetricValuePartitionService( RockContext ).Queryable()
                .Where( mvp => metricGuids.Contains( mvp.MetricValue.Metric.Guid ) && mvp.EntityId.HasValue )
                .Select( mvp => mvp.MetricValue.Metric.Guid )
                .Distinct()
                .ToList();
            var nonPartitionedMetricGuids = metricGuids.Except( partitionedMetricGuids ).ToList();
            var metricService = new MetricService( RockContext );

            var metrics = metricService.GetByGuids( nonPartitionedMetricGuids )
                .Select( m => new MetricInfo
                {
                    Id = m.Id,
                    Title = m.Title,
                    Description = m.Description,
                    IconCssClass = m.IconCssClass,
                    UnitsLabel = m.YAxisLabel,
                    LastRunDateTime = m.MetricValues.OrderByDescending( v => v.MetricValueDateTime ).Select( v => v.MetricValueDateTime ).FirstOrDefault(),
                    LastValue = m.MetricValues.OrderByDescending( v => v.MetricValueDateTime ).Select( v => v.YValue ).FirstOrDefault()
                } ).ToList();

            // For partitioned metrics, sum the values from the most recent run.
            foreach ( var metricGuid in partitionedMetricGuids )
            {
                var lastRunDateTime = metricService.GetSelect( metricGuid, m => m.LastRunDateTime )?.Date;
                MetricInfo metricResult;

                if ( lastRunDateTime.HasValue )
                {
                    metricResult = metricService.Queryable()
                        .Where( m => m.Guid == metricGuid )
                        .Select( m => new MetricInfo
                        {
                            Id = m.Id,
                            Title = m.Title,
                            Description = m.Description,
                            IconCssClass = m.IconCssClass,
                            UnitsLabel = m.YAxisLabel,
                            LastRunDateTime = m.MetricValues.OrderByDescending( v => v.MetricValueDateTime ).Select( v => v.MetricValueDateTime ).FirstOrDefault(),
                            LastValue = m.MetricValues.Where( v => DbFunctions.TruncateTime( v.MetricValueDateTime ) == lastRunDateTime.Value ).Sum( v => v.YValue )
                        } ).FirstOrDefault();
                }
                else
                {
                    metricResult = metricService.Queryable()
                        .Where( m => m.Guid == metricGuid )
                        .Select( m => new MetricInfo
                        {
                            Id = m.Id,
                            Title = m.Title,
                            Description = m.Description,
                            IconCssClass = m.IconCssClass,
                            UnitsLabel = m.YAxisLabel,
                            LastRunDateTime = m.MetricValues.OrderByDescending( v => v.MetricValueDateTime ).Select( v => v.MetricValueDateTime ).FirstOrDefault(),
                            LastValue = m.MetricValues.OrderByDescending( v => v.MetricValueDateTime ).Select( v => v.YValue ).FirstOrDefault()
                        } ).FirstOrDefault();
                }

                if ( metricResult != null )
                {
                    metrics.Add( metricResult );
                }
            }

            // Optionally include a set of recent values for each metric (e.g. to build a chart).
            if ( metricValueCount > 0 )
            {
                var metricValueService = new MetricValueService( RockContext );

                foreach ( var metric in metrics )
                {
                    metric.MetricValues = metricValueService.Queryable()
                        .Where( v => v.MetricId == metric.Id )
                        .OrderByDescending( v => v.MetricValueDateTime )
                        .Select( v => new MetricValueInfo
                        {
                            DateTime = v.MetricValueDateTime,
                            Value = v.YValue,
                            Note = v.Note
                        } )
                        .Take( metricValueCount )
                        .ToList();
                }
            }

            return metrics;
        }

        /// <summary>
        /// Gets the cache key used to store this block instance's resolved data.
        /// </summary>
        /// <returns>The cache key for this block instance.</returns>
        private string GetCacheKey()
        {
            return $"Rock:InternalCommunicationView:Block:{BlockId}:Data";
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the rendered content for the requested page. Called by the paging controls.
        /// </summary>
        /// <param name="pageIndex">The zero-based page offset to render.</param>
        /// <returns>The resolved content for the requested page.</returns>
        [BlockAction]
        public BlockActionResult GetContent( int pageIndex )
        {
            if ( pageIndex < 0 )
            {
                pageIndex = 0;
            }

            return ActionOk( GetContentBag( pageIndex ) );
        }

        #endregion Block Actions

        #region Support Classes

        /// <summary>
        /// Holds the block data that is cached between renders.
        /// </summary>
        [Serializable]
        [DataContract]
        private class CachedBlockData
        {
            /// <summary>
            /// Gets or sets the metrics.
            /// </summary>
            [DataMember]
            public List<MetricInfo> Metrics { get; set; }

            /// <summary>
            /// Gets or sets the content channel item.
            /// </summary>
            [DataMember]
            public ContentChannelItem ContentChannelItem { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether an older item exists to page back to.
            /// </summary>
            [DataMember]
            public bool ShowPrevious { get; set; }
        }

        /*
            9/11/26 - NA

            The Lava data objects below derive from RockDynamic rather than the now-preferred
            LavaDataObject. LavaDataObject implements IDictionary and is documented to always
            serialize/deserialize as a dictionary; its IDictionary.Add throws ("read-only"), so
            JSON deserialization of a cached metric would fail and FromJsonOrNull would silently
            return null, disabling the block's data cache whenever metrics are configured.
            RockDynamic round-trips its declared members through JSON, which the cache requires.

            Reason: LavaDataObject breaks the JSON cache round-trip; RockDynamic is required here.
        */

        /// <summary>
        /// A metric and its most recent value, made available to Lava.
        /// </summary>
        [Serializable]
        [DataContract]
        private class MetricInfo : RockDynamic
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            [DataMember]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the title.
            /// </summary>
            [DataMember]
            public string Title { get; set; }

            /// <summary>
            /// Gets or sets the description.
            /// </summary>
            [DataMember]
            public string Description { get; set; }

            /// <summary>
            /// Gets or sets the units label.
            /// </summary>
            [DataMember]
            public string UnitsLabel { get; set; }

            /// <summary>
            /// Gets or sets the icon CSS class.
            /// </summary>
            [DataMember]
            public string IconCssClass { get; set; }

            /// <summary>
            /// Gets or sets the last run date time.
            /// </summary>
            [DataMember]
            public DateTime? LastRunDateTime { get; set; }

            /// <summary>
            /// Gets or sets the last value.
            /// </summary>
            [DataMember]
            public decimal? LastValue { get; set; }

            /// <summary>
            /// Gets or sets the metric values.
            /// </summary>
            [DataMember]
            public List<MetricValueInfo> MetricValues { get; set; }
        }

        /// <summary>
        /// A single metric value, made available to Lava.
        /// </summary>
        [Serializable]
        [DataContract]
        private class MetricValueInfo : RockDynamic
        {
            /// <summary>
            /// Gets or sets the date time.
            /// </summary>
            [DataMember]
            public DateTime? DateTime { get; set; }

            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            [DataMember]
            public decimal? Value { get; set; }

            /// <summary>
            /// Gets or sets the note.
            /// </summary>
            [DataMember]
            public string Note { get; set; }
        }

        #endregion Support Classes
    }
}
