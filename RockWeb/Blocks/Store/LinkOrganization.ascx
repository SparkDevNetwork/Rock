<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LinkOrganization.ascx.cs" Inherits="RockWeb.Blocks.Store.LinkOrganization" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="ti ti-link"></i>Link Organization</h1>
            </div>
            <div class="panel-body">

                <p>
                    To get the most out of Rock, link your organization to a registered
                    Spark Development Network account.
                </p>

                <p>
                    To do this you will be directed to the Spark Development Network website
                    where you will be asked to log in and authorize Rock to access your
                    organization.
                </p>

                <asp:Panel ID="pnlStart" runat="server">
                    <Rock:NotificationBox ID="nbStartError" runat="server" NotificationBoxType="Warning" />

                    <asp:Button ID="btnStart" CssClass="btn btn-primary" runat="server" OnClick="btnStart_Click" Text="Link Organization" />
                </asp:Panel>

                <asp:Panel ID="pnlSuccess" runat="server" Visible="false">
                    <Rock:NotificationBox ID="nbLinkSuccess" runat="server" NotificationBoxType="Success" />
                </asp:Panel>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
