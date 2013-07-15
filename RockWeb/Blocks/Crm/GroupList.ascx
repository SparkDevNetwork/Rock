<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupList.ascx.cs" Inherits="RockWeb.Blocks.Crm.GroupList" %>

<asp:UpdatePanel ID="upGroupList" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        <Rock:Grid ID="gGroups" runat="server" AllowSorting="true" OnRowSelected="gGroups_Edit">
            <Columns>
                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                <asp:BoundField DataField="GroupTypeName" HeaderText="Group Type" SortExpression="GroupTypeName" />
                <asp:BoundField DataField="MembersCount" HeaderText="Members" SortExpression="MembersCount" />
                <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                <Rock:DeleteField OnClick="gGroups_Delete" />
            </Columns>
        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>
