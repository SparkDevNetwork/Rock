$(document).ready(function () {
	// Checks to see if the test has been scored
	// the isScored variable is passed in from the code-behind file
	if (!isScored) {
		$('#tabs li a:not(:first)').addClass('inactive');
		$('.tabContent:not(:first)').hide();
		$('.tabContent:first').show();
	}
	else {
		$('#tabs li a:not(:last)').addClass('inactive');
		$('.tabContent:not(:last)').hide();
		$('.tabContent:last').show();
	}

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