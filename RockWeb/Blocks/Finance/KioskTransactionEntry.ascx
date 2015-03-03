<%@ Control Language="C#" AutoEventWireup="true" CodeFile="KioskTransactionEntry.ascx.cs" Inherits="RockWeb.Blocks.Finance.KioskTransactionEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfSwipe" runat="server" ClientIDMode="Static" />

        <script>
        
            /*var date = new Date();
            var lastKeyPress = date.getTime();
            console.log('Last Key Press: ' + lastKeyPress);
            var keyboardBuffer = '';*/

            var isTouchDevice = 'ontouchstart' in document.documentElement;

            Sys.Application.add_load(function () {

                //
                // setup swipe detection
                //
                $(document).scannerDetection({
                    
                    preventDefault:true,
                    onComplete: function (e, data) {
                        console.log('e:' + e);
                        console.log('data:' + data);
                        return false;
                    }
                });

                //
                // search 
                //
                if ($(".js-pnlsearch").is(":visible")) {

                    // setup digits buttons
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
                    if (!isTouchDevice) {
                        $('.kiosk-phoneentry').focus();
                    }
                }

                //
                // account entry
                //
                if ($(".js-pnlaccountentry").is(":visible")) {

                    // set focus
                    if (!isTouchDevice) {
                        $(".input-account .form-control:first").focus();
                    }

                    // setup digits buttons
                    $('.js-pnlaccountentry .tenkey a.digit').on('click', function () {
                        $amount = $(".input-group.active .form-control");
                        $amount.val($amount.val() + $(this).html());
                        return false;
                    });
                    $('.js-pnlaccountentry .tenkey a.clear').on('click', function () {
                        $amount = $(".input-group.active .form-control");
                        $amount.val('');
                        return false;
                    });

                    $('.form-control').on('click', function () {
                        $('.input-group').removeClass("active");
                        $(this).closest('.input-group').addClass("active");
                    });
                }

                //
                // register entry
                //

                if ($(".js-pnlregister").is(":visible")) {
                    $(".input-account .form-control:first").focus();
                }

                //
                // configure the swipe capability
                //
                /*$(document).keypress(function (event) {

                    

                    if ($('#pnlSwipe').is(":visible")) {

                        if ($('#hfSwipe').val() == '') {
                            // Start buffering keypresses when % char is recieved
                            if ((date.getTime() - lastKeyPress) > 500)
                                keyboardBuffer = String.fromCharCode(event.which);

                            // Continue buffering as long as keypresses are within 1/10 sec
                            if ((date.getTime() - lastKeyPress) < 100)
                                keyboardBuffer += String.fromCharCode(event.which);

                            // Stop buffering on CR char
                            if (event.which == 13) {

                                $('#hfSwipe').val(keyboardBuffer);

                                $('#swipeContent').hide();
                                $('#swipeNavButtons').hide();
                                $('#swipeLoading').show();

                                eval($('#swipeNext').attr('href').replace('javascript:', ''));
                            }
                        }

                        event.preventDefault();
                    }
                    else {

                        // If not on the swipe panel, ignore the % key and any keypress that was less 
                        // then 1/10 sec since the last keypress (card scanned)        
                        if (event.which == 37 || ((date.getTime() - lastKeyPress) < 50)) {
                            console.log('Date: ' + date.getTime());
                            console.log('Last Key Press: ' + lastKeyPress);
                            console.log('Delay: ' + (date.getTime() - lastKeyPress));
                            console.log('Key Event: ' + event.which);
                            event.preventDefault();

                        }
                        else {

                            // If enter key is pressed, execute the next buttons postback script
                            if (event.which == 13) {
                                switch (panelName) {
                                    case 'phone':
                                        eval($('#phoneNext').attr('href').replace('javascript:', ''));
                                        break;
                                    case 'family':
                                        break;
                                    case 'register':
                                        eval($('#registerNext').attr('href').replace('javascript:', ''));
                                        break;
                                    case 'amount':
                                        eval($('#amountNext').attr('href').replace('javascript:', ''));
                                        break;
                                }
                            }

                            else if (event.which == 45 && panelName == 'amount')
                                event.preventDefault();
                        }
                    }

                    lastKeyPress = date.getTime();

                });*/
            });
        
        </script>

        <Rock:NotificationBox ID="nbBlockConfigErrors" runat="server" NotificationBoxType="Danger" />

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
                    <div class="row">
                        <div class="col-md-8">
                            <asp:LinkButton ID="lbSearchCancel" runat="server" OnClick="lbSearchCancel_Click" CssClass="btn btn-default btn-kiosk">Cancel</asp:LinkButton>
                        </div>
                        <div class="col-md-4">
                            <asp:LinkButton ID="lbGiveAnonymously" runat="server" OnClick="lbGiveAnonymously_Click" CssClass="btn btn-default btn-kiosk btn-kiosk-lg">Give Anonymously</asp:LinkButton>
                        </div>
                    </div>
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

        <asp:Panel ID="pnlRegister" runat="server" Visible="false" CssClass="js-pnlregister">
            <header>
                <h1>Register</h1>
            </header>
            <main>
                <div class="row">
                    <div class="col-md-8">
                        <Rock:RockTextBox ID="tbFirstName" Label="First Name" runat="server"  Required="true" />
                        <Rock:RockTextBox ID="tbLastName" Label="Last Name" runat="server" Required="true" />
                        <Rock:PhoneNumberBox ID="pnbHomePhone" Label="Home Phone" runat="server" Required="true" />
                        <Rock:AddressControl ID="acAddress" ShowAddressLine2="false" runat="server" Required="true"/>
                        <Rock:EmailBox ID="tbEmail" runat="server" Required="true" />
                    </div>
                    <div class="col-md-4">
                        <asp:LinkButton ID="lbRegisterNext" runat="server" OnClick="lbRegisterNext_Click" CssClass="btn btn-primary btn-kiosk btn-kiosk-lg">Next</asp:LinkButton>
                    </div>
                </div>
            </main>
            <footer>
                <div class="container">
                    <asp:LinkButton ID="lbRegisterBack" runat="server" CausesValidation="false" OnClick="lbRegisterBack_Click" CssClass="btn btn-default btn-kiosk">Back</asp:LinkButton>
                    <asp:LinkButton ID="lbRegisterCancel" runat="server" CausesValidation="false" OnClick="lbRegisterCancel_Click" CssClass="btn btn-default btn-kiosk">Cancel</asp:LinkButton>
                </div>
            </footer>
        </asp:Panel>

        <asp:Panel ID="pnlAccountEntry" runat="server" CssClass="js-pnlaccountentry" Visible="false">
            <header>
                <h1>Enter An Amount</h1>
            </header>
            
            <main>
                <div class="row">
                    <div class="col-md-8">
                        <Rock:NotificationBox ID="nbAccountEntry" runat="server" NotificationBoxType="Warning"></Rock:NotificationBox>

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
                    </div>
                    <div class="col-md-4">
                        <asp:LinkButton ID="lbAccountEntryNext" runat="server" OnClick="lbAccountEntryNext_Click" CssClass="btn btn-primary btn-kiosk btn-kiosk-lg">Next</asp:LinkButton>
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
            <header>
                <h1>Please Swipe Your Card</h1>
            </header>

            <main>
                
                <div>
                    <i class="fa fa-cc-visa fa-2x"></i>
                    <i class="fa fa-cc-mastercard fa-2x"></i>
                    <i class="fa fa-cc-amex fa-2x"></i>
                    <i class="fa fa-cc-discover fa-2x"></i>
                </div>
                <asp:Image ID="imgSwipe" runat="server" ImageUrl="<%$ Fingerprint:~/Assets/Images/Kiosk/card_swipe.png %>" />
                <asp:LinkButton id="lbSwipeNext" runat="server" OnClick="lbSwipeNext_Click" ClientIDMode="Static" CssClass="btn btn-primary btn-kiosk btn-kiosk-lg">Next</asp:LinkButton>
            </main>
            
            <footer>
                <div class="container">
                    <asp:LinkButton ID="lbSwipeBack" runat="server" OnClick="lbSwipeBack_Click" CssClass="btn btn-default btn-kiosk">Back</asp:LinkButton>
                    <asp:LinkButton ID="lbSwipeCancel" runat="server" OnClick="lbSwipeCancel_Click" CssClass="btn btn-default btn-kiosk">Cancel</asp:LinkButton>
                </div>
            </footer>
        </asp:Panel>

        <asp:Panel ID="pnlReceipt" runat="server" ClientIDMode="Static" Visible="false">
            <header>
                <h1>Thank You!</h1>
            </header>
            <main>
                <div class="row">
                    <div class="col-md-8">
                        <asp:Literal id="lReceiptContent" runat="server" />

                        <asp:Literal id="lDebug" runat="server" />
                    </div>
                    <div class="col-md-4">
                        <asp:LinkButton id="lbReceiptDone" runat="server" OnClick="lbReceiptDone_Click" CssClass="btn btn-primary btn-kiosk btn-kiosk-lg">Done</asp:LinkButton>
                    </div>
                </div>
                
                
            </main>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
