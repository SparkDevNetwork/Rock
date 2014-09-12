<%@ control language="C#" autoeventwireup="true" inherits="RockWeb.Blocks.Crm.PersonDetail.AttributeValues, RockWeb" %>

<asp:UpdatePanel ID="upnlAttributeValues" runat="server" class="context-attribute-values">
<ContentTemplate>

    <section class="panel panel-persondetails">
        <div class="panel-heading rollover-container clearfix">
            <h3 class="panel-title pull-left">
                <asp:Literal ID="lCategoryName" runat="server" />
            </h3>
            <div class="actions rollover-item pull-right">
                <asp:LinkButton ID="lbEdit" runat="server" CssClass="edit" OnClick="lbEdit_Click"><i class="fa fa-pencil"></i></asp:LinkButton>
            </div>
        </div>
        <div class="panel-body">
            <fieldset id="fsAttributes" runat="server"></fieldset>
            <asp:Panel ID="pnlActions" runat="server" CssClass="actions" Visible="false">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary btn-xs" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-xs btn-link" OnClick="btnCancel_Click" CausesValidation="false" />
            </asp:Panel>
        </div>
    </section>

</ContentTemplate>
</asp:UpdatePanel>

