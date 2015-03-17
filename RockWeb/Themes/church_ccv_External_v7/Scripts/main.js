// @prepros-prepend "_slider.js"
// @prepros-prepend "_fullmenu.js"

// BS Tooltips
$(function () {
  $('[data-toggle="tooltip"]').tooltip()
})

$(function () {
  $('.flexbg').css('background-image', function() {
    return 'url(' + $(this).attr('data-flexbg') + ')'
  })
})