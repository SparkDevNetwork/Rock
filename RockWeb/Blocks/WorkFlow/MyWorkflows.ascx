<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MyWorkflows.ascx.cs" Inherits="RockWeb.Blocks.WorkFlow.MyWorkflows" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:Toggle ID="tglDisplay" runat="server" OnText="Active" OffText="All" AutoPostBack="true" OnCheckedChanged="tglDisplay_CheckedChanged" />

        <div class="page-list-as-blocks">
            <ul class="list-unstyled">
                <asp:Repeater ID="rptWorkflowTypes" runat="server">
                    <ItemTemplate>
                        <li class='<%# Eval("Class") %>'>
                            <asp:LinkButton ID="lbWorkflowType" runat="server" CommandArgument='<%# Eval("WorkflowType.Id") %>' CommandName="Display">
                                <i class='<%# Eval("WorkflowType.IconCssClass") %>'></i>
                                <h3><%# Eval("WorkflowType.Name") %></h3>
                                <h3> ( <%# ((int)Eval("Count")).ToString("N0") %> <%# Eval("WorkflowType.WorkTerm").ToString() %> )</h3>
                            </asp:LinkButton>
                        </li>
                    </ItemTemplate>
                </asp:Repeater>
            </ul>
        </div>

        <h4><asp:Literal ID="lWorkflow" runat="server"></asp:Literal></h4>
        <Rock:Grid ID="gWorkflows" runat="server" OnRowSelected="gWorkflows_Edit" >
            <Columns>
                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                <asp:BoundField DataField="Status" HeaderText="Status" SortExpression="Status" />
                <asp:BoundField DataField="ActiveActivityNames" HeaderText="Active Activities" HtmlEncode="false" />
            </Columns>
        </Rock:Grid>

    </ContentTemplate>
</asp:UpdatePanel>
