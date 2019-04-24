<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContentChannelDetail.ascx.cs" Inherits="RockWeb.Blocks.Cms.ContentChannelDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" >

            <asp:HiddenField ID="hfId" runat="server" />
            <asp:HiddenField ID="hfTypeId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lIcon" runat="server" />
                    <asp:Literal ID="lTitle" runat="server" />
                </h1>
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlContentChannel" runat="server" LabelType="Type" />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

                <div id="pnlEditDetails" runat="server">

                    <Rock:ModalAlert ID="maContentChannelWarning" runat="server" />
                    <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <div class="row">
                        <div class="col-md-6">
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.ContentChannel, Rock" PropertyName="Name" />
                        </div>
                        <div class="col-md-6">
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.ContentChannel, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockDropDownList ID="ddlChannelType" runat="server" Label="Type" Required="true" AutoPostBack="true" OnSelectedIndexChanged="ddlChannelType_SelectedIndexChanged" />
                            <Rock:RockControlWrapper ID="rcContentChannels" runat="server" Label="Child Content Channels"
                                Help="The types of content channel items that can be added as children to items of this type. This is used to define the item hierarchy. To allow an unlimited hierarchy add this type as an allowed child content channel type.">
                                <div class="grid">
                                    <Rock:Grid ID="gChildContentChannels" runat="server" DisplayType="Light" ShowHeader="false" RowItemText="Content Channel">
                                        <Columns>
                                            <Rock:RockBoundField DataField="Name" />
                                            <Rock:DeleteField OnClick="gChildContentChannels_Delete" />
                                        </Columns>
                                    </Rock:Grid>
                                </div>
                            </Rock:RockControlWrapper>
                            <Rock:RockDropDownList ID="ddlContentControlType" runat="server" Label="Default Content Control" Required="true" AutoPostBack="true" OnSelectedIndexChanged="ddlContentControlType_SelectedIndexChanged" />
                            <Rock:RockTextBox ID="tbRootImageDirectory" runat="server" Label="Root Image Directory" Help="The path to use for the HTML editor's image folder root (e.g. '~/content/my_channel_images' ) " />
                            <Rock:RockCheckBox ID="cbRequireApproval" runat="server" Label="Items Require Approval" Text="Yes" />

                            <Rock:RockCheckBox ID="cbIndexChannel" runat="server" Label="Indexing Enabled" Text="Yes" />
                            <Rock:RockTextBox ID="tbContentChannelItemPublishingPoint" runat="server" Label="Content Channel Item Publishing Point" Help="Lava template to the URL that the content item can be viewed (Keys: 'Id', 'Title', 'ContentChannelId', 'Slug')." />

                            <div class="row">
                                <div class="col-xs-6">
                                    <Rock:RockCheckBox ID="cbItemsManuallyOrdered" runat="server" Label="Items Manually Ordered" Text="Yes" Help="Are items of this channel manually ordered? If not they will be ordered by their Start Date/Time." />
                                </div>
                                <div class="col-xs-6">
                                    <Rock:RockCheckBox ID="cbChildItemsManuallyOrdered" runat="server" Label="Child Items Manually Ordered" Text="Yes" Help="If items in this channel have child itesm, should those child items be manually ordered? If not they will be ordered by their Start Date/Time." />
                                </div>
                            </div>
                        </div>
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbIconCssClass" runat="server" Label="Icon CSS Class" SourceTypeName="Rock.Model.ContentChannel, Rock" PropertyName="IconCssClass" />
                            <Rock:DynamicPlaceHolder ID="phAttributes" runat="server" />
                            <Rock:RockCheckBox ID="cbEnableRss" runat="server" Label="Enable RSS" Text="Yes" CssClass="js-content-channel-enable-rss" />
                            <div id="divRss" runat="server" class="js-content-channel-rss"> 
                                <Rock:DataTextBox ID="tbChannelUrl" runat="server" Label="Channel Url" SourceTypeName="Rock.Model.ContentChannel, Rock" PropertyName="ChannelUrl" />
                                <Rock:DataTextBox ID="tbItemUrl" runat="server" Label="Item Url" SourceTypeName="Rock.Model.ContentChannel, Rock" PropertyName="ItemUrl" />
                                <Rock:NumberBox ID="nbTimetoLive" runat="server" Label="Time to Live (TTL)" NumberType="Integer" MinimumValue="0" 
                                    Help="The number of minutes a feed can stay cached before it is refreshed from the source."/>
                            </div>
                            <Rock:RockCheckBox ID="cbEnableTag" runat="server" Label="Enable Tagging" Help="When enabled, items can be tagged by editors however if categories (below) are used, the category must have 'Tag' security rights for people to use existing organizational tags." CssClass="js-content-channel-enable-tags" />
                            <div id="divTag" runat="server" class="js-content-channel-tags">
                                <Rock:CategoryPicker ID="cpCategory" runat="server" EntityTypeName="Rock.Model.Tag" Label="Tag Category" Help="Remember to apply appropriate security (action level 'Tag') to these categories." />

                            </div>
                        </div>
                    </div>

                    <Rock:PanelWidget ID="wpItemAttributes" runat="server" Title="Item Attributes">
                        <div class="grid">
                            <Rock:Grid ID="gItemAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Item Attribute" ShowConfirmDeleteDialog="false">
                                <Columns>
                                    <Rock:ReorderField />
                                    <Rock:RockBoundField DataField="Name" HeaderText="Attribute" />
                                    <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                    <Rock:BoolField DataField="IsRequired" HeaderText="Required" />
                                    <Rock:EditField OnClick="gItemAttributes_Edit" />
                                    <Rock:DeleteField OnClick="gItemAttributes_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </Rock:PanelWidget>

                    <div class="actions">
                        <asp:LinkButton ID="lbSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                        <asp:LinkButton ID="lbCancel" runat="server"  AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                    </div>

                </div>

                <fieldset id="fieldsetViewSummary" runat="server" >
                    <p class="description">
                        <asp:Literal ID="lGroupDescription" runat="server"></asp:Literal>
                    </p>

                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lDetailsLeft" runat="server" />
                        </div>
                        <div class="col-md-6">
                            <asp:Literal ID="lDetailsRight" runat="server" />
                        </div>
                    </div>
                    
                    <div class="actions">
                        <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CssClass="btn btn-primary" CausesValidation="false" OnClick="lbEdit_Click" />
                        <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-security pull-right" />
                    </div>

                </fieldset>

            </div>

        </asp:Panel>
        
        <Rock:ModalAlert ID="modalAlert" runat="server" />

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgChildContentChannel" runat="server" OnSaveClick="dlgChildContentChannel_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="ChildContentChannels">
            <Content>
                <Rock:RockDropDownList ID="ddlChildContentChannel" runat="server" DataTextField="Name" DataValueField="Id" Label="Child Group Type" ValidationGroup="ChildContentChannels" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgItemAttributes" runat="server" Title="Content Item Attributes" OnSaveClick="dlgItemAttributes_SaveClick"  OnCancelScript="clearActiveDialog();" ValidationGroup="ItemAttributes">
            <Content>
                <Rock:AttributeEditor ID="edtItemAttributes" runat="server" ShowActions="false" ValidationGroup="ItemAttributes" />
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
