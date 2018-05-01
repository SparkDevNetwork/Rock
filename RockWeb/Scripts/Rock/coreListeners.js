Sys.Application.add_load(function () {
  var navbarFixedTop = document.querySelector('.navbar-fixed-top');
  var stickyHeaders = document.querySelector('.js-sticky-headers');

  if ($(navbarFixedTop).length){
    $(stickyHeaders).stickyTableHeaders({fixedOffset: $(navbarFixedTop)});
  } else {
    $(stickyHeaders).stickyTableHeaders();
  }
})