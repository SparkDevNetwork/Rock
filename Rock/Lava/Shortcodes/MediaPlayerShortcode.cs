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
using System.IO;
using System.Web;
using Rock.Media;
using Rock.Model;
using Rock.Web.UI;

namespace Rock.Lava.Shortcodes
{
    /// <summary>
    /// Lava shortcode for displaying scripture links
    /// </summary>
    [LavaShortcodeMetadata(
        Name = "Media Player",
        TagName = "mediaplayer",
        Description = "Media Player displays a single URL or a Media Element in a player that can also record metric data.",
        Documentation = DocumentationMetadata,
        Parameters = ParameterNamesMetadata,
        Categories = "C3270142-E72E-4FBF-BE94-9A2505DE7D54" )]
    public class MediaPlayerShortcode : LavaShortcodeBase, ILavaBlock
    {
        #region Attribute Constants

        /// <summary>
        /// The parameter names that are used in the shortcode.
        /// </summary>
        internal static class ParameterKeys
        {
            /// <summary>
            /// This will automatically pause other media players on the page
            /// when this player begins playing. Default is true.
            /// </summary>
            public const string AutoPause = "autopause";

            /// <summary>
            /// If set to true then the player will attempt to automatically
            /// play as soon as it can. This will likely only work if you also
            /// specify the muted parameter as most browsers do not allow auto
            /// play with sound. Default is false.
            /// </summary>
            public const string AutoPlay = "autoplay";

            /// <summary>
            /// The number of days back to look for an existing interaction
            /// for the media element will be used to find where the user
            /// left off previously and resume from that point. Pass value of
            /// -1 to look back forever or value of 0 to never resume. The
            /// default value is 7.
            /// </summary>
            public const string AutoResumeInDays = "autoresumeindays";

            /// <summary>
            /// Enables the click to play and click to pause feature of the
            /// media player on desktop browsers. This allows the user to click
            /// anywhere on the player that isn't a button and play or pause
            /// the video. Default is true.
            /// </summary>
            public const string ClickToPlay = "clicktoplay";

            /// <summary>
            /// The number of days back to look for an existing interaction
            /// for the media element that should be updated. If one is not
            /// found then a new interaction will be created. Pass value of
            /// -1 to look back forever or value of 0 to always create a new
            /// interaction. The default value is 7.
            /// </summary>
            public const string CombinePlayStatisticsInDays = "combineplaystatisticsindays";

            /// <summary>
            /// The user interface controls to make available to the user
            /// during playback. This is a comma separated string of
            /// control identifiers. Use a blank string if you don't want
            /// any controls to show up. Default is "play-large,play,
            /// progress,current-time,mute,volume,captions,settings,pip,
            /// airplay,fullscreen".
            /// </summary>
            public const string Controls = "controls";

            /// <summary>
            /// Enables developer level logging information to the JavaScript
            /// console during operation. Default is false.
            /// </summary>
            public const string Debug = "debug";

            /// <summary>
            /// When enabled the on screen controls will automatically hide
            /// after 2 seconds without user activity. Default is true.
            /// </summary>
            public const string HideControls = "hidecontrols";

            /// <summary>
            /// When enabled the on screen controls will automatically hide
            /// until the media has started playing.
            /// </summary>
            public const string HideControlsStopped = "hidecontrolsstopped";

            /// <summary>
            /// Defines the interaction that should be updated with new session
            /// information. If not provided it will be determined automatically.
            /// </summary>
            public const string InteractionGuid = "interactionguid";

            /// <summary>
            /// Specifies either the id or the guid of the media element to
            /// load from. This will automatically set the video URL. If a
            /// thumbnail URL has not been provided it will be set as well.
            /// </summary>
            public const string Media = "media";

            /// <summary>
            /// If enabled then the media player will initially be muted.
            /// Default value is false.
            /// </summary>
            public const string Muted = "muted";

            /// <summary>
            /// The primary color to use for player elements, such as the play
            /// button. This can be any valid CSS color. Default is to use the
            /// primary brand color of the theme.
            /// </summary>
            public const string PrimaryColor = "primarycolor";

            /// <summary>
            /// The related entity identifier to store with the interaction if
            /// the session is being tracked.
            /// </summary>
            public const string RelatedEntityId = "relatedentityid";

            /// <summary>
            /// The related entity type identifier to store with the
            /// interaction if the session is being tracked.
            /// </summary>
            public const string RelatedEntityTypeId = "relatedentitytypeid";

            /// <summary>
            /// Determines if the video should be resumed if the last playback
            /// position is known. The default value is true.
            /// </summary>
            public const string Resume = "resume";

            /// <summary>
            /// The number of seconds to seek forward or backward when the
            /// fast-forward or rewind controls are clicked. Default is 10.
            /// </summary>
            public const string SeekTime = "seektime";

            /// <summary>
            /// The URL of the media file to be played.
            /// </summary>
            public const string Source = "src";

            /// <summary>
            /// The thumbnail image URL to display before the video starts
            /// playing. This only works with HTML5 style videos, it will
            /// not work with embed links such as YouTube uses.
            /// </summary>
            public const string Thumbnail = "thumbnail";

            /// <summary>
            /// Determines if an anonymous user's session should be tracked
            /// and stored as an Interaction in the system. This is required
            /// to provide play metrics for non-logged in users but will not
            /// provide the resume feature. The default value is true.
            /// </summary>
            public const string TrackAnonymousSession = "trackanonymoussession";

            /// <summary>
            /// Determines if the user's session should be tracked and stored
            /// as an Interaction in the system. This is required to provide
            /// play metrics as well as use the resume feature later. The
            /// default value is true.
            /// </summary>
            public const string TrackSession = "tracksession";

            /// <summary>
            /// Specifies the type of media to be played. Can be either "audio"
            /// or "video". Default is to auto-detect.
            /// </summary>
            public const string Type = "type";

            /// <summary>
            /// The initial volume to start the media player at. This is a
            /// value between 0 and 1, with 1 meaning full volume. Default
            /// is 1.
            /// </summary>
            public const string Volume = "volume";

            /// <summary>
            /// The existing watch map data that will be used to resume the
            /// session and used when updating watch progress.
            /// </summary>
            public const string WatchMap = "watchmap";

            /// <summary>
            /// The width of the media container. By default the container
            /// will be responsive and take up as much space as is available.
            /// However, if you provide a value here you can set an explicit
            /// width in either pixels or percentage.
            /// </summary>
            public const string Width = "width";
        }

        /// <summary>
        /// The parameter names that will be used in the <see cref="LavaShortcodeMetadataAttribute"/>.
        /// </summary>
        internal const string ParameterNamesMetadata = ParameterKeys.AutoPause
            + "," + ParameterKeys.AutoPlay
            + "," + ParameterKeys.AutoResumeInDays
            + "," + ParameterKeys.ClickToPlay
            + "," + ParameterKeys.CombinePlayStatisticsInDays
            + "," + ParameterKeys.Controls
            + "," + ParameterKeys.Debug
            + "," + ParameterKeys.HideControls
            + "," + ParameterKeys.HideControlsStopped
            + "," + ParameterKeys.InteractionGuid
            + "," + ParameterKeys.Media
            + "," + ParameterKeys.Muted
            + "," + ParameterKeys.RelatedEntityId
            + "," + ParameterKeys.RelatedEntityTypeId
            + "," + ParameterKeys.Resume
            + "," + ParameterKeys.PrimaryColor
            + "," + ParameterKeys.SeekTime
            + "," + ParameterKeys.Source
            + "," + ParameterKeys.Thumbnail
            + "," + ParameterKeys.TrackAnonymousSession
            + "," + ParameterKeys.TrackSession
            + "," + ParameterKeys.Type
            + "," + ParameterKeys.Volume
            + "," + ParameterKeys.WatchMap
            + "," + ParameterKeys.Width;

        /// <summary>
        /// The documentation for the shortcode that will be used in the <see cref="LavaShortcodeMetadataAttribute"/>.
        /// </summary>
        internal const string DocumentationMetadata = @"
<p>The world revolves around media. Everywhere you look you find people watching
videos. Your church likely has lots of media files you want to make available
to users. But trying to get those to display on a page is not always easy.
Especially when it needs to be dynamic.</p>

<p>Using this shortcode takes the pain out of finding a media player and making
it work with your site. Let's take a look at how simple this shortcode is to use.</p>

<pre>{[ mediaplayer src:'https://rockrms.blob.core.windows.net/sampledata/podcasting/money-wise.mp4' ]}{[ endmediaplayer ]}</pre>

<p>Isn't that easy? With just that much you get a simple media player on screen
that the user can use to watch a video. Better still, you can also use this to
embed Vimeo and YouTube videos. Just use the same URL you use in the browser to
watch the video in the src parameter.</p>

<p>What is even more amazing is when you use this shortcode along with the
media system in Rock. By passing either the Id number or Guid of a media element
it will automatically get the best video URL to use and the thumbnail URL. Even
better it will automatically track playback sessions and record them in Rock so
you get metrics about the videos being watched.</p>

<pre>{[ mediaplayer media:'18' ]}{[ endmediaplayer ]}</pre>

<p>Let's take a look at some of the parameters and options that are available so
so you can customize this to be exactly what you want.</p>

<ul>
    <li><strong>autopause</strong> (true) - This will automatically pause other media players on the page when this player begins playing.</li>
    <li><strong>autoplay</strong> (false) - If set to true then the player will attempt to automatically play as soon as it can. This will likely only work if you also specify the muted parameter as most browsers do not allow auto play with sound.</li>
    <li><strong>autoresumeindays</strong> (7) - The number of days back to look for an existing interaction for the media element will be used to find where the user left off previously and resume from that point. Pass value of -1 to look back forever or value of 0 to never resume.</li>
    <li><strong>clicktoplay</strong> (true) - Enables the click to play and click to pause feature of the media player on desktop browsers. This allows the user to click anywhere on the player that isn't a button and play or pause the video.</li>
    <li><strong>combineplaystatisticsindays</strong> (7) - The number of days back to look for an existing interaction for the media element that should be updated. If one is not found then a new interaction will be created. Pass value of -1 to look back forever or value of 0 to always create a new interaction.</li>
    <li><strong>controls</strong> (play-large,play,progress,current-time,mute,volume,captions,settings,pip,airplay,fullscreen) - The user interface controls to make available to the user during playback. This is a comma separated string of control identifiers. Use a blank string if you don't want any controls to show up.</li>
    <li><strong>debug</strong> (false) - Enables developer level logging information to the JavaScript console during operation.</li>
    <li><strong>hidecontrols</strong> (true) - When enabled the on screen controls will automatically hide after 2 seconds without user activity.</li>
    <li><strong>hidecontrolsstopped</strong> (false) - When enabled the on screen controls will automatically hide until the media has started playing.</li>
    <li><strong>interactionguid</strong> - Defines the interaction that should be updated with new session information.</li>
    <li><strong>media</strong> - Specifies either the id or the guid of the media element to load from. This will automatically set the video URL. If a thumbnail URL has not been provided it will be set as well.</li>
    <li><strong>muted</strong> (false) - If enabled then the media player will initially be muted.</li>
    <li><strong>primarycolor</strong> - The primary color to use for player elements, such as the play button. This can be any valid CSS color. Default is to use the primary brand color of the theme.</li>
    <li><strong>relatedentityid</strong> - The related entity identifier to store with the interaction if the session is being tracked.</li>
    <li><strong>relatedentitytypeid</strong> - The related entity type identifier to store with the interaction if the session is being tracked.</li>
    <li><strong>resume</strong> (true) - Determines if the video should be resumed if the last playback position is known.</li>
    <li><strong>seektime</strong> (10) - The number of seconds to seek forward or backward when the fast-forward or rewind controls are clicked.</li>
    <li><strong>src</strong> - The URL of the media file to be played.</li>
    <li><strong>thumbnail</strong> - The thumbnail image URL to display before the video starts playing. This only works with HTML5 style videos, it will not work with embed links such as YouTube uses.</li>
    <li><strong>trackanonymoussession</strong> (true) - Determines if an anonymous user's session should be tracked and stored as an Interaction in the system. This is required to provide play metrics for non-logged in users but will not provide the resume feature.
    <li><strong>tracksession</strong> (true) - Determines if the user's session should be tracked and stored as an Interaction in the system. This is required to provide play metrics as well as use the resume feature later.</li>
    <li><strong>type</strong> - Specifies the type of media to be played. Can be either ""audio"" or ""video"". Default is to auto-detect.</li>
    <li><strong>volume</strong> (1) - The initial volume to start the media player at. This is a value between 0 and 1, with 1 meaning full volume.</li>
    <li><strong>watchmap</strong> - The existing watch map data that will be used to resume the session and used when updating watch progress.
    <li><strong>width</strong> - The width of the media container. By default the container will be responsive and take up as much space as is available. However, if you provide a value here you can set an explicit width in either pixels or percentage.</li>
</ul>

<p>When customizing which UI controls to use, there are a number of options available.</p>

<ul>
    <li><strong>play-large</strong> - The large play button in the center.</li>
    <li><strong>restart</strong> - Restart playback at beginning.</li>
    <li><strong>rewind</strong> - Rewind by the seek time.</li>
    <li><strong>play</strong> - Play or pause playback.</li>
    <li><strong>fast-forward</strong> - Fast forward by the seek time.</li>
    <li><strong>progress</strong> - The progress bar and scrubber for playback and buffering.</li>
    <li><strong>current-time</strong> - The current time of playback.</li>
    <li><strong>duration</strong> - The full duration of the media.</li>
    <li><strong>mute</strong> - Toggle mute on and off.</li>
    <li><strong>volume</strong> - Volume control.</li>
    <li><strong>captions</strong> - Toggle captions on and off.</li>
    <li><strong>settings</strong> - Settings menu to adjust playback speed and other options depending on media.</li>
    <li><strong>pip</strong> - Picture-in-picture (currently Safari only).</li>
    <li><strong>airplay</strong> - Airplay button (currently Safari only).</li>
    <li><strong>download</strong> - Show a download button with a link to the media source. Note: This is only supported for certain media and is not supported for YouTube embed, Vimeo embed, HLS, etc.</li>
    <li><strong>fullscreen</strong> - Toggle fullscreen playback.</li>
</ul>
";

        #endregion

        #region Properties

        /// <summary>
        /// Specifies the type of Liquid element for this shortcode.
        /// </summary>
        public override LavaShortcodeTypeSpecifier ElementType
        {
            get
            {
                return LavaShortcodeTypeSpecifier.Block;
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// The markup that was passed after the shortcode name and before the closing ]}.
        /// </summary>
        string _markup = string.Empty;

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="markup">The markup.</param>
        /// <param name="tokens">The tokens.</param>
        /// <exception cref="System.Exception">Could not find the variable to place results in.</exception>
        public override void OnInitialize( string tagName, string markup, List<string> tokens )
        {
            _markup = markup;

            base.OnInitialize( tagName, markup, tokens );
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public override void OnRender( ILavaRenderContext context, TextWriter result )
        {
            int? visitorAliasId = null;
            var currentPerson = GetCurrentPerson( context );
            var settings = GetAttributesFromMarkup( _markup, context );

            // Attempt to get the session guid
            Guid? sessionGuid;
            try
            {
                sessionGuid = ( HttpContext.Current?.Handler as RockPage )?.Session["RockSessionId"]?.ToString().AsGuidOrNull();
            }
            catch
            {
                sessionGuid = null;
            }

            if ( currentPerson == null )
            {
                visitorAliasId = GetVisitorAliasId( context );
            }

            RenderToWriter( settings.Attributes, currentPerson, visitorAliasId, sessionGuid, result );
        }

        internal static LavaElementAttributes GetAttributesFromMarkup( string markup, ILavaRenderContext context )
        {
            // Parse attributes string.
            var settings = LavaElementAttributes.NewFromMarkup( markup, context );

            // Add default settings.
            settings.AddOrIgnore( ParameterKeys.AutoPause, "true" );
            settings.AddOrIgnore( ParameterKeys.AutoPlay, "false" );
            settings.AddOrIgnore( ParameterKeys.AutoResumeInDays, "7" );
            settings.AddOrIgnore( ParameterKeys.ClickToPlay, "true" );
            settings.AddOrIgnore( ParameterKeys.CombinePlayStatisticsInDays, "7" );
            settings.AddOrIgnore( ParameterKeys.Controls, "play-large,play,progress,current-time,mute,volume,captions,settings,pip,airplay,fullscreen" );
            settings.AddOrIgnore( ParameterKeys.Debug, "false" );
            settings.AddOrIgnore( ParameterKeys.HideControls, "true" );
            settings.AddOrIgnore( ParameterKeys.HideControlsStopped, "false" );
            settings.AddOrIgnore( ParameterKeys.InteractionGuid, "" );
            settings.AddOrIgnore( ParameterKeys.Media, "" );
            settings.AddOrIgnore( ParameterKeys.Muted, "false" );
            settings.AddOrIgnore( ParameterKeys.PrimaryColor, "var(--color-primary)" );
            settings.AddOrIgnore( ParameterKeys.RelatedEntityId, "" );
            settings.AddOrIgnore( ParameterKeys.RelatedEntityTypeId, "" );
            settings.AddOrIgnore( ParameterKeys.Resume, "true" );
            settings.AddOrIgnore( ParameterKeys.SeekTime, "10" );
            settings.AddOrIgnore( ParameterKeys.Source, "" );
            settings.AddOrIgnore( ParameterKeys.Thumbnail, "" );
            settings.AddOrIgnore( ParameterKeys.TrackAnonymousSession, "true" );
            settings.AddOrIgnore( ParameterKeys.TrackSession, "true" );
            settings.AddOrIgnore( ParameterKeys.Type, "" );
            settings.AddOrIgnore( ParameterKeys.Volume, "1" );
            settings.AddOrIgnore( ParameterKeys.WatchMap, "" );
            settings.AddOrIgnore( ParameterKeys.Width, "" );

            return settings;
        }

        /// <summary>
        /// Gets the current person.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static Person GetCurrentPerson( ILavaRenderContext context )
        {
            // First check for a person override value included in lava context
            var currentPerson = context.GetMergeField( "CurrentPerson", null ) as Person;

            if ( currentPerson == null )
            {
                var httpContext = HttpContext.Current;

                if ( context != null
                    && httpContext != null
                    && httpContext.Items.Contains( "CurrentPerson" ) )
                {
                    currentPerson = httpContext.Items["CurrentPerson"] as Person;
                }
            }

            return currentPerson;
        }

        /// <summary>
        /// Gets the visitor alias identifier.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static int? GetVisitorAliasId( ILavaRenderContext context )
        {
            // First check for a person override value included in lava context
            var personAlias = context.GetMergeField( "CurrentVisitor", null ) as PersonAlias;

            return personAlias?.Id;
        }

        /// <summary>
        /// Renders the shortcode contents to the writer.
        /// </summary>
        /// <param name="parms">The parameters that will be used to construct the content.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="personAliasId">The optional person alias identifier if <paramref name="currentPerson"/> is <c>null</c>.</param>
        /// <param name="rockSessionGuid">The current Rock session unique identifier.</param>
        /// <param name="result">The writer that output should be written to.</param>
        internal static void RenderToWriter( Dictionary<string, string> parms, Person currentPerson, int? personAliasId, Guid? rockSessionGuid, TextWriter result )
        {
            var options = new MediaPlayerOptions
            {
                Autoplay = parms[ParameterKeys.AutoPlay].AsBoolean(),
                Autopause = parms[ParameterKeys.AutoPause].AsBoolean( true ),
                ClickToPlay = parms[ParameterKeys.ClickToPlay].AsBoolean( true ),
                Controls = parms[ParameterKeys.Controls],
                Debug = parms[ParameterKeys.Debug].AsBoolean(),
                HideControls = parms[ParameterKeys.HideControls].AsBoolean( true ),
                InteractionGuid = parms[ParameterKeys.InteractionGuid].AsGuidOrNull(),
                Map = parms[ParameterKeys.WatchMap],
                MediaElementGuid = parms[ParameterKeys.Media].AsGuidOrNull(),
                MediaUrl = parms[ParameterKeys.Source],
                Muted = parms[ParameterKeys.Muted].AsBoolean(),
                PosterUrl = parms[ParameterKeys.Thumbnail],
                RelatedEntityId = parms[ParameterKeys.RelatedEntityId].AsIntegerOrNull(),
                RelatedEntityTypeId = parms[ParameterKeys.RelatedEntityTypeId].AsIntegerOrNull(),
                ResumePlaying = parms[ParameterKeys.Resume].AsBooleanOrNull(),
                SeekTime = parms[ParameterKeys.SeekTime].AsIntegerOrNull() ?? 10,
                SessionGuid = rockSessionGuid,
                TrackProgress = true,
                Type = parms[ParameterKeys.Type],
                Volume = parms[ParameterKeys.Volume].AsDoubleOrNull() ?? 1.0,
                WriteInteraction = currentPerson != null || personAliasId.HasValue
                    ? parms[ParameterKeys.TrackSession].AsBoolean( true )
                    : parms[ParameterKeys.TrackAnonymousSession].AsBoolean( true )
            };

            // Get the rest of the parameters in easy to access variables.
            var mediaId = parms[ParameterKeys.Media].AsIntegerOrNull();
            var autoResumeInDays = parms[ParameterKeys.AutoResumeInDays].AsIntegerOrNull() ?? 7;
            var combinePlayStatisticsInDays = parms[ParameterKeys.CombinePlayStatisticsInDays].AsIntegerOrNull() ?? 7;
            var primaryColor = parms[ParameterKeys.PrimaryColor];
            var width = parms[ParameterKeys.Width];

            options.UpdateValuesFromMedia( mediaId, options.MediaElementGuid, autoResumeInDays, combinePlayStatisticsInDays, currentPerson, personAliasId );

            // Ensure the resume playing value is set.
            options.ResumePlaying = options.ResumePlaying ?? true;

            var elementId = $"mediaplayer_{Guid.NewGuid()}";

            // Construct the CSS style for this media player.
            var style = $"--plyr-color-main: {( primaryColor.IsNotNullOrWhiteSpace() ? primaryColor : "var(--color-primary)" )};";
            style += ( width.IsNotNullOrWhiteSpace() ? $" width:{width}" : "" );

            // Construct the JavaScript to initialize the player.
            var script = $@"<script>
(function() {{
    new Rock.UI.MediaPlayer(""#{elementId}"", {options.ToJson( indentOutput: false )});
}})();
</script>";

            var classAttribute = string.Empty;

            if ( parms[ParameterKeys.HideControlsStopped].AsBoolean( false ) )
            {
                classAttribute = " class=\"mediaplayer-hidecontrolswhenstopped\"";
            }

            result.WriteLine( $"<div id=\"{elementId}\" style=\"{style}\"${classAttribute}></div>" );
            result.WriteLine( script );

            // If we have a RockPage related to the current request then
            // register all the JS and CSS links we need.
            if ( HttpContext.Current != null && HttpContext.Current.Handler is System.Web.UI.Page page )
            {
                Rock.Web.UI.Controls.MediaPlayer.AddLinksForMediaToPage( options.MediaUrl, page );
            }
        }

        #endregion
    }
}
