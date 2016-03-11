<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PageParameterSelection.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Cms.PageParameterSelection" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:RockDropDownList ID="ddlSelection" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlSelection_SelectedIndexChanged" />
    </ContentTemplate>
</asp:UpdatePanel>