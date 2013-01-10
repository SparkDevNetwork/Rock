<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Search.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Search" %>
<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <fieldset>
    <legend>Search</legend>

        <Rock:LabeledTextBox ID="tbPhone" runat="server" LabelText="Phone Number" />
        
    </fieldset>

    <div class="actions">
        <asp:LinkButton CssClass="btn btn-primary" ID="lbSearch" runat="server" OnClick="lbSearch_Click" Text="Search" />
        <asp:LinkButton CssClass="btn btn-secondary" ID="lbBack" runat="server" OnClick="lbBack_Click" Text="Back" />
        <asp:LinkButton CssClass="btn btn-secondary" ID="lbCancel" runat="server" OnClick="lbCancel_Click" Text="Cancel" />
    </div>

</ContentTemplate>
</asp:UpdatePanel>
