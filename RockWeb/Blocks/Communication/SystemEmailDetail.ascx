<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SystemEmailDetail.ascx.cs" Inherits="RockWeb.Blocks.Communication.SystemEmailDetail" %>

<asp:UpdatePanel ID="upSettings" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfEmailTemplateId" runat="server" />

        <div class="banner">
            <h1><asp:Literal ID="lActionTitle" runat="server" /></h1>
        </div>

        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

        <div class="row">
            <div class="col-md-6">
                <Rock:DataTextBox ID="tbTitle" runat="server" SourceTypeName="Rock.Model.SystemEmail, Rock" PropertyName="Title" />
                <Rock:DataTextBox ID="tbFromName" runat="server" SourceTypeName="Rock.Model.SystemEmail, Rock" PropertyName="FromName" Label="From Name" />
                <Rock:DataTextBox ID="tbFrom" runat="server" SourceTypeName="Rock.Model.SystemEmail, Rock" PropertyName="From" Label="From Address" />
                <Rock:DataTextBox ID="tbTo" runat="server" SourceTypeName="Rock.Model.SystemEmail, Rock" PropertyName="To" />
            </div>
            <div class="col-md-6">
                <Rock:DataTextBox ID="tbCategory" runat="server" SourceTypeName="Rock.Model.SystemEmail, Rock" PropertyName="Category" />
                <Rock:DataTextBox ID="tbCc" runat="server" SourceTypeName="Rock.Model.SystemEmail, Rock" PropertyName="Cc" />
                <Rock:DataTextBox ID="tbBcc" runat="server" SourceTypeName="Rock.Model.SystemEmail, Rock" PropertyName="Bcc" />
            </div>
        </div>

        <Rock:DataTextBox ID="tbSubject" runat="server" SourceTypeName="Rock.Model.SystemEmail, Rock" PropertyName="Subject" />
        <Rock:CodeEditor ID="tbBody" EditorHeight="500" Label="Message Body" EditorMode="Liquid" EditorTheme="Rock" runat="server" SourceTypeName="Rock.Model.SystemEmail, Rock" PropertyName="Body" />

        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Danger" Visible="false" />

        <div class="actions">
            <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click"></asp:LinkButton>
            <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_Click" CausesValidation="false"></asp:LinkButton>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
