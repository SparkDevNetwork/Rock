<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AssetManager.ascx.cs" Inherits="RockWeb.Blocks.Cms.AssetManager" %>

<asp:Panel ID="pnlAssetManager" runat="server" CssClass="picker-wrapper clearfix">

    <asp:UpdatePanel ID="upnlHiddenValues" runat="server" UpdateMode="Always" style="display:none;" >
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
                
                <div class="actions">
                    <a href="#" class="btn btn-xs btn-default js-createfolder js-folderselect" title="Create a new folder in the selected folder">
                        <i class="fa fa-folder"></i> Add Folder
                    </a>
                    <asp:LinkButton ID="lbDeleteFolder" runat="server" CssClass="btn btn-xs btn-default js-deletefolder js-folderselect" OnClick="lbDeleteFolder_Click" CausesValidation="false" ToolTip="Delete the selected folder">
                        <i class="fa fa-trash-alt"></i> Delete Folder
                    </asp:LinkButton>
                </div>

                <asp:Panel runat="server" ID="pnlCreateFolder" CssClass="actions well well-sm clearfix js-createfolder-div" style="display: none;">
                    <div class="pull-left margin-r-md">
                        <Rock:RockTextBox ID="tbCreateFolder" runat="server" CssClass="js-createfolder-input input-sm" />
                    </div>
                    <div class="pull-left padding-v-sm">
                        <asp:LinkButton ID="lbCreateFolderAccept" runat="server" CssClass="btn btn-xs btn-default" OnClick="lbCreateFolderAccept_Click" >
                            <i class="fa fa-check"></i> Create Folder
                        </asp:LinkButton>

                        <a href="#" class="btn btn-xs btn-default js-createfolder-cancel">
                            <i class="fa fa-times"></i> Cancel
                        </a>
                    </div>
                </asp:Panel>
             
                <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" Text="Folder not found" Visible="false" CssClass="margin-v-md" />

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
                    <div class="actions">
                        <div class="pull-left">
                            <Rock:FileUploader ID="fupUpload" runat="server" CausesValidation="false" ToolTip="Upload a file to the selected location" IsBinaryFile="false" DisplayMode="DefaultButton" Enabled="false"/>&nbsp;
                        </div>
                        <asp:LinkButton ID="lbDownload" runat="server" CssClass="btn btn-xs btn-default js-singleselect aspNetDisabled" OnClick="lbDownload_Click" CausesValidation="false" ToolTip="Download selected file"><i class="fa fa-download"></i> Download</asp:LinkButton>
                        
                        <asp:LinkButton ID="lbRename" runat="server" CssClass="btn btn-xs btn-default js-singleselect js-renamefile aspNetDisabled" CausesValidation="false" ToolTip="Rename selected file" OnClientClick="return false;">
                            <i class="fa fa-exchange"></i> Rename
                        </asp:LinkButton>
                        
                        <asp:LinkButton ID="lbDelete" runat="server"  CssClass="btn btn-xs btn-default js-minselect aspNetDisabled" OnClick="lbDelete_Click" CausesValidation="false" ToolTip="Delete selected files" OnClientClick="Rock.dialogs.confirmDelete(event, ' file')"><i class="fa fa-trash-alt"></i> Delete</asp:LinkButton>
                        <asp:LinkButton ID="lbRefresh" runat="server" CssClass="btn btn-xs btn-default js-assetselect" OnClick="lbRefresh_Click" CausesValidation="false" ToolTip="Refresh"><i class="fa fa-sync"></i></asp:LinkButton>

                    </div>

                    <div class="actions well well-sm js-renamefile-div" id="divRenameFile" style="display: none;">
                        <div class="pull-left">
                            <Rock:RockTextBox ID="tbRenameFile" runat="server" CssClass="js-renamefile-input input-sm" />
                        </div>
                        
                        <asp:LinkButton ID="lbRenameFileAccept" runat="server" CssClass="btn btn-xs btn-default" OnClick="lbRenameFileAccept_Click" >
                            <i class="fa fa-check"></i> Rename File
                        </asp:LinkButton>
                        
                        <a id="lbRenameFileCancel" href="#" class="btn btn-xs btn-default js-renamefile-cancel">
                            <i class="fa fa-times"></i> Cancel
                        </a>
                    </div>
                    
                    <Rock:NotificationBox ID="nbErrorMessage" runat="server" NotificationBoxType="Danger" Text="Error..." Visible="false" Title="Error" Dismissable="true" CssClass="margin-v-md" />

                    <table class="table table-striped table-responsive table-no-border">
                        <asp:Repeater ID="rptFiles" runat="server">
                            <ItemTemplate>
                                <tr>
                                    <td><Rock:RockCheckBox ID="cbSelected" runat="server" CssClass="js-checkbox" /></td>
                                    <%--<div class="col-md-1"><i class='<%# Eval("IconCssClass") %>'></i></div>--%>
                                    <td><img src='<%# Eval("IconPath") %>' style='width: 24px; height: 24px;'></img></td>
                                    <td><asp:Label ID="lbName" runat="server" Text='<%# Eval("Name") %>'></asp:Label></td>
                                    <td data-priority="3"><asp:Label ID="lbLastModified" runat="server" Text='<%# Eval("LastModifiedDateTime") %>'></asp:Label></td>
                                    <td><asp:Label ID="lbFileSize" runat="server" Text='<%# Eval("FormattedFileSize") %>'></asp:Label>
                                    <asp:Label ID="lbKey" runat="server" Text='<%# Eval("Key") %>' Visible="false"></asp:Label></td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </table>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

</asp:Panel>

