<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReminderTypes.ascx.cs" Inherits="RockWeb.Blocks.Reminders.ReminderTypes" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-bell"></i> Reminder Types
                </h1>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gReminderTypes" runat="server" DataKeyNames="Id" OnRowSelected="gReminderTypes_RowSelected">
                        <Columns>
                            <Rock:ReorderField />
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                            <Rock:RockBoundField DataField="EntityType" HeaderText="Entity Type" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" />
                            <Rock:SecurityField TitleField="Name" />
                            <Rock:DeleteField OnClick="gReminderTypes_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
        
            </div>
        </asp:Panel>


        <Rock:ModalDialog ID="mdEditReminderType" Title="Edit Reminder Type" runat="server" ValidationGroup="EditReminderType" OnSaveClick="mdEditReminderType_SaveClick" CancelLinkVisible="true">
            <Content>
                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="EditReminderType" />

                <asp:HiddenField ID="hfReminderTypeId" runat="server" Value="0" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbName" runat="server" Label="Name" Required="true" ValidationGroup="EditReminderType" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbActive" runat="server" Label="Active" />
                    </div>
                </div>

                <Rock:RockTextBox ID="tbDescription" runat="server" Label="Description" TextMode="MultiLine" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlNotificationType" runat="server" Label="Notification Type" Required="true" ValidationGroup="EditReminderType" AutoPostBack="true" OnSelectedIndexChanged="ddlNotificationType_SelectedIndexChanged" />
                    </div>
                    <div class="col-md-6">
                        <Rock:WorkflowTypePicker ID="wtpWorkflowType" runat="server" Label="Notification Workflow" ValidationGroup="EditReminderType" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbShouldShowNote" runat="server" Label="Should Show Note" />
                    </div>
                    <div class="col-md-6">
                        <Rock:EntityTypePicker ID="etpEntityType" runat="server" Label="Entity Type" Required="true" ValidationGroup="EditReminderType" EnhanceForLongLists="true" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbShouldAutoComplete" runat="server" Label="Should Auto-Complete When Notified" />
                    </div>
                    <div class="col-md-6">
                        <Rock:ColorPicker ID="cpHighlightColor" runat="server" Label="Highlight Color" />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>