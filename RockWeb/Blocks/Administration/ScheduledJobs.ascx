<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduledJobs.ascx.cs"
    Inherits="RockWeb.Blocks.Administration.ScheduledJobs" %>
<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlGrid" runat="server">
            <Rock:Grid ID="grdScheduledJobs" runat="server" OnEditRow="grdScheduledJobs_Edit">
                <Columns>                  
                    <asp:BoundField DataField="Name" HeaderText="Name" />
                    <asp:BoundField DataField="Description" HeaderText="Description" />
                    <asp:BoundField DataField="IsActive" HeaderText="Active" />
                    <asp:BoundField DataField="Assembly" HeaderText="Assembly" />                
                    <asp:BoundField DataField="LastSuccessfulRunDateTime" HeaderText="Last Successful Run" />
                    <asp:BoundField DataField="LastRunDateTime" HeaderText="Last Run Date" />
                    <asp:BoundField DataField="LastRunDurationSeconds" HeaderText="Last Run Duration" />
                    <asp:BoundField DataField="LastStatus" HeaderText="Last Status" />
                    <asp:BoundField DataField="NotificationEmails" HeaderText="Notification Emails" />
                    <asp:BoundField DataField="NotificationStatus" HeaderText="Notification Status" />
                    <Rock:DeleteField OnClick="grdScheduledJobs_Delete" />
                </Columns>
            </Rock:Grid>
        </asp:Panel>
        <asp:Panel ID="pnlDetails" runat="server" Visible="false" Style="padding: 20px;">
            <asp:HiddenField ID="hfId" runat="server" />
            <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following"
                CssClass="alert-message block-message error" />
            <div class="row">
                <div class="span6">
                    <fieldset>
                        <legend>&nbsp;</legend>
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.ServiceJob, Rock"
                            PropertyName="Name" />
                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.ServiceJob, Rock"
                            PropertyName="Description" />
                        <Rock:LabeledCheckBox ID="cbActive" runat="server" LabelText="Active" />
                        <Rock:DataTextBox ID="tbAssembly" runat="server" SourceTypeName="Rock.Model.ServiceJob, Rock"
                            PropertyName="Assembly" />
                        <Rock:DataTextBox ID="tbClass" runat="server" SourceTypeName="Rock.Model.ServiceJob, Rock"
                            PropertyName="Class" />
                        <Rock:DataTextBox ID="tbNotificationEmails" runat="server" SourceTypeName="Rock.Model.ServiceJob, Rock"
                            PropertyName="NotificationEmails" />
                        <Rock:LabeledDropDownList ID="drpNotificationStatus" runat="server" LabelText="Notification Status" />
                    </fieldset>
                </div>
                <div class="span6">
                    <fieldset>
                        <legend>&nbsp;</legend>
                        <Rock:DataTextBox ID="tbCronExpression" runat="server" SourceTypeName="Rock.Model.ServiceJob, Rock"
                            PropertyName="CronExpression" />
                    </fieldset>
                </div>
            </div>
            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn"
                    CausesValidation="false" OnClick="btnCancel_Click" />
            </div>
        </asp:Panel>
        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error"
            Visible="false" />
    </ContentTemplate>
</asp:UpdatePanel>
