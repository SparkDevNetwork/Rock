<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PrayerCommentList.ascx.cs" Inherits="RockWeb.Blocks.Prayer.PrayerCommentsList" %>
<asp:UpdatePanel ID="upPrayerRequests" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlLists" runat="server" Visible="true">
            <Rock:GridFilter ID="rFilter" runat="server" OnApplyFilterClick="rFilter_ApplyFilterClick" OnDisplayFilterValue="rFilter_DisplayFilterValue">
                <Rock:DateTimePicker ID="dtDateRangeStartDate" runat="server" SourceTypeName="Rock.Model.Note, Rock" PropertyName="Date" LabelText="From date" />
                <Rock:DateTimePicker ID="dtDateRangeEndDate" runat="server" SourceTypeName="Rock.Model.Note, Rock" PropertyName="Date" LabelText="To date" />
                <Rock:LabeledRadioButtonList ID="rblApprovedFilter" runat="server" LabelText="Approval Status">
                    <asp:ListItem Text="all" Value="all" Selected="True"></asp:ListItem>
                    <asp:ListItem Text="only approved" Value="approved"></asp:ListItem>
                    <asp:ListItem Text="only unapproved" Value="unapproved"></asp:ListItem>
                </Rock:LabeledRadioButtonList>

            </Rock:GridFilter>
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
            <Rock:Grid ID="gPrayerComments" runat="server" AllowSorting="true" OnRowSelected="gPrayerComments_Edit" ShowActionExcelExport="false">
                <Columns>
                    <Rock:DateField DataField="Date" HeaderText="Date" SortExpression="Date"/>
                    <asp:BoundField DataField="Caption" HeaderText="From" SortExpression="Text" />
                    <asp:BoundField DataField="Text" HeaderText="Comment" SortExpression="Text" />
                    <%--<Rock:ToggleField DataField="IsApproved" HeaderText="Approval Status" CssClass="switch-mini" Enabled="True" OnText="yes" OffText="no" SortExpression="IsApproved" OnCheckedChanged="gPrayerComments_CheckChanged" />--%>
                    <Rock:DeleteField OnClick="gPrayerComments_Delete" />
                </Columns>
            </Rock:Grid>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
