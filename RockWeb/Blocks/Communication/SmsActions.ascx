<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SmsActions.ascx.cs" Inherits="RockWeb.Blocks.Communication.SmsActions" %>

<style>
.sms-container {
    min-height: 25px;
    border: 1px solid #ddd;
    background-color: #eee;
    padding: 12px;
    margin-bottom: 12px;
}
    .sms-container .sms-action-component,
    .sms-container .sms-action {
        margin-top: 12px;
    }
    .sms-container .sms-action-component:first-child,
    .sms-container .sms-action:first-child {
        margin-top: 0px;
    }
    .sms-container .sms-header {
        text-align: center;
        font-weight: bold;
        margin-bottom: 12px;
    }

.sms-action-component
{
    border: 1px solid #666;
    padding: 6px;
    height: 37px;
    cursor: move;
}
    .sms-action-component > .fa {
        border: 1px solid #888;
        padding: 2px;
    }

.sms-action
{
    border: 1px solid #666;
    padding: 6px;
    height: 37px;
    cursor: pointer;
}
    .sms-action.inactive {
        font-style: italic;
        color: #aaa;
    }
    .sms-action.editing {
        background-color: #cce3e9;
    }
    .sms-action > .fa {
        border: 1px solid #888;
        padding: 2px;
    }
    .sms-action .js-reorder {
        cursor: move;
    }

/* Dragula */
.gu-mirror {
  position: fixed !important;
  margin: 0 !important;
  z-index: 9999 !important;
  opacity: 0.8;
  -ms-filter: "progid:DXImageTransform.Microsoft.Alpha(Opacity=80)";
  filter: alpha(opacity=80);
}
.gu-hide {
  display: none !important;
}
.gu-unselectable {
  -webkit-user-select: none !important;
  -moz-user-select: none !important;
  -ms-user-select: none !important;
  user-select: none !important;
}
.gu-transit {
  opacity: 0.2;
  -ms-filter: "progid:DXImageTransform.Microsoft.Alpha(Opacity=20)";
  filter: alpha(opacity=20);
}
</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:LinkButton ID="lbDragCommand" runat="server" CssClass="hidden" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-sms"></i>&nbsp;SMS Actions</h1>
            </div>

            <div class="panel-body">
                <div class="row">
                    <div class="col-md-3 col-sm-6">
                        <div class="js-sms-action-components sms-container">
                            <div class="sms-header">
                                Components
                            </div>

                            <div class="js-drag-container">
                                <asp:Repeater ID="rptrComponents" runat="server">
                                    <ItemTemplate>
                                        <div class="sms-action-component" data-component-id="<%# Eval( "Id" ) %>" data-toggle="tooltip" title="<%# Eval( "Description" ) %>">
                                            <i class="<%# Eval( "IconCssClass" ) %>"></i>
                                            <%# Eval( "Title" ) %>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </div>
                    </div>

                    <div class="col-md-4 col-sm-6">
                        <div class="js-sms-actions sms-container">
                            <div class="sms-header">
                                Actions
                            </div>

                            <div class="js-drag-container">
                                <asp:Repeater ID="rptrActions" runat="server" OnItemCommand="rptrActions_ItemCommand">
                                    <ItemTemplate>
                                        <div class="sms-action<%# Eval( "IsActive" ).ToString() == "True" ? "" : " inactive" %><%# Eval( "Id" ).ToString() == hfEditActionId.Value ? " editing" : "" %>">
                                            <i class="<%# Eval( "Component.IconCssClass" ) %>"></i>
                                            <%# Eval( "Name" ) %>
                                            <div class="pull-right">
                                                <i class="fa fa-arrow-alt-circle-right<%# Eval( "ContinueAfterProcessing" ).ToString() == "True" ? "" : " hidden" %>"></i>
                                                <i class="fa fa-bars js-reorder"></i>
                                                <asp:LinkButton ID="lbEditAction" runat="server" CssClass="js-edit-button hidden" CommandName="EditAction" CommandArgument='<%# Eval( "Id" ) %>' />
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </div>
                    </div>

                    <div class="col-md-5 col-sm-12">
                        <asp:Panel ID="pnlEditAction" runat="server" CssClass="js-sms-action-settings sms-container" Visible="false">
                            <asp:HiddenField ID="hfEditActionId" runat="server" />

                            <div class="sms-header">
                                Action Settings
                            </div>

                            <Rock:RockLiteral ID="lActionType" runat="server" Label="Action Type" />

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockCheckBox ID="cbActive" runat="server" Label="Active" Help="An action that is not active will not attempt to process any SMS messages." />
                                </div>

                                <div class="col-md-6">
                                    <Rock:RockCheckBox ID="cbContinue" runat="server" Label="Continue" Help="If the continue option is enabled then processing will continue even if this action successfully processes the SMS message." />
                                </div>
                            </div>

                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.SmsAction" PropertyName="Name" />

                            <Rock:AttributeValuesContainer ID="avcAttributes" runat="server" />

                            <div class="actions">
                                <asp:LinkButton ID="btnSaveActionSettings" runat="server" CssClass="btn btn-primary" Text="Save" OnClick="btnSaveActionSettings_Click" />
                                <asp:LinkButton ID="btnCancelActionSettings" runat="server" CssClass="btn btn-link" Text="Cancel" OnClick="btnCancelActionSettings_Click" />

                                <asp:LinkButton ID="btnDeleteAction" runat="server" CssClass="pull-right btn btn-danger" Text="Delete" OnClick="btnDeleteAction_Click" CausesValidation="false" />
                            </div>
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </div>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">Test Message</h1>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col-sm-6">
                        <Rock:RockTextBox ID="tbFromNumber" runat="server" Label="From Number" />
                    </div>
                    <div class="col-sm-6">
                        <Rock:RockTextBox ID="tbToNumber" runat="server" Label="To Number" />
                    </div>
                </div>

                <Rock:RockTextBox ID="tbSendMessage" runat="server" Label="Message" />

                <asp:LinkButton ID="lbSendMessage" runat="server" Text="Send" CssClass="btn btn-primary margin-t-sm" OnClick="lbSendMessage_Click" />

                <Rock:RockLiteral ID="lResponse" runat="server" Label="Response" CssClass="margin-t-lg" />
            </div>

        </div>

    </ContentTemplate>
</asp:UpdatePanel>

<script>
    Sys.Application.add_load(function () {
        //
        // Setup dragula to allow dragging from components to actions.
        //
        var componentDrake = dragula([$('.js-sms-action-components .js-drag-container').get(0), $('.js-sms-actions .js-drag-container').get(0)], {
            moves: function (el, source, handle, sibling) {
                return $(el).hasClass('sms-action-component');
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
            var postback = "javascript:__doPostBack('<%= lbDragCommand.ClientID %>', 'add-action|" + component + "|" + order + "')";
            window.location = postback;
        });

        //
        // Setup dragula to allow re-ordering within the actions.
        //
        var reorderOldIndex = -1;
        var reorderDrake = dragula([$('.js-sms-actions .js-drag-container').get(0), $('.js-sms-actions .js-drag-container').get(0)], {
            moves: function (el, source, handle, sibling) {
                reorderOldIndex = $(source).children().index(el);
                return $(handle).hasClass('js-reorder');
            },
            revertOnSpill: true
        });

        reorderDrake.on('drop', function (el, target, source, sibling) {
            var newIndex = $(target).children().index(el);
            if (reorderOldIndex !== newIndex) {
                var postback = "javascript:__doPostBack('<%= lbDragCommand.ClientID %>', 'reorder-action|" + reorderOldIndex + "|" + newIndex + "')";
                window.location = postback;
            }
        });

        //
        // Bit of a cheat, probably a safer way to do this.
        //
        $('.sms-action').on('click', function (e) {
            e.preventDefault();
            window.location = $(this).find('.js-edit-button').attr('href');
        });

        //
        // Turn on tooltips.
        //
        $('.sms-container [data-toggle="tooltip"]').tooltip({
            delay: {
                show: 500,
                hide: 100
            }
        });
    });
</script>