<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SmsPipelineDetail.ascx.cs" Inherits="RockWeb.Blocks.Communication.SmsPipelineDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:LinkButton ID="lbDragCommand" runat="server" CssClass="hidden" />

        <div class="panel panel-block js-pipeline-panel">
            <div class="panel-heading">
                <h1 class="panel-title pull-left"><i class="fa fa-sms"></i>&nbsp;SMS Pipeline Detail</h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlInactive" runat="server" CssClass="js-inactivepipeline-label" LabelType="Danger" Text="Inactive" />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body" runat="server" id="divEditDetails">
                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbPipelineName" runat="server" SourceTypeName="Rock.Model.SmsPipeline, Rock" PropertyName="Name" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbPipelineIsActive" runat="server" Label="Active" CssClass="js-isactivepipeline" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-12">
                        <Rock:DataTextBox ID="tbPipelineDescription" runat="server" SourceTypeName="Rock.Model.SmsPipeline, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                    </div>
                </div>
                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_Click" CausesValidation="false" />
                </div>
            </div>

            <div class="panel-body" runat="server" id="divReadOnlyDetails">
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockLiteral ID="lSmsName" runat="server" Label="Name" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockLiteral ID="lWebhookUrl" runat="server" Label="Webhook URL" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-12">
                        <Rock:RockLiteral ID="lSmsPipelineDescription" runat="server" Label="Description" />
                    </div>
                </div>
                <div class="actions">
                    <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                    <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
                </div>
            </div>
        </div>

        <div class="panel panel-block sms-main-panel" runat="server" id="divSmsActionsPanel" ClientIDMode="Static">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-sms"></i>&nbsp;SMS Pipeline Actions</h1>

                <div class="panel-labels">
                    <a href="#" class="btn btn-xs btn-square btn-default" onclick="$('.js-test-results').slideToggle( function () { $('#hfIsTestingDrawerOpen').val( $('#divTestingDrawer').css('display') !== 'none' ) } )">
                        <i class='fa fa-cog'></i>
                    </a>
                </div>
            </div>

            <asp:HiddenField runat="server" ID="hfIsTestingDrawerOpen" Value="false" ClientIDMode="Static" />
            <div runat="server" ClientIDMode="Static" id="divTestingDrawer" class="js-test-results panel-drawer" style="display: none">
                <div class="drawer-content">
                    <div class="alert alert-info" role="alert">
                        <p>Using the form below will allow you to test your pipeline without having to use your SMS Provider.</p>
                        <p>While this message originates and ends here in this testing block, there could be side effects such as running a workflow, creating a conversation, or sending additional SMS messages.</p>
                    </div>

                    <div class="row">
                        <div class="col-sm-6">
                            <Rock:RockTextBox ID="tbFromNumber" runat="server" Label="From Number" Placeholder="+16235553322" Help="This is the number for the person sending the text. The number must be formatted correctly. (ex: +16235553322)" />
                        </div>
                        <div class="col-sm-6">
                            <Rock:RockTextBox ID="tbToNumber" runat="server" Label="To Number" Placeholder="+15559991234" Help="This will probably be the number linked to your SMS provider. The number must be formatted correctly. (ex: +15559991234)" />
                        </div>
                    </div>

                    <Rock:RockTextBox ID="tbSendMessage" runat="server" Label="Message" DisableTextTrim />

                    <asp:LinkButton ID="lbSendMessage" runat="server" Text="Send" CssClass="btn btn-primary margin-t-sm" OnClick="lbSendMessage_Click" />

                    <Rock:RockLiteral ID="lResponse" runat="server" Label="Response" CssClass="margin-t-lg" />

                    <div class="form-group static-control margin-t-lg">
                        <label class="control-label">Action Outcomes</label>
                        <div class="control-wrapper">
                            <pre runat="server" id="preOutcomes"></pre>
                        </div>
                    </div>
                </div>
            </div>

            <div class="panel-body padding-all-none">
                <div class="row row-eq-height row-no-gutters">
                    <div class="col-md-2 col-sm-6 js-sms-action-components sms-action-components">
                        <ul class="components-list list-unstyled">
                            <li>
                                <ol class="drag-container js-drag-container">
                                    <asp:Repeater ID="rptrComponents" runat="server">
                                        <ItemTemplate>
                                            <li class="component unselectable" data-component-id="<%# Eval( "Id" ) %>" data-toggle="tooltip" title="<%# Eval( "Description" ) %>">
                                                <i class="<%# Eval( "IconCssClass" ) %>"></i>
                                                <span><%# Eval( "Title" ) %></span>
                                            </li>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </ol>
                            </li>
                        </ul>
                    </div>

                    <div class="col-md-6 col-sm-6 js-sms-actions sms-actions-container">
                        <div class="sms-heading">
                            Incoming SMS Message
                        </div>
                        <ol id="olActions" runat="server" class="drag-container js-drag-container list-unstyled">
                            <asp:Repeater ID="rptrActions" runat="server" OnItemCommand="rptrActions_ItemCommand">
                                <ItemTemplate>
                                    <li class="sms-action unselectable<%# Eval( "IsActive" ).ToString() == "True" ? "" : " inactive" %><%# Eval( "Id" ).ToString() == hfEditActionId.Value ? " editing" : "" %>">
                                        <i class="<%# Eval( "Component.IconCssClass" ) %>"></i>
                                        <%# Eval( "Name" ) %>
                                        <div class="pull-right">
                                            <i class="fa fa-arrow-alt-circle-down<%# Eval( "ContinueAfterProcessing" ).ToString() == "True" ? "" : " hidden" %>"></i>
                                            <i class="fa fa-bars reorder js-reorder"></i>
                                            <asp:LinkButton ID="lbEditAction" runat="server" CssClass="js-edit-button hidden" CommandName="EditAction" CommandArgument='<%# Eval( "Id" ) %>' />
                                        </div>
                                    </li>
                                </ItemTemplate>
                            </asp:Repeater>
                        </ol>
                    </div>

                    <div class="col-md-4 col-sm-12">
                        <asp:Panel ID="pnlEditAction" runat="server" CssClass="js-sms-action-settings" Visible="false">
                            <asp:HiddenField ID="hfEditActionId" runat="server" />
                            <div class="panel panel-palette">
                                <div class="panel-heading">
                                    <div class="panel-title">
                                        <asp:Literal ID="lActionType" runat="server" />
                                    </div>
                                </div>

                                <div class="panel-body">

                                    <asp:ValidationSummary ID="valValidation" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                                    <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.SmsAction" PropertyName="Name" />

                                    <div class="row form-row">
                                        <div class="col-md-6">
                                            <Rock:RockCheckBox ID="cbActive" runat="server" Label="Active" Help="An action that is not active will not attempt to process any SMS messages." />
                                        </div>

                                        <div class="col-md-6">
                                            <Rock:RockCheckBox ID="cbContinue" runat="server" Label="Continue" Help="If the continue option is enabled then processing will continue even if this action successfully processes the SMS message." />
                                        </div>
                                    </div>
                                    <div class="row form-row">
                                        <div class="col-sm-12 text-right">
                                            <asp:HiddenField ID="hfShowAdvancedSettings" runat="server" />
                                            <asp:LinkButton ID="lbToggleAdvancedSettings" runat="server" OnClick="lbToggleAdvancedSettings_Click" CausesValidation="false" />
                                        </div>
                                    </div>
                                    <div class="row form-row" id="divAdvanced" runat="server">

                                        <div class="col-md-12">
                                            <Rock:DatePicker ID="dpExpireDate" runat="server" Label="Expire Date" Help="The Rock clean-up job will remove this SMS action after this date. The exact time and day will depend on your clean-up job schedule settings." />
                                        </div>
                                        <div class="col-md-12">
                                            <Rock:RockCheckBox ID="cbLogInteraction" runat="server" Label="Log Interaction When Action Runs" Help="If enabled, when the action runs, an interaction will be written that includes the data from the incoming message (Pipeline Name and Id, the Action's Name, ToPhone, FromPhone, MessageBody, ReceivedDateTime, FromPerson)" />
                                        </div>

                                    </div>

                                    <h4>Filters</h4>
                                    <Rock:AttributeValuesContainer ID="avcFilters" runat="server" ShowCategoryLabel="false" />

                                    <Rock:AttributeValuesContainer ID="avcAttributes" runat="server" />

                                    <div class="actions">
                                        <asp:LinkButton ID="btnSaveActionSettings" runat="server" CssClass="btn btn-primary" Text="Save" OnClick="btnSaveActionSettings_Click" />
                                        <asp:LinkButton ID="btnCancelActionSettings" runat="server" CssClass="btn btn-link" Text="Cancel" OnClick="btnCancelActionSettings_Click" CausesValidation="false" />

                                        <asp:LinkButton ID="btnDeleteAction" runat="server" CssClass="pull-right btn btn-danger" Text="Delete" OnClick="btnDeleteAction_Click" CausesValidation="false" />
                                    </div>
                                </div>
                            </div>
                        </asp:Panel>
                    </div>
                </div>
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
        $('.sms-action-components [data-toggle="tooltip"]').tooltip({
            delay: {
                show: 800,
                hide: 100
            }
        });

        function setIsActiveControls(activeCheckbox) {

            var $inactiveLabel = $(activeCheckbox).closest(".js-pipeline-panel").find('.js-inactivepipeline-label');
            if ($(activeCheckbox).is(':checked')) {
                $inactiveLabel.hide();
            }
            else {
                $inactiveLabel.show();
            }
        }

        $('.js-isactivepipeline').on('click', function () {
            setIsActiveControls(this);
        });

        $('.js-isactivepipeline').each(function (i) {
            setIsActiveControls(this);
        });
    });
</script>