<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MetricDatePicker.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.ChurchMetrics.MetricDatePicker" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server">
            <asp:Label ID="lHoliday" runat="server" Text="Select the holiday you would like to view metrics for:" />
            <asp:DropDownList ID="ddlHoliday" runat="server" OnSelectedIndexChanged="ddlHoliday_SelectedIndexChanged" AutoPostBack="true" Font-Size="Large">
                <asp:ListItem Value="Christmas" Text="Christmas" />
                <asp:ListItem Value="Thanksgiving" Text="Thanksgiving" />
                <asp:ListItem Value="Easter" Text="Easter" />               
            </asp:DropDownList>
            <div class="row">
                <div class="col-md-10">
                    <asp:Literal ID="lOutput" runat="server" />
                </div>
                <div class="col-md-2">
                    <div class="pull-right">
                        <asp:Calendar ID="calCalendar" runat="server" DayNameFormat="FirstLetter" SelectionMode="Day" BorderStyle="Double"
                            TitleStyle-BackColor="#ffffff" NextPrevStyle-ForeColor="#333333" FirstDayOfWeek="Sunday" Width="100%" CssClass="calendar-month" OnSelectionChanged="calCalendar_SelectionChanged">
                            <DayStyle CssClass="calendar-day" />
                            <TodayDayStyle CssClass="calendar-today" />
                            <SelectedDayStyle CssClass="calendar-selected" BackColor="Transparent" />
                            <OtherMonthDayStyle CssClass="calendar-last-month" />
                            <DayHeaderStyle CssClass="calendar-day-header" />
                            <NextPrevStyle CssClass="calendar-next-prev" />
                            <TitleStyle CssClass="calendar-title" />
                        </asp:Calendar>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
