<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EventRegistrationWizard.ascx.cs" Inherits="RockWeb.Blocks.Event.EventRegistrationWizard" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }

    Sys.Application.add_load( function () {
        $('.js-follow-status').tooltip();
    });

    Sys.WebForms.PageRequestManager.getInstance().add_endRequest( function () {
        $(document).ready(function () {

            var tbRegistrationNameId = '<%=tbRegistrationName.ClientID %>';
            if ($('#' + tbRegistrationNameId).length) {

                var tbPublicNameNameId = '<%=tbPublicName.ClientID %>';
                var previousRegistrationName = $('#' + tbRegistrationNameId).val();

                $('#' + tbRegistrationNameId).keyup(function () {
                    var registrationNameValue = $('#' + tbRegistrationNameId).val();
                    var publicNameValue = $('#' + tbPublicNameNameId).val();

                    if ((publicNameValue == '') || (publicNameValue == previousRegistrationName))
                    {
                        $('#' + tbPublicNameNameId).val(registrationNameValue);
                    }
                    previousRegistrationName = registrationNameValue;
                });
            }

        });
    });
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlLavaInstructions" runat="server" Visible="false" >
            <asp:Literal ID="lLavaInstructions" runat="server" />
        </asp:Panel>

        <div>
            <div class="panel panel-block">
                <asp:Panel ID="pnlInitiate_Header" runat="server" CssClass="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-clipboard"></i>
                        Event Registration Wizard - Choose a Template</h1>
                </asp:Panel>
                <asp:Panel ID="pnlRegistration_Header" runat="server" CssClass="panel-heading" Visible="false">
                    <h1 class="panel-title"><i class="fa fa-file-o"></i>
                        Event Registration Wizard - Registration</h1>
                </asp:Panel>
                <asp:Panel ID="pnlGroup_Header" runat="server" CssClass="panel-heading" Visible="false">
                    <h1 class="panel-title"><i class="fa fa-users"></i>
                        Event Registration Wizard - Group</h1>
                </asp:Panel>
                <asp:Panel ID="pnlEvent_Header" runat="server" CssClass="panel-heading" Visible="false">
                    <h1 class="panel-title"><i class="fa fa-calendar-check-o"></i>
                        Event Registration Wizard - Event</h1>
                </asp:Panel>
                <asp:Panel ID="pnlEventOccurrence_Header" runat="server" CssClass="panel-heading" Visible="false">
                    <h1 class="panel-title"><i class="fa fa-clock-o"></i>
                        Event Registration Wizard - Event Occurrence</h1>
                </asp:Panel>
                <asp:Panel ID="pnlSummary_Header" runat="server" CssClass="panel-heading" Visible="false">
                    <h1 class="panel-title"><i class="fa fa-list-ul"></i>
                        Event Registration Wizard - Summary</h1>
                </asp:Panel>
                <asp:Panel ID="pnlFinished_Header" runat="server" CssClass="panel-heading" Visible="false">
                    <h1 class="panel-title"><i class="fa fa-check"></i>
                        Event Registration Wizard - Finished</h1>
                </asp:Panel>

                <asp:Panel ID="pnlWizard" runat="server" CssClass="wizard" Visible="false">

                    <div id="divTemplate" runat="server" class="wizard-item">
                        <asp:LinkButton ID="lbTemplate" runat="server" OnClick="lbTemplate_Click" CausesValidation="false" >
                            <%-- Placeholder needed for bug. See: http://stackoverflow.com/questions/5539327/inner-image-and-text-of-asplinkbutton-disappears-after-postback--%>
                            <asp:PlaceHolder runat="server">
                                <div class="wizard-item-icon">
                                    <i class="fa fa-fw fa-clipboard"></i>
                                </div>
                                <div class="wizard-item-label">
                                    Registration Template
                                </div>
                            </asp:PlaceHolder>
                        </asp:LinkButton>
                    </div>

                    <div id="divRegistration" runat="server" class="wizard-item">
                        <asp:LinkButton ID="lbRegistration" runat="server" OnClick="lbRegistration_Click" CausesValidation="false" >
                            <asp:PlaceHolder runat="server">
                                <div class="wizard-item-icon">
                                    <i class="fa fa-fw fa-file-o"></i>
                                </div>
                                <div class="wizard-item-label">
                                    Registration
                                </div>
                            </asp:PlaceHolder>
                        </asp:LinkButton>
                    </div>

                    <div id="divGroup" runat="server" class="wizard-item">
                        <asp:LinkButton ID="lbGroup" runat="server" OnClick="lbGroup_Click" CausesValidation="false" >
                            <asp:PlaceHolder runat="server">
                                <div class="wizard-item-icon">
                                    <i class="fa fa-fw fa-users"></i>
                                </div>
                                <div class="wizard-item-label">
                                    Group
                                </div>
                            </asp:PlaceHolder>
                        </asp:LinkButton>
                    </div>

                    <div id="divEvent" runat="server" class="wizard-item">
                        <asp:LinkButton ID="lbEvent" runat="server" OnClick="lbEvent_Click" CausesValidation="false" >
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
                        <asp:LinkButton ID="lbEventOccurrence" runat="server" OnClick="lbEventOccurrence_Click" CausesValidation="false" Enabled="false" >
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

                    <asp:Panel ID="pnlInitiate" runat="server">
                        <asp:ValidationSummary ID="vsInitiate" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-warning" />

                        <fieldset>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:DataDropDownList ID="ddlTemplate" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.RegistrationTemplate, Rock" AppendDataBoundItems="true"
                                        PropertyName="Name" Label="Registration Template" AutoPostBack="true" OnSelectedIndexChanged="ddlTemplate_SelectedIndexChanged" Required="true">
                                        <asp:ListItem Text="" Value="" />
                                    </Rock:DataDropDownList>
                                    <asp:Literal ID="lTemplateDescription" runat="server" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockDropDownList ID="ddlCampus" runat="server" Label="Campus" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.Campus, Rock" PropertyName="Name" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-12">
                                    <Rock:PersonPicker ID="ppContact" runat="server" Label="Contact" EnableSelfSelection="true" OnSelectPerson="ppContact_SelectPerson" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:EmailBox ID="tbContactEmail" runat="server" Label="Contact Email" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:PhoneNumberBox ID="tbContactPhone" runat="server" Label="Contact Phone" />
                                </div>
                            </div>
                            <div class="actions">
                                <asp:LinkButton ID="lbNext_Initiate" runat="server" AccessKey="n" Text="Next" DataLoadingText="Next" CssClass="btn btn-primary pull-right js-wizard-navigation" OnClick="lbNext_Initiate_Click" />
                            </div>
                        </fieldset>
                    </asp:Panel>

                    <asp:Panel ID="pnlRegistration" runat="server" Visible="false">
                        <asp:ValidationSummary ID="vsRegistration" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-warning" />

                        <fieldset>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockTextBox ID="tbRegistrationName" runat="server" Label="Registration Name" Required="true" OnTextChanged="tbRegistrationName_TextChanged" />
                                    <asp:HiddenField ID="hfPreviousName" runat="server" Value="" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:DateTimePicker ID="dtpRegistrationStarts" runat="server" Label="Registration Starts" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:DateTimePicker ID="dtpRegistrationEnds" runat="server" Label="Registration Ends" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:DateTimePicker ID="dtpReminderDate" runat="server" Label="Send Reminder Date" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:NumberBox ID="numbMaximumAttendees" runat="server" Label="Maximum Attendees" />
                                </div>
                            </div>

                            <asp:Panel ID="pnlCosts" runat="server">
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:CurrencyBox ID="cbCost" runat="server" Label="Cost" Help="The cost per registrant." />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:CurrencyBox ID="cbMinimumInitialPayment" runat="server" Label="Minimum Initial Payment"
                                            Help="The minimum amount required per registrant. Leave value blank if full amount is required." />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:AccountPicker ID="apAccount" runat="server" Label="Account" Required="true" />
                                    </div>
                                </div>
                            </asp:Panel>

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockTextBox ID="tbPublicName" runat="server" Label="Public Name" Required="true" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockTextBox ID="tbSlug" runat="server" Label="Slug" />
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-md-12">
                                    <Rock:PanelWidget ID="pwRegistrationCustomization" runat="server" Title="Registration Customization">
                                        <Rock:HtmlEditor ID="htmlRegistrationInstructions" runat="server" Label="Registration Instructions" Height="100" Help="These instructions will appear at the beginning of the registration process when selecting how many registrants for the registration." Toolbar="Light" />
                                        <Rock:HtmlEditor ID="htmlReminderDetails" runat="server" Label="Reminder Details" Height="100" Help="These reminder details will be included in the reminder notification." Toolbar="Light" />
                                        <Rock:HtmlEditor ID="htmlConfirmationDetails" runat="server" Label="Confirmation Details" Height="100" Help="These confirmation details will be appended to those from the registration template when displayed at the end of the registration process." Toolbar="Light" />
                                    </Rock:PanelWidget>
                                </div>
                            </div>
                            <div class="actions">
                                <asp:LinkButton ID="lbPrev_Registration" runat="server" AccessKey="p" ToolTip="Alt+p" Text="Previous" CssClass="btn btn-default js-wizard-navigation" CausesValidation="false" OnClick="lbPrev_Registration_Click"  />
                                <asp:LinkButton ID="lbNext_Registration" runat="server" AccessKey="n" Text="Next" DataLoadingText="Next" CssClass="btn btn-primary pull-right js-wizard-navigation" OnClick="lbNext_Registration_Click" />
                            </div>
                        </fieldset>

                    </asp:Panel>

                    <asp:Panel ID="pnlGroup" runat="server" Visible="false">
                        <asp:ValidationSummary ID="vsGroup" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-warning" />

                        <fieldset>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockTextBox ID="tbGroupName" runat="server" Label="New Group Name" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:GroupPicker ID="gpParentGroup" runat="server" Label="Parent Group" OnSelectItem="gpParentGroup_SelectItem" />
                                </div>
                            </div>
                            <asp:Panel ID="pnlCheckinOptions" runat="server" Visible="false" CssClass="row">
                                <div class="col-md-6">
                                    <Rock:LocationPicker ID="lpGroupLocation" runat="server" AllowedPickerModes="Named" Label="Location" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:SchedulePicker ID="spGroupLocationSchedule" runat="server" Label="Schedule" AllowMultiSelect="true" />
                                </div>
                            </asp:Panel>
                            <div class="row">
                                <div class="col-md-12">
                                    <Rock:NotificationBox ID="nbNotAuthorized" runat="server" NotificationBoxType="Danger" Visible="false" Heading="Not Authorized">
                                        You are not authorized to modify the selected group.  Please choose another parent group.
                                    </Rock:NotificationBox>
                                    <Rock:NotificationBox ID="nbNotPermitted" runat="server" NotificationBoxType="Danger" Visible="false" Heading="Group Type Not Permitted">
                                    </Rock:NotificationBox>
                                </div>
                            </div>
                            <div class="actions">
                                <asp:LinkButton ID="lbPrev_Group" runat="server" AccessKey="p" ToolTip="Alt+p" Text="Previous" CssClass="btn btn-default js-wizard-navigation" CausesValidation="false" OnClick="lbPrev_Group_Click"  />
                                <asp:LinkButton ID="lbNext_Group" runat="server" AccessKey="n" Text="Next" DataLoadingText="Next" CssClass="btn btn-primary pull-right js-wizard-navigation" OnClick="lbNext_Group_Click" />
                            </div>
                        </fieldset>
                    </asp:Panel>

                    <asp:Panel ID="pnlEvent" runat="server" Visible="false">
                        <asp:ValidationSummary ID="vsEvent" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-warning" />

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
                                        <Rock:EventItemPicker ID="eipSelectedEvent" runat="server"  Label="Event" Required="true" />
                                    </div>
                                </div>
                            </asp:Panel>
                            <asp:Panel ID="pnlNewEvent" runat="server" Visible="false">
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:RockTextBox ID="tbCalendarEventName" runat="server" Label="Calendar Event Name" Required="true" />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-12">
                                        <Rock:RockTextBox ID="tbEventSummary" runat="server" Label="Summary" TextMode="MultiLine" Rows="4" />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-12">
                                        <Rock:HtmlEditor ID="htmlEventDescription" runat="server" Label="Description" Toolbar="Light" />
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

                                        <Rock:RockCheckBoxList ID="cblCalendars" runat="server" Label="Calendars"
                                            Help="Calendars that this item should be added to (at least one is required)."
                                            OnSelectionChanged="cblCalendars_SelectionChanged" AutoPostBack="true"
                                            RepeatDirection="Horizontal" Required="true" />
                                        <Rock:RockTextBox ID="tbDetailUrl" runat="server" Label="Details URL"
                                            Help="A custom url to use for showing details of the calendar item (if the default item detail page should not be used)."/>
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:ImageUploader ID="imgupPhoto" runat="server" Label="Photo" />
                                    </div>
                                </div>

                                <div class="row">
                                    <div class="col-md-12">
                                        <Rock:PanelWidget ID="wpAttributes" runat="server" Title="Attribute Values">
                                            <Rock:DynamicPlaceHolder ID="phAttributes" runat="server" />
                                        </Rock:PanelWidget>
                                    </div>
                                </div>
                            </asp:Panel>
                            <div class="actions">
                                <asp:LinkButton ID="lbPrev_Event" runat="server" AccessKey="p" ToolTip="Alt+p" Text="Previous" CssClass="btn btn-default js-wizard-navigation" CausesValidation="false" OnClick="lbPrev_Event_Click"  />
                                <asp:LinkButton ID="lbNext_Event" runat="server" AccessKey="n" Text="Next" DataLoadingText="Next" CssClass="btn btn-primary pull-right js-wizard-navigation" OnClick="lbNext_Event_Click" />
                            </div>
                        </fieldset>
                    </asp:Panel>

                    <asp:Panel ID="pnlEventOccurrence" runat="server" Visible="false">
                        <asp:ValidationSummary ID="vsEventOccurrence" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-warning" />

                        <fieldset>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockTextBox ID="tbLocationDescription" runat="server" Label="Location Description" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockControlWrapper ID="rcwSchedule" runat="server" Label="Schedule" >
                                        <Rock:ScheduleBuilder ID="sbSchedule" runat="server" ValidationGroup="Schedule" AllowMultiSelect="true" Required="true" OnSaveSchedule="sbSchedule_SaveSchedule" />
                                        <asp:Literal ID="lScheduleText" runat="server" />
                                    </Rock:RockControlWrapper>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-12">
                                    <Rock:HtmlEditor ID="htmlOccurrenceNote" runat="server" Label="Occurrence Note" Toolbar="Light" />
                                </div>
                            </div>
                            <div class="actions">
                                <asp:LinkButton ID="lbPrev_EventOccurrence" runat="server" AccessKey="p" ToolTip="Alt+p" Text="Previous" CssClass="btn btn-default js-wizard-navigation" CausesValidation="false" OnClick="lbPrev_EventOccurrence_Click"  />
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
                                <asp:LinkButton ID="lbPrev_Summary" runat="server" AccessKey="p" ToolTip="Alt+p" Text="Previous" CssClass="btn btn-default js-wizard-navigation" CausesValidation="false" OnClick="lbPrev_Summary_Click"  />
                                <asp:LinkButton ID="lbNext_Summary" runat="server" AccessKey="n" Text="Finish" DataLoadingText="Finish" CssClass="btn btn-primary pull-right js-wizard-navigation" OnClick="lbNext_Summary_Click" />
                            </div>
                        </fieldset>
                    </asp:Panel>

                    <asp:Panel ID="pnlFinished" runat="server" Visible="false">
                        <div class="alert alert-success">
                            <div><strong>Success</strong></div>
                            <div>
                                Registration created for
                                <asp:Label ID="lblEventRegistrationTitle" runat="server" />
                            </div>
                            <hr />

                            <ul>
                                <li id="liRegistrationLink" runat="server"><asp:HyperLink ID="hlRegistrationInstance" runat="server" Text="View Registration Instance" /></li>
                                <li id="liGroupLink" runat="server"><asp:HyperLink ID="hlGroup" runat="server" Text="View Group" /></li>
                                <li id="liEventLink" runat="server"><asp:HyperLink ID="hlEventDetail" runat="server" Text="View Event Detail" /></li>
                                <li id="liEventOccurrenceLink" runat="server"><asp:HyperLink ID="hlEventOccurrence" runat="server" Text="View Event Occurrence" /></li>
                            </ul>
                        </div>
                    </asp:Panel>

                </div>
            </div>
        </div>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />
        <Rock:ModalDialog ID="dlgAudience" runat="server" ScrollbarEnabled="false" ValidationGroup="Audience" SaveButtonText="Add" OnCancelScript="clearActiveDialog();" OnSaveClick="btnSaveAudience_Click" Title="Select Audience">
            <Content>
                <asp:ValidationSummary ID="vsAudience" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="Audience" />
                <Rock:RockDropDownList ID="ddlAudience" runat="server" Label="Select Audience" ValidationGroup="Audience" Required="true" DataValueField="Id" DataTextField="Value" />
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>

