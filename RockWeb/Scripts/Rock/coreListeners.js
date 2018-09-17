Sys.Application.add_load(function () {
  $(function () {
    $('[data-toggle="tooltip"]').tooltip()
  })

  var navbarFixedTop = document.querySelector('.navbar-fixed-top');
  var stickyHeaders = document.querySelector('.js-sticky-headers');

  if ($(navbarFixedTop).length){
    $(stickyHeaders).stickyTableHeaders({fixedOffset: $(navbarFixedTop)});
  } else {
    $(stickyHeaders).stickyTableHeaders();
  }
})