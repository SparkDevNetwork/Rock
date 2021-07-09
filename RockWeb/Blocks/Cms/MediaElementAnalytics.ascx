<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MediaElementAnalytics.ascx.cs" Inherits="RockWeb.Blocks.Cms.MediaElementAnalytics" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading ">
                <h1 class="panel-title"><i class="fa fa-play-circle"></i>
                    <asp:Literal ID="lActionTitle" runat="server" /></h1>
            </div>

            <div class="panel-body">
                <div class="row">
                    <div class="col-md-6 col-lg-7">
                        <div class="margin-b-lg">
                            <asp:Literal ID="lDescription" runat="server" />
                        </div>
                    </div>

                    <div class="col-md-6 col-lg-5">
                        <Rock:MediaPlayer ID="mpMedia" runat="server" AutoResumeInDays="0" CombinePlayStatisticsInDays="0" TrackSession="false" />
                        <div class="text-right">
                            <Rock:HighlightLabel ID="hlDuration" runat="server" LabelType="Default" />
                        </div>
                    </div>
                </div>

                <asp:Panel ID="pnlProviderAnalytics" runat="server">
                    <div><dt>Provider Analytics</dt></div>

                    <div class="well">
                        <asp:Literal ID="lMetricData" runat="server" />
                    </div>
                </asp:Panel>

                <Rock:NotificationBox ID="nbNoData" runat="server" NotificationBoxType="Info" Visible="false" Text="No statistical data is available yet." CssClass="margin-t-md" />

                <asp:Panel ID="pnlAnalytics" runat="server">
                    <asp:HiddenField ID="hfAllTimeVideoData" runat="server" Value="" />
                    <asp:HiddenField ID="hfLast12MonthsVideoData" runat="server" Value="" />
                    <asp:HiddenField ID="hfLast90DaysVideoData" runat="server" Value="" />

                    <ul class="nav nav-tabs">
                      <li role="presentation" class="active">
                          <a href="#<%= pnlLast90DaysDetails.ClientID %>" data-toggle="tab" data-video-data="#<%= hfLast90DaysVideoData.ClientID %>" data-days-back="90">Last 90 Days</a>
                      </li>

                      <li role="presentation">
                          <a href="#<%= pnlLast12MonthsDetails.ClientID %>" data-toggle="tab" data-video-data="#<%= hfLast12MonthsVideoData.ClientID %>" data-days-back="365">Last 12 Months</a>
                      </li>

                      <li role="presentation">
                          <a href="#<%= pnlAllTimeDetails.ClientID %>" data-toggle="tab" data-video-data="#<%= hfAllTimeVideoData.ClientID %>" data-days-back="36500">All Time</a>
                      </li>
                    </ul>

                    <div class="tab-content">
                        <asp:Panel ID="pnlLast90DaysDetails" runat="server" CssClass="tab-pane active">
                            <asp:Literal ID="lLast90DaysContent" runat="server" />
                        </asp:Panel>

                        <asp:Panel ID="pnlLast12MonthsDetails" runat="server" CssClass="tab-pane">
                            <asp:Literal ID="lLast12MonthsContent" runat="server" />
                        </asp:Panel>

                        <asp:Panel ID="pnlAllTimeDetails" runat="server" CssClass="tab-pane">
                            <asp:Literal ID="lAllTimeContent" runat="server" />
                        </asp:Panel>
                    </div>
                </asp:Panel>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
