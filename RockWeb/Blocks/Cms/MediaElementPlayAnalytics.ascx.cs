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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Blocks;
using Rock.Data;
using Rock.Media;
using Rock.Model;
using Rock.Net;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// Shows detailed analytics of a Media Element.
    /// </summary>
    [DisplayName( "Media Element Play Analytics" )]
    [Category( "CMS" )]
    [Description( "Shows detailed analytics of a Media Element" )]

    public partial class MediaElementPlayAnalytics : RockBlock, IDetailBlock, IRockBlockType
    {
        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string MediaElementId = "MediaElementId";
        }

        #endregion PageParameterKey

        #region IRockBlockType implementation

        /// <inheritdoc/>
        BlockCache IRockBlockType.BlockCache
        {
            get
            {
                return BlockCache;
            }

            set
            {
                SetBlock( PageCache, value, false, false );
            }
        }

        /// <inheritdoc/>
        PageCache IRockBlockType.PageCache
        {
            get
            {
                return PageCache;
            }
            set
            {
                SetBlock( value, BlockCache, false, false );
            }
        }

        /// <inheritdoc/>
        RockRequestContext IRockBlockType.RequestContext { get; set; }

        /// <inheritdoc/>
        string IRockBlockType.GetControlMarkup()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/Chartjs/Chart.min.js" );
            RockPage.AddScriptLink( "~/Scripts/moment.min.js" );
            RockPage.AddScriptLink( "~/Scripts/Rock/Controls/MediaElementDetail/mediaElementPlayAnalytics.js" );

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
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( PageParameterKey.MediaElementId ).AsInteger() );
            }
        }

        /// <summary>
        /// Gets the bread crumbs.
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? mediaElementId = PageParameter( pageReference, PageParameterKey.MediaElementId ).AsIntegerOrNull();
            if ( mediaElementId != null )
            {
                var rockContext = new RockContext();

                var mediaElement = new MediaElementService( rockContext ).Get( mediaElementId.Value );

                if ( mediaElement != null )
                {
                    breadCrumbs.Add( new BreadCrumb( mediaElement.Name, pageReference ) );
                }
            }

            return breadCrumbs;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Registers the startup script.
        /// </summary>
        /// <param name="mediaElement">The media element.</param>
        private void RegisterStartupScript( MediaElement mediaElement )
        {
            Rock.Web.UI.Controls.MediaPlayer.AddLinksForMediaToPage( mediaElement.DefaultFileUrl, Page );

            var script = string.Format( @"Sys.Application.add_load(function () {{
    const playerOptions = {{
        mediaUrl: '{6}',
        posterUrl: '{7}',
        type: 'video',
        trackProgress: false,
        controls: '',
        clickToPlay: false,
    }};

    let player = null;

    if (playerOptions.mediaUrl !== '') {{
        player = new Rock.UI.MediaPlayer('#{8}', playerOptions);
    }}

    Rock.controls.mediaElementPlayAnalytics.initialize({{
        chartId: '{0}',
        mediaElementGuid: '{1}',
        blockGuid: '{2}',
        averageWatchEngagementId: '{3}',
        totalPlaysId: '{4}',
        minutesWatchedId: '{5}',
        individualPlaysId: '{9}',
        player: player
    }});
}});",
                cChart.ClientID, // {0}
                mediaElement.Guid, // {1}
                BlockCache.Guid, // {2}
                pnlAverageWatchEngagement.ClientID, // {3}
                pnlTotalPlays.ClientID, // {4}
                pnlMinutesWatched.ClientID, // {5}
                mediaElement.DefaultFileUrl, // {6}
                mediaElement.DefaultThumbnailUrl, // {7}
                pnlMediaPlayer.ClientID, // {8}
                pnlIndividualPlays.ClientID // {9}
                );

            RockPage.ClientScript.RegisterStartupScript( GetType(), "startup-script", script, true );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="mediaElementId">The media element identifier.</param>
        public void ShowDetail( int mediaElementId )
        {
            var rockContext = new RockContext();
            var mediaElementService = new MediaElementService( rockContext );
            MediaElement mediaElement = null;

            if ( mediaElementId != 0 )
            {
                mediaElement = mediaElementService.Get( mediaElementId );
            }

            if ( mediaElement == null )
            {
                pnlView.Visible = false;
                return;
            }

            pnlView.Visible = true;

            lActionTitle.Text = mediaElement.Name.FormatAsHtmlTitle();
            mpMedia.MediaElementId = mediaElement.Id;

            RegisterStartupScript( mediaElement );
        }

        /// <summary>
        /// Gets all of the interactions that have watch map data for the media element.
        /// </summary>
        /// <param name="mediaElement">The media element.</param>
        /// <param name="newerThanDate">The optional date that interactions must be newer than.</param>
        /// <param name="rockContext">The rock context to load from.</param>
        /// <returns>A list of interaction data.</returns>
        private static List<InteractionData> GetInteractions( MediaElement mediaElement, DateTime? newerThanDate, RockContext rockContext )
        {
            var interactionChannelId = InteractionChannelCache.Get( Rock.SystemGuid.InteractionChannel.MEDIA_EVENTS ).Id;

            // Get all the interactions for this media element and then pull
            // in just the columns we need for performance.
            var interactionData = new InteractionService( rockContext ).Queryable()
                .Where( i => i.InteractionComponent.InteractionChannelId == interactionChannelId
                    && ( !newerThanDate.HasValue || i.InteractionDateTime > newerThanDate.Value )
                    && i.InteractionComponent.EntityId == mediaElement.Id
                    && i.Operation == "WATCH" )
                .Select( i => new
                {
                    i.Id,
                    i.InteractionDateTime,
                    i.InteractionData
                } )
                .ToList();

            // Do some post-processing on the data to parse the watch map and
            // filter out any that had invalid watch maps.
            return interactionData
                .Select( i => new InteractionData
                {
                    InteractionDateTime = i.InteractionDateTime,
                    WatchMap = i.InteractionData.FromJsonOrNull<WatchMapData>()
                } )
                .Where( i => i.WatchMap != null && i.WatchMap.WatchedPercentage > 0 )
                .ToList();
        }

        /// <summary>
        /// Gets the video overlay data. This is the data that
        /// will be used to draw our chart information over top of the video.
        /// </summary>
        /// <param name="mediaElement">The media element.</param>
        /// <param name="interactions">The interactions.</param>
        /// <returns>The data.</returns>
        private static object GetVideoData( MediaElement mediaElement, List<InteractionData> interactions )
        {
            var duration = mediaElement.DurationSeconds ?? 0;
            var totalCount = interactions.Count;
            var totalSecondsWatched = 0;

            // Construct the arrays that will hold the value at each second.
            var engagementMap = new double[duration];
            var watchedMap = new int[duration];
            var rewatchedMap = new int[duration];

            totalSecondsWatched += ( int ) interactions.Select( i => i.WatchMap.WatchedPercentage / 100.0 * duration ).Sum();

            // Loop through every second of the video and build the maps.
            // Be cautious when modifying this code. A 70 minute video is
            // 4,200 seconds long, and if there are 10,000 watch maps to
            // process that means we are going to have 42 million calls to
            // GetCountAtPosition().
            for ( int second = 0; second < duration; second++ )
            {
                var counts = interactions.Select( i => i.WatchMap.GetCountAtPosition( second ) ).ToList();

                watchedMap[second] = counts.Count( a => a > 0 );
                rewatchedMap[second] = counts.Sum();
            }

            string averageWatchEngagement = "n/a";

            if ( duration > 0 )
            {
                averageWatchEngagement = string.Format( "{0}%", ( int ) ( totalSecondsWatched / duration * totalCount / 100.0 ) );
            }

            return new
            {
                PlayCount = totalCount,
                MinutesWatched = string.Format( "{0:n0}", totalSecondsWatched / 60 ),
                AverageWatchEngagement = averageWatchEngagement,
                Duration = duration,
                Watched = watchedMap,
                Rewatched = rewatchedMap
            };
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the video metric JSON data for the caller.
        /// </summary>
        /// <param name="mediaElementGuid">The media element unique identifier.</param>
        /// <returns>The content that represents the per-second metric data.</returns>
        [BlockAction]
        public BlockActionResult GetVideoMetricData( Guid mediaElementGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var mediaElement = new MediaElementService( rockContext ).Get( mediaElementGuid );

                if ( mediaElement == null )
                {
                    return new BlockActionResult( HttpStatusCode.NotFound );
                }

                var interactions = GetInteractions( mediaElement, null, rockContext );

                return new BlockActionResult( HttpStatusCode.OK, GetVideoData( mediaElement, interactions ) );
            }
        }

        [BlockAction]
        public BlockActionResult LoadIndividualPlays( Guid mediaElementGuid, string pageContext )
        {
            var interactionChannelId = InteractionChannelCache.Get( Rock.SystemGuid.InteractionChannel.MEDIA_EVENTS ).Id;
            var rockContext = new RockContext();
            PageContext context = null;

            if ( pageContext.IsNotNullOrWhiteSpace() )
            {
                var jsonContext = System.Text.Encoding.UTF8.GetString( Convert.FromBase64String( pageContext ) );
                context = jsonContext.FromJsonOrNull<PageContext>();
            }

            var mediaElementId = new MediaElementService( rockContext ).Get( mediaElementGuid )?.Id ?? 0;

            // Get all the interactions for this media element and then pull
            // in just the columns we need for performance.
            var interactionQuery = new InteractionService( rockContext ).Queryable()
                .Include( i => i.PersonAlias.Person )
                .Include( i => i.InteractionSession )
                .Where( i => i.InteractionComponent.InteractionChannelId == interactionChannelId
                    && i.Operation == "WATCH"
                    && i.InteractionComponent.EntityId == mediaElementId );

            if ( context != null )
            {
                // Query for the next page of results.
                // If the dates are the same, then take only older Ids.
                // If dates are not the same, then only take older dates.
                interactionQuery = interactionQuery
                    .Where( i => ( i.InteractionDateTime == context.LastDate && i.Id < context.LastId ) || i.InteractionDateTime < context.LastDate );
            }

            List<Interaction> interactions = null;

            // Load the next 25 results.
            DateTimeParameterInterceptor.UseWith( rockContext, () =>
            {
                interactions = interactionQuery
                    .OrderByDescending( i => i.InteractionDateTime )
                    .ThenByDescending( i => i.Id )
                    .Take( 25 )
                    .ToList();
            } );

            // If we got any results then figure out the next page context.
            if ( interactions.Count > 0 )
            {
                context = new PageContext
                {
                    LastDate = interactions.Last().InteractionDateTime,
                    LastId = interactions.Last().Id
                };

                pageContext = Convert.ToBase64String( System.Text.Encoding.UTF8.GetBytes( context.ToJson() ) );
            }
            else
            {
                pageContext = null;
            }

            // Get the actual data to provide to the client.
            var data = interactions
                .Select( i => new
                {
                    DateTime = new DateTimeOffset( i.InteractionDateTime, RockDateTime.OrgTimeZoneInfo.GetUtcOffset( i.InteractionDateTime ) ),
                    FullName = i.PersonAlias?.Person?.FullName ?? "Unknown",
                    PhotoUrl = i.PersonAlias?.Person?.PhotoUrl ?? "/Assets/Images/person-no-photo-unknown.svg",
                    Platform = "Unknown",
                    Data = i.InteractionData.FromJsonDynamicOrNull()
                } );

            return new BlockActionResult( HttpStatusCode.OK, new {
                Items = data,
                NextPage = pageContext
            } );
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail( PageParameter( PageParameterKey.MediaElementId ).AsInteger() );
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// Holds the data retrieved from the database so we can pass it
        /// around to different methods.
        /// </summary>
        private class InteractionData
        {
            /// <summary>
            /// Gets or sets the interaction date time.
            /// </summary>
            /// <value>
            /// The interaction date time.
            /// </value>
            public DateTime InteractionDateTime { get; set; }

            /// <summary>
            /// Gets or sets the watch map.
            /// </summary>
            /// <value>
            /// The watch map.
            /// </value>
            public WatchMapData WatchMap { get; set; }
        }

        /// <summary>
        /// Watch Map Data that has been saved into the interaction.
        /// </summary>
        private class WatchMapData
        {
            /// <summary>
            /// Gets or sets the watch map run length encoded data.
            /// </summary>
            /// <value>
            /// The watch map.
            /// </value>
            public string WatchMap
            {
                get
                {
                    return _watchMap;
                }
                set
                {
                    _watchMap = value;

                    var segments = new List<WatchSegment>();
                    var segs = _watchMap.Split( ',' );

                    foreach ( var seg in segs )
                    {
                        if ( seg.Length < 2 )
                        {
                            continue;
                        }

                        var duration = seg.Substring( 0, seg.Length - 1 ).AsInteger();
                        var count = seg.Substring( seg.Length - 1 ).AsInteger();

                        segments.Add( new WatchSegment { Duration = duration, Count = count } );
                    }

                    Segments = segments;
                }
            }
            private string _watchMap;

            /// <summary>
            /// Gets or sets the watched percentage.
            /// </summary>
            /// <value>
            /// The watched percentage.
            /// </value>
            public double WatchedPercentage { get; set; }

            /// <summary>
            /// The segments
            /// </summary>
            /// <remarks>
            /// Defined as field instead of property as it is accessed
            /// millions of times per page load.
            /// </remarks>
            public IReadOnlyList<WatchSegment> Segments;

            /// <summary>
            /// Gets the count at the specified position of the segment map.
            /// </summary>
            /// <param name="position">The position.</param>
            /// <returns>The count or 0 if not found.</returns>
            public int GetCountAtPosition( int position )
            {
                // Walk through each segment until we find a segment that
                // "owns" the position we are interested in. Generally
                // speaking, there should probably always be less than 4 or 5
                // segments so this should be pretty fast.
                foreach ( var segment in Segments )
                {
                    // This segment owns the position we care about.
                    if ( position < segment.Duration )
                    {
                        return segment.Count;
                    }

                    // Decrement the position before moving to the next segment.
                    position -= segment.Duration;

                    if ( position < 0 )
                    {
                        break;
                    }
                }

                return 0;
            }
        }

        /// <summary>
        /// Individual segment of the watch map. Use fields instead of properties
        /// for performance reasons as they can be access tens of millions of
        /// times per page load.
        /// </summary>
        private class WatchSegment
        {
            /// <summary>
            /// The duration of this segment in seconds.
            /// </summary>
            public int Duration;

            /// <summary>
            /// The number of times this segment was watched.
            /// </summary>
            public int Count;
        }

        private class PageContext
        {
            public DateTime LastDate { get; set; }

            public int LastId { get; set; }
        }

        /// <summary>
        /// DateTimeParameterInterceptor fixes the incorrect behavior of
        /// Entity Framework library when for datetime columns it's generating
        /// datetime2(7) parameters  when using SQL Server 2016 and greater.
        /// Because of that, there were date comparison issues.
        /// Without this, there are rounding errors if the date time in the
        /// database has a value of 7 in the thousands milliseconds position.
        /// Links:
        /// https://github.com/aspnet/EntityFramework6/issues/49
        /// https://github.com/aspnet/EntityFramework6/issues/578
        /// </summary>
        public class DateTimeParameterInterceptor : DbCommandInterceptor
        {
            private readonly RockContext _context;

            public DateTimeParameterInterceptor( RockContext rockContext )
            {
                _context = rockContext;
            }

            public override void ReaderExecuting( DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext )
            {
                if ( interceptionContext.DbContexts.Any( db => db == _context ) )
                {
                    ChangeDateTime2ToDateTime( command );
                }
            }

            public static void UseWith( RockContext rockContext, Action action )
            {
                var interceptor = new DateTimeParameterInterceptor( rockContext );
                DbInterception.Add( interceptor );

                try
                {
                    action();
                }
                finally
                {
                    DbInterception.Remove( interceptor );
                }
            }

            private static void ChangeDateTime2ToDateTime( DbCommand command )
            {
                command.Parameters
                    .OfType<SqlParameter>()
                    .Where( p => p.SqlDbType == SqlDbType.DateTime2 )
                    .Where( p => p.Value != DBNull.Value )
                    .Where( p => p.Value is DateTime )
                    .Where( p => p.Value as DateTime? != DateTime.MinValue )
                    .ToList()
                    .ForEach( p => p.SqlDbType = SqlDbType.DateTime );
            }
        }

        #endregion
    }
}