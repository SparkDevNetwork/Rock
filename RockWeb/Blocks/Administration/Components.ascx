<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Components.ascx.cs" Inherits="RockWeb.Blocks.Administration.Components" %>

<asp:UpdatePanel runat="server">
<ContentTemplate>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert-message block-message error"/>

    <asp:Panel ID="pnlList" runat="server" Visible="false" >
        
        <Rock:Grid ID="rGrid" runat="server" EmptyDataText="No Components Found" >
            <Columns>
                <Rock:ReorderField />
                <asp:BoundField DataField="Name" HeaderText="Name" />
                <asp:BoundField DataField="Description" HeaderText="Description" />
                <Rock:BoolField DataField="IsActive" HeaderText="Active" />
                <Rock:EditField OnClick="rGrid_Edit" />
            </Columns>
        </Rock:Grid>

    </asp:Panel>

    <asp:Panel ID="pnlDetails" runat="server" Visible="false" CssClass="admin-details">
        
        <asp:ValidationSummary runat="server" CssClass="failureNotification" />

        <fieldset>
            <legend><asp:Literal ID="lProperties" runat="server"></asp:Literal></legend>
            <asp:PlaceHolder ID="phProperties" runat="server"></asp:PlaceHolder>
        </fieldset>

        <div class="actions">
            <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" onclick="btnSave_Click" />
            <asp:LinkButton id="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary" CausesValidation="false" OnClick="btnCancel_Click" />
        </div>

    </asp:Panel>

</ContentTemplate>
</asp:UpdatePanel>
