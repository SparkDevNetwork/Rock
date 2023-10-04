<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrationPaymentEntry.ascx.cs" Inherits="RockWeb.Plugins.com_simpledonation.Event.RegistrationPaymentEntry" %>
<asp:UpdatePanel ID="upPayment" runat="server">
    <ContentTemplate>
        <%-- hidden field to store the Transaction.Guid to use for the transaction. This is to help prevent duplicate transactions.   --%>
        <asp:HiddenField ID="hfTransactionGuid" runat="server" Value="" />
        <asp:HiddenField ID="hfCurrentPage" runat="server" Value="1" />


        <asp:HiddenField ID="hfPublicKey" runat="server" Value="" ClientIDMode="Static" />
        <asp:HiddenField ID="hfStripeToken" runat="server" Value="" ClientIDMode="Static" />
        <asp:HiddenField ID="hfWalletName" runat="server" Value="" ClientIDMode="Static" />
        <asp:HiddenField ID="hfOrganizationName" runat="server" Value="" ClientIDMode="Static" />
        <asp:HiddenField ID="hfPostbackFromModal" runat="server" Value="" ClientIDMode="Static" />
        <asp:HiddenField ID="hfPaymentAmount" runat="server" Value="" ClientIDMode="Static" />

        <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false"></Rock:NotificationBox>
        <Rock:NotificationBox ID="nbInvalidPersonWarning" runat="server" Visible="false"></Rock:NotificationBox>

        <asp:Panel ID="pnlSelection" CssClass="panel panel-block" runat="server">
            <div class="panel-body">

                <asp:Panel ID="pnlContributionInfo" runat="server">
                    <div class="panel panel-default contribution-info">
                        <div class="panel-heading">
                            <h3 class="panel-title">
                            Schedule Information
                        </div>
                        <div class="panel-body">

                            <Rock:NotificationBox ID="nbExistingScheduledPayments" runat="server" Visible="false"></Rock:NotificationBox>

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockDropDownList ID="ddlRegistrations" runat="server" DataTextField="Text" DataValueField="Value" Label="Registration" AutoPostBack="true" OnSelectedIndexChanged="ddlRegistrations_SelectedIndexChanged" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:DatePicker ID="dtpStartDate" runat="server" Label="First Payment" AutoPostBack="true" AllowPastDateSelection="false" OnTextChanged="dtpStartDate_TextChanged" />
                                    <asp:Literal ID="lStartDate" runat="server" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockLiteral ID="txtFrequency" runat="server" Label="Frequency" Visible="false" />
                                    <Rock:ButtonDropDownList ID="btnFrequency" runat="server" Label="Frequency"
                                        DataTextField="Value" DataValueField="Id" AutoPostBack="true" OnSelectionChanged="btnFrequency_SelectionChanged" />
                                </div>
                                <div class="col-md-6">
                                    <div id="payment-container">
                                        <Rock:RockCheckBox ID="cbCoverFees" CssClass="cover-fees" Label="Cover Fee?" Text='Cover the <span class="payment-offset">$0.00</span> in <span class="payment-type"></span> processing fees' ToolTip="Every donation has a transaction fee deducted - by checking this option you'll cover that fee and we will receive 100% of your original donation amount." runat="server" />
                                        <asp:HiddenField ID="hfAchRate" runat="server" Value="" ClientIDMode="Static" />
                                        <asp:HiddenField ID="hfCardRate" runat="server" Value="" ClientIDMode="Static" />
                                        <asp:HiddenField ID="hfCapAch" runat="server" Value="" ClientIDMode="Static" />
                                        <asp:HiddenField ID="hfFeeAmount" runat="server" Value="" ClientIDMode="Static" />
                                    </div>
                                </div>
                            </div>

                            <asp:Literal ID="lPaymentDates" runat="server" />

                            </fieldset>
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlPayment" runat="server" CssClass="panel panel-default contribution-payment">
                    <div class="panel-heading">
                        <h3 class="panel-title">Payment Information</h3>
                    </div>
                    <div class="panel-body">

                        <asp:HiddenField ID="hfSavedAccounts" runat="server" />
                        <Rock:RockRadioButtonList ID="rblSavedAccount" runat="server" CssClass="radio-list margin-b-lg" RepeatDirection="Vertical" DataValueField="Id" DataTextField="Name" />

                        <div id="divNewPayment" runat="server" class="radio-content">

                            <asp:HiddenField ID="hfPaymentTab" runat="server" />
                            <asp:PlaceHolder ID="phPills" runat="server" Visible="false">
                                <ul class="nav nav-pills">
                                    <li id="liCreditCard" runat="server"><a href='#<%=divCCPaymentInfo.ClientID%>' data-toggle="pill">Card</a></li>
                                    <li id="liACH" runat="server"><a href='#<%=divACHPaymentInfo.ClientID%>' data-toggle="pill">Bank Account</a></li>
                                </ul>
                            </asp:PlaceHolder>

                            <div class="tab-content">

                                <div id="divCCPaymentInfo" runat="server" visible="false" class="tab-pane">
                                    <Rock:RockTextBox ID="txtCardFirstName" runat="server" Label="First Name on Card" Visible="false"></Rock:RockTextBox>
                                    <Rock:RockTextBox ID="txtCardLastName" runat="server" Label="Last Name on Card" Visible="false"></Rock:RockTextBox>
                                    <Rock:RockTextBox ID="txtCardName" runat="server" Label="Name on Card" Visible="false"></Rock:RockTextBox>
                                    <Rock:RockTextBox ID="txtCreditCard" runat="server" Label="Card Number" MaxLength="19" CssClass="credit-card" />
                                    <ul class="card-logos list-unstyled">
                                        <li class="card-visa"></li>
                                        <li class="card-mastercard"></li>
                                        <li class="card-amex"></li>
                                        <li class="card-discover"></li>
                                    </ul>
                                    <div class="row">
                                        <div class="col-md-6">
                                            <Rock:MonthYearPicker ID="mypExpiration" runat="server" Label="Expiration Date" />
                                        </div>
                                        <div class="col-md-6">
                                            <Rock:RockTextBox ID="txtCVV" Label="Card Security Code" CssClass="input-width-xs" runat="server" MaxLength="4" />
                                        </div>
                                    </div>
                                    <Rock:RockCheckBox ID="cbBillingAddress" runat="server" Text="Enter a different billing address" CssClass="toggle-input" />
                                    <div id="divBillingAddress" runat="server" class="toggle-content">
                                        <Rock:AddressControl ID="acBillingAddress" runat="server" UseStateAbbreviation="true" UseCountryAbbreviation="false" />
                                    </div>
                                </div>

                                <div id="divACHPaymentInfo" runat="server" visible="false" class="tab-pane">
                                    <Rock:RockTextBox ID="txtAccountName" runat="server" Label="Name on Account" />
                                    <Rock:RockTextBox ID="txtRoutingNumber" runat="server" Label="Routing Number" />
                                    <Rock:RockTextBox ID="txtAccountNumber" runat="server" Label="Account Number" />
                                    <Rock:RockRadioButtonList ID="rblAccountType" runat="server" RepeatDirection="Horizontal" Label="Account Type">
                                        <asp:ListItem Text="Checking" Value="checking" Selected="true" />
                                        <asp:ListItem Text="Savings" Value="savings" />
                                    </Rock:RockRadioButtonList>
                                    <asp:Image ID="imgCheck" CssClass="img-responsive" runat="server" ImageUrl="<%$ Fingerprint:~/Assets/Images/check-image.png %>" />
                                </div>

                            </div>

                        </div>
                    </div>
                </asp:Panel>

            </div>

            <div class="panel panel-default no-border">
                <div class="panel-body">
                    <Rock:NotificationBox ID="nbSelectionMessage" runat="server" Visible="false"></Rock:NotificationBox>
                    <div id="gateway_errors">
                    </div>

                    <div class="actions clearfix">
                        <asp:LinkButton ID="btnPaymentInfoNext" runat="server" Text="Next" CssClass="btn btn-primary pull-right" OnClick="btnPaymentInfoNext_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlSuccess" runat="server" Visible="false">

            <asp:Literal ID="lSuccess" runat="server" />

            <asp:Panel ID="pnlSaveAccount" runat="server" Visible="false">
                <Rock:NotificationBox ID="nbSuccessMessage" runat="server" Visible="false"></Rock:NotificationBox>

                <div class="well">
                    <legend>Make Payments Even Easier</legend>
                    <fieldset>
                        <Rock:RockCheckBox ID="cbSaveAccount" runat="server" Text="Save account information for future transactions" CssClass="toggle-input" />
                        <div id="divSaveAccount" runat="server" class="toggle-content">
                            <Rock:RockTextBox ID="txtSaveAccount" runat="server" Label="Name for this account" CssClass="input-large"></Rock:RockTextBox>

                            <Rock:NotificationBox ID="nbSaveAccount" runat="server" Visible="false" NotificationBoxType="Danger"></Rock:NotificationBox>

                            <div id="divSaveActions" runat="server" class="actions">
                                <asp:LinkButton ID="lbSaveAccount" runat="server" Text="Save Account" CssClass="btn btn-primary" OnClick="lbSaveAccount_Click" />
                            </div>
                        </div>
                    </fieldset>
                </div>
            </asp:Panel>

        </asp:Panel>

        <script type="text/javascript">
            Sys.Application.add_load(function () {
                window.scrollTo = function (x, y) { return true; }
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>

<script type="text/javascript">
    var initPaymentEntry = function () {
        var publicKey = $('#hfPublicKey').val();
        var accountSelector = '.account-amount';
        var paymentSelector = '#payment-container';

        SimpleDonation.finance.stripeTokenTransactionEntry.init(publicKey);

        if (SimpleDonation.finance.coverFees != null) {
            SimpleDonation.finance.coverFees.init(
                paymentSelector,
                accountSelector,
                null,
                '<%= _organizationName %>'
            );
        }
    };

    $(initPaymentEntry);
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(initPaymentEntry);
</script>
