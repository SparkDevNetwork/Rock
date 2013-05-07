(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.grid = (function () {
        return {
            confirmDelete: function (e, nameText) {
                e.preventDefault();
                bootbox.dialog('Are you sure you want to delete this ' + nameText + '?',
                    [
                        {
                            label: 'OK', 
                            'class': 'btn-primary', 
                            callback: function () {
                                var postbackJs = e.target.href ? e.target.href : e.target.parentElement.href;

                                // need to do unescape because firefox might put %20 instead of spaces
                                postbackJs = unescape(postbackJs);
                                
                                // Careful!
                                eval(postbackJs);
                            }
                        },
                        {
                            label: 'Cancel',
                            'class': 'btn-secondary'
                        }
                    ]);
            }
        };
    }());
}(jQuery));