<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowTriggerDetail.ascx.cs" Inherits="RockWeb.Blocks.Core.WorkflowTriggerDetail" %>

<asp:UpdatePanel ID="upWorkflowTrigger" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server" Visible="false">
            <asp:HiddenField ID="hfWorkflowTriggerId" runat="server" />

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

            
            <div class="banner">
                <h1><asp:Literal ID="lActionTitle" runat="server" /></h1>
            </div>  
              
            <fieldset>
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataDropDownList ID="ddlEntityType" runat="server" SourceTypeName="Rock.Model.WorkflowTrigger, Rock" PropertyName="EntityType" AutoPostBack="true" OnSelectedIndexChanged="ddlEntityType_SelectedIndexChanged" />
                        <Rock:DataDropDownList ID="ddlQualifierColumn" runat="server" SourceTypeName="Rock.Model.WorkflowTrigger, Rock" PropertyName="EntityTypeQualifierColumn" Required="false" Help="Optional: Provide a specific column that you want to use as a filter for the trigger. You must also provide a value for this filter to work." />
                        <Rock:DataTextBox ID="tbQualifierValue" runat="server" SourceTypeName="Rock.Model.WorkflowTrigger, Rock" PropertyName="EntityTypeQualifierValue" Help="Optional: Provide a specific value for the property to filter on." />
                    </div>
                    <div class="col-md-6">
                        <Rock:DataDropDownList ID="ddlWorkflowType" runat="server" SourceTypeName="Rock.Model.WorkflowTrigger, Rock" PropertyName="WorkflowType" Required="true" Help="The workflow type to run when a change occurs." />
                        <Rock:RockRadioButtonList ID="rblTriggerType" runat="server" RepeatDirection="Horizontal" Label="Trigger Type" Help="Determines when the tigger should be fired. Using a 'pre' event allows the workflow to cancel the action and notify the user." />
                        <Rock:DataTextBox ID="tbWorkflowName" runat="server" SourceTypeName="Rock.Model.WorkflowTrigger, Rock" PropertyName="WorkflowName" Help="The name to use for each workflow created." />
                    </div>
                </div>
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
