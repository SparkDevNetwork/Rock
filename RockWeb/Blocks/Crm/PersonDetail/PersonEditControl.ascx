<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonEditControl.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.PersonEditControl" %>
<asp:UpdatePanel ID="pnlContent" runat="server">
    <ContentTemplate>

        <script>
            Sys.Application.add_load(function() {
                Rock.controls.priorityNav.initialize({controlId: 'overflow-nav'})
            });
        </script>
        <div class="profile-sticky-nav-placeholder"></div>

        <div class="d-flex flex-1 justify-content-end overflow-hidden">
            <div class="hide-scroll">
                <div class="d-flex flex-row flex-nowrap">
                    <div class="d-flex flex-column align-items-center p-1 text-sm text-muted position-relative" title="Edit Person">
                            <a href="/person/<%=Person.Id %>/edit" class="btn btn-default btn-sm btn-go btn-square">
                                <i class="fa fa-pencil"></i>
                            </a>
                        </div>
                
                </div>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>