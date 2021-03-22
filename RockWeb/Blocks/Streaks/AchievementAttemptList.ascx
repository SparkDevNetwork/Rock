<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AchievementAttemptList.ascx.cs" Inherits="RockWeb.Blocks.Streaks.AchievementAttemptList" %>

<asp:UpdatePanel ID="upMain" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbBlockStatus" runat="server" NotificationBoxType="Info" />

        <asp:Panel ID="pnlContent" runat="server">

            <div id="pnlAttempts" runat="server">

                <div class="panel panel-block">

                    <div class="panel-heading clearfix">
                        <h1 class="panel-title pull-left">
                            <i class="fa fa-users"></i>
                            <asp:Literal ID="lHeading" runat="server" />
                        </h1>
                    </div>

                    <div class="panel-body">
                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue" OnClearFilterClick="rFilter_ClearFilterClick" FieldLayout="Custom">
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:RockTextBox ID="tbAchieverName" runat="server" Label="Achiever Name" />
                                        <Rock:AchievementTypePicker ID="statPicker" runat="server" Label="Achievement Type" />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:DateRangePicker ID="drpStartDate" runat="server" Label="Start Date" />
                                        <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" />
                                    </div>
                                </div>
                            </Rock:GridFilter>
                            <Rock:Grid ID="gAttempts" runat="server" AllowSorting="true" OnRowDataBound="gAttempts_RowDataBound" ExportSource="ColumnOutput" OnRowSelected="gAttempts_RowSelected" >
                                <Columns>
                                    <Rock:RockBoundField DataField="AchieverName" HeaderText="Achiever" SortExpression="AchieverName" />
                                    <Rock:RockBoundField DataField="AchievementName" HeaderText="Achievement" />
                                    <Rock:DateTimeField HeaderText="Start Date" DataField="StartDate" SortExpression="StartDate" DataFormatString="{0:d}" />
                                    <Rock:DateTimeField HeaderText="End Date" DataField="EndDate" SortExpression="EndDate" DataFormatString="{0:d}" />
                                    <Rock:BoolField DataField="IsSuccessful" HeaderText="Successful" SortExpression="IsSuccessful" />
                                    <Rock:BoolField DataField="IsClosed" HeaderText="Closed" SortExpression="IsClosed" />
                                    <Rock:RockLiteralField ID="lProgress" HeaderText="Progress" SortExpression="Progress" />
                                    <Rock:DeleteField OnClick="gAttempts_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
