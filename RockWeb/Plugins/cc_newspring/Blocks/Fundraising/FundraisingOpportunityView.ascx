<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FundraisingOpportunityView.ascx.cs" Inherits="RockWeb.Plugins.cc_newspring.Blocks.Fundraising.FundraisingOpportunityView" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlView" runat="server">
            <asp:HiddenField ID="hfGroupId" runat="server" />
            <asp:HiddenField ID="hfActiveTab" runat="server" />

            <asp:Literal ID="lMainTopContentHtml" runat="server" />

            <asp:Panel ID="pnlParticipantActions" runat="server">         
                <asp:HiddenField ID="hfGroupMemberId" runat="server" />
                <asp:Literal ID="lParticipantActionsHtml" runat="server" />
            </asp:Panel>

            <section class="bg-white soft xs-soft-half push-bottom xs-push-half-bottom rounded shadowed">

                
                <div class="row">
                    <div class="col-xs-12 col-sm-12 col-md-8 margin-b-md">
                        <ul id="tlTabList" runat="server" class="nav nav-pills margin-b-md">
                            <li id="liDetailsTab" runat="server"><asp:LinkButton ID="btnDetailsTab" runat="server" Text="Details" OnClick="btnDetailsTab_Click" /></li>
                            <li id="liUpdatesTab" runat="server"><asp:LinkButton ID="btnUpdatesTab" runat="server" Text="Updates" OnClick="btnUpdatesTab_Click" /></li>
                            <li id="liCommentsTab" runat="server"><asp:LinkButton ID="btnCommentsTab" runat="server" Text="Comments" OnClick="btnCommentsTab_Click" /></li>
                        </ul>
                        <asp:Panel ID="pnlDetails" CssClass="margin-t-md" runat="server">
                            <asp:Literal ID="lDetailsHtml" runat="server" />
                        </asp:Panel>
                        <asp:Panel ID="pnlUpdates" runat="server">
                            <asp:Literal ID="lUpdatesContentItemsHtml" runat="server" />
                        </asp:Panel>
                        <asp:Panel ID="pnlComments" CssClass="margin-t-md" runat="server">
                            <Rock:NoteContainer ID="notesCommentsTimeline" runat="server" UsePersonIcon="true" AddAllowed="true" />
                            <asp:Literal ID="lNoLoginNoCommentsYet" runat="server" ><br /><i>No comments yet.</i></asp:Literal>
                            <asp:LinkButton ID="btnLoginToComment" CssClass="btn btn-link pull-right" runat="server" Text="Login to Comment" OnClick="btnLoginToComment_Click" />
                        </asp:Panel>
                    </div><div class="col-xs-12 col-sm-12 col-md-4">
                        <asp:Literal ID="lSidebarHtml" runat="server" />
                        <asp:LinkButton ID="btnDonateToParticipant" runat="server" CssClass="btn btn-primary btn-block margin-t-sm" Text="Donate to a Participant" OnClick="btnDonateToParticipant_Click" />
                        <asp:LinkButton ID="btnLeaderToolbox" runat="server" CssClass="btn btn-default btn-block margin-t-sm" Text="Leader Toolbox" OnClick="btnLeaderToolbox_Click" />
                        
                        <div class="hidden">
                            <asp:Image ID="imgOpportunityPhoto" runat="server" CssClass="img-responsive" />
                        </div>
                    </div>

                </div>
            </section>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
