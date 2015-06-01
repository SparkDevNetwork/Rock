<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Welcome.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Welcome" %>


<asp:HyperLink runat="server" href="../../Themes/CheckinAdventureKids/Styles/checkin-theme.css" rel="stylesheet" Visible="false" />

<asp:UpdatePanel ID="upContent" runat="server">



    <ContentTemplate>
        <style>
            .btn-checkin-configuration {
                position: absolute;
                bottom: 0;
                left: 0;
            }
        </style>

        <asp:PlaceHolder ID="phScript" runat="server"></asp:PlaceHolder>

        <Rock:HiddenFieldWithClass ID="hfRefreshTimerSeconds" runat="server" CssClass="js-refresh-timer-seconds" />


        <asp:Panel ID="pnlWelcome" runat="server">

            <Rock:ModalAlert ID="maWarning" runat="server" />

            <span style="display: none">
                <asp:LinkButton ID="lbRefresh" runat="server" OnClick="lbRefresh_Click"></asp:LinkButton>
                <asp:Label ID="lblActiveWhen" runat="server" CssClass="active-when" />
            </span>

            <%-- Panel for no schedules --%>
            <asp:Panel ID="pnlNotActive" runat="server">
                <div class="checkin-header">
                    <h1>Check-in Is Not Active</h1>
                </div>

                <div class="checkin-body">

                    <div class="checkin-scroll-panel">
                        <div class="scroller">
                            <p>There are no current or future schedules for this kiosk!</p>

                        </div>
                    </div>

                </div>
            </asp:Panel>

            <%-- Panel for schedule not active yet --%>
            <asp:Panel ID="pnlNotActiveYet" runat="server">
                <div class="checkin-header">
                    <h1>Check-in Is Not Active Yet</h1>
                </div>

                <div class="checkin-body">

                    <div class="checkin-scroll-panel">
                        <div class="scroller">

                            <p>This kiosk is not active yet.  Countdown until active: <span class="countdown-timer"></span></p>
                            <asp:HiddenField ID="hfActiveTime" runat="server" />

                        </div>
                    </div>

                </div>
            </asp:Panel>

            <%-- Panel for location closed --%>
            <asp:Panel ID="pnlClosed" runat="server">
                <div class="checkin-header">
                    <h1>Location Closed</h1>
                </div>

                <div class="checkin-body">
                    <div class="checkin-scroll-panel">
                        <div class="scroller">
                            <p>This location is currently closed.</p>
                        </div>
                    </div>
                </div>
            </asp:Panel>

            <%-- Panel for active checkin --%>
            <asp:Panel ID="pnlActive" runat="server">

                <asp:LinkButton runat="server" ID="btnManager" CssClass="btn-checkin-configuration" OnClick="btnManager_Click"><i class="fa fa-cog fa-4x"></i></asp:LinkButton>

                <div class="checkin-body">
                    <div class="checkin-scroll-panel">
                        <div class="scroller">
                            <div class="checkin-search-actions checkin-start">
                                <asp:LinkButton CssClass="btn btn-primary" ID="lbSearch" runat="server" OnClick="lbSearch_Click" Text="Check in" />
                            </div>
                        </div>
                    </div>
                </div>

            </asp:Panel>
        </asp:Panel>

        <%-- Panel for checkin manager --%>
        <asp:Panel ID="pnlManager" runat="server" Visible="false">
            <div class="checkin-body">
                <div class="row">
                    <div class="col-md-6">
                        <h1>Locations</h1>
                        <div class="checkin-scroll-panel">
                            <div class="scroller">
                                <div class="control-group checkin-body-container">
                                    <label class="control-label">Locations</label>
                                    <div class="controls">
                                        <Rock:Toggle ID="tgOpenClose" runat="server" CommandArgument='<%# Eval("Location.Id") %>' OnText="Open" OffText="Closed" />
                                        <asp:Repeater ID="rLocations" runat="server" OnItemCommand="rLocations_ItemCommand">
                                            <ItemTemplate>

                                                <Rock:Toggle ID="tgOpenClose" runat="server" CommandArgument='<%# Eval("Location.Id") %>' OnText="Open" OffText="Closed" />
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="col-md-6">
                        <div class="controls">
                            <asp:LinkButton ID="btnOverride" runat="server" CssClass="btn btn-default btn-large btn-block btn-checkin-select" Text="Override" OnClick="btnOverride_Click" />
                            <asp:LinkButton ID="btnScheduleLocations" runat="server" CssClass="btn btn-default btn-large btn-block btn-checkin-select" Text="Schedule Locations" OnClick="btnScheduleLocations_Click" />
                            <asp:LinkButton ID="btnBack" runat="server" CssClass="btn btn-default btn-large btn-block btn-checkin-select" Text="Back" OnClick="btnBack_Click" />
                        </div>
                    </div>
                </div>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
