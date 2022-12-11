function BindNavEvents() {
  $(document).ready(function() {
    var bodyElement = $('body'),
    navElement = $('.navbar-side'),
    hoverDelay = 200,
    hideDelay = 150;

    $('.navbar-side > li').on("mouseenter", function() {
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
            $('body').addClass('nav-open');
            if ($(document).height() > $(window).height()) {
                var scrollWidth = Rock.controls.util.getScrollbarWidth();
                $('body').css('padding-right', scrollWidth);
                $('.navbar-fixed-top').css('right', scrollWidth);
            }
            $this[0].navHoverTimeout = undefined;
          }, hoverDelay);
        }
      }
    });

    $('.navbar-side > li').on("mouseleave", function() {
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
              .removeClass('nav-open')
              .css('padding-right', '');
              $('.navbar-fixed-top').css('right', '');
          }
          $this[0].navUnHoverTimeout = undefined;
        }, hideDelay);
      }
    });

    $('.navbar-side > li.has-children').on("click", function() {
      if ($(this).hasClass("open")) {
        $('.navbar-side > li').removeClass('open');
        $('.navbar-static-side').removeClass('open-secondary-nav');
      } else {
        $('.navbar-side > li').removeClass('open');
        $('.navbar-static-side').addClass('open-secondary-nav');
        $(this).addClass('open');
      }
    });


    $('#content-wrapper').on("click", function() {
      $('body').removeClass('nav-open');
      $('.navbar-static-side').removeClass('open-secondary-nav');
      $('.navbar-side li').removeClass('open');
    });

    // show/hide sidebar nav
    $('.navbar-toggle-side-left').on("click", function() {
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
  });

  $('.js-note-editor .meta-body').on("focusin", function() {
    var noteBody = $(this);
    // calculate height of noteBody
    var height = noteBody.prop('scrollHeight');
    noteBody.addClass("focus-within").css('height', height);
    
    
    noteBody.on('webkitTransitionEnd otransitionend oTransitionEnd msTransitionEnd transitionend', function(e) {
      if ($(this).hasClass("focus-within")) {
        noteBody.addClass("overflow-visible");
      }
    });

    ResizeTextarea();

    unfocusOnClickOutside(this, function() {
        noteBody.removeClass("focus-within overflow-visible").css('height', '');
    })
  })
}

function unfocusOnClickOutside(element, callback) {
    $(document).on("click.unfocus", function(e) {
        if (!$(element).is(e.target) && $(element).has(e.target).length === 0) {
            callback();
            // disable the event handler to avoid unwanted behaviours
            $(document).off(".unfocus");
        }
    });
}

// use mutuation observer to resize textarea
function ResizeTextarea() {
    $('.js-notetext').on("input", function() {
        // resize textarea
        var textarea = $(this);
        textarea.css('height', 'auto').css('height', textarea.prop('scrollHeight'));
        // get closest note-editor and resize it
        var noteEditor = textarea.closest('.focus-within').addClass("no-transition");
        noteEditor.css('height', 'auto').css('height', noteEditor.prop('scrollHeight'));
    });
}

// Fixes an issue with the wait spinner caused by browser Back/Forward caching.
function HandleBackForwardCache() {

    // Forcibly hide the wait spinner if the page is being reloaded from cache.
    // Browsers that implement back/forward caching may otherwise continue to display the wait spinner when the page is restored.
    // This fix is not effective for Safari browsers prior to v13, due to a known bug in the bfcache implementation.
    // (https://bugs.webkit.org/show_bug.cgi?id=156356)
    $(window).bind('pageshow', function (e) {
        if ( e.originalEvent.persisted )
        {
            $('#updateProgress').hide();
        }
    });
}
