(function ($) {
    'use strict';
    window.Rock = window.Rock || {};

    Rock.htmlEditor = (function () {
        var exports = {
            // full list of available items is at http://summernote.org/deep-dive/#custom-toolbar-popover

            // the toolbar items to include in the HtmlEditor when Toolbar is set to Light 
            toolbar_RockCustomConfigLight:
                [
                    // [groupName, [list of button]]
                    ['source_group', ['rockcodeeditor']],
                    ['style_group1', ['bold', 'italic', 'strikethrough', 'link', 'color', 'style', 'ol', 'ul']],
                    ['style_group3', ['clear']],
                    ['para', ['paragraph']],
                    ['plugins1', ['rockmergefield']],
                    ['plugins2', ['rockimagebrowser', 'rockfilebrowser', 'rockassetmanager']],
                    ['plugins3', ['rockpastetext', 'rockpastefromword']],
                    ['style_group2', ['undo', 'redo']]
                ],

            // the toolbar items to include in the HtmlEditor when Toolbar is set to Full 
            toolbar_RockCustomConfigFull:
                [
                    // [groupName, [list of button]]
                    ['source_group', ['rockcodeeditor']],
                    ['style_group1', ['bold', 'italic', 'underline', 'strikethrough', 'ol', 'ul', 'link']],
                    ['style_group2', ['undo', 'redo']],
                    ['style_group3', ['clear']],
                    ['style_group4', ['style', 'color']],
                    ['full_toolbar_only', ['fontname', 'fontsize', 'superscript', 'subscript', 'table', 'hr']],
                    ['para', ['paragraph']],
                    ['plugins1', ['rockmergefield']],
                    ['plugins2', ['rockimagebrowser', 'rockfilebrowser', 'rockassetmanager']],
                    ['plugins3', ['rockpastetext', 'rockpastefromword']],
                    ['help_group1', ['help']]
                ]
        };

        return exports;

    }());
}(jQuery));
