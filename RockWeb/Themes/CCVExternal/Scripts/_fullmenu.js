$(document).ready(function(){


  var isMobile = function() {
    if ($(document).width() < 768) return true // @screen-sm-min
    else return false
  }


  // Fade in animation
  $('.js-fullmenu').on('show.bs.collapse', function () {

    // use settimeout the add to the event queue, making fade in visible
    setTimeout(function(){
      $('.js-fullmenu .fade').addClass('in')
    },0)
  })

  $('.js-fullmenu').on('hide.bs.collapse', function () {
    $('.js-fullmenu .fade').removeClass('in')
  })


  // Scroll Prevention
  $('.js-fullmenu').on('shown.bs.collapse', function () {
    if (!isMobile())
      $('html').addClass('has-open-fullmenu')
  })

  $('.js-fullmenu').on('hidden.bs.collapse', function () {
    $('html').removeClass('has-open-fullmenu')
  })
})