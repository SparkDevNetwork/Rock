<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PageProperties.ascx.cs" Inherits="RockWeb.Blocks.Administration.PageProperties" %>

<asp:UpdatePanel ID="upPanel" runat="server">

    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
            <div id="pnlHeading" runat="server" class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lIcon" runat="server" />
                    <asp:Literal ID="lTitle" runat="server" /></h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlblSiteName" runat="server" />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <asp:Panel ID="pnlBody" runat="server" CssClass="panel-body">

                <asp:HiddenField ID="hfPageId" runat="server" />

                <!-- Edit Page -->
                <div id="pnlEditDetails" runat="server">
                    <asp:PlaceHolder ID="phContent" runat="server">

                        <ul class="nav nav-pills margin-b-md">
                            <asp:Repeater ID="rptProperties" runat="server">
                                <ItemTemplate>
                                    <li class='<%# GetTabClass(Container.DataItem) %>'>
                                        <asp:LinkButton ID="lbProperty" runat="server" Text='<%# Container.DataItem %>' OnClick="lbProperty_Click" CausesValidation="false">
                                        </asp:LinkButton>
                                    </li>
                                </ItemTemplate>
                            </asp:Repeater>
                        </ul>

                        <div class="tabContent">

                            <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                            <asp:Panel ID="pnlBasicProperty" runat="server" Visible="true">
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:PagePicker ID="ppParentPage" runat="server" Label="Parent Page" Required="false" ShowSelectCurrentPage="false" />
                                        <Rock:DataTextBox ID="tbPageName" runat="server" SourceTypeName="Rock.Model.Page, Rock" PropertyName="InternalName" Required="true"
                                            Help="The internal page name to use when administering this page" />
                                        <Rock:DataTextBox ID="tbPageTitle" runat="server" SourceTypeName="Rock.Model.Page, Rock" PropertyName="PageTitle"
                                            Help="The page title to display in menus, breadcrumbs and page headings." />
                                        <Rock:DataTextBox ID="tbBrowserTitle" runat="server" SourceTypeName="Rock.Model.Page, Rock" PropertyName="BrowserTitle"
                                            Help="The page title to display in the browser." />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:RockDropDownList ID="ddlSite" runat="server" Label="Site" Help="The Site that the page should belong to." AutoPostBack="true" OnSelectedIndexChanged="ddlSite_SelectedIndexChanged" />
                                        <Rock:DataDropDownList ID="ddlLayout" runat="server" SourceTypeName="Rock.Model.Page, Rock" PropertyName="Layout" Required="true" />
                                        <Rock:RockCheckBox ID="cbMenuIcon" runat="server" Label="Show Icon" Text="Yes" />
                                        <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.Page, Rock" PropertyName="IconCssClass" Label="Icon CSS Class" />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-12">
                                        <Rock:DataTextBox ID="tbDescription" runat="server" TextMode="MultiLine" Rows="3" SourceTypeName="Rock.Model.Page, Rock" PropertyName="Description"
                                            Help="The description of the page to include as a meta tag for the page" />
                                    </div>
                                </div>

                                <Rock:PanelWidget ID="wpPageAttributes" runat="server" Title="Page Attribute Values">
                                    <Rock:DynamicPlaceholder ID="phPageAttributes" runat="server"></Rock:DynamicPlaceholder>
                                </Rock:PanelWidget>
                            </asp:Panel>

                            <asp:Panel ID="pnlDisplaySettings" runat="server" Visible="false">
                                <fieldset>
                                    <h4>Page</h4>
                                    <Rock:RockCheckBox ID="cbPageTitle" runat="server" Text="Show Title on Page" Help="If supported by the layout, should the title be displayed when viewing this page?" />
                                    <Rock:RockCheckBox ID="cbPageBreadCrumb" runat="server" Text="Show Breadcrumbs on Page" Help="If supported by the layout, should breadcrumbs (links to parent pages) be displayed when viewing this page?" />
                                    <Rock:RockCheckBox ID="cbPageIcon" runat="server" Text="Show Icon on Page" Help="If supported by the layout, should the page icon be displayed when viewing this page?" />
                                    <Rock:RockCheckBox ID="cbPageDescription" runat="server" Text="Show Description on Page" Help="If supported by the layout, should the page description be displayed when viewing this page?" />
                                </fieldset>
                                <fieldset>
                                    <h4>Menu</h4>
                                    <Rock:DataDropDownList ID="ddlMenuWhen" runat="server" Label="Display When" SourceTypeName="Rock.Model.Page, Rock" PropertyName="DisplayInNavWhen" />
                                    <Rock:RockCheckBox ID="cbMenuDescription" runat="server" Text="Show Description" Help="If supported by the menu, should this page's description be included with its title in the menu?" />
                                    <Rock:RockCheckBox ID="cbMenuChildPages" runat="server" Text="Show Child Pages" Help="Should the child pages be displayed in the menu?" />
                                </fieldset>
                                <fieldset>
                                    <h4>Breadcrumbs</h4>
                                    <Rock:RockCheckBox ID="cbBreadCrumbName" runat="server" Text="Show Name in Breadcrumb" Help="Should this page's name be displayed in the breadcrumb trail when viewing this page or a child page?" />
                                    <Rock:RockCheckBox ID="cbBreadCrumbIcon" runat="server" Text="Show Icon in Breadcrumb" Help="Should this page's icon be displayed in the breadcrumb trail when viewing this page or a child page?" />
                                </fieldset>
                            </asp:Panel>

                            <asp:Panel ID="pnlAdvancedSettings" runat="server" Visible="false">
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:RockCheckBox ID="cbRequiresEncryption" runat="server" Text="Force SSL" />
                                        <Rock:RockCheckBox ID="cbEnableViewState" runat="server" Text="Enable ViewState" />
                                        <Rock:RockCheckBox ID="cbIncludeAdminFooter" runat="server" Text="Allow Configuration" />
                                        <Rock:RockCheckBox ID="cbAllowIndexing" runat="server" Text="Allow Indexing" />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:DataTextBox ID="tbBodyCssClass" runat="server" SourceTypeName="Rock.Model.Page, Rock" PropertyName="BodyCssClass" Label="Body CSS Class"
                                            Help="The CSS class to add to the body tag (if theme and layout supports it)." />
                                        <fieldset>
                                            <Rock:NotificationBox ID="nbPageRouteWarning" runat="server" />
                                            <Rock:RockTextBox ID="tbPageRoute" runat="server" TextMode="MultiLine" Rows="3" Label="Page Routes" Help="A unique, friendly route name for the page (e.g. 'Login' or 'Community/GetInvolved')" />
                                            <asp:CustomValidator ID="cvPageRoute" runat="server" ControlToValidate="tbPageRoute" OnServerValidate="cvPageRoute_ServerValidate" Display="None" ErrorMessage="Invalid Route(s)" />
                                        </fieldset>
                                        <asp:PlaceHolder ID="phContextPanel" runat="server">
                                            <fieldset>
                                                <h4>Context Parameters</h4>
                                                <p>
                                                    There are one or more blocks on this page that can load content based on a 'context' parameter.  
                                Please enter the route parameter name or query string parameter name that will contain the id for 
                                each of the objects below.
                                                </p>
                                                <asp:PlaceHolder ID="phContext" runat="server"></asp:PlaceHolder>
                                            </fieldset>
                                        </asp:PlaceHolder>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-12">
                                        <Rock:CacheabilityPicker ID="cpCacheSettings" runat="server" Label="" />
                                    </div>
                                    <div class="col-md-12">
                                        <Rock:RockCheckBox runat="server" ID="cbEnableRateLimiting" CssClass="js-rate-limit-checkbox" Label="Rate Limiting Enable" Help="Rate Limiting restricts the number of requests from each IP Address, helping to protect pages from bot spam." />
                                    </div>
                                    <asp:Panel runat="server" ID="pnlRateLimitingSettings" CssClass="js-rate-limit-settings d-none">
                                        <div class="col-md-6">
                                            <Rock:NumberBox runat="server" ID="nbRequestPerPeriod" Label="Max Request Per Period" />
                                        </div>
                                        <div class="col-md-6">
                                            <Rock:NumberBox runat="server" ID="nbRateLimitPeriod" Label="Rate Limit Period" AppendText="seconds" />
                                        </div>
                                    </asp:Panel>
                                </div>
                                <div class="row">
                                    <div class="col-md-12">
                                        <Rock:CodeEditor ID="ceHeaderContent" runat="server" Label="Header Content" EditorMode="Lava" EditorTheme="Rock" EditorHeight="400"
                                            Help="Additional HTML content to include in the &amp;lt;head&amp;gt; section of the rendered page." />
                                    </div>
                                </div>
                            </asp:Panel>

                        </div>

                    </asp:PlaceHolder>

                    <asp:Panel ID="pnlEditModeActions" runat="server" CssClass="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </asp:Panel>
                </div>

                <!-- Read Only summary of Page -->
                <fieldset id="fieldsetViewDetails" runat="server">

                    <asp:Literal ID="lblActiveHtml" runat="server" />

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lblMainDetailsCol1" runat="server" />
                        </div>
                        <div class="col-md-6">
                            <dl class="margin-b-md">
                                <dt>Median Time To Serve</dt>
                                <dd>
                                    <asp:Literal runat="server" ID="lMedianTime" />
                                    <asp:LinkButton runat="server" ID="lbMedianTimeDetails" CssClass="small" OnClick="lbMedianTimeDetails_Click">
                                        Details
                                    </asp:LinkButton>
                                </dd>
                            </dl>
                            <asp:Literal ID="lblMainDetailsCol2" runat="server" />
                        </div>
                    </div>

                    <asp:Panel ID="pnlReadOnlyModeActions" runat="server" CssClass="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="m" ToolTip="Alt+m" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" />
                        <div class="pull-right">
                            <a title="Child Pages" class="btn btn-default btn-sm btn-square page-child-pages" runat="server" id="aChildPages"><i class="fa fa-sitemap"></i></a>
                            <asp:LinkButton ID="btnCopy" runat="server" ToolTip="Copy Page" CssClass="btn btn-default btn-sm btn-square" OnClick="btnCopy_Click"><i class="fa fa-clone"></i></asp:LinkButton>
                            <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-square btn-security" />
                        </div>
                    </asp:Panel>
                </fieldset>

                <Rock:ModalDialog ID="mdCopyPage" runat="server" ValidationGroup="vgCopyPage" Title="Copy Page" OnSaveClick="mdCopyPage_SaveClick" SaveButtonText="Copy" Visible="false">
                    <Content>
                        <Rock:NotificationBox ID="mdCopyWarning" runat="server" NotificationBoxType="Warning" Text="Verify all the block setting's values because they are not duplicates but point to the exact same item. You may want to create copies of certain things like images, so block copies are not referencing the same items." />
                        <Rock:RockCheckBox ID="cbCopyPageIncludeChildPages" runat="server" Text="Include Child Pages" Checked="true" />
                    </Content>
                </Rock:ModalDialog>

                <Rock:ModalDialog ID="mdDeleteModal" runat="server" ValidationGroup="vgDeleteModal" Title="Are you sure?" OnSaveClick="mdDeleteModal_DeleteClick" SaveButtonText="Delete" Visible="false">
                    <Content>
                        <p>Are you sure you want to delete this page?</p>
                        <Rock:RockCheckBox ID="cbDeleteInteractions" runat="server" Text="Delete any interactions for this page" Checked="true" />
                    </Content>
                </Rock:ModalDialog>

            </asp:Panel>
        </asp:Panel>
        <script>
            Sys.Application.add_load(function () {
                $('#<%=tbPageName.ClientID%>').on('blur', function () {
                    var isNewPage = $('#<%=hfPageId.ClientID%>').val() == '0';
                    if (isNewPage) {
                        // Default the Page Title and Browser Title to the Internal Name if this is a new page and they aren't filled in with anything yet
                        var $tbPageName = $('#<%=tbPageName.ClientID%>');
                        var $tbPageTitle = $('#<%=tbPageTitle.ClientID%>');
                        var $tbBrowserTitle = $('#<%=tbBrowserTitle.ClientID%>');
                        if ($tbPageTitle.val() == '') {
                            $tbPageTitle.val($tbPageName.val());
                        }

                        if ($tbBrowserTitle.val() == '') {
                            $tbBrowserTitle.val($tbPageName.val());
                        }
                    }
                });

                // toggle rate limiting fields
                $('.js-rate-limit-checkbox').on('click', function (e) {

                    showHideRateLimitFields($(this));
                });

                function showHideRateLimitFields(rateLimitCheckbox) {
                    var isChecked = rateLimitCheckbox.prop('checked');

                    var panel = $(".js-rate-limit-settings")
                    if (isChecked) {
                        panel.removeClass('d-none');
                    } else {
                        panel.addClass('d-none');
                    }
                }

                showHideRateLimitFields($('.js-rate-limit-checkbox'));
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
