<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CheckinTypes.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Config.CheckinTypes" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-check-square-o"></i> Check-in Configurations</h1>

                <div class="pull-right">
                    <asp:LinkButton ID="lbAddCheckinType" runat="server" CssClass="btn btn-action btn-xs btn-square pull-right" OnClick="lbAddCheckinType_Click" CausesValidation="false"><i class="fa fa-plus"></i></asp:LinkButton>
                </div>
            </div>
            <div class="panel-body">

                <div class="list-as-blocks clearfix">
                    <ul>
                        <asp:Repeater ID="rptCheckinTypes" runat="server">
                            <ItemTemplate>
                                <li class='<%# Eval("ActiveCssClass") %>'>
                                    <asp:LinkButton ID="lbCheckinType" runat="server" CommandArgument='<%# Eval("Id") %>' CommandName="Display">
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

    </ContentTemplate>
</asp:UpdatePanel>