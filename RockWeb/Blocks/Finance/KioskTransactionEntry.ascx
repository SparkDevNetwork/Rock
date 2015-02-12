<%@ Control Language="C#" AutoEventWireup="true" CodeFile="KioskTransactionEntry.ascx.cs" Inherits="RockWeb.Blocks.Finance.KioskTransactionEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <script>
        
            Sys.Application.add_load(function () {
                $('.tenkey a.digit').click(function () {
                    $phoneNumber = $("input[id$='tbPhone']");
                    $phoneNumber.val($phoneNumber.val() + $(this).html());
                });
                $('.tenkey a.back').click(function () {
                    $phoneNumber = $("input[id$='tbPhone']");
                    $phoneNumber.val($phoneNumber.val().slice(0,-1));
                });
                $('.tenkey a.clear').click(function () {
                    $phoneNumber = $("input[id$='tbPhone']");
                    $phoneNumber.val('');
                });

                // set focus to the input unless on a touch device
                var isTouchDevice = 'ontouchstart' in document.documentElement;
                if (!isTouchDevice) {
                    $('.checkin-phone-entry').focus();
                }
            });
        
        </script>

        <asp:Panel ID="pnlSearch" runat="server">

            <div class="row">
                <div class="col-md-8">
                    
                    <h1>Please Enter Your Phone Number</h1>

                    <Rock:NotificationBox ID="nbSearch" runat="server" NotificationBoxType="Warning"></Rock:NotificationBox>
                    
                    <Rock:RockTextBox ID="tbPhone" MaxLength="10" CssClass="checkin-phone-entry" runat="server" Label="Phone Number" />

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
                    <asp:LinkButton ID="lbSearchNext" runat="server" OnClick="lbSearchNext_Click" CssClass="btn btn-primary btn-lg">Next</asp:LinkButton>
                </div>
            </div>
            
            <footer>
                <asp:LinkButton ID="lbSearchCancel" runat="server" OnClick="lbSearchCancel_Click" CssClass="btn btn-default">Cancel</asp:LinkButton>
            </footer>
            
        </asp:Panel>

        <asp:Panel ID="pnlFamilySelect" runat="server" Visible="false">
            <h1>Select Your Family</h1>
            family select

            <footer>
                <asp:LinkButton ID="lbFamilySelectBack" runat="server" OnClick="lbFamilySelectBack_Click" CssClass="btn btn-default">Back</asp:LinkButton>
                <asp:LinkButton ID="lbFamilySelectCancel" runat="server" OnClick="lbFamilySelectCancel_Click" CssClass="btn btn-default">Cancel</asp:LinkButton>
            </footer>
        </asp:Panel>

        <asp:Panel ID="pnlRegister" runat="server" Visible="false">

        </asp:Panel>

        <asp:Panel ID="pnlAccountEntry" runat="server" Visible="false">

        </asp:Panel>

        <asp:Panel ID="pnlSwipe" runat="server" Visible="false">

        </asp:Panel>

        <asp:Panel ID="pnlReceipt" runat="server" Visible="false">

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
