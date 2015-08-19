<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SiteDetail.ascx.cs" Inherits="RockWeb.Blocks.Cms.SiteDetail" %>

<asp:UpdatePanel ID="upSites" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDeleteConfirm" runat="server" CssClass="panel panel-body" Visible="false">
            <Rock:NotificationBox ID="nbDeleteConfirm" runat="server" NotificationBoxType="Warning">
                        Deleting a site will delete all the layouts and pages associated with the site. Are you sure you want to delete the site?
            </Rock:NotificationBox>
            <asp:LinkButton ID="btnDeleteConfirm" runat="server" Text="Confirm Delete" CssClass="btn btn-danger" OnClick="btnDeleteConfirm_Click" />
            <asp:LinkButton ID="btnDeleteCancel" runat="server" Text="Cancel" CssClass="btn btn-primary" OnClick="btnDeleteCancel_Click" />

        </asp:Panel>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <asp:HiddenField ID="hfSiteId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-desktop"></i>
                    <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <div id="pnlEditDetails" runat="server">

                    <Rock:NotificationBox ID="nbDefaultPageNotice" runat="server" NotificationBoxType="Info" Title="Note" Visible="false"
                        Text="If a Default Page is not specified, Rock will automatically create a new page at the root and set it as the default page for this new site." />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbSiteName" runat="server" SourceTypeName="Rock.Model.Site, Rock" PropertyName="Name" />
                        </div>
                        <div class="col-md-6">
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.Site, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataDropDownList ID="ddlTheme" runat="server" SourceTypeName="Rock.Model.Site, Rock" PropertyName="Theme" Help="The theme that should be used for the site.  Themes contain specific layouts and css styling that controls how a site and it's pages will look" />
                            <Rock:PagePicker ID="ppDefaultPage" runat="server" Label="Default Page" PromptForPageRoute="true" Help="The page and route that will be used whenever a specific page or page route is not provided." />
                            <Rock:PagePicker ID="ppLoginPage" runat="server" Label="Login Page" Required="false" PromptForPageRoute="true" Help="The page that user will be redirected to when they request a page that requires them to login." />
                            <Rock:PagePicker ID="ppCommunicationPage" runat="server" Label="Communication Page" Required="false" PromptForPageRoute="true" Help="The page that user will be redirected to when creating a new communication." />
                            <Rock:PagePicker ID="ppRegistrationPage" runat="server" Label="Registration Page" Required="false" PromptForPageRoute="true" Help="The page that user will be redirected to when they request to register for a group." />
                            <Rock:PagePicker ID="ppPageNotFoundPage" runat="server" Label="404 Page" Required="false" PromptForPageRoute="true" Help="Page to use instead of the server's 404 message." />
                        </div>
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbSiteDomains" runat="server" SourceTypeName="Rock.Model.Site, Rock" PropertyName="SiteDomains" TextMode="MultiLine" LabelTextFromPropertyName="false" Label="Domain(s)" Help="A comma delimited list of domain values that are associated with this site.  These values are used by Rock to load the correct site whenever a specific page or route is not provided in the url. Rock will determine the site to use by finding the first site with a domain value that is contained by the current request's hostname in the url.  It will then display that site's default page" />
                            <Rock:DataTextBox ID="tbErrorPage" runat="server" SourceTypeName="Rock.Model.Site, Rock" PropertyName="ErrorPage" Help="The url that user will be redirected to if an error occurs on site" />
                            <Rock:DataTextBox ID="tbGoogleAnalytics" runat="server" SourceTypeName="Rock.Model.Site, Rock" PropertyName="GoogleAnalyticsCode" Help="Optional Google Analytics Code.  If specified, the Google Analytics script with this code will be added to every page rendered for this site." />
                            <Rock:RockCheckBox ID="cbEnableMobileRedirect" runat="server" Label="Enable Mobile Redirect" AutoPostBack="true" OnCheckedChanged="cbEnableMobileRedirect_CheckedChanged" CausesValidation="false" />
                            <Rock:PagePicker ID="ppMobilePage" runat="server" Label="Mobile Page" Required="false" PromptForPageRoute="false" Help="The page that user will be redirected to if accessing site from a mobile device." />
                            <Rock:DataTextBox ID="tbExternalURL" runat="server" SourceTypeName="Rock.Model.Site, Rock" PropertyName="ExternalUrl" Help="If user should be redirected to an external URL when accessing this site from a mobile device, enter the URL here."  />
                            <Rock:RockCheckBox ID="cbRedirectTablets" runat="server" Label="Redirect Tablets" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </div>

                <fieldset id="fieldsetViewDetails" runat="server">
                    <p class="description">
                        <asp:Literal ID="lSiteDescription" runat="server"></asp:Literal>
                    </p>

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <div class="row">
                        <div class="col-md-12">
                            <asp:Literal ID="lblMainDetails" runat="server" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="m" Text="Edit" CssClass="btn btn-primary" CausesValidation="false" OnClick="btnEdit_Click" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" CausesValidation="false" OnClick="btnDelete_Click" />
                    </div>

                </fieldset>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>

