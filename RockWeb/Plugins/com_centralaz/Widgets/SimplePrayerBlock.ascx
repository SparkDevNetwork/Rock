<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SimplePrayerBlock.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Widgets.SimplePrayerBlock" %>
<asp:Panel ID="upAdd" runat="server" DefaultButton="lbComReq">
    <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
        <div class="input-group">
            <input type="text" ID="dtbRequest" class="form-control" runat="server" placeholder="Please pray that...">
            <span class="input-group-btn">
            <asp:LinkButton class="btn btn-default" type="button" id="lbComReq" runat="server" onclick="btnComplete_Click" CssClass="btn btn-default">&gt;</asp:LinkButton>
            </span>
        </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Panel>