<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContentComponent.ascx.cs" Inherits="RockWeb.Blocks.Cms.ContentComponent" %>


<script type="text/javascript">
    function clearDialog() {
        $('#rock-config-cancel-trigger').trigger('click');
    }
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <%-- View Panel --%>
        <asp:Panel ID="pnlView" runat="server">
            <Rock:NotificationBox ID="nbContentError" runat="server" Dismissable="true" Visible="false" />
            <asp:Literal ID="lContentOutput" runat="server" />
        </asp:Panel>

        <%-- Content Component Config --%>
        <asp:Panel ID="pnlContentComponentConfig" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdContentComponentConfig" runat="server" OnSaveClick="mdContentComponentConfig_SaveClick" Title="Content Component - Configuration" OnCancelScript="clearDialog();" ValidationGroup="vgContentComponentConfig">
                <Content>
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbComponentName" runat="server" Label="Component Name" Required="true" MaxLength="100" ValidationGroup="vgContentComponentConfig" />
                            <Rock:NumberBox ID="nbItemCacheDuration" runat="server" Label="Item Cache Duration" MinimumValue="0" CssClass="input-width-sm" Help="Number of seconds to cache the content item specified by the parameter." />
                            <Rock:DefinedValuePicker ID="dvpContentComponentTemplate" runat="server" Required="true" Label="Content Component Template" ValidationGroup="vgContentComponentConfig" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbAllowMultipleContentItems" runat="server" Label="Allow Multiple Content Items" Help="Allows you to provide more than one content item." />
                            <Rock:NumberBox ID="nbOutputCacheDuration" runat="server" Label="Output Cache Duration" MinimumValue="0" CssClass="input-width-sm" Help="Number of seconds to cache the resolved output. Only cache the output if you are not personalizing the output based on current user, current page, or any other merge field value." />
                            <Rock:RockCheckBoxList ID="cblCacheTags" runat="server" Label="Cache Tags" Help="Cached tags are used to link cached content so that it can be expired as a group" RepeatDirection="Horizontal" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:AttributeValuesContainer ID="avcContentChannelAttributes" runat="server" />
                        </div>
                    </div>

                    <Rock:PanelWidget ID="pwFilters" runat="server" Title="Filters">
                        <asp:HiddenField ID="hfDataFilterId" runat="server" />
                        <asp:PlaceHolder ID="phFilters" runat="server"></asp:PlaceHolder>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="pwAdvanced" runat="server" Title="Advanced">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:CodeEditor ID="cePreHtml" runat="server" Label="Pre-HTML" Help="HTML Content to render before the block <span class='tip tip-lava'></span>." EditorMode="Lava" EditorTheme="Rock" EditorHeight="100" />
                            </div>
                            <div class="col-md-6">
                                <Rock:CodeEditor ID="cePostHtml" runat="server" Label="Post-HTML" Help="HTML Content to render after the block <span class='tip tip-lava'></span>." EditorMode="Lava" EditorTheme="Rock" EditorHeight="100" />
                            </div>
                        </div>
                    </Rock:PanelWidget>

                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

        <%-- Content Component Edit - Content Channel Item(s)--%>
        <asp:Panel ID="pnlContentComponentEditContentChannelItems" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdContentComponentEditContentChannelItems" runat="server" OnSaveClick="mdContentComponentEditContentChannelItems_SaveCloseClick" Title="Content Component - Edit Content" ValidationGroup="vgContentComponentEditContentChannelItem" OnCancelScript="clearDialog();">
                <Content>
                    <div class="row">
                        <%-- NOTE: CssClass for pnlContentChannelItemEdit is set in CodeBehind based on AllowMultipleItems Option --%>
                        <asp:Panel ID="pnlContentChannelItemEdit" runat="server" CssClass="col-md-8">
                            <asp:HiddenField ID="hfContentChannelItemId" runat="server" />
                            <Rock:RockTextBox ID="tbContentChannelItemTitle" runat="server" Label="Title" Required="true" MaxLength="200" ValidationGroup="vgContentComponentEditContentChannelItem" />
                            <Rock:HtmlEditor ID="htmlContentChannelItemContent" runat="server" Label="Content" ResizeMaxWidth="720" Height="300" ValidationGroup="vgContentComponentEditContentChannelItem" Toolbar="Full" />
                            <Rock:AttributeValuesContainer ID="avcContentChannelItemAttributes" runat="server" />
                        </asp:Panel>
                        <asp:Panel ID="pnlContentChannelItemsList" runat="server" CssClass="col-md-4">
                            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                            <Rock:RockControlWrapper ID="rcwContentChannelItems" runat="server" Label="Content Items">
                                <Rock:Grid ID="gContentChannelItems" runat="server" DisplayType="Light" AllowSorting="false" RowItemText="Item" OnRowSelected="gContentChannelItems_RowSelected" OnRowDataBound="gContentChannelItems_RowDataBound" ShowHeader="false">
                                    <Columns>
                                        <Rock:ReorderField />
                                        <asp:BoundField DataField="Title" />
                                        <Rock:DeleteField OnClick="gContentChannelItems_DeleteClick" />
                                    </Columns>
                                </Rock:Grid>
                            </Rock:RockControlWrapper>
                        </asp:Panel>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSaveItem" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save Item" ValidationGroup="vgContentComponentEditContentChannelItem" CssClass="btn btn-primary" OnClick="btnSaveItem_Click" />
                    </div>

                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
