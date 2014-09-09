<%@ control language="C#" autoeventwireup="true" inherits="RockWeb.Blocks.Crm.PersonDetail.KeyAttributes, RockWeb" %>

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
                    <asp:LinkButton ID="lbEdit" runat="server" CssClass="edit" Text="Edit Attributes" OnClick="lbEdit_Click" CausesValidation="false"><i class="fa fa-pencil"></i></asp:LinkButton>
                    <asp:LinkButton ID="lbConfigure" runat="server" CssClass="edit" Text="Select Attributes" OnClick="lbConfigure_Click" CausesValidation="false" ><i class="fa fa-cog"></i></asp:LinkButton>
                </div>
            </div>

            <div class="panel-body">
                <fieldset id="fsAttributes" runat="server"></fieldset>
                <asp:Panel ID="pnlActions" runat="server" CssClass="actions" Visible="false">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary btn-xs" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link btn-xs" OnClick="btnCancel_Click" CausesValidation="false" />
                </asp:Panel>
            </div>
 
            <Rock:ModalDialog ID="dlgKeyAttribute" runat="server" Title="Bookmarked Attributes" Content-Height="380" ValidationGroup="KeyAttribute" OnCancelScript="clearActiveDialog();" >
            <Content>

                <Rock:RockDropDownList ID="ddlCategories" runat="server" Label="Category" AutoPostBack="true" OnSelectedIndexChanged="ddlCategories_SelectedIndexChanged" />
                <Rock:RockCheckBoxList ID="cblAttributes" runat="server" Label="Attributes" RepeatDirection="Vertical" />

            </Content>
            </Rock:ModalDialog>
                               
        </section>

    </ContentTemplate>
</asp:UpdatePanel>
