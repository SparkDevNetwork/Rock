<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BinaryFileDetail.ascx.cs" Inherits="RockWeb.Blocks.Administration.BinaryFileDetail" %>

<asp:UpdatePanel ID="upBinaryFile" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server" Visible="false" CssClass="panel panel-default">
            <asp:HiddenField ID="hfBinaryFileId" runat="server" />

            <div class="panel-body">
                
                <div class="banner"><h1><asp:Literal ID="lActionTitle" runat="server" /></h1></div>
                
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-danger" />
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                
                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.BinaryFile, Rock" PropertyName="FileName" Required="true" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-12">
                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.BinaryFile, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:FileUploader ID="fsFile" runat="server" Label="Upload New File" />
                        <Rock:DataTextBox ID="tbMimeType" runat="server" SourceTypeName="Rock.Model.BinaryFile, Rock" PropertyName="MimeType" />
                        <Rock:BinaryFileTypePicker ID="ddlBinaryFileType" runat="server" Visible="false" />
                        </div>
                    <div class="col-md-6">
                        <div class="attributes">
                            <asp:PlaceHolder ID="phAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>
                        </div>
                    </div>
                </div>


                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
