<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReminderList.ascx.cs" Inherits="RockWeb.Blocks.Reminders.ReminderList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlNotAuthenticated" runat="server" Visible="false">
            Please log in to use Reminders.
        </asp:Panel>

        <asp:Panel ID="pnlNoReminders" runat="server" Visible="false">
            You do not have any reminders.
        </asp:Panel>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block styled-scroll panel-groupscheduler">
            <script type="text/javascript">
                $(document).ready(function () {
                    var activeFilterSetting = $('#<%=hfActiveFilterSetting.ClientID %>').val();
                    if (activeFilterSetting == 'Custom Date Range') {
                        $('#reminders_custom_date_range').removeClass('d-none');
                    }
                });
            </script>
            <%-- Panel Header --%>
            <div class="panel-heading panel-follow">
                <h1 class="panel-title">
                    <i class="fa fa-bell"></i>
                    Reminders
                </h1>
            </div>

            <%-- Filter Options (Header) --%>
            <div class="panel-collapsable p-0">
                <div class="row row-eq-height no-gutters">
                    <div class="col-lg-3 col-md-4">
                        <%-- Entity Type - Filter Options (Header) --%>
                        <asp:Panel ID="pnlEntityType" runat="server" CssClass="panel-toolbar styled-scroll-white h-100 pr-1 resource-filter-options align-items-center">
                            <div class="btn-group">
                                <asp:Panel ID="pnlEntityTypeSelection" runat="server" CssClass="dropdown-toggle btn btn-xs btn-tool" data-toggle="dropdown">
                                    <asp:Literal ID="lSelectedEntityType" runat="server" Text="Entity Type" />
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

                    <div class="col-lg-9 col-md-8">
                        <%-- Group/Person - Filter Options (Header) --%>
                        <div class="panel-toolbar">
                            <!-- Filter for Groups/ChildGroups -->
                            <asp:Panel ID="pnlGroupPicker" runat="server" Visible="false" CssClass="d-flex">
                                <Rock:GroupPicker ID="gpSelectedGroup" runat="server" Label="" CssClass="occurrences-groups-picker" OnValueChanged="gpSelectedGroup_ValueChanged" />
                            </asp:Panel>

                            <!-- Filter for Person -->
                            <asp:Panel ID="pnlPersonPicker" runat="server" Visible="false" CssClass="d-flex">
                                <Rock:PersonPicker ID="ppSelectedPerson" runat="server" Label="" CssClass="occurrences-groups-picker" OnValueChanged="ppSelectedPerson_ValueChanged" />
                            </asp:Panel>
                        </div>

                        <div class="panel-toolbar">

                            <div class="btn-group">
                                <div class="dropdown-toggle btn btn-xs btn-tool" data-toggle="dropdown">
                                    <asp:Literal ID="lCompletionFilter" runat="server" Text="Incomplete" />
                                </div>

                                <ul class="dropdown-menu" role="menu">
                                    <li>
                                        <asp:LinkButton ID="btnCompletion1" runat="server" Text="Incomplete" CommandArgument="Incomplete" OnClick="btnCompletion_Click" />
                                        <asp:LinkButton ID="btnCompletion2" runat="server" Text="Complete" CommandArgument="Complete" OnClick="btnCompletion_Click" />
                                        <asp:LinkButton ID="btnCompletion3" runat="server" Text="All" CommandArgument="All" OnClick="btnCompletion_Click" />
                                    </li>
                                </ul>
                            </div>

                            <div class="btn-group">
                                <div class="dropdown-toggle btn btn-xs btn-tool" data-toggle="dropdown">
                                    <asp:Literal ID="lActiveFilter" runat="server" Text="Active" />'
                                </div>

                                <ul class="dropdown-menu" role="menu">
                                    <li>
                                        <asp:LinkButton ID="btnActive1" runat="server" Text="Active" CommandArgument="Active" OnClick="btnActive_Click" />
                                        <asp:LinkButton ID="btnActive2" runat="server" Text="Active This Week" CommandArgument="Active This Week" OnClick="btnActive_Click" />
                                        <asp:LinkButton ID="btnActive3" runat="server" Text="Active This Month" CommandArgument="Active This Month" OnClick="btnActive_Click" />
                                        <asp:LinkButton ID="btnActive4" runat="server" Text="Custom Date Range" CommandArgument="Custom Date Range" OnClick="btnActive_Click" />
                                    </li>
                                </ul>
                            </div>

                            <div id="reminders_custom_date_range" class="d-none">
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
                                    <asp:Literal ID="lReminderType" runat="server" Text="All" />'
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

            <%-- Panel Body --%>
            <div class="panel-body-parent">

                <asp:Repeater ID="rptReminders" runat="server">
                    <ItemTemplate>
                        <div class="row margin-b-sm">
                            <asp:HiddenField ID="hfReminderId" runat="server" Value='<%# Eval("Id") %>' />
                            <div class="col-md-2">
                                <asp:LinkButton ID="lbComplete" runat="server" CommandArgument='<%# Eval("Id") %>' OnClick="lbComplete_Click">
                                    <i class="fa fa-check-circle"></i>
                                </asp:LinkButton>
                                <asp:Literal ID="lReminderDate" runat="server" Text='<%# Eval("ReminderDate", "{0: M/d/yyyy}") %>' />
                            </div>
                            <div class="col-md-8">
                                <asp:Literal ID="lEntity" runat="server" Text='<%# Eval("EntityDescription") %>' />
                                <asp:Literal ID="lClock" runat="server" Visible='<%# Eval("IsRenewing") %>'><i class="fa fa-clock"></i></asp:Literal>
                                <asp:Literal ID="lIcon" runat="server" Text='<%# "<i class=\"fa fa-circle\" style=\"color: " + Eval("HighlightColor") + "\"></i>" %>' />
                                
                                <asp:Literal ID="lReminderType" runat="server"  Text='<%# Eval("ReminderType") %>' />
                                <asp:Literal ID="lNote" runat="server"  Text='<%# Eval("Note") %>' />
                            </div>
                            <div class="col-md-2">
                                <asp:LinkButton ID="lbEdit" runat="server" CommandArgument='<%# Eval("Id") %>' OnClick="lbEdit_Click">
                                    <i class="fa fa-pencil"></i>
                                </asp:LinkButton>
                                <asp:LinkButton ID="lbDelete" runat="server" CommandArgument='<%# Eval("Id") %>' OnClick="lbDelete_Click">
                                    <i class="fa fa-close"></i>
                                </asp:LinkButton>
                            </div>

                        </div>
                        <hr />
                    </ItemTemplate>
                </asp:Repeater>

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
