<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PopupCalendar.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Widgets.PopupCalendar" %>
<script type="text/javascript">
    Sys.Application.add_load(function () {
        $(document).click(function (event) {
            if (!$(event.target).closest('#<%=pnlPopup.ClientID %>').length) {
                if ($('#<%=pnlPopup.ClientID %>').is(":visible")) {
                    $('#<%=pnlPopup.ClientID %>').hide()
                }
            }
        });
    });
</script>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />

        <asp:Panel ID="pnlDetails" runat="server" CssClass="row">

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

            <asp:Panel ID="pnlPopup" runat="server" Visible="false">
                <asp:Literal ID="lOutput" runat="server"></asp:Literal>
            </asp:Panel>

            <asp:Literal ID="lDebug" Visible="false" runat="server"></asp:Literal>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
