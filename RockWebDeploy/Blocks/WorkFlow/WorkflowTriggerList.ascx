<%@ control language="C#" autoeventwireup="true" inherits="RockWeb.Blocks.WorkFlow.WorkflowTriggerList, RockWeb" %>

<asp:UpdatePanel ID="upWorkflowTrigger" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-magic"></i> Workflow Trigger List</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfWorkflowTrigger" runat="server" >
                        <Rock:RockCheckBox ID="cbIncludeInactive" runat="server" Label="Include Inactive" Text="Yes" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gWorkflowTrigger" runat="server" AllowSorting="false" OnRowSelected="gWorkflowTrigger_Edit">
                        <Columns>
                            <asp:BoundField DataField="EntityTypeFriendlyName" HeaderText="Entity" />
                            <Rock:EnumField DataField="WorkflowTriggerType" HeaderText="Type" />
                            <asp:BoundField DataField="EntityTypeQualifierColumn" HeaderText="Qualifier Column" />
                            <asp:BoundField DataField="EntityTypeQualifierValue" HeaderText="Qualifier Value"  />
                            <asp:BoundField DataField="WorkflowTypeName" HeaderText="Workflow"  />
                            <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                            <Rock:DeleteField OnClick="gWorkflowTrigger_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>


    </ContentTemplate>
</asp:UpdatePanel>
