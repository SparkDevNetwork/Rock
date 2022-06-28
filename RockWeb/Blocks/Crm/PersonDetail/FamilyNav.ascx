<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FamilyNav.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.FamilyNav" %>
<asp:UpdatePanel ID="pnlContent" runat="server">
    <ContentTemplate>
        <div class="hide-scroll">
            <div class="dropdown dropdown-family styled-scroll">
                <asp:Literal ID="litFamilyMemberNav" runat="server"></asp:Literal>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>