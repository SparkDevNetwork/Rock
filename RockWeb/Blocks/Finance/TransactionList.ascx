<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionList.ascx.cs" Inherits="RockWeb.Blocks.Finance.TransactionList" %>

<asp:UpdatePanel ID="upTransactions" runat="server">
    <ContentTemplate>

        <asp:Literal ID="lTitle" runat="server"></asp:Literal>

        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

        <div class="grid">
            <Rock:GridFilter ID="gfTransactions" runat="server">
                <Rock:DateRangePicker ID="drpDates" runat="server" Label="Date Range" />
                <Rock:NumberRangeEditor ID="nreAmount" runat="server" Label="Amount Range" NumberType="Double" />
                <Rock:RockTextBox ID="tbTransactionCode" runat="server" Label="Transaction Code"></Rock:RockTextBox>
                <Rock:RockDropDownList ID="ddlAccount" runat="server" Label="Account" />
                <Rock:RockDropDownList ID="ddlTransactionType" runat="server" Label="Transaction Type" />
                <Rock:RockDropDownList ID="ddlCurrencyType" runat="server" Label="Currency Type" />
                <Rock:RockDropDownList ID="ddlCreditCardType" runat="server" Label="Credit Card Type" />
                <Rock:RockDropDownList ID="ddlSourceType" runat="server" Label="Source Type" />
            </Rock:GridFilter>

            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
            <Rock:Grid ID="gTransactions" runat="server" EmptyDataText="No Transactions Found" OnRowSelected="gTransactions_Edit" AllowSorting="true" ToolTip="Description">
                <Columns>
                    <asp:BoundField DataField="Id" HeaderText="ID" SortExpression="Id" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                    <asp:BoundField DataField="TransactionDateTime" HeaderText="Date" SortExpression="TransactionDateTime" />                
                    <asp:BoundField DataField="TotalAmount" HeaderText="Amount" DataFormatString="{0:C}" SortExpression="TotalAmount" />
                    <Rock:DefinedValueField DataField="TransactionTypeValueId" HeaderText="Transaction Type" SortExpression="TransactionTypeValue.Name" />
                    <Rock:DefinedValueField DataField="CurrencyTypeValueId" HeaderText="Currency Type" SortExpression="CurrencyTypeValue.Name" />
                    <Rock:DefinedValueField DataField="CreditCardTypeValueId" HeaderText="Credit Card Type" SortExpression="CreditCardTypeValue.Name" />
                    <Rock:DefinedValueField DataField="SourceTypeValueId" HeaderText="Source Type" SortExpression="SourceTypeValue.Name" />
                    <Rock:DeleteField OnClick="gTransactions_Delete" Visible="false"/>
                </Columns>
            </Rock:Grid>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
