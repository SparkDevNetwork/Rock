<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionBlock.ascx.cs" Inherits="Blocks_Finance_Transaction" %>
<asp:UpdatePanel ID="upFinancialBatch" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert alert-error block-message error" />
<div>
                <asp:HiddenField ID="hfIdTransValue" runat="server" />
                <asp:HiddenField ID="hfBatchId" runat="server" />
                <asp:ValidationSummary ID="valTransactionsValue" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error" />

                <fieldset>
                    <legend>
                        <asp:Literal ID="lValue" runat="server">Financial Transaction</asp:Literal></legend>
                    
        <div class="row" style="margin-left:0">
            <div class="span12">

                    <div class="span4">
                        <Rock:DataTextBox ID="tbDescription" TabIndex="1" runat="server" LabelText="Description"
                            SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="Description" />

                        <Rock:DataTextBox ID="tbSummary" TabIndex="2" runat="server" LabelText="Summary" TextMode="MultiLine" Rows="4"
                            SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="Summary" />

                        <Rock:DateTimePicker ID="dtTransactionDateTime" TabIndex="3" runat="server" SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="TransactionDateTime" LabelText="Transaction Date" />

                        <%--<Rock:LabeledDropDownList ID="TranEntity" DataValueField="EntityId" runat="server" LabelText="Entity" />--%>
                      <Rock:DataTextBox ID="tbAmount" runat="server" LabelText="Amount" TabIndex="4"
                            SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="Amount" />
                       
                    </div>
                    <div class="span3">
                         <Rock:DataTextBox ID="tbRefundTransactionId" runat="server" LabelText="Refund Transaction Id" TabIndex="5"
                            SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="RefundTransactionId" />
                        <Rock:LabeledDropDownList ID="ddlSourceType" runat="server" LabelText="Source Type" TabIndex="6" />
                        <!-- SourceTypeValueId -->
                        <Rock:DataTextBox ID="tbTransactionImageId" runat="server" LabelText="Transaction Image Id" TabIndex="7"
                            SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="TransactionImageId" />
                        <Rock:DataTextBox ID="tbTransactionCode" runat="server" LabelText="Transaction Code" TabIndex="8"
                            SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="TransactionCode" />
                        

                    </div>
                    <div class="span4">
                        <Rock:LabeledDropDownList ID="ddlCurrencyType" runat="server" LabelText="Currency Type" TabIndex="9" />
                        <!-- CurrencyTypeValueId -->
                        <Rock:LabeledDropDownList ID="ddlCreditCardType" runat="server" LabelText="Credit Card Type" TabIndex="10" />
                        <!-- CreditCardTypeValueId -->
                        <Rock:LabeledDropDownList ID="ddlPaymentGateway" runat="server" LabelText="Payment Gateway" TabIndex="11" />
                        <!-- PaymentGatewayId -->
                      
                      <%--  <Rock:LabeledDropDownList ID="ddlEntityType" runat="server" LabelText="Entity Type" />--%>
                        <!-- EntityTypeId -->


                    </div>
                </div>
            </div>
                    <div class="row"><div class="span12" style="margin:auto;text-align:center; padding-bottom:20px">
                        
                        <asp:Button ID="Button1" CssClass="btn btn-primary" Text="Save" runat="server" OnClick="btnSaveFinancialTransaction_Click" />
                        &nbsp;<asp:Button ID="Button2" CssClass="btn" Text="Cancel" runat="server" OnClick="btnCancelFinancialTransaction_Click" />
                                     </div></div>
                </fieldset>

            </div>

    </ContentTemplate>
</asp:UpdatePanel>
