<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionList.ascx.cs" Inherits="RockWeb.Blocks.Finance.TransactionList" %>

<asp:UpdatePanel ID="upTransactions" runat="server">
    <ContentTemplate>

        <h4>Transactions</h4>

        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger block-message error alert" />
        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

        <Rock:GridFilter ID="rFilter" runat="server">
            <Rock:DateRangePicker ID="drpDates" runat="server" Label="Date Range" />
            <Rock:NumberRangeEditor ID="nreAmount" runat="server" Label="Amount Range" NumberType="Double" />
            <Rock:RockTextBox ID="txtTransactionCode" runat="server" Label="Transaction Code"></Rock:RockTextBox>
            <Rock:RockDropDownList ID="ddlAccount" runat="server" Label="Account" />
            <Rock:RockDropDownList ID="ddlTransactionType" runat="server" Label="Transaction Type" />
            <Rock:RockDropDownList ID="ddlCurrencyType" runat="server" Label="Currency Type" />
            <Rock:RockDropDownList ID="ddlCreditCardType" runat="server" Label="Credit Card Type" />
            <Rock:RockDropDownList ID="ddlSourceType" runat="server" Label="Source Type" />
        </Rock:GridFilter>

        <Rock:Grid ID="rGridTransactions" runat="server" EmptyDataText="No Transactions Found" OnRowSelected="rGridTransactions_Edit" AllowSorting="true" ToolTip="Description">
            <Columns>
                <asp:BoundField DataField="Id" HeaderText="ID" SortExpression="Id" />
                <asp:BoundField DataField="TransactionDateTime" HeaderText="Date" SortExpression="TransactionDateTime" />                
                <asp:BoundField DataField="Amount" HeaderText="Amount" DataFormatString="{0:C}" SortExpression="Amount" />
                <Rock:DefinedValueField DataField="TransactionTypeValueId" HeaderText="Transaction Type" SortExpression="TransactionTypeValue.Name" />
                <Rock:DefinedValueField DataField="CurrencyTypeValueId" HeaderText="Currency Type" SortExpression="CurrencyTypeValue.Name" />
                <Rock:DefinedValueField DataField="CreditCardTypeValueId" HeaderText="Credit Card Type" SortExpression="CreditCardTypeValue.Name" />
                <Rock:DefinedValueField DataField="SourceTypeValueId" HeaderText="Source Type" SortExpression="SourceTypeValue.Name" />
                <Rock:DeleteField OnClick="rGridTransactions_Delete" Visible="false"/>
            </Columns>
        </Rock:Grid>

    </ContentTemplate>
</asp:UpdatePanel>
