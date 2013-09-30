<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowTypeDetail.ascx.cs" Inherits="RockWeb.Blocks.Administration.WorkflowTypeDetail" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false">
            <asp:HiddenField ID="hfWorkflowTypeId" runat="server" />

            <div class="banner">
                <h1>
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </h1>

                <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                <Rock:HighlightLabel ID="hlType" runat="server" LabelType="Type" />
            </div>
            
            <asp:ValidationSummary ID="vsDetails" runat="server" CssClass="alert alert-error" />

            <div id="pnlEditDetails" runat="server">

                <fieldset>

                    <div class="row-fluid">
                        <div class="span6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.WorkflowType, Rock" PropertyName="Name" />
                        </div>
                        <div class="span6">
                            <Rock:RockCheckBox ID="cbIsActive" runat="server" Text="Active" />
                            
                        </div>
                    </div>

                    <div class="row-fluid">
                        <div class="span12">
                            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.WorkflowType, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                        </div>
                    </div>

                     <div class="row-fluid">
                        <div class="span6">
                            <Rock:DataTextBox ID="tbWorkTerm" runat="server" SourceTypeName="Rock.Model.WorkflowType, Rock" PropertyName="WorkTerm" Label="Work Term" />
                            <Rock:RockDropDownList ID="ddlLoggingLevel" Help="The level you would like to audit.  Start and stop times can be logged for each workflow, workflow activity, or activity action." runat="server" Label="Logging Level" />
                        </div>
                         <div class="span6">
                            <Rock:CategoryPicker ID="cpCategory" runat="server" Required="true" Label="Category" EntityTypeName="Rock.Model.WorkflowType" />
                            <Rock:DataTextBox ID="tbProcessingInterval" runat="server" SourceTypeName="Rock.Model.WorkflowType, Rock" PropertyName="ProcessingIntervalSeconds" Label="Processing Interval (seconds)" />
                            <Rock:RockCheckBox ID="cbIsPersisted" runat="server" Text="Persisted" />
                            <Rock:DataTextBox ID="tbOrder" runat="server" CssClass="input-mini" SourceTypeName="Rock.Model.WorkflowType, Rock" PropertyName="Order" Label="Order" Required="true" />
                        </div>
                    </div>
                </fieldset>

                <fieldset>
                    <legend>
                        <asp:Literal ID="lActivitiesTitle" runat="server" Text="Activities" />
                        <span class="pull-right">
                            <asp:LinkButton ID="lbAddActivityType" runat="server" CssClass="btn btn-mini" OnClick="lbAddActivityType_Click" CausesValidation="false"><i class="icon-plus"></i>Add Activity</asp:LinkButton>
                        </span>
                    </legend>
                    <div class="row-fluid workflow-activity-list">
                        <asp:PlaceHolder ID="phActivities" runat="server" />
                    </div>
                </fieldset>

                <div class="actions">
                    <Rock:BootstrapButton ID="btnSave" runat="server" Text="Save" DataLoadingText="Saving..." CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

            <fieldset id="fieldsetViewDetails" runat="server">

                <p class="description"><asp:Literal ID="lWorkflowTypeDescription" runat="server"></asp:Literal></p>

                <div class="row-fluid">
                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                </div>
                <div class="row-fluid">
                    <div class="span6">
                        <a class="workflow-activities-readonly-header" href="#" onclick="javascxript: toggleReadOnlyActivitiesList();">
                            <asp:Label ID="lblActivitiesReadonlyHeaderLabel" runat="server" Text="Activities" />
                            <b class="caret"></b>
                        </a>
                        
                        <div class="workflow-activities-readonly-list" style="display: none">
                            <asp:Literal ID="lblWorkflowActivitiesReadonly" runat="server" />
                        </div>
                    </div>
                    <div class="span6">
                        <asp:Panel ID="pnlAttributeTypes" runat="server">
                            <Rock:ModalAlert ID="mdGridWarningAttributes" runat="server" />
                            <Rock:Grid ID="gWorkflowTypeAttributes" runat="server" AllowPaging="false" DisplayType="Light">
                                <Columns>
                                    <asp:BoundField DataField="Name" HeaderText="Workflow Attributes" />
                                    <Rock:EditField OnClick="gWorkflowTypeAttributes_Edit" />
                                    <Rock:DeleteField OnClick="gWorkflowTypeAttributes_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </asp:Panel>
                    </div>
                </div>

                <div class="actions">
                    <Rock:BootstrapButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                </div>

            </fieldset>

        </asp:Panel>

        <asp:Panel ID="pnlWorkflowTypeAttributes" runat="server" Visible="false">
            <Rock:AttributeEditor ID="edtWorkflowTypeAttributes" runat="server" OnSaveClick="btnSaveWorkflowTypeAttribute_Click" OnCancelClick="btnCancelWorkflowTypeAttribute_Click" />
        </asp:Panel>
        <script>

            function toggleReadOnlyActivitiesList() {
                $('.workflow-activities-readonly-list').toggle(500);
            }

            Sys.Application.add_load(function () {
                var fixHelper = function (e, ui) {
                    ui.children().each(function () {
                        $(this).width($(this).width());
                    });
                    return ui;
                };

                $('.workflow-activity-list').sortable({
                    helper: fixHelper,
                    handle: '.workflow-activity-reorder',
                    containment: 'parent',
                    tolerance: 'pointer',
                    start: function (event, ui) {
                        {
                            var start_pos = ui.item.index();
                            ui.item.data('start_pos', start_pos);
                        }
                    },
                    update: function (event, ui) {
                        {
                            __doPostBack('<%=upDetail.ClientID %>', 're-order-activity:' + ui.item.attr('data-key') + ';' + ui.item.index());
                        }
                    }
                });

                $('.workflow-action-list').sortable({
                    helper: fixHelper,
                    handle: '.workflow-action-reorder',
                    containment: 'parent',
                    tolerance: 'pointer',
                    start: function (event, ui) {
                        {
                            var start_pos = ui.item.index();
                            ui.item.data('start_pos', start_pos);
                        }
                    },
                    update: function (event, ui) {
                        {
                            __doPostBack('<%=upDetail.ClientID %>', 're-order-action:' + ui.item.attr('data-key') + ';' + ui.item.index());
                        }
                    }
                });
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
