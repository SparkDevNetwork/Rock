<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BulkImportTool.ascx.cs" Inherits="RockWeb.Blocks.BulkImport.BulkImportTool" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-star"></i>&nbsp;Bulk Import Tool</h1>
            </div>
            <div class="panel-body">
                <Rock:FileUploader ID="fupSlingshotFile" runat="server" Label="Select Slingshot File" IsBinaryFile="false" RootFolder="~/App_Data/SlingshotFiles" DisplayMode="DropZone" OnFileUploaded="fupSlingshotFile_FileUploaded" OnFileRemoved="fupSlingshotFile_FileRemoved" />
                <asp:Literal ID="lSlingshotFileInfo" runat="server" Text="" />
                <div class="actions">
                    <asp:LinkButton ID="btnImport" runat="server" CssClass="btn btn-primary" Text="Import" OnClick="btnImport_Click" Visible="false" />
                </div>
            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>