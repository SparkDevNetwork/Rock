<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReservationTypeList.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.RoomManagement.ReservationTypeList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-plug"></i> Reservation Types</h1>

                <div class="pull-right">
                    <asp:LinkButton ID="lbAddReservationType" runat="server" CssClass="btn btn-action btn-xs pull-right" OnClick="lbAddReservationType_Click" CausesValidation="false"><i class="fa fa-plus"></i></asp:LinkButton>
                </div>
            </div>
            <div class="panel-body">

                <div class="list-as-blocks clearfix">
                    <ul class="list-unstyled">
                        <asp:Repeater ID="rptReservationTypes" runat="server">
                            <ItemTemplate>
                                <li>
                                    <asp:LinkButton ID="lbReservationType" runat="server" CommandArgument='<%# Eval("Id") %>' CommandName="Display">
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
