<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowTriggerDetail.ascx.cs" Inherits="RockWeb.Blocks.WorkFlow.WorkflowTriggerDetail" %>

<asp:UpdatePanel ID="upWorkflowTrigger" runat="server">
    <ContentTemplate>
        
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="false">
            <asp:HiddenField ID="hfWorkflowTriggerId" runat="server" />
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-magic"></i> <asp:Literal ID="lActionTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" /> 
              
                <fieldset>
                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataDropDownList ID="ddlEntityType" runat="server" SourceTypeName="Rock.Model.WorkflowTrigger, Rock" PropertyName="EntityType" AutoPostBack="true" OnSelectedIndexChanged="ddlEntityType_SelectedIndexChanged" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbIsActive" runat="server" Text="Active" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataDropDownList ID="ddlQualifierColumn" runat="server" SourceTypeName="Rock.Model.WorkflowTrigger, Rock" PropertyName="EntityTypeQualifierColumn" Required="false" Help="Optional: Provide a specific column that you want to use as a filter for the trigger. You must also provide a value for this filter to work." />
                            <Rock:DataTextBox ID="tbQualifierValue" runat="server" SourceTypeName="Rock.Model.WorkflowTrigger, Rock" PropertyName="EntityTypeQualifierValue" Help="Optional: Provide a specific value for the property to filter on." />
                            <Rock:DataTextBox ID="tbWorkflowName" runat="server" SourceTypeName="Rock.Model.WorkflowTrigger, Rock" PropertyName="WorkflowName" Help="The name to use for each workflow created." />
                        </div>
                        <div class="col-md-6">
                            <Rock:DataDropDownList ID="ddlWorkflowType" runat="server" SourceTypeName="Rock.Model.WorkflowTrigger, Rock" PropertyName="WorkflowType" Required="true" Help="The workflow type to run when a change occurs." />
                            <Rock:RockRadioButtonList ID="rblTriggerType" runat="server" RepeatDirection="Horizontal" Label="Trigger Type" Help="Determines when the trigger should be fired. Using a 'pre' event allows the workflow to cancel the action and notify the user. 'Post' events are considered more efficient because they run the workflow in a job within 60 seconds, thus preventing the user interface execution from being blocked. Immediate post save should be used when the Id of a new entity needs to be known and the workflow should not wait for the job-based execution." />
                            
                        </div>
                    </div>
                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
