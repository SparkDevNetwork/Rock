<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MobilePageDetail.ascx.cs" Inherits="RockWeb.Blocks.Mobile.MobilePageDetail" %>

<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfPageId" runat="server" />
        <asp:LinkButton ID="lbDragCommand" runat="server" CssClass="hidden" />

        <Rock:NotificationBox ID="nbError" runat="server" NotificationBoxType="Danger" />

        <div class="row">
            <div class="col-md-3 col-md-offset-9">
                <div class="form-horizontal label-sm">
                    <Rock:RockDropDownList ID="ddlPageList" EnhanceForLongLists="true" runat="server" Label="Page" AutoPostBack="true" OnSelectedIndexChanged="ddlPageList_SelectedIndexChanged" />
                </div>
            </div>
        </div>

        <asp:Panel ID="pnlDetails" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h3 class="panel-title"><i class="fa fa-mobile"></i> <asp:Literal ID="lPageName" runat="server" /></h3>

                <div class="panel-labels">
                    <button id="btnCopyToClipboard" runat="server" 
                        data-toggle="tooltip" data-placement="top" data-trigger="hover" data-delay="250" title="Copy Page Guid to Clipboard"
                        class="btn btn-info btn-xs btn-copy-to-clipboard"
                        onclick="$(this).attr('data-original-title', 'Copied').tooltip('show').attr('data-original-title', 'Copy Page Guid to Clipboard');return false;">
                        <i class='fa fa-clipboard'></i>
                    </button>
                </div>
            </div>

            <div class="panel-body">
                <div class="row">
                    <asp:Literal ID="ltDetails" runat="server" />
                </div>

                <div class="actions margin-t-md">
                    <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="lbEdit_Click" />
                    <asp:LinkButton ID="lbBack" runat="server" Text="Back" CssClass="btn btn-link" OnClick="lbBack_Click" CausesValidation="false" />

                    <div class="pull-right">
                        <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-square btn-security" />
                    </div>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlEditPage" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h3 class="panel-title">Page</h3>
            </div>

            <div class="panel-body">
                <asp:ValidationSummary ID="vsEditPage" runat="server" ValidationGroup="EditPage" CssClass="alert alert-danger" HeaderText="Please correct the following" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbName" runat="server" Label="Name" Required="true" ValidationGroup="EditPage" />
                    </div>

                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbInternalName" runat="server" Label="Internal Name" Required="true" ValidationGroup="EditPage" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlLayout" runat="server" Label="Layout" Required="true" ValidationGroup="EditPage" />
                    </div>

                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbDisplayInNavigation" runat="server" Label="Display In Navigation" ValidationGroup="EditPage" />
                    </div>
                </div>

                <Rock:RockTextBox ID="tbDescription" runat="server" Label="Description" TextMode="MultiLine" ValidationGroup="EditPage" />
                <Rock:ImageUploader ID="imgPageIcon" runat="server" Help="This image is used as a icon for your page." Label="Icon" />

                <Rock:PanelWidget ID="pwEditAdvancedSettings" runat="server" Title="Advanced Settings">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbCssClass" runat="server"
                                              SourceTypeName="Rock.Model.Page, Rock" PropertyName="BodyCssClass"
                                              Label="Body CSS Class"
                                              Help="The CSS class to add to the page." />
                        </div>

                        <div class="col-md-6">
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

                    <Rock:CodeEditor ID="ceEventHandler" runat="server" Label="Event Handler" Help="The lava to execute on the client whenever a page event is triggered." EditorMode="Lava" />

                    <Rock:CodeEditor ID="ceCssStyles" runat="server" Label="Page Scoped CSS" EditorMode="Css" Help="CSS styles that will only be applied to elements on this page." />
                </Rock:PanelWidget>

                <div class="actions margin-t-md">
                    <asp:LinkButton ID="lbSave" runat="server" CssClass="btn btn-primary" Text="Save" OnClick="lbSave_Click" ValidationGroup="EditPage" />
                    <asp:LinkButton ID="lbCancel" runat="server" CssClass="btn btn-link" Text="Cancel" OnClick="lbCancel_Click" CausesValidation="false" />
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlBlocks" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h3 class="panel-title"><i class="fa fa-th-large"></i> Blocks</h3>
            </div>

            <div class="panel-body padding-all-none">
                <div class="row row-eq-height row-no-gutters">
                    <div class="col-lg-3 col-sm-4 hidden-xs js-mobile-block-types" style="background: #f3f3f3;">
                        <Rock:RockDropDownList ID="ddlBlockTypeCategory" runat="server" OnSelectedIndexChanged="ddlBlockTypeCategory_SelectedIndexChanged" AutoPostBack="true" CausesValidation="false" />
                        <ul class="components-list list-unstyled">
                            <li>
                                <ol class="drag-container js-drag-container">
                                    <asp:Repeater ID="rptrBlockTypes" runat="server">
                                        <ItemTemplate>
                                            <li class="component" data-component-id="<%# Eval( "Id" ) %>">
                                                <i class="<%# Eval( "IconCssClass" ) %>"></i>
                                                <span><%# Eval( "Name" ) %></span>
                                            </li>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </ol>
                            </li>
                        </ul>

                    </div>

                    <div class="col-lg-9 col-sm-8 col-xs-12 js-mobile-zones">
                        <Rock:NotificationBox ID="nbZoneError" runat="server" NotificationBoxType="Danger" CssClass="margin-t-md margin-l-md margin-r-md" />

                        <asp:Repeater ID="rptrZones" runat="server" OnItemDataBound="rptrZones_ItemDataBound">
                            <ItemTemplate>
                                <div class="padding-all-md">
                                    <div data-zone-name="<%# Eval( "Name" ) %>" class="panel panel-default js-block-zone">
                                        <div class="panel-heading"><strong><%# Eval( "Name" ) %></strong></div>

                                        <div class="drag-container js-drag-container list-unstyled panel-body mobile-pages-container" style="min-height: 100px;">
                                            <asp:Repeater ID="rptrBlocks" runat="server" OnItemCommand="rptrBlocks_ItemCommand" OnItemDataBound="rptrBlocks_ItemDataBound">
                                                <ItemTemplate>
                                                    <div class="panel panel-widget">
                                                        <div class="panel-heading js-block clearfix" data-block-id="<%# Eval( "Id" ) %>">

                                                            <div class="pull-left">
                                                                <i class="<%# Eval( "IconCssClass" ) %>"></i>
                                                            </div>
                                                                
                                                            <div class="pull-left margin-l-md leading-snug">
                                                                <span><%# Eval( "Name" ) %></span> <br /><small class="margin-t-none"><%# Eval( "Type" ) %></small>
                                                            </div>

                                                            <div class="pull-right">

                                                                <a class="btn btn-default btn-sm btn-link panel-widget-reorder">
                                                                    <i class="fa fa-bars js-reorder"></i>
                                                                </a>

                                                                <asp:PlaceHolder ID="phAdminButtons" runat="server" />
                                                            </div>

                                                            <div class="pull-right padding-t-sm margin-r-lg">
                                                                <asp:PlaceHolder ID="phSettings" runat="server" />
                                                            </div>
                                                            
                                                        </div>
                                                    </div>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                        </div>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </div>
            </div>

            <%--  This will hold blocks that need to be added to the page so that Custom Admin actions will work --%>
            <%-- Display -9999 offscreen. This will hopefully hide everything except for any modals that get shown with the Custom Action --%>
            <asp:Panel ID="pnlBlocksHolder" runat="server" Style="position: absolute; left: -9999px">
            </asp:Panel>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>

<script>
    Sys.Application.add_load(function () {
        //
        // Setup dragula to allow dragging from components to actions.
        //
        var componentContainers = [];
        $('.js-block-zone .js-drag-container').each(function () { componentContainers.push($(this).get(0)); });
        var componentDrake = dragula(componentContainers.concat([$('.js-mobile-block-types .js-drag-container').get(0)]), {
            moves: function (el, source, handle, sibling) {
                return $(el).hasClass('component');
            },
            accepts: function (el, target, source, sibling) {
                return $(target).hasClass('js-drag-container');
            },
            copy: true,
            revertOnSpill: true
        });

        componentDrake.on('drop', function (el, target, source, sibling) {
            var component = $(el).data('component-id');
            var order = $(target).children().index(el);
            var zone = $(target).closest('.js-block-zone').data('zone-name');
            var postback = "javascript:__doPostBack('<%= lbDragCommand.ClientID %>', 'add-block|" + component + "|" + zone + "|" + order + "')";
            window.location = postback;
        });

        //
        // Setup dragula to allow re-ordering within the actions.
        //
        var reorderOldIndex = -1;
        var reorderDrake = dragula(componentContainers, {
            moves: function (el, source, handle, sibling) {
                reorderOldIndex = $(source).children().index(el);
                return $(handle).hasClass('js-reorder');
            },
            revertOnSpill: true
        });

        reorderDrake.on('drop', function (el, target, source, sibling) {
            var newIndex = $(target).children().index(el);
            var zone = $(el).closest('.js-block-zone').data('zone-name');
            var blockId = $(el).find('.js-block').data('block-id');
            if (reorderOldIndex !== newIndex) {
                var postback = "javascript:__doPostBack('<%= lbDragCommand.ClientID %>', 'reorder-block|" + zone + "|" + blockId + "|" + newIndex + "')";
                window.location = postback;
            }
        });
    });
</script>
