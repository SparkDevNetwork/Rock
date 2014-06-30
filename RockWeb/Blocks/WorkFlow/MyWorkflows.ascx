<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MyWorkflows.ascx.cs" Inherits="RockWeb.Blocks.WorkFlow.MyWorkflows" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:Toggle ID="tglRole" runat="server" OnText="Initiated By Me" OffText="Assigned To Me" AutoPostBack="true" OnCheckedChanged="tgl_CheckedChanged" />
        <Rock:Toggle ID="tglDisplay" runat="server" OnText="Active Types" OffText="All Types" AutoPostBack="true" OnCheckedChanged="tgl_CheckedChanged" />

        <div class="page-list-as-blocks">
            <ul class="list-unstyled">
                <asp:Repeater ID="rptWorkflowTypes" runat="server">
                    <ItemTemplate>
                        <li class='<%# Eval("Class") %>'>
                            <asp:LinkButton ID="lbWorkflowType" runat="server" CommandArgument='<%# Eval("WorkflowType.Id") %>' CommandName="Display">
                                <i class='<%# Eval("WorkflowType.IconCssClass") %>'></i>
                                <h3><%# Eval("WorkflowType.Name") %> <small><%# ((int)Eval("Count")).ToString("#,###,###") %></small></h3>
                            </asp:LinkButton>
                        </li>
                    </ItemTemplate>
                </asp:Repeater>
            </ul>
        </div>

        <h4><asp:Literal ID="lWorkflow" runat="server"></asp:Literal></h4>
        <div class="grid">
            <Rock:Grid ID="gWorkflows" runat="server" OnRowSelected="gWorkflows_Edit" >
                <Columns>
                    <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                    <asp:BoundField DataField="Status" HeaderText="Status" SortExpression="Status" />
                    <asp:BoundField DataField="ActiveActivityNames" HeaderText="Active Activities" HtmlEncode="false" />
                </Columns>
            </Rock:Grid>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
