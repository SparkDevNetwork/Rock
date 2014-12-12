<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PackageDetail.ascx.cs" Inherits="RockWeb.Blocks.Store.PackageDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="packagedetail">
            <asp:Image ID="imgPackageImage" runat="server" CssClass="packagedetail-image" />

            <h1><asp:Literal id="lPackageName" runat="server" /> <small>by <asp:Literal ID="lVendorName" runat="server" /></small></h1>

            <h4>Package Description</h4>
            <p class="margin-b-lg">
                <asp:Literal ID="lPackageDescription" runat="server" />
            </p>

            <h4><asp:Literal ID="lLatestVersionLabel" runat="server" /></h4>
            <p class="margin-b-lg">
                <asp:Literal ID="lLatestVersionDescription" runat="server" />
            </p>

            <h4>Additional Information</h4>
            <div class="row">
                <div class="col-md-3">
                    <strong>Last Updated</strong><br />
                    <asp:Literal ID="lLastUpdate" runat="server" />
                </div>
                <div class="col-md-3">
                    <strong>Required Rock Version</strong><br />
                    <asp:Literal ID="lRequiredRockVersion" runat="server" />
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
