<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CheckInGroupTypeList.ascx.cs" Inherits="RockWeb.Blocks.Crm.CheckinGroupTypeList" %>

<link rel="stylesheet" href="<%# ResolveUrl("~/Themes/RockChMS/Css/rock-boot.less") %>"/>

<asp:UpdatePanel ID="upGroupType" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        <Rock:Grid ID="gGroupType" runat="server" AllowSorting="true" OnRowCommand="gGroupType_RowCommand">
            <Columns>
                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />

                <%-- asp:ButtonField automatically set CommandArgument to RowIndex --%>
                <asp:ButtonField ButtonType="Link" ControlStyle-CssClass="btn btn-mini" CommandName="schedule" Text="<i class='icon-calendar'></i> Edit Schedule" />

                <asp:ButtonField ButtonType="Link" ControlStyle-CssClass="btn btn-mini" CommandName="configure" Text="<i class='icon-cog'></i> Configure Groups/Locations" />

                <%-- This is just an another option of showing the action buttons (two buttons in one column) --%>
                <asp:TemplateField  Visible="true">
                    <ItemTemplate>

                        <asp:LinkButton runat="server" ID="btnSchedule" CssClass="btn btn-mini" CommandName="schedule" CommandArgument="<%# Container.DataItemIndex %>"><i class="icon-calendar"></i> Edit Schedule</asp:LinkButton>
                        <asp:LinkButton runat="server" ID="btnConfigure" CssClass="btn btn-mini" CommandName="configure" CommandArgument="<%# Container.DataItemIndex %>"><i class="icon-cog"></i> Configure Groups/Locations</asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>
