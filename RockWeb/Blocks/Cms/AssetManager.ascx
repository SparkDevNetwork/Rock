<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AssetManager.ascx.cs" Inherits="RockWeb.Blocks.Cms.AssetManager" %>

<asp:Panel ID="pnlAssetManager" runat="server" CssClass="picker-wrapper clearfix">
    <div class="picker-folders js-pickerfolders">
        <asp:UpdatePanel ID="upnlFolders" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
            <ContentTemplate>
                <asp:HiddenField ID="hfScriptInitialized" runat="server" />

                <%-- Hidden Field to store the selected value that was set to, in case a new value wasn't selected--%>
                <asp:HiddenField ID="hfSelectedValue" runat="server" />

                <Rock:HiddenFieldWithClass ID="hfAssetStorageId" CssClass="js-assetstorage-id" runat="server" />
                <Rock:HiddenFieldWithClass ID="hfSelectFolder" CssClass="js-selectfolder" runat="server" />
                <Rock:HiddenFieldWithClass ID="hfExpandedFolders" CssClass="js-expandedFolders" runat="server" />
                 <Rock:HiddenFieldWithClass ID="hfIsRoot" CssClass="js-isroot" runat="server" />

                <div class="actions btn-group">
                    <a href="#" class="btn btn-xs btn-default js-createfolder" title="Create a new folder in the selected folder">
                        <i class="fa fa-folder-plus"></i> <span class="hidden-xs">Add Folder</span>
                    </a>
                    <asp:LinkButton ID="lbDeleteFolder" runat="server" CssClass="btn btn-xs btn-default js-deletefolder" OnClick="lbDeleteFolder_Click" CausesValidation="false" ToolTip="Delete the selected folder" >
                        <i class="fa fa-trash-alt"></i> <span class="hidden-xs">Delete Folder</span>
                    </asp:LinkButton>
                </div>

                <asp:Panel runat="server" ID="pnlCreateFolder" CssClass="actions well well-sm clearfix js-createfolder-div" style="display: none;">
                    <div class="pull-left margin-r-md">
                        <Rock:RockTextBox ID="tbCreateFolder" runat="server" CssClass="js-createfolder-input input-sm" />
                    </div>
                    <div class="pull-left padding-v-sm">
                        <asp:LinkButton ID="lbCreateFolderAccept" runat="server" CssClass="btn btn-xs btn-default js-createfolder-accept" OnClick="lbCreateFolderAccept_Click" OnClientClick="return Rock.controls.assetManager.createFolderAccept_click();" >
                            <i class="fa fa-check"></i> Create Folder
                        </asp:LinkButton>

                        <a href="#" class="btn btn-xs btn-default js-createfolder-cancel">
                            <i class="fa fa-times"></i> Cancel
                        </a>
                    </div>
                    <label class="js-createfolder-notification alert alert-warning" style="display:none"></label>
                </asp:Panel>

                <div>
                    <div class="treeview-scroll scroll-container scroll-container-horizontal scroll-container-picker js-folder-treeview">

                        <asp:Panel ID="pnlTreeViewPort" runat="server" CssClass="viewport js-treeviewport">
                            <div class="overview">
                                <asp:Label ID="lblFolders" CssClass="treeview treeview-items" runat="server" />
                            </div>
                        </asp:Panel>

                        <div class="scrollbar">
                            <asp:Panel ID="pnlTreeTrack" runat="server" CssClass="track js-treetrack">
                                <div class="thumb">
                                    <div class="end"></div>
                                </div>
                            </asp:Panel>
                        </div>

                    </div>
                </div>

            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

    <div class="picker-files">
        <asp:UpdatePanel ID="upnlFiles" runat="server">
            <ContentTemplate>

                <asp:Panel ID="pnlFiles" runat="server" CssClass="js-files">

                    <div class="actions assetmanager-actions">
                        <div class="pull-left">
                            <Rock:FileUploader ID="fupUpload" runat="server" CausesValidation="false" ToolTip="Upload a file to the selected location" IsBinaryFile="false" DisplayMode="DefaultButton" Enabled="false"/>&nbsp;
                        </div>

                        <div class="btn-group">
                            <asp:LinkButton ID="lbDownload" runat="server" CssClass="btn btn-xs btn-default js-singleselect aspNetDisabled" OnClick="lbDownload_Click" CausesValidation="false" ToolTip="Download selected file"><i class="fa fa-download"></i> <span class="hidden-xs">Download</span></asp:LinkButton>

                            <asp:LinkButton ID="lbRename" runat="server" CssClass="btn btn-xs btn-default js-singleselect js-renamefile aspNetDisabled" CausesValidation="false" ToolTip="Rename selected file" OnClientClick="return false;">
                                <i class="fa fa-i-cursor"></i> <span class="hidden-xs">Rename</span>
                            </asp:LinkButton>

                            <asp:LinkButton ID="lbDelete" runat="server"  CssClass="btn btn-xs btn-default js-minselect aspNetDisabled" OnClick="lbDelete_Click" CausesValidation="false" ToolTip="Delete selected files" OnClientClick="Rock.dialogs.confirmDelete(event, ' file')"><i class="fa fa-trash-alt"></i> <span class="hidden-xs">Delete</span></asp:LinkButton>
                        </div>
                        <asp:LinkButton ID="lbRefresh" runat="server" CssClass="btn btn-xs btn-default js-assetselect" OnClick="lbRefresh_Click" CausesValidation="false" ToolTip="Refresh"><i class="fa fa-sync"></i></asp:LinkButton>
                    </div>

                    <div class="actions well well-sm js-renamefile-div" id="divRenameFile" style="display: none;" runat="server">
                        <div class="pull-left">
                            <Rock:RockTextBox ID="tbRenameFile" runat="server" CssClass="js-renamefile-input input-sm"  />
                        </div>

                        <asp:LinkButton ID="lbRenameFileAccept" runat="server" CssClass="btn btn-xs btn-default js-renamefile-accept" OnClick="lbRenameFileAccept_Click" OnClientClick="return Rock.controls.assetManager.renameFileAccept_click();">
                            <i class="fa fa-check"></i> Rename File
                        </asp:LinkButton>

                        <a id="lbRenameFileCancel" href="#" class="btn btn-xs btn-default js-renamefile-cancel">
                            <i class="fa fa-times"></i> Cancel
                        </a>
                        <label class="js-renamefile-notification alert alert-warning clearfix" style="display:none" runat="server" id="lblRenameFileNotification"></label>
                    </div>

                    <table class="table table-striped table-responsive table-no-border assetmanager-files">
                        <asp:Repeater ID="rptFiles" runat="server">
                            <ItemTemplate>
                                <tr>
                                    <td><Rock:RockCheckBox ID="cbSelected" runat="server" CssClass="js-checkbox" /></td>
                                    <td><img src='<%# Eval("IconPath") %>' style='max-width:60px;'></td>
                                    <td>
                                        <asp:Label ID="lbName" runat="server" Text='<%# Eval("Name") %>' CssClass="align-middle js-assetManager-name"></asp:Label>
                                        <asp:Label ID="lbUrl" runat="server" Text='<%# Eval("Uri") %>' style="display: none" CssClass="js-assetManager-uri" ></asp:Label>
                                    </td>
                                    <td data-priority="3"><asp:Label ID="lbLastModified" runat="server" Text='<%# Eval("LastModifiedDateTime") %>'></asp:Label></td>
                                    <td>
                                        <asp:Label ID="lbFileSize" runat="server" Text='<%# Eval("FormattedFileSize") %>'></asp:Label>
                                        <asp:Label ID="lbKey" runat="server" Text='<%# Eval("Key") %>' style="display: none"></asp:Label>
                                    </td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                <asp:Label ID="lbNoFilesFound" runat="server" Visible='<%# rptFiles.Items.Count == 0 %>' Text="<tr><td>No files found.</td></tr>" CssClass="text-muted" />
                            </FooterTemplate>
                        </asp:Repeater>
                    </table>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

</asp:Panel>

