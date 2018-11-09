<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Notes.ascx.cs" Inherits="RockWeb.Blocks.Core.Notes" %>

<asp:UpdatePanel ID="upNotes" runat="server">
    <ContentTemplate>
        <Rock:NoteContainer ID="notesTimeline" runat="server" />
    </ContentTemplate>
</asp:UpdatePanel>
