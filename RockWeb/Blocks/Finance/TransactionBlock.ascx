<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionBlock.ascx.cs" Inherits="Blocks_Finance_Transaction" %>
<asp:UpdatePanel ID="upFinancialBatch" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert alert-error block-message error" />

        <div class="row">
            <div class="span12">

                <asp:HiddenField ID="hfIdTransValue" runat="server" />
                <asp:HiddenField ID="hfBatchId" runat="server" />
                <asp:ValidationSummary ID="valTransactionsValue" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error" />

                <fieldset>
                    <legend>
                        <asp:Literal ID="lValue" runat="server">Financial Transaction</asp:Literal></legend>

                    <div class="span4">
                        <Rock:DataTextBox ID="tbDescription" runat="server" LabelText="Description"
                            SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="Description" />

                        <Rock:DataTextBox ID="tbSummary" runat="server" LabelText="Summary" Rows="4"
                            SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="Summary" />

                        <Rock:DateTimePicker ID="dtTransactionDateTime" runat="server" SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="TransactionDateTime" LabelText="Transaction Date" />

                        <Rock:LabeledDropDownList ID="TranEntity" DataValueField="EntityId" runat="server" LabelText="Entity" />
                        <Rock:LabeledDropDownList ID="TranCampus" runat="server" LabelText="Campus" />
                    </div>
                    <div class="span4">
                        <Rock:DataTextBox ID="tbAmount" runat="server" LabelText="Amount"
                            SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="Amount" />
                        <Rock:DataTextBox ID="tbRefundTransactionId" runat="server" LabelText="Refund Transaction Id"
                            SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="RefundTransactionId" />
                        <Rock:DataTextBox ID="tbTransactionImageId" runat="server" LabelText="Transaction Image Id"
                            SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="TransactionImageId" />
                        <Rock:DataTextBox ID="tbTransactionCode" runat="server" LabelText="Transaction Code"
                            SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="TransactionCode" />
                    </div>
                    <div class="span4">
                        <Rock:LabeledDropDownList ID="ddlCurrencyType" runat="server" LabelText="Currency Type" />
                        <!-- CurrencyTypeValueId -->
                        <Rock:LabeledDropDownList ID="ddlCreditCartType" runat="server" LabelText="Credit Card Type" />
                        <!-- CreditCardTypeValueId -->
                        <Rock:LabeledDropDownList ID="ddlPaymentGateway" runat="server" LabelText="Payment Gateway" />
                        <!-- PaymentGatewayId -->
                        <Rock:LabeledDropDownList ID="ddlSourceType" runat="server" LabelText="Source Type" />
                        <!-- SourceTypeValueId -->
                        <Rock:LabeledDropDownList ID="ddlEntityType" runat="server" LabelText="Entity Type" />
                        <!-- EntityTypeId -->


                        <asp:Button ID="Button1" CssClass="btn btn-primary" Text="Save" runat="server" OnClick="btnSaveFinancialTransaction_Click" />
                        &nbsp;<asp:Button ID="Button2" CssClass="btn" Text="Cancel" runat="server" OnClick="btnCancelFinancialTransaction_Click" />

                    </div>
                </fieldset>

            </div>

        </div>

    </ContentTemplate>
</asp:UpdatePanel>
