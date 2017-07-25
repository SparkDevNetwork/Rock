<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ShortLink.ascx.cs" Inherits="RockWeb.Blocks.Administration.ShortLink" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
        <Rock:NotificationBox ID="nbError" runat="server" NotificationBoxType="Danger" Visible="false" />

        <div class="row">
            <div class="col-sm-6">
                <Rock:RockDropDownList ID="ddlSite" runat="server" Label="Site" Required="true" AutoPostBack="true" OnSelectedIndexChanged="ddlSite_SelectedIndexChanged"
                    RequiredErrorMessage="Site is Required" Help="The site to use for the short link." />
                <asp:HiddenField ID="hfSiteUrl" runat="server" />
            </div>
            <div class="col-sm-6">
                <Rock:RockTextBox ID="tbToken" runat="server" Label="Token" Required="true" RequiredErrorMessage="A Token is Required" Help="The token to use for the short link. Must be unique" />
            </div>
        </div>

        <div class="row">
            <div class="col-sm-12">
                <Rock:RockTextBox ID="tbUrl" runat="server" Label="URL" Required="true" RequiredErrorMessage="A URL is Required" Help="The URL that short link will direct users to." />
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
