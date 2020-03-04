﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Welcome.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Welcome" %>
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
        <Rock:HiddenFieldWithClass ID="hfConfigurationHash" runat="server" CssClass="js-configuration-hash" />

        <Rock:HiddenFieldWithClass ID="hfLocalDeviceConfiguration" runat="server" CssClass="js-local-device-configuration" />

        <Rock:HiddenFieldWithClass ID="hfCameraMode" runat="server" CssClass="js-camera-mode" />

        <script>

            Sys.WebForms.PageRequestManager.getInstance().add_pageLoading(function () {
                // Note: We need to destroy the old countdown timer so that it does not generate multiplier
                // expire events. There is a visual anomaly with doing this. Depending on when the response
                // from the server is received the displayed time could display the same second for more
                // than one second and/or skip displaying a second entirely.
                $('.js-countdown-timer').countdown('destroy');
                if (timeout)
                {
                    window.clearTimeout(timeout)
                }
            });

            function PostRefresh() {
                window.location = "javascript:__doPostBack('<%=lbTimerRefresh.UniqueID %>','')";
            }

            function GetLabelTypeSelection() {
                var ids = '';
                $('div.js-label-list').find('i.fa-check-square').each(function () {
                    ids += $(this).closest('a').attr('data-label-guid') + ',';
                });
                if (ids == '') {
                    bootbox.alert('Please select at least one tag');
                    return false;
                }
                else {
                    $('#<%=lbReprintSelectLabelTypes.ClientID %>').button('loading')
                    $('#<%=hfLabelFileGuids.ClientID %>').val(ids);
                    return true;
                }
            }

            Sys.Application.add_load(function () {

                $('a.js-label-select').off('click').on('click', function () {
                    $(this).toggleClass('active');
                    $(this).find('i').toggleClass('fa-check-square').toggleClass('fa-square-o');
                    var ids = '';
                    $('div.js-label-list').find('i.fa-check-square').each(function () {
                        ids += $(this).closest('a').attr('data-label-guid') + ',';
                    });
                    $('.js-label-file-guids').val(ids);
                });

                var timeoutSeconds = $('.js-refresh-timer-seconds').val();
                var isGettingConfigurationStatus = false;
                timeout = window.setInterval(checkForConfigurationChange, timeoutSeconds * 1000);

                function checkForConfigurationChange() {
                    if (isGettingConfigurationStatus) {
                        return;
                    }

                    isGettingConfigurationStatus = true;

                    var getConfigurationStatusUrl = Rock.settings.get('baseUrl') + 'api/checkin/configuration/status';

                    var localDeviceConfiguration = JSON.parse($('.js-local-device-configuration').val());
                    var $configurationHash = $('.js-configuration-hash');

                    // check if the configuration has changed by making a REST call. If it has changed, refresh the page.
                    $.ajax({
                        type: "POST",
                        url: getConfigurationStatusUrl,
                        timeout: 10000,
                        data: localDeviceConfiguration,
                        success: function (data) {

                            var localConfigurationStatus = data;

                            if ($configurationHash.val() != localConfigurationStatus.ConfigurationHash) {
                                $configurationHash.val(localConfigurationStatus.ConfigurationHash);
                                refreshKiosk();
                            }
                        },
                        dataType: 'json'
                    }).then(function () {
                        isGettingConfigurationStatus = false;
                    }).catch(function () {
                        console.log('offline');
                        isGettingConfigurationStatus = false;
                    });
                }

                function refreshKiosk() {
                    $('.js-countdown-timer').countdown('destroy')

                    setTimeout(function () {
                        $('.js-countdown-timer')
                            .text('00:00');
                    }, 0)

                    PostRefresh();
                }

                var $CountdownTimer = $('.js-countdown-timer');

                var secondsUntil = Number($('.js-countdown-seconds-until').val());
                if (secondsUntil > 0) {

                    $CountdownTimer.countdown({
                        until: secondsUntil,
                        compact: true,
                        onExpiry: refreshKiosk
                    });
                }

                var lastKeyPress = 0;
                var keyboardBuffer = '';
                var swipeProcessing = false;

                $(document).off('keypress');
                $(document).on('keypress', function (e) {

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

                // Install function the host-app can call to pass the scanned barcode.
                window.PerformScannedCodeSearch = function (code) {
                    if (!swipeProcessing) {
                        $('#hfSearchEntry').val(code);
                        swipeProcessing = true;
                        console.log('processing');
                        window.location = "javascript:__doPostBack('hfSearchEntry', 'Wedge_Entry')";
                    }
                }

                // handle click of scan button
                $('.js-camera-button').on('click', function (a) {
                    a.preventDefault();
                    if (typeof window.RockCheckinNative !== 'undefined' && typeof window.RockCheckinNative.StartCamera !== 'undefined') {
                        // Reset the swipe processing as it may have failed silently.
                        swipeProcessing = false;
                        window.RockCheckinNative.StartCamera(false);
                    }
                });

                // auto-show or auto-enable camera if configured to do so.
                if (typeof window.RockCheckinNative !== 'undefined' && typeof window.RockCheckinNative.StartCamera !== 'undefined') {
                    if ($('.js-camera-mode').val() === 'AlwaysOn') {
                        window.RockCheckinNative.StartCamera(false);
                    }
                    else if ($('.js-camera-mode').val() === 'Passive') {
                        window.RockCheckinNative.StartCamera(true);
                    }
                }

                if ($('.js-manager-login').length) {
                    $('.tenkey a.digit').on('click', function () {
                        $phoneNumber = $("input[id$='tbPIN']");
                        $phoneNumber.val($phoneNumber.val() + $(this).html());
                    });
                    $('.tenkey a.back').on('click', function () {
                        $phoneNumber = $("input[id$='tbPIN']");
                        $phoneNumber.val($phoneNumber.val().slice(0, -1));
                    });
                    $('.tenkey a.clear').on('click', function () {
                        $phoneNumber = $("input[id$='tbPIN']");
                        $phoneNumber.val('');
                    });

                    // set focus to the input unless on a touch device
                    var isTouchDevice = 'ontouchstart' in document.documentElement;
                    if (!isTouchDevice) {
                        if ($('.checkin-phone-entry').length) {
                            $('.checkin-phone-entry').trigger('focus');
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
            <asp:LinkButton ID="lbTimerRefresh" runat="server" OnClick="lbTimerRefresh_Click" />
            <Rock:HiddenFieldWithClass ID="hfCountdownSecondsUntil" runat="server" CssClass="js-countdown-seconds-until" />
            <Rock:HiddenFieldWithClass ID="hfActiveWhen" runat="server" CssClass="js-active-when" />
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

                        <!-- NOTE: lNotActiveYetCaption will rendered with a css class of 'js-countdown-timer'  -->
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
            <asp:HiddenField ID="hfAllowOpenClose" runat="server" />
            <div class="checkin-header">
                <h1>Locations</h1>
            </div>
            <div class="checkin-body kioskmanager-locations">

                <div class="checkin-scroll-panel">
                    <div class="scrollers">
                        <asp:Repeater ID="rLocations" runat="server" OnItemCommand="rLocations_ItemCommand" OnItemDataBound="rLocations_ItemDataBound">
                            <ItemTemplate>
                                <div class="controls kioskmanager-location">
                                    <div ID="divLocationToggle" runat="server" class="btn-group kioskmanager-location-toggle">
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
                <asp:LinkButton ID="btnReprintLabels" runat="server" CssClass="btn btn-default btn-large btn-block btn-checkin-select" Text="Reprint Labels" OnClick="btnReprintLabels_Click" />
                <asp:LinkButton ID="btnOverride" runat="server" CssClass="btn btn-default btn-large btn-block btn-checkin-select" Text="Override" OnClick="btnOverride_Click" />
                <asp:LinkButton ID="btnScheduleLocations" runat="server" CssClass="btn btn-default btn-large btn-block btn-checkin-select" Text="Schedule Locations" OnClick="btnScheduleLocations_Click" />
                <asp:LinkButton ID="btnBack" runat="server" CssClass="btn btn-default btn-large btn-block btn-checkin-select" Text="Back" OnClick="btnBack_Click" />
            </div>

        </asp:Panel>

        <%-- Device Manager Reprint Label Panel for searching for person --%>
        <asp:Panel ID="pnlReprintLabels" runat="server" Visible="false" DefaultButton="lbManagerReprintSearch">
            <div class="checkin-body">
                <div class="checkin-scroll-panel">
                    <div class="scroller">
                        <div class="checkin-search-body">

                            <asp:Panel ID="pnlSearchName" CssClass="clearfix center-block" runat="server">
                                <Rock:RockTextBox ID="tbNameOrPhone" runat="server" Label="Phone or Name" AutoCompleteType="Disabled" spellcheck="false" autocorrect="off" CssClass="search-input namesearch input-lg" FormGroupCssClass="search-name-form-group" />
                                <Rock:ScreenKeyboard id="skKeyboard" runat="server" ControlToTarget="tbNameOrPhone" KeyboardType="TenKey" KeyCssClass="checkin btn btn-default btn-lg btn-keypad digit" WrapperCssClass="center-block" ></Rock:ScreenKeyboard>
                            </asp:Panel>

                            <div class="checkin-actions margin-t-md">
                                <Rock:BootstrapButton CssClass="btn btn-primary btn-block" ID="lbManagerReprintSearch" runat="server" OnClick="lbManagerReprintSearch_Click" Text="Search" DataLoadingText="Searching..." ></Rock:BootstrapButton>
                                <asp:LinkButton CssClass="btn btn-default btn-block btn-cancel" ID="lbReprintCancelBack" runat="server" OnClick="lbManagerCancel_Click" Text="Cancel" />
                            </div>

                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <!-- Device Manager Reprint Label Panel showing person search results -->
        <asp:Panel ID="pnlReprintSearchPersonResults" runat="server" Visible="false">
            <div class="checkin-body">
                <div class="checkin-scroll-panel">
                    <div class="scroller">

                    <div class="control-group checkin-body-container">
                        <label class="control-label"><asp:Literal ID="lCaption" runat="server"></asp:Literal></label>
                        <div class="controls">
                            <asp:Repeater ID="rReprintLabelPersonResults" runat="server" OnItemCommand="rReprintLabelPersonResults_ItemCommand">
                                <ItemTemplate>
                                    <asp:HiddenField ID="hfAttendanceIds" runat="server" Value='<%#String.Join(",",((Rock.Utility.ReprintLabelPersonResult)Container.DataItem).AttendanceIds )%>' />
                                    <Rock:BootstrapButton ID="lbSelectPersonForReprint" runat="server" Text='<%# Container.DataItem.ToString() %>' CommandName='<%# Eval("AttendanceIds") %>' CommandArgument='<%# Eval("Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select text-left" DataLoadingText="Loading..." />
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </div>

                    </div>
                </div>
            </div>

            <div class="checkin-footer">
                <div class="checkin-actions">
                    <asp:LinkButton CssClass="btn btn-default btn-cancel" ID="lbReprintSearchPersonCancel" runat="server" OnClick="lbManagerCancel_Click" Text="Cancel" />
                </div>
            </div>
        </asp:Panel>

        <!-- Device Manager Reprint Label Panel showing selected person's available labels -->
        <asp:Panel ID="pnlReprintSelectedPersonLabels" runat="server" Visible="false">
            <Rock:ModalAlert ID="maNoLabelsFound" runat="server"></Rock:ModalAlert>
            <div class="checkin-body">
                <div class="checkin-scroll-panel">
                    <div class="scroller">

                    <div class="control-group checkin-body-container">
                        <label class="control-label">Select Tags to Reprint</label>
                        <div class="controls">

                           <div class="controls js-label-list">
                                <asp:Repeater ID="rReprintLabelTypeSelection" runat="server" OnItemDataBound="rReprintLabelTypeSelection_ItemDataBound">
                                    <ItemTemplate>
                                        <div class="row">
                                                <a data-label-guid='<%# Eval("FileGuid") %>' class="btn btn-primary btn-checkin-select btn-block js-label-select <%# GetSelectedClass( false ) %>">
                                                    <div class="row">
                                                        <div class="col-md-1 col-sm-2 col-xs-3 checkbox-container">
                                                            <i class='fa fa-3x <%# GetCheckboxClass( false ) %>'></i>
                                                        </div>
                                                        <asp:Panel ID="pnlLabel" runat="server"><asp:Literal ID="lLabelButton" runat="server"></asp:Literal></asp:Panel>
                                                    </div>
                                                </a>

                                            <asp:Panel ID="pnlChangeButton" runat="server" CssClass="col-xs-9 col-sm-3 col-md-2" Visible="false">
                                                <asp:LinkButton ID="lbChange" runat="server" CssClass="btn btn-default btn-checkin-select btn-block" CommandArgument='<%# Eval("FileGuid") %>' CommandName="Change">Change</asp:LinkButton>
                                            </asp:Panel>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </div>
                    </div>

                    </div>
                </div>

                <asp:HiddenField ID="hfSelectedPersonId" runat="server"></asp:HiddenField>
                <asp:HiddenField ID="hfSelectedAttendanceIds" runat="server"></asp:HiddenField>
                <Rock:HiddenFieldWithClass ID="hfLabelFileGuids" runat="server" CssClass="js-label-file-guids" />
            </div>

            <div class="checkin-footer">
                <div class="checkin-actions">
                    <asp:LinkButton CssClass="btn btn-primary " ID="lbReprintSelectLabelTypes" runat="server" OnClientClick="return GetLabelTypeSelection();" OnClick="lbReprintSelectLabelTypes_Click" Text="Print" data-loading-text="Printing..." />
                    <asp:LinkButton CssClass="btn btn-default btn-cancel" ID="lbReprintSelectLabelCancel" runat="server" OnClick="lbManagerCancel_Click" Text="Cancel" />
                </div>
            </div>
        </asp:Panel>

        <!-- Device Manager Reprint results -->
        <asp:Panel ID="pnlReprintResults" runat="server" Visible="false">
            <div class="checkin-body">
                <div class="checkin-scroll-panel">
                    <div class="scroller">
                        <h2><asp:Literal ID="lReprintResultsHtml" runat="server" /></h2>
                    </div>
                </div>
            </div>

            <div class="checkin-footer">
                <div class="checkin-actions">
                    <asp:LinkButton CssClass="btn btn-primary" ID="lbMangerReprintDone" runat="server" OnClick="lbManagerReprintDone_Click" Text="Done" />
                </div>
            </div>
        </asp:Panel>

        <%-- Panel for device manager login --%>
        <asp:Panel ID="pnlManagerLogin" CssClass="js-manager-login" runat="server" Visible="false" DefaultButton="lbLogin">

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
