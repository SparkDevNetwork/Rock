<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MetricValueList.ascx.cs" Inherits="RockWeb.Blocks.Reporting.MetricValueList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" CssClass="panel panel-block" runat="server">
            <asp:HiddenField ID="hfMetricId" runat="server" />
            <asp:HiddenField ID="hfEntityTypeName" runat="server" />
            <asp:HiddenField ID="hfEntityName" runat="server" />
            <asp:HiddenField ID="hfMetricCategoryId" runat="server" />
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-list"></i> Metric Values</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfMetricValues" runat="server">
                        <Rock:DateRangePicker ID="drpDates" runat="server" Label="Date Range" />
                        <Rock:RockDropDownList ID="ddlGoalMeasure" runat="server" Label="Goal/Measure" />
                        <Rock:EntityPicker ID="epEntity" runat="server" EntityTypePickerVisible="false" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gMetricValues" runat="server" AllowSorting="true" OnRowSelected="gMetricValues_Edit" OnRowDataBound="gMetricValues_RowDataBound">
                        <Columns>
                            <Rock:DateField DataField="MetricValueDateTime" HeaderText="Date" SortExpression="MetricValueDateTime" />
                            <Rock:EnumField DataField="MetricValueType" HeaderText="Type" SortExpression="MetricValueType" />
                            <Rock:RockBoundField DataField="YValue" HeaderText="Value" SortExpression="YValue" ItemStyle-HorizontalAlign="Right" />
                            <Rock:RockBoundField DataField="EntityId" HeaderText="EntityTypeName" />

                            <%-- Hide X Value for now until we implement XValue (In most cases, the Metric.Xaxis is ignored and MetricValueDateTime is used as the X-Axis --%>
                            <Rock:RockBoundField DataField="XValue" HeaderText="X Value" SortExpression="XValue" ItemStyle-HorizontalAlign="Right" Visible="false" />

                            <Rock:DeleteField OnClick="gMetricValues_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>

