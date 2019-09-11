<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RapidAttendanceEntry.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.RapidAttendanceEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" />

        <asp:Panel ID="pnlEntry" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-calendar-check-o"></i> Rapid Attendance</h1>
                <div class="pull-right">
                    <Rock:HighlightLabel ID="hlCurrentCount" runat="server" LabelType="Info" Text="Current Attendance: 0" />
                </div>
            </div>

            <div class="panel-body">
                <div class="row">
                    <asp:Panel ID="pnlGroupPicker" runat="server" CssClass="col-md-4">
                        <Rock:RockDropDownList ID="ddlGroup" runat="server" Label="Group" Required="true" OnSelectedIndexChanged="ddlGroup_SelectedIndexChanged" AutoPostBack="true" />
                    </asp:Panel>

                    <asp:Panel ID="pnlLocationPicker" runat="server" CssClass="col-md-4">
                        <Rock:RockDropDownList ID="ddlLocation" runat="server" Label="Location" Required="true" OnSelectedIndexChanged="ddlLocation_SelectedIndexChanged" AutoPostBack="true" />
                    </asp:Panel>

                    <asp:Panel ID="pnlSchedulePicker" runat="server" CssClass="col-md-4">
                        <Rock:RockDropDownList ID="ddlSchedule" runat="server" Label="Schedule" Required="true" OnSelectedIndexChanged="ddlSchedule_SelectedIndexChanged" AutoPostBack="true" />
                    </asp:Panel>
                </div>

                <Rock:DatePicker ID="dpAttendanceDate" runat="server" Required="true" Label="Attendance Date" Help="The date to use when recording the person as having attended." OnTextChanged="dpAttendanceDate_TextChanged" AutoPostBack="true" />

                <hr />

                <div class="row">
                    <div class="col-md-4">
                        <Rock:PersonPicker ID="ppAttendee" runat="server" Label="Attendee" Required="true" CssClass="js-attendee" OnSelectPerson="ppAttendee_SelectPerson" />
                        <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-primary" AccessKey="s" Text="Save" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnSaveActivate" runat="server" CssClass="btn btn-default pull-right" AccessKey="a" Text="Activate & Save" OnClick="btnSave_Click" Visible="false" />
                    </div>

                    <div class="col-md-8">
                        <Rock:NotificationBox ID="nbInactivePerson" runat="server" NotificationBoxType="Warning" Text="This person record is currently inactive." Visible="false" />
                        <Rock:NotificationBox ID="nbAttended" runat="server" NotificationBoxType="Success" Text="" />
                    </div>
                </div>
            </div>
        </asp:Panel>

        <div class="row">
            <div class="col-md-12">
                <div class="pull-right padding-r-sm">
                    <asp:CheckBox ID="cbShowCurrent" runat="server" OnCheckedChanged="cbShowCurrent_CheckedChanged" Text="Show Current Attendees" AutoPostBack="true" />
                </div>
            </div>
        </div>

        <asp:Panel ID="pnlCurrentAttendees" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">Current Attendees</h1>
            </div>

            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:Grid ID="gAttendees" runat="server" DataKeyNames="Id">
                        <Columns>
                            <asp:BoundField DataField="PersonAlias.Person.FullNameReversed" HeaderText="Name" />
                            <asp:BoundField DataField="CreatedByPersonName" HeaderText="Entered By" />
                            <Rock:DeleteField OnClick="gAttendeesDelete_Click"></Rock:DeleteField>
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>