<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupTypeList.ascx.cs" Inherits="RockWeb.Blocks.Crm.GroupTypeList" %>

<asp:UpdatePanel ID="upGroupType" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        <Rock:Grid ID="gGroupType" runat="server" AllowSorting="true" OnRowSelected="gGroupType_Edit">
            <Columns>
                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                <asp:BoundField DataField="GroupsCount" HeaderText="Group Count" SortExpression="GroupsCount" />
                <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                <Rock:DeleteField OnClick="gGroupType_Delete" />
            </Columns>
        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>
