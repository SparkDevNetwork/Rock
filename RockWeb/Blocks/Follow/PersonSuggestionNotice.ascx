<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonSuggestionNotice.ascx.cs" Inherits="RockWeb.Blocks.Follow.PersonSuggestionNotice" %>

<asp:UpdatePanel ID="pnlSuggestionListUpdatePanel" runat="server">
    <ContentTemplate>

        <asp:LinkButton ID="lbSuggestions" runat="server" CssClass="btn btn-primary btn-block margin-b-md" OnClick="lbSuggestions_Click" />

    </ContentTemplate>
</asp:UpdatePanel>
