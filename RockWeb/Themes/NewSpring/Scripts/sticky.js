var sticky = $("#sticky");
var stickyOffset = sticky.offset().top;
var elementHeight = sticky.height();
var parentHeight = sticky.parent().height();

var myEfficientFn = function() {

	console.log('trigger');
	
	var navigationHeight = $('#navigation').height();
	var scrollDistance = $(window).scrollTop();

	if (scrollDistance > (stickyOffset + parentHeight - elementHeight * 2 - 5)) {
		sticky.removeClass("position-fixed").addClass("position-absolute");
		sticky[0].style.top = (parentHeight - elementHeight) + 'px';
	} else if (scrollDistance > (stickyOffset - navigationHeight - 25)) {
		sticky.removeClass("position-absolute").addClass("position-fixed");
		sticky[0].style.top = "100px";
	} else {
		sticky.removeClass("position-fixed").addClass("position-absolute");
		sticky[0].style.top = "0px";
	}
};

window.addEventListener('scroll', myEfficientFn);