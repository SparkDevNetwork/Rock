<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FinancialBatchDetail.ascx.cs" Inherits="RockWeb.Blocks.Administration.FinancialBatchDetail" %>

<asp:UpdatePanel ID="upFinancialBatch" runat="server">
    <ContentTemplate>
   <div>
       <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert alert-error block-message error" />

        <asp:HiddenField ID="hfIdValue" runat="server" />
        <asp:ValidationSummary ID="valSummaryValue" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error" />
 
                <fieldset>
                    <legend>
                        <asp:Literal ID="lValue" runat="server">Financial Batch</asp:Literal></legend>
            <div class="row" style="margin-left:0">
            <div class="span12">
                    <div class="span4">
                        <Rock:DataTextBox ID="tbName" runat="server" LabelText="Title" TabIndex="1" 
                            SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="Name" />
                        <Rock:CampusPicker ID="CampusPicker1" runat="server" TabIndex="2" >
                        </Rock:CampusPicker>
                       
                    </div>
                    <div class="span3">
                        <Rock:DateTimePicker ID="dtBatchDate" TabIndex="3"  runat="server" SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="BatchDate" LabelText="Batch Date" />
                        <Rock:LabeledDropDownList ID="ddlStatus" TabIndex="5" runat="server" LabelText="Status"></Rock:LabeledDropDownList>
                        </div>
                    <div class="span4">
                        <Rock:DataTextBox ID="tbControlAmount" runat="server" LabelText="Control Amount" TabIndex="7" 
                            SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="ControlAmount" />
                        </div>
                </div>
                <div class="row"></div>
                    <div class="span12" style="margin:auto;text-align:center; padding-bottom:20px">
                        <asp:Button ID="Button1" CssClass="btn btn-primary" Text="Save" runat="server" OnClick="btnSaveFinancialBatch_Click" />
                        &nbsp;<asp:Button ID="Button2" CssClass="btn" Text="Cancel" runat="server" OnClick="btnCancelFinancialBatch_Click" />
                    </div>
                </div>
                </fieldset>
  
                <Rock:Grid ID="transactionGrid" runat="server" EmptyDataText="No Transactions Found" ShowConfirmDeleteDialog="true">
                    <Columns>
                        <asp:BoundField DataField="Id" HeaderText="Id" SortExpression="Id" />
                        <asp:BoundField DataField="BatchId" HeaderText="BatchId" SortExpression="BatchId" />
                        <asp:BoundField DataField="TransactionCode" HeaderText="Transaction Code" SortExpression="TransactionCode" />
                        <asp:BoundField DataField="TransactionDate" HeaderText="Transaction Date" SortExpression="TransactionDate" />
                        <asp:BoundField DataField="Summary" HeaderText="Description" SortExpression="Description" />
                        <asp:BoundField DataField="Person Name" HeaderText="Person Name" SortExpression="Person Name" />
                        <asp:BoundField DataField="Amount" HeaderText="Amount" SortExpression="Amount" />
                        <asp:BoundField DataField="Funds" HeaderText="Funds (with split amounts)" SortExpression="Funds" />
                        <Rock:BoolField DataField="RefundTransactionId" HeaderText="Is Refunded" SortExpression="RefundTransactionId" />

                        <asp:BoundField DataField="Summary" HeaderText="Summary" SortExpression="Summary" />
                        <asp:HyperLinkField DataNavigateUrlFormatString="{0}" DataNavigateUrlFields="TransactionImageId" DataTextField="Image" HeaderText="Image" Target="_parent" />
                        <asp:BoundField DataField="Id" HeaderText="Id" SortExpression="Id" />

                        <Rock:EditValueField OnClick="rTransactionsGrid_EditValue" />
                        <Rock:DeleteField OnClick="grdFinancialTransactions_Delete" />
                    </Columns>
                </Rock:Grid>
      
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
