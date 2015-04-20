<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Pages.ascx.cs" Inherits="RockWeb.Blocks.Administration.Pages" %>
<asp:UpdatePanel ID="upPages" runat="server">
<ContentTemplate>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert alert-danger block-message error"/>
    
    <asp:PlaceHolder ID="phContent" runat="server">

        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger"/>
        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
        
        <div class="grid">
            <Rock:Grid ID="rGrid" runat="server" AllowPaging="false" RowItemText="page" OnRowSelected="rGrid_Edit">
                <Columns>
                    <Rock:ReorderField />
                    <Rock:RockBoundField DataField="Id" HeaderText="Id" />
                    <asp:HyperLinkField DataNavigateUrlFormatString="~/page/{0}" DataNavigateUrlFields="Id" DataTextField="InternalName" HeaderText="Name" Target="_parent" />
                    <Rock:RockBoundField DataField="Layout.Name" HeaderText="Layout"  />
                    <Rock:DeleteField OnClick="rGrid_Delete" />
                </Columns>
            </Rock:Grid>
        </div>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false" CssClass="admin-details">

            <asp:HiddenField ID="hfPageId" runat="server" />

            <fieldset>
                <legend><asp:Literal ID="lEditAction" runat="server"/> Child Page</legend>
                <Rock:DataTextBox ID="dtbPageName" runat="server" SourceTypeName="Rock.Model.Page, Rock" PropertyName="InternalName" />
                <Rock:RockDropDownList ID="ddlLayout" runat="server" Label="Layout"></Rock:RockDropDownList>
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="lbSave" runat="server" Text="Add" CssClass="btn btn-primary" onclick="lbSave_Click" />
                <asp:LinkButton id="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="lbCancel_Click" CausesValidation="false" />
            </div>

        </asp:Panel>

    </asp:PlaceHolder>

</ContentTemplate>
</asp:UpdatePanel>
