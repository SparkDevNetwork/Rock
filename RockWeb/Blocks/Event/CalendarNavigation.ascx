<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CalendarNavigation.ascx.cs" Inherits="RockWeb.Blocks.Event.CalendarNavigation" %>

<asp:UpdatePanel ID="pnlContent" runat="server">
    <ContentTemplate>

        <div class="wizard">

             <div id="divCalendars" runat="server" >
                <asp:LinkButton ID="lbCalendars" runat="server" OnClick="lbCalendars_Click" CausesValidation="false" >
                    <%-- Placeholder needed for bug. See: http://stackoverflow.com/questions/5539327/inner-image-and-text-of-asplinkbutton-disappears-after-postback--%>
                    <asp:PlaceHolder runat="server">
                        <div class="wizard-item-icon">
                            <i class="fa fa-fw fa-calendar"></i>
                        </div>
                        <div class="wizard-item-label">
                            Event Calendars
                        </div>
                    </asp:PlaceHolder>
                </asp:LinkButton>
            </div>
    
            <div id="divCalendar" runat="server" >
                <asp:LinkButton ID="lbCalendar" runat="server" OnClick="lbCalendar_Click" CausesValidation="false" >
                    <asp:PlaceHolder runat="server">
                        <div class="wizard-item-icon">
                            <i class="fa fa-fw fa-calendar"></i>
                        </div>
                        <div class="wizard-item-label">
                            <asp:Literal ID="lCalendarName" runat="server" Text="Calendar" />
                        </div>
                    </asp:PlaceHolder>
                </asp:LinkButton>
            </div>
    
            <div id="divCalendarItem" runat="server" >
                <asp:LinkButton ID="lbCalendarItem" runat="server" OnClick="lbCalendarItem_Click" CausesValidation="false" >
                    <asp:PlaceHolder runat="server">
                        <div class="wizard-item-icon">
                            <i class="fa fa-fw fa-calendar-o"></i>
                        </div>
                        <div class="wizard-item-label">
                            <asp:Literal ID="lCalendarItemName" runat="server" Text="Calendar Item" />
                        </div>
                    </asp:PlaceHolder>
                </asp:LinkButton>
            </div>
    
            <div id="divEventOccurrence" runat="server" >
                <asp:LinkButton ID="lbEventOccurrence" runat="server" OnClick="lbEventOccurrence_Click" CausesValidation="false" >
                    <asp:PlaceHolder runat="server">
                        <div class="wizard-item-icon">
                            <i class="fa fa-fw fa-building-o"></i>
                        </div>
                        <div class="wizard-item-label">
                            <asp:Literal ID="lEventOccurrenceName" runat="server" Text="Event Occurrence" />
                        </div>
                    </asp:PlaceHolder>
                </asp:LinkButton>
            </div>

            <div id="divContentItem" runat="server" >
                <div class="wizard-item-icon">
                    <i class="fa fa-fw fa-bullhorn"></i>
                </div>
                <div class="wizard-item-label">
                    <asp:Literal ID="lContentItemName" runat="server" Text="Content Item" />
                </div>
            </div>

        </div>

    </ContentTemplate>
</asp:UpdatePanel>
