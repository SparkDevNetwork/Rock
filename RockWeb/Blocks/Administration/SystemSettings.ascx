<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SystemSettings.ascx.cs" Inherits="RockWeb.Blocks.Administration.SystemSettings" %>

<asp:UpdatePanel ID="upnlSystemSettings" runat="server">
<ContentTemplate>

    <section class="panel panel-block">
        <div class="panel-heading">
            <h1 class="panel-title">
                <asp:Literal ID="lCategoryName" runat="server" />
            </h1>
        </div>
        <div class="panel-body">
            <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
            <asp:ValidationSummary ID="valSummaryTop" runat="server"  HeaderText="Please correct the following:" CssClass="alert alert-validation" />
            <fieldset id="fsAttributes" runat="server" class="attribute-values"></fieldset>
            <asp:Panel ID="pnlEditActions" runat="server" CssClass="actions" Visible="false">
                <asp:LinkButton ID="lbEdit" runat="server" CssClass="btn btn-primary" ToolTip="Edit Attributes" OnClick="lbEdit_Click" Text="Edit"></asp:LinkButton>
            </asp:Panel>
            <asp:Panel ID="pnlActions" runat="server" CssClass="actions" Visible="false">
                <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" ValidationGroup="vgAttributeValues" />
                <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_Click" CausesValidation="false" />
            </asp:Panel>
        </div>
    </section>

</ContentTemplate>
</asp:UpdatePanel>

