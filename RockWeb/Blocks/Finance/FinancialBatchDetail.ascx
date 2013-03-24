<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FinancialBatchDetail.ascx.cs" Inherits="RockWeb.Blocks.Administration.FinancialBatchDetail" %>

<asp:UpdatePanel ID="upFinancialBatch" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert alert-error block-message error" />

        <asp:HiddenField ID="hfIdValue" runat="server" />
        <asp:ValidationSummary ID="valSummaryValue" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error" />
        <div class="row">
            <div class="span12">
                <fieldset>
                    <legend>
                        <asp:Literal ID="lValue" runat="server">Financial Batch</asp:Literal></legend>

                    <div class="span6">
                        <Rock:DataTextBox ID="tbName" runat="server" LabelText="Title"
                            SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="Name" />
                        <Rock:CampusPicker ID="CampusPicker1" runat="server">
                        </Rock:CampusPicker>
                        <Rock:DateTimePicker ID="dtBatchDateStart" runat="server" SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="BatchDateStart" LabelText="Batch Date Start" />
                        <Rock:DateTimePicker ID="dtBatchDateEnd" runat="server" SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="BatchDateEnd" LabelText="Batch Date End" />

                        <Rock:LabeledCheckBox ID="cbIsClosed" runat="server" LabelText="Is Closed" />
                    </div>
                    <div class="span6">
                        <Rock:LabeledDropDownList ID="ddlCampus" runat="server" LabelText="Campus" />
                        <Rock:LabeledDropDownList ID="ddlEntity" runat="server" LabelText="Entity" />

                        <Rock:LabeledDropDownList ID="ddlBatchType2" runat="server" LabelText="Batch Type" />
                        <Rock:DataTextBox ID="tbControlAmount" runat="server" LabelText="Control Amount"
                            SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="ControlAmount" />

                        <asp:Button ID="Button1" CssClass="btn btn-primary" Text="Save" runat="server" OnClick="btnSaveFinancialBatch_Click" />
                        &nbsp;<asp:Button ID="Button2" CssClass="btn" Text="Cancel" runat="server" OnClick="btnCancelFinancialBatch_Click" />
                    </div>
                </fieldset>
            </div>
        </div>
        <div class="row">
            <div class="span12">
                <Rock:Grid ID="transactionGrid" runat="server" EmptyDataText="No Transactions Found" ShowConfirmDeleteDialog="true">
                    <Columns>
                        <asp:BoundField DataField="Id" HeaderText="Id" SortExpression="Id" />
                        <asp:BoundField DataField="BatchId" HeaderText="Id" SortExpression="Id" />
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
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
