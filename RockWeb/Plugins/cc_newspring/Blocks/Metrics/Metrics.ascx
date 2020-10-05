<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Metrics.ascx.cs" Inherits="RockWeb.Plugins.cc_newspring.Blocks.Metrics.Metrics" %>

<div class="col-md-<%= metricWidth.Value %>">

    <asp:UpdatePanel ID="pnlContent" runat="server">
        <ContentTemplate>

            <asp:HiddenField ID="metricTitle" runat="server" />
            <asp:HiddenField ID="metricBlockId" runat="server" />
            <asp:HiddenField ID="metricBlockNumber" runat="server" />
            <asp:HiddenField ID="metricWidth" runat="server" />
            <asp:HiddenField ID="metricClass" runat="server" />
            <asp:HiddenField ID="metricValue" runat="server" />

            <div class="panel panel-block">
                <div class="panel-heading clearfix">
                    <h1 class="panel-title pull-left">
                        <%= metricTitle.Value %>
                    </h1>
                </div>

                <asp:Panel id="pnlMetricDisplay" runat="server" class="panel-body">
                    <h1 class="text-right">
                        <%= metricValue.Value %>

                        <% if ( metricClass.Value != "" )
                            { %>
                        <i class="fa fa-fw <%= metricClass.Value %> pull-right"></i>
                        <% } %>
                    </h1>
                </asp:Panel>
            </div>

            <Rock:NotificationBox ID="nbMetricWarning" runat="server" NotificationBoxType="Warning" Visible="false"
                Text="Please select a metric source or key in the block settings." />

        </ContentTemplate>
    </asp:UpdatePanel>
</div>