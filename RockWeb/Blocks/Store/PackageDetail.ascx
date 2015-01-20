<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PackageDetail.ascx.cs" Inherits="RockWeb.Blocks.Store.PackageDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-gift"></i> Store Item Detail</h1>

            </div>
            <div class="panel-body">
                <div class="packagedetail">
                    
                    <asp:Panel ID="pnlPackageDetails" runat="server">
                        <asp:Image ID="imgPackageImage" runat="server" CssClass="packagedetail-image" />

                        <div class="row">
                            <div class="col-md-3">
                            </div>
                            <div class="col-md-9">
                                <h1><asp:Literal id="lPackageName" runat="server" /> <small>by <asp:Literal ID="lVendorName" runat="server" /></small></h1>
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-3">
                            
                                <asp:Literal ID="lCost" runat="server" />

                                <p>
                                    <asp:LinkButton ID="lbInstall" runat="server" OnClick="lbInstall_Click" CssClass="btn btn-primary btn-install">Install</asp:LinkButton>
                                    <asp:Literal ID="lInstallNotes" runat="server" />
                                </p>

                                <p>
                                    <asp:Literal ID="lRatingSummary" runat="server" /> <asp:LinkButton ID="lbRate" Visible="false" runat="server">Rate</asp:LinkButton>
                                </p>

                                <p>
                                    <strong>Last Updated</strong><br />
                                    <asp:Literal ID="lLastUpdate" runat="server" />
                                </p>
                                <p>
                                    <strong>Required Rock Version</strong><br />
                                    <asp:Literal ID="lRequiredRockVersion" runat="server" />
                                </p>
                                <p>
                                    <strong>Author</strong><br />
                                    <asp:Literal ID="lAuthorInfo" runat="server" />
                                </p>
                                <p>
                                    <strong>Documentation</strong><br />
                                    <asp:Literal ID="lDocumenationLink" runat="server" />
                                </p>
                            </div>
                            <div class="col-md-9">

                                <h4>Package Description</h4>
                                <p class="margin-b-lg">
                                    <asp:Literal ID="lPackageDescription" runat="server" />
                                </p>

                                <p class="clearfix">
                                    <asp:LinkButton ID="lbPackageLink" runat="server" CssClass="btn btn-default btn-sm pull-right"><i class="fa fa-desktop"></i> Package Website</asp:LinkButton>
                                </p>

                                <asp:Literal ID="lVersionWarning" runat="server" />

                                <div class="row">
                                <asp:Repeater ID="rptScreenshots" runat="server">
                                    <ItemTemplate>
                                        <div class="col-sm-6">
                                            <a href='' class='package-screenshot'>
                                                <%# string.Format("<img src=\"{0}\" style=\"width: 100%\" class=\"margin-b-lg\" />", Eval("ImageUrl"))%>
                                            </a>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                                </div>

                                <asp:Literal ID="lScreenshots" runat="server" />

                                <h4><asp:Literal ID="lLatestVersionLabel" runat="server" /></h4>
                                <p class="margin-b-lg">
                                    <asp:Literal ID="lLatestVersionDescription" runat="server" />
                                </p>

                                <asp:Panel ID="pnlAdditionalVersions" runat="server">
                                    <p>
                                        <a href="#" class="btn btn-xs btn-default pull-right js-showmoreversions">More <i class="fa fa-chevron-down"></i></a>
                                    </p>
                                    <div class="packagedetail-additionalversions">
                                        <asp:Repeater ID="rptAdditionalVersions" runat="server">
                                            <ItemTemplate>
                                                <h4><%# Eval("VersionLabel")%></h4>
                                                <p class="margin-b-lg">
                                                    <%# Eval("Description")%>
                                                </p>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </div>
                                </asp:Panel>
                            </div>
                        </div>
                    </asp:Panel>

                    <asp:Panel ID="pnlError" runat="server" Visible="false">
                        <div class="alert alert-warning">
                            <h4>Store Currently Not Available</h4>
                            <p>We're sorry, the Rock Store is currently not available. Check back soon!</p>
                            <small><em><asp:Literal ID="lErrorMessage" runat="server" /></em></small>
                        </div>
                    </asp:Panel>
                </div>
            </div>

            <script type="text/javascript">
                Sys.Application.add_load(function () {
                    $(".js-showmoreversions").on("click", function () {
                        $('.packagedetail-additionalversions').slideToggle();
                        $(this).hide();
                        return false;
                    });

                    $(function () {
                        $("a.package-screenshot").fluidbox();
                    });
                });
        </script>

        <style>
            .fluidbox .fluidbox-ghost {
                    cursor: -webkit-zoom-in;
                    cursor: -moz-zoom-in;
                    cursor: zoom-in;
            }

            .fluidbox-opened .fluidbox-ghost {
                    cursor: -webkit-zoom-out;
                    cursor: -moz-zoom-out;
                    cursor: zoom-out;
            }
        </style>


       </asp:Panel>
        
    </ContentTemplate>
</asp:UpdatePanel>
