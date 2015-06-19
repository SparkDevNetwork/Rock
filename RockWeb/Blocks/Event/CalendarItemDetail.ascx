<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CalendarItemDetail.ascx.cs" Inherits="RockWeb.Blocks.Event.CalendarItemDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=btnHideDialog.ClientID%>').click();
    }
</script>

<asp:UpdatePanel ID="upnlEventItemList" runat="server">
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="cblAdditionalCalendars" EventName="SelectedIndexChanged" />
    </Triggers>
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server">

            <asp:HiddenField ID="hfEventItemId" runat="server" />

            <div class="panel panel-block">

                <div class="panel-heading clearfix">
                    <h1 class="panel-title pull-left"><i class="fa fa-calendar-o"></i>
                        <asp:Literal ID="lReadOnlyTitle" runat="server" />
                    </h1>

                    <div class="panel-labels">
                        <Rock:HighlightLabel ID="hlStatus" runat="server" />
                    </div>
                </div>

                <div class="panel-body">

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <Rock:NotificationBox ID="nbIncorrectCalendarItem" runat="server" NotificationBoxType="Danger" Visible="false"
                        Text="The item selected does not belong to the selected calendar." />

                    <Rock:NotificationBox ID="nbNotAllowedToEdit" runat="server" NotificationBoxType="Danger" Visible="false"
                        Text="You are not authorized to save calendar items for the configured calendar(s)." />

                    <asp:ValidationSummary ID="vsSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <div id="pnlEditDetails" runat="server">

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.EventItem, Rock" PropertyName="Name" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                            </div>
                        </div>

                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.EventItem, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockControlWrapper ID="rcwAudiences" runat="server" Label="Audiences">
                                    <div class="grid">
                                        <Rock:Grid ID="gAudiences" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Audience" ShowHeader="false">
                                            <Columns>
                                                <Rock:RockBoundField DataField="Value" />
                                                <Rock:DeleteField OnClick="gAudiences_Delete" />
                                            </Columns>
                                        </Rock:Grid>
                                    </div>
                                </Rock:RockControlWrapper>
                                <Rock:RockCheckBoxList ID="cblAdditionalCalendars" runat="server" Label="Additional Calendars" 
                                    OnSelectedIndexChanged="cblAdditionalCalendars_SelectedIndexChanged" AutoPostBack="true"
                                    RepeatDirection="Horizontal" />
                                <Rock:RockTextBox ID="tbDetailUrl" runat="server" Label="Details URL" />
                            </div>
                            <div class="col-md-6">
                                <Rock:ImageUploader ID="imgupPhoto" runat="server" Label="Photo" />
                            </div>
                        </div>

                        <Rock:PanelWidget ID="wpAttributes" runat="server" Title="Attribute Values">
                            <asp:PlaceHolder ID="phAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>
                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="wpCampusDetails" runat="server" Title="Campus Details">
                            <div class="grid">
                                <Rock:Grid ID="gCampusDetails" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Campus Detail">
                                    <Columns>
                                        <Rock:RockBoundField DataField="Campus" HeaderText="Campus" />
                                        <Rock:RockBoundField DataField="Location" HeaderText="Location" />
                                        <Rock:RockBoundField DataField="Contact" HeaderText="Contact" />
                                        <Rock:RockBoundField DataField="Phone" HeaderText="Phone" />
                                        <Rock:RockBoundField DataField="Email" HeaderText="Email" />
                                        <Rock:RockBoundField DataField="Registration" HeaderText="Registration" HtmlEncode="false" />
                                        <Rock:EditField OnClick="gCampusDetails_Edit" />
                                        <Rock:DeleteField OnClick="gCampusDetails_Delete" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </Rock:PanelWidget>

                        <div class="actions">
                            <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                            <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                        </div>
                    </div>
                </div>
            </div>

        </asp:Panel>

        <asp:Button ID="btnHideDialog" runat="server" Style="display: none" OnClick="btnHideDialog_Click" />
        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgCampusDetails" runat="server" Title="Campus Details" OnSaveClick="dlgCampusDetails_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="CampusDetails">
            <Content>

                <asp:HiddenField ID="hfCampusGuid" runat="server" />

                <asp:ValidationSummary ID="valCampusDetails" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="CampusDetails" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlCampus" runat="server" Label="Campus" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.Campus, Rock" PropertyName="Name" />
                        <Rock:RockTextBox ID="tbLocation" runat="server" Label="Location" ValidationGroup="CampusDetails" Required="true" />
                        <Rock:RockTextBox ID="tbRegistration" runat="server" Label="Registration URL" ValidationGroup="CampusDetails" />
                    </div>
                    <div class="col-md-6">
                        <Rock:PersonPicker ID="ppContact" runat="server" Label="Contact" ValidationGroup="CampusDetails" OnSelectPerson="ppContact_SelectPerson" Required="true" />
                        <Rock:PhoneNumberBox ID="pnPhone" runat="server" Label="Phone" ValidationGroup="CampusDetails" />
                        <Rock:EmailBox ID="tbEmail" runat="server" Label="Email" ValidationGroup="CampusDetails" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbCampusNote" runat="server" Label="Campus Note" TextMode="MultiLine" Rows="5" />
                    </div>
                    <div class="col-md-6">
                        <div class="grid">
                            <Rock:RockControlWrapper ID="rcwSchedules" runat="server" Label="Schedules">
                                <asp:HiddenField ID="hfSchedules" runat="server" />
                                <Rock:HiddenFieldValidator ID="hfvSchedules" runat="server" Display="None" ErrorMessage="At least one Schedule is required." ControlToValidate="hfSchedules" ValidationGroup="CampusDetails" />
                                <Rock:Grid ID="gSchedules" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Schedule">
                                    <Columns>
                                        <Rock:RockBoundField DataField="Schedule" />
                                        <Rock:RockBoundField DataField="Details" />
                                        <Rock:EditField OnClick="gSchedules_Edit" />
                                        <Rock:DeleteField OnClick="gSchedules_Delete" />
                                    </Columns>
                                </Rock:Grid>
                            </Rock:RockControlWrapper>
                        </div>
                    </div>
                </div>

            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgAudience" runat="server" ScrollbarEnabled="false" ValidationGroup="Audience" SaveButtonText="Add" OnCancelScript="clearActiveDialog();" OnSaveClick="btnSaveAudience_Click" Title="Select Audience">
            <Content>
                <asp:ValidationSummary ID="vsAudience" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="Audience" />
                <Rock:RockDropDownList ID="ddlAudience" runat="server" Label="Select Audience" ValidationGroup="Audience" Required="true" DataValueField="Id" DataTextField="Value" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgSchedule" runat="server" ScrollbarEnabled="false" ValidationGroup="Schedule" SaveButtonText="Ok" OnCancelScript="clearActiveDialog();" OnSaveClick="btnSaveSchedule_Click" Title="Schedule">
            <Content>
                <asp:ValidationSummary ID="vsSchedule" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="Schedule" />
                <asp:HiddenField ID="hfScheduleGuid" runat="server" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbScheduleName" runat="server" Label="Name" ValidationGroup="Schedule" Required="true" />
                    </div>
                    <div class="col-md-6">
                        <Rock:ScheduleBuilder ID="sbSchedule" runat="server" Label="Schedule" ValidationGroup="Schedule" AllowMultiSelect="true" Required="true" OnSaveSchedule="sbSchedule_SaveSchedule"/>
                        <Rock:RockLiteral ID="lScheduleText" runat="server" />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
