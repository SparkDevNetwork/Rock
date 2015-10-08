<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowTriggerDetail.ascx.cs" Inherits="RockWeb.Blocks.WorkFlow.WorkflowTriggerDetail" %>

<asp:UpdatePanel ID="upWorkflowTrigger" runat="server">
    <ContentTemplate>
        <script>
            function clearQualifierData(element, keyCode) {
                $('#<%=tbQualifierValue.ClientID%>').val('');
            }

            function clearAltQualifierData(element, keyCode) {
                $('#<%=tbPreviousQualifierValue.ClientID%>').val('');
                $('#<%=tbQualifierValueAlt.ClientID%>').val('');
            }
        </script>
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
                            <Rock:RockRadioButtonList ID="rblTriggerType" runat="server" RepeatDirection="Horizontal" Label="Trigger Type" AutoPostBack="true" OnSelectedIndexChanged="rblTriggerType_SelectedIndexChanged" Help="Determines when the trigger should be fired. 'Pre' events allow the workflow to cancel the action and notify the user. They should be used when the entity's original and new values need to be compared. 'Post' events are more efficient because they prevent the user interface from being blocked. 'Immediate Post Save' events are used when the workflow should run immediately and the entity ID is required." />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbIsActive" runat="server" Text="Active" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataDropDownList ID="ddlEntityType" runat="server" SourceTypeName="Rock.Model.WorkflowTrigger, Rock" PropertyName="EntityType" AutoPostBack="true" OnSelectedIndexChanged="ddlEntityType_SelectedIndexChanged" />
                            <Rock:DataDropDownList ID="ddlWorkflowType" runat="server" SourceTypeName="Rock.Model.WorkflowTrigger, Rock" PropertyName="WorkflowType" Required="true" Help="The workflow type to run when a change occurs." />
                            <Rock:DataTextBox ID="tbWorkflowName" runat="server" SourceTypeName="Rock.Model.WorkflowTrigger, Rock" PropertyName="WorkflowName" Help="The name to use for each workflow created." />
                        </div>
                        <div class="col-md-6">
                            <Rock:DataDropDownList ID="ddlQualifierColumn" runat="server" SourceTypeName="Rock.Model.WorkflowTrigger, Rock" PropertyName="EntityTypeQualifierColumn" Required="false" Help="Optional: Provide a specific column that you want to use as a filter for the trigger. You must also provide a qualifier value for this filter to work." />
                            <div class="well">
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:DataTextBox ID="tbPreviousQualifierValue" runat="server" onkeypress="javascript:clearQualifierData()" SourceTypeName="Rock.Model.WorkflowTrigger, Rock" PropertyName="EntityTypeQualifierValuePrevious" Label="Changing From" Required="false" Visible="false" Help="Optional: Provide a specific previous value for the property to filter on. If left blank it means the new value must be different than its previous value (i.e. changed)." />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:DataTextBox ID="tbQualifierValueAlt" runat="server"  onkeypress="javascript:clearQualifierData()" SourceTypeName="Rock.Model.WorkflowTrigger, Rock" PropertyName="EntityTypeQualifierValue" Label="To" Help="Optional: Provide a specific value for the property to filter on.  If left blank it means the previous value must be different than its new value (i.e. changed)." />
                                    </div>
                                </div>
                                <Rock:DataTextBox ID="tbQualifierValue" runat="server" onkeypress="javascript:clearAltQualifierData()" SourceTypeName="Rock.Model.WorkflowTrigger, Rock" PropertyName="EntityTypeQualifierValue" Label="Or value is" Help="Optional: Provide a specific value for the property to filter on." />                            </div>
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
