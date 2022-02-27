<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupAttendanceList.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupAttendanceList" %>

<asp:UpdatePanel ID="pnlContent" runat="server">
    <ContentTemplate>


        <div class="row">
            <div class="col-xs-12 form-group">
            <asp:LinkButton id="grpAttendanceLink" runat="server" class="btn btn-primary" OnClick="grpAttendanceLink_Click"><span class='fa fa-plus'></span> Add Attendance</asp:LinkButton>
        </div>
        </div>
        

        <div class="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title pull-left">
                    <i class="fa fa-check-square-o"></i>
                    <asp:Literal ID="lHeading" runat="server" Text="Group Attendance" />
                </h1>

                <Rock:ButtonDropDownList ID="bddlCampus" runat="server" FormGroupCssClass="panel-options pull-right dropdown-right" Title="All Campuses" SelectionStyle="Checkmark" OnSelectionChanged="bddlCampus_SelectionChanged" DataTextField="Name" DataValueField="Id" />

            </div>

            <div class="panel-body">
                <style>
                    .grid-actions{
                        text-align: left;
                    }
                </style>
                <div class="grid grid-panel">
                        <Rock:GridFilter ID="rFilter" Visible="false" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue" OnClearFilterClick="rFilter_ClearFilterClick">
                        <Rock:DateRangePicker ID="drpDates" runat="server" Label="Date Range" />
                        <Rock:RockDropDownList ID="ddlLocation" runat="server" Label="Location" DataValueField="Key" DataTextField="Value" />
                        <Rock:RockDropDownList ID="ddlSchedule" runat="server" Label="Schedule" DataValueField="Key" DataTextField="Value"  />
                    </Rock:GridFilter>
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gOccurrences" runat="server" DisplayType="Light" AllowSorting="true" RowItemText="Occurrence" OnRowSelected="gOccurrences_Edit" ShowActionsInHeader="true" HeaderStyle-HorizontalAlign="Left" >
                        <Columns>
                            <Rock:DateField DataField="OccurrenceDate" HeaderText="Date" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" SortExpression="OccurrenceDate" />
		                    <Rock:RockTemplateField HeaderText="Location" Visible="false" SortExpression="LocationPath,LocationName" ColumnPriority="TabletSmall">
		                        <ItemTemplate>
		                            <%#Eval("LocationName")%><br />
		                            <small><%#Eval("ParentLocationPath")%></small>
		                        </ItemTemplate>
		                    </Rock:RockTemplateField>
                            <Rock:RockBoundField DataField="ScheduleName" Visible="false" HeaderText="Schedule" SortExpression="ScheduleName" ColumnPriority="Tablet" HtmlEncode="false" />
                            <Rock:BoolField DataField="AttendanceEntered" Visible="false" HeaderText="Attendance Entered" SortExpression="AttendanceEntered" />
                            <Rock:BoolField DataField="DidNotOccur" Visible="false" HeaderText="Didn't Meet" SortExpression="DidNotOccur" />
                            <Rock:RockBoundField DataField="DidAttendCount" Visible="false" HeaderText="Attendance Count" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:N0}" SortExpression="DidAttendCount" HeaderStyle-HorizontalAlign="Right" />
                            <Rock:RockBoundField DataField="AttendanceRate" Visible="false" HtmlEncode="false" HeaderText="Percent Attended&nbsp;<i class='fa fa-info-circle' data-toggle='tooltip' data-placement='top' title='The percentage of attendees that were marked &quot;Did Attend&quot;.'></i>" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:P0}" SortExpression="AttendanceRate" HeaderStyle-HorizontalAlign="Right"/>
                            <Rock:RockBoundField DataField="Notes" HeaderText="Notes" HtmlEncode="false" ColumnPriority="Desktop" SortExpression="Notes"/>
                            <Rock:EditField IconCssClass="fa fa-check-square-o" OnClick="gOccurrences_Edit" ToolTip="Enter Attendance" />
                            <Rock:DeleteField Visible="false" OnClick="gOccurrences_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </div>

    </ContentTemplate>
</asp:UpdatePanel>
