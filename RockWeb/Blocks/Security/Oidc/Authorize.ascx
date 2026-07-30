<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Authorize.ascx.cs" Inherits="RockWeb.Blocks.Security.Oidc.Authorize" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbNotificationBox" runat="server" NotificationBoxType="Danger" Visible="false" Title="Error" />

        <div style="max-width: 360px; margin-left: auto; margin-right: auto;">
            <div class="card">
                <div class="card-body" style="display: flex; flex-direction: column; gap: var(--spacing-medium); padding: var(--spacing-large);">
                    <h3 style="text-align: center;">Authorization</h3>

                    <p style="text-align: center;">
                        <strong><asp:Literal ID="lNickName" runat="server" />, <asp:Literal ID="lClientName" runat="server" /> is requesting access to your account.</strong>
                        <br />
                        Please review the permissions below and grant or deny access.
                    </p>

                    <div style="display: flex; flex-direction: column; gap: var(--spacing-small);">
                        <asp:Repeater ID="rScopes" runat="server">
                            <ItemTemplate>
                                <div style="border: 1px solid var(--color-interface-soft); border-radius: var(--rounded-xsmall); padding: var(--spacing-small); text-align: center;"><%# Eval("Name") %></div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>

                    <div style="display: flex; justify-content: space-between;">
                        <a href="<%= Request.RawUrl %>&action=deny&token=<%= HttpUtility.UrlEncode(_antiXsrfTokenValue) %>" class="btn btn-default btn-lg">Decline</a>
                        <a href="<%= Request.RawUrl %>&action=approve&token=<%= HttpUtility.UrlEncode(_antiXsrfTokenValue) %>" class="btn btn-success btn-lg">Accept</a>
                    </div>
                </div>
           </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>

