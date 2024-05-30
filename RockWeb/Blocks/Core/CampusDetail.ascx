<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CampusDetail.ascx.cs" Inherits="RockWeb.Blocks.Core.CampusDetail" %>

<asp:UpdatePanel ID="upCampusDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <asp:HiddenField ID="hfCampusId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-building-o"></i> <asp:Literal ID="lActionTitle" runat="server" /></h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlStatus" runat="server" />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div id="pnlEditDetails" runat="server">

                    <fieldset>

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbCampusName" runat="server" SourceTypeName="Rock.Model.Campus, Rock" PropertyName="Name" ValidateRequestMode="Disabled" />
                            </div>

                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                            </div>
                        </div>

                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.Campus, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DefinedValuePicker ID="dvpCampusStatus" runat="server" Label="Status" SourceTypeName="Rock.Model.Campus, Rock" PropertyName="CampusStatusValueId" />
                                <Rock:DataTextBox ID="tbCampusCode" runat="server" SourceTypeName="Rock.Model.Campus, Rock" PropertyName="ShortCode" Label="Code" />
                                <Rock:RockDropDownList ID="ddlTimeZone" runat="server" CausesValidation="false" CssClass="input-width-xxl" Label="Time Zone" Help="The time zone you want certain time calculations of the Campus to operate in. Leave this blank to use the default Rock TimeZone." ></Rock:RockDropDownList>
                                <Rock:PersonPicker ID="ppCampusLeader" runat="server" Label="Campus Leader" />
                                <Rock:KeyValueList ID="kvlServiceTimes" runat="server" label="Service Times" KeyPrompt="Day" ValuePrompt="Time" Help="A list of days and times that this campus has services." />
                            </div>
                            <div class="col-md-6">
                                <Rock:DefinedValuePicker ID="dvpCampusType" runat="server" Label="Type" SourceTypeName="Rock.Model.Campus, Rock" PropertyName="CampusTypeValueId" />
                                <Rock:UrlLinkBox ID="urlCampus" runat="server" Label="URL" />
                                <Rock:PhoneNumberBox ID="pnbPhoneNumber" runat="server" Label="Phone Number" />
                                <Rock:LocationPicker ID="lpLocation" runat="server" AllowedPickerModes="Named" Required="true" Label="Location" Help="Select a Campus Location" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <div class="form-group data-text-box ">
                                    <asp:Label ID="lblCampusSchedules" runat="server" Text="Campus Schedules" CssClass="control-label" AssociatedControlID="gCampusSchedules" />
                                    <div class="control-wrapper">

                                        <div class="grid">
                                            <Rock:Grid ID="gCampusSchedules" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Schedule">
                                                <Columns>
                                                    <Rock:RockBoundField DataField="Schedule" HeaderText="Schedule" />
                                                    <Rock:RockBoundField DataField="ScheduleType" HeaderText="Type" />
                                                    <Rock:EditField OnClick="gCampusSchedules_Edit" />
                                                    <Rock:DeleteField OnClick="gCampusSchedules_Delete" />
                                                </Columns>
                                            </Rock:Grid>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <div class="col-md-6">
                                <div class="form-group data-text-box ">
                                    <asp:Label ID="lblCampusTopics" runat="server" Text="Topics" CssClass="control-label" AssociatedControlID="gCampusTopics" />
                                    <div class="control-wrapper">

                                        <div class="grid">
                                            <Rock:Grid ID="gCampusTopics" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Topic">
                                                <Columns>
                                                    <Rock:RockBoundField DataField="TopicType" HeaderText="Type" />
                                                    <Rock:RockBoundField DataField="Email" HeaderText="Email" />
                                                    <Rock:BoolField DataField="IsPublic" HeaderText="Public" />
                                                    <Rock:EditField OnClick="gCampusTopics_Edit" />
                                                    <Rock:DeleteField OnClick="gCampusTopics_Delete" />
                                                </Columns>
                                            </Rock:Grid>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <div class="attributes">
                                    <Rock:AttributeValuesContainer ID="avcAttributes" runat="server" />
                                </div>
                            </div>
                        </div>

                    </fieldset>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </div>

                <fieldset id="fieldsetViewDetails" runat="server">

                    <p class="description">
                        <asp:Literal ID="lDescription" runat="server"></asp:Literal>
                    </p>

                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lMainDetailsLeft" runat="server" />
                        </div>
                        <div class="col-md-6">
                            <asp:Literal ID="lMainDetailsRight" runat="server" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" data-shortcut-key="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" />
                    </div>

                </fieldset>

            </div>

        </asp:Panel>

        <!-- CampusSchedule Modal Dialog -->
        <Rock:ModalDialog ID="dlgSchedule" runat="server" Title="Campus Schedule" SaveButtonText="Ok" OnSaveClick="dlgSchedule_SaveClick"  ValidationGroup="Location">
            <Content>
                <asp:HiddenField ID="hfCampusScheduleGuid" runat="server" />
                <asp:ValidationSummary ID="valSummaryCampusSchedule" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="CampusSchedule" />

                <Rock:SchedulePicker ID="spCampusSchedule" runat="server" Label="Schedule" Required="true" ValidationGroup="CampusSchedule" />
                <Rock:DefinedValuePicker id="dvpScheduleType" runat="server" Label="Schedule Type" Required="true" ValidationGroup="CampusSchedule" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgTopic" runat="server" Title="Campus Topic" SaveButtonText="Ok" OnSaveClick="dlgTopic_SaveClick"  ValidationGroup="CampusTopic">
            <Content>
                <asp:HiddenField ID="hfCampusTopicGuid" runat="server" />
                <asp:ValidationSummary ID="valSummaryCampusTopic" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="CampusTopic" />

                <Rock:DefinedValuePicker ID="dvpTopicType" runat="server" Label="Topic Type" Required="true" ValidationGroup="CampusTopic" Help="A campus can only have one instance of a topic entry." />
                <asp:CustomValidator runat="server"
                            ID="cvTopicType"
                            ErrorMessage="A campus can only have one instance of a topic entry."
                            ControlToValidate="dvpTopicType"
                            OnServerValidate="cvTopicType_ServerValidate"
                            ValidationGroup="CampusTopic" />
                <Rock:EmailBox ID="ebEmail" runat="server" Label="Email" Required="true" ValidationGroup="CampusTopic" />
                <Rock:RockCheckBox ID="cbIsPublic" runat="server" Label="Public" />
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
