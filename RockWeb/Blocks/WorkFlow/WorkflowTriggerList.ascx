<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowTriggerList.ascx.cs" Inherits="RockWeb.Blocks.WorkFlow.WorkflowTriggerList" %>

<asp:UpdatePanel ID="upWorkflowTrigger" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        
        <div class="grid">
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

    </ContentTemplate>
</asp:UpdatePanel>
