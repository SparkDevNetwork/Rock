<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduledJobList.ascx.cs" Inherits="RockWeb.Blocks.Administration.ScheduledJobList" %>

<asp:UpdatePanel ID="upScheduledJobList" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <asp:Panel ID="pnlScheduledJobs" runat="server">
            <Rock:Grid ID="gScheduledJobs" runat="server" OnRowSelected="gScheduledJobs_Edit" AllowSorting="true" OnRowDataBound="gScheduledJobs_RowDataBound">
                <Columns>
                    <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                    <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                    <Rock:DateTimeField DataField="LastSuccessfulRunDateTime" HeaderText="Last Successful Run" SortExpression="LastSuccessfulRunDateTime" />
                    <Rock:DateTimeField DataField="LastRunDateTime" HeaderText="Last Run Date" SortExpression="LastRunDateTime" />
                    <asp:BoundField SortExpression="LastRunDurationSeconds" HeaderText="Last Run Duration" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="LastStatus" HeaderText="Last Status" SortExpression="LastStatus" ItemStyle-HorizontalAlign="Center" />
                    <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                    <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                    <Rock:DeleteField OnClick="gScheduledJobs_Delete" />
                </Columns>
            </Rock:Grid>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
