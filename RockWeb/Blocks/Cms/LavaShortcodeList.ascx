<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LavaShortcodeList.ascx.cs" Inherits="RockWeb.Blocks.Cms.LavaShortcodeList" %>

<asp:UpdatePanel ID="upLavaShortcode" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Danger" Visible="true" />

        <script>
            Sys.Application.add_load(function () {
                $('.js-shortcode-toggle').on('click', function () {
                    $(this).closest('.panel').toggleClass('collapsed').find('.panel-body').slideToggle();
                    $(this).find('.example-toggle i').toggleClass('fa-circle-o fa-circle');
                });
            });
        </script>

        <asp:Panel ID="pnlLavaShortcodeList" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-cube"></i>Lava Shortcodes</h1>
                <div class="panel-labels">
                    <asp:LinkButton ID="btnAddShortcut" runat="server" CssClass="btn btn-xs btn-default btn-square" OnClick="btnAddShortcut_Click" Title="Add Shortcode">
                            <i class="fa fa-plus"></i>
                    </asp:LinkButton>
                </div>
            </div>

            <div class="panel-body">
                <div class="d-flex flex-wrap justify-content-between align-items-center mb-3">
                    <Rock:Switch ID="swShowInactive" runat="server" OnCheckedChanged="swShowInactive_CheckedChanged" Text="Show Inactive" AutoPostBack="true" />

                    <Rock:RockDropDownList ID="ddlCategoryFilter" runat="server" CssClass="input-width-xl input-sm mt-1 mt-sm-0" OnSelectedIndexChanged="ddlCategoryFilter_SelectedIndexChanged" AutoPostBack="true"/>
                </div>
                <asp:Repeater ID="rptShortcodes" runat="server" OnItemDataBound="rptShortcodes_ItemDataBound">
                    <ItemTemplate>
                        <div class="panel panel-widget panel-shortcodeitem collapsed">
                            <div class="panel-heading cursor-pointer js-shortcode-toggle flex-column flex-sm-row align-items-start align-items-sm-center">
                                <a name="<%# Eval("TagName").ToString().ToLower() %>"></a>
                                <div class="order-1">
                                    <h1 class="panel-title mb-1"><%# Eval("Name") %></h1>
                                    <p class="text-sm text-muted w-auto">
                                        <%# Eval("Description") %>
                                    </p>
                                </div>
                                <div class="ml-sm-auto order-0 order-sm-1">
                                    <div class="example-toggle text-nowrap text-right d-none d-sm-block">
                                        <i class="fa fa-circle-o"></i> Show Details
                                    </div>
                                    <div class="mb-2 mb-sm-0 mt-sm-2 text-right">
                                        <%# !Boolean.Parse(Eval("IsActive").ToString()) ? "<span class='label label-warning'>Inactive</span>" : "" %>
                                        <%# Boolean.Parse(Eval("IsSystem").ToString()) ? "<span class='label label-default'>System</span>" : "" %>
                                        <asp:Literal ID="litCategories" runat="server"></asp:Literal>
                                    </div>
                                </div>

                            </div>
                            <div class="panel-body" style="display: none;">
                                <%# Eval("Documentation") %>
                                <asp:Literal ID="lMessages" runat="server" />
                                <asp:HiddenField ID="hfShortcodeId" runat="server" Value='<%# DataBinder.Eval(Container.DataItem, "Id").ToString() %>' />
                                <div id="divViewPanel" runat="server" class="pull-right">
                                    <asp:LinkButton ID="btnView" runat="server" CssClass="btn btn-default btn-xs btn-square" OnClick="btnEdit_Click"><i class="fa fa-search"></i></asp:LinkButton>
                                </div>
                                <div id="divEditPanel" runat="server" class="pull-right">
                                    <asp:LinkButton ID="btnEdit" runat="server" CssClass="btn btn-default btn-xs btn-square" OnClick="btnEdit_Click"><i class="fa fa-pencil"></i></asp:LinkButton>
                                    <asp:LinkButton ID="btnDelete" runat="server" CssClass="btn btn-danger btn-xs btn-square" OnClick="btnDelete_Click" OnClientClick="return Rock.dialogs.confirmDelete(event, 'Lava Shortcode');"><i class="fa fa-times"></i></asp:LinkButton>
                                </div>
                            </div>
                        </div>

                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
