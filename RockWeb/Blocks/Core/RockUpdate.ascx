<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RockUpdate.ascx.cs" Inherits="RockWeb.Blocks.Core.RockUpdate" %>
<%@ Import namespace="Rock" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
    <div class="container">

        <!-- This should eventually be moved to a message center notification -->
        <div class="row">
            <div class="col-md-12">
                <asp:Literal ID="litRockVersion" runat="server"></asp:Literal>
                <div class="pull-right">
                    <Rock:HighlightLabel ID="hlUpdates" runat="server" LabelType="Danger" Visible="false" ToolTip="There are one or more updates available." Text="updates available" />
                </div>
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

            <asp:Repeater ID="rptPackageVersions" runat="server" Visible="True"  OnItemDataBound="rptPackageVersions_ItemDataBound" OnItemCommand="rptPackageVersions_ItemCommand">
                    <ItemTemplate>
                        <div id="divPanel" runat="server" class="panel">
                            <div class="panel-heading">
                                <h3 class="panel-title"><asp:Literal runat="server" Text='<%# Eval( "Title" ) %>' /> <small><asp:Literal runat="server" Text='<%# Eval( "Version" ) %>' /></small></h3>
                            </div>
                            <div class="panel-body">
                                <div class="row">
                                    <div class="col-md-3">
                                        <asp:LinkButton ID="lbInstall" runat="server" Text='<%# Eval( "Version", "<i class=\"fa fa-download\"></i> Install v{0}" ) %>'
                                            CssClass="btn" CommandName="Install" CommandArgument='<%# Eval( "Version" ) %>' />
                                    </div>
                                    <div class="col-md-9">
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

