/*
Copyright (c) 2003-2012, CKSource - Frederico Knabben. All rights reserved.
For licensing, see LICENSE.html or http://ckeditor.com/license
*/

CKEDITOR.editorConfig = function( config )
{
	// Custom config that removes some of the buggy plugins
	config.toolbar = 'RockCustomConfigLight';
 
	config.toolbar_RockCustomConfigLight =
	[
        { name: 'document', items: ['Source'] },
        { name: 'basicstyles', items: ['Bold', 'Italic', 'Underline', 'Strike', 'NumberedList', 'BulletedList', 'Link', 'Image', 'PasteFromWord', '-', 'RemoveFormat'] },
        { name: 'editing', items: ['Format'] },
	];

	config.toolbar_RockCustomConfigFull =
	[
        { name: 'document', items: ['Source'] },
        { name: 'clipboard', items: ['Cut', 'Copy', 'Paste', 'PasteText', 'PasteFromWord', '-', 'Undo', 'Redo'] },
        { name: 'editing', items: ['Find', 'Replace', '-', 'Scayt'] },
        { name: 'links', items: ['Link', 'Unlink', 'Anchor'] },
        { name: 'styles', items: ['Styles', 'Format'] },
                '/',
        { name: 'basicstyles', items: ['Bold', 'Italic', 'Underline', 'Strike', 'Subscript', 'Superscript', '-', 'RemoveFormat'] },
        {
            name: 'paragraph', items: ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', '-', 'Blockquote', 'CreateDiv', '-',
              'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock', '-', 'Image', 'Table']
        },
	];
};
