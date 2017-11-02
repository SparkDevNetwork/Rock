//<script type="text/javascript" src="https://localhost:44347/Themes/church_ccv_External_v8/Scripts/effects.js"></script>

$(document).scroll( function() {
   updateNavbarForScroll( );
   
   updateSubNavbarForScroll( );
});

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

var subNavbarStartingPos = null;
function updateSubNavbarForScroll( )
{
	var subNavbar = $("#subnavbar");
	
	// if we haven't gotten its starting position, do so now.
	// I hate doing this here, but I don't have access to OnLoad()
	if( subNavbarStartingPos == null )
	{
		subNavbarStartingPos = subNavbar.offset().top;
	}
	
	var windowPos = $(window).scrollTop();
	
	if( windowPos + (subNavbar.outerHeight() * 1.5) > subNavbarStartingPos )
	{
		var topNavbar = $(".masthead");
		
		// get the height of the top navbar
		var navbarHeight = topNavbar.outerHeight( );
		
		subNavbar.css( "position", "fixed" );
		subNavbar.css( "top", navbarHeight + "px" );
	}
	else
	{
		subNavbar.css( "position", "static" );
		subNavbar.css( "top", subNavbarStartingPos + "px" );
	}
}

// Spinning Loader
// To use this, include the following HTML divs
/*
<div class="loader-bg loader-bg-hidden">
	<div class="loader loader-hidden"></div>
</div>

// and CSS
.loader {
	border: 16px solid #f3f3f3;
	border-top: 16px solid #7A1315;
	border-radius: 50%;
	width: 120px;
	height: 120px;
	animation: spin 2s linear infinite;
	position: relative;
	margin: 0 auto;
	top: 200px;
}

@keyframes spin {
	0% 		{ transform: rotate(0deg); }
	100% 	{ transform: rotate(360deg); }
}

.loader-bg {
	background: rgba(0, 0, 0, .45);
	position: absolute;
	padding: 0;
	margin: 0;
	left: 0;
	top: 0;
	width: 100%;
	height: 100%;
	z-index: 1500;
}

.loader-bg-visible {
	visibility: visible;
}

.loader-bg-hidden {
	visibility: hidden;
}

.loader-visible {
	visibility: visible;	
}

.loader-hidden {
	visibility: hidden;
}
*/
 function displayLoader() {
	var loaderBg = $(".loader-bg");
	loaderBg.removeClass("loader-bg-hidden");
	loaderBg.addClass("loader-bg-visible");

	var loader = $(".loader");
	loader.removeClass("loader-hidden");
	loader.addClass("loader-visible");
}

function hideLoader() {
	var loaderBg = $(".loader-bg");
	loaderBg.removeClass("loader-bg-visible");
	loaderBg.addClass("loader-bg-hidden");

	var loader = $(".loader");
	loader.removeClass("loader-visible");
	loader.addClass("loader-hidden");
}
//
