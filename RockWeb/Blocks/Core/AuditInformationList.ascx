<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AuditInformationList.ascx.cs" Inherits="RockWeb.Blocks.Core.AuditInformationList" %>

<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server" Visible="true">
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
            <Rock:GridFilter ID="gAuditInformationListFilter" runat="server">
                <Rock:RockDropDownList ID="ddlEntityTypeFilter" runat="server" Label="Entity Type" />
                <Rock:RockTextBox ID="txtEntityIdFilter" runat="server" Label="Entity Id" />
            </Rock:GridFilter>
            <Rock:Grid ID="gAuditInformationList" runat="server" AllowSorting="true" OnRowSelected="gAuditInformationList_Edit">
                <Columns>
                    <asp:BoundField DataField="EntityType" HeaderText="Entity Type" SortExpression="EntityType" />
                    <asp:BoundField DataField="EntityId" HeaderText="Entity Id" SortExpression="EntityId" />
                    <Rock:DateTimeField DataField="DateTime" HeaderText="Date" SortExpression="DateTime" />
                    <asp:BoundField DataField="PersonName" HeaderText="Name" SortExpression="PersonName" />
                </Columns>
            </Rock:Grid>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
