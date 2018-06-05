// @prepros-prepend "_slider.js"
// @prepros-prepend "_fullmenu.js"
// @prepros-prepend "_vimeo-thumb.js"
// @prepros-prepend "_search.js"

// BS Tooltips
$(function () {
  $('[data-toggle="tooltip"]').tooltip()
})

// Setup defaults for magnific popup
$(function() {
  $('.popup-youtube, .popup-vimeo, .popup-gmaps').magnificPopup({
    disableOn: 700,
    type: 'iframe',
    mainClass: 'mfp-fade',
    removalDelay: 160,
    preloader: false,
    fixedContentPos: false
  })
  $('.lightbox').magnificPopup({
    type: 'image',
    closeOnContentClick: true,
    mainClass: 'mfp-img-mobile',
    image: {
      verticalFit: true
    }
  })
})

// Setup fitvids
$(function() {
  $('body').fitVids();
})
