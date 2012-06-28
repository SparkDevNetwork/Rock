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

function moveOn(curQ) {
	var qmTxt = curQ + "m";
	var qlTxt = curQ + "l";
	var qm = $('input[name$="' + qmTxt + '"]:radio');
	var ql = $('input[name$="' + qlTxt + '"]:radio');

	var mDone = 0;
	var lDone = 0;

	for (var x = 0; x < qm.length; x++) {
		if (qm[x].checked) {
			mDone = 1;
			break;
		}
	}

	for (var x = 0; x < ql.length; x++) {
		if (ql[x].checked) {
			lDone = 1;
			break;
		}
	}

	if (mDone && lDone) {
		if (curQ != 30) {
			curQ++;
			var hdr = $('#page-header');
			$(window).scrollTop($('td[id$="q' + pad2(curQ) + '"]').offset().top - hdr.height() - 20);
		}
	}
	return true;
};

function pad2(number) {
	return (number < 10 ? '0' : '') + number;
}