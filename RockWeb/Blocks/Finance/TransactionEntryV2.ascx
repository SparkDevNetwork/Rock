<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionEntryV2.ascx.cs" Inherits="RockWeb.Blocks.Finance.TransactionEntryV2" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <%-- Message for any configuration warnings --%>
        <Rock:NotificationBox ID="nbConfigurationNotification" runat="server" Visible="false" />

        <Rock:NotificationBox ID="nbInvalidPersonWarning" runat="server" Visible="false" />


        <%-- Friendly Help if there is no Gateway configured --%>
        <asp:Panel ID="pnlGatewayHelp" runat="server" Visible="false">
            <h4>Welcome to Rock's On-line Giving Experience</h4>
            <p>There is currently no gateway configured. Below are a list of supported gateways installed on your server. You can also add additional gateways through the Rock Shop.</p>
            <asp:Repeater ID="rptInstalledGateways" runat="server" OnItemDataBound="rptInstalledGateways_ItemDataBound">
                <ItemTemplate>
                    <div class="panel panel-block">
                        <div class="panel-body">
                            <asp:HiddenField ID="hfGatewayEntityTypeId" runat="server" />
                            <h4>
                                <asp:Literal ID="lGatewayName" runat="server" /></h4>
                            <p>
                                <asp:Literal ID="lGatewayDescription" runat="server" />
                            </p>
                            <div class="actions">
                                <asp:HyperLink ID="aGatewayConfigure" runat="server" CssClass="btn btn-xs btn-success" Text="Configure" />
                                <asp:HyperLink ID="aGatewayLearnMore" runat="server" CssClass="btn btn-xs btn-link" Text="Learn More" />
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </asp:Panel>

        <asp:Panel ID="pnlTransactionEntry" runat="server">
            <div class="row">
                <%-- Transaction Entry Panel --%>
                <asp:Panel ID="pnlTransactionEntryPanel" runat="server" CssClass="col-xs-12 col-sm-8">
                    <div class="transaction-entry-v2">

                        <%-- Collect Transaction Info (step 1) --%>
                        <asp:Panel ID="pnlPromptForAmounts" runat="server">

                            <asp:Literal ID="lIntroMessage" runat="server" />

                            <Rock:CampusAccountAmountPicker ID="caapPromptForAccountAmounts" runat="server" />

                            <asp:Panel ID="pnlScheduledTransactionFrequency" runat="server">
                                <div class="form-group">
                                    <Rock:RockDropDownList ID="ddlFrequency" runat="server" Label="Frequency" AutoPostBack="true" OnSelectedIndexChanged="ddlFrequency_SelectedIndexChanged" />
                                </div>
                            </asp:Panel>

                            <asp:Panel ID="pnlSavedAccounts" runat="server" class="form-group" Visible="false">
                                <Rock:RockDropDownList ID="ddlPersonSavedAccount" runat="server" Label="Giving Method" AutoPostBack="true" OnSelectedIndexChanged="ddlPersonSavedAccount_SelectedIndexChanged" />
                            </asp:Panel>

                            <asp:Panel ID="pnlScheduledTransactionStartDate" runat="server">
                                <Rock:DatePicker ID="dtpStartDate" runat="server" AllowPastDateSelection="false" Label="Start Date" />
                            </asp:Panel>

                            <Rock:RockTextBox ID="tbCommentEntry" runat="server" Required="true" Label="Comment" />

                            <Rock:NotificationBox ID="nbPromptForAmountsWarning" runat="server" NotificationBoxType="Validation" Visible="false" />

                            <Rock:HiddenFieldWithClass ID="hfCoverTheFeeCreditCardPercent" runat="server" CssClass="js-coverthefee-percent" Value="" />

                            <%-- Cover the Fee checkbox (When a Saved Account is selected and we know the currency type already) --%>
                            <asp:Panel ID="pnlGiveNowCoverTheFee" runat="server" CssClass="js-coverthefee-container" Visible="false">
                                <Rock:RockCheckBox ID="cbGiveNowCoverTheFee" runat="server" Text="$<span class='js-coverthefee-checkbox-fee-amount-text'></span>" CssClass="js-givenow-coverthefee" />
                            </asp:Panel>

                            <Rock:Captcha ID="cpCaptcha" runat="server" />

                            <Rock:BootstrapButton ID="btnGiveNow" runat="server" CssClass="btn btn-primary btn-give-now" Text="Give Now" OnClick="btnGiveNow_Click" />

                            <a id="aHistoryBackButton" runat="server" class="btn btn-link">Previous</a>
                        </asp:Panel>

                        <%-- Show Amount Summary (step 2)--%>
                        <asp:Panel ID="pnlAmountSummary" runat="server" Visible="false" CssClass="amount-summary">
                            <div class="amount-summary-text">
                                <asp:Literal ID="lAmountSummaryText" runat="server" />
                            </div>
                            <div class="amount-display">
                                <Rock:HiddenFieldWithClass ID="hfAmountWithoutCoveredFee" runat="server" CssClass="js-amount-without-covered-fee" />

                                <span class="js-account-summary-amount">
                                    <asp:Literal runat="server" ID="lAmountSummaryAmount" /></span>
                            </div>
                        </asp:Panel>

                        <%-- Collect Payment Info (step 2). Skip this if they using a saved giving method. --%>
                        <asp:Panel ID="pnlPaymentInfo" runat="server" Visible="false">
                            <div class="hosted-payment-control">
                                <Rock:DynamicPlaceholder ID="phHostedPaymentControl" runat="server" />
                            </div>

                            <Rock:NotificationBox ID="nbPaymentTokenError" runat="server" NotificationBoxType="Validation" Visible="false" />

                            <%-- Cover the Fee checkbox (When a Saved Account is not selected and we know the amount already) --%>
                            <asp:Panel ID="pnlGetPaymentInfoCoverTheFeeCreditCard" runat="server">
                                <Rock:RockCheckBox ID="cbGetPaymentInfoCoverTheFeeCreditCard" runat="server" Text="##" CssClass="js-getpaymentinfo-select-coverthefee-creditcard" />
                                <Rock:HiddenFieldWithClass ID="hfAmountWithCoveredFeeCreditCard" runat="server" CssClass="js-amount-with-covered-fee-creditcard" />
                            </asp:Panel>

                            <asp:Panel ID="pnlGetPaymentInfoCoverTheFeeACH" runat="server">
                                <Rock:RockCheckBox ID="cbGetPaymentInfoCoverTheFeeACH" runat="server" Text="##" CssClass="js-getpaymentinfo-select-coverthefee-ach" />
                                <Rock:HiddenFieldWithClass ID="hfAmountWithCoveredFeeACH" runat="server" CssClass="js-amount-with-covered-fee-ach" />
                            </asp:Panel>

                            <div class="navigation actions">
                                <asp:LinkButton ID="btnGetPaymentInfoBack" runat="server" CssClass="btn btn-default" Text="Back" OnClick="btnGetPaymentInfoBack_Click" />

                                <Rock:HiddenFieldWithClass ID="hfHostPaymentInfoSubmitScript" runat="server" CssClass="js-hosted-payment-script" />
                                <%-- NOTE: btnGetPaymentInfoNext ends up telling the HostedPaymentControl (via the js-submit-hostedpaymentinfo hook) to request a token, which will cause the _hostedPaymentInfoControl_TokenReceived postback
                               		Even though this is a LinkButton, btnGetPaymentInfoNext won't autopostback  (see $('.js-submit-hostedpaymentinfo').off().on('click').. )
                                --%>
                                <Rock:BootstrapButton ID="btnGetPaymentInfoNext" runat="server" Text="Next" CssClass="btn btn-primary js-submit-hostedpaymentinfo pull-right" DataLoadingText="Processing..." />
                            </div>
                        </asp:Panel>

                        <%-- Collect/Update Personal Information (step 3) --%>
                        <asp:Panel ID="pnlPersonalInformation" runat="server" Visible="false">

                            <Rock:Toggle ID="tglIndividualOrBusiness" runat="server" ButtonGroupCssClass="btn-group-justified" OnText="Business" OffText="Individual" OnCheckedChanged="tglIndividualOrBusiness_CheckedChanged" />

                            <asp:Panel ID="pnlPersonInformationAsIndividual" runat="server">
                                <asp:Panel ID="pnlLoggedInNameDisplay" runat="server">
                                    <div class="form-control-static">
                                        <asp:Literal ID="lCurrentPersonFullName" runat="server" />
                                    </div>
                                </asp:Panel>
                                <asp:Panel ID="pnlNotLoggedInNameEntry" runat="server">
                                    <div class="form-group">
                                        <Rock:FirstNameTextBox ID="tbFirstName" runat="server" Placeholder="First Name" CssClass="margin-b-sm" Required="true" NoSpecialCharacters="true" NoEmojisOrSpecialFonts="true" DisplayInlineValidationError="true" />
                                    </div>
                                    <div class="form-group">
                                        <Rock:RockTextBox ID="tbLastName" runat="server" Placeholder="Last Name" CssClass="margin-b-sm" Required="true" NoSpecialCharacters="true" NoEmojisOrSpecialFonts="true" />
                                    </div>
                                </asp:Panel>

                                <Rock:AddressControl ID="acAddressIndividual" runat="server" UseStateAbbreviation="true" UseCountryAbbreviation="false" Label="" ShowAddressLine2="false" Required="true" />

                                <Rock:PhoneNumberBox ID="pnbPhoneIndividual" runat="server" Placeholder="Phone" CssClass="margin-b-sm" />
                                <div class="form-group">
                                    <Rock:EmailBox ID="tbEmailIndividual" runat="server" Placeholder="Email" CssClass="margin-b-sm" Required="true"/>
                                </div>
                                <Rock:RockCheckBox ID="cbGiveAnonymouslyIndividual" runat="server" Text="Give Anonymously" />
                            </asp:Panel>

                            <asp:Panel ID="pnlPersonInformationAsBusiness" runat="server" Visible="false">
                                <Rock:RockRadioButtonList ID="cblSelectBusiness" runat="server" Label="Business" RepeatDirection="Horizontal" AutoPostBack="true" OnSelectedIndexChanged="cblSelectBusiness_SelectedIndexChanged" />

                                <div class="form-group">
                                    <Rock:RockTextBox ID="tbBusinessName" runat="server" Placeholder="Business Name" Required="true" />
                                </div>

                                <Rock:AddressControl ID="acAddressBusiness" runat="server" UseStateAbbreviation="true" UseCountryAbbreviation="false" Label="" ShowAddressLine2="false" Required="true" />
                                <Rock:PhoneNumberBox ID="pnbPhoneBusiness" runat="server" Placeholder="Business Phone" CssClass="margin-b-sm" />
                                <div class="form-group">
                                    <Rock:EmailBox ID="tbEmailBusiness" runat="server" Placeholder="Business Email" CssClass="margin-b-sm required" Required="true" />
                                 </div>
                                <Rock:RockCheckBox ID="cbGiveAnonymouslyBusiness" runat="server" Text="Give Anonymously" />

                                <%-- If anonymous and giving as a new business, prompt for Contact information --%>
                                <asp:Panel ID="pnlBusinessContactAnonymous" runat="server" Visible="false">
                                    <hr />
                                    <h4>Business Contact</h4>
                                    <div class="form-group">
                                        <Rock:FirstNameTextBox ID="tbBusinessContactFirstName" runat="server" Placeholder="First Name" CssClass="margin-b-sm" Required="true" NoSpecialCharacters="true" NoEmojisOrSpecialFonts="true" />
                                    </div>
                                    <div class="form-group">
                                        <Rock:RockTextBox ID="tbBusinessContactLastName" runat="server" Placeholder="Last Name" CssClass="margin-b-sm" Required="true" NoSpecialCharacters="true" NoEmojisOrSpecialFonts="true" />
                                    </div>
                                    <Rock:PhoneNumberBox ID="pnbBusinessContactPhone" runat="server" Placeholder="Phone" CssClass="margin-b-sm" />
                                    <div class="form-group">
                                        <Rock:EmailBox ID="tbBusinessContactEmail" runat="server" Placeholder="Email" CssClass="margin-b-sm" Required="true" />
                                    </div>
                                </asp:Panel>
                            </asp:Panel>

                            <Rock:NotificationBox ID="nbProcessTransactionError" runat="server" NotificationBoxType="Danger" Visible="false" />

                            <div class="navigation actions margin-t-md">
                                <asp:LinkButton ID="btnPersonalInformationBack" runat="server" CssClass="btn btn-default" Text="Back" CausesValidation="false" OnClick="btnPersonalInformationBack_Click" />
                                <Rock:BootstrapButton ID="btnPersonalInformationNext" runat="server" CssClass="btn btn-primary pull-right" Text="Finish" OnClick="btnPersonalInformationNext_Click" />
                            </div>
                        </asp:Panel>

                        <%-- Transaction Summary (step 4) --%>
                        <asp:Panel ID="pnlTransactionSummary" runat="server" Visible="false">
                            <asp:HiddenField ID="hfTransactionGuid" runat="server" />
                            <asp:Literal ID="lTransactionSummaryHTML" runat="server" />

                            <%-- Make Giving Even Easier --%>
                            <asp:Panel ID="pnlSaveAccountPrompt" runat="server" Visible="false" CssClass="margin-t-xl">

                                <h3>
                                    <asp:Literal ID="lSaveAccountTitle" runat="server" /></h3>
                                <Rock:RockCheckBox ID="cbSaveAccount" runat="server" Text="Save account information for future gifts" CssClass="js-save-account" />

                                <asp:Panel ID="pnlSaveAccountEntry" runat="server" class="js-save-account-entry">
                                    <Rock:RockTextBox ID="tbSaveAccount" runat="server" Label="Name for this account" CssClass="input-large" Required="true" ValidationGroup="vgSaveAccount" />

                                    <asp:Panel ID="pnlCreateLogin" runat="server" Visible="false">

                                        <div class="control-group">
                                            <div class="controls">
                                                <div class="alert alert-info">
                                                    <b>Note:</b> For security purposes you will need to log in to use your saved account information. To create
	    			                    a login account please provide a user name and password below. You will be sent an email with the account
	    			                    information above as a reminder.
                                                </div>
                                            </div>
                                        </div>

                                        <Rock:RockTextBox ID="tbUserName" runat="server" Label="Username" CssClass="input-medium" Required="true" ValidationGroup="vgSaveAccount" />
                                        <Rock:RockTextBox ID="tbPassword" runat="server" Label="Password" CssClass="input-medium" Required="true" TextMode="Password" ValidationGroup="vgSaveAccount" />
                                        <Rock:RockTextBox ID="tbPasswordConfirm" runat="server" Label="Confirm Password" CssClass="input-medium" TextMode="Password" Required="true" ValidationGroup="vgSaveAccount" />

                                    </asp:Panel>

                                    <Rock:NotificationBox ID="nbSaveAccountError" runat="server" Visible="false" NotificationBoxType="Danger" />

                                    <div id="divSaveActions" runat="server" class="actions">
                                        <Rock:BootstrapButton ID="btnSaveAccount" runat="server" Text="Save Account" CssClass="btn btn-primary" ValidationGroup="vgSaveAccount" OnClick="btnSaveAccount_Click" DataLoadingText="Saving..." />
                                    </div>
                                </asp:Panel>

                                <Rock:NotificationBox ID="nbSaveAccountSuccess" runat="server" Visible="false" NotificationBoxType="Success" />

                            </asp:Panel>
                        </asp:Panel>

                    </div>
                </asp:Panel>

                <%-- Scheduled Gifts Panel --%>
                <asp:Panel ID="pnlScheduledTransactions" runat="server" CssClass="col-xs-12 col-sm-4 scheduled-transactions" Visible="false">
                    <asp:Literal ID="lScheduledTransactionsHTML" runat="server" />
                </asp:Panel>
            </div>

        </asp:Panel>

        <script type="text/javascript">

            function showSaveAccount(animate) {
                var show = $('.js-save-account').is(':checked');
                var $savedAccountEntry = $('.js-save-account-entry');
                if (show) {
                    if (animate) {
                        $savedAccountEntry.slideDown();
                    }
                    else {
                        $savedAccountEntry.show();
                    }
                }
                else {
                    if (animate) {
                        $savedAccountEntry.slideUp();
                    }
                    else {
                        $savedAccountEntry.hide();
                    }
                }
            }

            function toggleCoverTheFeeSummaryAmount() {
                var showWithFee = $('.js-getpaymentinfo-select-coverthefee-creditcard, .js-getpaymentinfo-select-coverthefee-ach').is(':checked');
                var $amountSummaryAmount = $('.js-account-summary-amount');
                var $amountWithFee = $('.js-amount-with-covered-fee-creditcard, .js-amount-with-covered-fee-ach');
                var $amountWithoutFee = $('.js-amount-without-covered-fee')
                if (showWithFee) {
                    $amountSummaryAmount.text($amountWithFee.val());
                }
                else {
                    $amountSummaryAmount.text($amountWithoutFee.val())
                }
            }

            function updateCoverTheFeePercent(feePercent) {
                var $coverTheFeeContainer = $('.js-coverthefee-container');
                var $coverTheFeeAmountText = $('.js-coverthefee-checkbox-fee-amount-text');

                var totalAmt = Number(0);

                var $amountInputs = $('input.js-amount-input, .js-amount-input input');

                $amountInputs.each(function (index) {
                    var itemValue = $(this).val();
                    if (itemValue && !isNaN(itemValue)) {
                        var num = Number(itemValue);
                        totalAmt = totalAmt + num;
                    }
                });

                var decimalPlaces = $coverTheFeeAmountText.attr('decimal-places');
                if (!decimalPlaces && decimalPlaces != 0) {
                    decimalPlaces = 2;
                }
                console.log(decimalPlaces);
                var feeAmount = (totalAmt * (feePercent / 100)).toFixed(decimalPlaces);
                var displayFeeAmount = (totalAmt * (feePercent / 100)).toLocaleString(undefined, { minimumFractionDigits: decimalPlaces, maximumFractionDigits: decimalPlaces });

                $coverTheFeeAmountText.html(displayFeeAmount);
                if (feeAmount > 0) {
                    $coverTheFeeContainer.show();
                }
                else {
                    $coverTheFeeContainer.hide();
                }
            }

            Sys.Application.add_load(function () {

                $('.js-submit-hostedpaymentinfo').off().on('click', function (e) {
                    // Prevent the btnGetPaymentInfoNext autopostback event from firing by doing stopImmediatePropagation and returning false
                    e.stopImmediatePropagation();

                    const hfHostedPaymentScript = document.querySelector(".js-hosted-payment-script");
                    window.location = "javascript: " + hfHostedPaymentScript.value;

                    return false;
                });

                if ($('.js-save-account').length > 0) {

                    showSaveAccount();

                    // Hide or show a div based on selection of checkbox
                    $('.js-save-account').on('click', function () {
                        showSaveAccount();
                    });
                }

                var $paymentInfoCoverTheFeeCheckbox = $('.js-getpaymentinfo-select-coverthefee-creditcard, .js-getpaymentinfo-select-coverthefee-ach');
                if ($paymentInfoCoverTheFeeCheckbox.length) {
                    toggleCoverTheFeeSummaryAmount();

                    $paymentInfoCoverTheFeeCheckbox.off().on('click', function () {
                        toggleCoverTheFeeSummaryAmount();
                    });
                }

                var coverTheFeePercent = Number($('.js-coverthefee-percent').val()) || 0.00;
                if (coverTheFeePercent > 0.00) {
                    updateCoverTheFeePercent(coverTheFeePercent);

                    // selector for input elements which could be either a single input or multiple account mode
                    var $amountInputs = $('input.js-amount-input, .js-amount-input input');

                    // As amounts are entered, update the 'cover the fees' checkbox text
                    // Do it on 'keyup' instead of 'change'. Otherwise, they might not see if if they go straight to the GiveNow button
                    $amountInputs.keyup(function () {
                        updateCoverTheFeePercent(coverTheFeePercent);
                    });
                }
            });
        </script>


    </ContentTemplate>
</asp:UpdatePanel>
