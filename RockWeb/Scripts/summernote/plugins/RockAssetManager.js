var RockAssetManager = function (context) {
    var ui = $.summernote.ui;

    var button = ui.button({
        contents: '<i class="fa fa-folder-open"/>',
        tooltip: 'Asset Manager',
        click: function () {
            context.invoke('editor.saveRange');
            var iframeUrl = Rock.settings.get('baseUrl') + "htmleditorplugins/rockassetmanager";
            // Add query string to the iframeUrl if needed

            Rock.controls.modal.show(context.layoutInfo.editor, iframeUrl);

            $modalPopupIFrame = Rock.controls.modal.getModalPopupIFrame();
            $modalPopupIFrame.load(function () {

                $modalPopupIFrame.contents().off('click').on('click', '.js-select-file-button', function (e) {
                    Rock.controls.modal.close();

                    var uri = "";
                    var fileName = "";

                    // Only one file will be selected as the button is disabled if selected items is not 1.
                    var $assetManagerFileTable = $(this).closest('.js-AssetManager-modal').find('.assetmanager-files');
                    $assetManagerFileTable.find('tr').each(function () {
                        var row = $(this);
                        if (row.find('input[type="checkbox"]').is(':checked')) {
                            uri = row.find('.js-assetManager-uri').text();
                            fileName = row.find('.js-assetManager-name').text();
                        }
                    });
                    
                    // if we have values let summernote create the link and return it.
                    if (uri.length > 0) {
                        context.invoke('editor.restoreRange');

                        if (IsImage(fileName)) {
                            // Create an image tag

                            // If there is already an img selected, just change the src
                            var imgTarget = context.invoke('editor.restoreTarget');
                            if (imgTarget) {
                                imgTarget.src = uri;
                                imgTarget.alt = fileName;
                            }
                            else {
                                // insert the image at 25% to get them started
                                context.invoke('editor.insertImage', uri, function ($image) {
                                    $image.css('width', '25%');
                                    $image.attr('alt', fileName);
                                });
                            }
                        }
                        else {
                            // Create an anchor tag
                            context.invoke('editor.createLink', {
                                text: fileName,
                                url: uri,
                                newWindow: false
                            });
                        }

                        context.invoke('triggerEvent', 'change');
                    }
                });

                $modalPopupIFrame.contents().on('click', '.js-cancel-file-button', function () {
                    Rock.controls.modal.close();
                });
            });
        }
    });

    var IsImage = function (e) {
        if (e.match(/.(jpg|jpeg|png|gif|svg)$/i)) {
            return true;
        }
        return false;
    };

    if (context.options.rockAssetManagerOptions.enabled) {
        // return button as jquery object 
        return button.render();
    }
    else {
        return null;
    }

};
