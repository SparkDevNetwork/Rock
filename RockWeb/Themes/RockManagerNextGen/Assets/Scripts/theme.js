

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

    $('.js-notetext').on("blur", function() {
      $(this).parent().removeClass("focus-within");
    })
    .on("focus", function() {
      $(this).parent().addClass("focus-within")
    });
  });
};

var docCookies = {
    getItem: function (sKey) {
      if (!sKey) { return null; }
      return decodeURIComponent(document.cookie.replace(new RegExp("(?:(?:^|.*;)\\s*" + encodeURIComponent(sKey).replace(/[\-\.\+\*]/g, "\\$&") + "\\s*\\=\\s*([^;]*).*$)|^.*$"), "$1")) || null;
    },
    setItem: function (sKey, sValue, vEnd, sPath, sDomain, bSecure) {
      if (!sKey || /^(?:expires|max\-age|path|domain|secure)$/i.test(sKey)) { return false; }
      var sExpires = "";
      if (vEnd) {
        switch (vEnd.constructor) {
          case Number:
            sExpires = vEnd === Infinity ? "; expires=Fri, 31 Dec 9999 23:59:59 GMT" : "; max-age=" + vEnd;
            break;
          case String:
            sExpires = "; expires=" + vEnd;
            break;
          case Date:
            sExpires = "; expires=" + vEnd.toUTCString();
            break;
        }
      }
      document.cookie = encodeURIComponent(sKey) + "=" + encodeURIComponent(sValue) + sExpires + (sDomain ? "; domain=" + sDomain : "") + (sPath ? "; path=" + sPath : "") + (bSecure ? "; secure" : "");
      return true;
    },
    removeItem: function (sKey, sPath, sDomain) {
      if (!this.hasItem(sKey)) { return false; }
      document.cookie = encodeURIComponent(sKey) + "=; expires=Thu, 01 Jan 1970 00:00:00 GMT" + (sDomain ? "; domain=" + sDomain : "") + (sPath ? "; path=" + sPath : "");
      return true;
    },
    hasItem: function (sKey) {
      if (!sKey) { return false; }
      return (new RegExp("(?:^|;\\s*)" + encodeURIComponent(sKey).replace(/[\-\.\+\*]/g, "\\$&") + "\\s*\\=")).test(document.cookie);
    },
    keys: function () {
      var aKeys = document.cookie.replace(/((?:^|\s*;)[^\=]+)(?=;|$)|^\s*|\s*(?:\=[^;]*)?(?:\1|$)/g, "").split(/\s*(?:\=[^;]*)?;\s*/);
      for (var nLen = aKeys.length, nIdx = 0; nIdx < nLen; nIdx++) { aKeys[nIdx] = decodeURIComponent(aKeys[nIdx]); }
      return aKeys;
    }
  };

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

document.addEventListener("DOMContentLoaded", () => {
  topHeaderOffset();
  // Update on resize and header size changes.
  window.addEventListener("resize", topHeaderOffset, { passive: true });
  const header = document.querySelector(".rock-top-header");
  if (header && "ResizeObserver" in window) {
    new ResizeObserver(() => topHeaderOffset()).observe(header);
  }
});



let attributeName = "theme"
var htmlElement = document.documentElement;

const states = ["light", "dark", "system"];
let currentStateIndex;

const value = localStorage.getItem(attributeName);
if (value != null) {
htmlElement.setAttribute(attributeName, value);
currentStateIndex = states.indexOf(value);
} else {
currentStateIndex = 2;
};
document.addEventListener("DOMContentLoaded", function () {
    const button = document.getElementById("themeSwitchButton");

    button.addEventListener("click", () => {

        currentStateIndex = (currentStateIndex + 1) % states.length;

        localStorage.setItem(attributeName, states[currentStateIndex]);

        const value = localStorage.getItem(attributeName);
        htmlElement.setAttribute(attributeName, value);
    });
});
