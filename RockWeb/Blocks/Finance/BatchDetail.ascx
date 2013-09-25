<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BatchDetail.ascx.cs" Inherits="RockWeb.Blocks.Finance.Administration.BatchDetail" %>

<asp:UpdatePanel ID="upFinancialBatch" runat="server">
<ContentTemplate>

    <asp:Panel ID="pnlDetails" runat="server">
      
        <asp:HiddenField ID="hfIdValue" runat="server" />        
        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error alert" />
        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

        <div class="container-fluid">  

            <fieldset>

                <legend>
                    <asp:Literal ID="lValue" runat="server">Financial Batch</asp:Literal>
                </legend>

                <div class="span4">
                    <Rock:DataTextBox ID="tbName" runat="server" Label="Title" TabIndex="1" 
                        SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="Name" />
                    <Rock:CampusPicker ID="cpCampus" runat="server" />
                </div>

                <div class="span4">
                    <Rock:DateRangePicker ID="dtBatchDate" TabIndex="3"  runat="server" Label="Batch Date Range" />
                    <Rock:RockDropDownList ID="ddlStatus" TabIndex="5" runat="server" Label="Status"></Rock:RockDropDownList>
                    </div>
                <div class="span4">
                    <Rock:DataTextBox ID="tbControlAmount" runat="server" Label="Control Amount" TabIndex="7" 
                        SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="ControlAmount" />
                    </div>
                </div>
                
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSaveFinancialBatch_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancelFinancialBatch_Click" />
            </div>    
                
        </div>        
                   
    </asp:Panel>
  
    <asp:Panel ID="pnlTransactions" runat="server">

        <Rock:Grid ID="rGridTransactions" runat="server" EmptyDataText="No Transactions Found" 
            ShowConfirmDeleteDialog="true"  OnRowSelected="rGridTransactions_Edit">
        <Columns>
            <asp:BoundField DataField="Id" HeaderText="ID" SortExpression="Id" />
            <asp:BoundField DataField="TransactionDateTime" HeaderText="Date" SortExpression="TransactionDateTime" />                
            <asp:BoundField DataField="Summary" HeaderText="Description" SortExpression="Summary" />
            <asp:BoundField DataField="Amount" HeaderText="Amount" DataFormatString="{0:C}" SortExpression="Amount" />
            <asp:BoundField DataField="CurrencyTypeValue" HeaderText="Currency Type" SortExpression="CurrencyTypeValue" />
            <asp:BoundField DataField="CreditCardTypeValue" HeaderText="Credit Card Type" SortExpression="CreditCardTypeValue" />
            <asp:BoundField DataField="SourceTypeValue" HeaderText="Source Type" SortExpression="SourceTypeValue" />
            <Rock:DeleteField OnClick="rGridTransactions_Delete" Visible="false"/>
        </Columns>
        </Rock:Grid>

        <!--<asp:BoundField DataField="Person Name" HeaderText="Person Name" SortExpression="Person Name" />-->
        <!-- <asp:BoundField DataField="Funds" HeaderText="Funds (with split amounts)" SortExpression="Funds" />
        <Rock:BoolField DataField="RefundTransactionId" HeaderText="Is Refunded" SortExpression="RefundTransactionId" /> -->
        <!-- <asp:HyperLinkField DataNavigateUrlFormatString="{0}" DataNavigateUrlFields="TransactionImageId" DataTextField="Image" HeaderText="Image" Target="_parent" />-->

    </asp:Panel>

</ContentTemplate>
</asp:UpdatePanel>
