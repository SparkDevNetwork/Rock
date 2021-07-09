<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MediaElementPlayAnalytics.ascx.cs" Inherits="RockWeb.Blocks.Cms.MediaElementPlayAnalytics" %>

<style>
    .media-element-play-analytics .video-container {
        position: relative;
    }

    .media-element-play-analytics .video-container > .chart-container {
        position: absolute;
        width: 100%;
        height: 100%;
        z-index: 1;
    }

    .media-element-play-analytics .analytic-value {
        font-size: 28px;
        font-weight: 600;
    }

    .media-element-play-analytics .individual-play-row {
        display: flex;
        flex-direction: row;
        margin-bottom: 4px;
    }

    .individual-play-row .individual-play-date {
        padding: 4px 4px 4px 0px;
        font-size: 12px;
        color: #686868;
        line-height: initial;
        width: 100px;
        flex: 0 0 auto;
    }

    .individual-play-row .individual-play-person {
        width: 200px;
        padding: 8px;
        background-color: #ccc;
        border-top-left-radius: 4px;
        border-bottom-left-radius: 4px;
        flex: 0 0 auto;
        display: flex;
        align-items: center;
    }

    .individual-play-row .individual-play-chart {
        flex-grow: 1;
        flex-shrink: 1;
        height: 60px;
    }

    .individual-play-row .individual-play-chart > svg {
        width: 100%;
        height: 60px;
        pointer-events: none;
    }

    .individual-play-row .individual-play-percent {
        width: 60px;
        padding: 8px;
        flex: 0 0 auto;
        background-color: #ccc;
        border-top-right-radius: 4px;
        border-bottom-right-radius: 4px;
        display: flex;
        align-items: center;
        justify-content: center;
    }
</style>

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
                            <div>The total minutes played divided by the length of the video times the total number of plays.</div>
                            <asp:Panel ID="pnlAverageWatchEngagement" runat="server" CssClass="analytic-value">
                                <i class="fa fa-refresh fa-spin"></i>
                            </asp:Panel>

                            <hr />

                            <div class="text-semibold">Total Plays</div>
                            <div>The total number of times the video was played.</div>
                            <asp:Panel ID="pnlTotalPlays" runat="server" CssClass="analytic-value">
                                <i class="fa fa-refresh fa-spin"></i>
                            </asp:Panel>

                            <hr />

                            <div class="text-semibold">Minutes Watched</div>
                            <div>The total number of minutes the video was watched by all viewers.</div>
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
                    <button class="js-load-more btn btn-primary">Load More</button>
                </asp:Panel>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
