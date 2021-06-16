<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Authorize.ascx.cs" Inherits="RockWeb.Blocks.Security.Oidc.Authorize" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbNotificationBox" runat="server" NotificationBoxType="Danger" Visible="false" Title="Error" />
        <asp:Panel ID="pnlPanel" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-key"></i>
                    Authorization
                </h1>
            </div>
            <div class="panel-body">
                <p>
                    Would you like to grant <asp:Literal ID="lClientName" runat="server" /> access to your information:
                </p>
                <ul>
                    <asp:Repeater ID="rScopes" runat="server">
                        <ItemTemplate>
                            <li><%# Eval("Name") %></li>
                        </ItemTemplate>
                    </asp:Repeater>
                </ul>

                <div class="actions">
                    <a href="<%= Request.RawUrl %>&action=approve&token=<%= HttpUtility.UrlEncode(_antiXsrfTokenValue) %>" class="btn btn-primary">Yes</a>
                    <a href="<%= Request.RawUrl %>&action=deny&token=<%= HttpUtility.UrlEncode(_antiXsrfTokenValue) %>" class="btn btn-default">No</a>
                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>

