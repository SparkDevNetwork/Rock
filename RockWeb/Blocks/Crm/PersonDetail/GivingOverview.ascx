<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GivingOverview.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.GivingOverview" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title">
                        <i class="fa fa-area-chart"></i>
                        Giving Overview
                    </h1>
                </div>
                <div class="panel-body pt-0">
                    <asp:Panel ID="pnlGiving" runat="server">
                        <div class="d-flex flex-wrap py-2">
                            <div>
                                <Rock:Badge ID="bdgFirstGift" runat="server" BadgeType=" align-text-bottom" />
                                <Rock:Badge ID="bdgLastGift" runat="server" BadgeType="success align-text-bottom" />
                            </div>
                            <div class="giving-alerts d-inline-flex align-items-center ml-sm-auto">
                                <span class="text-sm mr-2"><i class="fa fa-fw fa-comment-alt text-muted"></i> Giving Alerts</span>
                                <asp:Literal ID="lGivingAlertsBadgesHtml" runat="server" />
                            </div>
                        </div>
                        <hr class="my-0 mx--panel-body">

                        <div id="pnlInactiveGiver" class="alert alert-warning mt-3" runat="server">
                            <strong>Inactive Giver</strong><br/>
                            Last Gift: <asp:Literal ID="lLastGiver" runat="server" />
                        </div>
                        <div id="pnlGivingStats" runat="server">
                        <asp:Literal ID="lLastGiving" runat="server" />
                        <hr class="mt-1 mb-0 mx--panel-body">

                        <div class="row d-flex flex-wrap align-items-start py-3">
                            <div class="col-xs-12 col-sm-12 col-lg-12 giving-by-month">
                                <h5 class="mt-0 mb-3">Giving by Month</h5>
                                <ul class="trend-chart trend-chart-gap" style="height:85px;">
                                    <asp:Repeater ID="rptGivingByMonth" runat="server" OnItemDataBound="rptGivingByMonth_ItemDataBound">
                                        <ItemTemplate>
                                            <asp:Literal ID="lGivingByMonthPercentHtml" runat="server" />
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </ul>
                            </div>
                        </div>

                        <hr class="my-0 mx--panel-body">
                            <div class="py-3">
                                <h5 class="m-0">Giving Characteristics</h5>
                                <%-- Warning if Giving Characteristics are stale, probably because the person hasn't given for a while --%>
                                <Rock:NotificationBox ID="nbGivingCharacteristicsStaleWarning" runat="server" NotificationBoxType="Warning" CssClass="my-3" Text="The giving characteristics below were generated 3 months ago at the time of the last gift. Information on bin, percentile and typical gift patterns represent values from that time period." />
                                <div class="row d-flex flex-wrap align-items-lg-center">
                                    <div class="col-xs-12 col-sm-7 col-md-8 giving-characteristics">
                                        <%-- Lava Output for Giving Characteristics  --%>
                                        <asp:Literal ID="lGivingCharacteristicsHtml" runat="server" />
                                    </div>
                                    <%-- Community View --%>
                                    <div class="col-xs-12 col-sm-5 col-md-4 percentile-giving mx-auto">
                                        <hr class="my-3 d-sm-none">
                                        <span class="text-sm text-muted p-0 mb-1">Community View</span>
                                        <div class="d-flex flex-column rollover-container">
                                            <div class="rollover-item inset-0 z-10 well m-0 border-0 small"><asp:Literal ID="lHelpText" runat="server" /></div>
                                            <div class="d-flex align-items-center mb-2">
                                                <span class="stat-value" style="font-size:30px;line-height:1;font-weight:700;"><asp:Literal ID="lPercent" runat="server" />%</span>
                                                <span class="stat-label leading-snug text-sm ml-2">Percentile<br/>Bin <asp:Literal ID="lGivingBin" runat="server" /></span>
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
                            </div>

                        </div>
                        <hr class="my-0 mx--panel-body">

                        <%-- Yearly Summary for each year  --%>
                        <h5 class="mt-3">Yearly Summary</h5>
                        <div class="row d-flex flex-wrap">
                            <asp:Repeater ID="rptYearSummary" runat="server" OnItemDataBound="rptYearSummary_ItemDataBound">
                                <ItemTemplate>
                                    <div class="col-xs-12 col-sm-6">
                                        <table class="table table-condensed my-2">
                                            <thead>
                                                <th class="bg-transparent" colspan="2"><asp:Literal ID="lYearlySummaryYear" runat="server" /></th>
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
