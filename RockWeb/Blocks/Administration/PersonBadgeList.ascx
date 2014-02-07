<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonBadgeList.ascx.cs" Inherits="RockWeb.Blocks.Administration.PersonBadgeList" %>

<asp:UpdatePanel ID="upPersonBadge" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        
        <Rock:Grid ID="gPersonBadge" runat="server" AllowSorting="false" OnRowSelected="gPersonBadge_Edit">
            <Columns>
                <Rock:ReorderField />
                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                <Rock:SecurityField TitleField="Name" />
                <Rock:DeleteField OnClick="gPersonBadge_Delete" />
            </Columns>
        </Rock:Grid>

    </ContentTemplate>
</asp:UpdatePanel>
