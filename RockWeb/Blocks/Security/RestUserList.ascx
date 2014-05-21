<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RestUserList.ascx.cs" Inherits="RockWeb.Blocks.Security.RestUserList" %>

<asp:UpdatePanel ID="upnlRestUsers" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Danger" Visible="true" />
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <asp:Panel ID="pnlRestUserList" runat="server">
            <Rock:Grid ID="gRestUserList" runat="server" EmptyDataText="No Rest Users Found" AllowSorting="true" OnRowDataBound="gRestUserList_RowDataBound" ShowConfirmDeleteDialog="true" OnRowSelected="gRestUserList_RowSelected">
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
                    <Rock:DeleteField OnClick="gRestUserList_Delete" />
                </Columns>
            </Rock:Grid>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
