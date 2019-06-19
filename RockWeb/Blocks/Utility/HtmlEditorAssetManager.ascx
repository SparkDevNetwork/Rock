<%@ Control Language="C#" AutoEventWireup="true" CodeFile="HtmlEditorAssetManager.ascx.cs" Inherits="RockWeb.Blocks.Utility.HtmlEditorAssetManager" %>
<%@ Register TagPrefix="uc1" TagName="AssetManager" Src="~/Blocks/Cms/AssetManager.ascx" %>

<asp:Panel ID="pnlAssetManager" runat="server" CssClass="js-AssetManager-modal is-postback">

    <asp:Panel ID="pnlModalHeader" CssClass="modal-header" runat="server">
        <h3 class="modal-title">
            Select Asset
            <span class="js-cancel-file-button cursor-pointer pull-right" style="opacity: .5">&times;</span>
        </h3>
    </asp:Panel>

    <asp:Panel ID="pnlAssetManagerControl" runat="server" ScrollBars="Vertical" Height="423px">
        <uc1:AssetManager ID="amAssetManager" runat="server" />
    </asp:Panel>

    <asp:Panel ID="pnlModalFooterActions" CssClass="modal-footer" runat="server">
        <div class="row">
            <div class="actions">
                <a class="btn btn-primary js-select-file-button js-singleselect aspNetDisabled">OK</a>
                <a class="btn btn-link js-cancel-file-button">Cancel</a>
            </div>
        </div>
    </asp:Panel>

</asp:Panel>
