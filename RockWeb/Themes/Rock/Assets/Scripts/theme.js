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
    hoverDelay = 200,
    hideDelay = 150;

    $('.navbar-side > li').mouseenter(function() {
      const $this = $(this);
      if ($this.navUnHoverTimeout !== undefined) {
        clearTimeout($this[0].navUnHoverTimeout);
        $this[0].navUnHoverTimeout = undefined;
      } else if ($this[0].navHoverTimeout === undefined) {
        if (navElement.find('li.open').length > 0) {
            $('.navbar-side > li').removeClass('open');
            $('.navbar-static-side').addClass('open-secondary-nav');
            $this.addClass('open');
            $this[0].navHoverTimeout = undefined;
        } else {
          $this[0].navHoverTimeout = setTimeout(function() {
            $this.addClass('open');
            $('.navbar-static-side').addClass('open-secondary-nav');
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
          $this.removeClass('open');
          if ($('.navbar-side').find('li.open').length < 1) {
            $('.navbar-static-side').removeClass('open-secondary-nav');
            $('body')
              .removeClass('modal-open')
              .css('padding-right', '');
              $('.navbar-fixed-top').css('right', '');
          }
          $this[0].navUnHoverTimeout = undefined;
        }, hideDelay);
      }
    });

    $('.navbar-side > li.has-children').click(function() {
      if ($(this).hasClass("open")) {
        $('.navbar-side > li').removeClass('open');
        $('.navbar-static-side').removeClass('open-secondary-nav');
      } else {
        $('.navbar-side > li').removeClass('open');
        $('.navbar-static-side').addClass('open-secondary-nav');
        $(this).addClass('open');
      }
    });


    $('#content-wrapper').click(function() {
      $('body').removeClass('modal-open');
      $('.navbar-static-side').removeClass('open-secondary-nav');
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

function PreventNumberScroll() {
  $(document).ready(function() {
    // disable mousewheel on a input number field when in focus
    // (to prevent Cromium browsers change the value when scrolling)
    $('form').on('focus', 'input[type=number]', function (e) {
      $(this).on('mousewheel.disableScroll', function (e) {
        e.preventDefault()
      })
    });
    $('form').on('blur', 'input[type=number]', function (e) {
      $(this).off('mousewheel.disableScroll')
    });
    $('form').on('keydown', 'input[type=number]', function (e) {
        if (e.which === 38 || e.which === 40) {
            e.preventDefault();
        }
    });

    $('.js-notetext').blur(function() {
      $(this).parent().removeClass("focus-within");
    })
    .focus(function() {
      $(this).parent().addClass("focus-within")
    });
  });
}
