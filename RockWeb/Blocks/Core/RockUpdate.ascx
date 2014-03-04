<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RockUpdate.ascx.cs" Inherits="RockWeb.Blocks.Core.RockUpdate" %>
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
    <div class="container">

        <!-- This should eventually be moved to a message center notification -->
        <div class="row margin-b-md">
            <div class="col-md-9">
                <asp:Literal ID="litRockVersion" runat="server"></asp:Literal>
            </div>
            <div class="col-md-3">
                <Rock:RockCheckBox ID="cbIncludeStats" runat="server" Checked="true" Visible="false" Text="Include impact statistics" Help="Having high level usage stats are very valuable to us.  Sharing them with us allows us to celebrate the impact Rock is making.  You can read about our <a href='http://www.rockrms.com/page/318'>impact statistics here</a>." />
            </div>
        </div>

        <div class="row">
            <div class="col-md-12">
                <asp:Literal ID="litMessage" runat="server"></asp:Literal>
                <Rock:NotificationBox ID="nbSuccess" runat="server" NotificationBoxType="Success" Visible="false" Heading="<i class='fa fa-check-circle-o'></i> Success" />
                <Rock:NotificationBox ID="nbErrors" runat="server" NotificationBoxType="Danger" Visible="false" Heading="<i class='fa fa-frown-o'></i> Sorry..." />
            </div>
        </div>

        <div runat="server" id="divPackage" visible="false">

        <Rock:NotificationBox runat="server" Title="Note" NotificationBoxType="Warning">
            We recommend that you always take a backup of your database and website before updating Rock.
            The changes that are made during the update process can't be undone.
            Also, be patient when updating. An update can take anywhere from a few seconds
            to 10 minutes depending on the size and your download speed.</Rock:NotificationBox>

            <asp:Repeater ID="rptPackageVersions" runat="server" Visible="True"  OnItemDataBound="rptPackageVersions_ItemDataBound" OnItemCommand="rptPackageVersions_ItemCommand">
                    <ItemTemplate>
                        <div id="divPanel" runat="server" class="panel">
                            <div class="panel-heading">
                                <h3 class="panel-title"><asp:Literal runat="server" Text='<%# Eval( "Title" ) %>' /></h3>
                            </div>
                            <div class="panel-body">
                                <div class="row">
                                    <div class="col-md-2">
                                        <asp:LinkButton ID="lbInstall" runat="server" CssClass="btn" CommandName="Install" CommandArgument='<%# Eval( "Version" ) %>'><i class="fa fa-download"></i> Install</asp:LinkButton>
                                    </div>
                                    <div class="col-md-10">
                                        <asp:Literal ID="litPackageDescription" runat="server" Text='<%# Eval( "Description" ) %>'></asp:Literal>
                                        <div class="releasenotes">
                                            <div class="releasenotes-heading margin-v-md">
                                                <strong>
                                                    <asp:Label ID="lblReleaseNotes" runat="server" Text="Release Notes" />
                                                    <i class="fa fa-caret-right"></i>
                                                </strong>
                                            </div>
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
        </div>
    </div>
    </ContentTemplate>
</asp:UpdatePanel>

<script>
    $(function () {
        $(".releasenotes-heading").on("click", function (event) {
            var $top = $(event.target).closest(".releasenotes");
            $top.find("i").toggleClass("fa-caret-right").toggleClass("fa-caret-down");
            $top.find(".releasenotes-body").slideToggle(500);
        });
    });
</script>