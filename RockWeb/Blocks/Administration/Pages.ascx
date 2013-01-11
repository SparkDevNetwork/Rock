<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Pages.ascx.cs" Inherits="RockWeb.Blocks.Administration.Pages" %>
<asp:UpdatePanel ID="upPages" runat="server">
<ContentTemplate>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert alert-error block-message error"/>
    
    <asp:PlaceHolder ID="phContent" runat="server">

        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error"/>

        <Rock:Grid ID="rGrid" runat="server" AllowPaging="false" RowItemText="page" OnRowSelected="rGrid_Edit">
            <Columns>
                <Rock:ReorderField />
                <asp:BoundField DataField="Id" HeaderText="Id" />
                <asp:HyperLinkField DataNavigateUrlFormatString="~/page/{0}" DataNavigateUrlFields="Id" DataTextField="Name" HeaderText="Name" Target="_parent" />
                <asp:BoundField DataField="Layout" HeaderText="Layout"  />
                <Rock:DeleteField OnClick="rGrid_Delete" />
            </Columns>
        </Rock:Grid>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false" CssClass="admin-details">

            <asp:HiddenField ID="hfPageId" runat="server" />

            <fieldset>
                <legend><asp:Literal ID="lEditAction" runat="server"/> Child Page</legend>
                <Rock:DataTextBox ID="tbPageName" runat="server" SourceTypeName="Rock.Model.Page, Rock" PropertyName="Name" />
                <Rock:LabeledDropDownList ID="ddlLayout" runat="server" LabelText="Layout"></Rock:LabeledDropDownList>
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Add" CssClass="btn btn-primary" onclick="btnSave_Click" />
                <asp:LinkButton id="btnCancel" runat="server" Text="Cancel" CssClass="btn" OnClick="btnCancel_Click" CausesValidation="false" />
            </div>

        </asp:Panel>

    </asp:PlaceHolder>

</ContentTemplate>
</asp:UpdatePanel>
