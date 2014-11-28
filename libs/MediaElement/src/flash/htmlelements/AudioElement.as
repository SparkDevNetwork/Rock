﻿
package htmlelements
{
	import flash.events.Event;
	import flash.events.IOErrorEvent;
	import flash.events.ProgressEvent;
	import flash.events.TimerEvent;
	import flash.media.ID3Info;
	import flash.media.Sound;
	import flash.media.SoundChannel;
	import flash.media.SoundLoaderContext;
	import flash.media.SoundTransform;
	import flash.net.URLRequest;
	import flash.utils.Timer;



	/**
	* ...
	* @author DefaultUser (Tools -> Custom Arguments...)
	*/
	public class AudioElement implements IMediaElement
	{

		private var _sound:Sound;
		private var _soundTransform:SoundTransform;
		private var _soundChannel:SoundChannel;
		private var _soundLoaderContext:SoundLoaderContext;

		private var _volume:Number = 1;
		private var _preMuteVolume:Number = 0;
		private var _isMuted:Boolean = false;
		private var _isPaused:Boolean = true;
		private var _isEnded:Boolean = false;
		private var _isLoaded:Boolean = false;
		private var _currentTime:Number = 0;
		private var _duration:Number = 0;
		private var _bytesLoaded:Number = 0;
		private var _bytesTotal:Number = 0;
		private var _bufferedTime:Number = 0;
		private var _bufferingChanged:Boolean = false;

		private var _currentUrl:String = "";
		private var _autoplay:Boolean = true;
		private var _preload:String = "";

		private var _element:FlashMediaElement;
		private var _timer:Timer;
		private var _firedCanPlay:Boolean = false;

		public function setSize(width:Number, height:Number):void {
			// do nothing!
		}

		public function duration():Number {
			return _duration;
		}

		public function currentTime():Number {
			return _currentTime;
		}

		public function currentProgress():Number {
				return Math.round(_bytesLoaded/_bytesTotal*100);
		}

		public function AudioElement(element:FlashMediaElement, autoplay:Boolean, preload:String, timerRate:Number, startVolume:Number):void
		{
			_element = element;
			_autoplay = autoplay;
			_volume = startVolume;
			_preload = preload;

			_timer = new Timer(timerRate);
			_timer.addEventListener(TimerEvent.TIMER, timerEventHandler);

			_soundTransform = new SoundTransform(_volume);
			_soundLoaderContext = new SoundLoaderContext();
		}

		// events
		private function progressHandler(e:ProgressEvent):void {

			_bytesLoaded = e.bytesLoaded;
			_bytesTotal = e.bytesTotal;

			// this happens too much to send every time
			//sendEvent(HtmlMediaEvent.PROGRESS);

			// so now we just trigger a flag and send with the timer
			_bufferingChanged = true;
		}

		private function id3Handler(e:Event):void {
			sendEvent(HtmlMediaEvent.LOADEDMETADATA);

			try {
				var id3:ID3Info = _sound.id3;
				var obj:Object = {
					type:'id3',
					album:id3.album,
					artist:id3.artist,
					comment:id3.comment,
					genre:id3.genre,
					songName:id3.songName,
					track:id3.track,
					year:id3.year
				};
			} catch (err:Error) {}


		}

		private function timerEventHandler(e:TimerEvent):void {
			_currentTime = _soundChannel.position/1000;

			// calculate duration
			var duration:Number = Math.round(_sound.length * _sound.bytesTotal/_sound.bytesLoaded/100) / 10;

			// check to see if the estimated duration changed
			if (_duration != duration && !isNaN(duration)) {

				_duration = duration;
				sendEvent(HtmlMediaEvent.LOADEDMETADATA);
			}

			// check for progress
			if (_bufferingChanged) {

				sendEvent(HtmlMediaEvent.PROGRESS);

				_bufferingChanged = false;
			}

			// send timeupdate
			sendEvent(HtmlMediaEvent.TIMEUPDATE);

			// sometimes the ended event doesn't fire, here's a fake one
			if (_duration > 0 && _currentTime >= _duration-0.2) {
				handleEnded();
			}
		}

		private function soundCompleteHandler(e:Event):void {
			handleEnded();
		}

		private function handleEnded():void {
			_timer.stop();
			_currentTime = 0;
			_isEnded = true;

			sendEvent(HtmlMediaEvent.ENDED);
		}

		//events


		// METHODS
		public function setSrc(url:String):void {
			_currentUrl = url;
			_isLoaded = false;
		}


		public function load():void {

			if (_currentUrl == "") {
				return;
			}

			if (_sound) {
				if (_sound.hasEventListener(ProgressEvent.PROGRESS)) {
					_sound.removeEventListener(ProgressEvent.PROGRESS, progressHandler);
				}

				if (_sound.hasEventListener(Event.ID3)) {
					_sound.removeEventListener(Event.ID3, id3Handler);
				}
			}

			_sound = new Sound();
			//sound.addEventListener(IOErrorEvent.IO_ERROR,errorHandler);
			_sound.addEventListener(ProgressEvent.PROGRESS,progressHandler);
			_sound.addEventListener(Event.ID3,id3Handler);
			_sound.load(new URLRequest(_currentUrl));
			_currentTime = 0;

			sendEvent(HtmlMediaEvent.LOADSTART);

			_isLoaded = true;

			sendEvent(HtmlMediaEvent.LOADEDDATA);
			sendEvent(HtmlMediaEvent.CANPLAY);
			_firedCanPlay = true;

			if (_playAfterLoading) {
				_playAfterLoading = false;
				play();
			}
		}

		private var _playAfterLoading:Boolean= false;

		public function play():void {

			if (!_isLoaded) {
				_playAfterLoading = true;
				load();
				return;
			}

			_timer.stop();

			_soundChannel = _sound.play(_currentTime*1000, 0, _soundTransform);
			_soundChannel.removeEventListener(Event.SOUND_COMPLETE, soundCompleteHandler);
			_soundChannel.addEventListener(Event.SOUND_COMPLETE, soundCompleteHandler);

			_timer.start();

			didStartPlaying();
		}

		public function pause():void {

			_timer.stop();
			if (_soundChannel != null) {
				_currentTime = _soundChannel.position/1000;
				_soundChannel.stop();
			}

			_isPaused = true;
			sendEvent(HtmlMediaEvent.PAUSE);
		}


		public function stop():void {
			if (_timer != null) {
				_timer.stop();
			}
			if (_soundChannel != null) {
				_soundChannel.stop();
				_sound.close();
			}
			unload();
			sendEvent(HtmlMediaEvent.STOP);
		}

		public function setCurrentTime(pos:Number):void {
			sendEvent(HtmlMediaEvent.SEEKING);
			_timer.stop();
			_currentTime = pos;
			_soundChannel.stop();
			_sound.length
			_soundChannel = _sound.play(_currentTime * 1000, 0, _soundTransform);
			sendEvent(HtmlMediaEvent.SEEKED);

			_timer.start();

			didStartPlaying();
		}

		private function didStartPlaying():void {
			_isPaused = false;
			sendEvent(HtmlMediaEvent.PLAY);
			sendEvent(HtmlMediaEvent.PLAYING);
			if (!_firedCanPlay) {
				sendEvent(HtmlMediaEvent.LOADEDDATA);
				sendEvent(HtmlMediaEvent.CANPLAY);
				_firedCanPlay = true;
			}
		}


		public function setVolume(volume:Number):void {

			_volume = volume;
			_soundTransform.volume = volume;

			if (_soundChannel != null) {
				_soundChannel.soundTransform = _soundTransform;
			}

			_isMuted = (_volume == 0);

			sendEvent(HtmlMediaEvent.VOLUMECHANGE);
		}

		public function getVolume():Number {
			if(_isMuted) {
				return 0;
			} else {
				return _volume;
			}
		}


		public function setMuted(muted:Boolean):void {

			// ignore if already set
			if ( (muted && _isMuted) || (!muted && !_isMuted))
				return;

			if (muted) {
				_preMuteVolume = _soundTransform.volume;
				setVolume(0);
			} else {
				setVolume(_preMuteVolume);
			}

			_isMuted = muted;
		}

		public function unload():void {
			_sound = null;
			_isLoaded = false;
		}

		private function sendEvent(eventName:String):void {

			// calculate this to mimic HTML5
			_bufferedTime = _bytesLoaded / _bytesTotal * _duration;

			// build JSON
			var values:String = "duration:" + _duration +
							",currentTime:" + _currentTime +
							",muted:" + _isMuted +
							",paused:" + _isPaused +
							",ended:" + _isEnded +
							",volume:" + _volume +
							",src:\"" + _currentUrl + "\"" +
							",bytesTotal:" + _bytesTotal +
							",bufferedBytes:" + _bytesLoaded +
							",bufferedTime:" + _bufferedTime +
							"";

			_element.sendEvent(eventName, values);
		}

	}

}

