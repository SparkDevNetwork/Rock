CKEDITOR.dialog.add('rockfilebrowserDialog', function (editor) {

    var imageUploaderHtml = "<div class='imageupload-group'> \n" +
                            "	<div class='imageupload-thumbnail'> \n" +
                            "		<img id='" + editor.id + "_imageUploader_img' src='" + Rock.settings.get('baseUrl') + "Assets/Images/no-picture.svg'> \n" +
                            "		<input id='" + editor.id + "_imageUploader_hfBinaryFileId' type='hidden'> \n" +
                            "		<input id='" + editor.id + "_imageUploader_hfBinaryFileTypeGuid' type='hidden'> \n" +
                            "	</div><div class='imageupload-remove'> \n" +
                            "		<a id='" + editor.id + "_imageUploader_rmv' style='display: none;'> \n" +
                            "		  <i class='fa fa-times'></i> \n" +
                            "		</a> \n" +
                            "	</div> \n" +
                            "	<div class='imageupload-dropzone'> \n" +
                            "		<span>drop / click to upload</span> \n" +
                            "		<input id='" + editor.id + "_imageUploader_fu' type='file'> \n" +
                            "	</div> \n" +
                            "</div> \n";

    return {
        title: 'File Browser',
        minWidth: 800,
        minHeight: 200,
        editorId: editor.id,
        contents: [
            {
                id: 'tab0',
                label: '',
                title: '',
                elements: [
                    {
                        type: 'html',
                        html: "" +
                        "<div class='js-file-browser'> \n" +
                        "  <div> \n" +
                        imageUploaderHtml +
                        "    <a class='add btn btn-mini btn-action' href='#' title='Create Folder' id='file-browser-add-folder-button_" + editor.id + "' ><i class='fa fa-plus-circle'></i> Add Folder</a>\n" +
                        "    <a class='delete btn btn-mini btn-danger' href='#' title='Delete Folder' id='file-browser-delete-folder-button_" + editor.id + "' ><i class='fa fa-times'></i> Delete Folder</a>\n" +
                        "    <a class='delete btn btn-mini btn-action' href='#' title='Rename Folder' id='file-browser-rename-folder-button_" + editor.id + "' ><i class='fa fa-exchange'></i> Rename Folder</a>\n" +
                        "    <a class='delete btn btn-mini btn-action' href='#' title='Refresh Folders' id='file-browser-refresh-folders-button_" + editor.id + "' ><i class='fa fa-refresh'></i> Refresh Folders</a>\n" +
                        "  </div> \n" +
                        "  <div class='row'> \n" +
                        "     <div class='col-md-6'> \n" +
                        "         <div id='file-browser-folder-tree_" + editor.id + "' class='picker picker-select js-rock-tree-folders'> \n" +
                        "            <input id='hfItemId_file-browser-folder-tree_" + editor.id + "' type='hidden' ></input> \n" +
                        "            <div id='treeview-scroll-container_" + editor.id + "' class='scroll-container scroll-container-vertical scroll-container-picker'> \n" +
                        "               <div class='scrollbar'> \n" +
                        "                   <div class='track'> \n" +
                        "                       <div class='thumb'> \n" +
                        "                           <div class='end'></div> \n" +
                        "                       </div> \n" +
                        "                   </div> \n" +
                        "               </div> \n" +
                        "               <div class='viewport'> \n" +
                        "                   <div class='overview'> \n" +
                        "                       <div id='treeviewFolderItems_" + editor.id + "' class='treeview treeview-items'></div> \n" +
                        "                   </div> \n" +
                        "               </div> \n" +
                        "            </div> \n" +
                        "         </div> \n" +
                        "     </div> \n" +
                        "     <div class='col-md-6'>" +

                        "         <div id='file-browser-file-tree_" + editor.id + "' class='picker picker-select js-rock-tree-files'> \n" +
                        "            <div id='treeview-scroll-container-files_" + editor.id + "' class='scroll-container scroll-container-vertical scroll-container-picker'> \n" +
                        "               <div class='scrollbar'> \n" +
                        "                   <div class='track'> \n" +
                        "                       <div class='thumb'> \n" +
                        "                           <div class='end'></div> \n" +
                        "                       </div> \n" +
                        "                   </div> \n" +
                        "               </div> \n" +
                        "               <div class='viewport'> \n" +
                        "                   <div class='overview'> \n" +
                        "                       <div id='treeviewFileItems_" + editor.id + "' class='treeview treeview-items'></div> \n" +
                        "                   </div> \n" +
                        "               </div> \n" +
                        "            </div> \n" +
                        "         </div> \n" +
                        "     </div> \n" +
                        "  </div> \n" +
                        "</div> \n"
                    }
                ]
            }
        ],
        onLoad: function (eventParam) {
            var foldersControlId = 'file-browser-folder-tree_' + eventParam.sender.definition.editorId;
            var addFolderButtonId = 'file-browser-add-folder-button_' + eventParam.sender.definition.editorId;
            var deleteFolderButtonId = 'file-browser-delete-folder-button_' + eventParam.sender.definition.editorId;
            var renameFolderButtonId = 'file-browser-rename-folder-button_' + eventParam.sender.definition.editorId;
            var refreshFoldersButtonId = 'file-browser-refresh-folders-button_' + eventParam.sender.definition.editorId;
            var filesControlId = 'file-browser-file-tree_' + eventParam.sender.definition.editorId;
            var imageUploaderIdPrefix = eventParam.sender.definition.editorId + "_imageUploader_";

            // setup events
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

            $('#' + addFolderButtonId).on('click', function (e, data) {
                editor.execCommand("createfolderDialog");
            });

            $('#' + deleteFolderButtonId).on('click', function (e, data) {
                editor.execCommand("deletefolderDialog");
            });

            $('#' + renameFolderButtonId).on('click', function (e, data) {
                editor.execCommand("renamefolderDialog");
            });

            $('#' + refreshFoldersButtonId).on('click', function (e, data) {
                // refresh the folder itempicker
                editor.execCommand("refreshFolderTree", { controlId: foldersControlId, selectedFolder: null });
            });

            // make the \\External Site folder be selected on show
            $('#hfItemId_' + foldersControlId).val('\\External Site');

            // make an itemPicker for the Folders Tree
            Rock.controls.itemPicker.initialize({
                controlId: foldersControlId,
                startingId: '/',
                restUrl: Rock.settings.get('baseUrl') + 'api/FileBrowser/GetSubFolders?folderName=',
                allowMultiSelect: false
            });

            // initialize the imageUploader
            Rock.controls.imageUploader.initialize({
                controlId: imageUploaderIdPrefix + 'fu',
                isBinaryFile: 'F',
                hfFileId: imageUploaderIdPrefix + 'hfBinaryFileId',
                imgThumbnail: imageUploaderIdPrefix + 'img',
                aRemove: imageUploaderIdPrefix + 'rmv',
                submitFunction: function (e, data) {
                    var fileBrowser = $(this).closest('.js-file-browser');
                    var foldersRockTree = $(fileBrowser).find('.js-rock-tree-folders .treeview').data('rockTree');
                    var selectedFolderPath = foldersRockTree.$el.find('.selected').closest('.rocktree-item').attr('data-id');

                    if (selectedFolderPath) {
                        // include the selected folder in the post to ~/ImageUploader.ashx
                        data.formData = { folderPath: selectedFolderPath };
                    }
                    else {
                        // no directory selected
                        return false;
                    }
                },
                doneFunction: function (e, data) {
                    var fileBrowser = $('#' + this.controlId).closest('.js-file-browser');
                    var foldersRockTree = $(fileBrowser).find('.js-rock-tree-folders .treeview').data('rockTree');
                    var selectedFolderPath = foldersRockTree.$el.find('.selected').closest('.rocktree-item').attr('data-id');
                    if (selectedFolderPath) {
                        // reselect the node to refresh the list of files
                        foldersRockTree.$el.trigger('rockTree:selected', selectedFolderPath);
                    }
                    else {
                        // no directory selected
                        return false;
                    }
                },
                fileType: 'image'
            });
        },
        onOk: function (sender) {
            var dialog = this;
            var filesRockTree = $('#file-browser-file-tree_' + dialog.definition.editorId + ' .treeview').data('rockTree');
            if (filesRockTree && filesRockTree.selectedNodes.length) {
                // get the image path from the first selected node
                var selectedNode = filesRockTree.selectedNodes[0];

                // convert the path into a URL then construct the img tag with an alt of the filename
                var imgUrl = Rock.settings.get('baseUrl') + 'content' + selectedNode.id.replace(/\\/g, '/');
                var filename = imgUrl.replace(/^.*[\\\/]/, '')
                var imageHtml = '<img src="' + imgUrl + '" alt="' + filename + '" />'

                // insert the img html into the editor
                editor.insertHtml(imageHtml);
            }
        }
    };
});