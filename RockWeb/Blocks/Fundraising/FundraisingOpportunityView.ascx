<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FundraisingOpportunityView.ascx.cs" Inherits="RockWeb.Blocks.Fundraising.FundraisingOpportunityView" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfGroupId" runat="server" />
        <asp:HiddenField ID="hfActiveTab" runat="server" />
        <asp:Literal ID="lLavaHelp" runat="server" />
        <div class="row">
            <%-- NOTE: this is RIGHT side of the page (The main content), but declared first so that it ends up at the top when the window is narrow --%>
            <div class="col-md-8 col-md-push-4 margin-b-md">
                <asp:Literal ID="lMainTopContentHtml" runat="server" />

                <br />
                <asp:Panel ID="pnlParticipantActions" runat="server">
                    <div class="well">
                        <div class="row">
                            <div class="col-md-9">
                                <asp:Image ID="imgParticipant" runat="server" CssClass="pull-left" />
                                <label>
                                    <asp:Literal ID="lFundraisingProgressTitle" runat="server" Text="Fundraising Progress" />
                                </label>
                                <label class='pull-right'>
                                    <asp:Literal ID="lFundraisingAmountLeftText" runat="server" Text="$320 left" />
                                </label>
                                <asp:Literal ID="lFundraisingProgressBar" runat="server" />
                            </div>
                            <div class="col-md-3">
                                <div class="pull-right">
                                    <asp:LinkButton ID="btnParticipantPage" runat="server" Text="Participant Page" CssClass="btn btn-xs btn-block btn-primary" OnClick="btnParticipantPage_Click" />
                                    <asp:LinkButton ID="btnMakePayment" runat="server" Text="Make Payment" CssClass="btn btn-xs btn-block btn-default" OnClick="btnMakePayment_Click" />
                                </div>
                            </div>
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
                    <asp:Literal ID="lUpdatesContentItemsHtml" runat="server" />
                </asp:Panel>
                <asp:Panel ID="pnlComments" runat="server">
                    <Rock:NoteContainer ID="notesCommentsTimeline" runat="server" UsePersonIcon="true" AddAllowed="true" />
                </asp:Panel>
            </div>

            <%-- NOTE: this is LEFT side of the page (The left sidebar stuff), but declared 2nd so that it ends up at the bottom when the window is narrow --%>
            <div class="col-md-4 col-md-pull-8">
                <asp:Image ID="imgPhoto" runat="server" CssClass="title-image img-responsive" ImageUrl="#todo#" />
                <asp:LinkButton ID="btnDonateToParticipant" runat="server" CssClass="btn btn-primary btn-block margin-t-md" Text="Donate to a Participant" OnClick="btnDonateToParticipant_Click" />
                <asp:LinkButton ID="btnLeaderToolbox" runat="server" CssClass="btn btn-primary btn-block margin-t-md" Text="Leader Toolbox" OnClick="btnLeaderToolbox_Click" />
                <asp:Literal ID="lSidebarHtml" runat="server" />
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
