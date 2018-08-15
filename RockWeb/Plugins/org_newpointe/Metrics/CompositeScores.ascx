<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CompositeScores.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.Metrics.CompositeScores" %>

<div class="panel panel-block" style="background: #f9f9f9">
    <div class="panel-heading" style="display: block !important;">
        <h4 class="panel-title"><i class="fa fa-line-chart"></i>Campus Composite Scores - <%= SelectedCampus %> </h4>
    </div>
    <div class="panel-body">
        <div class="row center-block">
            <div class="col-md-12 center-block">
                <h4 style="text-align: center;"><strong><%= SelectedCampus %> Index:
                <td><%= String.Format("{0:0.00}", CompositeScore) %>%</td>
                </strong></h4>
            </div>
        </div>
        <div class="container">
            <Rock:Grid ID="gMetricsChart" runat="server">
                <Columns>
                    <asp:BoundField HeaderText="Metric" DataField="Name" />
                    <asp:BoundField HeaderText="YTD" DataField="YearToDateValue" />
                    <asp:BoundField HeaderText="LYTD" DataField="LastYearToDateValue" />
                    <asp:BoundField HeaderText="Growth" DataField="YearToDateGrowth" />
                    <asp:BoundField HeaderText="Goal" DataField="Goal" />
                    <asp:BoundField HeaderText="Progress" DataField="GoalProgress" />
                </Columns>
            </Rock:Grid>
        </div>
    </div>
    <div class="panel-footer" style="text-align: right;">
        Choose a different campus:
        <Rock:ButtonDropDownList ID="cpCampus" runat="server" OnSelectionChanged="cpCampus_OnSelectionChanged" ToolTip="Choose a Campus" />
    </div>
</div>
