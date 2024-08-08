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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Data.Entity;
using System.Text;
using System.Runtime.Serialization;
using Rock.Utility;
using Rock.Lava;

namespace RockWeb.Blocks.Utility
{
    [DisplayName( "Internal Communication View" )]
    [Category( "Utility" )]
    [Description( "Block for showing the contents of internal content channels." )]
    [CodeEditorField( "Block Title Template", "Lava template for determining the title of the block.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 100, true, "Staff Updates <small>({{ Item.StartDateTime | Date:'sd' }})</small>", order: 0 )]
    [TextField( "Block Title Icon CSS Class", "The icon CSS class for use in the block title.", false, "fa fa-newspaper", order: 1, key: "BlockTitleIconCssClass" )]
    [ContentChannelField( "Content Channel", "The content channel to display with the template. The contant channel must be of type 'Internal Communication Template'.", true, "", order: 2 )]
    [MetricCategoriesField( "Metrics", "Select the metrics you would like to display on the page.", false, "", order: 3 )]
    [IntegerField( "Metric Value Count", "The number of metric values to return per metric. You will always get the lastest value, but if you would like to return additional values (i.e. to create a chart) you can specify that here.", false, 0, order: 4 )]
    [CodeEditorField( "Body Template", "The Lava template for rendering the body of the block.", CodeEditorMode.Less, CodeEditorTheme.Rock, 600, true, "d", order: 5 )]
    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be made available to the block.", false, order: 6 )]
    [IntegerField( "Cache Duration", "The time, in seconds, to cache the data for this block. The Lava template will still be run to enable personalization. Only the data for the block will be cached.", false, 3600, order: 7 )]
    [CustomCheckboxListField( "Cache Tags", "Cached tags are used to link cached content so that it can be expired as a group", CACHE_TAG_LIST, false, key: "CacheTags", order: 10 )]

    [Rock.SystemGuid.BlockTypeGuid( "D526F4A5-19B9-410F-A663-400D93C61D3C" )]
    public partial class InternalCommunicationView : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables
        private int _currentPage = 0;

        private const string CACHE_TAG_LIST = @"
            SELECT CAST([DefinedValue].[Value] AS VARCHAR) AS [Value], [DefinedValue].[Value] AS [Text]
            FROM[DefinedType]
            JOIN[DefinedValue] ON[DefinedType].[Id] = [DefinedValue].[DefinedTypeId]
            WHERE[DefinedType].[Guid] = 'BDF73089-9154-40C1-90E4-74518E9937DC'";
        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ShowView();
            }
            else
            {
                _currentPage = hfCurrentPage.Value.AsInteger();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowView();
        }

        /// <summary>
        /// Handles the Click event of the btnPrevious control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnPrevious_Click( object sender, EventArgs e )
        {
            _currentPage = hfCurrentPage.Value.AsInteger() + 1;
            ShowView();
        }

        /// <summary>
        /// Handles the Click event of the btnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnNext_Click( object sender, EventArgs e )
        {
            _currentPage = hfCurrentPage.Value.AsInteger();

            if ( _currentPage > 0 )
            {
                _currentPage--;
            }

            ShowView();
        }

        #endregion

        #region Methods

        private void ShowView()
        {
            var rockContext = new RockContext();

            var missingConfiguration = new List<string>();

            var contentChannelGuid = GetAttributeValue( "ContentChannel" ).AsGuidOrNull();
            if ( !contentChannelGuid.HasValue )
            {
                missingConfiguration.Add( "The content channel has not yet been configured." );
            }

            var blockTitleTemplate = GetAttributeValue( "BlockTitleTemplate" );
            if ( blockTitleTemplate.IsNullOrWhiteSpace() )
            {
                missingConfiguration.Add( "The block title template appears to be blank." );
            }

            var bodyTemplate = GetAttributeValue( "BodyTemplate" );
            if ( bodyTemplate.IsNullOrWhiteSpace() )
            {
                missingConfiguration.Add( "The body template appears to be blank." );
            }

            if ( missingConfiguration.Count > 0 )
            {
                StringBuilder message = new StringBuilder();
                message.Append( "Currently, there are some missing configuration items. These items are summarized below: <ul>" );

                foreach ( var configurationItem in missingConfiguration )
                {
                    message.Append( string.Format( "<li>{0}</li>", configurationItem ) );
                }

                message.Append( "</ul>" );

                nbMessages.Text = message.ToString();
                nbMessages.NotificationBoxType = NotificationBoxType.Validation;
                return;
            }

            var enabledLavaCommands = GetAttributeValue( "EnabledLavaCommands" );
            var blockTitleIconCssClass = GetAttributeValue( "BlockTitleIconCssClass" );
            var metricValueCount = GetAttributeValue( "MetricValueCount" ).AsInteger();
            var cacheDuration = GetAttributeValue( "CacheDuration" ).AsInteger();
            string cacheTags = GetAttributeValue( "CacheTags" ) ?? string.Empty;

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson );

            var cacheKey = "internal-commmunication-view-" + this.BlockId.ToString();

            ContentChannelItem contentChannelItem = null;
            List<MetricResult> metrics = null;
            var showPrev = false;

            CachedBlockData cachedItem = null;

            if ( cacheDuration > 0 && _currentPage == 0 )
            {
                var serializedCachedItem = RockCache.Get( cacheKey, true );
                if ( serializedCachedItem != null
                    && serializedCachedItem is string
                    && !string.IsNullOrWhiteSpace( ( string ) serializedCachedItem ) )
                {
                        cachedItem = ( ( string ) serializedCachedItem ).FromJsonOrNull<CachedBlockData>();
                }
            }

            if ( cachedItem != null )
            {
                contentChannelItem = cachedItem.ContentChannelItem;
                metrics = cachedItem.Metrics;
                showPrev = cachedItem.ShowPrev;
            }
            else
            {
                var channel = ContentChannelCache.Get( contentChannelGuid.Value );

                // Get latest content channel items, get two so we know if a previous one exists for paging
                var contentChannelItemsQry = new ContentChannelItemService( rockContext )
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
                    .Take( 2 )
                    .Skip( _currentPage )
                    .ToList();

                if ( contentChannelItems == null || contentChannelItems.Count == 0 )
                {
                    nbMessages.Text = "It appears that there are no active communications to display for this content channel.";
                    nbMessages.NotificationBoxType = NotificationBoxType.Info;
                    return;
                }

                contentChannelItem = contentChannelItems.First();
                showPrev = ( contentChannelItems.Count > 1 );

                // Get metrics
                var metricCategories = Rock.Attribute.MetricCategoriesFieldAttribute.GetValueAsGuidPairs( GetAttributeValue( "Metrics" ) );

                var metricGuids = metricCategories.Select( a => a.MetricGuid ).ToList();
                metrics = new MetricService( rockContext ).GetByGuids( metricGuids )
                                .Select( m => new MetricResult
                                {
                                    Id = m.Id,
                                    Title = m.Title,
                                    Description = m.Description,
                                    IconCssClass = m.IconCssClass,
                                    UnitsLabel = m.YAxisLabel,
                                    LastRunDateTime = m.MetricValues.OrderByDescending( v => v.MetricValueDateTime ).Select( v => v.MetricValueDateTime ).FirstOrDefault(),
                                    LastValue = m.MetricValues.OrderByDescending( v => v.MetricValueDateTime ).Select( v => v.YValue ).FirstOrDefault()
                                } ).ToList();

                // Get metric values for each metric if requested 
                if ( metricValueCount > 0 )
                {
                    foreach ( var metric in metrics )
                    {
                        metric.MetricValues = new MetricValueService( rockContext ).Queryable()
                                                .Where( v => v.MetricId == metric.Id )
                                                .OrderByDescending( v => v.MetricValueDateTime )
                                                .Select( v => new MetricValue
                                                {
                                                    DateTime = v.MetricValueDateTime,
                                                    Value = v.YValue,
                                                    Note = v.Note
                                                } )
                                                .Take( metricValueCount )
                                                .ToList();
                    }
                }

                // Set Cache
                if ( cacheDuration > 0 && _currentPage == 0 )
                {
                    var cachedData = new CachedBlockData();
                    cachedData.ContentChannelItem = contentChannelItem.Clone( false );
                    cachedData.ShowPrev = showPrev;
                    cachedData.Metrics = metrics;

                    var expiration = RockDateTime.Now.AddSeconds( cacheDuration );
                    RockCache.AddOrUpdate( cacheKey, string.Empty, cachedData.ToJson(), expiration, cacheTags );
                }
            }

            mergeFields.Add( "Item", contentChannelItem );
            mergeFields.Add( "Metrics", metrics );

            lBlockTitleIcon.Text = string.Format( "<i class='{0}'></i>", blockTitleIconCssClass );
            lBlockTitle.Text = blockTitleTemplate.ResolveMergeFields( mergeFields, enabledLavaCommands );

            lBlockBody.Text = bodyTemplate.ResolveMergeFields( mergeFields, enabledLavaCommands );

            // Determine if we can page backwards
            btnPrevious.Visible = showPrev;

            // Determine if we can page forwards
            btnNext.Visible = ( _currentPage > 0 );

            // Set the current page hidden field
            hfCurrentPage.Value = _currentPage.ToString();
        }

        #endregion

        #region POCO

        /// <summary>
        /// Class to hold data for caching
        /// </summary>
		[Serializable]
        [DataContract]
        protected class CachedBlockData
        {
            /// <summary>
            /// Gets or sets the metrics.
            /// </summary>
            /// <value>
            /// The metrics.
            /// </value>
            [DataMember]
            public List<MetricResult> Metrics { get; set; }
            /// <summary>
            /// Gets or sets the content channel item.
            /// </summary>
            /// <value>
            /// The content channel item.
            /// </value>
            [DataMember]
            public ContentChannelItem ContentChannelItem { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether [show previous].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [show previous]; otherwise, <c>false</c>.
            /// </value>
            [DataMember]
            public bool ShowPrev { get; set; }
        }

        /// <summary>
        /// Result class for metrics
        /// </summary>
		[Serializable]
        [DataContract]
        protected class MetricResult : RockDynamic
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            [DataMember]
            public int Id { get; set; }
            /// <summary>
            /// Gets or sets the title.
            /// </summary>
            /// <value>
            /// The title.
            /// </value>
            [DataMember]
            public string Title { get; set; }
            /// <summary>
            /// Gets or sets the description.
            /// </summary>
            /// <value>
            /// The description.
            /// </value>
            [DataMember]
            public string Description { get; set; }
            /// <summary>
            /// Gets or sets the units label.
            /// </summary>
            /// <value>
            /// The units label.
            /// </value>
            [DataMember]
            public string UnitsLabel { get; set; }
            /// <summary>
            /// Gets or sets the icon CSS class.
            /// </summary>
            /// <value>
            /// The icon CSS class.
            /// </value>
            [DataMember]
            public string IconCssClass { get; set; }
            /// <summary>
            /// Gets or sets the last run date time.
            /// </summary>
            /// <value>
            /// The last run date time.
            /// </value>
            [DataMember]
            public DateTime? LastRunDateTime { get; set; }
            /// <summary>
            /// Gets or sets the last value.
            /// </summary>
            /// <value>
            /// The last value.
            /// </value>
            [DataMember]
            public decimal? LastValue { get; set; }
            /// <summary>
            /// Gets or sets the metric values.
            /// </summary>
            /// <value>
            /// The metric values.
            /// </value>
            [DataMember]
            public List<MetricValue> MetricValues { get; set; }
        }

        /// <summary>
        /// Result class for metric values
        /// </summary>
		[Serializable]
        [DataContract]
        protected class MetricValue : RockDynamic
        {
            /// <summary>
            /// Gets or sets the date time.
            /// </summary>
            /// <value>
            /// The date time.
            /// </value>
            [DataMember]
            public DateTime? DateTime { get; set; }
            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            [DataMember]
            public decimal? Value { get; set; }
            /// <summary>
            /// Gets or sets the note.
            /// </summary>
            /// <value>
            /// The note.
            /// </value>
            [DataMember]
            public string Note { get; set; }
        }
        #endregion
    }
}