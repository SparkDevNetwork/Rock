<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PageRouteList.ascx.cs" Inherits="RockWeb.Blocks.Administration.PageRouteList" %>

<asp:UpdatePanel ID="upPageRoutes" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        <Rock:Grid ID="gPageRoutes" runat="server" AllowSorting="true" OnRowSelected="gPageRoutes_Edit">
            <Columns>
                <asp:BoundField DataField="Route" HeaderText="Route" SortExpression="Route" />
                <asp:BoundField DataField="PageName" HeaderText="Page Name" SortExpression="PageName" />
                <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                <Rock:DeleteField OnClick="gPageRoutes_Delete" />
            </Columns>
        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>


