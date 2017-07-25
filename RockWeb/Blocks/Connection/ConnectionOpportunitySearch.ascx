<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConnectionOpportunitySearch.ascx.cs" Inherits="RockWeb.Blocks.Connection.OpportunitySearch" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="maWarning" runat="server" />
        
        <asp:Panel ID="pnlSearch" runat="server">
            <h3>Search</h3>

            <Rock:RockTextBox ID="tbSearchName" runat="server" Label="Name" />

            <Rock:RockCheckBoxList ID="cblCampus" runat="server" Label="Campuses" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" />

            <asp:PlaceHolder ID="phAttributeFilters" runat="server" />

            <Rock:BootstrapButton ID="btnSearch" CssClass="btn btn-primary" runat="server" OnClick="btnSearch_Click" Text="Search" />

        </asp:Panel>

        <asp:Literal ID="lOutput" runat="server"></asp:Literal>

        <asp:Literal ID="lDebug" Visible="false" runat="server"></asp:Literal>

    </ContentTemplate>
</asp:UpdatePanel>
