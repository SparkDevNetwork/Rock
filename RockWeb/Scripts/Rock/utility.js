(function () {
    'use strict';
    window.Rock = window.Rock || {};

    Rock.utility = (function () {
        var _utility = {},
            exports = {

                setContext: function (restController, entityId) {
                    // Get the current block instance object
                    $.ajax({
                        type: 'PUT',
                        url: Rock.settings.get('baseUrl') + 'api/' + restController + '/SetContext/' + entityId,
                        success: function (getData, status, xhr) {
                        },
                        error: function (xhr, status, error) {
                            alert(status + ' [' + error + ']: ' + xhr.responseText);
                        }
                    });
                },

                uuidv4: function () {
                    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                        var r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
                        return v.toString(16);
                    });
                },
            };
 
        return exports;
    }());
}());
