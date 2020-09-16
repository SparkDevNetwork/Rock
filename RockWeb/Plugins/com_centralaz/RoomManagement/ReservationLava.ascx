<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReservationLava.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.RoomManagement.ReservationLava" %>
<%@ Register TagPrefix="CentralAZ" Assembly="com.centralaz.RoomManagement" Namespace="com.centralaz.RoomManagement.Web.UI.Controls" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfShowBy" runat="server" />

        <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />

        <asp:Panel ID="pnlDetails" runat="server" CssClass="row">

            <asp:Panel ID="pnlFilters" CssClass="col-md-3 hidden-print" runat="server">
                <Rock:YearPicker ID="ypYearPicker" runat="server" CssClass="margin-b-md" Visible="false" AutoPostBack="true" />

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

                <% if ( LocationPanelOpen || LocationPanelClosed )
                    { %>
                <div class="panel panel-default">
                    <div class="panel-heading">
                        <a role="button" data-toggle="collapse" href="#collapseOne">
                            <h4 class="panel-title">Locations                                
                            </h4>
                        </a>
                    </div>
                    <div id="collapseOne" class='<%= LocationPanelOpen ? "panel-collapse collapse in" : "panel-collapse collapse out" %>'>
                        <div class="panel-body">
                            <% } %>

                            <Rock:LocationItemPicker ID="lipLocation" runat="server" Label="Filter by Locations" AllowMultiSelect="true" OnSelectItem="lipLocation_SelectItem" />

                            <% if ( LocationPanelOpen || LocationPanelClosed )
                                { %>
                        </div>
                    </div>
                </div>
                <% } %>

                <% if ( ResourcePanelOpen || ResourcePanelClosed )
                    { %>
                <div class="panel panel-default">
                    <div class="panel-heading">
                        <a role="button" data-toggle="collapse" href="#collapseTwo">
                            <h4 class="panel-title">Resources                                
                            </h4>
                        </a>
                    </div>
                    <div id="collapseTwo" class='<%= ResourcePanelOpen ? "panel-collapse collapse in" : "panel-collapse collapse out" %>'>
                        <div class="panel-body">
                            <% } %>

                            <CentralAZ:ResourcePicker ID="rpResource" runat="server" Label="Filter by Resources" AllowMultiSelect="true" OnSelectItem="rpResource_SelectItem" />

                            <% if ( ResourcePanelOpen || ResourcePanelClosed )
                                { %>
                        </div>
                    </div>
                </div>
                <% } %>

                <% if ( CampusPanelOpen || CampusPanelClosed )
                    { %>
                <div class="panel panel-default">
                    <div class="panel-heading">
                        <a role="button" data-toggle="collapse" href="#collapseThree">
                            <h4 class="panel-title">Campuses                                
                            </h4>
                        </a>
                    </div>
                    <div id="collapseThree" class='<%= CampusPanelOpen ? "panel-collapse collapse in" : "panel-collapse collapse out" %>'>
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
                        <a role="button" data-toggle="collapse" href="#collapseFour">
                            <h4 class="panel-title">Ministries                            
                            </h4>
                        </a>
                    </div>
                    <div id="collapseFour" class='<%= MinistryPanelOpen ? "panel-collapse collapse in" : "panel-collapse collapse out" %>'>
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

                <% if ( ApprovalPanelOpen || ApprovalPanelClosed )
                    { %>
                <div class="panel panel-default">
                    <div class="panel-heading">
                        <a role="button" data-toggle="collapse" href="#collapseFive">
                            <h4 class="panel-title">Statuses                            
                            </h4>
                        </a>
                    </div>
                    <div id="collapseFive" class='<%= ApprovalPanelOpen ? "panel-collapse collapse in" : "panel-collapse collapse out" %>'>
                        <div class="panel-body">
                            <% } %>

                            <Rock:RockControlWrapper ID="rcwApproval" runat="server" Label="Filter by Approval State">
                                <div class="controls">
                                    <asp:CheckBoxList ID="cblApproval" RepeatDirection="Vertical" runat="server" OnSelectedIndexChanged="cblApproval_SelectedIndexChanged" AutoPostBack="true" />
                                </div>
                            </Rock:RockControlWrapper>

                            <% if ( ApprovalPanelOpen || ApprovalPanelClosed )
                                { %>
                        </div>
                    </div>
                </div>
                <% } %>

                <% if ( ReservationTypePanelOpen || ReservationTypePanelClosed )
                    { %>
                <div class="panel panel-default">
                    <div class="panel-heading">
                        <a role="button" data-toggle="collapse" href="#collapseSix">
                            <h4 class="panel-title">Reservation Types                                
                            </h4>
                        </a>
                    </div>
                    <div id="collapseSix" class='<%= ReservationTypePanelOpen ? "panel-collapse collapse in" : "panel-collapse collapse out" %>'>
                        <div class="panel-body">
                            <% } %>

                            <%-- Note: RockControlWrapper/Div/CheckboxList is being used instead of just a RockCheckBoxList, because autopostback does not currently work for RockControlCheckbox--%>
                            <Rock:RockControlWrapper ID="rcwReservationType" runat="server" Label="Filter by Reservation Type">
                                <div class="controls">
                                    <asp:CheckBoxList ID="cblReservationType" RepeatDirection="Vertical" runat="server" DataTextField="Name" DataValueField="Id"
                                        OnSelectedIndexChanged="cblReservationType_SelectedIndexChanged" AutoPostBack="true" />
                                </div>
                            </Rock:RockControlWrapper>

                            <% if ( ReservationTypePanelOpen || ReservationTypePanelClosed )
                                { %>
                        </div>
                    </div>
                </div>
                <% } %>

                <Rock:DatePicker ID="dpStartDate" runat="server" Label="Start Date" OnTextChanged="dpStartDate_TextChanged" AutoPostBack="true" />
                <Rock:DatePicker ID="dpEndDate" runat="server" Label="End Date" OnTextChanged="dpEndDate_TextChanged" AutoPostBack="true" />

                <small class="text-muted">v<asp:Literal ID="lVersionText" runat="server"></asp:Literal></small>
            </asp:Panel>

            <asp:Panel ID="pnlList" CssClass="col-md-9" runat="server">
                <div class="row">
                    <div class="col-md-12">
                        <div class="btn-group hidden-print pull-left" role="group">
                            <Rock:BootstrapButton ID="btnDay" runat="server" CssClass="btn btn-xs btn-default" Text="Day" OnClick="btnViewMode_Click" />
                            <Rock:BootstrapButton ID="btnWeek" runat="server" CssClass="btn btn-xs btn-default" Text="Week" OnClick="btnViewMode_Click" />
                            <Rock:BootstrapButton ID="btnMonth" runat="server" CssClass="btn btn-xs btn-default" Text="Month" OnClick="btnViewMode_Click" />
                            <Rock:BootstrapButton ID="btnYear" runat="server" CssClass="btn btn-xs btn-default" Text="Year" OnClick="btnViewMode_Click" />
                        </div>

                        <div id="divViewDropDown" runat="server" class="pull-left margin-l-sm">
                            <div class="btn-group">
                                <asp:HiddenField ID="hfSelectedView" runat="server" />
                                <button class="btn btn-default btn-xs dropdown-toggle" type="button" data-toggle="dropdown">
                                    <asp:Literal ID="lSelectedView" runat="server" />
                                    <span class="caret"></span>
                                </button>

                                <ul id="ulViewDropDown" runat="server" enableviewstate="false" class="dropdown-menu dropdown-menu-right">
                                    <asp:Repeater runat="server" ID="rptViews" OnItemCommand="rptViews_ItemCommand">
                                        <ItemTemplate>
                                            <li>
                                                <asp:LinkButton ID="btnView" runat="server" Text='<%# Eval("Value") %>' CommandArgument='<%# Eval("Id") %>' />
                                            </li>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </ul>
                            </div>
                        </div>

                        <div class="pull-right">
                            <div class="btn-group">
                                <button class="btn btn-default btn-xs dropdown-toggle" type="button" data-toggle="dropdown">Print <i class="fa fa-print"></i> <span class="caret"></span></button>

                                <ul id="ulReportDropDown" runat="server" enableviewstate="false" class="dropdown-menu dropdown-menu-right">
                                    <asp:Repeater runat="server" ID="rptReports" OnItemCommand="rptReports_ItemCommand">
                                        <ItemTemplate>
                                            <li>
                                                <asp:LinkButton ID="btnReport" runat="server" Text='<%# Eval("Value") %>' CommandArgument='<%# Eval("Id") %>' />
                                            </li>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </ul>
                            </div>
                        </div>

                        <div class="pull-right margin-r-md">
                            <asp:LinkButton ID="btnAllReservations" runat="server" CssClass="btn btn-xs btn-default" Text="&nbsp; All &nbsp;" data-val="0" OnClick="btnAllReservations_Click" />
                            <asp:LinkButton ID="btnMyReservations" runat="server" CssClass="btn btn-xs btn-default" Text="My Reservations" data-val="1" OnClick="btnMyReservations_Click" />
                            <asp:LinkButton ID="btnMyApprovals" runat="server" CssClass="btn btn-xs btn-default" Text="My Approvals" data-val="2" OnClick="btnMyApprovals_Click" />
                        </div>
                    </div>
                </div>


                <asp:Literal ID="lOutput" runat="server"></asp:Literal>
                <asp:Literal ID="lDebug" Visible="false" runat="server"></asp:Literal>

            </asp:Panel>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
