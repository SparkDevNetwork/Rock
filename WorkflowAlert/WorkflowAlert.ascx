<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowAlert.ascx.cs" Inherits="RockWeb.Plugins.cc_newspring.Blocks.WorkflowAlert.WorkflowAlert" %>

<asp:HiddenField ID="workflowAlertNumber" runat="server" />
<asp:HiddenField ID="filterWorkflowTypes" runat="server" />

<asp:LinkButton ID="lbListingPage" class="workflowNotifications" runat="server" OnClick="lbListingPage_Click" EnableViewState="false">
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

<% if ( filterWorkflowTypes.Value == "active" ) { %>
<script>
    $(document).ready(function () {
        // Currently Hard Coded
        WebForm_DoPostBackWithOptions(new WebForm_PostBackOptions("ctl00$main$ctl23$ctl01$ctl06$tglDisplay$tglDisplay_btnOn", "", true, "", "", false, true))
    });
</script>
<% } %>

<script type="text/javascript" src="/plugins/cc_newspring/Blocks/WorkflowAlert/loadcss.js"></script>