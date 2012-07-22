<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EmailTemplates.ascx.cs" Inherits="RockWeb.Blocks.Administration.EmailTemplates" %>

<asp:UpdatePanel ID="upSettings" runat="server">
<ContentTemplate>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert-message block-massage error"/>

    <asp:PlaceHolder ID="phList" runat="server">

        <div class="grid-filter">
            <fieldset>
                <legend>Filter Options</legend>
                <Rock:LabeledDropDownList ID="ddlCategoryFilter" runat="server" LabelText="Category" AutoPostBack="true" OnSelectedIndexChanged="ddlCategoryFilter_SelectedIndexChanged" />
            </fieldset>
        </div>

        <Rock:Grid ID="rGrid" runat="server" EmptyDataText="No Templates Found" AllowSorting="true"  >
            <Columns>
                <asp:BoundField DataField="Category" HeaderText="Category" SortExpression="Category"  />
                <asp:BoundField DataField="Title" HeaderText="Title" SortExpression="Title" />
                <asp:BoundField DataField="From" HeaderText="From" SortExpression="From" />
                <asp:BoundField DataField="Subject" HeaderText="Subject" SortExpression="Subject" />
                <Rock:EditField OnClick="rGrid_Edit" />
                <Rock:DeleteField OnClick="rGrid_Delete" />
            </Columns>
        </Rock:Grid>

    </asp:PlaceHolder>

    <asp:Panel ID="pnlDetails" runat="server" Visible="false" CssClass="admin-details">

        <asp:HiddenField ID="hfEmailTemplateId" runat="server" />

        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert-message block-message error"/>
    
        <div class="row">

            <div class="span6">

                <fieldset>
                    <legend><asp:Literal ID="lAction" runat="server"></asp:Literal> Email Template</legend>
                    <Rock:DataTextBox ID="tbCategory" runat="server" SourceTypeName="Rock.CRM.EmailTemplate, Rock" PropertyName="Category" />
                    <Rock:DataTextBox ID="tbTitle" runat="server" SourceTypeName="Rock.CRM.EmailTemplate, Rock" PropertyName="Title" />
                    <Rock:DataTextBox ID="tbFrom" runat="server" SourceTypeName="Rock.CRM.EmailTemplate, Rock" PropertyName="From" />
                    <Rock:DataTextBox ID="tbTo" runat="server" SourceTypeName="Rock.CRM.EmailTemplate, Rock" PropertyName="To" />
                    <Rock:DataTextBox ID="tbCc" runat="server" SourceTypeName="Rock.CRM.EmailTemplate, Rock" PropertyName="Cc" />
                    <Rock:DataTextBox ID="tbBcc" runat="server" SourceTypeName="Rock.CRM.EmailTemplate, Rock" PropertyName="Bcc" />
                </fieldset>

            </div>

            <div class="span6">

                <fieldset>
                    <legend>&nbsp;</legend>
                    <Rock:DataTextBox ID="tbSubject" runat="server" SourceTypeName="Rock.CRM.EmailTemplate, Rock" PropertyName="Subject" />
                    <Rock:DataTextBox ID="tbBody" runat="server" SourceTypeName="Rock.CRM.EmailTemplate, Rock" PropertyName="Body" TextMode="MultiLine" Rows="10" CssClass="xxlarge" />
                </fieldset>

            </div>

        </div>
    
        <div class="actions">
            <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn primary" onclick="btnSave_Click" />
            <asp:LinkButton id="btnCancel" runat="server" Text="Cancel" CssClass="btn secondary" CausesValidation="false" OnClick="btnCancel_Click" />
        </div>

    </asp:Panel>

</ContentTemplate>
</asp:UpdatePanel>
