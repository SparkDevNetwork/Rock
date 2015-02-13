<%@ Control Language="C#" AutoEventWireup="true" CodeFile="KioskTransactionEntry.ascx.cs" Inherits="RockWeb.Blocks.Finance.KioskTransactionEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <script>
        
            Sys.Application.add_load(function () {

                //
                // setup keyboard for phone search
                //
                if ($(".js-pnlsearch").length) {

                    $('.js-pnlsearch .tenkey a.digit').click(function () {
                        $phoneNumber = $("input[id$='tbPhone']");
                        $phoneNumber.val($phoneNumber.val() + $(this).html());
                        return false;
                    });
                    $('.js-pnlsearch .tenkey a.back').click(function () {
                        $phoneNumber = $("input[id$='tbPhone']");
                        $phoneNumber.val($phoneNumber.val().slice(0, -1));
                        return false;
                    });
                    $('.js-pnlsearch .tenkey a.clear').click(function () {
                        $phoneNumber = $("input[id$='tbPhone']");
                        $phoneNumber.val('');
                        return false;
                    });

                    // set focus to the input unless on a touch device
                
                    var isTouchDevice = 'ontouchstart' in document.documentElement;
                    if (!isTouchDevice) {
                        $('.kiosk-phoneentry').focus();
                    }
                }

                //
                // setup keyboard for account entry
                //
                if ($(".js-pnlaccountentry").length) {

                    $('.js-pnlaccountentry .tenkey a.digit').click(function () {
                        $amount = $(".input-group.active .form-control");
                        $amount.val($amount.val() + $(this).html());
                        return false;
                    });
                    $('.js-pnlaccountentry .tenkey a.clear').click(function () {
                        $amount = $(".input-group.active .form-control");
                        $amount.val('');
                        return false;
                    });

                    $('.form-control').click(function () {
                        $('.input-group').removeClass("active");
                        $(this).closest('.input-group').addClass("active");
                    });
                }
            });
        
        </script>

        <asp:Panel ID="pnlSearch" runat="server" CssClass="js-pnlsearch">
            <header>
                <h1>Please Enter Your Phone Number</h1>
            </header>

            <main>
                <div class="row">
                    <div class="col-md-8 margin-b-lg">
                        <Rock:NotificationBox ID="nbSearch" runat="server" NotificationBoxType="Warning"></Rock:NotificationBox>
                    
                        <Rock:RockTextBox ID="tbPhone" type="number" CssClass="kiosk-phoneentry" runat="server" Label="Phone Number" />

                        <div class="tenkey kiosk-phone-keypad">
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
                    </div>
                    <div class="col-md-4">
                        <asp:LinkButton ID="lbSearchNext" runat="server" OnClick="lbSearchNext_Click" CssClass="btn btn-primary btn-kiosk btn-kiosk-lg">Next</asp:LinkButton>
                    </div>
                </div>
            </main>

            <footer>
                <div class="container">
                    <asp:LinkButton ID="lbSearchCancel" runat="server" OnClick="lbSearchCancel_Click" CssClass="btn btn-default btn-kiosk">Cancel</asp:LinkButton>
                </div>
            </footer>
            
        </asp:Panel>

        <asp:Panel ID="pnlGivingUnitSelect" runat="server" Visible="false" CssClass="js-pnlgivingunitselect">
            <header>
                <h1>Select Your Family</h1>
            </header>

            <main class="clearfix">
                <div class="scrollpanel">
                    <div class="scroller">
                        <asp:PlaceHolder ID="phGivingUnits" runat="server"></asp:PlaceHolder>
                    </div>
                </div>
            </main>

            <footer>
                <div class="container">
                    <div class="row">
                        <div class="col-md-8">
                            <asp:LinkButton ID="lbGivingUnitSelectBack" runat="server" OnClick="lbGivingUnitSelectBack_Click" CssClass="btn btn-default btn-kiosk">Back</asp:LinkButton>
                            <asp:LinkButton ID="lbGivingUnitSelectCancel" runat="server" OnClick="lbGivingUnitSelectCancel_Click" CssClass="btn btn-default btn-kiosk">Cancel</asp:LinkButton>
                        </div>
                        <div class="col-md-4">
                            <asp:LinkButton ID="lbRegisterFamily" runat="server" OnClick="lbRegisterFamily_Click" CssClass="btn btn-default btn-kiosk btn-kiosk-lg pull-right">Register</asp:LinkButton>
                        </div>
                    </div>
                </div>
            </footer>
        </asp:Panel>

        <asp:Panel ID="pnlRegister" runat="server" Visible="false">
            Register Page
        </asp:Panel>

        <asp:Panel ID="pnlAccountEntry" runat="server" CssClass="js-pnlaccountentry" Visible="false">
            <header>
                <h1>Enter An Amount</h1>
            </header>
            
            <main>
                <asp:Label ID="lblGivingAs" runat="server"></asp:Label>

                <asp:PlaceHolder id="phAccounts" runat="server"></asp:PlaceHolder>

                <div class="tenkey kiosk-phone-keypad">
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
                        <a href="#" class="btn btn-default btn-lg digit">.</a>
                        <a href="#" class="btn btn-default btn-lg digit">0</a>
                        <a href="#" class="btn btn-default btn-lg command clear">Clear</a>
                    </div>
                </div>
            </main>
            
            <footer>
                <div class="container">
                    <asp:LinkButton ID="lbAccountEntryBack" runat="server" OnClick="lbAccountEntryBack_Click" CssClass="btn btn-default btn-kiosk">Back</asp:LinkButton>
                    <asp:LinkButton ID="lbAccountEntryCancel" runat="server" OnClick="lbAccountEntryCancel_Click" CssClass="btn btn-default btn-kiosk">Cancel</asp:LinkButton>
                </div>
            </footer>

        </asp:Panel>

        <asp:Panel ID="pnlSwipe" runat="server" Visible="false">

        </asp:Panel>

        <asp:Panel ID="pnlReceipt" runat="server" Visible="false">

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
