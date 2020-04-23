<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CalendarLava.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.CustomBlocks.SUCH.Event.CalendarLava" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="cblCampus" />
        <asp:AsyncPostBackTrigger ControlID="cblCategory" />
    </Triggers>
    <ContentTemplate>

        <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />

        <asp:Panel id="pnlDetails" runat="server" CssClass="row">

            <asp:Panel ID="pnlFilters" CssClass="hidden-print " runat="server">

                <div class="col-md-3">

                    <div class="col-md-4 margin-b-md" style="display: none !important;">
                        <label class="control-label clearfix">Filter by Date</label>
                        <div class="hidden-print clearfix margin-b-md" role="group">

                            <Rock:BootstrapButton ID="btnDay" runat="server" CssClass="btn btn-event" Text="Day" OnClick="btnViewMode_Click" />
                            <Rock:BootstrapButton ID="btnWeek" runat="server" CssClass="btn btn-event" Text="Week" OnClick="btnViewMode_Click" />
                            <Rock:BootstrapButton ID="btnMonth" runat="server" CssClass="btn btn-event" Text="Month" OnClick="btnViewMode_Click" />
                            <Rock:BootstrapButton ID="btnYear" runat="server" CssClass="btn btn-event" Text="Year" OnClick="btnViewMode_Click" />
                        </div>
                        <label class="control-label clearfix">Calendar Type</label>
                        <div class=" hidden-print clearfix " role="group">

                            <Rock:BootstrapButton ID="btnList" runat="server" CssClass="btn btn-event" Text="List" OnClick="btnViewFeaturedMode_Click" />
                            <Rock:BootstrapButton ID="btnFeatured" runat="server" CssClass="btn btn-event" Text="Featured" OnClick="btnViewFeaturedMode_Click" />

                        </div>


                    </div>





                    <div id="campus-filter" class="filter-menu">
                        <%--Added campus-filter id - JM - 8/3/2018--%>
                        <% if ( CampusPanelOpen || CampusPanelClosed )
                            { %>
                        <div class="panel panel-default">
                            <div class="panel-heading">
                                <h4 class="panel-title">
                                    <a role="button" class="btn-block" data-toggle="collapse" href="#collapseOne">Filter by Campus</a>
                                </h4>
                            </div>
                            <div id="collapseOne" class='<%= CampusPanelOpen ? "panel-collapse collapse in" : "panel-collapse collapse out" %>'>
                                <div class="panel-body">
                                    <% } %>

                                    <%-- Note: RockControlWrapper/Div/CheckboxList is being used instead of just a RockCheckBoxList, because autopostback does not currently work for RockControlCheckbox--%>
                                    <Rock:RockControlWrapper ID="rcwCampus" runat="server">
                                        <div class="controls">
                                            <asp:CheckBoxList ID="cblCampus" RepeatDirection="Vertical" runat="server" DataTextField="Name" DataValueField="Id"
                                                OnSelectedIndexChanged="cblCampus_SelectedIndexChanged" AutoPostBack="true" />
                                        </div>
                                    </Rock:RockControlWrapper>

                                    <% if ( CampusPanelOpen || CampusPanelClosed )
                                        { %>
                                </div>
                            </div>
                        </div>
                        <% } %>
                    </div>
                    <div class="filter-menu">

                        <% if ( CategoryPanelOpen || CategoryPanelClosed )
                            { %>
                        <div class="panel panel-default">
                            <div class="panel-heading">
                                <h4 class="panel-title">
                                    <a role="button" class="btn-block" data-toggle="collapse" href="#collapseTwo">Filter by Category</a>
                                </h4>
                            </div>
                            <div id="collapseTwo" class='<%= CategoryPanelOpen ? "panel-collapse collapse in" : "panel-collapse collapse out" %>'>
                                <div class="panel-body">
                                    <% } %>

                                    <Rock:RockControlWrapper ID="rcwCategory" runat="server">
                                        <div class="controls">
                                            <asp:CheckBoxList ID="cblCategory" RepeatDirection="Vertical" runat="server" DataTextField="Value" DataValueField="Id" OnSelectedIndexChanged="cblCategory_SelectedIndexChanged" AutoPostBack="true" />
                                        </div>
                                    </Rock:RockControlWrapper>

                                    <% if ( CategoryPanelOpen || CategoryPanelClosed )
                                        { %>
                                </div>
                            </div>
                        </div>
                        <% } %>

                        <% if ( TypePanelOpen || TypePanelClosed )
                            { %>
                        <div class="panel panel-default">
                            <div class="panel-heading">
                                <h4 class="panel-title">
                                    <a role="button" class="btn-block" data-toggle="collapse" href="#collapseThree">Filter by Type</a>
                                </h4>
                            </div>
                            <div id="collapseThree" class='<%= TypePanelOpen ? "panel-collapse collapse in" : "panel-collapse collapse out" %>'>
                                <div class="panel-body">
                                    <% } %>

                                    <Rock:RockControlWrapper ID="rcwType" runat="server">
                                        <div class="controls">
                                            <asp:CheckBoxList ID="cblType" RepeatDirection="Vertical" runat="server" DataTextField="Value" DataValueField="Id" OnSelectedIndexChanged="cblCategory_SelectedIndexChanged" AutoPostBack="true" />
                                        </div>
                                    </Rock:RockControlWrapper>

                                    <% if ( TypePanelOpen || TypePanelClosed )
                                        { %>
                                </div>
                            </div>
                        </div>
                        <% } %>
                    </div>

                    <div class="col-md-4">
                        <Rock:DateRangePicker ID="drpDateRange" runat="server" Label="" />
                        <asp:LinkButton ID="lbDateRangeRefresh" runat="server" CssClass="btn btn-default btn-sm" Text="Refresh" OnClick="lbDateRangeRefresh_Click" />
                    </div>



                </div>

            </asp:Panel>

            <asp:Panel ID="pnlList" CssClass="col-md-8" runat="server">

                <asp:Panel ID="pnlCalendar" CssClass="calendar" runat="server">
                    <asp:Calendar ID="calEventCalendar" runat="server" DayNameFormat="FirstLetter" SelectionMode="Day" BorderStyle="None"
                        TitleStyle-BackColor="#ffffff" NextPrevStyle-ForeColor="#333333" FirstDayOfWeek="Sunday" Width="100%" CssClass="calendar-month" OnSelectionChanged="calEventCalendar_SelectionChanged" OnDayRender="calEventCalendar_DayRender" OnVisibleMonthChanged="calEventCalendar_VisibleMonthChanged">
                        <DayStyle CssClass="calendar-day" />
                        <TodayDayStyle CssClass="calendar-today" />
                        <SelectedDayStyle CssClass="calendar-selected" BackColor="Transparent" />
                        <OtherMonthDayStyle CssClass="calendar-last-month" />
                        <DayHeaderStyle CssClass="calendar-day-header" />
                        <NextPrevStyle CssClass="calendar-next-prev" />
                        <TitleStyle CssClass="calendar-title" />
                    </asp:Calendar>
                </asp:Panel>


                <asp:Literal ID="lOutput" runat="server"></asp:Literal>
                <asp:Literal ID="lDebug" Visible="false" runat="server"></asp:Literal>

            </asp:Panel>

        </asp:Panel>



    </ContentTemplate>
</asp:UpdatePanel>
