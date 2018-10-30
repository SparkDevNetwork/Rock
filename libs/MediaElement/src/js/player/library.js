'use strict';

import window from 'global/window';
import mejs from '../core/mejs';
import MediaElementPlayer from '../player';

if (typeof jQuery !== 'undefined') {
	mejs.$ = window.jQuery = window.$ = jQuery;
} else if (typeof Zepto !== 'undefined') {
	mejs.$ = window.Zepto = window.$ = Zepto;
} else if (typeof ender !== 'undefined') {
	mejs.$ = window.ender = window.$ = ender;
}

// turn into plugin
(($) => {
	if (typeof $ !== 'undefined') {
		$.fn.mediaelementplayer = function (options) {
			if (options === false) {
				this.each(function () {
					const player = $(this).data('mediaelementplayer');
					if (player) {
						player.remove();
					}
					$(this).removeData('mediaelementplayer');
				});
			} else {
				this.each(function () {
					$(this).data('mediaelementplayer', new MediaElementPlayer(this, options));
				});
			}
			return this;
		};

		$(document).ready(() => {
			// auto enable using JSON attribute
			$(`.${mejs.MepDefaults.classPrefix}player`).mediaelementplayer();
		});
	}
})(mejs.$);