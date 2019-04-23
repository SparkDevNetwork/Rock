<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TextToGiveSetup.ascx.cs" Inherits="RockWeb.Blocks.Finance.TextToGiveSetup" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <%-- Message for any configuration warnings --%>
        <Rock:NotificationBox ID="nbConfigurationNotification" runat="server" Visible="false" />
        <Rock:NotificationBox ID="nbInvalidPersonWarning" runat="server" Visible="false" />

        <%-- Friendly Help if there is no Gateway configured --%>
        <asp:Panel ID="pnlGatewayHelp" runat="server" Visible="false">
            <h4>Welcome to Rock's SMS Giving Setup Experience</h4>
            <p>There is currently no gateway configured. Below are a list of supported gateways installed on your server. You can also add additional gateways through the Rock Shop.</p>
            <asp:Repeater ID="rptInstalledGateways" runat="server" OnItemDataBound="rptInstalledGateways_ItemDataBound">
                <ItemTemplate>
                    <div class="panel panel-block">
                        <div class="panel-body">
                            <asp:HiddenField ID="hfGatewayEntityTypeId" runat="server" />
                            <h4>
                                <asp:Literal ID="lGatewayName" runat="server" />
                            </h4>
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

                <%-- Transaction Entry Panel --%>
                <div class="col-sm-8">

                    <%-- Collect Transaction Info (step 1) --%>
                    <asp:Panel ID="pnlPromptForAmounts" runat="server">

                        <asp:Literal ID="lIntroMessage" runat="server" />

                        <Rock:CampusAccountAmountPicker ID="caapPromptForAccountAmounts" runat="server" />
                        <Rock:RockDropDownList ID="ddlPersonSavedAccount" runat="server" Label="Giving Method" />

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

                        <Rock:RockTextBox ID="tbSaveAccount" runat="server" Label="Name for this account" CssClass="input-large" Required="true" ValidationGroup="vgSaveAccount" />

                        <Rock:NotificationBox ID="nbPaymentTokenError" runat="server" NotificationBoxType="Validation" Visible="false" />

                        <div class="navigation actions">
                            <asp:LinkButton ID="btnGetPaymentInfoBack" runat="server" CssClass="btn btn-default" Text="Back" OnClick="btnGetPaymentInfoBack_Click" />

                            <%-- NOTE: btnGetPaymentInfoNext ends up telling the HostedPaymentControl (via the js-submit-hostedpaymentinfo hook) to request a token, which will cause the _hostedPaymentInfoControl_TokenReceived postback --%>
                            <a id="btnGetPaymentInfoNext" runat="server" class="btn btn-primary js-submit-hostedpaymentinfo">Next</a>
                        </div>
                    </asp:Panel>

                    <%-- Collect/Update Personal Information (step 3) --%>
                    <asp:Panel ID="pnlPersonalInformation" runat="server" Visible="false">

                        <asp:Panel ID="pnlPersonInformationAsIndividual" runat="server">
                            <asp:Panel ID="pnlLoggedInNameDisplay" runat="server">
                                <asp:Literal ID="lCurrentPersonFullName" runat="server" />
                            </asp:Panel>
                            <asp:Panel ID="pnlNotLoggedInNameEntry" runat="server">
                                <Rock:RockTextBox ID="tbFirstName" runat="server" Placeholder="First Name" />
                                <Rock:RockTextBox ID="tbLastName" runat="server" Placeholder="Last Name" />
                            </asp:Panel>

                            <Rock:AddressControl ID="acAddressIndividual" runat="server" UseStateAbbreviation="true" UseCountryAbbreviation="false" Label="" ShowAddressLine2="false" />
                            <Rock:EmailBox ID="tbEmailIndividual" runat="server" Placeholder="Email" />
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