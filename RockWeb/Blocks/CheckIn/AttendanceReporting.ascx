<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttendanceReporting.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.AttendanceReporting" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:LineChart ID="lcAttendance" runat="server" DataSourceUrl="" Title="" Subtitle=""/>

    </ContentTemplate>
</asp:UpdatePanel>
