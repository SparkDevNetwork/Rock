<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduledJobDetail.ascx.cs" Inherits="RockWeb.Blocks.Administration.ScheduledJobDetail" %>
<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false" CssClass="panel panel-block">
            
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-clock-o"></i> <asp:Literal ID="lActionTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <asp:HiddenField ID="hfId" runat="server" />
                
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                
                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.ServiceJob, Rock" PropertyName="Name" TabIndex="1" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbActive" runat="server" Label="Active" Text="Yes" TabIndex="2" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-12">
                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.ServiceJob, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" TabIndex="3" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlNotificationStatus" runat="server" Label="Notification Status" TabIndex="5" />
                        <Rock:DataTextBox ID="tbNotificationEmails" runat="server" SourceTypeName="Rock.Model.ServiceJob, Rock" PropertyName="NotificationEmails" TabIndex="4" />
                        <Rock:NotificationBox ID="nbJobTypeError" runat="server" NotificationBoxType="Danger" Dismissable="true" />
                        <Rock:RockDropDownList ID="ddlJobTypes" runat="server" Label="Job Type" OnSelectedIndexChanged="ddlJobTypes_SelectedIndexChanged" AutoPostBack="true" TabIndex="6" Required="true" />
                        <Rock:DynamicControlsPanel ID="phAttributes" runat="server" ></Rock:DynamicControlsPanel>
                        <Rock:DynamicControlsPanel ID="phAttributesReadOnly" runat="server" Visible="false" ></Rock:DynamicControlsPanel>
                    </div>
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbCronExpression" runat="server" SourceTypeName="Rock.Model.ServiceJob, Rock" PropertyName="CronExpression" 
                            Help="Add a valid cron expression. Need help? Try <a href='http://www.cronmaker.com' target='_blank'>CronMaker</a>." TabIndex="7" />
                        <Rock:RockLiteral ID="lCronExpressionDesc" Label="Cron Description" runat="server" />
                        <Rock:RockLiteral ID="lLastStatusMessage" Label="Last Status Message" runat="server" />
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
