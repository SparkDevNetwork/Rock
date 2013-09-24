<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowTriggerDetail.ascx.cs" Inherits="RockWeb.Blocks.Administration.WorkflowTriggerDetail" %>

<asp:UpdatePanel ID="upWorkflowTrigger" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server" Visible="false">
            <asp:HiddenField ID="hfWorkflowTriggerId" runat="server" />

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-error" />

            <fieldset>
                <legend>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </legend>

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div class="row-fluid">
                    <div class="span6">
                        <Rock:DataDropDownList ID="ddlEntityType" runat="server" SourceTypeName="Rock.Model.WorkflowTrigger, Rock" PropertyName="EntityType" AutoPostBack="true" OnSelectedIndexChanged="ddlEntityType_SelectedIndexChanged" />
                        <Rock:DataDropDownList ID="ddlQualifierColumn" runat="server" SourceTypeName="Rock.Model.WorkflowTrigger, Rock" PropertyName="EntityTypeQualifierColumn" Required="false" />
                        <Rock:DataTextBox ID="tbQualifierValue" runat="server" SourceTypeName="Rock.Model.WorkflowTrigger, Rock" PropertyName="EntityTypeQualifierValue" />
                    </div>
                    <div class="span6">
                        <Rock:DataDropDownList ID="ddlWorkflowType" runat="server" SourceTypeName="Rock.Model.WorkflowTrigger, Rock" PropertyName="WorkflowType" Required="true" Help="The workflow type to run" />
                        <Rock:RockRadioButtonList ID="rblTriggerType" runat="server" RepeatDirection="Horizontal" Label="Trigger Type" Help="When should the workflow be triggered" />
                        <Rock:DataTextBox ID="tbWorkflowName" runat="server" SourceTypeName="Rock.Model.WorkflowTrigger, Rock" PropertyName="WorkflowName" Help="The name to use for each workflow created" />
                    </div>
                </div>
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
