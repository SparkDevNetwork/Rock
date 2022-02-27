<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CheckInStateDebugger.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.CheckIn.CheckInStateDebugger" %>
<asp:UpdatePanel ID="upnlContent" runat="server" Visible="false">
    <ContentTemplate>
        <div class="alert alert-info margin-t-lg">
            <asp:Literal ID="lDebugBrief" runat="server"></asp:Literal>
            <asp:Literal ID="lDebugFull" runat="server"></asp:Literal>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
