<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduledJobHistoryList.ascx.cs" Inherits="RockWeb.Blocks.Administration.ScheduledJobHistoryList" %>

<asp:UpdatePanel ID="upScheduledJobHistoryList" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <asp:Panel ID="pnlScheduledJobs" CssClass="panel panel-block" runat="server">
            
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-history"></i> Scheduled Job History for <asp:Literal ID="lJobName" runat="server" /></h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:Grid ID="gScheduledJobHistory" runat="server" TooltipField="Description" AllowSorting="true" OnRowDataBound="gScheduledJobHistory_RowDataBound">
                        <Columns>
                            <Rock:DateTimeField DataField="StartDateTime" HeaderText="Start DateTime" SortExpression="StartDateTime"/>
                            <Rock:DateTimeField DataField="StopDateTime" HeaderText="Stop DateTime" SortExpression="StopDateTime"/>
                            <Rock:RockLiteralField ID="lDurationSeconds" HeaderText="Run Duration" SortExpression="DurationSeconds" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" />
                            <Rock:RockLiteralField ID="lStatus" HeaderText="Status" SortExpression="Status" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center"/>
                            <Rock:RockLiteralField ID="lStatusMessageAsHtml" HeaderText="Status Message" SortExpression="StatusMessage" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
