<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RestKeyList.ascx.cs" Inherits="RockWeb.Blocks.Security.RestKeyList" %>

<asp:UpdatePanel ID="upnlRestKeys" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Danger" Visible="true" />
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <asp:Panel ID="pnlRestKeyList" runat="server">
            
            <div class="grid">
                <Rock:Grid ID="gRestKeyList" runat="server" EmptyDataText="No Rest Keys Found" AllowSorting="true" OnRowDataBound="gRestKeyList_RowDataBound" ShowConfirmDeleteDialog="true" OnRowSelected="gRestKeyList_RowSelected">
                    <Columns>
                        <asp:BoundField DataField="LastName" HeaderText="Name" SortExpression="LastName" />
                        <asp:TemplateField>
                            <HeaderTemplate>Description</HeaderTemplate>
                            <ItemTemplate>
                                <asp:Label ID="lblDescription" runat="server" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField>
                            <HeaderTemplate>Key</HeaderTemplate>
                            <ItemTemplate>
                                <asp:Label ID="lblKey" runat="server" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <Rock:DeleteField OnClick="gRestKeyList_Delete" />
                    </Columns>
                </Rock:Grid>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
