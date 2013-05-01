<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BinaryFileList.ascx.cs" Inherits="RockWeb.Blocks.Administration.BinaryFileList" %>

<asp:UpdatePanel ID="upBinaryFile" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        
        <Rock:GridFilter ID="fBinaryFile" runat="server">
            <Rock:LabeledTextBox ID="tbName" runat="server" LabelText="File Name" />
            <Rock:LabeledTextBox ID="tbType" runat="server" LabelText="Mime Type" />
            <Rock:LabeledCheckBox ID="dbIncludeTemporary" runat="server" Checked="false" LabelText="Include Temporary" Text="Yes"
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
