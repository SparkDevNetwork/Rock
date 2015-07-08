// @prepros-prepend "_slider.js"
// @prepros-prepend "_fullmenu.js"

// BS Tooltips
$(function () {
  $('[data-toggle="tooltip"]').tooltip()
})

// Select Campus
$(function () {
  window.CCV = window.CCV || {}

  CCV.selectCampus = function (campusId) {
    Rock.utility.setContext('campuses', campusId)

    // wait until API adds the cookie
    $(document).ajaxComplete(function( event, xhr, settings ) {
      if (settings.url.indexOf('/api/campuses/SetContext/') > -1) {
        // reloads page from cache
        location.reload()
      }
    })
  }
})
