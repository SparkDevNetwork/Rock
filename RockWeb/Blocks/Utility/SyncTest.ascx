<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SyncTest.ascx.cs" Inherits="RockWeb.Blocks.Utility.SyncTest" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Button ID="btnGo" runat="server" Text="Go" OnClick="btnGo_Click" />

    </ContentTemplate>
</asp:UpdatePanel>
