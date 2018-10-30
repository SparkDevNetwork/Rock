'use strict';

/**
 * SoundCloud renderer
 *
 * Uses <iframe> approach and uses SoundCloud Widget API to manipulate it.
 * @see https://developers.soundcloud.com/docs/api/html5-widget
 */
const SoundCloudApi = {

	promise: null,

	/**
	 * Create a queue to prepare the creation of <iframe>
	 *
	 * @param {Object} settings - an object with settings needed to create <iframe>
	 */
	load: (settings) => {

		if (typeof SC !== 'undefined') {
			SoundCloudApi._createPlayer(settings);
		} else {
			SoundCloudApi.promise = SoundCloudApi.promise || mejs.Utils.loadScript('https://w.soundcloud.com/player/api.js');
			SoundCloudApi.promise.then(() => {
				SoundCloudApi._createPlayer(settings);
			});
		}
	},

	/**
	 * Create a new instance of SoundCloud Widget player and trigger a custom event to initialize it
	 *
	 * @param {Object} settings - an object with settings needed to create <iframe>
	 */
	_createPlayer: (settings) => {
		const player = SC.Widget(settings.iframe);
		window[`__ready__${settings.id}`](player);
	}
};

const SoundCloudIframeRenderer = {
	name: 'soundcloud_iframe',
	options: {
		prefix: 'soundcloud_iframe'
	},

	/**
	 * Determine if a specific element type can be played with this render
	 *
	 * @param {String} type
	 * @return {Boolean}
	 */
	canPlayType: (type) => ~['video/soundcloud', 'video/x-soundcloud'].indexOf(type.toLowerCase()),

	/**
	 * Create the player instance and add all native events/methods/properties as possible
	 *
	 * @param {MediaElement} mediaElement Instance of mejs.MediaElement already created
	 * @param {Object} options All the player configuration options passed through constructor
	 * @param {Object[]} mediaFiles List of sources with format: {src: url, type: x/y-z}
	 * @return {Object}
	 */
	create: (mediaElement, options, mediaFiles) => {

		// create our fake element that allows events and such to work
		const
			sc = {},
			apiStack = [],
			readyState = 4,
			autoplay = mediaElement.originalNode.autoplay,
			isVideo = mediaElement.originalNode !== null && mediaElement.originalNode.tagName.toLowerCase() === 'video'
		;

		let
			duration = 0,
			currentTime = 0,
			bufferedTime = 0,
			volume = 1,
			muted = false,
			paused = true,
			ended = false,
			scPlayer = null,
			scIframe = null
		;

		sc.options = options;
		sc.id = mediaElement.id + '_' + options.prefix;
		sc.mediaElement = mediaElement;

		const
			props = mejs.html5media.properties,
			assignGettersSetters = (propName) => {
				const capName = `${propName.substring(0, 1).toUpperCase()}${propName.substring(1)}`;

				sc[`get${capName}`] = () => {
					if (scPlayer !== null) {
						const value = null;

						// figure out how to get dm dta here
						switch (propName) {
							case 'currentTime':
								return currentTime;
							case 'duration':
								return duration;
							case 'volume':
								return volume;
							case 'paused':
								return paused;
							case 'ended':
								return ended;
							case 'muted':
								return muted;
							case 'buffered':
								return {
									start: () => {
										return 0;
									},
									end: () => {
										return bufferedTime * duration;
									},
									length: 1
								};
							case 'src':
								return (scIframe) ? scIframe.src : '';
							case 'readyState':
								return readyState;
						}
						return value;
					} else {
						return null;
					}
				};

				sc[`set${capName}`] = (value) => {
					if (scPlayer !== null) {
						switch (propName) {
							case 'src':
								const url = typeof value === 'string' ? value : value[0].src;
								scPlayer.load(url);
								if (autoplay) {
									scPlayer.play();
								}
								break;
							case 'currentTime':
								scPlayer.seekTo(value * 1000);
								break;
							case 'muted':
								if (value) {
									scPlayer.setVolume(0); // ?
								} else {
									scPlayer.setVolume(1); // ?
								}
								setTimeout(() => {
									const event = mejs.Utils.createEvent('volumechange', sc);
									mediaElement.dispatchEvent(event);
								}, 50);
								break;
							case 'volume':
								scPlayer.setVolume(value);
								setTimeout(() => {
									const event = mejs.Utils.createEvent('volumechange', sc);
									mediaElement.dispatchEvent(event);
								}, 50);
								break;
							case 'readyState':
								const event = mejs.Utils.createEvent('canplay', sc);
								mediaElement.dispatchEvent(event);
								break;

							default:
								console.log('sc ' + sc.id, propName, 'UNSUPPORTED property');
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
				sc[methodName] = () => {
					if (scPlayer !== null) {
						switch (methodName) {
							case 'play':
								return scPlayer.play();
							case 'pause':
								return scPlayer.pause();
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

		window['__ready__' + sc.id] = (_scPlayer) => {

			mediaElement.scPlayer = scPlayer = _scPlayer;

			if (autoplay) {
				scPlayer.play();
			}

			if (apiStack.length) {
				for (let i = 0, total = apiStack.length; i < total; i++) {

					const stackItem = apiStack[i];

					if (stackItem.type === 'set') {
						const propName = stackItem.propName,
							capName = `${propName.substring(0, 1).toUpperCase()}${propName.substring(1)}`;

						sc[`set${capName}`](stackItem.value);
					} else if (stackItem.type === 'call') {
						sc[stackItem.methodName]();
					}
				}
			}

			// SoundCloud properties are async, so we don't fire the event until the property callback fires
			scPlayer.bind(SC.Widget.Events.PLAY_PROGRESS, () => {
				paused = false;
				ended = false;
				scPlayer.getPosition((_currentTime) => {
					currentTime = _currentTime / 1000;
					const event = mejs.Utils.createEvent('timeupdate', sc);
					mediaElement.dispatchEvent(event);
				});
			});
			scPlayer.bind(SC.Widget.Events.PAUSE, () => {
				paused = true;
				const event = mejs.Utils.createEvent('pause', sc);
				mediaElement.dispatchEvent(event);
			});
			scPlayer.bind(SC.Widget.Events.PLAY, () => {
				paused = false;
				ended = false;
				const event = mejs.Utils.createEvent('play', sc);
				mediaElement.dispatchEvent(event);
			});
			scPlayer.bind(SC.Widget.Events.FINISHED, () => {
				paused = false;
				ended = true;
				const event = mejs.Utils.createEvent('ended', sc);
				mediaElement.dispatchEvent(event);
			});
			scPlayer.bind(SC.Widget.Events.READY, () => {
				scPlayer.getDuration((_duration) => {
					duration = _duration / 1000;
					const event = mejs.Utils.createEvent('loadedmetadata', sc);
					mediaElement.dispatchEvent(event);
				});
			});
			scPlayer.bind(SC.Widget.Events.LOAD_PROGRESS, () => {
				scPlayer.getDuration((loadProgress) => {
					if (duration > 0) {
						bufferedTime = duration * loadProgress;
						const event = mejs.Utils.createEvent('progress', sc);
						mediaElement.dispatchEvent(event);
					}
				});
				scPlayer.getDuration((_duration) => {
					duration = _duration;

					const event = mejs.Utils.createEvent('loadedmetadata', sc);
					mediaElement.dispatchEvent(event);
				});
			});

			// give initial events
			const initEvents = ['rendererready', 'loadeddata', 'loadedmetadata', 'canplay'];
			for (let i = 0, total = initEvents.length; i < total; i++) {
				const event = mejs.Utils.createEvent(initEvents[i], sc);
				mediaElement.dispatchEvent(event);
			}
		};

		// container for API API
		scIframe = document.createElement('iframe');
		scIframe.id = sc.id;
		scIframe.width = isVideo ? '100%' : 1;
		scIframe.height = isVideo ? '100%' : 1;
		scIframe.frameBorder = 0;
		scIframe.style.visibility = isVideo ? 'visible' : 'hidden';
		scIframe.src = mediaFiles[0].src;
		scIframe.scrolling = 'no';

		mediaElement.appendChild(scIframe);
		mediaElement.originalNode.style.display = 'none';

		const scSettings = {
			iframe: scIframe,
			id: sc.id
		};

		SoundCloudApi.load(scSettings);

		sc.setSize = () => {};
		sc.hide = () => {
			sc.pause();
			if (scIframe) {
				scIframe.style.display = 'none';
			}
		};
		sc.show = () => {
			if (scIframe) {
				scIframe.style.display = '';
			}
		};
		sc.destroy = () => {
			scPlayer.destroy();
		};

		return sc;
	}
};

/**
 * Register SoundCloud type based on URL structure
 *
 */
mejs.Utils.typeChecks.push((url) => /\/\/(w\.)?soundcloud.com/i.test(url) ? 'video/x-soundcloud' : null);

mejs.Renderers.add(SoundCloudIframeRenderer);