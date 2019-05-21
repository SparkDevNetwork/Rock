<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CalendarTypes.ascx.cs" Inherits="RockWeb.Blocks.Event.CalendarTypes" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-th-large"></i> Event Calendars</h1>

                <div class="pull-right">
                    <asp:LinkButton ID="lbEventCalenderAttributes" runat="server" CssClass="btn btn-default btn-xs btn-square" OnClick="lbEventCalenderAttributes_Click" CausesValidation="false" ToolTip="Calendar Attributes"><i class="fa fa-gear"></i></asp:LinkButton>
                    <asp:LinkButton ID="lbAddEventCalendar" runat="server" CssClass="btn btn-action btn-xs btn-square" OnClick="lbAddEventCalendar_Click" CausesValidation="false" ToolTip="Add Event Calendar"><i class="fa fa-plus"></i></asp:LinkButton>
                </div>
            </div>
            <div class="panel-body">

                <div class="list-as-blocks clearfix">
                    <ul>
                        <asp:Repeater ID="rptEventCalendars" runat="server">
                            <ItemTemplate>
                                <li>
                                    <asp:LinkButton ID="lbEventCalendar" runat="server" CommandArgument='<%# Eval("Id") %>' CommandName="Display">
                                        <i class='<%# Eval("IconCssClass") %>'></i>
                                        <h3><%# Eval("Name") %> </h3>
                                    </asp:LinkButton>
                                </li>
                            </ItemTemplate>
                        </asp:Repeater>
                    </ul>
                </div>
            </div>
        </div>
        <script>
            $(".my-workflows .list-as-blocks li").on("click", function () {
                $(".my-workflows .list-as-blocks li").removeClass('active');
                $(this).addClass('active');
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>