<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ForgotUserName.ascx.cs" Inherits="RockWeb.Blocks.Security.ForgotUserName" %>

<asp:UpdatePanel runat="server">
<ContentTemplate>

    <asp:Panel ID="pnlEntry" runat="server">
    
        <asp:Literal ID="lCaption" runat="server" ></asp:Literal> 

        <fieldset>
            <legend>Enter Your Email</legend>
            <Rock:RockTextBox ID="tbEmail" runat="server" Label="Email" Required="true" ></Rock:RockTextBox>
        </fieldset>

        <asp:Panel ID="pnlWarning" runat="server" Visible="false" CssClass="alert warning">
            <asp:Literal ID="lWarning" runat="server"></asp:Literal>
        </asp:Panel>

        <div class="actions">
            <asp:Button ID="btnSend" runat="server" Text="Send Username" CssClass="btn btn-primary" OnClick="btnSend_Click" />
        </div>

    </asp:Panel>

    <asp:Panel ID="pnlSuccess" runat="server" Visible="false" CssClass="alert alert-success success">
        <asp:Literal ID="lSuccess" runat="server"></asp:Literal>
    </asp:Panel>

</ContentTemplate>
</asp:UpdatePanel>
