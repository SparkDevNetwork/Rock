<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SmsActions.ascx.cs" Inherits="RockWeb.Blocks.Communication.SmsActions" %>

<style>
.sms-container {
    min-height: 25px;
    border: 1px solid #ddd;
    background-color: #eee;
    padding: 12px;
}
    .sms-container > div {
        margin-bottom: 12px;
    }
    .sms-container > div:last-child {
        margin-bottom: 0px;
    }
.sms-action-component
{
    border: 1px solid #666;
    padding: 6px;
    height: 43px;
}
    .sms-action-component > .fa {
        border: 1px solid #888;
        padding: 2px;
    }
.sms-action
{
    border: 1px solid #666;
    padding: 6px;
    height: 43px;
}
    .sms-action.inactive {
        font-style: italic;
    }
    .sms-action.editing {
        background-color: #cce3e9;
    }
    .sms-action > .fa {
        border: 1px solid #888;
        padding: 2px;
    }
    .sms-action:first-child .js-move-up {
        display: none;
    }
    .sms-action:last-child .js-move-down {
        display: none;
    }
</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-sms"></i>&nbsp;SMS Actions</h1>
            </div>

            <div class="panel-body">
                <div class="row">
                    <div class="col-md-3">
                        <div class="js-sms-action-components sms-container">
                        <asp:Repeater ID="rptrComponents" runat="server" OnItemCommand="rptrComponents_ItemCommand">
                            <ItemTemplate>
                                <div class="sms-action-component">
                                    <i class="<%# Eval( "IconCssClass" ) %>"></i>
                                    <%# Eval( "Title" ) %>
                                    <div class="pull-right">
                                        <asp:LinkButton ID="lbAddComponent" runat="server" CssClass="btn btn-primary btn-xs" CommandName="AddComponent" CommandArgument='<%# Eval( "Id" ) %>'>
                                            <i class="fa fa-plus"></i>
                                        </asp:LinkButton>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                        </div>
                    </div>

                    <div class="col-md-4">
                        <div class="js-sms-actions sms-container">
                            <asp:Repeater ID="rptrActions" runat="server" OnItemCommand="rptrActions_ItemCommand">
                                <ItemTemplate>
                                    <div class="sms-action<%# Eval( "IsActive" ).ToString() == "True" ? "" : " inactive" %><%# Eval( "Id" ).ToString() == hfEditActionId.Value ? " editing" : "" %>">
                                        <i class="<%# Eval( "Component.IconCssClass" ) %>"></i>
                                        <%# Eval( "Title" ) %>
                                        <div class="pull-right">
                                            <asp:LinkButton ID="lbMoveUp" runat="server" CssClass="btn btn-default btn-xs js-move-up" CommandName="MoveUp" CommandArgument='<%# Eval( "Id" ) %>'>
                                                <i class="fa fa-arrow-up"></i>
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="lbMoveDown" runat="server" CssClass="btn btn-default btn-xs js-move-down" CommandName="MoveDown" CommandArgument='<%# Eval( "Id" ) %>'>
                                                <i class="fa fa-arrow-down"></i>
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="lbEditAction" runat="server" CssClass="btn btn-default btn-xs" CommandName="EditAction" CommandArgument='<%# Eval( "Id" ) %>'>
                                                <i class="fa fa-pencil"></i>
                                            </asp:LinkButton>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </div>

                    <div class="col-md-5">
                        <asp:Panel ID="pnlEditAction" runat="server" CssClass="js-sms-action-settings sms-container" Visible="false">
                            <asp:HiddenField ID="hfEditActionId" runat="server" />

                            <Rock:RockLiteral ID="lActionType" runat="server" Label="Action Type" />

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockCheckBox ID="cbActive" runat="server" Label="Active" Help="An action that is not active will not attempt to process any SMS messages." />
                                </div>

                                <div class="col-md-6">
                                    <Rock:RockCheckBox ID="cbContinue" runat="server" Label="Continue" Help="If the continue option is enabled then processing will continue even if this action successfully processes the SMS message." />
                                </div>
                            </div>

                            <Rock:DataTextBox ID="tbTitle" runat="server" SourceTypeName="Rock.Model.SmsAction" PropertyName="Title" />

                            <Rock:AttributeValuesContainer ID="avcAttributes" runat="server" />

                            <div class="actions">
                                <asp:LinkButton ID="btnSaveActionSettings" runat="server" CssClass="btn btn-primary" Text="Save" OnClick="btnSaveActionSettings_Click" />
                                <asp:LinkButton ID="btnCancelActionSettings" runat="server" CssClass="btn btn-link" Text="Cancel" OnClick="btnCancelActionSettings_Click" />

                                <asp:LinkButton ID="btnDeleteAction" runat="server" CssClass="pull-right btn btn-danger" Text="Delete" OnClick="btnDeleteAction_Click" />
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
});
</script>