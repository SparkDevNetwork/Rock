<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FundraisingOpportunityView.ascx.cs" Inherits="RockWeb.Blocks.Fundraising.FundraisingOpportunityView" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlView" runat="server">
            <asp:HiddenField ID="hfGroupId" runat="server" />
            <asp:HiddenField ID="hfActiveTab" runat="server" />
            <asp:Literal ID="lLavaHelp" runat="server" />

            <div class="row">

                <%-- Left Sidebar --%>
                <div class="col-md-4 margin-t-lg">
                    <asp:Image ID="imgOpportunityPhoto" runat="server" CssClass="title-image img-responsive" />
                    <asp:LinkButton ID="btnDonateToParticipant" runat="server" CssClass="btn btn-primary btn-block margin-t-md" Text="Donate to a Participant" OnClick="btnDonateToParticipant_Click" />
                    <asp:LinkButton ID="btnLeaderToolbox" runat="server" CssClass="btn btn-primary btn-block margin-t-md" Text="Leader Toolbox" OnClick="btnLeaderToolbox_Click" />
                    <asp:Literal ID="lSidebarHtml" runat="server" />
                </div>

                <%-- Main --%>
                <div class="col-md-8 margin-b-md">

                    <asp:Literal ID="lMainTopContentHtml" runat="server" />

                    <asp:Panel ID="pnlParticipantActions" runat="server">
                        <asp:HiddenField ID="hfGroupMemberId" runat="server" />
                        <div class="well margin-t-md">
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

                    <br />

                    <div class="btn-group margin-t-md">
                        <asp:LinkButton ID="btnDetailsTab" runat="server" Text="Details" CssClass="btn btn-primary" OnClick="btnDetailsTab_Click" />
                        <asp:LinkButton ID="btnUpdatesTab" runat="server" Text="Updates" CssClass="btn btn-default" OnClick="btnUpdatesTab_Click" />
                        <asp:LinkButton ID="btnCommentsTab" runat="server" Text="Comments" CssClass="btn btn-default" OnClick="btnCommentsTab_Click" />
                    </div>
                    <asp:Panel ID="pnlDetails" CssClass="margin-t-md" runat="server">
                        <asp:Literal ID="lDetailsHtml" runat="server" />
                    </asp:Panel>
                    <asp:Panel ID="pnlUpdates" runat="server">
                        <asp:Literal ID="lUpdatesContentItemsHtml" runat="server" />
                    </asp:Panel>
                    <asp:Panel ID="pnlComments" CssClass="margin-t-md" runat="server">
                        <Rock:NoteContainer ID="notesCommentsTimeline" runat="server" UsePersonIcon="true" AddAllowed="true" />
                    </asp:Panel>
                </div>

            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
