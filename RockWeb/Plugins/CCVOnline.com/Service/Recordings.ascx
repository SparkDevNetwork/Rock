<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Recordings.ascx.cs" Inherits="RockWeb.Plugins.CCVOnline.Service.Recordings" %>

<asp:UpdatePanel ID="upRecordings" runat="server">
<ContentTemplate>

    <asp:Panel ID="pnlList" runat="server">
        
        <Rock:Grid ID="gRecordings" runat="server" EmptyDataText="No Recordings Found">
            <Columns>
                <asp:BoundField HeaderText="Campus" DataField="Campus" />
                <asp:BoundField HeaderText="Date" DataField="Date" DataFormatString="{0:MM/dd/yy}" />
                <asp:BoundField HeaderText="Stream" DataField="StreamName" />
                <asp:BoundField HeaderText="Label" DataField="Label" />
                <asp:BoundField HeaderText="Recording" DataField="RecordingName" />
                <asp:BoundField HeaderText="Started" DataField="StartTime" DataFormatString="{0:MM/dd/yy hh:mm:ss tt}" />
                <asp:BoundField HeaderText="Stopped" DataField="StopTime" DataFormatString="{0:MM/dd/yy hh:mm:ss tt}"  />
                <asp:BoundField HeaderText="Length" DataField="Length" />
                <asp:TemplateField><ItemTemplate><asp:LinkButton ID="lbStart" runat="server" Text="Start" CssClass="start-recording" CommandName="START" CommandArgument='<%# Eval("Id") %>'></asp:LinkButton></ItemTemplate></asp:TemplateField>
                <asp:TemplateField><ItemTemplate><asp:LinkButton ID="lbStop" runat="server" Text="Stop" CssClass="stop-recording" CommandName="STOP" CommandArgument='<%# Eval("Id") %>'></asp:LinkButton></ItemTemplate></asp:TemplateField>
                <Rock:EditField OnClick="gRecordings_Edit" />
                <Rock:DeleteField OnClick="gRecordings_Delete" />
            </Columns>
        </Rock:Grid>

    </asp:Panel>

    <asp:Panel ID="pnlDetails" runat="server" Visible="false">
    
        <asp:HiddenField ID="hfRecordingId" runat="server" />

        <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="failureNotification"/>

        <fieldset>
            <legend><asp:Literal ID="lAction" runat="server"></asp:Literal> Recording</legend>
            <Rock:DataDropDownList ID="ddlCampus" runat="server" LabelText="Campus" SourceTypeName="Rock.Com.CCVOnline.Service.Recording, Rock.Com.CCVOnline" PropertyName="CampusId"/>
            <Rock:DataTextBox ID="tbApp" runat="server" SourceTypeName="Rock.Com.CCVOnline.Service.Recording, Rock.Com.CCVOnline" PropertyName="App" />
            <Rock:DataTextBox ID="tbDate" runat="server" SourceTypeName="Rock.Com.CCVOnline.Service.Recording, Rock.Com.CCVOnline" PropertyName="Date" />
            <Rock:DataTextBox ID="tbStream" runat="server" SourceTypeName="Rock.Com.CCVOnline.Service.Recording, Rock.Com.CCVOnline" PropertyName="StreamName" />
            <Rock:DataTextBox ID="tbLabel" runat="server" SourceTypeName="Rock.Com.CCVOnline.Service.Recording, Rock.Com.CCVOnline" PropertyName="Label" />
            <Rock:DataTextBox ID="tbRecording" runat="server" SourceTypeName="Rock.Com.CCVOnline.Service.Recording, Rock.Com.CCVOnline" PropertyName="RecordingName" />
            <Rock:LabeledText ID="lStarted" runat="server" LabelText="Started"  />
            <Rock:LabeledText ID="lStartResponse" runat="server" LabelText="Start Response" />
            <Rock:LabeledText ID="lStopped" runat="server" LabelText="Stopped" />
            <Rock:LabeledText ID="lStopResponse" runat="server" LabelText="Stop Response" />
            <Rock:LabeledCheckBox ID="cbStartRecording" runat="server" LabelText="Start Recording" />
        </fieldset>

        <div class="actions">
            <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn primary" onclick="btnSave_Click" />
            <asp:LinkButton id="btnCancel" runat="server" Text="Cancel" CssClass="btn secondary" CausesValidation="false" OnClick="btnCancel_Click" />
        </div>

    </asp:Panel>

    <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error" Visible="false" />

</ContentTemplate>
</asp:UpdatePanel>

