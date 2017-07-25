﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ActiveUsers.ascx.cs" Inherits="RockWeb.Blocks.Cms.ActiveUsers" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Literal ID="lSiteName" runat="server" />
        <asp:Literal ID="lMessages" runat="server" />

        <asp:Literal ID="lUsers" runat="server" />
        <script>
            Sys.Application.add_load(function () {
                $('.active-user').tooltip({ 'html': 'true' });
                $('.js-current-guests .badge').tooltip();
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
