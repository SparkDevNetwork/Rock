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
	// Forcibly hide the wait spinner, and clear the pending request if the page is being reloaded from bfcache. (Currently WebKit only)
	// Browsers that implement bfcache will otherwise trigger updateprogress because the pending request is still in the PageRequestManager state.
	// This fix is not effective for Safari browsers prior to v13, due to a known bug in the bfcache implementation.
	// (https://bugs.webkit.org/show_bug.cgi?id=156356)
	window.addEventListener('pageshow', function (e) {
		if ( e.persisted ) {
			document.querySelector('#updateProgress').style.display = 'none';
			// Check if the page is in postback, and if so, reset the PageRequestManager state.
			if (Sys.WebForms.PageRequestManager.getInstance().get_isInAsyncPostBack()) {
				// Reset the PageRequestManager state. & Manually clear the request object
				Sys.WebForms.PageRequestManager.getInstance()._processingRequest = false;
				Sys.WebForms.PageRequestManager.getInstance()._request = null;
			}
		}
	});
}
