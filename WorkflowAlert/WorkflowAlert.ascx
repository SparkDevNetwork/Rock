<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowAlert.ascx.cs" Inherits="RockWeb.Plugins.cc_newspring.Blocks.WorkflowAlert.WorkflowAlert" %>

<asp:HiddenField ID="workflowAlertNumber" runat="server" />

<asp:LinkButton ID="lbListingPage" class="workflowNotifications" runat="server" OnClick="lbListingPage_Click">

    <% if ( workflowAlertNumber.Value != "0" )
       { %>
    <i class="fa fa-bell">
        <span class="badge badge-danger"><%= workflowAlertNumber.Value %></span>
    </i>
    <% }
       else
       { %>
    <i class="fa fa-bell-o"></i>
   <% } %>
</asp:LinkButton>

<script type="text/javascript" src="../plugins/cc_newspring/Blocks/WorkflowAlert/loadcss.js"></script>