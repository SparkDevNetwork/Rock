<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupTypeList.ascx.cs" Inherits="RockWeb.Blocks.Crm.GroupTypeList" %>

<asp:UpdatePanel ID="upGroupType" runat="server">
    <ContentTemplate>
        <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue">
            <Rock:RockDropDownList ID="ddlPurpose" runat="server" Label="Purpose"></Rock:RockDropDownList>
            <Rock:RockDropDownList ID="ddlIsSystem" runat="server" Label="System Group Type">
                <asp:ListItem Value="" Text=" " />
                <asp:ListItem Value="Yes" Text="Yes" />
                <asp:ListItem Value="No" Text="No" />
            </Rock:RockDropDownList>
        </Rock:GridFilter>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        <Rock:Grid ID="gGroupType" runat="server" RowItemText="Group Type" OnRowSelected="gGroupType_Edit" DescriptionField="Description">
            <Columns>
                <Rock:ReorderField />
                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                <asp:BoundField DataField="Purpose" HeaderText="Purpose" SortExpression="Purpose" />
                <asp:BoundField DataField="GroupsCount" HeaderText="Group Count" SortExpression="GroupsCount" />
                <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                <Rock:SecurityField TitleField="Name" />
                <Rock:DeleteField OnClick="gGroupType_Delete" />
            </Columns>
        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>
