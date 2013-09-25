<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MetricList.ascx.cs" Inherits="RockWeb.Blocks.Administration.MetricList" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <asp:ValidationSummary ID="valSummary" runat="server" CssClass="alert alert-error" />

        <asp:Panel ID="pnlMetricList" runat="server">

            <Rock:GridFilter ID="rFilter" runat="server">
                <Rock:RockDropDownList ID="ddlCategoryFilter" runat="server" Label="Category" />
            </Rock:GridFilter>
            <Rock:Grid ID="gMetrics" runat="server" AllowSorting="true" EmptyDataText="No Metrics Found" RowItemText="Metric" OnRowSelected="gMetrics_Edit">
                <Columns>
                    <asp:BoundField DataField="Id" HeaderText="ID" SortExpression="Id" />
                    <asp:BoundField DataField="Category" HeaderText="Category" SortExpression="Category" />
                    <asp:BoundField DataField="Title" HeaderText="Title" SortExpression="Title" />
                    <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                    <asp:BoundField DataField="MinValue" HeaderText="Minimum Value" SortExpression="MinValue" />
                    <asp:BoundField DataField="MaxValue" HeaderText="Maximum Value" SortExpression="MaxValue" />
                    <asp:BoundField DataField="CollectionFrequencyValue.Name" HeaderText="Collection Frequency" SortExpression="CollectionFrequencyValue.Name" />
                    <asp:BoundField DataField="Source" HeaderText="Source" SortExpression="Source" />
                    <Rock:DeleteField OnClick="gMetrics_Delete" />
                </Columns>
            </Rock:Grid>

        </asp:Panel>        

    </ContentTemplate>
</asp:UpdatePanel>
