<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Success.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Success" %>
<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <div class="alert alert-success">
        <strong>Success!</strong> You've sucessfully checked in.  Details...
    </div>

    <ol>
        <asp:PlaceHolder ID="phResults" runat="server"></asp:PlaceHolder>
    </ol>


    <div class="actions">
        <asp:LinkButton CssClass="btn btn-primary" ID="lbDone" runat="server" OnClick="lbDone_Click" Text="Done" />
        <asp:LinkButton CssClass="btn btn-secondary" ID="lbAnother" runat="server" OnClick="lbAnother_Click" Text="Another Person" />
    </div>

</ContentTemplate>
</asp:UpdatePanel>
