﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RockUpdate.ascx.cs" Inherits="RockWeb.Blocks.Core.RockUpdate" %>
<%@ Import namespace="Rock" %>
<style>
    /* This is here because it prevents the contents from jumping around when/if
        the user clicks the release notes and the content expands the height of the
        page enough to need the scrollbar.
    */
    html {overflow-y: scroll;}
</style>
<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-cloud-download"></i> Rock Update</h1>
            </div>
            <div class="panel-body">
                 <Rock:NotificationBox ID="nbVersionIssue" runat="server" NotificationBoxType="Danger" Visible="false">
                     <h2><i class="fa fa-exclamation-triangle"></i> .NET Framework Update Required</h2>
                     <p>As of Rock McKinley v6, Rock requires Microsoft .NET Framework 4.5.2 or greater on the hosting server.
                        This framework version was released by Microsoft on May 5th, 2014.</p>
                 </Rock:NotificationBox>
                 <asp:Panel ID="pnlNoUpdates" runat="server">
                    <div class="well well-message">
                        <h1>Everything Is Shipshape</h1>
                        <i class="fa fa-anchor"></i>
                        <p>You run a tight ship, there is nothing to update since <asp:Literal id="lNoUpdateVersion" runat="server"/>. Check back soon as we're working hard on something amazing or
                           check out the <a href="http://www.rockrms.com/Rock/ReleaseNotes">release notes</a>.
                        </p>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlUpdatesAvailable" Visible="false" runat="server">
                    <div class="well well-message well-message-info">
                        <h1>New Pieces Available</h1>
                        <i class="fa fa-puzzle-piece"></i>
                        <p>We’ve expanded the puzzle, let’s get you up-to-date.</p>
                    </div>

                    <Rock:NotificationBox runat="server" Title="Remember..." NotificationBoxType="Warning" ID="nbRepoWarning" Visible="false">
                        You're using a <b>beta</b> or <b>alpha</b> update repository.</Rock:NotificationBox>

                    <Rock:NotificationBox runat="server" Title="Note:" NotificationBoxType="Warning" ID="nbBackupMessage">
                        We <em>strongly urge</em> you to backup your database and website before updating Rock.
                        The changes that are made during the update process can’t be undone.
                        Also, be patient when updating. It takes anywhere from a few seconds
                        to several minutes depending on the update size and your download speed.</Rock:NotificationBox>
                </asp:Panel>

                <!-- Early Access Messages -->
                <div class="well">
                    <div class="row margin-b-lg">
                        <div class="col-sm-3 col-md-2 margin-b-md">
                            <Rock:HighlightLabel runat="server" ID="hlblEarlyAccess" LabelType="Warning" Text="Early Access: Not Enabled" CssClass="padding-all-sm"></Rock:Highlightlabel>
                        </div>
                        <div class="col-sm-9 col-md-10">
                            <!-- Early Access Not Enabled -->
                            <asp:Panel runat="server" ID="pnlEarlyAccessNotEnabled" Visible="true">
                                <p>
                                    Community Contributors have early access to major releases of Rock. Find out
                                <a href="http://www.rockrms.com/earlyaccess">how to get early access to releases as a Community Contributor</a>.

                                    If you are already a Community Contributor and are having trouble with your access,
                                    <asp:Hyperlink ID="btnIssues" runat="server">let us know so we can resolve the problem</asp:Hyperlink>.
                                </p>
                            </asp:Panel>
                            <!-- Early Access is Enabled -->
                            <asp:Panel runat="server" ID="pnlEarlyAccessEnabled" Visible="false"> Thank you for being a Community Contributor! <a href="http://www.rockrms.com/earlyaccess">Learn more about the Early Access program</a>.
                            </asp:Panel>
                        </div>
                    </div>
                </div>

                <asp:Panel ID="pnlUpdates" runat="server" Visible="false">

                    <div class="row margin-b-md">
                        <div class="col-md-6 margin-v-sm">
                            <asp:Literal ID="lRockVersion" runat="server"></asp:Literal>
                        </div>
                        <div class="col-md-6">
                            <div class="pull-right">
                                <Rock:RockCheckBox ID="cbIncludeStats" runat="server" Checked="true" Visible="false" Text="Include impact statistics" Help="Having high level usage stats are very valuable to us.  Sharing them with us allows us to celebrate the impact Rock is making.  You can read about our <a href='http://www.rockrms.com/page/318'>impact statistics here</a>." />
                            </div>
                        </div>
                    </div>

                    <asp:Repeater ID="rptPackageVersions" runat="server" Visible="True"  OnItemDataBound="rptPackageVersions_ItemDataBound" OnItemCommand="rptPackageVersions_ItemCommand">
                            <ItemTemplate>
                                <div id="divPanel" runat="server" class="panel">
                                    <div class="panel-heading">
                                        <h3 class="panel-title"><asp:Literal runat="server" Text='<%# GetRockVersion( Eval( "Version" ) )%>' /></h3>
                                    </div>
                                    <div class="panel-body">
                                        <div class="row">
                                            <div class="col-md-2">
                                                <asp:LinkButton ID="lbInstall" runat="server" CssClass="btn" CommandName="Install" CommandArgument='<%# Eval( "Version" ) %>'><i class="fa fa-download"></i> Install</asp:LinkButton>
                                            </div>
                                            <div class="col-md-10">
                                                <asp:Literal ID="litPackageDescription" runat="server" Text='<%# Eval( "Description" ) %>'></asp:Literal>

                                                <div class="releasenotes">
                                                    <div class="btn btn-sm btn-default margin-v-sm js-releasenote">Release Notes <i class="fa fa-caret-down"></i></div>

                                                    <div class="releasenotes-body" style="display: none">
                                                        <asp:Literal ID="litReleaseNotes" runat="server" Text='<%# ConvertToHtmlLiWrappedUl( Eval( "ReleaseNotes" ).ToStringSafe() ).ConvertCrLfToHtmlBr()  %>'></asp:Literal>
                                                    </div>
                                             </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </ItemTemplate>
                    </asp:Repeater>
                </asp:Panel>

                <asp:Panel ID="pnlUpdateSuccess" runat="server" Visible="false">

                    <Rock:NotificationBox ID="nbMoreUpdatesAvailable" runat="server" NotificationBoxType="Info" Visible="false" Heading="More Updates Available! " Text="There are additional updates available."/>

                    <div class="well well-message well-message-success">
                        <h1>Eureka, Pay Dirt!</h1>
                        <i class="fa fa-exclamation-triangle"></i>
                        <p>Update completed successfully... You're now running <asp:Literal ID="lSuccessVersion" runat="server" /> .</p>

                        <div class="text-left margin-t-md">
                            <strong>Below is a summary of the new toys you have to play with...</strong>
                                <asp:Literal ID="nbSuccess" runat="server"></asp:Literal>
                        </div>

                        <button type="button" id="btn-restart" data-loading-text="Restarting..." class="btn btn-success">Restart</button>

                    </div>

                </asp:Panel>

                <asp:Panel ID="pnlError" runat="server" Visible="false">
                    <div class="well well-message well-message-danger">
                        <h1>Whoa... That Wasn't Suppose To Happen</h1>
                        <i class="fa fa-exclamation-circle"></i>
                        <p>An error occurred during the update process.</p>
                    </div>

                    <asp:Literal ID="lMessage" runat="server"></asp:Literal>

                    <Rock:NotificationBox ID="nbErrors" runat="server" NotificationBoxType="Danger" Heading="Here's what happened..." />
                </asp:Panel>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>

<script>
    $(function () {
        $(".js-releasenote").on("click", function (event) {
            var $top = $(event.target).closest(".releasenotes");
            $top.find("i").toggleClass("fa-caret-up").toggleClass("fa-caret-down");
            $top.find(".releasenotes-body").slideToggle(500);
        });
    });
</script>