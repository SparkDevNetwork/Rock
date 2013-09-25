<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduledJobDetail.ascx.cs" Inherits="RockWeb.Blocks.Administration.ScheduledJobDetail" %>
<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server" Visible="false">

            <asp:HiddenField ID="hfId" runat="server" />

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-error" />

            <fieldset>
                <legend>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </legend>

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                
                <div class="row-fluid">
                    <div class="span6">
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.ServiceJob, Rock" PropertyName="Name" />
                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.ServiceJob, Rock" PropertyName="Description" />
                        <Rock:RockCheckBox ID="cbActive" runat="server" Label="Active" />
                        <Rock:DataTextBox ID="tbAssembly" runat="server" SourceTypeName="Rock.Model.ServiceJob, Rock" PropertyName="Assembly" />
                        <Rock:DataTextBox ID="tbClass" runat="server" SourceTypeName="Rock.Model.ServiceJob, Rock" PropertyName="Class" />
                        <Rock:DataTextBox ID="tbNotificationEmails" runat="server" SourceTypeName="Rock.Model.ServiceJob, Rock" PropertyName="NotificationEmails" />
                        <Rock:RockDropDownList ID="drpNotificationStatus" runat="server" Label="Notification Status" />
                    </div>
                    <div class="span6">
                        <Rock:DataTextBox ID="tbCronExpression" runat="server" SourceTypeName="Rock.Model.ServiceJob, Rock" PropertyName="CronExpression" />
                    </div>
                </div>

            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
