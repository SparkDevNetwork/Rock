<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MediaDetail.ascx.cs" Inherits="RockWeb.Plugins.tech_triumph.WistiaIntegration.MediaDetail" %>

<asp:UpdatePanel ID="upnlAccounts" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-play"></i> <asp:Literal ID="lTitle" runat="server" /></h1>
                </div>
                <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
                <div class="panel-body">
                    <fieldset id="fieldsetViewSummary" runat="server">
                        <div class="row">
                            <div class="col-md-6">
                                <asp:Literal ID="lblMainDetails" runat="server" />
                            </div>
                            <div class="col-md-6">
                                <dl>
                                    <dt>Thumbnail</dt>
                                    <dd>
                                        <asp:Image ID="imgThumbnail" runat="server" CssClass="img-responsive" />
                                    </dd>
                                </dl>

                                <div class="row">
                                    <div class="col-sm-4">
                                        <div class="metriccard metriccard-success" >
                                            <h1>Engagement</h1>
                                            <span class="value"><asp:Literal ID="lEngagement" runat="server" /></span>
                                            <i class="fa fa-users"></i>
                                        </div>
                                    </div>
                                     <div class="col-sm-4">
                                        <div class="metriccard metriccard-info">
                                            <h1>Load Count</h1>
                                            <span class="value"><asp:Literal ID="lLoadCount" runat="server" /></span>
                                            <i class="fa fa-desktop"></i>
                                        </div>
                                    </div>
                                     <div class="col-sm-4">
                                        <div class="metriccard metriccard-info">
                                            <h1>Play Count</h1>
                                            <span class="value"><asp:Literal ID="lPlayCount" runat="server" /></span>
                                            <i class="fa fa-play-circle-o"></i>
                                        </div>
                                    </div>
                                    <div class="col-sm-4">
                                        <div class="metriccard metriccard-success">
                                            <h1>Hours Watched</h1>
                                            <span class="value"><asp:Literal ID="lHoursWatched" runat="server" /></span>
                                            <i class="fa fa-clock-o"></i>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </fieldset>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
