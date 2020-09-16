<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReservationTypeDetail.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.RoomManagement.ReservationTypeDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upReservationType" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDeleteConfirm" runat="server" CssClass="panel panel-body" Visible="false">
            <Rock:NotificationBox ID="nbDeleteConfirm" runat="server" NotificationBoxType="Warning" Text="Deleting a Reservation Type will delete all the reservations associated with the type. Are you sure you want to delete the type?" />
            <asp:LinkButton ID="btnDeleteConfirm" runat="server" Text="Confirm Delete" CssClass="btn btn-danger" OnClick="btnDeleteConfirm_Click" />
            <asp:LinkButton ID="btnDeleteCancel" runat="server" Text="Cancel" CssClass="btn btn-primary" OnClick="btnDeleteCancel_Click" />
        </asp:Panel>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="false">
            <asp:HiddenField ID="hfReservationTypeId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lIcon" runat="server" />
                    <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <asp:ValidationSummary ID="valReservationTypeDetail" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <div id="pnlViewDetails" runat="server">
                    <p class="description">
                        <asp:Literal ID="lReservationTypeDescription" runat="server"></asp:Literal>
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
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="com.bemaservices.RoomManagement.Model.ReservationType, com.bemaservices.RoomManagement" PropertyName="Name" />
                        </div>
                        <%--                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbActive" runat="server" SourceTypeName="com.bemaservices.RoomManagement.Model.ReservationType, com.bemaservices.RoomManagement" PropertyName="IsActive" Label="Active" Checked="true" Text="Yes" />
                        </div>--%>
                    </div>

                    <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="com.bemaservices.RoomManagement.Model.ReservationType, com.bemaservices.RoomManagement" PropertyName="Description" TextMode="MultiLine" Rows="4" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="com.bemaservices.RoomManagement.Model.ReservationType, com.bemaservices.RoomManagement" PropertyName="IconCssClass" ValidateRequestMode="Disabled" />
                            <Rock:RockDropDownList ID="ddlFinalApprovalGroup" runat="server" Label="Final Approval Group" ValidateRequestMode="Disabled" NumberType="Integer" MinimumValue="0" />
                            <Rock:RockDropDownList ID="ddlSuperAdminGroup" runat="server" Label="Super Admin Group" ValidateRequestMode="Disabled" NumberType="Integer" MinimumValue="0" />
                            <Rock:RockDropDownList ID="ddlNotificationEmail" runat="server" Label="Notification Email" ValidateRequestMode="Disabled" NumberType="Integer" MinimumValue="0" />
                        </div>
                        <div class="col-md-6">
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockCheckBox ID="cbIsSetupTimeRequired" runat="server" Label="Is Setup Time Required" SourceTypeName="com.bemaservices.RoomManagement.Model.ReservationType, com.bemaservices.RoomManagement" PropertyName="IsSetupTimeRequired" />
                                    <Rock:RockCheckBox ID="cbIsNumberAttendingRequired" runat="server" Label="Is Number Attending Required" SourceTypeName="com.bemaservices.RoomManagement.Model.ReservationType, com.bemaservices.RoomManagement" PropertyName="IsNumberAttendingRequired" />
                                    <Rock:RockCheckBox ID="cbIsContactDetailsRequired" runat="server" Label="Are Contact Details Required" SourceTypeName="com.bemaservices.RoomManagement.Model.ReservationType, com.bemaservices.RoomManagement" PropertyName="IsContactDetailsRequired" />
                                    <Rock:RockCheckBox ID="cbIsCommunicationHistorySaved" runat="server" Label="Is Communication History Saved" SourceTypeName="com.bemaservices.RoomManagement.Model.ReservationType, com.bemaservices.RoomManagement" PropertyName="IsCommunicationHistorySaved" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:NumberBox ID="nbDefaultSetupTime" runat="server" Label="Default Setup Time" Help="If you wish to default to a particular setup time, you can supply a value here. (empty or -1 indicates no default value)" NumberType="Integer" MinimumValue="-1" SourceTypeName="com.bemaservices.RoomManagement.Model.ReservationType, com.bemaservices.RoomManagement" PropertyName="DefaultSetupTime" />
                                    <Rock:NumberBox ID="nbDefaultCleanupTime" runat="server" Label="Default Cleanup Time" Help="If you wish to default to a particular cleanup time, you can supply a value here. (empty or -1 indicates no default value)" NumberType="Integer" MinimumValue="-1" SourceTypeName="com.bemaservices.RoomManagement.Model.ReservationType, com.bemaservices.RoomManagement" PropertyName="DefaultCleanupTime" />
                                    <Rock:DefinedValuesPicker ID="dvpReservableLocationTypes" runat="server" Label="Reservable Location Types" />
                                    <Rock:RockCheckBox ID="cbIsReservationBookedOnApproval" runat="server" Label="Is Reservation Booked On Approval" Help="Are the resources and locations in this reservation booked once it's created, or only once it's approved?" SourceTypeName="com.bemaservices.RoomManagement.Model.ReservationType, com.bemaservices.RoomManagement" PropertyName="IsReservationBookedOnApproval" />
                                </div>
                            </div>
                        </div>
                    </div>

                    <Rock:PanelWidget ID="wpAttributes" runat="server" Title="Reservation Attributes">
                        <div class="grid">
                            <Rock:Grid ID="gAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Reservation Attribute" ShowConfirmDeleteDialog="false">
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

                    <Rock:PanelWidget ID="wpMinistries" runat="server" Title="Ministries">
                        <div class="grid">
                            <Rock:Grid ID="gMinistries" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Ministry" ShowConfirmDeleteDialog="false">
                                <Columns>
                                    <Rock:RockBoundField DataField="Name" HeaderText="Ministries" />
                                    <Rock:EditField OnClick="gMinistries_Edit" />
                                    <Rock:DeleteField OnClick="gMinistries_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpWorkflowTriggers" runat="server" Title="Workflow Triggers">
                        <div class="grid">
                            <Rock:Grid ID="gWorkflowTriggers" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Workflow Trigger" ShowConfirmDeleteDialog="false">
                                <Columns>
                                    <Rock:RockBoundField DataField="WorkflowType" HeaderText="Workflow Type" />
                                    <Rock:RockBoundField DataField="Trigger" HeaderText="Trigger" />
                                    <Rock:EditField OnClick="gWorkflowTriggers_Edit" />
                                    <Rock:DeleteField OnClick="gWorkflowTriggers_Delete" />
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

        <Rock:ModalDialog ID="dlgAttribute" runat="server" Title="Reservation Attributes" OnSaveClick="dlgReservationTypeAttribute_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="Attributes">
            <Content>
                <Rock:AttributeEditor ID="edtAttributes" runat="server" ShowActions="false" ValidationGroup="Attributes" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgMinistries" runat="server" ScrollbarEnabled="false" SaveButtonText="Add" OnSaveClick="btnAddMinistry_Click" Title="Add Ministry" ValidationGroup="Ministry">
            <Content>
                <asp:HiddenField ID="hfAddMinistryGuid" runat="server" />
                <Rock:DataTextBox ID="tbMinistryName" SourceTypeName="com.bemaservices.RoomManagement.Model.ReservationMinistry, com.bemaservices.RoomManagement" PropertyName="Name" Label="Ministry Name" runat="server" ValidationGroup="Ministry" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgWorkflowTrigger" runat="server" Title="Select Workflow" OnSaveClick="dlgWorkflowTrigger_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="WorkflowTrigger">
            <Content>

                <asp:HiddenField ID="hfAddWorkflowTriggerGuid" runat="server" />

                <asp:ValidationSummary ID="valWorkflowTriggerSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="WorkflowTrigger" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlTriggerType" runat="server" Label="Launch Workflow When" DataTextField="Name" DataValueField="Id"
                            OnSelectedIndexChanged="ddlTriggerType_SelectedIndexChanged" AutoPostBack="true" Required="true" ValidationGroup="Workflow" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlWorkflowType" runat="server" Label="Workflow Type" DataTextField="Name" DataValueField="Id"
                            Required="true" ValidationGroup="Workflow" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlPrimaryQualifier" runat="server" Visible="false" ValidationGroup="Workflow" />
                        <Rock:RockDropDownList ID="ddlSecondaryQualifier" runat="server" Visible="false" ValidationGroup="Workflow" />
                    </div>
                    <div class="col-md-6">
                    </div>
                </div>

            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
