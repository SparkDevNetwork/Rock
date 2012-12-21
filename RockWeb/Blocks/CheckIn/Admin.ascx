<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Admin.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Admin" %>
<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <fieldset>
    <legend>Check-In Configuration</legend>

        <Rock:LabeledDropDownList ID="ddlKiosk" runat="server" LabelText="Kiosk Device" OnSelectedIndexChanged="ddlKiosk_SelectedIndexChanged" AutoPostBack="true" DataTextField="Name" DataValueField="Id" ></Rock:LabeledDropDownList>
        <Rock:LabeledCheckBoxList ID="cblGroupTypes" runat="server" LabelText="Group Type(s)" DataTextField="Name" DataValueField="Id" ></Rock:LabeledCheckBoxList>

    </fieldset>

    <div class="actions">
        <asp:LinkButton CssClass="btn btn-primary" ID="lbOk" runat="server" OnClick="lbOk_Click" Text="Ok" />
    </div>

</ContentTemplate>
</asp:UpdatePanel>
