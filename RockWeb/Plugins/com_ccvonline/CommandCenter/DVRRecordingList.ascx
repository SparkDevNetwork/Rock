<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DVRRecordingList.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.CommandCenter.DVRRecordingList" %>

<asp:UpdatePanel ID="upRecordings" runat="server">
    <ContentTemplate>

        <Rock:GridFilter ID="rFilter" runat="server">
            <Rock:DatePicker ID="dtStartDate" runat="server" Label="From Date" />
            <Rock:DatePicker ID="dtEndDate" runat="server" Label="To Date" />
            <Rock:CampusPicker ID="cpCampus" runat="server" />
            <Rock:RockDropDownList ID="ddlVenueType" runat="server" Label="Venue Type" />
        </Rock:GridFilter>

        <Rock:Grid ID="gRecordings" runat="server" EmptyDataText="No DVR Recordings Found" DataKeyNames="WeekendDate,CampusGuid,VenueType" AllowSorting="true" OnRowSelected="gRecordings_Edit">
            <Columns>
                <Rock:DateField HeaderText="Weekend Date" DataField="WeekendDate" SortExpression="WeekendDate" />
                <asp:BoundField HeaderText="Campus" DataField="Campus" SortExpression="CampusId" />
                <asp:BoundField HeaderText="Venue" DataField="VenueType" SortExpression="VenueType" />
                <asp:BoundField HeaderText="Recordings" DataField="RecordingCount" SortExpression="RecordingCount" />
            </Columns>
        </Rock:Grid>

    </ContentTemplate>
</asp:UpdatePanel>