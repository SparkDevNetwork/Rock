<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduledJobs.ascx.cs" Inherits="RockWeb.Blocks.Administration.ScheduledJobs" %>
<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlGrid" runat="server">
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
            <Rock:Grid ID="gScheduledJobs" runat="server" OnEditRow="gScheduledJobs_Edit" AllowSorting="true">
                <Columns>
                    <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name"/>
                    <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description"/>
                    <asp:BoundField DataField="IsActive" HeaderText="Active" SortExpression="IsActive"/>
                    <asp:BoundField DataField="Assembly" HeaderText="Assembly" SortExpression="Assembly"/>
                    <Rock:DateTimeField DataField="LastSuccessfulRunDateTime" HeaderText="Last Successful Run" SortExpression="LastSuccessfulRunDateTime"/>
                    <Rock:DateTimeField DataField="LastRunDateTime" HeaderText="Last Run Date" SortExpression="LastRunDateTime"/>
                    <asp:BoundField DataField="LastRunDurationSeconds" HeaderText="Last Run Duration" SortExpression="LastRunDurationSeconds"/>
                    <asp:BoundField DataField="LastStatus" HeaderText="Last Status" SortExpression="LastStatus"/>
                    <asp:BoundField DataField="NotificationEmails" HeaderText="Notification Emails" />
                    <asp:BoundField DataField="NotificationStatus" HeaderText="Notification Status" SortExpression="NotificationStatus"/>
                    <Rock:DeleteField OnClick="gScheduledJobs_Delete" />
                </Columns>
            </Rock:Grid>
        </asp:Panel>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false">

            <asp:HiddenField ID="hfId" runat="server" />

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-error" />

            <fieldset>
                <legend>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </legend>
            </fieldset>
            <div class="row-fluid">
                <div class="span6">
                    <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.ServiceJob, Rock" PropertyName="Name" />
                    <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.ServiceJob, Rock" PropertyName="Description" />
                    <Rock:LabeledCheckBox ID="cbActive" runat="server" LabelText="Active" />
                    <Rock:DataTextBox ID="tbAssembly" runat="server" SourceTypeName="Rock.Model.ServiceJob, Rock" PropertyName="Assembly" />
                    <Rock:DataTextBox ID="tbClass" runat="server" SourceTypeName="Rock.Model.ServiceJob, Rock" PropertyName="Class" />
                    <Rock:DataTextBox ID="tbNotificationEmails" runat="server" SourceTypeName="Rock.Model.ServiceJob, Rock" PropertyName="NotificationEmails" />
                    <Rock:LabeledDropDownList ID="drpNotificationStatus" runat="server" LabelText="Notification Status" />
                </div>
                <div class="span6">
                    <Rock:DataTextBox ID="tbCronExpression" runat="server" SourceTypeName="Rock.Model.ServiceJob, Rock" PropertyName="CronExpression" />
                </div>
            </div>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>

        <Rock:NotificationBox ID="nbWarning" runat="server" Title="Warning" NotificationBoxType="Warning" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>
