<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CalendarTypes.ascx.cs" Inherits="RockWeb.Blocks.Calendar.CalendarTypes" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-calendar"></i>Calendar Types</h1>

                <div class="pull-right">
                    <%-- Add button to add calendar type --%>
                    <Rock:Toggle ID="tglDisplay" runat="server" OnText="Active Types" ActiveButtonCssClass="btn-success" ButtonSizeCssClass="btn-xs" OffText="All Types" AutoPostBack="true" />
                </div>

            </div>
            <div class="panel-body">

                <div class="list-as-blocks margin-t-lg clearfix">
                    <ul class="list-unstyled">
                        <asp:Repeater ID="rptEventCalendarTypes" runat="server">
                            <ItemTemplate>
                                <li>
                                    <asp:LinkButton ID="lbEventCalendar" runat="server" CommandArgument='<%# Eval("EventCalendar.Id") %>' CommandName="Display">
                                        <i class='<%# Eval("EventCalendar.IconCssClass") %>'></i>
                                        <h3><%# Eval("EventCalendar.Name") %> </h3>
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
