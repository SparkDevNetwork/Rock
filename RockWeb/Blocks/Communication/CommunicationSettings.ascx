<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CommunicationSettings.ascx.cs" Inherits="RockWeb.Blocks.Communication.CommunicationSettings" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-wrench"></i> Communication Settings
                </h1>
            </div>
            <div class="panel-body">

                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlApprovalEmailTemplate" runat="server" Label="Communication Approval Email Template" Help="The system communication to use for emailing approval notifications when a new message is pending approval." Required="true" />
                    </div>
                </div>
                <div class="actions margin-t-lg">
                    <Rock:BootstrapButton ID="btnSave" runat="server" CssClass="btn btn-primary" AccessKey="s" OnClick="btnSave_Click" Text="Save" DataLoadingText="&lt;i class='fa fa-refresh fa-spin'&gt;&lt;/i&gt; Saving"
                        CompletedText="Success" CompletedMessage="<div class='margin-t-md alert alert-success'>Changes have been saved.</div>" CompletedDuration="3"></Rock:BootstrapButton>
                </div>

            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
