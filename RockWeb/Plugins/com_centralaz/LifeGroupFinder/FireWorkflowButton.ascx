<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FireWorkflowButton.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.LifeGroupFinder.FireWorkflowButton" %>

<asp:Panel ID="pnlContent" runat="server">
    <div class="leadertoolbox-workflowbuttons">
    <asp:Repeater ID="rptLeaderWorkflows" runat="server">
        <ItemTemplate>
            <a href="<%# Eval("Url") %>" class="btn btn-primary"><%# Eval("Name") %></a>  
        </ItemTemplate>
    </asp:Repeater></div>
</asp:Panel>


