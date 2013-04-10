<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SiteList.ascx.cs" Inherits="RockWeb.Blocks.Administration.SiteList" %>

<asp:UpdatePanel ID="upSites" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        <Rock:Grid ID="gSites" runat="server" AllowSorting="true" OnRowSelected="gSites_Edit">
            <Columns>
                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                <asp:BoundField HeaderText="Description" DataField="Description" SortExpression="Description" />
                <asp:BoundField HeaderText="Theme" DataField="Theme" SortExpression="Theme" />
                <Rock:SecurityField TitleField="Name" />
                <Rock:DeleteField OnClick="gSites_Delete" />
            </Columns>
        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>

