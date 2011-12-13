<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Pages.ascx.cs" Inherits="RockWeb.Blocks.Administration.Pages" %>

<asp:UpdatePanel ID="upPages" runat="server">
<ContentTemplate>

    <Rock:Grid ID="rGrid" runat="server" PageSize="10" >
        <Columns>
            <Rock:ReorderField />
            <asp:BoundField DataField="Id" HeaderText="Id" />
            <asp:BoundField DataField="Name" HeaderText="Name" />
            <asp:BoundField DataField="Layout" HeaderText="Layout"  />
            <Rock:EditField OnClick="rGrid_Edit" />
            <Rock:DeleteField OnClick="rGrid_Delete" />
        </Columns>
    </Rock:Grid>

    <asp:Panel ID="pnlDetails" runat="server" Visible="false" CssClass="admin-details">

        <asp:HiddenField ID="hfPageId" runat="server" />
        <asp:ValidationSummary ID="vsPages" runat="server" CssClass="failureNotification" ValidationGroup="PagesValidationGroup"/>
        <fieldset>
            <legend>Child Page</legend>
            <ol>
                <li>
                    <Rock:DataTextBox ID="tbPageName" runat="server" SourceTypeName="Rock.CMS.Page, Rock" PropertyName="Name" />
                </li>
                <li>
                    <asp:Label ID="lblLayout" runat="server" AssociatedControlID="ddlLayout">Layout</asp:Label>
                    <asp:DropDownList ID="ddlLayout" runat="server"></asp:DropDownList>
                </li>
            </ol>
        </fieldset>
        <br />
        <asp:Button id="btnCancel" runat="server" Text="Cancel" CausesValidation="false" OnClick="btnCancel_Click" />
        <asp:Button ID="btnSave" runat="server" Text="Save" ValidationGroup="PagesValidationGroup" CssClass="button" onclick="btnSave_Click" />

    </asp:Panel>

    <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error" Visible="false" />

</ContentTemplate>
</asp:UpdatePanel>
