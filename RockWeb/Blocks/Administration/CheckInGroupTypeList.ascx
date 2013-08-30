<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CheckInGroupTypeList.ascx.cs" Inherits="RockWeb.Blocks.Crm.CheckinGroupTypeList" %>

<asp:UpdatePanel ID="upGroupType" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        <Rock:Grid ID="gGroupType" runat="server" AllowSorting="true" OnRowCommand="gGroupType_RowCommand">
            
            <Columns>
                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                <asp:TemplateField ItemStyle-CssClass="grid-col-actions" HeaderStyle-CssClass="grid-col-actions" HeaderText="Actions">
                    <ItemTemplate>
                        <asp:LinkButton runat="server" ID="btnSchedule" CssClass="btn" CommandName="schedule" CommandArgument="<%# Container.DataItemIndex %>"><i class="icon-calendar"></i> Edit Schedule</asp:LinkButton>
                        <asp:LinkButton runat="server" ID="btnConfigure" CssClass="btn" CommandName="configure" CommandArgument="<%# Container.DataItemIndex %>"><i class="icon-cog"></i> Configure Groups/Locations</asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>

        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>
