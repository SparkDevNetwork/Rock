<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FundraisingOpportunityView.ascx.cs" Inherits="RockWeb.Blocks.Fundraising.FundraisingOpportunityView" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfGroupId" runat="server" />
        <div class="row">
            <%-- NOTE: this is RIGHT side of the page (The main content), but declared first so that it ends up at the top when the window is narrow --%>
            <div class="col-md-8 col-md-push-4 margin-b-md">
                <asp:Literal ID="lMainTopContentLava" runat="server" />

                <asp:Panel ID="pnlParticipantActions" runat="server">
                    <div class="well">
                        <asp:Image ID="imgParticipant" runat="server" CssClass="pull-left" />
                        <asp:Literal ID="lFundraisingProgress" runat="server" />
                        <div class="actions pull-right">
                            <asp:LinkButton ID="btnParticipantPage" runat="server" Text="Participant Page" CssClass="btn btn-primary" OnClick="btnParticipantPage_Click" />
                            <asp:LinkButton ID="btnMakePayment" runat="server" Text="Make Payment" CssClass="btn btn-default" OnClick="btnMakePayment_Click" />
                        </div>
                    </div>
                </asp:Panel>

                <div class="btn-group">
                    <asp:LinkButton ID="btnDetailsTab" runat="server" Text="Details" CssClass="btn btn-primary" OnClick="btnDetailsTab_Click" />
                    <asp:LinkButton ID="btnUpdatesTab" runat="server" Text="Updates" CssClass="btn btn-default" OnClick="btnUpdatesTab_Click" />
                    <asp:LinkButton ID="btnCommentsTab" runat="server" Text="Comments" CssClass="btn btn-default" OnClick="btnCommentsTab_Click" />
                </div>
                <asp:Panel ID="pnlDetails" runat="server">
                    <asp:Literal ID="lDetailsHtml" runat="server" />
                </asp:Panel>
                <asp:Panel ID="pnlUpdates" runat="server">
                    <asp:Repeater ID="rptUpdates" runat="server" OnItemDataBound="rptUpdates_ItemDataBound">
                        <ItemTemplate>
                            <asp:Literal ID="lUpdatesContentItemLava" runat="server" />
                        </ItemTemplate>
                    </asp:Repeater>

                </asp:Panel>
                <asp:Panel ID="pnlComments" runat="server">
                    <Rock:NoteContainer ID="notesCommentsTimeline" runat="server" UsePersonIcon="true" />
                </asp:Panel>
            </div>

            <%-- NOTE: this is LEFT side of the page (The left sidebar stuff), but declared 2nd so that it ends up at the bottom when the window is narrow --%>
            <div class="col-md-4 col-md-pull-8">
                <div class="well">
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
