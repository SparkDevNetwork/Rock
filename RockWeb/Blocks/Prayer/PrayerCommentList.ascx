<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PrayerCommentList.ascx.cs" Inherits="RockWeb.Blocks.Prayer.PrayerCommentsList" %>
<asp:UpdatePanel ID="upPrayerRequests" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlLists" runat="server" Visible="true">
            
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-comment"></i> Prayer Comments</h1>
                </div>
                <div class="panel-body">
                    <div class="grid grid-panel">
                        <Rock:GridFilter ID="gfFilter" runat="server" OnApplyFilterClick="gfFilter_ApplyFilterClick" OnDisplayFilterValue="gfFilter_DisplayFilterValue">
                            <Rock:DateRangePicker ID="drpDateRange" runat="server" Label="Date Range" />
                        </Rock:GridFilter>
                        <Rock:ModalAlert ID="maGridWarning" runat="server" />
                        <Rock:Grid ID="gPrayerComments" runat="server" AllowSorting="true" RowItemText="comment" OnRowSelected="gPrayerComments_Edit" ExcelExportEnabled="false">
                            <Columns>
                                <Rock:DateTimeField DataField="CreatedDateTime" HeaderText="Time" SortExpression="CreatedDateTime"/>
                                <Rock:RockBoundField DataField="CreatedByPersonAlias.Person.FullName" HeaderText="From" SortExpression="Text" />
                                <Rock:RockBoundField DataField="Text" HeaderText="Comment" SortExpression="Text" />
                                <Rock:DeleteField OnClick="gPrayerComments_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
