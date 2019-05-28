<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduledTransactionEditV2.ascx.cs" Inherits="RockWeb.Blocks.Finance.ScheduledTransactionEditV2" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfScheduledTransactionId" runat="server" />
        <asp:HiddenField ID="hfFinancialGatewayId" runat="server" />
        <Rock:HiddenFieldWithClass ID="hfPaymentInfoVisable" CssClass="js-add-payment-visible" runat="server" />

        <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />

        <%-- Prompt for Changes to Scheduled Transaction --%>
        <asp:Panel ID="pnlPromptForChanges" runat="server">
            <Rock:CampusAccountAmountPicker ID="caapPromptForAccountAmounts" runat="server" />

            <Rock:RockDropDownList ID="ddlFrequency" runat="server" FormGroupCssClass="margin-t-md" />

            <div class="margin-t-md">
                <Rock:DatePicker ID="dtpStartDate" runat="server" Label="Next Gift" />
            </div>

            <Rock:RockControlWrapper ID="rcsPaymentMethod" runat="server" Label="Payment Method">
                <div class="form-control-group">
                    <Rock:RockDropDownList ID="ddlPersonSavedAccount" CssClass="input-width-xxl" runat="server" />
                    <a class="js-add-payment btn btn-default">Add</a>
                </div>
            </Rock:RockControlWrapper>

            <div class="js-add-payment-new margin-b-md" style="display: none">
                <Rock:NotificationBox ID="nbPaymentTokenError" runat="server" NotificationBoxType="Validation" Visible="false" />
                <Rock:DynamicPlaceholder ID="phHostedPaymentControl" runat="server" />

                <Rock:AddressControl ID="acBillingAddress" runat="server" UseStateAbbreviation="true" UseCountryAbbreviation="false" Label="" ShowAddressLine2="false" />
            </div>

            <Rock:NotificationBox ID="nbUpdateScheduledPaymentWarning" runat="server" NotificationBoxType="Validation" Visible="false" />

            <div class="actions">
                <%-- NOTE: When in New Payment mode, btnUpdateScheduledPayment ends up telling the HostedPaymentControl (via the js-submit-hostedpaymentinfo hook) to request a token, which will cause the _hostedPaymentInfoControl_TokenReceived postback --%>
                <Rock:BootstrapButton ID="btnUpdateScheduledPayment" runat="server" Text="Update" CssClass="btn btn-primary js-submit-hostedpaymentinfo" OnClick="btnUpdateScheduledPayment_Click" DataLoadingText="Updating..." />
            </div>
        </asp:Panel>

        <%-- Transaction Summary (step 4) --%>
        <asp:Panel ID="pnlTransactionSummary" runat="server" Visible="false">
            <asp:Literal ID="lTransactionSummaryHTML" runat="server" />
        </asp:Panel>

        <script type="text/javascript">

            function showPaymentInfo(animate) {
                var $addPaymentNew = $('.js-add-payment-new');
                var $addPaymentVisible = $('.js-add-payment-visible');
                var $updateWithSavedAccount = $('.js-update-with-saved-account');

                var showNewPaymentInfo = $addPaymentVisible.val() == 1;
                if (showNewPaymentInfo) {
                    $updateWithSavedAccount.hide();
                    if (animate) {
                        $addPaymentNew.slideDown();
                    }
                    else {
                        $addPaymentNew.show();
                    }
                }
                else {
                    function showUpdateWithSavedAccount() {
                        $updateWithSavedAccount.show();
                    }

                    if (animate) {
                        promise = $addPaymentNew.slideUp(showUpdateWithSavedAccount);
                    }
                    else {
                        promise = $addPaymentNew.hide(showUpdateWithSavedAccount);
                    }
                }
            }

            Sys.Application.add_load(function () {

                showPaymentInfo(false);


                $('.js-submit-hostedpaymentinfo').off().on('click', function (e) {
                    // only get a payment token if prompting for a new payment
                    if ($('.js-add-payment-new').is(":visible")) {
                        debugger
                        e.stopImmediatePropagation();
                        <%=HostPaymentInfoSubmitScript%>
                        return false;
                    }
                });

                $('.js-add-payment').off().on('click', function () {
                    var $addPaymentNew = $('.js-add-payment-new');
                    var $addPaymentVisible = $('.js-add-payment-visible');
                    if ($addPaymentVisible.val() == '1') {
                        $addPaymentVisible.val(0);
                    } else {
                        $addPaymentVisible.val(1);
                    }

                    showPaymentInfo(true);
                })

            });
        </script>



    </ContentTemplate>
</asp:UpdatePanel>
