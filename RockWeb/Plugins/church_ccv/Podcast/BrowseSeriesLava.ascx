<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BrowseSeriesLava.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Podcast.BrowseSeriesLava" %>

<asp:UpdatePanel ID="upnlContent" runat="server" ChildrenAsTriggers="false" UpdateMode="Conditional">
    <ContentTemplate>
        
        <%-- View Panel --%>
        <asp:Panel ID="pnlView" runat="server">
            <asp:Literal ID="lContent" runat="server" Visible="true" />
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
