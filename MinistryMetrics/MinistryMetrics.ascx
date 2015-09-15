<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MinistryMetrics.ascx.cs" Inherits="RockWeb.Plugins.cc_newspring.Blocks.MinistryMetrics.MinistryMetrics" %>

<div class="col-md-<%= metricWidth.Value %>">

    <asp:UpdatePanel ID="pnlContent" runat="server">
        <ContentTemplate>
            
            <asp:HiddenField ID="metricWidth" runat="server" />
            <asp:HiddenField ID="metricTitle" runat="server" />
            <asp:HiddenField ID="currentMetricValue" runat="server" />

            <div class="panel panel-block">
                <div class="panel-heading clearfix">
                    <h1 class="panel-title pull-left">
                        <%= metricTitle.Value %>
                        
                    </h1>
                </div>
                <div class="panel-body">
                    <h1>
                        <%= currentMetricValue.Value %>
                    </h1>
                </div>
            </div>

            <Rock:NotificationBox ID="churchMetricWarning" runat="server" NotificationBoxType="Warning"
                Text="Please select a metric in the block settings." />
        </ContentTemplate>
    </asp:UpdatePanel>
</div>