<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FormAnalytics.ascx.cs" Inherits="RockWeb.Blocks.WorkFlow.FormBuilder.FormAnalytics" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdAlert" runat="server" />

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading panel-follow">

                <div class="pull-left">
                    <h1 class="panel-title"><span class="fa fa-poll-h"></span>&nbsp;<asp:Literal ID="lTitle" runat="server" /></h1>
                </div>

                <div class="rock-fullscreen-toggle js-fullscreen-trigger"></div>
            </div>

            <div class="panel-body">
                <div>
                    <ul class="nav nav-pills">
                        <li id="tabSubmissions" runat="server">
                            <asp:LinkButton ID="lnkSubmissions" runat="server" Text="Submissions" CssClass="show-pill" OnClick="lnkSubmissions_Click" pill="submissions-tab" />
                        </li>
                        <li id="tabFormBuilder" runat="server">
                            <asp:LinkButton ID="lnkFormBuilder" runat="server" Text="Form Builder" CssClass="show-pill" OnClick="lnkFormBuilder_Click" pill="formBuilder-tab" />
                        </li>
                        <li id="tabCommunications" runat="server">
                            <asp:LinkButton ID="lnkComminucations" runat="server" Text="Communications" CssClass="show-pill" OnClick="lnkComminucations_Click" pill="communications-tab" />
                        </li>
                        <li id="tabSettings" runat="server">
                            <asp:LinkButton ID="lnkSettings" runat="server" Text="Settings" CssClass="show-pill" OnClick="lnkSettings_Click" pill="settings-tab" />
                        </li>
                        <li id="tabAnalytics" runat="server" class="active">
                            <asp:LinkButton ID="lnkAnalytics" runat="server" Text="Analytics" CssClass="show-pill" OnClick="lnkAnalytics_Click" pill="analytics-tab" />
                        </li>
                    </ul>
                </div>

                <hr />

                <div>
                    <h4 class="step-title text-break">Form Analytics</h4>
                    <div class="row">
                        <div class="col-sm-7">
                            Below are the views and complete rates for the form over time. These statistics assume that the workflow entry block that hosted the form is configured to collect metrics.
                        </div>
                        <div class="col-sm-5">
                            <div class="flex-grow-1 d-flex justify-content-end">
                                <Rock:SlidingDateRangePicker ID="drpSlidingDateRange" runat="server" Label="" EnabledSlidingDateRangeTypes="Current, DateRange"
                                    EnabledSlidingDateRangeUnits="Year, Month, Day" FormGroupCssClass="input-group-sm d-flex flex-wrap justify-content-end" />

                                <asp:LinkButton ID="btnRefreshChart" runat="server" CssClass="btn btn-default btn-sm btn-square" ToolTip="Refresh Chart"
                                    OnClick="btnRefreshChart_Click"><i class="fa fa-refresh"></i></asp:LinkButton>
                            </div>
                        </div>
                    </div>
                    <hr />
                </div>

                <div class="row">
                    <div class="col-md-12">
                        <asp:Literal runat="server" ID="lKPIHtml" />
                    </div>
                </div>

                <br />

                <div class="panel-analytics">
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:NotificationBox ID="nbWorkflowIdNullMessage" runat="server" NotificationBoxType="Warning" Text="Invalid WorkflowId Specified" Visible="false" />
                            <Rock:NotificationBox ID="nbViewsAndCompletionsEmptyMessage" runat="server" NotificationBoxType="Info" Text="No Form Activity" Visible="false" />
                            <div id="dvCharts" runat="server" class="chart-container chart-banner w-100">
                                <canvas id="viewsAndCompletionsCanvas" runat="server" class="js-chart-canvas-main" style="height: 350px;" />
                            </div>
                        </div>
                    </div>
                </div>

            </div>

        </asp:Panel>

        <script type="text/javascript">
            Sys.Application.add_load(function () {
                Rock.controls.fullScreen.initialize('body');
            });
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
