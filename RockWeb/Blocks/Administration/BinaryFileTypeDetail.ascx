<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BinaryFileTypeDetail.ascx.cs" Inherits="RockWeb.Blocks.Administration.BinaryFileTypeDetail" %>

<asp:UpdatePanel ID="upBinaryFileType" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server" Visible="false">
            <asp:HiddenField ID="hfBinaryFileTypeId" runat="server" />

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-error" />

            <fieldset>
                <legend>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </legend>

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div class="row-fluid">
                    <div class="span6">
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.BinaryFileType, Rock" PropertyName="Name" />
                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.BinaryFileType, Rock" PropertyName="Description" />
                        <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.BinaryFileType, Rock" PropertyName="IconCssClass" />
                        <Rock:ImageUploader ID="imgIconSmall" runat="server" Label="Small Icon Image" />
                        <Rock:ImageUploader ID="imgIconLarge" runat="server" Label="Large Icon Image" />
                        <Rock:RockCheckBox ID="cbAllowCaching" runat="server" Label="Allows Caching"/>
                        <Rock:ComponentPicker ID="cpStorageType" runat="server" ContainerType="Rock.Storage.ProviderContainer, Rock" Label="Storage Type" />
                    </div>
                    <div class="span6">

                        <h5>Attributes
                        </h5>
                        <p>
                            Attributes allow for providing different values for each binary file of this type.
                        </p>
                        <Rock:Grid ID="gBinaryFileAttributes" runat="server" AllowPaging="false" DisplayType="Light" ShowHeader="false">
                            <Columns>
                                <asp:BoundField DataField="Name" />
                                <Rock:EditField OnClick="gBinaryFileAttributes_Edit" />
                                <Rock:DeleteField OnClick="gBinaryFileAttributes_Delete" />
                            </Columns>
                        </Rock:Grid>

                    </div>
                </div>
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>

        <asp:Panel ID="pnlBinaryFileAttribute" runat="server" Visible="false">
            <Rock:AttributeEditor ID="edtBinaryFileAttributes" runat="server" OnSaveClick="btnSaveBinaryFileAttribute_Click" OnCancelClick="btnCancelBinaryFileAttribute_Click" />
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
