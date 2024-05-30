<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Pages.ascx.cs" Inherits="RockWeb.Blocks.Administration.Pages" %>
<asp:UpdatePanel ID="upPages" runat="server">
<ContentTemplate>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert alert-danger block-message error"/>

    <asp:PlaceHolder ID="phContent" runat="server">

        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation"/>
        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />

        <Rock:ModalDialog ID="mdConfirmCopy" runat="server" Title="Please Confirm" SaveButtonText="Yes" OnSaveClick="mdConfirmCopy_Click">
            <Content>
                <asp:ValidationSummary ID="valSummaryValue" runat="server" CssClass="alert alert-error" />
                <asp:HiddenField ID="hfPageIdToCopy" runat="server" />
                Do you wish to copy this page and all child pages?
            </Content>
         </Rock:ModalDialog>

        <div class="grid">
            <Rock:Grid ID="rGrid" runat="server" AllowPaging="false" RowItemText="page" OnRowSelected="rGrid_Edit" ShowActionsInHeader="false" ShowConfirmDeleteDialog="false">
                <Columns>
                    <Rock:ReorderField />
                    <Rock:RockBoundField DataField="Id" HeaderText="Id" />
                    <asp:HyperLinkField DataNavigateUrlFormatString="~/page/{0}" DataNavigateUrlFields="Id" DataTextField="InternalName" HeaderText="Name" Target="_parent" />
                    <Rock:RockBoundField DataField="Layout.Name" HeaderText="Layout"  />
                    <Rock:LinkButtonField HeaderText="Copy" CssClass="btn btn-default btn-sm btn-square" Text="<i class='fa fa-clone'></i>" OnClick="rGrid_Copy" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                    <Rock:DeleteField OnClick="rGrid_Delete" />
                </Columns>
            </Rock:Grid>
        </div>

        <Rock:ModalDialog ID="mdDeleteModal" runat="server" ValidationGroup="vgDeleteModal" Title="Are you sure?" OnSaveClick="mdDeleteModal_SaveClick" SaveButtonText="Delete" Visible="false">
            <Content>
                <p>Are you sure you want to delete this page?</p>
                <Rock:RockCheckBox ID="cbDeleteInteractions" runat="server" Text="Delete any interactions for this page" Checked="true" />
            </Content>
        </Rock:ModalDialog>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false" CssClass="admin-details">

            <asp:HiddenField ID="hfPageId" runat="server" />

            <fieldset>
                <legend><asp:Literal ID="lEditAction" runat="server"/> Child Page</legend>
                <Rock:DataTextBox ID="dtbPageName" runat="server" SourceTypeName="Rock.Model.Page, Rock" PropertyName="InternalName" />
                <Rock:RockDropDownList ID="ddlLayout" runat="server" Label="Layout"></Rock:RockDropDownList>
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="lbSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Add" CssClass="btn btn-primary" onclick="lbSave_Click" />
                <asp:LinkButton id="lbCancel" runat="server"  data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" OnClick="lbCancel_Click" CausesValidation="false" />
            </div>

        </asp:Panel>

    </asp:PlaceHolder>

</ContentTemplate>
</asp:UpdatePanel>
