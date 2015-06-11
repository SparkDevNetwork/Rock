<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Welcome.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Welcome" %>

<asp:UpdatePanel ID="upContent" runat="server">
    <Triggers>
        <%-- make sure lbLogin and lbCancel causes a full postback due to an issue with buttons not firing in IE after clicking the login button --%>
        <asp:PostBackTrigger ControlID="lbLogin" />
        <asp:PostBackTrigger ControlID="lbCancel" />
    </Triggers>

    <ContentTemplate>
        <style>
            .btn-checkin-configuration {
                position: absolute;
                bottom: 0;
                left: 0;
            }

            .checkin-manager-locations {
                max-width: 75%;
            }

            .checkin-manager-location-label {
                font-weight: 600;
                font-size: 36px;
                padding-left: 8px;
            }

            .checkin-manager-location-count {
                font-weight: 600;
                font-size: 24px;
            }

            .checkin-manager-actions {
                position: absolute;
                bottom: 0px;
                left: 75%;
            }
        </style>

        <script>

            Sys.Application.add_load(function () {
                if ($('.js-manager-login').is(':visible')) {
                    $('.tenkey a.digit').click(function () {
                        $phoneNumber = $("input[id$='tbPIN']");
                        $phoneNumber.val($phoneNumber.val() + $(this).html());
                    });
                    $('.tenkey a.back').click(function () {
                        $phoneNumber = $("input[id$='tbPIN']");
                        $phoneNumber.val($phoneNumber.val().slice(0, -1));
                    });
                    $('.tenkey a.clear').click(function () {
                        $phoneNumber = $("input[id$='tbPIN']");
                        $phoneNumber.val('');
                    });

                    // set focus to the input unless on a touch device
                    var isTouchDevice = 'ontouchstart' in document.documentElement;
                    if (!isTouchDevice) {
                        if ($('.checkin-phone-entry').length) {
                            $('.checkin-phone-entry').focus();
                        }
                    }
                }
                else
                {
                    // set focus to body if the manager login (ten-key) isn't visible, to fix buttons not working after showing the ten-key panel
                    $('body').focus();
                }
            });

        </script>

        <asp:PlaceHolder ID="phScript" runat="server"></asp:PlaceHolder>

        <Rock:HiddenFieldWithClass ID="hfRefreshTimerSeconds" runat="server" CssClass="js-refresh-timer-seconds" />


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

        <asp:LinkButton runat="server" ID="btnManager" CssClass="btn-checkin-configuration" OnClick="btnManager_Click"><i class="fa fa-cog fa-4x"></i></asp:LinkButton>

        <%-- Panel for checkin manager --%>
        <asp:Panel ID="pnlManager" runat="server" Visible="false">
            <div class="checkin-header">
                <h1>Locations</h1>
            </div>
            <div class="checkin-body checkin-manager-locations">

                <div class="checkin-scroll-panel">
                    <div class="scrollers">
                        <asp:Repeater ID="rLocations" runat="server" OnItemCommand="rLocations_ItemCommand" OnItemDataBound="rLocations_ItemDataBound">
                            <ItemTemplate>
                                <div class="controls">
                                    <div class="btn-group">
                                        <asp:LinkButton runat="server" ID="lbOpen" CssClass="btn btn-default btn-lg btn-success" Text="Open" CommandName="Open" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "LocationId") %>'/>
                                        <asp:LinkButton runat="server" ID="lbClose" CssClass="btn btn-default btn-lg" Text="Close" CommandName="Close" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "LocationId") %>'/>
                                        
                                    </div>
                                    <label class="checkin-manager-location-label">
                                        <asp:Literal ID="lLocationName" runat="server" />
                                    </label>
                                    <label class="badge bg-info checkin-manager-location-count">
                                        <asp:Literal ID="lLocationCount" runat="server" />
                                    </label>
                                </div>

                                <br />
                            </ItemTemplate>
                        </asp:Repeater>

                    </div>
                </div>

            </div>

            <div class="controls checkin-manager-actions checkin-actions">
                <asp:LinkButton ID="btnOverride" runat="server" CssClass="btn btn-default btn-large btn-block btn-checkin-select" Text="Override" OnClick="btnOverride_Click" />
                <asp:LinkButton ID="btnScheduleLocations" runat="server" CssClass="btn btn-default btn-large btn-block btn-checkin-select" Text="Schedule Locations" OnClick="btnScheduleLocations_Click" />
                <asp:LinkButton ID="btnBack" runat="server" CssClass="btn btn-default btn-large btn-block btn-checkin-select" Text="Back" OnClick="btnBack_Click" />
            </div>

        </asp:Panel>

        <%-- Panel for checkin manager login --%>
        <asp:Panel ID="pnlManagerLogin" CssClass="js-manager-login" runat="server" Visible="false">

            <div class="checkin-header">
                <h1>Manager Login</h1>
            </div>

            <div class="checkin-body">

                <div class="checkin-scroll-panel">
                    <div class="scroller">
                        <div class="checkin-search-body">
                            <Rock:RockTextBox ID="tbPIN" CssClass="checkin-phone-entry" runat="server" Label="PIN" />

                            <div class="tenkey checkin-phone-keypad">
                                <div>
                                    <a href="#" class="btn btn-default btn-lg digit">1</a>
                                    <a href="#" class="btn btn-default btn-lg digit">2</a>
                                    <a href="#" class="btn btn-default btn-lg digit">3</a>
                                </div>
                                <div>
                                    <a href="#" class="btn btn-default btn-lg digit">4</a>
                                    <a href="#" class="btn btn-default btn-lg digit">5</a>
                                    <a href="#" class="btn btn-default btn-lg digit">6</a>
                                </div>
                                <div>
                                    <a href="#" class="btn btn-default btn-lg digit">7</a>
                                    <a href="#" class="btn btn-default btn-lg digit">8</a>
                                    <a href="#" class="btn btn-default btn-lg digit">9</a>
                                </div>
                                <div>
                                    <a href="#" class="btn btn-default btn-lg command back">Back</a>
                                    <a href="#" class="btn btn-default btn-lg digit">0</a>
                                    <a href="#" class="btn btn-default btn-lg command clear">Clear</a>
                                </div>
                            </div>

                            <div class="checkin-actions">
                                <asp:LinkButton ID="lbLogin" runat="server" OnClick="lbLogin_Click" CssClass="btn btn-primary">Login</asp:LinkButton>
                            </div>

                            <asp:LinkButton ID="lbCancel" runat="server" CausesValidation="false" OnClick="lbCancel_Click" CssClass="btn btn-default">Cancel</asp:LinkButton>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
