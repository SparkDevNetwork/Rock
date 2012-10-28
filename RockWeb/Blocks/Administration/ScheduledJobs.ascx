<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduledJobs.ascx.cs"
    Inherits="RockWeb.Blocks.Administration.ScheduledJobs" %>
<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlGrid" runat="server">
            <Rock:Grid ID="grdScheduledJobs" runat="server" EmptyDataText="No Scheduled Jobs Found">
                <Columns>
                    <%--  <asp:TemplateField HeaderText="Name">
                        <ItemTemplate>
                            <asp:Label ID="lblName" runat="server" Text='<%# Eval("Name") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Description">
                        <ItemTemplate>
                            <asp:Label ID="lblName" runat="server" Text='<%# Eval("Description") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Active">
                        <ItemTemplate>
                            <asp:Label ID="lblIsActive" runat="server" Text='<%# Eval("IsActive") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Assembly">
                        <ItemTemplate>
                            <asp:Label ID="lblAssembly" runat="server" Text='<%# Eval("Assemby") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Class">
                        <ItemTemplate>
                            <asp:Label ID="lblClass" runat="server" Text='<%# Eval("Class") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="CRON">
                        <ItemTemplate>
                            <asp:Label ID="lblCronExpression" runat="server" Text='<%# Eval("CronExpression") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Last Successful Run">
                        <ItemTemplate>
                            <asp:Label ID="lblLastSuccessfulRun" runat="server" Text='<%# Eval("LastSuccessfulRun") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Last Run Date">
                        <ItemTemplate>
                            <asp:Label ID="lblLastRunDate" runat="server" Text='<%# Eval("LastRunDate") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Last Run Duration">
                        <ItemTemplate>
                            <asp:Label ID="lblLastRunDuration" runat="server" Text='<%# Eval("LastRunDuration") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Last Status">
                        <ItemTemplate>
                            <asp:Label ID="lblLastStatus" runat="server" Text='<%# Eval("LastStatus") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Notification Emails">
                        <ItemTemplate>
                            <asp:Label ID="lblNotificationEmails" runat="server" Text='<%# Eval("NotificationEmails") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Notification Status">
                        <ItemTemplate>
                            <asp:Label ID="lblNotificationStatus" runat="server" Text='<%# Eval("NotificationStatus") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>         --%>
                    <asp:BoundField DataField="Name" HeaderText="Name" />
                    <asp:BoundField DataField="Description" HeaderText="Description" />
                    <asp:BoundField DataField="IsActive" HeaderText="Active" />
                    <asp:BoundField DataField="Assemby" HeaderText="Assembly" />
                   <%-- <asp:BoundField DataField="Class" HeaderText="Class" />
                    <asp:BoundField DataField="CronExpression" HeaderText="CRON" />--%>
                    <asp:BoundField DataField="LastSuccessfulRun" HeaderText="Last Successful Run" />
                    <asp:BoundField DataField="LastRunDate" HeaderText="Last Run Date" />
                    <asp:BoundField DataField="LastRunDuration" HeaderText="Last Run Duration" />
                    <asp:BoundField DataField="LastStatus" HeaderText="Last Status" />
                    <asp:BoundField DataField="NotificationEmails" HeaderText="Notification Emails" />
                    <asp:BoundField DataField="NotificationStatus" HeaderText="Notification Status" />
                    <Rock:EditField OnClick="grdScheduledJobs_Edit" />
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
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Util.Job, Rock"
                            PropertyName="Name" />
                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Util.Job, Rock"
                            PropertyName="Description" />
                        <Rock:LabeledCheckBox ID="cbActive" runat="server" LabelText="Active" />
                        <Rock:DataTextBox ID="tbAssembly" runat="server" SourceTypeName="Rock.Util.Job, Rock"
                            PropertyName="Assemby" />
                        <Rock:DataTextBox ID="tbClass" runat="server" SourceTypeName="Rock.Util.Job, Rock"
                            PropertyName="Class" />
                        <Rock:DataTextBox ID="tbNotificationEmails" runat="server" SourceTypeName="Rock.Util.Job, Rock"
                            PropertyName="NotificationEmails" />
                        <Rock:LabeledDropDownList ID="drpNotificationStatus" runat="server" LabelText="Notification Status" />
                    </fieldset>
                </div>
                <div class="span6">
                    <fieldset>
                        <legend>&nbsp;</legend>
                        <Rock:DataTextBox ID="tbCronExpression" runat="server" SourceTypeName="Rock.Util.Job, Rock"
                            PropertyName="CronExpression" />
                    </fieldset>
                </div>
            </div>
            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn secondary"
                    CausesValidation="false" OnClick="btnCancel_Click" />
            </div>
        </asp:Panel>
        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error"
            Visible="false" />
    </ContentTemplate>
</asp:UpdatePanel>
