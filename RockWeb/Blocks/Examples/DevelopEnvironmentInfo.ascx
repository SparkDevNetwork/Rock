<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DevelopEnvironmentInfo.ascx.cs" Inherits="RockWeb.Blocks.Examples.DevelopEnvironmentInfo" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="">
            <pre><asp:Literal ID="lDatabaseName" runat="server" /><asp:Literal ID="lHostingEnvironment" runat="server" />
                 <asp:LinkButton ID="btnShutdown" runat="server" CssClass="btn btn-danger btn-xs pull-right" Text="Restart" OnClick="btnShutdown_Click"/></pre>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
