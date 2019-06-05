<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContentChannelItemView.ascx.cs" Inherits="RockWeb.Blocks.Cms.ContentChannelItemView" %>

<script type="text/javascript">
    function clearDialog() {
        $('#rock-config-cancel-trigger').click();
    }
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <%-- View Panel --%>
        <asp:Panel ID="pnlView" runat="server">
            <Rock:NotificationBox ID="nbAlert" runat="server" NotificationBoxType="Danger" />
            <asp:Literal ID="lContentOutput" runat="server" />
        </asp:Panel>

        <%-- Custom Block Settings --%>
        <asp:Panel ID="pnlSettings" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdSettings" runat="server" OnSaveClick="mdSettings_SaveClick" Title="Channel Item Configuration" OnCancelScript="clearDialog();">
                <Content>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:RockDropDownList ID="ddlContentChannel" runat="server" Required="true" Label="Content Channel" Help="Limits content channel items to a specific channel and enables Social Media Settings to be configurable, or leave blank to leave unrestricted." AutoPostBack="true" OnSelectedIndexChanged="ddlContentChannel_SelectedIndexChanged" />
                            <Rock:RockCheckBoxList ID="cblStatus" runat="server" Label="Status" RepeatDirection="Horizontal" Help="Include items with the following status." />
                            <Rock:CodeEditor ID="ceLavaTemplate" runat="server" Label="Lava Template" Help="The template to use when formatting the content channel item." EditorMode="Lava" EditorTheme="Rock" EditorHeight="200" />
                        </div>
                    </div>

                    <Rock:PanelWidget ID="pwVisitorSettings" runat="server" Title="Visitor Settings">
                        <div class="row">
                            <div class="col-md-6">
                                <fieldset>
                                    <legend>Interactions</legend>
                                    <Rock:RockCheckBox ID="cbLogInteractions" runat="server" Label="Log Item Interaction" Help="Create an interaction for the current content channel item" OnCheckedChanged="cbLogInteractions_CheckedChanged" AutoPostBack="true" />
                                    <Rock:RockCheckBox ID="cbWriteInteractionOnlyIfIndividualLoggedIn" runat="server" Label="Write Interaction Only If Individual Logged In" Help="Set to true to only launch a workflow for logged in users, or set to false to launch for both logged in and anonymous users." />
                                </fieldset>
                            </div>
                            <div class="col-md-6">
                                <fieldset>
                                    <legend>Workflows</legend>
                                    <Rock:WorkflowTypePicker ID="wtpWorkflowType" runat="server" Label="Workflow Type" Help="The workflow type to launch when the content is viewed." OnSelectItem="wtpWorkflowType_SelectItem" />
                                    <Rock:RockCheckBox ID="cbLaunchWorkflowOnlyIfIndividualLoggedIn" runat="server" Label="Launch Workflow Only If Individual Logged In" Help="Set to true to only launch a workflow for logged in users, or set to false to launch for both logged in and anonymous users." />
                                    <Rock:RockDropDownList ID="ddlLaunchWorkflowCondition" runat="server" Label="Launch Workflow" />
                                </fieldset>
                            </div>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="pwSocialMediaSettings" runat="server" Title="Social Media Settings">
                        <Rock:RockDropDownList ID="ddlMetaDescriptionAttribute" Label="Meta Description Attribute" Help="Attribute to use for the page's meta description." runat="server" />

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockDropDownList ID="ddlOpenGraphTitleAttribute" Label="Facebook Title Attribute" Help="If you don't want to use the post title for sharing the post on Facebook but instead want another title. [og:title]" runat="server" />
                                <Rock:RockDropDownList ID="ddlOpenGraphDescriptionAttribute" Label="Facebook Description Attribute" Help="If you don't want to use the meta description for sharing the post on Facebook but instead want another title. [og:description]" runat="server" />
                                <Rock:RockDropDownList ID="ddlOpenGraphImageAttribute" Label="Facebook Image Attribute" Help="If you want to override the image used on Facebook for this post. [og:image]" runat="server" />
                                <Rock:RockDropDownList ID="ddlOpenGraphType" Label="Open Graph Object Type" runat="server">
                                    <asp:ListItem />
                                    <asp:ListItem Text="article" />
                                    <asp:ListItem Text="website" />
                                    <asp:ListItem Text="book" />
                                    <asp:ListItem Text="place" />
                                    <asp:ListItem Text="product" />
                                    <asp:ListItem Text="profile" />
                                    <asp:ListItem Text="video.episode" />
                                    <asp:ListItem Text="video.movie" />
                                    <asp:ListItem Text="video.other" />
                                    <asp:ListItem Text="video.tv_show" />
                                </Rock:RockDropDownList>
                            </div>

                            <div class="col-md-6">
                                <Rock:RockDropDownList ID="ddlTwitterTitleAttribute" Label="Twitter Title Attribute" Help="If you don't want to use the post title for sharing the post on Twitter but instead want another title." runat="server" />
                                <Rock:RockDropDownList ID="ddlTwitterDescriptionAttribute" Label="Twitter Description Attribute" Help="If you don't want to use the meta description for sharing the post on Twitter but instead want another title." runat="server" />
                                <Rock:RockDropDownList ID="ddlTwitterImageAttribute" Label="Twitter Image Attribute" Help="If you want to override the image used on Facebook for this post." runat="server" />
                                <Rock:RockDropDownList ID="ddlTwitterCard" Label="Twitter Card Type" runat="server">
                                    <asp:ListItem Text="" Value="none" />
                                    <asp:ListItem Text="Summary" Value="summary" />
                                    <asp:ListItem Text="Summary with large image" Value="summary_large_image" />
                                </Rock:RockDropDownList>
                            </div>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="pwAdvancedSettings" runat="server" Title="Advanced Settings">
                        <Rock:RockTextBox ID="tbContentChannelQueryParameter" runat="server" Label="Custom Query Parameter" Help="Specify the URL parameter to use to determine which Content Channel Item to show, or leave blank to use whatever the first parameter is.
The type of the value will determine how the content channel item will be determined as follows:

Integer - ContentChannelItem Id
String - ContentChannelItem Slug
Guid - ContentChannelItem Guid" />

                        <Rock:PagePicker ID="ppDetailPage" runat="server" Label="Detail Page"  Help="Page ID used to view a content item." />
                        <Rock:NumberBox ID="nbOutputCacheDuration" runat="server" Label="Output Cache Duration" MinimumValue="0" CssClass="input-width-sm" Help="Number of seconds to cache the resolved output. Only cache the output if you are not personalizing the output based on current user, current page, or any other merge field value." />
                        <Rock:NumberBox ID="nbItemCacheDuration" runat="server" Label="Item Cache Duration" MinimumValue="0" CssClass="input-width-sm" Help="Number of seconds to cache the content item specified by the parameter." />
                        <Rock:RockCheckBoxList ID="cblCacheTags" runat="server" Label="Cache Tags" Help="Cached tags are used to link cached content so that it can be expired as a group" RepeatDirection="Horizontal" />
                        <Rock:RockCheckBox ID="cbSetPageTitle" runat="server" Label="Set Page Title" Help="Determines if the block should set the page title with the channel name or content item." />
                    </Rock:PanelWidget>
                </Content>

            </Rock:ModalDialog>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
