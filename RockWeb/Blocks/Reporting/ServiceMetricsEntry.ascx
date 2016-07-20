<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ServiceMetricsEntry.ascx.cs" Inherits="RockWeb.Blocks.Reporting.ServiceMetricsEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="form-inline margin-b-lg">
            <div class="form-group">
                <Rock:ButtonDropDownList ID="bddlCampus" runat="server" Label="Location" OnSelectionChanged="bddl_SelectionChanged" />
            </div>
            <div class="form-group">
                <Rock:ButtonDropDownList ID="bddlWeekend" runat="server" Label="Week of" OnSelectionChanged="bddl_SelectionChanged" />
            </div>
            <div class="form-group">
                <Rock:ButtonDropDownList ID="bddlService" runat="server" Label="Service Time" OnSelectionChanged="bddl_SelectionChanged" />
            </div>
        </div>

        <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
        <Rock:NotificationBox ID="nbMetricsSaved" runat="server" Text="Metric Values Have Been Updated" NotificationBoxType="Success" Visible="false" />

        <div class="form-horizontal label-md">
            <asp:Repeater ID="rptrMetric" runat="server" OnItemDataBound="rptrMetric_ItemDataBound">
                <ItemTemplate>
                    <asp:HiddenField ID="hfMetricId" runat="server" Value='<%# Eval("Id") %>' />
                    <Rock:NumberBox ID="nbMetricValue" runat="server" NumberType="Double" Required="true"  Label='<%# Eval( "Name") %>' Text='<%# Eval( "Value") %>' />
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <div class="actions">
            <asp:LinkButton ID="btnSave" runat="server" Text="Save" AccessKey="s" CssClass="btn btn-primary" OnClick="btnSave_Click" />
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
