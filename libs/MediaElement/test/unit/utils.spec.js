'use strict';

import * as general from '../../src/js/utils/general';
import * as time from '../../src/js/utils/time';
import * as media from '../../src/js/utils/media';
import {expect} from 'chai';
import jsdom from 'mocha-jsdom';

describe('Utilities', () => {
	
	function* entries(obj) {
		for (let key of Object.keys(obj)) {
			yield [key, obj[key]];
		}
	}

	describe('#debounce', () => {

		it('executes a method in an specific interval of time', () => {

			let
				hasHappened = false,
				fn = general.debounce(() => {
					hasHappened = true;
				}, 100, true)
			;

			expect(hasHappened).to.be.false

			fn();

			expect(hasHappened).to.be.true

			setTimeout(() => {

				general.debounce(() => {
					hasHappened = true;
				}, 100);

				expect(hasHappened).to.be.true
			}, 100);
		});

		it('can only accept a method as a first argument', () => {

			const
				type = {},
				test = () => {
					general.debounce(type, 100);
				};
			expect(test).to.throw(Error);
		});

		it('can only accept a numeric value as a second argument', () => {

			const test = () => {
				general.debounce(() => {
					return;
				}, 'dummy');
			};
			expect(test).to.throw(Error);
		});

	});

	describe('#isObjectEmpty', () => {

		it('checks effectively that an object is empty', () => {
			const
				empty = {},
				nonEmpty = {name: 'aaa', alias: 'bbbb'}
			;

			expect(general.isObjectEmpty(empty)).to.equal(true);
			expect(general.isObjectEmpty(nonEmpty)).to.equal(false);
		});

	});

	describe('#splitEvents', () => {

		it('separates and group events depending their format', () => {
			const
				events = 'beforeunload hashchange message resize storage .mouseup .volumechange.test',
				id = 'mep_0',
				result = general.splitEvents(events, id)
			;

			expect(typeof result.d).to.equal('string');
			expect(result.d).to.equal('.mouseup.mep_0 .volumechange.test.mep_0');
			expect(typeof result.w).to.equal('string');
			expect(result.w).to.equal('beforeunload.mep_0 hashchange.mep_0 message.mep_0 resize.mep_0 storage.mep_0 .mouseup.mep_0 .volumechange.test.mep_0');
		});

		it('return an empty string if no arguments passed', () => {
			const result = general.splitEvents();
			expect(result.d).to.equal('');
			expect(result.w).to.equal('');

		});
	});

	describe('#createEvent', () => {

		jsdom();

		it('create a custom generic event', () => {

			const target = {};

			const event = general.createEvent('customevent', target);
			expect(event.detail.target).to.equal(target);
			expect(event instanceof window.CustomEvent).to.equal(true);
		});

		it('create a custom event with namespace', () => {

			const target = {};

			const event = general.createEvent('customevent.namespace', target);
			expect(event.detail.target).to.equal(target);
			expect(event.detail.namespace).to.equal('namespace');
			expect(event instanceof window.CustomEvent).to.equal(true);
		});

		it('only accepts strings as a first argument', () => {

			expect(() => {
				general.createEvent(12345);
			}).to.throw(Error);

		});
	});

	describe('#isString', () => {

		it('checks effectively that an argument is a string', () => {

			expect(general.isString('1234')).to.equal(true);
			expect(general.isString(1234)).to.equal(false);
			expect(general.isString({})).to.equal(false);
		});
	});

	describe('#isNodeAfter', () => {

		jsdom();

		it('checks position of element compared with another', () => {

			const parent = document.createElement('div');
			parent.innerHTML = '<div id="node1"></div><div id="node2"></div>';
			document.body.appendChild(parent);

			const
				node1 = parent.querySelector('#node1'),
				node2 = parent.querySelector('#node2')
			;

			expect(general.isNodeAfter(node1, node2)).to.equal(false);
			expect(general.isNodeAfter(node2, node1)).to.equal(true);
		});
	});

	describe('#escapeHTML', () => {

		it('can escape `<`, `"`, `&` and `>` symbols', () => {

			const html = '<p>Hello, "world" & welcome!</p>';
			expect(general.escapeHTML(html)).to.equal('&lt;p&gt;Hello, &quot;world&quot; &amp; welcome!&lt;/p&gt;');
		});

		it('can only accept strings', () => {

			const
				type = {},
				test = () => {
					general.escapeHTML(type);
				};
			expect(test).to.throw(Error);
		});
	});

	describe('#isDropFrame', () => {
		it('indicates if frames per second is a non-integer frame rates (i.e., 29.976)', () => {
			expect(time.isDropFrame()).to.equal(false);
			expect(time.isDropFrame(30)).to.equal(false);
			expect(time.isDropFrame(3.679)).to.equal(true);
		});
	});

	describe('#secondsToTimeCode', () => {

		it('can format a numeric time in format `00:00:00`', () => {

			expect(time.secondsToTimeCode(36)).to.equal('00:36');
			expect(time.secondsToTimeCode(70)).to.equal('01:10');
			expect(time.secondsToTimeCode(3600)).to.equal('01:00:00');
			expect(time.secondsToTimeCode(3660)).to.equal('01:01:00');
		});

		it('can force hours to be displayed', () => {

			expect(time.secondsToTimeCode(36, true)).to.equal('00:00:36');
			expect(time.secondsToTimeCode(70, true)).to.equal('00:01:10');
			expect(time.secondsToTimeCode(3600, true)).to.equal('01:00:00');
		});

		it('can show the number of frames multiplying the decimal portion of time by the frames per second indicated', () => {

			expect(time.secondsToTimeCode(36.45, false, true, 32)).to.equal('00:36:14');
			expect(time.secondsToTimeCode(70.89, true, true, 40)).to.equal('00:01:10:36');
			expect(time.secondsToTimeCode(3600.234, true, true, 25, 0, 'hh:mm:ss:ff')).to.equal('01:00:00:06');
		});

		it('checks if frames per second has decimals and adjust the time code', () => {
			expect(time.secondsToTimeCode(36.45, false, true, 32.46)).to.equal('00:36;31');
			expect(time.secondsToTimeCode(70.87465, false, true, 32.6891, 3, 'mm:ss:ff')).to.equal('01:10;09');
			expect(time.secondsToTimeCode(0.378, false, true, 300.2, 3)).to.equal('00:00;113');
		});

		it('can only accept numeric values for the time; otherwise, turns it to zero', () => {
			expect(time.secondsToTimeCode({})).to.equal('00:00');
			expect(time.secondsToTimeCode(undefined)).to.equal('00:00');
			expect(time.secondsToTimeCode('abcdef')).to.equal('00:00');
		});
	});

	describe('#timeCodeToSeconds', () => {

		it('returns a numeric value from a string with format `00:00:00` ', () => {

			expect(time.timeCodeToSeconds('20')).to.equal(20);
			expect(time.timeCodeToSeconds('00:36')).to.equal(36);
			expect(time.timeCodeToSeconds('01:10')).to.equal(70);
			expect(time.timeCodeToSeconds('01:00:00')).to.equal(3600);
			expect(time.timeCodeToSeconds('00:36;45')).to.equal(2205);
		});

		it('can show the numeric value with decimals of time when frames per second are indicated', () => {
			expect(time.timeCodeToSeconds('00:00:36:14', 32)).to.equal(36.438);
			expect(time.timeCodeToSeconds('00:01:10:35', 40)).to.equal(70.875);
			expect(time.timeCodeToSeconds('00:01:10:35', 15.77)).to.equal(1154);
			expect(time.timeCodeToSeconds('01:00:00:05')).to.equal(3600.2);
			expect(time.timeCodeToSeconds('01:00:00;05')).to.equal(3600.2);
		});

		it('can only accept a string for the time', () => {

			const
				type = {},
				test = () => {
					time.timeCodeToSeconds(type);
				};
			expect(test).to.throw(TypeError);
		});

		it('can only accept a string with format `00:00:00`', () => {

			const
				type = 'dummy',
				test = () => {
					time.timeCodeToSeconds(type);
				};
			expect(test).to.throw(TypeError);
		});

	});

	describe('#calculateTimeFormat', () => {

		const options = {
			timeFormat: 'mm:ss',
		};

		it('attempts to fix time format (i.e., `hh:mm:ss:ff`)', () => {
			time.calculateTimeFormat(36, options);
			expect(options.timeFormat).to.equal('mm:ss');

			options.timeFormat = 'mm:ss';

			time.calculateTimeFormat(3600, options);
			expect(options.timeFormat).to.equal('hh:mm:ss');

			options.timeFormat = 'mm:ss';

			time.calculateTimeFormat(36.432, options, 32);
			expect(options.timeFormat).to.equal('mm:ss');
		});

		it('can only accept numeric values for the time', () => {

			const
				type = {},
				test = () => {
					time.calculateTimeFormat(type);
				};
			expect(test).to.throw(Error);
		});

	});

	describe('#convertSMPTEtoSeconds', () => {

		it('returns a number when passing a `00:00:00.00` formatted string', () => {
			expect(time.convertSMPTEtoSeconds('00:12.34')).to.equal(12.34);
		});

		it('can only accept a string value for time', () => {

			const
				type = {},
				test = () => {
					time.convertSMPTEtoSeconds(type);
				};
			expect(test).to.throw(Error);
		});

	});

	describe('#absolutizeUrl', () => {

		jsdom({url: "http://localhost"});
		
		it ('returns the full URL for a relative URL', () => {
			expect(media.absolutizeUrl('/media/demo.html')).to.equal('http://localhost/media/demo.html');
		});

		it('accepts only strings', () => {
			expect(() => {
				media.absolutizeUrl(82618);
			}).to.throw(Error);
		});
	});

	describe('#formatType', () => {

		it('returns the format of a specific media using ONLY a URL', () => {

			const url = 'http://example.com/media.mp4';
			media.typeChecks = [
				(url) => {
					if (url.match(/.mp4/)) {
						return 'video/mp4';
					}
				},
				(url) => {
					if (url.match(/.mp3/)) {
						return 'audio/mp3';
					}
				}
			];

			expect(media.formatType(url)).to.equal('video/mp4');

		});

		it('returns the format of a specific media, using URL and MIME type', () => {

			const
				url = 'http://example.com/media.mp4',
				type = 'audio/mp3; codecs="avc1.42E01E, mp3a.40.2'
			;

			expect(media.formatType(url, type)).to.equal('audio/mp3; codecs="avc1.42E01E, mp3a.40.2');
		});
	});

	describe('#getMimeFromType', () => {

		it('returns the proper MIME type part in case the attribute contains codec specification', () => {

			const type = 'video/mp4; codecs="avc1.42E01E, mp4a.40.2"';
			expect(media.getMimeFromType(type)).to.equal('video/mp4');

		});

		it('returns the proper MIME type part in case the attribute DOES NOT contains codec specification', () => {

			const type = 'video/mp4';
			expect(media.getMimeFromType(type)).to.equal('video/mp4');
		});

		it('can only accept strings', () => {

			const
				type = [],
				test = () => {
					media.getMimeFromType(type);
				};
			expect(test).to.throw(Error);
		});
	});

	describe('#getTypeFromFile', () => {

		it('returns the type of media based on URL structure', () => {

			media.typeChecks =  [
				(url) => {
					if (url.match(/.mp4/)) {
						return 'video/mp4';
					}
				},
				(url) => {
					if (url.match(/.mp3/)) {
						return 'audio/mp3';
					}
				}

			];

			expect(media.getTypeFromFile('http://example.com/media.mp4')).to.equal('video/mp4');
			expect(media.getTypeFromFile('http://example.com/media2.mp4?x=1&y=2')).to.equal('video/mp4');
			expect(media.getTypeFromFile('http://example.com/media.mp3')).to.equal('audio/mp3');
			expect(media.getTypeFromFile('http://example.com/media2.mp3?x=1&y=2')).to.equal('audio/mp3');

			media.typeChecks =  [
				12345,
				'abcde',
				{}
			];

			expect(media.getTypeFromFile('http://example.com/media.m4v')).to.equal('video/mp4');
			expect(media.getTypeFromFile('http://example.com/media.midi')).to.equal('audio/midi');

		});

		it('can only accept strings', () => {
			expect(() => {
				media.getTypeFromFile({});
			}).to.throw(Error);
		});
	});

	describe('#getExtension', () => {

		it('returns the media file extension from a URL', () => {

			const urls = {
				'mp4': 'http://example.com/media.mp4',
				'm3u8': 'http://example.com/media.m3u8?string=dummy&manifest=123',
				'html': 'http://example.com/media.html',
				'': 'lorem ipsum'
			};

			for (let[ext, url] of entries(urls)) {
				expect(media.getExtension(url)).to.equal(ext);
			}
		});

		it('can only accept strings', () => {

			const
				type = {},
				test = () => {
					media.getExtension(type);
				};
			expect(test).to.throw(Error);
		});
	});

	describe('#normalizeExtension', () => {

		it('returns the standard extension of a media file', () => {

			const extensions = {
				'm4v': 'mp4',
				'webma': 'webm',
				'webm': 'webm',
				'm3u8': 'm3u8',
				'oga': 'ogg',
				'ogg': 'ogg'
			};

			for (let[ext, normal] of entries(extensions)) {
				expect(media.normalizeExtension(ext)).to.equal(normal);
			}
		});

		it('can only accept strings', () => {

			const
				type = {},
				test = () => {
					media.normalizeExtension(type);
				};
			expect(test).to.throw(Error);
		});
	});
});
