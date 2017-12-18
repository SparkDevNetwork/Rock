//<script type="text/javascript" src="/Themes/church_ccv_External_v8/Scripts/ui-effect-subnavbar.js"></script>

//This will be true / false depending on the page width.
var subNavbarEnabled = true; 

// adds on "onload" hook to the window.onload function chain
var oldonload = window.onload;
window.onload = (typeof window.onload != 'function') ?
  handleOnLoad : function() { 
   oldonload(); handleOnLoad();
};

function handleOnLoad() {   
   // setup a callback for when the media query triggers
   const mq = window.matchMedia( "(min-width: 800px)" );
   mq.addListener( subNavbarQueryTriggered );
   
   subNavbarQueryTriggered( mq );
}

function subNavbarQueryTriggered( mediaQuery ) {
	
   // we want to know if the browser is within our "desktop size" media query.
   // if so, enable the navbar fade effect. If not, we'll turn it off
	if( mediaQuery.matches ) {
		toggleSubNavbar( true );
	}
	else {
		toggleSubNavbar( false );
	}
}

function toggleSubNavbar( enabled ) {
   subNavbarEnabled = enabled;
   
   // if we're turning the navbar off, we need to cleanup
   // any adjustments made while it was on.
   if( subNavbarEnabled == false ) {
      resetSubNavbarPos( );
   }
}

$(document).scroll( function() {
   
   updateSubNavbarForScroll( );
});

$(window).resize(function() {
   
   updateSubNavbarForScroll();
});

function updateSubNavbarForScroll( )
{
	if( subNavbarEnabled ) {
      
      updateSubNavbarSnap( );
	
      updateSubNavbarLinks( );
	}
}

// "Snaps" the sub navbar to underneath the primary navbar when scrolling the page
function updateSubNavbarSnap( ) {
   
	// get the origin position of the subNavbar. This lets us know if we've scrolled beyond it or not,
	// and thus whether to snap it to the top or not.
	// Its origin is always below the "main-feature" section, so just get the bottom of that section.
	var mainFeature = document.getElementsByClassName("main-feature");
	var subNavbarOriginPos = mainFeature[0].offsetHeight;
	
	// get the "section A" element, which is where the body content starts
	var sectionA = document.getElementById("section-a-bg");
	
	// seed the navbar starting position on first update (since we don't have OnLoad() access)
	var subNavbar = document.getElementById("subnavbar-bg");
	
	
	// if the top navbar has clipped the sub navbar, we've gone far enough to snap
	var topNavbar = document.getElementById("masthead");	
	if( window.pageYOffset + topNavbar.offsetHeight >= subNavbarOriginPos ) {
		// snap the sub navbar to underneath the top navbar
		subNavbar.style.top = topNavbar.offsetHeight + "px";
		subNavbar.style.position = "fixed";
		subNavbar.style.width = "100%";
		
		// add the sub navbars height to the page so the page doesn't jump
		sectionA.style.paddingTop = subNavbar.offsetHeight + "px";
	}
	else {
		resetSubNavbarPos( );
	}
}

function resetSubNavbarPos( ) {
   
   // clear all styling to effectively put things like they were when the page loaded.
   var subNavbar = document.getElementById("subnavbar-bg");
   subNavbar.style.top = "";
   subNavbar.style.position = "";
   subNavbar.style.width = "";
   
   var sectionA = document.getElementById("section-a-bg");
   sectionA.style.paddingTop = "";	
}

// "Highlights" the appropriate sub navbar link based on where the user has scrolled in the page
// To make this work:
// Ensure the items in the subnavbar have the following ID naming convention:
// subnav-ID-NAME (ex: subnav-the-details)
// The actual anchor should have the same ID, but without "subnav" prefixed.
var subnavIdList = [];
function updateSubNavbarLinks( ) {
	// seed the list array if we haven't yet (since we don't have OnLoad() access)
	if( subnavIdList.length == 0 ) {
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
