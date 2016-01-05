<%@ Control Language="C#" AutoEventWireup="true" CodeFile="NoteFromDefinedType.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Core.NoteFromDefinedType" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:RockDropDownList ID="ddlNoteType" runat="server" Label="Note Type" />
        <Rock:RockDropDownList ID="ddlNoteValueList" runat="server" Label="Select Note" AutoPostBack="true" OnSelectedIndexChanged="ddlNoteValueList_SelectedIndexChanged" />
        <Rock:RockTextBox ID="tbOtherText" runat="server" Label="Other" TextMode="MultiLine" Rows="3" Required="true" />

        <asp:Literal ID="lLavaDebug" runat="server" />

        <div class="actions">
            <asp:LinkButton ID="btnAddNote" runat="server" AccessKey="s" Text="Add" CssClass="btn btn-default" OnClick="btnAddNote_Click" />
        </div>

        <div class="margin-t-md">
            <Rock:NoteContainer ID="noteList" runat="server" AddAllowed="false" Title="History" />
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
