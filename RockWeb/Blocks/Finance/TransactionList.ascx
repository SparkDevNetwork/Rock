<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionList.ascx.cs" Inherits="RockWeb.Blocks.Finance.TransactionList" %>

<asp:UpdatePanel ID="upTransactions" runat="server">
    <ContentTemplate>

        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error alert" />
        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

        <Rock:GridFilter ID="rFilter" runat="server">
            <Rock:DatePicker ID="dtStartDate" runat="server" SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="TransactionDateTime" Label="From Date" />
            <Rock:DatePicker ID="dtEndDate" runat="server"  SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="TransactionDateTime" Label="To Date" />
            <Rock:LabeledTextBox ID="txtFromAmount" runat="server" Label="From Amount"></Rock:LabeledTextBox>
            <Rock:LabeledTextBox ID="txtToAmount" runat="server" Label="To Amount"></Rock:LabeledTextBox>
            <Rock:LabeledTextBox ID="txtTransactionCode" runat="server" Label="Transaction Code"></Rock:LabeledTextBox>
            <Rock:LabeledDropDownList ID="ddlAccount" runat="server" Label="Account" />
            <Rock:LabeledDropDownList ID="ddlTransactionType" runat="server" Label="Transaction Type" />
            <Rock:LabeledDropDownList ID="ddlCurrencyType" runat="server" Label="Currency Type" />
            <Rock:LabeledDropDownList ID="ddlCreditCardType" runat="server" Label="Credit Card Type" />
            <Rock:LabeledDropDownList ID="ddlSourceType" runat="server" Label="Source Type" />
        </Rock:GridFilter>

        <Rock:Grid ID="rGridTransactions" runat="server" EmptyDataText="No Transactions Found" OnRowSelected="rGridTransactions_Edit" AllowSorting="true">
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

    </ContentTemplate>
</asp:UpdatePanel>
