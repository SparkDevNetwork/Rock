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
      }, 800);
    }));
  setHeightParallax();
    setTransform();
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

function setTransform() {
  if ($('.section-parallax').length && void 0 !== $('.section-parallax') && void 0 !== $('.section-parallax').offset()) {
    var main_position, $this = $('.section-parallax'), element_top = $this.offset().top;
    main_position = "translate(0, " + .3 * ($(window).scrollTop() + $(window).height() - element_top) + "px)",
      $this.children(".et_parallax_bg").css({
        "-webkit-transform": main_position,
        "-moz-transform": main_position,
        "-ms-transform": main_position,
        transform: main_position
      });
  }
}
function setHeightParallax() {
  var bg_height, $this = $('.section-parallax');
  bg_height = .3 * $(window).height() + $this.innerHeight(),
    $this.find(".et_parallax_bg").css({
      height: bg_height
    });
}
