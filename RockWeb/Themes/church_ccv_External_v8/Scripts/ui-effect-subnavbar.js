//<script type="text/javascript" src="/Themes/church_ccv_External_v8/Scripts/ui-effect-subnavbar.js"></script>

//var subNavbarCanSnap = true;
var pageHasSubNav = false; // Used to know if any of this snapping code should run on the page or not.
var firstSectionTopMargin = ""; // Used for maintaining the first section's margin as the Subnav bar Snaps and Unsnaps

var initialSetupDone = false;
function handleInitialSetup( ) {
    if( initialSetupDone == false ) {
        initialSetupDone = true;

        // does this page have subNav?
        var subNavElement = $("#subnavbar-bg");
        if( subNavElement.length > 0 && subNavElement.css("display") != "none" ) {
            pageHasSubNav = true;
        
            // We store the initial margin of the first section so that we can adjust it as the
            // subNavbar snaps.
            // NOTE - If we ever have variable heights defined in common.css, this would need to be updated
            // to handle window width changes, which could result in another media query that has a different margin defined.
            var firstSection = getFirstSectionElement( );
            firstSectionTopMargin = parseInt( firstSection.css("margin-top"), 10 );

            // setup a callback for when the media query triggers
            /*const mq = window.matchMedia( "(min-width: 1101px)" );
            mq.addListener( subNavbarQueryTriggered );
            subNavbarQueryTriggered( mq );*/
        }
    }
}

// We'll wait for the window to be 100% loaded before we configure subNav.
$( window ).on( "load", function() { handleInitialSetup(); if ( pageHasSubNav ) { updateSubNavbarSnap( ); } })

/*function subNavbarQueryTriggered( mediaQuery ) {
	
    // are we at desktop size?
    if( mediaQuery.matches ) {

        toggleSubNavbarCanSnap( true );
	}
	else {
        toggleSubNavbarCanSnap( false );
	}
}*/

/*function toggleSubNavbarCanSnap( enabled ) {

    subNavbarCanSnap = enabled;
        
    if ( subNavbarCanSnap == false ) {
        // if we're turning the navbar off, we need to cleanup
        // any adjustments made while it was on.
        resetSubNavbarPos( );
    }
}*/

$(window).scroll( function() {
   
    if ( pageHasSubNav ) {
        updateSubNavbarSnap( );
    }
});

$(window).resize(function() {
   
    if ( pageHasSubNav ) {
        updateSubNavbarSnap( );
    }
});

// "Snaps" the sub navbar to underneath the primary navbar when scrolling the page
function updateSubNavbarSnap( ) {
   
    //if( subNavbarCanSnap ) {
        // get the origin position of the subNavbar. This lets us know if we've scrolled beyond it or not,
        // and thus whether to snap it to the top or not.
        // Its origin is always below the "main-feature" section, so just get the bottom of that section.
        var mainFeature = $(".main-feature");
        var subNavbarOriginPos = mainFeature.outerHeight();
        
        var firstSection = getFirstSectionElement( );
        
        // if the top navbar has clipped the sub navbar, we've gone far enough to snap
        var topNavbar = $("#masthead");	
        if( $(window).scrollTop() + topNavbar.outerHeight() >= subNavbarOriginPos ) {

            var subNavbar = $("#subnavbar-bg");

            // snap the sub navbar to underneath the top navbar
            subNavbar.css("top", topNavbar.outerHeight() + "px" );
            subNavbar.css("position", "fixed" );
            subNavbar.css("width", "100%" );

            // is there a tertiary navbar we need to snap as well? 
            var terOuterHeight = 0; //store its outer height as a variable assumed to be 0, that way we can do the math below with no conditional
            var terNavbar = $("#ternavbar-bg");
            if( terNavbar.css("display") != "none" ) {

                var topPos = topNavbar.outerHeight() + subNavbar.outerHeight() + "px";

                terNavbar.css("top", topPos );
                terNavbar.css("position", "fixed" );
                terNavbar.css("width", "100%" );

                terOuterHeight = terNavbar.outerHeight();
            }
            
            // add the sub navbars height to the page so the page doesn't jump
            firstSection.css("margin-top", firstSectionTopMargin + subNavbar.outerHeight() + terOuterHeight + "px" );
        }
        else {
            resetSubNavbarPos( );
        }
    //}
}

function resetSubNavbarPos( ) {
    
    // see if there's a tertiary navbar we need to unsnap
    var terNavbar = $("#ternavbar-bg");
    if( terNavbar.css("display") != "none" ) {
        terNavbar.css("top", "" );
        terNavbar.css("position", "" );
        terNavbar.css("width", "" );
    }

    // clear all styling to effectively put things like they were when the page loaded.
    var subNavbar = $("#subnavbar-bg");
    subNavbar.css("top", "" );
    subNavbar.css("position", "" );
    subNavbar.css("width", "" );
    
    var firstSection = getFirstSectionElement( );
    firstSection.css("margin-top", firstSectionTopMargin + "px" );
}

function getFirstSectionElement( ) {
    
    // get the first section element, which is either "section-a-bg", or for single section pages, "single-section-bg"
    var firstSection = $("#section-a-bg");
    if( firstSection.length == 0 ) {
        firstSection = $("#single-section-bg");
    }

    return firstSection;
}
