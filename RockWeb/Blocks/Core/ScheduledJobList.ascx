<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduledJobList.ascx.cs" Inherits="RockWeb.Blocks.Administration.ScheduledJobList" %>

<asp:UpdatePanel ID="upScheduledJobList" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <asp:Panel ID="pnlScheduledJobs" CssClass="panel panel-block" runat="server">
            
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-clock-o"></i> Jobs List</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:Grid ID="gScheduledJobs" runat="server" TooltipField="Description" OnRowSelected="gScheduledJobs_Edit" AllowSorting="true" OnRowDataBound="gScheduledJobs_RowDataBound">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:DateTimeField DataField="LastSuccessfulRunDateTime" HeaderText="Last Successful Run" SortExpression="LastSuccessfulRunDateTime" />
                            <Rock:DateTimeField DataField="LastRunDateTime" HeaderText="Last Run Date" SortExpression="LastRunDateTime" />
                            <Rock:RockBoundField SortExpression="LastRunDurationSeconds" HeaderText="Last Run Duration" ItemStyle-HorizontalAlign="Center" />
                            <Rock:RockBoundField DataField="LastStatus" HeaderText="Last Status" SortExpression="LastStatus" ItemStyle-HorizontalAlign="Center" />
                            <Rock:RockBoundField DataField="LastStatusMessage" HeaderText="Last Status Message" SortExpression="LastStatusMessage" TruncateLength="255" />
                            <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                            <Rock:EditField OnClick="gScheduledJobs_RunNow" IconCssClass="fa fa-play" HeaderText="Run Now" ToolTip="Run Now" />
                            <Rock:DeleteField OnClick="gScheduledJobs_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
