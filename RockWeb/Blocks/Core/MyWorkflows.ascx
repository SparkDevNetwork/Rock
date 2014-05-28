<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MyWorkflows.ascx.cs" Inherits="RockWeb.Blocks.Core.MyWorkflows" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <ul>
            <asp:Repeater ID="rptWorkflows" runat="server" >
                <ItemTemplate >
                    <li><a href="<%# FormatUrl( (int)Eval("WorkflowTypeId"), (int)Eval("WorkflowId") ) %>"><%# Eval("WorkflowType") %>: <%# Eval("Workflow") %> (<%# Eval("Activity") %>)</a></li>
                </ItemTemplate>
            </asp:Repeater>
        </ul>

    </ContentTemplate>
</asp:UpdatePanel>
