<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReminderList.ascx.cs" Inherits="RockWeb.Blocks.Reminders.ReminderList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <script type="text/javascript">
            var currentReminderStartDate = '';
            var currentReminderEndDate = '';

            $(document).ready(function () {
                currentReminderStartDate = $('#<%= drpCustomDate.ClientID %>').find('.js-lower').children('input').val();
                currentReminderEndDate = $('#<%= drpCustomDate.ClientID %>').find('.js-upper').children('input').val();

                $(".js-slidingdaterange-text-value").change(function () { handleDateRangeChange(); });
            });

            function handleDateRangeChange() {
                if ($("div.datepicker")[0]) {
                    // If the date picker is still open, don't do anything.
                    return false;
                }

                var inputStartDate = $('#<%= drpCustomDate.ClientID %>').find('.js-lower').children('input').val();
                var inputEndDate = $('#<%= drpCustomDate.ClientID %>').find('.js-upper').children('input').val();

                // if date values changed, trigger the postback.
                if (currentReminderStartDate != inputStartDate || currentReminderEndDate != inputEndDate) {
                    var validationGroup = eval('<%= this.ClientID %>_drpCustomDate_nbNumber_drpCustomDate_customValidator.validationGroup');
                    var postBackElement = '<%= this.ClientID %>'.replace(/\_/g, "$") + '$btnDueFilter_Custom';
                    WebForm_DoPostBackWithOptions(new WebForm_PostBackOptions(postBackElement, '', true, validationGroup, '', false, true));
                }
            }
        </script>

        <asp:HiddenField ID="hfReminderTypesInclude" runat="server" />
        <asp:HiddenField ID="hfReminderTypesExclude" runat="server" />

        <div class="panel panel-block styled-scroll panel-groupscheduler">
            <%-- Panel Header --%>
            <div class="panel-heading panel-follow">
                <h1 class="panel-title">
                    <i class="fa fa-bell"></i>
                    Reminders
                </h1>
            </div>

            <asp:Panel ID="pnlNotAuthenticated" runat="server" CssClass="panel-body" Visible="false">
                <Rock:NotificationBox ID="nbNotAuthenticated" runat="server" NotificationBoxType="Warning" Text="Please log in to use Reminders." />
            </asp:Panel>

            <asp:Panel ID="pnlNoReminders" runat="server" CssClass="panel-body" Visible="false">
                <Rock:NotificationBox ID="nbNoReminders" runat="server" NotificationBoxType="Warning" Text="You do not have any reminders." />
            </asp:Panel>

            <asp:Panel ID="pnlView" runat="server" CssClass="panel-body">
                <script type="text/javascript">
                    if (typeof refreshReminderCount === "function") {
                        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(refreshReminderCount);
                    }

                    $(document).ready(function () {
                        var activeFilterSetting = $('#<%=hfDueFilterSetting.ClientID %>').val();
                        if (activeFilterSetting == 'Custom Date Range') {
                            $('#reminders-custom-date-range').removeClass('d-none');
                        }

                        $(".js-reminder-edit-trigger").click(function () {
                            var parentElement = $(this).parent();
                            var editButton = $(parentElement).find(".js-reminder-edit-button");
                            if (editButton.length > 0) {
                                editButton[0].click();
                            }
                        });
                    });
                </script>

                <div class="panel-body">
                    <%-- Filter Options (Header) --%>
                    <div class="panel-collapsable p-0">
                        <div class="row row-eq-height no-gutters align-items-end">
                            <div class="col flex-grow-0">
                                <%-- Entity Type - Filter Options (Header) --%>
                                <asp:Panel ID="pnlEntityType" runat="server" CssClass="panel-toolbar-border">
                                    <div class="btn-group">
                                        <asp:Panel ID="pnlEntityTypeSelection" runat="server" CssClass="dropdown-toggle btn btn-default p-1 btn-lg mb-1" data-toggle="dropdown">
                                            <asp:Literal ID="lSelectedEntityType" runat="server" Text="All Reminders" />
                                            <i class="fa fa-chevron-down"></i>
                                        </asp:Panel>

                                        <ul class="dropdown-menu" role="menu">
                                            <asp:Repeater ID="rptEntityTypeList" runat="server" OnItemDataBound="rptEntityTypeList_ItemDataBound">
                                                <ItemTemplate>
                                                    <li>
                                                        <asp:LinkButton ID="btnEntityType" runat="server" Text="-" CommandArgument="-" OnClick="btnEntityType_Click" />
                                                    </li>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                        </ul>
                                    </div>
                                </asp:Panel>
                            </div>

                            <asp:Panel ID="pnlFilterOptions" runat="server" CssClass="col">
                                <%-- Group/Person - Filter Options (Header) --%>

                                <div class="panel-toolbar panel-toolbar-right pr-0">
                                    <div>
                                        <!-- Filter for Groups/ChildGroups -->
                                        <Rock:GroupPicker ID="gpSelectedGroup" runat="server" Label="" Placeholder="All Groups" CssClass="occurrences-groups-picker" Visible="false" OnValueChanged="gpSelectedGroup_ValueChanged" />
                                        <!-- Filter for Person -->
                                        <Rock:PersonPicker ID="ppSelectedPerson" runat="server" Label="" Placeholder="All People" CssClass="occurrences-groups-picker" Visible="false" OnValueChanged="ppSelectedPerson_ValueChanged" />
                                    </div>

                                    <div class="btn-group">
                                        <div class="dropdown-toggle btn btn-xs btn-tool" data-toggle="dropdown">
                                            <i class="fa fa-check-circle-o"></i>
                                            <asp:Literal ID="lCompletionFilter" runat="server" Text="Incomplete" />
                                        </div>

                                        <ul class="dropdown-menu" role="menu">
                                            <li>
                                                <asp:LinkButton ID="btnCompletionFilter_Active" runat="server" Text="Active" CommandArgument="Active" OnClick="btnCompletionFilter_Click" />
                                            </li>
                                            <li>
                                                <asp:LinkButton ID="btnCompletionFilter_Complete" runat="server" Text="Complete" CommandArgument="Complete" OnClick="btnCompletionFilter_Click" />
                                            </li>
                                            <li>
                                                <asp:LinkButton ID="btnCompletionFilter_All" runat="server" Text="All" CommandArgument="All" OnClick="btnCompletionFilter_Click" />
                                            </li>
                                        </ul>
                                    </div>

                                    <div class="btn-group">
                                        <div class="dropdown-toggle btn btn-xs btn-tool" data-toggle="dropdown">
                                            <i class="fa fa-calendar-alt"></i>
                                            <asp:Literal ID="lDueFilter" runat="server" Text="Due" />
                                        </div>

                                        <ul class="dropdown-menu" role="menu">
                                            <li>
                                                <asp:LinkButton ID="btnDueFilter_Due" runat="server" Text="Due" CommandArgument="Due" OnClick="btnDueFilter_Click" />
                                            </li>
                                            <li>
                                                <asp:LinkButton ID="btnDueFilter_ThisWeek" runat="server" Text="Due This Week" CommandArgument="Due This Week" OnClick="btnDueFilter_Click" />
                                            </li>
                                            <li>
                                                <asp:LinkButton ID="btnDueFilter_ThisMonth" runat="server" Text="Due This Month" CommandArgument="Due This Month" OnClick="btnDueFilter_Click" />
                                            </li>
                                            <li>
                                                <asp:LinkButton ID="btnDueFilter_Custom" runat="server" Text="Custom Date Range" CommandArgument="Custom Date Range" OnClick="btnDueFilter_Click" />
                                            </li>
                                        </ul>
                                    </div>

                                    <div id="reminders-custom-date-range" class="d-none">
                                        <asp:HiddenField ID="hfDueFilterSetting" runat="server" Value="Active" />
                                        <Rock:SlidingDateRangePicker ID="drpCustomDate" runat="server"
                                            OnSelectedDateRangeChanged="drpCustomDate_SelectedDateRangeChanged"
                                            EnabledSlidingDateRangeTypes="Previous, Last, Current, DateRange"
                                            EnabledSlidingDateRangeUnits="Week, Month, Year, Day, Hour"
                                            SlidingDateRangeMode="Current"
                                            TimeUnit="Year"
                                            FormGroupCssClass="input-group-sm"
                                            Label="" />
                                    </div>

                                    <div class="btn-group">
                                        <div class="dropdown-toggle btn btn-xs btn-tool" data-toggle="dropdown">
                                            <i class="fa fa-list"></i>
                                            <asp:Literal ID="lReminderType" runat="server" Text="All Reminder Types" />
                                        </div>

                                        <ul class="dropdown-menu" role="menu">
                                            <li>
                                                <asp:LinkButton ID="btnReminderTypeFilter_All" runat="server" Text="All" CommandArgument="All" OnClick="btnReminderTypeFilter_Click" />
                                                <asp:Repeater ID="rptReminderType" runat="server" OnItemDataBound="rptReminderType_ItemDataBound">
                                                    <ItemTemplate>
                                                        <li>
                                                            <asp:LinkButton ID="btnReminderTypeFilter_EntityType" runat="server" Text="-" CommandArgument="-" OnClick="btnReminderTypeFilter_Click" />
                                                        </li>
                                                    </ItemTemplate>
                                                </asp:Repeater>
                                            </li>
                                        </ul>
                                    </div>
                                </div>
                            </asp:Panel>
                        </div>
                    </div>

                    <Rock:NotificationBox ID="nbFilteredReminders" runat="server" NotificationBoxType="Warning" Visible="false" />

                    <asp:Repeater ID="rptReminders" runat="server" OnItemDataBound="rptReminders_ItemDataBound">
                        <ItemTemplate>
                            <div class="d-flex flex-sm-wrap py-3 px-sm-4 py-sm-4 border-bottom border-panel cursor-pointer" data-reminder-id="<%# Eval("Id") %>">
                                <div class="flex-grow-0">
                                    <asp:LinkButton ID="btnComplete" runat="server" class="success-on-hover" CommandArgument='<%# Eval("Id") %>' OnClick="btnComplete_Click">
                                        <asp:Literal ID="lCheckIcon_Complete" runat="server" Visible='<%# Eval("IsComplete") %>'><i class="fa fa-lg fa-check-circle text-success"></i></asp:Literal>
                                        <asp:Literal ID="lCheckIcon_Incomplete" runat="server" Visible='<%# (bool) Eval("IsComplete") == true ? false : true %>'><i class="fa fa-lg fa-check-circle-o"></i></asp:Literal>
                                    </asp:LinkButton>
                                </div>
                                <div class="d-flex flex-wrap flex-eq">
                                    <div class="d-inline-flex align-items-center align-items-sm-start col-xs-6 col-sm flex-grow-0 text-nowrap js-reminder-edit-trigger">
                                        <span class="label label-default"><asp:Literal ID="lReminderDate" runat="server" Text='<%# Eval("ReminderDate", "{0: M/d/yyyy}") %>' /></span>
                                    </div>
                                    <div class="col-xs-12 col-sm order-3 order-sm-0 mt-2 mt-sm-0 js-reminder-edit-trigger">
                                        <div class="note reminder-note">
                                            <div class="meta">
                                                <asp:Literal ID="lProfilePhoto" runat="server" Visible="false"><div class="meta-figure">
                                                    <div class="avatar avatar-lg"><img src="{0}&w=50"></div>
                                                </div></asp:Literal>
                                                <asp:Literal ID="lGroupIcon" runat="server" Visible="false"><div class="meta-figure">
                                                    <div class="avatar avatar-icon avatar-lg">
                                                        <i class="{0}"></i>
                                                    </div>
                                                </div></asp:Literal>
                                                <div class="meta-body">
                                                    <span class="note-caption">
                                                        <asp:Literal ID="lEntity" runat="server" />
                                                    </span>
                                                    <span class="note-details">
                                                        <span class="tag-flair">
                                                            <asp:Literal ID="lIcon" runat="server" Text='<%# "<span class=\"tag-color\" style=\"background-color: " + Eval("HighlightColor") + "\"></span>" %>' />
                                                            <asp:Literal ID="lReminderType" runat="server"  Text='<%# "<span class=\"tag-label\">" + Eval("ReminderTypeName") + "</span>" %>' />
                                                        </span>
                                                    </span>
                                                </div>
                                                <asp:Literal ID="lClock" runat="server" Visible='<%# Eval("IsRenewing") %>'><div class="dropdown text-info"><i class="fa fa-clock-o"></i></div></asp:Literal>
                                            </div>

                                            <div class="note-content">
                                                <asp:Literal ID="lNote" runat="server"  Text='<%# Eval("Note") %>' />
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-xs-6 col-sm flex-grow-0 text-nowrap text-right">
                                        <%-- This button is used to initiate a postback for editing, but should not be displayed to the user. --%>
                                        <asp:LinkButton ID="btnEdit" runat="server" CssClass="btn btn-link btn-square btn-sm btn-overflow d-none js-reminder-edit-button" CommandArgument='<%# Eval("Id") %>' OnClick="btnEdit_Click" />
                                        <asp:LinkButton ID="btnDelete" runat="server" CssClass="btn btn-link btn-square btn-sm btn-overflow" CommandArgument='<%# Eval("Id") %>' OnClick="btnDelete_Click">
                                            <i class="fa fa-close"></i>
                                        </asp:LinkButton>
                                    </div>
                                </div>

                            </div>
                        </ItemTemplate>
                    </asp:Repeater>

                </div>
        
            </asp:Panel>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
