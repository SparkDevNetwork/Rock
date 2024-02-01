<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MobileApplicationDetail.ascx.cs" Inherits="RockWeb.Blocks.Mobile.MobileApplicationDetail" %>

<style>
    .mobile-app-preview {
        padding: 20px;
    }

        .mobile-app-preview img {
            width: 100%;
            height: auto;
            display: block;
            min-height: 50px;
            background-color: #ddd;
        }

    .mobile-app-icon {
        width: 100%;
        height: auto;
        display: block;
    }
</style>

<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbError" runat="server" NotificationBoxType="Danger" />
        <Rock:ModalAlert ID="mdWarning" runat="server" />
        <asp:Panel ID="pnlOverview" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h3 class="panel-title">
                    <i class="fa fa-mobile"></i>
                    <asp:Literal ID="ltAppName" runat="server" />
                </h3>
                <div class="panel-labels">
                    <span class="label label-default">Site Id:<asp:Literal ID="lSiteId" runat="server" /></span>
                    <asp:Literal ID="lLastDeployDate" runat="server" />
                </div>
            </div>

            <div class="panel-body">
                <asp:Panel ID="pnlContent" runat="server">
                    <asp:HiddenField ID="hfSiteId" runat="server" />
                    <asp:HiddenField ID="hfCurrentTab" runat="server" />

                    <div class="row">
                        <div class="col-md-12 col-lg-12">
                            <ul class="nav nav-tabs margin-b-lg">
                                <li id="liTabApplication" runat="server">
                                    <asp:LinkButton ID="lbTabApplication" runat="server" OnClick="lbTabApplication_Click">Application</asp:LinkButton>
                                </li>
                                <li id="liTabStyles" runat="server">
                                    <asp:LinkButton ID="lbTabStyles" runat="server" OnClick="lbTabStyles_Click">Styles</asp:LinkButton>
                                </li>
                                <li id="liTabLayouts" runat="server">
                                    <asp:LinkButton ID="lbTabLayouts" runat="server" OnClick="lbTabLayouts_Click">Layouts</asp:LinkButton>
                                </li>
                                <li id="liTabPages" runat="server">
                                    <asp:LinkButton ID="lbTabPages" runat="server" OnClick="lbTabPages_Click">Pages</asp:LinkButton>
                                </li>
                                <li id="liTabDeepLinks" runat="server">
                                    <asp:LinkButton ID="lbTabDeepLinks" runat="server" OnClick="lbTabDeepLinks_Click">Deep Links</asp:LinkButton>
                                </li>
                            </ul>

                            <asp:Panel ID="pnlApplication" runat="server">
                                <div class="row">
                                    <div class="col-md-8 col-lg-8">
                                        <fieldset>
                                            <p>
                                                <asp:Literal ID="ltDescription" runat="server" />
                                            </p>

                                            <div>
                                                <asp:Literal ID="ltAppDetails" runat="server" />
                                            </div>
                                        </fieldset>
                                    </div>
                                    <div class="col-md-4 hidden-sm hidden-xs">
                                        <asp:Panel ID="pnlPreviewImage" runat="server" CssClass="mobile-app-preview">
                                            <asp:Image ID="imgAppPreview" runat="server" />
                                        </asp:Panel>
                                    </div>
                                </div>

                                <div class="actions margin-t-md">
                                    <asp:LinkButton ID="lbEdit" runat="server" CssClass="btn btn-primary" Text="Edit" OnClick="lbEdit_Click" AccessKey="m" ToolTip="Alt+m" />
                                    <asp:LinkButton ID="lbCancel" runat="server" CssClass="btn btn-link" Text="Cancel" OnClick="lbCancel_Click" CausesValidation="false" AccessKey="c" ToolTip="Alt+c" />
                                    <div class="pull-right">
                                        <asp:LinkButton ID="lbDeploy" runat="server" CssClass="btn btn-default" OnClick="lbDeploy_Click" OnClientClick="Rock.dialogs.confirmPreventOnCancel( event, 'Are you sure you wish to replace the current package and deploy a new one?');"><i class="fa fa-upload"></i> Deploy</asp:LinkButton>
                                    </div>
                                </div>

                            </asp:Panel>

                            <asp:Panel ID="pnlStyles" runat="server">
                                <asp:HiddenField ID="hfShowAdvancedStylesFields" runat="server" />

                                <Rock:RockControlWrapper ID="rcwInterfaceColors" runat="server" Label="Interface Colors">
                                    <div class="row">
                                        <div class="col-md-4">
                                            <Rock:ColorPicker ID="cpEditBarBackgroundColor" runat="server" Label="Bar Background Color" Help="Override the default title bar background color provided by the mobile OS." />
                                        </div>
                                        <div class="col-md-4">
                                            <Rock:ColorPicker ID="cpEditMenuButtonColor" runat="server" Label="Menu Button Color" Help="The color of the menu button in the title bar." />
                                        </div>
                                        <div class="col-md-4">
                                            <Rock:ColorPicker ID="cpEditActivityIndicatorColor" runat="server" Label="Activity Indicator Color" Help="Defines the color that will be used when displaying an activity indicator, these alert the user that something is happening in the background." />
                                        </div>
                                        <div class="col-md-4">
                                            <Rock:ColorPicker ID="cpTextColor" runat="server" Label="Text Color" Help="The default color to use for text in the application." />
                                        </div>
                                        <div class="col-md-4">
                                            <Rock:ColorPicker ID="cpHeadingColor" runat="server" Label="Heading Color" Help="The default color to use for headings in the application." />
                                        </div>
                                        <div class="col-md-4">
                                            <Rock:ColorPicker ID="cpBackgroundColor" runat="server" Label="Background Color" Help="The background color for the application." />
                                        </div>
                                    </div>
                                </Rock:RockControlWrapper>


                                <div class="row">
                                    <div class="col-md-4">
                                        <Rock:RockCheckBox ID="cbNavbarTransclucent" runat="server" AutoPostBack="true" OnCheckedChanged="CbNavbarTransclucent_CheckedChanged" Label="Enable Navigation Bar Transparency" Help="Please note, your bar background color must have an opacity less than 100% for this to work. Enable to view different blurring options. (iOS Only)" />
                                        <Rock:RockDropDownList ID="ddlNavbarBlurStyle" runat="server" Label="Navigation Bar Blur Style" Help="Select between the different blur styles of the navigation bar. (iOS Only)" />
                                    </div>
                                </div>

                                <hr />

                                <Rock:RockControlWrapper ID="rcwAdditionalColors" runat="server" Label="Application Colors">
                                    <div class="row">
                                        <div class="col-md-4 col-sm-6">
                                            <Rock:ColorPicker ID="cpPrimary" runat="server" Label="Primary" Help="The primary color to use for buttons and other controls." />
                                        </div>

                                        <div class="col-md-4 col-sm-6">
                                            <Rock:ColorPicker ID="cpSecondary" runat="server" Label="Secondary" Help="Secondary color to use for various controls." />
                                        </div>

                                        <div class="col-md-4 col-sm-6">
                                            <Rock:ColorPicker ID="cpSuccess" runat="server" Label="Success" Help="Color to use for various controls to show that something was successful." />
                                        </div>

                                        <div class="col-md-4 col-sm-6">
                                            <Rock:ColorPicker ID="cpInfo" runat="server" Label="Info" Help="Color to use for various controls to show informational messages." />
                                        </div>

                                        <div class="col-md-4 col-sm-6">
                                            <Rock:ColorPicker ID="cpDanger" runat="server" Label="Danger" Help="Color to use for various controls to show that an action is dangerous." />
                                        </div>

                                        <div class="col-md-4 col-sm-6">
                                            <Rock:ColorPicker ID="cpWarning" runat="server" Label="Warning" Help="Color to use for various controls to show that caution is needed." />
                                        </div>

                                        <div class="col-md-4 col-sm-6">
                                            <Rock:ColorPicker ID="cpLight" runat="server" Label="Light" Help="A color to use for controls that need to be light." />
                                        </div>

                                        <div class="col-md-4 col-sm-6">
                                            <Rock:ColorPicker ID="cpDark" runat="server" Label="Dark" Help="A color to use for controls that need to be dark." />
                                        </div>

                                        <div class="col-md-4 col-sm-6">
                                            <Rock:ColorPicker ID="cpBrand" runat="server" Label="Brand" Help="Color to use for branding controls like the navigation header." />
                                        </div>
                                    </div>
                                </Rock:RockControlWrapper>

                                <div class="row">
                                    <div class="col-md-4">
                                        <Rock:NumberBox ID="nbRadiusBase" runat="server" NumberType="Integer" Label="Radius Base" Help=""></Rock:NumberBox>
                                    </div>
                                    <div class="col-md-4">
                                        <Rock:ImageUploader ID="imgEditHeaderImage" runat="server" Label="Navigation Bar Image" Help="The image that appears on the top header. While the size is dependent on design we recommend a height of 120px and minimum width of 560px." />
                                    </div>
                                </div>

                                <div class="clearfix">
                                    <div class="pull-right">
                                        <a href="#" class="btn btn-xs btn-link js-show-advanced-style-fields">Show Advanced Fields</a>
                                    </div>
                                </div>

                                <asp:Panel ID="pnlStylesAdvancedFields" runat="server" CssClass="js-advanced-style-fields" Style="display: none">
                                    <div class="row">

                                        <div class="col-md-4">
                                            <Rock:NumberBox ID="nbFontSizeDefault" runat="server" NumberType="Integer" Label="Font Size Default" Help="The default font size."></Rock:NumberBox>
                                        </div>
                                    </div>

                                    <Rock:CodeEditor ID="ceEditCssStyles" runat="server" Label="CSS Styles" Help="CSS Styles to apply to UI elements." EditorMode="Css" EditorHeight="600" />
                                </asp:Panel>

                                <div class="actions margin-t-md">
                                    <Rock:BootstrapButton ID="lbStylesEditSave" runat="server" CssClass="btn btn-primary" Text="Save" OnClick="lbStylesEditSave_Click"
                                        DataLoadingText="&lt;i class='fa fa-refresh fa-spin'&gt;&lt;/i&gt; Saving"
                                        CompletedText="Saved"
                                        CompletedMessage="<div class='margin-t-md alert alert-success'>Changes have been saved.</div>"
                                        CompletedDuration="5"></Rock:BootstrapButton>
                                </div>

                            </asp:Panel>

                            <asp:Panel ID="pnlLayouts" runat="server">
                                <Rock:Grid ID="gLayouts" runat="server" RowItemText="Layout" DisplayType="Light" OnGridRebind="gLayouts_GridRebind" OnRowSelected="gLayouts_RowSelected">
                                    <Columns>
                                        <Rock:RockBoundField DataField="Name" SortExpression="Name" HeaderText="Name" />
                                        <Rock:RockBoundField DataField="Description" SortExpression="Description" HeaderText="Description" />
                                        <Rock:DeleteField OnClick="gLayouts_DeleteClick" />
                                    </Columns>
                                </Rock:Grid>
                            </asp:Panel>

                            <asp:Panel ID="pnlPages" runat="server">
                                <Rock:Grid ID="gPages" runat="server" RowItemText="Page" DisplayType="Light" OnGridRebind="gPages_GridRebind" OnRowSelected="gPages_RowSelected" OnGridReorder="gPages_GridReorder" OnRowDataBound="gPages_RowDataBound">
                                    <Columns>
                                        <Rock:ReorderField />
                                        <Rock:RockBoundField DataField="InternalName" SortExpression="Name" HeaderText="Name" />
                                        <Rock:RockBoundField DataField="LayoutName" SortExpression="LayoutName" HeaderText="Layout" />
                                        <Rock:RockBoundField DataField="DisplayInNavWhen" SortExpression="DisplayInNav" HeaderText="Display In Nav" />
                                        <Rock:DeleteField OnClick="gPages_DeleteClick" />
                                    </Columns>
                                </Rock:Grid>
                            </asp:Panel>

                            <asp:Panel ID="pnlDeepLinks" runat="server">
                                <asp:Panel ID="pnlDeepLinkDomains" runat="server" class="alert alert-info" Width="40%">
                                    <asp:Label runat="server" Font-Bold="true" Text="Enabled domains:" />
                                    <asp:Label ID="lblDeepLinkDomains" runat="server" />
                                </asp:Panel>
                                <Rock:Grid ID="gDeepLinks" runat="server" RowItemText="Deep Link" DisplayType="Light" OnGridRebind="gDeepLinks_GridRebind" OnRowSelected="gDeepLinks_RowSelected" OnGridReorder="gDeepLinks_GridReorder">
                                    <Columns>
                                        <Rock:ReorderField />
                                        <Rock:RockBoundField DataField="Route" SortExpression="Route" HeaderText="Route" DataFormatString="/{0}" />
                                        <Rock:RockBoundField DataField="Page" SortExpression="Page" HeaderText="Mobile Page" />

                                        <Rock:RockTemplateField SortExpression="Fallback" HeaderText="Fallback">
                                            <ItemTemplate>
                                                <asp:Label runat="server" Visible='<%#Eval("IsUrl") %>' CssClass="badge badge-info" Text='URL' />
                                                <asp:Label runat="server" Text='<%#Eval("Fallback") %>' />
                                            </ItemTemplate>
                                        </Rock:RockTemplateField>
                                        <Rock:DeleteField OnClick="gDeepLinks_DeleteClick" />
                                    </Columns>
                                </Rock:Grid>
                            </asp:Panel>
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlEdit" runat="server">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbEditName" runat="server" Label="Application Name" Required="true" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbEditActive" runat="server" Label="Active" />
                        </div>
                    </div>

                    <Rock:RockTextBox ID="tbEditDescription" runat="server" Label="Description" TextMode="MultiLine" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockRadioButtonList ID="rblEditApplicationType" runat="server" Label="Application Type" RepeatDirection="Horizontal" OnSelectedIndexChanged="rblEditApplicationType_SelectedIndexChanged" AutoPostBack="true" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockRadioButtonList ID="rblEditAndroidTabLocation" runat="server" Label="Android Tab Location" RepeatDirection="Horizontal" Help="Determines where the Tab bar should be displayed on Android. iOS will always display at the bottom." />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockDropDownList ID="ddlEditLockPhoneOrientation" runat="server" Label="Lock Phone Orientation" Help="Setting this value will lock the orientation on phones to the specified orientation." />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockDropDownList ID="ddlEditLockTabletOrientation" runat="server" Label="Lock Tablet Orientation" Help="Setting this value will lock the orientation on tablets to the specified orientation." />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:PagePicker ID="ppEditLoginPage" runat="server" Label="Login Page" />
                        </div>
                        <div class="col-md-6">
                            <Rock:PagePicker ID="ppEditProfilePage" runat="server" Label="Profile Page" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:PagePicker ID="ppEditInteractiveExperiencePage" runat="server" Label="Interactive Experience Page" Help="If you are using interactive experiences then set this page to the mobile page that contains the Live Experience block." />
                        </div>

                        <div class="col-md-6">
                            <Rock:PagePicker ID="ppCommunicationViewPage" runat="server" Label="Communication View Page" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:PagePicker ID="ppEditSmsConversationPage" runat="server" Label="SMS Conversation Page" Help="If you are using the SMS conversations, then set this to the page that contains the SMS Conversation block." />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbEditApiKey" runat="server" Label="API Key" Required="true" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataViewItemPicker ID="dvpCampusFilter" runat="server" Label="Campus Filter" Help="Select a data view of campuses to use for the campus lists within the application. Leave blank if your application does not need to filter content by campus"></Rock:DataViewItemPicker>
                        </div>

                        <div class="col-md-6">
                            <Rock:CategoryPicker ID="cpEditPersonAttributeCategories" runat="server" Label="Person Attribute Categories" Help="All attributes in selected categories will be sent to the client and made available remotely." AllowMultiSelect="true" />
                        </div>
                    </div>

                    <Rock:PanelWidget ID="pwEditAdvancedSettings" runat="server" Title="Advanced Settings">
                        <Rock:RockCheckBox ID="cbEnableNotificationsAutomatically" runat="server" Label="Enable Notifications Automatically" Help="When turned on the mobile application will automatically request push notifications permission from the user at launch." />

                        <Rock:CodeEditor ID="ceEditFlyoutXaml" runat="server" Label="Flyout XAML" Help="The XAML template to use for the menu in the Flyout Shell." EditorMode="Xml" Required="true" />

                        <Rock:CodeEditor ID="ceEditNavBarActionXaml" runat="server" Label="Navigation Bar Action XAML" Help="The XAML template to use for placing content into the top navigation bar." EditorMode="Xml"></Rock:CodeEditor>

                        <Rock:CodeEditor ID="ceEditHomepageRoutingLogic" runat="server" Label="Homepage Routing Logic" Help="The Lava to be executed at application start to determine which page to open initially. Should output blank or a page Guid." EditorMode="Lava" Required="false" />

                        <div class="row">
                            <%--<div class="col-md-4">
                                <Rock:ImageUploader ID="imgEditHeaderImage" runat="server" Label="Header Image" Help="The image that appears on the top header. While the size is dependent on design we recommend a height of 120px and minimum width of 560px." />
                            </div>--%>

                            <div class="col-md-4">
                                <Rock:ImageUploader ID="imgEditPreviewThumbnail" runat="server" Label="Preview Thumbnail" Help="Preview thumbnail to be used by Rock to distinguish application." />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="tbEditPushTokenUpdateValue" runat="server" Label="Force Push Token Update" Help="Setting or changing this value will force all clients to update their push token. Use with caution." />
                            </div>

                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbCompressUpdatePackages" runat="server" Label="Compress Update Packages" Help="Compresses update packages to reduce their size by up to 95%. Not supported with mobile shell v1." />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:NumberBox ID="nbPageViewRetentionPeriodDays" runat="server" Label="Page View Retention Period" Help="The number of days to keep page views logged. Leave blank to keep page views logged indefinitely." />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbEnablePageViewGeoTracking" runat="server" Label="Enable Page View Geo Tracking" Help="Enabling this feature will allow the PopulateInteractionSessionData job to begin performing geolocation lookup on the IP addresses in the Interaction Session data. This also requires setting up a IP Address Location Service found under System Settings." />
                            </div>
                        </div>

                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="pwAuthenticationSettings" runat="server" Title="Authentication Settings">
                        <div class="rock-header">
                            <h3 class="title mb-2">Auth0</h3>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockTextBox ID="tbAuth0ClientId" runat="server" Label="Auth0 Client ID" Help="Set this to reflect the value in your configured Auth0 application to add support for Auth0 based login in your mobile application." />
                                </div>

                                <div class="col-md-6">
                                    <Rock:RockTextBox ID="tbAuth0ClientDomain" runat="server" Label="Auth0 Domain" Help="Set this to reflect the value in your configured Auth0 application to add support for Auth0 based login in your mobile application." />
                                </div>
                            </div>
                            <hr class="section-header-hr" />
                        </div>

                        <div class="rock-header">
                            <h3 class="title mb-2">Microsoft Entra (Azure AD)</h3>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockTextBox ID="tbEntraClientId" runat="server" Label="Entra Client ID" Help="The client ID of the Microsoft Entra application to add authentication support for." />
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockTextBox ID="tbEntraTenantId" runat="server" Label="Entra Tenant ID" Help="The tenant ID of the Microsoft Entra application to add authentication support for." />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:ComponentPicker ID="compEntraAuthComponent" runat="server" Label="Microsoft Entra Authentication Provider" ContainerType="Rock.Security.AuthenticationContainer, Rock" Help="The authentication component used for web Entra authentication. This is typically provided through a plugin." />
                                </div>
                            </div>
                            <hr class="section-header-hr, hidden" />
                        </div>

                        <div class="hidden">
                            <div class="row">
                                <div class="col-md-6">
                                </div>

                                <div class="col-md-6">
                                </div>
                            </div>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="pwEditDeepLinkSettings" runat="server" Title="Deep Link Settings">
                        <Rock:RockCheckBox ID="cbEnableDeepLinking" OnCheckedChanged="cbEnableDeepLinking_CheckedChanged" runat="server" Label="Enable Deep Linking" Help="Determines if specific web links should open in the app if it’s installed on the individual’s phone." AutoPostBack="true" />
                        <asp:Panel runat="server" class="info alert-info" Width="40%">
                        </asp:Panel>
                        <asp:Panel ID="pnlDeepLinkSettings" runat="server">
                            <Rock:NotificationBox ID="nbDeepLinks" runat="server" Visible="false" NotificationBoxType="Danger" />
                            <Rock:RockTextBox ID="tbDeepLinkPathPrefix" runat="server" Label="Deep Link Path Prefix" Required="true" Help="The URL path prefix that flags that a URL should be opened in the application. A value of ‘m’ would mean that all URLs like https://server.com/m/<route> will be routed to the mobile app if it’s installed on the individual’s phone." />
                            <Rock:ValueList ID="vlDeepLinkingDomain" runat="server" Label="Deep Linking Domains"
                                Help="The domains that you plan to accept deep links from. To accept all subdomains, use '*.<domain>'." />
                            <h4>iOS Settings</h4>
                            <Rock:RockTextBox ID="tbTeamId" runat="server" Label="Team Id" Required="true" Width="40%" />
                            <Rock:RockTextBox ID="tbBundleId" runat="server" Label="Bundle Id" Required="true" Help="The iOS bundle id. You will get this value from your shell hosting service." Width="40%" />
                            <h4>Android Settings</h4>
                            <Rock:RockTextBox ID="tbPackageName" runat="server" Label="Package Name" Required="true" Help="The Android package name. You will get this value from your shell hosting service." Width="40%" />
                            <Rock:RockTextBox ID="tbCertificateFingerprint" runat="server" Label="Certificate Fingerprint" Required="true" Help="The application’s certificate fingerprint. You will get this value from your shell hosting service." Width="40%" />
                        </asp:Panel>
                    </Rock:PanelWidget>

                    <div class="actions margin-t-md">
                        <asp:LinkButton ID="lbEditSave" runat="server" CssClass="btn btn-primary" Text="Save" OnClick="lbEditSave_Click" AccessKey="s" ToolTip="Alt+s" />
                        <asp:LinkButton ID="lbEditCancel" runat="server" CssClass="btn btn-link" Text="Cancel" OnClick="lbEditCancel_Click" CausesValidation="false" AccessKey="c" ToolTip="Alt+c" />
                    </div>
                </asp:Panel>
            </div>
        </asp:Panel>

        <script type="text/javascript">
            Sys.Application.add_load(function () {
                $('.js-show-advanced-style-fields').off('click').on('click', function () {
                    var isVisible = !$('.js-advanced-style-fields').is(':visible');
                    $('#<%=hfShowAdvancedStylesFields.ClientID %>').val(isVisible);
                    $('.js-show-advanced-style-fields').text(isVisible ? 'Hide Advanced Fields' : 'Show Advanced Fields');
                    $('.js-advanced-style-fields').slideToggle();
                    return false;
                });

                if ($('#<%=hfShowAdvancedStylesFields.ClientID %>').val() == "true") {
                    $('.js-advanced-style-fields').show();
                    $('.js-show-advanced-style-fields').text('Hide Additional Fields');
                }
            });

        </script>
    </ContentTemplate>
</asp:UpdatePanel>
