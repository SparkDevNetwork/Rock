<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EmailTemplates.ascx.cs" Inherits="RockWeb.Blocks.Administration.EmailTemplates" %>

<asp:UpdatePanel ID="upSettings" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlList" runat="server">

            <Rock:GridFilter ID="rFilter" runat="server">
                <Rock:RockDropDownList ID="ddlCategoryFilter" runat="server" Label="Category" />
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

        <asp:Panel ID="pnlDetails" runat="server" Visible="false" CssClass="panel panel-default">

            <div class="panel-body">
                <asp:HiddenField ID="hfEmailTemplateId" runat="server" />

                <div class="banner">
                    <h1><asp:Literal ID="lActionTitle" runat="server" /></h1>
                </div>

                <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger block-message error" />

                <fieldset>
                    
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DataTextBox ID="tbTitle" runat="server" SourceTypeName="Rock.Model.EmailTemplate, Rock" PropertyName="Title" />
                            <Rock:DataTextBox ID="tbCategory" runat="server" SourceTypeName="Rock.Model.EmailTemplate, Rock" PropertyName="Category" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbFrom" runat="server" SourceTypeName="Rock.Model.EmailTemplate, Rock" PropertyName="From" />
                            <Rock:DataTextBox ID="tbCc" runat="server" SourceTypeName="Rock.Model.EmailTemplate, Rock" PropertyName="Cc" />
                        </div>
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbTo" runat="server" SourceTypeName="Rock.Model.EmailTemplate, Rock" PropertyName="To" />
                            <Rock:DataTextBox ID="tbBcc" runat="server" SourceTypeName="Rock.Model.EmailTemplate, Rock" PropertyName="Bcc" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DataTextBox ID="tbSubject" runat="server" SourceTypeName="Rock.Model.EmailTemplate, Rock" PropertyName="Subject" />
                            <Rock:DataTextBox ID="tbBody" runat="server" SourceTypeName="Rock.Model.EmailTemplate, Rock" PropertyName="Body" TextMode="MultiLine" Rows="10" />
                        </div>
                    </div>
                    
                    
                    
                    
                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-default" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>
            </div>
        </asp:Panel>

        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Danger" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>
