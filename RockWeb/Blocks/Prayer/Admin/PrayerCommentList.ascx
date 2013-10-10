<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PrayerCommentList.ascx.cs" Inherits="RockWeb.Blocks.Prayer.PrayerCommentsList" %>
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

            </Rock:GridFilter>
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
            <Rock:Grid ID="gPrayerComments" runat="server" AllowSorting="true" RowItemText="comment" OnRowSelected="gPrayerComments_Edit" ExcelExportEnabled="false">
                <Columns>
                    <Rock:DateTimeField DataField="CreationDateTime" HeaderText="Time" SortExpression="CreationDateTime"/>
                    <asp:BoundField DataField="Caption" HeaderText="From" SortExpression="Text" />
                    <asp:BoundField DataField="Text" HeaderText="Comment" SortExpression="Text" />
                    <%--<Rock:ToggleField DataField="IsApproved" HeaderText="Approval Status" CssClass="switch-mini" Enabled="True" OnText="yes" OffText="no" SortExpression="IsApproved" OnCheckedChanged="gPrayerComments_CheckChanged" />--%>
                    <Rock:DeleteField OnClick="gPrayerComments_Delete" />
                </Columns>
            </Rock:Grid>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
