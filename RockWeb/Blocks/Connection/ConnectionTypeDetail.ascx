﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConnectionTypeDetail.ascx.cs" Inherits="RockWeb.Blocks.Connection.ConnectionTypeDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upConnectionType" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDeleteConfirm" runat="server" CssClass="panel panel-body" Visible="false">
            <Rock:NotificationBox ID="nbDeleteConfirm" runat="server" NotificationBoxType="Warning" Text="Deleting a site will delete all the layouts and pages associated with the site. Are you sure you want to delete the site?" />
            <asp:LinkButton ID="btnDeleteConfirm" runat="server" Text="Confirm Delete" CssClass="btn btn-danger" OnClick="btnDeleteConfirm_Click" />
            <asp:LinkButton ID="btnDeleteCancel" runat="server" Text="Cancel" CssClass="btn btn-primary" OnClick="btnDeleteCancel_Click" />
        </asp:Panel>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="false">
            <asp:HiddenField ID="hfConnectionTypeId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lIcon" runat="server" />
                    <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <Rock:NotificationBox ID="nbRequired" runat="server" NotificationBoxType="Danger" Text="A default connection status and at least one activity are required." Visible="false" />
                <asp:ValidationSummary ID="valConnectionTypeDetail" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <div id="pnlViewDetails" runat="server">
                    <p class="description">
                        <asp:Literal ID="lConnectionTypeDescription" runat="server"></asp:Literal>
                    </p>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
                        <span class="pull-right">
                            <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-security" />
                        </span>
                    </div>
                </div>

                <div id="pnlEditDetails" runat="server">

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.ConnectionType, Rock" PropertyName="Name" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbActive" runat="server" SourceTypeName="Rock.Model.ConnectionType, Rock" PropertyName="IsActive" Label="Active" Checked="true" Text="Yes" />
                        </div>
                    </div>

                    <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.ConnectionType, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.ConnectionType, Rock" PropertyName="IconCssClass" ValidateRequestMode="Disabled"/>
                            <Rock:NumberBox ID="nbDaysUntilRequestIdle" runat="server" SourceTypeName="Rock.Model.ConnectionType, Rock" PropertyName="DaysUntilRequestIdle" Label="Days Until Request Considered Idle" ValidateRequestMode="Disabled" NumberType="Integer" MinimumValue="0"/>
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbFutureFollowUp" runat="server" SourceTypeName="Rock.Model.ConnectionType, Rock" PropertyName="EnableFutureFollowUp" Label="Enable Future Follow-up" />
                            <Rock:RockCheckBox ID="cbFullActivityList" runat="server" SourceTypeName="Rock.Model.ConnectionType, Rock" PropertyName="EnableFullActivityList" Label="Enable Full Activity List" />
                            <Rock:RockCheckBox ID="cbRequiresPlacementGroup" runat="server" SourceTypeName="Rock.Model.ConnectionType, Rock" PropertyName="RequiresPlacementGroupToConnect" Label="Requires Placement Group To Connect" />
                        </div>
                    </div>

                    <Rock:PanelWidget ID="wpAttributes" runat="server" Title="Opportunity Attributes">
                        <div class="grid">
                            <Rock:Grid ID="gAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Opportunity Attribute" ShowConfirmDeleteDialog="false" >
                                <Columns>
                                    <Rock:ReorderField />
                                    <Rock:RockBoundField DataField="Name" HeaderText="Attribute" />
                                    <Rock:RockBoundField DataField="FieldType" HeaderText="Field Type" />
                                    <Rock:BoolField DataField="AllowSearch" HeaderText="Allow Search" HeaderStyle-HorizontalAlign="Center" />
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
                            <Rock:Grid ID="gStatuses" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Status" ShowConfirmDeleteDialog="false" >
                                <Columns>
                                    <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                                    <Rock:RockBoundField DataField="Description" HeaderText="Description" />
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
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>

        <Rock:ModalAlert ID="modalAlert" runat="server" />

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgAttribute" runat="server" Title="Connection Opportunity Attributes" OnSaveClick="dlgConnectionTypeAttribute_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="Attributes">
            <Content>
                <Rock:AttributeEditor ID="edtAttributes" runat="server" ShowActions="false" ValidationGroup="Attributes" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgConnectionActivityTypes" runat="server" ScrollbarEnabled="false" SaveButtonText="Add" OnSaveClick="btnAddConnectionActivityType_Click" Title="Create Activity" ValidationGroup="ConnectionActivityType">
            <Content>
                <asp:HiddenField ID="hfConnectionTypeAddConnectionActivityTypeGuid" runat="server" />
                <Rock:DataTextBox ID="tbConnectionActivityTypeName" SourceTypeName="Rock.Model.ConnectionActivityType, Rock" PropertyName="Name" Label="Activity Name" runat="server" ValidationGroup="ConnectionActivityType" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgConnectionStatuses" runat="server" ScrollbarEnabled="false" SaveButtonText="Add" OnSaveClick="btnAddConnectionStatus_Click" Title="Create Status" ValidationGroup="ConnectionStatus">
            <Content>
                <asp:HiddenField ID="hfConnectionTypeAddConnectionStatusGuid" runat="server" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbConnectionStatusName" SourceTypeName="Rock.Model.ConnectionStatus, Rock" PropertyName="Name" Label="Name" runat="server" ValidationGroup="ConnectionStatus" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Is Active" ValidationGroup="ConnectionStatus" />
                    </div>
                </div>
                <Rock:DataTextBox ID="tbConnectionStatusDescription" SourceTypeName="Rock.Model.ConnectionStatus, Rock" PropertyName="Description" Label="Description" runat="server" ValidationGroup="ConnectionStatus" TextMode="MultiLine" Rows="3" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbIsCritical" runat="server" Label="Is Critical" ValidationGroup="ConnectionStatus" Help="Requires immediate action." />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbIsDefault" runat="server" Label="Is Default" ValidationGroup="ConnectionStatus" />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgConnectionWorkflow" runat="server" Title="Select Workflow" OnSaveClick="dlgConnectionWorkflow_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="ConnectionWorkflow">
            <Content>

                <asp:HiddenField ID="hfAddConnectionWorkflowGuid" runat="server" />

                <asp:ValidationSummary ID="valConnectionWorkflowSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="ConnectionWorkflow" />

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
                        </Rock:RockDropDownList>
                    </div>
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlWorkflowType" runat="server" Label="Workflow Type" DataTextField="Name" DataValueField="Id" 
                            Required="true" ValidationGroup="ConnectionWorkflow" EnhanceForLongLists="true" />
                    </div>
                </div>

                <div class="row">
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
