<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupTypeSelect.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.GroupTypeSelect" %>
<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <fieldset>
    <legend><asp:Literal ID="lPersonName" runat="server" /></legend>

        <div class="control-group">
            <label class="control-label">Select Group Type</label>
            <div class="controls">
                <asp:ListBox ID="lbGroupTypes" runat="server"></asp:ListBox>
            </div>
        </div>
        
    </fieldset>

    <div class="actions">
        <asp:LinkButton CssClass="btn btn-primary" ID="lbSelect" runat="server" OnClick="lbSelect_Click" Text="Select" />
        <asp:LinkButton CssClass="btn btn-secondary" ID="lbBack" runat="server" OnClick="lbBack_Click" Text="Back" />
        <asp:LinkButton CssClass="btn btn-secondary" ID="lbCancel" runat="server" OnClick="lbCancel_Click" Text="Cancel" />
    </div>

</ContentTemplate>
</asp:UpdatePanel>
