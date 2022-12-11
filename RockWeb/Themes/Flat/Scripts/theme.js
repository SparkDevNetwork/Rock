var slider;
$(document).ready(function() {
	slider = $('.flexslider').flexslider({
		animation: 'slide',
		manualControls: '.slide-control li',
		directionNav: false,
		pauseOnAction: false,
		pauseOnHover: true,
		start: function(s) { moveTipper($('.slide-control li').eq(s.animatingTo), 'loaded') },
		before: function(s) { moveTipper($('.slide-control li').eq(s.animatingTo)) }
	})

	var moveTipper = function($current, addedClass) {
		var left = ($current.position().left) + ($current.width() / 2) - ($('.tipper').width() / 2)
		$('.tipper').css({ 'left': left+'px' }).addClass(addedClass)
	}

	$(window).resize(function(){
		var $current = $('.slide-control ul > li.flex-active')
		moveTipper($current)
	})
})

// Fixes an issue with the wait spinner caused by browser Back/Forward caching.
function HandleBackForwardCache() {

    // Forcibly hide the wait spinner if the page is being reloaded from cache.
    // Browsers that implement back/forward caching may otherwise continue to display the wait spinner when the page is restored.
    // This fix is not effective for Safari browsers prior to v13, due to a known bug in the bfcache implementation.
    // (https://bugs.webkit.org/show_bug.cgi?id=156356)
    $(window).bind('pageshow', function (e) {
        if ( e.originalEvent.persisted )
        {
            $('#updateProgress').hide();
        }
    });
}
