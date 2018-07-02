function getScrollbarWidth() {
  // thx d.walsh
  var scrollDiv = document.createElement('div');
  scrollDiv.className = 'modal-scrollbar-measure';
  document.body.appendChild(scrollDiv);
  var scrollbarWidth = scrollDiv.offsetWidth - scrollDiv.clientWidth;
  document.body.removeChild(scrollDiv);
  return scrollbarWidth;
} // Static

function BindNavEvents() {
  $(document).ready(function() {
    var bodyElement = $('body'),
    topNavElement = $('.navbar-fixed-top'),
    navElement = $('.navbar-side'),
    hoverDelay = 200,
    hideDelay = 150;

    $('.navbar-side > li').mouseenter(function() {
      const $this = $(this);
      if ($this.navUnHoverTimeout !== undefined) {
        clearTimeout($this[0].navUnHoverTimeout);
        $this[0].navUnHoverTimeout = undefined;
      } else if ($this[0].navHoverTimeout === undefined) {
        if (navElement.find('li.open').length > 0) {
            $('.navbar-side > li').removeClass('open');
            $this.addClass('open');
            $this[0].navHoverTimeout = undefined;
          //}, 0);
        } else {
          $this[0].navHoverTimeout = setTimeout(function() {
            $this.addClass('open');
            $('body')
              .addClass('modal-open')
              .css('padding-right', getScrollbarWidth());
              $('.navbar-fixed-top').css('right', getScrollbarWidth());
            $this[0].navHoverTimeout = undefined;
          }, hoverDelay);
        }
      }
    });

    $('.navbar-side > li').mouseleave(function() {
      const $this = $(this);
      if ($this[0].navHoverTimeout !== undefined) {
        clearTimeout($this[0].navHoverTimeout);
        $this[0].navHoverTimeout = undefined;
      } else if ($this[0].navUnHoverTimeout === undefined) {
        $this[0].navUnHoverTimeout = setTimeout(function() {
          // $('.navbar-side > li').removeClass('open');
          $this.removeClass('open');
          if (navElement.find('li.open').length <= 1) {
            $('body')
              .removeClass('modal-open')
              .css('padding-right', '');
              $('.navbar-fixed-top').css('right', '');
          }
          $this[0].navUnHoverTimeout = undefined;
        }, hideDelay);
      }
    });

    $('#content-wrapper').click(function() {
      $('.navbar-side li').removeClass('open');
    });

    // show/hide sidebar nav
    $('.navbar-toggle-side-left').click(function() {
      if ($('.navbar-static-side').is(':visible')) {
        bodyElement
          .addClass('navbar-side-close')
          .removeClass('navbar-side-open');
      } else {
        bodyElement
          .addClass('navbar-side-open')
          .removeClass('navbar-side-close');
      }
    });
  });
}

function PreventNumberScroll() {
  $(document).ready(function() {
    $('input[type=number]').on('wheel', function(e){
        return false;
    });
  });
}


// Focus Within Polyfill
(function(window, document){
	'use strict';
	var slice = [].slice;
	var removeClass = function(elem){
		elem.classList.remove('focus-within');
	};
	var update = (function(){
		var running, last;
		var action = function(){
			var element = document.activeElement;
			running = false;
			if(last !== element){
				last = element;
				slice.call(document.getElementsByClassName('focus-within')).forEach(removeClass);
				while(element && element.classList){
					element.classList.add('focus-within');
					element = element.parentNode;
				}
			}
		};
		return function(){
			if(!running){
				requestAnimationFrame(action);
				running = true;
			}
		};
	})();
	document.addEventListener('focus', update, true);
	document.addEventListener('blur', update, true);
	update();
})(window, document);
