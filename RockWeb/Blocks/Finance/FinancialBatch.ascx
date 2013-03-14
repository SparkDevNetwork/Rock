<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FinancialBatch.ascx.cs" Inherits="RockWeb.Blocks.Administration.FinancialBatch" %>

<asp:UpdatePanel ID="upFinancialBatch" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert alert-error block-message error"/>

        <Rock:GridFilter ID="rFBFilter" runat="server">
            <Rock:DateTimePicker ID="dtFromDate" runat="server" SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="BatchDateStart" LabelText="From Date" />
            <Rock:DateTimePicker ID="dtThroughDate" runat="server"  SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="BatchDateEnd" LabelText="Through Date" />
            <Rock:LabeledTextBox ID="txtTitle" runat="server" LabelText="Title"></Rock:LabeledTextBox>            
            <Rock:LabeledCheckBox ID="cbIsClosedFilter" runat="server" LabelText="Is Closed" />        
            <Rock:LabeledDropDownList ID="ddlBatchType" runat="server" LabelText="Batch Type" />
        </Rock:GridFilter>
        
        <Rock:Grid ID="grdFinancialBatch" runat="server" EmptyDataText="No Batches Found" OnRowDataBound="grdFinancialBatch_RowDataBound">
            <Columns> 
                <asp:BoundField DataField="Id" HeaderText="Id" SortExpression="Id" />
                <asp:BoundField DataField="Name" HeaderText="Title" SortExpression="Name" />
                <asp:TemplateField  HeaderText="Date Range" >  <%--SortExpression="BatchDateStart"--%>
                    <ItemTemplate>
                        <span><%# Eval("BatchDateStart") %> to <%# Eval("BatchDateEnd") %></span>

                    </ItemTemplate>
                    </asp:TemplateField>
                <Rock:BoolField DataField="IsClosed" HeaderText="Is Closed" SortExpression="IsClosed" />
                
                <asp:BoundField DataField="ControlAmount" HeaderText="Control Amount" />
                <asp:TemplateField  HeaderText="Transaction Total" >
                    <ItemTemplate>
                        <asp:Literal ID="TransactionTotal" runat="server" />
                    </ItemTemplate>
                    </asp:TemplateField>
                <asp:TemplateField  HeaderText="Variance" > 
                    <ItemTemplate>
                        <asp:Literal ID="Variance" runat="server" />
                    </ItemTemplate>
                    </asp:TemplateField>
                <asp:TemplateField  HeaderText="Transaction Count" >
                    <ItemTemplate>
                        <asp:Literal ID="TransactionCount" runat="server" />
                    </ItemTemplate>
                    </asp:TemplateField>
                <asp:BoundField DataField="BatchType.Name" HeaderText="Batch Type" />

                <asp:TemplateField  HeaderText="Funds listed w/ transaction totals" >
                      <ItemTemplate>
                          
                             <Rock:Grid ID="transactionGrid" runat="server" EmptyDataText="No Transactions Found">
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
                       
                     </ItemTemplate>
                </asp:TemplateField>


                
                <Rock:EditValueField OnClick="rGrid_EditValue" />
                <Rock:DeleteField OnClick="grdFinancialBatch_Delete" />
               

            </Columns>
        </Rock:Grid>
        
        <Rock:ModalDialog ID="modalValue" runat="server" Title="Financial Batch">
            <Content>
                <asp:HiddenField ID="hfIdValue" runat="server" />
                <asp:ValidationSummary ID="valSummaryValue" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error" />

                <legend>
                    <asp:Literal ID="lValue" runat="server">Financial Batch</asp:Literal></legend>

                <fieldset>
                    <div class="span6">
                        <Rock:DataTextBox ID="tbName" runat="server" LabelText="Title"
                            SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="Name" />
                         <Rock:DateTimePicker ID="dtBatchDateStart" runat="server" SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="BatchDateStart" LabelText="Batch Date Start" />
                         <Rock:DateTimePicker ID="dtBatchDateEnd" runat="server" SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="BatchDateEnd" LabelText="Batch Date End" />
           
                        <Rock:LabeledDropDownList ID="ddlCampus" runat="server" LabelText="Campus" />
                        <Rock:LabeledDropDownList ID="ddlEntity" runat="server" LabelText="Entity" />
                        
                        <Rock:LabeledCheckBox ID="cbIsClosed" runat="server" LabelText="Is Closed"  />
      
                        <Rock:LabeledDropDownList ID="ddlBatchType2" runat="server" LabelText="Batch Type" />
                        <Rock:DataTextBox ID="tbControlAmount" runat="server" LabelText="Control Amount"
                            SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="ControlAmount" />


                    </div>
                </fieldset>


            </Content>
        </Rock:ModalDialog>


          <Rock:ModalDialog ID="modalTransactions" runat="server" Title="Financial Transaction">
            <Content>
                <asp:HiddenField ID="hfIdTransValue" runat="server" />
                <asp:HiddenField ID="hfBatchId" runat="server" />
                <asp:ValidationSummary ID="valTransactionsValue" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error" />

                <legend>Financial Transaction</legend>

                <fieldset>
                    <div class="span6">
                        <Rock:DataTextBox ID="tbDescription" runat="server" LabelText="Description"
                            SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="Description" />

                        <Rock:DataTextBox ID="tbSummary" runat="server" LabelText="Summary"
                            SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="Summary" />

                         <Rock:DateTimePicker ID="dtTransactionDateTime" runat="server" SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="TransactionDateTime" LabelText="Transaction Date" />
           
                        <Rock:LabeledDropDownList ID="TranEntity"  DataValueField="EntityId" runat="server" LabelText="Entity" />
                        
                      <Rock:DataTextBox ID="tbAmount" runat="server" LabelText="Amount"
                            SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="Amount" />
                        <Rock:DataTextBox ID="tbRefundTransactionId" runat="server" LabelText="Refund Transaction Id"
                        SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="RefundTransactionId" />
                        <Rock:DataTextBox ID="tbTransactionImageId" runat="server" LabelText="Transaction Image Id"
                        SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="TransactionImageId" />
                        <Rock:DataTextBox ID="tbTransactionCode" runat="server" LabelText="Transaction Code"
                        SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="TransactionCode" />
                        
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



                    </div>
                </fieldset>


            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>