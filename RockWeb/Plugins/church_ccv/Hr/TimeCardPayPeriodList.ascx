<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TimeCardPayPeriodList.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Hr.TimeCardPayPeriodList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-calendar-o"></i>&nbsp;Pay Periods</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gList" runat="server" AllowSorting="true" RowItemText="Pay Period" OnRowSelected="gList_RowSelected">
                        <Columns>
                            <Rock:RockBoundField HeaderText="Pay Period" DataField="PayPeriod" SortExpression="PayPeriod.StartDate" />
                            <Rock:RockBoundField HeaderText="In Progress" DataField="CardsInProgressCount" SortExpression="CardsInProgressCount" />
                            <Rock:RockBoundField HeaderText="Submitted" DataField="CardsSubmittedCount" SortExpression="CardsSubmittedCount" />
                            <Rock:RockBoundField HeaderText="Approved" DataField="CardsApprovedCount" SortExpression="CardsApprovedCount" />
                            <Rock:RockBoundField HeaderText="Paid" DataField="CardsPaidCount" SortExpression="CardsPaidCount" />
                            <Rock:RockBoundField HeaderText="Total" DataField="CardsCount" SortExpression="CardsCount" />
                            <Rock:DeleteField OnClick="gList_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
