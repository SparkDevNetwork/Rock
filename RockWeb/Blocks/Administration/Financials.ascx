<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Financials.ascx.cs" Inherits="RockWeb.Blocks.Administration.Financials" %>

<asp:UpdatePanel ID="upFinancial" runat="server">
    <ContentTemplate>

        <Rock:GridFilter ID="rFilter" runat="server">
            <asp:Panel ID="pnlDateRange" runat="server" Style="margin-bottom: 18px; margin-left: 20px;">
                <Rock:DateTimePicker ID="dtStartDate" runat="server" SourceTypeName=" Rock.Financial.Transaction, Rock" PropertyName="TransactionDate" LabelText="StartDate" />
                <Rock:DateTimePicker ID="dtEndDate" runat="server"  SourceTypeName=" Rock.Financial.Transaction, Rock" PropertyName="TransactionDate" LabelText="to" />
            </asp:Panel>
            <asp:Panel ID="pnlAmountRange" runat="server" Style="margin-bottom: 18px;">
                <label id="lblAmountRange">
                    Amount Range</label>
                <asp:TextBox ID="txtFromAmount" runat="server" Columns="10" Style="width: auto; margin-left: 20px;" />
                to
            <asp:TextBox ID="txtToAmount" runat="server" Columns="10" Style="width: auto;" />
            </asp:Panel>
            <asp:Panel ID="pnlTransactionCode" runat="server" Style="margin-bottom: 18px;">
                <label id="lblTransactionCode">
                    Transaction Code</label>
                <asp:TextBox ID="txtTransactionCode" runat="server" Style="margin-left: 20px;" />
            </asp:Panel>
            <Rock:LabeledDropDownList ID="ddlFundType" runat="server" LabelText="Fund Type" />
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
