<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CkEditorFileBrowser.ascx.cs" Inherits="RockWeb.Blocks.Utility.CkEditorFileBrowser" %>

<div class="ck-folders">
    <%-- Folders - Separate UpdatePanel so that Tree doesn't get rebuilt on postbacks (unless the server explicity wants it to get rebuilt) --%>
    <asp:UpdatePanel ID="upnlFolders" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
        <ContentTemplate>

            <div class="actions">
                <asp:LinkButton ID="lbCreateFolder" runat="server" CssClass="btn btn-sm btn-action" OnClick="lbCreateFolder_Click" CausesValidation="false" ToolTip="New Folder"><i class="fa fa-plus"></i></asp:LinkButton>
                <asp:LinkButton ID="lbRenameFolder" runat="server" CssClass="btn btn-sm  btn-action" OnClick="lbRenameFolder_Click" CausesValidation="false" ToolTip="Rename Folder"><i class="fa fa-pencil"></i></asp:LinkButton>
                <asp:LinkButton ID="lbDeleteFolder" runat="server" CssClass="btn btn-sm btn-action" OnClientClick="Rock.dialogs.confirmDelete(event, 'folder and all its contents');" OnClick="lbDeleteFolder_Click" CausesValidation="false" ToolTip="Delete Folder"><i class="fa fa-times"></i></asp:LinkButton>
                <asp:LinkButton ID="lbRefresh" runat="server" CssClass="btn btn-sm  btn-action" OnClick="lbRefresh_Click" CausesValidation="false" ToolTip="Refresh"><i class="fa fa-refresh"></i></asp:LinkButton>
            </div>

            <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" Text="Folder not found" Visible="false" />

            <div>
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
                        $('.js-folder-treeview').tinyscrollbar({ size: 120, sizethumb: 20 });

                        $('.js-folder-treeview .treeview').on('rockTree:expand rockTree:collapse rockTree:dataBound rockTree:rendered', function (evt) {
                            // update the folder treeview scroll bar
                            $('.js-folder-treeview').tinyscrollbar_update('relative');
                        });
                    }

                    // init the file list RockList on every load
                    $('.js-file-list .js-listview').rockList();
                    $('.js-file-list').tinyscrollbar({ size: 120, sizethumb: 20 });

                    // js for when a file delete is clicked
                    $('.js-file-list .js-delete-file').off('click');
                    $('.js-file-list .js-delete-file').on('click', function (e, data) {
                        var selectedFileId = $(this).closest('li').attr('data-id');
                        Rock.dialogs.confirm("Are you sure you want to delete this file?", function (confirmResult, data) {
                            if (confirmResult) {
                                // use setTimeout so that the doPostBack happens later (to avoid javascript exception that occurs due to timing)
                                setTimeout(function () {
                                    __doPostBack('<%=upnlFiles.ClientID %>', 'file-delete:' + selectedFileId + '');
                                });
                            }
                        });
                    });

                    // js for when a folder is selected
                    $('.js-folder-treeview .treeview').off('rockTree:selected');
                    $('.js-folder-treeview .treeview').on('rockTree:selected', function (e, data) {
                        var relativeFolderPath = data;
                        $('#<%=hfSelectedFolder.ClientID%>').val(data);
                        // use setTimeout so that the doPostBack happens later (to avoid javascript exception that occurs due to timing)
                        setTimeout(function () {
                            __doPostBack('<%=upnlFiles.ClientID %>', 'folder-selected:' + relativeFolderPath + '');
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
                    var selectedFolderPath = $('#<%=hfSelectedFolder.ClientID%>').val();
                    if (selectedFolderPath && selectedFolderPath != '') {
                        $('#<%=lbRenameFolder.ClientID%>').removeAttr('disabled');
                        $('#<%=lbDeleteFolder.ClientID%>').removeAttr('disabled');
                    }
                    else {
                        $('#<%=lbRenameFolder.ClientID%>').attr('disabled', 'disabled');
                        $('#<%=lbDeleteFolder.ClientID%>').attr('disabled', 'disabled');
                    }
                });

            </script>
        </ContentTemplate>
    </asp:UpdatePanel>
</div>

<div class="ck-files">
    <%-- Files and Modals --%>
    <asp:UpdatePanel ID="upnlFiles" runat="server">
        <ContentTemplate>
            <Rock:NotificationBox ID="nbErrorMessage" runat="server" NotificationBoxType="Danger" Text="Error..." Visible="true" Title="Error" Dismissable="true" />

            <Rock:ModalDialog runat="server" Title="Rename Folder" ID="mdRenameFolder" OnSaveClick="mdRenameFolder_SaveClick" ValidationGroup="vgRenameFolder" ScrollbarEnabled="false" >
                <Content>
                    <Rock:RockTextBox runat="server" ID="tbOrigFolderName" Label="Folder Name" ReadOnly="true" />
                    <Rock:RockTextBox runat="server" ID="tbRenameFolderName" Label="New Folder Name" Required="true" ValidationGroup="vgRenameFolder"  />
                </Content>
            </Rock:ModalDialog>

            <Rock:ModalDialog runat="server" Title="Create Folder" ID="mdCreateFolder" OnSaveClick="mdCreateFolder_SaveClick" ValidationGroup="vgCreateFolder" ScrollbarEnabled="false">
                <Content>
                    <!-- prevent carriage return from making mdRenameFolder popup when you press enter( on FF and Chrome) -->
                    <Rock:RockTextBox runat="server" ID="tbNewFolderName" Label="New Folder Name" Required="true" ValidationGroup="vgCreateFolder" onkeypress="if (event.keyCode == 13) { event.preventDefault(); }"  />
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
                            <asp:Label ID="lblFiles" CssClass="js-listview" runat="server" />
                        </div>
                    </div>
                </div>
            </div>

            <div class="js-filebrowser-result">
                <asp:HiddenField ID="hfResultValue" runat="server" />
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</div>
