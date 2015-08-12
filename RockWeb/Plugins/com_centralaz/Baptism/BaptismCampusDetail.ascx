<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BaptismCampusDetail.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Baptism.BaptismCampusDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="row">
            <div class="col-md-12">
                <asp:Panel ID="pnlHeaderBar" runat="server" CssClass="panel panel-block">
                    <div class="panel-body">
                        <div class="col-md-2 col-sm-4">
                            <asp:LinkButton ID="lbAddBaptism" runat="server" CssClass="btn btn-link" OnClick="lbAddBaptism_Click"><h4>+Add Baptism</h4></asp:LinkButton>
                        </div>
                        <div class="col-md-2 col-sm-4">
                            <asp:LinkButton ID="lbAddBlackOut" runat="server" CssClass="btn btn-link" OnClick="lbAddBlackout_Click"><h4>Black Out a Day</h4></asp:LinkButton>
                        </div>
                        <div class="col-md-2 col-sm-4">
                            <asp:LinkButton ID="lbPrintReport" runat="server" CssClass="btn btn-link" OnClick="lbPrintReport_Click"><h4>Print Report</h4></asp:LinkButton>
                        </div>
                    </div>
                </asp:Panel>
            </div>
        </div>

        <div class="row">
            <div class="col-md-3">
                <asp:Panel ID="pnlCalendar" runat="server" CssClass="panel panel-block">

                    <div class="panel-heading">
                        <h1 class="panel-title">Calendar<h1>
                    </div>
                    <div class="panel-body">
                        <center>
                        <div id="calendar">
                            <asp:Calendar ID="calBaptism" runat="server"  DayNameFormat="FirstTwoLetters" SelectionMode="Day" PrevMonthText="«" NextMonthText="»"
                                TitleStyle-BackColor="#ffffff" NextPrevStyle-ForeColor="#333333" FirstDayOfWeek="Monday" Width="200" CssClass="calendar table-condensed" OnSelectionChanged="calBaptism_SelectionChanged" OnDayRender="calBaptisms_DayRender">
                                <DayStyle CssClass="calendar-day" ForeColor="#6a6a6a" />
                                <TodayDayStyle CssClass="calendar-today" />
                                <SelectedDayStyle CssClass="calendar-selected" />
                                <OtherMonthDayStyle CssClass="calendar-last-month" ForeColor="#dddddd" />
                                <DayHeaderStyle CssClass="calendar-day-header" />
                                <NextPrevStyle CssClass="calendar-next-prev" ForeColor="#777777" />
                                <TitleStyle CssClass="calendar-title" />
                            </asp:Calendar>
                        </div>
                        </center>
                    </div>

                </asp:Panel>
            </div>

            <div class="col-md-9">
                <asp:Panel ID="pnlBaptismList" runat="server" CssClass="panel panel-block">

                    <div class="panel-heading">
                        <h1 class="panel-title">
                            <asp:Literal ID="lPanelHeadingDateRange" runat="server"></asp:Literal>
                        </h1>
                    </div>
                    <div class="panel-body">
                        <Rock:NotificationBox ID="nbNoBaptisms" NotificationBoxType="Info" runat="server"></Rock:NotificationBox>
                        <Rock:NotificationBox ID="nbBlackOutWeek" NotificationBoxType="Danger" runat="server"></Rock:NotificationBox>
                        <asp:LinkButton ID='lbEditBlackout' CssClass="btn btn-link" runat='server' OnClick='lbEditBlackout_Click' Text='Edit' />
                        <asp:PlaceHolder ID="phBaptismList" runat="server"></asp:PlaceHolder>
                    </div>

                </asp:Panel>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
