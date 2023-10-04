<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DecompressCommunication.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.DatabaseThinner.DecompressCommunication" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <div class="alert alert-info">
            This block is not intended to be browsed to directly.
            It is only for decompressing communications passed in via the URL parameters.
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
