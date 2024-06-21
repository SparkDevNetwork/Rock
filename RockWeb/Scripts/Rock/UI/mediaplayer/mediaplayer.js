"use strict";
var Rock;
(function (Rock) {
    var UI;
    (function (UI) {
        const DefaultOptions = {
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
        };
        class MediaPlayer {
            constructor(elementSelector, options = {}) {
                this.timerId = null;
                this.watchBits = Array();
                this.watchBitsInitialized = false;
                this.watchBitsDirty = false;
                this.percentWatchedInternal = 0;
                this.previousPlayBit = null;
                this.elementId = "";
                this.callbacks = {};
                this.options = Object.assign({}, DefaultOptions, options);
                this.elementId = elementSelector;
                MediaPlayer.AllPlayers.push(this);
                const element = document.querySelector(elementSelector);
                if (element === null) {
                    throw "Element not found to initialize media player with.";
                }
                this.element = element;
                if (this.options.mediaUrl === "") {
                    throw "Required mediaUrl option was not specified.";
                }
                this.options.mediaUrl = this.translateWellKnownUrls(this.options.mediaUrl);
                if (this.options.type !== "video" && this.options.type !== "audio") {
                    this.options.type = this.isAudio(this.options.mediaUrl) ? "audio" : "video";
                }
                while (this.element.firstChild !== null) {
                    this.element.removeChild(this.element.firstChild);
                }
                this.setupPlayer();
            }
            get map() {
                return MediaPlayer.toRle(this.watchBits);
            }
            get percentWatched() {
                return this.percentWatchedInternal;
            }
            get duration() {
                return this.player.duration;
            }
            seek(positionInSeconds) {
                this.player.currentTime = positionInSeconds;
            }
            setupPlayer() {
                const mediaElement = document.createElement(this.options.type === "audio" ? "audio" : "video");
                mediaElement.setAttribute("playsinline", "");
                mediaElement.setAttribute("controls", "");
                mediaElement.setAttribute("style", "width: 100%;");
                this.element.appendChild(mediaElement);
                const plyrOptions = {
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
                const updateQuality = function (newQuality) {
                    if (newQuality === 0) {
                        hls.currentLevel = -1;
                    }
                    else {
                        hls.levels.forEach((level, levelIndex) => {
                            if (level.height === newQuality) {
                                hls.currentLevel = levelIndex;
                            }
                        });
                    }
                };
                hls.on(Hls.Events.MANIFEST_PARSED, () => {
                    const availableQualities = hls.levels.map((l) => l.height);
                    availableQualities.unshift(0);
                    plyrOptions.quality = {
                        default: 0,
                        options: availableQualities,
                        forced: true,
                        onChange: (e) => updateQuality(e),
                    };
                    plyrOptions.i18n = plyrOptions.i18n || {};
                    plyrOptions.i18n.qualityLabel = {
                        0: 'Auto'
                    };
                    hls.currentLevel = -1;
                    this.initializePlayer(mediaElement, plyrOptions);
                });
                if (this.options.posterUrl !== undefined) {
                    mediaElement.setAttribute("poster", this.options.posterUrl);
                }
                hls.loadSource(this.options.mediaUrl);
                hls.attachMedia(mediaElement);
            }
            initializePlayer(mediaElement, plyrOptions) {
                if (this.isYouTubeEmbed(this.options.mediaUrl) || this.isVimeoEmbed(this.options.mediaUrl) || this.isHls(this.options.mediaUrl)) {
                    var control = plyrOptions.controls;
                    let index = control.findIndex(d => d === "download");
                    if (index !== -1) {
                        control.splice(index, 1);
                    }
                    plyrOptions.controls = control;
                }
                this.player = new Plyr(mediaElement, plyrOptions);
                if (this.isYouTubeEmbed(this.options.mediaUrl)) {
                    let listenrsready = false;
                    this.player.on("statechange", () => {
                        if (!listenrsready) {
                            listenrsready = true;
                            this.player.listeners.media();
                        }
                    });
                }
                this.writeDebugMessage(`Setting media URL to ${this.options.mediaUrl}`);
                let sourceInfo = {
                    type: this.options.type === "audio" ? "audio" : "video",
                    title: this.options.title,
                    sources: [
                        {
                            src: this.options.mediaUrl
                        }
                    ],
                    poster: this.options.posterUrl
                };
                if (this.isYouTubeEmbed(this.options.mediaUrl)) {
                    sourceInfo.sources[0].provider = "youtube";
                }
                else if (this.isVimeoEmbed(this.options.mediaUrl)) {
                    sourceInfo.sources[0].provider = "vimeo";
                }
                else if (this.isHls(this.options.mediaUrl)) {
                    sourceInfo = null;
                }
                if (sourceInfo !== null) {
                    this.player.source = sourceInfo;
                }
                this.wireEvents();
            }
            getFilenameFromUrl(url) {
                var _a, _b;
                return ((_b = (_a = url.split('#').shift()) === null || _a === void 0 ? void 0 : _a.split('?').shift()) === null || _b === void 0 ? void 0 : _b.split('/').pop()) || url;
            }
            stringEndsWith(haystack, needle) {
                return haystack.substr(-needle.length) === needle;
            }
            isAudio(url) {
                const filename = this.getFilenameFromUrl(url).toLowerCase();
                return this.stringEndsWith(filename, ".aac") === true
                    || this.stringEndsWith(filename, ".flac") === true
                    || this.stringEndsWith(filename, ".mp3") === true
                    || this.stringEndsWith(filename, ".wav") === true;
            }
            isHls(url) {
                const filename = this.getFilenameFromUrl(url).toLowerCase();
                return this.stringEndsWith(filename, ".m3u8") === true && Hls !== undefined && Hls.isSupported() === true;
            }
            isYouTubeEmbed(url) {
                const pattern = /https?:\/\/www\.youtube\.com\/embed\/([^\?]+)\??/i;
                const match = pattern.exec(url);
                return match !== null;
            }
            isVimeoEmbed(url) {
                const pattern = /https?:\/\/player\.vimeo\.com\/video\/([^\?]+)\??/i;
                const match = pattern.exec(url);
                return match !== null;
            }
            translateWellKnownUrls(url) {
                const youTubePattern = /https?:\/\/(?:www\.)youtube\.com\/watch(?:[?&]v=([^&]+))/i;
                const vimeoPattern = /https?:\/\/vimeo\.com\/([0-9]+)/i;
                let match = youTubePattern.exec(url);
                if (match !== null) {
                    return `https://www.youtube.com/embed/${match[1]}`;
                }
                match = vimeoPattern.exec(url);
                if (match !== null) {
                    return `https://player.vimeo.com/video/${match[1]}`;
                }
                return url;
            }
            markBitWatched() {
                var playBit = Math.floor(this.player.currentTime);
                if (playBit == this.previousPlayBit) {
                    return;
                }
                if (this.watchBits[playBit] < 9) {
                    this.watchBits[playBit]++;
                    this.watchBitsDirty = true;
                }
                const watchedItemCount = this.watchBits.filter(item => item > 0).length;
                this.percentWatchedInternal = watchedItemCount / this.watchBits.length;
                this.previousPlayBit = playBit;
                this.emit("progress");
                this.writeDebugMessage(`RLE: ${MediaPlayer.toRle(this.watchBits)}`);
                this.writeDebugMessage(`Player Time: ${this.player.currentTime}; Current Time: ${playBit}; Percent Watched: ${this.percentWatched}; Unwatched Items: ${this.watchBits.length - watchedItemCount}; Map Size: ${this.watchBits.length}`);
            }
            prepareForPlay() {
                if (this.watchBitsInitialized !== false) {
                    return;
                }
                this.writeDebugMessage("Preparing the player.");
                this.initializeMap();
                this.setResume();
                this.emit("ready");
            }
            setResume() {
                this.writeDebugMessage(`The media length is ${this.player.duration} seconds.`);
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
            initializeMap() {
                this.loadExistingMap();
                this.validateMap();
                this.watchBitsInitialized = true;
                this.writeDebugMessage(`Initialize Map (${this.watchBits.length}): ${this.watchBits.join("")}`);
            }
            loadExistingMap() {
                this.writeDebugMessage("Loading existing map.");
                if (this.options.map === "") {
                    this.createBlankMap();
                    this.writeDebugMessage("No previous map provided, creating a blank map.");
                    return;
                }
                const existingMapString = this.options.map;
                this.writeDebugMessage(`Map provided in .map property: ${existingMapString}`);
                this.watchBits = MediaPlayer.rleToArray(existingMapString);
                const watchedItemCount = this.watchBits.filter(item => item > 0).length;
                this.percentWatchedInternal = watchedItemCount / this.watchBits.length;
            }
            validateMap() {
                let mediaLength = Math.ceil(this.player.duration);
                if (this.watchBits.length !== mediaLength) {
                    this.writeDebugMessage(`Provided map size (${this.watchBits.length}) did not match the media (${mediaLength}). Using a blank map.`);
                    this.createBlankMap();
                }
            }
            createBlankMap() {
                let mapSize = Math.ceil(this.player.duration);
                if (mapSize < 0) {
                    mapSize = 0;
                }
                this.watchBits = new Array(mapSize).fill(0);
                this.percentWatchedInternal = 0;
                this.writeDebugMessage(`Blank map created of size: ${this.watchBits.length}`);
            }
            static rleToArray(value) {
                let unencoded = new Array();
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
            trackPlay() {
                this.writeDebugMessage(`${this.player.currentTime}`);
                this.markBitWatched();
            }
            writeDebugMessage(message) {
                if (this.options.debug) {
                    console.log(`RMP(${this.elementId}): ${message}`);
                }
            }
            wireEvents() {
                const self = this;
                const pageHideHandler = function () {
                    self.writeInteraction(true);
                };
                const visibilityChangeHandler = function () {
                    if (document.visibilityState === "hidden") {
                        self.writeInteraction(true);
                    }
                };
                this.player.on("play", () => {
                    this.prepareForPlay();
                    if (this.options.trackProgress) {
                        this.timerId = setInterval(() => this.trackPlay(), 250);
                    }
                    if (this.options.autopause) {
                        MediaPlayer.AllPlayers.forEach(mp => {
                            if (mp !== this && mp.player.playing === true) {
                                mp.player.pause();
                            }
                        });
                    }
                    this.emit("play");
                    window.addEventListener("pagehide", pageHideHandler);
                    window.addEventListener("visibilitychange", visibilityChangeHandler);
                    this.writeDebugMessage("Event 'play' called.");
                    if (!this.options.interactionGuid) {
                        this.watchBitsDirty = true;
                        this.writeInteraction(false);
                    }
                });
                this.player.on("pause", () => {
                    if (this.timerId) {
                        clearInterval(this.timerId);
                    }
                    this.markBitWatched();
                    window.removeEventListener("pagehide", pageHideHandler);
                    window.removeEventListener("visibilitychange", visibilityChangeHandler);
                    this.emit("pause");
                    this.writeInteraction(false);
                    this.writeDebugMessage("Event 'pause' called.");
                });
                this.player.on("ended", () => {
                    this.emit("completed");
                    this.writeDebugMessage("Event 'ended' called.");
                });
                this.player.on("ready", () => {
                    this.writeDebugMessage(`Event 'ready' called: ${this.player.duration}`);
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
                        const readyTimerId = setInterval(() => {
                            if (this.player.duration > 0) {
                                clearInterval(readyTimerId);
                                this.prepareForPlay();
                            }
                        }, 250);
                        setTimeout(() => clearInterval(readyTimerId), 5000);
                    }
                });
            }
            writeInteraction(beacon) {
                if (this.options.writeInteraction === false || this.options.mediaElementGuid === undefined || this.options.mediaElementGuid.length === 0) {
                    return;
                }
                if (this.watchBitsDirty === false) {
                    return;
                }
                this.watchBitsDirty = false;
                const data = {
                    InteractionGuid: this.options.interactionGuid,
                    MediaElementGuid: this.options.mediaElementGuid,
                    WatchMap: MediaPlayer.toRle(this.watchBits),
                    RelatedEntityTypeId: this.options.relatedEntityTypeId,
                    RelatedEntityId: this.options.relatedEntityId,
                    SessionGuid: this.options.sessionGuid,
                    OriginalUrl: window.location.href,
                    PageId: Rock.settings.get("pageId")
                };
                if (typeof navigator.sendBeacon !== "undefined" && beacon && this.options.interactionGuid) {
                    var beaconData = new Blob([JSON.stringify(data)], { type: 'application/json; charset=UTF-8' });
                    navigator.sendBeacon("/api/MediaElements/WatchInteraction", beaconData);
                    return;
                }
                const xmlRequest = new XMLHttpRequest();
                const self = this;
                xmlRequest.open("POST", "/api/MediaElements/WatchInteraction");
                xmlRequest.setRequestHeader("Content-Type", "application/json");
                xmlRequest.onreadystatechange = function () {
                    if (xmlRequest.readyState === 4) {
                        if (xmlRequest.status === 200) {
                            const response = JSON.parse(xmlRequest.responseText);
                            self.options.interactionGuid = response.InteractionGuid;
                        }
                        else {
                            self.writeDebugMessage("Failed to record watch map interaction.");
                        }
                        self.emit("recordinteraction", xmlRequest.status === 200);
                    }
                };
                xmlRequest.send(JSON.stringify(data));
            }
            static toRle(value) {
                if (!Array.isArray(value)) {
                    value = value.split("").map(x => +x);
                }
                if (value.length == 0) {
                    return "";
                }
                let encoding = [];
                let previous = value[0];
                let count = 1;
                for (let i = 1; i < value.length; i++) {
                    if (value[i] !== previous) {
                        encoding.push(count.toString() + previous.toString());
                        count = 1;
                        previous = value[i];
                    }
                    else {
                        count++;
                    }
                }
                encoding.push(count.toString() + previous.toString());
                return encoding.join(",");
            }
            on(event, fn) {
                this.callbacks = this.callbacks || {};
                if (event === undefined || fn === undefined) {
                    return this;
                }
                const key = `$${event}`;
                this.callbacks[key] = this.callbacks[key] || [];
                this.callbacks[key].push(fn);
                return this;
            }
            off(event, fn) {
                this.callbacks = this.callbacks || {};
                if (event === undefined) {
                    this.callbacks = {};
                    return this;
                }
                const key = `$${event}`;
                if (fn === undefined) {
                    delete this.callbacks[key];
                    return this;
                }
                if (this.callbacks[key] === undefined) {
                    return this;
                }
                for (let i = 0; i < this.callbacks[key].length; i++) {
                    if (fn === this.callbacks[key][i]) {
                        this.callbacks[key].slice(i, 1);
                        break;
                    }
                }
                if (this.callbacks[key].length === 0) {
                    delete this.callbacks[key];
                }
                return this;
            }
            emit(event, ...args) {
                this.callbacks = this.callbacks || {};
                let callbacks = this.callbacks[`$${event}`];
                if (callbacks === undefined) {
                    return;
                }
                callbacks = callbacks.slice(0);
                for (let i = 0; i < callbacks.length; i++) {
                    callbacks[i].apply(this, args);
                }
            }
        }
        MediaPlayer.AllPlayers = [];
        UI.MediaPlayer = MediaPlayer;
    })(UI = Rock.UI || (Rock.UI = {}));
})(Rock || (Rock = {}));
//# sourceMappingURL=mediaplayer.js.map