
function checkAndMoveToNext($pRow, partialSelector) {
    var $checkedItem = $pRow.find('input[type=radio][id*="' + partialSelector + '"]:checked');
    //If both RBLs have a selected item
    if ($checkedItem.length > 0) {
        var $nextPanel = $pRow.closest(".panel-default").next();
        //alert($nextPanel.html());
        if ($nextPanel.length > 0) {
            $("body").animate({ scrollTop: $nextPanel.offset().top - 20 }, 250);
        }
    }
}

function isComplete() {
    var $completedQuestions = $('.js-disc-questions input[type=radio]:checked');
    if ($completedQuestions.length < 60) {
        $('[id$="divError"]').fadeIn();
        return false;
    }
    else {
        return true;
    }
}

// fade-in effect for the panel
function FadePanelIn() {
    $("[id$='upnlContent']").rockFadeIn();
}

///<summary>
/// Adjust the given color by a certain amount
///<summary>
function AdjustColor(col, amt) {

    var hash = false;

    if (col[0] == "#") {
        col = col.slice(1);
        hash = true;
    }

    var num = parseInt(col, 16);

    var r = (num >> 16) + amt;
    if (r > 255) r = 255;
    else if (r < 0) r = 0;

    var g = (num & 0x0000FF) + amt;
    if (g > 255) g = 255;
    else if (g < 0) g = 0;

    var b = ((num >> 8) & 0x00FF) + amt;
    if (b > 255) b = 255;
    else if (b < 0) b = 0;

    return (hash ? "#" : "") + (g | (b << 8) | (r << 16)).toString(16);
}

///<summary>
/// Converts an rbg color value to hex.
///<summary>
function rgb2hex(rgb) {
    rgb = rgb.match(/^rgb\((\d+),\s*(\d+),\s*(\d+)\)$/);
    return "#" +
     ("0" + parseInt(rgb[1], 10).toString(16)).slice(-2) +
     ("0" + parseInt(rgb[2], 10).toString(16)).slice(-2) +
     ("0" + parseInt(rgb[3], 10).toString(16)).slice(-2);
}

///<summary>
/// Unchecks the corresponding item from the other rbl
/// and moves to next question.
///<summary>
function initQuestionValidation() {

    $('.js-disc-questions input[type=radio]').change(function () {
        var selectedIdx = this.id.charAt(this.id.length - 1);
        var $parentRow = $(this).closest(".row");
        var moreOrLess = (this.name.substr(length - 8, 7) == "rblMore") ? "_rblLess_" : "_rblMore_";
        var selector = 'input[type=radio][id$="' + moreOrLess + selectedIdx + '"]';

        // uncheck the corresponding inverse variable
        var $correspondingItem = $parentRow.find(selector);
        if ($correspondingItem) {
            $correspondingItem.removeAttr('checked');
        }

        $('[id$="divError"]').fadeOut();

        checkAndMoveToNext($parentRow, moreOrLess);
    });
}

///<summary>
/// Darkens the color of the discbar-primary color to 20% of the label-default
///<summary>
function DarkenPrimaryDiscScore() {
    var bgcolor = $(".label-default").css("background-color");
    if ( bgcolor != null ) {
        var color = rgb2hex( bgcolor );
        var newColor = AdjustColor(color, -30);
        $(".discbar-primary").css("background-color", newColor);
    }
}

function pageLoad(sender, args) { 
    initQuestionValidation();
    DarkenPrimaryDiscScore();
}

$(document).ready(function () {
    FadePanelIn();
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(FadePanelIn);
});
