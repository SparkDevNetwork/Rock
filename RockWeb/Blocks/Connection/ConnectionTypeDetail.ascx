<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConnectionTypeDetail.ascx.cs" Inherits="RockWeb.Blocks.Connection.ConnectionTypeDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upConnectionType" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDeleteConfirm" runat="server" CssClass="panel panel-body" Visible="false">
            <Rock:NotificationBox ID="nbDeleteConfirm" runat="server" NotificationBoxType="Warning" Text="Deleting a Connection Type will delete all the Connection Opportunities associated with the Connection Type. Are you sure you want to delete the Connection Type?" />
            <asp:LinkButton ID="btnDeleteConfirm" runat="server" Text="Confirm Delete" CssClass="btn btn-danger" OnClick="btnDeleteConfirm_Click" />
            <asp:LinkButton ID="btnDeleteCancel" runat="server" Text="Cancel" CssClass="btn btn-primary" OnClick="btnDeleteCancel_Click" />
        </asp:Panel>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="false">
            <asp:HiddenField ID="hfConnectionTypeId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lIcon" runat="server" />
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </h1>
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <Rock:NotificationBox ID="nbRequired" runat="server" NotificationBoxType="Danger" Text="A default connection status and at least one activity are required." Visible="false" />
                <asp:ValidationSummary ID="valConnectionTypeDetail" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <div id="pnlViewDetails" runat="server">
                    <p class="description">
                        <asp:Literal ID="lConnectionTypeDescription" runat="server"></asp:Literal>
                    </p>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" data-shortcut-key="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
                        <span class="pull-right">
                            <asp:LinkButton ID="btnCopy" runat="server" CssClass="btn btn-default btn-sm btn-square" Text="<i class='fa fa-clone'></i>" OnClick="btnCopy_Click" ToolTip="Copy Connection Type" />
                            <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-square btn-security" />
                        </span>
                    </div>
                </div>

                <div id="pnlEditDetails" runat="server">

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.ConnectionType, Rock" PropertyName="Name" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbActive" runat="server" SourceTypeName="Rock.Model.ConnectionType, Rock" PropertyName="IsActive" Label="Active" Checked="true" />
                        </div>
                    </div>

                    <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.ConnectionType, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.ConnectionType, Rock" PropertyName="IconCssClass" Label="Icon CSS Class" ValidateRequestMode="Disabled"/>
                            <Rock:NumberBox ID="nbDaysUntilRequestIdle" runat="server" SourceTypeName="Rock.Model.ConnectionType, Rock" PropertyName="DaysUntilRequestIdle" Label="Days Until Request Considered Idle" ValidateRequestMode="Disabled" NumberType="Integer" MinimumValue="0"/>
                            <Rock:PagePicker ID="ppConnectionRequestDetail" runat="server" Label="Connection Request Detail Page" Required="false" PromptForPageRoute="true" Help="Choose a page that should be used for viewing connection requests of this type. This is useful if you have different detail pages with different settings. A default page will be used if this is left blank." />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox
                                ID="cbFutureFollowUp"
                                runat="server"
                                SourceTypeName="Rock.Model.ConnectionType, Rock"
                                PropertyName="EnableFutureFollowUp"
                                Label="Enable Future Follow-up"
                                Help="Allows a request to be frozen until a specific date, at which point a job will turn it back to Active." />
                            <Rock:RockCheckBox
                                ID="cbFullActivityList"
                                runat="server"
                                SourceTypeName="Rock.Model.ConnectionType, Rock"
                                PropertyName="EnableFullActivityList"
                                Label="Enable Full Activity List"
                                Help="Show activities from other requests made by the same individual." />
                            <Rock:RockCheckBox
                                ID="cbRequiresPlacementGroup"
                                runat="server"
                                SourceTypeName="Rock.Model.ConnectionType, Rock"
                                PropertyName="RequiresPlacementGroupToConnect"
                                Label="Requires Placement Group To Connect"
                                Help="If checked, this will prevent the Connect button from activating on a Request unless a Placement Group is set."/>
                            <Rock:RockCheckBox
                                ID="cbEnableRequestSecurity"
                                runat="server"
                                SourceTypeName="Rock.Model.ConnectionType, Rock"
                                PropertyName="EnableRequestSecurity"
                                Label="Enable Request Security"
                                Help="If enabled, connection request blocks will have an additional setting allowing security to be applied to individual requests. A special rule is also applied, which automatically allows an assigned connector to view or edit their requests when the connector doesn't have security to the connection opportunity or type. Enabling this setting will noticeably impact performance when there are a significant amount of requests." />
                        </div>
                    </div>
                    <Rock:PanelWidget ID="wpConnectionRequestAttributes" runat="server" Title="Connection Request Attributes" CssClass="connection-request-attribute-panel">
                        <Rock:NotificationBox ID="nbConnectionRequestAttributes" runat="server" NotificationBoxType="Info"
                            Text="Connection Request Attributes apply to all of the connection requests in every Opportunity of this type.  Each connection request will have their own value for these attributes" />
                        <div class="grid">
                            <Rock:Grid ID="gConnectionRequestAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Connection Request Attribute">
                                <Columns>
                                    <Rock:ReorderField />
                                    <Rock:RockBoundField DataField="Name" HeaderText="Attribute" />
                                    <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                    <Rock:BoolField DataField="IsRequired" HeaderText="Required" />
                                    <Rock:SecurityField TitleField="Name" />
                                    <Rock:EditField OnClick="gConnectionRequestAttributes_Edit" />
                                    <Rock:DeleteField OnClick="gConnectionRequestAttributes_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpAttributes" runat="server" Title="Opportunity Attributes">
                        <div class="grid">
                            <Rock:Grid ID="gAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Opportunity Attribute" ShowConfirmDeleteDialog="false" >
                                <Columns>
                                    <Rock:ReorderField />
                                    <Rock:RockBoundField DataField="Name" HeaderText="Attribute" />
                                    <Rock:RockBoundField DataField="FieldType" HeaderText="Field Type" />
                                    <Rock:BoolField DataField="AllowSearch" HeaderText="Allow Search" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                                    <Rock:EditField OnClick="gAttributes_Edit" />
                                    <Rock:DeleteField OnClick="gAttributes_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpActivityTypes" runat="server" Title="Activities">
                        <div class="grid">
                            <Rock:Grid ID="gActivityTypes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Activity" ShowConfirmDeleteDialog="false" >
                                <Columns>
                                    <Rock:RockBoundField DataField="Name" HeaderText="Activities" />
                                    <Rock:EditField OnClick="gActivityTypes_Edit" />
                                    <Rock:DeleteField OnClick="gActivityTypes_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpStatuses" runat="server" Title="Statuses">
                        <div class="grid">
                            <Rock:Grid ID="gStatuses" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Status" ShowConfirmDeleteDialog="false" OnGridReorder="gStatuses_GridReorder" >
                                <Columns>
                                    <Rock:ReorderField />
                                    <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                                    <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                    <Rock:BoolField DataField="IsDefault" HeaderText="Is Default" />
                                    <Rock:BoolField DataField="IsCritical" HeaderText="Is Critical" />
                                    <Rock:EditField OnClick="gStatuses_Edit" />
                                    <Rock:DeleteField OnClick="gStatuses_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpWorkflow" runat="server" Title="Workflows">
                        <div class="grid">
                            <Rock:Grid ID="gWorkflows" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Workflow" ShowConfirmDeleteDialog="false" >
                                <Columns>
                                    <Rock:RockBoundField DataField="WorkflowType" HeaderText="Workflow Type" />
                                    <Rock:RockBoundField DataField="Trigger" HeaderText="Trigger" />
                                    <Rock:EditField OnClick="gWorkflows_Edit" />
                                    <Rock:DeleteField OnClick="gWorkflows_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </Rock:PanelWidget>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>

       <Rock:ModalAlert ID="mdCopy" runat="server" />

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgConnectionRequestAttribute" runat="server" Title="Connection Request Attributes" OnSaveClick="dlgConnectionRequestAttribute_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="ConnectionRequestAttributes">
            <Content>
                <Rock:AttributeEditor ID="edtConnectionRequestAttributes" runat="server" ShowActions="false" ValidationGroup="ConnectionRequestAttributes" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgAttribute" runat="server" Title="Connection Opportunity Attributes" OnSaveClick="dlgConnectionTypeAttribute_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="Attributes">
            <Content>
                <Rock:AttributeEditor ID="edtAttributes" runat="server" ShowActions="false" ValidationGroup="Attributes" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgConnectionActivityTypes" runat="server" ScrollbarEnabled="false" SaveButtonText="Add" OnSaveClick="btnAddConnectionActivityType_Click" Title="Create Activity" ValidationGroup="ConnectionActivityType">
            <Content>
                <asp:HiddenField ID="hfConnectionTypeAddConnectionActivityTypeGuid" runat="server" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbConnectionActivityTypeName" SourceTypeName="Rock.Model.ConnectionActivityType, Rock" PropertyName="Name" Label="Activity Name" runat="server" ValidationGroup="ConnectionActivityType" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbActivityTypeIsActive" runat="server" Label="Is Active" ValidationGroup="ConnectionActivityType" />
                    </div>
                </div>
                <Rock:AttributeValuesContainer ID="avcActivityAttributes" runat="server" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgConnectionStatuses" runat="server" ScrollbarEnabled="false" SaveButtonText="Add" OnSaveClick="btnAddConnectionStatus_Click" Title="Create Status" ValidationGroup="ConnectionStatus">
            <Content>
                <asp:HiddenField ID="hfConnectionTypeAddConnectionStatusGuid" runat="server" />
                <Rock:NotificationBox ID="nbDuplicateConnectionStatus" runat="server" NotificationBoxType="Danger" Visible="false" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbConnectionStatusName" SourceTypeName="Rock.Model.ConnectionStatus, Rock" PropertyName="Name" Label="Name" runat="server" ValidationGroup="ConnectionStatus" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbConnectionStatusIsActive" runat="server" Label="Is Active" ValidationGroup="ConnectionStatus" />
                    </div>
                </div>
                <Rock:DataTextBox ID="tbConnectionStatusDescription" SourceTypeName="Rock.Model.ConnectionStatus, Rock" PropertyName="Description" Label="Description" runat="server" ValidationGroup="ConnectionStatus" TextMode="MultiLine" Rows="3" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:ColorPicker ID="cpStatus" runat="server" Label="Highlight Color" Help="The highlight color for this status." />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbIsCritical" runat="server" Label="Is Critical" ValidationGroup="ConnectionStatus" Help="Requires immediate action." />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbIsDefault" runat="server" Label="Is Default" ValidationGroup="ConnectionStatus" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbAutoInactivateState" runat="server" Label="Auto-Inactivate State" ValidationGroup="ConnectionStatus" Help="Selecting this status will change the state to Inactive." />
                    </div>
                </div>
                <div>
                    <h4 class="margin-t-md">Status Automations</h4>
                    <span class="text-muted">Below are a list of automations that can run on the requests with the specific status. These can be used to change the status based on the criteria you provide. These rules will be considered after each save of the request and on the configured schedule of the 'Connection Request Automation' job.</span>
                    <hr class="margin-t-sm" >
                    <Rock:NotificationBox ID="nbMessage" NotificationBoxType="Info" runat="server" Text="Please save the new Status before adding any Status Automations." Visible="false" />
                    <Rock:RockControlWrapper ID="rcwStatusAutomationsView" runat="server">
                        <div class="grid">
                            <Rock:Grid ID="gStatusAutomations" runat="server" AllowPaging="false" DisplayType="Light" ShowHeader="true" RowItemText="Status Automation" OnGridReorder="gStatusAutomations_GridReorder">
                                <Columns>
                                    <Rock:ReorderField />
                                    <Rock:RockBoundField DataField="AutomationName" HeaderText="Automation Name" />
                                    <Rock:RockBoundField DataField="DataViewName" HeaderText="Data View" />
                                    <Rock:EnumField DataField="GroupRequirementsFilter" HeaderText="Group Requirements Filter" />
                                    <Rock:RockBoundField DataField="DestinationStatusName" HeaderText="Move To" />
                                    <Rock:EditField OnClick="gStatusAutomations_Edit" />
                                    <Rock:DeleteField OnClick="gStatusAutomations_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </Rock:RockControlWrapper>
                     <Rock:RockControlWrapper ID="rcwStatusAutomationsEdit" runat="server">
                         <Rock:NotificationBox ID="nbStatusWarning" runat="server" NotificationBoxType="Warning" Title="Warning" Visible="false" />
                         <asp:HiddenField ID="hfConnectionStatusAutomationGuid" runat="server" />
                         <Rock:DataTextBox ID="tbAutomationName" SourceTypeName="Rock.Model.ConnectionStatusAutomation, Rock" PropertyName="AutomationName" Label="Automation Name" runat="server" ValidationGroup="vgConnectionStatusAutomation" />
                         <div class="row">
                             <div class="col-md-6">
                                 <Rock:DataViewsPicker ID="dvpDataView" runat="server" Label="Data View" Help="The data view that should be used to filter requests by. This data view should be on the connection requests." SelectionMode="Single" ValidationGroup="vgConnectionStatusAutomation"/>
                             </div>
                             <div class="col-md-6">
                                 <Rock:RockRadioButtonList ID="rblGroupRequirementsFilter" runat="server" Help="Determines if group requirements should be checked. These requirements would come from the selected placement group." RepeatDirection="Horizontal" Label="Group Requirements Filter" ValidationGroup="vgConnectionStatusAutomation"/>
                             </div>
                         </div>
                         <Rock:RockDropDownList ID="ddlMoveTo" runat="server" Label="Move To" Required="true" ValidationGroup="vgConnectionStatusAutomation" DataTextField="Name" DataValueField="Guid" />
                         <div class="actions">
                             <asp:LinkButton ID="lbSaveAutomation" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save Automation" CssClass="btn btn-xs btn-default" OnClick="lbSaveAutomation_Click"  ValidationGroup="vgConnectionStatusAutomation" />
                             <asp:LinkButton ID="lbCancelAutomation" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-xs btn-link" CausesValidation="false" OnClick="lbCancelAutomation_Click" />
                         </div>
                    </Rock:RockControlWrapper>

                </div>
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgConnectionWorkflow" runat="server" Title="Select Workflow" OnSaveClick="dlgConnectionWorkflow_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="ConnectionWorkflow">
            <Content>

                <asp:HiddenField ID="hfAddConnectionWorkflowGuid" runat="server" />

                <asp:ValidationSummary ID="valConnectionWorkflowSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="ConnectionWorkflow" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlTriggerType" runat="server" Label="Launch Workflow When"
                            OnSelectedIndexChanged="ddlTriggerType_SelectedIndexChanged" AutoPostBack="true" Required="true" ValidationGroup="ConnectionWorkflow" >
                            <asp:ListItem Value="0" Text="Request Started" />
                            <asp:ListItem Value="8" Text="Request Assigned" />
                            <asp:ListItem Value="7" Text="Request Transferred" />
                            <asp:ListItem Value="1" Text="Request Connected" />
                            <asp:ListItem Value="5" Text="Placement Group Assigned" />
                            <asp:ListItem Value="2" Text="Status Changed" />
                            <asp:ListItem Value="3" Text="State Changed" />
                            <asp:ListItem Value="4" Text="Activity Added" />
                            <asp:ListItem Value="6" Text="Manual" />
                            <asp:ListItem Value="9" Text="Future Follow-up Date Reached" />
                        </Rock:RockDropDownList>
                    </div>
                    <div class="col-md-6">
                        <Rock:WorkflowTypePicker ID="wpWorkflowType" runat="server" Label="Workflow Type" Required="true" ValidationGroup="ConnectionWorkflow"/>
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-12">
                        <Rock:RockRadioButtonList ID="rblConnectionStatuses" runat="server" RepeatDirection="Horizontal" Label="Manual Trigger Status Filter" Help="Filters workflows to display based on the current status of the connection request." />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlPrimaryQualifier" runat="server" Visible="false" ValidationGroup="ConnectionWorkflow" />
                        <Rock:RockDropDownList ID="ddlSecondaryQualifier" runat="server" Visible="false" ValidationGroup="ConnectionWorkflow" />
                    </div>
                    <div class="col-md-6">
                    </div>
                </div>

            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
