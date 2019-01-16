<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Override.ascx.cs" Inherits="RockWeb.Plugins.com_bemadev.CheckIn.Override" %>
<style>
    .override-btn {
        position: absolute;
        left: 20px;
    }

</style>


<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>

        <asp:LinkButton runat="server" ID="btnOverrideCustom" CssClass="kioskmanager-activate override-btn" OnClick="btnOverrideCustom_Click"><i class="fa fa-exchange fa-4x"></i></asp:LinkButton>

    </ContentTemplate>
</asp:UpdatePanel>
