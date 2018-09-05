<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AssetStorageSystemBrowser.ascx.cs" Inherits="RockWeb.Blocks.Core.AssetStorageSystemBrowser" %>

<asp:Panel ID="pnlAssetStorageSystemBrowser" runat="server" CssClass="picker-wrapper clearfix">

    <asp:UpdatePanel ID="upnlHiddenValues" runat="server" UpdateMode="Always" style="display:none">
        <ContentTemplate>
            <asp:Label ID="lbAssetStorageId" CssClass="js-assetstorage-id" runat="server"></asp:Label><br />
            <asp:Label ID="lbSelectFolder" CssClass="js-selectfolder" runat="server"></asp:Label><br />
            <asp:Label ID="lbExpandedFolders" CssClass="js-expandedFolders" runat="server"></asp:Label>
        </ContentTemplate>
    </asp:UpdatePanel>

    <div class="picker-folders js-pickerfolders">
        <asp:UpdatePanel ID="upnlFolders" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
            <ContentTemplate>
                <asp:HiddenField ID="hfScriptInitialized" runat="server" />
                
                <div class="actions row justify-content-center" style="display: flex; vertical-align:top;">
                    <div class="col-md-4">
                        <a href="#" class="btn btn-xs btn-default js-createfolder js-folderselect" title="Create a new folder in the selected folder" style="width:125px">
                            <i class="fa fa-folder"></i>
                            Add Folder
                        </a>
                    </div>
                    <div class="col-md-4">
                        <asp:LinkButton ID="lbDeleteFolder" runat="server" CssClass="btn btn-xs btn-default js-deletefolder js-folderselect" OnClick="lbDeleteFolder_Click" CausesValidation="false" ToolTip="Delete the selected folder" Width="125px">
                            <i class="fa fa-trash-alt"></i>
                            Delete Folder
                        </asp:LinkButton>
                    </div>
                </div>

                <asp:Panel runat="server" ID="pnlCreateFolder" CssClass="actions row js-createfolder-div" style="display: none;">
                    <div class="col-md-4">
                        <Rock:RockTextBox ID="tbCreateFolder" runat="server" CssClass="js-createfolder-input" />
                    </div>
                    <div class="col-md-2">
                        <asp:LinkButton ID="lbCreateFolderAccept" runat="server" CssClass="btn btn-xs btn-default" OnClick="lbCreateFolderAccept_Click" >
                            <i class="fa fa-check"></i>
                            Create Folder
                        </asp:LinkButton>
                    </div>
                    <div class="col-md-2">
                        <a href="#" class="btn btn-xs btn-default js-createfolder-cancel">
                            <i class="fa fa-times"></i>
                            Cancel
                        </a>
                    </div>
                    <div class="col-md-4"></div>
                </asp:Panel>
                <br />
                <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" Text="Folder not found" Visible="false" />
                <br />

                <div>
                    <div class="scroll-container scroll-container-vertical scroll-container-picker js-folder-treeview">
                        <div class="scrollbar">
                            <asp:Panel ID="pnlTreeTrack" runat="server" CssClass="track js-treetrack">
                                <div class="thumb">
                                    <div class="end"></div>
                                </div>
                            </asp:Panel>
                        </div>
                        <asp:Panel ID="pnlTreeViewPort" runat="server" CssClass="viewport js-treeviewport">
                            <div class="overview">
                                <asp:Label ID="lblFolders" CssClass="treeview treeview-items" runat="server" />
                            </div>
                        </asp:Panel>
                    </div>
                </div>

            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

    <div class="picker-files">
        <asp:UpdatePanel ID="upnlFiles" runat="server">
            <ContentTemplate>
                <asp:Panel ID="pnlFiles" runat="server" CssClass="js-files">
                    <div class="actions row justify-content-center" style="display: flex; vertical-align:top;">
                        <div class="col-md-2"><Rock:FileUploader ID="fupUpload" runat="server" CausesValidation="false" ToolTip="Upload a file to the selected location" IsBinaryFile="false" DisplayMode="DefaultButton" Enabled="false"/></div>
                        <div class="col-md-2"><asp:LinkButton ID="lbDownload" runat="server" CssClass="btn btn-xs btn-default js-singleselect aspNetDisabled" OnClick="lbDownload_Click" CausesValidation="false" ToolTip="Download the selected files" Width="100px" ><i class="fa fa-download"></i>Download</asp:LinkButton></div>
                        <div class="col-md-2">
                            <asp:LinkButton ID="lbRename" runat="server" CssClass="btn btn-xs btn-default js-singleselect js-renamefile aspNetDisabled" CausesValidation="false" ToolTip="Rename the selected file" Width="100px">
                                <i class="fa fa-exchange"></i>
                                Rename
                            </asp:LinkButton>
                        </div>
                        <div class="col-md-2"><asp:LinkButton ID="lbDelete" runat="server"  CssClass="btn btn-xs btn-default js-minselect aspNetDisabled" OnClick="lbDelete_Click" CausesValidation="false" ToolTip="Delete the selected files" OnClientClick="Rock.dialogs.confirmDelete(event, ' file')" Width="100px"><i class="fa fa-trash-alt"></i>Delete</asp:LinkButton></div>
                        <div class="col-md-2"><asp:LinkButton ID="lbRefresh" runat="server" CssClass="btn btn-xs btn-default js-assetselect" OnClick="lbRefresh_Click" CausesValidation="false" ToolTip="Refresh the file list" Width="100px"><i class="fa fa-sync"></i>Refresh</asp:LinkButton></div>

                    </div>

                    <div class="actions row js-renamefile-div" id="divRenameFile" style="display: none;">
                        <div class="col-md-2"></div>
                        <div class="col-md-2"></div>
                        <div class="col-md-4">
                            <Rock:RockTextBox ID="tbRenameFile" runat="server" CssClass="js-renamefile-input" />
                        </div>
                        <div class="col-md-2">
                            <asp:LinkButton ID="lbRenameFileAccept" runat="server" CssClass="btn btn-xs btn-default" OnClick="lbRenameFileAccept_Click" >
                                <i class="fa fa-check"></i>
                                Rename File
                            </asp:LinkButton>
                        </div>
                        <div class="col-md-2">
                            <a id="lbRenameFileCancel" href="#" class="btn btn-xs btn-default js-renamefile-cancel">
                                <i class="fa fa-times"></i>
                                Cancel
                            </a>
                        </div>
                    </div>
                    <br />
                    <Rock:NotificationBox ID="nbErrorMessage" runat="server" NotificationBoxType="Danger" Text="Error..." Visible="false" Title="Error" Dismissable="true" />
                    <br />

                        <asp:Repeater ID="rptFiles" runat="server">
                            <AlternatingItemTemplate>
                                <div class="row" style="background-color:#f2f2f2; display: flex; align-items: center;">
                                    <div class="col-md-1" ><Rock:RockCheckBox ID="cbSelected" runat="server" CssClass="js-checkbox" /></div>
                                    <%--<div class="col-md-1"><i class='<%# Eval("IconCssClass") %>'></i></div>--%>
                                    <div class="col-md-1" ><img src='<%# Eval("IconPath") %>' style='width: 24px; height: 24px;'></img></div>
                                    <div class="col-md-4" ><asp:Label ID="lbName" runat="server" Text='<%# Eval("Name") %>'></asp:Label></div>
                                    <div class="col-md-4" ><asp:Label ID="lbLastModified" runat="server" Text='<%# Eval("LastModifiedDateTime") %>'></asp:Label></div>
                                    <div class="col-md-2" ><asp:Label ID="lbFileSize" runat="server" Text='<%# Eval("FormattedFileSize") %>'></asp:Label></div>
                                    <asp:Label ID="lbKey" runat="server" Text='<%# Eval("Key") %>' Visible="false"></asp:Label>
                                </div>
                            </AlternatingItemTemplate>
                            <ItemTemplate>
                                <div class="row" style="display: flex; align-items: center;">
                                    <div class="col-md-1" ><Rock:RockCheckBox ID="cbSelected" runat="server" CssClass="js-checkbox" /></div>
                                    <%--<div class="col-md-1"><i class='<%# Eval("IconCssClass") %>'></i></div>--%>
                                    <div class="col-md-1" ><img src='<%# Eval("IconPath") %>' style='width: 24px; height: 24px;'></img></div>
                                    <div class="col-md-4" ><asp:Label ID="lbName" runat="server" Text='<%# Eval("Name") %>'></asp:Label></div>
                                    <div class="col-md-4" ><asp:Label ID="lbLastModified" runat="server" Text='<%# Eval("LastModifiedDateTime") %>'></asp:Label></div>
                                    <div class="col-md-2" ><asp:Label ID="lbFileSize" runat="server" Text='<%# Eval("FormattedFileSize") %>'></asp:Label></div>
                                    <asp:Label ID="lbKey" runat="server" Text='<%# Eval("Key") %>' Visible="false"></asp:Label>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>

                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

</asp:Panel>

