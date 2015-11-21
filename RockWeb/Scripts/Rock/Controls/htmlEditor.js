(function ($) {
    'use strict';
    window.Rock = window.Rock || {};

    Rock.htmlEditor = (function () {
        var exports = {
            // full list of available items is at http://ckeditor.com/comment/133657#comment-133657

            // the toolbar items to include in the HtmlEditor when Toolbar is set to Light 
            toolbar_RockCustomConfigLight:
	            [
                    ['Source'],
                    ['Bold', 'Italic', 'Underline', 'Strike', 'NumberedList', 'BulletedList', 'Link', 'PasteFromWord', '-', 'Undo', 'Redo', '-', 'RemoveFormat'],
                    ['Format'],
                    ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'],
                    ['rockmergefield', '-', 'rockimagebrowser', 'rockdocumentbrowser']
	            ],

            // the toolbar items to include in the HtmlEditor when Toolbar is set to Full 
            toolbar_RockCustomConfigFull:
                [
                    ['Source'],
                    ['Cut', 'Copy', 'Paste', 'PasteText', 'PasteFromWord', '-', 'Undo', 'Redo'],
                    ['Find', 'Replace', '-', 'Scayt'],
                    ['Link', 'Unlink', 'Anchor'],
                    ['Styles', 'Format'],
                    '/',
                    ['Bold', 'Italic', 'Underline', 'Strike', 'Subscript', 'Superscript', '-', 'RemoveFormat'],
                    ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', '-', 'Blockquote', 'CreateDiv', '-'],
                    ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'],
                    ['-', 'Table'],
                    ['rockmergefield', '-', 'rockimagebrowser', 'rockdocumentbrowser']
                ]
        }

        return exports;

    }());
}(jQuery));