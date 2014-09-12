<%@ control language="C#" autoeventwireup="true" inherits="RockWeb.Blocks.Core.Notes, RockWeb" %>

<asp:UpdatePanel ID="upNotes" runat="server">
    <ContentTemplate>
        <Rock:NoteContainer ID="notesTimeline" runat="server"/>
    </ContentTemplate>
</asp:UpdatePanel>
