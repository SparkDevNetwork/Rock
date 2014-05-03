<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowTypeDetail.ascx.cs" Inherits="RockWeb.Blocks.Core.WorkflowTypeDetail" %>


<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

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
            
            <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

            <div id="pnlEditDetails" runat="server">

                <Rock:PanelWidget ID="pwDetails" runat="server" Title="Details" Expanded="true">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.WorkflowType, Rock" PropertyName="Name" />
                        </div>
                        <div class="col-md-3">
                            <Rock:RockCheckBox ID="cbIsActive" runat="server" Text="Active" />
                        </div>
                        <div class="col-md-3">
                            <Rock:RockCheckBox ID="cbIsPersisted" runat="server" Text="Persisted" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.WorkflowType, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbWorkTerm" runat="server" SourceTypeName="Rock.Model.WorkflowType, Rock" PropertyName="WorkTerm" Label="Work Term" />
                            <Rock:RockDropDownList ID="ddlLoggingLevel" Help="The level you would like to audit.  Start and stop times can be logged for each workflow, workflow activity, or activity action." runat="server" Label="Logging Level" />
                        </div>
                            <div class="col-md-6">
                            <Rock:CategoryPicker ID="cpCategory" runat="server" Required="true" Label="Category" EntityTypeName="Rock.Model.WorkflowType" />
                            <Rock:DataTextBox ID="tbProcessingInterval" runat="server" SourceTypeName="Rock.Model.WorkflowType, Rock" PropertyName="ProcessingIntervalSeconds" Label="Processing Interval (seconds)" />
                        </div>
                    </div>
                </Rock:PanelWidget>

                <Rock:PanelWidget ID="wpAttributes" runat="server" Title="Attributes" CssClass="attribute-panel">
                    <Rock:Grid ID="gAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Attribute">
                        <Columns>
                            <Rock:ReorderField />
                            <asp:BoundField DataField="Name" HeaderText="Attribute" />
                            <asp:BoundField DataField="Description" HeaderText="Description" />
                            <Rock:BoolField DataField="IsRequired" HeaderText="Required" />
                            <Rock:EditField OnClick="gAttributes_Edit" />
                            <Rock:DeleteField OnClick="gAttributes_Delete" />
                        </Columns>
                    </Rock:Grid>
                </Rock:PanelWidget>

                <fieldset>
                    <legend>
                        Activities
                        <span class="pull-right">
                            <asp:LinkButton ID="lbAddActivityType" runat="server" CssClass="btn btn-action btn-xs" OnClick="lbAddActivityType_Click" CausesValidation="false"><i class="fa fa-plus"></i> Add Activity</asp:LinkButton>
                        </span>
                    </legend>
                    <div class="workflow-activity-list">
                        <asp:PlaceHolder ID="phActivities" runat="server" />
                    </div>
                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

            <fieldset id="fieldsetViewDetails" runat="server">

                <p class="description"><asp:Literal ID="lWorkflowTypeDescription" runat="server"></asp:Literal></p>

                <div class="row">
                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <a class="workflow-activities-readonly-header" href="#" onclick="javascxript: toggleReadOnlyActivitiesList();">
                            <asp:Label ID="lblActivitiesReadonlyHeaderLabel" runat="server" Text="Activities" />
                            <b class="fa fa-caret-down"></b>
                        </a>
                        
                        <div class="workflow-activities-readonly-list" style="display: none">
                            <asp:Literal ID="lblWorkflowActivitiesReadonly" runat="server" />
                        </div>
                    </div>
                    <div class="col-md-6">
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                </div>
                
            </fieldset>

        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgAttribute" runat="server" Title="Attributes" OnSaveClick="dlgAttribute_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="Attributes">
            <Content>
                <Rock:AttributeEditor ID="edtAttributes" runat="server" ShowActions="false" ValidationGroup="Attributes" />
            </Content>
        </Rock:ModalDialog>

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

                $('.workflow-formfield-list').sortable({
                    helper: fixHelper,
                    handle: '.workflow-formfield-reorder',
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
                            __doPostBack('<%=upDetail.ClientID %>', 're-order-formfield:' + ui.item.attr('data-key') + ';' + ui.item.index());
                        }
                    }
                });

            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
