var CCVSlider
$(document).ready(function() {
  var $slider = $('.flexslider')
  if ($slider[0]) {

    var slider = $('.flexslider').flexslider({
      animation: 'slide',
      manualControls: '.slide-control li',
      directionNav: false,
      pauseOnAction: false,
      pauseOnHover: true,
      start: function(s) {
        console.log('starting')
        var $sliderWrap = $('.announcement-rotator')
        if ($sliderWrap[0]) {
          $sliderWrap.removeClass('loading')
        }
        CCVSlider = s
        moveTipper($('.slide-control li').eq(s.animatingTo), 'loaded')
      },
      before: function(s) {
        moveTipper($('.slide-control li').eq(s.animatingTo))
      }
    })

    var moveTipper = function($current, addedClass) {
      var left = ($current.position().left) + ($current.width() / 2) - ($('.tipper').width() / 2)
      $('.tipper').css({ 'left': left+'px' }).addClass(addedClass)
    }

    $(window).resize(function(){
      var $current = $('.slide-control ul > li.flex-active')
      moveTipper($current)
    })
  }
})
