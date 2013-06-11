<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionDetail.ascx.cs" Inherits="RockWeb.Blocks.Finance.Administration.TransactionDetail" %>
<asp:UpdatePanel ID="upFinancialBatch" runat="server">
<ContentTemplate>
    
    <asp:Panel ID="pnlDetails" runat="server">   
        
        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error alert" />
        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

        <asp:HiddenField ID="hfIdTransValue" runat="server" />
        <asp:HiddenField ID="hfBatchId" runat="server" />
        
        <fieldset>

            <legend>
                <asp:Literal ID="lValue" runat="server">Financial Transaction</asp:Literal>
            </legend>
                    
            <div class="span4">

                <Rock:DataTextBox ID="tbSummary" TabIndex="2" runat="server" LabelText="Summary" TextMode="MultiLine" Rows="4"
                    SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="Summary" />

                <Rock:DateTimePicker ID="dtTransactionDateTime" TabIndex="3" runat="server" SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="TransactionDateTime" LabelText="Transaction Date/Time" />

                <Rock:DataTextBox ID="tbAmount" runat="server" LabelText="Amount" TabIndex="4"
                    SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="Amount" />
                       
            </div>

            <div class="span4">
                <Rock:LabeledDropDownList ID="ddlSourceType" runat="server" LabelText="Source Type" TabIndex="6" />
                        
                <Rock:LabeledDropDownList ID="ddlTransactionType" runat="server" LabelText="Transaction Type" TabIndex="7" />

                <Rock:DataTextBox ID="tbTransactionCode" runat="server" LabelText="Transaction Code" TabIndex="8"
                    SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="TransactionCode" />
            </div>
            
            <div class="span4">
                <Rock:LabeledDropDownList ID="ddlCurrencyType" runat="server" LabelText="Currency Type" TabIndex="9" />
                <Rock:LabeledDropDownList ID="ddlCreditCardType" runat="server" LabelText="Credit Card Type" TabIndex="10" />
                <Rock:LabeledDropDownList ID="ddlPaymentGateway" runat="server" LabelText="Payment Gateway" TabIndex="11" />
                <%--  <Rock:LabeledDropDownList ID="ddlEntityType" runat="server" LabelText="Entity Type" />--%>
            </div>

        </fieldset>

        <div class="actions">
            <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSaveTransaction_Click" />
            <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancelTransaction_Click" />
        </div>

    </asp:Panel>     

</ContentTemplate>
</asp:UpdatePanel>
