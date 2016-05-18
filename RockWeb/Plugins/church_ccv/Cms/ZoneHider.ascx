<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ZoneHider.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Cms.ZoneHider" ViewStateMode="Disabled" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        <style runat="server" id="sBreadCrumbStyleHidden" visible="false">
            .breadcrumb {
                display: none;
            }
        </style>
    </ContentTemplate>
</asp:UpdatePanel>

