<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PrayerCardView.ascx.cs" Inherits="RockWeb.Blocks.Prayer.PrayerCardView" %>

<asp:UpdatePanel ID="upPrayer" runat="server">
    <ContentTemplate>
        <div class="block-header">
            <div class="block-filter">
                <Rock:CampusPicker ID="cpCampus" runat="server" IncludeInactive="false" CssClass="input-width-lg" AutoPostBack="true" OnSelectedIndexChanged="cpCampus_SelectedIndexChanged"/>
            </div>
             <div class="block-body">
                 <asp:Literal ID="lContent" runat="server"></asp:Literal>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>

