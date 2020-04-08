<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FamilyNotes.ascx.cs" Inherits="RockWeb.Plugins.org_hfbc.Legacy685.FamilyNotes" %>

<asp:UpdatePanel ID="upNotes" runat="server">
    <ContentTemplate>
        <Rock:NoteContainer ID="notesTimeline" runat="server"/>
    </ContentTemplate>
</asp:UpdatePanel>
