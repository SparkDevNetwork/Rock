<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ExternalFileList.ascx.cs" Inherits="RockWeb.Blocks.Administration.ExternalFileList" %>

<asp:UpdatePanel ID="upBinaryFile" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        
        <Rock:Grid ID="gBinaryFile" runat="server" AllowSorting="true" DisplayType="Light" >
            <Columns>
                <Rock:AttributeField DataField="Icon" HeaderText="Icon" />
                <Rock:AttributeField DataField="Name" HeaderText="Name" />
                <asp:BoundField DataField="Description" HeaderText="Description" />
                <asp:BoundField DataField="FileName" HeaderText="File Name" SortExpression="FileName" />
                <Rock:LinkButtonField CssClass="" Text="Download" OnClick="Download_Click" />
            </Columns>
        </Rock:Grid>

    </ContentTemplate>
</asp:UpdatePanel>
