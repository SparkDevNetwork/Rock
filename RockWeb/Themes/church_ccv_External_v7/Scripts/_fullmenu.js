$(document).ready(function(){

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

})