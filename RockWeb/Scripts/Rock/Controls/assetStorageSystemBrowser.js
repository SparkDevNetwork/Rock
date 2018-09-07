(function ($) {
  'use strict';
  window.Rock = window.Rock || {};
  Rock.controls = Rock.controls || {};

  Rock.controls.assetStorageSystemBrowser = (function () {
    var exports;

    exports = {
      initialize: function (options) {
        var self = this;

        var $assetBrowser = $('#'+ options.controlId);
        var $folderTreeView = $assetBrowser.find('.js-folder-treeview .treeview');
        var $selectFolder = $assetBrowser.find('.js-selectfolder');
        var $expandedFolders = $assetBrowser.find('.js-expandedFolders');
        var $assetStorageId = $assetBrowser.find('.js-assetstorage-id');
        var $treePort = $assetBrowser.find('.js-treeviewport');
        var $treeTrack = $assetBrowser.find('.js-treetrack');

        var $createFolder = $assetBrowser.find('.js-createfolder');
        var $createFolderDiv = $assetBrowser.find('.js-createfolder-div');
        var $createFolderInput = $assetBrowser.find('.js-createfolder-input');
        var $createFolderCancel = $assetBrowser.find('.js-createfolder-cancel');

        var $deleteFolder = $assetBrowser.find('.js-deletefolder')
        var $fileCheckboxes = $assetBrowser.find('.js-checkbox');

        var $renameFile = $assetBrowser.find('.js-renamefile');
        var $renameFileDiv = $assetBrowser.find('.js-renamefile-div');
        var $renameFileInput = $assetBrowser.find('.js-renamefile-input');
        var $renameFileCancel = $assetBrowser.find('.js-renamefile-cancel');
        
        if ($folderTreeView.length == 0) {
          return;
        }

        // Some buttons need an asset selected in order to work
        var temp1 = $assetStorageId.text();
        if ($assetStorageId.text() == "-1") {
          $('.js-assetselect').addClass('aspNetDisabled');
        }
        var temp2 = $selectFolder.text();
        // Some buttons need a folder selected in order to work
        if ($selectFolder.text() == "") {
          $('.js-folderselect').addClass('aspNetDisabled');
        }

        var folderTreeData = $folderTreeView.data('rockTree');

        if (!folderTreeData) {
          var selectedFolders = $selectFolder.text().split(',');
          var expandedFolders = $expandedFolders.text().split('||');

          $folderTreeView.rockTree({
            selectedIds: selectedFolders,
            expandedIds: expandedFolders
          });

          var treePortIScroll = new IScroll($treePort[0], {
            mouseWheel: true,
            indicators: {
              el: '#' + $treeTrack.attr('id'),
              interactive: true,
              resize: false,
              listenY: true,
              listenX: false
            },
            click: false,
            preventDefaultException: { tagName: /.*/ }
          });

          $folderTreeView.on('rockTree:dataBound rockTree:rendered', function (evt) {
            if (treePortIScroll) {
              treePortIScroll.refresh();
            }
          });

          $folderTreeView.on('rockTree:expand rockTree:collapse', function (evt, data) {
            if (treePortIScroll) {
              treePortIScroll.refresh();
            }

            // get the data-id values of rock-tree items that are showing visible children (in other words, Expanded Nodes)
            var expandedDataIds = $(evt.currentTarget).find('.rocktree-children').filter(":visible").closest('.rocktree-item').map(function () {
              var dataId = $(this).attr('data-id');
              if (dataId != data) {
                return dataId;
              }
            }).get().join('||');

            $expandedFolders.text(expandedDataIds);
          });
        }

        $folderTreeView.off('rockTree:selected').on('rockTree:selected', function (e, data) {
          var relativeFolderPath = data;
          var postbackArg;
          var previousStorageId = $assetStorageId.text();
          var expandedFolders = $expandedFolders.text();

          // Some buttons are only active if at least one folder is selected once the tree has been selected then a folder is always selected.
          $('.js-folderselect').removeClass('aspNetDisabled');

          if (data.endsWith("/")) {
            $selectFolder.text(data);
            postbackArg = 'asset-selected:' + $assetStorageId.text() + ',folder-selected:' + relativeFolderPath.replace(/\\/g, "/") + ',previous-asset:' + previousStorageId + ',expanded-folders:' + expandedFolders;
          }
          else {
            $assetStorageId.text(data);
            $selectFolder.text('');
            $expandedFolders.text('');
            postbackArg = 'asset-selected:' + data + ',previous-asset:' + previousStorageId + ',folder-selected:,expanded-folders:';
          }

          var jsPostback = "javascript:__doPostBack('" + options.filesUpdatePanelId + "','" +  postbackArg+ "');"

          window.location = jsPostback;
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
        });

        $renameFile.off('click').on('click', function () {
          $renameFileDiv.fadeToggle();
          $renameFileInput.val('');
        });

        $renameFileCancel.off('click').on('click', function () {
          $renameFileDiv.fadeOut();
          $renameFileInput.val('');
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



