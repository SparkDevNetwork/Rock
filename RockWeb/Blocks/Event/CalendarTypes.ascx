<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CalendarTypes.ascx.cs" Inherits="RockWeb.Blocks.Event.CalendarTypes" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="wizard">

             <div class="wizard-item active">
                <div class="wizard-item-icon">
                    <i class="fa fa-fw fa-calendar"></i>
                </div>
                <div class="wizard-item-label">
                    Event Calendars
                </div>
            </div>
            
            <div class="wizard-item">
                <div class="wizard-item-icon">
                    <i class="fa fa-fw fa-calendar"></i>
                </div>
                <div class="wizard-item-label">
                    Calendar
                </div>
            </div>
    
            <div class="wizard-item">
                <div class="wizard-item-icon">
                    <i class="fa fa-fw fa-calendar-o"></i>
                </div>
                <div class="wizard-item-label">
                    Calendar Item
                </div>
            </div>
    
            <div class="wizard-item">
                <div class="wizard-item-icon">
                    <i class="fa fa-fw fa-building-o"></i>
                </div>
                <div class="wizard-item-label">
                    Campus Detail
                </div>
            </div>

        </div>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-calendar"></i> Event Calendars</h1>

                <div class="pull-right">
                    <asp:LinkButton ID="lbAddEventCalendar" runat="server" CssClass="btn btn-action btn-xs pull-right" OnClick="lbAddEventCalendar_Click" CausesValidation="false"><i class="fa fa-plus"></i></asp:LinkButton>
                </div>
            </div>
            <div class="panel-body">

                <div class="list-as-blocks clearfix">
                    <ul class="list-unstyled">
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