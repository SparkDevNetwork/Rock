<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupMemberNavigation.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.GroupMemberNavigation" %>
<asp:UpdatePanel ID="pnlContent" runat="server">
    <ContentTemplate>
        <div class="hide-scroll">
            <div class="dropdown dropdown-family styled-scroll">
                <asp:Literal ID="litGroupMemberNav" runat="server"></asp:Literal>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>