﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CalendarLava.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.Event.CalendarLava" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="cblCampus" />
        <asp:AsyncPostBackTrigger ControlID="cblCategory" />
    </Triggers>
    <ContentTemplate>

        <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />

        <asp:Panel id="pnlDetails" runat="server" CssClass="row">

            <!-- BEMA.FE3.Start -->
            <asp:Panel ID="pnlBemaFilters" runat="server" Visible="false">
                <div class="row margin-b-md">
                    <div id="divCampus" runat="server" class="col-md-6">
                        <Rock:CampusPicker runat="server" ID="cpCampus" AutoPostBack="true" Label="Congregation" OnSelectedIndexChanged="cpCampus_SelectedIndexChanged" />
                    </div>

                    <div id="divAudience" runat="server" class="col-md-6">
                        <Rock:DefinedValuePicker runat="server" ID="ddlCatPicker" AutoPostBack="true" Label="Ministries" Visible="false" OnSelectedIndexChanged="ddlCatPicker_SelectedIndexChanged" />
                    </div>
             
                    <div id="divDefinedValue" runat="server" class="col-md-4" visible="false">
                        <Rock:DefinedValuePicker runat="server" ID="ddlTopicPicker" AutoPostBack="true" Label="Topic" OnSelectedIndexChanged="ddlTopicPicker_SelectedIndexChanged" />
                    </div>
                </div>
            </asp:Panel>
            <!-- BEMA.FE3.End -->

            <asp:Panel ID="pnlFilters" CssClass="col-md-3 hidden-print" runat="server">

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

                <% if ( CampusPanelOpen || CampusPanelClosed )
                  { %>
                    <div class="panel panel-default">
                        <div class="panel-heading">
                            <h4 class="panel-title">
                                <a role="button" data-toggle="collapse" href="#collapseOne">Campuses
                                </a>
                            </h4>
                        </div>
                        <div id="collapseOne" class='<%= CampusPanelOpen ? "panel-collapse collapse in" : "panel-collapse collapse out" %>'>
                            <div class="panel-body">
                <% } %>

                                <%-- Note: RockControlWrapper/Div/CheckboxList is being used instead of just a RockCheckBoxList, because autopostback does not currently work for RockControlCheckbox--%>
                                <Rock:RockControlWrapper ID="rcwCampus" runat="server" Label="Filter by Campus">
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

                <% if ( CategoryPanelOpen || CategoryPanelClosed )
                   { %>
                    <div class="panel panel-default">
                        <div class="panel-heading">
                            <h4 class="panel-title">
                                <a role="button" data-toggle="collapse" href="#collapseTwo">Categories
                                </a>
                            </h4>
                        </div>
                        <div id="collapseTwo" class='<%= CategoryPanelOpen ? "panel-collapse collapse in" : "panel-collapse collapse out" %>'>
                            <div class="panel-body">
                <% } %>

                                <Rock:RockControlWrapper ID="rcwCategory" runat="server" Label="Filter by Category">
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

                <Rock:DateRangePicker ID="drpDateRange" runat="server" Label="Select Range" /><asp:LinkButton ID="lbDateRangeRefresh" runat="server" CssClass="btn btn-default btn-sm" Text="Refresh" OnClick="lbDateRangeRefresh_Click" />

            </asp:Panel>

            <asp:Panel ID="pnlList" CssClass="col-md-9" runat="server">

                <div class="btn-group hidden-print" role="group">
                    <Rock:BootstrapButton ID="btnDay" runat="server" CssClass="btn btn-default" Text="Day" OnClick="btnViewMode_Click" />
                    <Rock:BootstrapButton ID="btnWeek" runat="server" CssClass="btn btn-default" Text="Week" OnClick="btnViewMode_Click" />
                    <Rock:BootstrapButton ID="btnMonth" runat="server" CssClass="btn btn-default" Text="Month" OnClick="btnViewMode_Click" />
                    <Rock:BootstrapButton ID="btnYear" runat="server" CssClass="btn btn-default" Text="Year" OnClick="btnViewMode_Click" />
                    <Rock:BootstrapButton ID="btnAll" runat="server" CssClass="btn btn-default" Text="All" OnClick="btnViewMode_Click" />
                </div>

                <asp:Literal ID="lOutput" runat="server"></asp:Literal>
                <asp:Literal ID="lDebug" Visible="false" runat="server"></asp:Literal>

            </asp:Panel>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
