var numberOfQuestionsAnswered = 0,
	isScored = false;

$(document).ready(function () {
    "use strict";
    $('[id$="btnScoreTest"]').attr('disabled', 'disabled');
    // Checks to see if the test has been scored
    // the isScored variable is passed in from the code-behind file
    if (isScored) {
        $('[id$="navTabs"] a:last').tab('show'); // Select last tab
    }
});

function moveOn(curQ) {
    "use strict";
    var qmTxt = curQ + "m",
		qlTxt = curQ + "l",
		mResponse = "",
		lResponse = "",
		qm = $('input[name$="' + qmTxt + '"]:radio'),
		ql = $('input[name$="' + qlTxt + '"]:radio'),
		mDone = 0,
		lDone = 0,
		x = 0,
		hdr = $('#page-header');

    for (x = 0; x < qm.length; x++) {
        if (qm[x].checked) {
            mDone = 1;
            mResponse = qm[x].id.substr(-4, 3);
            break;
        }
    }

    for (x = 0; x < ql.length; x++) {
        if (ql[x].checked) {
            lDone = 1;
            lResponse = ql[x].id.substr(-4, 3);
            break;
        }
    }

    if (mDone && lDone && (mResponse !== lResponse)) {
        if (curQ < 30) {
            curQ++;
            $(window).scrollTop($('td[id$="q' + pad2(curQ) + '"]').offset().top - hdr.height() - 20);
        }
        numberOfQuestionsAnswered++;
        if (numberOfQuestionsAnswered >= 30) {
            $('[id$="btnScoreTest"]').removeAttr('disabled');
        }
    }
    return true;
}

function pad2(number) {
    "use strict";
    return (number < 10 ? '0' : '') + number;
}