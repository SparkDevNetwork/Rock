<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GivingOverview.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.GivingOverview" %>
<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-chart-area"></i>Giving Overview</h1>
                </div>
                <div class="panel-body pt-0">
                    <asp:Panel ID="pnlGiving" runat="server">

                        <asp:Literal ID="lLastGiving" runat="server" />
                        <hr>
                        <div class="row d-flex flex-wrap align-items-end py-1">
                            <div class="col-xs-12 col-sm-8 col-lg-9 giving-by-month">
                                <h5 class="mt-0">Giving by Month</h5>
                                <ul class="attendance-chart" style="height: 70px;">
                                    <asp:Repeater ID="rptGivingByMonth" runat="server">
                                        <ItemTemplate>
                                            <li title="<%#( ( ( DateTime ) Eval( "key" ) ).ToString( "MMM yyyy" ) ) %>"><span style="<%#  GetGivingByMonthPercent ( (decimal)Eval( "value" ) ) %>"></span></li>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </ul>
                            </div>

                            <div class="col-xs-6 col-sm-4 col-lg-3 percentile-giving">
                                <div class="d-flex flex-row">
                                    <div class="pr-3">
                                        <span class="stat-value-lg"><asp:Literal ID="lGivingBin" runat="server" /></span>
                                        <span class="stat-label"><asp:Literal ID="lPercent" runat="server" /> %</span>
                                    </div>
                                    <div class="flex-fill">
                                        <ul class="attendance-chart" style="height: 70px;">
                                            <li><span id="lStage1" runat="server" style="height: 100%"></span></li>
                                            <li><span id="lStage2" runat="server" style="height: 70%"></span></li>
                                            <li><span id="lStage3" runat="server" style="height: 48%"></span></li>
                                            <li><span id="lStage4" runat="server" style="height: 33%"></span></li>
                                            <li><span id="lStage5" runat="server" style="height: 25%"></span></li>
                                            <li><span id="lStage6" runat="server" style="height: 20%"></span></li>
                                            <li><span id="lStage7" runat="server" style="height: 15%"></span></li>
                                            <li><span id="lStage8" runat="server" style="height: 12.5%"></span></li>
                                            <li><span id="lStage9" runat="server" style="height: 10%"></span></li>
                                            <li><span id="lStage10" runat="server" style="height: 8%"></span></li>
                                            <ul>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <hr class="mb-4">
                        <asp:Literal ID="lGivingAnalytics" runat="server" />
                        <small>Metrics are as of the last gift.</small>
                        <hr class="mt-4">
                        <h5>Yearly Summary</h5>
                        <div class="row d-flex flex-wrap">
                            <asp:Repeater ID="rptYearSummary" runat="server" OnItemDataBound="rptYearSummary_ItemDataBound">
                                <ItemTemplate>
                                    <div class="col-xs-12 col-sm-6">
                                        <table class="table table-condensed my-2">
                                            <thead>
                                                <th colspan="2"><%# Eval("Year") %></th>
                                            </thead>
                                            <tbody>
                                                <asp:Literal ID="lAccount" runat="server" />
                                            </tbody>
                                            <tfoot>
                                                <tr>
                                                    <th>Total</th>
                                                    <th class="text-right">
                                                        <asp:Literal ID="lTotalAmount" runat="server" /></th>
                                                </tr>
                                            </tfoot>
                                        </table>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>

                        </div>
                    </asp:Panel>
                    <asp:Panel ID="pnlNoGiving" runat="server">
                        <p>No giving data available.</p>
                    </asp:Panel>
                    <div id="pnlInactiveGiver" runat="server">
                        Inactive Giver <br />
                        Last Gift: <asp:Literal ID="lLastGiver" runat="server" />
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
