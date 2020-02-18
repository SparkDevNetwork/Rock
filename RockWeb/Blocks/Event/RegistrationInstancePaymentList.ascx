<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrationInstancePaymentList.ascx.cs" Inherits="RockWeb.Blocks.Event.RegistrationInstancePaymentList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server">

                <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />

                <asp:Panel ID="pnlPayments" runat="server" CssClass="panel panel-block">
                    <div class="panel-heading">
                        <h1 class="panel-title">
                            <i class="fa fa-credit-card"></i>
                            Payments
                        </h1>
                    </div>
                    <div class="panel-body">
                        <Rock:ModalAlert ID="mdPaymentsGridWarning" runat="server" />
                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="fPayments" runat="server" OnDisplayFilterValue="fPayments_DisplayFilterValue" OnClearFilterClick="fPayments_ClearFilterClick">
                                <Rock:SlidingDateRangePicker ID="sdrpPaymentDateRange" runat="server" Label="Transaction Date Range" />
                            </Rock:GridFilter>
                            <Rock:Grid ID="gPayments" runat="server" DisplayType="Full" AllowSorting="true" RowItemText="Payment" OnRowSelected="gPayments_RowSelected" ExportSource="ColumnOutput">
                                <Columns>
                                    <Rock:RockBoundField DataField="AuthorizedPersonAlias.Person.FullNameReversed" HeaderText="Person"
                                        SortExpression="AuthorizedPersonAlias.Person.LastName,AuthorizedPersonAlias.Person.NickName" />
                                    <Rock:RockBoundField DataField="TransactionDateTime" HeaderText="Date / Time" SortExpression="TransactionDateTime" />
                                    <Rock:CurrencyField DataField="TotalAmount" HeaderText="Amount" SortExpression="TotalAmount" />
                                    <Rock:RockBoundField DataField="FinancialPaymentDetail.CurrencyAndCreditCardType" HeaderText="Payment Method" />
                                    <Rock:RockBoundField DataField="FinancialPaymentDetail.AccountNumberMasked" HeaderText="Account" />
                                    <Rock:RockBoundField DataField="TransactionCode" HeaderText="Transaction Code" SortExpression="TransactionCode" ColumnPriority="DesktopSmall" />
                                    <Rock:RockTemplateFieldUnselected HeaderText="Registrar">
                                        <ItemTemplate>
                                            <asp:Literal ID="lRegistrar" runat="server" />
                                        </ItemTemplate>
                                    </Rock:RockTemplateFieldUnselected>
                                    <Rock:RockTemplateField HeaderText="Registrant(s)">
                                        <ItemTemplate>
                                            <asp:Literal ID="lRegistrants" runat="server" />
                                        </ItemTemplate>
                                    </Rock:RockTemplateField>
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>
                </asp:Panel>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
