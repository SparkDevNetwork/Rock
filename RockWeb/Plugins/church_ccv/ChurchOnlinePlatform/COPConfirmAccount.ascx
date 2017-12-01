<%@ Control Language="C#" AutoEventWireup="true" CodeFile="COPConfirmAccount.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.COP.ConfirmAccount" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
<ContentTemplate>

    <div class="sso-createaccount-panel-wrapper">
    <img src="themes/church_ccv_external_v8/assets/ccv_logo-hi-res.png" style="width: 125px; margin-bottom: 25px;"/>
        <asp:Panel ID="pnlCode" runat="server" Visible="false">

            <asp:Panel ID="pnlInvalid" runat="server" CssClass="alert alert-danger error">
                <asp:Literal ID="lInvalid" runat="server" ></asp:Literal> 
            </asp:Panel>

            <fieldset>
                <legend>Enter Code</legend>
                <Rock:RockTextBox ID="tbConfirmationCode" runat="server" Label="Code" Required="true" ></Rock:RockTextBox>
            </fieldset>

            <div class="actions">
                <asp:Button ID="btnCodeConfirm" runat="server" Text="Confirm Account" CssClass="btn btn-primary" OnClick="btnCodeConfirm_Click" />
            </div>

        </asp:Panel>
    </div>

</ContentTemplate>
</asp:UpdatePanel>
