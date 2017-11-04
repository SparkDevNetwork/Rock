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

var subNavbarStartingPos = 0;

function updateSubNavbarForScroll( )
{
	updateSubNavbarSnap( );
	
	updateSubNavbarLinks( );
}

// "Snaps" the sub navbar to underneath the primary navbar when scrolling the page
function updateSubNavbarSnap( )
{
	// get the "section A" element, which is where the body content starts
	var sectionA = document.getElementById("section-a-bg");
	
	// seed the navbar starting position on first update (since we don't have OnLoad() access)
	var subNavbar = document.getElementById("subnavbar-bg");
	if ( subNavbarStartingPos == 0 )
	{
		subNavbarStartingPos = subNavbar.offsetTop;
	}

	// if the top navbar has clipped the sub navbar, we've gone far enough to snap
	var topNavbar = document.getElementById("masthead");	
	if( window.pageYOffset + topNavbar.offsetHeight >= subNavbarStartingPos )
	{
		// snap the sub navbar to underneath the top navbar
		subNavbar.style.top = topNavbar.offsetHeight + "px";
		subNavbar.style.position = "fixed";
		subNavbar.style.width = "100%";
		
		// add the sub navbars height to the page so the page doesn't jump
		sectionA.style.paddingTop = subNavbar.offsetHeight + "px";
	}
	else
	{
		// clear all styling to effectively put things like they were when the page loaded.
		subNavbar.style.top = "";
		subNavbar.style.position = "";
		subNavbar.style.width = "";
		
		sectionA.style.paddingTop = "";	
	}
}

// "Highlights" the appropriate sub navbar link based on where the user has scrolled in the page
// To make this work:
// Ensure the items in the subnavbar have the following ID naming convention:
// subnav-ID-NAME (ex: subnav-the-details)
// The actual anchor should have the same ID, but without "subnav" prefixed.
var subnavIdList = [];
function updateSubNavbarLinks( )
{
	// seed the list array if we haven't yet (since we don't have OnLoad() access)
	if( subnavIdList.length == 0 )
	{
		// first get all subnav links
		var navLinks = $("#subnavbar ul li").children();
		
		for( var i = 0; i < navLinks.length; i++ ) {
			// get the anchor element
			var aElem = navLinks[ i ];
			
			// get the ID each element links to
			var idVal = $(aElem).attr("href");
			subnavIdList.push( idVal );
		}
	}
	
	// clear the color of each element
	for( var i = 0; i < subnavIdList.length; i++ ) {
		
		var navAnchor = $(subnavIdList[i]);
		
		var subnavAnchorElem = $("#subnav-" + navAnchor[0].id);
		subnavAnchorElem.removeClass( "subnavbar-anchor-active" );
	}
	
	// get the scroll position of the window
	var scrollOffset = $(window).scrollTop();
	
	var nearestDist = 9999;
	var nearestElem = null;
		
	for( var i = 0; i < subnavIdList.length; i++ ) {
		var navAnchor = $(subnavIdList[i]);
		
		// get the delta from the scroll position to each element
		var deltaPos = Math.abs( scrollOffset - navAnchor.offset().top );
		if( deltaPos < nearestDist )
		{
			nearestDist = deltaPos;
			nearestElem = navAnchor;
		}
	}
	
	// now color the link associated with this nav anchor
	var subnavAnchorElem = $("#subnav-" + nearestElem[0].id);
	subnavAnchorElem.addClass( "subnavbar-anchor-active");
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
