<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonEditControl.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.PersonEditControl" %>
<asp:UpdatePanel ID="pnlContent" runat="server">
    <ContentTemplate>
        <div id="divEditButton" runat="server" class="d-flex flex-row flex-nowrap">
            <div class="d-flex flex-column align-items-center p-1 text-sm text-muted position-relative" title="Edit Person">
                <a href="/person/<%=Person.Id %>/edit" class="btn btn-default btn-sm btn-go btn-square">
                    <i class="fa fa-pencil"></i>
                </a>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>