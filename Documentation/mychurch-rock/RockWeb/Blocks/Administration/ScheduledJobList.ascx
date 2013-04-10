<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduledJobList.ascx.cs" Inherits="RockWeb.Blocks.Administration.ScheduledJobList" %>
<asp:UpdatePanel ID="upScheduledJobList" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        <Rock:Grid ID="gScheduledJobs" runat="server" OnRowSelected="gScheduledJobs_Edit" AllowSorting="true">
            <Columns>
                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                <asp:BoundField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                <asp:BoundField DataField="Assembly" HeaderText="Assembly" SortExpression="Assembly" />
                <Rock:DateTimeField DataField="LastSuccessfulRunDateTime" HeaderText="Last Successful Run" SortExpression="LastSuccessfulRunDateTime" />
                <Rock:DateTimeField DataField="LastRunDateTime" HeaderText="Last Run Date" SortExpression="LastRunDateTime" />
                <asp:BoundField DataField="LastRunDurationSeconds" HeaderText="Last Run Duration" SortExpression="LastRunDurationSeconds" />
                <asp:BoundField DataField="LastStatus" HeaderText="Last Status" SortExpression="LastStatus" />
                <asp:BoundField DataField="NotificationEmails" HeaderText="Notification Emails" />
                <asp:BoundField DataField="NotificationStatus" HeaderText="Notification Status" SortExpression="NotificationStatus" />
                <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                <Rock:DeleteField OnClick="gScheduledJobs_Delete" />
            </Columns>
        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>
