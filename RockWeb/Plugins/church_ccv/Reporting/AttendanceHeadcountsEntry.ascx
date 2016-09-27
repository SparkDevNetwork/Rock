<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttendanceHeadcountsEntry.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Reporting.AttendanceHeadcountsEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-pencil-square-o"></i>&nbsp;Adult Worship Headcounts</h1>
            </div>

            <div class="panel-body">
                <div class="row">
                    <div class="col-md-4">
                        <Rock:CampusPicker ID="bddlCampus" runat="server" OnSelectedIndexChanged="bddlCampus_SelectionChanged" AutoPostBack="true" />
                        <Rock:RockDropDownList ID="bddlWeekend" runat="server" OnSelectedIndexChanged="bddlWeekend_SelectedIndexChanged" AutoPostBack="true" Label="Weekend" />
                    </div>
                </div>

                <asp:Repeater ID="rptrMetric" runat="server" OnItemDataBound="rptrMetric_ItemDataBound">
                    <ItemTemplate>
                        <h3><%# Eval( "ServiceName") %></h3>
                        <div class="row">
                            <asp:HiddenField ID="hfScheduleId" runat="server" Value='<%# Eval("ScheduleId") %>' />
                            <div class="col-md-2">
                                <Rock:NumberBox ID="nbMetricMainValue" runat="server" NumberType="Double" Label='Worship' Text='<%# Eval( "MainValue") %>' />
                            </div>
                            <div class="col-md-2">
                                <Rock:NumberBox ID="nbMetricOverflowValue" runat="server" NumberType="Double" Label='Overflow' Text='<%# Eval( "OverflowValue") %>' />
                            </div>
                            <div class="col-md-8">
                                <Rock:RockTextBox ID="tbNote" runat="server" Label="Notes" Text='<%# Eval("Note") %>' />
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>

                <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <Rock:NotificationBox ID="nbMetricsSaved" runat="server" Text="Metric Values Have Been Updated" NotificationBoxType="Success" Visible="false" />

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" AccessKey="s" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                </div>

            </div>

        </div>

        <%-- Configuration Panel --%>
        <asp:Panel ID="pnlConfigure" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdConfigure" runat="server" ValidationGroup="vgConfigure" OnSaveClick="mdConfigure_SaveClick">
                <Content>
                    <Rock:SchedulePicker ID="spSchedules" runat="server" Label="Schedules" AllowMultiSelect="true" />
                    <Rock:MetricCategoryPicker ID="mpHeadcountsMetric" runat="server" Label="Metric for Headcounts" />
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
