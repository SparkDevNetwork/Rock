<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountabilityQuestionSummary.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Accountability.AccountabilityQuestionSummary" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><asp:Literal ID="lBlockTitle" runat="server" /></h1>


                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlblTest" runat="server" LabelType="Info" Text="Label" />
                </div>
            </div>

            <div class="panel-body">
                <asp:PlaceHolder ID="phQuestionSummary" runat="server" />
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
