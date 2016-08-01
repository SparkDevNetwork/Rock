<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SignatureDocumentTemplateList.ascx.cs" Inherits="RockWeb.Blocks.Core.SignatureDocumentTemplateList" %>

<asp:UpdatePanel ID="upSettings" runat="server">
    <ContentTemplate>
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa fa-pencil-square-o"></i> Document Templates</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gSignatureDocumentTemplates" runat="server" AllowSorting="true" OnRowSelected="gSignatureDocumentTemplate_Edit" TooltipField="Description" RowItemText="Document Template">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                            <Rock:RockBoundField DataField="BinaryFileType" HeaderText="File Type" SortExpression="BinaryFileType.Name" />
                            <Rock:RockBoundField DataField="Documents" HeaderText="Requested Documents" SortExpression="Documents" DataFormatString="{0:N0}" />
                            <Rock:SecurityField TitleField="Name" />
                            <Rock:DeleteField OnClick="gSignatureDocumentTemplate_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
