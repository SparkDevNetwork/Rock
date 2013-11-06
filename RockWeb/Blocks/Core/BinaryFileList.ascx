<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BinaryFileList.ascx.cs" Inherits="RockWeb.Blocks.Administration.BinaryFileList" %>

<asp:UpdatePanel ID="upBinaryFile" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        
        <Rock:GridFilter ID="fBinaryFile" runat="server">
            <Rock:RockTextBox ID="tbName" runat="server" Label="File Name" />
            <Rock:RockTextBox ID="tbType" runat="server" Label="Mime Type" />
            <Rock:RockCheckBox ID="dbIncludeTemporary" runat="server" Checked="false" Label="Include Temporary" Text="Yes"
                Help="Temporary files are files that were uploaded to the server, but a reference to the file was never saved." />
        </Rock:GridFilter>
        
        <Rock:Grid ID="gBinaryFile" runat="server" AllowSorting="true" OnRowSelected="gBinaryFile_Edit">
            <Columns>
                <asp:BoundField DataField="FileName" HeaderText="File Name" SortExpression="FileName" />
                <asp:BoundField DataField="MimeType" HeaderText="Mime Type" SortExpression="MimeType" />
                <Rock:DateTimeField DataField="LastModifiedDateTime" HeaderText="Last Modified" SortExpression="LastModifiedDateTime" />
                <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                <Rock:DeleteField OnClick="gBinaryFile_Delete" />
            </Columns>
        </Rock:Grid>

    </ContentTemplate>
</asp:UpdatePanel>
