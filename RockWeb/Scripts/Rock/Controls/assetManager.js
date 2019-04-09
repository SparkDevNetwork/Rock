(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.assetManager = (function () {
        var exports;

        exports = {
            isValidName: function (name) {
                var regex = new RegExp("^[^*/><?\\|:,~]+$");
                return regex.test(name);
            },
            createFolderAccept_click: function () {
                var name = $('.js-createfolder-input').val();

                if (name === "") {
                    $('.js-createfolder-notification').show().text('Folder name is required.');
                    return false;
                }

                if (!this.isValidName(name)) {
                    $('.js-createfolder-notification').show().text('Invalid characters in path');
                    return false;
                }
            },
            renameFileAccept_click: function () {
                var name = $('.js-renamefile-input').val();

                if (name === "") {
                    $('.js-renamefile-notification').show().text('File name is required.');
                    return false;
                }

                if (!this.isValidName(name)) {
                    $('.js-renamefile-notification').show().text('Invalid characters in file name.');
                    return false;
                }
            },
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

                var $fileCheckboxes = $assetBrowser.find('.js-checkbox');

                var $renameFile = $assetBrowser.find('.js-renamefile');
                var $renameFileDiv = $assetBrowser.find('.js-renamefile-div');
                var $renameFileInput = $assetBrowser.find('.js-renamefile-input');
                var $renameFileCancel = $assetBrowser.find('.js-renamefile-cancel');
                var $renameFileAccept = $assetBrowser.find('.js-renamefile-accept');

                if ($folderTreeView.length == 0) {
                    return;
                }

                // Some buttons need an asset selected in order to work
                if ($assetStorageId.val() == "-1") {
                    $('.js-assetselect').addClass('aspNetDisabled');
                }

                // Can only add if either an asset or folder is selected
                if ($selectFolder.val() == "" && $assetStorageId.val() == "-1") {
                    $('.js-createfolder').addClass('aspNetDisabled');
                }
                else {
                    $('.js-createfolder').removeClass('aspNetDisabled');
                }

                // Can delete folder only if a folder is selected
                if ($selectFolder.val() == "" || $isRoot.val() == "True"  ) {
                    $('.js-deletefolder').addClass('aspNetDisabled');
                }
                else {
                    $('.js-deletefolder').removeClass('aspNetDisabled');
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
                            if (dataId != data) {
                                return decodeURIComponent(dataId);
                            }
                        }).get().join('||');

                        $expandedFolders.val(expandedDataIds);
                    });

                    function resizeScrollAreaHeight() {
                        var overviewHeight = $folderTreeView.closest('.overview').height();
                        $treePort.height(overviewHeight);
                        if (treePortIScroll) {
                            treePortIScroll.refresh();
                        }
                    }
                }

                $folderTreeView.off('rockTree:selected').on('rockTree:selected', function (e, data) {
                    debugger
                    var assetFolderIdParts = unescape(data).split(",");
                    var storageId = assetFolderIdParts[0] || "";
                    var folder = assetFolderIdParts[1] || "";
                    var isRoot = assetFolderIdParts[2] || false;
                    var postbackArg;
                    var expandedFolders = encodeURIComponent($expandedFolders.val());

                    // Some buttons are only active if at least one folder is selected once the tree has been selected then a folder is always selected.
                    //$('.js-folderselect').removeClass('aspNetDisabled');

                    if ($selectFolder.val() != folder || $assetStorageId.val() != storageId) {
                        $selectFolder.val(folder);
                        $assetStorageId.val(storageId);
                        $isRoot.val(isRoot);
                        postbackArg = 'storage-id:' + storageId + '?folder-selected:' + folder.replace(/\\/g, "/").replace("'", "\\'") + '?expanded-folders:' + expandedFolders.replace("'", "\\'") + '?isRoot:' + isRoot;

                        var jsPostback = "javascript:__doPostBack('" + options.filesUpdatePanelId + "','" + postbackArg + "');";
                        window.location = jsPostback;
                    }
                });

                // Some buttons are only active if one file is selected.
                $fileCheckboxes.off('click').on('click', function () {
                    var numberChecked = $fileCheckboxes.filter(':checked').length;

                    if (numberChecked == 0) {
                        $('.js-singleselect').addClass('aspNetDisabled');
                        $('.js-minselect').addClass('aspNetDisabled');
                    }
                    else if (numberChecked == 1) {
                        $('.js-singleselect').removeClass('aspNetDisabled');
                        $('.js-minselect').removeClass('aspNetDisabled');
                    }
                    else {
                        $('.js-singleselect').addClass('aspNetDisabled');
                        $('.js-minselect').removeClass('aspNetDisabled');
                    }
                });

                $createFolder.off('click').on('click', function () {
                    $createFolderDiv.fadeToggle();
                    $createFolderInput.val('');
                });

                $createFolderCancel.off('click').on('click', function () {
                    $createFolderDiv.fadeOut();
                    $createFolderInput.val('');
                    $('.js-createfolder-notification').hide().text('');
                });

                $renameFile.off('click').on('click', function () {
                    $renameFileDiv.fadeToggle();
                    $renameFileInput.val('');
                });

                $renameFileCancel.off('click').on('click', function () {
                    $renameFileDiv.fadeOut();
                    $renameFileInput.val('');
                    $('.js-renamefile-notification').hide().text('');
                });

                $deleteFolder.off('click').on('click', function (e) {
                    if ($(this).attr('disabled') == 'disabled') {
                        return false;
                    }

                    Rock.dialogs.confirmDelete(e, 'folder and all its contents');
                });
            }
        };

        return exports;

    }());
}(jQuery));



