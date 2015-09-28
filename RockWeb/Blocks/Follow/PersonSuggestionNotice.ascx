<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonSuggestionNotice.ascx.cs" Inherits="RockWeb.Blocks.Follow.PersonSuggestionNotice" %>

<asp:UpdatePanel ID="pnlSuggestionListUpdatePanel" runat="server">
    <ContentTemplate>

        <asp:LinkButton ID="lbSuggestions" runat="server" CssClass="btn btn-primary btn-block margin-b-sm" OnClick="lbSuggestions_Click" />

        <asp:LinkButton ID="lbFollowing" runat="server" CssClass="btn btn-default btn-block margin-b-md" OnClick="lbFollowing_Click"><i class="fa fa-flag"></i> Following List</asp:LinkButton>

    </ContentTemplate>
</asp:UpdatePanel>
