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
            <asp:HiddenField ID="hfTargetPersonId" runat="server" />

            <div class="row">
                <%-- Scheduled Gifts Panel --%>
                <asp:Panel ID="pnlScheduledTransactions" runat="server" CssClass="col-sm-4 scheduled-transactions" Visible="false">
                    <asp:Literal ID="lScheduledTransactionsHTML" runat="server" />
                </asp:Panel>

                <%-- Transaction Entry Panel --%>
                <div class="col-sm-8">

                    <%-- Collect Transaction Info (step 1) --%>
                    <asp:Panel ID="pnlPromptForAmounts" runat="server">

                        <asp:Literal ID="lIntroMessage" runat="server" />

                        <Rock:CampusAccountAmountPicker ID="caapPromptForAccountAmounts" runat="server" />

                        <asp:Panel ID="pnlScheduledTransaction" runat="server">

                            <Rock:RockDropDownList ID="ddlFrequency" runat="server" FormGroupCssClass=" margin-t-md" AutoPostBack="true" OnSelectedIndexChanged="ddlFrequency_SelectedIndexChanged" />

                            <div class="margin-t-md">
                                <Rock:RockDropDownList ID="ddlPersonSavedAccount" runat="server" Label="Giving Method" />
                                <Rock:DatePicker ID="dtpStartDate" runat="server" Label="First Gift" AllowPastDateSelection="false" />
                            </div>

                        </asp:Panel>

                        <Rock:RockTextBox ID="tbCommentEntry" runat="server" Required="true" Label="Comment" />

                        <Rock:NotificationBox ID="nbPromptForAmountsWarning" runat="server" NotificationBoxType="Validation" Visible="false" />
                        <Rock:BootstrapButton ID="btnGiveNow" runat="server" CssClass="btn btn-primary" Text="Give Now" OnClick="btnGiveNow_Click" />

                        <a id="aHistoryBackButton" runat="server" class="btn btn-link">Previous</a>
                    </asp:Panel>


                    <asp:Panel ID="pnlAmountSummary" runat="server" Visible="false">
                        <div class="amount-summary-account-campus">
                            <asp:Literal runat="server" ID="lAmountSummaryAccounts" />
                            -
                            <asp:Literal runat="server" ID="lAmountSummaryCampus" />
                        </div>
                        <div class="amount-summary-amount">
                            <asp:Literal runat="server" ID="lAmountSummaryAmount" />
                        </div>
                    </asp:Panel>


                    <%-- Collect Payment Info (step 2). Skip this if they using a saved giving method. --%>
                    <asp:Panel ID="pnlPaymentInfo" runat="server" Visible="false">
                        
                        <div class="margin-b-md">
                            <Rock:DynamicPlaceholder ID="phHostedPaymentControl" runat="server" />
                        </div>

                        <Rock:NotificationBox ID="nbPaymentTokenError" runat="server" NotificationBoxType="Validation" Visible="false" />

                        <div class="navigation actions">
                            <asp:LinkButton ID="btnGetPaymentInfoBack" runat="server" CssClass="btn btn-default" Text="Back" OnClick="btnGetPaymentInfoBack_Click" />

                            <%-- NOTE: btnGetPaymentInfoNext ends up telling the HostedPaymentControl (via the js-submit-hostedpaymentinfo hook) to request a token, which will cause the _hostedPaymentInfoControl_TokenReceived postback --%>
                            <a id="btnGetPaymentInfoNext" runat="server" class="btn btn-primary js-submit-hostedpaymentinfo">Next</a>
                        </div>
                    </asp:Panel>

                    <%-- Collect/Update Personal Information (step 3) --%>
                    <asp:Panel ID="pnlPersonalInformation" runat="server" Visible="false">

                        <Rock:Toggle ID="tglIndividualOrBusiness" runat="server" OnText="Business" OffText="Individual" OnCheckedChanged="tglIndividualOrBusiness_CheckedChanged" />

                        <asp:Panel ID="pnlPersonInformationAsIndividual" runat="server">
                            <asp:Panel ID="pnlLoggedInNameDisplay" runat="server">
                                <asp:Literal ID="lCurrentPersonFullName" runat="server" />
                            </asp:Panel>
                            <asp:Panel ID="pnlNotLoggedInNameEntry" runat="server">
                                <Rock:RockTextBox ID="tbFirstName" runat="server" Placeholder="First Name" />
                                <Rock:RockTextBox ID="tbLastName" runat="server" Placeholder="Last Name" />
                            </asp:Panel>

                            <Rock:AddressControl ID="acAddressIndividual" runat="server" UseStateAbbreviation="true" UseCountryAbbreviation="false" Label="" ShowAddressLine2="false" />
                            <Rock:PhoneNumberBox ID="pnbPhoneIndividual" runat="server" Placeholder="Phone" />
                            <Rock:EmailBox ID="tbEmailIndividual" runat="server" Placeholder="Email" />
                            <Rock:RockCheckBox ID="cbGiveAnonymouslyIndividual" runat="server" Text="Give Anonymously" />
                        </asp:Panel>

                        <asp:Panel ID="pnlPersonInformationAsBusiness" runat="server" Visible="false">
                            <Rock:RockRadioButtonList ID="cblSelectBusiness" runat="server" Label="Business" RepeatDirection="Horizontal" AutoPostBack="true" OnSelectedIndexChanged="cblSelectBusiness_SelectedIndexChanged" />
                            <Rock:RockTextBox ID="tbBusinessName" runat="server" Placeholder="Business Name" />
                            <Rock:AddressControl ID="acAddressBusiness" runat="server" UseStateAbbreviation="true" UseCountryAbbreviation="false" Label="" ShowAddressLine2="false" />
                            <Rock:PhoneNumberBox ID="pnbPhoneBusiness" runat="server" Placeholder="Business Phone" />
                            <Rock:EmailBox ID="tbEmailBusiness" runat="server" Placeholder="Business Email" />
                            <Rock:RockCheckBox ID="cbGiveAnonymouslyBusiness" runat="server" Text="Give Anonymously" />

                            <%-- If anonymous and giving as a new business, prompt for Contact information --%>
                            <asp:Panel ID="pnlBusinessContactAnonymous" runat="server" Visible="false">
                                <hr />
                                <h4>Business Contact</h4>
                                <div class="row">
                                    <div class="col-sm-6">
                                        <Rock:RockTextBox ID="tbBusinessContactFirstName" runat="server" Placeholder="First Name" />
                                    </div>
                                    <div class="col-sm-6">
                                        <Rock:RockTextBox ID="tbBusinessContactLastName" runat="server" Placeholder="Last Name" />
                                    </div>
                                </div>
                                <Rock:PhoneNumberBox ID="pnbBusinessContactPhone" runat="server" Placeholder="Phone"></Rock:PhoneNumberBox>
                                <Rock:RockTextBox ID="tbBusinessContactEmail" runat="server" Placeholder="Email"></Rock:RockTextBox>
                            </asp:Panel>
                        </asp:Panel>

                        <Rock:NotificationBox ID="nbProcessTransactionError" runat="server" NotificationBoxType="Danger" Visible="false" />

                        <div class="navigate-actions actions">
                            <asp:LinkButton ID="btnPersonalInformationBack" runat="server" CssClass="btn btn-default" Text="Back" OnClick="btnPersonalInformationBack_Click" />
                            <Rock:BootstrapButton ID="btnPersonalInformationNext" runat="server" CssClass="btn btn-primary" Text="Finish" OnClick="btnPersonalInformationNext_Click" />
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
                                                <b>Note:</b> For security purposes you will need to login to use your saved account information. To create
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

            Sys.Application.add_load(function () {

                $('.js-submit-hostedpaymentinfo').off().on('click', function () {
                    <%=HostPaymentInfoSubmitScript%>
                });

                if ($('.js-save-account').length > 0) {

                    showSaveAccount();

                    // Hide or show a div based on selection of checkbox
                    $('.js-save-account').on('click', function () {
                        showSaveAccount();
                    });
                }

            });
        </script>


    </ContentTemplate>
</asp:UpdatePanel>
