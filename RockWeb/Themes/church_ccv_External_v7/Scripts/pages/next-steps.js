// @prepros-prepend "../Vendor/greensock/TweenMax.min.js"
// @prepros-prepend "../Vendor/scrollmagic/uncompressed/ScrollMagic.js"
// @prepros-prepend "../Vendor/scrollmagic/uncompressed/plugins/animation.gsap.js"
// prepros-prepend "../Vendor/scrollmagic/uncompressed/plugins/debug.addIndicators.js"

var controller = new ScrollMagic.Controller();

$(document).ready(function(){
  var pinDurationCache
  function pinDuration () {
    return pinDurationCache
  }
  function calcPinDuration () {
    var r = $('.js-pin-height').height()
    r -= $('.js-pin-height-trim').height()
    r -= 40*2
    if (window.innerWidth >= 1200) {
      r -= 40
    }
    pinDurationCache = r
  }

  var t1DurationCache
  function t1Duration () {
    return t1DurationCache
  }
  function calcT1Duration () {
    t1DurationCache = $("#trigger1").next(".bigblock").height() + 90
  }

  var t2DurationCache
  function t2Duration () {
    return t2DurationCache
  }
  function calcT2Duration () {
    t2DurationCache = $("#trigger2").next(".bigblock").height() + 90
  }

  var t3DurationCache
  function t3Duration () {
    return t3DurationCache
  }
  function calcT3Duration () {
    t3DurationCache = $("#trigger3").next(".bigblock").height() + 90
  }
  
  var t4DurationCache
  function t4Duration () {
    return t4DurationCache
  }
  function calcT4Duration () {
    t4DurationCache = $("#trigger4").next(".bigblock").height() + 90
  }

  function calcAllSizes () {
    calcPinDuration()
    calcT1Duration()
    calcT2Duration()
    calcT3Duration()
    calcT4Duration()
  }

  calcAllSizes()
  $(window).on("resize", calcAllSizes)

  new ScrollMagic.Scene({triggerElement: "#trigger1", duration: pinDuration})
    .setPin("#pin1")
    // .addIndicators({name: "pin"})
    .triggerHook("onLeave")
    .addTo(controller);

  new ScrollMagic.Scene({triggerElement: "#trigger2", duration: t2Duration, offset: -100})
    .setClassToggle("#hide2", "active")
    // .addIndicators()
    .triggerHook("onLeave")
    .on("start", function(e){
      if (e.scrollDirection === "FORWARD") $("#hide1").removeClass("active")
      if (e.scrollDirection === "REVERSE") $("#hide1").addClass("active")
    })
    .addTo(controller);

  new ScrollMagic.Scene({triggerElement: "#trigger3", duration: t3Duration, offset: -100})
    .setClassToggle("#hide3", "active")
    // .addIndicators()
    .triggerHook("onLeave")
    .addTo(controller);

  new ScrollMagic.Scene({triggerElement: "#trigger4", offset: -100})
    .setClassToggle("#hide4", "active")
    // .addIndicators()
    .triggerHook("onLeave")
    .addTo(controller);

})
