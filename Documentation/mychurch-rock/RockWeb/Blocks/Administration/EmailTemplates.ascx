<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EmailTemplates.ascx.cs" Inherits="RockWeb.Blocks.Administration.EmailTemplates" %>

<asp:UpdatePanel ID="upSettings" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlList" runat="server">

            <Rock:GridFilter ID="rFilter" runat="server">
                <Rock:LabeledDropDownList ID="ddlCategoryFilter" runat="server" LabelText="Category" />
            </Rock:GridFilter>
            <Rock:Grid ID="gEmailTemplates" runat="server" AllowSorting="true" OnRowSelected="gEmailTemplates_Edit">
                <Columns>
                    <asp:BoundField DataField="Category" HeaderText="Category" SortExpression="Category" />
                    <asp:BoundField DataField="Title" HeaderText="Title" SortExpression="Title" />
                    <asp:BoundField DataField="From" HeaderText="From" SortExpression="From" />
                    <asp:BoundField DataField="Subject" HeaderText="Subject" SortExpression="Subject" />
                    <Rock:DeleteField OnClick="gEmailTemplates_Delete" />
                </Columns>
            </Rock:Grid>

        </asp:Panel>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false" CssClass="admin-details">

            <asp:HiddenField ID="hfEmailTemplateId" runat="server" />

            <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error" />

            <fieldset>
                <legend>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </legend>
                <Rock:DataTextBox ID="tbCategory" runat="server" SourceTypeName="Rock.Model.EmailTemplate, Rock" PropertyName="Category" />
                <Rock:DataTextBox ID="tbTitle" runat="server" SourceTypeName="Rock.Model.EmailTemplate, Rock" PropertyName="Title" />
                <Rock:DataTextBox ID="tbFrom" runat="server" SourceTypeName="Rock.Model.EmailTemplate, Rock" PropertyName="From" />
                <Rock:DataTextBox ID="tbTo" runat="server" SourceTypeName="Rock.Model.EmailTemplate, Rock" PropertyName="To" />
                <Rock:DataTextBox ID="tbCc" runat="server" SourceTypeName="Rock.Model.EmailTemplate, Rock" PropertyName="Cc" />
                <Rock:DataTextBox ID="tbBcc" runat="server" SourceTypeName="Rock.Model.EmailTemplate, Rock" PropertyName="Bcc" />
                <Rock:DataTextBox ID="tbSubject" runat="server" SourceTypeName="Rock.Model.EmailTemplate, Rock" PropertyName="Subject" />
                <Rock:DataTextBox ID="tbBody" runat="server" SourceTypeName="Rock.Model.EmailTemplate, Rock" PropertyName="Body" TextMode="MultiLine" Rows="10" CssClass="input-xxlarge" />
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>

        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>
