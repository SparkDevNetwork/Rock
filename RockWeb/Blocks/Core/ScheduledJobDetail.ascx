<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduledJobDetail.ascx.cs" Inherits="RockWeb.Blocks.Administration.ScheduledJobDetail" %>
<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false" CssClass="panel panel-block">
            
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-clock-o"></i>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </h1>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <asp:HiddenField ID="hfId" runat="server" />
                
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                
                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.ServiceJob, Rock" PropertyName="Name"/>
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbActive" runat="server" Label="Active" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-12">
                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.ServiceJob, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlNotificationStatus" runat="server" Label="Notification Status" />
                        <Rock:RockTextBox ID="tbNotificationEmails" runat="server" Label="Notification Emails" Help="Additional email addresses that the notification email should be sent to for this job. Emails are sent using the 'Job Notification' system email template. If there are recipients defined in the template, it will send a job notification to those, too. <span class='tip tip-lava'></span>"/>
                        <Rock:NotificationBox ID="nbJobTypeError" runat="server" NotificationBoxType="Danger" Dismissable="true" />
                        <Rock:RockDropDownList ID="ddlJobTypes" runat="server" Label="Job Type" EnhanceForLongLists="true" OnSelectedIndexChanged="ddlJobTypes_SelectedIndexChanged" AutoPostBack="true"  Required="true" />
                        <Rock:AttributeValuesContainer ID="avcAttributes" runat="server" />
                        <Rock:AttributeValuesContainer ID="avcAttributesReadOnly" runat="server" Visible="false" />
                    </div>
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbCronExpression" runat="server" SourceTypeName="Rock.Model.ServiceJob, Rock" PropertyName="CronExpression" 
                            Help="Add a valid cron expression. Need help? Try <a href='http://www.cronmaker.com' target='_blank' rel='noopener noreferrer'>CronMaker</a>.<br>Examples:<br>Daily at 2:15am: <em>0 15 2 1/1 * ? *</em><br>Every Monday and Friday at 4:30pm: <em>0 30 16 ? * MON,FRI *</em>" AutoPostBack="true" OnTextChanged="tbCronExpression_TextChanged"  />
                        <Rock:RockLiteral ID="lCronExpressionDesc" Label="Cron Description" runat="server" />
                        <Rock:NumberBox ID="nbHistoryCount" runat="server" Label="Job History Count" Help="The number of job history records to keep for this job instance." CssClass="input-width-lg" MinimumValue="0" />
                        <Rock:RockLiteral ID="lLastStatusMessage" Label="Last Status Message" runat="server" />
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
