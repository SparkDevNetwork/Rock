CKEDITOR.dialog.add('rockimagebrowserDialog', function (editor) {
    return {
        title: 'File Browser',
        minWidth: 800,
        minHeight: 200,
        folderTreeId: 'image-browser-folder-tree_' + editor.id,
        filesTreeId: 'image-browser-file-tree_' + editor.id,
        contents: [
            {
                id: 'tab0',
                label: '',
                title: '',
                elements: [
                    {
                        type: 'html',
                        html: "" +
                        "<div class='row'>" +
                        "   <div class='col-md-6'>" +
                        "       <div id='image-browser-folder-tree_" + editor.id + "' class='picker picker-select'> \n" +
                        "          <div id='treeview-scroll-container_" + editor.id + "' class='scroll-container scroll-container-vertical scroll-container-picker'> \n" +
                        "             <div class='scrollbar'> \n" +
                        "                 <div class='track'> \n" +
                        "                     <div class='thumb'> \n" +
                        "                         <div class='end'></div> \n" +
                        "                     </div> \n" +
                        "                 </div> \n" +
                        "             </div> \n" +
                        "             <div class='viewport'> \n" +
                        "                 <div class='overview'> \n" +
                        "                     <div id='treeviewFolderItems_" + editor.id + "' class='treeview treeview-items'></div> \n" +
                        "                 </div> \n" +
                        "             </div> \n" +
                        "          </div> \n" +
                        "       </div> \n" +
                        "   </div> \n" +
                        "   <div class='col-md-6'>" +
                        "       <div id='image-browser-file-tree_" + editor.id + "' class='picker picker-select'> \n" +
                        "          <div id='treeview-scroll-container-files_" + editor.id + "' class='scroll-container scroll-container-vertical scroll-container-picker'> \n" +
                        "             <div class='scrollbar'> \n" +
                        "                 <div class='track'> \n" +
                        "                     <div class='thumb'> \n" +
                        "                         <div class='end'></div> \n" +
                        "                     </div> \n" +
                        "                 </div> \n" +
                        "             </div> \n" +
                        "             <div class='viewport'> \n" +
                        "                 <div class='overview'> \n" +
                        "                     <div id='treeviewFileItems_" + editor.id + "' class='treeview treeview-items'>Hello!!!</div> \n" +
                        "                 </div> \n" +
                        "             </div> \n" +
                        "          </div> \n" +
                        "       </div> \n" +

                        "   </div> \n" +
                        "</div> \n"
                    }
                ]
            }
        ],
        onShow: function (eventParam) {

            var foldersControlId = eventParam.sender.definition.folderTreeId;
            var filesControlId = eventParam.sender.definition.filesTreeId;

            // make an itemPicker for the Folders Tree
            Rock.controls.itemPicker.initialize({
                controlId: foldersControlId,
                startingId: '/',
                restUrl: Rock.settings.get('baseUrl') + 'api/FileBrowser/GetSubFolders?folderName=',
                allowMultiSelect: false
            });

            // 
            $('#' + foldersControlId).find('.treeview').on('rockTree:selected', function (sender, itemId) {

                var folderParam = encodeURIComponent(itemId);

                // initialize another itemPicker for the Files when a folder is selected. (It'll end up being just a list since there are no children)
                Rock.controls.itemPicker.initialize({
                    controlId: filesControlId,
                    startingId: '',
                    restUrl: Rock.settings.get('baseUrl') + 'api/FileBrowser/GetFiles?folderName=' + folderParam,
                    allowMultiSelect: false
                });
            });

            $('#' + foldersControlId).show();

        },
        onOk: function () {
            var dialog = this;

            editor.insertElement('##TODO##');
        }
    };
});