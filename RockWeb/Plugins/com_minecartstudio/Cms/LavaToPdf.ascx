<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LavaToPdf.ascx.cs" Inherits="RockWeb.Plugins.com_mineCartStudio.Cms.LavaToPdf" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Literal ID="lMessages" runat="server" />

        <iframe id="ifDocumentPreview" runat="server" style="width: 100%; height: 400px;" />
                
        <asp:Literal ID="lDebug" runat="server" />

    </ContentTemplate>
</asp:UpdatePanel>

