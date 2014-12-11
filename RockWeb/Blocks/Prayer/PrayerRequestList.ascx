<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PrayerRequestList.ascx.cs" Inherits="RockWeb.Blocks.Prayer.PrayerRequestList" %>
<asp:UpdatePanel ID="upPrayerRequests" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlLists" runat="server" Visible="true">
            

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-cloud-upload"></i> Prayer Requests</h1>
                </div>
                <div class="panel-body">
                    <div class="grid grid-panel">
                        <Rock:GridFilter ID="gfFilter" runat="server" OnApplyFilterClick="gfFilter_ApplyFilterClick" OnDisplayFilterValue="gfFilter_DisplayFilterValue">
                            <Rock:DateRangePicker ID="drpDateRange" runat="server" Label="Date Range" />

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

                            <Rock:CategoryPicker ID="catpPrayerCategoryFilter" runat="server" Label="Category" EntityTypeName="Rock.Model.PrayerRequest"/>

                            <Rock:RockCheckBox ID="cbShowExpired" runat="server" Label="Show Expired Requests?" />

                        </Rock:GridFilter>

                        <Rock:ModalAlert ID="maGridWarning" runat="server" />

                        <Rock:Grid ID="gPrayerRequests" runat="server" AllowSorting="true" RowItemText="request" OnRowSelected="gPrayerRequests_Edit" OnRowDataBound="gPrayerRequests_RowDataBound" >
                            <Columns>
                                <Rock:RockBoundField DataField="FullName" HeaderText="Name" SortExpression="FullName" />
                                <Rock:RockBoundField DataField="CategoryName" HeaderText="Category" SortExpression="CategoryName" />
                                <Rock:DateField DataField="EnteredDate" HeaderText="Entered" SortExpression="EnteredDate"/>
                                <Rock:RockBoundField DataField="Text" HeaderText="Request" SortExpression="Text" />
                                <Rock:BadgeField DataField="FlagCount" HeaderText="Flag Count" SortExpression="FlagCount" ImportantMin="4" WarningMin="2" InfoMin="1" InfoMax="2" />
                                <Rock:ToggleField DataField="IsApproved" HeaderText="Approved?" ButtonSizeCssClass="btn-xs" Enabled="True" OnCssClass="btn-success" OnText="Yes" OffText="No" SortExpression="IsApproved" OnCheckedChanged="gPrayerRequests_CheckChanged" />
                                <Rock:DeleteField OnClick="gPrayerRequests_Delete"  />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </div>

            

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
