CKEDITOR.dialog.add('deletefolderDialog', function (editor) {
    return {
        title: "Delete Folder",
        editorId: editor.id,
        minWidth: 200,
        minHeight: 60,
        resizable: false,
        contents: [
            {
                id: 'tab0',
                label: '',
                title: '',
                elements: [
                    {
                        type: 'html',
                        html: 'Are you sure you want to delete this folder and all its contents?'
                    }
                ]
            }
        ],
        onOk: function (eventParam) {
            var foldersControlId = 'file-browser-folder-tree_' + eventParam.sender.definition.editorId;
            var foldersRockTree = $('#' + foldersControlId).find('.treeview').data('rockTree');
            var selectedFolderPath = foldersRockTree.$el.find('.selected').closest('.rocktree-item').attr('data-id');
            var restUrl = Rock.settings.get('baseUrl') + 'api/FileBrowser/DeleteFolder?relativeFolderPath=' + encodeURIComponent(selectedFolderPath);
            return $.ajax({
                type: 'POST',
                url: restUrl,
                context: {
                    Editor: editor,
                    FoldersControlId: foldersControlId,
                    FolderPath: selectedFolderPath
                }
            }).done(function (data, textStatus, jqXHR) {
                // refresh the folder itempicker
                var parentFolder = this.FolderPath.substring(0, this.FolderPath.lastIndexOf('\\'));
                this.Editor.execCommand("refreshFolderTree", { controlId: this.FoldersControlId, selectedFolder: parentFolder });
            });
        },
        buttons: [CKEDITOR.dialog.cancelButton, CKEDITOR.dialog.okButton]
    }
});