<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SystemEmailDetail.ascx.cs" Inherits="RockWeb.Blocks.Communication.SystemEmailDetail" %>

<asp:UpdatePanel ID="upSettings" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfEmailTemplateId" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-envelope"></i> <asp:Literal ID="lActionTitle" runat="server" /></h1>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbTitle" runat="server" SourceTypeName="Rock.Model.SystemEmail, Rock" PropertyName="Title" />
                        <Rock:DataTextBox ID="tbFromName" runat="server" SourceTypeName="Rock.Model.SystemEmail, Rock" PropertyName="FromName" Label="From Name" Help="<small><span class='tip tip-lava'></span></small>" />
                        <Rock:DataTextBox ID="tbFrom" runat="server" SourceTypeName="Rock.Model.SystemEmail, Rock" PropertyName="From" Label="From Address" Help="<small><span class='tip tip-lava'></span></small>"/>
                        <Rock:DataTextBox ID="tbTo" runat="server" SourceTypeName="Rock.Model.SystemEmail, Rock" PropertyName="To" />
                    </div>
                    <div class="col-md-6">
                        <Rock:CategoryPicker ID="cpCategory" runat="server" Required="true" Label="Category" EntityTypeName="Rock.Model.SystemEmail" />
                        <Rock:DataTextBox ID="tbCc" runat="server" SourceTypeName="Rock.Model.SystemEmail, Rock" PropertyName="Cc" Help="<small><span class='tip tip-lava'></span></small>" />
                        <Rock:DataTextBox ID="tbBcc" runat="server" SourceTypeName="Rock.Model.SystemEmail, Rock" PropertyName="Bcc" Help="<small><span class='tip tip-lava'></span></small>" />
                    </div>
                </div>

                <Rock:DataTextBox ID="tbSubject" runat="server" SourceTypeName="Rock.Model.SystemEmail, Rock" PropertyName="Subject" Help="<small><span class='tip tip-lava'></span></small>" />
                <Rock:CodeEditor ID="tbBody" EditorHeight="500" Label="Message Body" EditorMode="Lava" EditorTheme="Rock" runat="server" SourceTypeName="Rock.Model.SystemEmail, Rock" PropertyName="Body" Required="true" />

                <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Danger" Visible="false" />

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s"  Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click"></asp:LinkButton>
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_Click" CausesValidation="false"></asp:LinkButton>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
