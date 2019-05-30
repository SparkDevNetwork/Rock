<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupSchedulerAnalytics.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupSchedulerAnalytics" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block panel-analytics">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-line-chart"></i>
                    Group Scheduler Analytics
                </h1>

                <div class="panel-labels">
                    <a href="#" onclick="$('.js-slidingdaterange-help').toggle()">
                        <i class='fa fa-question-circle'></i>
                    </a>
                    <button id="btnCopyToClipboard" runat="server" disabled="disabled"
                        data-toggle="tooltip" data-placement="top" data-trigger="hover" data-delay="250" title="Copy Report Link to Clipboard"
                        class="btn btn-link padding-all-none btn-copy-to-clipboard"
                        onclick="$(this).attr('data-original-title', 'Copied').tooltip('show').attr('data-original-title', 'Copy Link to Clipboard');return false;">
                        <i class='fa fa-clipboard'></i>
                    </button>
                </div>
            </div>

            <div class="panel-info">
                <div class="alert alert-info js-slidingdaterange-help margin-b-none" style="display: none">
                     <asp:Literal ID="lSlidingDateRangeHelp" runat="server" />
                </div>
            </div>

            <div class="panel-body">
                <div class="row row-eq-height-md">
                    <div class="col-md-3 filter-options" role="tabpanel">
                        <Rock:NotificationBox ID="nbFilterNotification" runat="server" NotificationBoxType="Warning" visible="false"></Rock:NotificationBox>
                        <asp:HiddenField ID="hfTabs" runat="server" />
                        <div class="form-group rock-control-wrapper">
                            <label class="control-label">View By</label>
                            <div class="toggle-container">
                                <div class="btn-group" data-toggle="view-btns" role="tablist" id="tablist">
                                    <a href="#group" class="btn btn-default" aria-controls="group" role="tab" data-toggle="tab" onclick='$("#<%= hfTabs.ClientID %>").attr( "value", "group");'>Group</a>
                                    <a href="#dataview" class="btn btn-default" aria-controls="dataview" role="tab" data-toggle="tab" onclick='$("#<%= hfTabs.ClientID %>").attr( "value", "dataview");'>Dataview</a>
                                    <a href="#person" class="btn btn-default" aria-controls="person" role="tab" data-toggle="tab" onclick='$("#<%= hfTabs.ClientID %>").attr( "value", "person");'>Person</a>
                                </div>
                            </div>
                        </div>

                        <Rock:SlidingDateRangePicker ID="sdrpDateRange" runat="server" Label="Date Range" EnabledSlidingDateRangeTypes="Previous, Last, Current, DateRange" EnabledSlidingDateRangeUnits="Week, Month, Year" SlidingDateRangeMode="Current"/>

                        <div class="tab-content" style="padding-bottom:20px">
                            <div role="tabpanel" class="tab-pane fade in active" id="group">
                                <Rock:GroupPicker ID="gpGroups" runat="server" AllowMultiSelect="false" Label="Select Groups" LimitToSchedulingEnabledGroups="true" OnSelectItem="gpGroups_SelectItem" />
                                <Rock:NotificationBox ID="nbGroupWarning" runat="server" NotificationBoxType="Warning" Text="Please select a group." Visible="false"/>
                                <Rock:RockCheckBoxList ID="cblLocations" runat="server" Label="Locations" RepeatColumns="1" RepeatDirection="Vertical" OnSelectedIndexChanged="cblLocations_SelectedIndexChanged" AutoPostBack="true" Visible="false" ></Rock:RockCheckBoxList>
                                <Rock:RockCheckBoxList ID="cblSchedules" runat="server" Label="Schedules" RepeatColumns="1" RepeatDirection="Vertical" Visible="false" ></Rock:RockCheckBoxList>
                            </div>
                            <div role="tabpanel" class="tab-pane fade" id="person">
                                <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" OnSelectPerson="ppPerson_SelectPerson" />
                                <Rock:NotificationBox ID="nbPersonWarning" runat="server" NotificationBoxType="Warning" Text="Please select a person." Visible="false"/>
                            </div>
                            <div role="tabpanel" class="tab-pane fade" id="dataview">
                                <Rock:DataViewItemPicker ID="dvDataViews" runat="server" Label="Data View" OnSelectItem="dvDataViews_SelectItem" ></Rock:DataViewItemPicker>
                                <Rock:NotificationBox ID="nbDataviewWarning" runat="server" NotificationBoxType="Warning" Text="Please select a Dataview." Visible="false"/>
                            </div>
                        </div>


                    </div>

                    <div class="col-md-9 resource-area">
                        <div class="row analysis-types">

                            <div class="col-md-12">
                                <div class="actions text-right">
                                    <asp:LinkButton ID="btnUpdate" runat="server" OnClick="btnUpdate_Click" CssClass="btn btn-primary" ToolTip="Update the chart"><i class="fa fa-refresh"></i> Update</asp:LinkButton>
                                </div>
                            </div>
                        </div>

                        <Rock:NotificationBox ID="nbBarChartMessage" runat="server" NotificationBoxType="Default" CssClass="text-center padding-all-lg" Heading="Confirm Settings"
                                    Text="<p>Confirm your settings and select the Update button to display your results.</p>" Visible="true" />
                        <Rock:NotificationBox ID="nbNoData" runat="server" NotificationBoxType="Default" CssClass="text-center padding-all-lg" Text="No Data Found" Visible="false"></Rock:NotificationBox>
                        <div class="row">
                            <%-- Bar chart to show the data in the tabular --%>
                            <div class="chart-container col-md-9">
                                <canvas id="barChartCanvas" runat="server" style="height: 450px;" />
                            </div>

                            <%-- Doughnut chart to show the decline reasons--%>
                            <div class="chart-container col-md-3">
                                <canvas id="doughnutChartCanvas" runat="server"></canvas>
                            </div>

                        </div>
                        <div class="row">
                            <div class="col-md-12">
                            <%-- tabular data --%>
                            <Rock:Grid ID="gData" runat="server" AllowPaging="true" EmptyDataText="No Data Found" ShowActionsInHeader="false">
                                <Columns>
                                    <Rock:RockBoundField DataField="Name" HeaderText="Name"></Rock:RockBoundField>
                                    <Rock:RockBoundField DataField="Scheduled" HeaderText="Scheduled"></Rock:RockBoundField>
                                    <Rock:RockBoundField DataField="NoResponse" HeaderText="No Response"></Rock:RockBoundField>
                                    <Rock:RockBoundField DataField="Declines" HeaderText="Declines"></Rock:RockBoundField>
                                    <Rock:RockBoundField DataField="Attended" HeaderText="Attended"></Rock:RockBoundField>
                                    <Rock:RockBoundField DataField="CommittedNoShow" HeaderText="Committed No Show"></Rock:RockBoundField>
                                </Columns>
                            </Rock:Grid>
                                </div>
                        </div>

                    </div>
                </div>
            </div>
            <script type="text/javascript">
                $(document).ready(function () {
                    showTab();
                });

                function showTab() {

                    var tab = document.getElementById('<%= hfTabs.ClientID%>').value;
                    $('#tablist a[href="#' + tab + '"]').tab('show');

                    // register the "on" event here else it will be lost on partial postback
                    $('a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
                        // e.target is the activated tab
                        if (e.target) {
                            $(e.target).parent().find('.active').removeClass('active');
                            $(e.target).addClass('active');
                        }
                    })
                }

                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(showTab);
            </script>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>