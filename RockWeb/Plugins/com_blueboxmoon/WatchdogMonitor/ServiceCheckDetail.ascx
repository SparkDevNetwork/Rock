<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ServiceCheckDetail.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor.ServiceCheckDetail" %>
<%@ Register Namespace="com.blueboxmoon.WatchdogMonitor.Web.UI.Controls" Assembly="com.blueboxmoon.WatchdogMonitor" TagPrefix="WM" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

        <asp:Panel ID="pnlDetails" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlSilenced" runat="server" LabelType="Warning" Text="Silenced" Visible="false" />
                    <asp:Literal ID="lState" runat="server" />
                </div>

                <h1 class="panel-title">
                    <i class="fa fa-crosshairs"></i>
                    <asp:Literal ID="lDetailsName" runat="server" />
                </h1>
            </div>

            <div class="panel-body">
                <fieldset>
                    <div class="row">
                        <div class="col-md-6">
                            <dl>
                                <dt>Device</dt>
                                <dd><asp:Literal ID="lDevice" runat="server" /></dd>
                            </dl>

                            <dl>
                                <dt>Value</dt>
                                <dd><asp:Literal ID="lValue" runat="server" /></dd>
                            </dl>

                            <dl>
                                <dt>Summary</dt>
                                <dd><asp:Literal ID="lSummary" runat="server" /></dd>
                            </dl>
                        </div>
                        <div class="col-md-6">
                            <dl>
                                <dt>Last State Change</dt>
                                <dd><asp:Literal ID="lLastStateChange" runat="server" /></dd>
                            </dl>

                            <dl>
                                <dt>Last Check</dt>
                                <dd><asp:Literal ID="lLastCheck" runat="server" /></dd>
                            </dl>
                        </div>
                    </div>
                </fieldset>

                <div class="js-service-check-chart" data-id='<%= PageParameter( "ServiceCheckId" ) %>' data-range="all" data-default-view="1w">
                    <div style="height: 300px; width: 100%; position: relative;">
                        <canvas></canvas>
                        <div style="position: absolute; top: 0; bottom: 0; left: 0; right: 0; background-color: rgba(0,0,0,0.02);" class="text-center js-service-check-loading">
                            <i class="fa fa-spinner fa-spin fa-3x" style="line-height: 300px;"></i>
                        </div>
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="lbToggleSilence" runat="server" CssClass="btn btn-default" Text="Silence" OnClick="lbToggleSilence_Click" />
                    <asp:LinkButton ID="lbCancel" runat="server" CssClass="btn btn-link" Text="Cancel" OnClick="lbCancel_Click" />
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
