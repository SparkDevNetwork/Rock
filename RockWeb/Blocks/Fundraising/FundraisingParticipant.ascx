<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FundraisingParticipant.ascx.cs" Inherits="RockWeb.Blocks.Fundraising.FundraisingParticipant" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfGroupId" runat="server" />
        <asp:HiddenField ID="hfActiveTab" runat="server" />
        <asp:Literal ID="lLavaHelp" runat="server" />
        <div class="row">
            <div class="col-md-4">
                <asp:Image ID="imgPhoto" runat="server" CssClass="title-image img-responsive" />
                <asp:LinkButton ID="btnEditPreferences" runat="server" CssClass="btn btn-primary btn-block margin-t-md" Text="Edit Preferences" OnClick="btnEditPreferences_Click" />
                <asp:LinkButton ID="btnMainPage" runat="server" CssClass="btn btn-primary btn-block margin-t-md" Text="Main Page" OnClick="btnMainPage_Click" />
            </div>
            <div class="col-md-8">
                <asp:Literal ID="lMainTopContentHtml" runat="server" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-12">
                <label>
                    <asp:Literal ID="lFundraisingProgressTitle" runat="server" Text="Fundraising Progress" />
                </label>
                <label class='pull-right'>
                    <asp:Literal ID="lFundraisingAmountLeftText" runat="server" Text="$320 left" />
                </label>
                <asp:Literal ID="lFundraisingProgressBar" runat="server" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-8">
                <div class="btn-group">
                    <asp:LinkButton ID="btnUpdatesTab" runat="server" Text="Updates" CssClass="btn btn-default" OnClick="btnUpdatesTab_Click" />
                    <asp:LinkButton ID="btnContributionsTab" runat="server" Text="Contributions" CssClass="btn btn-default" OnClick="btnContributionsTab_Click" />
                </div>
                <asp:Panel ID="pnlUpdates" runat="server">
                    <asp:Literal ID="lUpdatesContentItemsHtml" runat="server" />
                </asp:Panel>
                <asp:Panel ID="pnlContributions" runat="server">
                    <Rock:Grid ID="gContributions" runat="server" DisplayType="Light">
                        <Columns>
                            <asp:BoundField DataField="PersonFullName" HeaderText="Name" />
                            <Rock:RockLiteralField ID="lAddress" HeaderText="Address" />
                            <Rock:DateTimeField DataField="DateTime" HeaderText="Date" />
                            <Rock:CurrencyField DataField="Amount" HeaderText="Amount" />
                        </Columns>
                    </Rock:Grid>
                </asp:Panel>
            </div>
            <div class="col-md-4">
                <asp:Panel ID="pnlComments" runat="server">
                    <Rock:NoteContainer ID="notesCommentsTimeline" runat="server" UsePersonIcon="true" AddAllowed="true" />
                </asp:Panel>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
