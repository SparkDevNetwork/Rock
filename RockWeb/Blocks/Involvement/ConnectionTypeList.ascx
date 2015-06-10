<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConnectionTypeList.ascx.cs" Inherits="RockWeb.Blocks.Involvement.ConnectionTypeList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-plug"></i> Connection Types</h1>

                <div class="pull-right">
                    <asp:LinkButton ID="lbAddConnectionType" runat="server" CssClass="btn btn-action btn-xs pull-right" OnClick="lbAddConnectionType_Click" CausesValidation="false"><i class="fa fa-plus"></i></asp:LinkButton>
                </div>
            </div>
            <div class="panel-body">

                <div class="list-as-blocks margin-t-lg clearfix">
                    <ul class="list-unstyled">
                        <asp:Repeater ID="rptConnectionTypes" runat="server">
                            <ItemTemplate>
                                <li>
                                    <asp:LinkButton ID="lbConnectionType" runat="server" CommandArgument='<%# Eval("ConnectionType.Id") %>' CommandName="Display">
                                        <i class='<%# Eval("ConnectionType.IconCssClass") %>'></i>
                                        <h3><%# Eval("ConnectionType.Name") %> </h3>
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