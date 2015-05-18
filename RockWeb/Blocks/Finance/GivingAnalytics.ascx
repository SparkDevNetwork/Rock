<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GivingAnalytics.ascx.cs" Inherits="RockWeb.Blocks.Finance.GivingAnalytics" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-check-square-o"></i>Giving Analysis</h1>

                <div class="panel-labels">
                    <a href="#" onclick="$('.js-slidingdaterange-help').toggle()">
                        <i class='fa fa-question-circle'></i>
                        Date Range Help
                    </a>
                </div>

            </div>

            <div class="panel-info">
                <div class="alert alert-info js-slidingdaterange-help" style="display: none">
                    <asp:Literal ID="lSlidingDateRangeHelp" runat="server" />
                </div>
            </div>

            <div class="panel-body">
                <div class="row row-eq-height-md">
                    <div class="col-md-3 filter-options">

                        <Rock:SlidingDateRangePicker ID="drpSlidingDateRange" runat="server" Label="Date Range" />
                        <Rock:NumberRangeEditor ID="nreAmount" runat="server" NumberType="Currency" Label="Total Amount" />
                        <Rock:RockCheckBoxList ID="cblCurrencyTypes" runat="server" Label="Currency Types" RepeatDirection="Vertical" />
                        <Rock:RockCheckBoxList ID="cblTransactionSource" runat="server" Label="Transaction Source" RepeatDirection="Vertical" />
                        <Rock:CampusesPicker ID="cpCampuses" runat="server" Label="Account Campus" RepeatDirection="Vertical" />
                        <Rock:DataViewPicker ID="dvpDataView" runat="server" Label="Limit by DataView" />
                        <Rock:RockRadioButtonList ID="rblDataViewAction" runat="server" Label="Dataview Results" RepeatDirection="Vertical">
                            <asp:ListItem Text="Include All" Value="All" />
                            <asp:ListItem Text="Limit to results with matching transactions" Value="Limit" />
                        </Rock:RockRadioButtonList>

                    </div>
                    <div class="col-md-9">

                        <div class="row analysis-types">
                            <div class="col-sm-8">
                                <div class="controls">
                                    <div class="js-show-by">
                                        <Rock:HiddenFieldWithClass ID="hfShowBy" CssClass="js-hidden-selected" runat="server" />
                                        <div class="btn-group">
                                            <asp:LinkButton ID="btnShowChart" runat="server" CssClass="btn btn-default active" data-val="0" OnClick="btnShowChart_Click">
                                                    <i class="fa fa-line-chart"></i> Chart
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="btnShowDetails" runat="server" CssClass="btn btn-default" data-val="1" OnClick="btnShowDetails_Click">
                                                    <i class="fa fa-users"></i> Details
                                            </asp:LinkButton>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="col-sm-4">
                                <div class="actions text-right">
                                    <asp:LinkButton ID="btnApply" runat="server" CssClass="btn btn-primary" ToolTip="Update the chart" OnClick="btnApply_Click"><i class="fa fa-refresh"></i> Update</asp:LinkButton>
                                </div>
                            </div>
                        </div>

                        <asp:Panel ID="pnlChart" runat="server">
                            <div class="clearfix">
                                <div class="pull-right">
                                
                                        <Rock:RockControlWrapper ID="rcwGraphBy" runat="server" Label="Graph By">
                                            <div class="controls">
                                                <div class="js-graph-by">
                                                    <Rock:HiddenFieldWithClass ID="hfGraphBy" CssClass="js-hidden-selected" runat="server" />
                                                    <div class="btn-group">
                                                        <asp:LinkButton ID="btnGraphByTotal" runat="server" CssClass="btn btn-xs btn-default active" Text="Total" data-val="0" OnClick="btnGraphBy_Click" />
                                                        <asp:LinkButton ID="btnGraphByAccount" runat="server" CssClass="btn btn-xs btn-default" Text="Account" data-val="1" OnClick="btnGraphBy_Click" />
                                                        <asp:LinkButton ID="btnGraphByCampus" runat="server" CssClass="btn btn-xs btn-default" Text="Campus" data-val="2" OnClick="btnGraphBy_Click" />
                                                    </div>
                                                </div>
                                            </div>
                                        </Rock:RockControlWrapper>
                                
                                </div>
                            </div>
                            <Rock:LineChart ID="lcAmount" runat="server" DataSourceUrl="" Title="" Subtitle="" ChartHeight="300" />
                            <div class="row margin-t-sm">
                                <div class="col-md-12">
                                    <div class="pull-right">
                                        <asp:LinkButton ID="lShowChartAmountGrid" runat="server" CssClass="btn btn-default btn-xs margin-b-sm" Text="Show Data <i class='fa fa-chevron-down'></i>" ToolTip="Show Data" OnClick="lShowChartAmountGrid_Click" />
                                    </div>
                                </div>
                            </div>
                            <asp:Panel ID="pnlChartAmountGrid" runat="server" Visible="false">

                                <div class="grid">
                                    <Rock:Grid ID="gChartAmount" runat="server" AllowSorting="true" DataKeyNames="DateTimeStamp,SeriesId" RowItemText="Amount Summary">
                                        <Columns>
                                            <Rock:DateField DataField="DateTime" HeaderText="Date" SortExpression="DateTimeStamp" />
                                            <Rock:RockBoundField DataField="SeriesId" HeaderText="Series" SortExpression="SeriesId" />
                                            <Rock:RockBoundField DataField="YValue" HeaderText="Amount" SortExpression="YValue" />
                                        </Columns>
                                    </Rock:Grid>
                                </div>
                            </asp:Panel>
                        </asp:Panel>

                        <asp:Panel ID="pnlDetails" runat="server">
                            <div class="panel">
                                <div class="grid-filter">
                                    <div class="controls pull-right margin-t-sm">
                                        <div class="js-view-by">
                                            <Rock:HiddenFieldWithClass ID="hfViewBy" CssClass="js-hidden-selected" runat="server" />
                                            <div class="btn-group">
                                                <asp:HyperLink ID="btnViewGivers" runat="server" CssClass="btn btn-default btn-sm active" data-val="0">
                                                    Giver
                                                </asp:HyperLink>
                                                <asp:HyperLink ID="btnViewAdults" runat="server" CssClass="btn btn-default btn-sm" data-val="1">
                                                    Adults
                                                </asp:HyperLink>
                                                <asp:HyperLink ID="btnViewChildren" runat="server" CssClass="btn btn-default btn-sm" data-val="2">
                                                    Children
                                                </asp:HyperLink>
                                                <asp:HyperLink ID="btnViewFamily" runat="server" CssClass="btn btn-default btn-sm" data-val="3">
                                                    Family
                                                </asp:HyperLink>
                                            </div>
                                        </div>
                                    </div>
                                    <Rock:RockControlWrapper ID="rcwGiversFilter" runat="server" Label="Filter">
                                        <p>
                                            <Rock:RockRadioButton ID="radAllGivers" runat="server" GroupName="grpFilterBy" Text="All Givers" CssClass="js-givers-all" />
                                        </p>
                                        <p>
                                            <Rock:RockRadioButton ID="radFirstTime" runat="server" GroupName="grpFilterBy" Text="First Time Givers" CssClass="js-givers-by-first-time" />
                                        </p>
                                        <p>
                                            <Rock:RockRadioButton ID="radByPattern" runat="server" GroupName="grpFilterBy" Text="Pattern" CssClass="js-givers-by-pattern" />

                                            <asp:Panel ID="pnlByPatternOptions" runat="server" CssClass="js-givers-by-pattern-options padding-l-lg">
                                                <div class="form-inline">
                                                    <span>Gave at least </span>
                                                    <Rock:NumberBox ID="tbPatternXTimes" runat="server" CssClass="input-width-xs" /><span> times for the selected date range </span>
                                                </div>
                                                <div class="padding-l-lg">
                                                    <div class="form-inline">
                                                        <Rock:RockCheckBox ID="cbPatternAndMissed" runat="server" />and did not give between
                                                        <Rock:NotificationBox ID="nbMissedDateRangeRequired" runat="server" NotificationBoxType="Warning" Text="Date Range is required" Visible="false" />
                                                        <Rock:DateRangePicker ID="drpPatternDateRange" runat="server" />
                                                    </div>
                                                </div>
                                            </asp:Panel>
                                        </p>
                                    </Rock:RockControlWrapper>

                                    <div class="actions margin-b-md">
                                        <asp:LinkButton ID="btnApplyGiversFilter" runat="server" Visible="false" CssClass="btn btn-primary" Text="Apply" ToolTip="Update the Givers grid" OnClick="btnApplyGiversFilter_Click" />
                                    </div>

                                </div>
                            </div>

                            <Rock:NotificationBox ID="nbGiversError" runat="server" NotificationBoxType="Danger" Dismissable="true" Visible="false" />
                            <Rock:Grid ID="gGiversGifts" runat="server" AllowSorting="true" RowItemText="Attendee" OnRowDataBound="gGiversGifts_RowDataBound" ExportGridAsWYSIWYG="true">
                                <Columns>
                                    <Rock:SelectField />
                                    <Rock:PersonField DataField="Person" HeaderText="Name" SortExpression="Person.LastName, Person.NickName" />
                                    <Rock:CurrencyField DataField="Amount" HeaderText="Total Amount" SortExpression="Total" />
                                    <Rock:BoolField DataField="FirstTimeGiver" HeaderText="First Time Giver" SortExpression="FirstTimeGiver" />
                                    <Rock:DateField DataField="FirstTxnDate" HeaderText="First Transaction" SortExpression="FirstTxnDate" />
                                    <Rock:DateField DataField="FirstTxnInRange" HeaderText="First Transaction in Date Range" SortExpression="FirstTxnInRange" />
                                </Columns>
                            </Rock:Grid>

                        </asp:Panel>
                    </div>
                </div>
            </div>
        </div>

        <script>
            function setActiveButtonGroupButton($activeBtn) {
                $activeBtn.addClass('active');
                $activeBtn.siblings('.btn').removeClass('active');
                $activeBtn.closest('.btn-group').siblings('.js-hidden-selected').val($activeBtn.data('val'));
            }

            function showFilterByOptions() {
                if ($('.js-givers-all').is(':checked')) {
                    $('.js-givers-by-pattern-options').hide();
                } else if ($('.js-givers-by-first-time').is(':checked')) {
                    $('.js-givers-by-pattern-options').hide();
                } else if ($('.js-givers-by-pattern').is(':checked')) {
                    $('.js-givers-by-pattern-options').show();
                }
            }

            Sys.Application.add_load(function () {
                // Graph-By button group
                $('.js-graph-by .btn').on('click', function (e) {
                    setActiveButtonGroupButton($(this));
                });

                setActiveButtonGroupButton($('.js-graph-by').find("[data-val='" + $('.js-graph-by .js-hidden-selected').val() + "']"));

                // Group-By button group
                $('.js-group-by .btn').on('click', function (e) {
                    setActiveButtonGroupButton($(this));
                });

                setActiveButtonGroupButton($('.js-group-by').find("[data-val='" + $('.js-group-by .js-hidden-selected').val() + "']"));

                // Show-By button group
                $('.js-show-by .btn').on('click', function (e) {
                    setActiveButtonGroupButton($(this));
                });

                setActiveButtonGroupButton($('.js-show-by').find("[data-val='" + $('.js-show-by .js-hidden-selected').val() + "']"));

                // View-By button group
                $('.js-view-by .btn').on('click', function (e) {
                    setActiveButtonGroupButton($(this));
                });

                setActiveButtonGroupButton($('.js-view-by').find("[data-val='" + $('.js-view-by .js-hidden-selected').val() + "']"));

                // Attendees Filter
                $('.js-givers-all, .js-givers-by-first-time, .js-givers-by-pattern').on('click', function (e) {
                    showFilterByOptions();
                });

                showFilterByOptions();
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
