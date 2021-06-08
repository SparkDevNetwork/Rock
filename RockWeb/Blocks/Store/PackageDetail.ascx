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

                        <div class="panel-headerimage">
                            <asp:Image ID="imgPackageImage" runat="server" CssClass="packagedetail-image" />
                        </div>

                        <div class="row">
                            <div class="col-md-3">
                            </div>
                            <div class="col-md-9">
                                <h1>
                                    <asp:Literal id="lPackageName" runat="server" /> <small>by
                                        <asp:Literal ID="lVendorName" runat="server" />
                                    </small>
                                </h1>
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-3">

                                <asp:Literal ID="lCost" runat="server" />

                                <p>
                                    <asp:LinkButton ID="lbInstall" runat="server" OnClick="lbInstall_Click" CssClass="btn btn-primary btn-install">Install</asp:LinkButton>
                                    <asp:Literal ID="lInstallNotes" runat="server" />
                                </p>

                                <div class="clearfix margin-v-lg">
                                    <div class="rating text-center margin-b-sm" style="color:#ffc870;">
                                        <asp:Literal ID="lRatingSummary" runat="server" />
                                    </div>
                                    <asp:LinkButton ID="lbRate" Visible="true" runat="server" CssClass="btn btn-default btn-block" OnClick="lbRate_Click">
                                        Add a rating
                                        <span class="rating-bg">
                                            <i class="fas fa-star"></i>
                                            <i class="fas fa-star"></i>
                                            <i class="fas fa-star"></i>
                                            <i class="fas fa-star"></i>
                                            <i class="fas fa-star"></i>
                                        </span>
                                    </asp:LinkButton>
                                </div>

                                <dl>
                                    <dt>Last Updated</dt>
                                    <dd>
                                        <asp:Literal ID="lLastUpdate" runat="server" />
                                    </dd>
                                    <dt>Required Rock Version</dt>
                                    <dd>
                                        <asp:Literal ID="lRequiredRockVersion" runat="server" />
                                    </dd>
                                    <dt>Author</dt>
                                    <dd>
                                        <asp:Literal ID="lAuthorInfo" runat="server" />
                                    </dd>
                                    <dt>Documentation</dt>
                                    <dd>
                                        <asp:Literal ID="lDocumenationLink" runat="server" />
                                    </dd>
                                    <dt>Support</dt>
                                    <dd>
                                        <asp:Literal ID="lSupportLink" runat="server" />
                                    </dd>
                                </dl>
                            </div>
                            <div class="col-md-9">

                                <h4>Package Description</h4>
                                <p class="margin-b-lg">
                                    <asp:Literal ID="lPackageDescription" runat="server" />
                                </p>

                                <p class="clearfix margin-t-md">
                                    <asp:HyperLink ID="hlPackageLink" runat="server" CssClass="btn btn-default btn-sm pull-right"><i class="fa fa-desktop"></i> Package Website</asp:HyperLink>
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

                                <h2>
                                    <asp:Literal ID="lLatestVersionLabel" runat="server" /> <small>released
                                        <asp:Literal ID="lLatestVersionDate" runat="server" />
                                    </small>
                                </h2>
                                <p class="margin-b-lg">
                                    <asp:Literal ID="lLatestVersionDescription" runat="server" />
                                </p>

                                <h4>Reviews</h4>
                                <asp:Literal ID="lNoReviews" runat="server" Text="No reviews exist." />
                                <div class="row">
                                    <asp:Repeater ID="rptLatestVersionRatings" runat="server">
                                        <ItemTemplate>
                                            <div class="col-md-12 margin-b-lg">
                                                <div class="pull-left" style="width: 65px;">
                                                    <img src='<%# PersonPhotoUrl( Eval( "PersonAlias.Person.PhotoUrl" ).ToString() ) %>&width=50' class="img-circle" />
                                                </div>
                                                <div style="width: 100%;">
                                                    <div class="rating pull-left margin-r-sm" style="color:#ffc870;">
                                                        <%# FormatRating((int)Eval("Rating")) %>
                                                    </div>
                                                    <strong><%# Eval("PersonAlias.Person.FullName")%></strong>
                                                    <p class="margin-b-lg">
                                                        <%# FormatReviewText(Eval("Review").ToString())%>
                                                    </p>
                                                </div>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>

                                <asp:Panel ID="pnlAdditionalVersions" runat="server">
                                    <p>
                                        <a href="#" class="btn btn-xs btn-default pull-right js-showmoreversions">Additional Versions <i class="fa fa-chevron-down"></i></a>
                                    </p>
                                    <div class="packagedetail-additionalversions">
                                        <asp:Repeater ID="rptAdditionalVersions" runat="server">
                                            <ItemTemplate>
                                                <div class="clearfix">
                                                    <h4 class="pull-left margin-r-sm"><%# Eval("VersionLabel")%></h4>
                                                    <div class="rating pull-left margin-t-sm">
                                                        <%# FormatRating(Convert.ToInt32( GetRating((int)Eval("Id")))) %>
                                                    </div>
                                                    <small class="pull-right text-muted">released <%# Eval("AddedDate", "{0:MMM dd, yyyy}" )%></small>
                                                </div>

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