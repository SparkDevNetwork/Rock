<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountabilityQuestionSummary.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Accountability.AccountabilityQuestionSummary" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <div class="panel panel-default">

                <div class="panel-heading">
                    <h1 class="panel-title">
                        <asp:Literal ID="lBlockTitle" runat="server" /></h1>
                </div>

                <div class="panel-body">
                    <asp:PlaceHolder ID="phQuestionSummary" runat="server" />
                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
