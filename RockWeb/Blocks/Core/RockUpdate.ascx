<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RockUpdate.ascx.cs" Inherits="RockWeb.Blocks.Core.RockUpdate" %>
<%@ Import namespace="Rock" %>

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

        <Rock:NotificationBox runat="server" NotificationBoxType="Warning" Title="Note" 
            Text="We recommend that you always take a backup of your database and website before updating Rock. The changes that are made during the update process can't be undone." />

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
                                        <h4>Release Notes</h4>
                                        <asp:Literal ID="litReleaseNotes" runat="server" Text='<%# System.Web.HttpUtility.HtmlEncode( Eval( "ReleaseNotes" ) ).ConvertCrLfToHtmlBr()  %>'></asp:Literal>
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

