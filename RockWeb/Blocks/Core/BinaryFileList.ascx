<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BinaryFileList.ascx.cs" Inherits="RockWeb.Blocks.Core.BinaryFileList" %>

<asp:UpdatePanel ID="upBinaryFile" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-files-o"></i> File List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="fBinaryFile" runat="server">
                        <Rock:RockTextBox ID="tbName" runat="server" Label="File Name" />
                        <Rock:RockTextBox ID="tbType" runat="server" Label="Mime Type" />
                        <Rock:RockCheckBox ID="dbIncludeTemporary" runat="server" Checked="false" Label="Include Temporary" Text="Yes"
                            Help="Temporary files are files that were uploaded to the server, but a reference to the file was never saved." />
                    </Rock:GridFilter>
        
                    <Rock:Grid ID="gBinaryFile" runat="server" AllowSorting="true" OnRowSelected="gBinaryFile_Edit">
                        <Columns>
                            <Rock:RockBoundField DataField="FileName" HeaderText="File Name" SortExpression="FileName" />
                            <Rock:RockBoundField DataField="MimeType" HeaderText="Mime Type" SortExpression="MimeType" />
                            <Rock:DateTimeField DataField="ModifiedDateTime" HeaderText="Last Modified" SortExpression="ModifiedDateTime" />
                            <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                            <Rock:DeleteField OnClick="gBinaryFile_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        
        

    </ContentTemplate>
</asp:UpdatePanel>
