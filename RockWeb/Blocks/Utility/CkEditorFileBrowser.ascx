<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CkEditorFileBrowser.ascx.cs" Inherits="RockWeb.Blocks.Utility.CkEditorFileBrowser" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
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

        <%-- 
        <div class="row">
            <div class="col-md-2">
            </div>

            <div class="col-md-4">
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
                            <asp:Literal ID="lFiles" runat="server" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
        --%>

        <div class="js-filebrowser-result">
            <asp:HiddenField ID="hfResultValue" runat="server" />
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
