var numberOfQuestionsAnswered = 0,
	isScored = false;

$(document).ready(function () {
    "use strict";
    // $('[id$="btnScoreTest"]').attr('disabled', 'disabled');
    // Checks to see if the test has been scored
    // the isScored variable is passed in from the code-behind file
    if (isScored) {
        $('[id$="navTabs"] a:last').tab('show'); // Select last tab
    }

    ///<summary>
    /// Unchecks the corresponding item from the other rbl
    ///<summary>
    $('#questions input[type=radio]').change(function () {
        var selectedIdx = this.id.charAt(this.id.length - 1);
        var $parentRow = $(this).closest(".row");        
        var moreOrLess = (this.name.substr(length - 8, 7) == "rblMore") ? "_rblLess_" : "_rblMore_";
        var selector = 'input[type=radio][id$="' + moreOrLess + selectedIdx + '"]';

        // uncheck the corresponding inverse variable
        var $correspondingItem = $parentRow.find(selector);
        if ($correspondingItem) {
            $correspondingItem.removeAttr('checked');
        }
    });
});
