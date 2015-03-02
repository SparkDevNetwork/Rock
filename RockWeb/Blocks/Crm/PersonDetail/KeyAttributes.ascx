<%@ Control Language="C#" AutoEventWireup="true" CodeFile="KeyAttributes.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.KeyAttributes" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upKeyAttributes" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <section class="panel panel-persondetails bookmarkattributes attributeholder">

            <div class="panel-heading rollover-container clearfix">
                <h3 class="panel-title pull-left"><i class="fa fa-bookmark"></i> Bookmarked Attributes</h3> 
                <div class="actions rollover-item pull-right">
                    <asp:LinkButton ID="lbOrder" runat="server" CssClass="edit" ToolTip="Order Attributes" OnClick="lbOrder_Click" CausesValidation="false"><i class="fa fa-bars"></i></asp:LinkButton>
                    <asp:LinkButton ID="lbEdit" runat="server" CssClass="edit" ToolTip="Edit Attributes" OnClick="lbEdit_Click" CausesValidation="false"><i class="fa fa-pencil"></i></asp:LinkButton>
                    <asp:LinkButton ID="lbConfigure" runat="server" CssClass="edit" ToolTip="Select Attributes" OnClick="lbConfigure_Click" CausesValidation="false" ><i class="fa fa-cog"></i></asp:LinkButton>
                </div>
            </div>

            <div class="panel-body">
                <Rock:HiddenFieldWithClass ID="hfAttributeOrder" runat="server" CssClass="js-attribute-values-order" />
                <fieldset id="fsAttributes" runat="server" class="attribute-values"></fieldset>
                <asp:Panel ID="pnlActions" runat="server" CssClass="actions" Visible="false">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary btn-xs" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link btn-xs" OnClick="btnCancel_Click" CausesValidation="false" />
                </asp:Panel>
            </div>
 
            <Rock:ModalDialog ID="dlgKeyAttribute" runat="server" Title="Bookmarked Attributes" ValidationGroup="KeyAttribute" OnCancelScript="clearActiveDialog();" >
            <Content>

                <Rock:RockDropDownList ID="ddlCategories" runat="server" Label="Category" AutoPostBack="true" OnSelectedIndexChanged="ddlCategories_SelectedIndexChanged" />
                <Rock:RockCheckBoxList ID="cblAttributes" runat="server" Label="Attributes" RepeatDirection="Vertical" />

            </Content>
            </Rock:ModalDialog>
                               
        </section>

    </ContentTemplate>
</asp:UpdatePanel>
