<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AdvancedEventNametags.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Event.AdvancedEventNametags" %>
<asp:UpdatePanel ID="upContent" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Danger" Visible="true" />
        <asp:Panel ID="pnlFilter" runat="server" CssClass="panel panel-block margin-t-md">
            <div class="panel-heading clearfix">
                <h1 class="panel-title"><i class="far fa-id-badge"></i>
                    <asp:Literal ID="lReportHeader" runat="server" /></h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfAttendeeFilter" runat="server" OnApplyFilterClick="gfAttendeeFilter_ApplyFilterClick" OnClearFilterClick="gfAttendeeFilter_ClearFilterClick" OnDisplayFilterValue="gfAttendeeFilter_DisplayFilterValue">
                        <Rock:RegistrationInstancePicker ID="riPicker" runat="server" Visible="true" />
                        <Rock:RockCheckBoxList ID="cblCampuses" runat="server" Label="Campus Attending With" RepeatColumns="2" RepeatDirection="Horizontal" />
                        <Rock:RockCheckBoxList ID="cblAttendees" runat="server" Label="Attendees to Include">
                            <asp:ListItem Value="0" Text="Registrants" Selected="True" />
                            <asp:ListItem Value="1" Text="Volunteers" Selected="True" />
                        </Rock:RockCheckBoxList>
                    </Rock:GridFilter>
                    <Rock:Grid ID="gAttendeeList" runat="server" RowItemText="Attendee" AllowSorting="true" ExportSource="DataSource" ExportFilename="Attendees" OnGridRebind="gAttendeeList_GridRebind">
                        <Columns>
                            <Rock:SelectField />
                            <Rock:RockBoundField DataField="Id" HeaderText="Id" SortExpression="Id" Visible="false" />
                            <Rock:RockBoundField DataField="RegistrantId" HeaderText="RegistrantId" Visible="false" />
                            <Rock:RockBoundField DataField="NickName" HeaderText="Nick Name" SortExpression="NickName" Visible="false" />
                            <Rock:RockBoundField DataField="LastName" HeaderText="Last Name" SortExpression="LastName" Visible="false" />
                            <Rock:RockBoundField DataField="FullName" HeaderText="Name" SortExpression="FullNameReversed" Visible="true" />
                            <Rock:RockBoundField DataField="CampusName" HeaderText="Campus" SortExpression="CampusName" Visible="true" />
                            <Rock:RockBoundField DataField="Transportation" HeaderText="Transportation" SortExpression="Transportation" Visible="true" />
                            <Rock:RockBoundField DataField="Lodging" HeaderText="Lodging" SortExpression="Lodging" Visible="true" />
                            <Rock:RockBoundField DataField="Team" HeaderText="Team" SortExpression="Team" Visible="true" />
                            <Rock:RockBoundField DataField="RecTeam" HeaderText="RecTeam" SortExpression="RecTeam" Visible="true" />
                            <Rock:RockBoundField DataField="Activity1" HeaderText="Activity 1" SortExpression="Activity1" Visible="true" />
                            <Rock:RockBoundField DataField="Activity2" HeaderText="Activity 2" SortExpression="Activity2" Visible="true" />
                            <Rock:RockBoundField DataField="Activity3" HeaderText="Activity 3" SortExpression="Activity3" Visible="true" />
                            <Rock:RockBoundField DataField="Counselor" HeaderText="Counselor" SortExpression="Counselor" Visible="true" />
                            <Rock:RockBoundField DataField="FeesPaid" HeaderText="Fees" SortExpression="FeesPaid" Visible="true" />
                            <Rock:BoolField DataField="IsVolunteer" HeaderText=" Is Volunteer" SortExpression="IsVolunteer" Visible="true" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
            <asp:Button ID="btnRegistrationInstance" runat="server" Text="Update" Style="display: none;" OnClick="btnRegistrationInstance_Click" />
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
