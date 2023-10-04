/*!
 * jQuery Countdown Control plugin v2.3.17
 * http://plugins.twinpictures.de/premium-plugins/t-minus-countdown-control/
 *
 * Copyright 2017, Twinpictures
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, blend, trade,
 * bake, hack, scramble, difiburlate, digest and/or sell copies of the Software,
 * and to permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

(function($){
	var oldhtmlStr;

	$.fn.tminusCountDown = function (options) {
		config = {};

		$.extend(config, options);

		if (options.event_id) {
			$.data($(this)[0], 'event_id', options.event_id);
			$.data($(this)[0], 'eventObj', options.eventObj);
		}

		//standard nowTime and Hangtime
		localTime = new Date( options.targetDate.localtime )
		$.data($(this)[0], 'nowTime', localTime);

		if (config.hangtime > 0 ){
			$.data($(this)[0], 'hangtime', config.hangtime);
		}
		else{
			$.data($(this)[0], 'hangtime', 1);
		}

		//cache_check servertime
		if (options.cache_check == 1) {
			$.data($(this)[0], 'cache_check', options.cache_check);
			nowTime = new Date();
			//console.log('update from servertime', localTime, nowTime);
			cached_time = Math.floor((nowTime.valueOf()-localTime.valueOf())/1000);
			//console.log(cached_time + ' seconds have passed since file was cached');

			if(cached_time > 1){
				//update now
				$.data($(this)[0], 'nowTime', nowTime );

				//update hangtime
				new_hangtime = Math.floor(config.hangtime - cached_time);
				//console.log('new hangtime is now: ' + new_hangtime);
				if (new_hangtime > 0 ){
					$.data($(this)[0], 'hangtime', new_hangtime);
				}
				else{
					$.data($(this)[0], 'hangtime', 1);
				}
			}


		}

		//ajaxtime
		if (options.cache_check == 2) {
			//ajax time
			data = {
				action: 'tminusnow',
				//countdownNonce : tCountAjax.countdownNonce
			};
			var ajaxtime = $(this).grabAjax(data);
			$.data($(this)[0], 'cache_check', options.cache_check);
			server_time = new Date(); //need to caculate time diff between WP time and Server time

			ajaxtime.done( function(data) {
				nowTime = new Date(data.now);
				time_diff = Math.floor((nowTime.valueOf()-server_time.valueOf())/1000);

				if(time_diff > 1 || time_diff < -1){
					//console.log(time_diff + ' seconds difference between wp and server');
					$.data($(this)[0], 'time_diff', time_diff);
				}

				cached_time = Math.floor((nowTime.valueOf()-localTime.valueOf())/1000);
				//console.log(cached_time + ' seconds have passed since file was cached');

				if(cached_time > 1){
					//update now
					$.data($(this)[0], 'nowTime', nowTime );

					//update hangtime
					new_hangtime = Math.floor(config.hangtime - cached_time);
					//console.log('new hangtime is now: ' + new_hangtime);
					if (new_hangtime > 0 ){
						$.data($(this)[0], 'hangtime', new_hangtime);
					}
					else{
						$.data($(this)[0], 'hangtime', 1);
					}
				}
			});
		}

		deltaSecs = this.setTminusCountDown(config);

		if (config.targetDate){
			tminus_type = options.targetDate.tminus_type;
			use_cookie = options.targetDate.use_cookie;
			expiry_days = options.targetDate.expiry_days;
			//cookies?
			if (use_cookie) {
				var cookie_time = readCookie( 'tmcc_tminus_cookie_time' );

				if(cookie_time){
					targetTime = new Date(cookie_time);
				}
				else{
					expires = expiry_days * 86400000;
					setCookie( 'tmcc_tminus_cookie_time', options.targetDate.launchtime, expires);
					targetTime = new Date(options.targetDate.launchtime);
				}
			}
			else{
				targetTime = new Date(options.targetDate.launchtime);
			}

			//uncomment below to force-delete cookie
			//setCookie( 'tmcc_tminus_cookie_time', options.targetDate.launchtime, 'Thu, 01 Jan 1970 00:00:01 GMT');
		}
		$.data($(this)[0], 'targetTime', targetTime);

		//$.data($(this)[0], 'timeGap', timeGap);
		$.data($(this)[0], 'status', 'play');

		style = config.style;
		$.data($(this)[0], 'style', config.style);

		if(config.launchTarget){
			$.data($(this)[0], 'launchtarget', config.launchTarget);
		}
		if (config.onComplete != undefined){
			//console.log(config.launchTarget, config.onComplete);
			if(config.launchTarget.indexOf('-swap') != -1){
				cdcontainer = config.launchTarget.replace('-swap', '');
				$.data($(this)[0], 'cdcontainer', cdcontainer);
			}
			$.data($(this)[0], 'callback', config.onComplete);
		}

		//flip Effect
		if(config.no_flip){
			$.data($(this)[0], 'no_flip', config.no_flip);
		}

		$.data($(this)[0], 'cdid', config.id);
		$.data($(this)[0], 'pid', config.pid);
		$.data($(this)[0], 'cid', config.cid);

		$.data($(this)[0], 'omitSecs', config.omitSecs);
		$.data($(this)[0], 'omitMins', config.omitMins);
		$.data($(this)[0], 'omitDays', config.omitDays);
		$.data($(this)[0], 'omitWeeks', config.omitWeeks);

		$("div[data-digit_cid='" + config.id + "']").html("<div class='top'></div><div class='bottom'></div>");

		//console.log('call initial doTminusCountDown with: ' + deltaSecs);
		$(this).doTminusCountDown($(this).attr('id'), deltaSecs, 500);

		return this;
	};

	$.fn.grabAjax = function(data) {
		return $.ajax({
			url: tCountAjax.ajaxurl,
			type : 'post',
			dataType : 'json',
			data: data
		});
	};

	$.fn.stopTminusCountDown = function () {
		//clearTimeout($.data(this[0], 'timer'));
		$.data(this[0], 'status', 'stop');
	};

	$.fn.startTminusCountDown = function () {
		$.data(this[0], 'status', 'play');
		//console.log('call doTminusCountDown from startTminusCountDown with: ' + $(this).attr('id'), $.data(this[0], 'deltaSecs') );
		if ( $.data(this[0], 'event_id') ){
			//check for the reset event
			$this.checkEvent('reset', false);
		}
		this.doTminusCountDown($(this).attr('id'), $.data(this[0], 'deltaSecs'), 500);
	};

	$.fn.setTminusCountDown = function (options) {
		var targetTime = new Date();

		if (options.targetDate){
			targetTime = new Date(options.targetDate.month + '/' + options.targetDate.day + '/' + options.targetDate.year + ' ' + options.targetDate.hour + ':' + options.targetDate.min + ':' + options.targetDate.sec + (options.targetDate.utc ? ' UTC' : ''));
		}
		else if (options.targetOffset){
			targetTime.setFullYear(options.targetOffset.year + targetTime.getFullYear());
			targetTime.setMonth(options.targetOffset.month + targetTime.getMonth());
			targetTime.setDate(options.targetOffset.day + targetTime.getDate());
			targetTime.setHours(options.targetOffset.hour + targetTime.getHours());
			targetTime.setMinutes(options.targetOffset.min + targetTime.getMinutes());
			targetTime.setSeconds(options.targetOffset.sec + targetTime.getSeconds());
		}

		nowTime = $.data(this[0], 'nowTime');

		deltaSecs = Math.floor((targetTime.valueOf()-nowTime.valueOf())/1000);

		$.data(this[0], 'deltaSecs', deltaSecs);

		//check for any sticky events
		if ( $.data(this[0], 'event_id') ){
			//check for the reset event
			this.checkEvent(deltaSecs, true);
		}

		return deltaSecs;
	};

	$.fn.doTminusCountDown = function (id, deltaSecs, duration) {
		// console.log('doTminusCountDown called with: ' + deltaSecs);
		$this = $('#' + id);

		//call the event if there is one
		if ( deltaSecs >= 0 && $.data($this[0], 'event_id') ){
			$this.checkEvent(deltaSecs, false);
		}

		if (deltaSecs <= 0){

			if( $.data($this[0], 'launchtarget') && $.data($this[0], 'launchtarget').indexOf('countup') != -1){

				if($.data($this[0], 'negloop') != 1){
					$.data($this[0], 'negloop', '1');
					//console.log('launching negloop with a diffsedcs of: ' + deltaSecs);
					$this.launchTminusCountdown();
				}

				//remove that negative attitude
				deltaSecs = Math.abs(deltaSecs);
			}
			else{
				deltaSecs = 0;
				$.data($this[0], 'status', 'stop');
			}
		}

		if ($.data($this[0], 'omitWeeks') == 'true' && $.data($this[0], 'omitDays') == 'true' && $.data($this[0], 'omitMins') == 'true' ) {
			secs = deltaSecs;
			mins = 0;
			hours = 0;
			days = 0;
			weeks = 0;
		}
		else{
			secs = deltaSecs % 60;
			mins = Math.floor(deltaSecs/60)%60;

			if ($.data($this[0], 'omitDays') == 'true'){
				hours = Math.floor(deltaSecs/60/60);
			}
			else{
				hours = Math.floor(deltaSecs/60/60)%24;
			}

			if ($.data($this[0], 'omitWeeks') == 'true'){
				days = Math.floor(deltaSecs/60/60/24);
				weeks = Math.floor(deltaSecs/60/60/24/7);
			}
			else{
				days = Math.floor(deltaSecs/60/60/24)%7;
				weeks = Math.floor(deltaSecs/60/60/24/7);
			}
		}

		style = $.data($this[0], 'style');
		if($.data($this[0], 'no_flip')){
			quick = 1;
			slow = 1;
		}
		else{
			quick = 500;
			slow = 1000;
		}

		cid = $.data($this[0], 'cdid');
		$this.tminusDashChangeTo(cid, 'seconds', secs, duration ? duration : quick);
		$this.tminusDashChangeTo(cid, 'minutes', mins, duration ? duration : slow);
		$this.tminusDashChangeTo(cid, 'hours', hours, duration ? duration : slow);
		if ($.data($this[0], 'omitDays') != 'true'){
			$this.tminusDashChangeTo(cid, 'days', days, duration ? duration : slow);
		}
		if ($.data($this[0], 'omitWeeks') != 'true'){
			$this.tminusDashChangeTo(cid, 'weeks', weeks, duration ? duration : slow);
		}


		$.data($this[0], 'deltaSecs', deltaSecs);

		//console.log(deltaSecs, $.data($this[0], 'negloop'));
		if (deltaSecs > 0 || $.data($this[0], 'negloop') == 1 ){
			if($.data($this[0], 'status') == 'play'){
				nowTime = new Date();
				if($.data($this[0], 'time_diff')){
					nowTime.setSeconds(nowTime.getSeconds() + $.data($this[0], 'time_diff'));
				}
				//console.log(nowTime);

				targetTime = $.data($this[0], 'targetTime');
				//timeGap = $.data($this[0], 'timeGap');
				//deltaSecs = Math.floor((targetTime.valueOf()-nowTime.valueOf()+timeGap)/1000);
				deltaSecs = Math.floor((targetTime.valueOf()-nowTime.valueOf())/1000);
				looper = setTimeout( function() {
						//console.log('call doTminusCountDown for id: ' + id + ' with: ' + deltaSecs);
						$this.doTminusCountDown(id, deltaSecs);
					}, 1000);
			}
		}
		else{
			if ( $.data($this[0], 'event_id') ){
				$this.checkEvent('launch', false);
			}
			$this.launchTminusCountdown();
		}
	};

	$.fn.pad = function(n) {
		if(n < 10){
			return "0" + n;
		}
    	return n.toString();
	}

	$.fn.tminusDashChangeTo = function(id, dash, n, duration) {
		this_dash = $("div[data-" + dash + "_cid='" + id + "']");
		style = $.data($this[0], 'style');
		//console.log(dash + ' is currently ' + this_dash.data('value') + ' and will be changed to: ' + n);
		//value compare
		if( !jQuery.isNumeric( this_dash.data('value') ) || this_dash.data('value') != n ){
			this_dash.data('value', n);

			sNumber = $this.pad(n);
			for (var i = 0, len = sNumber.length; i < len; i += 1) {
			   //console.log(i, sNumber.charAt(i));
			   $this.tminusDigitChangeTo("div[data-" + dash + "_cid='" + id + "']" + " ." + style + "-digit:eq("+i+")", sNumber.charAt(i), duration);
			}
			//console.log('----');
		}
	};

	$.fn.tminusDigitChangeTo = function (digit, n, duration) {
		if (!duration){
			duration = 500;
		}

		//check for number translation object
		if(typeof numTransObj != "undefined"){
			n = numTransObj[n];
		}

		if ( parseInt( $(digit + ' div.top').html() , 10) != n ){
			$(digit + ' div.top').css({'display': 'none'});
			$(digit + ' div.top').html((n ? n : '0')).stop(true, true).slideDown(duration);

			$(digit + ' div.bottom').stop(true, true).animate({'height': ''}, duration, function() {
				$(digit + ' div.bottom').html($(digit + ' div.top').html());
				$(digit + ' div.bottom').css({'display': 'block', 'height': ''});
				$(digit + ' div.top').hide().slideUp(10);
			});
		}
	};

	$.fn.recurTminusCountDown = function (thisTminusCountdown, data) {
		old_cid = $.data(thisTminusCountdown[0], 'cid' );
		timeout = setTimeout(function() {
			//console.log('timeout just got triggered');
			//grab the next countdown
			var ajaxcountdown = $(this).grabAjax(data);

			ajaxcountdown.done( function(jsonobj) {
				//console.log('good stuff: ' + jsonobj);
				//if ending
				if ( jsonobj.status  == 'suspended') {
					//hide the timer and exit
					$('#' + $.data(thisTminusCountdown[0], 'cdid') + '-countdown').hide();
					$('#' + $.data(thisTminusCountdown[0], 'cdid') + '-countdown-swap').hide();
				}
				else{
					//reset any negloop
					if($.data(thisTminusCountdown[0], 'negloop') == 1){
						deltaSecs = 0;
						$.data(thisTminusCountdown[0], 'negloop', 0);
					}

					//set up new timer
					targetTime = new Date(jsonobj.launchTime);
					$.data(thisTminusCountdown[0], 'targetTime', targetTime);

					thisTminusCountdown.setTminusCountDown({
						targetDate: {
							'day': jsonobj.day,
							'month': jsonobj.month,
							'year': jsonobj.year,
							'hour': jsonobj.hour,
							'min': 	jsonobj.min,
							'sec': 	jsonobj.sec,
							'localtime': jsonobj.localTime
						}
					});

					//update hangtime
					$.data(thisTminusCountdown[0], 'hangtime', jsonobj.hangTime);

					//change wrapper css
					$this.parent().parent().removeClass('tminus_launched').addClass('tminus_countdown');

					//update top and bottom html
					$('#' + $.data(thisTminusCountdown[0], 'cdid') + '-tophtml').html( jsonobj.tophtml ).show();
					$('#' + $.data(thisTminusCountdown[0], 'cdid') + '-bothtml').html( jsonobj.bothtml ).show();

					if($.data(thisTminusCountdown[0], 'cdcontainer') != undefined && $.data(thisTminusCountdown[0], 'cdcontainer')){
						//empty the target
						$('#' + $.data(thisTminusCountdown[0], 'launchtarget')).html('').hide();
						//show the countdown
						$('#' + $.data(thisTminusCountdown[0], 'cdcontainer')).show();
					}
					else{
						if(old_cid == jsonobj.cid){
							//put back the stuff... really?
							$('#' + $.data(thisTminusCountdown[0], 'launchtarget')).html(oldhtmlStr);
						}
					}

					//update the cid
					$.data(thisTminusCountdown[0], 'cid', jsonobj.cid);

					//reset the countdown container & launchtarget
					$.data(thisTminusCountdown[0], 'cdcontainer', '');

					$.data(thisTminusCountdown[0], 'launchtarget', jsonobj.launchTarget);
					if(jsonobj.launchTarget.indexOf('-swap') != -1){
						cdcontainer = jsonobj.launchTarget.replace('-swap', '');
						$.data(thisTminusCountdown[0], 'cdcontainer', cdcontainer);
					}

					//update callback
					var funk = new Function ( jsonobj.onComplete );
					$.data(thisTminusCountdown[0], 'callback', funk);

					//if this was the countdown
					thisTminusCountdown.startTminusCountDown();
				}
			});

			ajaxcountdown.fail( function(jqXHR, error, msg) {
				console.log(jqXHR);
				console.log('error dude: ', error);
				console.log(msg);
			});

		} , $.data($this[0], 'hangtime') * 1000);
	}

	$.fn.launchTminusCountdown = function (){
		//console.log( $.data($this[0], 'cid') + ' launched...');
		$this.parent().parent().removeClass('tminus_countdown').addClass('tminus_launched');

		cdid = $.data($this[0], 'cdid');
		pid = $.data($this[0], 'pid');
		cid = $.data($this[0], 'cid');

		if($.data($this[0], 'negloop') != 1){
			if($.data($this[0], 'cdcontainer') != undefined && $.data($this[0], 'cdcontainer')){
				//console.log('countdown container is: #' + $.data($this[0], 'cdcontainer'));
				//hide the countdown
				$('#' + $.data($this[0], 'cdcontainer')).hide();
				$('#' + $.data($this[0], 'launchtarget')).show();
			}
			else if( $.data($this[0], 'launchtarget') != undefined){
				//console.log('launch target is: #' + $.data($this[0], 'launchtarget'));
				var container = $('#' + $.data($this[0], 'launchtarget'));
				oldhtmlStr = container.html();
			}
		}

		if (tminus_type == 'recurring' || tminus_type == 'opening_hours') {
			data = {
				action: 'reschedulecountdown',
				id: cdid,
				pid: pid,
				cid: cid,
				//countdownNonce: tCountAjax.countdownNonce
			};
			//console.log(data);
			$this.recurTminusCountDown($this, data);
		}

		//launch event action
		if (typeof($.data($this[0], 'callback')) == 'function'){
			$.data($this[0], 'callback')();
		}

		//show countdown_toggle class
		$('.countdown_toggle').show();
	}

	$.fn.checkEvent = function ( trigger, sticky ) {
		//console.log('trigger: ' + trigger );
		if ( ! $.data( this[0], 'eventObj' ) ) {
			//console.log('no event object found: ' + $.data( this[0], 'eventObj' ) );
			return;
		}
		var eventObj = $.data( this[0], 'eventObj' ).tevent;

		for (var key in eventObj) {
			var found_event = false;
			//time
			if (eventObj[key].hasOwnProperty('tevents_event_time') && eventObj[key]['tevents_event_time'] == trigger ) {
				found_event = true;
			}
			//event
			if (eventObj[key].hasOwnProperty('tevents_event_type') && eventObj[key]['tevents_event_type'] == trigger ) {
				found_event = true;
			}
			//sticky
			if (sticky && eventObj[key].hasOwnProperty('tevents_sticky') && eventObj[key]['tevents_event_time'] > trigger ) {
				//console.log(eventObj[key]['tevents_event_time'], trigger);
				found_event = true;
			}

			if(found_event){
				//content (even if it's blank)
				if (eventObj[key].hasOwnProperty('tevents_target_elem') && eventObj[key]['tevents_event_target'] == 'other') {
					target_elem = eventObj[key]['tevents_target_elem'];
				}
				else{
					target_elem = '#' + config.id + '-' + eventObj[key]['tevents_event_target'];
				}
				//console.log( eventObj[key]['tevents_event_content'] );
				$(target_elem).html( eventObj[key]['tevents_event_content'] );

				//function
				if ( eventObj[key]['tevents_event_function'] ) {
					var fn = window[ eventObj[key]['tevents_event_function'] ];
					if(typeof fn === 'function') {
					    fn();
					}
				}
			}
		}

	}


})(jQuery);
