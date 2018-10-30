'use strict';

/**
 * DailyMotion renderer
 *
 * Uses <iframe> approach and uses DailyMotion API to manipulate it.
 * @see https://developer.dailymotion.com/player
 *
 */
const DailyMotionApi = {
	/**
	 * @type {Boolean}
	 */
	isSDKStarted: false,
	/**
	 * @type {Boolean}
	 */
	isSDKLoaded: false,
	/**
	 * @type {Array}
	 */
	iframeQueue: [],

	/**
	 * Create a queue to prepare the creation of <iframe>
	 *
	 * @param {Object} settings - an object with settings needed to create <iframe>
	 */
	enqueueIframe: (settings) => {

		if (DailyMotionApi.isLoaded) {
			DailyMotionApi.createIframe(settings);
		} else {
			DailyMotionApi.loadIframeApi();
			DailyMotionApi.iframeQueue.push(settings);
		}
	},

	/**
	 * Load DailyMotion API script on the header of the document
	 *
	 */
	loadIframeApi: () => {
		if (!DailyMotionApi.isSDKStarted) {
			mejs.Utils.loadScript('https://api.dmcdn.net/all.js');
			DailyMotionApi.isSDKStarted = true;
		}
	},

	/**
	 * Process queue of DailyMotion <iframe> element creation
	 *
	 */
	apiReady: () => {

		DailyMotionApi.isLoaded = true;
		DailyMotionApi.isSDKLoaded = true;

		while (DailyMotionApi.iframeQueue.length > 0) {
			const settings = DailyMotionApi.iframeQueue.pop();

			// Init SDK
			DM.init({
				apiKey: settings.apiKey,
				status: settings.status,
				cookie: settings.cookie
			});

			DailyMotionApi.createIframe(settings);
		}
	},

	/**
	 * Create a new instance of DailyMotion API player and trigger a custom event to initialize it
	 *
	 * @param {Object} settings - an object with settings needed to create <iframe>
	 */
	createIframe: (settings) => {

		const player = DM.player(settings.container, {
			height: settings.height || '100%',
			width: settings.width || '100%',
			video: settings.videoId,
			params: Object.assign({api: true}, settings.params),
			origin: location.host
		});

		player.addEventListener('apiready', () => {
			window[`__ready__${settings.id}`](player, {paused: true, ended: false});
		});
	},

	/**
	 * Extract ID from DailyMotion's URL to be loaded through API
	 * Valid URL format(s):
	 * - http://www.dailymotion.com/embed/video/x35yawy
	 * - http://dai.ly/x35yawy
	 *
	 * @param {String} url
	 * @return {String}
	 */
	getDailyMotionId: (url) => {
		const
			parts = url.split('/'),
			lastPart = parts[parts.length - 1],
			dashParts = lastPart.split('_')
		;

		return dashParts[0];
	}
};

const DailyMotionIframeRenderer = {
	name: 'dailymotion_iframe',
	options: {
		prefix: 'dailymotion_iframe',
		dailymotion: {
			width: '100%',
			height: '100%',
			params: {
				autoplay: false,
				chromeless: 1,
				info: 0,
				logo: 0,
				related: 0
			},
			apiKey: null,
			status: true,
			cookie: true
		}
	},

	/**
	 * Determine if a specific element type can be played with this render
	 *
	 * @param {String} type
	 * @return {Boolean}
	 */
	canPlayType: (type) => ~['video/dailymotion', 'video/x-dailymotion'].indexOf(type.toLowerCase()),

	/**
	 * Create the player instance and add all native events/methods/properties as possible
	 *
	 * @param {MediaElement} mediaElement Instance of mejs.MediaElement already created
	 * @param {Object} options All the player configuration options passed through constructor
	 * @param {Object[]} mediaFiles List of sources with format: {src: url, type: x/y-z}
	 * @return {Object}
	 */
	create: (mediaElement, options, mediaFiles) => {

		const
			dm = {},
			apiStack = [],
			readyState = 4
		;

		let
			events,
			dmPlayer = null,
			dmIframe = null,
			muted = mediaElement.originalNode.muted
		;

		dm.options = options;
		dm.id = mediaElement.id + '_' + options.prefix;
		dm.mediaElement = mediaElement;

		// wrappers for get/set
		const
			props = mejs.html5media.properties,
			assignGettersSetters = (propName) => {

				// add to flash state that we will store

				const capName = `${propName.substring(0, 1).toUpperCase()}${propName.substring(1)}`;

				dm[`get${capName}`] = () => {
					if (dmPlayer !== null) {
						const value = null;

						// figure out how to get dm dta here
						switch (propName) {
							case 'currentTime':
								return dmPlayer.currentTime;
							case 'duration':
								return isNaN(dmPlayer.duration) ? 0 : dmPlayer.duration;
							case 'volume':
								return dmPlayer.volume;
							case 'paused':
								return dmPlayer.paused;
							case 'ended':
								return dmPlayer.ended;
							case 'muted':
								muted = dmPlayer.muted;
								return muted;
							case 'buffered':
								const percentLoaded = dmPlayer.bufferedTime,
									duration = dmPlayer.duration;
								return {
									start: () => {
										return 0;
									},
									end: () => {
										return percentLoaded / duration;
									},
									length: 1
								};
							case 'src':
								return mediaElement.originalNode.getAttribute('src');
							case 'readyState':
								return readyState;
						}

						return value;
					} else {
						return null;
					}
				};

				dm[`set${capName}`] = (value) => {
					if (dmPlayer !== null) {
						switch (propName) {
							case 'src':
								const url = typeof value === 'string' ? value : value[0].src;
								dmPlayer.load(DailyMotionApi.getDailyMotionId(url));
								break;
							case 'currentTime':
								dmPlayer.seek(value);
								break;
							case 'muted':
								if (value === true) {
									dmPlayer.setVolume(0);
								}
								dmPlayer.setMuted(value);
								muted = value;
								setTimeout(() => {
									const event = mejs.Utils.createEvent('volumechange', dm);
									mediaElement.dispatchEvent(event);
								}, 50);
								break;
							case 'volume':
								dmPlayer.setVolume(value);
								if (value === 0 && !dmPlayer.muted) {
									dmPlayer.setMuted(true);
									muted = true;
								} else if (value > 0 && dmPlayer.muted) {
									dmPlayer.setMuted(false);
									muted = false;
								}

								setTimeout(() => {
									const event = mejs.Utils.createEvent('volumechange', dm);
									mediaElement.dispatchEvent(event);
								}, 50);
								break;
							case 'readyState':
								const event = mejs.Utils.createEvent('canplay', dm);
								mediaElement.dispatchEvent(event);
								break;
							default:
								console.log('dm ' + dm.id, propName, 'UNSUPPORTED property');
								break;
						}
					} else {
						// store for after "READY" event fires
						apiStack.push({type: 'set', propName: propName, value: value});
					}
				};

			}
		;

		for (let i = 0, total = props.length; i < total; i++) {
			assignGettersSetters(props[i]);
		}

		const
			methods = mejs.html5media.methods,
			assignMethods = (methodName) => {
				dm[methodName] = () => {
					if (dmPlayer !== null) {
						switch (methodName) {
							case 'play':
								return dmPlayer.play();
							case 'pause':
								return dmPlayer.pause();
							case 'load':
								return null;
						}
					} else {
						apiStack.push({type: 'call', methodName: methodName});
					}
				};

			}
		;

		for (let i = 0, total = methods.length; i < total; i++) {
			assignMethods(methods[i]);
		}

		// Initial method to register all DailyMotion events when initializing <iframe>
		window['__ready__' + dm.id] = (_dmPlayer) => {

			mediaElement.dmPlayer = dmPlayer = _dmPlayer;

			if (apiStack.length) {
				for (let i = 0, total = apiStack.length; i < total; i++) {

					const stackItem = apiStack[i];

					if (stackItem.type === 'set') {
						const
							propName = stackItem.propName,
							capName = `${propName.substring(0, 1).toUpperCase()}${propName.substring(1)}`
						;

						dm[`set${capName}`](stackItem.value);

					} else if (stackItem.type === 'call') {
						dm[stackItem.methodName]();
					}
				}
			}

			dmIframe = document.getElementById(dm.id);

			events = ['mouseover', 'mouseout'];
			const assignEvents = (e) => {
				const event = mejs.Utils.createEvent(e.type, dm);
				mediaElement.dispatchEvent(event);
			};

			for (let i = 0, total = events.length; i < total; i++) {
				dmIframe.addEventListener(events[i], assignEvents, false);
			}

			if (mediaElement.originalNode.muted) {
				dmPlayer.setVolume(0);
				dmPlayer.setMuted(true);
			} else {
				dmPlayer.setVolume(dmPlayer.volume);
				dmPlayer.setMuted(false);
			}

			events = mejs.html5media.events;
			events = events.concat(['click', 'mouseover', 'mouseout']);
			const assignNativeEvents = (eventName) => {

				// Deprecated event; not consider it
				if (eventName !== 'ended') {
					dmPlayer.addEventListener(eventName, (e) => {
						const event = mejs.Utils.createEvent(e.type, dm);
						mediaElement.dispatchEvent(event);
					});
				}
			};

			for (let i = 0, total = events.length; i < total; i++) {
				assignNativeEvents(events[i]);
			}

			// Custom DailyMotion events
			dmPlayer.addEventListener('ad_start', () => {
				let event = mejs.Utils.createEvent('play', dm);
				mediaElement.dispatchEvent(event);

				event = mejs.Utils.createEvent('progress', dm);
				mediaElement.dispatchEvent(event);

				event = mejs.Utils.createEvent('timeupdate', dm);
				mediaElement.dispatchEvent(event);
			});
			dmPlayer.addEventListener('ad_timeupdate', () => {
				const event = mejs.Utils.createEvent('timeupdate', dm);
				mediaElement.dispatchEvent(event);
			});
			dmPlayer.addEventListener('ad_pause', () => {
				const event = mejs.Utils.createEvent('pause', dm);
				mediaElement.dispatchEvent(event);
			});
			dmPlayer.addEventListener('start', function () {
				if (dmPlayer.muted) {
					const event = mejs.Utils.createEvent('volumechange', dm);
					mediaElement.dispatchEvent(event);
				}
			});
			dmPlayer.addEventListener('video_start', () => {
				const event = mejs.Utils.createEvent('play', dm);
				mediaElement.dispatchEvent(event);

				const playingEvent = mejs.Utils.createEvent('playing', dm);
				mediaElement.dispatchEvent(playingEvent);
			});
			dmPlayer.addEventListener('ad_timeupdate', () => {
				const event = mejs.Utils.createEvent('timeupdate', dm);
				mediaElement.dispatchEvent(event);
			});
			dmPlayer.addEventListener('video_end', () => {
				const event = mejs.Utils.createEvent('ended', dm);
				mediaElement.dispatchEvent(event);

				// Check `loop` attribute
				if (mediaElement.originalNode.getAttribute('loop')) {
					dmPlayer.play();
				}
			});

			// give initial events
			const initEvents = ['rendererready', 'loadedmetadata', 'loadeddata', 'canplay'];

			for (let i = 0, total = initEvents.length; i < total; i++) {
				const event = mejs.Utils.createEvent(initEvents[i], dm);
				mediaElement.dispatchEvent(event);
			}
		};

		const dmContainer = document.createElement('div');
		dmContainer.id = dm.id;
		mediaElement.appendChild(dmContainer);
		if (mediaElement.originalNode) {
			dmContainer.style.width = mediaElement.originalNode.style.width;
			dmContainer.style.height = mediaElement.originalNode.style.height;
		}
		mediaElement.originalNode.style.display = 'none';

		const
			videoId = DailyMotionApi.getDailyMotionId(mediaFiles[0].src),
			dmSettings = {
				id: dm.id,
				container: dmContainer,
				videoId: videoId
			};

		dmSettings.params = Object.assign({}, dm.options.dailymotion);

		// Check for `autoplay` and `muted` attributes to override settings
		dmSettings.params.controls = !!mediaElement.originalNode.controls;

		if (mediaElement.originalNode.autoplay) {
			dmSettings.params.autoplay = true;
		}
		if (mediaElement.originalNode.muted) {
			dmSettings.params.mute = true;
		}
		dmSettings.params.api = '1';

		DailyMotionApi.enqueueIframe(dmSettings);

		dm.hide = () => {
			dm.pause();
			if (dmIframe) {
				dmIframe.style.display = 'none';
			}
		};
		dm.show = () => {
			if (dmIframe) {
				dmIframe.style.display = '';
			}
		};
		dm.setSize = (width, height) => {
			if (dmIframe) {
				dmIframe.width = width;
				dmIframe.height = height;
			}
		};
		dm.destroy = () => {
			dmPlayer.destroy();
		};

		return dm;
	}
};


/*
 * Register DailyMotion event globally
 *
 */
mejs.Utils.typeChecks.push((url) => /\/\/((www\.)?dailymotion\.com|dai\.ly)/i.test(url) ? 'video/x-dailymotion' : null);

window.dmAsyncInit = () => {
	DailyMotionApi.apiReady();
};

mejs.Renderers.add(DailyMotionIframeRenderer);