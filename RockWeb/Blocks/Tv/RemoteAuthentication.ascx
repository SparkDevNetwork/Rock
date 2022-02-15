<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RemoteAuthentication.ascx.cs" Inherits="RockWeb.Blocks.Tv.RemoteAuthentication" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <style>
            .code-container {
                text-align: center;

            }

            .code-entry {
                width: 200px;
                text-align: center;
                font-size: 24px;
                font-weight: 700;
                padding: 24px 24px;
                letter-spacing: 6px;
                text-transform: uppercase;
                margin: auto;
            }

        </style>

        <Rock:NotificationBox ID="nbWarningMessages" runat="server" NotificationBoxType="Warning" />

        <asp:Panel ID="pnlAuthenticate" runat="server" CssClass="code-container">

            <asp:Literal ID="lHeaderInfo" runat="server" />

            <Rock:RockTextBox ID="txtSecurityCode" runat="server" CssClass="code-entry" />

            <asp:LinkButton ID="btnSubmit" runat="server" CssClass="btn btn-primary btn-remoteauth my-2" Text="Submit" OnClick="btnSubmit_Click" />

            <Rock:NotificationBox ID="nbAuthenticationMessages" runat="server" NotificationBoxType="Info" CssClass="my-3" />

            <asp:Literal ID="lFooterInfo" runat="server" />

        </asp:Panel>

        <asp:Panel ID="pnlSuccess" runat="server" Visible="false">
            <asp:Literal ID="lSuccessContent" runat="server" />
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>