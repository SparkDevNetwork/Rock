<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BinaryFileTypeList.ascx.cs" Inherits="RockWeb.Blocks.Administration.BinaryFileTypeList" %>

<asp:UpdatePanel ID="upBinaryFileType" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        <Rock:Grid ID="gBinaryFileType" runat="server" AllowSorting="true" OnRowSelected="gBinaryFileType_Edit">
            <Columns>
                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                <asp:BoundField DataField="StorageEntityType" HeaderText="Storage Type" SortExpression="StorageEntityType" />
                <asp:BoundField DataField="BinaryFileCount" HeaderText="File Count" SortExpression="BinaryFileCount" />
                <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                <Rock:BoolField DataField="AllowCaching" HeaderText="Allows Caching" SortExpression="AllowCaching" />
                <Rock:DeleteField OnClick="gBinaryFileType_Delete" />
            </Columns>
        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>
