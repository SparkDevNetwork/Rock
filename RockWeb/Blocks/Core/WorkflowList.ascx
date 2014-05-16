<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowList.ascx.cs" Inherits="RockWeb.Blocks.Core.WorkflowList" %>

<asp:UpdatePanel ID="upnlSettings" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlWorkflowList" runat="server">
            <h4>
                <asp:Literal ID="lGridTitle" runat="server" Text="Workflows" />
            </h4>
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
            <Rock:Grid ID="gWorkflows" runat="server" DisplayType="Full" OnRowSelected="gWorkflows_Edit" >
                <Columns>
                    <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                    <asp:BoundField DataField="Status" HeaderText="Status" SortExpression="Status" />
                    <asp:TemplateField ItemStyle-Wrap="false">
                        <HeaderTemplate>Active Activities</HeaderTemplate>
                        <ItemTemplate>
                            <asp:Literal ID="lActivities" runat="server"></asp:Literal>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </Rock:Grid>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
