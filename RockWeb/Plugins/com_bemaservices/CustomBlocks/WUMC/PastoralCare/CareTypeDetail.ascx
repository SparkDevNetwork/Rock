<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CareTypeDetail.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.PastoralCare.CareTypeDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upCareType" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDeleteConfirm" runat="server" CssClass="panel panel-body" Visible="false">
            <Rock:NotificationBox ID="nbDeleteConfirm" runat="server" NotificationBoxType="Warning" Text="Deleting a Care Type will delete all the Care Items associated with the Care Type. Are you sure you want to delete the Care Type?" />
            <asp:LinkButton ID="btnDeleteConfirm" runat="server" Text="Confirm Delete" CssClass="btn btn-danger" OnClick="btnDeleteConfirm_Click" />
            <asp:LinkButton ID="btnDeleteCancel" runat="server" Text="Cancel" CssClass="btn btn-primary" OnClick="btnDeleteCancel_Click" />
        </asp:Panel>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="true">
            <asp:HiddenField ID="hfCareTypeId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <asp:ValidationSummary ID="valCareTypeDetail" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <div id="pnlViewDetails" runat="server">
                    <p class="description">
                        <asp:Literal ID="lCareTypeDescription" runat="server"></asp:Literal>
                    </p>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
                        <span class="pull-right">
                            <asp:LinkButton ID="btnCopy" runat="server" CssClass="btn btn-default btn-sm btn-square fa fa-clone" OnClick="btnCopy_Click" ToolTip="Copy Care Type" />
                            <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-security" />
                        </span>
                    </div>
                </div>

                <div id="pnlEditDetails" runat="server">

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="com.bemaservices.PastoralCare.Model.CareType, com.bemaservices.PastoralCare" PropertyName="Name" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbActive" runat="server" SourceTypeName="com.bemaservices.PastoralCare.Model.CareType, com.bemaservices.PastoralCare" PropertyName="IsActive" Label="Active" Checked="true" Text="Yes" />
                        </div>
                    </div>

                    <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="com.bemaservices.PastoralCare.Model.CareType, com.bemaservices.PastoralCare" PropertyName="Description" TextMode="MultiLine" Rows="4" />                   

                    <Rock:PanelWidget ID="wpAttributes" runat="server" Title="Care Item Attributes">
                        <div class="grid">
                            <Rock:Grid ID="gAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Care Item Attribute" ShowConfirmDeleteDialog="false" >
                                <Columns>
                                    <Rock:ReorderField />
                                    <Rock:RockBoundField DataField="Name" HeaderText="Attribute" />
                                    <Rock:RockBoundField DataField="FieldType" HeaderText="Field Type" />
                                    <Rock:BoolField DataField="AllowSearch" HeaderText="Allow Search" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                                    <Rock:EditField OnClick="gAttributes_Edit" />
                                    <Rock:DeleteField OnClick="gAttributes_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </Rock:PanelWidget>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>

       <Rock:ModalAlert ID="mdCopy" runat="server" />

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgAttribute" runat="server" Title="Care Opportunity Attributes" OnSaveClick="dlgCareTypeAttribute_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="Attributes">
            <Content>
                <Rock:AttributeEditor ID="edtAttributes" runat="server" ShowActions="false" ValidationGroup="Attributes" />
            </Content>
        </Rock:ModalDialog>       
    </ContentTemplate>
</asp:UpdatePanel>
