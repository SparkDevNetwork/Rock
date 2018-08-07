<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ProcessOnly.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.ProcessOnly" %>

<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />

     <div class="row-fluid checkin-footer">
        <div class="checkin-actions">
            <asp:LinkButton CssClass="btn btn-default btn-cancel" ID="lbCancel" runat="server" OnClick="lbCancel_Click" Text="Cancel" />
        </div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>
