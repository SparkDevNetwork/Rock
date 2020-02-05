/*

NAVIGATION HIDE/SHOW

*/

// Hide Header on on scroll down
// var didScroll;
// var lastScrollTop = 0;
// var delta = 5;
// var navbarHeight = $('#sidebar').outerHeight();

// $('#content').scroll(function(event){
//     didScroll = true;
// });

// setInterval(function() {
//     if (didScroll) {
//         hasScrolled();
//         didScroll = false;
//     }
// }, 150);

// function hasScrolled() {

//     var st = $('#content').scrollTop();
    
//     // Make sure they scroll more than delta
//     if(Math.abs(lastScrollTop - st) <= delta)
//         return;
    
//     // If they scrolled down and are past the navbar, add class .nav-up.
//     // This is necessary so you never see what is "behind" the navbar.
//         // Scroll Down
//         $('#sidebar').removeClass('nav-down').addClass('nav-up');
//         // Scroll Up
//         if(st < lastScrollTop) {
//             $('#sidebar').removeClass('nav-up').addClass('nav-down');
//         }
    
//     lastScrollTop = st;
// }

/* ========================================== 
scrollTop() >= 300
Should be equal the the height of the header
========================================== */
var subnav = $('#secondary-nav');

if(subnav.length >= 1) {
$(window).scroll(function(){
    if ($(window).scrollTop() >= 63) {
        $('#secondary-nav').addClass('fixed-header');
        $('body').css('padding-top', 60);
    }
    else {
        $('#secondary-nav').removeClass('fixed-header');
        $('body').css('padding-top', 0);
    }
});
}