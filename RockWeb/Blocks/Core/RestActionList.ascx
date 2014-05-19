<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RestActionList.ascx.cs" Inherits="RockWeb.Blocks.Administration.RestActionList" %>

<asp:UpdatePanel ID="upnlList" runat="server">
    <ContentTemplate>
        <h4><asp:Literal ID="lControllerName" runat="server"/></asp:Literal></h4>
        <Rock:Grid ID="gActions" runat="server" AllowSorting="true">
            <Columns>
                <asp:BoundField DataField="Method" HeaderText="Method" SortExpression="Method" />
                <asp:BoundField DataField="Path" HeaderText="Relative Path" SortExpression="Path" />
                <Rock:SecurityField TitleField="Method" />
            </Columns>
        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>
