<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BinaryFileTypeDetail.ascx.cs" Inherits="RockWeb.Blocks.Core.BinaryFileTypeDetail" %>

<asp:UpdatePanel ID="upBinaryFileType" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server" Visible="false">
            <asp:HiddenField ID="hfBinaryFileTypeId" runat="server" />

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-danger" />

            <div class="banner">
                <h1><asp:Literal ID="lActionTitle" runat="server" /></h1>
            </div>

            <fieldset>

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.BinaryFileType, Rock" PropertyName="Name" />
                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.BinaryFileType, Rock" PropertyName="Description" />
                        <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.BinaryFileType, Rock" PropertyName="IconCssClass" />
                        <Rock:ImageUploader ID="imgIconSmall" runat="server" Label="Small Icon Image" />
                        <Rock:ImageUploader ID="imgIconLarge" runat="server" Label="Large Icon Image" />
                        <Rock:RockCheckBox ID="cbAllowCaching" runat="server" Label="Allows Caching" Help="If 'true' the file will be cached from the storage provider to the server's file system.  This is not recommended for files that need heightened security. "/>
                        <Rock:ComponentPicker ID="cpStorageType" runat="server" ContainerType="Rock.Storage.ProviderContainer, Rock" Label="Storage Type" Required="true" />
                    </div>
                    <div class="col-md-6">

                        <h5>Attributes</h5>
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
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>

        <asp:Panel ID="pnlBinaryFileAttribute" runat="server" Visible="false">
            <Rock:AttributeEditor ID="edtBinaryFileAttributes" runat="server" OnSaveClick="btnSaveBinaryFileAttribute_Click" OnCancelClick="btnCancelBinaryFileAttribute_Click" ValidationGroup="Attribute" />
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
