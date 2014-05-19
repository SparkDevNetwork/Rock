<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RecordingDetail.ascx.cs" Inherits="RockWeb.Plugins.com_CcvOnline.CommandCenter.RecordingDetail" %>

<asp:UpdatePanel ID="upRecordings" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

            <asp:HiddenField ID="hfRecordingId" runat="server" />

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-error" />
            <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

            <fieldset>
                <legend>
                    <asp:Literal ID="lActionTitle" runat="server"></asp:Literal></legend>
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <Rock:CampusPicker ID="cpCampus" runat="server" />
                <Rock:DataTextBox ID="tbApp" runat="server" SourceTypeName="com.CcvOnline.CommandCenter.Model.Recording, com.CcvOnline.CommandCenter" PropertyName="App" />
                <Rock:DatePicker ID="dpDate" runat="server" SourceTypeName="com.CcvOnline.CommandCenter.Model.Recording, com.CcvOnline.CommandCenter" PropertyName="Date" />
                <Rock:DataTextBox ID="tbStream" runat="server" SourceTypeName="com.CcvOnline.CommandCenter.Model.Recording, com.CcvOnline.CommandCenter" PropertyName="StreamName" />
                <Rock:DataTextBox ID="tbLabel" runat="server" SourceTypeName="com.CcvOnline.CommandCenter.Model.Recording, com.CcvOnline.CommandCenter" PropertyName="Label" />
                <Rock:DataTextBox ID="tbRecording" runat="server" SourceTypeName="com.CcvOnline.CommandCenter.Model.Recording, com.CcvOnline.CommandCenter" PropertyName="RecordingName" CssClass="input-xlarge" />
                <Rock:RockTextBox ID="lStarted" runat="server" LabelText="Started" />
                <Rock:RockTextBox ID="lStartResponse" runat="server" LabelText="Start Response" />
                <Rock:RockTextBox ID="lStopped" runat="server" LabelText="Stopped" />
                <Rock:RockTextBox ID="lStopResponse" runat="server" LabelText="Stop Response" />
                <Rock:RockCheckBox ID="cbStartRecording" runat="server" LabelText="Start Recording" />
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>

