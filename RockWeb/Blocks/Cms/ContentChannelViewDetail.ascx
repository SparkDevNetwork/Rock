<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContentChannelViewDetail.ascx.cs" Inherits="RockWeb.Blocks.Cms.ContentChannelViewDetail" %>


<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <%-- View Panel --%>
        <asp:Panel ID="pnlView" runat="server">
            <Rock:NotificationBox ID="nbContentError" runat="server" Dismissable="true" Visible="false" />
            <asp:PlaceHolder ID="phContent" runat="server" />
            <asp:Literal ID="lDebug" runat="server" />
        </asp:Panel>

        <%-- Custom Block Settings --%>
        <asp:Panel ID="pnlSettings" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdSettings" runat="server" OnSaveClick="mdSettings_SaveClick">
                <Content>
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:RockDropDownList ID="ddlContentChannel" runat="server" Label="Content Channel" Help="Limits content channel items to a specific channel and enables Social Media Settings to be configurable, or leave blank to leave unrestricted." AutoPostBack="true" OnSelectedIndexChanged="ddlContentChannel_SelectedIndexChanged" />
                            
                            <Rock:RockTextBox ID="tbContentChannelQueryParameter" runat="server" Label="Content Channel Query Parameter" Help="Specify the URL parameter to use to determine which Content Channel Item to show, or leave blank to use whatever the first parameter is. 
The type of the value will determine how the content channel item will be determined as follows:

Integer - ContentChannelItem Id
String - ContentChannelItem Slug
Guid - ContentChannelItem Guid" />

                            <Rock:CodeEditor ID="ceLavaTemplate" runat="server" Label="Lava Template" Help="The template to use when formatting the content channel item." EditorMode="Lava" EditorTheme="Rock" EditorHeight="200" />
                            <Rock:NumberBox ID="nbOutputCacheDuration" runat="server" Label="Output Cache Duration" Help="Number of seconds to cache the resolved output. Only cache the output if you are not personalizing the output based on current user, current page, or any other merge field value." />
                            <Rock:RockCheckBox ID="cbSetPageTitle" runat="server" Label="Set Page Title" Help="Determines if the block should set the page title with the channel name or content item." />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-6">
                            <fieldset>
                                <legend>Interactions</legend>
                                <Rock:RockCheckBox ID="cbLogInteractions" runat="server" Label="Log Interactions" />
                                <Rock:RockCheckBox ID="cbWriteInteractionOnlyIfIndividualLoggedIn" runat="server" Label="Write Interaction Only If Individual Logged In" Help="Set to true to only launch a workflow for logged in users, or set to false to launch for both logged in and anonymous users." />
                            </fieldset>
                        </div>
                        <div class="col-md-6">
                            <fieldset>
                                <legend>Workflows</legend>

                                <Rock:WorkflowTypePicker ID="wtpWorkflowType" runat="server" Label="Workflow Type" Help="The workflow type to launch when the content is viewed." />
                                <Rock:RockCheckBox ID="cbLaunchWorkflowOnlyIfIndividualLoggedIn" runat="server" Label="Launch Workflow Only If Individual Logged In" Help="Set to true to only launch a workflow for logged in users, or set to false to launch for both logged in and anonymous users." />
                                <Rock:RockDropDownList ID="ddlLaunchWorkflowCondition" runat="server" Label="Launch Workflow" />
                            </fieldset>
                        </div>
                    </div>

                    <Rock:PanelWidget ID="pwSocialMediaSettings" runat="server" Title="Social Media Settings">
                        <Rock:RockDropDownList ID="ddlMetaDescriptionAttribute" Label="Meta Description Attribute" Help="Attribute to use for the page's meta description." runat="server" />

                        <Rock:RockDropDownList ID="ddlOpenGraphType" Label="Open Graph Type (og:type)" runat="server">
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
                        <Rock:RockDropDownList ID="ddlOpenGraphTitleAttribute" Label="Open Graph Title (og:title) Attribute" runat="server" />
                        <Rock:RockDropDownList ID="ddlOpenGraphDescriptionAttribute" Label="Open Graph Description (og:description) Attribute" runat="server" />
                        <Rock:RockDropDownList ID="ddlOpenGraphImageAttribute" Label="Open Graph Image (og:image) Attribute" runat="server" />

                        <Rock:RockDropDownList ID="ddlTwitterTitleAttribute" Label="Twitter Title (twitter:title) Attribute" runat="server" />
                        <Rock:RockDropDownList ID="ddlTwitterDescriptionAttribute" Label="Twitter Description (twitter:description) Attribute" runat="server" />
                        <Rock:RockDropDownList ID="ddlTwitterImageAttribute" Label="Twitter Image (twitter:image) Attribute" runat="server" />
                        <Rock:RockRadioButtonList ID="rblTwitterCard" Label="Twitter Card (twitter:card)" runat="server">
                            <asp:ListItem Text="none" Value="none" />
                            <asp:ListItem Text="Summary" Value="summary" />
                            <asp:ListItem Text="Large Image" Value="summary_large_image" />
                        </Rock:RockRadioButtonList>
                    </Rock:PanelWidget>
                </Content>

            </Rock:ModalDialog>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
