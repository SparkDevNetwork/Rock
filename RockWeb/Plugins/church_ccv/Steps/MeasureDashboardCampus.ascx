<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MeasureDashboardCampus.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Steps.MeasureDashboardCampus" %>

<style>
    
    h2 small {
        font-size: 14px;
    }

    .measure h4 {
        margin-bottom: 4px;
    }
    
    .measure-value{
        padding-top: 12px;
    }

    .measure-icon {
        width: 60px;
        float: left;
        font-size: 40px;
        padding-top: 31px;
    }

    .measure-details {
        width: 100%;
    }
    
    .measure-bar {
        height: 40px;
    }

    .is-tbd .measure-bar,
    .is-tbd .measure-value {
        display: none;
    }

    .measure-tbd {
        font-size: 20px;
        opacity: .5;
        padding-top: 6px;
    }

    .measure-bar .progress-bar{
        font-size: 18px;
        padding-top: 9px;
    }
</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-bar-chart"></i> Measure Dashboard</h1>
                <div class="pull-right">
                    <Rock:Toggle ID="tglCompareTo" CssClass="margin-r-sm pull-left"  runat="server" OnText="Active Adults" ActiveButtonCssClass="btn-info" ButtonSizeCssClass="btn-xs" OffText="Weekend Attendance" AutoPostBack="true" OnCheckedChanged="tglCompareTo_CheckedChanged" Checked="true" />
                    <Rock:HighlightLabel ID="hlDate" runat="server" />
                </div>
            </div>
            <div class="panel-body">
                
                <asp:Panel ID="pnlCampus" runat="server">
                    <div class="row">
                        <div class="col-md-8">
                            <h2><asp:Literal ID="lCampusTitle" runat="server" /></h2>
                        </div>
                        <div class="col-md-4">
                            <Rock:CampusPicker ID="cpCampus" runat="server" OnSelectedIndexChanged="cpCampus_SelectedIndexChanged" AutoPostBack="true" />
                        </div>
                    </div>

                    <div class="row">
                        <asp:Repeater ID="rptCampusMeasures" runat="server">
                            <ItemTemplate>
                                <a href="?MeasureId=<%# Eval("MeasureId") %>&CompareTo=<%=tglCompareTo.Checked %>">
                                    <div class="col-md-6 measure row-eq-height">
                                        <div class="measure-icon hidden-sm hidden-xs">
                                            <i class="fa fa-fw <%# Eval("IconCssClass") %>" style="color: <%# Eval("MeasureColor") %>;"></i>
                                        </div>

                                        <div class="measure-details <%# (bool)Eval("IsTbd") ? "is-tbd" : ""%>">
                                    
                                            <div class="clearfix">
                                                <h4 class="pull-left"><%# Eval("Title") %></h4>
                                                <div class="pull-right measure-value">
                                                    <%# Eval("MeasureValue","{0:#,0}") %>
                                                </div>
                                            </div>
                                            <%# (bool)Eval("IsTbd") ? "<div class='measure-tbd'>TBD</div>" : ""%>
                                            <div class="progress measure-bar <%# (int)Eval("Percentage") > 100 ? "percent-over-100": "" %>" style="background-color: <%# Eval("MeasureColorBackground") %>; ">
                                                  <div class="progress-bar" role="progressbar" aria-valuenow="60" aria-valuemin="0" aria-valuemax="100" style="width: <%# Eval("Percentage") %>%; background-color: <%# Eval("MeasureColor") %>;">
                                                    <%# Eval("Percentage") %>%
                                                  </div>
                                            </div>
                                        </div>
                                    </div>
                                </a>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </asp:Panel>
                
                <asp:Panel ID="pnlMeasure" runat="server">
                    <h2><asp:Literal ID="lMeasureIcon" runat="server" /> <asp:Literal id="lMeasureTitle" runat="server" /></h2>
                    <p>
                        <asp:Literal ID="lMeasureDescription" runat="server" />
                    </p>

                    <!-- Campus Chart -->
                    <div class="row">
                        <div class="col-md-6 measure ">
                            <div class="measure-details">
                                <div class="clearfix">
                                    <h4 class="pull-left">All Campuses</h4>
                                    <div class="pull-right measure-value">
                                        <asp:Literal ID="lMeasureCampusSumValue" runat="server" />
                                    </div>
                                </div>
                                <div class="progress measure-bar <%# (int)Eval("Percentage") > 100 ? "percent-over-100": "" %>" style='background-color: <asp:Literal id="lMeasureBackgroundColor" runat="server" />'>
                                    <div class="progress-bar" role="progressbar" aria-valuenow="60" aria-valuemin="0" aria-valuemax="100" style='width: <asp:Literal id="lMeasureBarPercent" runat="server" />%; background-color: <asp:Literal id="lMeasureColor" runat="server" />;'>
                                        <asp:Literal id="lMeasureBarTextPercent" runat="server" />%
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <hr />

                    <asp:Repeater ID="rptMeasuresByCampus" runat="server">
                        <ItemTemplate>
                                <div class="row">
                                    <div class="col-md-6 measure">
                                        <div class="measure-details">
                                            <div class="clearfix">
                                                <h4 class="pull-left"><%# Eval("Campus") %></h4>
                                                <div class="pull-right measure-value">
                                                    <%# Eval("MeasureValue","{0:#,0}") %>
                                                </div>
                                            </div>
                                            <div class="progress measure-bar <%# (int)Eval("Percentage") > 100 ? "percent-over-100": "" %>" style="background-color: <%# Eval("MeasureColorBackground") %>; ">
                                                  <div class="progress-bar" role="progressbar" aria-valuenow="60" aria-valuemin="0" aria-valuemax="100" style="width: <%# Eval("Percentage") %>%; background-color: <%# Eval("MeasureColor") %>;">
                                                    <%# Eval("Percentage") %>%
                                                  </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </ItemTemplate>
                    </asp:Repeater>

                    <asp:LinkButton ID="btnBackToCampus" runat="server" CssClass="btn btn-default" OnClick="btnBackToCampus_Click"><i class="fa fa-chevron-left"></i> Campus View</asp:LinkButton>
                </asp:Panel>
            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
