<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RecordingList.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.CommandCenter.RecordingList" %>

<asp:UpdatePanel ID="upRecordings" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <Rock:GridFilter ID="rFilter" runat="server">
            <Rock:CampusPicker ID="cpCampus" runat="server" />
            <Rock:DatePicker ID="dtStartDate" runat="server" Label="From Date" />
            <Rock:DatePicker ID="dtEndDate" runat="server" Label="To Date" />
            <Rock:RockTextBox ID="tbStream" runat="server" Label="Stream"></Rock:RockTextBox>
            <Rock:RockTextBox ID="tbVenue" runat="server" Label="Venue"></Rock:RockTextBox>
            <Rock:RockTextBox ID="tbLabel" runat="server" Label="Label"></Rock:RockTextBox>
            <Rock:RockTextBox ID="tbRecording" runat="server" Label="Recording"></Rock:RockTextBox>
        </Rock:GridFilter>

        <Rock:Grid ID="gRecordings" runat="server" EmptyDataText="No Recordings Found" AllowSorting="true" OnRowSelected="gRecordings_Edit">
            <Columns>
                <asp:BoundField HeaderText="Campus" DataField="Campus" SortExpression="Campus" />
                <asp:BoundField HeaderText="Date" DataField="Date" SortExpression="Date" DataFormatString="{0:MM/dd/yy}" />
                <asp:BoundField HeaderText="Stream" DataField="StreamName" SortExpression="StreamName" />
                <asp:BoundField HeaderText="Venue" DataField="VenueType" SortExpression="VenueType" />
                <asp:BoundField HeaderText="Label" DataField="Label" SortExpression="Label" />
                <asp:BoundField HeaderText="Recording" DataField="RecordingName" SortExpression="RecordingName" />
                <asp:BoundField HeaderText="Started" DataField="StartTime" SortExpression="StartTime" DataFormatString="{0:MM/dd/yy hh:mm:ss tt}" />
                <asp:BoundField HeaderText="Stopped" DataField="StopTime" SortExpression="StopTime" DataFormatString="{0:MM/dd/yy hh:mm:ss tt}" />
                <asp:BoundField HeaderText="Length" DataField="Length" />
                <Rock:LinkButtonField CssClass="btn btn-default btn-sm" OnClick="gRecordings_Start" Text='<i class="fa fa-play"></i>' /> 
                <Rock:LinkButtonField CssClass="btn btn-default btn-sm" OnClick="gRecordings_Stop" Text='<i class="fa fa-stop"></i>' /> 
                <Rock:DeleteField OnClick="gRecordings_Delete" />
            </Columns>
        </Rock:Grid>

    </ContentTemplate>
</asp:UpdatePanel>

