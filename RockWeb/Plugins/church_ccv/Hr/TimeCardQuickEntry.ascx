<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TimeCardQuickEntry.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Hr.TimeCardQuickEntry" %>
<script runat="server">


</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfTimeCardId" runat="server" />

        <asp:Label ID="lblCurrentTimePeriod" runat="server" Text=""></asp:Label><br />

        <asp:Label ID="lblClockedStatus" runat="server" Text=""></asp:Label>
        
        <asp:LinkButton ID="lbClockIn" runat="server" CssClass="btn btn-primary btn-block margin-b-sm" OnClick="lbClockIn_Click">Clock In</asp:LinkButton>

        <asp:LinkButton ID="lbLunchOut" runat="server" CssClass="btn btn-primary btn-block margin-b-sm" OnClick="lbLunchOut_Click" Visible="false">Lunch Out</asp:LinkButton>

        <asp:LinkButton ID="lbLunchIn" runat="server" CssClass="btn btn-primary btn-block margin-b-sm" OnClick="lbLunchIn_Click" Visible="false">Lunch In</asp:LinkButton>

        <asp:LinkButton ID="lbClockOut" runat="server" CssClass="btn btn-primary btn-block margin-b-sm" OnClick="lbClockOut_Click" Visible="false">Clock Out</asp:LinkButton>

        <br />


    </ContentTemplate>
</asp:UpdatePanel>
