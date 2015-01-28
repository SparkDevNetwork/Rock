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
                <div class="panel-body">
                    <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <fieldset>

                        <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.BinaryFileType, Rock" PropertyName="Name" />
                                <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.BinaryFileType, Rock" PropertyName="Description" />
                                <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.BinaryFileType, Rock" PropertyName="IconCssClass" />
                                <Rock:RockCheckBox ID="cbAllowCaching" runat="server" Label="Allows Caching" Text="Yes" Help="Should the file be cached from the storage provider to the server's file system.  This is not recommended for files that need heightened security. " />
                                <Rock:RockCheckBox ID="cbRequiresViewSecurity" runat="server" Label="Requires View Security" Text="Yes" Help="Enable this to always do a security check before displaying images of this type. Leave disabled for files that can be viewed by any user." />
                                <Rock:ComponentPicker ID="cpStorageType" runat="server" ContainerType="Rock.Storage.ProviderContainer, Rock" Label="Storage Type" Required="true" AutoPostBack="true" />
                                <div class="attributes">
                                    <asp:PlaceHolder ID="phAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>
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
                            </div>
                        </div>

                        <div class="row">
                            <h3>Preferred File Settings</h3>
                            <div class="col-md-6">
                                <Rock:NumberBox ID="nbMaxWidth" runat="server" Label="Maximum Width" />
                                <Rock:NumberBox ID="nbMaxHeight" runat="server" Label="Maximum Height" />
                                <Rock:RockDropDownList ID="ddlPreferredColorDepth" runat="server" Label="Preferred Color Depth" Required="true" />
                                <Rock:RockCheckBox ID="cbPreferredRequired" runat="server" Label="Preferred Settings Required" Help="Should the preferred settings for this file type be the required settings?" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockDropDownList ID="ddlPreferredFormat" runat="server" Label="Preferred Format" Required="true" />
                                <Rock:RockDropDownList ID="ddlPreferredResolution" runat="server" Label="Preferred Resolution" Required="true" />
                            </div>

                        </div>

                    </fieldset>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>
            </div>




        </asp:Panel>

        <asp:Panel ID="pnlBinaryFileAttribute" runat="server" Visible="false">
            <Rock:AttributeEditor ID="edtBinaryFileAttributes" runat="server" OnSaveClick="btnSaveBinaryFileAttribute_Click" OnCancelClick="btnCancelBinaryFileAttribute_Click" ValidationGroup="Attribute" />
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
