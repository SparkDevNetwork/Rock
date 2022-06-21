<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FamilyNav.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.FamilyNav" %>
<asp:UpdatePanel ID="pnlContent" runat="server">
    <ContentTemplate>

        <script>
            Sys.Application.add_load(function() {
                Rock.controls.priorityNav.initialize({controlId: 'overflow-nav'})
            });
        </script>

        <div class="profile-sticky-nav-placeholder"></div>
        <div class="hide-scroll">
            <div class="profile-nav" data-layout="PersonProfileHome">
                <div class="flex-1 z-10">
                
                    <div class="dropdown dropdown-family styled-scroll">
                        <asp:Literal ID="litFamilyMemberNav" runat="server"></asp:Literal>
                    </div>
                </div>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>