<%@ control language="C#" autoeventwireup="true" inherits="RockWeb.Blocks.Administration.ScheduledJobList, RockWeb" %>

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
                            <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:DateTimeField DataField="LastSuccessfulRunDateTime" HeaderText="Last Successful Run" SortExpression="LastSuccessfulRunDateTime" />
                            <Rock:DateTimeField DataField="LastRunDateTime" HeaderText="Last Run Date" SortExpression="LastRunDateTime" />
                            <asp:BoundField SortExpression="LastRunDurationSeconds" HeaderText="Last Run Duration" ItemStyle-HorizontalAlign="Center" />
                            <asp:BoundField DataField="LastStatus" HeaderText="Last Status" SortExpression="LastStatus" ItemStyle-HorizontalAlign="Center" />
                            <asp:BoundField DataField="LastStatusMessage" HeaderText="Last Status Message" SortExpression="LastStatusMessage" />
                            <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                            <Rock:DeleteField OnClick="gScheduledJobs_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
