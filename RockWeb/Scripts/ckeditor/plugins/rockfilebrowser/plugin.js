(function () {
    CKEDITOR.plugins.add('rockfilebrowser', {
        init: function (editor) {

            editor.addCommand('rockfilebrowserDialog', new CKEDITOR.dialogCommand('rockfilebrowserDialog'));
            editor.addCommand('createfolderDialog', new CKEDITOR.dialogCommand('createfolderDialog'));
            editor.addCommand('deletefolderDialog', new CKEDITOR.dialogCommand('deletefolderDialog'));
            editor.addCommand('renamefolderDialog', new CKEDITOR.dialogCommand('renamefolderDialog'));

            editor.addCommand('refreshFolderTree', {
                exec: function (editor, options) {
                    var expandedParentIds = [];

                    if (options.selectedFolder) {
                        var previousFolder = '';
                        var folderParts = options.selectedFolder.split('\\');
                        $.each(folderParts, function (index, value) {
                            if (value) {
                                expandedParentIds.push(previousFolder + '\\' + value);
                                previousFolder = previousFolder + '\\' + value;
                            }
                        });

                        $('#hfItemId_' + options.controlId).val(options.selectedFolder);
                    }
                    else {
                        $('#hfItemId_' + options.controlId).val('\\External Site');
                    }

                    Rock.controls.itemPicker.initialize({
                        controlId: options.controlId,
                        startingId: '/',
                        restUrl: Rock.settings.get('baseUrl') + 'api/FileBrowser/GetSubFolders?folderName=',
                        allowMultiSelect: false,
                        expandedIds: expandedParentIds
                    });
                }
            });

            editor.ui.addButton && editor.ui.addButton('rockfilebrowser', {
                label: 'File Browser',
                command: 'rockfilebrowserDialog',
                icon: this.path + 'rockfilebrowser.png'
            });

            CKEDITOR.dialog.add('rockfilebrowserDialog', this.path + 'dialogs/rockfilebrowser.js');
            CKEDITOR.dialog.add('createfolderDialog', this.path + 'dialogs/createfolder.js');
            CKEDITOR.dialog.add('deletefolderDialog', this.path + 'dialogs/deletefolder.js');
            CKEDITOR.dialog.add('renamefolderDialog', this.path + 'dialogs/renamefolder.js');
        }
    });
})()