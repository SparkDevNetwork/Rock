<%@ Control Language="C#" AutoEventWireup="true" CodeFile="HtmlEditorFileBrowser.ascx.cs" Inherits="RockWeb.Blocks.Utility.HtmlEditorFileBrowser" %>

<asp:Panel ID="pnlFileBrowser" runat="server">
    <asp:Panel ID="pnlModalHeader" CssClass="modal-header" runat="server" Visible="false">
        <h3 class="modal-title">
            <asp:Literal ID="lTitle" runat="server"></asp:Literal>
            <span class="js-cancel-file-button cursor-pointer pull-right" style="opacity: .5">&times;</span>
        </h3>
    </asp:Panel>

    <div class="picker-wrapper clearfix">
        <%-- Folders - Separate UpdatePanel so that Tree doesn't get rebuilt on postbacks (unless the server explicitly wants it to get rebuilt) --%>
        <asp:UpdatePanel ID="upnlFolders" Class="picker-folders" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
            <ContentTemplate>
                <div class="actions btn-group">
                    <asp:LinkButton ID="lbCreateFolder" runat="server" CssClass="btn btn-sm btn-default" OnClick="lbCreateFolder_Click" CausesValidation="false" ToolTip="New Folder"><i class="fa fa-folder-plus"></i></asp:LinkButton>
                    <asp:LinkButton ID="lbRenameFolder" runat="server" CssClass="btn btn-sm btn-default" OnClientClick="if ($(this).attr('disabled') == 'disabled') { return false; }" OnClick="lbRenameFolder_Click" CausesValidation="false" ToolTip="Rename Folder"><i class="fa fa-i-cursor"></i></asp:LinkButton>
                    <asp:LinkButton ID="lbMoveFolder" runat="server" CssClass="btn btn-sm btn-default" OnClientClick="if ($(this).attr('disabled') == 'disabled') { return false; }" OnClick="lbMoveFolder_Click" CausesValidation="false" ToolTip="Move Folder"><i class="fa fa-external-link"></i></asp:LinkButton>
                    <asp:LinkButton ID="lbDeleteFolder" runat="server" CssClass="btn btn-sm btn-default" OnClientClick="if ($(this).attr('disabled') == 'disabled') { return false; } Rock.dialogs.confirmDelete(event, 'folder and all its contents');" OnClick="lbDeleteFolder_Click" CausesValidation="false" ToolTip="Delete Folder"><i class="fa fa-trash-alt"></i></asp:LinkButton>
                    <asp:LinkButton ID="lbRefresh" runat="server" CssClass="btn btn-sm btn-default" OnClick="lbRefresh_Click" CausesValidation="false" ToolTip="Refresh"><i class="fa fa-refresh"></i></asp:LinkButton>
                </div>

                <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" Text="Folder not found" Visible="false" />

                <div>
                    <div class="scroll-container scroll-container-vertical scroll-container-picker js-folder-treeview">
                        <div class="scrollbar">
                            <asp:Panel ID="pnlTreeTrack" runat="server" CssClass="track">
                                <div class="thumb">
                                    <div class="end"></div>
                                </div>
                            </asp:Panel>
                        </div>
                        <asp:Panel ID="pnlTreeViewPort" runat="server" CssClass="viewport">
                            <div class="overview">
                                <asp:Label ID="lblFolders" CssClass="treeview treeview-items" runat="server" />
                            </div>
                        </asp:Panel>
                    </div>
                </div>

                <script type="text/javascript">
                    var <%=pnlTreeViewPort.ClientID%>IScroll = null;
                    Sys.Application.add_load(function () {

                        var folderTreeData = $('.js-folder-treeview .treeview').data('rockTree');

                        // init the folder list treeview if it hasn't been created already
                        if (!folderTreeData) {
                            var selectedFolders = $('#<%=hfSelectedFolder.ClientID%>').val().split(',');
                            // init rockTree on folder (no url option since we are generating off static html)
                            $('.js-folder-treeview .treeview').rockTree({
                                selectedIds: selectedFolders
                            });

                            // init scroll bars for folder divs
                            <%=pnlTreeViewPort.ClientID%>IScroll = new IScroll('#<%=pnlTreeViewPort.ClientID%>', {
                                mouseWheel: true,
                                indicators: {
                                    el: '#<%=pnlTreeTrack.ClientID%>',
                                    interactive: true,
                                    resize: false,
                                    listenY: true,
                                    listenX: false,
                                },
                                click: false,
                                preventDefaultException: { tagName: /.*/ }
                            });

                            $('.js-folder-treeview .treeview').on('rockTree:expand rockTree:collapse rockTree:dataBound rockTree:rendered', function (evt) {
                                // update the folder treeview scroll bar
                                if (<%=pnlTreeViewPort.ClientID%>IScroll) {
                                    <%=pnlTreeViewPort.ClientID%>IScroll.refresh();
                                }
                            });
                        }

                        // init the file list RockList on every load
                        $('.js-file-list .js-listview').rockList();
                        new IScroll('#<%=pnlListViewPort.ClientID%>', {
                            mouseWheel: false,
                            indicators: {
                                el: '#<%=pnlListTrack.ClientID%>',
                                interactive: true,
                                resize: false,
                                listenY: true,
                                listenX: false,
                            },
                            click: false,
                            preventDefaultException: { tagName: /.*/ }
                        });

                        // js for when a file delete is clicked
                        $('.js-file-list .js-delete-file').off('click');
                        $('.js-file-list .js-delete-file').on('click', function (e, data) {
                            var selectedFileId = $(this).closest('li').attr('data-id');
                            Rock.dialogs.confirm("Are you sure you want to delete this file?", function (confirmResult, data) {
                                if (confirmResult) {
                                    // use setTimeout so that the doPostBack happens later (to avoid javascript exception that occurs due to timing)
                                    setTimeout(function () {
                                        var postbackArg = 'file-delete:' + selectedFileId.replace(/\\/g, "/").replace(/'/g, "\\'");
                                        window.location = "javascript:__doPostBack('<%=upnlFiles.ClientID %>', '" + postbackArg + "')";
                                    });
                                }
                            });
                        });

                        // js for when a file download is clicked, allow standard href functionality.
                        $('.js-file-list .js-download-file').off('click');
                        $('.js-file-list .js-download-file').on('click', function (e, data) {
                            e.stopPropagation();
                        });

                        // js for when a file download is clicked, allow standard href functionality.
                        $('.js-file-list .js-edit-file').off('click');
                        $('.js-file-list .js-edit-file').on('click', function (e, data) {
                            e.stopPropagation();
                            if ($(this).data("href") && $(this).data("href") !== '') {
                                window.top.location.href = $(this).data("href");
                            }
                        });

                        // js for when a folder is selected
                        $('.js-folder-treeview .treeview').off('rockTree:selected');
                        $('.js-folder-treeview .treeview').on('rockTree:selected', function (e, data) {
                            var relativeFolderPath = data;
                            $('#<%=hfSelectedFolder.ClientID%>').val(data);
                            // use setTimeout so that the doPostBack happens later (to avoid javascript exception that occurs due to timing)
                            setTimeout(function () {
                                var postbackArg = 'folder-selected:' + relativeFolderPath.replace(/\\/g, "/").replace(/'/g, "\\'");
                                window.location = "javascript:__doPostBack('<%=upnlFiles.ClientID %>', '" + postbackArg + "')";
                            });
                        });

                        // js for when a file is selected
                        $('.js-file-list .js-listview').off('rockList:selected');
                        $('.js-file-list .js-listview').on('rockList:selected', function (e, data) {
                            // do an ajax call back to the current url to get the image src and alt
                            // we want the server to figure this out in order to minimize exposing how the rootfolder is encrypted
                            $.ajax({
                                url: window.location + '&getSelectedFileResult=true',
                                type: 'POST',
                                data: {
                                    selectedFileId: data
                                },
                                context: this
                            }).done(function (returnData) {
                                $('#<%=hfResultValue.ClientID%>').val(returnData);
                                });

                            var selectedFileId = data;
                        });

                        // disable/hide actions depending on if a folder is selected
                        var isRestrictedFolder = $('#<%=hfIsRestrictedFolder.ClientID%>').val();
                        var selectedFolderPath = $('#<%=hfSelectedFolder.ClientID%>').val();
                        var isUploadRestrictedFolder = $('#<%=hfIsUploadRestrictedFolder.ClientID%>').val();

                        if (selectedFolderPath && selectedFolderPath != '' && isRestrictedFolder === 'False') {
                            $('#<%=lbRenameFolder.ClientID%>').removeAttr('disabled');
                            $('#<%=lbMoveFolder.ClientID%>').removeAttr('disabled');
                            $('#<%=lbDeleteFolder.ClientID%>').removeAttr('disabled');
                        }
                        else {
                            $('#<%=lbRenameFolder.ClientID%>').attr('disabled', 'disabled');
                            $('#<%=lbMoveFolder.ClientID%>').attr('disabled', 'disabled');
                            $('#<%=lbDeleteFolder.ClientID%>').attr('disabled', 'disabled');
                        }

                        if (selectedFolderPath && selectedFolderPath != '' && isUploadRestrictedFolder === 'False') {
                            $('#<%=lbArchive.ClientID%>').removeAttr('disabled');
                        }
                        else {
                            $('#<%=lbArchive.ClientID%>').attr('disabled', 'disabled');
                        }
                    });

                </script>
            </ContentTemplate>
        </asp:UpdatePanel>

        <%-- Files and Modals --%>
        <asp:UpdatePanel ID="upnlFiles" class="picker-files" runat="server">
            <ContentTemplate>
                <Rock:NotificationBox ID="nbErrorMessage" runat="server" NotificationBoxType="Danger" Text="Error..." Visible="true" Title="Error" Dismissable="true" />

                <Rock:ModalDialog runat="server" Title="Rename Folder" ID="mdRenameFolder" OnSaveClick="mdRenameFolder_SaveClick" ValidationGroup="vgRenameFolder" ScrollbarEnabled="false">
                    <Content>
                        <div class="row">
                            <div class="col-md-12 margin-b-sm">
                                <Rock:TermDescription runat="server"  ID="tbOrigFolderName" Term="Current Location" />
                            </div>
                        </div>
                        <Rock:RockTextBox runat="server" ID="tbRenameFolderName" Label="New Folder Name" Required="true" ValidationGroup="vgRenameFolder" />
                    </Content>
                </Rock:ModalDialog>

                <Rock:ModalDialog runat="server" Title="Move Folder" ID="mdMoveFolder" OnSaveClick="mdMoveFolder_SaveClick" ValidationGroup="vgMoveFolder" ScrollbarEnabled="false">
                    <Content>
                        <div class="row">
                            <div class="col-md-12 margin-b-sm">
                                <Rock:TermDescription runat="server"  ID="tbMoveOrigFolderName" Term="Current Location" />
                            </div>
                        </div>
                        <Rock:RockDropDownList runat="server" ID="ddlMoveFolderTarget" Label="Move To Folder" Required="true" ValidationGroup="vgMoveFolder" EnhanceForLongLists="true" />
                    </Content>
                </Rock:ModalDialog>

                <Rock:ModalDialog runat="server" Title="Create Folder" ID="mdCreateFolder" OnSaveClick="mdCreateFolder_SaveClick" ValidationGroup="vgCreateFolder" ScrollbarEnabled="false">
                    <Content>
                        <!-- prevent carriage return from making mdRenameFolder popup when you press enter( on FF and Chrome) -->
                        <Rock:RockTextBox runat="server" ID="tbNewFolderName" Label="New Folder Name" Required="true" ValidationGroup="vgCreateFolder" onkeypress="if (event.keyCode == 13) { event.preventDefault(); }" />
                    </Content>
                </Rock:ModalDialog>

                <Rock:ModalDialog runat="server" Title="Archive Files" ID="mdArchive" OnSaveClick="mdArchive_SaveClick" ValidationGroup="vgArchiveFiles" ScrollbarEnabled="false">
                    <Content>
                        <div class="row">
                            <div class="col-md-4">
                            <Rock:FileUploader ID="fupZipUpload" runat="server" Label="Select Zip Upload" Required="true" FormGroupCssClass="zip-upload" IsBinaryFile="false" ValidationGroup="vgArchiveFiles" RootFolder="~/App_Data"  />
                            </div>
                        </div>
                    </Content>
                </Rock:ModalDialog>

                <asp:HiddenField ID="hfSelectedFolder" runat="server" />
                <asp:HiddenField ID="hfIsRestrictedFolder" runat="server" />
                <asp:HiddenField ID="hfIsUploadRestrictedFolder" runat="server" />
                <div class="actions assetmanager-actions">

                    <div class="pull-right"><Rock:FileUploader ID="fuprFileUpload" runat="server" IsBinaryFile="false" DisplayMode="Button" /></div>
                    <div class="pull-right">
                    <asp:LinkButton ID="lbArchive" runat="server" CssClass="btn btn-default btn-sm margin-r-sm" OnClientClick="if ($(this).attr('disabled') == 'disabled') { return false; }" OnClick="lbArchive_Click" CausesValidation="false" ToolTip="Archive"><i class="fa fa-file-archive"></i> Upload Zip</asp:LinkButton>
                    </div>
                </div>

                <div>
                    <div class="scroll-container scroll-container-vertical scroll-container-picker js-file-list">
                        <div class="scrollbar">
                            <asp:Panel ID="pnlListTrack" runat="server" CssClass="track">
                                <div class="thumb">
                                    <div class="end"></div>
                                </div>
                            </asp:Panel>
                        </div>
                        <asp:Panel ID="pnlListViewPort" runat="server" CssClass="viewport">
                            <div class="overview">
                                <asp:Label ID="lbNoFilesFound" runat="server" Visible="false" Text="This folder is empty" CssClass="text-muted padding-all-md margin-v-md empty-folder-notification" />
                                <asp:Label ID="lblFiles" CssClass="js-listview" runat="server" />
                            </div>
                        </asp:Panel>
                    </div>
                </div>

                <div class="js-filebrowser-result">
                    <asp:HiddenField ID="hfResultValue" runat="server" />
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

    <asp:Panel ID="pnlModalFooterActions" CssClass="modal-footer" runat="server" Visible="false">
        <div class="row">
            <div class="actions">
                <a class="btn btn-primary js-select-file-button">OK</a>
                <a class="btn btn-link js-cancel-file-button">Cancel</a>
            </div>
        </div>
    </asp:Panel>
</asp:Panel>
