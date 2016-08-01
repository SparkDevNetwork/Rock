<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SignatureDocumentTypeList.ascx.cs" Inherits="RockWeb.Blocks.Core.SignatureDocumentTypeList" %>

<asp:UpdatePanel ID="upSettings" runat="server">
    <ContentTemplate>
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-file-text-o"></i> Document Types</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gSignatureDocumentType" runat="server" AllowSorting="true" OnRowSelected="gSignatureDocumentType_Edit" TooltipField="Description" RowItemText="Document Type">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                            <Rock:RockBoundField DataField="BinaryFileType" HeaderText="File Type" SortExpression="BinaryFileType.Name" />
                            <Rock:RockBoundField DataField="Documents" HeaderText="Requested Documents" SortExpression="Documents" DataFormatString="{0:N0}" />
                            <Rock:SecurityField TitleField="Name" />
                            <Rock:DeleteField OnClick="gSignatureDocumentType_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
