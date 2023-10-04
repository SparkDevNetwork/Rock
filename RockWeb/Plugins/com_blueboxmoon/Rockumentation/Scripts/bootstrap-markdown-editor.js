define("com/blueboxmoon/rockumentation-markdown-highlight-rules", ['require', 'exports', 'module', 'ace/lib/oop', 'ace/mode/markdown_highlight_rules'], function (require, exports, module) {
    "use strict";

    var oop = require("ace/lib/oop");
    var MarkdownHighlightRules = require("ace/mode/markdown_highlight_rules").MarkdownHighlightRules;

    var RockumentationMarkdownHighlightRules = function () {
        MarkdownHighlightRules.call(this);

        this.$rules["start"].unshift({
            token: "string.strikethrough",
            regex: "([~]{2}(?=\\S))(.*?\\S[~]*)(\\1)"
        });
    };

    oop.inherits(RockumentationMarkdownHighlightRules, MarkdownHighlightRules);
    exports.MarkdownHighlightRules = RockumentationMarkdownHighlightRules;
});

(function ($) {
    "use strict";

    function uploadFiles (url, files, editor, snippetManager, loading, uploadHelper) {
        if (! files.length) {
            return;
        }
        if (files.length > 1) {
          alert('Please upload a single file at a time.');
          return;
        }

        loading.show();

        var data = new FormData(),
            i = 0;

        for (i = 0; i < files.length; i++) {
            data.append('file' + i, files[i]);
        }

        $.ajax({
            url: url,
            type: 'POST',
            contentType: false,
            data: data,
            processData: false,
            cache: false,
            dataType: 'json'
        }).done (function (uploadedFile) {
            if (uploadedFile.Id && uploadedFile.FileName) {
                snippetManager.insertSnippet(editor, uploadHelper(uploadedFile));
            }
        }).always(function () {
            loading.hide();
        });
    }

    function adjustFullscreenLayout (mdPanel) {
        var hWindow = $(window).height(),
            tEditor = mdPanel.offset().top,
            hEditor;

        if(hWindow > tEditor) {
            hEditor = hWindow - tEditor;
            mdPanel.css('height', hEditor + 'px');
        }
    }

    function setShortcuts (editor, snippetManager) {
        editor.commands.addCommand({
            name: 'bold',
            bindKey: {win: 'Ctrl-B',  mac: 'Command-B'},
            exec: function (editor) {
                var selectedText = editor.session.getTextRange(editor.getSelectionRange());

                if (selectedText === '') {
                    snippetManager.insertSnippet(editor, '**${1:text}**');
                } else {
                    snippetManager.insertSnippet(editor, '**' + selectedText + '**');
                }
            },
            readOnly: false
        });

        editor.commands.addCommand({
            name: 'italic',
            bindKey: {win: 'Ctrl-I',  mac: 'Command-I'},
            exec: function (editor) {
                var selectedText = editor.session.getTextRange(editor.getSelectionRange());

                if (selectedText === '') {
                    snippetManager.insertSnippet(editor, '*${1:text}*');
                } else {
                    snippetManager.insertSnippet(editor, '*' + selectedText + '*');
                }
            },
            readOnly: false
        });

        editor.commands.addCommand({
            name: 'strikethrough',
            exec: function (editor) {
                var selectedText = editor.session.getTextRange(editor.getSelectionRange());

                if (selectedText === '') {
                    snippetManager.insertSnippet(editor, '~~${1:text}~~');
                } else {
                    snippetManager.insertSnippet(editor, '~~' + selectedText + '~~');
                }
            },
            readOnly: false
        });

        editor.commands.addCommand({
            name: 'link',
            bindKey: {win: 'Ctrl-K',  mac: 'Command-K'},
            exec: function (editor) {
                var selectedText = editor.session.getTextRange(editor.getSelectionRange());

                if (selectedText === '') {
                    snippetManager.insertSnippet(editor, '[${1:text}](http://$2)');
                } else {
                    snippetManager.insertSnippet(editor, '[' + selectedText + '](http://$1)');
                }
            },
            readOnly: false
        });
    }

    function insertBeforeText (editor, string) {

        if (editor.getCursorPosition().column === 0) {
            editor.navigateLineStart();
            editor.insert(string + ' ');
        } else {
            editor.navigateLineStart();
            editor.insert(string + ' ');
            editor.navigateLineEnd();
        }
    }

    function editorHtml (content, options) {
        var html = '';

        html += '<div class="md-loading"><span class="md-icon-container"><span class="md-icon"><i class="fa fa-spinner fa-pulse fa-3x fa-fw"></i></span></span></div>';
        html += '<div class="md-toolbar">';
            html += '<div class="btn-toolbar">';

                html += '<div class="btn-group">';
                    html += '<button type="button" data-mdtooltip="tooltip" title="' + options.label.btnHeader1 + '" class="md-btn btn btn-sm btn-default" data-btn="h1">H1</button>';
                    html += '<button type="button" data-mdtooltip="tooltip" title="' + options.label.btnHeader2 + '" class="md-btn btn btn-sm btn-default" data-btn="h2">H2</button>';
                    html += '<button type="button" data-mdtooltip="tooltip" title="' + options.label.btnHeader3 + '" class="md-btn btn btn-sm btn-default" data-btn="h3">H3</button>';
                html += '</div>'; // .btn-group

                html += '<div class="btn-group">';
                    html += '<button type="button" data-mdtooltip="tooltip" title="' + options.label.btnBold + '" class="md-btn btn btn-sm btn-default" data-btn="bold"><span class="fa fa-bold"></span></button>';
                    html += '<button type="button" data-mdtooltip="tooltip" title="' + options.label.btnItalic + '" class="md-btn btn btn-sm btn-default" data-btn="italic"><span class="fa fa-italic"></span></button>';
                    html += '<button type="button" data-mdtooltip="tooltip" title="' + options.label.btnStrikethrough + '" class="md-btn btn btn-sm btn-default" data-btn="strikethrough"><span class="fa fa-strikethrough"></span></button>';
                html += '</div>'; // .btn-group

                html += '<div class="btn-group">';
                    html += '<button type="button" data-mdtooltip="tooltip" title="' + options.label.btnList + '" class="md-btn btn btn-sm btn-default" data-btn="ul"><span class="fa fa-list-ul"></span></button>';
                    html += '<button type="button" data-mdtooltip="tooltip" title="' + options.label.btnOrderedList + '" class="md-btn btn btn-sm btn-default" data-btn="ol"><span class="fa fa-list-ol"></span></button>';
                html += '</div>'; // .btn-group

                html += '<div class="btn-group">';
                    html += '<button type="button" data-mdtooltip="tooltip" title="' + options.label.btnLink + '" class="md-btn btn btn-sm btn-default" data-btn="link"><span class="fa fa-link"></span></button>';
                    html += '<button type="button" data-mdtooltip="tooltip" title="' + options.label.btnImage + '" class="md-btn btn btn-sm btn-default" data-btn="image"><span class="fa fa-image"></span></button>';
                    if (options.imageUpload === true) {
                        html += '<div data-mdtooltip="tooltip" title="' + options.label.btnUpload + '" class="btn btn-sm btn-default md-btn-file"><span class="fa fa-upload"></span><input class="md-input-upload" type="file"></div>';
                    }
                html += '</div>'; // .btn-group

                if (options.fullscreen === true) {
                    html += '<div class="btn-group pull-right">';
                        html += '<button type="button" class="md-btn btn btn-sm btn-default" data-btn="fullscreen"><span class="fa fa-fullscreen"></span> ' + options.label.btnFullscreen + '</button>';
                    html += '</div>'; // .btn-group
                }

                if (options.preview === true) {
                    html += '<div class="btn-group pull-right">';
                        html += '<button type="button" class="md-btn btn btn-sm btn-default btn-edit active" data-btn="edit"><span class="fa fa-pencil"></span> ' + options.label.btnEdit + '</button>';
                        html += '<button type="button" class="md-btn btn btn-sm btn-default btn-preview" data-btn="preview"><span class="fa fa-eye"></span> ' + options.label.btnPreview + '</button>';
                    html += '</div>'; // .btn-group
                }

            html += '</div>'; // .btn-toolbar
        html += '</div>'; // .md-toolbar

        html += '<div class="md-editor">' + $('<div>').text(content).html() + '</div>';
        html += '<div class="md-preview" style="display:none"></div>';

        return html;
    }

    var methods = {
        init: function (options) {

            var defaults = $.extend(true, {}, $.fn.markdownEditor.defaults, options),
                plugin = this,
                container,
                preview = false,
                fullscreen = false;

            // Hide the textarea
            plugin.addClass('md-textarea-hidden');

            // Create the container div after textarea
            container = $('<div/>');
            plugin.after(container);

            // Replace the content of the div with our html
            container.addClass('md-container').html(editorHtml(plugin.val(), defaults));

            // If the Bootstrap tooltip library is loaded, initialize the tooltips of the toolbar
            if (typeof $().tooltip === 'function') {
                container.find('[data-mdtooltip="tooltip"]').tooltip({
                    container: 'body'
                });
            }

            var mdEditor = container.find('.md-editor'),
                mdPreview = container.find('.md-preview'),
                mdLoading = container.find('.md-loading');

            container.css({
                width: defaults.width
            });

            mdEditor.css({
                height: defaults.height,
                fontSize: defaults.fontSize
            });

            mdPreview.css({
                height: defaults.height
            });

            // Initialize Ace
            var editor = ace.edit(mdEditor[0]),
                snippetManager;
            $(this).data('editor', editor);

            editor.setTheme('ace/theme/' + defaults.theme);

            // Can't find a way to make ace load the markdown module first so
            // we can modify it. So trick it. Have it load the markdown mode
            // and then create a new markdown mode with our custom highlighter.
            var skipModeChange = false;
            editor.getSession().on('changeMode', function () {
                var originalMode = editor.getSession().getMode();
                if (originalMode.$id === 'ace/mode/markdown' && skipModeChange === false) {
                    skipModeChange = true;

                    var modeType = ace.require('ace/mode/markdown').Mode;
                    var mode = new modeType();
                    mode.HighlightRules = ace.require('com/blueboxmoon/rockumentation-markdown-highlight-rules').MarkdownHighlightRules;
                    editor.getSession().setMode(mode);
                }
            });
            editor.getSession().setMode('ace/mode/markdown');

            editor.getSession().setUseWrapMode(true);
            editor.getSession().setUseSoftTabs(defaults.softTabs);

            // Sync ace with the textarea
            editor.getSession().on('change', function() {
                plugin.val(editor.getSession().getValue());
            });

            editor.setHighlightActiveLine(false);
            editor.setShowPrintMargin(false);
            editor.renderer.setShowGutter(false);

            ace.config.loadModule('ace/ext/language_tools', function () {
                snippetManager = ace.require('ace/snippets').snippetManager;
                setShortcuts(editor, snippetManager);
            });


            // Image drag and drop and upload events
            if (defaults.imageUpload) {

                container.find('.md-input-upload').on('change', function() {
                    var files = $(this).get(0).files;

                    uploadFiles(defaults.uploadPath, files, editor, snippetManager, mdLoading, defaults.uploadHelper);
                });

                container.on('dragenter', function (e)
                {
                    e.stopPropagation();
                    e.preventDefault();
                    container.addClass('md-drag-active');
                });

                container.on('dragover', function (e)
                {
                    e.stopPropagation();
                    e.preventDefault();
                    container.addClass('md-drag-active');
                });

                container.on('dragleave', function (e)
                {
                    e.stopPropagation();
                    e.preventDefault();
                    container.removeClass('md-drag-active');
                });

                container.on('drop', function (e)
                {
                    e.preventDefault();
                    container.removeClass('md-drag-active');

                    var files = e.originalEvent.dataTransfer.files;

                    uploadFiles(defaults.uploadPath, files, editor, snippetManager, mdLoading, defaults.uploadHelper);
                });
            }

            // Window resize event
            if (defaults.fullscreen === true) {
                $(window).resize(function () {
                    if (fullscreen === true) {
                        if (preview === false) {
                            adjustFullscreenLayout(mdEditor);
                        } else {
                            adjustFullscreenLayout(mdPreview);
                        }
                    }
                });
            }

            // Toolbar events
            container.find('.md-btn').click(function () {
                var btnType = $(this).data('btn'),
                    selectedText = editor.session.getTextRange(editor.getSelectionRange());

                if (btnType === 'h1') {
                    insertBeforeText(editor, '#');

                } else if (btnType === 'h2') {
                    insertBeforeText(editor, '##');

                } else if (btnType === 'h3') {
                    insertBeforeText(editor, '###');

                } else if (btnType === 'ul') {
                    insertBeforeText(editor, '*');

                } else if (btnType === 'ol') {
                    insertBeforeText(editor, '1.');

                } else if (btnType === 'bold') {
                    editor.execCommand('bold');

                } else if (btnType === 'italic') {
                    editor.execCommand('italic');

                } else if (btnType === 'strikethrough') {
                    editor.execCommand('strikethrough');

                } else if (btnType === 'link') {
                    editor.execCommand('link');

                } else if (btnType === 'image') {
                    if (selectedText === '') {
                        snippetManager.insertSnippet(editor, '![${1:text}](http://$2)');
                    } else {
                        snippetManager.insertSnippet(editor, '![' + selectedText + '](http://$1)');
                    }

                } else if (btnType === 'edit') {
                    preview = false;

                    mdPreview.hide();
                    mdEditor.show();
                    container.find('.btn-edit').addClass('active');
                    container.find('.btn-preview').removeClass('active');

                    if (fullscreen === true) {
                        adjustFullscreenLayout(mdEditor);
                    }

                } else if (btnType === 'preview') {
                    preview = true;

                    mdPreview.html('<p style="text-align:center; font-size:16px">' + defaults.label.loading + '...</p>');

                    defaults.onPreview(editor.getSession().getValue(), function (content) {
                        mdPreview.html(content);
                    });

                    mdEditor.hide();
                    mdPreview.show();
                    container.find('.btn-preview').addClass('active');
                    container.find('.btn-edit').removeClass('active');

                    if (fullscreen === true) {
                        adjustFullscreenLayout(mdPreview);
                    }

                } else if (btnType === 'fullscreen') {

                    if (fullscreen === true) {
                        fullscreen = false;

                        $('body, html').removeClass('md-body-fullscreen');
                        container.removeClass('md-fullscreen');

                        mdEditor.css('height', defaults.height);
                        mdPreview.css('height', defaults.height);

                    } else {
                        fullscreen = true;

                        $('body, html').addClass('md-body-fullscreen');
                        container.addClass('md-fullscreen');

                        if (preview === false) {
                            adjustFullscreenLayout(mdEditor);
                        } else {
                            adjustFullscreenLayout(mdPreview);
                        }
                    }

                    editor.resize();
                }

                editor.focus();
            });

            return this;
        },
        content: function () {
            return $(this).data('editor').getSession().getValue();
        },
        setContent: function(str) {
            $(this).data('editor').getSession().setValue(str, 1);
        },
        getEditor: function () {
            return $(this).data('editor');
        }
    };

    $.fn.markdownEditor = function (options) {

        if (methods[options]) {
            return methods[options].apply(this, Array.prototype.slice.call(arguments, 1));

        } else if (typeof options === 'object' || ! options) {
            return methods.init.apply(this, arguments);

        } else {
            $.error('Method ' +  options + ' does not exist on jQuery.markdownEditor');
        }
    };

    $.fn.markdownEditor.defaults = {
        width: '100%',
        height: '400px',
        fontSize: '14px',
        theme: 'tomorrow',
        softTabs: true,
        fullscreen: true,
        imageUpload: false,
        uploadPath: '',
        uploadHelper: function (uploadedFile)
        {
            var imageChar = '';
            if ((/\.(gif|jpg|jpeg|tif|tiff|png)$/i).test(uploadedFile.FileName))
            {
                imageChar = '!';
            }
            return imageChar + '[' + uploadedFile.FileName + '](~/GetFile.ashx?Id=' + uploadedFile.Id + ')';
        },
        preview: false,
        onPreview: function (content, callback) {
            callback(content);
        },
        label: {
            btnHeader1: 'Header 1',
            btnHeader2: 'Header 2',
            btnHeader3: 'Header 3',
            btnBold: 'Bold',
            btnItalic: 'Italic',
            btnStrikethrough: 'Strikethrough',
            btnList: 'Unordered list',
            btnOrderedList: 'Ordered list',
            btnLink: 'Link',
            btnImage: 'Insert image',
            btnUpload: 'Upload image',
            btnEdit: 'Edit',
            btnPreview: 'Preview',
            btnFullscreen: 'Fullscreen',
            loading: 'Loading'
        }
    };

}(jQuery));
