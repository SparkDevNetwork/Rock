<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupMemberUsernameTools.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Security.GroupMemberUsernameTools" %>
<asp:UpdatePanel id="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlCommands" runat="server">
            <div class="panel panel-block">
                <div class="panel-heading clearfix">
                    <h1 class="panel-title pull-left">
                        <i class="fa fa-wrench"></i>
                            <asp:Literal ID="lHeading" runat="server" Text="Group Member Tools" />              
                    </h1>
                </div>
                <div class="panel-body">
                    <div class="row">
                        <div class="col-sm-12">
                            <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />
                        </div>
                    </div>
                    <div class="list-as-blocks">
                    <ul>
                        <li>
                            <asp:LinkButton ID="lbGenerateUserLogin" runat="server" OnClick="lbGenerateUserLogin_Click">
                                <i class="far fa-user-plus"></i>
                                <h3>Generate User Login</h3>
                            </asp:LinkButton>
                        </li>
                        <li>
                            <asp:LinkButton ID="lbConvertArenaLogin" runat="server" OnClick="lbConvertArenaLogin_Click">
                                <i class="fa fa-user-secret"></i>
                                <h3>Convert Unused Arena Logins</h3>
                            </asp:LinkButton>
                        </li>
                    </ul>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>