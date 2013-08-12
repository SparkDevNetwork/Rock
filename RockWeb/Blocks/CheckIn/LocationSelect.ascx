<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LocationSelect.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.LocationSelect" %>
<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <fieldset>
    <legend><asp:Literal ID="lGroupName" runat="server" /></legend>

        <div class="control-group">
            <label class="control-label">Select Location</label>
            <div class="controls">
                <asp:Repeater ID="rSelection" runat="server" OnItemCommand="rSelection_ItemCommand">
                    <ItemTemplate>
                        <asp:LinkButton ID="lbSelect" runat="server" CommandArgument='<%# Eval("Location.Id") %>' CssClass="btn btn-primary btn-large btn-block"><%# Container.DataItem.ToString() %><%# FormatCount( (int)Eval("Location.Id") ) %></asp:LinkButton>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>
        
    </fieldset>

    <div class="actions">
        <asp:LinkButton CssClass="btn btn-secondary" ID="lbBack" runat="server" OnClick="lbBack_Click" Text="Back" />
        <asp:LinkButton CssClass="btn btn-secondary" ID="lbCancel" runat="server" OnClick="lbCancel_Click" Text="Cancel" />
    </div>

</ContentTemplate>
</asp:UpdatePanel>
