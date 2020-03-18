<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CareTypeList.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.PastoralCare.CareTypeList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-plug"></i>Care Types</h1>

                <div class="pull-right">
                    <asp:LinkButton ID="lbAddCareType" runat="server" CssClass="btn btn-action btn-xs pull-right" OnClick="lbAddCareType_Click" CausesValidation="false"><i class="fa fa-plus"></i></asp:LinkButton>
                </div>
            </div>
            <div class="panel-body">

                <div class="list-as-blocks clearfix">
                    <ul class="list-unstyled">
                        <asp:Repeater ID="rptCareTypes" runat="server">
                            <ItemTemplate>
                                <li>
                                    <asp:LinkButton ID="lbCareType" runat="server" CommandArgument='<%# Eval("Id") %>' CommandName="Display">
                                        <h3><%# Eval("Name") %> </h3>
                                    </asp:LinkButton>
                                </li>
                            </ItemTemplate>
                        </asp:Repeater>
                        <li runat="server" id="liSharedAttributes">
                            <asp:LinkButton ID="lbSharedAttributes" runat="server" OnClick="lbSharedAttributes_Click">
                                        <h3>Shared Attributes </h3>
                            </asp:LinkButton>
                        </li>
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
