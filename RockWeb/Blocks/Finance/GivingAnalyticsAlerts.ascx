﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GivingAnalyticsAlerts.ascx.cs" Inherits="RockWeb.Blocks.Finance.GivingAnalyticsAlerts" %>

<asp:UpdatePanel ID="upnlAlerts" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Danger" Visible="true" />

        <asp:Panel ID="pnlAlertList" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-comment-alt"></i> Giving Alerts</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">

                    <Rock:GridFilter ID="gfAlertFilter" runat="server">
                        <Rock:DateRangePicker ID="drpDateRange" runat="server" Label="Date Range" />
                        <Rock:RockCheckBoxList ID="cblAlertType" runat="server" Label="Alert Type" RepeatDirection="Horizontal" />
                        <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" />
                        <Rock:NumberRangeEditor ID="nreTransaction" runat="server" Label="Transaction" NumberType="Double" />
                        <Rock:CampusPicker ID="cpCampus" runat="server" Label="Campus" />
                    </Rock:GridFilter>

                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gAlertList" runat="server" RowItemText="Alert" EmptyDataText="No Alert Found" AllowSorting="true" OnRowDataBound="gAlertList_RowDataBound" ExportSource="ColumnOutput">
                        <Columns>
                            <Rock:RockLiteralField ID="lStatusIcons" HeaderText="" HeaderStyle-CssClass="w-1" ItemStyle-CssClass="w-1 badge-legend" />
                            <Rock:DateField DataField="AlertDateTime" HeaderText="Alert Date" SortExpression="AlertDateTime" />
                            <Rock:PersonField DataField="PersonAlias.Person" HeaderText="Name" SortExpression="PersonAlias.Person.LastName,PersonAlias.Person.NickName" />
                            <Rock:RockBoundField DataField="FinancialTransactionAlertType.Campus" HeaderText="Campus" />
                            <Rock:RockBoundField DataField="FinancialTransactionAlertType.Name" HeaderText="Alert Name" SortExpression="FinancialTransactionAlertType.Name" />
                            <Rock:RockLiteralField ID="lGiftAmount" HeaderText="Gift Amount" SortExpression="Amount" />
                            <Rock:RockLiteralField ID="lAmountMedian" HeaderText="Amount +/- Median" />
                            <Rock:RockLiteralField ID="lDaysMean" HeaderText="Days +/- Mean" />
                            <Rock:RockLiteralField ID="lAmtMeasures" HeaderText="Amt Measures" />
                            <Rock:RockLiteralField ID="lFreqMeasures" HeaderText="Freq Measures" />
                        </Columns>
                    </Rock:Grid>

                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
