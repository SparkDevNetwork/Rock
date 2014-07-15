var slider;
$(document).ready(function() {
	slider = $('.flexslider').flexslider({
		animation: 'slide',
		manualControls: '.slide-control li',
		directionNav: false,
		pauseOnAction: false,
		pauseOnHover: true,
		start: function(s) { moveTipper(s, 'loaded') },
		before: function(s) { moveTipper(s) }
	})

	var moveTipper = function(slider, addedClass) {
		var $current = $('.slide-control li').eq(slider.animatingTo)
		var left = ($current.position().left) + ($current.width() / 2) - ($('.tipper').width() / 2)
		$('.tipper').css({ 'left': left+'px' }).addClass(addedClass)
	}
})