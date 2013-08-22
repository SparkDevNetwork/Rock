<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupRoleList.ascx.cs" Inherits="RockWeb.Blocks.Crm.GroupRoleList" %>

<asp:UpdatePanel ID="upGroupRoles" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        <Rock:Grid ID="gGroupRoles" runat="server" AllowSorting="true" OnRowSelected="gGroupRoles_Edit">
            <Columns>
                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                <asp:BoundField DataField="GroupTypeName" HeaderText="Group Type" SortExpression="GroupTypeName" />
                <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                <Rock:DeleteField OnClick="gGroupRoles_Delete" />
            </Columns>
        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>
