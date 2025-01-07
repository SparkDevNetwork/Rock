<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionDetail.ascx.cs" Inherits="RockWeb.Blocks.Finance.TransactionDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upFinancialBatch" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <asp:HiddenField ID="hfTransactionId" runat="server" />
            <asp:HiddenField ID="hfBatchId" runat="server" />
            <Rock:HiddenFieldWithClass ID="hfIsZeroTransaction" CssClass="is-zero-transaction" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-credit-card"></i>
                    Financial Transaction
                </h1>
                <div class="panel-labels">
                    <asp:Literal ID="lBatchId" runat="server" />
                    <Rock:HighlightLabel ID="hlType" runat="server" />
                </div>
            </div>

            <div class="panel-body">

                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

                <div id="pnlEditDetails" runat="server">
                    <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                    <Rock:NotificationBox ID="nbErrorMessage" runat="server" NotificationBoxType="Danger" Visible="false" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:PersonPicker ID="ppAuthorizedPerson" CssClass="js-authorizedperson" runat="server" Label="Person" IncludeBusinesses="true" OnSelectPerson="ppAuthorizedPerson_SelectPerson" />
                            <Rock:RockCheckBox ID="cbShowAsAnonymous" runat="server" Label="Show As Anonymous" />
                            <Rock:DateTimePicker ID="dtTransactionDateTime" runat="server" Label="Transaction Date/Time" Required="true" />
                        </div>
                        <div class="col-md-6">
                            <asp:Panel ID="pnlSingleAccount" runat="server" Visible="false" CssClass="row">
                                <div class="col-sm-6">
                                    <Rock:CurrencyBox ID="tbSingleAccountAmountMinusFeeCoverageAmount" Label="Amount" runat="server" CssClass="input-width-lg" AutoPostBack="true" OnTextChanged="tbSingleAccountAmountMinusFeeCoverageAmount_TextChanged" />
                                    <Rock:CurrencyBox ID="tbSingleAccountForeignCurrencyAmount" Label="Foreign Currency Amount" runat="server" CssClass="input-width-lg" />
                                    <Rock:CurrencyBox ID="tbSingleAccountFeeAmount" Label="Processing Fees" runat="server" CssClass="input-width-lg" Help="The fee amount associated with the processing of the transaction." />
                                    <Rock:CurrencyBox ID="tbSingleAccountFeeCoverageAmount" runat="server" Label="Covered Fee" Required="false" ValidationGroup="Account" Help="The fee amount that the person elected to cover." />
                                </div>
                                <div class="col-sm-6">
                                    <div class="form-group">
                                        <label class="control-label">&nbsp;</label>
                                        <div class="input-group">
                                            <asp:LinkButton ID="lbShowMore" runat="server" Text="Additional Accounts" CssClass="btn btn-link" TabIndex="0" OnClick="lbShowMore_Click" />
                                        </div>
                                    </div>
                                </div>
                            </asp:Panel>
                            <asp:Panel ID="pnlAccounts" runat="server">
                                <Rock:Grid ID="gAccountsEdit" runat="server" EmptyDataText="No Account Details" RowItemText="Account" DisplayType="Light"
                                    ShowConfirmDeleteDialog="false" OnRowDataBound="gAccountsEdit_RowDataBound">
                                    <Columns>
                                        <Rock:RockLiteralField ID="lAccountsEditAccountName" HeaderText="Accounts" />
                                        <Rock:RockBoundField DataField="Summary" SortExpression="Summary" />
                                        <Rock:CurrencyField DataField="ForeignCurrencyAmount" HeaderText="Foreign Currency" SortExpression="Foreign Currency" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                                        <Rock:CurrencyField HeaderText="Covered Fees" DataField="FeeCoverageAmount" SortExpression="FeeCoverageAmount" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                                        <Rock:CurrencyField HeaderText="Processing Fees" DataField="FeeAmount" SortExpression="FeeAmount" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                                        <Rock:RockLiteralField ID="lAccountsEditAmountMinusFeeCoverageAmount" HeaderText="Amount" SortExpression="Amount" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                                    </Columns>
                                </Rock:Grid>
                            </asp:Panel>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DefinedValuePicker ID="dvpTransactionType" runat="server" Label="Transaction Type" Required="true" />
                            <Rock:DefinedValuePicker ID="dvpSourceType" runat="server" Label="Source" />
                            <Rock:DefinedValuePicker ID="dvpCurrencyType" runat="server" Label="Currency Type" AutoPostBack="true" OnSelectedIndexChanged="ddlCurrencyType_SelectedIndexChanged" Required="true" />
                            <Rock:DefinedValuePicker ID="dvpNonCashAssetType" runat="server" Label="Non-Cash Asset Type" />
                            <Rock:DefinedValuePicker ID="dvpCreditCardType" runat="server" Label="Credit Card Type" />
                            <Rock:DefinedValuePicker ID="dvpForeignCurrencyCode" runat="server" Label="Foreign Currency" AutoPostBack="true" OnSelectedIndexChanged="dvpForeignCurrencyCode_SelectedIndexChanged" />
                            <Rock:DynamicPlaceholder ID="phPaymentAttributeEdits" runat="server" />
                            <Rock:FinancialGatewayPicker ID="gpPaymentGateway" runat="server" Label="Payment Gateway" IncludeInactive="true" />
                            <Rock:DataTextBox ID="tbTransactionCode" runat="server" Label="Transaction Code"
                                SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="TransactionCode" />
                            <Rock:DynamicPlaceholder ID="phAttributeEdits" runat="server" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbIsRefund" runat="server" Label="This is a Refund" AutoPostBack="true" OnCheckedChanged="cbIsRefund_CheckedChanged" />
                            <Rock:DefinedValuePicker ID="dvpRefundReasonEdit" runat="server" Label="Refund Reason" Visible="false" />
                            <Rock:RockTextBox ID="tbRefundSummaryEdit" runat="server" Label="Refund Reason Summary" TextMode="MultiLine" Rows="3" Visible="false" />
                            <h4>Images</h4>
                            <asp:DataList ID="dlImages" runat="server" RepeatDirection="Horizontal" RepeatColumns="2" OnItemDataBound="dlImages_ItemDataBound">
                                <ItemTemplate>
                                    <Rock:ImageUploader ID="imgupImage" runat="server" OnImageRemoved="imgupImage_ImageRemoved" BinaryFileTypeGuid="6D18A9C4-34AB-444A-B95B-C644019465AC" OnImageUploaded="imgupImage_ImageUploaded" />
                                </ItemTemplate>
                            </asp:DataList>
                            <Rock:RockLiteral ID="lScheduledTransaction" runat="server" Label="Scheduled Transaction" Visible="false" />
                            <Rock:RockLiteral ID="lProcessedBy" runat="server" Label="Matched By" Visible="false" />
                        </div>
                    </div>

                    <Rock:DataTextBox ID="tbComments" runat="server" Label="Comments" TextMode="MultiLine" Rows="2" ValidateRequestMode="Disabled"
                        SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="Summary" />

                    <div class="actions">
                        <asp:LinkButton ID="lbSave" runat="server" Text="Save" data-shortcut-key="s" ToolTip="Alt+s" CssClass="btn btn-primary btn-transaction-save" OnClick="lbSave_Click" />
                        <asp:LinkButton ID="btnSaveThenAdd" runat="server" data-shortcut-key="a" ToolTip="Alt+a" Text="Save Then Add" CssClass="btn btn-link btn-transaction-save" OnClick="btnSaveThenAdd_Click" />
                        <asp:LinkButton ID="btnSaveThenViewBatch" runat="server" Text="Save Then View Batch" CssClass="btn btn-link btn-transaction-save" OnClick="btnSaveThenViewBatch_Click" />
                        <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" data-shortcut-key="c" ToolTip="Alt+c" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                    </div>
                </div>

                <fieldset id="fieldsetViewSummary" runat="server">
                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <div class="panel panel-section">
                        <div class="panel-heading">
                            <h5 class="panel-title">Authorized By</h5>
                        </div>
                        <div class="panel-body">
                            <div class="row">
                                <div class="col-md-3">
                                    <asp:Literal ID="lAuthorizedPerson" runat="server" />
                                </div>

                                <div class="col-md-6">
                                    <div class="row">
                                        <asp:Repeater ID="rptAddresses" runat="server">
                                            <ItemTemplate>
                                                <div class="col-md-4">
                                                    <p>
                                                        <strong>
                                                            <%# FormatAddressType(Eval("GroupLocationTypeValue.Value")) %>
                                                        </strong>
                                                        <br>
                                                        <%# Eval("Location.FormattedHtmlAddress") %>
                                                    </p>
                                                </div>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </div>
                                </div>

                                <div class="col-md-3">
                                    <asp:Literal ID="lCampus" runat="server" />
                                </div>

                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-4">
                            <asp:Literal ID="lDetailsLeft" runat="server" />
                            <asp:PlaceHolder ID="phAttributes" runat="server"></asp:PlaceHolder>
                        </div>
                        <div class="col-md-8">
                            <label>Transaction Details</label>
                            <Rock:Grid ID="gAccountsView" runat="server" EmptyDataText="No Account Details" RowItemText="Account" DisplayType="Light" ShowHeader="true" OnRowDataBound="gAccountsView_RowDataBound">
                                <Columns>
                                    <Rock:RockLiteralField ID="lAccountsViewAccountName" HeaderText="Accounts" />
                                    <Rock:RockBoundField DataField="Summary" SortExpression="Summary" />
                                    <Rock:CurrencyField DataField="ForeignCurrencyAmount" HeaderText="Foreign Currency" SortExpression="Foreign Currency" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                                    <Rock:CurrencyField HeaderText="Covered Fees" DataField="FeeCoverageAmount" SortExpression="FeeCoverageAmount" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                                    <Rock:CurrencyField HeaderText="Processing Fees" DataField="FeeAmount" SortExpression="FeeAmount" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                                    <Rock:RockLiteralField ID="lAccountsViewAmountMinusFeeCoverageAmount" HeaderText="Amount" SortExpression="Amount" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                                </Columns>
                            </Rock:Grid>

                            <asp:Panel ID="pnlImages" runat="server" CssClass="margin-t-md">
                                <label>Images</label>
                                <div class="row">
                                    <div class="col-md-6">
                                        <asp:Image ID="imgPrimary" runat="server" CssClass="transaction-image" />
                                    </div>

                                    <asp:Repeater ID="rptrImages" runat="server">
                                        <ItemTemplate>
                                            <div class="col-md-6">
                                                <asp:Image ID="imgOther" ImageUrl='<%# ImageUrl( (int)Eval("BinaryFileId") ) %>' runat="server" CssClass="transaction-image" ToolTip="Click to toggle" />
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>
                            </asp:Panel>

                            <asp:Panel ID="pnlRefunds" runat="server" CssClass="margin-t-md">
                                <label>Refunds</label>
                                <Rock:Grid ID="gRefunds" runat="server" RowItemText="Refund" DisplayType="Light" ShowHeader="false">
                                    <Columns>
                                        <asp:HyperLinkField DataTextField="TransactionDateTime" DataNavigateUrlFields="Id" />
                                        <Rock:RockBoundField DataField="TransactionCode" />
                                        <Rock:RockBoundField DataField="RefundReasonValue" />
                                        <Rock:RockBoundField DataField="RefundReasonSummary" />
                                        <Rock:CurrencyField DataField="TotalAmount" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                                    </Columns>
                                </Rock:Grid>
                            </asp:Panel>

                            <asp:Panel ID="pnlRelated" runat="server" Visible="false">
                                <label>Related Transactions</label>
                                <Rock:Grid ID="gRelated" runat="server" RowItemText="Transaction" DisplayType="Light" ShowHeader="false">
                                    <Columns>
                                        <asp:HyperLinkField DataTextField="TransactionDateTime" DataNavigateUrlFields="Id" />
                                        <Rock:RockBoundField DataField="TransactionCode" />
                                        <Rock:CurrencyField DataField="TotalAmount" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                                    </Columns>
                                </Rock:Grid>
                            </asp:Panel>

                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" data-shortcut-key="e" AccessKey="m" ToolTip="Alt+e" CssClass="btn btn-primary" CausesValidation="false" OnClick="lbEdit_Click" />
                        <div class="pull-right">
                            <asp:LinkButton ID="lbRefund" runat="server" Text="Refund" data-shortcut-key="r" ToolTip="Alt+r" CssClass="btn btn-default margin-r-sm" CausesValidation="false" OnClick="lbRefundTransaction_Click" />
                            <asp:LinkButton ID="lbAddTransaction" runat="server" Text="Add New Transaction" data-shortcut-key="a" ToolTip="Alt+a" CssClass="btn btn-default margin-r-sm" CausesValidation="false" OnClick="lbAddTransaction_Click" />
                            <asp:HyperLink ID="lbBack" runat="server" data-shortcut-key="arrowleft" ToolTip="Alt+🡄" CssClass="btn btn-default margin-r-sm" OnClick="lbBack_Click"><i class="fa fa-chevron-left"></i> Back</asp:HyperLink>
                            <asp:HyperLink ID="lbNext" runat="server" data-shortcut-key="arrowright" ToolTip="Alt+🡆" CssClass="btn btn-default margin-r-sm">Next <i class="fa fa-chevron-right"></i></asp:HyperLink>
                        </div>
                    </div>

                </fieldset>

            </div>

        </asp:Panel>

        <Rock:ModalAlert ID="modalAlert" runat="server" />

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <script>

            Sys.Application.add_load(function () {
                // delete/archive prompt
                $('.btn-transaction-save').on('click', function (e) {
                    var isZeroTransaction = $('.is-zero-transaction').val();
                    var isSingleAccountAmountMinusFeeCoverageAmountVisible = $('#<%= tbSingleAccountAmountMinusFeeCoverageAmount.ClientID %>').is(":visible");

                    if (isSingleAccountAmountMinusFeeCoverageAmountVisible) {
                        if ($('#<%= tbSingleAccountAmountMinusFeeCoverageAmount.ClientID %>').val() > 0) {
                            var isSingleAccountFeeCoverageAmountVisible = $('#<%= tbSingleAccountFeeCoverageAmount.ClientID %>').is(":visible");
                            if (!isSingleAccountFeeCoverageAmountVisible || $('#<%= tbSingleAccountFeeCoverageAmount.ClientID %>').val() > 0) {
                                return true;
                            }
                        }
                    } else if (isZeroTransaction !== 'True') {
                        return true;
                    }

                    e.preventDefault();
                    Rock.dialogs.confirm('This will create a financial transaction without an amount. Do you want to continue?', function (result) {
                        if (result) {
                            window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                        }
                    });
                });

            });

        </script>

        <Rock:ModalDialog ID="mdAccount" runat="server" Title="Account" OnSaveClick="mdAccount_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="Account">
            <Content>
                <asp:ValidationSummary ID="valSummaryAccount" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="Account" />
                <asp:HiddenField ID="hfAccountGuid" runat="server" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:AccountPicker ID="apAccount" runat="server" Label="Account" Required="true" ValidationGroup="Account" />
                        <Rock:CurrencyBox ID="tbAccountAmountMinusFeeCoverageAmount" runat="server" Label="Amount" Required="true" ValidationGroup="Account" />
                        <Rock:CurrencyBox ID="tbAccountForeignCurrencyAmount" runat="server" Label="Foreign Currency Amount" ValidationGroup="Account" />
                        <Rock:CurrencyBox ID="tbAccountFeeAmount" runat="server" Label="Processing Fees" Required="false" ValidationGroup="Account" Help="The fee amount associated with the processing of the transaction." />
                        <Rock:CurrencyBox ID="tbAccountFeeCoverageAmount" runat="server" Label="Covered Fee" Required="false" ValidationGroup="Account" Help="The fee amount that the person elected to cover." />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbAccountSummary" runat="server" Label="Summary" TextMode="MultiLine" Rows="3" ValidationGroup="Account" />
                        <div class="attributes">
                            <asp:PlaceHolder ID="phAccountAttributeEdits" runat="server" />
                        </div>
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="mdRefund" runat="server" Title="Refund" OnSaveClick="mdRefund_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="Refund">
            <Content>
                <asp:ValidationSummary ID="valSummaryRefund" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="Refund" />
                <Rock:NotificationBox ID="nbRefundError" runat="server" NotificationBoxType="Danger" Visible="false" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:CurrencyBox ID="tbRefundAmount" runat="server" Label="Amount" Required="true" ValidationGroup="Refund" />
                    </div>
                    <div class="col-md-6">
                        <Rock:DefinedValuePicker ID="dvpRefundReason" runat="server" Label="Reason" Required="true" ValidationGroup="Refund" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-12">
                        <Rock:RockTextBox ID="tbRefundSummary" runat="server" Label="Summary" TextMode="MultiLine" Rows="3" ValidationGroup="Refund" />
                        <Rock:RockCheckBox ID="cbProcess" runat="server" Text="Process refund through financial gateway" />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
