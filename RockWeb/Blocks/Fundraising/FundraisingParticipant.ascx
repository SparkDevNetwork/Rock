<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FundraisingParticipant.ascx.cs" Inherits="RockWeb.Blocks.Fundraising.FundraisingParticipant" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server">
            <%-- Main Panel --%>
            <asp:Panel ID="pnlMain" runat="server">
                <asp:HiddenField ID="hfGroupId" runat="server" />
                <asp:HiddenField ID="hfGroupMemberId" runat="server" />
                <asp:HiddenField ID="hfActiveTab" runat="server" />

                <div class="row">
                    <div class="col-md-4 margin-t-md">
                        <asp:Image ID="imgOpportunityPhoto" runat="server" CssClass="title-image img-responsive" />
                        <asp:LinkButton ID="btnMainPage" runat="server" CssClass="btn btn-primary btn-block margin-t-md" Text="Main Page" OnClick="btnMainPage_Click" />
                        <asp:LinkButton ID="btnEditProfile" runat="server" CssClass="btn btn-default btn-sm btn-block margin-t-sm" Text="Edit Profile" OnClick="btnEditProfile_Click" />
                    </div>
                    <div class="col-md-8">

                        <div class="pull-right">
                            <button id="btnCopyToClipboard" runat="server"
                                data-toggle="tooltip" data-placement="top" data-trigger="hover" data-delay="250" title="Copy Link to Clipboard"
                                class="btn btn-default btn-xs btn-copy-to-clipboard cursor-pointer"
                                onclick="$(this).attr('data-original-title', 'Copied').tooltip('show').attr('data-original-title', 'Copy Link to Clipboard');return false;">
                                <i class='fa fa-clipboard'></i>&nbsp;Copy Profile Link
                            </button>
                        </div>
                        <asp:Literal ID="lMainTopContentHtml" runat="server" />

                        <Rock:NotificationBox ID="nbProfileWarning" runat="server" Text="Tip! A personal opportunity introduction a great way personalize your page. " Dismissable="true" NotificationBoxType="Success" Visible="false" />
                    </div>
                </div>

                <asp:Literal ID="lProgressHtml" runat="server" />

                <br />

                <ul id="tlTabList" runat="server" class="nav nav-pills margin-v-md">
                    <li id="liUpdatesTab" runat="server"><asp:LinkButton ID="btnUpdatesTab" runat="server" Text="Updates" OnClick="btnUpdatesTab_Click" /></li>
                    <li id="liContributionsTab" runat="server"><asp:LinkButton ID="btnContributionsTab" runat="server" Text="Contributions" OnClick="btnContributionsTab_Click" /></li>
                </ul>

                <asp:Panel ID="pnlUpdatesComments" CssClass="row" runat="server">
                    <div class="col-md-8">
                        <asp:Literal ID="lUpdatesContentItemsHtml" runat="server" />
                    </div>
                    <div id="pnlComments" runat="server" class="col-md-4">
                        <label>Comments</label>
                        <Rock:NoteContainer ID="notesCommentsTimeline" runat="server" UsePersonIcon="true" AddAllowed="true" DisplayType="Full" />
                        <asp:Literal ID="lNoLoginNoCommentsYet" runat="server" ><br /><i>No comments yet.</i></asp:Literal>
                        <asp:LinkButton ID="btnLoginToComment" CssClass="btn btn-link pull-right" runat="server" Text="Login to Comment" OnClick="btnLoginToComment_Click" />
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlContributions" runat="server">
                    <Rock:Grid ID="gContributions" runat="server" DisplayType="Light" OnRowDataBound="gContributions_RowDataBound">
                        <Columns>
                            <Rock:RockLiteralField ID="lPersonName" HeaderText="Name" />
                            <Rock:RockLiteralField ID="lAddress" HeaderText="Address" />
                            <Rock:DateTimeField DataField="TransactionDateTime" HeaderText="Date" ItemStyle-HorizontalAlign="Left" />
                            <Rock:CurrencyField DataField="TotalAmount" HeaderText="Amount" HeaderStyle-HorizontalAlign="Right" />
                        </Columns>
                    </Rock:Grid>
                </asp:Panel>


            </asp:Panel>

            <%-- Edit Preferences Panel --%>
            <asp:Panel ID="pnlEditPreferences" runat="server">

                <div class="margin-b-lg">

                    <h1>
                        <asp:Literal ID="lProfileTitle" runat="server" Text="Profile..." /></h1>

                    <asp:Literal ID="lDateRange" runat="server" Text="Dates..." />

                </div>

                <div class="row">

                    <div class="col-md-3">
                        <Rock:ImageEditor ID="imgProfilePhoto" runat="server" Label="Photo" ValidationGroup="vgProfileEdit" BinaryFileTypeGuid="03BD8476-8A9F-4078-B628-5B538F967AFC" />
                    </div>

                    <div class="col-md-9">
                        <asp:PlaceHolder ID="phGroupMemberAttributes" runat="server" />
                        <asp:PlaceHolder ID="phPersonAttributes" runat="server" />
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnSaveEditPreferences" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" ValidationGroup="vgProfileEdit" OnClick="btnSaveEditPreferences_Click" />
                    <asp:LinkButton ID="btnCancelEditPreferences" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" ValidationGroup="vgProfileEdit" CausesValidation="false" OnClick="btnCancelEditPreferences_Click" />
                </div>

            </asp:Panel>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
