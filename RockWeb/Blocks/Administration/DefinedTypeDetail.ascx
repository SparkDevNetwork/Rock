<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DefinedTypeDetail.ascx.cs" Inherits="RockWeb.Blocks.Administration.DefinedTypeDetail" %>

<asp:UpdatePanel ID="upSettings" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfDefinedTypeId" runat="server" />

        <asp:Panel ID="pnlDetails" runat="server" Visible="false">

            <div id="pnlEditDetails" runat="server" class="well">

                <asp:ValidationSummary ID="vsDetails" runat="server" CssClass="alert alert-error" />

                <fieldset>
                    <legend>
                        <asp:Literal ID="lActionTitle" runat="server" />
                    </legend>

                    <div class="row-fluid">
                        <div class="span6">
                            <Rock:DataTextBox ID="tbTypeName" runat="server" SourceTypeName="Rock.Model.DefinedType, Rock" PropertyName="Name" />
                            <Rock:DataTextBox ID="tbTypeDescription" runat="server" SourceTypeName="Rock.Model.DefinedType, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                        </div>

                        <div class="span6">
                            <Rock:DataTextBox ID="tbTypeCategory" runat="server" SourceTypeName="Rock.Model.DefinedType, Rock" PropertyName="Category" />
                            <Rock:FieldTypeList ID="ddlTypeFieldType" runat="server" SourceTypeName="Rock.Model.DefinedType, Rock" PropertyName="FieldType" />
                        </div>
                    </div>

                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnSaveType" runat="server" Text="Save" CssClass="btn primary" OnClick="btnSaveType_Click" />
                    <asp:LinkButton ID="btnCancelType" runat="server" Text="Cancel" CssClass="btn secondary" CausesValidation="false" OnClick="btnCancelType_Click" />
                </div>
            </div>

            <fieldset id="fieldsetViewDetails" runat="server">
                <div class="well">
                    <div class="row-fluid">
                        <div class="span6">
                            <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                            <asp:Literal ID="lblMainDetails" runat="server" />
                        </div>
                        <div class="span6">
                            <asp:Panel ID="pnlAttributeTypes" runat="server">
                                <Rock:ModalAlert ID="mdGridWarningAttributes" runat="server" />
                                <Rock:Grid ID="gDefinedTypeAttributes" runat="server" AllowPaging="false" DisplayType="Light">
                                    <Columns>
                                        <asp:BoundField DataField="Name" HeaderText="Attributes for Defined Type" />
                                        <Rock:EditField OnClick="gDefinedTypeAttributes_Edit" />
                                        <Rock:DeleteField OnClick="gDefinedTypeAttributes_Delete" />
                                    </Columns>
                                </Rock:Grid>
                            </asp:Panel>
                        </div>
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary btn-mini" CausesValidation="false" OnClick="btnEdit_Click" />
                    </div>
                </div>
            </fieldset>
                        
        </asp:Panel>

        <asp:Panel ID="pnlDefinedTypeAttributes" runat="server" Visible="false">
            <Rock:AttributeEditor ID="edtDefinedTypeAttributes" runat="server" OnSaveClick="btnSaveDefinedTypeAttribute_Click" OnCancelClick="btnCancelDefinedTypeAttribute_Click" />
        </asp:Panel>
        
    </ContentTemplate>
</asp:UpdatePanel>
