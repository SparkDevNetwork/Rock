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

/*
    ========================================
    SPECIAL NOTES
    ========================================

    1. Resume Playing
    -------------------
    4/6/2020 - JME
    Resume playing works on all video providers (HTML5, YouTube, Vimeo), but the
    scrub bar will only auto forward before play on HTML5. It's possible we could
    add support for this with YouTube by implementing an event for 'onstatechange'
    and look for value 5 (video cued).
    https://stackoverflow.com/questions/4718404/retrieve-player-state-with-multiple-youtube-players-on-a-page

    2. CDN Resources
    ------------------
    6/8/2021 - DSH
    The following CDN resources are used by this script and must be included on
    the page in order for this script to work correctly:
    https://cdnjs.cloudflare.com/ajax/libs/plyr/3.6.8/plyr.min.js
    https://cdnjs.cloudflare.com/ajax/libs/plyr/3.6.8/plyr.min.css
    https://cdnjs.cloudflare.com/ajax/libs/hls.js/1.0.2/hls.min.js



    ========================================
    */

// Since we aren't building as a module, we can't use standard imports.
// Fake it so TypeScript knows that Hls is a global variable.
declare var Hls: any;

/* eslint-disable @typescript-eslint/no-namespace */
namespace Rock.UI {
    /**
     * The options that can be passed into the MediaPlayer
     */
    interface Options {
        /** The URL of the media to be played. */
        mediaUrl: string;

        /**
         * The URL of the poster image to use before playback begins. Only
         * valid with HTML5 (non-embed) video sources.
         */
        posterUrl?: string;

        /** The type of player to initialize. Defaults to auto-detect. */
        type?: undefined | "video" | "audio";

        /** True if watch tracking should be enabled. */
        trackProgress: boolean;

        /** True if the media should be resumed where the user left off in the watch map. */
        resumePlaying: boolean;

        /** The title of the media. */
        title?: string;

        /** The controls elements to display over the media. */
        controls: string;

        /**
         * Attempts to automatically start playing the media. Probably won't
         * work unless you also specify muted=true.
         */
        autoplay: boolean;

        /** Automatically pauses other RockMediaPlayers on the same page. */
        autopause: boolean;

        /** The number of seconds to skip forward or backwards when seeking. */
        seekTime: number;

        /** The initial volume between 0 and 1 of the media. */
        volume: number;

        /** True if the media should initially play muted. */
        muted: boolean;

        /** True if clicking anywhere on the video will play/pause the video. */
        clickToPlay: boolean;

        /** True to automatically hide controls after 2 seconds. */
        hideControls: boolean;

        /** True if the interaction should be written. */
        writeInteraction: boolean;

        /** The RLE map data. */
        map: string;

        /** Existing interaction unique identifier to be updated. */
        interactionGuid?: string;

        /**
         * Media Element unique identifier that is being played, required
         * to write interactions.
         */
        mediaElementGuid?: string;

        /** Related EntityTypeId value to include in the interaction. */
        relatedEntityTypeId?: number;

        /** Related EntityId value to include in the interaction. */
        relatedEntityId?: number;

        /** True if debug information should be logged to the console. */
        debug: boolean;

        /** The unique identifier for the current interaciton session. */
        sessionGuid?: string;
    }

    /**
     * The object format that is posted when writing an interaction.
     */
    interface MediaElementInteraction {
        /** The interaction unique identifier to update, if not set a new one is created. */
        InteractionGuid?: string;

        /** The MediaElement unique identifier being watched. */
        MediaElementGuid: string;

        /** Overrides the person this interaction is for. */
        PersonGuid?: string;

        /** Overrides the person this interaction is for. */
        PersonAliasGuid?: string;

        /** The watch map in RLE format. */
        WatchMap: string;

        /** The page identifier of the current Rock page. */
        PageId?: number;

        /** The related EntityTypeId for the interaction. */
        RelatedEntityTypeId?: number;

        /** The related EntityId for the interaction. */
        RelatedEntityId?: number;

        /** The unique session identifier. */
        SessionGuid?: string;

        /** The original page URL. */
        OriginalUrl?: string;
    }

    /**
     * Default options to use if not otherwise specified.
     */
    const DefaultOptions: Options = {
        mediaUrl: "",
        trackProgress: true,
        resumePlaying: true,
        controls: "play-large,play,progress,current-time,mute,volume,captions,settings,pip,airplay,fullscreen",
        autoplay: false,
        autopause: true,
        seekTime: 10,
        volume: 1,
        muted: false,
        clickToPlay: true,
        hideControls: true,
        writeInteraction: true,
        map: "",
        debug: false
    }

    /**
     * The types of events that can be emitted by the media player.
     */
    const enum EventType {
        /** Emitted when the media starts playing. */
        Play = "play",

        /** Emitted when the media is paused. */
        Pause = "pause",

        /** Emitted when the media is ready to play. */
        Ready = "ready",

        /** Emitted when the media has played to completion. */
        Completed = "completed",

        /** Emitted whenever the watch map or watched percentage changes. */
        Progress = "progress",

        /** Emitted whenever an attempt to write an interaction completes. */
        RecordInteraction = "recordinteraction"
    }

    /**
     * Provides a common UI control to play media files in Rock. This has tight
     * integration with MediaElements in Rock.
     **/
    export class MediaPlayer {
        static AllPlayers: Array<MediaPlayer> = [];

        // #region Public Properties

        /** The run length encoded watch map. */
        get map(): string {
            return MediaPlayer.toRle(this.watchBits);
        }

        /**
         * The percentage of the video that has been watched. This is a value
         * between 0 and 1.
         */
        public get percentWatched(): number {
            return this.percentWatchedInternal;
        }

        /**
         * Get the duration of the video.
         */
        public get duration(): number {
            return this.player.duration;
        }

        // #endregion

        // #region Private Properties

        /** The options this media player was initialized with. */
        private options: Options;

        /** The identifier of the timer that is updating the watch map. */
        private timerId: number | null = null;

        /** The core player. */
        private player!: Plyr;

        /** The array of bits tracking how many times each second has been watched. */
        private watchBits: number[] = Array<number>();

        /** The size of the map. */
        private watchBitsInitialized = false;

        /** True if the watchBits property is dirty and needs to be saved. */
        private watchBitsDirty = false;

        /** The percent of the video that has been watched. */
        private percentWatchedInternal = 0;

        /** The previous bit in watchBits that was last updated. */
        private previousPlayBit: number | null = null;

        /** The identifier of the element we were initialized with. */
        private elementId = "";

        /** The element that was found by the elementId. */
        private element!: HTMLElement;

        /** The callbacks that are registered for various events we emit. */
        private callbacks: Record<string, Array<Function>> = {};

        // #endregion

        // #region Constructor

        /**
         * Creates a new media player using the specified element identifier
         * as the placeholder for the player.
         *
         * @param elementSelector The identifier or CSS selector of the placeholder element. All contents will be replaced with the media player.
         * @param options The options to initialize this instance with.
         */
        constructor(elementSelector: string, options: Partial<Options> = {}) {
            this.options = Object.assign({}, DefaultOptions, options);
            this.elementId = elementSelector;
            MediaPlayer.AllPlayers.push(this);

            const element = document.querySelector(elementSelector);

            if (element === null) {
                throw "Element not found to initialize media player with.";
            }

            this.element = <HTMLElement>element;

            if (this.options.mediaUrl === "") {
                throw "Required mediaUrl option was not specified.";
            }

            // Update the URL if it needs to be translated.
            this.options.mediaUrl = this.translateWellKnownUrls(this.options.mediaUrl);

            // Auto-detect the media type if it hasn't been specified.
            if (this.options.type !== "video" && this.options.type !== "audio") {
                this.options.type = this.isAudio(this.options.mediaUrl) ? "audio" : "video";
            }

            // Clear any existing content.
            while (this.element.firstChild !== null) {
                this.element.removeChild(this.element.firstChild);
            }

            this.setupPlayer();
        }

        // #endregion

        // #region Methods

        /**
         * Seek to the specified position in the video.
         *
         * @param positionInSeconds The position in seconds, this can be a floating point number.
         */
        public seek(positionInSeconds: number) {
            this.player.currentTime = positionInSeconds;
        }

        /**
         * Configure the options that will be passed to the initializePlayer
         * function.
         *
         * @param mediaElement The media element that will be used during initialization.
         * @param plyrOptions The options that will be used during initialization.
         * @returns True if initializePlayer should be called, otherwise false.
         */
        private setupPlayer() {
            // Create the media element placeholder. This is needed for Plyr
            // to have a valid constructor call.
            const mediaElement: HTMLMediaElement = document.createElement(this.options.type === "audio" ? "audio" : "video");
            mediaElement.setAttribute("playsinline", "");
            mediaElement.setAttribute("controls", "");
            mediaElement.setAttribute("style", "width: 100%;");
            this.element.appendChild(mediaElement);

            const plyrOptions: Plyr.Options = {
                // Storage causes problems with things like auto-play. It will
                // override whatever we set below which isn't what we want.
                storage: {
                    enabled: false
                },
                youtube: {
                    customControls: false
                },
                autoplay: this.options.autoplay,
                controls: this.options.controls.split(","),
                seekTime: this.options.seekTime,
                volume: this.options.volume,
                muted: this.options.muted,
                clickToPlay: this.options.clickToPlay,
                hideControls: this.options.hideControls,
                fullscreen: {
                    iosNative: true
                }
            };

            if (!this.isHls(this.options.mediaUrl)) {
                this.initializePlayer(mediaElement, plyrOptions);
                return;
            }

            var hls = new Hls();

            // Callback to update the quality of the HLS stream based
            // on the user's selection.
            const updateQuality = function (newQuality: number) {
                if (newQuality === 0) {
                    hls.currentLevel = -1;
                }
                else {
                    hls.levels.forEach((level: any, levelIndex: any) => {
                        if (level.height === newQuality) {
                            hls.currentLevel = levelIndex;
                        }
                    });
                }
            }

            // Once the manifest is parsed we have our quality levels and
            // can finish initializing the player.
            hls.on(Hls.Events.MANIFEST_PARSED, () => {
                // Transform available levels into an array of integers (height values).
                const availableQualities = <number[]>hls.levels.map((l: any) => l.height);

                // Add value for "auto".
                availableQualities.unshift(0);

                // Add new qualities to option
                plyrOptions.quality = {
                    default: 0,
                    options: availableQualities,
                    forced: true,
                    onChange: (e) => updateQuality(e),
                }

                // Override the name of "0p" to be "Auto".
                plyrOptions.i18n = plyrOptions.i18n || {};
                plyrOptions.i18n.qualityLabel = {
                    0: 'Auto'
                };

                // Default to auto quality.
                hls.currentLevel = -1;

                this.initializePlayer(mediaElement, plyrOptions);
            });

            if (this.options.posterUrl !== undefined) {
                mediaElement.setAttribute("poster", this.options.posterUrl);
            }

            hls.loadSource(this.options.mediaUrl);
            hls.attachMedia(mediaElement);
        }

        /**
         * Initialize the player object and prepares for playback.
         *
         * @param mediaElement The media element that will be used for the Plyr instance.
         * @param plyrOptions The options that will be passed to Plyr.
         */
        private initializePlayer(mediaElement: HTMLMediaElement, plyrOptions: Plyr.Options) {
            if (this.isYouTubeEmbed(this.options.mediaUrl) || this.isVimeoEmbed(this.options.mediaUrl) || this.isHls(this.options.mediaUrl)) {
                var control = plyrOptions.controls as string[];
                let index = control.findIndex(d => d === "download"); //find index in your array
                if (index !== -1) {
                    control.splice(index, 1);
                }
                
                plyrOptions.controls = control;
            }

            this.player = new Plyr(mediaElement, plyrOptions);

            // This is a hack to get playback events for youtube videos. Plyr has a bug
            // where it does not initialize the media event listers unless a custom UI
            // is present.
            // Issue: https://github.com/sampotts/plyr/issues/2378
            if (this.isYouTubeEmbed(this.options.mediaUrl)) {
                let listenrsready = false;
                this.player.on("statechange", () => {
                    if (!listenrsready) {
                        listenrsready = true;

                        (this.player as unknown as {
                            listeners: {
                                media: () => void
                            }
                        }).listeners.media();
                    }
                });
            }

            this.writeDebugMessage(`Setting media URL to ${this.options.mediaUrl}`);

            let sourceInfo: Plyr.SourceInfo | null = {
                type: this.options.type === "audio" ? "audio" : "video",
                title: this.options.title,
                sources: [
                    {
                        src: this.options.mediaUrl
                    }
                ],
                poster: this.options.posterUrl
            };

            // Check for any special URL handling.
            if (this.isYouTubeEmbed(this.options.mediaUrl)) {
                sourceInfo.sources[0].provider = "youtube";
            }
            else if (this.isVimeoEmbed(this.options.mediaUrl)) {
                sourceInfo.sources[0].provider = "vimeo";
            }
            else if (this.isHls(this.options.mediaUrl)) {
                // Source has already been set.
                sourceInfo = null;
            }

            if (sourceInfo !== null) {
                this.player.source = sourceInfo;
            }

            this.wireEvents();
        }

        /**
         * Extracts and returns just the filename from the given URL.
         *
         * @param url The URL to be parsed.
         * @returns The filename component of a URL.
         */
        private getFilenameFromUrl(url: string) {
            return url.split('#').shift()?.split('?').shift()?.split('/').pop() || url;
        }

        /**
         * Tests if the string ends with the specified search string.
         *
         * @param haystack The string to be tested.
         * @param needle The string that the haystack must end with.
         * @returns True if the haystack ends with the needle; otherwise false.
         */
        private stringEndsWith(haystack: string, needle: string) {
            return haystack.substr(-needle.length) === needle;
        }

        /**
         * Checks if the URL is for an audio stream. This is a best guess based
         * on the filename of the URL.
         *
         * @param url The URL to check.
         * @returns True if the URL likely specifies an audio stream; otherwise false;
         */
        private isAudio(url: string) {
            const filename = this.getFilenameFromUrl(url).toLowerCase();

            return this.stringEndsWith(filename, ".aac") === true
                || this.stringEndsWith(filename, ".flac") === true
                || this.stringEndsWith(filename, ".mp3") === true
                || this.stringEndsWith(filename, ".wav") === true;
        }

        /**
         * Checks if the URL is for an HLS stream.
         *
         * @param url The URL to check.
         * @returns True if the URL specifies an HLS stream; otherwise false.
         */
        private isHls(url: string) {
            const filename = this.getFilenameFromUrl(url).toLowerCase();

            return this.stringEndsWith(filename, ".m3u8") === true && Hls !== undefined && Hls.isSupported() === true;
        }

        /**
         * Checks if the URL is for a YouTube embed link.
         *
         * @param url The URL to check.
         * @returns True if the URL specifies a YouTube embed link; otherwise false.
         */
        private isYouTubeEmbed(url: string) {
            const pattern = /https?:\/\/www\.youtube\.com\/embed\/([^\?]+)\??/i;
            const match = pattern.exec(url);

            return match !== null;
        }

        /**
         * Checks if the URL is for a Vimeo embed link.
         *
         * @param url The URL to check.
         * @returns True if the URL specifies a Vimeo embed link; otherwise false.
         */
        private isVimeoEmbed(url: string) {
            const pattern = /https?:\/\/player\.vimeo\.com\/video\/([^\?]+)\??/i;
            const match = pattern.exec(url);

            return match !== null;
        }

        /**
         * Examines the URL and translates well known browser URLS into
         * their embed versions. For example, if a user copies and pastes
         * a YouTube URL from their browser it will be translated into the
         * embed version of the URL.
         *
         * @param url The URL to be translated if we know what it is.
         * @returns A modified URL if it was translated or the original url if not.
         */
        private translateWellKnownUrls(url: string) {
            // https://www.youtube.com/watch?v=uQpLrumQP0E
            const youTubePattern = /https?:\/\/(?:www\.)youtube\.com\/watch(?:[?&]v=([^&]+))/i;
            const vimeoPattern = /https?:\/\/vimeo\.com\/([0-9]+)/i;
            const vimeoHLSPattern = /https?:\/\/player\.vimeo\.com\/external\/([0-9]+)\.m3u8(\?.*)?/i;

            // Check if this URL looks like a standard YouTube link from the browser.
            let match = youTubePattern.exec(url);
            if (match !== null) {
                return `https://www.youtube.com/embed/${match[1]}`;
            }

            // Check if this URL looks like a standard Vimeo link from the browser.
            match = vimeoPattern.exec(url);
            if (match !== null) {
                return `https://player.vimeo.com/video/${match[1]}`;
            }

            // Check if this URL looks like a standard Vimeo HLS link from the browser.
            match = vimeoHLSPattern.exec(url);
            if (match !== null) {
                return `https://player.vimeo.com/video/${match[1]}`;
            }

            return url;
        }

        /**
         * Marks the current second as watched.
         */
        private markBitWatched() {
            var playBit = Math.floor(this.player.currentTime);

            // Make sure to not double count. This can occur with timings of play/pause.
            if (playBit == this.previousPlayBit) {
                return;
            }

            // Max times we will count a watch is 9 to keep the map single digits
            if (this.watchBits[playBit] < 9) {
                this.watchBits[playBit]++;
                this.watchBitsDirty = true;
            }

            // Get count of watched bits
            const watchedItemCount = this.watchBits.filter(item => item > 0).length;

            // Calculate percent watched
            this.percentWatchedInternal = watchedItemCount / this.watchBits.length;

            // Update previous played bit
            this.previousPlayBit = playBit;

            this.emit(EventType.Progress);

            this.writeDebugMessage(`RLE: ${MediaPlayer.toRle(this.watchBits)}`);
            this.writeDebugMessage(`Player Time: ${this.player.currentTime}; Current Time: ${playBit}; Percent Watched: ${this.percentWatched}; Unwatched Items: ${this.watchBits.length - watchedItemCount}; Map Size: ${this.watchBits.length}`);
        }

        /**
         * Prepares this instance for playing. Initialize the map and set
         * resume location. This should be called as early as possible once
         * the DOM player element has the media metadata downloaded.
         */
        private prepareForPlay() {
            if (this.watchBitsInitialized !== false) {
                return;
            }

            this.writeDebugMessage("Preparing the player.");

            this.initializeMap();

            this.setResume();

            this.emit(EventType.Ready);
        }

        /**
         * If resumePlaying is enabled, sets the start position at the first gap
         * found in the playback history. If the entire media has already been
         * watched then starts from the beginning.
         */
        private setResume() {
            this.writeDebugMessage(`The media length is ${this.player.duration} seconds.`);

            // Check that resume was configured and that we haven't already
            // watched the entire media.
            if (this.options.resumePlaying === false || this.percentWatched === 1) {
                return;
            }

            let startPosition = 0;

            for (let i = this.watchBits.length - 1; i >= 0; i--) {
                if (this.watchBits[i] !== 0) {
                    startPosition = i + 1;
                    break;
                }
            }

            if (startPosition < this.watchBits.length) {
                this.player.currentTime = startPosition;
            }
            else {
                this.player.currentTime = 0;
            }

            this.writeDebugMessage(`Set starting position at: ${startPosition}`);
            this.writeDebugMessage(`The current starting position is: ${this.player.currentTime}`);
        }

        /**
         * Configures the map to track watching points.
         */
        private initializeMap() {
            // Load an existing map if it exists
            this.loadExistingMap();

            // Confirm map matches media
            this.validateMap();

            this.watchBitsInitialized = true;

            // Write debug message
            this.writeDebugMessage(`Initialize Map (${this.watchBits.length}): ${this.watchBits.join("")}`);
        }

        /**
         * Load an existing map if we have one, otherwise generate a blank
         * map.
         */
        private loadExistingMap() {
            this.writeDebugMessage("Loading existing map.");

            // Check if no existing map provided.
            if (this.options.map === "") {
                this.createBlankMap();
                this.writeDebugMessage("No previous map provided, creating a blank map.");
                return;
            }

            // Map was provided as a string to the map property, we'll need
            // to convert it to an array.
            const existingMapString = this.options.map;
            this.writeDebugMessage(`Map provided in .map property: ${existingMapString}`);

            this.watchBits = MediaPlayer.rleToArray(existingMapString);

            // Get count of watched bits
            const watchedItemCount = this.watchBits.filter(item => item > 0).length;

            // Calculate percent watched
            this.percentWatchedInternal = watchedItemCount / this.watchBits.length;
        }

        /**
         * Checks that the size of the map array matches the length of the
         * media. If not then it creates a new blank map.
         */
        private validateMap() {
            let mediaLength = Math.ceil(this.player.duration);

            if (this.watchBits.length !== mediaLength) {
                this.writeDebugMessage(`Provided map size (${this.watchBits.length}) did not match the media (${mediaLength}). Using a blank map.`);
                this.createBlankMap();
            }
        }

        /**
         * Creates a blank map of the length of the current media
         */
        private createBlankMap() {
            let mapSize = Math.ceil(this.player.duration);

            // Duration will be -1 if the media does not exist.
            if (mapSize < 0) {
                mapSize = 0;
            }

            this.watchBits = new Array(mapSize).fill(0);
            this.percentWatchedInternal = 0;

            this.writeDebugMessage(`Blank map created of size: ${this.watchBits.length}`);
        }

        /**
         * Takes a RLE string and returns a plain number array.
         *
         * @param value The RLE string that contains the map data.
         */
        private static rleToArray(value: string) {
            let unencoded = new Array<number>();

            let rleArray = value.split(",");

            for (var i = 0; i < rleArray.length; i++) {
                let components = rleArray[i];

                let value = parseInt(components[components.length - 1]);
                let size = parseInt(components.substring(0, components.length - 1));

                let segment = new Array(size).fill(value);

                unencoded.push(...segment);
            }

            return unencoded;
        }

        /**
         * Called repeatedly during playback so we can track playback progress.
         */
        private trackPlay() {
            this.writeDebugMessage(`${this.player.currentTime}`);
            this.markBitWatched();
        }

        /**
         * Writes a message to the console if debug output has been enabled.
         *
         * @param message The message to be logged.
         */
        private writeDebugMessage(message: string) {
            if (this.options.debug) {
                console.log(`RMP(${this.elementId}): ${message}`);
            }
        }

        /**
         * Wires up all events on the Plyr object.
         */
        private wireEvents() {
            const self = this;
            const pageHideHandler = function () {
                self.writeInteraction(true);
            };
            const visibilityChangeHandler = function () {
                if (document.visibilityState === "hidden") {
                    self.writeInteraction(true);
                }
            };

            // Define play event
            this.player.on("play", () => {
                // Check that player is prepped. In an HTML 5 media this will
                // have already happened. But embedded players do not support
                // the events we need.
                this.prepareForPlay();

                // Start play timer
                if (this.options.trackProgress) {
                    this.timerId = setInterval(() => this.trackPlay(), 250);
                }

                // Pause any other players that are currently playing if they
                // don't want other playings to play concurrently.
                if (this.options.autopause) {
                    MediaPlayer.AllPlayers.forEach(mp => {
                        if (mp !== this && mp.player.playing === true) {
                            mp.player.pause();
                        }
                    });
                }

                this.emit(EventType.Play);

                window.addEventListener("pagehide", pageHideHandler);
                window.addEventListener("visibilitychange", visibilityChangeHandler);

                this.writeDebugMessage("Event 'play' called.");

                if (!this.options.interactionGuid) {
                    // Force a write, this will make sure we have an interaction
                    // guid for later beacon saves.
                    this.watchBitsDirty = true;
                    this.writeInteraction(false);
                }
            });

            // Define pause event
            this.player.on("pause", () => {
                // Clear timer
                if (this.timerId) {
                    clearInterval(this.timerId);
                }

                // Check if we need to write a watch bit. Not checking here can
                // lead to gaps in the map depending on the timing of the
                // play/pause event.
                this.markBitWatched();

                window.removeEventListener("pagehide", pageHideHandler);
                window.removeEventListener("visibilitychange", visibilityChangeHandler);

                this.emit(EventType.Pause);

                this.writeInteraction(false);

                this.writeDebugMessage("Event 'pause' called.");
            });

            // Define ended event
            this.player.on("ended", () => {
                // Paused event has already triggered.

                this.emit(EventType.Completed);

                this.writeDebugMessage("Event 'ended' called.");
            });

            // Define ready event
            this.player.on("ready", () => {
                this.writeDebugMessage(`Event 'ready' called: ${this.player.duration}`);

                // If a download url wasn't figured out automatically then set
                // it manually. Issue #5426
                if (!this.player.download) {
                    const canDownload = !this.isYouTubeEmbed(this.options.mediaUrl)
                        && !this.isVimeoEmbed(this.options.mediaUrl)
                        && !this.isHls(this.options.mediaUrl);
                    if (canDownload) {
                        this.player.download = this.options.mediaUrl;
                    }
                }

                if (this.player.duration > 0) {
                    this.prepareForPlay();
                }
                else {
                    // Check every 250ms to see if we are actually ready.
                    const readyTimerId = setInterval(() => {
                        if (this.player.duration > 0) {
                            clearInterval(readyTimerId);
                            this.prepareForPlay();
                        }
                    }, 250);

                    // Stop checking after 5 seconds.
                    setTimeout(() => clearInterval(readyTimerId), 5000);
                }
            });
        }

        /**
         * Writes the watch map interaction to the server.
         *
         * @param beacon If true then the call is made asynchronously using beacons.
         */
        private writeInteraction(beacon: boolean) {
            // Check for the required mediaElementGuid value.
            if (this.options.writeInteraction === false || this.options.mediaElementGuid === undefined || this.options.mediaElementGuid.length === 0) {
                return;
            }

            // If the watchBits aren't dirty then we don't need to save.
            if (this.watchBitsDirty === false) {
                return;
            }
            this.watchBitsDirty = false;

            // Construct the data we will be posting to the API.
            const data: MediaElementInteraction = {
                InteractionGuid: this.options.interactionGuid,
                MediaElementGuid: this.options.mediaElementGuid,
                WatchMap: MediaPlayer.toRle(this.watchBits),
                RelatedEntityTypeId: this.options.relatedEntityTypeId,
                RelatedEntityId: this.options.relatedEntityId,
                SessionGuid: this.options.sessionGuid,
                OriginalUrl: window.location.href,
                PageId: (Rock as any).settings.get("pageId")
            }

            if (typeof navigator.sendBeacon !== "undefined" && beacon && this.options.interactionGuid) {
                var beaconData = new Blob([JSON.stringify(data)], { type: 'application/json; charset=UTF-8' });

                navigator.sendBeacon("/api/MediaElements/WatchInteraction", beaconData);
                return;
            }

            // Initialize the API request.
            const xmlRequest = new XMLHttpRequest();
            const self = this;
            xmlRequest.open("POST", "/api/MediaElements/WatchInteraction");
            xmlRequest.setRequestHeader("Content-Type", "application/json");

            // Add a handler for when the state changes.
            xmlRequest.onreadystatechange = function () {
                // Check for request has completed.
                if (xmlRequest.readyState === 4) {
                    // Check if the request succeeded or not.
                    if (xmlRequest.status === 200) {
                        const response = <MediaElementInteraction>JSON.parse(xmlRequest.responseText);
                        self.options.interactionGuid = response.InteractionGuid;
                    }
                    else {
                        self.writeDebugMessage("Failed to record watch map interaction.");
                    }

                    self.emit(EventType.RecordInteraction, xmlRequest.status === 200);
                }
            }

            // Send the request.
            xmlRequest.send(JSON.stringify(data));
        }

        /**
         * Takes an array and returns the RLE string for it.
         *
         * @param value The value to be converted to an RLE string.
         *
         * @description RLE mapping is that segments are separated by commas.
         * The last character of the segment is the value.
         * Example:  1100011 = 21,30,21 (two ones, three zeros, two ones).
         */
        private static toRle(value: number[] | string): string {

            // If passed value is a string convert it to an array of numbers
            if (!Array.isArray(value)) {
                value = value.split("").map(x => +x);
            }

            if (value.length == 0) {
                return "";
            }

            let encoding: string[] = [];
            let previous = value[0];
            let count = 1;

            for (let i = 1; i < value.length; i++) {
                if (value[i] !== previous) {
                    encoding.push(count.toString() + previous.toString());
                    count = 1;
                    previous = value[i];
                } else {
                    count++;
                }
            }

            // Add last pair
            encoding.push(count.toString() + previous.toString());

            return encoding.join(",");
        }

        // #endregion

        // #region Event Emitter Methods

        /**
         * Adds a new callback function to the specified event.
         *
         * @param event The name of the event.
         * @param fn The function to be called when the event occurs.
         * @returns The RockMediaPlayer instance.
         */
        public on(event?: string, fn?: Function) {
            this.callbacks = this.callbacks || {};

            if (event === undefined || fn === undefined) {
                return this;
            }

            const key = `$${event}`;
            this.callbacks[key] = this.callbacks[key] || [];
            this.callbacks[key].push(fn);

            return this;
        }

        /**
         * Removes callback function from the specified event. If no function is
         * specified then all functions are removed. If no event is specified then
         * all callbacks for all events are removed.
         *
         * @param event The name of the event.
         * @param fn The function to be removed.
         * @returns The RockMediaPlayer instance.
         */
        public off(event?: string, fn?: Function) {
            this.callbacks = this.callbacks || {};

            // Remove all callbacks.
            if (event === undefined) {
                this.callbacks = {};
                return this;
            }

            const key = `$${event}`;

            // Remove all callbacks for this event.
            if (fn === undefined) {
                delete this.callbacks[key];
                return this;
            }

            // Check if there are even any callbacks to remove.
            if (this.callbacks[key] === undefined) {
                return this;
            }

            // Loop through the callbacks and remove the passed on
            // if found.
            for (let i = 0; i < this.callbacks[key].length; i++) {
                if (fn === this.callbacks[key][i]) {
                    this.callbacks[key].slice(i, 1);
                    break;
                }
            }

            // Delete the callback array if it's empty.
            if (this.callbacks[key].length === 0) {
                delete this.callbacks[key];
            }

            return this;
        }

        /**
         * Emits a named event with the given arguments.
         *
         * @param event The name of the event to emit.
         * @param args The arguments to pass to the event callbacks.
         */
        private emit(event: EventType, ...args: any[]) {
            this.callbacks = this.callbacks || {};

            let callbacks = this.callbacks[`$${event}`];

            if (callbacks === undefined) {
                return;
            }

            // Duplicate the array so it doesn't get modified. For example if
            // a callback removes itself while it's running.
            callbacks = callbacks.slice(0);
            for (let i = 0; i < callbacks.length; i++) {
                callbacks[i].apply(this, args);
            }
        }

        // #endregion
    }
}
