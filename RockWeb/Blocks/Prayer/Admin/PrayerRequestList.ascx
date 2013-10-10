<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PrayerRequestList.ascx.cs" Inherits="RockWeb.Blocks.Prayer.PrayerRequestList" %>
<asp:UpdatePanel ID="upPrayerRequests" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlLists" runat="server" Visible="true">
            <Rock:GridFilter ID="rFilter" runat="server" OnApplyFilterClick="rFilter_ApplyFilterClick" OnDisplayFilterValue="rFilter_DisplayFilterValue">
                <Rock:DateRangePicker ID="pDateRange" runat="server" Label="Date Range" />

                <Rock:RockDropDownList ID="ddlApprovedFilter" runat="server" Label="Approval Status">
                    <asp:ListItem Text="[All]" Value="all"></asp:ListItem>
                    <asp:ListItem Text="Approved" Value="approved"></asp:ListItem>
                    <asp:ListItem Text="Unapproved" Value="unapproved"></asp:ListItem>
                </Rock:RockDropDownList>

                <Rock:RockDropDownList ID="ddlUrgentFilter" runat="server" Label="Urgent Status">
                    <asp:ListItem Text="[All]" Value="all"></asp:ListItem>
                    <asp:ListItem Text="Urgent" Value="urgent"></asp:ListItem>
                    <asp:ListItem Text="Non-Urgent" Value="non-urgent"></asp:ListItem>
                </Rock:RockDropDownList>

                <Rock:RockDropDownList ID="ddlPublicFilter" runat="server" Label="Private/Public">
                    <asp:ListItem Text="[All]" Value="all"></asp:ListItem>
                    <asp:ListItem Text="Public" Value="public"></asp:ListItem>
                    <asp:ListItem Text="Non-Public" Value="non-public"></asp:ListItem>
                </Rock:RockDropDownList>

                <Rock:RockDropDownList ID="ddlActiveFilter" runat="server" Label="Active Status">
                    <asp:ListItem Text="[All]" Value="all"></asp:ListItem>
                    <asp:ListItem Text="Active" Value="active"></asp:ListItem>
                    <asp:ListItem Text="Inactive" Value="inactive"></asp:ListItem>
                </Rock:RockDropDownList>

                <Rock:RockDropDownList ID="ddlAllowCommentsFilter" runat="server" Label="Commenting">
                    <asp:ListItem Text="[All]" Value="all"></asp:ListItem>
                    <asp:ListItem Text="Allowed" Value="allow"></asp:ListItem>
                    <asp:ListItem Text="Not Allowed" Value="unallow"></asp:ListItem>
                </Rock:RockDropDownList>

                <Rock:CategoryPicker ID="cpPrayerCategoryFilter" runat="server" Label="Category" Required="true" EntityTypeName="Rock.Model.PrayerRequest"/>
            </Rock:GridFilter>

            <Rock:ModalAlert ID="mdGridWarning" runat="server" />

            <Rock:Grid ID="gPrayerRequests" runat="server" AllowSorting="true" RowItemText="request" OnRowSelected="gPrayerRequests_Edit" OnRowDataBound="gPrayerRequests_RowDataBound" >
                <Columns>
                    <asp:BoundField DataField="FullName" HeaderText="Name" SortExpression="FullName" />
                    <asp:BoundField DataField="CategoryName" HeaderText="Category" SortExpression="CategoryName" />
                    <Rock:DateField DataField="EnteredDate" HeaderText="Entered" SortExpression="EnteredDate"/>
                    <asp:BoundField DataField="Text" HeaderText="Request" SortExpression="Text" />
                    <Rock:BadgeField DataField="FlagCount" HeaderText="Flag Count" SortExpression="FlagCount" ImportantMin="4" WarningMin="2" InfoMin="1" InfoMax="2" />
                    <Rock:ToggleField DataField="IsApproved" HeaderText="Approval Status" CssClass="switch-mini" Enabled="True" OnText="Approved" OffText="Unapproved" SortExpression="IsApproved" OnCheckedChanged="gPrayerRequests_CheckChanged" />
                    <Rock:DeleteField OnClick="gPrayerRequests_Delete"  />
                </Columns>
            </Rock:Grid>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
