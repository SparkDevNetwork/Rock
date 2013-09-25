<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BinaryFileDetail.ascx.cs" Inherits="RockWeb.Blocks.Administration.BinaryFileDetail" %>

<asp:UpdatePanel ID="upBinaryFile" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server" Visible="false">
            <asp:HiddenField ID="hfBinaryFileId" runat="server" />

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-error" />

            <fieldset>
                <legend>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </legend>

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div class="row-fluid">
                    <div class="span6">
                        <Rock:FileUploader ID="fsFile" runat="server" Label="Upload New File" />
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.BinaryFile, Rock" PropertyName="FileName" Required="true" />
                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.BinaryFile, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                        <Rock:DataTextBox ID="tbMimeType" runat="server" SourceTypeName="Rock.Model.BinaryFile, Rock" PropertyName="MimeType" />
                        <Rock:BinaryFileTypePicker ID="ddlBinaryFileType" runat="server" Visible="false" />
                     </div>
                    <div class="span6">
                        <div class="attributes">
                            <asp:PlaceHolder ID="phAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>
                        </div>
                    </div>
                </div>
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
