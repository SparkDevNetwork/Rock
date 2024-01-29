function BindNavEvents() {
  $(document).ready(function() {
    const bodyElement = $('body');

    // if the window is greater than 768px then use the custom hover events
    navMouseEvents();

    // on resize, run the navMouseEvents function
    $(window).on("resize", function() {
      navMouseEvents();
    });

    $('.navbar-side > li.has-children').on("click", function(e) {
      const isOpen = $(this).hasClass('open');
      if ($(e.target).closest('.title').length || $(window).width() > 768) {
        $('.navbar-side > li').removeClass('open');
      }
      if (!isOpen) {
        $('.navbar-side > li').removeClass('open');
        bodyElement.addClass('nav-open');
        $('.navbar-static-side').addClass('open-secondary-nav');
        $(this).addClass('open');
      } else {
        $('.navbar-side > li').removeClass('open');
        bodyElement.removeClass('nav-open');
        $('.navbar-static-side').removeClass('open-secondary-nav');
      }
    });

    $('#content-wrapper').on("click", function() {
      bodyElement.removeClass('nav-open');
      $('.navbar-static-side').removeClass('open-secondary-nav');
      $('.navbar-side li').removeClass('open');
    });

    // show/hide sidebar nav
    $('.navbar-static-side').on("show.bs.collapse hide.bs.collapse", function(e) {
      e.preventDefault();
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

    // Because the header naturally closes addClass navbar-side-open when a modal is open
    if ($('#fixed-header').length) {
      if ( $('#fixed-header').find('.modal.in').length ) {
        bodyElement.addClass('navbar-side-open');
        // Listen for modal close
        $('#fixed-header').find('.modal.in').on('hidden.bs.modal', function (e) {
          bodyElement.removeClass('navbar-side-open');
        })
      }
    }

    topHeaderOffset()

    var topHeader = $('.rock-top-header');
    // create a new resize observer to watch for changes in the top header height
    var topHeaderResizeObserver = new ResizeObserver(function(entries) {
      topHeaderOffset()
    });
    // start observing the top header element
    topHeaderResizeObserver.observe(topHeader[0]);

  });
}

function topHeaderOffset() {
  // watch the .rock-top-header element for changes in height and write the new height to the body element as a css variable
  var topHeader = document.querySelector('.rock-top-header');
  document.body.style.setProperty('--top-header-height', topHeader.offsetHeight + 'px');
  // if the top header is a fixed header, and is not position relative, then also set the --top-header-fixed-height css variable
  // get computed topHeader style and check if position is relative
  var topHeaderStyle = window.getComputedStyle(topHeader);
  if (topHeaderStyle.position !== 'relative') {
    document.body.style.setProperty('--sticky-element-offset', topHeader.offsetHeight + 'px');
  } else {
    document.body.style.setProperty('--sticky-element-offset', '0px');
  }
}

function navMouseEvents() {
  var hoverDelay = 200,
  hideDelay = 150;

  const navbarSide = $('.navbar-side');
  const navbarSideLi = navbarSide.children('li');
  const navbarStaticSide = $('.navbar-static-side');
  const navbarFixedTop = $('.navbar-fixed-top');
  const bodyElement = $('body');

  if ($(window).width() > 768) {
    navbarSideLi.on("mouseenter.sidenav", function() {
      const $this = $(this);
      if ($this.data('navUnHoverTimeout')) {
        clearTimeout($this.data('navUnHoverTimeout'));
        $this.removeData('navUnHoverTimeout');
      } else if (!$this.data('navHoverTimeout')) {
        const openLi = navbarSide.find('li.open');
        if (openLi.length > 0) {
          navbarSideLi.removeClass('open');
          navbarStaticSide.addClass('open-secondary-nav');
          $this.addClass('open');
          $this.removeData('navHoverTimeout');
        } else {
          $this.data('navHoverTimeout', setTimeout(function() {
            if ($(document).height() > $(window).height()) {
              const scrollWidth = window.innerWidth - document.documentElement.clientWidth; //window.innerWidth - document.documentElement.clientWidth;
              bodyElement.css('padding-right', scrollWidth);
              navbarFixedTop.css('right', scrollWidth);
            }

            $this.addClass('open');
            navbarStaticSide.addClass('open-secondary-nav');
            bodyElement.addClass('nav-open');
            $this.removeData('navHoverTimeout');
          }, hoverDelay));
        }
      }
    });

    navbarSideLi.on("mouseleave.sidenav", function() {
      const $this = $(this);
      if ($this.data('navHoverTimeout')) {
        clearTimeout($this.data('navHoverTimeout'));
        $this.removeData('navHoverTimeout');
      } else if (!$this.data('navUnHoverTimeout')) {
        $this.data('navUnHoverTimeout', setTimeout(function() {
          $this.removeClass('open');
          if (navbarSide.find('li.open').length < 1) {
            navbarStaticSide.removeClass('open-secondary-nav');
            bodyElement.removeClass('nav-open').css('padding-right', '');
            navbarFixedTop.css('right', '');
          }
          $this.removeData('navUnHoverTimeout');
        }, hideDelay));
      }
    });
  } else {
    console.log("remove mouseenter mouseleave");
    navbarSideLi.off(".sidenav");
  }
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
	// Forcibly hide the wait spinner, and clear the pending request if the page is being reloaded from bfcache. (Currently WebKit only)
	// Browsers that implement bfcache will otherwise trigger updateprogress because the pending request is still in the PageRequestManager state.
	// This fix is not effective for Safari browsers prior to v13, due to a known bug in the bfcache implementation.
	// (https://bugs.webkit.org/show_bug.cgi?id=156356)
	window.addEventListener('pageshow', function (e) {
		if ( e.persisted ) {
			document.querySelector('#updateProgress').style.display = 'none';
			// Check if the page is in postback, and if so, reset the PageRequestManager state.
			if (Sys.WebForms.PageRequestManager.getInstance().get_isInAsyncPostBack()) {
				// Reset the PageRequestManager state. & Manually clear the request object
				Sys.WebForms.PageRequestManager.getInstance()._processingRequest = false;
				Sys.WebForms.PageRequestManager.getInstance()._request = null;
			}
		}
	});
}
