<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MediaFolderList.ascx.cs" Inherits="RockWeb.Blocks.Cms.MediaFolderList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-play-circle"></i> <asp:Literal ID="lTitle" runat="server" />
                </h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfFilter" runat="server">
                        <Rock:RockTextBox ID="txtFolderName" runat="server" Label="Name" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gFolderList" runat="server" AllowSorting="true" OnRowSelected="gFolderList_RowSelected"  CssClass="js-grid-folders">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" ItemStyle-CssClass="js-name-folder" />
                            <Rock:RockBoundField DataField="ContentChannel.Name" HeaderText="Content Channel Sync" SortExpression="ContentChannel.Name" />
                            <Rock:RockBoundField DataField="Videos" HeaderText="Videos" SortExpression="Videos" />
                            <Rock:DeleteField OnClick="gFolderList_DeleteClick" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
