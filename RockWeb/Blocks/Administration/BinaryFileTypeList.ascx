<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BinaryFileTypeList.ascx.cs" Inherits="RockWeb.Blocks.Administration.BinaryFileTypeList" %>

<asp:UpdatePanel ID="upBinaryFileType" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        <Rock:Grid ID="gBinaryFileType" runat="server" AllowSorting="true" OnRowSelected="gBinaryFileType_Edit">
            <Columns>
                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                <asp:BoundField DataField="FileCount" HeaderText="File Count" SortExpression="BinaryFiles.Count" />
                <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                <Rock:DeleteField OnClick="gBinaryFileType_Delete" />
            </Columns>
        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>
