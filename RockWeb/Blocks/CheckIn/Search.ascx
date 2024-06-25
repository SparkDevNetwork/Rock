<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Search.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Search" %>

<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <script>
        Sys.Application.add_load(function () {

            $(document).ready(function() {
                // set focus to the input unless on a touch device
                var isTouchDevice = 'ontouchstart' in document.documentElement;
                if (!isTouchDevice) {

                    if ($('.modal-open').length == 0) {
                        $('.search-input').focus();
                    }

                }

                $('.tenkey a.digit').on('click', function () {
                    $phoneNumber = $("input[id$='tbPhone']");
                    $phoneNumber.val($phoneNumber.val() + $(this).html());
                });
                $('.tenkey a.back').on('click', function () {
                    $phoneNumber = $("input[id$='tbPhone']");
                    $phoneNumber.val($phoneNumber.val().slice(0,-1));
                });
                $('.tenkey a.clear').on('click', function () {
                    $phoneNumber = $("input[id$='tbPhone']");
                    $phoneNumber.val('');
                });
            });

            if ($('#<%=hfShowAddFamilyPrompt.ClientID%>').val() == "1") {

                bootbox.confirm({
                    message: '<%=this.ConditionMessage%>',
                    buttons: {
                        cancel: {
                            label: 'Try Again'
                        },
                        confirm: {
                            label: 'Add a New Family'
                        }
                    },
                    callback: function (result) {
                        if (result) {
                            window.location = "javascript:__doPostBack('<%=upContent.ClientID %>', 'AddFamily')";
                        }
                    }
                });

            }

        });
    </script>

    <Rock:ModalAlert ID="maWarning" runat="server" />
    <asp:HiddenField ID="hfShowAddFamilyPrompt" runat="server" Value="0" />

    <div class="checkin-header">
        <h1><asp:Literal ID="lPageTitle" runat="server" /></h1>
    </div>

    <div class="checkin-body">
        <div class="checkin-scroll-panel">
            <div class="scroller">
                <div class="checkin-search-body <%= (!CurrentCheckInState.Kiosk.RegistrationModeEnabled && (CurrentCheckInType == null || CurrentCheckInType.SearchType.Guid == new Guid(Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_PHONE_NUMBER)) ? "search-phone" : "search-name") %>">

                    <asp:Panel ID="pnlSearchPhone" runat="server" CssClass="clearfix">
                        <Rock:RockTextBox ID="tbPhone" MaxLength="10" CssClass="search-input checkin-phone-entry input-lg js-search-input js-phone-search" FormGroupCssClass="search-phone-form-group" runat="server" Label="Phone Number" autocomplete="off" />

                        <!-- LPC MODIFIED CODE -->
                        <div class="tenkey checkin-phone-keypad">
                            <div class="keypad-row row">
                                <div class="keypad-column col-xs-4">
                                    <a href="#" class="btn btn-default btn-lg btn-keypad digit">1</a>
                                </div>
                                <div class="keypad-column col-xs-4">
                                    <a href="#" class="btn btn-default btn-lg btn-keypad digit">2</a>
                                </div>
                                <div class="keypad-column col-xs-4">
                                    <a href="#" class="btn btn-default btn-lg btn-keypad digit">3</a>
                                </div>
                            </div>
                            <div class="keypad-row row">
                                <div class="keypad-column col-xs-4">
                                    <a href="#" class="btn btn-default btn-lg btn-keypad digit">4</a>
                                </div>
                                <div class="keypad-column col-xs-4">
                                    <a href="#" class="btn btn-default btn-lg btn-keypad digit">5</a>
                                </div>
                                <div class="keypad-column col-xs-4">
                                    <a href="#" class="btn btn-default btn-lg btn-keypad digit">6</a>
                                </div>
                            </div>
                            <div class="keypad-row row">
                                <div class="keypad-column col-xs-4">
                                    <a href="#" class="btn btn-default btn-lg btn-keypad digit">7</a>
                                </div>
                                <div class="keypad-column col-xs-4">
                                    <a href="#" class="btn btn-default btn-lg btn-keypad digit">8</a>
                                </div>
                                <div class="keypad-column col-xs-4">
                                    <a href="#" class="btn btn-default btn-lg btn-keypad digit">9</a>
                                </div>
                            </div>
                            <div class="keypad-row row">
                                <div class="keypad-column col-xs-4">
                                    <a href="#" class="btn btn-default btn-lg btn-keypad command clear">Clear</a>
                                </div>
                                <div class="keypad-column col-xs-4">
                                    <a href="#" class="btn btn-default btn-lg btn-keypad digit">0</a>
                                </div>
                                <div class="keypad-column col-xs-4">
                                    <a href="#" class="btn btn-default btn-lg btn-keypad command back"><i class="fas fa-backspace"></i></a>
                                </div>
                            </div>
                        </div>
                        <!-- END LPC MODIFIED CODE -->
                    </asp:Panel>

                    <asp:Panel ID="pnlSearchName" CssClass="clearfix" runat="server">
                        <Rock:RockTextBox ID="txtName" runat="server" Label="Name" CssClass="search-input namesearch input-lg js-search-input js-name-search" FormGroupCssClass="search-name-form-group" autocapitalize="off" autocomplete="off" spellcheck="false" autocorrect="off" />
                    </asp:Panel>

                    <div class="checkin-actions">
                        <Rock:BootstrapButton CssClass="btn btn-primary btn-block" ID="lbSearch" runat="server" OnClick="lbSearch_Click" Text="Search" DataLoadingText="Searching..." ></Rock:BootstrapButton>
                        <asp:LinkButton CssClass="btn btn-default btn-block btn-cancel" ID="lbBack" runat="server" OnClick="lbBack_Click" Text="Cancel" />
                    </div>

                </div>
            </div>
        </div>
    </div>
    <!-- LPC CODE -->
    <style>
        .keypad-row {
            height: 84px;
            width: 100%;
            margin: 0;
        }

        .keypad-column {
            height: 100%;
            padding: 2px 4px;
        }

        .tenkey {
            width: 300px;
            margin-left: 0;
            margin-right: 0;
        }

        .tenkey .btn {
            width: 100%;
            height: 100%;
        }

        .checkin-search-body.search-phone, .search-phone-form-group, input.checkin-phone-entry {
            width: 300px;
        }
    </style>
    <!-- END LPC CODE -->

</ContentTemplate>
</asp:UpdatePanel>
