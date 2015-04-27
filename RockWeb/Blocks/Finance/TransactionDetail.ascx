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

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-credit-card"></i> Financial Transaction
                </h1>
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlType" runat="server" />
                </div>
            </div>

            <div class="panel-body">

                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

                <div id="pnlEditDetails" runat="server">
                    <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:PersonPicker ID="ppAuthorizedPerson" CssClass="js-authorizedperson" runat="server" Label="Person" IncludeBusinesses="true" OnSelectPerson="ppAuthorizedPerson_SelectPerson" />
                            <Rock:DateTimePicker ID="dtTransactionDateTime" runat="server" Label="Transaction Date/Time" />
                        </div>
                        <div class="col-md-6">
                            <asp:Panel ID="pnlSingleAccount" runat="server" Visible="false" CssClass="row">
                                <div class="col-sm-6">
                                    <Rock:CurrencyBox id="tbSingleAccountAmount" runat="server" CssClass="input-width-lg"></Rock:CurrencyBox>
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
                                <div class="grid">
                                    <Rock:Grid ID="gAccountsEdit" runat="server" EmptyDataText="No Account Details" RowItemText="Account" DisplayType="Light" 
                                        OnRowSelected="gAccountsEdit_RowSelected" ShowConfirmDeleteDialog="false">
                                        <Columns>
                                            <Rock:RockTemplateField HeaderText="Accounts">
                                                <ItemTemplate><%# AccountName( (int)Eval("AccountId") ) %></ItemTemplate>
                                            </Rock:RockTemplateField>
                                            <Rock:RockBoundField DataField="Amount" SortExpression="Amount" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:C2}" />
                                            <Rock:RockBoundField DataField="Summary" SortExpression="Summary" />
                                            <Rock:DeleteField OnClick="gAccountsEdit_DeleteClick" />
                                        </Columns>
                                    </Rock:Grid>
                                </div>
                            </asp:Panel>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockDropDownList ID="ddlTransactionType" runat="server" Label="Transaction Type" Required="true" />
                            <Rock:RockDropDownList ID="ddlSourceType" runat="server" Label="Source" />
                            <Rock:RockDropDownList ID="ddlCurrencyType" runat="server" Label="Currency Type" AutoPostBack="true" OnSelectedIndexChanged="ddlCurrencyType_SelectedIndexChanged" />
                            <Rock:RockDropDownList ID="ddlCreditCardType" runat="server" Label="Credit Card Type" />
                            <Rock:FinancialGatewayPicker ID="gpPaymentGateway" runat="server" Label="Payment Gateway" />
                            <Rock:DataTextBox ID="tbTransactionCode" runat="server" Label="Transaction Code"
                                SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="TransactionCode" />
                        </div>
                        <div class="col-md-6">
                            <h4>Images</h4>
                            <asp:DataList ID="dlImages" runat="server" RepeatDirection="Horizontal" RepeatColumns="2" OnItemDataBound="dlImages_ItemDataBound">
                                <ItemTemplate>
                                    <Rock:ImageUploader ID="imgupImage" runat="server" OnImageRemoved="imgupImage_ImageRemoved" OnImageUploaded="imgupImage_ImageUploaded" />
                                </ItemTemplate>
                            </asp:DataList>
                            <Rock:RockLiteral ID="lScheduledTransaction" runat="server" Label="Scheduled Transaction" Visible="false" />
                            <Rock:RockLiteral ID="lProcessedBy" runat="server" Label="Matched By" Visible="false" />
                        </div>
                    </div>

                    <Rock:DataTextBox ID="tbSummary" runat="server" Label="Summary" TextMode="MultiLine" Rows="2"
                        SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="Summary" />

                    <div class="actions">
                        <asp:LinkButton ID="lbSave" runat="server" Text="Save" AccessKey="s" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                        <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" AccessKey="c" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                    </div>
                </div>

                <fieldset id="fieldsetViewSummary" runat="server">
                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lDetailsLeft" runat="server" />
                        </div>
                        <div class="col-md-6">

                            <Rock:Grid ID="gAccountsView" runat="server" EmptyDataText="No Account Details" RowItemText="Account" DisplayType="Light">
                                <Columns>
                                    <Rock:RockTemplateField HeaderText="Accounts">
                                        <ItemTemplate><%# AccountName( (int)Eval("AccountId") ) %></ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:RockBoundField DataField="Amount" SortExpression="Amount" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:C2}" />
                                    <Rock:RockBoundField DataField="Summary" SortExpression="Summary" />
                                </Columns>
                            </Rock:Grid>

                            <h4>Images</h4>
                            <div>
                                <asp:Image ID="imgPrimary" runat="server" CssClass="transaction-image" />
                            </div>
                            <div>
                                <asp:Repeater ID="rptrImages" runat="server">
                                    <ItemTemplate>
                                        <asp:Image ID="imgOther" ImageUrl='<%# ImageUrl( (int)Eval("BinaryFileId") ) %>' runat="server" CssClass="transaction-image-thumbnail" ToolTip="Click to toggle" />
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" AccessKey="m" CssClass="btn btn-primary" CausesValidation="false" OnClick="lbEdit_Click" />
                        <asp:LinkButton ID="lbAddTransaction" runat="server" Text="Add New Transaction" AccessKey="a" CssClass="btn btn-default pull-right" CausesValidation="false" OnClick="lbAddTransaction_Click" />
                    </div>

                </fieldset>

            </div>

        </asp:Panel>

        <Rock:ModalAlert ID="modalAlert" runat="server" />

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="mdAccount" runat="server" Title="Account" OnSaveClick="mdAccount_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="Account">
            <Content>
                <asp:HiddenField ID="hfAccountGuid" runat="server" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlAccount" runat="server" Label="Account" DataTextField="Value" DataValueField="Key" Required="true" ValidationGroup="Account" />
                        <Rock:CurrencyBox ID="tbAccountAmount" runat="server" Label="Amount" Required="true" ValidationGroup="Account" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbAccountSummary" runat="server" Label="Summary" TextMode="MultiLine" Rows="3" ValidationGroup="Account" />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
