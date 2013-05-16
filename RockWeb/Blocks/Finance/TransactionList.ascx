<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionList.ascx.cs" Inherits="RockWeb.Blocks.Finance.TransactionList" %>

<asp:UpdatePanel ID="upTransactions" runat="server">
    <ContentTemplate>

        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error alert" />
        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

        <Rock:GridFilter ID="rFilter" runat="server">
            <Rock:DatePicker ID="dtStartDate" runat="server" SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="TransactionDateTime" LabelText="From Date" />
            <Rock:DatePicker ID="dtEndDate" runat="server"  SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="TransactionDateTime" LabelText="To Date" />
            <Rock:LabeledTextBox ID="txtFromAmount" runat="server" LabelText="From Amount"></Rock:LabeledTextBox>
            <Rock:LabeledTextBox ID="txtToAmount" runat="server" LabelText="To Amount"></Rock:LabeledTextBox>
            <Rock:LabeledTextBox ID="txtTransactionCode" runat="server" LabelText="Transaction Code"></Rock:LabeledTextBox>
            <Rock:LabeledDropDownList ID="ddlAccount" runat="server" LabelText="Account" />
            <Rock:LabeledDropDownList ID="ddlTransactionType" runat="server" LabelText="Transaction Type" />
            <Rock:LabeledDropDownList ID="ddlCurrencyType" runat="server" LabelText="Currency Type" />
            <Rock:LabeledDropDownList ID="ddlCreditCardType" runat="server" LabelText="Credit Card Type" />
            <Rock:LabeledDropDownList ID="ddlSourceType" runat="server" LabelText="Source Type" />
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
