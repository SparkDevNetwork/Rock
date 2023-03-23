<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Insights.ascx.cs" Inherits="RockWeb.Blocks.Reporting.Insights" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="panel panel-block">

            <div class="panel-heading panel-follow">
                <h1 class="panel-title">
                    Insights
                </h1>
            </div>

            <div class="panel-body">

                <div class="rock-header">
                    <h3 class="title">Demographics</h3>
                    <span class="description">The demographic charts below provide a broad view of who we are as a community.</span>
                    <hr class="section-header-hr" />
                </div>

                <!-- Demographics -->
                <div class="row">
                    <asp:Repeater ID="rptDemographics" runat="server">
                        <ItemTemplate>
                            <div class="col-md-3">
                                <div class="panel panel-block">
                                    <div class="panel-heading">
                                        <div class="pull-left">
                                            <h1 class="panel-title"><%# Eval("Title") %></h1>
                                        </div>

                                        <div class="panel-labels">
                                            <a class="btn btn-default btn-xs hidden">
                                                <i class="fa fa-line-chart"></i>
                                            </a>
                                        </div>
                                    </div>

                                    <div class="panel-body">
                                        <%# Eval("Chart") %>
                                    </div>

                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

                <!-- Information Statistics -->
                <div class="rock-header">
                    <h3 class="title">Information Statistics</h3>
                    <span class="description">The more we know about our people the more we can serve them and tailor our ministry for their unique needs.</span>
                    <hr class="section-header-hr" />
                </div>

                <!-- Information Completeness -->
                <div class="panel panel-block">
                    <div class="panel-heading">
                        <div class="pull-left">
                            <h1 class="panel-title">Information Completeness</h1>
                        </div>

                        <div class="panel-labels">
                            <a class="btn btn-default btn-xs hidden">
                                <i class="fa fa-line-chart"></i>
                            </a>
                        </div>
                    </div>

                    <div class="panel-body">
                        <Rock:RockLiteral ID="rlInformationCompleteness" runat="server" />
                    </div>

                </div>

                <!-- Percent of Active Individuals With Assessments -->
                <div class="panel panel-block">
                    <div class="panel-heading">
                        <div class="pull-left">
                            <h1 class="panel-title">Percent of Active Individuals With Assessments</h1>
                        </div>

                        <div class="panel-labels">
                            <a class="btn btn-default btn-xs hidden">
                                <i class="fa fa-line-chart"></i>
                            </a>
                        </div>
                    </div>

                    <div class="panel-body">
                        <Rock:RockLiteral ID="rlActiveIndividualsWithAssessments" runat="server" />
                    </div>

                </div>

                <div class="row">
                    <div class="col-md-3">
                        <!-- Percent of Active Records -->
                        <div class="panel panel-block">
                            <div class="panel-heading">
                                <div class="pull-left">
                                    <h1 class="panel-title">Percent of Active Records</h1>
                                </div>

                                <div class="panel-labels">
                                    <a class="btn btn-default btn-xs hidden">
                                        <i class="fa fa-line-chart"></i>
                                    </a>
                                </div>
                            </div>

                            <div class="panel-body">
                                <Rock:RockLiteral ID="rlActiveRecords" runat="server" />
                            </div>

                        </div>
                    </div>
                </div>


            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
