﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduledTransactionList.ascx.cs" Inherits="RockWeb.Blocks.Finance.ScheduledTransactionList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">

            <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-calendar"></i> Scheduled Transaction List</h1>
                </div>
                <div class="panel-body">

                    <div class="grid grid-panel">
                        <Rock:GridFilter ID="gfSettings" runat="server">
                            <Rock:NumberRangeEditor ID="nreAmount" runat="server" Label="Amount Range" NumberType="Double" />
                            <Rock:RockDropDownList ID="ddlFrequency" runat="server" Label="Frequency" />
                            <Rock:DateRangePicker ID="drpDates" runat="server" Label="Created" />
                            <Rock:RockDropDownList ID="ddlAccount" runat="server" Label="Account" />
                            <Rock:RockCheckBox ID="cbIncludeInactive" runat="server" Label="Include Inactive Schedules" Text="Yes" />
                        </Rock:GridFilter>
                        <Rock:Grid ID="gList" AllowSorting="true" runat="server" EmptyDataText="No Scheduled Transactions Found"
                            ShowConfirmDeleteDialog="true" RowItemText="Scheduled Transaction" OnRowSelected="gList_Edit" ExportSource="ColumnOutput">
                            <Columns>
                                <Rock:RockBoundField DataField="AuthorizedPersonAlias.Person.FullNameReversed" HeaderText="Contributor"
                                    SortExpression="AuthorizedPersonAlias.Person.LastName,AuthorizedPersonAlias.Person.NickName" />
                                <Rock:CurrencyField DataField="TotalAmount" HeaderText="Amount" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" SortExpression="Amount" />
                                <Rock:RockTemplateField HeaderText="Accounts">
                                    <ItemTemplate><%# GetAccounts( Container.DataItem ) %></ItemTemplate>
                                </Rock:RockTemplateField>
                                <Rock:RockBoundField DataField="TransactionFrequencyValue" HeaderText="Frequency" SortExpression="TransactionFrequencyValue.Value" />
                                <Rock:DateField DataField="CreatedDateTime" HeaderText="Created" SortExpression="CreatedDateTime" />
                                <Rock:DateField DataField="StartDate" HeaderText="Starting" SortExpression="StartDate" />
                                <Rock:DateField DataField="EndDate" HeaderText="Ending" SortExpression="EndDate" />
                                <Rock:DateField DataField="NextPaymentDate" HeaderText="Next Payment" SortExpression="NextPaymentDate" />
                                <Rock:DefinedValueField DataField="FinancialPaymentDetail.CurrencyTypeValueId" HeaderText="Currency Type" SortExpression="FinancialPaymentDetail.CurrencyTypeValue.Value" />
                                <Rock:RockBoundField DataField="GatewayScheduleId" HeaderText="Schedule ID" SortExpression="GatewayScheduleId" />
                                <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                                <Rock:DateTimeField DataField="LastStatusUpdateDateTime" HeaderText="Last Refresh" SortExpression="LastStatusUpdateDateTime" />
                            </Columns>
                        </Rock:Grid>
                    </div>

                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>





