<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReservationDetail.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.RoomManagement.ReservationDetail" %>
<%@ Register TagPrefix="CentralAZ" Assembly="com.centralaz.RoomManagement" Namespace="com.centralaz.RoomManagement.Web.UI.Controls" %>
<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="cblCalendars" EventName="SelectedIndexChanged" />
    </Triggers>
    <ContentTemplate>
        <Rock:NotificationBox ID="nbNotAuthorized" runat="server" NotificationBoxType="Danger" />

        <asp:Panel ID="pnlLavaInstructions" runat="server" Visible="false">
            <asp:Literal ID="lLavaInstructions" runat="server" />
        </asp:Panel>
        <asp:Panel ID="pnlDetails" runat="server" CssClass="panel panel-block">

            <asp:HiddenField ID="hfReservationId" runat="server" />
            <asp:HiddenField ID="hfEventItemGuid" runat="server" />
            <asp:HiddenField ID="hfEventItemOccurrenceGuid" runat="server" />

            <asp:Panel ID="pnlReservation_Header" runat="server" CssClass="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lPanelTitle" runat="server" />
                </h1>
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlStatus" runat="server" />
                </div>
            </asp:Panel>
            <asp:Panel ID="pnlEvent_Header" runat="server" CssClass="panel-heading" Visible="false">
                <h1 class="panel-title"><i class="fa fa-calendar-check-o"></i>
                    Reservation Event Linkage - Event</h1>
            </asp:Panel>
            <asp:Panel ID="pnlEventOccurrence_Header" runat="server" CssClass="panel-heading" Visible="false">
                <h1 class="panel-title"><i class="fa fa-clock-o"></i>
                    Reservation Event Linkage - Event Occurrence</h1>
            </asp:Panel>
            <asp:Panel ID="pnlSummary_Header" runat="server" CssClass="panel-heading" Visible="false">
                <h1 class="panel-title"><i class="fa fa-list-ul"></i>
                    Reservation Event Linkage - Summary</h1>
            </asp:Panel>
            <asp:Panel ID="pnlFinished_Header" runat="server" CssClass="panel-heading" Visible="false">
                <h1 class="panel-title"><i class="fa fa-check"></i>
                    Reservation Event Linkage - Finished</h1>
            </asp:Panel>

            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>

            <asp:Panel ID="pnlWizard" runat="server" CssClass="wizard" Visible="false">
                <div id="divEvent" runat="server" class="wizard-item">
                    <asp:LinkButton ID="lbEvent" runat="server" OnClick="lbEvent_Click" CausesValidation="false">
                            <asp:PlaceHolder runat="server">
                                <div class="wizard-item-icon">
                                    <i class="fa fa-fw fa-calendar-check-o"></i>
                                </div>
                                <div class="wizard-item-label">
                                    Event
                                </div>
                            </asp:PlaceHolder>
                    </asp:LinkButton>
                </div>

                <div id="divEventOccurrence" runat="server" class="wizard-item">
                    <asp:LinkButton ID="lbEventOccurrence" runat="server" OnClick="lbEventOccurrence_Click" CausesValidation="false" Enabled="false">
                            <asp:PlaceHolder runat="server">
                                <div class="wizard-item-icon">
                                    <i class="fa fa-fw fa-clock-o"></i>
                                </div>
                                <div class="wizard-item-label">
                                    Event Occurrence
                                </div>
                            </asp:PlaceHolder>
                    </asp:LinkButton>
                </div>

                <div id="divSummary" runat="server" class="wizard-item">
                    <div class="wizard-item-icon">
                        <i class="fa fa-fw fa-list-ul"></i>
                    </div>
                    <div class="wizard-item-label">
                        Summary
                    </div>
                </div>
            </asp:Panel>

            <div class="panel-body">
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <asp:ValidationSummary ID="valReservationEventOccurrenceSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="ReservationEventOccurrence" />

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                <Rock:NotificationBox ID="nbError" runat="server" NotificationBoxType="Danger" />
                <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" />

                <asp:Panel ID="pnlViewDetails" runat="server">

                    <div class="row">
                        <div class="col-md-6">
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockLiteral ID="lName" runat="server" Label="Name" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockLiteral ID="lReservationType" runat="server" Label="Reservation Type" />
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockLiteral ID="lSchedule" runat="server" Label="Schedule" />
                                </div>
                                <div class="col-md-3">
                                    <Rock:RockLiteral ID="lSetupTime" runat="server" Label="Setup Time" />
                                </div>
                                <div class="col-md-3">
                                    <Rock:RockLiteral ID="lCleanupTime" runat="server" Label="Cleanup Time" />
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockLiteral ID="lCampus" runat="server" Label="Campus" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockLiteral ID="lMinistry" runat="server" Label="Ministry" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockLiteral ID="lNumberAttending" runat="server" Label="Number Attending" />
                                </div>
                                <div class="col-md-3">
                                    <Rock:RockLiteral ID="lSetupPhoto" runat="server" Label="Setup Photo" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockLiteral ID="lEventContact" runat="server" Label="Event Contact" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockLiteral ID="lAdminContact" runat="server" Label="Administrative Contact" />
                                </div>
                            </div>
                            <Rock:RockLiteral ID="lNotes" runat="server" Label="Notes" />
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockLiteral ID="lApproval" runat="server" Label="Approval State" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockLiteral ID="lEventOccurrence" runat="server" Label="Linked Event" />
                                </div>
                            </div>


                        </div>
                        <div class="col-md-6">

                            <div class="grid">
                                <label class="control-label">Locations</label>
                                <Rock:Grid ID="gViewLocations" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Location" OnRowDataBound="gViewLocations_RowDataBound">
                                    <Columns>
                                        <Rock:RockBoundField DataField="Location" HeaderText="Location" />
                                        <Rock:RockBoundField DataField="LocationLayout.Name" HeaderText="Layout" />
                                        <Rock:RockBoundField DataField="LocationLayout.Description" HeaderText="Description" />
                                        <Rock:RockTemplateField HeaderText="Photo">
                                            <ItemTemplate>
                                                <asp:Literal ID="lLayoutPhoto" runat="server" />
                                            </ItemTemplate>
                                        </Rock:RockTemplateField>
                                        <Rock:RockBoundField DataField="ApprovalState" HeaderText="Approved?" />
                                        <Rock:LinkButtonField CssClass="btn btn-success btn-sm" OnClick="gViewLocations_ApproveClick" ToolTip="Approve" Text="<i class='fa fa-check'></i>" Visible="true" />
                                        <Rock:LinkButtonField CssClass="btn btn-danger btn-sm" OnClick="gViewLocations_DenyClick" ToolTip="Deny" Text="<i class='fa fa-ban'></i>" Visible="true" />
                                    </Columns>
                                </Rock:Grid>
                            </div>

                            <div class="grid">
                                <label class="control-label">Resources</label>
                                <Rock:Grid ID="gViewResources" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Resource" OnRowDataBound="gViewResources_RowDataBound">
                                    <Columns>
                                        <Rock:RockBoundField DataField="Resource.Name" HeaderText="Resource" />
                                        <Rock:RockTemplateField>
                                            <ItemTemplate><em class="text-muted"><%# Convert.ToString( Eval( "Resource.Location.Name") ) == string.Empty ? "" : "(attached to " +  Eval("Resource.Location.Name") + ")" %></em></ItemTemplate>
                                        </Rock:RockTemplateField>
                                        <Rock:RockBoundField DataField="Quantity" HeaderText="Qty" />
                                        <Rock:RockBoundField DataField="ApprovalState" HeaderText="Approved?" />
                                        <Rock:LinkButtonField CssClass="btn btn-sm btn-success" OnClick="gViewResources_ApproveClick" ToolTip="Approve"  Text="<i class='fa fa-check'></i>" Visible="true" />
                                        <Rock:LinkButtonField CssClass="btn btn-sm btn-danger" OnClick="gViewResources_DenyClick" ToolTip="Deny" Text="<i class='fa fa-ban'></i>" Visible="true" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </div>
                    </div>

                    <asp:PlaceHolder ID="phAttributes" runat="server"></asp:PlaceHolder>
                    <asp:PlaceHolder ID="phViewLocationAnswers" runat="server" EnableViewState="false" />
                    <asp:PlaceHolder ID="phViewResourceAnswers" runat="server" EnableViewState="false" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:ModalAlert ID="mdWorkflowLaunched" runat="server" />
                            <asp:Label ID="lblWorkflows" Text="Available Workflows" Font-Bold="true" runat="server" />
                            <div class="margin-b-md">
                                <asp:Repeater ID="rptWorkflows" runat="server">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="lbWorkflow" runat="server" CssClass="btn btn-default btn-xs" CommandArgument='<%# Eval("Id") %>' CommandName="LaunchWorkflow">
                                        <%# Eval("WorkflowType.Name") %>
                                        </asp:LinkButton>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </div>
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_OnClick" CausesValidation="false" />
                        <asp:LinkButton ID="lbDeleteEventLinkage" runat="server" CausesValidation="false" CssClass="btn btn-danger" OnClick="lbDeleteEventLinkage_Click"><i class="fa fa-times"></i> Remove Event Link</asp:LinkButton>
                        <asp:LinkButton ID="lbCreateNewEventLinkage" runat="server" CausesValidation="false" CssClass="btn btn-default" Text="Add Event Link" OnClick="lbCreateNewEventLinkage_Click" />

                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_OnClick" CausesValidation="false" />
                        <div class="pull-right">
                            <asp:LinkButton ID="btnApprove" runat="server" ToolTip="Approve Reservation" CssClass="btn btn-success" OnClick="btnApprove_Click" CausesValidation="false">Approve</asp:LinkButton>
                            <asp:LinkButton ID="btnDeny" runat="server" ToolTip="Approve Reservation" CssClass="btn btn-danger" OnClick="btnDeny_Click" CausesValidation="false">Deny</asp:LinkButton>
                            <asp:LinkButton ID="btnOverride" runat="server" ToolTip="Override Reservation" CssClass="btn btn-warning" OnClick="btnOverride_Click" CausesValidation="false">Override</asp:LinkButton>
                            <asp:Literal ID="btnDownload" runat="server" />
                            <asp:LinkButton ID="btnCopy" runat="server" ToolTip="Copy Reservation" CssClass="btn btn-default btn-sm fa fa-clone" OnClick="btnCopy_Click" CausesValidation="false" />
                        </div>
                    </div>


                </asp:Panel>

                <asp:Panel ID="pnlEditDetails" runat="server">

                    <div class="row">
                        <div class="col-md-6">
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:DataTextBox ID="rtbName" runat="server" Label="Event Name" Required="true" SourceTypeName="com.centralaz.RoomManagement.Model.Reservation, com.centralaz.RoomManagement" PropertyName="Name" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockDropDownList ID="ddlReservationType" Label="Reservation Type" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlReservationType_SelectedIndexChanged" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockControlWrapper ID="rcwSchedule" runat="server" Label="Schedule">
                                        <Rock:ScheduleBuilder ID="sbSchedule" runat="server" ValidationGroup="Schedule" Required="true" OnSaveSchedule="sbSchedule_SaveSchedule" />
                                        <asp:Literal ID="lScheduleText" runat="server" />
                                    </Rock:RockControlWrapper>
                                </div>
                                <div class="col-md-3">
                                    <Rock:NumberBox ID="nbSetupTime" runat="server" NumberType="Integer" MinimumValue="0" Label="How many minutes to set up?" OnTextChanged="nbSetupTime_TextChanged" Help="The number of minutes it will take to set up the event." RequiredErrorMessage="You must supply a number for setup time (even if 0 minutes) as this will effect when others can reserve the same location/resource." />
                                </div>
                                <div class="col-md-3">
                                    <Rock:NumberBox ID="nbCleanupTime" runat="server" NumberType="Integer" MinimumValue="0" Label="How many minutes to clean up?" OnTextChanged="nbCleanupTime_TextChanged" Help="The number of minutes it will take to clean up the event." RequiredErrorMessage="You must supply a number for cleanup time (even if 0 minutes) as this will effect when others can reserve the same location/resource." />
                                </div>
                            </div>

                            <Rock:RockDropDownList ID="ddlCampus" runat="server" Label="Campus" Required="false" OnSelectedIndexChanged="ddlCampus_SelectedIndexChanged" AutoPostBack="true" />
                            <Rock:RockDropDownList ID="ddlMinistry" runat="server" Label="Ministry" Required="false" />

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:NumberBox ID="nbAttending" runat="server" NumberType="Integer" MinimumValue="0" Label="Number Attending" Required="false" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:FileUploader ID="fuSetupPhoto" runat="server" Label="Setup Photo" Help="If you'd like a special setup for your event, please upload a photo or diagram here." />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:PersonPicker ID="ppEventContact" runat="server" Label="Event Contact" EnableSelfSelection="true" OnSelectPerson="ppEventContact_SelectPerson" Help="The person who will be on-site to manage this reservation." />
                                    <Rock:PhoneNumberBox ID="pnEventContactPhone" runat="server" Label="Event Contact Phone" />
                                    <Rock:EmailBox ID="tbEventContactEmail" runat="server" Label="Event Contact Email" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:PersonPicker ID="ppAdministrativeContact" runat="server" Label="Administrative Contact" EnableSelfSelection="true" OnSelectPerson="ppAdministrativeContact_SelectPerson" Help="The person who set up this reservation." />
                                    <Rock:PhoneNumberBox ID="pnAdministrativeContactPhone" runat="server" Label="Administrative Contact Phone" />
                                    <Rock:EmailBox ID="tbAdministrativeContactEmail" runat="server" Label="Administrative Contact Email" />
                                </div>
                            </div>
                            <Rock:DataTextBox ID="rtbNote" runat="server" Label="Notes" TextMode="MultiLine" Rows="4" MaxLength="2500" SourceTypeName="com.centralaz.RoomManagement.Model.Reservation, com.centralaz.RoomManagement" PropertyName="Note" />

                            <div class="row">
                                <div class="col-md-6">
                                    <div class="form-group" id="divStatus" runat="server">
                                        <div class="form-control-static">
                                            <asp:HiddenField ID="hfApprovalState" runat="server" OnValueChanged="hfApprovalState_ValueChanged" />
                                            <asp:Panel ID="pnlReadApprovalState" runat="server" Visible="false">
                                                <label class="control-label">Status</label>
                                                <asp:Literal ID="lApprovalState" runat="server" />
                                            </asp:Panel>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="col-md-6">
                            <Rock:PanelWidget ID="wpLocations" runat="server" Title="Locations">
                                <div class="grid">
                                    <Rock:ModalAlert ID="maLocationGridWarning" runat="server" />
                                    <Rock:Grid ID="gLocations" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Location" ShowConfirmDeleteDialog="false">
                                        <Columns>
                                            <Rock:RockBoundField DataField="Location" HeaderText="Location" />
                                            <Rock:RockBoundField DataField="LocationLayout.Name" HeaderText="Layout" />
                                            <Rock:RockBoundField DataField="ApprovalState" HeaderText="Approved?" />
                                            <Rock:EditField OnClick="gLocations_Edit" />
                                            <Rock:DeleteField OnClick="gLocations_Delete" />
                                        </Columns>
                                    </Rock:Grid>
                                </div>
                            </Rock:PanelWidget>

                            <Rock:PanelWidget ID="wpResources" runat="server" Title="Resources">
                                <div class="grid">
                                    <Rock:ModalAlert ID="maResourceGridWarning" runat="server" />
                                    <Rock:Grid ID="gResources" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Resource" ShowConfirmDeleteDialog="false">
                                        <Columns>
                                            <Rock:RockBoundField DataField="Resource.Name" HeaderText="Resource" />
                                            <Rock:RockTemplateField>
                                                <ItemTemplate><em class="text-muted"><%# Convert.ToString( Eval( "Resource.Location.Name") ) == string.Empty ? "" : "(attached to " +  Eval("Resource.Location.Name") + ")" %></em></ItemTemplate>
                                            </Rock:RockTemplateField>
                                            <Rock:RockBoundField DataField="Quantity" HeaderText="Qty" />
                                            <Rock:RockBoundField DataField="ApprovalState" HeaderText="Approved?" />
                                            <Rock:EditField OnClick="gResources_Edit" />
                                            <Rock:DeleteField OnClick="gResources_Delete" />
                                        </Columns>
                                    </Rock:Grid>
                                </div>
                            </Rock:PanelWidget>
                        </div>
                    </div>
                    <Rock:DynamicPlaceholder ID="phAttributeEdits" runat="server" />
                    <asp:PlaceHolder ID="phLocationAnswers" runat="server" EnableViewState="false" />
                    <asp:PlaceHolder ID="phResourceAnswers" runat="server" EnableViewState="false" />
                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_OnClick" />
                        <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_OnClick" CausesValidation="false" />
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlEvent" runat="server" Visible="false">
                    <asp:ValidationSummary ID="vsEvent" runat="server" HeaderText="Please correct the following:" ValidationGroup="ReservationEvent" CssClass="alert alert-warning" />
                    <fieldset>
                        <asp:Panel ID="pnlNewEventSelection" runat="server">
                            <div class="row">
                                <div class="col-md-12">
                                    <Rock:Toggle ID="tglEventSelection" runat="server" ActiveButtonCssClass="btn-primary" OnText="New Event" OffText="Existing Event"
                                        OnCheckedChanged="tglEventSelection_CheckedChanged" />
                                    <hr />
                                </div>
                            </div>
                        </asp:Panel>
                        <asp:Panel ID="pnlExistingEvent" runat="server">
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:EventItemPicker ID="eipSelectedEvent" runat="server" Label="Event" Required="true" ValidationGroup="ReservationEvent" />
                                </div>
                            </div>
                        </asp:Panel>
                        <asp:Panel ID="pnlNewEvent" runat="server" Visible="false">
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockTextBox ID="tbCalendarEventName" ValidationGroup="ReservationEvent" runat="server" Label="Calendar Event Name" Required="true" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-12">
                                    <Rock:RockTextBox ID="tbEventSummary" ValidationGroup="ReservationEvent" runat="server" Label="Summary" TextMode="MultiLine" Rows="4" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-12">
                                    <Rock:HtmlEditor ID="htmlEventDescription" ValidationGroup="ReservationEvent" runat="server" Label="Description" Toolbar="Light" />
                                </div>
                            </div>

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

                                    <Rock:RockCheckBoxList ID="cblCalendars" ValidationGroup="ReservationEvent" runat="server" Label="Calendars"
                                        Help="Calendars that this item should be added to (at least one is required)."
                                        AutoPostBack="true" OnSelectedIndexChanged="cblCalendars_SelectedIndexChanged"
                                        RepeatDirection="Horizontal" Required="true" />
                                    <Rock:RockTextBox ID="tbDetailUrl" ValidationGroup="ReservationEvent" runat="server" Label="Details URL"
                                        Help="A custom url to use for showing details of the calendar item (if the default item detail page should not be used)." />
                                </div>
                                <div class="col-md-6">
                                    <Rock:ImageUploader ID="imgupPhoto" ValidationGroup="ReservationEvent" runat="server" Label="Photo" />
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-md-12">
                                    <Rock:PanelWidget ID="wpAttributes" runat="server" Title="Attribute Values">
                                        <Rock:DynamicPlaceholder ID="phEventItemAttributes" runat="server" />
                                    </Rock:PanelWidget>
                                </div>
                            </div>
                        </asp:Panel>
                        <div class="actions">
                            <asp:LinkButton ID="lbPrev_Event" runat="server" AccessKey="p" ToolTip="Alt+p" Text="Previous" CssClass="btn btn-default js-wizard-navigation" CausesValidation="false" OnClick="lbPrev_Event_Click" />
                            <asp:LinkButton ID="lbNext_Event" ValidationGroup="ReservationEvent" runat="server" AccessKey="n" Text="Next" DataLoadingText="Next" CssClass="btn btn-primary pull-right js-wizard-navigation" OnClick="lbNext_Event_Click" />
                        </div>
                    </fieldset>
                </asp:Panel>

                <asp:Panel ID="pnlEventOccurrence" runat="server" Visible="false">
                    <asp:ValidationSummary ID="vsEventOccurrence" ValidationGroup="ReservationEventOccurrence" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-warning" />

                    <fieldset>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="tbLocationDescription" ValidationGroup="ReservationEventOccurrence" runat="server" Label="Location Description" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockControlWrapper ID="rcwEventOccurrenceSchedule" runat="server" Label="Schedule">
                                    <Rock:ScheduleBuilder ID="sbEventOccurrenceSchedule" runat="server" ValidationGroup="Schedule" AllowMultiSelect="true" Required="true" OnSaveSchedule="sbEventOccurrenceSchedule_SaveSchedule" />
                                    <asp:Literal ID="lEventOccurrenceScheduleText" runat="server" />
                                </Rock:RockControlWrapper>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12">
                                <Rock:HtmlEditor ID="htmlOccurrenceNote" ValidationGroup="ReservationEventOccurrence" runat="server" Label="Occurrence Note" Toolbar="Light" />
                            </div>
                        </div>
                        <div class="actions">
                            <asp:LinkButton ID="lbPrev_EventOccurrence" runat="server" AccessKey="p" ToolTip="Alt+p" Text="Previous" CssClass="btn btn-default js-wizard-navigation" CausesValidation="false" OnClick="lbPrev_EventOccurrence_Click" />
                            <asp:LinkButton ID="lbNext_EventOccurrence" runat="server" AccessKey="n" Text="Next" DataLoadingText="Next" CssClass="btn btn-primary pull-right js-wizard-navigation" OnClick="lbNext_EventOccurrence_Click" />
                        </div>
                    </fieldset>
                </asp:Panel>

                <asp:Panel ID="pnlSummary" runat="server" Visible="false">
                    <div class="alert alert-info">
                        <div><strong>Please confirm the following changes:</strong></div>
                        <asp:PlaceHolder ID="phChanges" runat="server" />
                    </div>

                    <fieldset>
                        <div class="actions">
                            <asp:LinkButton ID="lbPrev_Summary" runat="server" AccessKey="p" ToolTip="Alt+p" Text="Previous" CssClass="btn btn-default js-wizard-navigation" CausesValidation="false" OnClick="lbPrev_Summary_Click" />
                            <asp:LinkButton ID="lbNext_Summary" runat="server" AccessKey="n" Text="Finish" DataLoadingText="Finish" CssClass="btn btn-primary pull-right js-wizard-navigation" OnClick="lbNext_Summary_Click" />
                        </div>
                    </fieldset>
                </asp:Panel>

                <asp:Panel ID="pnlFinished" runat="server" Visible="false">
                    <div class="alert alert-success">
                        <div><strong>Success</strong></div>
                        <div>
                            Event Created for
                                <asp:Label ID="lblReservationTitle" runat="server" />
                        </div>
                        <hr />

                        <ul>
                            <li id="liEventLink" runat="server">
                                <asp:HyperLink ID="hlEventDetail" runat="server" Text="View Event Detail" /></li>
                            <li id="liExternalEventLink" runat="server">
                                <asp:HyperLink ID="hlExternalEventDetails" runat="server" Text="View External Event Details" /></li>
                            <li id="liEventOccurrenceLink" runat="server">
                                <asp:HyperLink ID="hlEventOccurrence" runat="server" Text="View Event Occurrence" /></li>
                        </ul>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="lbReturnToReservation" ValidationGroup="ReservationEvent" runat="server" AccessKey="n" Text="Return to Reservation" DataLoadingText="Return to Reservation" CssClass="btn btn-primary pull-right js-wizard-navigation" OnClick="lbReturnToReservation_Click" />
                    </div>
                </asp:Panel>
            </div>
        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgReservationLocation" runat="server" Title="Select Location" OnSaveThenAddClick="dlgReservationLocation_SaveThenAddClick" OnSaveClick="dlgReservationLocation_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="ReservationLocation">
            <Content>
                <asp:HiddenField ID="hfAddReservationLocationGuid" runat="server" />
                <asp:ValidationSummary ID="valReservationLocationSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="ReservationLocation" />
                <div class="row">
                    <div class="col-md-6">
                        <CentralAZ:ScheduledLocationItemPicker ID="slpLocation" runat="server" Label="Location" Required="false" Enabled="false" AllowMultiSelect="false" OnSelectItem="slpLocation_SelectItem" ValidationGroup="ReservationLocation" />
                        <div class="col-md-12 xs-text-center" style="width: 200px;">
                            <div class="photo">
                                <asp:Literal ID="lImage" runat="server" />
                            </div>
                        </div>
                    </div>
                    <div class="col-md-6">
                        <Rock:NotificationBox ID="nbLocationConflicts" Visible="false" NotificationBoxType="Danger" runat="server" />

                        <div class="grid">
                            <Rock:Grid ID="gLocationLayouts" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Location Layout" OnRowDataBound="gLocationLayouts_RowDataBound">
                                <Columns>
                                    <Rock:RockTemplateField>
                                        <ItemTemplate>
                                            <asp:HiddenField ID="hfLayoutId" runat="server" />
                                            <asp:RadioButton ID="rbSelected" runat="server" OnCheckedChanged="rbSelected_CheckedChanged" AutoPostBack="true" />
                                        </ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:RockBoundField DataField="Name" HeaderText="Layout" />
                                    <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                    <Rock:RockTemplateField HeaderText="Photo">
                                        <ItemTemplate>
                                            <asp:Literal ID="lPhoto" runat="server" />
                                        </ItemTemplate>
                                    </Rock:RockTemplateField>
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>

                </div>
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgReservationResource" runat="server" Title="Select Resource" OnSaveThenAddClick="dlgReservationResource_SaveThenAddClick" OnSaveClick="dlgReservationResource_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="ReservationResource">
            <Content>
                <asp:HiddenField ID="hfAddReservationResourceGuid" runat="server" />
                <asp:ValidationSummary ID="valReservationResourceSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="ReservationResource" />
                <Rock:NotificationBox ID="nbResourceNote" Visible="false" NotificationBoxType="Info" runat="server" />
                <div class="row">
                    <div class="col-md-6">
                        <CentralAZ:ScheduledResourcePicker ID="srpResource" runat="server" Label="Resource" Required="false" Enabled="false" AllowMultiSelect="false" OnSelectItem="srpResource_SelectItem" ValidationGroup="ReservationResource" />
                    </div>
                    <div class="col-md-6">
                        <Rock:NumberBox ID="nbQuantity" runat="server" NumberType="Integer" MinimumValue="1" ValidationGroup="ReservationResource" Label="Quantity" />
                        <Rock:NotificationBox ID="nbResourceConflicts" Visible="false" NotificationBoxType="Warning" runat="server" />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgAudience" runat="server" ScrollbarEnabled="false" ValidationGroup="Audience" SaveButtonText="Add" OnCancelScript="clearActiveDialog();" OnSaveClick="btnSaveAudience_Click" Title="Select Audience">
            <Content>
                <asp:ValidationSummary ID="vsAudience" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="Audience" />
                <Rock:RockDropDownList ID="ddlAudience" runat="server" Label="Select Audience" ValidationGroup="Audience" Required="true" DataValueField="Id" DataTextField="Value" />
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
