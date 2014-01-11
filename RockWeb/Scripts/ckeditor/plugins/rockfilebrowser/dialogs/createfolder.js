CKEDITOR.dialog.add('createfolderDialog', function (editor) {
    return {
        title: "Create Folder",
        contents: [
            {
                id: 'tab0',
                label: '',
                title: '',
                elements: [
                    {
                        type: 'text',
                        id: 'createFolderId',
                        label: 'Folder Name',
                        validate:function () {
                            if (!this.getValue()) {
                                debugger
                                return false;
                            }
                        }
                    }
                ]
            }
        ],
        onOk: function (sender) {
            var dialog = this;
            debugger;
            //editor
        }
    }
});