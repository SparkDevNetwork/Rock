<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttributeValues.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.AttributeValues" %>

<asp:UpdatePanel ID="upnlAttributeValues" runat="server" class="context-attribute-values">
<ContentTemplate>

    <section class="panel panel-persondetails">
        <div class="panel-heading rollover-container clearfix">
            <h3 class="panel-title pull-left">
                <asp:Literal ID="lCategoryName" runat="server" />
            </h3>
            <div class="actions rollover-item pull-right">
                <asp:LinkButton ID="lbOrder" runat="server" CssClass="edit" ToolTip="Order Attributes" OnClick="lbOrder_Click" CausesValidation="false"><i class="fa fa-bars"></i></asp:LinkButton>
                <asp:LinkButton ID="lbEdit" runat="server" CssClass="edit" ToolTip="Edit Attributes" OnClick="lbEdit_Click"><i class="fa fa-pencil"></i></asp:LinkButton>
            </div>
        </div>
        <div class="panel-body">
            <Rock:HiddenFieldWithClass ID="hfAttributeOrder" runat="server" CssClass="js-attribute-values-order" />
            <fieldset id="fsAttributes" runat="server" class="attribute-values"></fieldset>
            <asp:Panel ID="pnlActions" runat="server" CssClass="actions" Visible="false">
                <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary btn-xs" OnClick="btnSave_Click" ValidationGroup="vgAttributeValues" />
                <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-xs btn-link" OnClick="btnCancel_Click" CausesValidation="false" />
            </asp:Panel>
        </div>
    </section>

</ContentTemplate>
</asp:UpdatePanel>

