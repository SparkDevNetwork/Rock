<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Transactions.ascx.cs" Inherits="RockWeb.Blocks.Administration.Financials" %>

<asp:UpdatePanel ID="upFinancial" runat="server">
    <ContentTemplate>

        <Rock:GridFilter ID="rFilter" runat="server">
            <Rock:DateTimePicker ID="dtStartDate" runat="server" SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="TransactionDateTime" LabelText="From Date" />
            <Rock:DateTimePicker ID="dtEndDate" runat="server"  SourceTypeName="Rock.Model.FinancialTransaction, Rock" PropertyName="TransactionDateTime" LabelText="To Date" />
            <Rock:LabeledTextBox ID="txtFromAmount" runat="server" LabelText="From Amount"></Rock:LabeledTextBox>
            <Rock:LabeledTextBox ID="txtToAmount" runat="server" LabelText="To Amount"></Rock:LabeledTextBox>
            <Rock:LabeledTextBox ID="txtTransactionCode" runat="server" LabelText="Transaction Code"></Rock:LabeledTextBox>
            <Rock:LabeledDropDownList ID="ddlAccount" runat="server" LabelText="Account" />
            <Rock:LabeledDropDownList ID="ddlTransactionType" runat="server" LabelText="Transaction Type" />
            <Rock:LabeledDropDownList ID="ddlCurrencyType" runat="server" LabelText="Currency Type" />
            <Rock:LabeledDropDownList ID="ddlCreditCardType" runat="server" LabelText="Credit Card Type" />
            <Rock:LabeledDropDownList ID="ddlSourceType" runat="server" LabelText="Source Type" />
        </Rock:GridFilter>

        <Rock:Grid ID="grdTransactions" runat="server" EmptyDataText="No Transactions Found">
            <Columns>
                <asp:BoundField DataField="TransactionDate" HeaderText="Date" />
                <asp:BoundField DataField="Description" HeaderText="Description" />
                <asp:BoundField DataField="Amount" HeaderText="Amount" DataFormatString="{0:C}" />
            </Columns>
        </Rock:Grid>

    </ContentTemplate>
</asp:UpdatePanel>
