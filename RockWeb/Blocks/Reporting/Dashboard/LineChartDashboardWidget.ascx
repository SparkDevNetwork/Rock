<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LineChartDashboardWidget.ascx.cs" Inherits="RockWeb.Blocks.Reporting.Dashboard.LineChartDashboardWidget" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDashboardTitle" runat="server" CssClass="dashboard-title">
            <asp:Literal runat="server" ID="lDashboardTitle" />
        </asp:Panel>
        <asp:Panel ID="pnlDashboardSubtitle" runat="server" CssClass="dashboard-subtitle">
            <asp:Literal runat="server" ID="lDashboardSubtitle" />
        </asp:Panel>
        <Rock:NotificationBox ID="nbMetricWarning" runat="server" NotificationBoxType="Warning" Text="Please select a metric in the block settings." />
        <Rock:RockLineChart ID="metricChart" runat="server" />

        <%-- Edit Panel --%>
        <asp:Panel ID="pnlEditModel" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdEdit" runat="server" OnSaveClick="mdEdit_SaveClick" Title="Chart Dashboard Widget">
                <Content>

                    <asp:UpdatePanel runat="server" ID="upnlEdit">
                        <ContentTemplate>
                            <Rock:MetricCategoryPicker ID="mpMetricCategoryPicker"  runat="server" Label="Metric" OnSelectItem="mpMetricCategoryPicker_SelectItem" />
                            <Rock:RockRadioButtonList ID="rblSelectOrContext" runat="server" RepeatDirection="Horizontal" AutoPostBack="true" OnSelectedIndexChanged="rblSelectOrContext_SelectedIndexChanged" >
                                <asp:ListItem Text="Select Partition(s)" Value="0" />
                                <asp:ListItem Text="Get from page context" Value="1" />
                            </Rock:RockRadioButtonList>
                            <asp:PlaceHolder ID="phMetricValuePartitions" runat="server" />

                            <Rock:RockCheckBox ID="cbCombineValues" runat="server" Text="Combine multiple values to one line when showing values for multiple partition(s)" />

                            <Rock:SlidingDateRangePicker ID="drpSlidingDateRange" runat="server" />

                        </ContentTemplate>
                    </asp:UpdatePanel>

                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
