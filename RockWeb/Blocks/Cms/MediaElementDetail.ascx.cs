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
using System.Web.UI;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Blocks;
using Rock.Constants;
using Rock.Data;
using Rock.Media;
using Rock.Model;
using Rock.Net;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Media Element Detail" )]
    [Category( "CMS" )]
    [Description( "Edit details of a Media Element" )]

    [Rock.SystemGuid.BlockTypeGuid( "881DC0D1-FF98-4A5E-827F-49DD5CD0BD32" )]
    public partial class MediaElementDetail : RockBlock, IRockBlockType
    {
        #region Properties

        private List<MediaElementFileDataStateful> FileDataState { get; set; }

        private List<MediaElementThumbnailDataStateful> ThumbnailDataState { get; set; }

        #endregion Properties

        #region Keys

        private static class PageParameterKey
        {
            public const string MediaFolderId = "MediaFolderId";
            public const string MediaElementId = "MediaElementId";
        }

        private static class ViewStateKey
        {
            public const string FileDataState = "FileDataState";
            public const string ThumbnailDataState = "ThumbnailDataState";
        }

        #endregion Keys

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
        System.Threading.Tasks.Task<object> IRockBlockType.GetBlockInitializationAsync( RockClientType clientType )
        {
            return System.Threading.Tasks.Task.FromResult( ( object ) null );
        }

        #endregion IRockBlockType implementation

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            FileDataState = ViewState[ViewStateKey.FileDataState] as List<MediaElementFileDataStateful> ?? new List<MediaElementFileDataStateful>();
            ThumbnailDataState = ViewState[ViewStateKey.ThumbnailDataState] as List<MediaElementThumbnailDataStateful> ?? new List<MediaElementThumbnailDataStateful>();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            RockPage.AddCSSLink( "~/Styles/Blocks/Cms/MediaElementDetail.css", true );
            RockPage.AddScriptLink( "~/Scripts/Chartjs/Chart.min.js" );
            RockPage.AddScriptLink( "~/Scripts/moment.min.js" );
            RockPage.AddScriptLink( "~/Scripts/Rock/Controls/MediaElementDetail/mediaElementPlayAnalytics.js" );

            gMediaFiles.Actions.AddClick += gMediaFiles_Add;
            gMediaFiles.DataKeyNames = new string[] { "Guid" };
            gThumbnailFiles.Actions.AddClick += gThumbnailFiles_Add;
            gThumbnailFiles.DataKeyNames = new string[] { "Guid" };

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger(upnlContent);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                ShowDetail( PageParameter( PageParameterKey.MediaElementId ).AsInteger() );
            }
            else
            {
                var disAllowManualEntry = hfDisallowManualEntry.Value.AsBoolean();
                IsAllowManualEntry(!disAllowManualEntry);
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState[ViewStateKey.FileDataState] = FileDataState;
            ViewState[ViewStateKey.ThumbnailDataState] = ThumbnailDataState;

            return base.SaveViewState();
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
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Media Element", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion Base Control Methods

        #region Events

        /// <summary>
        /// Handles the Click event of the lbMediaFiles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbMediaFiles_Click(object sender, EventArgs e)
        {
            ShowFileDetail( PageParameter( PageParameterKey.MediaElementId ).AsInteger(), PageParameter( PageParameterKey.MediaFolderId ).AsIntegerOrNull() );
        }

        /// <summary>
        /// Handles the Click event of the lbMediaAnalytics control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbMediaAnalytics_Click(object sender, EventArgs e)
        {
            ShowDetail( PageParameter( PageParameterKey.MediaElementId ).AsInteger() );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail( PageParameter( PageParameterKey.MediaElementId ).AsInteger() );
        }

        #region Analytics View Events

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rddlTileFrame control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rddlTileFrame_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( rddlTileFrame.SelectedValue == "12Months" )
            {
                lblChartTitle.Text = "Plays Per Week";
                pnlLast12MonthsDetails.Visible = true;
                pnlLast90DaysDetails.Visible = false;
            }
            else
            {
                lblChartTitle.Text = "Plays Per Day";
                pnlLast12MonthsDetails.Visible = false;
                pnlLast90DaysDetails.Visible = true;
            }
        }

        #endregion Analytics View Events

        #region File View Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var mediaElement = new MediaElementService( new RockContext() ).Get( hfId.Value.AsInteger() );
            ShowEditFileDetails( mediaElement );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( !Page.IsValid )
            {
                return;
            }

            var rockContext = new RockContext();
            var mediaElementService = new MediaElementService( rockContext );
            var mediaElementId = hfId.Value.AsIntegerOrNull();
            MediaElement mediaElement = null;
            if ( mediaElementId.HasValue )
            {
                mediaElement = mediaElementService.Get( mediaElementId.Value );
            }

            var isNew = mediaElement == null;

            if ( isNew )
            {
                mediaElement = new MediaElement()
                {
                    Id = 0,
                    MediaFolderId = hfMediaFolderId.ValueAsInt()
                };
                mediaElementService.Add( mediaElement );
            }

            mediaElement.Name = tbName.Text;
            mediaElement.Description = tbDescription.Text;
            mediaElement.DurationSeconds = nbDuration.Text.AsIntegerOrNull();
            mediaElement.ThumbnailDataJson = ThumbnailDataState.ToJson();
            mediaElement.FileDataJson = FileDataState.ToJson();
            rockContext.SaveChanges();

            NavigateToCurrentPageReference( new Dictionary<string, string>
            {
                { PageParameterKey.MediaElementId, mediaElement.Id.ToString() }
            } );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfId.Value.Equals( "0" ) )
            {
                // Canceling on Add
                Dictionary<string, string> qryString = new Dictionary<string, string>();
                qryString[PageParameterKey.MediaFolderId] = hfMediaFolderId.Value;
                NavigateToParentPage( qryString );
            }
            else
            {
                // Canceling on Edit
                var mediaElement = new MediaElementService( new RockContext() ).Get( int.Parse( hfId.Value ) );
                ShowReadonlyFileDetails( mediaElement );
            }
        }

        /// <summary>
        /// Handles the Add event of the gMediaFiles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gMediaFiles_Add( object sender, EventArgs e )
        {
            gMediaFiles_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gMediaFiles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gMediaFiles_Edit( object sender, RowEventArgs e )
        {
            var mediaFileGuid = ( Guid ) e.RowKeyValue;
            gMediaFiles_ShowEdit( mediaFileGuid );
        }

        /// <summary>
        /// Handles the Add event of the gThumbnailFiles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gThumbnailFiles_Add( object sender, EventArgs e )
        {
            gThumbnailFiles_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gThumbnailFiles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gThumbnailFiles_Edit( object sender, RowEventArgs e )
        {
            var thumbnailFileGuid = ( Guid ) e.RowKeyValue;
            gThumbnailFiles_ShowEdit( thumbnailFileGuid );
        }

        /// <summary>
        /// Handles the SaveClick event of the mdMediaFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdMediaFile_SaveClick( object sender, EventArgs e )
        {
            Guid? guid = hfMediaElementData.Value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                var mediaElementData = FileDataState.Where( t => t.Guid.Equals( guid.Value ) ).FirstOrDefault();
                if ( mediaElementData == null )
                {
                    mediaElementData = new MediaElementFileDataStateful();
                    FileDataState.Add( mediaElementData );
                }

                mediaElementData.PublicName = tbPublicName.Text;
                mediaElementData.AllowDownload = cbAllowDownload.Checked;
                mediaElementData.Link = urlLink.Text;
                mediaElementData.Quality = ddlQuality.SelectedValueAsEnum<MediaElementQuality>( MediaElementQuality.Other );
                mediaElementData.Format = tbFormat.Text;
                mediaElementData.Width = nbWidth.Text.AsInteger();
                mediaElementData.Height = nbHeight.Text.AsInteger();
                mediaElementData.FPS = nbFPS.Text.AsInteger();

                long size;
                if ( !long.TryParse( nbSize.Text, out size ) )
                {
                    size = 0;
                }
                mediaElementData.Size = size;

                BindMediaFileGrid();
            }

            HideDialog();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdThumbnailFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdThumbnailFile_SaveClick( object sender, EventArgs e )
        {
            Guid? guid = hfThumbnailFile.Value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                var thumbnailData = ThumbnailDataState.Where( t => t.Guid.Equals( guid.Value ) ).FirstOrDefault();
                if ( thumbnailData == null )
                {
                    thumbnailData = new MediaElementThumbnailDataStateful();
                    ThumbnailDataState.Add( thumbnailData );
                }

                thumbnailData.Link = urlThumbnailLink.Text;
                thumbnailData.Width = nbThumbnailWidth.Text.AsInteger();
                thumbnailData.Height = nbThumbnailHeight.Text.AsInteger();

                long size;
                if ( !long.TryParse( nbThumbnailSize.Text, out size ) )
                {
                    size = 0;
                }
                thumbnailData.Size = size;

                BindThumbnailDataGrid();
            }

            HideDialog();
        }

        #endregion File View Events

        #endregion Events

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="mediaElementId">The media element identifier.</param>
        public void ShowDetail( int mediaElementId )
        {
            // If the control is being used to create a new element via manual entry, show the file entry
            // detail view.  Otherwise, show the analytics view.
            if ( mediaElementId == 0 )
            {
                ShowFileDetail( mediaElementId, PageParameter( PageParameterKey.MediaFolderId ).AsIntegerOrNull() );
            }
            else
            {
                ShowAnalyticsDetail( mediaElementId );
            }
        }

        #region Analytics View Methods

        /// <summary>
        /// Registers the startup script.
        /// </summary>
        /// <param name="mediaElement">The media element.</param>
        private void RegisterStartupScript( MediaElement mediaElement )
        {
            Rock.Web.UI.Controls.MediaPlayer.AddLinksForMediaToPage( mediaElement.DefaultFileUrl, Page );

            var script = string.Format(@"Sys.Application.add_load(function () {{
    const playerOptions = {{
        mediaUrl: '{3}',
        posterUrl: '{4}',
        type: 'video',
        trackProgress: false,
        controls: '',
        clickToPlay: false,
    }};

    let player = null;

    if (playerOptions.mediaUrl !== '' && $('#{5}').length > 0) {{
        player = new Rock.UI.MediaPlayer('#{5}', playerOptions);
    }}

    Rock.controls.mediaElementPlayAnalytics.initialize({{
        chartId: '{0}',
        mediaElementGuid: '{1}',
        blockGuid: '{2}',
        individualPlaysId: '{6}',
        player: player
    }});
}});",
                cChart.ClientID, // {0}
                mediaElement.Guid, // {1}
                BlockCache.Guid, // {2}
                mediaElement.DefaultFileUrl, // {3}
                mediaElement.DefaultThumbnailUrl, // {4}
                pnlMediaPlayer.ClientID, // {5}
                pnlIndividualPlays.ClientID // {6}
                );

            RockPage.ClientScript.RegisterStartupScript( GetType(), "startup-script", script, true );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="mediaElementId">The media element identifier.</param>
        public void ShowAnalyticsDetail( int mediaElementId )
        {
            pnlViewFile.Visible = false;
            lbMediaFiles.Visible = true;
            lbMediaAnalytics.Visible = false;
            hlDuration.Visible = true;

            var rockContext = new RockContext();
            var mediaElementService = new MediaElementService( rockContext );
            MediaElement mediaElement = null;

            if ( mediaElementId != 0 )
            {
                mediaElement = mediaElementService.Get( mediaElementId );
            }

            if ( mediaElement == null )
            {
                pnlViewAnalytics.Visible = false;
                return;
            }

            var interactions = GetInteractions( mediaElement, null, rockContext );

            // If we have no interactions yet then put up a friendly message about
            // not having any statistical data yet.
            if ( !interactions.Any() )
            {
                nbNoData.Visible = true;
                pnlAnalytics.Visible = false;
                pnlAllTimeDetails.Visible = false;
            }
            else
            {
                // Show all the statistics.
                ShowAllTimeDetails( mediaElement, interactions );
                ShowLast12MonthsDetails( mediaElement, interactions );
                ShowLast90DaysDetails( mediaElement, interactions );
            }

            pnlViewAnalytics.Visible = true;

            lActionTitle.Text = mediaElement.Name.FormatAsHtmlTitle();
            hlDuration.Text = mediaElement.DurationSeconds.ToFriendlyDuration();
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

        /// <summary>
        /// Gets the interaction data grouped by date.
        /// </summary>
        /// <param name="mediaElement">The media element.</param>
        /// <param name="interactions">The interactions.</param>
        /// <param name="dateSelector">The date selector used to determine the date of the interaction for grouping purposes.</param>
        /// <returns>A collection of <see cref="InteractionDataForDate"/> instances with the grouped data.</returns>
        private static List<InteractionDataForDate> GetInteractionDataForDates( MediaElement mediaElement, IEnumerable<InteractionData> interactions, Func<InteractionData, DateTime> dateSelector )
        {
            // Group the interactions by date, using the provided date selector
            // function to get the date for the interaction.
            var interactionsByDate = interactions
                .Select( i => new
                {
                    Date = dateSelector( i ),
                    i.InteractionDateTime,
                    i.WatchMap
                } )
                .GroupBy( i => i.Date );

            // Convert the data into a collection of InteractionDataForDate
            // instances.
            return interactionsByDate
                .Select( i => new InteractionDataForDate
                {
                    Date = i.Key,
                    Count = i.Count(),
                    Engagement = i.Sum( a => a.WatchMap.WatchedPercentage ) / i.Count(),
                    MinutesWatched = ( int ) ( ( mediaElement.DurationSeconds ?? 0 ) * i.Sum( a => a.WatchMap.WatchedPercentage ) / 100 / 60 )
                } )
                .OrderBy( i => i.Date )
                .ToList();
        }

        /// <summary>
        /// Updates the interaction data for any missing dates between the start and end
        /// dates, inclusive. Missing data is inserted as zeros.
        /// </summary>
        /// <param name="interactionData">The interaction data.</param>
        /// <param name="firstDate">The first date.</param>
        /// <param name="lastDate">The last date.</param>
        /// <param name="dateSelector">The date selector.</param>
        /// <returns>A new list of interaction data with the missing dates applied.</returns>
        private static List<InteractionDataForDate> UpdateInteractionDataForMissingDates( List<InteractionDataForDate> interactionData, DateTime firstDate, DateTime lastDate, Func<DateTime, DateTime> dateSelector )
        {
            for ( DateTime date = firstDate; date <= lastDate; date = date.AddDays( 1 ) )
            {
                var actualDate = dateSelector( date );

                if ( interactionData.SingleOrDefault( i => i.Date == actualDate ) == null )
                {
                    interactionData.Add( new InteractionDataForDate
                    {
                        Date = actualDate,
                        Count = 0,
                        Engagement = 0,
                        MinutesWatched = 0
                    } );
                }
            }

            return interactionData.OrderBy( i => i.Date ).ToList();
        }

        /// <summary>
        /// Gets the standard KPI metric lava snippets for the interactions.
        /// </summary>
        /// <param name="mediaElement">The media element.</param>
        /// <param name="interactions">The interactions.</param>
        /// <returns>
        /// A collection of KPI metric snippets.
        /// </returns>
        private static List<string> GetStandardKpiMetrics( MediaElement mediaElement, List<InteractionData> interactions )
        {
            var kpiMetrics = new List<string>();

            var engagement = interactions.Sum( a => a.WatchMap.WatchedPercentage ) / interactions.Count();
            kpiMetrics.Add( GetKpiMetricLava( "blue-400", "fa fa-broadcast-tower", $"{engagement:n1}%", "Engagement", $"The total minutes played divided by the length of the video times the total number of plays." ) );

            var playCount = interactions.Count();
            var playCountText = GetFormattedNumber( playCount );
            kpiMetrics.Add( GetKpiMetricLava( "green-500", "fa fa-play-circle", playCountText, "Plays", $"This video was played {playCount:n0} times." ) );

            var minutesWatched = ( int ) ( interactions.Sum( a => a.WatchMap.WatchedPercentage ) * ( mediaElement.DurationSeconds ?? 0 ) / 100 / 60 );
            var minutesWatchedText = GetFormattedNumber( minutesWatched );
            kpiMetrics.Add( GetKpiMetricLava( "orange-400", "fa fa-clock", minutesWatchedText, "Minutes Watched", $"A total of {minutesWatched:n0} minutes were watched." ) );

            return kpiMetrics;
        }

        /// <summary>
        /// Gets the standard trend chart lava snippets.
        /// </summary>
        /// <param name="interactions">The interactions.</param>
        /// <param name="chartPeriodTitle">The period title that goes above the trend chart.</param>
        /// <param name="periodTitle">The period title that goes on each element of the trend chart.</param>
        /// <returns>A collection of HTML and lava snippets to render the trend charts.</returns>
        private static string GetPlayCountTrendChart( List<InteractionDataForDate> interactions, string periodTitle )
        {
            var playCount = interactions
                .Select( i => $"[[ dataitem label:'{i.Count} plays {periodTitle} {i.Date.ToShortDateString()}' value:'{i.Count}']][[ enddataitem ]]" )
                .ToList();

            return $"{{[trendchart]}}" + string.Join( "", playCount ) + "{[endtrendchart]}";
        }

        /// <summary>
        /// Shows the interaction details for all time.
        /// </summary>
        /// <param name="mediaElement">The media element.</param>
        /// <param name="interactions">The interactions.</param>
        private void ShowAllTimeDetails( MediaElement mediaElement, List<InteractionData> interactions )
        {
            var metrics = GetStandardKpiMetrics( mediaElement, interactions );

            var lava = "{[kpis style:'card' columncount:'1']}" + string.Join( string.Empty, metrics ) + "{[endkpis]}";

            lAllTimeContent.Text = lava.ResolveMergeFields( new Dictionary<string, object>() );
        }

        /// <summary>
        /// Shows the interaction details for the last 12 months.
        /// </summary>
        /// <param name="mediaElement">The media element.</param>
        /// <param name="interactions">The interactions.</param>
        private void ShowLast12MonthsDetails( MediaElement mediaElement, List<InteractionData> interactions )
        {
            // Filter interactions to the past 12 months.
            var past12Months = RockDateTime.Now.AddMonths( -12 ).Date;
            interactions = interactions.Where( a => a.InteractionDateTime >= past12Months ).ToList();

            // Filter interactions to be after the start of the next week
            // from our initial filtering. This way we get a whole week so
            // the trend chart doesn't look sad.
            var past12MonthsStartOfWeek = GetNextStartOfWeek( past12Months );
            var recentInteractions = interactions.Where( i => i.InteractionDateTime >= past12MonthsStartOfWeek );
            var interactionsPerWeek = GetInteractionDataForDates( mediaElement, recentInteractions, i => GetPreviousStartOfWeek( i.InteractionDateTime.Date ) );
            interactionsPerWeek = UpdateInteractionDataForMissingDates( interactionsPerWeek, past12MonthsStartOfWeek, RockDateTime.Now.Date, d => GetPreviousStartOfWeek( d ) );
            var lava = GetPlayCountTrendChart( interactionsPerWeek, "during the week of" );

            lLast12MonthsContent.Text = lava.ResolveMergeFields( new Dictionary<string, object>() );
        }

        /// <summary>
        /// Shows the interaction details for the last 90 days details.
        /// </summary>
        /// <param name="mediaElement">The media element.</param>
        /// <param name="interactions">The interactions.</param>
        private void ShowLast90DaysDetails( MediaElement mediaElement, List<InteractionData> interactions )
        {
            // Filter interactions to the past 90 days.
            var past90Days = RockDateTime.Now.AddDays( -90 ).Date;
            interactions = interactions.Where( a => a.InteractionDateTime >= past90Days ).ToList();

            // Get the interaction counts per day and the associated trend charts.
            var interactionsPerDay = GetInteractionDataForDates( mediaElement, interactions, i => i.InteractionDateTime.Date );
            interactionsPerDay = UpdateInteractionDataForMissingDates( interactionsPerDay, past90Days, RockDateTime.Now.Date, d => d.Date );
            var lava = GetPlayCountTrendChart( interactionsPerDay, "on" );

            lLast90DaysContent.Text = lava.ResolveMergeFields( new Dictionary<string, object>() );
        }

        /// <summary>
        /// Gets the date of the previous start of the Rock week. If the date
        /// passed is already the first day of the week then the date is returned.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>A <see cref="DateTime"/> instance that represents the new date.</returns>
        private static DateTime GetPreviousStartOfWeek( DateTime date )
        {
            while ( date.DayOfWeek != RockDateTime.FirstDayOfWeek )
            {
                date = date.AddDays( -1 );
            }

            return date.Date;
        }

        /// <summary>
        /// Gets the date of the next start of the Rock week. If the date
        /// passed is already the first day of the week then the date is returned.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>A <see cref="DateTime"/> instance that represents the new date.</returns>
        private static DateTime GetNextStartOfWeek( DateTime date )
        {
            while ( date.DayOfWeek != RockDateTime.FirstDayOfWeek )
            {
                date = date.AddDays( 1 );
            }

            return date.Date;
        }

        /// <summary>
        /// Gets the formatted number in thousands and millions. That is, if
        /// number is more than 1,000 then divide by 1,000 and append "K".
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The string that represents the formatted number.</returns>
        private static string GetFormattedNumber( long value )
        {
            if ( value >= 1000000 )
            {
                return $"{value / 1000000f:n2}M";
            }
            else if ( value >= 1000 )
            {
                return $"{value / 1000f:n2}K";
            }
            else
            {
                return value.ToString( "n0" );
            }
        }

        /// <summary>
        /// Gets the metric HTML for the given values.
        /// </summary>
        /// <param name="styleClass">The background style class.</param>
        /// <param name="iconClass">The icon class.</param>
        /// <param name="value">The value.</param>
        /// <param name="label">The value label.</param>
        /// <param name="description">The tool tip description text.</param>
        /// <returns>A string of HTML content.</returns>
        private static string GetKpiMetricLava( string styleClass, string iconClass, string value, string label, string description )
        {
            return $"[[kpi icon:'{iconClass}' value:'{value}' label:'{label}' color:'{styleClass}' description:'{description}']][[ endkpi ]]";
        }

        #endregion Analytics View Methods

        #region File View Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="mediaElementId">The media element identifier.</param>
        public void ShowFileDetail( int mediaElementId )
        {
            ShowFileDetail( mediaElementId, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="mediaElementId">The media element identifier.</param>
        /// <param name="mediaFolderId">The media folder identifier.</param>
        public void ShowFileDetail( int mediaElementId, int? mediaFolderId )
        {
            var rockContext = new RockContext();
            var mediaElementService = new MediaElementService( rockContext );
            MediaElement mediaElement = null;

            if ( !mediaElementId.Equals( 0 ) )
            {
                mediaElement = mediaElementService.Get( mediaElementId );
            }

            bool isExistingElement = ( mediaElement != null );

            lbMediaAnalytics.Visible = isExistingElement;
            hlDuration.Visible = isExistingElement;
            lbMediaFiles.Visible = false;
            pnlViewAnalytics.Visible = false;

            if ( !isExistingElement )
            {
                MediaFolder mediaFolder = null;
                if ( mediaFolderId.HasValue )
                {
                    mediaFolder = new MediaFolderService( rockContext ).Get( mediaFolderId.Value );
                }

                if ( mediaFolder != null )
                {
                    mediaElement = new MediaElement { Id = 0, MediaFolderId = mediaFolder.Id };
                }
                else
                {
                    pnlNoFolder.Visible = true;
                    pnlViewFile.Visible = false;
                    return;
                }
            }

            hfId.SetValue( mediaElement.Id );
            hfMediaFolderId.SetValue( mediaElement.MediaFolderId );
            pnlViewFile.Visible = true;

            bool readOnly = false;

            var mediaComponent = mediaElement.MediaFolder?.MediaAccount?.GetMediaAccountComponent();
            if ( mediaComponent != null && !mediaComponent.AllowsManualEntry )
            {
                readOnly = true;
            }

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( MediaFolder.FriendlyTypeName );
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                ShowReadonlyFileDetails( mediaElement );
            }
            else
            {
                btnEdit.Visible = true;
                if ( mediaElement.Id > 0 )
                {
                    ShowReadonlyFileDetails( mediaElement );
                }
                else
                {
                    ShowEditFileDetails( mediaElement );
                }
            }
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="mediaAccount">The media element.</param>
        private void ShowReadonlyFileDetails( MediaElement mediaElement )
        {
            FileDataState = mediaElement.FileDataJson.FromJsonOrNull<List<MediaElementFileDataStateful>>();
            ThumbnailDataState = mediaElement.ThumbnailDataJson.FromJsonOrNull<List<MediaElementThumbnailDataStateful>>();

            SetEditMode( false );

            lActionTitle.Text = mediaElement.Name.FormatAsHtmlTitle();
            hlDuration.Text = mediaElement.DurationSeconds.ToFriendlyDuration();

            var descriptionList = new DescriptionList();
            descriptionList.Add( "Folder", mediaElement.MediaFolder.Name );
            if ( mediaElement.Description.IsNotNullOrWhiteSpace() )
            {
                descriptionList.Add( "Description", mediaElement.Description );
            }

            if ( mediaElement.DurationSeconds.HasValue )
            {
                descriptionList.Add( "Duration", mediaElement.DurationSeconds.ToFriendlyDuration() );
            }

            lDescription.Text = descriptionList.Html;

            gViewMediaFiles.DataSource = FileDataState
              .ToList();
            gViewMediaFiles.DataBind();

            gViewThumbnailFiles.DataSource = ThumbnailDataState
              .ToList();
            gViewThumbnailFiles.DataBind();
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="mediaElement">The media element.</param>
        public void ShowEditFileDetails( MediaElement mediaElement )
        {
            if ( mediaElement.Id == 0 )
            {
                lActionTitle.Text = ActionTitle.Add( "Media Element" ).FormatAsHtmlTitle();
            }
            else
            {
                lActionTitle.Text = mediaElement.Name.FormatAsHtmlTitle();
            }

            SetEditMode( true );

            tbName.Text = mediaElement.Name;
            tbDescription.Text = mediaElement.Description;
            nbDuration.Text = mediaElement.DurationSeconds.ToStringSafe();

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( MediaFolder.FriendlyTypeName );
            }

            var mediaFolder = new MediaFolderService( new RockContext() ).Get( mediaElement.MediaFolderId );
            if ( mediaFolder != null )
            {
                var mediaAccountComponent = GetMediaAccountComponent( mediaFolder.MediaAccount );
                if ( mediaAccountComponent != null )
                {
                    readOnly = !mediaAccountComponent.AllowsManualEntry;
                }
            }

            hfDisallowManualEntry.Value = readOnly.ToString();
            IsAllowManualEntry( !readOnly );

            tbName.ReadOnly = readOnly;
            tbDescription.ReadOnly = readOnly;
            nbDuration.ReadOnly = readOnly;

            BindThumbnailDataGrid();
            BindMediaFileGrid();
        }

        /// <summary>
        /// Allows the manual entry.
        /// </summary>
        private void IsAllowManualEntry( bool allowManualEntry )
        {
            gMediaFiles.Actions.ShowAdd = allowManualEntry;
            var editField = gMediaFiles.ColumnsOfType<EditField>().FirstOrDefault();
            if ( editField != null )
            {
                editField.Visible = allowManualEntry;
            }

            gThumbnailFiles.Actions.ShowAdd = allowManualEntry;
            var thumbnailEditField = gThumbnailFiles.ColumnsOfType<EditField>().FirstOrDefault();
            if ( editField != null )
            {
                thumbnailEditField.Visible = allowManualEntry;
            }
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            pnlViewFileDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Shows the edit dialog for the specified media file.
        /// </summary>
        /// <param name="mediaFileGuid">The media file identifier.</param>
        protected void gMediaFiles_ShowEdit( Guid mediaFileGuid )
        {
            var mediaElementData = FileDataState.FirstOrDefault( l => l.Guid.Equals( mediaFileGuid ) );

            ddlQuality.BindToEnum<MediaElementQuality>( true );

            if ( mediaElementData != null )
            {
                hfMediaElementData.Value = mediaElementData.Guid.ToString();
                tbPublicName.Text = mediaElementData.PublicName;
                cbAllowDownload.Checked = mediaElementData.AllowDownload;
                urlLink.Text = mediaElementData.Link;
                ddlQuality.SetValue( mediaElementData.Quality.ConvertToInt() );
                tbFormat.Text = mediaElementData.Format;
                nbWidth.IntegerValue = mediaElementData.Width;
                nbHeight.IntegerValue = mediaElementData.Height;
                nbFPS.IntegerValue = mediaElementData.FPS;
                nbSize.Text = mediaElementData.Size.ToString();
            }
            else
            {
                hfMediaElementData.Value = Guid.Empty.ToString();
                tbPublicName.Text = string.Empty;
                urlLink.Text = string.Empty;
                ddlQuality.SelectedValue = string.Empty;
                tbFormat.Text = string.Empty;
                nbWidth.IntegerValue = null;
                nbHeight.IntegerValue = null;
                nbFPS.IntegerValue = null;
                nbSize.IntegerValue = null;
                cbAllowDownload.Checked = true;
            }

            ShowDialog( "MEDIAFILES", true );
        }

        /// <summary>
        /// Shows the edit dialog for the specified thumbnail file.
        /// </summary>
        /// <param name="thumbnailFileGuid">The thumbnail file identifier.</param>
        protected void gThumbnailFiles_ShowEdit( Guid thumbnailFileGuid )
        {
            var thumbnailData = ThumbnailDataState.FirstOrDefault( l => l.Guid.Equals( thumbnailFileGuid ) );

            if ( thumbnailData != null )
            {
                hfThumbnailFile.Value = thumbnailData.Guid.ToString();
                urlThumbnailLink.Text = thumbnailData.Link;
                nbThumbnailWidth.IntegerValue = thumbnailData.Width;
                nbThumbnailHeight.IntegerValue = thumbnailData.Height;
                nbThumbnailSize.Text = thumbnailData.Size.ToString();
            }
            else
            {
                hfThumbnailFile.Value = Guid.Empty.ToString();
                urlThumbnailLink.Text = string.Empty;
                nbThumbnailWidth.IntegerValue = null;
                nbThumbnailHeight.IntegerValue = null;
                nbThumbnailSize.IntegerValue = null;
            }

            ShowDialog( "THUMBNAILFILES", true );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( string dialog, bool setValues = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( setValues );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( bool setValues = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "MEDIAFILES":
                    {
                        mdMediaFile.Show();
                        break;
                    }

                case "THUMBNAILFILES":
                    {
                        mdThumbnailFile.Show();
                        break;
                    }
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "MEDIAFILES":
                    {
                        mdMediaFile.Hide();
                        break;
                    }

                case "THUMBNAILFILES":
                    {
                        mdThumbnailFile.Hide();
                        break;
                    }
            }

            hfActiveDialog.Value = string.Empty;
        }

        /// <summary>
        /// Binds the media file grid.
        /// </summary>
        private void BindMediaFileGrid()
        {
            FileDataState = FileDataState ?? new List<MediaElementFileDataStateful>();
            gMediaFiles.DataSource = FileDataState
                .ToList();
            gMediaFiles.DataBind();
        }

        /// <summary>
        /// Binds the thumbnail file grid.
        /// </summary>
        private void BindThumbnailDataGrid()
        {
            ThumbnailDataState = ThumbnailDataState ?? new List<MediaElementThumbnailDataStateful>();
            gThumbnailFiles.DataSource = ThumbnailDataState
                .ToList();
            gThumbnailFiles.DataBind();
        }

        /// <summary>
        /// Gets the media account component.
        /// </summary>
        /// <param name="mediaAccount">The media account.</param>
        /// <returns></returns>
        private MediaAccountComponent GetMediaAccountComponent( MediaAccount mediaAccount )
        {
            var componentEntityTypeId = mediaAccount != null ? mediaAccount.ComponentEntityTypeId : ( int? ) null;

            if ( componentEntityTypeId.HasValue )
            {
                var componentEntityType = EntityTypeCache.Get( componentEntityTypeId.Value );
                return componentEntityType == null ? null : MediaAccountContainer.GetComponent( componentEntityType.Name );
            }

            return null;
        }

        #endregion File View Methods

        #endregion Internal Methods

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
                    return new BlockActionResult( System.Net.HttpStatusCode.NotFound );
                }

                var interactions = GetInteractions( mediaElement, null, rockContext );

                return new BlockActionResult( System.Net.HttpStatusCode.OK, GetVideoData( mediaElement, interactions ) );
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
                .Include( i => i.InteractionSession.InteractionSessionLocation )
                .Include( i => i.InteractionSession.DeviceType )
                .Where( i => i.InteractionComponent.InteractionChannelId == interactionChannelId
                    && i.Operation == "WATCH"
                    && i.InteractionComponent.EntityId == mediaElementId );

            var filteredQuery = interactionQuery;

            if ( context != null )
            {
                // Query for the next page of results.
                // If the dates are the same, then take only older Ids.
                // If dates are not the same, then only take older dates.
                filteredQuery = filteredQuery
                    .Where( i => ( i.InteractionDateTime == context.LastDate && i.Id < context.LastId ) || i.InteractionDateTime < context.LastDate );
            }

            List<Interaction> interactions = null;

            // Load the next 25 results.
            DateTimeParameterInterceptor.UseWith( rockContext, () =>
            {
                interactions = filteredQuery
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
                    Data = i.InteractionData.FromJsonDynamicOrNull(),
                    Location = i.InteractionSession?.InteractionSessionLocation?.Location,
                    ClientType = i.InteractionSession?.DeviceType.ClientType,
                    Isp = i.InteractionSession?.InteractionSessionLocation?.ISP,
                    OperatingSystem = i.InteractionSession?.DeviceType?.OperatingSystem,
                    Application = i.InteractionSession?.DeviceType?.Application,
                    InteractionsCount = interactionQuery.Where( m => m.PersonAliasId != null && m.PersonAliasId == i.PersonAliasId ).Count()
                } );

            return new BlockActionResult( System.Net.HttpStatusCode.OK, new {
                Items = data,
                NextPage = pageContext
            } );
        }

        #endregion Action Methods

        #region Support Classes

        #region Analytics View Support Classes

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
        /// Used to record grouped interaction data into a single date.
        /// </summary>
        private class InteractionDataForDate
        {
            /// <summary>
            /// Gets or sets the date this data is for.
            /// </summary>
            /// <value>
            /// The date this data is for.
            /// </value>
            public DateTime Date { get; set; }

            /// <summary>
            /// Gets or sets the number of interactions for this date.
            /// </summary>
            /// <value>
            /// The number of interactions for this date.
            /// </value>
            public long Count { get; set; }

            /// <summary>
            /// Gets or sets the average engagement for this date.
            /// </summary>
            /// <value>
            /// The average engagement for this date.
            /// </value>
            public double Engagement { get; set; }

            /// <summary>
            /// Gets or sets the number of minutes watched for this date.
            /// </summary>
            /// <value>
            /// The minutes number of watched for this date.
            /// </value>
            public int MinutesWatched { get; set; }
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

        #endregion Analytics View Support Classes

        #region File View Support Classes

        /// <summary>
        /// Provides an additional Guid property that can be used to track
        /// individual items in the grids when editing.
        /// </summary>
        /// <seealso cref="Rock.Media.MediaElementFileData" />
        [Serializable]
        private class MediaElementFileDataStateful : MediaElementFileData
        {
            [JsonIgnore]
            public Guid Guid { get; set; } = Guid.NewGuid();
        }

        /// <summary>
        /// Provides an additional Guid property that can be used to track
        /// individual items in the grids when editing.
        /// </summary>
        /// <seealso cref="Rock.Media.MediaElementThumbnailData" />
        [Serializable]
        private class MediaElementThumbnailDataStateful : MediaElementThumbnailData
        {
            [JsonIgnore]
            public Guid Guid { get; set; } = Guid.NewGuid();
        }

        #endregion File View Support Classes

        #endregion Support Classes
    }
}