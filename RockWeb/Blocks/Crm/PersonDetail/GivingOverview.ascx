<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GivingOverview.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.GivingOverview" %>
<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-chart-area"></i>Giving Overview</h1>
                </div>
                <div class="panel-body pt-0">
                    <div id="pnlInactiveGiver" class="alert alert-warning mt-3" runat="server">
                        <strong>Inactive Giver</strong><br/>
                        Last Gift: <asp:Literal ID="lLastGiver" runat="server" />
                    </div>
                    <asp:Panel ID="pnlGiving" runat="server">
                        <div id="pnlGivingStats" runat="server">
                        <asp:Literal ID="lLastGiving" runat="server" />
                        <hr class="m-0">

                        <div class="row d-flex flex-wrap align-items-start py-3">
                            <div class="col-xs-12 col-sm-8 col-lg-9 giving-by-month">
                                <h5 class="kpi-lg-label mt-0 mb-2">Giving by Month</h5>
                                <ul class="trend-chart trend-chart-gap" style="height:85px;">
                                    <asp:Repeater ID="rptGivingByMonth" runat="server">
                                        <ItemTemplate>
                                            <li title="<%#( ( ( DateTime ) Eval( "key" ) ).ToString( "MMM yyyy" ) ) %>: <%#  Rock.ExtensionMethods.FormatAsCurrency((decimal)Eval("value")) %>"><span style="<%# GetGivingByMonthPercent ( (decimal)Eval( "value" ) ) %>"></span></li>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </ul>
                            </div>

                            <div class="col-xs-10 col-sm-4 col-lg-3 percentile-giving mx-auto mt-3 m-sm-0">
                                <h5 class="kpi-lg-label mt-0 mb-2">Community View</h5>
                                <div class="d-flex flex-row rollover-container">
                                    <div class="rollover-item inset-0 z-10 bg-gray-100 p-1 small"><asp:Literal ID="lHelpText" runat="server" /></div>
                                    <div class="pr-2">
                                        <span class="stat-value-lg"><asp:Literal ID="lPercent" runat="server" />%</span>
                                        <span class="stat-label">Percentile<span class="d-block small">Bin <asp:Literal ID="lGivingBin" runat="server" /></span></span>
                                    </div>
                                    <div class="flex-fill">
                                        <ul class="trend-chart" style="height: 70px;">
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
                                        </ul>

                                        <ul class="trend-bar list-unstyled">
                                            <li style="width: 5%"><span id="lBin1" runat="server"></span></li>
                                            <li style="width: 15%"><span id="lBin2" runat="server"></span></li>
                                            <li style="width: 20%"><span id="lBin3" runat="server"></span></li>
                                            <li style="width: 60%"><span id="lBin4" runat="server"></span></li>
                                        </ul>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <hr class="m-0">
                        <asp:Literal ID="lGivingAnalytics" runat="server" />
                        </div>
                        <hr class="m-0">
                        <h5 class="mt-4">Yearly Summary</h5>
                        <div class="row d-flex flex-wrap">
                            <asp:Repeater ID="rptYearSummary" runat="server" OnItemDataBound="rptYearSummary_ItemDataBound">
                                <ItemTemplate>
                                    <div class="col-xs-12 col-sm-6">
                                        <table class="table table-condensed my-2">
                                            <thead>
                                                <th class="bg-transparent" colspan="2"><%# Eval("Year") %></th>
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

                        <div class="text-center margin-v-sm">
                            <asp:LinkButton runat="server" ID="lbShowMoreYearlySummary" class="btn btn-xs btn-default" OnClick="lbShowMoreYearlySummary_Click">
                                <i class="fa fa-chevron-down"></i>
                                Show More
                                <i class="fa fa-chevron-down"></i>
                            </asp:LinkButton>
                            <asp:LinkButton runat="server" ID="lbShowLessYearlySummary" class="btn btn-xs btn-default" OnClick="lbShowLessYearlySummary_Click">
                                <i class="fa fa-chevron-up"></i>
                                Show Less
                                <i class="fa fa-chevron-up"></i>
                            </asp:LinkButton>
                        </div>
                    </asp:Panel>
                    <asp:Panel ID="pnlNoGiving" runat="server">
                        <p class="mt-3 mb-0">No giving data available.</p>
                    </asp:Panel>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>