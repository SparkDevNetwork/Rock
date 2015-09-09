<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MinistryMetrics.ascx.cs" Inherits="RockWeb.Plugins.cc_newspring.Blocks.MinistryMetrics.MinistryMetrics" %>

<div class="col-md-<%= metricWidth.Value %>">

    <asp:UpdatePanel ID="pnlContent" runat="server">
        <ContentTemplate>
            
            <asp:HiddenField ID="metricWidth" runat="server" />

            <%--
                
            <asp:HiddenField ID="metricBlockNumber" runat="server" />
            
            <asp:HiddenField ID="metricClass" runat="server" />
            <asp:HiddenField ID="metricDisplay" runat="server" />
            
            <asp:HiddenField ID="metricTitle" runat="server" />
            <asp:HiddenField ID="metricBlockId" runat="server" />

            <asp:HiddenField ID="currentMetricValue" runat="server" />
            <asp:HiddenField ID="previousMetricValue" runat="server" />

            <% if ( metricDisplay.Value.Equals( "Text" ) ) { %>

                <div class="panel panel-block">
                    <div class="panel-heading clearfix">
                        <h1 class="panel-title pull-left"><%= metricTitle.Value %></h1>
                    </div>
                    <div class="panel-body">
                        <h1 <% if ( previousMetricValue.Value == "" || previousMetricValue.Value == "0" ) { %> class="flush" <% } %>><%= currentMetricValue.Value %><% if ( metricClass.Value != "" )
                                                                      { %> <i class="fa fa-fw <%= metricClass.Value %> pull-right"></i><% } %></h1>

                        <% if ( previousMetricValue.Value != "" && previousMetricValue.Value != "0" ) { %>
                        <h4>Last Year</h4>
                        <h3><%= previousMetricValue.Value %></h3>
                        <% } %>
                    </div>
                </div>

            <% } %>
                
            --%>



            <div class="panel panel-block">
                <div class="panel-heading clearfix">
                    <h1 class="panel-title pull-left">
                        <asp:Literal ID="metricTitle" runat="server" />
                    </h1>
                </div>
                <div class="panel-body">
                    <h1>
                        <asp:Literal ID="currentMetricValue" runat="server" />
                    </h1>
                </div>
            </div>

            <Rock:NotificationBox ID="churchMetricWarning" runat="server" NotificationBoxType="Warning"
                Text="Please select a metric in the block settings." />
        </ContentTemplate>
    </asp:UpdatePanel>
</div>