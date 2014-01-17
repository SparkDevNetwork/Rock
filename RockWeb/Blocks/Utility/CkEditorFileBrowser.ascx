<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CkEditorFileBrowser.ascx.cs" Inherits="RockWeb.Blocks.Utility.CkEditorFileBrowser" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="row">
            <div class="col-md-6">
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

            <div class="col-md-6">
                <div class="scroll-container scroll-container-vertical scroll-container-picker">
                    <div class="scrollbar">
                        <div class="track">
                            <div class="thumb">
                                <div class="end"></div>
                            </div>
                        </div>
                    </div>
                    <div class="viewport">
                        <div class="overview">
                            TODO: Files
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div class="js-filebrowser-result">
            <asp:HiddenField ID="hfResultValue" runat="server" />
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
