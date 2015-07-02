<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ExternalOpportunitySearch.ascx.cs" Inherits="RockWeb.Blocks.Involvement.ExternalOpportunitySearch" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="maWarning" runat="server" />
        <h3>Search</h3>

        <Rock:RockTextBox ID="tbSearchName" runat="server" Label="Name" />

        <asp:PlaceHolder ID="phAttributeFilters" runat="server" />

        <Rock:BootstrapButton ID="btnSearch" CssClass="btn btn-primary" runat="server" OnClick="btnSearch_Click" Text="Search" />

        <asp:Literal ID="lOutput" runat="server"></asp:Literal>

        <asp:Literal ID="lDebug" Visible="false" runat="server"></asp:Literal>

        </div>
    </ContentTemplate>
</asp:UpdatePanel>
