<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowList.ascx.cs" Inherits="RockWeb.Blocks.Administration.WorkflowList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlWorkflowList" runat="server" Visible="false">
            <h4>
                <asp:Literal ID="lGridTitle" runat="server" Text="Workflows" />
            </h4>
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
            <Rock:Grid ID="gWorkflows" runat="server" DisplayType="Full" OnRowSelected="gWorkflows_Edit">
                <Columns>
                    <asp:BoundField DataField="Name" HeaderText="Name" />
                    <asp:BoundField DataField="Status" HeaderText="Status" />
                    <Rock:DeleteField OnClick="gWorkflows_Delete" />
                </Columns>
            </Rock:Grid>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
