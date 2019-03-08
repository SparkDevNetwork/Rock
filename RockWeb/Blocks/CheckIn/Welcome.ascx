<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Welcome.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Welcome" %>
<style>
    .js-search-value {
        position: absolute;
        top: 200px;
    }
</style>
<script>
    var timeout = 0;
</script>

<asp:UpdatePanel ID="upContent" runat="server">

    <Triggers>
        <%-- make sure lbLogin and lbCancel causes a full postback due to an issue with buttons not firing in IE after clicking the login button --%>
        <asp:PostBackTrigger ControlID="lbLogin" />
        <asp:PostBackTrigger ControlID="lbCancel" />
    </Triggers>

    <ContentTemplate>

        <asp:HiddenField ID="hfSearchEntry" runat="server" ClientIDMode="Static" />

        <script>

            Sys.WebForms.PageRequestManager.getInstance().add_pageLoading(function () {
                // Note: We need to destroy the old countdown timer so that it does not generate multiplier
                // expire events. There is a visual anomaly with doing this. Depending on when the response
                // from the server is received the displayed time could display the same second for more
                // than one second and/or skip displaying a second entirely.
                $('.countdown-timer').countdown('destroy');
                if (timeout)
                {
                    window.clearTimeout(timeout)
                }
            });

            Sys.Application.add_load(function () {

                var timeoutSeconds = $('.js-refresh-timer-seconds').val();
                timeout = window.setTimeout(refreshKiosk, timeoutSeconds * 1000);

                function refreshKiosk() {
                    $('.countdown-timer').countdown('destroy');
                    PostRefresh();
                }

                var $ActiveWhen = $('.active-when');
                var $CountdownTimer = $('.countdown-timer');

                if ($ActiveWhen.text() != '') {
                    // Ensure date is parsed as local timezone
                    var tc = $ActiveWhen.text().split(/\D/);
                    var timeActive = new Date(tc[0], tc[1]-1, tc[2], tc[3], tc[4], tc[5]);
                    $CountdownTimer.countdown({
                        until: timeActive,
                        compact: true,
                        onExpiry: refreshKiosk
                    });
                }

                var lastKeyPress = 0;
                var keyboardBuffer = '';
                var swipeProcessing = false;

                $(document).off('keypress');
                $(document).on('keypress', function (e) {

                    //console.log('Keypressed: ' + e.which + ' - ' + String.fromCharCode(e.which));
                    var date = new Date();

                    if ($(".js-active").is(":visible")) {

                        // if the character is a line break stop buffering and call postback
                        if (e.which == 13) {
                            if (keyboardBuffer.length != 0 && !swipeProcessing) {
                                $('#hfSearchEntry').val(keyboardBuffer);
                                keyboardBuffer = '';
                                swipeProcessing = true;
                                window.location = "javascript:__doPostBack('hfSearchEntry', 'Wedge_Entry')";
                            }
                        }
                        else {
                            if ((date.getTime() - lastKeyPress) > 500) {
                                // if it's been more than 500ms, assume it is a new wedge read, so start a new keyboardBuffer
                                keyboardBuffer = String.fromCharCode(e.which);
                            } else if ((date.getTime() - lastKeyPress) < 100) {
                                // if it's been more less than 100ms, assume a wedge read is coming in and append to the keyboardBuffer
                                keyboardBuffer += String.fromCharCode(e.which);
                            }
                        }

                        // if the character is a line break stop buffering and call postback
                        if (e.which == 13 && keyboardBuffer.length != 0) {
                            if (!swipeProcessing) {
                                $('#hfSearchEntry').val(keyboardBuffer);
                                keyboardBuffer = '';
                                swipeProcessing = true;
                                console.log('processing');
                                window.location = "javascript:__doPostBack('hfSearchEntry', 'Wedge_Entry')";
                            }
                        }

                        // stop the keypress
                        e.preventDefault();

                    }

                    lastKeyPress = date.getTime();

                });

                function submitFamilyIdSearch( familyIds ) {
                    $('#hfSearchEntry').val(familyIds);
                    window.location = "javascript:__doPostBack('hfWedgeEntry', 'Family_Id_Search')";
                }

                // try to find the start button using js-start-button hook, otherwise, just hook to the first anchor tag
                var $startButton = $('.js-start-button-container .js-start-button');
                if ($startButton.length == 0) {
                    $startButton = $('.js-start-button-container a');
                }

                // handle click of start button in js-start-button-container
                $startButton.on('click', function (a, b, c) {
                    window.location = "javascript:__doPostBack('<%=upContent.ClientID%>', 'StartClick')";
                });

                if ($('.js-manager-login').length) {
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
                <h1><asp:Literal ID="lNotActiveTitle" runat="server" /></h1>
            </div>

            <div class="checkin-body">

                <div class="checkin-scroll-panel">
                    <div class="scroller">
                        <p><h1><asp:Literal ID="lNotActiveCaption" runat="server" /></h1></p>
                    </div>
                </div>

            </div>
        </asp:Panel>

        <%-- Panel for schedule not active yet --%>
        <asp:Panel ID="pnlNotActiveYet" runat="server">
            <div class="checkin-header">
                <h1><asp:Literal ID="lNotActiveYetTitle" runat="server" /></h1>
            </div>

            <div class="checkin-body">

                <div class="checkin-scroll-panel">
                    <div class="scroller">

                        <p><asp:Literal ID="lNotActiveYetCaption" runat="server" /></p>
                        <asp:HiddenField ID="hfActiveTime" runat="server" />

                    </div>
                </div>

            </div>
        </asp:Panel>

        <%-- Panel for location closed --%>
        <asp:Panel ID="pnlClosed" runat="server">
            <div class="checkin-header checkin-closed-header">
                <h1><asp:Literal ID="lClosedTitle" runat="server" /></h1>
            </div>

            <div class="checkin-body checkin-closed-body">
                <div class="checkin-scroll-panel">
                    <div class="scroller">
                        <p><asp:Literal ID="lClosedCaption" runat="server" /></p>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <%-- Panel for active checkin --%>
        <asp:Panel ID="pnlActive" runat="server" CssClass="js-active">

            <div class="checkin-body">
                <div class="checkin-scroll-panel">
                    <div class="scroller">
                        <%-- lStartButtonHtml will be the button HTML from Lava  --%>
                        <div class="js-start-button-container">
                            <asp:Literal ID="lStartButtonHtml" runat="server" />
                        </div>
                    </div>
                </div>
            </div>

        </asp:Panel>

        <asp:LinkButton runat="server" ID="btnManager" CssClass="kioskmanager-activate" OnClick="btnManager_Click"><i class="fa fa-cog fa-4x"></i></asp:LinkButton>

        <%-- Panel for checkin manager --%>
        <asp:Panel ID="pnlManager" runat="server" Visible="false">
            <div class="checkin-header">
                <h1>Locations</h1>
            </div>
            <div class="checkin-body kioskmanager-locations">

                <div class="checkin-scroll-panel">
                    <div class="scrollers">
                        <asp:Repeater ID="rLocations" runat="server" OnItemCommand="rLocations_ItemCommand" OnItemDataBound="rLocations_ItemDataBound">
                            <ItemTemplate>
                                <div class="controls kioskmanager-location">
                                    <div class="btn-group kioskmanager-location-toggle">
                                        <asp:LinkButton runat="server" ID="lbOpen" CssClass="btn btn-default btn-lg btn-success" Text="Open" CommandName="Open" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "LocationId") %>' />
                                        <asp:LinkButton runat="server" ID="lbClose" CssClass="btn btn-default btn-lg" Text="Close" CommandName="Close" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "LocationId") %>' />

                                    </div>
                                    <div class="kioskmanager-location-label">
                                        <asp:Literal ID="lLocationName" runat="server" />
                                    </div>
                                    <div class="badge badge-info kioskmanager-location-count">
                                        <asp:Literal ID="lLocationCount" runat="server" />
                                    </div>
                                </div>

                                <br />
                            </ItemTemplate>
                        </asp:Repeater>

                    </div>
                </div>

            </div>

            <div class="controls kioskmanager-actions checkin-actions">
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
                        <div class="row">
                            <div class="col-md-6">
                                <div class="checkin-search-body">
                                    <Rock:RockTextBox ID="tbPIN" CssClass="checkin-phone-entry input-lg" TextMode="Password" runat="server" Label="PIN" />

                                    <div class="tenkey checkin-phone-keypad">
                                        <div>
                                            <a href="#" class="btn btn-default btn-lg btn-keypad digit">1</a>
                                            <a href="#" class="btn btn-default btn-lg btn-keypad digit">2</a>
                                            <a href="#" class="btn btn-default btn-lg btn-keypad digit">3</a>
                                        </div>
                                        <div>
                                            <a href="#" class="btn btn-default btn-lg btn-keypad digit">4</a>
                                            <a href="#" class="btn btn-default btn-lg btn-keypad digit">5</a>
                                            <a href="#" class="btn btn-default btn-lg btn-keypad digit">6</a>
                                        </div>
                                        <div>
                                            <a href="#" class="btn btn-default btn-lg btn-keypad digit">7</a>
                                            <a href="#" class="btn btn-default btn-lg btn-keypad digit">8</a>
                                            <a href="#" class="btn btn-default btn-lg btn-keypad digit">9</a>
                                        </div>
                                        <div>
                                            <a href="#" class="btn btn-default btn-lg btn-keypad command clear">Clear</a>
                                            <a href="#" class="btn btn-default btn-lg btn-keypad digit">0</a>
                                            <a href="#" class="btn btn-default btn-lg btn-keypad command back"><i class="fas fa-backspace"></i></a>
                                        </div>
                                    </div>

                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="kioskmanager-counts">
                                    <h3>Current Counts</h3>
                                    <asp:PlaceHolder ID="phCounts" runat="server"></asp:PlaceHolder>
                                </div>
                            </div>
                        </div>
                    </div>



                </div>
            </div>

            <div class="checkin-footer">

                <div class="checkin-actions">
                    <asp:LinkButton ID="lbLogin" runat="server" OnClick="lbLogin_Click" CssClass="btn btn-primary">Login</asp:LinkButton>
                    <asp:LinkButton ID="lbCancel" runat="server" CausesValidation="false" OnClick="lbCancel_Click" CssClass="btn btn-default btn-cancel">Cancel</asp:LinkButton>
                </div>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
