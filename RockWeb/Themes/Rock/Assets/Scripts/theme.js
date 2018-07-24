function getScrollbarWidth() {
  // thx d.walsh
  var scrollDiv = document.createElement('div');
  scrollDiv.className = 'modal-scrollbar-measure';
  document.body.appendChild(scrollDiv);
  var scrollbarWidth = scrollDiv.offsetWidth - scrollDiv.clientWidth;
  document.body.removeChild(scrollDiv);
  return scrollbarWidth;
} // Static

function BindNavEvents() {
  $(document).ready(function() {
    var bodyElement = $('body'),
    topNavElement = $('.navbar-fixed-top'),
    navElement = $('.navbar-side'),
    hoverDelay = 250,
    hideDelay = 100;

    $('.navbar-side > li').mouseenter(function() {
      const $this = $(this);
      if ($this.navUnHoverTimeout !== undefined) {
        clearTimeout($this[0].navUnHoverTimeout);
        $this[0].navUnHoverTimeout = undefined;
      } else if ($this[0].navHoverTimeout === undefined) {
        if (navElement.find('li.open').length > 0) {
            $('.navbar-side > li').removeClass('open');
            $this.addClass('open');
            $this[0].navHoverTimeout = undefined;
          //}, 0);
        } else {
          $this[0].navHoverTimeout = setTimeout(function() {
            $this.addClass('open');
            $('body')
              .addClass('modal-open')
              .css('padding-right', getScrollbarWidth());
              $('.navbar-fixed-top').css('right', getScrollbarWidth());
            $this[0].navHoverTimeout = undefined;
          }, hoverDelay);
        }
      }
    });

    $('.navbar-side > li').mouseleave(function() {
      const $this = $(this);
      if ($this[0].navHoverTimeout !== undefined) {
        clearTimeout($this[0].navHoverTimeout);
        $this[0].navHoverTimeout = undefined;
      } else if ($this[0].navUnHoverTimeout === undefined) {
        $this[0].navUnHoverTimeout = setTimeout(function() {
          // $('.navbar-side > li').removeClass('open');
          $this.removeClass('open');
          if (navElement.find('li.open').length <= 1) {
            $('body')
              .removeClass('modal-open')
              .css('padding-right', '');
              $('.navbar-fixed-top').css('right', '');
          }
          $this[0].navUnHoverTimeout = undefined;
        }, hideDelay);
      }
    });

    $('#content-wrapper').click(function() {
      $('.navbar-side li').removeClass('open');
    });

    // show/hide sidebar nav
    $('.navbar-toggle-side-left').click(function() {
      if ($('.navbar-static-side').is(':visible')) {
        bodyElement
          .addClass('navbar-side-close')
          .removeClass('navbar-side-open');
      } else {
        bodyElement
          .addClass('navbar-side-open')
          .removeClass('navbar-side-close');
      }
    });
  });
}
