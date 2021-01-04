(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.assetManager = (function () {
        var exports;

        exports = {
            initialize: function (options) {
                var self = this;

                var $assetBrowser = $('#' + options.controlId);
                var $folderTreeView = $assetBrowser.find('.js-folder-treeview .treeview');
                var $selectFolder = $assetBrowser.find('.js-selectfolder');
                var $expandedFolders = $assetBrowser.find('.js-expandedFolders');
                var $assetStorageId = $assetBrowser.find('.js-assetstorage-id');
                var $treePort = $assetBrowser.find('.js-treeviewport');
                var $treeTrack = $assetBrowser.find('.js-treetrack');
                var $isRoot = $assetBrowser.find('.js-isroot');

                var $createFolder = $assetBrowser.find('.js-createfolder');
                var $createFolderDiv = $assetBrowser.find('.js-createfolder-div');
                var $createFolderInput = $assetBrowser.find('.js-createfolder-input');
                var $createFolderCancel = $assetBrowser.find('.js-createfolder-cancel');
                var $createFolderAccept = $assetBrowser.find('.js-createfolder-accept');
                var $deleteFolder = $assetBrowser.find('.js-deletefolder');
                var $createFolderNotification = $assetBrowser.find('.js-createfolder-notification');


                // 2020-09-07 MDP
                // Asset manager can be used in 3 ways.
                // 1) An ItemFromBlockPicker control (An Asset attribute).
                // 2) Within the HTML Editor as a summernote plugin.
                // 3) In Home > CMS Configuration > Asset Manager.
                // The OK/Save button is the parent modal (but different types of modals) in case 1 and 2, and without an OK/Save button in case 3.
                // So to find all the js-singleselect items in cases 1 and 2, we'll have to look in the modal so that the OK/Save button is found
                var $assetManagerModal = $assetBrowser.closest('.js-AssetManager-modal');
                var $singleSelectControls;
                if ($assetManagerModal.length) {
                    // this would be cases #1 and #2
                    $singleSelectControls = $assetManagerModal.find('.js-singleselect');
                } else {
                    // this would be case #3
                    $singleSelectControls = $assetBrowser.find('.js-singleselect');
                }

                var $minSelectControls = $assetBrowser.find('.js-minselect');
                var $fileCheckboxes = $assetBrowser.find('.js-checkbox');

                var $renameFile = $assetBrowser.find('.js-renamefile');
                var $renameFileDiv = $assetBrowser.find('.js-renamefile-div');
                var $renameFileInput = $assetBrowser.find('.js-renamefile-input');
                var $renameFileCancel = $assetBrowser.find('.js-renamefile-cancel');
                var $renameFileAccept = $assetBrowser.find('.js-renamefile-accept');
                var $renamefilenotification = $assetBrowser.find('.js-renamefile-notification');

                if ($folderTreeView.length === 0) {
                    return;
                }

                // Some buttons need an asset selected in order to work
                if ($assetStorageId.val() === "-1") {
                    $('.js-assetselect').addClass('aspNetDisabled');
                }

                $fileCheckboxes.removeAttr('checked');

                // Can only add if either an asset or folder is selected
                if ($selectFolder.val() === "" && $assetStorageId.val() === "-1") {
                    $createFolder.addClass('aspNetDisabled');
                }
                else {
                    $createFolder.removeClass('aspNetDisabled');
                }

                // Can delete folder only if a folder is selected
                if ($selectFolder.val() === "" || $isRoot.val() === "True") {
                    $deleteFolder.addClass('aspNetDisabled');
                }
                else {
                    $deleteFolder.removeClass('aspNetDisabled');
                }

                var folderTreeData = $folderTreeView.data('rockTree');

                if (!folderTreeData) {
                    var selectedFolders = [encodeURIComponent($assetStorageId.val() + ',' + $selectFolder.val())];
                    var expandedFolders = $expandedFolders.val().split('||');
                    expandedFolders.forEach(function (part, i, array) {
                        array[i] = encodeURIComponent(array[i]);
                    });

                    $folderTreeView.rockTree({
                        restUrl: options.restUrl,
                        selectedIds: selectedFolders,
                        expandedIds: expandedFolders
                    });

                    var treePortIScroll = new IScroll($treePort[0], {
                        mouseWheel: false,
                        scrollX: true,
                        scrollY: false,
                        indicators: {
                            el: '#' + $treeTrack.attr('id'),
                            interactive: true,
                            resize: false,
                            listenY: false,
                            listenX: true
                        },
                        click: false,
                        preventDefaultException: { tagName: /.*/ }
                    });

                    // resize scrollbar when the window resizes
                    $(document).ready(function () {
                        $(window).on('resize', function () {
                            resizeScrollAreaHeight();
                        });
                    });

                    $folderTreeView.on('rockTree:dataBound rockTree:rendered', function (evt) {
                        resizeScrollAreaHeight();
                    });

                    $folderTreeView.on('rockTree:expand rockTree:collapse', function (evt, data) {
                        resizeScrollAreaHeight();

                        // get the data-id values of rock-tree items that are showing visible children (in other words, Expanded Nodes)
                        var expandedDataIds = $(evt.currentTarget).find('.rocktree-children').filter(":visible").closest('.rocktree-item').map(function () {
                            var dataId = $(this).attr('data-id');
                            if (dataId !== data) {
                                return decodeURIComponent(dataId);
                            }
                        }).get().join('||');

                        $expandedFolders.val(expandedDataIds);
                    });
                }

                function resizeScrollAreaHeight() {
                    var overviewHeight = $folderTreeView.closest('.overview').height();
                    $treePort.height(overviewHeight);

                    if (treePortIScroll) {
                        treePortIScroll.refresh();
                    }
                }

                function isValidName(name) {
                    var regex = new RegExp("^[^*/><?\\|:,~]+$");
                    return regex.test(name);
                }

                $folderTreeView.off('rockTree:selected').on('rockTree:selected', function (e, data) {
                    var assetFolderIdParts = unescape(data).split(",");
                    var storageId = assetFolderIdParts[0] || "";
                    var folder = assetFolderIdParts[1] || "";
                    var isRoot = assetFolderIdParts[2] || false;
                    var postbackArg;
                    var expandedFolders = encodeURIComponent($expandedFolders.val());

                    // Can only add if either an asset or folder is selected
                    if ($selectFolder.val() === "" && $assetStorageId.val() === "-1") {
                        $createFolder.addClass('aspNetDisabled');
                    }
                    else {
                        $createFolder.removeClass('aspNetDisabled');
                    }

                    // Can delete folder only if a folder is selected
                    if ($selectFolder.val() === "" || $isRoot.val() === "True") {
                        $deleteFolder.addClass('aspNetDisabled');
                    }
                    else {
                        $deleteFolder.removeClass('aspNetDisabled');
                    }

                    if ($selectFolder.val() !== folder || $assetStorageId.val() !== storageId) {
                        $selectFolder.val(folder);
                        $assetStorageId.val(storageId);
                        $isRoot.val(isRoot);
                        postbackArg = 'storage-id:' + storageId + '?folder-selected:' + folder.replace(/\\/g, "/").replace("'", "\\'") + '?expanded-folders:' + expandedFolders.replace("'", "\\'") + '?isRoot:' + isRoot;

                        var jsPostback = "javascript:__doPostBack('" + options.filesUpdatePanelId + "','" + postbackArg + "');";
                        window.location = jsPostback;
                    }
                });

                $fileCheckboxes.off('click').on('click', function () {
                    var numberChecked = $fileCheckboxes.filter(':checked').length;

                    if (numberChecked === 0) {
                        $singleSelectControls.addClass('aspNetDisabled');
                        $('.js-minselect').addClass('aspNetDisabled');
                    }
                    else if (numberChecked === 1) {
                        $singleSelectControls.removeClass('aspNetDisabled');
                        $minSelectControls.removeClass('aspNetDisabled');
                    }
                    else {
                        $singleSelectControls.addClass('aspNetDisabled');
                        $minSelectControls.removeClass('aspNetDisabled');
                    }
                });

                $createFolder.off('click').on('click', function () {
                    $createFolderDiv.fadeToggle();
                    $createFolderInput.val('');
                });

                $createFolderCancel.off('click').on('click', function () {
                    $createFolderDiv.fadeOut();
                    $createFolderInput.val('');
                    $createFolderNotification.hide().text('');
                });

                $createFolderAccept.off('click').on('click', function (e) {
                    var name = $createFolderInput.val();

                    if (name === "") {
                        $createFolderNotification.show().text('Folder name is required.');
                        e.preventDefault();
                    }

                    if (!isValidName(name)) {
                        $createFolderNotification.show().text('Invalid characters in path');
                        e.preventDefault();
                    }
                });

                $renameFile.off('click').on('click', function () {
                    $renameFileDiv.fadeToggle();
                    $renameFileInput.val('');
                });

                $renameFileCancel.off('click').on('click', function () {
                    $renameFileDiv.fadeOut();
                    $renameFileInput.val('');
                    $renamefilenotification.hide().text('');
                });

                $renameFileAccept.off('click').on('click', function (e) {
                    var name = $renameFileInput.val();

                    if (name === "") {
                        $renamefilenotification.show().text('File name is required.');
                        e.preventDefault();
                    }

                    if (!isValidName(name)) {
                        $renamefilenotification.show().text('Invalid characters in file name.');
                        e.preventDefault();
                    }
                });

                $deleteFolder.off('click').on('click', function (e) {
                    if ($(this).attr('disabled') === 'disabled') {
                        return false;
                    }

                    Rock.dialogs.confirmDelete(e, 'folder and all its contents');
                });
            }
        };

        return exports;

    }());
}(jQuery));
