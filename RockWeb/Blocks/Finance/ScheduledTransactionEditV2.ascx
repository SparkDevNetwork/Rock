<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduledTransactionEditV2.ascx.cs" Inherits="RockWeb.Blocks.Finance.ScheduledTransactionEditV2" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfScheduledTransactionGuid" runat="server" />
        <asp:HiddenField ID="hfFinancialGatewayId" runat="server" />
        <Rock:HiddenFieldWithClass ID="hfChangePaymentInfoVisible" CssClass="js-change-paymentinfo-visible" runat="server" />
        <Rock:HiddenFieldWithClass ID="hfSaveNewAccount" CssClass="js-save-new-account" runat="server" />

        <%-- Message for any configuration warnings --%>
        <Rock:NotificationBox ID="nbConfigurationNotification" runat="server" Visible="false" />

        <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />

        <%-- Prompt for Changes to Scheduled Transaction --%>
        <asp:Panel ID="pnlPromptForChanges" runat="server">
            <Rock:CampusAccountAmountPicker ID="caapPromptForAccountAmounts" runat="server" />

            <Rock:ButtonDropDownList ID="btnAddAccount" runat="server" Visible="false" Label=" "
                DataTextField="PublicName" DataValueField="Guid" OnSelectionChanged="btnAddAccount_SelectionChanged" />

            <Rock:RockDropDownList ID="ddlFrequency" runat="server" Label="Frequency" FormGroupCssClass="margin-t-md" />

            <div class="margin-t-md">
                <Rock:DatePicker ID="dtpStartDate" runat="server" Label="Next Gift" />
            </div>

            <Rock:RockControlWrapper ID="rcsPaymentMethod" runat="server" FormGroupCssClass="form-group-auto" Label="Payment Method">

                <%-- If no saved account, choose between existing, or click change to show hosted payment --%>
                <asp:Panel ID="pnlUseExistingPaymentNoSavedAccounts" runat="server" CssClass="js-use-existing-payment-method-no-saved-accounts">
                    <asp:Literal ID="lUseExistingPaymentMethodNoSavedAccounts" runat="server" /><asp:LinkButton runat="server" ID="btnChangeFromExistingToHostedPayment" CssClass="margin-l-sm" OnClick="btnChangeToHostedPayment_Click">Change</asp:LinkButton>
                </asp:Panel>

                <%-- If the are saved accounts, choose between existing, a saved account, or click 'add method' to show hosted paymemnt --%>
                <asp:Panel ID="pnlUseExistingPaymentWithSavedAccounts" runat="server" CssClass="js-use-existing-payment-method-with-saved-accounts">
                    <Rock:RockRadioButtonList ID="rblExistingPaymentOrPersonSavedAccount" runat="server" Required="true" />
                    <asp:LinkButton runat="server" ID="btnChangeFromExistingOrSavedAccountToHostedPayment" OnClick="btnChangeToHostedPayment_Click">Add Method</asp:LinkButton>
                </asp:Panel>

                <%-- If clicking 'change' on using existing, or clicking 'add method', show hosted payment  --%>
                <asp:Panel ID="pnlHostedPaymentControl" CssClass="js-hosted-payment-control margin-t-md" runat="server">
                    <Rock:RockControlWrapper ID="rcwHostedPaymentControl" runat="server" Label="Add New Account">
                        <Rock:NotificationBox ID="nbPaymentTokenError" runat="server" NotificationBoxType="Validation" Visible="false" />
                        <Rock:DynamicPlaceholder ID="phHostedPaymentControl" runat="server" />
                        <Rock:AddressControl ID="acBillingAddress" runat="server" UseStateAbbreviation="true" UseCountryAbbreviation="false" Label="" ShowAddressLine2="false" />
                    </Rock:RockControlWrapper>

                    <%-- Save as Saved Account --%>
                    <a class="js-show-saved-account-prompt">Save Account</a>
                    <div class="js-saved-account-prompt margin-t-md">
                        <Rock:RockTextBox ID="tbSaveAccount" runat="server" Label="Account Name" Required="false" Help="To save this payment information as a saved account, specify a name for the saved account." />
                    </div>
                </asp:Panel>

            </Rock:RockControlWrapper>

            <Rock:NotificationBox ID="nbUpdateScheduledPaymentWarning" runat="server" NotificationBoxType="Validation" Visible="false" />

            <div class="actions">
                <%-- NOTE: When in New Payment mode, btnUpdateScheduledPayment ends up telling the HostedPaymentControl (via the js-submit-hostedpaymentinfo hook) to request a token, which will cause the _hostedPaymentInfoControl_TokenReceived postback
                    btnUpdateScheduledPayment_Click will only fire if using a saved payment (see $('.js-submit-hostedpaymentinfo').off().on('click').. )
                --%>
                <Rock:BootstrapButton ID="btnUpdateScheduledPayment" runat="server" Text="Update" CssClass="btn btn-primary js-submit-hostedpaymentinfo margin-t-md" OnClick="btnUpdateScheduledPayment_Click" DataLoadingText="Updating..." />
            </div>
        </asp:Panel>

        <%-- Transaction Summary (step 4) --%>
        <asp:Panel ID="pnlTransactionSummary" runat="server" Visible="false">
            <asp:Literal ID="lTransactionSummaryHTML" runat="server" />
        </asp:Panel>

        <script type="text/javascript">

            Sys.Application.add_load(function () {

                $('.js-submit-hostedpaymentinfo').off().on('click', function (e) {
                    // only get a payment token if prompting for a new payment
                    if ($('.js-hosted-payment-control').is(":visible")) {
                        // prevent the btnUpdateScheduledPayment_Click autopostback event from firing by doing stopImmediatePropagation and returning false
                        e.stopImmediatePropagation();
                        <%=HostPaymentInfoSubmitScript%>
                        return false;
                    }
                });

                var $savedAccountPrompt = $('.js-saved-account-prompt');
                $savedAccountPrompt.hide();

                $('.js-show-saved-account-prompt').off().on('click', function () {
                    if ($savedAccountPrompt.is(':visible')) {
                        $savedAccountPrompt.slideUp();
                        $('.js-save-new-account').val(0);
                    }
                    else {
                        $savedAccountPrompt.slideDown();
                        $('.js-save-new-account').val(1);
                    }
                })
                
            });
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
