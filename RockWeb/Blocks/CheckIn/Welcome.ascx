<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Welcome.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Welcome" %>
<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <asp:PlaceHolder ID="phScript" runat="server"></asp:PlaceHolder>

    <Rock:ModalAlert ID="maWarning" runat="server" />


        <span style="display:none">
            <asp:LinkButton ID="lbRefresh" runat="server" OnClick="lbRefresh_Click"></asp:LinkButton>
            <asp:Label ID="lblActiveWhen" runat="server" CssClass="active-when" />
        </span>

        <%-- Panel for no schedules --%>
        <asp:Panel ID="pnlNotActive" runat="server">
            <div class="row checkin-header">
                <div class="col-md-12">
                    <h1>Checkin Not Active</h1>
                </div>
            </div>
                
            <div class="row checkin-body">
                <div class="col-md12">
                    <p>There are no current or future schedules for this kiosk!</p>
                </div>
            </div>
        </asp:Panel>

        <%-- Panel for schedule not active yet --%>
        <asp:Panel ID="pnlNotActiveYet" runat="server">
            <div class="row checkin-header">
                <div class="col-md-12">
                    <h1>Checkin Not Active Yet</h1>
                </div>
            </div>
                
            <div class="row checkin-body">
                <div class="col-md12">
                    <p>This kiosk is not active yet.  Countdown until active: <span class="countdown-timer"></span>  </p>
                    <asp:HiddenField ID="hfActiveTime" runat="server" />
                </div>
            </div>
        </asp:Panel>

        <%-- Panel for location closed --%>
        <asp:Panel ID="pnlClosed" runat="server">
            <div class="row checkin-header">
                <div class="col-md-12">
                    <h1>Location Closed</h1>
                </div>
            </div>
                
            <div class="row checkin-body">
                <div class="col-md-12">
                    <p>This location is currently closed.</p>
                </div>
            </div>
        </asp:Panel>

        <%-- Panel for active checkin --%>
        <asp:Panel ID="pnlActive" runat="server">
            <div class="row checkin-header">
                <div class="col-md-12">
                    <h1>Check-in Kiosk</h1>
                </div>
            </div>
                
            <div class="row checkin-body">
                <div class="col-md-12">
                    <div class="checkin-search-actions">
                        <asp:LinkButton CssClass="btn btn-primary" ID="lbSearch" runat="server" OnClick="lbSearch_Click" Text="Search By Phone"/>
                    </div>
                </div>
            </div>

            <div class="row checkin-footer">
                <div class="col-md-12 checkin-count">
                    <h3>Current Counts</h3>
            
                    <asp:PlaceHolder ID="phCounts" runat="server"></asp:PlaceHolder>
                </div>
            </div>
        </asp:Panel>

</ContentTemplate>
</asp:UpdatePanel>
