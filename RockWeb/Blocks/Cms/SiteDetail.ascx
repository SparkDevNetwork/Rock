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
                <div class="panel-labels">
                    <asp:Literal ID="lVisitSite" runat="server" />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <asp:ValidationSummary ID="vsSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <div id="pnlEditDetails" runat="server">

                    <Rock:NotificationBox ID="nbDefaultPageNotice" runat="server" NotificationBoxType="Info" Title="Note" Visible="false"
                        Text="If a Default Page is not specified, Rock will automatically create a new page at the root and set it as the default page for this new site." />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbSiteName" runat="server" SourceTypeName="Rock.Model.Site, Rock" PropertyName="Name" />
                        </div>
                        <div class="col-md-6">
                               <Rock:RockCheckBox ID="cbIsActive" runat="server" CssClass="js-isactivegroup" Text="Active" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.Site, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataDropDownList ID="ddlTheme" runat="server" SourceTypeName="Rock.Model.Site, Rock" PropertyName="Theme" Help="The theme that should be used for the site.  Themes contain specific layouts and css styling that controls how a site and its pages will look" />
                            <Rock:PagePicker ID="ppDefaultPage" runat="server" Label="Default Page" PromptForPageRoute="true" Help="The page and route that will be used whenever a specific page or page route is not provided." />
                            <Rock:PagePicker ID="ppLoginPage" runat="server" Label="Login Page" Required="false" PromptForPageRoute="true" Help="The page that user will be redirected to when they request a page that requires them to login." />
                            <Rock:PagePicker ID="ppChangePasswordPage" runat="server" Label="Change Password Page" Required="false" PromptForPageRoute="true" Help="The page for changing a password for the site." />
                            <Rock:PagePicker ID="ppCommunicationPage" runat="server" Label="Communication Page" Required="false" PromptForPageRoute="true" Help="The page that user will be redirected to when creating a new communication." />
                            <Rock:PagePicker ID="ppRegistrationPage" runat="server" Label="Group Registration Page" Required="false" PromptForPageRoute="true" Help="The page that user will be redirected to when they request to register for a group." />
                            <Rock:PagePicker ID="ppPageNotFoundPage" runat="server" Label="404 Page" Required="false" PromptForPageRoute="true" Help="Page to use instead of the server's 404 message." />
                        </div>
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbSiteDomains" runat="server" SourceTypeName="Rock.Model.Site, Rock" PropertyName="SiteDomains" TextMode="MultiLine" LabelTextFromPropertyName="false" Label="Domain(s)" Help="A list of domains that are associated with this site (list can be either comma delimited or each on a separate line).  These values are used by Rock to load the correct site whenever a specific page or route is not provided in the url. Rock will determine the site to use by finding the first site with a domain value that is contained by the current request's hostname in the url.  It will then display that site's default page." />
                            <Rock:DataTextBox ID="tbErrorPage" runat="server" SourceTypeName="Rock.Model.Site, Rock" PropertyName="ErrorPage" Help="The url that user will be redirected to if an error occurs on site" />
                            <Rock:DataTextBox ID="tbGoogleAnalytics" runat="server" SourceTypeName="Rock.Model.Site, Rock" PropertyName="GoogleAnalyticsCode" Help="Optional Google Analytics Code.  If specified, the Google Analytics script with this code will be added to every page rendered for this site." />
                            <Rock:RockCheckBox ID="cbRequireEncryption" runat="server" Label="Require Encryption" Help="Ensures that the site is loaded over SSL by redirecting to https." />
                            <Rock:RockCheckBox ID="cbEnableForShortening" runat="server" Label="Enabled for Shortening" Help="Should this site (and its first domain) be an available option when creating shortlinks?" />
                            <div class="row">
                                <div class="col-md-4 col-lg-3">
                                    <Rock:ImageUploader ID="imgSiteIcon" runat="server" Help="Commonly called a 'favicon', this image is used as a browser and app icon for your site. Recommended image size is 192x192. Rock will automatically create all the sizes required by various devices." Label="Site Icon" />
                                </div>
                                <div class="col-md-4 col-lg-3">
                                    <Rock:ImageUploader ID="imgSiteLogo" runat="server" Help="The site logo is used by certain themes to apply to the changes on the site. See the theme's documentation for information on sizing" 
                                        Label="Site Logo" />
                                </div>
                            </div>
                            
                        </div>
                    </div>

                    <Rock:PanelWidget ID="wpPageAttributes" runat="server" Title="Page Attributes" CssClass="site-page-attribute-panel">
                        <Rock:NotificationBox ID="NotificationBox1" runat="server" NotificationBoxType="Info" 
                            Text="Page Attributes apply to all of the pages of this site. Each page will have its own value for these attributes" />
                        <div class="grid">
                            <Rock:Grid ID="gPageAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Page Attribute">
                                <Columns>
                                    <Rock:ReorderField />
                                    <Rock:RockBoundField DataField="Name" HeaderText="Attribute" />
                                    <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                    <Rock:BoolField DataField="IsRequired" HeaderText="Required" />
                                    <Rock:EditField OnClick="gPageAttributes_Edit" />
                                    <Rock:DeleteField OnClick="gPageAttributes_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpAdvancedSettings" runat="server" Title="Advanced Settings">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbEnableMobileRedirect" runat="server" Label="Enable Mobile Redirect" AutoPostBack="true" OnCheckedChanged="cbEnableMobileRedirect_CheckedChanged" CausesValidation="false" />
                                <Rock:PagePicker ID="ppMobilePage" runat="server" Label="Mobile Page" Required="false" PromptForPageRoute="false" Help="The page that user will be redirected to if accessing site from a mobile device." />
                                <Rock:DataTextBox ID="tbExternalURL" runat="server" SourceTypeName="Rock.Model.Site, Rock" Label="External URL" PropertyName="ExternalUrl" Help="If user should be redirected to an external URL when accessing this site from a mobile device, enter the URL here."  />
                                <Rock:RockCheckBox ID="cbRedirectTablets" runat="server" Label="Redirect Tablets" />
                                <Rock:RockCheckBox ID="cbEnablePageViews" runat="server" Label="Log Page Views" AutoPostBack="true" OnCheckedChanged="cbEnablePageViews_CheckedChanged" CausesValidation="false" />
                                <Rock:NumberBox ID="nbPageViewRetentionPeriodDays" runat="server" Label="Page View Retention Period" Help="The number of days to keep page views logged. Leave blank to keep page views logged indefinitely." />
                                <Rock:DataTextBox ID="tbAllowedFrameDomains" runat="server" SourceTypeName="Rock.Model.Site, Rock" PropertyName="AllowedFrameDomains" TextMode="MultiLine" LabelTextFromPropertyName="false" Label="Allowed Frame Domain(s)"
                                    Help="A space delimited list of domain values that are allowed to embed this site (via an iframe). The value you enter here will be used for the &lt;source&gt; as described in [Content-Security-Policy frame-ancestors directive](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Security-Policy/frame-ancestors#Syntax). Be sure to include your own server domain(s) in the list to prevent locking yourself out from modal use.  If left blank, Rock will inject properties into the HTTP Header which modern web browsers will use to prevent site embedding and it will use a frame-ancestors value of 'self'." />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbAllowIndexing" runat="server" Label="Allow Indexing" Help="This setting will enable or disable the pages of the site from being indexed." />
                                <Rock:RockCheckBox ID="cbEnableIndexing" runat="server" Label="Is Indexed" Help="Enables the Rock indexer for this site." AutoPostBack="true" OnCheckedChanged="cbEnableIndexing_CheckedChanged" />
                                <Rock:RockTextBox ID="tbIndexStartingLocation" runat="server" Label="Index Starting Location" Help="The URL for the Rock indexer to use to start crawling the site." />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12">
                                <Rock:CodeEditor ID="cePageHeaderContent" runat="server" Label="Page Header Content" Help="The content provided here will be added to each page's head section." EditorMode="Lava" EditorTheme="Rock" Height="300" />
                            </div>
                        </div>
                    </Rock:PanelWidget>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
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
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="m" ToolTip="Alt+m" Text="Edit" CssClass="btn btn-primary" CausesValidation="false" OnClick="btnEdit_Click" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" CausesValidation="false" OnClick="btnDelete_Click" />

                        <Rock:ModalAlert ID="mdThemeCompile" runat="server" />
                        <asp:LinkButton ID="btnCompileTheme" runat="server" Text="Compile Theme" CssClass="btn btn-link pull-right" CausesValidation="false" OnClick="btnCompileTheme_Click" />
                    </div>

                </fieldset>

            </div>

        </asp:Panel>

        <Rock:ModalDialog ID="dlgPageAttribute" runat="server" Title="Page Attributes" OnSaveClick="dlgPageAttribute_SaveClick" ValidationGroup="PageAttributes" Visible="false">
            <Content>
                <Rock:AttributeEditor ID="edtPageAttributes" runat="server" ShowActions="false" ValidationGroup="PageAttributes" />
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>

