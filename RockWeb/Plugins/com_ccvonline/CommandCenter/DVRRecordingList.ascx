<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DVRRecordingList.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.CommandCenter.DVRRecordingList" %>

<asp:UpdatePanel ID="upRecordings" runat="server">
    <ContentTemplate>

        <Rock:GridFilter ID="rFilter" runat="server">
            <Rock:DatePicker ID="dtStartDate" runat="server" Label="From Date" />
            <Rock:DatePicker ID="dtEndDate" runat="server" Label="To Date" />
            <Rock:CampusPicker ID="cpCampus" runat="server" />
            <Rock:RockTextBox ID="tbVenue" runat="server" Label="Venue" />
        </Rock:GridFilter>

        <Rock:Grid ID="gRecordings" runat="server" EmptyDataText="No DVR Recordings Found" DataKeyNames="WeekendDate,CampusGuid,Venue" AllowSorting="true" OnRowSelected="gRecordings_Edit">
            <Columns>
                <Rock:DateField HeaderText="Weekend Date" DataField="WeekendDate" SortExpression="WeekendDate" />
                <asp:BoundField HeaderText="Campus" DataField="Campus" SortExpression="CampusId" />
                <asp:BoundField HeaderText="Venue" DataField="Venue" SortExpression="Venue" />
                <asp:BoundField HeaderText="Recordings" DataField="RecordingCount" SortExpression="RecordingCount" />
            </Columns>
        </Rock:Grid>

    </ContentTemplate>
</asp:UpdatePanel>