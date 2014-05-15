<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RestKeyList.ascx.cs" Inherits="RockWeb.Blocks.Security.RestKeyList" %>

<asp:UpdatePanel ID="upnlRestKeys" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Danger" Visible="true" />
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <asp:Panel ID="pnlRestKeyList" runat="server">
            <Rock:Grid ID="gRestKeyList" runat="server" EmptyDataText="No Rest Keys Found" AllowSorting="true" OnRowDataBound="gRestKeyList_RowDataBound" ShowConfirmDeleteDialog="true" OnRowSelected="gRestKeyList_RowSelected">
                <Columns>
                    <asp:BoundField DataField="LastName" HeaderText="Name" SortExpression="LastName" />
                    <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                    <asp:BoundField DataField="ApiKey" HeaderText="Key" SortExpression="ApiKey" />
                    <Rock:DeleteField OnClick="gRestKeyList_Delete" />
                </Columns>
            </Rock:Grid>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
