<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PrayerRequestList.ascx.cs" Inherits="RockWeb.Blocks.Prayer.PrayerRequestList" %>
<asp:UpdatePanel ID="upPrayerRequests" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlLists" runat="server" Visible="true">
            <Rock:GridFilter ID="rFilter" runat="server" OnApplyFilterClick="rFilter_ApplyFilterClick" OnDisplayFilterValue="rFilter_DisplayFilterValue">
                <Rock:DatePicker ID="dtRequestEnteredDateRangeStartDate" runat="server" SourceTypeName="Rock.Model.PrayerRequest, Rock" PropertyName="EnteredDate" Label="From date" />
                <Rock:DatePicker ID="dtRequestEnteredDateRangeEndDate" runat="server" SourceTypeName="Rock.Model.PrayerRequest, Rock" PropertyName="EnteredDate" Label="To date" />

                <%--<Rock:RockDropDownList ID="ddlGroupCategoryFilter" runat="server" Label="Prayer Group Category" OnTextChanged="ddlGroupCategoryFilter_TextChanged" />
                <Rock:RockDropDownList ID="ddlPrayerCategoryFilter" runat="server" Label="Prayer Category" />--%>
                <Rock:CategoryPicker ID="cpPrayerCategoryFilter" runat="server" Label="Category" Required="true" EntityTypeName="Rock.Model.PrayerRequest"/>

                <Rock:RockRadioButtonList ID="rblApprovedFilter" runat="server" Label="Approval Status">
                    <asp:ListItem Text="all" Value="all"></asp:ListItem>
                    <asp:ListItem Text="only approved" Value="approved"></asp:ListItem>
                    <asp:ListItem Text="only unapproved" Value="unapproved"></asp:ListItem>
                </Rock:RockRadioButtonList>

                <Rock:RockRadioButtonList ID="rblUrgentFilter" runat="server" Label="Urgent Status">
                    <asp:ListItem Text="all" Value="all"></asp:ListItem>
                    <asp:ListItem Text="only urgent" Value="urgent"></asp:ListItem>
                    <asp:ListItem Text="only non-urgent" Value="non-urgent"></asp:ListItem>
                </Rock:RockRadioButtonList>

                <Rock:RockRadioButtonList ID="rblPublicFilter" runat="server" Label="Private/Public">
                    <asp:ListItem Text="all" Value="all"></asp:ListItem>
                    <asp:ListItem Text="only public" Value="public"></asp:ListItem>
                    <asp:ListItem Text="only non-public" Value="non-public"></asp:ListItem>
                </Rock:RockRadioButtonList>

                <Rock:RockRadioButtonList ID="rblActiveFilter" runat="server" Label="Active Status">
                    <asp:ListItem Text="all" Value="all"></asp:ListItem>
                    <asp:ListItem Text="only active" Value="active"></asp:ListItem>
                    <asp:ListItem Text="only inactive" Value="inactive"></asp:ListItem>
                </Rock:RockRadioButtonList>

                <Rock:RockRadioButtonList ID="rblAllowCommentsFilter" runat="server" Label="Commenting Status">
                    <asp:ListItem Text="all" Value="all"></asp:ListItem>
                    <asp:ListItem Text="only allow" Value="allow"></asp:ListItem>
                    <asp:ListItem Text="only unallow" Value="unallow"></asp:ListItem>
                </Rock:RockRadioButtonList>

            </Rock:GridFilter>

            <Rock:ModalAlert ID="mdGridWarning" runat="server" />

            <Rock:Grid ID="gPrayerRequests" runat="server" AllowSorting="true" RowItemText="request" OnRowSelected="gPrayerRequests_Edit" OnRowDataBound="gPrayerRequests_RowDataBound" >
                <Columns>
                    <asp:BoundField DataField="FullName" HeaderText="Name" SortExpression="FullName" />
                    <asp:BoundField DataField="CategoryName" HeaderText="Category" SortExpression="CategoryName" />
                    <Rock:DateField DataField="EnteredDate" HeaderText="Entered" SortExpression="EnteredDate"/>
                    <asp:BoundField DataField="Text" HeaderText="Request" SortExpression="Text" />
                    <Rock:BadgeField DataField="FlagCount" HeaderText="Flag Count" SortExpression="FlagCount" ImportantMin="4" WarningMin="2" InfoMin="1" InfoMax="2" />
                    <Rock:ToggleField DataField="IsApproved" HeaderText="Approval Status" CssClass="switch-mini" Enabled="True" OnText="approved" OffText="unapproved" SortExpression="IsApproved" OnCheckedChanged="gPrayerRequests_CheckChanged" />
                    <Rock:DeleteField OnClick="gPrayerRequests_Delete"  />
                </Columns>
            </Rock:Grid>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
