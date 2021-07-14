"use strict";
var Rock;
(function (Rock) {
    var UI;
    (function (UI) {
        var DefaultOptions = {
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
        var MediaPlayer = (function () {
            function MediaPlayer(elementSelector, options) {
                if (options === void 0) { options = {}; }
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
                var element = document.querySelector(elementSelector);
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
            Object.defineProperty(MediaPlayer.prototype, "map", {
                get: function () {
                    return MediaPlayer.toRle(this.watchBits);
                },
                enumerable: false,
                configurable: true
            });
            Object.defineProperty(MediaPlayer.prototype, "percentWatched", {
                get: function () {
                    return this.percentWatchedInternal;
                },
                enumerable: false,
                configurable: true
            });
            Object.defineProperty(MediaPlayer.prototype, "duration", {
                get: function () {
                    return this.player.duration;
                },
                enumerable: false,
                configurable: true
            });
            MediaPlayer.prototype.seek = function (positionInSeconds) {
                this.player.currentTime = positionInSeconds;
            };
            MediaPlayer.prototype.setupPlayer = function () {
                var _this = this;
                var mediaElement = document.createElement(this.options.type === "audio" ? "audio" : "video");
                mediaElement.setAttribute("playsinline", "");
                mediaElement.setAttribute("controls", "");
                this.element.appendChild(mediaElement);
                var plyrOptions = {
                    storage: {
                        enabled: false
                    },
                    autoplay: this.options.autoplay,
                    controls: this.options.controls.split(","),
                    seekTime: this.options.seekTime,
                    volume: this.options.volume,
                    muted: this.options.muted,
                    clickToPlay: this.options.clickToPlay,
                    hideControls: this.options.hideControls
                };
                if (!this.isHls(this.options.mediaUrl)) {
                    this.initializePlayer(mediaElement, plyrOptions);
                    return;
                }
                var hls = new Hls();
                var updateQuality = function (newQuality) {
                    if (newQuality === 0) {
                        hls.currentLevel = -1;
                    }
                    else {
                        hls.levels.forEach(function (level, levelIndex) {
                            if (level.height === newQuality) {
                                hls.currentLevel = levelIndex;
                            }
                        });
                    }
                };
                hls.on(Hls.Events.MANIFEST_PARSED, function () {
                    var availableQualities = hls.levels.map(function (l) { return l.height; });
                    availableQualities.unshift(0);
                    plyrOptions.quality = {
                        default: 0,
                        options: availableQualities,
                        forced: true,
                        onChange: function (e) { return updateQuality(e); },
                    };
                    plyrOptions.i18n = plyrOptions.i18n || {};
                    plyrOptions.i18n.qualityLabel = {
                        0: 'Auto'
                    };
                    hls.currentLevel = -1;
                    _this.initializePlayer(mediaElement, plyrOptions);
                });
                if (this.options.posterUrl !== undefined) {
                    mediaElement.setAttribute("poster", this.options.posterUrl);
                }
                hls.loadSource(this.options.mediaUrl);
                hls.attachMedia(mediaElement);
            };
            MediaPlayer.prototype.initializePlayer = function (mediaElement, plyrOptions) {
                this.player = new Plyr(mediaElement, plyrOptions);
                this.writeDebugMessage("Setting media URL to " + this.options.mediaUrl);
                var sourceInfo = {
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
            };
            MediaPlayer.prototype.getFilenameFromUrl = function (url) {
                var _a, _b;
                return ((_b = (_a = url.split('#').shift()) === null || _a === void 0 ? void 0 : _a.split('?').shift()) === null || _b === void 0 ? void 0 : _b.split('/').pop()) || url;
            };
            MediaPlayer.prototype.stringEndsWith = function (haystack, needle) {
                return haystack.substr(-needle.length) === needle;
            };
            MediaPlayer.prototype.isAudio = function (url) {
                var filename = this.getFilenameFromUrl(url).toLowerCase();
                return this.stringEndsWith(filename, ".aac") === true
                    || this.stringEndsWith(filename, ".flac") === true
                    || this.stringEndsWith(filename, ".mp3") === true
                    || this.stringEndsWith(filename, ".wav") === true;
            };
            MediaPlayer.prototype.isHls = function (url) {
                var filename = this.getFilenameFromUrl(url).toLowerCase();
                return this.stringEndsWith(filename, ".m3u8") === true && Hls !== undefined && Hls.isSupported() === true;
            };
            MediaPlayer.prototype.isYouTubeEmbed = function (url) {
                var pattern = /https?:\/\/www\.youtube\.com\/embed\/([^\?]+)\??/i;
                var match = pattern.exec(url);
                return match !== null;
            };
            MediaPlayer.prototype.isVimeoEmbed = function (url) {
                var pattern = /https?:\/\/player\.vimeo\.com\/video\/([^\?]+)\??/i;
                var match = pattern.exec(url);
                return match !== null;
            };
            MediaPlayer.prototype.translateWellKnownUrls = function (url) {
                var youTubePattern = /https?:\/\/(?:www\.)youtube\.com\/watch(?:[?&]v=([^&]+))/i;
                var vimeoPattern = /https?:\/\/vimeo\.com\/([0-9]+)/i;
                var match = youTubePattern.exec(url);
                if (match !== null) {
                    return "https://www.youtube.com/embed/" + match[1];
                }
                match = vimeoPattern.exec(url);
                if (match !== null) {
                    return "https://player.vimeo.com/video/" + match[1];
                }
                return url;
            };
            MediaPlayer.prototype.markBitWatched = function () {
                var playBit = Math.floor(this.player.currentTime);
                if (playBit == this.previousPlayBit) {
                    return;
                }
                if (this.watchBits[playBit] < 9) {
                    this.watchBits[playBit]++;
                    this.watchBitsDirty = true;
                }
                var watchedItemCount = this.watchBits.filter(function (item) { return item > 0; }).length;
                this.percentWatchedInternal = watchedItemCount / this.watchBits.length;
                this.previousPlayBit = playBit;
                this.emit("progress");
                this.writeDebugMessage("RLE: " + MediaPlayer.toRle(this.watchBits));
                this.writeDebugMessage("Player Time: " + this.player.currentTime + "; Current Time: " + playBit + "; Percent Watched: " + this.percentWatched + "; Unwatched Items: " + (this.watchBits.length - watchedItemCount) + "; Map Size: " + this.watchBits.length);
            };
            MediaPlayer.prototype.prepareForPlay = function () {
                if (this.watchBitsInitialized !== false) {
                    return;
                }
                this.writeDebugMessage("Preparing the player.");
                this.initializeMap();
                this.setResume();
                this.emit("ready");
            };
            MediaPlayer.prototype.setResume = function () {
                this.writeDebugMessage("The media length is " + this.player.duration + " seconds.");
                if (this.options.resumePlaying === false || this.percentWatched === 1) {
                    return;
                }
                var startPosition = 0;
                for (var i = 0; i < this.watchBits.length; i++) {
                    if (this.watchBits[i] == 0) {
                        startPosition = i;
                        break;
                    }
                }
                this.player.currentTime = startPosition;
                this.writeDebugMessage("Set starting position at: " + startPosition);
                this.writeDebugMessage("The current starting position is: " + this.player.currentTime);
            };
            MediaPlayer.prototype.initializeMap = function () {
                this.loadExistingMap();
                this.validateMap();
                this.watchBitsInitialized = true;
                this.writeDebugMessage("Initialize Map (" + this.watchBits.length + "): " + this.watchBits.join(""));
            };
            MediaPlayer.prototype.loadExistingMap = function () {
                this.writeDebugMessage("Loading existing map.");
                if (this.options.map === "") {
                    this.createBlankMap();
                    this.writeDebugMessage("No previous map provided, creating a blank map.");
                    return;
                }
                var existingMapString = this.options.map;
                this.writeDebugMessage("Map provided in .map property: " + existingMapString);
                this.watchBits = MediaPlayer.rleToArray(existingMapString);
                var watchedItemCount = this.watchBits.filter(function (item) { return item > 0; }).length;
                this.percentWatchedInternal = watchedItemCount / this.watchBits.length;
            };
            MediaPlayer.prototype.validateMap = function () {
                var mediaLength = Math.ceil(this.player.duration);
                if (this.watchBits.length !== mediaLength) {
                    this.writeDebugMessage("Provided map size (" + this.watchBits.length + ") did not match the media (" + mediaLength + "). Using a blank map.");
                    this.createBlankMap();
                }
            };
            MediaPlayer.prototype.createBlankMap = function () {
                var mapSize = Math.ceil(this.player.duration);
                if (mapSize < 0) {
                    mapSize = 0;
                }
                this.watchBits = new Array(mapSize).fill(0);
                this.percentWatchedInternal = 0;
                this.writeDebugMessage("Blank map created of size: " + this.watchBits.length);
            };
            MediaPlayer.rleToArray = function (value) {
                var unencoded = new Array();
                var rleArray = value.split(",");
                for (var i = 0; i < rleArray.length; i++) {
                    var components = rleArray[i];
                    var value_1 = parseInt(components[components.length - 1]);
                    var size = parseInt(components.substring(0, components.length - 1));
                    var segment = new Array(size).fill(value_1);
                    unencoded.push.apply(unencoded, segment);
                }
                return unencoded;
            };
            MediaPlayer.prototype.trackPlay = function () {
                this.writeDebugMessage("" + this.player.currentTime);
                this.markBitWatched();
            };
            MediaPlayer.prototype.writeDebugMessage = function (message) {
                if (this.options.debug) {
                    console.log("RMP(" + this.elementId + "): " + message);
                }
            };
            MediaPlayer.prototype.wireEvents = function () {
                var _this = this;
                var self = this;
                var pageHideHandler = function () {
                    self.writeInteraction(false);
                };
                var visibilityChangeHandler = function () {
                    if (document.visibilityState === "hidden") {
                        self.writeInteraction(false);
                    }
                };
                this.player.on("play", function () {
                    _this.prepareForPlay();
                    if (_this.options.trackProgress) {
                        _this.timerId = setInterval(function () { return _this.trackPlay(); }, 250);
                    }
                    if (_this.options.autopause) {
                        MediaPlayer.AllPlayers.forEach(function (mp) {
                            if (mp !== _this && mp.player.playing === true) {
                                mp.player.pause();
                            }
                        });
                    }
                    _this.emit("play");
                    window.addEventListener("pagehide", pageHideHandler);
                    window.addEventListener("visibilitychange", visibilityChangeHandler);
                    _this.writeDebugMessage("Event 'play' called.");
                });
                this.player.on("pause", function () {
                    if (_this.timerId) {
                        clearInterval(_this.timerId);
                    }
                    _this.markBitWatched();
                    window.removeEventListener("pagehide", pageHideHandler);
                    window.removeEventListener("visibilitychange", visibilityChangeHandler);
                    _this.emit("pause");
                    _this.writeInteraction(true);
                    _this.writeDebugMessage("Event 'pause' called.");
                });
                this.player.on("ended", function () {
                    _this.emit("completed");
                    _this.writeDebugMessage("Event 'ended' called.");
                });
                this.player.on("ready", function () {
                    _this.writeDebugMessage("Event 'ready' called: " + _this.player.duration);
                    if (_this.player.duration > 0) {
                        _this.prepareForPlay();
                    }
                    else {
                        var readyTimerId_1 = setInterval(function () {
                            if (_this.player.duration > 0) {
                                clearInterval(readyTimerId_1);
                                _this.prepareForPlay();
                            }
                        }, 250);
                        setTimeout(function () { return clearInterval(readyTimerId_1); }, 5000);
                    }
                });
            };
            MediaPlayer.prototype.writeInteraction = function (async) {
                if (this.options.writeInteraction === false || this.options.mediaElementGuid === undefined || this.options.mediaElementGuid.length === 0) {
                    return;
                }
                if (this.watchBitsDirty === false) {
                    return;
                }
                this.watchBitsDirty = false;
                var data = {
                    InteractionGuid: this.options.interactionGuid,
                    MediaElementGuid: this.options.mediaElementGuid,
                    WatchMap: MediaPlayer.toRle(this.watchBits),
                    RelatedEntityTypeId: this.options.relatedEntityTypeId,
                    RelatedEntityId: this.options.relatedEntityId
                };
                var xmlRequest = new XMLHttpRequest();
                var self = this;
                xmlRequest.open("POST", "/api/MediaElements/WatchInteraction", async);
                xmlRequest.setRequestHeader("Content-Type", "application/json");
                xmlRequest.onreadystatechange = function () {
                    if (xmlRequest.readyState === 4) {
                        if (xmlRequest.status === 200) {
                            var response = JSON.parse(xmlRequest.responseText);
                            self.options.interactionGuid = response.InteractionGuid;
                        }
                        else {
                            self.writeDebugMessage("Failed to record watch map interaction.");
                        }
                        self.emit("recordinteraction", xmlRequest.status === 200);
                    }
                };
                xmlRequest.send(JSON.stringify(data));
            };
            MediaPlayer.toRle = function (value) {
                if (!Array.isArray(value)) {
                    value = value.split("").map(function (x) { return +x; });
                }
                if (value.length == 0) {
                    return "";
                }
                var encoding = [];
                var previous = value[0];
                var count = 1;
                for (var i = 1; i < value.length; i++) {
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
            };
            MediaPlayer.prototype.on = function (event, fn) {
                this.callbacks = this.callbacks || {};
                if (event === undefined || fn === undefined) {
                    return this;
                }
                var key = "$" + event;
                this.callbacks[key] = this.callbacks[key] || [];
                this.callbacks[key].push(fn);
                return this;
            };
            MediaPlayer.prototype.off = function (event, fn) {
                this.callbacks = this.callbacks || {};
                if (event === undefined) {
                    this.callbacks = {};
                    return this;
                }
                var key = "$" + event;
                if (fn === undefined) {
                    delete this.callbacks[key];
                    return this;
                }
                if (this.callbacks[key] === undefined) {
                    return this;
                }
                for (var i = 0; i < this.callbacks[key].length; i++) {
                    if (fn === this.callbacks[key][i]) {
                        this.callbacks[key].slice(i, 1);
                        break;
                    }
                }
                if (this.callbacks[key].length === 0) {
                    delete this.callbacks[key];
                }
                return this;
            };
            MediaPlayer.prototype.emit = function (event) {
                var args = [];
                for (var _i = 1; _i < arguments.length; _i++) {
                    args[_i - 1] = arguments[_i];
                }
                this.callbacks = this.callbacks || {};
                var callbacks = this.callbacks["$" + event];
                if (callbacks === undefined) {
                    return;
                }
                callbacks = callbacks.slice(0);
                for (var i = 0; i < callbacks.length; i++) {
                    callbacks[i].apply(this, args);
                }
            };
            MediaPlayer.AllPlayers = [];
            return MediaPlayer;
        }());
        UI.MediaPlayer = MediaPlayer;
    })(UI = Rock.UI || (Rock.UI = {}));
})(Rock || (Rock = {}));
//# sourceMappingURL=mediaplayer.js.map