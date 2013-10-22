<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RestControllerList.ascx.cs" Inherits="RockWeb.Blocks.Administration.RestControllerList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        <Rock:Grid ID="gItems" runat="server" AllowSorting="true" OnRowSelected="gItems_RowSelected">
            <Columns>
                <asp:BoundField DataField="ControllerName" HeaderText="Controller Name" SortExpression="ControllerName" />
                <asp:BoundField DataField="ControllerType" HeaderText="Controller Type" SortExpression="ControllerType" />
                <asp:BoundField DataField="Actions" HeaderText="Actions" SortExpression="Actions" />
            </Columns>
        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>
