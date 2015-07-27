<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CalendarItemOccurrenceDetail.ascx.cs" Inherits="RockWeb.Blocks.Event.CalendarItemOccurrenceDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upnlEventItemList" runat="server">
    <ContentTemplate>

        <div class="wizard">
    
             <div class="wizard-item complete">
                <asp:LinkButton ID="lbCalendars" runat="server" OnClick="lbCalendarsDetail_Click" CausesValidation="false" >
                    <%-- Placeholder needed for bug. See: http://stackoverflow.com/questions/5539327/inner-image-and-text-of-asplinkbutton-disappears-after-postback--%>
                    <asp:PlaceHolder runat="server">
                        <div class="wizard-item-icon">
                            <i class="fa fa-fw fa-calendar"></i>
                        </div>
                        <div class="wizard-item-label">
                            Event Calendars
                        </div>
                    </asp:PlaceHolder>
                </asp:LinkButton>
            </div>
    
            <div class="wizard-item complete">
                <asp:LinkButton ID="lbCalendarDetail" runat="server" OnClick="lbCalendarDetail_Click" CausesValidation="false" >
                    <asp:PlaceHolder runat="server">
                        <div class="wizard-item-icon">
                            <i class="fa fa-fw fa-calendar"></i>
                        </div>
                        <div class="wizard-item-label">
                            Calendar
                        </div>
                    </asp:PlaceHolder>
                </asp:LinkButton>
            </div>
    
            <div class="wizard-item complete">
                <asp:LinkButton ID="lbCalendarItem" runat="server" OnClick="lbCalendarItem_Click" CausesValidation="false" >
                    <asp:PlaceHolder runat="server">
                        <div class="wizard-item-icon">
                            <i class="fa fa-fw fa-calendar-o"></i>
                        </div>
                        <div class="wizard-item-label">
                            Calendar Item
                        </div>
                    </asp:PlaceHolder>
                </asp:LinkButton>
            </div>
    
            <div class="wizard-item active">
                <div class="wizard-item-icon">
                    <i class="fa fa-fw fa-building-o"></i>
                </div>
                <div class="wizard-item-label">
                    Event Occurrence
                </div>
            </div>

        </div>

        <asp:Panel ID="pnlDetails" runat="server" CssClass="panel panel-block">

            <asp:HiddenField ID="hfEventItemOccurrenceId" runat="server" />

            <div class="panel-heading clearfix">
                <h1 class="panel-title"><asp:Literal ID="lActionTitle" runat="server" /></h1>
            </div>

            <div class="panel-body">

                <asp:ValidationSummary ID="vsSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div class="row">

                    <div class="col-md-6">

                        <Rock:RockDropDownList ID="ddlCampus" runat="server" Label="Campus" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.Campus, Rock" PropertyName="Name" />

                        <Rock:RockControlWrapper ID="rcwSchedules" runat="server" Label="Schedule">
                            <asp:HiddenField ID="hfSchedules" runat="server" />
                            <Rock:HiddenFieldValidator ID="hfvSchedules" runat="server" Display="None" ErrorMessage="At least one Schedule is required." ControlToValidate="hfSchedules" ValidationGroup="CampusDetails" />
                            <div class="grid">
                                <Rock:Grid ID="gSchedules" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Schedule" ShowHeader="false">
                                    <Columns>
                                        <Rock:RockBoundField DataField="Schedule" />
                                        <Rock:RockBoundField DataField="Details" />
                                        <Rock:EditField OnClick="gSchedules_Edit" />
                                        <Rock:DeleteField OnClick="gSchedules_Delete" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </Rock:RockControlWrapper>

                        <Rock:RockLiteral ID="lRegistration" runat="server" Label="Registration Instance - Group" CssClass="margin-b-none" />
                        <asp:LinkButton ID="lbEditRegistration" runat="server" CssClass="btn btn-default btn-xs margin-b-md" OnClick="lbEditRegistration_Click" ><i class="fa fa-pencil"></i> Edit</asp:LinkButton>
                        <asp:LinkButton ID="lbDeleteRegistration" runat="server" CssClass="btn btn-danger btn-xs margin-b-md" OnClick="lbDeleteRegistration_Click" ><i class="fa fa-times"></i> Remove</asp:LinkButton>
                        <asp:LinkButton ID="lbCreateNewRegistration" runat="server" CssClass="btn btn-primary btn-xs margin-b-md" Text="Add New Registration Instance" OnClick="lbCreateNewRegistration_Click" />
                        <asp:LinkButton ID="lbLinkToExistingRegistration" runat="server" CssClass="btn btn-default btn-xs margin-b-md" Text="Use Existing Registration Instance" OnClick="lbLinkToExistingRegistration_Click" />

                    </div>

                    <div class="col-md-6">

                        <Rock:RockTextBox ID="tbLocation" runat="server" Label="Location Description" ValidationGroup="CampusDetails" />
                        <Rock:PersonPicker ID="ppContact" runat="server" Label="Contact" ValidationGroup="CampusDetails" OnSelectPerson="ppContact_SelectPerson" />
                        <Rock:PhoneNumberBox ID="pnPhone" runat="server" Label="Phone" ValidationGroup="CampusDetails" />
                        <Rock:EmailBox ID="tbEmail" runat="server" Label="Email" ValidationGroup="CampusDetails" />

                    </div>

                </div>

                <Rock:HtmlEditor ID="htmlCampusNote" runat="server" Label="Campus Note" />
                
                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgNewLinkage" runat="server" Title="New Registration Instance" SaveButtonText="OK" OnSaveClick="dlgNewLinkage_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="NewLinkage">
            <Content>
                <asp:ValidationSummary ID="vsNewLinkage" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="NewLinkage" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlNewLinkageTemplate" runat="server" Label="Registration Template" ValidationGroup="NewLinkage" Required="true" />
                    </div>
                    <div class="col-md-6">
                        <Rock:GroupPicker ID="gpNewLinkageGroup" runat="server" Label="Group" ValidationGroup="NewLinkage" />
                    </div>
                </div>
                <Rock:RegistrationInstanceEditor ID="rieNewLinkage" runat="server" ValidationGroup="NewLinkage" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgExistingLinkage" runat="server" Title="Existing Registration Instance" SaveButtonText="OK" OnSaveClick="dlgExistingLinkage_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="ExistingLinkage">
            <Content>
                <asp:ValidationSummary ID="vsExistingLinkage" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="ExistingLinkage" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlExistingLinkageTemplate" runat="server" Label="Registration Template" ValidationGroup="ExistingLinkage" 
                            AutoPostBack="true" OnSelectedIndexChanged="ddlExistingLinkageTemplate_SelectedIndexChanged" CausesValidation="false" 
                            Required="true" />
                        <Rock:RockDropDownList ID="ddlExistingLinkageInstance" runat="server" Label="Registration Instance" ValidationGroup="ExistingLinkage"
                            Required="true" />
                    </div>
                    <div class="col-md-6">
                        <Rock:GroupPicker ID="gpExistingLinkageGroup" runat="server" Label="Group" ValidationGroup="ExistingLinkage" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbExistingLinkagePublicName" runat="server" Label="Public Name" Required="true" ValidationGroup="ExistingLinkage" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbExistingLinkageUrlSlug" runat="server" Label="URL Slug" ValidationGroup="ExistingLinkage" />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgSchedule" runat="server" Title="Schedule" OnSaveClick="dlgSchedule_SaveClick" SaveButtonText="OK" OnCancelScript="clearActiveDialog();" ValidationGroup="Schedule">
            <Content>
                <asp:ValidationSummary ID="vsSchedule" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="Schedule" />
                <asp:HiddenField ID="hfScheduleGuid" runat="server" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbScheduleName" runat="server" Label="Schedule Label" ValidationGroup="Schedule" Required="true" />
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
