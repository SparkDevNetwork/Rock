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