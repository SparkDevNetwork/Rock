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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Media;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Presents a media player that plays either a URL or a
    /// <see cref="Rock.Model.MediaElement"/>.
    /// </summary>
    /// <seealso cref="System.Web.UI.WebControls.WebControl" />
    /// <seealso cref="Rock.Web.UI.Controls.IRockControl" />
    public class MediaPlayer : CompositeControl, IRockControl
    {
        #region Constants

        /// <summary>
        /// The default player controls if not specified by the user.
        /// </summary>
        private static readonly string DefaultPlayerControls = string.Join( ",",
            MediaPlayerControls.PlayLarge,
            MediaPlayerControls.Play,
            MediaPlayerControls.Progress,
            MediaPlayerControls.CurrentTime,
            MediaPlayerControls.Mute,
            MediaPlayerControls.Volume,
            MediaPlayerControls.Captions,
            MediaPlayerControls.Settings,
            MediaPlayerControls.PictureInPicture,
            MediaPlayerControls.Airplay,
            MediaPlayerControls.Fullscreen
        );

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether to automatically pause
        /// other media players on the page when this player begins playing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if other players should be paused; otherwise, <c>false</c>.
        /// </value>
        [DefaultValue( true )]
        public bool AutoPause
        {
            get => ViewState[nameof( AutoPause )] as bool? ?? true;
            set => ViewState[nameof( AutoPause )] = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to attempt to automatically
        /// play as soon as it can. This will likely only work if you also
        /// specify the <see cref="Muted"/> parameter as most browsers do not
        /// allow auto play with sound.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the player should attempt to play automatically; otherwise, <c>false</c>.
        /// </value>
        [DefaultValue( false )]
        public bool AutoPlay
        {
            get => ViewState[nameof( AutoPlay )] as bool? ?? false;
            set => ViewState[nameof( AutoPlay )] = value;
        }

        /// <summary>
        /// Gets or sets the number of days back to look for an existing interaction
        /// for the media element will be used to find where the user left off
        /// previously and resume from that point. Set to <c>0</c> to never resume.
        /// </summary>
        /// <value>
        /// The number of days to use when attempting to detect the auto-resume position.
        /// </value>
        [DefaultValue( 7 )]
        public int AutoResumeInDays
        {
            get => ViewState[nameof( AutoResumeInDays )] as int? ?? 7;
            set => ViewState[nameof( AutoResumeInDays )] = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user will be allowed to
        /// click anywhere on the media player to play or pause the playback.
        /// </summary>
        /// <value>
        ///   <c>true</c> if click-to-play should be supported; otherwise, <c>false</c>.
        /// </value>
        [DefaultValue( true )]
        public bool ClickToPlay
        {
            get => ViewState[nameof( ClickToPlay )] as bool? ?? true;
            set => ViewState[nameof( ClickToPlay )] = value;
        }

        /// <summary>
        /// Gets or sets the number of days back to look for an existing
        /// interaction for the media element that should be updated. If one
        /// is not found then a new interaction will be created. Set to
        /// <c>0</c> to always create a new interaction.
        /// </summary>
        /// <value>
        /// The number of days back to look for an existing interaction to
        /// update when saving playback progress.
        /// </value>
        [DefaultValue( 7 )]
        public int CombinePlayStatisticsInDays
        {
            get => ViewState[nameof( CombinePlayStatisticsInDays )] as int? ?? 7;
            set => ViewState[nameof( CombinePlayStatisticsInDays )] = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether debug information should
        /// be written to the JavaScript console.
        /// </summary>
        /// <value>
        ///   <c>true</c> if debug logging is enabled; otherwise, <c>false</c>.
        /// </value>
        [DefaultValue( false )]
        public bool Debug
        {
            get => ViewState[nameof( Debug )] as bool? ?? false;
            set => ViewState[nameof( Debug )] = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user interface controls
        /// should be automatically hidden after a brief period of inactivity.
        /// </summary>
        /// <value>
        ///   <c>true</c> if user interface controls should be automatically hidden; otherwise, <c>false</c>.
        /// </value>
        [DefaultValue( true )]
        public bool HideControls
        {
            get => ViewState[nameof( HideControls )] as bool? ?? true;
            set => ViewState[nameof( HideControls )] = value;
        }

        /// <summary>
        /// Gets or sets the maximum width of the video.
        /// </summary>
        /// <value>
        /// The maximum width of the video.
        /// </value>
        public string MaxVideoWidth
        {
            get => ViewState[nameof( MaxVideoWidth )] as string;
            set => ViewState[nameof( MaxVideoWidth )] = value;
        }

        /// <summary>
        /// Gets or sets the media element identifier to use for playback and
        /// recording of interaction data.
        /// </summary>
        /// <value>
        /// The media element identifier to use for playback and recording of
        /// interaction data.
        /// </value>
        public int? MediaElementId
        {
            get => ViewState[nameof( MediaElementId )] as int?;
            set => ViewState[nameof( MediaElementId )] = value;
        }

        /// <summary>
        /// Gets or sets the type of media player to display.
        /// </summary>
        /// <value>
        /// The type of media player to display.
        /// </value>
        [DefaultValue( MediaPlayerInterfaceType.Audio )]
        public MediaPlayerInterfaceType MediaType
        {
            get => ViewState[nameof( MediaType )] as MediaPlayerInterfaceType? ?? MediaPlayerInterfaceType.Automatic;
            set => ViewState[nameof( MediaType )] = value;
        }

        /// <summary>
        /// Gets or sets the URL to use for playback. This will be overridden
        /// by <see cref="MediaElementId"/> if it is set.
        /// </summary>
        /// <value>
        /// The URL of the media to be played.
        /// </value>
        public string MediaUrl
        {
            get => ViewState[nameof( MediaUrl )] as string;
            set => ViewState[nameof( MediaUrl )] = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the media player will be
        /// initially muted on page load.
        /// </summary>
        /// <value>
        ///   <c>true</c> if initially muted; otherwise, <c>false</c>.
        /// </value>
        [DefaultValue( false )]
        public bool Muted
        {
            get => ViewState[nameof( Muted )] as bool? ?? false;
            set => ViewState[nameof( Muted )] = value;
        }

        /// <summary>
        /// Gets or sets the user interface controls to display on the player.
        /// </summary>
        /// <value>
        /// Comma delimited list of values from <see cref="MediaPlayerControls"/>.
        /// </value>
        public string PlayerControls
        {
            get => ViewState[nameof( PlayerControls )] as string ?? DefaultPlayerControls;
            set => ViewState[nameof( PlayerControls )] = value;
        }

        /// <summary>
        /// Gets or sets the primary color to use for the player. This can be
        /// any CSS valid color such as "red" or "#ff0000".
        /// </summary>
        /// <value>
        /// The primary color to use for the player.
        /// </value>
        public string PrimaryColor
        {
            get => ViewState[nameof( PrimaryColor )] as string ?? "var(--color-primary)";
            set => ViewState[nameof( PrimaryColor )] = value;
        }

        /// <summary>
        /// Gets or sets the related entity identifier to store with the
        /// interaction if the session is being tracked.
        /// </summary>
        /// <value>
        /// The related entity identifier for the interaction.
        /// </value>
        public int? RelatedEntityId
        {
            get => ViewState[nameof( RelatedEntityId )] as int?;
            set => ViewState[nameof( RelatedEntityId )] = value;
        }

        /// <summary>
        /// Gets or sets the related entity type identifier to store with
        /// the interaction if the session is being tracked.
        /// </summary>
        /// <value>
        /// The related entity type identifier for the interaction.
        /// </value>
        public int? RelatedEntityTypeId
        {
            get => ViewState[nameof( RelatedEntityTypeId )] as int?;
            set => ViewState[nameof( RelatedEntityTypeId )] = value;
        }

        /// <summary>
        /// Gets or sets the required watch percentage.
        /// </summary>
        /// <value>
        /// The required watch percentage as a value between 0 and 1.0.
        /// </value>
        [DefaultValue( 0.9 )]
        public double RequiredWatchPercentage
        {
            get => ViewState[nameof( RequiredWatchPercentage )] as double? ?? 0.9;
            set => ViewState[nameof( RequiredWatchPercentage )] = value;
        }

        /// <summary>
        /// Gets or sets the number of seconds to seek forward or backward
        /// when the fast-forward or rewind controls are clicked.
        /// </summary>
        /// <value>
        /// The number of seconds to fast forward or rewind.
        /// </value>
        [DefaultValue( 10 )]
        public int SeekTime
        {
            get => ViewState[nameof( SeekTime )] as int? ?? 10;
            set => ViewState[nameof( SeekTime )] = value;
        }

        /// <summary>
        /// Gets or sets the thumbnail image URL to display before the video
        /// starts playing. This only works with HTML5 style videos, it will
        /// not work with embed links such as YouTube uses.
        /// </summary>
        /// <value>
        /// The thumbnail image URL to display before playback starts.
        /// </value>
        public string ThumbnailUrl
        {
            get => ViewState[nameof( ThumbnailUrl )] as string;
            set => ViewState[nameof( ThumbnailUrl )] = value;
        }

        /// <summary>
        /// Determines if an anonymous playback session should be tracked and stored as an
        /// <see cref="Rock.Model.Interaction"/> in the system. This is required
        /// to provide play metrics.
        /// </summary>
        /// <value>
        ///   <c>true</c> if an anonymous playback session should be tracked in the Interactions table; otherwise, <c>false</c>.
        /// </value>
        [DefaultValue( true )]
        public bool TrackAnonymousSession
        {
            get => ViewState[nameof( TrackAnonymousSession )] as bool? ?? true;
            set => ViewState[nameof( TrackAnonymousSession )] = value;
        }

        /// <summary>
        /// Determines if the user's session should be tracked and stored as an
        /// <see cref="Rock.Model.Interaction"/> in the system. This is required
        /// to provide play metrics as well as use the resume feature later.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the playback session should be tracked in the Interactions table; otherwise, <c>false</c>.
        /// </value>
        [DefaultValue( true )]
        public bool TrackSession
        {
            get => ViewState[nameof( TrackSession )] as bool? ?? true;
            set => ViewState[nameof( TrackSession )] = value;
        }

        /// <summary>
        /// Gets or sets the initial volume for playback.
        /// </summary>
        /// <value>
        /// The initial volume for playback between 0 and 1.0.
        /// </value>
        [DefaultValue( 1d )]
        public double Volume
        {
            get => ViewState[nameof( Volume )] as double? ?? 1d;
            set => ViewState[nameof( Volume )] = value;
        }

        /// <summary>
        /// Gets or sets the percentage of the video that has been watched.
        /// </summary>
        /// <value>
        /// The percentage of the video that has been watched between 0 and 1.0.
        /// </value>
        public double WatchedPercentage
        {
            get
            {
                EnsureChildControls();

                return _hfWatchedPercentage.Value.AsDoubleOrNull() ?? 0d;
            }
        }

        #endregion

        #region IRockControl implementation

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the label." )
        ]
        public string Label
        {
            get { return ViewState["Label"] as string ?? string.Empty; }
            set { ViewState["Label"] = value; }
        }

        /// <summary>
        /// Gets or sets the form group class.
        /// </summary>
        /// <value>
        /// The form group class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        Description( "The CSS class to add to the form-group div." )
        ]
        public string FormGroupCssClass
        {
            get { return ViewState["FormGroupCssClass"] as string ?? string.Empty; }
            set { ViewState["FormGroupCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The help block." )
        ]
        public string Help
        {
            get
            {
                return HelpBlock?.Text ?? string.Empty;
            }
            set
            {
                if ( HelpBlock != null )
                {
                    HelpBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the warning text.
        /// </summary>
        /// <value>
        /// The warning text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The warning block." )
        ]
        public string Warning
        {
            get
            {
                return WarningBlock?.Text ?? string.Empty;
            }
            set
            {
                if ( WarningBlock != null )
                {
                    WarningBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RockTextBox"/> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "false" ),
        Description( "Is the value required?" )
        ]
        public virtual bool Required
        {
            get { return ViewState["Required"] as bool? ?? false; }
            set { ViewState["Required"] = value; }
        }

        /// <summary>
        /// Gets or sets the required error message.  If blank, the LabelName name will be used
        /// </summary>
        /// <value>
        /// The required error message.
        /// </value>
        public string RequiredErrorMessage
        {
            get
            {
                return ViewState["RequiredErrorMessage"] as string ?? "You did not watch enough of the video in order for it to be considered complete.";
            }

            set
            {
                ViewState["RequiredErrorMessage"] = value;

                if ( _cvWatchedPercentage != null )
                {
                    _cvWatchedPercentage.ErrorMessage = value;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsValid
        {
            get
            {
                return !Required || WatchedPercentage >= RequiredWatchPercentage;
            }
        }

        /// <summary>
        /// Gets or sets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        public HelpBlock HelpBlock { get; set; }

        /// <summary>
        /// Gets or sets the warning block.
        /// </summary>
        /// <value>
        /// The warning block.
        /// </value>
        public WarningBlock WarningBlock { get; set; }

        /// <summary>
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get
            {
                return ViewState["ValidationGroup"] as string ?? string.Empty;
            }
            set
            {
                ViewState["ValidationGroup"] = value;

                if ( _cvWatchedPercentage != null )
                {
                    _cvWatchedPercentage.ValidationGroup = value;
                }
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// The player options that should be passed to javascript.
        /// </summary>
        private MediaPlayerOptions _playerOptions;

        #endregion

        #region Controls

        /// <summary>
        /// The hidden field that contains the watched percentage.
        /// </summary>
        private HiddenField _hfWatchedPercentage;

        /// <summary>
        /// The custom validator for this instance.
        /// </summary>
        private CustomValidator _cvWatchedPercentage;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaPlayer"/> class.
        /// </summary>
        public MediaPlayer() : base()
        {
            HelpBlock = new HelpBlock();
            WarningBlock = new WarningBlock();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds the page links for media playback to the page.
        /// </summary>
        /// <param name="mediaUrl">The media URL.</param>
        /// <param name="page">The page.</param>
        public static void AddLinksForMediaToPage( string mediaUrl, Page page )
        {
            /*
                6/16/2023 - JPH

                Make sure we always "fingerprint" these resources so we can control the browser's caching behavior.
                The libraries represented within the plyr.js file (Plyr, hls.js) are doing the heavy lifting for
                us with respect to video streaming compatibility with modern browsers, and we might need to be able
                to instruct individuals how to manually deploy hotfixes by overwriting the final JS file. Without
                fingerprinting in place, the browser will cache this file far too aggressively.

                Reason: Issue #5445: v15 media player endless buffering with tracking through video
                (https://github.com/SparkDevNetwork/Rock/issues/5445)
            */
            RockPage.AddScriptLink( page, "~/Scripts/Rock/plyr.js", true );
            RockPage.AddScriptLink( page, "~/Scripts/Rock/UI/mediaplayer/mediaplayer.js", true );
        }

        /// <inheritdoc/>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            _hfWatchedPercentage = new HiddenField
            {
                ID = this.ID + "_hfWatchedPercentage"
            };
            Controls.Add( _hfWatchedPercentage );

            _cvWatchedPercentage = new CustomValidator
            {
                ID = ID + "_cfv",
                CssClass = "validation-error help-inline js-media-player-validator",
                ClientValidationFunction = "Rock.controls.mediaplayer.clientValidate",
                ErrorMessage = RequiredErrorMessage,
                Enabled = true,
                Display = ValidatorDisplay.Dynamic,
                ValidationGroup = ValidationGroup
            };
            Controls.Add( _cvWatchedPercentage );
        }

        /// <inheritdoc/>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( Visible )
            {
                RockControlHelper.RenderControl( this, writer );
            }
        }

        /// <inheritdoc/>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            var rockPage = Page as RockPage;

            // Build the options we have been given by the user.
            _playerOptions = new MediaPlayerOptions
            {
                Autoplay = AutoPlay,
                Autopause = AutoPause,
                ClickToPlay = ClickToPlay,
                Controls = string.Join( ",", PlayerControls ),
                Debug = Debug,
                HideControls = HideControls,
                MediaUrl = MediaUrl,
                Muted = Muted,
                PosterUrl = ThumbnailUrl,
                RelatedEntityId = RelatedEntityId,
                RelatedEntityTypeId = RelatedEntityTypeId,
                SeekTime = SeekTime,
                TrackProgress = true,
                Type = MediaType == MediaPlayerInterfaceType.Automatic ? string.Empty : MediaType.ToString().ToLower(),
                Volume = Volume,
                WriteInteraction = rockPage?.CurrentPerson != null ? TrackSession : TrackAnonymousSession
            };

            // Update the options with any values from the MediaElement.
            _playerOptions.UpdateValuesFromMedia( MediaElementId, null, AutoResumeInDays, CombinePlayStatisticsInDays, rockPage?.CurrentPerson, rockPage?.CurrentVisitor?.Id );

            // Add the CSS and JavaScript links to the page.
            AddLinksForMediaToPage( _playerOptions.MediaUrl, Page );

            // Build the final options that will go to our JavaScript
            // initialization function.
            var options = new Dictionary<string, object>
            {
                ["id"] = ClientID,
                ["required"] = Required,
                ["progressId"] = _hfWatchedPercentage.ClientID,
                ["requiredPercentage"] = RequiredWatchPercentage,
                ["requiredErrorMessage"] = GetValidatorErrorMessage(),
                ["playerId"] = $"{ClientID}_player"
            };

            // Construct the JavaScript to initialize the player.
            var script = $@"
;(function() {{
    new Rock.controls.mediaplayer.initialize({options.ToJson()});
}})();";

            ScriptManager.RegisterStartupScript( this, GetType(), "media-player-" + ClientID, script, true );
        }

        /// <summary>
        /// Renders the base control.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public virtual void RenderBaseControl( HtmlTextWriter writer )
        {
            // add ace.js on demand only when there will be a codeeditor rendered
            if ( ScriptManager.GetCurrent( this.Page ).IsInAsyncPostBack )
            {
                ScriptManager.RegisterClientScriptInclude( this.Page, this.Page.GetType(), "plyr-include", ResolveUrl( "~/Scripts/Rock/plyr.js" ) );
                ScriptManager.RegisterClientScriptInclude( this.Page, this.Page.GetType(), "mediaplayer-include", ResolveUrl( "~/Scripts/Rock/UI/mediaplayer/mediaplayer.js" ) );
            }

            // Render the container for everything.
            writer.AddAttribute( HtmlTextWriterAttribute.Id, ClientID );
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "js-media-player" + CssClass );
            writer.AddAttribute( "data-player-options", _playerOptions.ToJson() );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // Render the container for the media player control.
            writer.AddAttribute( HtmlTextWriterAttribute.Id, $"{ClientID}_player" );
            writer.AddStyleAttribute( "--plyr-color-main", PrimaryColor );
            if ( MaxVideoWidth.IsNotNullOrWhiteSpace() )
            {
                writer.AddStyleAttribute( "max-width", MaxVideoWidth );
            }
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.RenderEndTag();

            _hfWatchedPercentage.RenderControl( writer );
            _cvWatchedPercentage.RenderControl( writer );

            writer.RenderEndTag();
        }

        /// <summary>
        /// Gets the validator error message.
        /// </summary>
        /// <returns>A string that contains the text to display if validation fails.</returns>
        private string GetValidatorErrorMessage()
        {
            if ( !string.IsNullOrWhiteSpace( RequiredErrorMessage ) )
            {
                return RequiredErrorMessage;
            }
            else
            {
                return "You did not watch enough of the video in order for it to be considered complete.";
            }
        }

        #endregion
    }
}
