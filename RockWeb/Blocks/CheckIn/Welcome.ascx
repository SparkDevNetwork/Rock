<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Welcome.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Welcome" %>
<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <asp:PlaceHolder ID="phScript" runat="server"></asp:PlaceHolder>

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <div class="row-fluid">
        <div class="span8">

            <span style="display:none">
                <asp:LinkButton ID="lbRefresh" runat="server" OnClick="lbRefresh_Click"></asp:LinkButton>
                <asp:Label ID="lblActiveWhen" runat="server" CssClass="active-when" />
            </span>

            <asp:Panel ID="pnlNotActive" runat="server">
                <h2>Not Active</h2>
                <p>There are no current or future schedules for this kiosk!</p>
            </asp:Panel>

            <asp:Panel ID="pnlNotActiveYet" runat="server">
                <h2>Not Active Yet</h2>
                <p>This kiosk is not active yet.  Countdown until active: <span class="countdown-timer"></span>  </p>
                <asp:HiddenField ID="hfActiveTime" runat="server" />
            </asp:Panel>

            <asp:Panel ID="pnlClosed" runat="server">
                <h2>Location Closed</h2>
                <p>This location is currently closed!</p>
            </asp:Panel>

            <asp:Panel ID="pnlActive" runat="server">
                <h2>Welcome!</h2>
                <div class="actions">
                    <asp:LinkButton CssClass="btn btn-primary" ID="lbSearch" runat="server" OnClick="lbSearch_Click" Text="Search"/>
                </div>
            </asp:Panel>

        </div>
        <div class="span4">

            <h3>Current Counts</h3>
            
            <asp:PlaceHolder ID="phCounts" runat="server"></asp:PlaceHolder>

        </div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>
