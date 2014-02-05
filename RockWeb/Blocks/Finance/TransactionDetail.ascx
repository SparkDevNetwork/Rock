<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionDetail.ascx.cs" Inherits="RockWeb.Blocks.Finance.TransactionDetail" %>
<asp:UpdatePanel ID="upFinancialBatch" runat="server">
<ContentTemplate>
    
    <asp:Panel ID="pnlDetails" runat="server">   
        
        <div class="banner"><h1><asp:Literal ID="lTitle" runat="server"></asp:Literal></h1></div>

        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

        <asp:HiddenField ID="hfIdTransValue" runat="server" />
        <asp:HiddenField ID="hfBatchId" runat="server" />

        <div class="row">
            <div class="col-md-6">
                <Rock:DataTextBox ID="tbAmount" runat="server" PrependText="$" CssClass="input-width-md" Label="Amount" TabIndex="1"
                    SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="Amount" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-12">
                <Rock:DataTextBox ID="tbSummary" TabIndex="2" runat="server" Label="Summary" TextMode="MultiLine" Rows="4"
                    SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="Summary" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-6">
                <Rock:DateTimePicker ID="dtTransactionDateTime" TabIndex="3" runat="server" Label="Transaction Date/Time" />
                <Rock:RockDropDownList ID="ddlSourceType" runat="server" Label="Source Type" TabIndex="4" />
                <Rock:RockDropDownList ID="ddlTransactionType" runat="server" Label="Transaction Type" TabIndex="5" />
                <Rock:PersonPicker ID="ppAuthorizedPerson" runat="server" Label="Authorized Person" TabIndex="6" />
            </div>

            <div class="col-md-6">
                <Rock:DataTextBox ID="tbTransactionCode" runat="server" Label="Transaction Code" TabIndex="7"
                    SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="TransactionCode" />
                <Rock:RockDropDownList ID="ddlCurrencyType" runat="server" Label="Currency Type" TabIndex="8" AutoPostBack="true" OnSelectedIndexChanged="ddlCurrencyType_SelectedIndexChanged" />
                <Rock:RockDropDownList ID="ddlCreditCardType" runat="server" Label="Credit Card Type" TabIndex="9" />
                <Rock:ComponentPicker ID="ddlPaymentGateway" runat="server" Label="Payment Gateway" TabIndex="10" />
            </div>
        </div>


        <div class="actions">
            <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary btn-sm" OnClick="btnSaveTransaction_Click" />
            <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link btn-sm" CausesValidation="false" OnClick="btnCancelTransaction_Click" />
        </div>

    </asp:Panel>     

</ContentTemplate>
</asp:UpdatePanel>
