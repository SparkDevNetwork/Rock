<%@ Control Language="C#" AutoEventWireup="true" CodeFile="InteractionComponentDetail.ascx.cs" Inherits="RockWeb.Blocks.Reporting.InteractionComponentDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-th"></i>
                    <asp:Literal ID="lTitle" runat="server" />
                </h1>
            </div>

            <div class="panel-body">
                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" Visible="false" />
                <asp:Literal ID="lContent" runat="server"></asp:Literal>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
