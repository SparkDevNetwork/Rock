<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GivingAnalytics.ascx.cs" Inherits="RockWeb.Blocks.Finance.GivingAnalytics" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block panel-analyics">
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

                        <Rock:RockControlWrapper ID="rcwGroupBy" runat="server" Label="Group By">
                            <div class="controls">
                                <div class="js-group-by">
                                    <Rock:HiddenFieldWithClass ID="hfGroupBy" CssClass="js-hidden-selected" runat="server" />
                                    <div class="btn-group">
                                        <asp:LinkButton ID="btnGroupByWeek" runat="server" CssClass="btn btn-xs btn-default active" Text="Week" data-val="0" OnClick="btnGroupBy_Click" />
                                        <asp:LinkButton ID="btnGroupByMonth" runat="server" CssClass="btn btn-xs btn-default" Text="Month" data-val="1" OnClick="btnGroupBy_Click" />
                                        <asp:LinkButton ID="btnGroupByYear" runat="server" CssClass="btn btn-xs btn-default" Text="Year" data-val="2" OnClick="btnGroupBy_Click" />
                                    </div>
                                </div>
                            </div>
                        </Rock:RockControlWrapper>

                        <Rock:NumberRangeEditor ID="nreAmount" runat="server" NumberType="Currency" Label="Total Amount" />
                        <Rock:DataViewPicker ID="dvpDataView" runat="server" Label="Limit by DataView" AutoPostBack="true" OnSelectedIndexChanged="dvpDataView_SelectedIndexChanged" />
                        <Rock:RockCheckBoxList ID="cblCurrencyTypes" runat="server" Label="Currency Types" RepeatDirection="Vertical" />
                        <Rock:RockCheckBoxList ID="cblTransactionSource" runat="server" Label="Transaction Source" RepeatDirection="Vertical" />
                        <asp:PlaceHolder ID="phAccounts" runat="server" />

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
                                            <Rock:CurrencyField DataField="YValue" HeaderText="Amount" SortExpression="YValue" />
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

                                    <Rock:RockControlWrapper ID="rcwGiversFilter" runat="server" Label="Filter" CssClass="rock-radio-button-list">
                                        <p>
                                            <Rock:RockRadioButton ID="radAllGivers" runat="server" GroupName="grpFilterBy" Text="All Givers" CssClass="js-givers-all" />
                                        </p>
                                        <p>
                                            <Rock:RockRadioButton ID="radFirstTime" runat="server" GroupName="grpFilterBy" Text="First Time Givers" CssClass="js-givers-by-first-time" />
                                        </p>
                                        <p>
                                            <Rock:RockRadioButton ID="radByPattern" runat="server" GroupName="grpFilterBy" Text="Pattern" CssClass="js-givers-by-pattern" />
                                        </p>
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
                                    </Rock:RockControlWrapper>

                                    <Rock:RockRadioButtonList ID="rblDataViewAction" runat="server" Label="Dataview Results" RepeatDirection="Vertical" Visible="false">
                                        <asp:ListItem Text="Only show people from dataview that have giving data" Value="Limit" />
                                        <asp:ListItem Text="Include all people from dataview" Value="All" />
                                    </Rock:RockRadioButtonList>

                                    <div class="actions margin-b-md">
                                        <asp:LinkButton ID="btnApplyGiversFilter" runat="server" Visible="false" CssClass="btn btn-primary" Text="Apply" ToolTip="Update the Givers grid" OnClick="btnApplyGiversFilter_Click" />
                                    </div>

                                </div>
                            </div>

                            <Rock:NotificationBox ID="nbGiversError" runat="server" NotificationBoxType="Danger" Dismissable="true" Visible="false" />

                            <Rock:Grid ID="gGiversGifts" runat="server" AllowSorting="true" RowItemText="Attendee" ExportGridAsWYSIWYG="true">
                                <Columns>
                                    <Rock:SelectField />
                                    <asp:TemplateField HeaderText="Person" SortExpression="LastName,NickName">
                                        <ItemTemplate><%# Eval("NickName") %> <%# Eval("LastName") %></ItemTemplate>
                                    </asp:TemplateField>
                                    <Rock:DateField DataField="FirstEverGift" HeaderText="First Ever Gift" SortExpression="FirstEverGift" />
                                    <Rock:DateField DataField="FirstGift" HeaderText="First Gift" SortExpression="FirstGift" />
                                    <Rock:CurrencyField DataField="Total" HeaderText="Total Amount" SortExpression="Total" />
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

                // Set checkbox labels to toggle child checkboxes when clicked
                $('div.rock-check-box-list').find('label').prop('data-selected', false);
                $('div.rock-check-box-list').find('label').on('click', function (e) {
                    var selected = $(this).prop('data-selected')
                    $(this).siblings().find('input:checkbox').prop('checked', !selected);
                    $(this).prop('data-selected', !selected);
                });

                showFilterByOptions();
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
