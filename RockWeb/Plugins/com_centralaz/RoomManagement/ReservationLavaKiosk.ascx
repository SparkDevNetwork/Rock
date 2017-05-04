<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReservationLavaKiosk.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.RoomManagement.ReservationLavaKiosk" %>
<style>
    .updateprogress-status, .updateprogress-bg {
        display:none;
    }

</style>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbMessage" NotificationBoxType="Danger" runat="server" Visible="false" />
        <asp:Literal ID="lOutput" runat="server"></asp:Literal>
        <asp:Literal ID="lDebug" Visible="false" runat="server"></asp:Literal>
        <asp:Button ID="btnSubmit" runat="server" OnClick="btnSubmit_Click" style="display:none"/>
    </ContentTemplate>
</asp:UpdatePanel>

<script type="text/javascript">
window.onload = function () {
    setInterval(function () {
        document.getElementById("<%=btnSubmit.ClientID %>").click();
    }, 60000);
};
</script>