<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CkEditorFileBrowser.ascx.cs" Inherits="RockWeb.Blocks.Utility.CkEditorFileBrowser" %>

<table>
    <tr>
        <td>
            <%-- Folders - Separate UpdatePanel so that Tree doesn't get rebuilt on postbacks (unless the server explicity wants it to get rebuilt) --%>
            <asp:UpdatePanel ID="upnlFolders" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
                <ContentTemplate>
                    <asp:LinkButton ID="lbCreateFolder" runat="server" CssClass="btn btn-xs btn-action" Text="Create Folder" OnClick="lbCreateFolder_Click" CausesValidation="false" />
                    <asp:LinkButton ID="lbRenameFolder" runat="server" CssClass="btn btn-xs  btn-action" Text="Rename Folder" OnClick="lbRenameFolder_Click" CausesValidation="false" />
                    <asp:LinkButton ID="lbDeleteFolder" runat="server" CssClass="btn btn-xs btn-action" Text="Delete Folder" OnClientClick="Rock.dialogs.confirmDelete(event, 'folder and all its contents');" OnClick="lbDeleteFolder_Click" CausesValidation="false" />
                    <asp:LinkButton ID="lbRefresh" runat="server" CssClass="btn btn-xs  btn-action" Text="Refresh" OnClick="lbRefresh_Click" CausesValidation="false" />

                    <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" Text="Folder not found" Visible="false" />

                    <div style="height: 100%;">
                        <div class="scroll-container scroll-container-vertical scroll-container-picker js-folder-treeview">
                            <div class="scrollbar">
                                <div class="track">
                                    <div class="thumb">
                                        <div class="end"></div>
                                    </div>
                                </div>
                            </div>
                            <div class="viewport">
                                <div class="overview">
                                    <asp:Label ID="lblFolders" CssClass="treeview treeview-items" runat="server" />
                                </div>
                            </div>
                        </div>
                    </div>
                    </div>
                    <script type="text/javascript">

                        // init scroll bars for folder and file list divs
                        $('.js-folder-treeview').tinyscrollbar({ size: 120, sizethumb: 20 });

                        $('.js-folder-treeview .treeview').on('rockTree:expand rockTree:collapse rockTree:dataBound rockTree:rendered', function (evt) {
                            // update the folder treeview scroll bar
                            $('.js-folder-treeview').tinyscrollbar_update('relative');
                        });

                        Sys.Application.add_load(function () {

                            var folderTreeData = $('.js-folder-treeview .treeview').data('rockTree');

                            // init the folder list treeview if it hasn't been created already
                            if (!folderTreeData) {
                                var selectedFolders = $('#<%=hfSelectedFolder.ClientID%>').val().split(',');
                                // init rockTree on folder (no url option since we are generating off static html)
                                $('.js-folder-treeview .treeview').rockTree({
                                    selectedIds: selectedFolders
                                });
                            }

                            // init the file list treeview on every load
                            $('.js-file-list .treeview').rockTree({});
                            $('.js-file-list').tinyscrollbar({ size: 120, sizethumb: 20 });

                            // js for when a file delete is clicked
                            $('.js-file-list .js-delete-file').off('click');
                            $('.js-file-list .js-delete-file').on('click', function (e, data) {
                                var selectedFileId = $(this).closest('li').attr('data-id');
                                Rock.dialogs.confirm("Are you sure you want to delete this file?", function (confirmResult, data) {
                                    if (confirmResult) {
                                        __doPostBack('<%=upnlFiles.ClientID %>', 'file-delete:' + selectedFileId + '');
                                    }
                                });
                            });

                            // js for when a folder is selected
                            $('.js-folder-treeview .treeview').off('rockTree:selected');
                            $('.js-folder-treeview .treeview').on('rockTree:selected', function (e, data) {
                                var relativeFolderPath = data;
                                $('#<%=hfSelectedFolder.ClientID%>').val(data);
                            __doPostBack('<%=upnlFiles.ClientID %>', 'folder-selected:' + relativeFolderPath + '');
                            });

                            // js for when a file is selected
                            $('.js-file-list .treeview').off('rockTree:selected');
                            $('.js-file-list .treeview').on('rockTree:selected', function (e, data) {
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
                            var selectedFolderPath = $('#<%=hfSelectedFolder.ClientID%>').val();
                            if (selectedFolderPath && selectedFolderPath != '') {
                                $('#<%=lbRenameFolder.ClientID%>').removeAttr('disabled');
                                $('#<%=lbDeleteFolder.ClientID%>').removeAttr('disabled');
                                $('#<%=fuprFileUpload.ClientID%>').css('visibility', 'visible');
                            }
                            else {
                                $('#<%=lbRenameFolder.ClientID%>').attr('disabled', 'disabled');
                                $('#<%=lbDeleteFolder.ClientID%>').attr('disabled', 'disabled');
                                $('#<%=fuprFileUpload.ClientID%>').css('visibility', 'hidden');
                            }
                        });

                    </script>
                </ContentTemplate>
            </asp:UpdatePanel>
        </td>

        <td>
            <%-- Files and Modals --%>
            <asp:UpdatePanel ID="upnlFiles" runat="server">
                <ContentTemplate>
                    <Rock:ModalDialog runat="server" ID="mdError">
                        <Content>
                            <Rock:NotificationBox ID="nbErrorMessage" runat="server" NotificationBoxType="Danger" Text="Error..." Visible="true" Title="Error" />
                        </Content>
                    </Rock:ModalDialog>

                    <Rock:ModalDialog runat="server" Title="Rename Folder" ID="mdRenameFolder" OnSaveClick="mdRenameFolder_SaveClick" ValidationGroup="vgRenameFolder">
                        <Content>
                            <Rock:RockTextBox runat="server" ID="tbOrigFolderName" Label="Folder Name" ReadOnly="true" />
                            <Rock:RockTextBox runat="server" ID="tbRenameFolderName" Label="New Folder Name" Required="true" ValidationGroup="vgRenameFolder" />
                        </Content>
                    </Rock:ModalDialog>

                    <Rock:ModalDialog runat="server" Title="Create Folder" ID="mdCreateFolder" OnSaveClick="mdCreateFolder_SaveClick" ValidationGroup="vgCreateFolder">
                        <Content>
                            <Rock:RockTextBox runat="server" ID="tbNewFolderName" Label="New Folder Name" Required="true" ValidationGroup="vgCreateFolder" />
                        </Content>
                    </Rock:ModalDialog>

                    <asp:HiddenField ID="hfSelectedFolder" runat="server" />
                    <Rock:FileUploader ID="fuprFileUpload" runat="server" IsBinaryFile="false" DisplayMode="Button" />

                    <div>
                        <div class="scroll-container scroll-container-vertical scroll-container-picker js-file-list">
                            <div class="scrollbar">
                                <div class="track">
                                    <div class="thumb">
                                        <div class="end"></div>
                                    </div>
                                </div>
                            </div>
                            <div class="viewport">
                                <div class="overview">
                                    <Rock:NotificationBox ID="nbNoFilesInfo" runat="server" Text="No Files Found" Visible="false" NotificationBoxType="Info" />
                                    <asp:Label ID="lblFiles" CssClass="treeview treeview-items" runat="server" />
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="js-filebrowser-result">
                        <asp:HiddenField ID="hfResultValue" runat="server" />
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </td>
    </tr>
</table>

