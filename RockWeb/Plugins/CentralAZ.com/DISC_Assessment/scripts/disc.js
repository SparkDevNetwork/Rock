$(document).ready(function () {
	$('#tabs li a:not(:first)').addClass('inactive');
	$('.tabContent:not(:first)').hide();

	$('#tabs li a').click(function () {
		var t = $(this).attr('href');
		if ($(this).hasClass('inactive')) {
			$('#tabs li a').addClass('inactive');
			$(this).removeClass('inactive');
			$('.tabContent').hide();
			$(t).fadeIn('slow');
		}
		return false;
	})
});