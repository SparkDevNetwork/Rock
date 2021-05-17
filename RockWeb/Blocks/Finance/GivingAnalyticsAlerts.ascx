<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GivingAnalyticsAlerts.ascx.cs" Inherits="RockWeb.Blocks.Finance.GivingAnalyticsAlerts" %>

<asp:UpdatePanel ID="upnlAlerts" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Danger" Visible="true" />

        <asp:Panel ID="pnlAlertList" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-comment-alt"></i> Giving Alerts</h1>
                <div class="pull-right d-flex align-items-center">
                    <asp:LinkButton ID="lbConfig" runat="server" CssClass="btn btn-xs btn-square btn-default pull-right" OnClick="lbConfig_Click" CausesValidation="false">
                        <i title="Options" class="fa fa-gear"></i>
                    </asp:LinkButton>
                </div>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">

                    <Rock:GridFilter ID="gfAlertFilter" runat="server">
                        <Rock:DateRangePicker ID="drpDateRange" runat="server" Label="Date Range" />
                        <Rock:RockCheckBoxList ID="cblAlertCategory" runat="server" Label="Alert Category" RepeatDirection="Horizontal" />
                        <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" />
                        <Rock:NumberRangeEditor ID="nreTransactionAmount" runat="server" Label="Transaction Amount" NumberType="Double" />
                        <Rock:CampusPicker ID="cpCampus" runat="server" Label="Campus" />
                        <Rock:RockCheckBoxList ID="cblAlertTypes" runat="server" Label="Alert Types" RepeatDirection="Horizontal" />
                    </Rock:GridFilter>

                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gAlertList" runat="server" RowItemText="Alert" EmptyDataText="No Alert Found" AllowSorting="true" OnRowDataBound="gAlertList_RowDataBound" ExportSource="ColumnOutput">
                        <Columns>
                            <Rock:RockLiteralField ID="lStatusIcons" HeaderText="" HeaderStyle-CssClass="w-1" ItemStyle-CssClass="w-1 badge-legend" />
                            <Rock:DateField DataField="AlertDateTime" HeaderText="Alert Date" SortExpression="AlertDateTime" />
                            <Rock:PersonField DataField="PersonAlias.Person" HeaderText="Name" SortExpression="PersonAlias.Person.LastName,PersonAlias.Person.NickName" UrlFormatString="~/Person/{0}/Contributions" />
                            <Rock:RockBoundField DataField="FinancialTransactionAlertType.Campus" HeaderText="Campus" />
                            <Rock:RockBoundField DataField="FinancialTransactionAlertType.Name" HeaderText="Alert Name" SortExpression="FinancialTransactionAlertType.Name" />
                            <Rock:RockLiteralField ID="lGiftAmount" HeaderText="Gift Amount" SortExpression="Amount" ItemStyle-HorizontalAlign="Right" />
                            <Rock:RockLiteralField ID="lAmountMedian" HeaderText="Amount +/- Median" ItemStyle-HorizontalAlign="Right" />
                            <Rock:RockLiteralField ID="lDaysMean" HeaderText="Days +/- Mean" ItemStyle-HorizontalAlign="Right" />
                            <Rock:RockLiteralField ID="lAmtMeasures" HeaderText="Amt Measures" ItemStyle-HorizontalAlign="Right" />
                            <Rock:RockLiteralField ID="lFreqMeasures" HeaderText="Freq Measures" ItemStyle-HorizontalAlign="Right" />
                        </Columns>
                    </Rock:Grid>

                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
