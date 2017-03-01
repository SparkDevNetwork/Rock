<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReservationLava.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.RoomManagement.ReservationLava" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />

        <asp:Panel ID="pnlDetails" runat="server" CssClass="row">

            <asp:Panel ID="pnlFilters" CssClass="col-md-3 hidden-print" runat="server">

                <asp:Panel ID="pnlCalendar" CssClass="calendar" runat="server">
                    <asp:Calendar ID="calReservationCalendar" runat="server" DayNameFormat="FirstLetter" SelectionMode="Day" BorderStyle="None"
                        TitleStyle-BackColor="#ffffff" NextPrevStyle-ForeColor="#333333" FirstDayOfWeek="Sunday" Width="100%" CssClass="calendar-month" OnSelectionChanged="calReservationCalendar_SelectionChanged" OnDayRender="calReservationCalendar_DayRender" OnVisibleMonthChanged="calReservationCalendar_VisibleMonthChanged">
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
                        <a role="button" data-toggle="collapse" href="#collapseOne">
                            <h4 class="panel-title">Campuses                                
                            </h4>
                        </a>
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

                <% if ( MinistryPanelOpen || MinistryPanelClosed )
                   { %>
                <div class="panel panel-default">
                    <div class="panel-heading">
                        <a role="button" data-toggle="collapse" href="#collapseTwo">
                            <h4 class="panel-title">Ministries                            
                            </h4>
                        </a>
                    </div>
                    <div id="collapseTwo" class='<%= MinistryPanelOpen ? "panel-collapse collapse in" : "panel-collapse collapse out" %>'>
                        <div class="panel-body">
                            <% } %>

                            <Rock:RockControlWrapper ID="rcwMinistry" runat="server" Label="Filter by Ministry">
                                <div class="controls">
                                    <asp:CheckBoxList ID="cblMinistry" RepeatDirection="Vertical" runat="server" DataTextField="Name" DataValueField="Id" OnSelectedIndexChanged="cblMinistry_SelectedIndexChanged" AutoPostBack="true" />
                                </div>
                            </Rock:RockControlWrapper>

                            <% if ( MinistryPanelOpen || MinistryPanelClosed )
                               { %>
                        </div>
                    </div>
                </div>
                <% } %>

                <Rock:DateRangePicker ID="drpDateRange" runat="server" Label="Select Range" />
                <asp:LinkButton ID="lbDateRangeRefresh" runat="server" CssClass="btn btn-default btn-sm" Text="Refresh" OnClick="lbDateRangeRefresh_Click" />

            </asp:Panel>

            <asp:Panel ID="pnlList" CssClass="col-md-9" runat="server">

                <div class="btn-group hidden-print" role="group">
                    <Rock:BootstrapButton ID="btnDay" runat="server" CssClass="btn btn-default" Text="Day" OnClick="btnViewMode_Click" />
                    <Rock:BootstrapButton ID="btnWeek" runat="server" CssClass="btn btn-default" Text="Week" OnClick="btnViewMode_Click" />
                    <Rock:BootstrapButton ID="btnMonth" runat="server" CssClass="btn btn-default" Text="Month" OnClick="btnViewMode_Click" />
                </div>

                <asp:Literal ID="lOutput" runat="server"></asp:Literal>
                <asp:Literal ID="lDebug" Visible="false" runat="server"></asp:Literal>

            </asp:Panel>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
