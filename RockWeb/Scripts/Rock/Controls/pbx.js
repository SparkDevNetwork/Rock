(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.pbx = (function () {

        var exports = {
            // implements the click to call feature
            originate: function (sourcePersonGuid, destinationNumber, callerId, destinationPersonName, destinationNumberFormatted) {
                var originateUrl = Rock.settings.get('baseUrl') + 'api/Pbx/Originate?sourcePersonGuid=' + sourcePersonGuid + '&destinationPhone=' + destinationNumber + '&callerId=' + callerId;

                console.log('From Rock.controls.originateCall: ' + originateUrl);
                $.get(originateUrl, function (r) {
                    if (!r.Success) {
                        Rock.dialogs.alert("An error occurred while attempting to place call.  " + r.Message);
                    }
                });
            }
        };

        return exports;
    }());
}(jQuery));