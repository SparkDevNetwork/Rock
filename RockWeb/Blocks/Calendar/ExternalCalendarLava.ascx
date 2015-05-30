﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ExternalCalendarLava.ascx.cs" Inherits="RockWeb.Blocks.Calendar.ExternalCalendarLava" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="col-md-3">
            <div class="row">
                <div id="calendar">
                    <asp:Calendar ID="calEventCalendar" runat="server" DayNameFormat="FirstLetter" SelectionMode="Day" BorderColor="#999999"
                        TitleStyle-BackColor="#e5e5e5" NextPrevStyle-ForeColor="#333333" FirstDayOfWeek="Sunday" Width="200" CssClass="calendar" OnSelectionChanged="calEventCalendar_SelectionChanged" OnDayRender="calEventCalendar_DayRender">
                        <DayStyle CssClass="calendar-day" />
                        <TodayDayStyle CssClass="calendar-today" />
                        <SelectedDayStyle CssClass="calendar-selected" />
                        <OtherMonthDayStyle CssClass="calendar-last-month" ForeColor="#999999" />
                        <DayHeaderStyle CssClass="calendar-day-header" />
                        <NextPrevStyle CssClass="calendar-next-prev" ForeColor="#777777" />
                        <TitleStyle CssClass="calendar-title" />
                    </asp:Calendar>
                </div>
            </div>
            <div class="row">
                <Rock:RockCheckBoxList ID="cblCampus" RepeatDirection="Vertical" runat="server" Label="Filter by Campus" DataTextField="Name" DataValueField="Id" OnSelectedIndexChanged="cblCampus_SelectedIndexChanged" />
            </div>
            <div class="row">
                <Rock:RockCheckBoxList ID="cblCategory" RepeatDirection="Vertical" runat="server" Label="Filter by Category" DataTextField="Name" DataValueField="Id" OnSelectedIndexChanged="cblCategory_SelectedIndexChanged" />
            </div>
            <div class="row">
                <Rock:DateRangePicker ID="drpDateRange" runat="server" Label="Select Range" />
            </div>
        </div>
        <div class="col-md-9">
            <div class="row">
                <div class="btn-group" role="group">
                    <Rock:BootstrapButton ID="btnDay" runat="server" CssClass="btn btn-default" Text="Day" OnClick="btnDay_Click" />
                    <Rock:BootstrapButton ID="btnWeek" runat="server" CssClass="btn btn-default" Text="Week" OnClick="btnWeek_Click" />
                    <Rock:BootstrapButton ID="btnMonth" runat="server" CssClass="btn btn-default" Text="Month" OnClick="btnMonth_Click" />
                </div>
            </div>
            <div class="panel">
                <div class="row">
                    <asp:Literal ID="lOutput" runat="server"></asp:Literal>
                </div>

            </div>
            <div class="row">
                <asp:Literal ID="lDebug" Visible="false" runat="server"></asp:Literal>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
