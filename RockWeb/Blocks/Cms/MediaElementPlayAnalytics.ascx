<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MediaElementPlayAnalytics.ascx.cs" Inherits="RockWeb.Blocks.Cms.MediaElementPlayAnalytics" %>


<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading ">
                <h1 class="panel-title"><i class="fa fa-play-circle"></i>
                    <asp:Literal ID="lActionTitle" runat="server" /></h1>
            </div>

            <div class="panel-body">
                <div class="row">
                    <div class="col-md-6">
                        <asp:Panel ID="pnlChart" runat="server">
                            <div class="video-container">
                                <div class="chart-container">
                                    <canvas id="cChart" runat="server" class="chart-canvas"></canvas>
                                </div>

                                <asp:Panel ID="pnlMediaPlayer" runat="server" />
                                <Rock:MediaPlayer Visible="false" ID="mpMedia" runat="server" ClickToPlay="false" PlayerControls="" AutoResumeInDays="0" CombinePlayStatisticsInDays="0" TrackSession="false" />
                            </div>
                        </asp:Panel>
                    </div>

                    <div class="col-md-6">
                        <div>
                            <div class="text-semibold">Average Watch Engagement</div>
                            <div class="text-sm mb-1">The total minutes played divided by the length of the video times the total number of plays.</div>
                            <asp:Panel ID="pnlAverageWatchEngagement" runat="server" CssClass="analytic-value">
                                <i class="fa fa-refresh fa-spin"></i>
                            </asp:Panel>

                            <hr />

                            <div class="text-semibold">Total Plays</div>
                            <div class="text-sm mb-1">The total number of times the video was played.</div>
                            <asp:Panel ID="pnlTotalPlays" runat="server" CssClass="analytic-value">
                                <i class="fa fa-refresh fa-spin"></i>
                            </asp:Panel>

                            <hr />

                            <div class="text-semibold">Minutes Watched</div>
                            <div class="text-sm mb-1">The total number of minutes the video was watched by all viewers.</div>
                            <asp:Panel ID="pnlMinutesWatched" runat="server" CssClass="analytic-value">
                                <i class="fa fa-refresh fa-spin"></i>
                            </asp:Panel>
                        </div>
                    </div>
                </div>

                <p class="margin-t-lg">
                    <strong>Individual Plays</strong>
                </p>

                <asp:Panel ID="pnlIndividualPlays" runat="server">
                    <div class="text-center">
                        <button class="js-load-more btn btn-primary mt-2">Load More</button>
                    </div>
                </asp:Panel>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
