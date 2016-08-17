<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Metrics.ascx.cs" Inherits="RockWeb.Plugins.cc_newspring.Blocks.Metrics.Metrics" %>

<div class="col-md-<%= metricWidth.Value %>">

    <asp:UpdatePanel ID="pnlContent" runat="server">
        <ContentTemplate>

            <% if ( churchMetricWarning.Visible ) { %>

                <Rock:NotificationBox ID="churchMetricWarning" runat="server" NotificationBoxType="Warning" Visible="false"
                    Text="Please select a metric source or key in the block settings." />

            <% } else { %>

                <asp:HiddenField ID="metricBlockNumber" runat="server" />
                <asp:HiddenField ID="metricWidth" runat="server" />
                <asp:HiddenField ID="metricClass" runat="server" />
                <asp:HiddenField ID="metricDisplay" runat="server" />
                <asp:HiddenField ID="metricComparisonDisplay" runat="server" />

                <asp:HiddenField ID="metricTitle" runat="server" />
                <asp:HiddenField ID="metricBlockId" runat="server" />

                <asp:HiddenField ID="currentMetricValue" runat="server" />
                <asp:HiddenField ID="previousMetricValue" runat="server" />

                <div class="panel panel-block">
                    <div class="panel-heading clearfix">
                        <h1 class="panel-title pull-left">
                            <%= metricTitle.Value %>
                        </h1>
                    </div>
                    <div class="panel-body">
                        <h1 class="text-right">
                            <%= currentMetricValue.Value %><% if ( metricComparisonDisplay.Value != "" ) { %>%
                            <% } %>
                            
                            <% if ( metricClass.Value != "" ) { %>
                                <i class="fa fa-fw <%= metricClass.Value %> pull-right"></i>
                            <% } %>
                        </h1>

                        <% if ( previousMetricValue.Value != "" ) { %>
                            <h4>Last Year</h4>
                            <h3><%= previousMetricValue.Value %></h3>
                        <% } %>
                    </div>
                </div>

            <% } %>
        </ContentTemplate>
    </asp:UpdatePanel>
</div>