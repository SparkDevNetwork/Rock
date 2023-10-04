$(document).ready(function (e) {
    $("#input-hdr-search").val("");
    $("#input-hdr-search").keypress(function (e) {
        if (e.keyCode === 13) {
            e.preventDefault();
            inputHdrSearch();
            return false;
        }
    });
    if ($("header .navbar-form").prev(".navbar-nav").outerWidth() > 100)
    {
      $("header .navbar-form").css("max-width", $("header .navbar-form").prev(".navbar-nav").outerWidth());
    }
    $('ul.dropdown-menu [data-toggle=dropdown]').on('click', function (event)
    {
      // Avoid following the href location when clicking
      event.preventDefault();
      // Avoid having the menu to close when clicking
      event.stopPropagation();
      var isOpen = $(this).parent().hasClass('open');
      // If a menu is already open we close it
      $('ul.dropdown-menu [data-toggle=dropdown]').parent().removeClass('open');
      // opening the one you clicked on
      if (!isOpen) $(this).parent().addClass('open');
    });
    $('[data-toggle="tooltip"]').tooltip();
    $(".et_pb_scroll_top").length && ($(window).scroll(function ()
    {
      $(this).scrollTop() > 800 ? $(".et_pb_scroll_top").show().removeClass("et-hidden").addClass("et-visible") : $(".et_pb_scroll_top").removeClass("et-visible").addClass("et-hidden")
    }),
    $(".et_pb_scroll_top").click(function ()
    {
      $("html, body").animate({
        scrollTop: 0
      }, 800)
    }));
});

function inputHdrSearch() {
    var searchVal = "";
    searchVal = $("#input-hdr-search").val();
    if (searchVal !== "") {
        window.location = "//www.lakepointe.org/?s=" + searchVal;
    } else {
        return false;
    }
}