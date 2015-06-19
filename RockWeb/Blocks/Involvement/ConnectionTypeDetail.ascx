<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConnectionTypeDetail.ascx.cs" Inherits="RockWeb.Blocks.Involvement.ConnectionTypeDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upConnectionType" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDeleteConfirm" runat="server" CssClass="panel panel-body" Visible="false">
            <Rock:NotificationBox ID="nbDeleteConfirm" runat="server" NotificationBoxType="Warning">
                       Deleting a site will delete all the layouts and pages associated with the site. Are you sure you want to delete the site?
            </Rock:NotificationBox>
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
            <div class="panel-body">

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <Rock:NotificationBox ID="nbNotAllowedToEdit" runat="server" NotificationBoxType="Danger" Visible="false"
                    Text="You are not authorized to save connection type." />
                <asp:ValidationSummary ID="valConnectionTypeDetail" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <div id="pnlViewDetails" runat="server">
                    <p class="description">
                        <asp:Literal ID="lConnectionTypeDescription" runat="server"></asp:Literal>
                    </p>

                    <div class="row">
                        <div class="cold-md-12">
                            <asp:Literal ID="lblMainDetails" runat="server" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
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

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.ConnectionType, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.ConnectionType, Rock" PropertyName="IconCssClass" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbFutureFollowUp" runat="server" SourceTypeName="Rock.Model.ConnectionType, Rock" PropertyName="EnableFutureFollowUp" Label="Enable Future Follow-up" />
                            <Rock:RockCheckBox ID="cbFullActivityList" runat="server" SourceTypeName="Rock.Model.ConnectionType, Rock" PropertyName="EnableFullActivityList" Label="Enable Full Activity List" />
                        </div>
                    </div>

                    <Rock:PanelWidget ID="wpConnectionTypeAttributes" runat="server" Title="Opportunity Attributes">
                        <div class="grid">
                            <Rock:Grid ID="gConnectionTypeAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Opportunity Attribute">
                                <Columns>
                                    <Rock:ReorderField />
                                    <Rock:RockBoundField DataField="Name" HeaderText="Attribute" />
                                    <Rock:RockBoundField DataField="FieldType" HeaderText="Field Type" />
                                    <Rock:BoolField DataField="AllowSearch" HeaderText="Allow Search" />
                                    <Rock:EditField OnClick="gConnectionTypeAttributes_Edit" />
                                    <Rock:DeleteField OnClick="gConnectionTypeAttributes_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpConnectionActivityTypes" runat="server" Title="Activities">
                        <div class="grid">
                            <Rock:Grid ID="gConnectionActivityTypes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Activity">
                                <Columns>
                                    <Rock:RockBoundField DataField="Name" HeaderText="Activities" />
                                    <Rock:DeleteField OnClick="gConnectionActivityTypes_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpConnectionStatuses" runat="server" Title="Statuses">
                        <div class="grid">
                            <Rock:Grid ID="gConnectionStatuses" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Status">
                                <Columns>
                                    <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                                    <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                    <Rock:DeleteField OnClick="gConnectionStatuses_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpConnectionWorkflow" runat="server" Title="Workflows">
                        <div class="grid">
                            <Rock:Grid ID="gConnectionWorkflows" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Workflow">
                                <Columns>
                                    <Rock:RockBoundField DataField="WorkflowType" HeaderText="Workflow Type" />
                                    <Rock:RockBoundField DataField="Trigger" HeaderText="Trigger" />
                                    <Rock:EditField OnClick="gConnectionWorkflows_Edit" />
                                    <Rock:DeleteField OnClick="gConnectionWorkflows_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </Rock:PanelWidget>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>

        <Rock:ModalAlert ID="modalAlert" runat="server" />

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgConnectionTypeAttribute" runat="server" Title="Event Calendar Attributes" OnSaveClick="dlgConnectionTypeAttribute_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="ConnectionTypeAttributes">
            <Content>
                <Rock:AttributeEditor ID="edtConnectionTypeAttributes" runat="server" ShowActions="false" ValidationGroup="ConnectionTypeAttributes" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgConnectionActivityTypes" runat="server" ScrollbarEnabled="false" SaveButtonText="Add" OnSaveClick="btnAddConnectionActivityType_Click" Title="Create Activity" ValidationGroup="ConnectionActivityType">
            <Content>
                <asp:HiddenField ID="hfConnnectionTypeAddConnectionActivityTypeGuid" runat="server" />
                <Rock:DataTextBox ID="tbConnectionActivityTypeName" SourceTypeName="Rock.Model.ConnectionActivityType, Rock" PropertyName="Name" Label="Activity Name" runat="server" ValidationGroup="ConnectionActivityType" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgConnectionStatuses" runat="server" ScrollbarEnabled="false" SaveButtonText="Add" OnSaveClick="btnAddConnectionStatus_Click" Title="Create Activity" ValidationGroup="ConnectionStatus">
            <Content>
                <asp:HiddenField ID="hfConnnectionTypeAddConnectionStatusGuid" runat="server" />
                <Rock:DataTextBox ID="tbConnectionStatusName" SourceTypeName="Rock.Model.ConnectionStatus, Rock" PropertyName="Name" Label="Name" runat="server" ValidationGroup="ConnectionStatus" />
                <Rock:DataTextBox ID="tbConnectionStatusDescription" SourceTypeName="Rock.Model.ConnectionStatus, Rock" PropertyName="Description" Label="Description" runat="server" ValidationGroup="ConnectionStatus" />
                <Rock:RockCheckBox ID="cbIsCritical" runat="server" Label="Is Critical" ValidationGroup="ConnectionStatus" />
                <Rock:RockCheckBox ID="cbIsDefault" runat="server" Label="Is Default" ValidationGroup="ConnectionStatus" />
                <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Is Active" ValidationGroup="ConnectionStatus" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgConnectionWorkflow" runat="server" Title="Campus Select" OnSaveClick="dlgConnectionWorkflow_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="ConnectionWorkflow">
            <Content>

                <asp:HiddenField ID="hfAddConnectionWorkflowGuid" runat="server" />

                <asp:ValidationSummary ID="valConnectionWorkflowSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="ConnectionWorkflow" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlTriggerType" runat="server" Label="Launch Workflow When" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.ConnectionType, Rock" PropertyName="TriggerType" OnSelectedIndexChanged="ddlTriggerType_SelectedIndexChanged" AutoPostBack="true" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlWorkflowType" runat="server" Label="Workflow Type" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.ConnectionType, Rock" PropertyName="WorkflowType" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlPrimaryQualifier" runat="server" Visible="false" />
                    </div>
                    <div class="col-md-6">
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlSecondaryQualifier" runat="server" Visible="false" />
                    </div>
                    <div class="col-md-6">
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
