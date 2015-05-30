﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EventItemDetail.ascx.cs" Inherits="RockWeb.Blocks.Calendar.EventItemDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upnlEventItemList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">
            <asp:HiddenField ID="hfEventItemId" runat="server" />

            <div class="panel panel-block">

                <div class="panel-heading clearfix">
                    <h1 class="panel-title pull-left">
                        <asp:Literal ID="lReadOnlyTitle" runat="server" />
                    </h1>

                    <div class="panel-labels">
                        <Rock:HighlightLabel ID="hlStatus" runat="server" />
                    </div>
                </div>

                <div class="panel-body">
                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    <Rock:NotificationBox ID="nbNotAllowedToEdit" runat="server" NotificationBoxType="Danger" Visible="false"
                        Text="You are not authorized to save group with the selected group type and/or parent group." />
                    <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <div id="pnlEditDetails" runat="server">

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.EventItem, Rock" PropertyName="Name" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbIsActive" runat="server" Text="Active" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-12">
                                <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.EventItem, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <div class="grid">
                                    <Rock:Grid ID="gEventItemAudiences" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Audience">
                                        <Columns>
                                            <Rock:RockBoundField DataField="Audience" HeaderText="Audiences" />
                                            <Rock:DeleteField OnClick="gEventItemAudiences_Delete" />
                                        </Columns>
                                    </Rock:Grid>
                                </div>
                            </div>
                            <div class="col-md-6">
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="tbDetailUrl" runat="server" Label="Detail URL" />
                            </div>
                            <div class="col-md-6">
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:ImageUploader ID="imgupPhoto" runat="server" Label="Photo" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBoxList ID="cblEventCalendars" runat="server" Label="Calendars" OnSelectedIndexChanged="cblEventCalendars_SelectedIndexChanged" AutoPostBack="true" DataValueField="Id" DataTextField="Name" />
                            </div>
                        </div>
                        <Rock:PanelWidget ID="wpEventItemAttributes" runat="server" Title="Item Attribute Values">
                            <asp:PlaceHolder ID="phAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>
                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="wpEventItemCampus" runat="server" Title="Campuses">
                            <div class="grid">
                                <Rock:Grid ID="gEventItemCampuses" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Location">
                                    <Columns>
                                        <Rock:RockBoundField DataField="Campus" HeaderText="Campus" />
                                        <Rock:RockBoundField DataField="Location" HeaderText="Location" />
                                        <Rock:RockBoundField DataField="Contact" HeaderText="Contact" />
                                        <Rock:RockBoundField DataField="Phone" HeaderText="Phone" />
                                        <Rock:RockBoundField DataField="Email" HeaderText="Email" />
                                        <Rock:RockBoundField DataField="Registration" HeaderText="Registration" HtmlEncode="false" />
                                        <Rock:EditField OnClick="gEventItemCampuses_Edit" />
                                        <Rock:DeleteField OnClick="gEventItemCampuses_Delete" />
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

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgEventItemCampus" runat="server" Title="Campus Select" OnSaveClick="dlgEventItemCampus_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="EventItemCampus">
            <Content>

                <asp:HiddenField ID="hfAddEventItemCampusGuid" runat="server" />

                <asp:ValidationSummary ID="valEventItemCampusSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="EventItemCampus" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlCampus" runat="server" Label="Campus" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.Campus, Rock" PropertyName="Name" />
                    </div>
                    <div class="col-md-6">
                        <Rock:PersonPicker ID="ppContact" runat="server" Label="Contact" ValidationGroup="EventItemCampus" OnSelectPerson="ppContact_SelectPerson" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbLocation" runat="server" Label="Location" ValidationGroup="EventItemCampus" Required="true" />
                    </div>
                    <div class="col-md-6">
                        <Rock:PhoneNumberBox ID="pnPhone" runat="server" Label="Phone" ValidationGroup="EventItemCampus" Required="true" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbRegistration" runat="server" Label="Registration URL" />
                    </div>
                    <div class="col-md-6">
                        <Rock:EmailBox ID="tbEmail" runat="server" Label="Email" ValidationGroup="EventItemCampus" Required="true" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <div class="grid">
                            <Rock:Grid ID="gEventItemSchedules" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Schedule">
                                <Columns>
                                    <Rock:RockBoundField DataField="Schedule" HeaderText="Schedules" />
                                    <Rock:DeleteField OnClick="gEventItemSchedules_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>
                    <div class="col-md-6">
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-12">
                        <Rock:RockTextBox ID="tbCampusNote" runat="server" Label="Campus Note" TextMode="MultiLine" />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>
        <Rock:ModalDialog ID="dlgEventItemAudiences" runat="server" ScrollbarEnabled="false" ValidationGroup="EventItemAudience" SaveButtonText="Add" OnSaveClick="btnAddEventItemAudience_Click" Title="Select Audience">
            <Content>
                <asp:HiddenField ID="hfEventItemAddAudienceGuid" runat="server" />
                <Rock:RockDropDownList ID="ddlAudience" runat="server" Label="Select Audience" ValidationGroup="EventItemAudience" />
            </Content>
        </Rock:ModalDialog>
        <Rock:ModalDialog ID="dlgEventItemSchedules" runat="server" ScrollbarEnabled="false" ValidationGroup="EventItemSchedule" SaveButtonText="Add" OnSaveClick="btnAddEventItemSchedule_Click" Title="Select Schedule">
            <Content>
                <asp:HiddenField ID="hfAddEventItemScheduleGuid" runat="server" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbEventItemSchedule" runat="server" Label="Name" ValidationGroup="EventItemSchedule" />
                    </div>
                    <div class="col-md-6">
                        <Rock:ScheduleBuilder ID="sbSchedule" runat="server" Label="Schedule" ValidationGroup="EventItemSchedule" AllowMultiSelect="true" />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>