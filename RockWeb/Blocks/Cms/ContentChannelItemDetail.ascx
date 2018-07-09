<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContentChannelItemDetail.ascx.cs" Inherits="RockWeb.Blocks.Cms.ContentChannelItemDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
    $(document).ready(function () {
        contentSlug.init({
            contentChannelItem: '#<%=hfId.ClientID %>',
            contentSlug: '#<%=hfSlug.ClientID %>',
            SaveSlug: {
                restUrl: '<%=ResolveUrl( "~/api/ContentChannelItemSlugs/SaveContentSlug" ) %>',
                restParams: '/' + ($('#<%=hfId.ClientID%>').val() || 0) + "/{slug}/{contentChannelItemSlugId?}",
            },
            UniqueSlug: {
                restUrl: '<%=ResolveUrl( "~/api/ContentChannelItemSlugs/GetUniqueContentSlug" ) %>',
                restParams: '/{slug}'
            },
            RemoveSlug: {
               restUrl: '<%=ResolveUrl( "~/api/ContentChannelItemSlugs" ) %>',
                restParams: '/{id}'
            },
            txtTitle: '#<%=tbTitle.ClientID %>'
        });
    });

</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <asp:HiddenField ID="hfIsDirty" runat="server" Value="false" />
            <asp:HiddenField ID="hfId" runat="server" />
            <asp:HiddenField ID="hfSlug" runat="server" />
            <asp:HiddenField ID="hfChannelId" runat="server" />
            <asp:HiddenField ID="hfApprovalStatusPersonAliasId" runat="server" />
            <asp:HiddenField ID="hfApprovalStatus" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lIcon" runat="server" />
                    <asp:Literal ID="lTitle" runat="server" />
                </h1>
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlContentChannel" runat="server" LabelType="Type" />
                    <Rock:HighlightLabel ID="hlStatus" runat="server" />
                    <asp:PlaceHolder ID="phOccurrences" runat="server" />
                </div>
            </div>

            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>

            <div class="panel-body">

                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-validation" />
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <asp:Panel ID="pnlEditDetails" runat="server" CssClass="js-item-details">

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbTitle" runat="server" SourceTypeName="Rock.Model.ContentChannelItem, Rock" PropertyName="Title" />
                        </div>
                        <div class="col-md-3">
                            <div class="form-group" id="divStatus" runat="server">

                                <div class="form-control-static">
                                    <asp:HiddenField ID="hfStatus" runat="server" />
                                    <asp:Panel ID="pnlStatus" runat="server">
                                        <label class="control-label">Status</label>

                                        <div class="toggle-container">
                                            <div class="btn-group btn-toggle">
                                                <a class="btn btn-xs <%=PendingCss%>" data-status="1" data-active-css="btn-warning">Pending</a>
                                                <a class="btn btn-xs <%=ApprovedCss%>" data-status="2" data-active-css="btn-success">Approved</a>
                                                <a class="btn btn-xs <%=DeniedCss%>" data-status="3" data-active-css="btn-danger">Denied</a>
                                            </div>
                                        </div>
                                    </asp:Panel>
                                </div>
                            </div>
                        </div>
                        <div class="col-md-3">
                            <Rock:NumberBox ID="nbPriority" runat="server" Label="Priority" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-3">
                            <Rock:RockControlWrapper ID="rcwSlugs" runat="server" Label="Slugs">
                                <div class="js-slugs">
                                    <asp:Repeater ID="rSlugs" runat="server">
                                        <ItemTemplate>
                                            <div class="row margin-l-sm margin-b-sm rollover-container js-slug-row clearfix">
                                                <input id="slugId" class="js-slug-id" type="hidden" value="<%# Eval("Id") %>" />
                                                <literal class="js-slug-literal"> <%# Eval("Slug") %> </literal>
                                                <div class="rollover-item actions pull-right">
                                                    <a class="js-slug-edit" href="#"><i class="fa fa-pencil"></i></a>
                                                    <a class="js-slug-remove" href="#"><i class="fa fa-close"></i></a>
                                                </div>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                    <a id="lbAdd" class="btn btn-xs btn-action margin-l-sm">
                                        <i class="fa fa-plus-circle"></i>
                                    </a>
                                </div>
                            </Rock:RockControlWrapper>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DatePicker ID="dpStart" runat="server" Label="Start" Required="true" Visible="false" />
                            <Rock:DateTimePicker ID="dtpStart" runat="server" Label="Start" Required="true" />
                        </div>
                        <div class="col-md-6">
                            <Rock:DatePicker ID="dpExpire" runat="server" Label="Expire" Required="true" Visible="false" />
                            <Rock:DateTimePicker ID="dtpExpire" runat="server" Label="Expire" />
                        </div>
                    </div>

                    <Rock:RockControlWrapper ID="rcwTags" runat="server" Label="Tags">
                        <Rock:TagList ID="taglTags" runat="server" CssClass="clearfix" />
                    </Rock:RockControlWrapper>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:HtmlEditor ID="htmlContent" runat="server" Label="Content" ResizeMaxWidth="720" Height="300" />
                        </div>
                    </div>


                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DynamicPlaceholder ID="phAttributes" runat="server" />
                        </div>
                    </div>



                    <div class="actions">
                        <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                        <asp:LinkButton ID="lbDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="lbDelete_Click" />
                        <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                    </div>

                </asp:Panel>

            </div>

        </asp:Panel>

        <asp:Panel ID="pnlChildrenParents" runat="server" CssClass="panel panel-widget">

            <div class="panel-heading">
                <asp:Literal ID="lChildrenParentsTitle" runat="server" Text="Related Items" />
            </div>

            <div class="panel-body">
                <asp:HiddenField ID="hfActivePill" runat="server" />
                <asp:PlaceHolder ID="phPills" runat="server">
                    <ul class="nav nav-pills">
                        <li id="liChildren" runat="server" class="active"><a href='#<%=divChildItems.ClientID%>' data-toggle="pill">Child Items</a></li>
                        <li id="liParents" runat="server"><a href='#<%=divParentItems.ClientID%>' data-toggle="pill">Parent Items</a></li>
                    </ul>
                </asp:PlaceHolder>

                <div class="tab-content margin-t-lg">
                    <div id="divChildItems" runat="server" class="tab-pane active">
                        <Rock:Grid ID="gChildItems" runat="server" DisplayType="Light" EmptyDataText="No Child Items" RowItemText="Child Item" ShowConfirmDeleteDialog="false" OnRowSelected="gChildItems_RowSelected">
                            <Columns>
                                <Rock:ReorderField />
                                <Rock:RockBoundField DataField="Title" HeaderText="Title" SortExpression="Title" />
                                <Rock:DateField DataField="StartDateTime" HeaderText="Start" SortExpression="StartDateTime" ColumnPriority="Desktop" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" />
                                <Rock:DateField DataField="ExpireDateTime" HeaderText="Expire" SortExpression="ExpireDateTime" ColumnPriority="Desktop" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" />
                                <Rock:RockBoundField DataField="Priority" HeaderText="Priority" SortExpression="Priority" DataFormatString="{0:N0}" ColumnPriority="Desktop" />
                                <Rock:RockBoundField DataField="Status" HeaderText="Status" SortExpression="Status" HtmlEncode="false" ColumnPriority="Desktop" />
                                <Rock:DeleteField OnClick="gChildItems_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                    <div id="divParentItems" runat="server" class="tab-pane">
                        <Rock:Grid ID="gParentItems" runat="server" DisplayType="Light" EmptyDataText="No Child Items" RowItemText="Parent Item" OnRowSelected="gParentItems_RowSelected">
                            <Columns>
                                <Rock:RockBoundField DataField="Title" HeaderText="Title" SortExpression="Title" />
                                <Rock:DateField DataField="StartDateTime" HeaderText="Start" SortExpression="StartDateTime" ColumnPriority="Desktop" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" />
                                <Rock:DateField DataField="ExpireDateTime" HeaderText="Expire" SortExpression="ExpireDateTime" ColumnPriority="Desktop" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" />
                                <Rock:RockBoundField DataField="Priority" HeaderText="Priority" SortExpression="Priority" DataFormatString="{0:N0}" ColumnPriority="Desktop" />
                                <Rock:RockBoundField DataField="Status" HeaderText="Status" SortExpression="Status" HtmlEncode="false" ColumnPriority="Desktop" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>

            </div>

        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgAddChild" runat="server" Title="Add Child Item" OnCancelScript="clearActiveDialog();" CancelLinkVisible="false" ValidationGroup="AddChild">
            <Content>
                <div class="row">
                    <div class="col-md-6">
                        <asp:ValidationSummary ID="valSummaryAddChildNew" runat="server" ValidationGroup="AddChildNew" HeaderText="Please Correct the Following" CssClass="alert alert-validation" />
                        <Rock:RockDropDownList ID="ddlAddNewItemChannel" runat="server" Label="Add New Item" ValidationGroup="AddChildNew" Required="true"></Rock:RockDropDownList>
                        <asp:LinkButton ID="lbAddNewChildItem" runat="server" CssClass="btn btn-primary" Text="Add" ValidationGroup="AddChildNew" OnClick="lbAddNewChildItem_Click" />
                    </div>
                    <div class="col-md-6">
                        <asp:ValidationSummary ID="valSummaryAddChildExisting" runat="server" ValidationGroup="AddChildExisting" HeaderText="Please Correct the Following" CssClass="alert alert-validation" />
                        <Rock:RockDropDownList ID="ddlAddExistingItemChannel" runat="server" Label="Add Existing Item" ValidationGroup="AddChildExisting" Required="true" AutoPostBack="true" OnSelectedIndexChanged="ddlAddExistingItemChannel_SelectedIndexChanged" CausesValidation="false"></Rock:RockDropDownList>
                        <Rock:RockDropDownList ID="ddlAddExistingItem" runat="server" Label="Item" ValidationGroup="AddChildExisting" Required="true"></Rock:RockDropDownList>
                        <asp:LinkButton ID="lbAddExistingChildItem" runat="server" CssClass="btn btn-primary" Text="Add" ValidationGroup="AddChildExisting" OnClick="lbAddExistingChildItem_Click" />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgRemoveChild" runat="server" Title="Remove Child Item" OnCancelScript="clearActiveDialog();" CancelLinkVisible="false" ValidationGroup="RemoveChild">
            <Content>
                <asp:HiddenField ID="hfRemoveChildItem" runat="server" />
                <asp:LinkButton ID="lbRemoveChildItem" runat="server" CssClass="btn btn-primary btn-block" Text="Remove Child Item" OnClick="lbRemoveChildItem_Click" />
                <asp:LinkButton ID="lbDeleteChildItem" runat="server" CssClass="btn btn-primary btn-block" Text="Delete Child Item" OnClick="lbDeleteChildItem_Click" />
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
