<%@ Control Language="C#" AutoEventWireup="true" CodeFile="HtmlSnippetEditor.ascx.cs" Inherits="RockWeb.Blocks.Utility.HtmlSnippetEditor" %>
<asp:Panel ID="pnlModalHeader" runat="server" Visible="false">
    <h3 class="modal-title">
        <asp:Literal ID="lTitle" runat="server"></asp:Literal>
        <span class="js-cancel-button cursor-pointer pull-right" style="opacity: .5">&times;</span>
    </h3>
</asp:Panel>
<asp:UpdatePanel runat="server" ID="upSnippets">
    <ContentTemplate>
        <Rock:RockTextBox runat="server" ID="tbName" Label="Name" Required="true" />
        <Rock:HtmlEditor runat="server" ID="heSnippet" Rows="100" Height="400"/>
        <Rock:BootstrapButton runat="server" ID="btnSave" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
        <asp:LinkButton runat="server" ID="btnCancel" Text="Cancel" CausesValidation="false" OnClick="btnCancel_Click" />
    </ContentTemplate>
</asp:UpdatePanel>
<Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Danger" Visible="false" Dismissable="true" />