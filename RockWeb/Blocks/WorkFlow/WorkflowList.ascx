<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowList.ascx.cs" Inherits="RockWeb.Blocks.WorkFlow.WorkflowList" %>

<asp:UpdatePanel ID="upnlSettings" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlWorkflowList" runat="server">

            <h4><asp:Literal ID="lGridTitle" runat="server" Text="Workflows" /></h4>

            <Rock:GridFilter ID="gfWorkflows" runat="server">
                <Rock:RockTextBox ID="tbName" runat="server" Label="Name"></Rock:RockTextBox>
                <Rock:RockTextBox ID="tbStatus" runat="server" Label="Status"></Rock:RockTextBox>
                <Rock:RockDropDownList ID="ddlState" runat="server" Label="State">
                    <asp:ListItem Text="All" Value="" />
                    <asp:ListItem Text="Active" Value="0" />
                    <asp:ListItem Text="Completed" Value="1" />
                 </Rock:RockDropDownList>
                <Rock:DateRangePicker ID="drpActivated" runat="server" Label="Activated" />
                <Rock:DateRangePicker ID="drpCompleted" runat="server" Label="Completed" />
            </Rock:GridFilter>

            <Rock:ModalAlert ID="mdGridWarning" runat="server" />

            <Rock:Grid ID="gWorkflows" runat="server" DisplayType="Full" OnRowSelected="gWorkflows_Edit">
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
