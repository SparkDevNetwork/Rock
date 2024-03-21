<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContentChannelView.ascx.cs" Inherits="RockWeb.Blocks.Cms.ContentChannelView" %>

<script type="text/javascript">
    function clearDialog() {
        $('#rock-config-cancel-trigger').trigger('click');
    }
</script>

<asp:UpdatePanel ID="upnlContent" runat="server" UpdateMode="Conditional">
    <ContentTemplate>

        <%-- View Panel --%>
        <asp:Panel ID="pnlView" runat="server">
            <Rock:NotificationBox ID="nbContentError" runat="server" Dismissable="true" Visible="false" />
            <asp:PlaceHolder ID="phContent" runat="server" />
            <asp:Literal ID="lDebug" runat="server" />
        </asp:Panel>

        <%-- Edit Panel --%>
        <asp:Panel ID="pnlEditModal" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdEdit" runat="server" OnSaveClick="lbSave_Click" Title="Channel Configuration" OnCancelScript="clearDialog();">
                <Content>

                    <asp:UpdatePanel ID="upnlEdit" runat="server">
                        <ContentTemplate>

                            <Rock:NotificationBox ID="nbError" runat="server" Heading="Error" Title="Query Error!" NotificationBoxType="Danger" Visible="false" />

                            <div class="row">
                                <div class="col-md-5">
                                    <Rock:RockDropDownList ID="ddlChannel" runat="server" Required="true" Label="Channel" EnhanceForLongLists="true"
                                        DataTextField="Name" DataValueField="Guid" AutoPostBack="true" OnSelectedIndexChanged="ddlChannel_SelectedIndexChanged"
                                        Help="The channel to display items from." />
                                </div>
                                <div class="col-md-7">
                                    <Rock:RockCheckBoxList ID="cblStatus" runat="server" Label="Status" RepeatDirection="Horizontal"
                                        Help="Include items with the following status." />
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-md-12">
                                    <Rock:CodeEditor ID="ceTemplate" runat="server" EditorHeight="200" EditorMode="Lava" EditorTheme="Rock" Label="Format"
                                        Help="The template to use when formatting the list of items." />
                                </div>
                            </div>

                            <div class="rock-header mt-4">
                                <h3 class="title">Settings</h3>
                                <hr class="section-header-hr">
                            </div>

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:NumberBox ID="nbCount" runat="server" MinimumValue="0" CssClass="input-width-sm" Label="Items Per Page"
                                        Help="The maximum number of items to display per page (0 means unlimited)." />
                                    <Rock:NumberBox ID="nbItemCacheDuration" runat="server" MinimumValue="0" CssClass="input-width-sm" Label="Item Cache Duration"
                                        Help="Number of seconds to cache the content items returned by the selected filter. Only cache the items if they are NOT secured, otherwise you will have unexpected results. (use '0' for no caching)." />
                                    <Rock:NumberBox ID="nbOutputCacheDuration" runat="server" MinimumValue="0" CssClass="input-width-sm" Label="Output Cache Duration"
                                        Help="Number of seconds to cache the resolved output. Only cache the output if you are not personalizing the output based on current user, current page, or any other merge field value. (use '0' for no caching)." />
                                    <Rock:RockCheckBoxList ID="cblCacheTags" runat="server" Label="Cache Tags" EmptyListMessage="<div class='text-muted small'>No cache tags defined.</div>" Help="Cached tags are used to link cached content so that it can be expired as a group" RepeatDirection="Horizontal" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockCheckBox ID="cbSetPageTitle" runat="server" Label="Set Page Title"
                                        Help="When enabled will update the page title with the channel's name unless there is an item id in the query string then it will display the item's title." />
                                    <Rock:RockCheckBox ID="cbMergeContent" runat="server" Label="Merge Content"
                                        Help="Enabling this option will result in the content data and attribute values to be merged using the lava template engine." />
                                    <Rock:PagePicker ID="ppDetailPage" runat="server" Label="Detail Page" />
                                    <Rock:RockCheckBox ID="cbEnableTags" runat="server" Label="Enable Tag List"
                                        Help="Determines if the 'ItemTagList' lava merge field will be populated and passed to the lava template. The ItemTagList is a list of objects with the following fields 'Id', 'Guid', 'Name', 'Count'. Example: {% for tag in ItemTagList %} {{ tag.Name }} ({{ tag.Count }}) {% endfor %} <small><span class='tip tip-lava'></span></small>" />
                                    <Rock:RockCheckBox ID="cbEnableArchiveSummary" runat="server" Label="Enable Archive Summary"
                                        Help="When enabled an additional 'ArchiveSummary' collection will be available in Lava to help create a summary list of content channel items by month/year. This collection will be cached using the same duration as the Item Cache and will hold the following properties: Month (int), MonthName, Year, Count." />
                                </div>
                            </div>

                             <div class="rock-header mt-4">
                                <h3 class="title">Filters and Sorting</h3>
                                <hr class="section-header-hr">
                            </div>

                            <div class="form-group">
                                <asp:HiddenField ID="hfDataFilterId" runat="server" />
                                <asp:PlaceHolder ID="phFilters" runat="server"></asp:PlaceHolder>
                            </div>


                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockDropDownList ID="ddlContextAttribute" runat="server" Label="Context Filter Attribute" Help="Item attribute to compare when filtering items using the block Context. If the block doesn't have a context, this setting will be ignored." />
                                    <Rock:RockCheckBox ID="cbQueryParamFiltering" runat="server" Label="Enable Query/Route Parameter Filtering"
                                        Help="Enabling this option will allow results to be filtered further by any query string our route parameters that are included. This includes item properties or attributes. This will disable Cache Tags." />
                                    <Rock:KeyValueList ID="kvlOrder" runat="server" Label="Order Items By" KeyPrompt="Field" ValuePrompt="Direction"
                                        Help="The field value and direction that items should be ordered by." />
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockRadioButtonList ID="rblPersonalization" runat="server" Label="Personalization" RepeatDirection="Horizontal"
                                        Help="The setting determines how personalization effect the results shown. Ignore will not consider segments or request filters, Prioritize will add items with matching items to the top of the list (in order by the sort order) and Filter will only show items that match the current individuals segments and request filters." />
                                </div>
                            </div>

                            <div class="rock-header mt-4">
                                <h3 class="title">Social Media Settings</h3>
                                <hr class="section-header-hr">

                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:RockDropDownList ID="ddlMetaDescriptionAttribute" runat="server" Label="Meta Description Attribute"
                                            Help="Attribute to use for the page's meta description." />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:RockDropDownList ID="ddlMetaImageAttribute" runat="server" Label="Meta Image Attribute"
                                            Help="Attribute to use for the page's og:image and image_src meta tags." />
                                    </div>
                                </div>
                            </div>

                            <Rock:RockCheckBox ID="cbSetRssAutodiscover" runat="server" Label="Set RSS Autodiscover Link"
                                Help="Sets an RSS autodiscover link to the header section of the page." />

                        </ContentTemplate>
                    </asp:UpdatePanel>
                </Content>
            </Rock:ModalDialog>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
