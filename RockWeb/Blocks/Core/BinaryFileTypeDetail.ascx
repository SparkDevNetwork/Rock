<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BinaryFileTypeDetail.ascx.cs" Inherits="RockWeb.Blocks.Core.BinaryFileTypeDetail" %>

<asp:UpdatePanel ID="upBinaryFileType" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server" Visible="false">
            <asp:HiddenField ID="hfBinaryFileTypeId" runat="server" />

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-file-o"></i>
                        <asp:Literal ID="lActionTitle" runat="server" /></h1>
                </div>
                <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
                <div class="panel-body">
                    <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                    <fieldset>

                        <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.BinaryFileType, Rock" PropertyName="Name" />
                                <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.BinaryFileType, Rock" PropertyName="Description" />
                                <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.BinaryFileType, Rock" PropertyName="IconCssClass" Label="Icon CSS Class" />

                                <Rock:RockCheckBox ID="cbRequiresViewSecurity" runat="server" Label="Requires View Security" Help="Enable this to always do a security check before displaying images of this type. Leave disabled for files that can be viewed by any user." />
                                <Rock:ComponentPicker ID="cpStorageType" runat="server" ContainerType="Rock.Storage.ProviderContainer, Rock" Label="Storage Type" Required="true" AutoPostBack="true" />
                                <div class="attributes">
                                    <Rock:DynamicPlaceholder ID="phAttributes" runat="server" />
                                </div>
                            </div>
                            <div class="col-md-6">
                                <h5>Attributes</h5>
                                <p>
                                    Attributes allow for providing different values for each binary file of this type.
                                </p>
                                <div class="grid">
                                    <Rock:Grid ID="gBinaryFileAttributes" runat="server" AllowPaging="false" DisplayType="Light" ShowHeader="false">
                                        <Columns>
                                            <asp:BoundField DataField="Name" />
                                            <Rock:EditField OnClick="gBinaryFileAttributes_Edit" />
                                            <Rock:DeleteField OnClick="gBinaryFileAttributes_Delete" />
                                        </Columns>
                                    </Rock:Grid>
                                </div>

                                <h5>Cache Settings</h5>
                                <Rock:RockCheckBox ID="cbCacheToServerFileSystem" runat="server" Label="Cache To Server File System" Help="Should the file be cached from the storage provider to the server's file system.  This is not recommended for files that need heightened security. " />

                                <h5>Cache Control Header Settings</h5>
                                <Rock:CacheabilityPicker ID="cpCacheSettings" runat="server" Label="" />
                            </div>
                        </div>

                        <h3>Preferred File Settings</h3>
                        <div class="row">

                            <div class="col-md-6">
                                <Rock:NumberBox ID="nbMaxFileSizeBytes" runat="server" Label="Max File Size" Help="The max size allowed for the files in bytes. Leaving this blank will allow any size, 0 is not allowed." NumberType="Integer" AppendText="Bytes" Required="false" MinimumValue="1" />
                                <Rock:NumberBox ID="nbMaxWidth" runat="server" Label="Maximum Width" Help="Sets the maximum width for images in pixels. Leave this field blank for no limit." />
                                <Rock:NumberBox ID="nbMaxHeight" runat="server" Label="Maximum Height" Help="Sets the maximum height in pixels. Leave this field blank for no limit." />
                                <Rock:RockCheckBox ID="cbPreferredRequired" runat="server" Label="Preferred Settings Required" Help="Should the preferred settings for this file type be the required settings?" />
                            </div>
                        </div>

                    </fieldset>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>
            </div>




        </asp:Panel>

        <asp:Panel ID="pnlBinaryFileAttribute" runat="server" Visible="false">
            <Rock:AttributeEditor ID="edtBinaryFileAttributes" runat="server" OnSaveClick="btnSaveBinaryFileAttribute_Click" OnCancelClick="btnCancelBinaryFileAttribute_Click" ValidationGroup="Attribute" />
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
