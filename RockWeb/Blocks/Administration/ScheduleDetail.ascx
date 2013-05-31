<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduleDetail.ascx.cs" Inherits="RockWeb.Blocks.Administration.ScheduleDetail" %>

<asp:UpdatePanel ID="upScheduleDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

            <asp:HiddenField ID="hfScheduleId" runat="server" />

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-error" />
            <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

            <fieldset>
                <legend>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </legend>

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <Rock:DataTextBox ID="tbScheduleName" runat="server" SourceTypeName="Rock.Model.Schedule, Rock" PropertyName="Name" />
                <Rock:DataTextBox ID="tbScheduleDescription" runat="server" SourceTypeName="Rock.Model.Schedule, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3"/>
                <Rock:LabeledCheckBox ID="cbIsShared" runat="server" LabelText="Shared" />
                <Rock:TimePicker ID="tpCheckInStartTime" runat="server" LabelText="Check-in Start Time" SourceTypeName="Rock.Model.Schedule, Rock" PropertyName="CheckInStartTime" />
                <Rock:TimePicker ID="tpCheckInEndTime" runat="server" LabelText="Check-in End Time" SourceTypeName="Rock.Model.Schedule, Rock" PropertyName="CheckInEndTime" />
                <div class="control-group controls">
                    <Rock:ScheduleBuilder ID="sbSchedule" runat="server" LabelText="Edit Schedule" OnSaveSchedule="sbSchedule_SaveSchedule" />
                    <Rock:HelpBlock ID="hbSchedulePreview" runat="server" />
                </div>
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
