<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReminderList.ascx.cs" Inherits="RockWeb.Blocks.Reminders.ReminderList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="panel panel-block styled-scroll panel-groupscheduler">
            <%-- Panel Header --%>
            <div class="panel-heading panel-follow">
                <h1 class="panel-title">
                    <i class="fa fa-bell"></i>
                    Reminders
                </h1>
            </div>

        <asp:Panel ID="pnlNotAuthenticated" runat="server" CssClass="panel-body" Visible="false">
            <div class="panel-body">
                <Rock:NotificationBox ID="nbNotAuthenticated" runat="server" NotificationBoxType="Warning" Text="Please log in to use Reminders." />
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlNoReminders" runat="server" CssClass="panel-body" Visible="false">
            <div class="panel-body">
                <Rock:NotificationBox ID="nbNoReminders" runat="server" NotificationBoxType="Warning" Text=" You do not have any reminders." />
            </div>
        </asp:Panel>

            <asp:Panel ID="pnlView" runat="server" CssClass="panel-body">
                <script type="text/javascript">
                    $(document).ready(function () {
                        var activeFilterSetting = $('#<%=hfActiveFilterSetting.ClientID %>').val();
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

                            <div class="col">
                                <%-- Group/Person - Filter Options (Header) --%>

                                <div class="panel-toolbar panel-toolbar-right pr-0">
                                    <div>
                                        <!-- Filter for Groups/ChildGroups -->
                                        <Rock:GroupPicker ID="gpSelectedGroup" runat="server" Label="" CssClass="occurrences-groups-picker" Visible="false" OnValueChanged="gpSelectedGroup_ValueChanged" />
                                        <!-- Filter for Person -->
                                        <Rock:PersonPicker ID="ppSelectedPerson" runat="server" Label="" CssClass="occurrences-groups-picker" Visible="false" OnValueChanged="ppSelectedPerson_ValueChanged" />
                                    </div>

                                    <div class="btn-group">
                                        <div class="dropdown-toggle btn btn-xs btn-tool" data-toggle="dropdown">
                                            <i class="fa fa-check-circle-o"></i>
                                            <asp:Literal ID="lCompletionFilter" runat="server" Text="Incomplete" />
                                        </div>

                                        <ul class="dropdown-menu" role="menu">

                                                <li>
                                                    <asp:LinkButton ID="btnCompletion1" runat="server" Text="Incomplete" CommandArgument="Incomplete" OnClick="btnCompletion_Click" />
                                                </li>
                                                <li>
                                                    <asp:LinkButton ID="btnCompletion2" runat="server" Text="Complete" CommandArgument="Complete" OnClick="btnCompletion_Click" />
                                                </li>
                                                <li>
                                                    <asp:LinkButton ID="btnCompletion3" runat="server" Text="All" CommandArgument="All" OnClick="btnCompletion_Click" />
                                                </li>

                                        </ul>
                                    </div>

                                    <div class="btn-group">
                                        <div class="dropdown-toggle btn btn-xs btn-tool" data-toggle="dropdown">
                                            <i class="fa fa-calendar-alt"></i>
                                            <asp:Literal ID="lActiveFilter" runat="server" Text="Active" />
                                        </div>

                                        <ul class="dropdown-menu" role="menu">
                                        
                                                <li>
                                                    <asp:LinkButton ID="btnActive1" runat="server" Text="Active" CommandArgument="Active" OnClick="btnActive_Click" />
                                                </li>
                                                <li>
                                                    <asp:LinkButton ID="btnActive2" runat="server" Text="Active This Week" CommandArgument="Active This Week" OnClick="btnActive_Click" />
                                                </li>
                                                <li>
                                                    <asp:LinkButton ID="btnActive3" runat="server" Text="Active This Month" CommandArgument="Active This Month" OnClick="btnActive_Click" />
                                                </li>
                                                <li>
                                                    <asp:LinkButton ID="btnActive4" runat="server" Text="Custom Date Range" CommandArgument="Custom Date Range" OnClick="btnActive_Click" />
                                                </li>
                                        
                                        </ul>
                                    </div>

                                    <div id="reminders-custom-date-range" class="d-none">
                                        <asp:HiddenField ID="hfActiveFilterSetting" runat="server" Value="Active" />
                                        <Rock:SlidingDateRangePicker ID="drpCustomDate" runat="server"
                                            OnSelectedDateRangeChanged="drpCustomDate_SelectedDateRangeChanged"
                                            EnabledSlidingDateRangeTypes="Previous, Last, Current, DateRange"
                                            EnabledSlidingDateRangeUnits="Week, Month, Year, Day, Hour"
                                            SlidingDateRangeMode="Current"
                                            TimeUnit="Year"
                                            Label="" />
                                    </div>

                                    <div class="btn-group">
                                        <div class="dropdown-toggle btn btn-xs btn-tool" data-toggle="dropdown">
                                            <i class="fa fa-list"></i>
                                            <asp:Literal ID="lReminderType" runat="server" Text="All Reminder Types" />
                                        </div>

                                        <ul class="dropdown-menu" role="menu">
                                            <li>
                                                <asp:LinkButton ID="btnReminderType1" runat="server" Text="All" CommandArgument="All" OnClick="btnReminderType_Click" />
                                                <asp:Repeater ID="rptReminderType" runat="server" OnItemDataBound="rptReminderType_ItemDataBound">
                                                    <ItemTemplate>
                                                        <li>
                                                            <asp:LinkButton ID="btnEntityType" runat="server" Text="-" CommandArgument="-" OnClick="btnReminderType_Click" />
                                                        </li>
                                                    </ItemTemplate>
                                                </asp:Repeater>
                                            </li>
                                        </ul>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <asp:Repeater ID="rptReminders" runat="server" OnItemDataBound="rptReminders_ItemDataBound">
                        <ItemTemplate>
                            <div class="d-flex flex-sm-wrap py-3 px-sm-4 py-sm-4 border-bottom border-panel" data-reminder-id="<%# Eval("Id") %>">
                                <div class="flex-grow-0">
                                    <asp:LinkButton ID="lbComplete" runat="server" class="text-color" CommandArgument='<%# Eval("Id") %>' OnClick="lbComplete_Click">
                                        <asp:Literal ID="litCheckIcon_Complete" runat="server" Visible='<%# Eval("IsComplete") %>'><i class="fa fa-lg fa-check-circle text-success"></i></asp:Literal>
                                        <asp:Literal ID="litCheckIcon_Incomplete" runat="server" Visible='<%# (bool) Eval("IsComplete") == true ? false : true %>'><i class="fa fa-lg fa-check-circle-o"></i></asp:Literal>
                                    </asp:LinkButton>
                                </div>
                                <div class="d-flex flex-wrap flex-fill">
                                    <div class="d-inline-flex align-items-center align-items-sm-start col-xs-6 col-sm flex-grow-0 text-nowrap js-reminder-edit-trigger">
                                        <span class="label label-default"><asp:Literal ID="lReminderDate" runat="server" Text='<%# Eval("ReminderDate", "{0: M/d/yyyy}") %>' /></span>
                                    </div>
                                    <div class="col-xs-12 col-sm order-3 order-sm-0 mt-2 mt-sm-0 js-reminder-edit-trigger">
                                        <div class="d-flex">
                                            <div>
                                                <asp:Literal ID="litProfilePhoto" runat="server" Visible="false"><div class="photo-icon photo-round photo-round-xs pull-left margin-r-sm js-person-popover" personid="{0}" data-original="{1}&w=50" style="background-image: url( '{2}' ); background-size: cover; background-repeat: no-repeat;"></div></asp:Literal>
                                                <span class="d-block font-weight-semibold">
                                                    <asp:Literal ID="lEntity" runat="server" Text='<%# Eval("EntityDescription") %>' />
                                                </span>
                                                <span class="tag-flair text-sm">
                                                    <asp:Literal ID="lIcon" runat="server" Text='<%# "<span class=\"tag-color\" style=\"background-color: " + Eval("HighlightColor") + "\"></span>" %>' />
                                                    <asp:Literal ID="lReminderType" runat="server"  Text='<%# "<span class=\"tag-label\">" + Eval("ReminderType") + "</span>" %>' />
                                                </span>
                                            </div>

                                            <asp:Literal ID="lClock" runat="server" Visible='<%# Eval("IsRenewing") %>'><span class="text-info ml-auto"><i class="fa fa-clock-o"></i></span></asp:Literal>
                                        </div>
                                    
                                    
                                        <asp:Literal ID="lNote" runat="server"  Text='<%# Eval("Note") %>' />
                                    </div>
                                    <div class="col-xs-6 col-sm flex-grow-0 text-nowrap text-right">
                                        <%-- This button is used to initiate a postback for editing, but should not be displayed to the user. --%>
                                        <asp:LinkButton ID="lbEdit" runat="server" CssClass="btn btn-link btn-square btn-sm btn-overflow d-none js-reminder-edit-button" CommandArgument='<%# Eval("Id") %>' OnClick="lbEdit_Click" />
                                        <asp:LinkButton ID="lbDelete" runat="server" CssClass="btn btn-link btn-square btn-sm btn-overflow" CommandArgument='<%# Eval("Id") %>' OnClick="lbDelete_Click">
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
