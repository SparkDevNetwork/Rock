<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SimpleEventList.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Calendar.SimpleEventList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server" CssClass="row">
            <asp:Panel ID="pnlList" CssClass="col-md-9" runat="server">
                <asp:Literal ID="lOutput" runat="server"></asp:Literal>
                <asp:Literal ID="lDebug" Visible="false" runat="server"></asp:Literal>
            </asp:Panel>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
