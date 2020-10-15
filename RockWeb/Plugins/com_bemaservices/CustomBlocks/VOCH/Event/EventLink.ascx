﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EventLink.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.CustomBlocks.VOCH.Event.EventLink" %>

<%@ Register TagPrefix="BEMA" Assembly="com.bemaservices.RoomManagement" Namespace="com.bemaservices.RoomManagement.Web.UI.Controls" %>
<script type="text/javascript">
    $(document).ready(function () {
        contentSlug.init({
            contentChannelItem: '#<%=hfId.ClientID %>',
            contentSlug: '#<%=hfSlug.ClientID %>',
            SaveSlug: {
                restUrl: '<%=ResolveUrl( "~/api/ContentChannelItemSlugs/SaveContentSlug" ) %>',
                restParams: '/' + ($('#<%=hfId.ClientID%>').val() || 0) + "/{slug}/{contentChannelItemSlugId?}",
            },
            UniqueSlug: {
                restUrl: '<%=ResolveUrl( "~/api/ContentChannelItemSlugs/GetUniqueContentSlug" ) %>',
                restParams: "/" + ($('#<%=hfId.ClientID%>').val() || 0) + "/{slug}"
            },
            RemoveSlug: {
                restUrl: '<%=ResolveUrl( "~/api/ContentChannelItemSlugs" ) %>',
                restParams: '/{id}'
            },
            txtTitle: '#<%=tbInternalEventName.ClientID %>'
        });
    });

</script>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbNotice" runat="server" Visible="false" NotificationBoxType="Danger" />
        <Rock:NotificationBox ID="nbWarning" runat="server" Visible="false" NotificationBoxType="Warning" />

        <asp:Panel ID="pnlView" runat="server">
            <asp:HiddenField ID="hfSlug" runat="server" />
            <asp:HiddenField ID="hfId" runat="server" />

            <asp:Literal ID="lLavaOverview" runat="server" />
            <asp:Literal ID="lLavaOutputDebug" runat="server" />

            <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

            <div class="well">
                <div class="row">
                    <div class="col-md-12">

                        <h2 style="margin-top: 0px;">Event Information</h2>
                        <hr />

                        <div class="col-md-4">
                            <Rock:RockTextBox ID="tbEventName" runat="server" Label="Public Event Name" Required="true"></Rock:RockTextBox>
                            <Rock:RockTextBox ID="tbInternalEventName" runat="server" Label="Internal Event Name" Required="true"></Rock:RockTextBox>
                            <Rock:RockTextBox ID="tbEventDescription" runat="server" Label="Promotional Summary" Required="true" TextMode="MultiLine" Rows="4" MaxLength="200" ShowCountDown="true" />

                        </div>


                        <div class="col-md-4">
                            <Rock:CampusPicker ID="cpCampus" runat="server" Label="Campus" Required="true" />
                            <Rock:PersonPicker ID="ppPrimaryContact" runat="server" Label="Primary Contact" Required="true" EnableSelfSelection="true" />
                        </div>

                        <div class="col-md-3">
                            <Rock:DatePicker ID="dpEventEndDate" runat="server" Label="The Date This Will Be Removed from Event Link List" Required="true" />
                            <Rock:ImageUploader ID="iuImage" runat="server" Label="Event Image" />
                        </div>

                    </div>
                </div>
            </div>

            <div class="row">
                <div class="col-md-12">

                    <div class="panel panel-block">
                        <div class="panel-heading">
                            <h6 style="margin-top: 0px;">Does this Event need a room? </h6>
                            <div class="pull-right">
                                <Rock:Toggle ID="tEventRoom" runat="server" OnText="Yes" OffText="No" OnCssClass="btn-success" OffCssClass="btn-danger" OnCheckedChanged="tEventRoom_CheckedChanged" />
                            </div>
                        </div>

                        <asp:Panel ID="pnlEventRoom" runat="server" Visible="false">
                            <div class="panel-body">
                                <div class="row">

                                    <div class="col-md-12">
                                        <Rock:NotificationBox ID="nbEventRoom" runat="server" Visible="false" NotificationBoxType="Info" />
                                    </div>

                                    <Rock:NotificationBox ID="nbLocationConflicts" Visible="false" NotificationBoxType="Danger" runat="server" />

                                    <asp:HiddenField ID="hfAddReservationLocationGuid" runat="server" />
                                    <asp:HiddenField ID="hfReservationId" runat="server" />

                                    <div class="col-md-4">
                                        <Rock:RockDropDownList ID="ddlReservationType" Label="Reservation Type" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlReservationType_SelectedIndexChanged" />
                                    </div>

                                    <div class="col-md-4">
                                        <Rock:RockControlWrapper ID="rcwSchedule" runat="server" Label="Schedule" Required="true">
                                            <Rock:ScheduleBuilder ID="sbSchedule" runat="server" ValidationGroup="Schedule" Required="true" OnSaveSchedule="sbSchedule_SaveSchedule" />
                                            <asp:Literal ID="lScheduleText" runat="server" />
                                        </Rock:RockControlWrapper>
                                    </div>

                                    <div class="col-md-4">
                                        <BEMA:ScheduledLocationItemPicker ID="slpLocation" runat="server" Label="Location" Required="true" Enabled="false" AllowMultiSelect="false" OnSelectItem="slpLocation_SelectItem" />
                                    </div>

                                </div>

                                <asp:PlaceHolder ID="phReservationAttributes" runat="server"></asp:PlaceHolder>

                            </div>
                        </asp:Panel>
                    </div>

                    <div class="panel panel-block">
                        <div class="panel-heading">
                            <h6 style="margin-top: 0px;">Does this Event need an event registration? </h6>
                            <div class="pull-right">
                                <Rock:Toggle ID="tEventReg" runat="server" OnText="Yes" OffText="No" OnCssClass="btn-success" OffCssClass="btn-danger" OnCheckedChanged="tEventReg_CheckedChanged" />
                            </div>

                        </div>

                        <asp:Panel ID="pnlEventReg" runat="server" Visible="false">
                            <div class="panel-body">
                                <div class="row">

                                    <div class="col-md-12">
                                        <Rock:NotificationBox ID="nbEventReg" runat="server" Visible="false" NotificationBoxType="Info" />
                                    </div>
                                    <div class="col-md-6">
                                        <div class="row">
                                            <div class="col-md-6">
                                                <Rock:RockDropDownList ID="ddlTemplate" runat="server" Label="Registration Template" Required="true" />
                                            </div>

                                            <div class="col-md-6">
                                                <Rock:CategoryPicker ID="cpCategory" runat="server" Label="Category" Required="true" EntityTypeId="234" />
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="col-md-6">
                                                <Rock:DateTimePicker ID="dpEventRegStartDate" runat="server" Label="Registration Start Date" Required="true" />
                                            </div>

                                            <div class="col-md-6">
                                                <Rock:DateTimePicker ID="dpEventRegEndDate" runat="server" Label="Registration End Date" Required="true" />
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="col-md-6">
                                                <Rock:CurrencyBox ID="nbInstanceCost" runat="server" Label="Instance Cost" />
                                            </div>

                                            <div class="col-md-6">
                                                <Rock:NumberBox ID="nbMaximumRegistrants" runat="server" Label="Maximum Registrants" />
                                            </div>
                                        </div>
                                        <Rock:HtmlEditor ID="htmlAdditionalNoteDetails" runat="server" Label="Additional Options" Help="This will be emailed to an Event Admin, and they will update the template to include these options." />

                                    </div>
                                    <div class="col-md-6">
                                        <Rock:HtmlEditor ID="htmlRegistrationInstructions" runat="server" Label="Registration Instructions" />

                                        <Rock:HtmlEditor ID="htmlAdditionalConfirmationDetails" runat="server" Label="Additional Confirmation Details" />
                                        <Rock:HtmlEditor ID="htmlAdditionalReminderDetails" runat="server" Label="Additional Reminder Details" />

                                    </div>
                                </div>
                            </div>
                        </asp:Panel>
                    </div>

                    <div class="panel panel-block">
                        <div class="panel-heading">
                            <h6 style="margin-top: 0px;">Does this Event need a calendar item? </h6>
                            <div class="pull-right">
                                <Rock:Toggle ID="tEventCalendar" runat="server" OnText="Yes" OffText="No" OnCssClass="btn-success" OffCssClass="btn-danger" OnCheckedChanged="tEventCalendar_CheckedChanged" />
                            </div>
                        </div>

                        <asp:Panel ID="pnlEventCalendar" runat="server" Visible="false">
                            <div class="panel-body">
                                <div class="row">

                                    <div class="col-md-12">
                                        <Rock:NotificationBox ID="nbCalendar" runat="server" Visible="false" NotificationBoxType="Info" />
                                    </div>

                                    <div class="col-md-4">

                                        <Rock:RockControlWrapper ID="rcwSchedule2" runat="server" Label="Schedule" Required="true">
                                            <Rock:ScheduleBuilder ID="sbEventSchedule2" runat="server" AllowMultiSelect="true" ValidationGroup="Schedule" Required="true" OnSaveSchedule="sbSchedule_SaveSchedule2" />
                                            <asp:Literal ID="lScheduleText2" runat="server" />
                                        </Rock:RockControlWrapper>

                                    </div>
                                    <div class="col-md-4">

                                        <Rock:RockCheckBoxList ID="cblCalendars" runat="server" Label="Calendars" AutoPostBack="true" Help="Calendars that this item should be added to (at least one is required)."
                                            RepeatDirection="Horizontal" Required="true" OnSelectedIndexChanged="cblCalendars_SelectedIndexChanged" />
                                    </div>
                                    <div class="col-md-4">
                                        <Rock:CampusesPicker ID="cpCampuses" runat="server" Label="Campuses" Visible="true" ForceVisible="true" />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:RockTextBox ID="tbLocationDescription" runat="server" Label="Location Description for Off-Campus Events" />
                                    </div>
                                    <!--
                                    <div class="col-md-4">
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
                                    </div>
                                    -->
                                    <div class="col-md-4">
                                        <Rock:DynamicPlaceholder ID="phEventAttributes" runat="server" />
                                    </div>
                                </div>
                            </div>
                        </asp:Panel>

                    </div>

                    <asp:Panel ID="pnlContentItem" runat="server" CssClass="panel panel-block" Visible="false">
                        <div class="panel-heading">
                            <h6 style="margin-top: 0px;">Does this Event need to be promoted?</h6>
                            <Rock:Toggle ID="tAnnoucement" runat="server" OnText="Yes" OffText="No" OnCssClass="btn-success" OffCssClass="btn-danger" CssClass="pull-right" OnCheckedChanged="tAnnoucement_CheckedChanged" />
                        </div>

                        <asp:Panel ID="pnlAnnoucement" runat="server" Visible="false">

                            <div class="panel-body">
                                <div class="row">
                                    <div class="col-md-7">
                                        <div class="form-row">
                                            <div class="col-sm-6">
                                                <Rock:DatePicker ID="dpStart" runat="server" Label="Start" Required="true" Visible="false" />
                                                <Rock:DateTimePicker ID="dtpStart" runat="server" Label="Start" Required="true" />
                                            </div>
                                            <div class="col-sm-6">
                                                <Rock:DatePicker ID="dpExpire" runat="server" Label="Expire" Required="false" Visible="false" />
                                                <Rock:DateTimePicker ID="dtpExpire" runat="server" Label="Expire" />
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-md-5">
                                    </div>
                                </div>

                                <Rock:DynamicPlaceholder ID="phPromotionAttributes" runat="server" />
                            </div>

                        </asp:Panel>


                    </asp:Panel>

                    <div class="panel panel-block">
                        <div class="panel-heading">
                            <h6 style="margin-top: 0px;">Vox Kids</h6>
                            <div class="pull-right">
                                <Rock:Toggle ID="tVoxKids" runat="server" OnText="Yes" OffText="No" OnCssClass="btn-success" OffCssClass="btn-danger" OnCheckedChanged="tVoxKids_CheckedChanged" />
                            </div>
                        </div>

                        <asp:Panel ID="pnlVoxKids" runat="server" Visible="false">
                            <div class="panel-body">
                                <div class="row">
                                    <div class="col-md-12">
                                        <Rock:NotificationBox ID="nbVoxKids" runat="server" Visible="false" NotificationBoxType="Info" />
                                        <Rock:DynamicPlaceholder ID="phVoxKids" runat="server" />

                                    </div>
                                </div>
                            </div>
                        </asp:Panel>
                    </div>

                    <div class="panel panel-block">
                        <div class="panel-heading">
                            <h6 style="margin-top: 0px;">Vox Music</h6>
                            <div class="pull-right">
                                <Rock:Toggle ID="tVoxMusic" runat="server" OnText="Yes" OffText="No" OnCssClass="btn-success" OffCssClass="btn-danger" OnCheckedChanged="tVoxMusic_CheckedChanged" />
                            </div>
                        </div>

                        <asp:Panel ID="pnlVoxMusic" runat="server" Visible="false">
                            <div class="panel-body">
                                <div class="row">
                                    <div class="col-md-12">
                                        <Rock:NotificationBox ID="nbVoxMusic" runat="server" Visible="false" NotificationBoxType="Info" />
                                        <Rock:DynamicPlaceholder ID="phVoxMusic" runat="server" />

                                    </div>
                                </div>
                            </div>
                        </asp:Panel>
                    </div>

                    <div class="panel panel-block">
                        <div class="panel-heading">
                            <h6 style="margin-top: 0px;">Production Needs</h6>
                            <div class="pull-right">
                                <Rock:Toggle ID="tProduction" runat="server" OnText="Yes" OffText="No" OnCssClass="btn-success" OffCssClass="btn-danger" OnCheckedChanged="tProduction_CheckedChanged" />
                            </div>
                        </div>

                        <asp:Panel ID="pnlProduction" runat="server" Visible="false">
                            <div class="panel-body">
                                <div class="row">
                                    <div class="col-md-12">
                                        <Rock:NotificationBox ID="nbProduction" runat="server" Visible="false" NotificationBoxType="Info" />
                                        <Rock:DynamicPlaceholder ID="phVoxProduction" runat="server" />
                                    </div>
                                </div>
                            </div>
                        </asp:Panel>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-primary" OnClick="btnSave_Click">Save</asp:LinkButton>
                    </div>

                </div>

            </div>



        </asp:Panel>

        <asp:Panel ID="pnlResult" runat="server" Visible="false">
            <asp:Literal ID="lResult" runat="server" />
            <asp:Literal ID="lResultDebug" runat="server" />
        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgAudience" runat="server" ScrollbarEnabled="false" ValidationGroup="Audience" SaveButtonText="Add" OnCancelScript="clearActiveDialog();" OnSaveClick="btnSaveAudience_Click" Title="Select Audience">
            <Content>
                <asp:ValidationSummary ID="vsAudience" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="Audience" />
                <Rock:RockDropDownList ID="ddlAudience" runat="server" Label="Select Audience" ValidationGroup="Audience" Required="true" DataValueField="Id" DataTextField="Value" />
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
