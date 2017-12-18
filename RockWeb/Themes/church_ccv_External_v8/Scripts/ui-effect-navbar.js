//<script type="text/javascript" src="https://localhost:44347/Themes/church_ccv_External_v8/Scripts/effects.js"></script>

// adds on "onload" hook to the window.onload function chain
var oldonload = window.onload;
window.onload = (typeof window.onload != 'function') ?
  handleOnLoad : function() { 
   oldonload(); handleOnLoad();
};

function handleOnLoad() {
  // setup a callback for when the media query triggers
  const mq = window.matchMedia( "(min-width: 800px)" );
  mq.addListener( mediaQueryTriggered );
  
  mediaQueryTriggered( mq );
}

$(document).scroll( function() {
	
	// anytime the browser is resized or scrolled, 
	// we need to update the positioning of the nav bars
   if( navbarFadeEnabled ) {
      updateNavbarForScroll( );
   }
});

$(window).resize(function() {
	
	// anytime the browser is resized or scrolled, 
	// we need to update the positioning of the nav bars
	if( navbarFadeEnabled ) {
      updateNavbarForScroll( );
   }
});

var navbarFadeEnabled = true;
function mediaQueryTriggered( mediaQuery ) {
	
   // we want to know if the browser is within our "desktop size" media query.
   // if so, enable the navbar fade effect. If not, we'll turn it off
	if( mediaQuery.matches ) {
		toggleNavbarEffect( true );
	}
	else {
		toggleNavbarEffect( false );
	}
}

function toggleNavbarEffect( enabled ) {
	navbarFadeEnabled = enabled;
	
   // whenever we turn it off, force the background to black.
	if( navbarFadeEnabled == false ) {
		var navBar = $(".masthead");
		navBar.css( "background-color", "rgba( 0, 0, 0, 1 )" );
	}
   else {
      // otherwise, let it update according to where the page is scrolled
      updateNavbarForScroll( );
   }
}

function updateNavbarForScroll( )
{
   // first get the navbar
   var navBar = $(".masthead");

   var windowPos = $(window).scrollTop();

   // get the height of the navbar
   var navbarHeight = navBar.outerHeight( );
          
   // take the distance from the windowPos TO the end of the navbar (since the navbar is at 0 we can just use its height)
   var deltaPos = Math.max( navbarHeight - windowPos, 0 );
   
   // invert alpha so its transparent when we aren't scrolled
   var alpha = 1.0 - (deltaPos / navbarHeight);

   navBar.css( "background-color", 'rgba( 0, 0, 0,' +  alpha + ')' );
}
//
