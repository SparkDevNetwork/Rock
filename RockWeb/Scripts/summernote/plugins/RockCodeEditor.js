var RockCodeEditor = function (context, keepEditorContent) {
    var ui = $.summernote.ui;
    var $codeEditor = $('#codeeditor-div-' + context.options.codeEditorOptions.controlId);
    var $codeEditorContainer = $codeEditor.closest('.code-editor-container');
    $codeEditorContainer.hide();
    $codeEditorContainer.height(context.layoutInfo.editingArea.height());
    $inCodeEditorModeHiddenField = $('#' + context.options.codeEditorOptions.inCodeEditorModeHiddenFieldId);
    $inCodeEditorModeHiddenField.val("0");

    // move code editor into summernote div
    var element = $codeEditorContainer.detach();
    context.layoutInfo.editingArea.closest('.note-editor').append(element);

    // create button
    var button = ui.button({
        contents: '<i class="fa fa-file-code-o"/>',
        tooltip: 'Code Editor',
        className: 'btn-codeview', // swap out the default btn-codeview with the RockCodeEditor
        click: function () {
            if ($codeEditorContainer.is(':visible')) {
                context.invoke('toolbar.updateCodeview', true);
                var content = ace.edit($codeEditor.attr('id')).getValue();
                context.code(content);
                context.layoutInfo.editingArea.show();
                context.layoutInfo.statusbar.show();
                $codeEditorContainer.hide();
                $inCodeEditorModeHiddenField.val("0");
                context.invoke('toolbar.updateCodeview', false);
            } else {
                $codeEditorContainer.height(context.layoutInfo.editingArea.height());

                // make sure it has at least some usable height
                if ($codeEditorContainer.height() < 100) {
                    $codeEditorContainer.height(100)
                }

                context.layoutInfo.editingArea.hide();
                context.layoutInfo.statusbar.hide();

                // HtmlEditor.cs will initialize this with keepEditorContent = true and set the codeEditor content instead of the summernoteNote editor content
                // this will prevent bad html or scripts from trying to render when startInCodeEditor mode is enabled
                if (!keepEditorContent) {
                    var content = context.code();
                    ace.edit($codeEditor.attr('id')).setValue(content);
                }
                $codeEditorContainer.show();

                // set the hiddenfield so we know which editor to get the value from on postback
                $inCodeEditorModeHiddenField.val("1");
                context.invoke('toolbar.updateCodeview', true);
            }
        }
    });

    return button.render();   // return button as jquery object 
}