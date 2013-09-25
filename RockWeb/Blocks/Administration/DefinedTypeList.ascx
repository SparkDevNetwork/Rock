<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DefinedTypeList.ascx.cs" Inherits="RockWeb.Blocks.Administration.DefinedTypeList" %>

<asp:UpdatePanel ID="upSettings" runat="server">
    <ContentTemplate>
        <Rock:GridFilter ID="tFilter" runat="server">
            <Rock:RockDropDownList ID="ddlCategoryFilter" runat="server" Label="Category" AutoPostBack="false" />
        </Rock:GridFilter>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        <Rock:Grid ID="gDefinedType" runat="server" AllowSorting="true" OnRowSelected="gDefinedType_Edit">
            <Columns>
                <asp:BoundField DataField="Category" HeaderText="Category" SortExpression="Category" />
                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                <asp:BoundField DataField="FieldTypeName" HeaderText="Field Type" SortExpression="FieldTypeName" />
                <Rock:DeleteField OnClick="gDefinedType_Delete" />
            </Columns>
        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>
